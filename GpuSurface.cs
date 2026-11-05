using System;
using System.Drawing;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
namespace OdysseyCompanion {
 sealed class GpuSurface:IDisposable {
  readonly SharpDX.Direct3D11.Device device;readonly SwapChain1 swapChain;readonly Texture2D buffer;readonly RenderTargetView target;
  public GpuSurface(SharpDX.Direct3D11.Device d,IntPtr window,int width,int height){device=d;
   using(var dxgiDevice=d.QueryInterface<SharpDX.DXGI.Device>())using(var adapter=dxgiDevice.Adapter)using(var factory=adapter.GetParent<Factory2>()){
    var desc=new SwapChainDescription1{Width=width,Height=height,Format=Format.B8G8R8A8_UNorm,SampleDescription=new SampleDescription(1,0),Usage=Usage.RenderTargetOutput,BufferCount=2,Scaling=Scaling.Stretch,SwapEffect=SwapEffect.FlipSequential,AlphaMode=AlphaMode.Ignore,Flags=SwapChainFlags.None};
    swapChain=new SwapChain1(factory,d,window,ref desc,null,null);factory.MakeWindowAssociation(window,WindowAssociationFlags.IgnoreAll);
   }
   buffer=swapChain.GetBackBuffer<Texture2D>(0);target=new RenderTargetView(d,buffer);
  }
  public void Present(Texture2D image){device.ImmediateContext.CopyResource(image,buffer);swapChain.Present(0,PresentFlags.None).CheckError();}
  public void Color(Color c){device.ImmediateContext.ClearRenderTargetView(target,new SharpDX.Mathematics.Interop.RawColor4(c.R/255f,c.G/255f,c.B/255f,1));swapChain.Present(0,PresentFlags.None).CheckError();}
  public void Dispose(){target.Dispose();buffer.Dispose();swapChain.Dispose();}
 }
}
