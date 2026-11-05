using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Collections.Generic;

// Original vector engraving: every accent stays inside the circular silhouette.
class IconMaker {
 static readonly Color Gold=Color.FromArgb(168,126,51),Light=Color.FromArgb(247,223,159),Shade=Color.FromArgb(86,58,22),Teal=Color.FromArgb(51,166,176),Ink=Color.FromArgb(20,32,36);
 static LinearGradientBrush Metal(RectangleF bounds){
  var brush=new LinearGradientBrush(bounds,Light,Gold,72f);
  brush.InterpolationColors=new ColorBlend{Colors=new[]{Light,Gold,Shade,Light,Gold},Positions=new[]{0f,.28f,.49f,.68f,1f}};return brush;
 }
 static GraphicsPath Letters(string text,RectangleF bounds){
  var path=new GraphicsPath();using(var family=new FontFamily("Palatino Linotype"))path.AddString(text,family,(int)FontStyle.Regular,120,PointF.Empty,StringFormat.GenericTypographic);
  var b=path.GetBounds();float scale=Math.Min(bounds.Width/b.Width,bounds.Height/b.Height);
  using(var fit=new Matrix()){fit.Translate(-b.X,-b.Y);fit.Scale(scale,scale,MatrixOrder.Append);fit.Translate(bounds.X+(bounds.Width-b.Width*scale)/2,bounds.Y+(bounds.Height-b.Height*scale)/2,MatrixOrder.Append);path.Transform(fit);}return path;
 }
 static void Engrave(Graphics g,GraphicsPath path,float edge,bool turquoise){
  var b=path.GetBounds();b.Inflate(edge*2,edge*2);
  using(var black=new Pen(Ink,edge*2.8f){LineJoin=LineJoin.Round})using(var metal=Metal(b))using(var gold=new Pen(metal,edge*2){LineJoin=LineJoin.Round})using(var inner=new Pen(Light,edge*.45f){LineJoin=LineJoin.Round})using(var face=new LinearGradientBrush(b,turquoise?Color.FromArgb(39,119,129):Color.FromArgb(53,69,69),Ink,90f)){
   g.DrawPath(black,path);g.DrawPath(gold,path);g.FillPath(face,path);g.DrawPath(inner,path);
  }
  var state=g.Save();g.SetClip(path,CombineMode.Intersect);
  using(var echo=(GraphicsPath)path.Clone())using(var offset=new Matrix())using(var pen=new Pen(Color.FromArgb(150,Light),edge*.55f)){
   offset.Translate(edge*1.1f,edge*1.1f);echo.Transform(offset);g.DrawPath(pen,echo);
  }g.Restore(state);
 }
 static void Gem(Graphics g,float x,float y,float radius){
  var points=new[]{new PointF(x,y-radius),new PointF(x+radius*.65f,y),new PointF(x,y+radius),new PointF(x-radius*.65f,y)};
  using(var p=new Pen(Shade,3))using(var gold=new Pen(Light,1))using(var fill=new LinearGradientBrush(new RectangleF(x-radius,y-radius,radius*2,radius*2),Light,Teal,55f)){g.FillPolygon(fill,points);g.DrawPolygon(p,points);g.DrawPolygon(gold,points);}
 }
 static void Scroll(Graphics g,float x,float y,float length,int direction){
  var state=g.Save();g.TranslateTransform(x,y);g.ScaleTransform(direction,1);
  using(var p=new Pen(Gold,1.7f))using(var light=new Pen(Light,.65f)){
   g.DrawBezier(p,0,0,length*.25f,-20,length*.62f,21,length,0);
   g.DrawBezier(light,0,2,length*.26f,-17,length*.62f,24,length,2);
   g.DrawBezier(p,length*.4f,-2,length*.2f,-28,length*.18f,-28,length*.2f,-8);
   g.DrawArc(p,length*.48f,-9,16,16,15,290);
   g.DrawBezier(p,length*.75f,2,length*.96f,26,length*1.08f,22,length,2);
  }g.Restore(state);
 }
 static void Emblem(Graphics g,float x,float y,float size,bool small){
  var state=g.Save();g.TranslateTransform(x,y);g.ScaleTransform(size/256f,size/256f);g.SmoothingMode=SmoothingMode.AntiAlias;
  using(var silhouette=new GraphicsPath()){
   silhouette.AddEllipse(7,7,242,242);g.SetClip(silhouette,CombineMode.Intersect);
   using(var fill=new LinearGradientBrush(new Rectangle(7,7,242,242),Color.FromArgb(47,63,64),Ink,120f))g.FillEllipse(fill,7,7,242,242);
   using(var metal=Metal(new RectangleF(8,8,240,240)))using(var rim=new Pen(metal,small?9:6))using(var black=new Pen(Ink,3))using(var light=new Pen(Light,1.3f))using(var teal=new Pen(Teal,1.4f)){
    g.DrawEllipse(rim,12,12,232,232);g.DrawEllipse(light,8,8,240,240);g.DrawEllipse(black,19,19,218,218);g.DrawEllipse(light,23,23,210,210);g.DrawEllipse(teal,31,31,194,194);
   }
   if(small){using(var text=Letters("+1",new RectangleF(40,67,176,120)))Engrave(g,text,5,true);}
   else{
    using(var ticks=new Pen(Gold,1.25f))for(int i=0;i<64;i++){
     double angle=i*Math.PI/32;float r=i%4==0?101:105;
     g.DrawLine(ticks,128+(float)Math.Cos(angle)*r,128+(float)Math.Sin(angle)*r,128+(float)Math.Cos(angle)*109,128+(float)Math.Sin(angle)*109);
    }
    using(var text=Letters("2n",new RectangleF(59,50,138,85)))Engrave(g,text,2.8f,false);
    using(var text=Letters("+1",new RectangleF(76,146,104,60)))Engrave(g,text,2.4f,true);
    using(var line=new Pen(Gold,1.4f)){g.DrawLine(line,66,140,116,140);g.DrawLine(line,140,140,190,140);}Gem(g,128,140,5);
    for(int side=-1;side<=1;side+=2){var scroll=g.Save();g.TranslateTransform(128,128);g.ScaleTransform(side,1);using(var p=new Pen(Gold,1.5f))using(var shine=new Pen(Light,.65f)){
     g.DrawBezier(p,72,-55,100,-20,85,4,74,12);g.DrawBezier(shine,75,-53,104,-20,88,4,77,14);g.DrawArc(p,70,-9,17,20,15,290);
     g.DrawBezier(p,73,28,72,53,52,70,35,73);g.DrawBezier(shine,77,26,76,56,54,75,35,77);
    }g.Restore(scroll);}
    Gem(g,128,27,8);Gem(g,128,229,8);Gem(g,27,128,6);Gem(g,229,128,6);
   }
  }g.Restore(state);
 }
 static void Wordmark(string folder){
  using(var b=new Bitmap(1400,250,PixelFormat.Format32bppArgb))using(var g=Graphics.FromImage(b)){
   g.SmoothingMode=SmoothingMode.AntiAlias;g.PixelOffsetMode=PixelOffsetMode.HighQuality;
   // An engraved mathematical seal, distinct from PortalAI's portal motif.
   Emblem(g,6,14,220,false);
   using(var line=new Pen(Gold,1.8f))using(var glint=new Pen(Light,.7f)){
    g.DrawLine(line,260,118,1362,118);g.DrawLine(glint,260,121,1362,121);
   }
   using(var letters=Letters("ANY2ODD",new RectangleF(269,35,1072,145)))Engrave(g,letters,3.2f,false);
   Gem(g,252,111,9);Gem(g,1367,111,9);
   Scroll(g,695,202,135,-1);Scroll(g,970,202,135,1);
   using(var math=Letters("2n  +  1",new RectangleF(736,184,193,40)))Engrave(g,math,1.1f,true);
   using(var p=new Pen(Gold,1.1f)){
    g.DrawLine(p,274,202,526,202);g.DrawLine(p,1140,202,1342,202);
    g.DrawArc(p,267,196,12,12,20,285);g.DrawArc(p,1336,196,12,12,180,285);
   }
   b.Save(Path.Combine(folder,"Any2Odd-wordmark.png"),ImageFormat.Png);
  }
 }
 static int Main(string[] args){
  string folder=args[0];Directory.CreateDirectory(folder);var images=new List<byte[]>();int[] sizes={16,24,32,48,64,128,256};
  foreach(int size in sizes)using(var large=new Bitmap(size*4,size*4,PixelFormat.Format32bppArgb))using(var g=Graphics.FromImage(large))using(var b=new Bitmap(size,size,PixelFormat.Format32bppArgb)){
   Emblem(g,0,0,size*4,size<=24);
   using(var target=Graphics.FromImage(b)){target.InterpolationMode=InterpolationMode.HighQualityBicubic;target.PixelOffsetMode=PixelOffsetMode.HighQuality;target.DrawImage(large,new Rectangle(0,0,size,size));}
   // Bicubic downsampling can leave faint ringing outside the small icon's rim.
   for(int y=0;y<size;y++)for(int x=0;x<size;x++){double dx=x+.5-size/2.0,dy=y+.5-size/2.0;if(dx*dx+dy*dy>size*size/4.0)b.SetPixel(x,y,Color.Transparent);}
   using(var stream=new MemoryStream()){b.Save(stream,ImageFormat.Png);images.Add(stream.ToArray());}if(size==256)b.Save(Path.Combine(folder,"Any2Odd.png"),ImageFormat.Png);
  }
  using(var stream=File.Create(Path.Combine(folder,"Any2Odd.ico")))using(var writer=new BinaryWriter(stream)){
   writer.Write((ushort)0);writer.Write((ushort)1);writer.Write((ushort)sizes.Length);int offset=6+16*sizes.Length;
   for(int i=0;i<sizes.Length;i++){writer.Write((byte)(sizes[i]==256?0:sizes[i]));writer.Write((byte)(sizes[i]==256?0:sizes[i]));writer.Write((byte)0);writer.Write((byte)0);writer.Write((ushort)1);writer.Write((ushort)32);writer.Write(images[i].Length);writer.Write(offset);offset+=images[i].Length;}foreach(var png in images)writer.Write(png);
  }Wordmark(folder);return 0;
 }
}
