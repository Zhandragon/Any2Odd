using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OdysseyCompanion {
 // A desktop-only viewport. It never creates a Windows white/native scrollbar.
 sealed class ScrollSurface:Panel {
  readonly Panel viewport=new Panel{Dock=DockStyle.Fill};readonly GoldScrollBar scrollbar=new GoldScrollBar{Dock=DockStyle.Right,Width=17};
  Control content;bool arranging;int offset;
  public ScrollSurface(){DoubleBuffered=true;BackColor=DesktopTheme.Background;Controls.Add(viewport);Controls.Add(scrollbar);scrollbar.ValueChanged+=delegate{offset=scrollbar.Value;if(content!=null)content.Top=-offset;};viewport.SizeChanged+=delegate{Arrange();};}
  public void SetContent(Control control){content=control;control.Dock=DockStyle.None;control.Margin=Padding.Empty;viewport.Controls.Add(control);control.SizeChanged+=delegate{Arrange();};HookWheel(control);Arrange();}
  void HookWheel(Control c){if(!(c is TrackBar)&&!(c is ComboBox)&&!(c is TextBoxBase)){c.MouseWheel+=delegate(object s,MouseEventArgs e){var handled=e as HandledMouseEventArgs;if(handled!=null&&handled.Handled)return;ScrollBy(-e.Delta/120*60);if(handled!=null)handled.Handled=true;};}foreach(Control child in c.Controls)HookWheel(child);c.ControlAdded+=delegate(object s,ControlEventArgs e){HookWheel(e.Control);};}
  public int ScrollOffset{get{return offset;}}
  public void ScrollBy(int pixels){scrollbar.Value=offset+pixels;}
  public void ResetScroll(){scrollbar.Value=0;}
  public void Arrange(){if(arranging||content==null||viewport.Width<2)return;arranging=true;try{content.Width=viewport.Width;content.PerformLayout();int height=content.GetPreferredSize(new Size(viewport.Width,0)).Height;content.Height=Math.Max(height,1);scrollbar.SetRange(content.Height,viewport.Height);offset=scrollbar.Value;content.Location=new Point(0,-offset);}finally{arranging=false;}}
  protected override void OnLayout(LayoutEventArgs e){base.OnLayout(e);Arrange();}
 }
 sealed class GoldScrollBar:Control {
  int content=1,viewport=1,value,dragOrigin,dragValue;bool dragging;
  public event Action ValueChanged;
  public int Value{get{return value;}set{int next=Math.Max(0,Math.Min(Math.Max(0,content-viewport),value));if(this.value==next)return;this.value=next;Invalidate();if(ValueChanged!=null)ValueChanged();}}
  public GoldScrollBar(){DoubleBuffered=true;SetStyle(ControlStyles.Selectable,true);TabStop=true;AccessibleRole=AccessibleRole.ScrollBar;AccessibleName="Page scroll";BackColor=DesktopTheme.Background;Cursor=Cursors.Hand;}
  public void SetRange(int total,int visible){content=Math.Max(1,total);viewport=Math.Max(1,visible);Enabled=content>viewport;Value=value;Invalidate();}
  Rectangle Thumb(){int track=Math.Max(1,Height-24);int h=Math.Min(track,Math.Max(32,(int)(track*Math.Min(1,viewport/(double)content))));int y=12+(content>viewport?(int)((track-h)*value/(double)(content-viewport)):0);return new Rectangle(4,y,Math.Max(6,Width-8),h);}
  protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(DesktopTheme.Background);if(!Enabled)return;var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;using(var track=new Pen(DesktopTheme.Border,1))g.DrawLine(track,Width/2,10,Width/2,Height-10);var r=Thumb();using(var path=DesktopTheme.Round(r,4))using(var fill=new LinearGradientBrush(r,DesktopTheme.Shine,DesktopTheme.Gold,0f))using(var pen=new Pen(DesktopTheme.Border)){g.FillPath(fill,path);g.DrawPath(pen,path);}using(var p=new Pen(DesktopTheme.Accent,1)){int cy=r.Top+r.Height/2;g.DrawEllipse(p,r.X+2,cy-3,r.Width-4,6);g.DrawLine(p,Width/2,cy-9,Width/2,cy-5);g.DrawLine(p,Width/2,cy+5,Width/2,cy+9);}}
  protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);Focus();if(Thumb().Contains(e.Location)){dragging=true;dragOrigin=e.Y;dragValue=value;Capture=true;}else Value+=e.Y<Thumb().Top?-viewport:viewport;}
  protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(dragging){int travel=Height-24-Thumb().Height;if(travel>0)Value=dragValue+(int)((e.Y-dragOrigin)*(content-viewport)/(double)travel);}}
  protected override void OnMouseUp(MouseEventArgs e){dragging=false;Capture=false;base.OnMouseUp(e);}
  protected override bool IsInputKey(Keys key){return key==Keys.Up||key==Keys.Down||key==Keys.PageUp||key==Keys.PageDown||key==Keys.Home||key==Keys.End||base.IsInputKey(key);}
  protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Up)Value-=40;else if(e.KeyCode==Keys.Down)Value+=40;else if(e.KeyCode==Keys.PageUp)Value-=viewport;else if(e.KeyCode==Keys.PageDown)Value+=viewport;else if(e.KeyCode==Keys.Home)Value=0;else if(e.KeyCode==Keys.End)Value=content;else{base.OnKeyDown(e);return;}e.Handled=true;}
 }
 sealed class GoldComboBox:ComboBox {
  [DllImport("uxtheme.dll",CharSet=CharSet.Unicode)]static extern int SetWindowTheme(IntPtr hwnd,string app,string id);
  public GoldComboBox(){DrawMode=DrawMode.OwnerDrawFixed;FlatStyle=FlatStyle.Flat;ItemHeight=25;DropDownStyle=ComboBoxStyle.DropDownList;}
  protected override void OnHandleCreated(EventArgs e){base.OnHandleCreated(e);SetWindowTheme(Handle,"","");}
  void PaintSelection(Graphics g){g.SmoothingMode=SmoothingMode.AntiAlias;using(var background=new SolidBrush(Parent==null?DesktopTheme.Panel:Parent.BackColor))g.FillRectangle(background,ClientRectangle);using(var path=DesktopTheme.Round(new RectangleF(1,1,Width-3,Height-3),7))using(var fill=new SolidBrush(DesktopTheme.Panel))using(var edge=new Pen(DesktopTheme.Gold)){g.FillPath(fill,path);g.DrawPath(edge,path);}TextRenderer.DrawText(g,Text,Font,new Rectangle(9,0,Math.Max(1,Width-35),Height),Enabled?DesktopTheme.Text:DesktopTheme.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);using(var arrow=new SolidBrush(Enabled?DesktopTheme.Accent:DesktopTheme.Muted))g.FillPolygon(arrow,new[]{new Point(Width-21,Height/2-2),new Point(Width-13,Height/2-2),new Point(Width-17,Height/2+3)});}
  protected override void WndProc(ref Message m){base.WndProc(ref m);if(m.Msg==0xF&&IsHandleCreated){using(var g=Graphics.FromHwnd(Handle))PaintSelection(g);}else if((m.Msg==0x317||m.Msg==0x318)&&m.WParam!=IntPtr.Zero){using(var g=Graphics.FromHdc(m.WParam))PaintSelection(g);}}
  protected override void OnDrawItem(DrawItemEventArgs e){if(e.Bounds.Width<1)return;bool selected=(e.State&DrawItemState.Selected)!=0;using(var bg=new SolidBrush(selected?DesktopTheme.Header:DesktopTheme.Panel))e.Graphics.FillRectangle(bg,e.Bounds);string text=e.Index>=0?GetItemText(Items[e.Index]):Text;TextRenderer.DrawText(e.Graphics,text,Font,new Rectangle(e.Bounds.X+7,e.Bounds.Y,e.Bounds.Width-8,e.Bounds.Height),Enabled?DesktopTheme.Text:DesktopTheme.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);e.DrawFocusRectangle();}
 }
 sealed class GoldTrackBar:TrackBar {
  public GoldTrackBar(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);ValueChanged+=delegate{Invalidate();};}
  protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(Parent==null?DesktopTheme.Panel:Parent.BackColor);g.SmoothingMode=SmoothingMode.AntiAlias;float left=13,right=Width-14,cy=Height/2f-2;using(var track=new Pen(DesktopTheme.Border,2))g.DrawLine(track,left,cy,right,cy);float x=left+(right-left)*(Value-Minimum)/(float)Math.Max(1,Maximum-Minimum);using(var fill=new Pen(DesktopTheme.Accent,3))g.DrawLine(fill,left,cy,x,cy);using(var brush=new SolidBrush(DesktopTheme.Header))using(var pen=new Pen(DesktopTheme.Gold,1.5f)){g.FillEllipse(brush,x-6,cy-7,12,14);g.DrawEllipse(pen,x-6,cy-7,12,14);}using(var gem=new SolidBrush(DesktopTheme.Accent))g.FillEllipse(gem,x-2,cy-3,4,6);if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(g,new Rectangle(2,2,Width-4,Height-4));}
 }
}
