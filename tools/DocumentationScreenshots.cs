using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// Real release widgets in an unattached child host: no desktop capture or converter.
class DocumentationScreenshots {
 [DllImport("user32.dll")] static extern bool SetProcessDpiAwarenessContext(IntPtr context);
 const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 static object Field(object target,string name){return target.GetType().GetField(name,Flags).GetValue(target);}
 static object Invoke(object target,string name,params object[] args){return target.GetType().GetMethod(name,Flags).Invoke(target,args);}
 static System.Collections.Generic.IEnumerable<Control> Children(Control parent){foreach(Control c in parent.Controls){yield return c;foreach(var child in Children(c))yield return child;}}
 static void Arrange(Control form){form.PerformLayout();foreach(var c in Children(form).Where(c=>c.GetType().Name=="ScrollSurface"))Invoke(c,"Arrange");Application.DoEvents();}
 static void Save(Control control,string destination){using(var image=new Bitmap(control.Width,control.Height)){control.DrawToBitmap(image,control.ClientRectangle);image.Save(destination);}}
 [STAThread] static int Main(string[] args){try{
  SetProcessDpiAwarenessContext(new IntPtr(-4));Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
  string output=Path.GetFullPath(args[0]);Directory.CreateDirectory(output);
  var assembly=Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Any2Odd.exe"));
  var preferences=Activator.CreateInstance(assembly.GetType("OdysseyCompanion.Preferences"));
  using(var form=(Form)Activator.CreateInstance(assembly.GetType("OdysseyCompanion.MainForm"),Flags,null,new[]{preferences,(object)true},null))
  using(var host=new UserControl{Size=new Size(850,1080)}){
   form.TopLevel=false;form.TopMost=false;form.Dock=DockStyle.Fill;host.Controls.Add(form);host.CreateControl();form.Show();
   var tray=(NotifyIcon)Field(form,"tray");tray.Visible=false;
   var windows=(ComboBox)Field(form,"windows");windows.Items.Clear();windows.Items.Add("Choose a window in Selected window mode");windows.SelectedIndex=0;
   var setDark=assembly.GetType("OdysseyCompanion.DesktopTheme").GetMethod("SetDark",BindingFlags.Public|BindingFlags.Static);
   foreach(bool dark in new[]{false,true}){
    setDark.Invoke(null,new object[]{dark,false});Invoke(form,"ShowControlsPage");Arrange(form);
    string suffix=dark?"-dark.png":"-light.png";
    Save(form,Path.Combine(output,"controls"+suffix));
    var sections=Children((Control)Field(form,"controlsPage")).Where(c=>c.GetType().Name=="SectionPanel").ToArray();
    Save(sections[1],Path.Combine(output,"source"+suffix));Save(sections[2],Path.Combine(output,"settings"+suffix));
    Invoke(form,"ShowDonation");Arrange(form);Save(form,Path.Combine(output,"donate"+suffix));
    Invoke(form,"ShowHelp","input");Arrange(form);Save(form,Path.Combine(output,"help"+suffix));
   }
   if(Field(form,"worker")!=null)throw new Exception("Documentation capture must not start rendering.");
  }
  Console.WriteLine("Rendered controls, source, settings, Help and Donate in both themes; no conversion started.");return 0;
 }catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
