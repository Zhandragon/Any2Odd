using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Collections.Generic;
class IconMaker {
 static Color Gold=Color.FromArgb(194,149,66),Shine=Color.FromArgb(248,222,146),Teal=Color.FromArgb(81,186,195),Ink=Color.FromArgb(18,27,31);
 static void Emblem(Graphics g,float x,float y,float size,bool small){var state=g.Save();g.TranslateTransform(x,y);g.ScaleTransform(size/256f,size/256f);g.SmoothingMode=SmoothingMode.AntiAlias;
  using(var dark=new SolidBrush(Ink))g.FillEllipse(dark,12,12,232,232);
  using(var ring=new Pen(Gold,small?12:6))g.DrawArc(ring,15,15,226,226,40,285);
  using(var shine=new Pen(Shine,small?3:2))g.DrawArc(shine,24,24,208,208,45,275);
  using(var teal=new Pen(Teal,small?11:6)){g.DrawArc(teal,32,32,192,192,225,125);g.DrawBezier(teal,204,190,218,197,230,190,240,177);}
  using(var gold=new SolidBrush(Shine))using(var cyan=new SolidBrush(Teal)){
   if(small){using(var f=new Font("Segoe UI",111,FontStyle.Bold,GraphicsUnit.Pixel))g.DrawString("+1",f,gold,new RectangleF(29,48,207,145),new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center});}
   else{using(var f=new Font("Cambria Math",96,FontStyle.Regular,GraphicsUnit.Pixel))g.DrawString("2n",f,gold,new RectangleF(40,42,176,104),new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center});using(var f=new Font("Cambria Math",68,FontStyle.Regular,GraphicsUnit.Pixel))g.DrawString("+1",f,cyan,new RectangleF(45,133,166,74),new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center});}
   g.FillEllipse(cyan,216,39,16,16);g.FillEllipse(gold,232,57,8,8);g.FillEllipse(gold,208,24,8,8);
  }
  if(!small)using(var gold=new Pen(Gold,2)){g.DrawBezier(gold,42,172,20,141,29,117,46,119);g.DrawArc(gold,34,112,17,17,50,280);g.DrawBezier(gold,194,78,224,94,217,122,201,120);g.DrawArc(gold,195,109,16,16,170,280);}
  g.Restore(state);
 }
 static void Wordmark(string folder){using(var b=new Bitmap(800,170,PixelFormat.Format32bppArgb))using(var g=Graphics.FromImage(b)){
  g.SmoothingMode=SmoothingMode.AntiAlias;g.TextRenderingHint=TextRenderingHint.AntiAliasGridFit;Emblem(g,4,17,134,false);
  using(var display=new Font("Copperplate Gothic Bold",12))using(var family=display.FontFamily)using(var text=new GraphicsPath()){
   text.AddString("Any2Odd",family,0,92,new PointF(147,15),StringFormat.GenericTypographic);var bounds=text.GetBounds();using(var fit=new Matrix()){fit.Translate(-bounds.X,-bounds.Y);fit.Scale(625/bounds.Width,85/bounds.Height,MatrixOrder.Append);fit.Translate(153,24,MatrixOrder.Append);text.Transform(fit);}
   using(var gradient=new LinearGradientBrush(new Rectangle(150,23,630,86),Shine,Gold,90)){gradient.InterpolationColors=new ColorBlend{Colors=new[]{Shine,Gold,Color.FromArgb(101,65,21),Color.FromArgb(228,190,108)},Positions=new[]{0f,.35f,.53f,1f}};using(var shadow=new Pen(Ink,5))g.DrawPath(shadow,text);g.FillPath(gradient,text);using(var edge=new Pen(Shine,1))g.DrawPath(edge,text);}
  }
  using(var gold=new Pen(Gold,1.4f))using(var shine=new Pen(Shine,.7f)){
   g.DrawLine(gold,154,120,312,120);g.DrawLine(gold,568,120,775,120);g.DrawBezier(gold,154,120,186,108,190,133,215,121);g.DrawBezier(shine,775,120,738,109,730,133,710,121);
   g.DrawArc(gold,141,112,13,13,10,290);g.DrawArc(gold,775,112,13,13,185,290);
  }
  using(var math=new Font("Cambria Math",26,FontStyle.Regular,GraphicsUnit.Pixel))using(var brush=new SolidBrush(Teal))g.DrawString("2n  ↦  2n + 1",math,brush,new RectangleF(310,113,259,48),new StringFormat{Alignment=StringAlignment.Center});
  b.Save(Path.Combine(folder,"Any2Odd-wordmark.png"),ImageFormat.Png);
 }}
 static int Main(string[] args){string folder=args[0];Directory.CreateDirectory(folder);var images=new List<byte[]>();int[] sizes={16,24,32,48,64,128,256};
  foreach(int size in sizes)using(var b=new Bitmap(size,size,PixelFormat.Format32bppArgb))using(var g=Graphics.FromImage(b)){
   Emblem(g,0,0,size,size<=24);
   using(var stream=new MemoryStream()){b.Save(stream,ImageFormat.Png);images.Add(stream.ToArray());}if(size==256)b.Save(Path.Combine(folder,"Any2Odd.png"),ImageFormat.Png);
  }
  using(var stream=File.Create(Path.Combine(folder,"Any2Odd.ico")))using(var writer=new BinaryWriter(stream)){
   writer.Write((ushort)0);writer.Write((ushort)1);writer.Write((ushort)sizes.Length);int offset=6+16*sizes.Length;
   for(int i=0;i<sizes.Length;i++){writer.Write((byte)(sizes[i]==256?0:sizes[i]));writer.Write((byte)(sizes[i]==256?0:sizes[i]));writer.Write((byte)0);writer.Write((byte)0);writer.Write((ushort)1);writer.Write((ushort)32);writer.Write(images[i].Length);writer.Write(offset);offset+=images[i].Length;}foreach(var png in images)writer.Write(png);
  }Wordmark(folder);return 0;
 }
}
