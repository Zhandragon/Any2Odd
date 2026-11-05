using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OdysseyCompanion {
 static class RendererTaskbar {
  [ComImport,Guid("56FDF342-FD6D-11D0-958A-006097C9A090"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  interface ITaskbarList {void HrInit();void AddTab(IntPtr window);void DeleteTab(IntPtr window);void ActivateTab(IntPtr window);void SetActiveAlt(IntPtr window);}
  [StructLayout(LayoutKind.Sequential)] struct FlashInfo {public uint Size;public IntPtr Window;public uint Flags,Count,Timeout;}
  [DllImport("user32.dll")]static extern bool FlashWindowEx(ref FlashInfo info);
  public static void Remove(IntPtr window){
   uint owner;Win.GetWindowThreadProcessId(window,out owner);
   if(owner!=Process.GetCurrentProcess().Id||Win.Class(window)!="GLFW30"||!OverlayController.IsOutputTitle(Win.Title(window)))
    throw new InvalidOperationException("Refusing taskbar cleanup for a window outside this renderer.");
   // The engine creates its taskbar entry before we set TOOLWINDOW. Changing
   // that style alone leaves a stale shell entry which blocks auto-hide.
   // Remove only our native renderer's entry; the companion UI/tray is separate.
   var flash=new FlashInfo{Size=(uint)Marshal.SizeOf(typeof(FlashInfo)),Window=window,Flags=0};FlashWindowEx(ref flash);
   object shell=Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("56FDF344-FD6D-11D0-958A-006097C9A090")));
   try{var taskbar=(ITaskbarList)shell;taskbar.HrInit();taskbar.DeleteTab(window);}finally{Marshal.FinalReleaseComObject(shell);}
   Files.Log("Removed native renderer taskbar entry so Windows auto-hide can resume.");
  }
 }
}
