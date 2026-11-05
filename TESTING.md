# Testing

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
