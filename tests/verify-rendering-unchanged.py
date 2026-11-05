"""Compare protected rendering code with the current deployed 0.5.0 baseline."""
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
def baseline(name):
    return subprocess.check_output(['git', 'show', 'v0.5.0:' + name], cwd=ROOT).decode('utf-8-sig').replace('\r\n', '\n')
for name in ('OverlayController.cs', 'RendererTaskbar.cs', 'Projection.cs',
             'DesktopCapture.cs', 'GpuDesktopCapture.cs', 'GpuSurface.cs',
             'DisplayColor.cs', 'WgcFrameSource.cs', 'TaskbarMenu.cs'):
    assert (ROOT / name).read_text(encoding='utf-8-sig') == baseline(name), name
current = (ROOT / 'Companion.cs').read_text(encoding='utf-8-sig')
old = baseline('Companion.cs')
for start, end in [(' static class Files {', ' sealed class MainForm'),
                   (' static class Program {', None)]:
    # MainForm is now partial; everything before it (including Mirror/Engine) is protected.
    actual_end = ' sealed partial class MainForm' if end else None
    expected = old[old.index(start):old.index(end) if end else len(old)]
    actual = current[current.index(start):current.index(actual_end) if actual_end else len(current)]
    assert actual == expected, start
for start, end in [('  void StartConversion()', '  void ShowControls()'),
                   ('  protected override void WndProc(ref Message m){if(m.Msg==0x312)', '  protected override void Dispose')]:
    assert current[current.index(start):current.index(end, current.index(start))] == old[old.index(start):old.index(end, old.index(start))], start
print('PASS: capture, projection, overlay, click-through, renderer lifecycle, IPC and hotkeys unchanged')
