# Platform support and experimental ports

**Release 0.7.1: macOS and Linux versions are experimental and have not been tested on their target operating systems.** The shared Python/Qt app implements limited stereo viewing/conversion. The native Windows application retains the existing Samsung conversion backend.

| Capability | Windows x64 | Experimental macOS / Linux |
| --- | --- | --- |
| Light/dark controls, Help, Donate | Native WinForms, ornate custom chrome | Shared Qt frontend, themed controls, native OS title bar |
| Tray/menu | Windows notification icon | Qt tray where supported; taskbar/dock fallback |
| Ordinary 2D → AI depth | Installed Samsung engine | Unavailable; ordinary 2D stays flat |
| Existing SBS input | Samsung/Leia calibrated output | SBS viewing or red/cyan anaglyph; optional eye swap |
| G90XF tracking, calibration and lens output | Samsung/Leia runtime required | Not implemented or verified |
| Whole-desktop source | Windows Graphics Capture | Qt screen capture, separate output display required; Wayland live output disabled |
| Single screen capture/output | Capture exclusion prevents feedback | Refused for live capture; image-file output allowed |
| Click-through output | Existing Windows implementation | Native Qt window flags requested; target-OS behavior untested |
| Global shortcuts / automatic start / recovery | Existing implementation | Local shortcuts only; output starts manually; no automatic recovery |
| Update mechanism | Version-aware installer, preserves 0.5.0+ settings | Extract a new bundle; shared per-user desktop preferences retained |

[Portable setup and usage](../portable/START-HERE.md) · [Illustrated Help](README.md#portable)

## What is actually distributed

- `Any2Odd-0.7.1-windows-x64.zip`: compiled native app and version-aware Windows installer.
- `Any2Odd-0.7.1-macos-experimental.zip`: source launch bundle with `Launch-macOS.command` for macOS 13+ Intel/Apple silicon and Python 3.10–3.13.
- `Any2Odd-0.7.1-linux-experimental.tar.gz`: source launch bundle with `launch-linux.sh`, targeting x86-64/glibc 2.34+ and Python 3.10–3.13 in a graphical desktop session.

The portable launchers install pinned PySide6/Qt wheels into `.venv` in their extracted folders. They do not bundle a Python interpreter, install Odyssey Hub, register a system service, or promise optical Odyssey output. Binary-wheel platform requirements come from [PySide6 6.10.1's published files](https://pypi.org/project/PySide6/6.10.1/#files).

## Existing Windows dependencies

The original `Companion.cs` engine directly loads `C:\Program Files\Odyssey 3D Hub\Conversion\ConversionWrapper.dll`, uses a hash-pinned native `Start`/`Stop` interface and resolves Samsung/Leia assets alongside the worker. The capture and overlay implementations rely on Windows Graphics Capture, Direct3D 11/DXGI, DWM and Win32 input/window styles. The GUI uses .NET Framework/WinForms. Those components are not portable by changing a ZIP's extension.

The portable code is separate and never imports or edits the Windows projection/overlay implementation. It performs only explicit layout conversion of existing stereo images; its default stereo input is not a claim of automatic stereo detection.

## Native backend research, checked 2026-11-05

- **Linux:** [DisplayXR's Leia plug-in](https://github.com/DisplayXR/displayxr-leia-plugin) describes a Linux integration with the SR SDK. This is a possible route to native calibrated output, not evidence that Any2Odd or this particular G90XF setup works. Runtime version, camera access, display calibration, lens switching and packaging would need validation on the target machine. The current portable app does not load that plug-in.
- **macOS:** [DisplayXR's architecture](https://displayxr.org/architecture) documents its macOS compositor against a simulated display backend. That does not establish a working G90XF hardware path. [Apple ScreenCaptureKit](https://developer.apple.com/documentation/screencapturekit) offers native capture, but capture alone cannot replace tracking or calibration.
- **Capture:** [Qt QScreenCapture](https://doc.qt.io/qtforpython-6/PySide6/QtMultimedia/QScreenCapture.html) supplies the portable app's screen source. Wayland's [ScreenCast portal](https://flatpak.github.io/xdg-desktop-portal/docs/doc-org.freedesktop.portal.ScreenCast.html) negotiates source selection and PipeWire streams. Reliable capture exclusion and overlay placement are compositor-dependent, so this release limits live output to distinct screens on non-Wayland sessions.

These are candidate integration paths, not dependencies installed by the experimental launcher. Any future native AI/optical backend should be optional, advertise its actual capabilities, and preserve the existing Windows backend.

## Acceptance still needed

On each real target OS: install from a fresh archive; verify dependencies and permission prompts; view known SBS/anaglyph patterns; test tray availability and no-tray fallback; start/stop capture between separate monitors; verify focus/click-through; disconnect a display; change scaling; test update/removal and retained settings. A calibrated-output backend additionally needs a person at the G90XF to verify each eye, tracking and lens transitions.

Current validation covers the shared stereo math, source/output guards, UI construction and rendering on **Windows using Qt's offscreen platform**, plus package contents and launcher syntax. It does not substitute for macOS/Linux or optical tests.
