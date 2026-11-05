using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OdysseyCompanion {
 static class ProjectionDiagnostics {
  public static void Save(int parent,int worker){
   var rows=new List<object>();var screen=Screen.PrimaryScreen.Bounds;
   using(var destination=new Form()){
    foreach(var h in Win.Windows(parent,worker)){
     Win.Rect raw,frame; if(!Win.GetWindowRect(h,out raw))continue;
     int frameResult=Win.DwmGetFrameBounds(h,9,out frame,16);
     IntPtr thumb;var size=new Win.Size();
     int registered=Win.DwmRegisterThumbnail(destination.Handle,h,out thumb);
     if(registered==0){try{Win.DwmQueryThumbnailSourceSize(thumb,out size);}finally{Win.DwmUnregisterThumbnail(thumb);}}
     var oldClip=Rectangle.Intersect(screen,raw.Box);var newClip=Rectangle.Intersect(screen,frame.Box);
     rows.Add(new{handle=h.ToInt64(),windowClass=Win.Class(h),raw=raw.Box,visibleFrame=frame.Box,frameResult,thumbnailWidth=size.X,thumbnailHeight=size.Y,oldClip,newClip,oldIncluded=oldClip.Width>1&&oldClip.Height>1,visibleIncluded=frameResult==0&&newClip.Width>1&&newClip.Height>1});
    }
   }
   Files.Save(Path.Combine(Files.Data,"projection-diagnostics.json"),new{time=DateTime.Now.ToString("o"),monitors=Screen.AllScreens.Select(s=>new{name=s.DeviceName,bounds=s.Bounds,workingArea=s.WorkingArea,primary=s.Primary}).ToArray(),windows=rows});
  }
 }
}
