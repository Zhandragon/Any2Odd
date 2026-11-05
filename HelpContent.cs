using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OdysseyCompanion {
 sealed class HelpLink {public string Label{get;set;}public string Url{get;set;}}
 sealed class HelpTopic {
  public string Id{get;set;}public string Title{get;set;}public string Body{get;set;}
  public HelpLink[] Links{get;set;}
  public override string ToString(){return Title;}
 }
 static class HelpContent {
  public static readonly HelpTopic[] Topics=Load();
  static HelpTopic[] Load(){using(var stream=typeof(HelpContent).Assembly.GetManifestResourceStream("Any2Odd.Help.json"))using(var reader=new StreamReader(stream)){return Files.Json.Deserialize<HelpTopic[]>(reader.ReadToEnd());}}
 }
 sealed class HelpPanel:Panel {
  readonly ListBox topics;readonly Label article,title;readonly ScrollSurface articleScroll;
  public HelpPanel(Action goBack){Dock=DockStyle.Fill;BackColor=DesktopTheme.Background;ForeColor=DesktopTheme.Text;
   var back=DesktopTheme.Button("‹  Controls",delegate{goBack();});var header=new FlowLayoutPanel{Dock=DockStyle.Top,AutoSize=true,Padding=new Padding(18,8,18,0)};header.Controls.Add(back);
   var layout=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(18,10,10,18),ColumnCount=2,RowCount=1};layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,29));layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,71));Controls.Add(layout);Controls.Add(header);
   topics=new ListBox{Dock=DockStyle.Fill,BackColor=DesktopTheme.Panel,ForeColor=DesktopTheme.Text,BorderStyle=BorderStyle.None,IntegralHeight=false,ItemHeight=34,DrawMode=DrawMode.OwnerDrawFixed,AccessibleName="Help topics"};
   topics.DrawItem+=delegate(object s,DrawItemEventArgs e){if(e.Index<0)return;bool chosen=(e.State&DrawItemState.Selected)!=0;using(var brush=new SolidBrush(chosen?DesktopTheme.Header:DesktopTheme.Panel))e.Graphics.FillRectangle(brush,e.Bounds);TextRenderer.DrawText(e.Graphics,topics.Items[e.Index].ToString(),topics.Font,new Rectangle(e.Bounds.X+8,e.Bounds.Y,e.Bounds.Width-12,e.Bounds.Height),DesktopTheme.Text,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);e.DrawFocusRectangle();};
   articleScroll=new ScrollSurface{Dock=DockStyle.Fill,Margin=new Padding(14,0,0,0)};
   var content=new TableLayoutPanel{AutoSize=true,ColumnCount=1,Padding=new Padding(10,6,14,18),BackColor=DesktopTheme.Panel};content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   title=new Label{AutoSize=true,Font=DesktopTheme.Display(17),ForeColor=DesktopTheme.Accent,UseMnemonic=false,Margin=new Padding(0,0,0,18)};
   article=new Label{AutoSize=true,BackColor=DesktopTheme.Panel,ForeColor=DesktopTheme.Text,Font=new Font("Segoe UI",11),UseMnemonic=false,AccessibleName="Help article",Margin=Padding.Empty};content.Controls.Add(title);content.Controls.Add(article);content.SizeChanged+=delegate{article.MaximumSize=new Size(Math.Max(150,content.Width-28),0);title.MaximumSize=article.MaximumSize;};articleScroll.SetContent(content);
   layout.Controls.Add(topics,0,0);layout.Controls.Add(articleScroll,1,0);topics.Items.AddRange(HelpContent.Topics);topics.SelectedIndexChanged+=delegate{var topic=(HelpTopic)topics.SelectedItem;title.Text=topic.Title;article.Text=topic.Body;articleScroll.ResetScroll();articleScroll.Arrange();};topics.SelectedIndex=0;
  }
  public void SelectTopic(string id){for(int i=0;i<HelpContent.Topics.Length;i++)if(HelpContent.Topics[i].Id==id){topics.SelectedIndex=i;return;}}
 }
}
