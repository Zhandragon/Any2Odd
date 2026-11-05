using System;
using System.Windows.Shell;
namespace OdysseyCompanion {
 static class TaskbarMenu {
  static System.Windows.Application shellApplication;
  public static void Install(){try{
   shellApplication=System.Windows.Application.Current??new System.Windows.Application();
   var list=new JumpList{ShowFrequentCategory=false,ShowRecentCategory=false};
   string exe=System.Windows.Forms.Application.ExecutablePath;
   list.JumpItems.Add(new JumpTask{Title="Turn 3D on / off",Description="Toggle Any2Odd 3D conversion",ApplicationPath=exe,Arguments="--toggle",IconResourcePath=exe,IconResourceIndex=0});
   list.JumpItems.Add(new JumpTask{Title="Open 3D controls",Description="Show Any2Odd",ApplicationPath=exe,Arguments="--show",IconResourcePath=exe,IconResourceIndex=0});
   list.JumpItemsRejected+=delegate(object s,JumpItemsRejectedEventArgs e){Files.Log("Taskbar menu rejected "+e.RejectedItems.Count+" items: "+string.Join(",",e.RejectionReasons));};
   JumpList.SetJumpList(shellApplication,list);list.Apply();Files.Log("Taskbar toggle and controls menu registered.");
  }catch(Exception e){Files.Log("Taskbar menu unavailable: "+e.Message);}}
 }
}
