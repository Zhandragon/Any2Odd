using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

[assembly: System.Reflection.AssemblyTitle("Any2Odd")]
[assembly: System.Reflection.AssemblyProduct("Any2Odd")]
[assembly: System.Reflection.AssemblyDescription("Continuous desktop 3D for Samsung Odyssey G90XF")]

namespace OdysseyCompanion {
 static class Files {
  public static readonly string Data=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Odyssey3DCompanion");
  public const string Engine=@"C:\Program Files\Odyssey 3D Hub\Conversion";
  public static string Worker { get { return Path.Combine(Engine,"OdysseyCompanionWorker.exe"); } }
  public static string Settings { get {return Path.Combine(Data,"settings.json");} }
  public static string Overlay {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),@"AppData\LocalLow\Samsung\Odyssey3DHubUI\SettingsData\OverlaySettings.json");}}
  public static string ConversionLog {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),@"AppData\LocalLow\Samsung\Odyssey3DHubUI\Conversion.log");}}
  public static JavaScriptSerializer Json=new JavaScriptSerializer();
  public static void Save(string path,object value) {Directory.CreateDirectory(Path.GetDirectoryName(path));string tmp=path+".tmp";File.WriteAllText(tmp,Json.Serialize(value),new UTF8Encoding(false));if(File.Exists(path))File.Replace(tmp,path,null);else File.Move(tmp,path);}
  public static void Log(string s) {try {Directory.CreateDirectory(Data);File.AppendAllText(Path.Combine(Data,"companion.log"),DateTime.Now.ToString("o")+" "+s+Environment.NewLine);} catch {}}
  public static string Tail(string p,int chars) {try {using(var f=new FileStream(p,FileMode.Open,FileAccess.Read,FileShare.ReadWrite)){f.Seek(Math.Max(0,f.Length-chars),SeekOrigin.Begin);using(var r=new StreamReader(f))return r.ReadToEnd();}}catch{return "";}}
 }
 public sealed class Preferences {
  public string Mode="Desktop";
  public bool SideBySide=false,AutoConvert=true,MinimizeToTray=true,Recover=true,KeepOverlayOnTop=true;
  public double Depth=0.9,Popout=0;
  public static Preferences Load(){try{return Files.Json.Deserialize<Preferences>(File.ReadAllText(Files.Settings))??new Preferences();}catch{return new Preferences();}}
  public void Save(){Depth=Math.Max(0,Math.Min(2,Depth));Popout=Math.Max(-1,Math.Min(1,Popout));if(!new[]{"Desktop","Window","Active"}.Contains(Mode))Mode="Desktop";Files.Save(Files.Settings,this);}
 }
 static class Win {
  [StructLayout(LayoutKind.Sequential)] public struct Rect {public int L,T,R,B;public Rectangle Box {get{return Rectangle.FromLTRB(L,T,R,B);}} public Rect(Rectangle r){L=r.Left;T=r.Top;R=r.Right;B=r.Bottom;}}
  [StructLayout(LayoutKind.Sequential)] public struct Size {public int X,Y;}
  [StructLayout(LayoutKind.Sequential)] public struct Thumbnail {public uint Flags;public Rect Dest,Source;public byte Opacity;[MarshalAs(UnmanagedType.Bool)]public bool Visible;[MarshalAs(UnmanagedType.Bool)]public bool ClientOnly;}
  public delegate bool EnumFn(IntPtr h,IntPtr p);
  [DllImport("user32")]public static extern bool EnumWindows(EnumFn f,IntPtr p);
  [DllImport("user32")]public static extern bool IsWindowVisible(IntPtr h);
  [DllImport("user32")]public static extern bool IsWindow(IntPtr h);
  [DllImport("user32")]public static extern bool IsIconic(IntPtr h);
  [DllImport("user32")]public static extern bool GetWindowRect(IntPtr h,out Rect r);
  [DllImport("user32",CharSet=CharSet.Unicode)]public static extern int GetWindowText(IntPtr h,StringBuilder s,int n);
  [DllImport("user32",CharSet=CharSet.Unicode)]public static extern bool SetWindowText(IntPtr h,string text);
  [DllImport("user32",CharSet=CharSet.Unicode)]public static extern int GetClassName(IntPtr h,StringBuilder s,int n);
  [DllImport("user32")]public static extern uint GetWindowThreadProcessId(IntPtr h,out uint p);
  [DllImport("user32")]public static extern IntPtr GetForegroundWindow();
  [DllImport("user32",EntryPoint="GetWindowLongPtrW")]public static extern IntPtr GetWindowLongPtr(IntPtr h,int index);
  [DllImport("user32",EntryPoint="SetWindowLongPtrW",SetLastError=true)]public static extern IntPtr SetWindowLongPtr(IntPtr h,int index,IntPtr value);
  [DllImport("user32",SetLastError=true)]public static extern bool SetLayeredWindowAttributes(IntPtr h,uint key,byte alpha,uint flags);
  [DllImport("user32")]public static extern bool ShowWindow(IntPtr h,int command);
  [DllImport("user32")]public static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int w,int ht,uint flags);
  [DllImport("user32")]public static extern bool RegisterHotKey(IntPtr h,int id,uint mods,uint key);
  [DllImport("user32")]public static extern bool UnregisterHotKey(IntPtr h,int id);
  [DllImport("user32")]public static extern bool SetProcessDpiAwarenessContext(IntPtr context);
  [DllImport("dwmapi")]public static extern int DwmRegisterThumbnail(IntPtr dest,IntPtr src,out IntPtr thumb);
  [DllImport("dwmapi")]public static extern int DwmUnregisterThumbnail(IntPtr t);
  [DllImport("dwmapi")]public static extern int DwmUpdateThumbnailProperties(IntPtr t,ref Thumbnail p);
  [DllImport("dwmapi")]public static extern int DwmQueryThumbnailSourceSize(IntPtr t,out Size size);
  [DllImport("dwmapi")]public static extern int DwmGetWindowAttribute(IntPtr h,int attr,out int value,int size);
  [DllImport("dwmapi",EntryPoint="DwmGetWindowAttribute")]public static extern int DwmGetFrameBounds(IntPtr h,int attr,out Rect value,int size);
  public static string Title(IntPtr h){var b=new StringBuilder(512);GetWindowText(h,b,b.Capacity);return b.ToString();}
  public static string Class(IntPtr h){var b=new StringBuilder(256);GetClassName(h,b,b.Capacity);return b.ToString();}
  public static bool VisibleBounds(IntPtr h,out Rectangle bounds){Rect frame;if(DwmGetFrameBounds(h,9,out frame,16)==0&&frame.Box.Width>0&&frame.Box.Height>0){bounds=frame.Box;return true;}Rect raw;if(GetWindowRect(h,out raw)){bounds=raw.Box;return bounds.Width>0&&bounds.Height>0;}bounds=Rectangle.Empty;return false;}
  public static object[] Inspect(int target){var result=new List<object>();EnumWindows(delegate(IntPtr h,IntPtr p){uint pid;GetWindowThreadProcessId(h,out pid);if(pid==target){Rect r;GetWindowRect(h,out r);result.Add(new{handle=h.ToInt64(),title=Title(h),windowClass=Class(h),style=GetWindowLongPtr(h,-16).ToInt64().ToString("X"),extended=GetWindowLongPtr(h,-20).ToInt64().ToString("X"),visible=IsWindowVisible(h),minimized=IsIconic(h),bounds=r.Box.ToString(),foreground=GetForegroundWindow()==h});}return true;},IntPtr.Zero);return result.ToArray();}
  public static List<IntPtr> Windows(int parentPid,int workerPid){var a=new List<IntPtr>();EnumWindows(delegate(IntPtr h,IntPtr p){uint pid;GetWindowThreadProcessId(h,out pid);int cloaked;DwmGetWindowAttribute(h,14,out cloaked,4);if(IsWindowVisible(h)&&!IsIconic(h)&&cloaked==0&&pid!=parentPid&&pid!=workerPid){a.Add(h);}return true;},IntPtr.Zero);return a;}
 }
 sealed class WindowChoice {public IntPtr H;public string Name;public override string ToString(){return Name;}}
 sealed class Mirror : Form {
  readonly Preferences prefs;readonly int parent;readonly IntPtr selected;readonly Rectangle screen;IntPtr lastActive;
  List<IntPtr> thumbs=new List<IntPtr>();string signature="";readonly System.Windows.Forms.Timer update=new System.Windows.Forms.Timer();
  DesktopCapture desktop;DateTime sampled=DateTime.UtcNow;long sampleFrames;double captureMilliseconds;
  int frameQueued;
  public Func<bool> CanCaptureDesktop;
  public event Action<string> CaptureFailed;
  public int Sources {get;private set;}
  public Mirror(Preferences p,int parentPid,IntPtr source){prefs=p;parent=parentPid;selected=source;screen=Screen.PrimaryScreen.Bounds;Text="Any2Odd capture surface";FormBorderStyle=FormBorderStyle.None;StartPosition=FormStartPosition.Manual;Bounds=screen;ShowInTaskbar=false;BackColor=Color.FromArgb(20,24,32);
   if(prefs.Mode=="Desktop"){Bounds=OffscreenBounds(screen.Size,Screen.AllScreens.Select(s=>s.Bounds).ToArray());desktop=new DesktopCapture(screen);desktop.FrameAvailable+=QueueFrame;update.Interval=100;}else update.Interval=250;
   update.Tick+=delegate{if(desktop!=null){if(CanCaptureDesktop!=null&&CanCaptureDesktop())CaptureDesktop();}else RefreshSources();};}
  protected override bool ShowWithoutActivation {get{return true;}}
  void QueueFrame(){if(IsDisposed||!IsHandleCreated||Interlocked.Exchange(ref frameQueued,1)!=0)return;try{BeginInvoke((Action)delegate{try{if(!IsDisposed&&desktop!=null&&CanCaptureDesktop!=null&&CanCaptureDesktop())CaptureDesktop();}finally{Interlocked.Exchange(ref frameQueued,0);}});}catch(InvalidOperationException){Interlocked.Exchange(ref frameQueued,0);}}
  protected override CreateParams CreateParams {get{var p=base.CreateParams;p.ExStyle|=0x08000000|0x80|0x20;return p;}}
  protected override void WndProc(ref Message m){if(m.Msg==0x84){m.Result=new IntPtr(-1);return;}if(m.Msg==0x21){m.Result=new IntPtr(3);return;}base.WndProc(ref m);}
  public static Rectangle OffscreenBounds(System.Drawing.Size size,Rectangle[] monitors){return new Rectangle(monitors.Max(r=>r.Right)+64,monitors.Min(r=>r.Top),size.Width,size.Height);}
  void AssertSourceIsOffscreen(){Win.Rect actual;if(!Win.GetWindowRect(Handle,out actual)||Screen.AllScreens.Any(s=>actual.Box.IntersectsWith(s.Bounds)))throw new InvalidOperationException("Capture surface overlaps a monitor. Conversion stopped to prevent feedback.");}
  protected override void OnShown(EventArgs e){Win.SetWindowPos(Handle,new IntPtr(1),0,0,0,0,0x13);if(desktop!=null){AssertSourceIsOffscreen();CaptureDesktop();}else RefreshSources();base.OnShown(e);update.Start();}
  void CaptureDesktop(){try{AssertSourceIsOffscreen();var watch=Stopwatch.StartNew();desktop.Present(Handle);captureMilliseconds+=watch.Elapsed.TotalMilliseconds;
    double seconds=(DateTime.UtcNow-sampled).TotalSeconds;if(seconds>=10){long count=desktop.Frames-sampleFrames;Files.Save(Path.Combine(Files.Data,"desktop-capture-status.json"),new{pid=Process.GetCurrentProcess().Id,time=DateTime.Now.ToString("o"),backend="WGC GPU, cursor excluded, latest frame",frames=desktop.Frames,fps=count/seconds,averageCaptureMilliseconds=count>0?captureMilliseconds/count:0,bounds=screen,sourceBounds=Bounds});sampleFrames=desktop.Frames;sampled=DateTime.UtcNow;captureMilliseconds=0;}
   }catch(Exception e){update.Stop();Files.Log("Desktop capture failed: "+e);if(CaptureFailed!=null)CaptureFailed(e.Message);}}
  protected override void OnPaint(PaintEventArgs e){if(desktop==null)base.OnPaint(e);}
  protected override void OnPaintBackground(PaintEventArgs e){if(desktop==null)base.OnPaintBackground(e);}
  public static Rectangle Fit(Rectangle source,Rectangle target){if(source.Width<1||source.Height<1)return Rectangle.Empty;double scale=Math.Min((double)target.Width/source.Width,(double)target.Height/source.Height);int w=(int)(source.Width*scale),h=(int)(source.Height*scale);return new Rectangle((target.Width-w)/2,(target.Height-h)/2,w,h);}
  void RefreshSources(){
   var all=Win.Windows(parent,Process.GetCurrentProcess().Id);var candidates=new List<IntPtr>();
   IntPtr target=selected;if(prefs.Mode=="Active"){var active=Win.GetForegroundWindow();if(all.Contains(active)){Rectangle r;if(Win.VisibleBounds(active,out r)&&r.IntersectsWith(screen))lastActive=active;}target=lastActive!=IntPtr.Zero?lastActive:selected;}if(target!=IntPtr.Zero&&all.Contains(target))candidates.Add(target);
   string next=string.Join(";",candidates.Select(h=>{Win.Rect r;Win.GetWindowRect(h,out r);return h+":"+r.Box.ToString();}));if(next==signature)return;signature=next;
   foreach(var t in thumbs)Win.DwmUnregisterThumbnail(t);thumbs.Clear();Sources=0;
   foreach(var h in candidates){IntPtr t;if(Win.DwmRegisterThumbnail(Handle,h,out t)!=0)continue;Win.Size size;if(Win.DwmQueryThumbnailSourceSize(t,out size)!=0||size.X<2||size.Y<2){Win.DwmUnregisterThumbnail(t);continue;}
    Rectangle src=new Rectangle(0,0,size.X,size.Y),dest=Fit(src,new Rectangle(0,0,screen.Width,screen.Height));
    var props=new Win.Thumbnail{Flags=31,Dest=new Win.Rect(dest),Source=new Win.Rect(src),Opacity=255,Visible=true,ClientOnly=false};if(Win.DwmUpdateThumbnailProperties(t,ref props)==0){thumbs.Add(t);Sources++;}else Win.DwmUnregisterThumbnail(t);
   }
  }
  protected override void Dispose(bool disposing){if(disposing){update.Stop();update.Dispose();foreach(var t in thumbs)Win.DwmUnregisterThumbnail(t);thumbs.Clear();if(desktop!=null){desktop.Dispose();desktop=null;}}base.Dispose(disposing);}
 }
 static class Engine {
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]public delegate void Closed();
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]public delegate void StartFn(IntPtr w,bool sbs,int cores,Closed done);
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]public delegate void StopFn();
  [DllImport("kernel32",CharSet=CharSet.Unicode,SetLastError=true)]static extern IntPtr LoadLibraryEx(string p,IntPtr file,uint flags);
  [DllImport("kernel32",CharSet=CharSet.Ansi)]static extern IntPtr GetProcAddress(IntPtr m,string n);
  [DllImport("kernel32",CharSet=CharSet.Unicode)]static extern bool SetDllDirectory(string p);
  public static string Validate(){
   var dll=Path.Combine(Files.Engine,"ConversionWrapper.dll");if(!File.Exists(dll))return "Samsung's conversion engine is missing.";
   using(var sha=SHA256.Create())using(var f=File.OpenRead(dll)){var hash=BitConverter.ToString(sha.ComputeHash(f)).Replace("-","").ToLowerInvariant();if(hash!="0116b33697ca0192fbe26da96cb9e2592599c0cacfbb1129ae03e445cb3a840b")return "Samsung's engine changed. This build needs a compatibility update.";}
   if(!File.Exists(Files.Worker))return "The companion worker is not installed.";
   if(Screen.PrimaryScreen.Bounds.Width!=3840||Screen.PrimaryScreen.Bounds.Height!=2160)return "Set Odyssey as the primary display at 3840 x 2160.";
   try{using(var q=new ManagementObjectSearcher("SELECT Name,ConfigManagerErrorCode FROM Win32_PnPEntity WHERE Name='3D Stereo WebCam'")){if(!q.Get().Cast<ManagementObject>().Any(x=>Convert.ToInt32(x["ConfigManagerErrorCode"])==0))return "Reconnect the Odyssey USB cable; its 3D camera is unavailable.";}}catch(Exception e){return "Could not check the camera: "+e.Message;}
   if(Process.GetProcessesByName("Odyssey3DPlayer").Length>0)return "Stop the conversion running in Samsung's player, then try again.";
   return null;
  }
  public static void SaveOverlay(Preferences p){
   Directory.CreateDirectory(Files.Data);var backup=Path.Combine(Files.Data,"Samsung-OverlaySettings-before.json");if(File.Exists(Files.Overlay)&&!File.Exists(backup))File.Copy(Files.Overlay,backup);
   Dictionary<string,object> doc;try{doc=Files.Json.Deserialize<Dictionary<string,object>>(File.ReadAllText(Files.Overlay));}catch{doc=new Dictionary<string,object>();}
   string section=p.SideBySide?"SideBySideVideo":"StandardVideo";
   object raw;Dictionary<string,object> values;if(doc.TryGetValue(section,out raw)&&raw is Dictionary<string,object>)values=(Dictionary<string,object>)raw;else {values=new Dictionary<string,object>();doc[section]=values;}
   values["3D Depth"]=p.Depth.ToString("0.000000",CultureInfo.InvariantCulture);values["3D Popout"]=p.Popout.ToString("0.000000",CultureInfo.InvariantCulture);values["3D Off"]=false;values["Overlay Off"]=true;Files.Save(Files.Overlay,doc);
  }
  public static int Run(string eventName,int parent,long source){
   var pref=Preferences.Load();string status=Path.Combine(Files.Data,"worker-status.json");Action<string> report=s=>Files.Save(status,new {state=s,time=DateTime.Now.ToString("o"),pid=Process.GetCurrentProcess().Id});
   try {bool first;using(var mutex=new Mutex(true,"Local\\OdysseyCompanionRenderer",out first)){if(!first){report("Another companion renderer is running.");return 2;}
    string error=Validate();if(error!=null){report(error);return 3;}
    Environment.CurrentDirectory=Files.Engine;Environment.SetEnvironmentVariable("PATH",Files.Engine+";"+Path.Combine(Files.Engine,"CUDA")+";"+Path.Combine(Files.Engine,"ONNX")+@";C:\Program Files\LeiaSR\Platform\bin;C:\Program Files\Odyssey 3D Hub;"+Environment.GetEnvironmentVariable("PATH"));SetDllDirectory(Files.Engine);
    IntPtr lib=LoadLibraryEx(Path.Combine(Files.Engine,"ConversionWrapper.dll"),IntPtr.Zero,8);if(lib==IntPtr.Zero)throw new Exception("Engine load error "+Marshal.GetLastWin32Error());
    var start=(StartFn)Marshal.GetDelegateForFunctionPointer(GetProcAddress(lib,"Start"),typeof(StartFn));var stop=(StopFn)Marshal.GetDelegateForFunctionPointer(GetProcAddress(lib,"Stop"),typeof(StopFn));
    using(var ev=EventWaitHandle.OpenExisting(eventName))using(var repair=EventWaitHandle.OpenExisting(eventName+"-restore"))using(var surface=new Mirror(pref,parent,new IntPtr(source))){bool closing=false,started=false,ready=false;int callback=0;DateTime begun=DateTime.Now;
     var overlay=new OverlayController(parent,pref.KeepOverlayOnTop,pref.Mode=="Desktop");surface.CanCaptureDesktop=delegate{return overlay.CaptureSafe;};var overlayTimer=new System.Windows.Forms.Timer{Interval=100};
     Closed done=delegate{Interlocked.Exchange(ref callback,1);};var poll=new System.Windows.Forms.Timer{Interval=500};
     Action shutdown=delegate{if(closing)return;closing=true;poll.Stop();overlayTimer.Stop();if(started){stop();started=false;}surface.Close();};
     surface.CaptureFailed+=delegate(string message){report("Desktop capture error: "+message);shutdown();};
     overlayTimer.Tick+=delegate{try{overlay.Tick(repair.WaitOne(0));}catch(Exception e){Files.Log("Overlay error: "+e);report(e.Message);shutdown();}};
     poll.Tick+=delegate {bool alive=true;try{using(var owner=Process.GetProcessById(parent))alive=!owner.HasExited;}catch{alive=false;}if(ev.WaitOne(0)||!alive||Interlocked.CompareExchange(ref callback,0,0)!=0){shutdown();return;}
      string tail=Files.Tail(Files.ConversionLog,9000);if(!ready&&overlay.Attached&&tail.Contains("First time MinimizeUI")){ready=true;report("3D active — click-through overlay; "+(pref.KeepOverlayOnTop?"stays above apps":"Ctrl+Alt+F8 restores it"));}
      if(!ready&&(DateTime.Now-begun).TotalSeconds>30){report("Conversion did not initialize. See conversion log.");shutdown();}
     };
     surface.Shown+=delegate{report("Starting conversion");SaveOverlay(pref);begun=DateTime.Now;start(surface.Handle,pref.SideBySide,16384,done);started=true;poll.Start();overlayTimer.Start();};
     Application.Run(surface);overlayTimer.Dispose();poll.Dispose();GC.KeepAlive(done);Files.Log("Renderer stopped cleanly.");return ready?0:4;
    }
   }}catch(Exception e){Files.Log(e.ToString());report("Renderer error: "+e.Message);return 1;}
  }
 }
 sealed partial class MainForm:DesktopForm {
  Preferences pref;NotifyIcon tray;Label status,depthValue,popValue;Button toggle;ComboBox mode,format,windows;TrackBar depth,pop;CheckBox auto,recover,minimize,keepOverlay;Process worker;EventWaitHandle stopEvent,restoreEvent;bool wanted=false,exiting=false,changing=false;int failures=0;DateTime retryAfter=DateTime.MinValue;System.Windows.Forms.Timer heartbeat=new System.Windows.Forms.Timer();
  void RefreshWindows(){var selected=windows.SelectedItem as WindowChoice;windows.Items.Clear();foreach(var h in Win.Windows(Process.GetCurrentProcess().Id,worker==null?-1:worker.Id)){string title=Win.Title(h);if(title.Length>0&&title!="Program Manager")windows.Items.Add(new WindowChoice{H=h,Name=title});}if(windows.Items.Count>0){windows.SelectedIndex=0;for(int i=0;i<windows.Items.Count;i++)if(selected!=null&&((WindowChoice)windows.Items[i]).H==selected.H)windows.SelectedIndex=i;}}
  void SavePreferences(){if(preview||changing||auto==null)return;pref.Mode=mode.SelectedIndex==1?"Window":mode.SelectedIndex==2?"Active":"Desktop";pref.SideBySide=format.SelectedIndex==1;pref.Depth=depth.Value/100.0;pref.Popout=pop.Value/100.0;pref.AutoConvert=auto.Checked;pref.Recover=recover.Checked;pref.MinimizeToTray=minimize.Checked;if(keepOverlay!=null)pref.KeepOverlayOnTop=keepOverlay.Checked;pref.Save();}
  void StartConversion(){SavePreferences();wanted=true;failures=0;StartWorker();}
  void StartWorker(){if(worker!=null&&!worker.HasExited)return;string error=Engine.Validate();if(error!=null){status.Text=error;wanted=false;UpdateToggle();return;}SavePreferences();long source=windows.SelectedItem is WindowChoice?((WindowChoice)windows.SelectedItem).H.ToInt64():0;if(pref.Mode=="Window"&&source==0){status.Text="Choose a source window first.";wanted=false;UpdateToggle();return;}
   ReleaseEvents();string name="Local\\OdysseyCompanionStop-"+Guid.NewGuid().ToString("N");stopEvent=new EventWaitHandle(false,EventResetMode.ManualReset,name);restoreEvent=new EventWaitHandle(false,EventResetMode.AutoReset,name+"-restore");
   try{var info=new ProcessStartInfo(Files.Worker,"--worker "+name+" "+Process.GetCurrentProcess().Id+" "+source){UseShellExecute=false,WorkingDirectory=Files.Engine,CreateNoWindow=true};worker=Process.Start(info);Files.Log("Started renderer PID "+worker.Id+" mode "+pref.Mode);status.Text="Starting 3D — allow a few seconds for the AI model.";}catch(Exception e){ReleaseEvents();wanted=false;status.Text="Could not start conversion: "+e.Message;Files.Log(e.ToString());}UpdateToggle();}
  void ReleaseEvents(){if(stopEvent!=null){stopEvent.Dispose();stopEvent=null;}if(restoreEvent!=null){restoreEvent.Dispose();restoreEvent=null;}}
  void StopWorker(){if(stopEvent!=null)stopEvent.Set();if(worker!=null){try{if(!worker.HasExited&&!worker.WaitForExit(7000)){Files.Log("Renderer did not stop in 7 seconds; ending only the owned renderer.");worker.Kill();worker.WaitForExit(2000);}}catch{}worker.Dispose();worker=null;}ReleaseEvents();}
  void RestoreOverlay(){if(worker!=null&&!worker.HasExited&&restoreEvent!=null){restoreEvent.Set();Files.Log("Overlay restore requested.");}else status.Text="Turn on 3D before restoring its overlay.";}
  void StopConversion(){wanted=false;StopWorker();status.Text="3D is off. Your normal desktop is restored.";UpdateToggle();}
  void UpdateToggle(){toggle.Text=wanted?"Turn off 3D":"Turn on 3D";tray.Text="Any2Odd — "+(wanted?"on":"off");}
  void Poll(){if(Program.QuitRequested.WaitOne(0)){Quit();return;}if(Program.ShowRequested.WaitOne(0))ShowControls();if(Program.ToggleRequested.WaitOne(0)){if(wanted)StopConversion();else StartConversion();}if(worker==null){if(wanted&&DateTime.Now>=retryAfter)StartWorker();return;}if(worker.HasExited){int code=worker.ExitCode;worker.Dispose();worker=null;ReleaseEvents();if(wanted&&pref.Recover&&failures<3){failures++;retryAfter=DateTime.Now.AddSeconds(8);status.Text="Conversion stopped ("+code+"). Recovery attempt "+failures+" in 8 seconds.";}else{wanted=false;status.Text="Conversion stopped. Check the USB connection and logs, then turn 3D on again.";UpdateToggle();}}else{try{var d=Files.Json.Deserialize<Dictionary<string,object>>(File.ReadAllText(Path.Combine(Files.Data,"worker-status.json")));if(Convert.ToInt32(d["pid"])==worker.Id)status.Text=Convert.ToString(d["state"]);}catch{}}}
  void ShowControls(){ShowControlsPage();Show();WindowState=FormWindowState.Normal;TopMost=true;Activate();}
  void Quit(){exiting=true;SavePreferences();StopConversion();tray.Visible=false;Application.Exit();}
  protected override void WndProc(ref Message m){if(m.Msg==0x312){if(m.WParam.ToInt32()==1){if(wanted)StopConversion();else StartConversion();}else if(m.WParam.ToInt32()==2)ShowControls();else if(m.WParam.ToInt32()==3)RestoreOverlay();return;}base.WndProc(ref m);}
  protected override void Dispose(bool disposing){if(disposing){DesktopTheme.Changed-=ApplyTheme;heartbeat.Dispose();controlTips.Dispose();if(tray!=null)tray.Dispose();if(appMenu!=null)appMenu.Dispose();if(helpMenu!=null)helpMenu.Dispose();if(IsHandleCreated){Win.UnregisterHotKey(Handle,1);Win.UnregisterHotKey(Handle,2);Win.UnregisterHotKey(Handle,3);}}base.Dispose(disposing);}
 }
 static class Program {
  internal static EventWaitHandle QuitRequested,ToggleRequested,ShowRequested;
  internal static bool Start3DRequested;
  static bool Signal(string name){try{using(var ev=EventWaitHandle.OpenExisting(name))ev.Set();return true;}catch(WaitHandleCannotBeOpenedException){return false;}}
  [STAThread]static int Main(string[] args){Win.SetProcessDpiAwarenessContext(new IntPtr(-4));Directory.CreateDirectory(Files.Data);Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
   if(args.Length==4&&args[0]=="--worker")return Engine.Run(args[1],int.Parse(args[2]),long.Parse(args[3]));
   if(args.Length==2&&args[0]=="--inspect"){Files.Save(Path.Combine(Files.Data,"window-diagnostics.json"),Win.Inspect(int.Parse(args[1])));return 0;}
   if(args.Length==3&&args[0]=="--layout-probe"){ProjectionDiagnostics.Save(int.Parse(args[1]),int.Parse(args[2]));return 0;}
   if(args.Length==1&&args[0]=="--test-capture-exclusion")return DesktopCapture.TestExclusion();
   if(args.Length==1&&args[0]=="--test-offscreen")return DesktopCapture.TestOffscreen();
   if(args.Length==1&&args[0]=="--toggle"){if(Signal("Local\\Odyssey3DCompanionToggle"))return 0;Start3DRequested=true;}
   if(args.Length==1&&args[0]=="--show"&&Signal("Local\\Odyssey3DCompanionShow"))return 0;
   if(args.Length==1&&args[0]=="--quit"){try{using(var ev=EventWaitHandle.OpenExisting("Local\\Odyssey3DCompanionQuit"))ev.Set();return 0;}catch(WaitHandleCannotBeOpenedException){return 0;}}
   if(args.Length==1&&args[0]=="--self-test"){var fit=Mirror.Fit(new Rectangle(0,0,1920,1080),new Rectangle(0,0,3840,2160));if(fit!=new Rectangle(0,0,3840,2160))return 1;if(Mirror.Fit(Rectangle.Empty,new Rectangle(0,0,3840,2160))!=Rectangle.Empty)return 2;foreach(var arrangement in new[]{new[]{new Rectangle(0,0,3840,2160),new Rectangle(610,-1440,2560,1440),new Rectangle(3840,473,1440,2560)},new[]{new Rectangle(-2560,-1440,2560,1440),new Rectangle(0,0,3840,2160)}}){var offscreen=Mirror.OffscreenBounds(new System.Drawing.Size(3840,2160),arrangement);if(arrangement.Any(r=>r.IntersectsWith(offscreen))||offscreen.Size!=new System.Drawing.Size(3840,2160))return 3;}Files.Log("Geometry and multi-monitor source-isolation checks passed.");return 0;}
   bool first;using(var mutex=new Mutex(true,"Local\\Odyssey3DCompanionUI",out first)){if(!first){Signal("Local\\Odyssey3DCompanionShow");return 0;}using(QuitRequested=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\Odyssey3DCompanionQuit"))using(ToggleRequested=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\Odyssey3DCompanionToggle"))using(ShowRequested=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\Odyssey3DCompanionShow")){Application.ThreadException+=delegate(object s,ThreadExceptionEventArgs e){Files.Log(e.Exception.ToString());MessageBox.Show(e.Exception.Message,"Any2Odd");};Application.Run(new MainForm());return 0;}}
  }
 }
}
