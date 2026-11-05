using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OdysseyCompanion {
 // Theme/chrome are restricted to desktop controls, never capture/output surfaces.
 static class DesktopTheme {
  static readonly string[] Light={"#EEE7DC","#F8F3EA","#DED1BC","#202224","#625E59","#1F6F82","#A87D32","#E4CA88","#7B5A22","#E6DCCB","#D9CBB3"};
  static readonly string[] Night={"#111315","#1C1E20","#302D27","#F2ECE2","#B7AFA4","#7CC4D3","#B0863A","#D6BA70","#BC9451","#2A2D2F","#363A3D"};
  static Color[] colors=Palette(false);
  public static bool Dark{get;private set;}
  public static event Action Changed;
  static Color[] Palette(bool dark){return Array.ConvertAll(dark?Night:Light,ColorTranslator.FromHtml);}
  public static Color Background{get{return colors[0];}} public static Color Panel{get{return colors[1];}} public static Color Header{get{return colors[2];}} public static Color Text{get{return colors[3];}} public static Color Muted{get{return colors[4];}} public static Color Accent{get{return colors[5];}} public static Color Gold{get{return colors[6];}} public static Color Shine{get{return colors[7];}} public static Color Border{get{return colors[8];}} public static Color ButtonColor{get{return colors[9];}} public static Color Hover{get{return colors[10];}}
  public static readonly string ThemeFile=Path.Combine(Files.Data,"ui-settings.json");
  static DesktopTheme(){try{var saved=Files.Json.Deserialize<Dictionary<string,object>>(File.ReadAllText(ThemeFile));object value;if(saved.TryGetValue("DarkMode",out value)){Dark=Convert.ToBoolean(value);colors=Palette(Dark);}}catch{}}
  public static void Load(){try{var saved=Files.Json.Deserialize<Dictionary<string,object>>(File.ReadAllText(ThemeFile));object value;if(saved.TryGetValue("DarkMode",out value))SetDark(Convert.ToBoolean(value),false);}catch{}}
  public static void SetDark(bool dark,bool save){var old=colors;Dark=dark;colors=Palette(dark);foreach(Form form in Application.OpenForms)Recolor(form,old);if(Changed!=null)Changed();if(save)try{Files.Save(ThemeFile,new{DarkMode=dark});}catch(Exception e){Files.Log("Could not save desktop theme: "+e.Message);}}
  static Color Remap(Color value,Color[] old){for(int i=0;i<old.Length;i++)if(value.ToArgb()==old[i].ToArgb())return colors[i];return value;}
  public static void ApplyCurrent(Control control){Recolor(control,Palette(!Dark));}
  static void Recolor(Control control,Color[] old){control.BackColor=Remap(control.BackColor,old);control.ForeColor=Remap(control.ForeColor,old);if(control.ContextMenuStrip!=null){control.ContextMenuStrip.BackColor=Panel;control.ContextMenuStrip.ForeColor=Text;}foreach(Control child in control.Controls)Recolor(child,old);control.Invalidate(true);}
  public static Font Display(float size){return new Font("Copperplate Gothic Light",size,FontStyle.Regular);}
  public static Button Button(string text,EventHandler action){var b=new GoldButton{Text=text,AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,FlatStyle=FlatStyle.Flat,Padding=new Padding(13,7,13,7),BackColor=ButtonColor,ForeColor=Text,Font=Display(9),Margin=new Padding(0,4,8,4),Cursor=Cursors.Hand,UseVisualStyleBackColor=false};b.Click+=action;return b;}
  public static ContextMenuStrip Menu(){var menu=new ContextMenuStrip{Renderer=new GoldMenuRenderer(),BackColor=Panel,ForeColor=Text,Font=new Font("Segoe UI",10),ShowImageMargin=false,ShowCheckMargin=true,Padding=new Padding(9,10,9,10)};menu.Opening+=delegate{menu.BackColor=Panel;menu.ForeColor=Text;foreach(ToolStripItem item in menu.Items){item.ForeColor=Text;item.Padding=new Padding(7,4,7,4);}};menu.SizeChanged+=delegate{if(menu.Width>2&&menu.Height>2)using(var path=Round(new RectangleF(0,0,menu.Width,menu.Height),12)){var old=menu.Region;menu.Region=new Region(path);if(old!=null)old.Dispose();}};return menu;}
  public static void Open(string destination,IWin32Window owner){try{System.Diagnostics.Process.Start(destination);}catch(Exception e){MessageBox.Show(owner,"Could not open this item.\n\n"+e.Message,"Any2Odd",MessageBoxButtons.OK,MessageBoxIcon.Information);}}
  public static GraphicsPath Round(RectangleF r,float radius){var path=new GraphicsPath();radius=Math.Max(1,Math.Min(radius,Math.Min(r.Width,r.Height)/2));float d=radius*2;path.AddArc(r.X,r.Y,d,d,180,90);path.AddArc(r.Right-d,r.Y,d,d,270,90);path.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);path.AddArc(r.X,r.Bottom-d,d,d,90,90);path.CloseFigure();return path;}
  public static Color Blend(Color a,Color b,float amount){return Color.FromArgb((int)(a.R+(b.R-a.R)*amount),(int)(a.G+(b.G-a.G)*amount),(int)(a.B+(b.B-a.B)*amount));}
  public static void Flourish(Graphics g,float cx,float y,float span){using(var gold=new Pen(Gold,1))using(var shine=new Pen(Shine,1))using(var turquoise=new SolidBrush(Accent)){
   for(int side=-1;side<=1;side+=2){g.DrawBezier(gold,cx+side*7,y,cx+side*18,y+9,cx+side*32,y-9,cx+side*span,y);g.DrawBezier(shine,cx+side*14,y+2,cx+side*26,y+12,cx+side*39,y-4,cx+side*(span+10),y+2);g.DrawArc(gold,cx+side*20-5,y-5,10,10,side<0?20:160,270);}
   g.DrawEllipse(gold,cx-4,y-4,8,8);g.FillEllipse(turquoise,cx-2,y-2,4,4);
  }}
  public static void BorderLines(Graphics g,Rectangle r,bool flourish){if(r.Width<20||r.Height<20)return;g.SmoothingMode=SmoothingMode.AntiAlias;using(var path=Round(new RectangleF(r.X+1,r.Y+1,r.Width-3,r.Height-3),16))using(var inner=Round(new RectangleF(r.X+4,r.Y+4,r.Width-9,r.Height-9),13))using(var gold=new Pen(Gold,1.5f))using(var shine=new Pen(Shine,1)){g.DrawPath(gold,path);g.DrawPath(shine,inner);}
   if(flourish&&r.Width>210){Flourish(g,r.Left+r.Width/2f,r.Top+5,48);Flourish(g,r.Left+r.Width/2f,r.Bottom-6,30);using(var p=new Pen(Gold,1)){foreach(int side in new[]{-1,1}){float x=side<0?r.Left+12:r.Right-12;float y=r.Top+15;g.DrawBezier(p,x,y+22,x+side*6,y+6,x-side*22,y+10,x-side*30,y+1);g.DrawArc(p,x-6,y+12,12,12,side<0?0:180,250);}}}
  }
 }
 sealed class GoldButton:Button {
  bool hover,pressed;public bool Primary;
  public GoldButton(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);}
  protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}protected override void OnMouseLeave(EventArgs e){hover=false;pressed=false;Invalidate();base.OnMouseLeave(e);}protected override void OnMouseDown(MouseEventArgs e){pressed=true;Invalidate();base.OnMouseDown(e);}protected override void OnMouseUp(MouseEventArgs e){pressed=false;Invalidate();base.OnMouseUp(e);}
  protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(Parent==null?DesktopTheme.Background:Parent.BackColor);Color fill=Primary?Color.FromArgb(29,104,119):(hover?DesktopTheme.Hover:BackColor);if(pressed)fill=DesktopTheme.Blend(fill,DesktopTheme.Border,.15f);var r=new RectangleF(1,1,Width-3,Height-3);using(var path=DesktopTheme.Round(r,9))using(var gradient=new LinearGradientBrush(r,DesktopTheme.Blend(fill,Color.White,DesktopTheme.Dark?.03f:.18f),fill,90))using(var edge=new Pen(DesktopTheme.Gold,1)){g.FillPath(gradient,path);g.DrawPath(edge,path);}using(var inner=DesktopTheme.Round(new RectangleF(3,3,Width-7,Height-7),7))using(var shine=new Pen(Color.FromArgb(100,DesktopTheme.Shine)))g.DrawPath(shine,inner);
   TextRenderer.DrawText(g,Text,Font,new Rectangle(3,pressed?2:0,Width-6,Height),Enabled?(Primary?Color.FromArgb(250,245,230):ForeColor):DesktopTheme.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding|TextFormatFlags.NoPrefix);
   if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(g,new Rectangle(5,5,Width-10,Height-10),DesktopTheme.Accent,fill);
  }
 }
 sealed class GoldMenuRenderer:ToolStripProfessionalRenderer {
  public GoldMenuRenderer():base(new GoldMenuColors()){RoundedEdges=false;}
  protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e){DesktopTheme.BorderLines(e.Graphics,new Rectangle(Point.Empty,e.ToolStrip.Size),true);}
  protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e){float y=e.Item.Height/2f;using(var pen=new Pen(DesktopTheme.Gold)){e.Graphics.DrawLine(pen,12,y,e.Item.Width/2-10,y);e.Graphics.DrawLine(pen,e.Item.Width/2+10,y,e.Item.Width-12,y);}DesktopTheme.Flourish(e.Graphics,e.Item.Width/2,y,15);}
 }
 sealed class GoldMenuColors:ProfessionalColorTable {
  public override Color ToolStripDropDownBackground{get{return DesktopTheme.Panel;}}public override Color ImageMarginGradientBegin{get{return DesktopTheme.Panel;}}public override Color ImageMarginGradientMiddle{get{return DesktopTheme.Panel;}}public override Color ImageMarginGradientEnd{get{return DesktopTheme.Panel;}}public override Color MenuItemSelected{get{return DesktopTheme.Hover;}}public override Color MenuItemBorder{get{return DesktopTheme.Gold;}}public override Color MenuBorder{get{return DesktopTheme.Gold;}}
 }
 class DesktopForm:Form {
  protected readonly Panel Body=new Panel{Dock=DockStyle.Fill};
  protected readonly FlowLayoutPanel TitleActions=new FlowLayoutPanel{Dock=DockStyle.Right,AutoSize=true,FlowDirection=FlowDirection.LeftToRight,WrapContents=false,Padding=new Padding(0,4,8,0)};
  readonly Panel titlebar;readonly Label caption;readonly ToolTip tips=new ToolTip();
  [DllImport("user32.dll")]static extern bool ReleaseCapture();[DllImport("user32.dll")]static extern IntPtr SendMessage(IntPtr h,int message,IntPtr w,IntPtr l);
  protected int Dpi(int value){return (int)Math.Round(value*DeviceDpi/96.0);}
  public DesktopForm(){
   AutoScaleDimensions=new SizeF(96,96);AutoScaleMode=AutoScaleMode.Dpi;Font=new Font("Segoe UI",10);BackColor=DesktopTheme.Background;ForeColor=DesktopTheme.Text;FormBorderStyle=FormBorderStyle.None;Padding=new Padding(9);DoubleBuffered=true;StartPosition=FormStartPosition.CenterParent;MinimumSize=new Size(560,420);
   Icon=Icon.ExtractAssociatedIcon(typeof(DesktopForm).Assembly.Location);titlebar=new Panel{Dock=DockStyle.Top,Height=52,BackColor=DesktopTheme.Header};
   caption=new Label{Text="ANY2ODD",Font=DesktopTheme.Display(10),Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,Padding=new Padding(14,0,0,0),AutoEllipsis=true};
   titlebar.Controls.Add(caption);titlebar.Controls.Add(TitleActions);Controls.Add(Body);Controls.Add(titlebar);
   foreach(Control control in new Control[]{titlebar,caption}){control.MouseDown+=delegate(object sender,MouseEventArgs e){if(e.Button==MouseButtons.Left){ReleaseCapture();SendMessage(Handle,0xA1,new IntPtr(2),IntPtr.Zero);}};control.DoubleClick+=delegate{ToggleMaximize();};}
   TextChanged+=delegate{caption.Text=Text.ToUpperInvariant();};SizeChanged+=delegate{if(Width>20&&Height>20){using(var path=DesktopTheme.Round(new RectangleF(0,0,Width,Height),WindowState==FormWindowState.Maximized?1:Dpi(18))){var old=Region;Region=new Region(path);if(old!=null)old.Dispose();}}};
  }
  protected override void OnShown(EventArgs e){var area=Screen.FromControl(this).WorkingArea;MinimumSize=new Size(Math.Min(MinimumSize.Width,area.Width),Math.Min(MinimumSize.Height,area.Height));if(Width>area.Width||Height>area.Height){Size=new Size(Math.Min(Width,area.Width),Math.Min(Height,area.Height));Location=area.Location;}base.OnShown(e);}
  protected void ChromeButton(string text,string help,Action action){var b=DesktopTheme.Button(text,delegate{action();});b.Font=new Font("Segoe UI",11);b.AutoSize=false;b.Size=new Size(34,34);b.Padding=Padding.Empty;b.Margin=new Padding(3,4,0,0);b.AccessibleName=help;tips.SetToolTip(b,help);TitleActions.Controls.Add(b);}
  protected void StandardChrome(Action trayAction){ChromeButton("—","Minimize",delegate{WindowState=FormWindowState.Minimized;});ChromeButton("□","Maximize / restore",ToggleMaximize);if(trayAction!=null)ChromeButton("▾","Minimize to tray",trayAction);ChromeButton("×","Close",Close);}
  void ToggleMaximize(){if(!MaximizeBox)return;MaximizedBounds=Screen.FromControl(this).WorkingArea;WindowState=WindowState==FormWindowState.Maximized?FormWindowState.Normal:FormWindowState.Maximized;}
  protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);DesktopTheme.BorderLines(e.Graphics,ClientRectangle,true);}
  protected override void WndProc(ref Message m){base.WndProc(ref m);if(m.Msg==0x84&&WindowState==FormWindowState.Normal){long raw=m.LParam.ToInt64();var pt=PointToClient(new Point(unchecked((short)(raw&0xffff)),unchecked((short)((raw>>16)&0xffff))));int edge=Dpi(9);bool l=pt.X<edge,r=pt.X>=Width-edge,t=pt.Y<edge,b=pt.Y>=Height-edge;int hit=t?(l?13:r?14:12):b?(l?16:r?17:15):l?10:r?11:0;if(hit!=0)m.Result=new IntPtr(hit);}}
  protected override void Dispose(bool disposing){if(disposing)tips.Dispose();base.Dispose(disposing);}
 }
 sealed class SectionPanel:Panel {
  public SectionPanel(){BackColor=DesktopTheme.Panel;Padding=new Padding(20,20,20,18);Margin=new Padding(0,7,0,7);DoubleBuffered=true;AutoSize=true;AutoSizeMode=AutoSizeMode.GrowAndShrink;}
  protected override void OnPaintBackground(PaintEventArgs e){e.Graphics.Clear(Parent==null?DesktopTheme.Background:Parent.BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var path=DesktopTheme.Round(new RectangleF(1,1,Width-3,Height-3),16))using(var brush=new SolidBrush(BackColor))e.Graphics.FillPath(brush,path);}
  protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);DesktopTheme.BorderLines(e.Graphics,ClientRectangle,true);}
 }
 sealed class BrandBanner:Control {
  readonly Image wordmark;
  public BrandBanner(){Height=96;DoubleBuffered=true;using(var stream=typeof(BrandBanner).Assembly.GetManifestResourceStream("Any2Odd.Wordmark.png")){if(stream!=null)using(var image=Image.FromStream(stream))wordmark=new Bitmap(image);}AccessibleName="Any2Odd — 2n maps to 2n plus 1";}
  protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);if(wordmark!=null){float height=Math.Min(Height,100),width=wordmark.Width*height/wordmark.Height;e.Graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;e.Graphics.DrawImage(wordmark,new RectangleF(0,0,Math.Min(Width,width),height));}}
  protected override void Dispose(bool disposing){if(disposing&&wordmark!=null)wordmark.Dispose();base.Dispose(disposing);}
 }
}
