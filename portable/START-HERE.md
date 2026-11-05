# Any2Odd — experimental macOS / Linux

**These versions have not been tested on macOS or Linux.** They are source-based launch bundles with a small shared Qt frontend. Windows remains the full Samsung Odyssey converter. No Odyssey Hub, Samsung monitor or Any2C1 is needed for the portable image-viewing features.

## Launch

1. Extract the whole archive to a writable folder. Keep `portable`, `assets` and `Documentation` alongside the launcher.
2. Provide **Python 3.10–3.13**, with `venv` and `pip`. Python can be obtained from [python.org](https://www.python.org/downloads/); on Linux, use your distribution's Python packages (including `python3-venv` on Debian/Ubuntu).
3. **macOS:** double-click `Launch-macOS.command`, or run `sh Launch-macOS.command` from Terminal in the extracted folder. The bundle is unsigned and not notarized; no security-setting changes are performed by the launcher.
4. **Linux:** run `sh launch-linux.sh` in the extracted folder from a graphical desktop session. The initial target is x86-64 with glibc 2.34 or newer and Qt's desktop dependencies. Install your distribution's Qt/XCB/Wayland support libraries if Qt reports a missing platform plug-in. The pinned macOS wheel targets macOS 13 or newer on Intel or Apple silicon.

The first launch creates `.venv` inside the extracted folder and downloads **PySide6 6.10.1** and its matching Qt packages from PyPI. Internet is needed for that download; later image viewing can run offline. No administrator command, global Python install, background service or login-startup entry is added. If Python or your OS has no compatible binary wheel, setup stops with an error rather than compiling a different runtime silently.

## What works in the experimental implementation

- Light/dark controls, tray/menu commands, internal written Help, Donate and PortalAI's sponsor button/QR destinations.
- Open PNG/JPEG/BMP images; choose an existing side-by-side stereo pair or a flat 2D image.
- Convert existing side-by-side stereo into **red/cyan anaglyph**, preserve SBS output, swap eyes, and export the resulting PNG. Red/cyan output needs matching glasses; SBS needs a viewer/display configured for that format. Neither activates the G90XF's calibrated glasses-free mode.
- **Half-width SBS** stretches each eye to the full original frame width for anaglyph. Uncheck it for full-width SBS. Flat input stays flat; no artificial depth is claimed.
- Optional live screen capture through Qt, bounded to 1920 × 1080 and at most 15 preview updates per second. Select a **different output display**; the same-screen path is refused to prevent feedback. macOS may request Screen Recording permission. Live output on Wayland is disabled until capture-source/output exclusion is verified; static images remain available.
- A frameless output window requests always-on-top and mouse click-through. Actual focus, click-through and display placement remain untested on either target OS. Stop from the controls or tray. Native controls-window chrome preserves OS window management.

## What is unavailable

Samsung's Windows AI depth converter, calibrated Odyssey/Leia optical output, eye tracking, Windows global hotkeys, selected-window/follow-active capture, automatic start and automatic worker recovery are not ported. Depth and pop-out are therefore unavailable. The Windows app's projection/click-through implementation is unchanged.

Anaglyph conversion rearranges the two eyes already present in stereo input; it does not reconstruct depth from ordinary 2D. This bundle is not an equivalent substitute for the Windows G90XF converter.

## Start, stop and tray behavior

Output starts only when you press **Start output**. Input/output-screen changes stop it. Disconnecting a monitor also stops output. **Stop output** ends capture and closes its window; **Quit completely** also exits the app. The tray preference saves immediately. If the desktop has no supported tray, minimize goes to the taskbar/dock and closing exits instead of hiding an unreachable application. F1 and Ctrl+Alt+F9/F10 are local shortcuts while the controls have focus, not system-wide hooks.

## Updates and removal

Extract a newer bundle into a new folder and launch that copy; do not copy an old `.venv` between OSes or folders. Desktop preferences live in Qt's per-user `Zhandragon/Any2OddExperimental` configuration location (`portable-settings.json`); Windows conversion settings are never read or overwritten. These source bundles do not use the Windows registry/version-aware installer. To remove them, quit and delete the extracted folder; remove the separate preference file only if you want a reset.

## Dependencies and validation

App source is GPL-3.0-only. PySide6/Qt is downloaded separately under its upstream open-source licenses; see [Qt for Python licensing](https://doc.qt.io/qtforpython-6/licenses.html). Dependency license files remain in `.venv`. Samsung/Leia binaries or models are not included. `portable/stereo.py` is an independent stereo-layout converter.

Shared conversion and UI smoke tests run on Windows using Qt's offscreen platform. Packaging and launcher syntax are checked separately. **No macOS/Linux launch, capture, tray, permission, optical-output or installer acceptance has been performed.** See `Documentation/PLATFORMS.md` for the remaining backend work and evidence.
