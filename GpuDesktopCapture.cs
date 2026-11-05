using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace OdysseyCompanion {
 // Each acquired DXGI texture is a complete current desktop frame. Never blend
 // it with the previous frame: disappeared menus must be erased as well.
 sealed class GpuDesktopCapture:IDisposable {
  SharpDX.Direct3D11.Device device;
  WgcFrameSource source;
  public event Action FrameAvailable;
  Texture2D staging;
  Texture2D hdrInput,sdrOutput;ShaderResourceView hdrView;RenderTargetView sdrTarget;VertexShader vertexShader;PixelShader pixelShader;
  readonly float whiteScale;
  GpuSurface surface;Texture2D sdrInput;
  readonly Rectangle capture,outputBounds;
  public GpuDesktopCapture(Rectangle bounds){
   capture=bounds;string monitor=Screen.FromRectangle(bounds).DeviceName;whiteScale=DisplayColor.SdrWhiteScale(monitor);
   try{
    using(var factory=new Factory1()){
     foreach(var adapter in factory.Adapters1){using(adapter){
      foreach(var output in adapter.Outputs){using(output){
       var description=output.Description;if(description.DeviceName!=monitor)continue;
       outputBounds=Rectangle.FromLTRB(description.DesktopBounds.Left,description.DesktopBounds.Top,description.DesktopBounds.Right,description.DesktopBounds.Bottom);
       if(!outputBounds.Contains(capture))throw new InvalidOperationException("Capture rectangle crosses a monitor boundary.");
       if(description.Rotation!=DisplayModeRotation.Identity&&description.Rotation!=DisplayModeRotation.Unspecified)throw new InvalidOperationException("Desktop capture requires landscape orientation on the Odyssey.");
       device=new SharpDX.Direct3D11.Device(adapter,DeviceCreationFlags.BgraSupport);
       using(var threads=device.QueryInterface<SharpDX.Direct3D11.Multithread>())threads.SetMultithreadProtected(true);
       using(var latency=device.QueryInterface<SharpDX.DXGI.Device1>())latency.MaximumFrameLatency=1;
       source=new WgcFrameSource(device,description.MonitorHandle);source.FrameAvailable+=delegate{var handler=FrameAvailable;if(handler!=null)handler();};
       Files.Log("GPU desktop capture format: R16G16B16A16_Float; SDR white scale "+whiteScale);
       staging=new Texture2D(device,new Texture2DDescription{Width=capture.Width,Height=capture.Height,MipLevels=1,ArraySize=1,Format=Format.B8G8R8A8_UNorm,SampleDescription=new SampleDescription(1,0),Usage=ResourceUsage.Staging,BindFlags=BindFlags.None,CpuAccessFlags=CpuAccessFlags.Read,OptionFlags=ResourceOptionFlags.None});
       return;
      }}
     }}
    }
    throw new InvalidOperationException("The Odyssey graphics output was not found.");
   }catch{Dispose();throw;}
  }
  public bool Update(Bitmap bitmap){return Capture(bitmap,IntPtr.Zero);}
  public bool Present(IntPtr window){return Capture(null,window);}
  bool Capture(Bitmap bitmap,IntPtr window){
   using(var frame=source.TakeLatest()){
   if(frame==null)return false;
   using(var texture=WgcFrameSource.Texture(frame)){
    var context=device.ImmediateContext;
    Texture2D current;
    if(texture.Description.Format==Format.R16G16B16A16_Float){
     EnsureHdrConverter();CopyRegion(texture,hdrInput);
     context.InputAssembler.InputLayout=null;context.InputAssembler.PrimitiveTopology=SharpDX.Direct3D.PrimitiveTopology.TriangleList;
     context.VertexShader.Set(vertexShader);context.PixelShader.Set(pixelShader);context.PixelShader.SetShaderResource(0,hdrView);
     context.OutputMerger.SetRenderTargets(sdrTarget);context.Rasterizer.SetViewport(0,0,capture.Width,capture.Height);context.Draw(3,0);
     context.OutputMerger.ResetTargets();context.PixelShader.SetShaderResource(0,null);current=sdrOutput;
    }else if(texture.Description.Format==Format.B8G8R8A8_UNorm){if(capture==outputBounds)current=texture;else{if(sdrInput==null){var desc=staging.Description;desc.Usage=ResourceUsage.Default;desc.CpuAccessFlags=CpuAccessFlags.None;sdrInput=new Texture2D(device,desc);}CopyRegion(texture,sdrInput);current=sdrInput;}}
    else throw new InvalidOperationException("Unsupported desktop pixel format: "+texture.Description.Format);
    if(window!=IntPtr.Zero){if(surface==null)surface=new GpuSurface(device,window,capture.Width,capture.Height);surface.Present(current);return true;}
    context.CopyResource(current,staging);
    var mapped=context.MapSubresource(staging,0,MapMode.Read,SharpDX.Direct3D11.MapFlags.None);
    try{var pixels=bitmap.LockBits(new Rectangle(0,0,capture.Width,capture.Height),ImageLockMode.WriteOnly,bitmap.PixelFormat);
     try{if(mapped.RowPitch==pixels.Stride)SharpDX.Utilities.CopyMemory(pixels.Scan0,mapped.DataPointer,pixels.Stride*capture.Height);
      else for(int y=0;y<capture.Height;y++)SharpDX.Utilities.CopyMemory(IntPtr.Add(pixels.Scan0,y*pixels.Stride),IntPtr.Add(mapped.DataPointer,y*mapped.RowPitch),capture.Width*4);
     }finally{bitmap.UnlockBits(pixels);}
    }finally{context.UnmapSubresource(staging,0);}
   }return true;}
  }
  void CopyRegion(Texture2D source,Texture2D destination){if(capture==outputBounds)device.ImmediateContext.CopyResource(source,destination);else device.ImmediateContext.CopySubresourceRegion(source,0,new ResourceRegion(capture.Left-outputBounds.Left,capture.Top-outputBounds.Top,0,capture.Right-outputBounds.Left,capture.Bottom-outputBounds.Top,1),destination,0);}
  void EnsureHdrConverter(){if(hdrInput!=null)return;
   var description=new Texture2DDescription{Width=capture.Width,Height=capture.Height,MipLevels=1,ArraySize=1,Format=Format.R16G16B16A16_Float,SampleDescription=new SampleDescription(1,0),Usage=ResourceUsage.Default,BindFlags=BindFlags.ShaderResource};
   hdrInput=new Texture2D(device,description);hdrView=new ShaderResourceView(device,hdrInput);description.Format=Format.B8G8R8A8_UNorm;description.BindFlags=BindFlags.RenderTarget;sdrOutput=new Texture2D(device,description);sdrTarget=new RenderTargetView(device,sdrOutput);
   string shader="Texture2D<float4> inputTexture:register(t0); float4 VS(uint id:SV_VertexID):SV_POSITION {float2 uv=float2((id<<1)&2,id&2);return float4(uv*float2(2,-2)+float2(-1,1),0,1);} float4 PS(float4 p:SV_POSITION):SV_TARGET {float3 c=saturate(inputTexture.Load(int3(int2(p.xy),0)).rgb/"+whiteScale.ToString("0.000000",System.Globalization.CultureInfo.InvariantCulture)+");float3 s=lerp(1.055*pow(c,1.0/2.4)-0.055,12.92*c,step(c,0.0031308));return float4(s,1);}";
   using(var code=SharpDX.D3DCompiler.ShaderBytecode.Compile(shader,"VS","vs_5_0"))vertexShader=new VertexShader(device,code);
   using(var code=SharpDX.D3DCompiler.ShaderBytecode.Compile(shader,"PS","ps_5_0"))pixelShader=new PixelShader(device,code);
  }
  public void Dispose(){FrameAvailable=null;if(source!=null){source.Dispose();source=null;}if(surface!=null)surface.Dispose();if(sdrInput!=null)sdrInput.Dispose();if(pixelShader!=null)pixelShader.Dispose();if(vertexShader!=null)vertexShader.Dispose();if(sdrTarget!=null)sdrTarget.Dispose();if(hdrView!=null)hdrView.Dispose();if(sdrOutput!=null)sdrOutput.Dispose();if(hdrInput!=null)hdrInput.Dispose();if(staging!=null){staging.Dispose();staging=null;}if(device!=null){device.Dispose();device=null;}}
 }
}
