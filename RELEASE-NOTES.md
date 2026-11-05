# Any2Odd 0.7.1

Fixes the installer hiding its actual error and adds a Windows controls-only installation for PCs without Odyssey Hub or a Samsung 3D display.

## Choose your download

| Platform | Asset | Start after extraction |
| --- | --- | --- |
| Windows x64 | `Any2Odd-0.7.1-windows-x64.zip` | `Install-GUI.cmd` for controls only; `Install.cmd` for full conversion |
| macOS 13+, Intel / Apple silicon — **experimental, not tested on macOS** | `Any2Odd-0.7.1-macos-experimental.zip` | `Launch-macOS.command` |
| Linux x86-64, glibc 2.34+ — **experimental, not tested on Linux** | `Any2Odd-0.7.1-linux-experimental.tar.gz` | `sh launch-linux.sh` |

Each archive has a matching `.sha256` file. Extract the whole archive before running a launcher; GitHub's automatic Source code downloads require a build.

## Windows setup fix

- Both launchers keep the underlying error visible in the original console. Full setup checks prerequisites before requesting elevation and reports failures from the elevated installer. Elevation under a different Windows account is refused.
- **Without Odyssey Hub:** run **Install-GUI.cmd**. It installs the themed desktop controls, tray, written Help and internal Donate, with desktop/Start menu shortcuts and an Installed apps entry. No administrator access, Samsung screen or Hub is needed; Windows 11 x64 24H2+ and the existing .NET runtime baseline still apply.
- **Controls-only mode cannot convert to 3D.** It installs no worker and does not change Samsung's folders. Automatic conversion still checks prerequisites and leaves 3D off with a missing-engine/worker message. You may uncheck automatic conversion while exploring the controls.
- **On the 3D computer:** use **Install.cmd** with the supported Samsung Hub installed. Upgrade from **0.5.0, 0.6.0 or 0.7.0 without uninstalling**. App/worker versions update together; preferences, logs and the original Samsung settings backup remain intact.
- A controls-only installation can later become a full installation: configure the supported Samsung setup and run **Install.cmd** from the same or a newer release. Controls-only repair/removal needs no elevation and leaves Samsung files alone.

Written Help and offline documentation explain both modes. Screenshots remain only in the [GitHub README and illustrated guide](https://github.com/Zhandragon/Any2Odd/blob/main/Documentation/README.md). Donation QR images and destinations are unchanged.

Windows capture, projection, click-through, overlay handling, renderer lifecycle and global shortcuts remain unchanged. Full 3D retains the exact Samsung engine compatibility check.

## Experimental macOS and Linux

**Experimental and not tested on macOS or Linux.** Their runtime code is unchanged from 0.7.0; these bundles carry the updated version and written documentation. They are Python/Qt source launch bundles, not native self-contained installers. Provide Python 3.10–3.13; first launch downloads pinned PySide6 6.10.1 wheels into an application-local environment.

They offer themed controls/tray, written Help/Donate, SBS image viewing, red/cyan anaglyph conversion, eye swapping and PNG export. Optional live capture requires separate source/output screens; Wayland live output is disabled. Native permissions, capture, focus, tray and click-through remain untested. **They do not implement Samsung AI depth, eye tracking or calibrated G90XF output.** Ordinary 2D stays flat. See [setup and limits](https://github.com/Zhandragon/Any2Odd/blob/main/portable/START-HERE.md).

## Validation

Windows build, headless UI/Help resources, geometry, documentation consistency and protected-rendering checks passed. Isolated installer tests cover controls-only install/repair/removal, promotion to full installation, engine checks, legacy upgrades, rollback, settings preservation and error delivery. UAC/elevated failure is simulated in the launcher test; no actual host installation or new optical 3D test was performed. This development PC has no Odyssey Hub, Samsung display or Any2C1. macOS/Linux have no native acceptance testing.
