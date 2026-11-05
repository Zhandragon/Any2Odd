using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OdysseyCompanion {
 // Capture the monitor's final composition, including Explorer's wallpaper,
 // icons, translucent windows and menus. The output is excluded from capture,
 // and the intermediate source surface is placed outside ALL monitor bounds.
 sealed class DesktopCapture : IDisposable {
  [DllImport("user32",SetLastError=true)] internal static extern bool SetWindowDisplayAffinity(IntPtr window,uint affinity);
  [DllImport("user32",SetLastError=true)] internal static extern bool GetWindowDisplayAffinity(IntPtr window,out uint affinity);
  readonly Rectangle screen;
  Bitmap frame;
  public Bitmap Frame {get{if(frame==null)frame=new Bitmap(screen.Width,screen.Height,PixelFormat.Format32bppRgb);return frame;}}
  readonly GpuDesktopCapture gpu;
  public event Action FrameAvailable {add{gpu.FrameAvailable+=value;}remove{gpu.FrameAvailable-=value;}}
  public long Frames {get;private set;}
  public DesktopCapture(Rectangle bounds){screen=bounds;gpu=new GpuDesktopCapture(bounds);}
  public bool Update(){if(!gpu.Update(Frame))return false;Frames++;return true;}
  public bool Present(IntPtr window){if(!gpu.Present(window))return false;Frames++;return true;}
  public void Dispose(){gpu.Dispose();if(frame!=null)frame.Dispose();}

  // A deterministic desktop integration test: both windows and their pixels
  // belong to this test. No screenshots of user content are saved.
  sealed class TestWindow:Form {
   public bool GpuDrawing;
   public TestWindow(Rectangle bounds,Color color){FormBorderStyle=FormBorderStyle.None;StartPosition=FormStartPosition.Manual;Bounds=bounds;BackColor=color;ShowInTaskbar=false;TopMost=true;}
   protected override bool ShowWithoutActivation {get{return true;}}
   protected override void OnPaint(PaintEventArgs e){if(!GpuDrawing)base.OnPaint(e);}
   protected override void OnPaintBackground(PaintEventArgs e){if(!GpuDrawing)base.OnPaintBackground(e);}
   protected override CreateParams CreateParams {get{var cp=base.CreateParams;cp.ExStyle|=0x08000000|0x80;return cp;}}
  }
  public static int TestExclusion(){
   Rectangle bounds=new Rectangle(Screen.PrimaryScreen.Bounds.X+80,Screen.PrimaryScreen.Bounds.Y+80,160,80);
   var cyan=Color.FromArgb(30,170,210);var green=Color.FromArgb(170,230,50);int step=0,retries=0;bool passed=false;string result="Test did not complete.";var checks=new System.Collections.Generic.List<string>();
   using(var background=new TestWindow(bounds,cyan))using(var excluded=new TestWindow(bounds,Color.Magenta))using(var popup=new TestWindow(new Rectangle(bounds.X,bounds.Y,40,80),Color.Red))using(var sample=new DesktopCapture(bounds))using(var timer=new Timer{Interval=100}){
    background.Shown+=delegate{timer.Start();};
    timer.Tick+=delegate{try{
     sample.Update();var left=sample.Frame.GetPixel(20,40);var right=sample.Frame.GetPixel(120,40);bool match=false;
     if(step==0)match=left.ToArgb()==cyan.ToArgb()&&right.ToArgb()==cyan.ToArgb();
     if(step==1)match=left.ToArgb()==green.ToArgb()&&right.ToArgb()==green.ToArgb();
     if(step==2)match=left.ToArgb()==Color.Red.ToArgb()&&right.ToArgb()==green.ToArgb();
     if(step==3)match=left.ToArgb()==green.ToArgb()&&right.ToArgb()==Color.Red.ToArgb();
     if(step==4)match=left.ToArgb()==green.ToArgb()&&right.ToArgb()==green.ToArgb();
     if(!match){if(++retries<30){background.Invalidate();background.Update();return;}throw new Exception("Stage "+step+" failed: left="+left+" right="+right+" frames="+sample.Frames);}
     retries=0;
     if(step==0){checks.Add("Initial desktop frame");if(!SetWindowDisplayAffinity(excluded.Handle,0x11))throw new Exception("Window exclusion failed: "+Marshal.GetLastWin32Error());excluded.Show();background.BackColor=green;}
     if(step==1){checks.Add("Fresh underlying pixels through excluded output");excluded.Hide();popup.Show();}
     if(step==2){checks.Add("Transient popup appears");popup.Location=new Point(bounds.X+100,bounds.Y);}
     if(step==3){checks.Add("Moving popup clears its previous position");popup.Hide();}
     if(step==4){checks.Add("Closing popup restores the background");passed=true;result="All desktop capture regression checks passed.";}
     step++;if(!passed)return;
    }catch(Exception e){result=e.Message;}timer.Stop();popup.Close();excluded.Close();background.Close();};
    Application.Run(background);
   }
   Files.Save(System.IO.Path.Combine(Files.Data,"capture-exclusion-test.json"),new{passed,result,checks,time=DateTime.Now.ToString("o")});return passed?0:1;
  }
  public static int TestOffscreen(){
   var size=Screen.PrimaryScreen.Bounds.Size;var monitors=Array.ConvertAll(Screen.AllScreens,s=>s.Bounds);var outside=Mirror.OffscreenBounds(size,monitors);var onScreen=new Rectangle(Screen.PrimaryScreen.Bounds.X+80,Screen.PrimaryScreen.Bounds.Y+80,80,80);
   var cyan=Color.FromArgb(30,170,210);var green=Color.FromArgb(170,230,50);bool passed=false;string result="Not completed";int stage=0,retries=0;IntPtr thumb=IntPtr.Zero;
   using(var source=new TestWindow(outside,cyan){GpuDrawing=true})using(var preview=new TestWindow(onScreen,Color.Black))using(var capture=new DesktopCapture(onScreen))using(var device=new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware,SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport))using(var surface=new GpuSurface(device,source.Handle,size.Width,size.Height))using(var timer=new Timer{Interval=100}){
    preview.Shown+=delegate{source.Show();surface.Color(cyan);Win.Rect actual;Win.GetWindowRect(source.Handle,out actual);if(Array.Exists(monitors,r=>r.IntersectsWith(actual.Box)))throw new Exception("Source surface overlaps a monitor.");Files.Log("Thumbnail test GPU source visible="+Win.IsWindowVisible(source.Handle)+" at "+actual.Box);
     int hr=Win.DwmRegisterThumbnail(preview.Handle,source.Handle,out thumb);if(hr!=0)throw new Exception("Offscreen thumbnail failed: "+hr);
     var props=new Win.Thumbnail{Flags=31,Dest=new Win.Rect(new Rectangle(0,0,80,80)),Source=new Win.Rect(new Rectangle(Point.Empty,size)),Opacity=255,Visible=true,ClientOnly=false};Files.Log("Thumbnail test properties HRESULT="+Win.DwmUpdateThumbnailProperties(thumb,ref props));timer.Start();};
    timer.Tick+=delegate{try{capture.Update();Color expected=stage==0?cyan:green;if(capture.Frame.GetPixel(40,40).ToArgb()!=expected.ToArgb()){if(++retries<30){surface.Color(expected);return;}throw new Exception("Offscreen surface did not deliver fresh pixels at stage "+stage+"; pixel "+capture.Frame.GetPixel(40,40));}
     if(stage++==0){retries=0;surface.Color(green);return;}passed=true;result="A GPU offscreen surface remains live and capturable, without overlapping any monitor.";
    }catch(Exception e){result=e.Message;}timer.Stop();if(thumb!=IntPtr.Zero){Win.DwmUnregisterThumbnail(thumb);thumb=IntPtr.Zero;}source.Close();preview.Close();};
    Application.Run(preview);
   }
   Files.Save(System.IO.Path.Combine(Files.Data,"offscreen-surface-test.json"),new{passed,result,bounds=outside,time=DateTime.Now.ToString("o")});return passed?0:1;
  }
 }
}
