$ErrorActionPreference='Stop'
$app=Join-Path $env:LOCALAPPDATA 'Programs\Odyssey3DCompanion'
$controlsOnly=$false
$versionFile=Join-Path $app 'version.json'
if(Test-Path -LiteralPath $versionFile){$record=Get-Content -LiteralPath $versionFile -Raw | ConvertFrom-Json;$controlsOnly=$record.ControlsOnly -eq $true}
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
$principal=New-Object Security.Principal.WindowsPrincipal($identity)
if(-not $controlsOnly -and -not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){
 $arguments='-NoProfile -ExecutionPolicy Bypass -File "'+$PSCommandPath+'"'
 $elevated=Start-Process -FilePath 'powershell.exe' -ArgumentList $arguments -Verb RunAs -WindowStyle Hidden -Wait -PassThru
 exit $elevated.ExitCode
}
$exe=Join-Path $app 'Any2Odd.exe'
$worker='C:\Program Files\Odyssey 3D Hub\Conversion\OdysseyCompanionWorker.exe'
if(Test-Path -LiteralPath $exe){Start-Process -FilePath $exe -ArgumentList '--quit' -WindowStyle Hidden -Wait}
$deadline=(Get-Date).AddSeconds(12)
while((Get-Process Any2Odd,Odyssey3DCompanion,OdysseyCompanionWorker -ErrorAction SilentlyContinue) -and (Get-Date) -lt $deadline){Start-Sleep -Milliseconds 250}
if(Get-Process Any2Odd,Odyssey3DCompanion,OdysseyCompanionWorker -ErrorAction SilentlyContinue){throw 'Exit Any2Odd from its tray, then run uninstall again.'}
$shell=New-Object -ComObject WScript.Shell
foreach($folder in @([Environment]::GetFolderPath('Desktop'),[Environment]::GetFolderPath('Programs'))){
 $link=Join-Path $folder 'Any2Odd.lnk'
 if((Test-Path -LiteralPath $link) -and $shell.CreateShortcut($link).TargetPath -eq $exe){Remove-Item -LiteralPath $link}
}
# Exact file list only: leave Samsung files, personal settings and logs intact.
$manifest=Join-Path $app 'dependencies.json'
if(Test-Path -LiteralPath $manifest){foreach($dependency in (Get-Content -LiteralPath $manifest | ConvertFrom-Json)){if($dependency.Name -notin @('SharpDX.dll','SharpDX.DXGI.dll','SharpDX.Direct3D11.dll','SharpDX.D3DCompiler.dll')){continue};$folders=@($app);if(-not $controlsOnly -and $dependency.OwnedEngine){$folders+=Split-Path -Parent $worker};foreach($folder in $folders){$file=Join-Path $folder $dependency.Name;if((Test-Path -LiteralPath $file) -and (Get-FileHash -LiteralPath $file).Hash -eq $dependency.SHA256){Remove-Item -LiteralPath $file}}}}
$ownedFiles=@($exe,$manifest,(Join-Path $app 'Any2Odd.ico'),(Join-Path $app 'LICENSE'),(Join-Path $app 'THIRD-PARTY-LICENSES.txt'),(Join-Path $app 'USER-GUIDE.html'),(Join-Path $app 'Uninstall.ps1'))
if(-not $controlsOnly){$ownedFiles+=$worker}
foreach($file in $ownedFiles){
 if(Test-Path -LiteralPath $file){Remove-Item -LiteralPath $file}
}
$registration='HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\Odyssey3DCompanion'
$versionFile=Join-Path $app 'version.json'
if(Test-Path -LiteralPath $versionFile){Remove-Item -LiteralPath $versionFile}
if(Test-Path -LiteralPath $registration){Remove-Item -LiteralPath $registration}
Write-Host 'Any2Odd removed. Samsung software, your settings and logs were retained. No reboot is needed.'
