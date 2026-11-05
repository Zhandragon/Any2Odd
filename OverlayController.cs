using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OdysseyCompanion {
 // Only changes the renderer window created inside our own worker process.
 // Keeping Z order is separate from keyboard focus: never call SetForegroundWindow.
 sealed class OverlayController {
  internal const string OutputTitle="Any2Odd - 3D output";
  internal static bool IsOutputTitle(string title){return title==OutputTitle||title=="Odyssey 3D Hub - 3D Player";}
  internal const long InputStyles=0x08000000L|0x00080000L|0x00000020L|0x00000080L;
  internal static long DesiredStyle(long current){return (current|InputStyles)&~0x00040000L;}
  readonly int worker=Process.GetCurrentProcess().Id;
  readonly int parent;
  readonly bool keepOnTop;
  readonly bool excludeFromCapture;
  IntPtr output,previousFocus;
  DateTime lastRaise=DateTime.MinValue;
  DateTime verifyTaskbarAt=DateTime.MaxValue;
  bool configured;
  public bool Attached {get{return configured&&Win.IsWindow(output);}}
  public OverlayController(int parentPid,bool persistent,bool exclude){parent=parentPid;keepOnTop=persistent;excludeFromCapture=exclude;}
  public bool CaptureSafe {get{uint affinity;return Attached&&DesktopCapture.GetWindowDisplayAffinity(output,out affinity)&&affinity==0x11;}}

  public void Tick(bool restore){
   if(output==IntPtr.Zero||!Win.IsWindow(output)){
    output=IntPtr.Zero;configured=false;
    Win.EnumWindows(delegate(IntPtr h,IntPtr p){uint pid;Win.GetWindowThreadProcessId(h,out pid);
     if(pid==worker&&Win.Class(h)=="GLFW30"&&IsOutputTitle(Win.Title(h)))output=h;
     return true;
    },IntPtr.Zero);
    if(output==IntPtr.Zero)return;
   }
   long old=Win.GetWindowLongPtr(output,-20).ToInt64();
   long next=DesiredStyle(old);
   bool changed=old!=next;
   if(changed)Win.SetWindowLongPtr(output,-20,new IntPtr(next));
   if(!configured||changed){
    Win.SetWindowText(output,OutputTitle);
    // Alpha 255 preserves the image. LAYERED + TRANSPARENT passes real mouse
    // input to other processes; HTTRANSPARENT alone only covers our own thread.
    if(!Win.SetLayeredWindowAttributes(output,0,255,2))
     throw new InvalidOperationException("Could not make the 3D output click-through: "+Marshal.GetLastWin32Error());
    long actual=Win.GetWindowLongPtr(output,-20).ToInt64();
    if((actual&InputStyles)!=InputStyles||(actual&0x40000L)!=0)
     throw new InvalidOperationException("The 3D output did not accept its input-safe styles.");
    Files.Log("Renderer overlay configured: click-through, no activation, hidden from Alt+Tab; HWND "+output);
    RendererTaskbar.Remove(output);
    // Shell registration is asynchronous. Verify once more after startup.
    verifyTaskbarAt=DateTime.UtcNow.AddSeconds(2);
    configured=true;
   }
   if(excludeFromCapture&&!CaptureSafe){
    if(!DesktopCapture.SetWindowDisplayAffinity(output,0x11)||!CaptureSafe)
     throw new InvalidOperationException("Windows could not exclude the 3D output from desktop capture; conversion stopped to prevent image feedback.");
    Files.Log("3D output excluded from desktop capture to prevent feedback.");
   }
   if(DateTime.UtcNow>=verifyTaskbarAt){RendererTaskbar.Remove(output);verifyTaskbarAt=DateTime.MaxValue;}
   var focus=Win.GetForegroundWindow();
   bool focusChanged=focus!=previousFocus;
   previousFocus=focus;
   if(restore||(keepOnTop&&(changed||focusChanged||Win.IsIconic(output)||!Win.IsWindowVisible(output)||(old&8)==0||(DateTime.UtcNow-lastRaise).TotalSeconds>=1))){
    if(Win.IsIconic(output)||!Win.IsWindowVisible(output))Win.ShowWindow(output,4); // SW_SHOWNOACTIVATE
    if(!Win.SetWindowPos(output,new IntPtr(-1),0,0,0,0,(uint)(0x53|(changed?0x20:0))))
     throw new InvalidOperationException("Could not restore the 3D overlay: "+Marshal.GetLastWin32Error());
    // Keep our controls and their menus reachable above the output, without
    // activating either. Other apps keep whatever input focus the user chose.
    Win.EnumWindows(delegate(IntPtr h,IntPtr p){uint pid;Win.GetWindowThreadProcessId(h,out pid);
     if(pid==parent&&Win.IsWindowVisible(h)&&!Win.IsIconic(h))
      Win.SetWindowPos(h,new IntPtr(-1),0,0,0,0,0x13);
     return true;
    },IntPtr.Zero);
    lastRaise=DateTime.UtcNow;
    if(focusChanged||restore)Files.Log("Restored 3D overlay without activation; foreground HWND "+focus);
   }
  }
 }
}
