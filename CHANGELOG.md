# Changelog

## 0.6.0 — Desktop controls and upgrades

- Add PortalAI-inspired light/dark palettes, rounded controls, gold filigree, custom scrollbars and movable/resizable desktop borders. Remember the theme independently of conversion settings.
- Replace the glasses icon with a mathematical `2n + 1` emblem, a `+1` mark at small tray sizes and an engraved Any2Odd wordmark.
- Share a themed right-click menu between the controls window and tray; preserve conversion, overlay, mode and quit actions.
- Add an eight-topic in-app Help menu/browser, F1 shortcut and matching offline guide. Help and Donate are internal pages with a Controls return button.
- Add Donate with GitHub Sponsors and the exact PayPal, Ethereum, Polygon and Bitcoin QR assets/destinations used by PortalAI.
- Make release versions come from `VERSION`; recognize 0.5.0 for in-place upgrades, same-version repair and explicit downgrade protection.
- Add `Install.cmd`, graceful application shutdown, transactional file/registration rollback, and preserve preferences and shared dependency ownership.
- Fix PowerShell 5.1 manifest-array handling so previously unowned shared engine DLLs remain unowned after an upgrade.
- Retain the 0.5.0 capture, projection, click-through, overlay, worker and global shortcut code.

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
