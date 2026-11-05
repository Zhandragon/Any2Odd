using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OdysseyCompanion {
 sealed class DonationQr:PictureBox {
  public string SourceName;
  protected override void OnPaint(PaintEventArgs e){
   e.Graphics.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;e.Graphics.Clear(Parent.BackColor);
   using(var white=new SolidBrush(Color.White))using(var rounded=DesktopTheme.Round(new RectangleF(0,0,Width-1,Height-1),12))e.Graphics.FillPath(white,rounded);
   if(Image==null)return;Rectangle source=new Rectangle(0,0,Image.Width,Image.Height);
   if(SourceName=="ethereum.png"&&Image.Width==217)source=new Rectangle(26,27,165,165);
   else if(SourceName=="polygon.png"&&Image.Width==215)source=new Rectangle(24,26,165,165);
   else if(SourceName=="bitcoin.png"&&Image.Width==220)source=new Rectangle(26,26,165,165);
   if(source.Width==165){int scale=Math.Max(1,Math.Min(Width-36,Height-36)/165);int size=165*scale;e.Graphics.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;e.Graphics.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.Half;e.Graphics.DrawImage(Image,new Rectangle((Width-size)/2,(Height-size)/2,size,size),source,GraphicsUnit.Pixel);}
   else{e.Graphics.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;e.Graphics.DrawImage(Image,new Rectangle(10,10,Width-20,Height-20),source,GraphicsUnit.Pixel);}
  }
 }
 sealed class DonationDestination {
  public readonly string Name,Network,File,Value;
  public DonationDestination(string name,string network,string file,string value){Name=name;Network=network;File=file;Value=value;}
 }
 sealed class DonationPanel:Panel {
  internal static readonly DonationDestination[] Destinations={
   new DonationDestination("PayPal","Personal donation link","paypal.png","https://www.paypal.com/qrcodes/managed/25c71b49-6280-4db1-a983-4db5f53183eb?utm_source=consweb_more"),
   new DonationDestination("Ethereum","Ethereum network · ETH","ethereum.png","0x9C4D852279801fB4379b11698fD5835Fe275307D"),
   new DonationDestination("Polygon","Polygon network · POL","polygon.png","0x9C4D852279801fB4379b11698fD5835Fe275307D"),
   new DonationDestination("Bitcoin","Bitcoin network · BTC","bitcoin.png","bc1q24y4u2xe5pzk9pg3q7qyjty8vhxvp4nskunqle")
  };
  readonly Label status;readonly System.Collections.Generic.List<Image> images=new System.Collections.Generic.List<Image>();
  public DonationPanel(Action goBack){Dock=DockStyle.Fill;BackColor=DesktopTheme.Background;ForeColor=DesktopTheme.Text;var scroll=new ScrollSurface{Dock=DockStyle.Fill,Padding=new Padding(20,12,12,20)};Controls.Add(scroll);
   var layout=new TableLayoutPanel{AutoSize=true,Dock=DockStyle.Top,ColumnCount=1};layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));layout.Controls.Add(DesktopTheme.Button("‹  Controls",delegate{goBack();}));layout.Controls.Add(new BrandBanner{Dock=DockStyle.Top});
   layout.Controls.Add(new Label{Text="Support Any2Odd",Font=DesktopTheme.Display(22),ForeColor=DesktopTheme.Accent,AutoSize=true,Margin=new Padding(0,0,0,8)});
   var intro=new Label{Text="If Any2Odd is useful to you, consider supporting its continued development. These are the same maintainer donation destinations used by PortalAI.",AutoSize=true,Margin=new Padding(0,0,0,8)};layout.Controls.Add(intro);layout.SizeChanged+=delegate{intro.MaximumSize=new Size(Math.Max(200,layout.Width-10),0);};
   layout.Controls.Add(DesktopTheme.Button("Sponsor on GitHub",delegate{DesktopTheme.Open("https://github.com/sponsors/Zhandragon",this);}));
   status=new Label{Text="Scan a code, or click its code/button to copy the destination.",AutoSize=true,ForeColor=DesktopTheme.Accent,Margin=new Padding(0,8,0,8)};layout.Controls.Add(status);
   var grid=new TableLayoutPanel{AutoSize=true,Dock=DockStyle.Fill,ColumnCount=2};grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));layout.Controls.Add(grid);
   for(int i=0;i<Destinations.Length;i++){var destination=Destinations[i];var card=new SectionPanel{Dock=DockStyle.Fill,Margin=new Padding(5),Padding=new Padding(18,22,18,16)};var stack=new TableLayoutPanel{AutoSize=true,Dock=DockStyle.Top,ColumnCount=1};stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));card.Controls.Add(stack);
    stack.Controls.Add(new Label{Text=destination.Name,Font=DesktopTheme.Display(14),AutoSize=true,ForeColor=DesktopTheme.Accent});stack.Controls.Add(new Label{Text=destination.Network,AutoSize=true,ForeColor=DesktopTheme.Muted,Margin=new Padding(0,2,0,8)});
    Image qr;using(var stream=typeof(DonationPanel).Assembly.GetManifestResourceStream("Any2Odd.Donations."+destination.File)){if(stream==null)throw new FileNotFoundException("Missing donation QR resource: "+destination.File);using(var original=Image.FromStream(stream))qr=new Bitmap(original);}images.Add(qr);
    var picture=new DonationQr{SourceName=destination.File,Image=qr,SizeMode=PictureBoxSizeMode.Zoom,Size=new Size(230,230),BackColor=Color.White,Cursor=Cursors.Hand,Anchor=AnchorStyles.None,Margin=new Padding(4,2,4,8),AccessibleName=destination.Name+" donation QR code"};picture.Click+=delegate{CopyDestination(destination);};stack.Controls.Add(picture);
    var address=new TextBox{Text=destination.Name=="PayPal"?"paypal.com · personal donation link":destination.Value,ReadOnly=true,Multiline=true,WordWrap=true,Height=44,Dock=DockStyle.Fill,BackColor=DesktopTheme.Panel,ForeColor=DesktopTheme.Muted,BorderStyle=BorderStyle.None,Font=new Font("Segoe UI",9),AccessibleName=destination.Name+" destination"};stack.Controls.Add(address);
    var copy=DesktopTheme.Button(destination.Name=="PayPal"?"Copy PayPal link":"Copy address",delegate{CopyDestination(destination);});copy.AccessibleName="Copy "+destination.Name+" destination";stack.Controls.Add(copy);grid.Controls.Add(card,i%2,i/2);
   }
   scroll.SetContent(layout);
  }
  void CopyDestination(DonationDestination destination){try{Clipboard.SetText(destination.Value);status.Text=destination.Name+" destination copied to clipboard.";}catch(System.Runtime.InteropServices.ExternalException){status.Text="Clipboard is busy. Please try copying again.";}}
  protected override void Dispose(bool disposing){if(disposing)foreach(var image in images)image.Dispose();base.Dispose(disposing);}
 }
}
