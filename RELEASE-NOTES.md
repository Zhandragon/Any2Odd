# Any2Odd 0.5.0

First public release of the renamed Odyssey 3D Companion: an application that minimizes to the tray and provides Any2C1-style continuous 3D for capturable content on a Samsung Odyssey G90XF.

- Desktop, Start menu, tray and application windows now use **Any2Odd** with the high-contrast yellow/cyan glasses icon.
- Whole-desktop AI conversion, click-through game/desktop controls, depth/pop-out settings, manual side-by-side mode and tray/taskbar toggles.
- Preserves the capture-feedback, cursor, borderless-capture and taskbar auto-hide fixes from 0.4.3.
- Existing preferences are retained during an upgrade.
- GPL-3.0-only source, pinned dependency restore, setup README and offline guide included.

**Download `Any2Odd-0.5.0-windows-x64.zip` for the prebuilt installer package.** Extract the entire ZIP, quit the running Companion/Any2Odd, then follow README.md. Source code ZIP/TAR downloads require a build. The `.sha256` file gives the installer ZIP checksum. Installation does not reboot Windows.

**Experimental compatibility:** G90XF + RTX 4090; Windows 11 24H2+; installed Odyssey 3D Hub 1.5.1 and matching engine hash. Samsung/Leia binaries, AI models and calibration data are not bundled. Protected video and exclusive fullscreen are unsupported, conversion adds latency, and the output is SDR. Automatic side-by-side detection is not implemented.

Validation: clean build and geometry/source-isolation checks passed; installer preflight and an upgrade from 0.4.3 succeeded. Runtime diagnostics confirmed active desktop conversion, click-through/capture-excluded output, borderless permission and native taskbar-entry cleanup after the rename. The five shipped EXE/DLL files had no detections in a ClamAV scan using the available local definitions. A clean scan is not a security guarantee; the source is available for review. Optical depth and broad hardware compatibility are not automated test results.
