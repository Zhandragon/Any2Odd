using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace OdysseyCompanion {
 sealed partial class MainForm {
  readonly bool preview;
  readonly ToolTip controlTips=new ToolTip{AutoPopDelay=15000};
  ContextMenuStrip appMenu,helpMenu;HelpPanel helpPanel;DonationPanel donationPanel;ScrollSurface controlsPage;Button themeButton;
  public MainForm():this(Preferences.Load(),false){}
  // Isolated UI validation can supply preferences without touching saved settings or starting capture.
  internal MainForm(Preferences preferences,bool previewOnly){
   pref=preferences;preview=previewOnly;
   Text="Any2Odd";Size=new Size(980,780);MinimumSize=new Size(700,580);StartPosition=FormStartPosition.CenterScreen;TopMost=true;
   var help=DesktopTheme.Button("Help",delegate{helpMenu.Show(TitleActions,0,TitleActions.Height);});help.AccessibleName="Help menu";TitleActions.Controls.Add(help);
   var donate=DesktopTheme.Button("Donate",delegate{ShowDonation();});donate.ForeColor=DesktopTheme.Accent;TitleActions.Controls.Add(donate);themeButton=DesktopTheme.Button(DesktopTheme.Dark?"Light":"Dark",delegate{ToggleTheme();});themeButton.AccessibleName="Toggle light / dark mode";TitleActions.Controls.Add(themeButton);StandardChrome(HideToTray);
   controlsPage=new ScrollSurface{Dock=DockStyle.Fill,Padding=new Padding(16,8,4,10)};Body.Controls.Add(controlsPage);
   var layout=new TableLayoutPanel{Dock=DockStyle.Top,AutoSize=true,ColumnCount=1,Margin=Padding.Empty};layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   Add(layout,new BrandBanner{Height=86,Margin=new Padding(0,0,0,4)});
   var session=Section(layout,"CONVERSION");
   var sessionRow=new TableLayoutPanel{AutoSize=true,ColumnCount=2,Dock=DockStyle.Fill,Margin=Padding.Empty};sessionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));sessionRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,190));
   status=new Label{Text="Ready",AutoSize=true,Anchor=AnchorStyles.Left|AnchorStyles.Right,Margin=new Padding(0,2,12,2)};sessionRow.Controls.Add(status,0,0);
   sessionRow.SizeChanged+=delegate{status.MaximumSize=new Size(Math.Max(100,sessionRow.Width-202),0);};
   toggle=Button("Turn on 3D",delegate{if(wanted)StopConversion();else StartConversion();});((GoldButton)toggle).Primary=true;toggle.ForeColor=Color.White;toggle.FlatAppearance.MouseOverBackColor=Color.FromArgb(62,145,162);toggle.Dock=DockStyle.Fill;sessionRow.Controls.Add(toggle,1,0);Add(session,sessionRow);
   var settings=new ResponsiveGrid();Add(layout,settings);
   var sources=Section(settings,"SOURCE & INPUT");
   mode=Combo(new[]{"Whole desktop — constant 3D","Selected window","Follow active window"});mode.SelectedIndex=pref.Mode=="Window"?1:pref.Mode=="Active"?2:0;Add(sources,Row("Capture",mode));
   format=Combo(new[]{"2D → 3D automatically (AI)","Side-by-side input (manual)"});format.SelectedIndex=pref.SideBySide?1:0;Add(sources,Row("Input format",format));
   windows=Combo(new string[0]);var picker=new TableLayoutPanel{AutoSize=true,ColumnCount=2,Dock=DockStyle.Fill,Margin=Padding.Empty};picker.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));picker.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));windows.Dock=DockStyle.Fill;picker.Controls.Add(windows,0,0);picker.Controls.Add(Button("Refresh",delegate{RefreshWindows();}),1,0);Add(sources,Row("Source window",picker));RefreshWindows();
   controlTips.SetToolTip(mode,"Whole desktop keeps interaction aligned. Selected/active window modes fit the window to the display and do not remap mouse coordinates.");controlTips.SetToolTip(format,"Choose side-by-side only for an existing stereo pair. Automatic stereo-layout detection is not implemented.");controlTips.SetToolTip(windows,"Used by Selected window; also seeds Follow active window. Refresh updates the list.");
   mode.SelectedIndexChanged+=delegate{windows.Enabled=mode.SelectedIndex!=0;};windows.Enabled=mode.SelectedIndex!=0;
   var tuning=Section(settings,"DEPTH & POP-OUT");
   depth=new GoldTrackBar{Minimum=0,Maximum=200,Value=(int)Math.Round(Math.Max(0,Math.Min(2,pref.Depth))*100),TickFrequency=25,AutoSize=false,Height=32,Dock=DockStyle.Fill};depthValue=new Label{AutoSize=true,Text=pref.Depth.ToString("0.00"),TextAlign=ContentAlignment.MiddleRight,Margin=new Padding(8,8,0,0)};Add(tuning,Row("Depth strength",Slider(depth,depthValue)));
   pop=new GoldTrackBar{Minimum=-100,Maximum=100,Value=(int)Math.Round(Math.Max(-1,Math.Min(1,pref.Popout))*100),TickFrequency=25,AutoSize=false,Height=32,Dock=DockStyle.Fill};popValue=new Label{AutoSize=true,Text=pref.Popout.ToString("0.00"),TextAlign=ContentAlignment.MiddleRight,Margin=new Padding(8,8,0,0)};Add(tuning,Row("Pop-out",Slider(pop,popValue)));
   depth.ValueChanged+=delegate{depthValue.Text=(depth.Value/100.0).ToString("0.00");};pop.ValueChanged+=delegate{popValue.Text=(pop.Value/100.0).ToString("0.00");};
   controlTips.SetToolTip(depth,"Depth strength: 0.00–2.00. Apply settings to save; active conversion briefly restarts.");controlTips.SetToolTip(pop,"Pop-out: −1.00–1.00. Apply settings to save; active conversion briefly restarts.");
   Add(tuning,Button("Apply settings / restart conversion",delegate{SavePreferences();if(wanted){StopWorker();StartWorker();}else status.Text="Preferences saved.";}));
   auto=Check("Convert automatically when this app opens",pref.AutoConvert);recover=Check("Recover if the conversion process stops",pref.Recover);minimize=Check("Minimize and close to the tray",pref.MinimizeToTray);keepOverlay=Check("Keep 3D above apps without taking focus (Apply to change)",pref.KeepOverlayOnTop);
   var behavior=Section(layout,"STARTUP & WINDOW BEHAVIOR");var options=new ResponsiveGrid();foreach(var box in new[]{auto,recover,minimize,keepOverlay})options.Controls.Add(box);Add(behavior,options);
   options.Layout+=delegate{foreach(var box in new[]{auto,recover,minimize,keepOverlay})box.MaximumSize=new Size(Math.Max(100,options.Width/(options.ColumnCount==2?2:1)-18),0);};
   Add(layout,new Label{Text="Ctrl+Alt+F10  On / off    ·    Ctrl+Alt+F9  Controls    ·    Ctrl+Alt+F8  Restore overlay    ·    F1  Help",AutoSize=true,ForeColor=DesktopTheme.Muted,Font=new Font("Segoe UI",9),Margin=new Padding(0,6,0,4)});
   var links=new FlowLayoutPanel{AutoSize=true,Dock=DockStyle.Fill};links.Controls.Add(Button("View logs",delegate{DesktopTheme.Open(Files.Data,this);}));links.Controls.Add(Button("Minimize to tray",delegate{HideToTray();}));links.Controls.Add(Button("Quit completely",delegate{Quit();}));Add(layout,links);
   Add(layout,new Label{Text="Version "+typeof(MainForm).Assembly.GetName().Version.ToString(3)+"  ·  Samsung Odyssey G90XF",AutoSize=true,ForeColor=DesktopTheme.Muted,Font=new Font("Segoe UI",8),Margin=new Padding(0,4,0,0)});
   controlsPage.SetContent(layout);
   BuildMenus();
   tray=new NotifyIcon{Icon=Icon,Text="Any2Odd — off",Visible=true,ContextMenuStrip=appMenu};tray.DoubleClick+=delegate{ShowControls();};
   ContextMenuStrip=appMenu;Body.ContextMenuStrip=appMenu;
   Resize+=delegate{if(WindowState==FormWindowState.Minimized&&minimize.Checked)HideToTray();};
   FormClosing+=delegate(object s,FormClosingEventArgs e){if(!exiting&&e.CloseReason==CloseReason.UserClosing&&minimize.Checked){e.Cancel=true;HideToTray();}else{StopConversion();tray.Visible=false;}};
   auto.CheckedChanged+=delegate{SavePreferences();};recover.CheckedChanged+=delegate{SavePreferences();};minimize.CheckedChanged+=delegate{SavePreferences();};
   Shown+=delegate{if(preview)return;TaskbarMenu.Install();bool hotkey=Win.RegisterHotKey(Handle,1,0x4003,0x79);bool showKey=Win.RegisterHotKey(Handle,2,0x4003,0x78);bool restoreKey=Win.RegisterHotKey(Handle,3,0x4003,0x77);if(!hotkey||!showKey||!restoreKey){status.Text="One of Ctrl+Alt+F8/F9/F10 is in use. Free the shortcuts before starting 3D.";Files.Log("Hotkey conflict: toggle="+hotkey+" controls="+showKey+" restore="+restoreKey);return;}if(pref.AutoConvert||Program.Start3DRequested)StartConversion();};
   heartbeat.Interval=250;heartbeat.Tick+=delegate{Poll();};if(!preview)heartbeat.Start();DesktopTheme.Changed+=ApplyTheme;
  }
  void BuildMenus(){
   helpMenu=DesktopTheme.Menu();foreach(var topic in HelpContent.Topics){var current=topic;helpMenu.Items.Add(topic.Title,null,delegate{ShowHelp(current.Id);});}helpMenu.Items.Add(new ToolStripSeparator());helpMenu.Items.Add("Open offline guide",null,delegate{DesktopTheme.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"USER-GUIDE.html"),this);});
   appMenu=DesktopTheme.Menu();appMenu.Items.Add("Show controls",null,delegate{ShowControls();});var switchItem=appMenu.Items.Add("Turn 3D on",null,delegate{if(wanted)StopConversion();else StartConversion();});appMenu.Items.Add("Restore overlay (Ctrl+Alt+F8)",null,delegate{RestoreOverlay();});
   var persistent=new ToolStripMenuItem("Keep overlay above apps"){CheckOnClick=true,Checked=pref.KeepOverlayOnTop};persistent.Click+=delegate{keepOverlay.Checked=persistent.Checked;SavePreferences();if(wanted){StopWorker();StartWorker();}};appMenu.Items.Add(persistent);appMenu.Items.Add(new ToolStripSeparator());
   appMenu.Items.Add("Always convert desktop (AI)",null,delegate{mode.SelectedIndex=0;format.SelectedIndex=0;SavePreferences();StopConversion();StartConversion();});appMenu.Items.Add("Side-by-side mode",null,delegate{format.SelectedIndex=1;SavePreferences();if(wanted){StopWorker();StartWorker();}});appMenu.Items.Add("AI conversion mode",null,delegate{format.SelectedIndex=0;SavePreferences();if(wanted){StopWorker();StartWorker();}});appMenu.Items.Add("Restart conversion",null,delegate{SavePreferences();StopWorker();wanted=true;StartWorker();});appMenu.Items.Add(new ToolStripSeparator());
   appMenu.Items.Add("Help",null,delegate{ShowHelp("start");});appMenu.Items.Add("Donate",null,delegate{ShowDonation();});appMenu.Items.Add("Toggle light / dark mode",null,delegate{ToggleTheme();});appMenu.Items.Add("View logs",null,delegate{DesktopTheme.Open(Files.Data,this);});appMenu.Items.Add("Minimize to tray",null,delegate{HideToTray();});appMenu.Items.Add(new ToolStripSeparator());appMenu.Items.Add("Quit completely",null,delegate{Quit();});
   appMenu.Opening+=delegate{persistent.Checked=keepOverlay.Checked;switchItem.Text=wanted?"Turn 3D off":"Turn 3D on";};
  }
  void HideToTray(){SavePreferences();tray.Visible=true;Hide();}
  void ShowPage(Control page){foreach(Control child in Body.Controls)child.Visible=child==page;page.BringToFront();if(!Visible){Show();WindowState=FormWindowState.Normal;Activate();}}
  void ShowControlsPage(){ShowPage(controlsPage);}
  void ShowHelp(string topic){if(helpPanel==null){helpPanel=new HelpPanel(ShowControlsPage);Body.Controls.Add(helpPanel);}helpPanel.SelectTopic(topic);ShowPage(helpPanel);}
  void ShowDonation(){if(donationPanel==null){donationPanel=new DonationPanel(ShowControlsPage);Body.Controls.Add(donationPanel);}ShowPage(donationPanel);}
  void ToggleTheme(){DesktopTheme.SetDark(!DesktopTheme.Dark,!preview);themeButton.Text=DesktopTheme.Dark?"Light":"Dark";}
  void ApplyTheme(){DesktopTheme.ApplyCurrent(this);themeButton.Text=DesktopTheme.Dark?"Light":"Dark";}
  protected override bool ProcessCmdKey(ref Message m,Keys key){if(key==Keys.F1){ShowHelp("start");return true;}return base.ProcessCmdKey(ref m,key);}
  Button Button(string text,EventHandler action){return DesktopTheme.Button(text,action);}
  CheckBox Check(string text,bool value){return new CheckBox{Text=text,Checked=value,AutoSize=true,Anchor=AnchorStyles.Left|AnchorStyles.Right,Margin=new Padding(0,4,0,4)};}
  ComboBox Combo(string[] values){var c=new GoldComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Dock=DockStyle.Fill,BackColor=DesktopTheme.Panel,ForeColor=DesktopTheme.Text,Margin=new Padding(0,4,0,4)};c.Items.AddRange(values);return c;}
  Control Slider(TrackBar slider,Label value){var row=new TableLayoutPanel{AutoSize=true,ColumnCount=2,Dock=DockStyle.Fill,Margin=Padding.Empty};row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,52));row.Controls.Add(slider,0,0);row.Controls.Add(value,1,0);return row;}
  TableLayoutPanel Row(string label,Control c){var r=new TableLayoutPanel{AutoSize=true,ColumnCount=2,Dock=DockStyle.Fill,Margin=new Padding(0,2,0,2)};r.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,26));r.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,74));r.Controls.Add(new Label{Text=label,Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,AutoSize=true},0,0);r.Controls.Add(c,1,0);return r;}
  TableLayoutPanel Section(TableLayoutPanel layout,string title){var card=new SectionPanel{Dock=DockStyle.Fill};var inside=new TableLayoutPanel{Dock=DockStyle.Top,AutoSize=true,ColumnCount=1};inside.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));card.Controls.Add(inside);Add(inside,new Label{Text=title,UseMnemonic=false,Font=DesktopTheme.Display(10),ForeColor=DesktopTheme.Text,AutoSize=true,Margin=new Padding(0,0,0,6)});Add(layout,card);return inside;}
  void Add(TableLayoutPanel layout,Control c){c.Anchor=AnchorStyles.Left|AnchorStyles.Right;layout.Controls.Add(c,0,layout.RowCount++);}
 }
}
