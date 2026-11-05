# Any2Odd 0.6.0

A desktop interface refresh with PortalAI-inspired light and dark themes, rounded controls, gold filigree and custom scrollbars. The applications remain independent.

- A mathematical `2n + 1` emblem, readable `+1` tray icon and engraved wordmark.
- Custom window borders and controls, plus a themed right-click menu on the window and tray.
- Internal Help and Donate panels. Help covers capture modes, tuning, overlay/click-through, shortcuts, recovery, troubleshooting and upgrades. Donate uses the same unchanged PayPal/Ethereum/Polygon/Bitcoin QR assets and GitHub Sponsors destination as PortalAI.
- A remembered light/dark preference stored separately from conversion settings.
- Version-aware installation: upgrades 0.5.0 in place, supports same-version repair, refuses to overwrite a newer installation unless explicitly requested, and attempts rollback if deployment fails. Preserves settings, logs, Samsung backups and shared dependency ownership.
- Capture, projection, overlay/click-through, worker lifecycle and global shortcut code retained from 0.5.0.

**Download `Any2Odd-0.6.0-windows-x64.zip`, extract everything, then run `Install.cmd` on your 3D-display computer. Use the same Windows account as your existing installation. Do not uninstall 0.5.0 first.** The installer requests a clean Any2Odd shutdown and updates its existing location. Stop Samsung's separate player before installation. No reboot is required. See README.md for PowerShell instructions and read-only preflight.

Validation: clean build, hosted UI checks, rendered light/dark page review, unchanged-rendering source checks, geometry self-test, real 0.5.0 executable detection, isolated upgrade/repair/rollback tests, shortcut registration and original QR asset hashes. Installation integration tests simulate the Samsung engine/elevation/process checks on this computer; optical output and target-hardware behavior still need validation at the G90XF. See TESTING.md for scope.

Compatibility remains Windows 11 x64 24H2+, G90XF as primary at 3840 × 2160, the supported Odyssey 3D Hub 1.5.1 engine and its LeiaSR installation. Tested GPU remains RTX 4090. Samsung/Leia binaries, models and calibration data are not bundled. The source downloads require a build; use the prebuilt ZIP for installation.
