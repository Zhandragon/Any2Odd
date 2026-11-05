# Testing

## 0.7.1 installer validation

- `tests/test-controls-install.ps1` runs the real install/uninstall scripts with destinations redirected into temporary folders and an isolated registry key. It checks read-only preflight, missing-Hub guidance, controls-only install/repair/removal without elevation, no Samsung-folder writes or deletions, preserved settings, promotion to full installation, engine-hash enforcement and refusal to split a full app/worker update. Engine compatibility is simulated only for the promotion step; no Samsung engine is loaded.
- `tests/test-install-launcher.ps1` uses a fake installer to verify visible preflight failures, precise worker reports, controls-only launch without elevation, different-account rejection and delivery of an elevated failure back to the original console. Elevation is simulated; no UAC/install is performed on the host.
- Existing isolated transaction/rollback and full-install tests cover upgrade from the published 0.5.0 and 0.7.0 packages, version detection, repair, dependency ownership and preserved settings. Build, headless UI/Help resources, geometry, documentation export and protected-rendering checks are repeated for the release.
- Actual host preflight passes in controls-only mode and explains the missing Hub in full mode. The app is not installed on this development computer. Real UAC, optical 3D and native macOS/Linux acceptance remain untested for this release; the portable runtime is unchanged.

## 0.7.0 validation and platform limits

**macOS and Linux distributions are experimental and not tested on those operating systems.** Tests below run on Windows without Odyssey Hub, a Samsung 3D display or Any2C1.

- Native Windows build, both UI validation modes, geometry self-test and protected rendering-code comparison against 0.5.0. All thirteen written Help topics are exercised without a renderer; a check confirms in-app Help has no screenshots.
- `tools/DocumentationScreenshots.cs` renders real controls/Help/Donate in an isolated child-control host, using unsaved default preferences and a neutral source-window placeholder. Artwork is inspected for the GitHub pages. `tools/export-guide.py` generates offline HTML and GitHub Help from `Documentation/help.json`; Only the GitHub pages use screenshots; the offline HTML and app Help are text-only.
- Existing installer transaction/rollback and isolated full-install tests exercise upgrade from the published 0.5.0 package and same-version repair. No actual Samsung engine is loaded. Windows projection and click-through code remain unchanged.
- `tests/test_portable.py` verifies exact anaglyph eye/channel mapping, SBS eye swap, half-width geometry, flat passthrough, invalid input, no autostart, real Help/QR resources, same-screen/Wayland guards, stopping on source/display changes, image-output lifecycle and Qt capture API setup without capture. UI renders use Qt's **offscreen platform on Windows**, with the host Segoe font loaded only for visual verification.
- Python syntax and POSIX launcher syntax are checked. Portable bundles are inspected for required files, executable launcher permissions, exact shared assets, no virtual environment/user data, and SHA-256 integrity. Matching Qt wheels are downloaded by the target launcher; no cross-platform binary build is claimed.

Remaining acceptance: real macOS/Linux installation, Screen Recording/portal permissions, native capture, tray and focus/click-through behavior, screen scaling/hot-plug, and any future optical G90XF backend. Windows native multi-DPI and optical acceptance remain target-machine checks as described below.

```powershell
python -m venv build/portable-venv
build/portable-venv/Scripts/python.exe -m pip install -r portable/requirements.txt
build/portable-venv/Scripts/python.exe tests/test_portable.py
```

Use the equivalent `.venv/bin/python` on a native POSIX host when performing target-platform acceptance; an offscreen test alone does not validate native desktop behavior.

The initial release is based on local testing with one G90XF/RTX 4090 setup, not a broad compatibility certification.

## Automated and diagnostic checks

- Clean dependency restore, native .NET Framework compilation and installer `-CheckOnly`.
- `Any2Odd.exe --self-test`: fitting geometry and source isolation across stacked and offset monitor layouts. Run with `Start-Process -Wait -PassThru` to inspect its exit code because this is a GUI executable.
- `--test-capture-exclusion`: visible diagnostic windows check that capture excludes the output and that moved/closed content clears. **Stop conversion before using this test.**
- `--test-offscreen`: visible diagnostic verifies a GPU source outside every physical screen remains live. **Stop conversion before using this test.**

