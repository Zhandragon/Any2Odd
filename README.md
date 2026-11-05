<img src="assets/Any2Odd-wordmark.png" alt="Any2Odd: 2n maps to 2n plus 1" width="520">

# Any2Odd

**Continuous desktop 3D for the Samsung Odyssey G90XF.**

Any2Odd is a Windows application that minimizes to the system tray and turns capturable on-screen content into glasses-free 3D: wallpaper, desktop icons, app windows, web video, and compatible borderless games. It aims for the always-on, turn-it-off-from-the-tray experience of Any2C1, using the Odyssey's own calibrated conversion engine.

It works around Odyssey 3D Hub's normal fullscreen/video-window workflow by feeding a continuous desktop capture into its renderer. There is no per-video approval step. Escape remains available to games, and changing focus or opening menus does not intentionally stop conversion. The 3D overlay passes mouse input through to the real desktop or game underneath.

**Experimental, hardware-specific software.** The tested configuration is an Odyssey G90XF + RTX 4090, Windows 11, and Odyssey 3D Hub 1.5.1. It is not a standalone converter for arbitrary monitors. Samsung's software, camera, tracking and calibration are still required. No Samsung binaries, AI models or calibration files are included.

[Download a release](https://github.com/Zhandragon/Any2Odd/releases) · [Report an issue](https://github.com/Zhandragon/Any2Odd/issues) · [GPL-3.0 license](LICENSE)

## Features

- Whole-desktop AI conversion, enabled by default when the app opens.
- PortalAI-inspired light and dark themes, rounded controls, gold filigree borders and custom desktop window controls. The theme choice is remembered separately from conversion settings.
- A mathematical `2n + 1` emblem, engraved wordmark and a readable `+1` variation for small tray icons.
- A themed tray/window right-click menu, gold scrollbars, internal Help (F1) and Donate panels. Donate uses the same maintainer QR codes as PortalAI, embedded unchanged.
- Right-click the tray icon to turn 3D on/off, switch input modes, restore the overlay, open controls or quit. The taskbar icon's jump list also has toggle and controls commands.
- Close/minimize to tray by default; configurable normal minimize behavior.
- Depth strength and pop-out controls. Applying them briefly restarts conversion.
- Manual side-by-side input mode, selected-window capture, and follow-active-window mode.
- Click-through output that can stay above apps without taking keyboard focus.
- GPU capture with a bounded frame queue, capture-output exclusion to avoid feedback trails, and exclusion of the captured hardware cursor to avoid a delayed duplicate pointer.
- Borderless capture when Windows grants permission; respects Windows taskbar auto-hide.

Automatic side-by-side detection is **not implemented**. Select side-by-side manually when the source already contains a stereo pair. Whole desktop + AI conversion is the recommended default.

## Requirements

| Component | Required / tested |
| --- | --- |
| OS | Windows 11 x64, 24H2 (build 26100) or newer; tested on build 26300 |
| Display | Samsung Odyssey 3D G90XF, primary monitor, landscape 3840 × 2160 |
| Connection | DisplayPort and the monitor's USB upstream connection; working `3D Stereo WebCam` |
| GPU | Tested on RTX 4090; this initial release passes a fixed 16,384-core hint to the native engine. Other GPUs are unvalidated |
| Samsung software | Odyssey 3D Hub 1.5.1 and its installed LeiaSR dependencies, in their default `C:\Program Files` locations |
| Runtime/build | .NET Framework 4.8, 64-bit Windows PowerShell 5.1 or newer |

The app and installer check the exact `ConversionWrapper.dll` SHA-256:

```text
0116b33697ca0192fbe26da96cb9e2592599c0cacfbb1129ae03e445cb3a840b
```

A different engine stops installation/conversion with a compatibility message. Do not remove this check to try an unknown engine. Samsung updates may require an Any2Odd update.

## Install

1. Install and configure Samsung Odyssey 3D Hub and verify that its ordinary 3D playback works. See [Samsung's setup guidance](https://www.samsung.com/uk/support/displays/how-to-use-3d-on-the-odyssey-3d-gaming-monitor/).
2. Download `Any2Odd-0.6.0-windows-x64.zip` from [Releases](https://github.com/Zhandragon/Any2Odd/releases) and extract it to a normal folder. The GitHub-generated **Source code** downloads need to be built first.
3. Stop Samsung's separate 3D player. The installer requests a clean shutdown of a running Any2Odd and waits for its worker. If it cannot stop, quit Any2Odd from its tray and retry.
4. Double-click **Install.cmd** and approve elevation **using the same Windows account** as the existing installation. Alternatively, open **64-bit PowerShell as administrator**, change to the extracted folder, and run:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\install.ps1
```

5. Open **Any2Odd** from its desktop or Start menu shortcut as your normal user. By default, conversion starts when the app opens. It does not register itself to start at Windows sign-in.

Administrator access is needed to install the worker beside Samsung's conversion engine. **No Windows restart is needed.** The installer adds its own worker and SharpDX dependencies; it does not patch Samsung's executables or DLLs. It preserves existing Any2Odd/Companion preferences.

For a read-only package/engine compatibility check, run `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\install.ps1 -CheckOnly`. This does not test the camera or prove optical 3D operation.

## Updating an existing installation

**Upgrade from 0.5.0 without uninstalling.** Extract the new ZIP into a new folder and run `Install.cmd` on the computer with your 3D display. The installer detects the old executable, reports `Upgrade Any2Odd 0.5.0.0 -> 0.6.0`, and updates the same app, worker, shortcuts and Windows Installed apps entry. Your preferences, logs and original Samsung overlay-settings backup remain in place.

`VERSION` is the single release-version source. The installer checks it against the packaged executable, reads the installed executable version (with registry/manifest fallbacks), and distinguishes a new install, upgrade and same-version repair. It refuses to replace a newer version unless you explicitly use `-AllowDowngrade`. Failed file or registration updates attempt to restore the previous installation; if rollback cannot finish, the error identifies the retained backup folder. Shared Samsung-folder dependencies keep their existing ownership status.

Do not run the older 0.5.0 installer to update a newer installation; that old installer predates version checks. No reboot is required.

## Controls

| Action | Control |
| --- | --- |
| Light / dark theme | Title-bar Light / Dark button, or tray/window context menu |
| Help | Title-bar Help menu, context menu, or **F1** |
| Donate | Title-bar Donate button or context menu |
| Move / resize | Drag the custom title bar / borders; double-click title bar to maximize or restore |
| Turn 3D on/off | Tray right-click → Turn 3D on/off, or **Ctrl+Alt+F10** |
| Show controls | Double-click tray icon, desktop icon, or **Ctrl+Alt+F9** |
| Restore overlay without taking game focus | **Ctrl+Alt+F8** |
| Change depth/pop-out | Adjust sliders → Apply settings / restart conversion |
| Convert ordinary content | Whole desktop + 2D → 3D automatically (AI) |
| Show an existing stereo pair | Side-by-side input (manual) |
| End the application | Tray → Quit, or Quit completely in the window |

Escape is not a stop hotkey. Use borderless/windowed games. Leave **Keep 3D above other apps without taking focus** enabled for continuous output. Minimize the controls when playing.

## How it works

```text
Primary desktop → Windows Graphics Capture → off-screen GPU surface
                → installed Samsung conversion engine → tracked 3D overlay
```

The source surface is wholly outside all physical monitor rectangles. Windows excludes the final 3D output from capture, preventing recursive feedback. Real input reaches the underlying applications. Samsung's installed engine provides AI depth conversion and calibrated display output; Any2Odd supplies capture, window/input handling, settings and tray controls.

The worker lives next to Samsung's engine because that engine resolves assets relative to its executable. Any2Odd uses an unpublished native interface, so compatibility is deliberately narrow. It does not inject into games or change their files.

## Limits and troubleshooting

- **Capture adds latency.** GPU capture reduced overhead in local testing, but this is not zero-latency native stereo rendering. Video/audio timing and responsiveness depend on GPU load, source and display. End-to-end latency has not been measured.
- **Not every surface can be captured.** Protected/DRM video, secure desktops, exclusive fullscreen, some overlays and apps that stop rendering when occluded may not work. This app does not bypass DRM, anti-cheat, camera or hardware requirements.
- **HDR input is normalized to SDR.** The conversion path is not native HDR; bright highlights can clip. Windows HDR settings are not changed.
- **AI depth is estimated.** Artifacts and inaccurate depth are possible. Ordinary screenshots cannot verify the monitor's optical 3D effect.
- **Yellow outline:** Windows may deny borderless-capture access. Conversion can continue with the capture indicator visible; the app does not disable Windows privacy protections.
- **No 3D / camera error:** check USB upstream, the camera in Device Manager, primary-display selection and 3840 × 2160 resolution. Confirm the Samsung player is stopped.
- **Overlay behind a game:** use Ctrl+Alt+F8 and enable “Keep 3D above other apps.” Exclusive fullscreen is unsupported.
- **Taskbar stays visible:** enable auto-hide in Windows. Any2Odd removes only its native renderer's stale taskbar entry, not other apps' notifications or Windows' taskbar behavior.
- **Hotkey conflict:** free Ctrl+Alt+F8/F9/F10 in other applications. Startup conversion is blocked if these controls cannot register.
- **Selected-window modes:** experimental; scaling a window does not remap mouse coordinates. Use whole-desktop mode for direct interaction and games.

Use **View logs** for local diagnostics. Review and redact usernames, window titles and file paths before attaching logs to a public issue. Include Windows build, GPU, Hub version, capture/input mode, and steps to reproduce.

## Build from source

On a compatible Windows installation:

```powershell
git clone https://github.com/Zhandragon/Any2Odd.git
cd Any2Odd
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

The build uses the .NET Framework C# compiler and Windows WinMetadata already on the machine; Visual Studio is not needed. `restore.ps1` downloads four **SharpDX 4.2.0** packages from NuGet, checks pinned SHA-256 hashes, and extracts their .NET 4.5 assemblies. SharpDX is an archived project. The icon and metallic wordmark are generated from `IconMaker.cs`. Small icons use a simplified `+1` mark; larger assets use `2n + 1`. No external font files are bundled.

Output: `build\Any2Odd.exe` plus four SharpDX DLLs. Donation QR images are embedded in the executable. The UI lives in `MainWindow.cs`, `DesktopTheme.cs`, `DesktopControls.cs`, `HelpContent.cs` and `DonationWindow.cs`; the capture and overlay surfaces do not inherit the desktop chrome. `python tools/export-guide.py` regenerates the offline HTML guide from the Help catalog when editing documentation. Use `package.ps1` to create the distributable ZIP and its checksum. All source needed to build Any2Odd is in this repository. The build produces a functionally equivalent executable; byte-for-byte reproducibility across machines/compiler metadata is not claimed.

See [TESTING.md](TESTING.md) for the checks and their scope, and [CHANGELOG.md](CHANGELOG.md) for the history.

## Installed files and removal

For compatibility with the former **Odyssey 3D Companion** name, internal directories and the worker filename remain:

| Item | Location |
| --- | --- |
| App | `%LOCALAPPDATA%\Programs\Odyssey3DCompanion\Any2Odd.exe` |
| Settings/logs | `%LOCALAPPDATA%\Odyssey3DCompanion` |
| Worker | `C:\Program Files\Odyssey 3D Hub\Conversion\OdysseyCompanionWorker.exe` |
| Original Samsung overlay settings backup | `%LOCALAPPDATA%\Odyssey3DCompanion\Samsung-OverlaySettings-before.json` |

Depth/overlay settings are written to Samsung's per-user `OverlaySettings.json`; its first existing version is backed up. Uninstall **Any2Odd** from Windows Installed apps, or run the installed `Uninstall.ps1`. The uninstaller removes its own files and unchanged dependencies it owns. It keeps Samsung's software, your preferences, logs and overlay-settings backup. It does not automatically restore that backup over newer Samsung preferences. Stop both players before restoring it manually if needed.

## License and acknowledgments

Any2Odd's own code and generated icon are licensed under **GPL-3.0-only**; see [LICENSE](LICENSE). Third-party MIT notices are in [THIRD-PARTY-LICENSES.txt](THIRD-PARTY-LICENSES.txt). Samsung/Leia software remains separately licensed and must be obtained independently; this repository grants no rights to redistribute it.

Inspired by the continuous-conversion workflow of Any2C1. Any2Odd is an independent project, not an Any2C1 fork and not affiliated with or endorsed by Samsung, Leia or Any2C1.

The desktop visual design references [PortalAI](https://github.com/Zhandragon/PortalAI). Its maintainer-supplied donation QR assets are reused unchanged; provenance and hashes are in `assets/donations/PROVENANCE.json`. Any2Odd and PortalAI are independent applications.
