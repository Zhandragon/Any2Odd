"""Stereo-layout conversion only. No AI depth estimation or panel calibration."""
from PySide6.QtGui import QImage, QPainter
from PySide6.QtCore import Qt


def convert(image, layout="sbs", half_width=True, swap=False):
    if image.isNull():
        raise ValueError("The source contains no image.")
    if layout == "flat":
        return image.copy()
    if image.width() < 2 or image.width() % 2:
        raise ValueError("Side-by-side input must have an even width of at least two pixels.")
    eye_width = image.width() // 2
    left = image.copy(0, 0, eye_width, image.height())
    right = image.copy(eye_width, 0, eye_width, image.height())
    if swap:
        left, right = right, left
    if layout == "sbs":
        result = QImage(image.size(), QImage.Format.Format_RGB32)
        painter = QPainter(result)
        painter.drawImage(0, 0, left)
        painter.drawImage(eye_width, 0, right)
        painter.end()
        return result
    if layout != "anaglyph":
        raise ValueError("Unknown stereo output layout.")
    if half_width:
        left = left.scaled(image.width(), image.height(), Qt.AspectRatioMode.IgnoreAspectRatio, Qt.TransformationMode.SmoothTransformation)
        right = right.scaled(left.size(), Qt.AspectRatioMode.IgnoreAspectRatio, Qt.TransformationMode.SmoothTransformation)
    left = left.convertToFormat(QImage.Format.Format_RGBA8888)
    right = right.convertToFormat(QImage.Format.Format_RGBA8888)
    # RGBA8888 is byte ordered on all supported architectures. Four-byte rows
    # need no padding, so channel slices preserve exact red/cyan eye separation.
    l = bytes(left.constBits())
    r = bytes(right.constBits())
    pixels = bytearray(r)
    pixels[0::4] = l[0::4]
    pixels[3::4] = bytes([255]) * (left.width() * left.height())
    return QImage(pixels, left.width(), left.height(), left.bytesPerLine(), QImage.Format.Format_RGBA8888).copy()
