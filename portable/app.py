"""Experimental macOS/Linux frontend. Windows conversion is intentionally separate."""
import html
import json
import os
from pathlib import Path
import sys
import time

from PySide6.QtCore import Qt, QUrl, QStandardPaths, QTimer, QSize
from PySide6.QtGui import QAction, QDesktopServices, QIcon, QImage, QPixmap, QPainter
from PySide6.QtWidgets import (QApplication, QMainWindow, QWidget, QVBoxLayout, QHBoxLayout,
    QFormLayout, QLabel, QPushButton, QComboBox, QCheckBox, QFileDialog, QGroupBox,
    QStackedWidget, QScrollArea, QTextBrowser, QListWidget, QSystemTrayIcon, QMenu)
from stereo import convert

ROOT = Path(__file__).resolve().parents[1]
SPONSOR = "https://github.com/sponsors/Zhandragon"
DONATIONS = [
    ("PayPal", "Personal donation link", "paypal.png", "https://www.paypal.com/qrcodes/managed/25c71b49-6280-4db1-a983-4db5f53183eb?utm_source=consweb_more"),
    ("Ethereum", "Ethereum network · ETH", "ethereum.png", "0x9C4D852279801fB4379b11698fD5835Fe275307D"),
    ("Polygon", "Polygon network · POL", "polygon.png", "0x9C4D852279801fB4379b11698fD5835Fe275307D"),
    ("Bitcoin", "Bitcoin network · BTC", "bitcoin.png", "bc1q24y4u2xe5pzk9pg3q7qyjty8vhxvp4nskunqle"),
]


def theme(dark):
    bg, panel, text, muted, accent = ("#111315", "#1c1e20", "#f2ece2", "#b7afa4", "#7cc4d3") if dark else ("#eee7dc", "#f8f3ea", "#202224", "#625e59", "#1f6f82")
    return f"""
    QWidget {{ background:{bg}; color:{text}; font-family:'Segoe UI','Helvetica Neue',sans-serif; font-size:14px; }}
    QGroupBox {{ background:{panel}; border:1px solid #b0863a; border-radius:14px; margin-top:16px; padding:20px 12px 12px; }}
    QGroupBox::title {{ subcontrol-origin:margin; left:16px; color:{accent}; padding:0 6px; }}
    QPushButton,QComboBox {{ background:{panel}; border:1px solid #b0863a; border-radius:9px; padding:8px 12px; min-height:20px; }}
    QPushButton:hover {{ border:1px solid {accent}; }} QPushButton:disabled {{ color:{muted}; }}
    QPushButton:focus,QComboBox:focus {{ border:2px solid {accent}; }}
    QTextBrowser,QListWidget {{ background:{panel}; border:1px solid #b0863a; border-radius:10px; padding:10px; }}
    QListWidget::item {{ padding:10px 4px; }} QListWidget::item:selected {{ background:#1f6f82; color:#fff7e8; }}
    QScrollArea {{ border:0; }} QScrollBar:vertical {{ background:{bg}; width:13px; margin:2px; }}
    QScrollBar::handle:vertical {{ background:#b0863a; min-height:36px; border-radius:5px; }}
    QScrollBar::add-line:vertical,QScrollBar::sub-line:vertical {{ height:0; }}
    QLabel#notice {{ color:{accent}; border:1px solid #b0863a; border-radius:10px; padding:10px; }}
    QMenu {{ background:{panel}; border:1px solid #b0863a; padding:8px; }} QMenu::item {{ padding:7px 18px; }}
    QMenu::item:selected {{ background:#1f6f82; color:white; }}
    """


class Output(QWidget):
    def __init__(self):
        super().__init__(None, Qt.WindowType.Tool | Qt.WindowType.FramelessWindowHint |
                         Qt.WindowType.WindowStaysOnTopHint | Qt.WindowType.WindowTransparentForInput |
                         Qt.WindowType.WindowDoesNotAcceptFocus)
        self.setAttribute(Qt.WidgetAttribute.WA_ShowWithoutActivating)
        self.image = QImage()

    def paintEvent(self, event):
        painter = QPainter(self)
        painter.fillRect(self.rect(), Qt.GlobalColor.black)
        if not self.image.isNull():
            size = self.image.size().scaled(self.size(), Qt.AspectRatioMode.KeepAspectRatio)
            painter.drawImage((self.width()-size.width())//2, (self.height()-size.height())//2,
                              self.image.scaled(size, Qt.AspectRatioMode.IgnoreAspectRatio, Qt.TransformationMode.SmoothTransformation))


