"""Host-independent logic/UI checks. Windows offscreen execution is not OS acceptance."""
import os
os.environ['QT_QPA_PLATFORM']='offscreen'
from pathlib import Path
import sys
import unittest
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'portable'))
from PySide6.QtWidgets import QApplication
from PySide6.QtGui import QImage,QColor,QFontDatabase
from stereo import convert
from app import MainWindow,DONATIONS


class StereoTests(unittest.TestCase):
    def setUp(self):
        self.image=QImage(4,2,QImage.Format.Format_RGBA8888)
        for y in range(2):
            for x in range(4):self.image.setPixelColor(x,y,QColor(20,40,60) if x<2 else QColor(100,120,140))

    def test_anaglyph_channels_and_geometry(self):
        result=convert(self.image,'anaglyph',False)
        self.assertEqual((result.width(),result.height()),(2,2))
        self.assertEqual(result.pixelColor(0,0).getRgb(),(20,120,140,255))
        self.assertEqual(convert(self.image,'anaglyph',True).width(),4)
        self.assertEqual(convert(self.image,'anaglyph',False,True).pixelColor(0,0).getRgb(),(100,40,60,255))

    def test_sbs_swapping_and_flat_passthrough(self):
        self.assertEqual(convert(self.image,'sbs').pixelColor(0,0),self.image.pixelColor(0,0))
        self.assertEqual(convert(self.image,'sbs',swap=True).pixelColor(0,0),self.image.pixelColor(3,0))
        self.assertEqual(convert(self.image,'flat'),self.image)
        with self.assertRaises(ValueError):convert(QImage(3,2,QImage.Format.Format_RGB32),'sbs')
        with self.assertRaises(ValueError):convert(QImage(),'flat')


class PortableUiTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.app=QApplication.instance() or QApplication([])
        # Qt's offscreen Windows plugin has no native font enumeration. Load
        # the host font for test rendering only; no font is redistributed.
        font=Path(os.environ.get('WINDIR',''))/'Fonts/segoeui.ttf'
        if font.is_file():QFontDatabase.addApplicationFont(str(font))

    def setUp(self):self.window=MainWindow(preview=True)
    def tearDown(self):self.window.quitting=True;self.window.close();self.window.deleteLater();self.app.processEvents()

    def test_no_autostart_help_and_resources(self):
        self.assertFalse(self.window.active)
        self.assertIsNone(self.window.capture)
        self.window.show_help()
        self.assertIn('EXPERIMENTAL AND NOT TESTED',self.window.article.toPlainText())
        self.window.toggle_theme()
        self.assertFalse(self.window.active)
        for _,_,file,_ in DONATIONS:self.assertFalse(QImage(str(ROOT/'assets/donations'/file)).isNull())

    def test_same_screen_and_wayland_guards(self):
        self.window.source.setCurrentIndex(1)
        self.window.source_screen.setCurrentIndex(0);self.window.target_screen.setCurrentIndex(0)
        self.window.toggle_output()
        self.assertFalse(self.window.active)
        self.assertIsNone(self.window.capture)
        old=os.environ.get('XDG_SESSION_TYPE')
        try:
            os.environ['XDG_SESSION_TYPE']='wayland';self.window.toggle_output()
            self.assertIn('disabled on Wayland',self.window.status.text())
            self.assertFalse(self.window.active)
        finally:
            if old is None:os.environ.pop('XDG_SESSION_TYPE',None)
            else:os.environ['XDG_SESSION_TYPE']=old

    def test_flat_preview_and_source_change_stop(self):
        self.window.current=QImage(4,2,QImage.Format.Format_RGB32);self.window.current.fill(QColor(10,20,30))
        self.window.input.setCurrentIndex(1);self.window.update_image()
        self.assertEqual(self.window.result.pixelColor(0,0).getRgb(),(10,20,30,255))
        self.window.active=True;self.window.source.setCurrentIndex(1)
        self.assertFalse(self.window.active)
        self.assertTrue(self.window.result.isNull())

    def test_render_frontend(self):
        self.window.show();self.app.processEvents()
        self.window.grab().save(str(ROOT/'build/portable-controls.png'))
        self.window.show_help();self.app.processEvents()
        self.window.grab().save(str(ROOT/'build/portable-help.png'))
        self.assertIsNone(self.window.capture)

    def test_image_output_start_stop_and_display_removal(self):
        self.window.current=QImage(4,2,QImage.Format.Format_RGB32);self.window.current.fill(QColor(20,30,40))
        self.window.toggle_output();self.app.processEvents()
        self.assertTrue(self.window.active)
        self.assertTrue(self.window.output.isVisible())
        self.assertFalse(self.window.output.image.isNull())
        self.window.screens_changed()
        self.assertFalse(self.window.active)
        self.assertFalse(self.window.output.isVisible())

    def test_capture_api_available_without_starting_capture(self):
        from PySide6.QtMultimedia import QScreenCapture,QMediaCaptureSession,QVideoSink
        capture=QScreenCapture();session=QMediaCaptureSession();sink=QVideoSink()
        session.setScreenCapture(capture);session.setVideoSink(sink)
        self.assertFalse(capture.isActive())
        session.setScreenCapture(None);session.setVideoSink(None)


if __name__=='__main__':unittest.main()
