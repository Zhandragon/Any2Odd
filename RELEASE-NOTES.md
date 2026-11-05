# Any2Odd 0.7.2

Fixes decorative border trails while resizing, fits the Windows controls into a more organized default window, and refines the artwork and light/dark framing.

## Desktop changes

- Full repainting of decorated controls during resize removes the repeated border fragments.
- A **980 × 780 logical-pixel default window** shows all primary controls at normal display scaling. Source & input and Depth & pop-out sit side by side; startup/window behavior is grouped underneath. Narrow windows stack the panels, and scrolling remains available for smaller work areas or larger scaling.
- A heavier black outer frame, darker gold title-bar/card borders, ivory surfaces and serif headings give the light theme stronger definition. Dark mode uses the same framing and artwork.
- A new engraved serif wordmark and circular mathematical medallion use outlined `2n` / `+1`, fine scrollwork, concentric rings and turquoise accents. All seven icon sizes have centered circular transparency and no decoration outside the circle.
- Refreshed GitHub screenshots and written layout documentation. Help remains text-only inside the app; Donate remains an internal panel with unchanged QR images and destinations.

**Capture, projection, overlay placement, click-through, renderer lifecycle, hotkeys and conversion settings are unchanged.**

## Downloads and upgrades

| Platform | Asset | Run after fully extracting |
| --- | --- | --- |
| Windows x64 | `Any2Odd-0.7.2-windows-x64.zip` | `Install-GUI.cmd` without Hub; `Install.cmd` for the full Samsung converter |
| macOS 13+, Intel / Apple silicon — **experimental, not tested on macOS** | `Any2Odd-0.7.2-macos-experimental.zip` | `Launch-macOS.command` |
| Linux x86-64, glibc 2.34+ — **experimental, not tested on Linux** | `Any2Odd-0.7.2-linux-experimental.tar.gz` | `sh launch-linux.sh` |

Each archive has a matching `.sha256` file. Upgrade existing Windows installations **without uninstalling**; the installer preserves preferences and reports upgrade/repair. Use the same mode as the existing installation. Controls-only setup installs no renderer and cannot perform 3D; full setup still requires the supported Samsung engine and display. The 0.7.1 visible-error reporting fix is retained.

macOS/Linux receive the shared artwork and documentation. Their Python/Qt runtime is unchanged, remains experimental and untested on those operating systems, and does not implement Samsung AI or calibrated Odyssey output. See [portable setup and limits](https://github.com/Zhandragon/Any2Odd/blob/main/portable/START-HERE.md).

## Validation

Windows build, UI behavior and layout tests, full-card resize invalidation checks, geometry, protected-rendering comparison, all icon sizes, documentation/QR integrity, isolated installer upgrade/repair/rollback and portable offscreen tests passed. A live Windows preview was resized and maximized without the border trails. No new optical 3D test or native macOS/Linux test was performed; other-monitor DPI behavior remains a target-machine check.