The capture diagnostics were run during predecessor development. They are included for regression testing; they are not automatically launched during installation.

## Confirmed on the predecessor (0.4.3)

- Warframe received controls while 3D stayed visible through Escape, menus and focus changes.
- Wallpaper and icons remained visible and clickable; moved windows and closed menus no longer left permanent trails.
- A single live pointer remained, video/audio sync improved, and borderless capture worked when Windows allowed it.
- Windows taskbar auto-hide resumed with 3D enabled.

The 0.5.0 changes concern branding, documentation and packaging; the capture pipeline is retained. Check each release's notes for additional installation/runtime verification.

## Manual release checks

1. Verify desktop, Start menu, controls window and tray all say Any2Odd and use the high-contrast icon.
2. Toggle from the tray menu and taskbar jump list; confirm Ctrl+Alt+F8/F9/F10.
3. With desktop conversion active, move a window, open/close Start and Alt+Tab, and check for residual images or duplicate pointers.
4. Focus a borderless game and test real mouse/keyboard input and Escape.
5. Verify auto-hide and borderless capture according to Windows permissions/preferences.
6. Apply depth settings; ensure conversion resumes. Quit completely and verify the native worker exits.

Capture frame rate or CPU submission time is not the final stereo-output frame rate or input-to-photon latency. Optical depth and perceived latency still require a person at the monitor.

## 0.6.0 validation

- Rebuilt with the Windows .NET Framework compiler and SHA-256-pinned SharpDX packages.
- Geometry and monitor/source-isolation self-test passed. `tests/verify-rendering-unchanged.py` compares capture, projection, overlay, worker and IPC code to tag `v0.5.0`.
- `tests/UiValidation.cs` checks the release assembly using injected unsaved preferences. It validates tuning ranges/readouts, tray restore/close semantics, internal Help/Donate, embedded resources, light/dark selection and persistence, layout at multiple widths and custom scrolling. It never starts a renderer. The default suite hosts the form as a child of an isolated control; `--headless` tests construction and layout; `--render` renders the light/dark pages in that host. `--preview` explicitly opens the real desktop preview without startup conversion.
- Light and dark controls/donation pages were rendered and inspected. Earlier desktop checks covered window chrome, Help and donation assets. Final design checks used the isolated control host. Native monitor-edge snapping, multi-DPI monitor movement and optical 3D should still be checked on the target setup.
- `tests/test-installer.ps1 -LegacyExecutable <0.5.0 exe>` checks real legacy version detection, install/upgrade/repair decisions, downgrade rejection/override, successful app/worker replacement, failure rollback, and unchanged preferences/Samsung files.
- `tests/test-install-flow.ps1 -LegacyZip <0.5.0 release zip>` runs the installer with the actual published package in isolated temporary folders/registry keys. Samsung engine compatibility, elevation and process discovery are simulated; file installation, version registration, dependency ownership, shortcuts and repeated repair run normally. `-CheckOnly` leaves the old installation untouched.
- Original PortalAI QR PNG hashes match `assets/donations/PROVENANCE.json`. Their original bytes are embedded in the executable; presentation crops only the supplied screenshot framing on crypto codes, retaining modules and quiet zones.

This development computer has no Samsung engine or 3D monitor. The real installer preflight correctly stops at the missing-engine check. No claim is made of a new optical 3D run on 0.6.0; the protected rendering code remains the 0.5.0 code.

Build the UI test runner after `build.ps1`:

```powershell
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:exe /platform:x64 /r:System.Windows.Forms.dll /r:System.Drawing.dll /out:build\UiValidation.exe tests\UiValidation.cs
Start-Process .\build\UiValidation.exe -Wait -PassThru
```

Before release on the G90XF: verify native title-bar dragging/resizing, tray restore, both palettes, all Help topics, donation copying, and then the hardware checks above. Confirm that an in-place update from 0.5.0 retains your depth, pop-out, input mode and tray preferences.
