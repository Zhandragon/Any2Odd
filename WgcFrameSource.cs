using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Security.Authorization.AppCapabilityAccess;

namespace OdysseyCompanion {
 // Interop follows Microsoft's Composition Win32 capture samples (MIT).
 // Keep only the newest frame. Cursor capture is explicitly disabled so the
 // normal live Windows pointer is never sent through the delayed AI pipeline.
 sealed class WgcFrameSource:IDisposable {
  [ComImport,Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface IItemInterop {IntPtr CreateForWindow(IntPtr window,ref Guid iid);IntPtr CreateForMonitor(IntPtr monitor,ref Guid iid);}
  [ComImport,Guid("A9B3D012-3DF2-4EE3-B8D1-8695F457D3C1"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface ISurfaceAccess {IntPtr GetInterface(ref Guid iid);}
  [DllImport("d3d11.dll")]static extern int CreateDirect3D11DeviceFromDXGIDevice(IntPtr dxgi,out IntPtr inspectable);
  IDirect3DDevice wrappedDevice;Direct3D11CaptureFramePool pool;GraphicsCaptureSession session;GraphicsCaptureItem item;
  bool subscribed;
  public event Action FrameAvailable;
  public WgcFrameSource(SharpDX.Direct3D11.Device device,IntPtr monitor){try{
   using(var dxgi=device.QueryInterface<SharpDX.DXGI.Device>()){IntPtr pointer;Marshal.ThrowExceptionForHR(CreateDirect3D11DeviceFromDXGIDevice(dxgi.NativePointer,out pointer));try{wrappedDevice=(IDirect3DDevice)Marshal.GetObjectForIUnknown(pointer);}finally{Marshal.Release(pointer);}}
   var factory=(IItemInterop)WindowsRuntimeMarshal.GetActivationFactory(typeof(GraphicsCaptureItem));var iid=new Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760");var raw=factory.CreateForMonitor(monitor,ref iid);try{item=(GraphicsCaptureItem)Marshal.GetObjectForIUnknown(raw);}finally{Marshal.Release(raw);}
   pool=Direct3D11CaptureFramePool.CreateFreeThreaded(wrappedDevice,DirectXPixelFormat.R16G16B16A16Float,2,item.Size);
   session=pool.CreateCaptureSession(item);session.IsCursorCaptureEnabled=false;
   if(session.IsCursorCaptureEnabled)throw new InvalidOperationException("Windows did not disable cursor capture.");
   ConfigureBorder(session);
   session.MinUpdateInterval=TimeSpan.FromMilliseconds(16);
   pool.FrameArrived+=OnFrame;subscribed=true;session.StartCapture();Files.Log("WGC desktop capture started: cursor excluded, latest frame only, 16 ms minimum interval.");
  }catch{Dispose();throw;}}
  static void ConfigureBorder(GraphicsCaptureSession target){
   string access="Unavailable",error=null;
   try{
    // Request the supported per-app permission; never change global capture
    // privacy settings. A denied request leaves Windows' border in place.
    // These Windows metadata files are split rather than the SDK's unified
    // Windows.winmd; wait for IAsyncOperation on an MTA thread without AsTask.
    var result=System.Threading.Tasks.Task.Run(delegate{
     var request=GraphicsCaptureAccess.RequestAccessAsync(GraphicsCaptureAccessKind.Borderless);
     try{var watch=System.Diagnostics.Stopwatch.StartNew();while(request.Status==Windows.Foundation.AsyncStatus.Started){if(watch.Elapsed.TotalSeconds>=10){request.Cancel();throw new TimeoutException("Borderless capture permission request timed out.");}System.Threading.Thread.Sleep(10);}return request.GetResults();}finally{request.Close();}
    }).GetAwaiter().GetResult();
    access=result.ToString();
    if(result==AppCapabilityAccessStatus.Allowed)target.IsBorderRequired=false;
   }catch(Exception e){error=e.Message;}
   Files.Log("Capture border permission: "+access+"; border required: "+target.IsBorderRequired+(error==null?"":"; "+error));
   Files.Save(System.IO.Path.Combine(Files.Data,"capture-border-status.json"),new{pid=System.Diagnostics.Process.GetCurrentProcess().Id,time=DateTime.Now.ToString("o"),access,borderRequired=target.IsBorderRequired,error});
  }
  void OnFrame(Direct3D11CaptureFramePool sender,object args){var handler=FrameAvailable;if(handler!=null)handler();}
  public Direct3D11CaptureFrame TakeLatest(){var latest=pool.TryGetNextFrame();if(latest==null)return null;for(int i=0;i<2;i++){var next=pool.TryGetNextFrame();if(next==null)break;latest.Dispose();latest=next;}if(latest.ContentSize.Width!=item.Size.Width||latest.ContentSize.Height!=item.Size.Height){latest.Dispose();throw new InvalidOperationException("Monitor size changed. Restart conversion.");}return latest;}
  public static SharpDX.Direct3D11.Texture2D Texture(Direct3D11CaptureFrame frame){var access=(ISurfaceAccess)frame.Surface;var iid=new Guid("6F15AAF2-D208-4E89-9AB4-489535D34F9C");return new SharpDX.Direct3D11.Texture2D(access.GetInterface(ref iid));}
  public void Dispose(){FrameAvailable=null;if(pool!=null&&subscribed){pool.FrameArrived-=OnFrame;subscribed=false;}if(session!=null){session.Dispose();session=null;}if(pool!=null){pool.Dispose();pool=null;}if(wrappedDevice!=null){wrappedDevice.Dispose();wrappedDevice=null;}item=null;}
 }
}