class MainWindow(QMainWindow):
    def __init__(self, preview=False):
        super().__init__()
        self.preview_mode = preview
        self.setWindowTitle("Any2Odd · Experimental macOS / Linux")
        self.setWindowIcon(QIcon(str(ROOT / "assets/Any2Odd.png")))
        self.resize(900, 980)
        self.setMinimumSize(700, 560)
        self.settings_path = Path(QStandardPaths.writableLocation(QStandardPaths.StandardLocation.AppConfigLocation)) / "portable-settings.json"
        try:
            self.preferences = {} if preview else json.loads(self.settings_path.read_text(encoding="utf-8"))
        except (OSError, ValueError):
            self.preferences = {}
        self.dark = bool(self.preferences.get("dark", False))
        self.capture = self.session = self.sink = None
        self.output = Output()
        self.current = self.result = QImage()
        self.active = False
        self.last_frame = 0
        self.quitting = False
        self.topics = json.loads((ROOT / "Documentation/help.json").read_text(encoding="utf-8"))
        self.pages = QStackedWidget()
        shell = QWidget(); outer = QVBoxLayout(shell)
        header = QHBoxLayout()
        logo = QLabel(); logo.setPixmap(QPixmap(str(ROOT / "assets/Any2Odd-wordmark.png")).scaledToWidth(310, Qt.TransformationMode.SmoothTransformation));header.addWidget(logo);header.addStretch()
        for text, action in [("Controls", lambda:self.pages.setCurrentIndex(0)), ("Help", self.show_help), ("Donate", lambda:self.pages.setCurrentIndex(2)), ("Light / Dark", self.toggle_theme)]:
            header.addWidget(self.button(text, action))
        outer.addLayout(header)
        notice = self.label("EXPERIMENTAL · Not tested on macOS or Linux. Stereo preview/conversion only; Samsung AI and calibrated Odyssey output are unavailable.")
        notice.setObjectName("notice");outer.addWidget(notice);outer.addWidget(self.pages)
        self.setCentralWidget(shell)
        self.build_controls();self.build_help();self.build_donate()
        self.tray = QSystemTrayIcon(self.windowIcon(), self)
        self.menu = QMenu(self)
        for text, action in [("Show controls",self.restore),("Start / stop output",self.toggle_output),("Stop output",self.stop),("Help",self.show_help),("Donate",lambda:self.restore_page(2)),("Light / Dark",self.toggle_theme),("Quit completely",self.quit)]:
            self.menu.addAction(text,action)
        self.tray.setContextMenu(self.menu);self.tray.setToolTip("Any2Odd · experimental")
        self.tray.activated.connect(lambda reason:self.restore() if reason in (QSystemTrayIcon.ActivationReason.Trigger,QSystemTrayIcon.ActivationReason.DoubleClick) else None)
        if not preview and QSystemTrayIcon.isSystemTrayAvailable():self.tray.show()
        self.setContextMenuPolicy(Qt.ContextMenuPolicy.CustomContextMenu)
        self.customContextMenuRequested.connect(lambda point:self.menu.popup(self.mapToGlobal(point)))
        # Local shortcuts work when these controls have focus. No global hook is claimed.
        for shortcut, action in [("F1",self.show_help),("Ctrl+Alt+F10",self.toggle_output),("Ctrl+Alt+F9",self.restore)]:
            item=QAction(self);item.setShortcut(shortcut);item.triggered.connect(action);self.addAction(item)
        QApplication.instance().screenRemoved.connect(self.screens_changed)
        QApplication.instance().screenAdded.connect(self.screens_changed)
        self.apply_theme()
        self.show_topic(next(i for i,t in enumerate(self.topics) if t['Id']=='portable'))

    @staticmethod
    def button(text, callback):
        button=QPushButton(text);button.clicked.connect(callback);return button

    @staticmethod
    def label(text):
        label=QLabel(text);label.setWordWrap(True);return label

    def scroll_page(self):
        scroll=QScrollArea();scroll.setWidgetResizable(True);widget=QWidget();layout=QVBoxLayout(widget);scroll.setWidget(widget);self.pages.addWidget(scroll);return layout

    def build_controls(self):
        layout=self.scroll_page()
        self.status=self.label("Ready. Open a stereo image or choose a source screen. Output starts only when requested.");layout.addWidget(self.status)
        box=QGroupBox("SOURCE & INPUT");form=QFormLayout(box)
        self.source=QComboBox();self.source.addItems(["Image file (PNG / JPEG / BMP)","Live screen (experimental)"]);form.addRow("Capture",self.source)
        self.file_button=self.button("Open image…",self.open_image);form.addRow("File",self.file_button)
        self.source_screen=QComboBox();self.target_screen=QComboBox();self.refresh_screens();form.addRow("Source screen",self.source_screen)
        self.input=QComboBox();self.input.addItems(["Side-by-side stereo pair","2D image (flat preview, no AI)"]);form.addRow("Input format",self.input)
        self.half=QCheckBox("Half-width SBS: stretch each eye for red/cyan output");self.half.setChecked(True);form.addRow(self.half)
        self.swap=QCheckBox("Swap left and right eyes");form.addRow(self.swap);layout.addWidget(box)
        box=QGroupBox("OUTPUT & BEHAVIOR");form=QFormLayout(box)
        self.format=QComboBox();self.format.addItems(["Red/cyan anaglyph (requires red/cyan glasses)","Side-by-side stereo (requires a matching viewer)"]);form.addRow("Output format",self.format)
        form.addRow("Output display",self.target_screen)
        form.addRow(self.label("A normal monitor shows anaglyph with glasses, or two side-by-side images. This output does not activate the Odyssey's calibrated glasses-free mode."))
        disabled=QPushButton("Samsung AI depth / pop-out — Windows backend required");disabled.setEnabled(False);form.addRow(disabled)
        self.to_tray=QCheckBox("Minimize and close to tray when a tray icon is available");self.to_tray.setChecked(bool(self.preferences.get("tray",True)));self.to_tray.toggled.connect(self.save);form.addRow(self.to_tray)
        form.addRow(self.label("Automatic start and global Windows shortcuts are not enabled in this experimental build. Native title bars keep window movement and resizing available on each OS."))
        row=QHBoxLayout();self.start_button=self.button("Start output",self.toggle_output);row.addWidget(self.start_button);row.addWidget(self.button("Stop output",self.stop));row.addWidget(self.button("Save preview PNG…",self.export));form.addRow(row);layout.addWidget(box)
        self.preview=QLabel("Image preview");self.preview.setAlignment(Qt.AlignmentFlag.AlignCenter);self.preview.setMinimumHeight(230);layout.addWidget(self.preview)
        row=QHBoxLayout();row.addWidget(self.button("Minimize to tray",self.hide_to_tray));row.addWidget(self.button("Quit completely",self.quit));layout.addLayout(row);layout.addStretch()
        self.source.currentIndexChanged.connect(self.selection_changed)
        self.source_screen.currentIndexChanged.connect(self.selection_changed)
        self.target_screen.currentIndexChanged.connect(self.selection_changed)
        for signal in (self.input.currentIndexChanged,self.format.currentIndexChanged,self.half.toggled,self.swap.toggled):signal.connect(self.update_image)

    def refresh_screens(self):
        self.screens=QApplication.screens()
        for combo in (self.source_screen,self.target_screen):
            combo.clear()
            for i,screen in enumerate(self.screens):combo.addItem(f"{i+1}. {screen.name()} · {screen.size().width()} × {screen.size().height()}")
        if len(self.screens)>1:self.target_screen.setCurrentIndex(1)

    def screens_changed(self,*args):
        self.stop();self.source_screen.blockSignals(True);self.target_screen.blockSignals(True)
        self.refresh_screens();self.source_screen.blockSignals(False);self.target_screen.blockSignals(False)
        self.status.setText("Display configuration changed. Choose your screens again before starting output.")

    def selection_changed(self,*args):
        self.stop();self.current=QImage();self.result=QImage();self.preview.setText("Select a source and start output.")

    def open_image(self):
        path,_=QFileDialog.getOpenFileName(self,"Open source image","","Images (*.png *.jpg *.jpeg *.bmp)")
        if not path:return
        image=QImage(path)
        if image.isNull():self.status.setText("Could not decode this image.");return
        self.stop();self.source.setCurrentIndex(0);self.current=image;self.update_image();self.status.setText("Image loaded. Choose the correct input layout before starting output.")

    def update_image(self,*args):
        if self.current.isNull():return
        try:
            layout="flat" if self.input.currentIndex()==1 else ("anaglyph" if self.format.currentIndex()==0 else "sbs")
            self.result=convert(self.current,layout,self.half.isChecked(),self.swap.isChecked())
            self.preview.setPixmap(QPixmap.fromImage(self.result).scaled(720,300,Qt.AspectRatioMode.KeepAspectRatio,Qt.TransformationMode.SmoothTransformation))
            self.output.image=self.result;self.output.update()
        except ValueError as error:self.stop();self.status.setText(str(error))

    def toggle_output(self):
        if self.active:self.stop();return
        if not self.screens:self.status.setText("No output display is available.");return
        if self.source.currentIndex()==0 and self.current.isNull():self.status.setText("Open an image first.");return
        if self.source.currentIndex()==1:
            if os.environ.get("XDG_SESSION_TYPE","").lower()=="wayland" or QApplication.platformName().lower().startswith("wayland"):
                self.status.setText("Live screen output is disabled on Wayland until source/output exclusion is verified. Image-file conversion remains available.");return
            if (self.source_screen.currentIndex()==self.target_screen.currentIndex() or
                self.screens[self.source_screen.currentIndex()].geometry().intersects(self.screens[self.target_screen.currentIndex()].geometry())):
                self.status.setText("Live screen capture needs a different output display to prevent capture feedback. Choose two separate screens, or use an image file.");return
            self.current=QImage();self.result=QImage();self.output.image=QImage()
        self.output.setGeometry(self.screens[self.target_screen.currentIndex()].geometry());self.active=True;self.start_button.setText("Stop output")
        if self.source.currentIndex()==0:
            self.update_image()
            if not self.active:return
            self.output.show()
            self.status.setText("Output active. Use Stop output or the tray menu to close it. Click-through behavior is experimental on this OS.")
        else:
            try:
                from PySide6.QtMultimedia import QScreenCapture,QMediaCaptureSession,QVideoSink
                self.capture=QScreenCapture(self);self.session=QMediaCaptureSession(self);self.sink=QVideoSink(self)
                self.session.setScreenCapture(self.capture);self.session.setVideoSink(self.sink)
                self.capture.errorOccurred.connect(self.capture_error);self.sink.videoFrameChanged.connect(self.frame)
                self.capture.setScreen(self.screens[self.source_screen.currentIndex()]);self.capture.start()
                self.status.setText("Waiting for screen capture permission/frames. The output appears after the first frame. Stop cancels capture.")
            except Exception as error:self.stop();self.status.setText("Capture could not start: "+str(error))

    def frame(self,frame):
        if not self.active or time.monotonic()-self.last_frame<1/15:return
        if self.screens[self.source_screen.currentIndex()].geometry().intersects(self.output.geometry()):
            self.stop();self.status.setText("Display layout changed and output overlaps the source. Capture stopped to prevent feedback.");return
        image=frame.toImage()
        if image.isNull():return
        self.last_frame=time.monotonic()
        self.current=image.scaled(1920,1080,Qt.AspectRatioMode.KeepAspectRatio,Qt.TransformationMode.SmoothTransformation)
        # Maintain an even stereo width after bounded scaling.
        if self.current.width()%2:self.current=self.current.copy(0,0,self.current.width()-1,self.current.height())
        self.update_image()
        if self.active:self.output.show();self.status.setText("Live output active · at most 15 preview updates/s · no AI depth or panel calibration.")

    def capture_error(self,*args):
        message=self.capture.errorString() if self.capture else "Unknown capture error"
        self.stop();self.status.setText("Capture stopped: "+message)

    def stop(self):
        self.active=False;self.output.hide();self.output.image=QImage();self.start_button.setText("Start output")
        if self.capture:
            capture,session,sink=self.capture,self.session,self.sink
            self.capture=self.session=self.sink=None
            try:capture.errorOccurred.disconnect(self.capture_error)
            except (RuntimeError,TypeError):pass
            if sink:
                try:sink.videoFrameChanged.disconnect(self.frame)
                except (RuntimeError,TypeError):pass
            capture.stop();capture.deleteLater()
            if session:session.deleteLater()
            if sink:sink.deleteLater()
        self.status.setText("Output stopped. No capture is running.")

    def export(self):
        if self.result.isNull():self.status.setText("Load an image or capture a frame first.");return
        path,_=QFileDialog.getSaveFileName(self,"Export converted preview","Any2Odd-preview.png","PNG (*.png)")
        if path:self.status.setText("Preview saved." if self.result.save(path,"PNG") else "Could not save the preview.")

    def build_help(self):
        widget=QWidget();layout=QHBoxLayout(widget);self.topic_list=QListWidget();self.topic_list.setMaximumWidth(250)
        for topic in self.topics:self.topic_list.addItem(topic['Title'])
        self.article=QTextBrowser();self.article.setOpenExternalLinks(True);layout.addWidget(self.topic_list);layout.addWidget(self.article,1)
        self.topic_list.currentRowChanged.connect(self.show_topic);self.pages.addWidget(widget)

    def show_topic(self,index):
        if index<0:return
        topic=self.topics[index];content=f"<h1>{html.escape(topic['Title'])}</h1>"
        if topic['Id']!='portable':content+="<p><b>Windows edition reference.</b> See Experimental macOS / Linux for this app's available features.</p>"
        content+=''.join('<p>'+html.escape(p).replace('\n','<br>')+'</p>' for p in topic['Body'].split('\n\n'))
        for link in topic.get('Links',[]):content+=f'<p><a href="{html.escape(link["Url"],quote=True)}">{html.escape(link["Label"])}</a></p>'
        self.article.setHtml(content)

    def build_donate(self):
        layout=self.scroll_page();layout.addWidget(self.label("Support Any2Odd · the same maintainer destinations and original QR assets as PortalAI."))
        sponsor=self.button("",lambda:QDesktopServices.openUrl(QUrl(SPONSOR)));sponsor.setIcon(QIcon(str(ROOT/'Documentation/assets/sponsor-button.svg')));sponsor.setIconSize(QSize(252,48));sponsor.setAccessibleName("Sponsor on GitHub");layout.addWidget(sponsor)
        for name,network,file,value in DONATIONS:
            card=QGroupBox(name+" · "+network);row=QHBoxLayout(card);label=QLabel();image=QImage(str(ROOT/'assets/donations'/file))
            # Present the original PNG intact; no inferred QR regeneration.
            label.setPixmap(QPixmap.fromImage(image).scaled(220,250,Qt.AspectRatioMode.KeepAspectRatio,Qt.TransformationMode.SmoothTransformation));row.addWidget(label)
            details=QVBoxLayout();details.addWidget(self.label(value));details.addWidget(self.button("Copy destination",lambda checked=False,v=value:self.copy_destination(v)));row.addLayout(details);layout.addWidget(card)
        self.donation_status=self.label("Copying a destination does not send a payment.");layout.addWidget(self.donation_status);layout.addStretch()

    def copy_destination(self,value):QApplication.clipboard().setText(value);self.donation_status.setText("Destination copied to clipboard.")
    def save(self,*args):
        if self.preview_mode:return
        try:
            self.settings_path.parent.mkdir(parents=True,exist_ok=True)
            temporary=self.settings_path.with_suffix('.tmp');temporary.write_text(json.dumps({'dark':self.dark,'tray':self.to_tray.isChecked()}),encoding='utf-8');temporary.replace(self.settings_path)
        except OSError as error:self.status.setText("Could not save desktop preferences: "+str(error))
    def apply_theme(self):
        QApplication.instance().setStyleSheet(theme(self.dark))
        if self.topic_list.currentRow()>=0:self.show_topic(self.topic_list.currentRow())
    def toggle_theme(self):self.dark=not self.dark;self.apply_theme();self.save()
    def restore(self):self.showNormal();self.raise_();self.activateWindow();self.pages.setCurrentIndex(0)
    def restore_page(self,index):self.restore();self.pages.setCurrentIndex(index)
    def show_help(self):self.restore_page(1);self.topic_list.setCurrentRow(next(i for i,t in enumerate(self.topics) if t['Id']=='portable'))
    def hide_to_tray(self):
        if self.tray.isVisible() and QSystemTrayIcon.isSystemTrayAvailable():self.hide()
        else:self.showMinimized()
    def changeEvent(self,event):
        super().changeEvent(event)
        if hasattr(self,'tray') and self.isMinimized() and self.to_tray.isChecked() and self.tray.isVisible() and QSystemTrayIcon.isSystemTrayAvailable():QTimer.singleShot(0,self.hide)
    def closeEvent(self,event):
        if not self.quitting and self.to_tray.isChecked() and self.tray.isVisible() and QSystemTrayIcon.isSystemTrayAvailable():event.ignore();self.hide()
        else:self.stop();self.save();self.tray.hide();event.accept()
    def quit(self):self.quitting=True;self.stop();self.save();self.tray.hide();self.close();QApplication.quit()


def main():
    application=QApplication(sys.argv);application.setApplicationName("Any2OddExperimental");application.setOrganizationName("Zhandragon")
    window=MainWindow();window.show();application.aboutToQuit.connect(window.stop)
    return application.exec()


if __name__=='__main__':sys.exit(main())
