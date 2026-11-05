@echo off
setlocal
echo Installing or updating Any2Odd. Approve elevation using the same Windows account as your existing installation.
rem Pass the path through the environment, not interpolated PowerShell source.
set "ANY2ODD_INSTALL_SCRIPT=%~dp0install.ps1"
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -Command "$ErrorActionPreference='Stop'; try { $q=[char]34; $installArgs='-NoProfile -ExecutionPolicy Bypass -File '+$q+$env:ANY2ODD_INSTALL_SCRIPT+$q; $p=Start-Process -FilePath ($env:SystemRoot+'\System32\WindowsPowerShell\v1.0\powershell.exe') -ArgumentList $installArgs -Verb RunAs -Wait -PassThru; exit $p.ExitCode } catch { Write-Host $_.Exception.Message; exit 1 }"
if errorlevel 1 (
 echo Installation did not complete. Run install.ps1 in administrator PowerShell for details.
) else (
 echo Installation complete. Open Any2Odd from your desktop or Start menu.
)
pause
