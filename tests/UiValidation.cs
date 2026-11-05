using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// Runs against the real release assembly with injected, unsaved preferences.
class UiValidation {
 [DllImport("user32.dll")]static extern bool SetProcessDpiAwarenessContext(IntPtr context);
 static BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 static object Field(object target,string name){return target.GetType().GetField(name,Flags).GetValue(target);}
 static object Invoke(object target,string name,params object[] args){return target.GetType().GetMethod(name,Flags).Invoke(target,args);}
 static void Check(bool condition,string message){if(!condition)throw new Exception(message);Console.WriteLine("PASS: "+message);}
 static System.Collections.Generic.IEnumerable<Control> Descendants(Control parent){foreach(Control c in parent.Controls){yield return c;foreach(var child in Descendants(c))yield return child;}}
 static void RenderOffscreen(Form form,Assembly assembly){
  var host=new UserControl{Size=new Size(850,940)};form.TopLevel=false;form.TopMost=false;form.Dock=DockStyle.Fill;host.Controls.Add(form);host.CreateControl();form.Show();
  var windows=(ComboBox)Field(form,"windows");windows.Items.Clear();windows.Items.Add("Choose a window in Selected window mode");windows.SelectedIndex=0;
  var theme=assembly.GetType("OdysseyCompanion.DesktopTheme");var setDark=theme.GetMethod("SetDark",BindingFlags.Static|BindingFlags.Public);var body=(Control)Field(form,"Body");var controls=(Control)Field(form,"controlsPage");
  Action back=delegate{};var donate=(Panel)Activator.CreateInstance(assembly.GetType("OdysseyCompanion.DonationPanel"),Flags,null,new object[]{back},null);body.Controls.Add(donate);donate.Visible=false;
  foreach(bool dark in new[]{false,true}){setDark.Invoke(null,new object[]{dark,false});
   foreach(bool donation in new[]{false,true}){controls.Visible=!donation;donate.Visible=donation;(donation?(Control)donate:controls).BringToFront();form.Size=new Size(850,940);form.PerformLayout();var handle=form.Handle;foreach(Control c in Descendants(form).Where(c=>c.GetType().Name=="ScrollSurface"))Invoke(c,"Arrange");
    using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(Point.Empty,form.Size));bitmap.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,(donation?"donate-":"controls-")+(dark?"dark":"light")+".png"));}
   }
  }
  Check(!form.TopLevel&&form.Parent==host,"Artwork renders in a child control host, without a desktop window");host.Dispose();
 }
 static void ValidateHeadless(Form form,Assembly assembly){
  Check(!form.Visible,"Headless UI checks do not show a desktop window");
  Check(Field(form,"worker")==null,"UI construction does not create a renderer");
  var depth=(TrackBar)Field(form,"depth");var pop=(TrackBar)Field(form,"pop");Check(depth.Minimum==0&&depth.Maximum==200&&pop.Minimum==-100&&pop.Maximum==100,"Original tuning ranges retained");depth.Value=133;pop.Value=-25;Check(((Label)Field(form,"depthValue")).Text=="1.33"&&((Label)Field(form,"popValue")).Text=="-0.25","Themed sliders preserve value-change behavior");
  var menu=(ContextMenuStrip)Field(form,"appMenu");Check(menu.Items.Cast<ToolStripItem>().Any(i=>i.Text=="Toggle light / dark mode"),"Theme selection is available from the tray menu");
  var theme=assembly.GetType("OdysseyCompanion.DesktopTheme");bool initial=(bool)theme.GetProperty("Dark").GetValue(null,null);Invoke(form,"ToggleTheme");Check((bool)theme.GetProperty("Dark").GetValue(null,null)!=initial,"Light/dark selection switches the palette");Check(form.BackColor==(Color)theme.GetProperty("Background").GetValue(null,null),"Theme updates the existing controls window");Invoke(form,"ToggleTheme");
  var themeFile=theme.GetField("ThemeFile",BindingFlags.Public|BindingFlags.Static);string original=(string)themeFile.GetValue(null);string saved=Path.Combine(Path.GetTempPath(),"Any2Odd-theme-test-"+Guid.NewGuid().ToString("N")+".json");try{themeFile.SetValue(null,saved);theme.GetMethod("SetDark").Invoke(null,new object[]{true,true});Check(File.ReadAllText(saved).Contains("true"),"Dark mode preference persists independently of conversion settings");theme.GetMethod("SetDark").Invoke(null,new object[]{false,false});theme.GetMethod("Load").Invoke(null,null);Check((bool)theme.GetProperty("Dark").GetValue(null,null),"Saved theme can be loaded again");}finally{themeFile.SetValue(null,original);if(File.Exists(saved))File.Delete(saved);theme.GetMethod("SetDark").Invoke(null,new object[]{initial,false});}
  Action back=delegate{};using(var donate=(Panel)Activator.CreateInstance(assembly.GetType("OdysseyCompanion.DonationPanel"),Flags,null,new object[]{back},null)){
   donate.Size=new Size(820,800);donate.PerformLayout();Check(Descendants(donate).OfType<PictureBox>().Count(p=>p.Image!=null)==4,"Internal Donate loads all four original QR resources");Check(!typeof(Form).IsAssignableFrom(donate.GetType()),"Donate is a panel, not a popup window");
   var scroll=Descendants(donate).Single(c=>c.GetType().Name=="ScrollSurface");Invoke(scroll,"Arrange");Invoke(scroll,"ScrollBy",120);Check((int)scroll.GetType().GetProperty("ScrollOffset").GetValue(scroll,null)>0,"Donation content scrolls with the themed scrollbar");
  }
  using(var help=(Panel)Activator.CreateInstance(assembly.GetType("OdysseyCompanion.HelpPanel"),Flags,null,new object[]{back},null)){
   help.Size=new Size(820,800);help.PerformLayout();Invoke(help,"SelectTopic","overlay");Check(Descendants(help).OfType<Label>().Single(c=>c.AccessibleName=="Help article").Text.Contains("passes mouse input through"),"Internal Help selects accurate overlay documentation");
   var catalog=(Array)assembly.GetType("OdysseyCompanion.HelpContent").GetField("Topics",BindingFlags.Static|BindingFlags.Public).GetValue(null);
   foreach(var topic in catalog){string id=(string)topic.GetType().GetProperty("Id").GetValue(topic,null);Invoke(help,"SelectTopic",id);Check(Descendants(help).OfType<Label>().Single(c=>c.AccessibleName=="Help article").Text.Length>100,"Written Help loads: "+id);}
   Check(!Descendants(help).OfType<PictureBox>().Any(),"In-app Help remains text-only");
  }
  foreach(var size in new[]{new Size(850,940),new Size(700,580),new Size(1200,900)}){form.Size=size;form.PerformLayout();Invoke(Field(form,"controlsPage"),"Arrange");Check(Descendants(form).OfType<ComboBox>().All(c=>c.Width>180),"Controls fit desktop width "+size.Width);}
  Check(!Descendants(form).OfType<ScrollableControl>().Any(c=>c.AutoScroll),"Desktop pages do not use native white scrollbars");
  Check(assembly.GetManifestResourceNames().Contains("Any2Odd.Wordmark.png"),"Mathematical wordmark is embedded in the release");
  Check(Field(form,"worker")==null,"Theme, page construction and layout checks leave rendering untouched");
 }
 [STAThread]static int Main(string[] args){
  try{
   SetProcessDpiAwarenessContext(new IntPtr(-4));Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
   var assembly=Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Any2Odd.exe"));var type=assembly.GetType("OdysseyCompanion.MainForm");var preferences=Activator.CreateInstance(assembly.GetType("OdysseyCompanion.Preferences"));preferences.GetType().GetField("AutoConvert").SetValue(preferences,false);
   var form=(Form)Activator.CreateInstance(type,Flags,null,new[]{preferences,(object)true},null);
   if(args.Contains("--preview")){Application.Run(form);return 0;}
   UserControl testHost=null;if(!args.Contains("--headless")&&!args.Contains("--render")){testHost=new UserControl{Size=new Size(1000,1000)};form.TopLevel=false;form.TopMost=false;form.Dock=DockStyle.None;testHost.Controls.Add(form);testHost.CreateControl();}
   if(args.Contains("--render")){RenderOffscreen(form,assembly);form.Dispose();return 0;}
   if(args.Contains("--headless")){ValidateHeadless(form,assembly);form.Dispose();return 0;}
   form.Show();Application.DoEvents();
   Check(form.FormBorderStyle==FormBorderStyle.None,"Custom desktop chrome is active");
   Check(Field(form,"worker")==null,"Showing controls does not start a renderer in validation mode");
   var depth=(TrackBar)Field(form,"depth");var pop=(TrackBar)Field(form,"pop");Check(depth.Minimum==0&&depth.Maximum==200&&pop.Minimum==-100&&pop.Maximum==100,"Original depth and pop-out ranges retained");
   depth.Value=133;pop.Value=-25;Check(((Label)Field(form,"depthValue")).Text=="1.33"&&((Label)Field(form,"popValue")).Text=="-0.25","Slider readouts update correctly");
   var mode=(ComboBox)Field(form,"mode");var source=(ComboBox)Field(form,"windows");mode.SelectedIndex=1;Check(source.Enabled,"Selected-window source is enabled");mode.SelectedIndex=0;Check(!source.Enabled,"Desktop source picker is disabled without changing capture mode options");
   var tray=(NotifyIcon)Field(form,"tray");var menu=(ContextMenuStrip)Field(form,"appMenu");Check(tray.ContextMenuStrip==menu&&form.ContextMenuStrip==menu,"Tray and desktop share the same action menu");
   Check(menu.Items.Cast<ToolStripItem>().Count(i=>i.Text=="Help"||i.Text=="Donate")==2,"Help and Donate reachable from context menu");
   var helpMenu=(ContextMenuStrip)Field(form,"helpMenu");Check(helpMenu.Items.Count==15,"Help menu contains all thirteen topics and the offline guide");
   Invoke(form,"HideToTray");Application.DoEvents();Check(!form.Visible&&tray.Visible,"Explicit tray hide keeps a restore icon");Invoke(form,"ShowControls");Application.DoEvents();Check(form.Visible&&form.WindowState==FormWindowState.Normal,"Controls restore from tray");
   form.Close();Application.DoEvents();Check(!form.IsDisposed&&!form.Visible,"Close honors enabled tray preference");Invoke(form,"ShowControls");
   ((CheckBox)Field(form,"minimize")).Checked=false;form.WindowState=FormWindowState.Minimized;Application.DoEvents();Check(form.Visible,"Minimize remains on taskbar with tray preference disabled");Invoke(form,"ShowControls");
   form.Size=form.MinimumSize;Application.DoEvents();Check(Descendants(form).OfType<ComboBox>().All(c=>c.Width>180),"Source controls remain usable at minimum window width");
   Invoke(form,"ShowHelp","overlay");Application.DoEvents();var help=(Panel)Field(form,"helpPanel");Check(Descendants(help).OfType<Label>().Single(c=>c.AccessibleName=="Help article").Text.Contains("passes mouse input through"),"Contextual Help opens the requested topic");Invoke(form,"ShowControlsPage");
   Invoke(form,"ShowDonation");Application.DoEvents();var donations=(Panel)Field(form,"donationPanel");Check(Descendants(donations).OfType<PictureBox>().Count(p=>p.Image!=null)==4,"All four embedded donation QR images load");Check(donations.FindForm()==form,"Donate is an internal panel in the controls window");Invoke(form,"ShowControlsPage");
   Check(Field(form,"worker")==null,"Help, Donate, resize and tray operations never create a renderer");form.Close();Application.DoEvents();Check(form.IsDisposed,"Close exits with tray preference disabled");
   if(testHost!=null)testHost.Dispose();return 0;
  }catch(Exception e){Console.Error.WriteLine(e);return 1;}
 }
}
