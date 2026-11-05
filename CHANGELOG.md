# Changelog

## 0.5.0 — Any2Odd

- Rename the app, desktop/Start menu shortcuts, tray tooltip, taskbar executable, controls window and owned renderer window to Any2Odd.
- Retain the bright, high-contrast stereo-glasses icon, preferences and shared single-instance identifiers.
- Publish the source under GPL-3.0-only, with public setup documentation and an offline guide.
- Add a clean build with SHA-256-pinned NuGet dependency restoration and release packaging.
- Add installer preflight and preserve pre-existing shared engine dependencies during uninstall.

## 0.4.3 — Odyssey 3D Companion (local predecessor)

- Restore taskbar auto-hide by removing only the native converter's stale taskbar entry.

## 0.4.2

- Request Windows borderless capture access and remove the capture outline when permitted.

## 0.4.1

- Exclude the captured hardware cursor and keep the latest GPU frame to reduce pointer duplication and capture delay.

## 0.4.0 and earlier

- Continuous capture of actual wallpaper, desktop icons, windows and menus.
- Off-screen GPU source and output capture exclusion to prevent persistent image trails.
- Click-through overlay, focus recovery, tray controls, taskbar commands and depth settings.
