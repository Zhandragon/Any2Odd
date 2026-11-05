# Changelog

## 0.7.2 — Clean resizing and engraved desktop artwork

- Fix repeated border fragments during resizing by repainting the full decorated desktop surface, including cards, viewport, banner, sliders and window frame.
- Fit all primary controls into a 980 × 780 logical-pixel default window at normal scaling. Put Source & input beside Depth & pop-out, group startup/window behavior underneath, and retain stacked panels and scrolling in smaller windows.
- Add a heavier black outer frame, dark-gold title-bar/card borders, ivory surfaces and serif headings. Keep light and dark themes and internal Help/Donate.
- Replace the broad metallic word art with fine serif lettering, engraved outlines and scrollwork. Redraw the mathematical medallion as a balanced complete circle with outlined `2n` / `+1`, concentric rings and turquoise accents; supersample every ICO size and remove edge spill.
- Refresh the GitHub screenshots and written layout guidance. Preserve donation QR data, version-aware full/controls-only installation, preferences and the Windows conversion/overlay/click-through implementation.
- macOS/Linux bundles receive shared branding and documentation; their runtime is unchanged and remains **experimental and untested on those operating systems**.

## 0.7.1 — Visible setup errors and controls-only installation

- Keep prerequisite and elevated-installation errors in the original launcher console. Check prerequisites before UAC and reject elevation under a different Windows account.
- Add `Install-GUI.cmd` / `install.ps1 -ControlsOnly` for the desktop controls, tray, written Help and Donate without Odyssey Hub or administrator access. No conversion worker is installed; 3D stays unavailable.
- Record controls-only installation explicitly, support repair/upgrade/removal and later full installation, preserve preferences, and leave Samsung directories untouched during controls-only installation/removal.
- Keep full app/worker updates together and retain the exact engine compatibility check, downgrade protection and transactional rollback.
- Document both installation paths. Windows rendering/overlay code and portable runtime code are unchanged. macOS/Linux remain **experimental and not tested on those operating systems**.

## 0.7.0 — Documentation and experimental portable editions

- Expand written Help to thirteen shared topics covering defaults, apply/save behavior, display checks, dependencies, troubleshooting and upgrades. Keep the original text-only app layout and offline guide; add real light/dark screenshots to the GitHub README and documentation.
- Credit Any2C1 as the CubeVi C1 companion app that inspired the workflow, and distinguish it from the Samsung/Leia/Windows runtime dependencies.
- Present the repository with centered branding, navigation, screenshot galleries and PortalAI's original GitHub Sponsors button/FUNDING configuration.
- Add experimental, source-based macOS and Linux launch bundles with a shared Qt frontend, themes, tray fallback, Help and original donation assets. **Not tested on macOS or Linux.**
- Portable features: image-file SBS viewing, red/cyan anaglyph conversion, eye swap, flat preview and PNG export; constrained live screen capture requires a separate output display and is disabled on Wayland.
- Portable builds do not implement Samsung AI depth, eye tracking or calibrated G90XF output. Native dependencies are fetched on first launch; Windows remains the full Samsung converter.
- Preserve Windows capture, projection, click-through, renderer lifecycle, global shortcuts and version-aware upgrade behavior.

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
