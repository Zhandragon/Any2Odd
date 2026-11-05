# Any2Odd 0.7.0

Illustrated GitHub documentation, expanded written Help and explicit dependency/origin documentation, PortalAI-style repository presentation and sponsorship, and experimental macOS/Linux launch bundles.

## Choose your download

| Platform | Asset | Start after extraction |
| --- | --- | --- |
| Windows x64 — full Samsung Odyssey converter | `Any2Odd-0.7.0-windows-x64.zip` | `Install.cmd` |
| macOS 13+, Intel / Apple silicon — **experimental, not tested on macOS** | `Any2Odd-0.7.0-macos-experimental.zip` | `Launch-macOS.command` |
| Linux x86-64, glibc 2.34+ — **experimental, not tested on Linux** | `Any2Odd-0.7.0-linux-experimental.tar.gz` | `sh launch-linux.sh` |

Each archive has a matching `.sha256` file. Extract the whole archive and read its setup instructions.

## Help and presentation

- Thirteen written Help topics explaining setting defaults and apply/save behavior, startup/display checks, troubleshooting, dependencies and updates. The original in-app text layout is retained.
- Text-only in-app Help and offline HTML, plus light/dark screenshots in the README and [illustrated GitHub guide](https://github.com/Zhandragon/Any2Odd/blob/main/Documentation/README.md).
- Explicit credit to Any2C1, the CubeVi C1 companion app that inspired the workflow. Any2C1/CubeStage/PortalAI are not dependencies. Samsung/Leia and Windows dependencies are explained separately.
- Centered branding, interface galleries, PortalAI's original teal/gold GitHub Sponsors button and matching repository sponsorship configuration. Donation QR data and destinations are unchanged.

## Experimental macOS and Linux scope

**These versions are experimental and have not been tested on macOS or Linux.** They are small Python/Qt source launch bundles, not native self-contained installers. Provide Python 3.10–3.13; first launch downloads pinned PySide6 6.10.1 wheels into an application-local virtual environment.

The shared frontend provides themes, tray fallback, internal Help/Donate, SBS image viewing, red/cyan anaglyph conversion, eye swapping and PNG export. Optional live screen capture requires separate source/output screens; Wayland live output is disabled pending source/output exclusion verification. Native focus, capture permissions, tray and click-through behavior remain untested.

**Samsung AI depth conversion, eye tracking and calibrated G90XF glasses-free output remain Windows-only.** Ordinary 2D stays flat in the portable build. It does not need Odyssey Hub, a Samsung monitor or Any2C1 for its image-viewing features. See [portable setup](https://github.com/Zhandragon/Any2Odd/blob/main/portable/START-HERE.md) and the [platform capability matrix](https://github.com/Zhandragon/Any2Odd/blob/main/Documentation/PLATFORMS.md).

## Windows update

Upgrade from **0.5.0 or 0.6.0 without uninstalling**. Extract the Windows ZIP to a new folder and run `Install.cmd` using the same Windows account as the existing installation. The installer updates the app/worker in place and preserves preferences, logs and the original Samsung settings backup. The supported Samsung engine is still required.

Windows capture, projection, click-through, overlay handling, renderer lifecycle and global shortcuts remain unchanged.

## Validation

Windows build, UI/Help resources, geometry, protected-rendering comparison, isolated upgrade/repair/rollback tests and package checks passed. Eight portable conversion/UI tests passed using Qt's offscreen platform on Windows; that is not native macOS/Linux acceptance. No Odyssey Hub, Samsung screen or Any2C1 was present on the development computer, and no new optical 3D test was performed.
