using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
class IconMaker {
 static GraphicsPath Round(float x,float y,float w,float h,float radius){var p=new GraphicsPath();p.AddArc(x,y,radius*2,radius*2,180,90);p.AddArc(x+w-radius*2,y,radius*2,radius*2,270,90);p.AddArc(x+w-radius*2,y+h-radius*2,radius*2,radius*2,0,90);p.AddArc(x,y+h-radius*2,radius*2,radius*2,90,90);p.CloseFigure();return p;}
 static int Main(string[] args){string folder=args[0];Directory.CreateDirectory(folder);var images=new List<byte[]>();int[] sizes={16,24,32,48,64,128,256};
  foreach(int size in sizes)using(var b=new Bitmap(size,size,PixelFormat.Format32bppArgb))using(var g=Graphics.FromImage(b)){
   g.SmoothingMode=SmoothingMode.AntiAlias;g.ScaleTransform(size/256f,size/256f);
   using(var dark=new SolidBrush(Color.FromArgb(9,19,37)))using(var yellow=new SolidBrush(Color.FromArgb(255,222,35)))using(var cyan=new SolidBrush(Color.FromArgb(0,235,255)))using(var white=new SolidBrush(Color.White)){
    using(var outer=Round(4,4,248,248,49))g.FillPath(dark,outer);
    using(var inner=Round(16,16,224,224,38))g.FillPath(yellow,inner);
    // Broad, simple stereo glasses remain legible at tray-icon size.
    using(var frame=Round(30,72,196,120,26))g.FillPath(dark,frame);
    using(var left=Round(46,89,65,83,14))g.FillPath(cyan,left);
    using(var right=Round(145,89,65,83,14))g.FillPath(white,right);
    g.FillRectangle(dark,111,130,34,66);
    g.FillRectangle(yellow,112,158,32,36);
    g.FillRectangle(white,51,96,12,42);
   }
   using(var stream=new MemoryStream()){b.Save(stream,ImageFormat.Png);images.Add(stream.ToArray());}
   if(size==256)b.Save(Path.Combine(folder,"Any2Odd.png"),ImageFormat.Png);
  }
  using(var stream=File.Create(Path.Combine(folder,"Any2Odd.ico")))using(var writer=new BinaryWriter(stream)){
   writer.Write((ushort)0);writer.Write((ushort)1);writer.Write((ushort)sizes.Length);int offset=6+16*sizes.Length;
   for(int i=0;i<sizes.Length;i++){writer.Write((byte)(sizes[i]==256?0:sizes[i]));writer.Write((byte)(sizes[i]==256?0:sizes[i]));writer.Write((byte)0);writer.Write((byte)0);writer.Write((ushort)1);writer.Write((ushort)32);writer.Write(images[i].Length);writer.Write(offset);offset+=images[i].Length;}
   foreach(var png in images)writer.Write(png);
  }return 0;
 }
}
