[CmdletBinding()]
param([switch]$CheckOnly)
$ErrorActionPreference='Stop'
# Legacy locations and IPC names preserve preferences and prevent duplicate renderers.
$app=Join-Path $env:LOCALAPPDATA 'Programs\Odyssey3DCompanion'
$engine='C:\Program Files\Odyssey 3D Hub\Conversion'
$binary=Join-Path $PSScriptRoot 'build\Any2Odd.exe'
$dependencyNames=@('SharpDX.dll','SharpDX.DXGI.dll','SharpDX.Direct3D11.dll','SharpDX.D3DCompiler.dll')
if(-not [Environment]::Is64BitProcess -or [Environment]::OSVersion.Version.Build -lt 26100){throw 'Use 64-bit PowerShell on Windows 11 24H2 or newer.'}
foreach($relative in @('build\Any2Odd.exe','assets\Any2Odd.ico','USER-GUIDE.html','Uninstall.ps1','LICENSE','THIRD-PARTY-LICENSES.txt')+@($dependencyNames | ForEach-Object {'build\'+$_})){
 if(-not (Test-Path -LiteralPath (Join-Path $PSScriptRoot $relative) -PathType Leaf)){throw "Missing package file: $relative. Build or extract the complete release first."}
}
$engineDll=Join-Path $engine 'ConversionWrapper.dll'
if(-not (Test-Path -LiteralPath $engineDll)){throw 'Install Samsung Odyssey 3D Hub first.'}
if((Get-FileHash -LiteralPath $engineDll).Hash -ne '0116B33697CA0192FBE26DA96CB9E2592599C0CACFBB1129AE03E445CB3A840B'){throw 'Samsung engine changed. This release needs a compatibility update.'}
$prior=@()
$manifest=Join-Path $app 'dependencies.json'
if(Test-Path -LiteralPath $manifest){$prior=@(Get-Content -LiteralPath $manifest -Raw | ConvertFrom-Json)}
$dependencies=foreach($name in $dependencyNames){
 $file=Join-Path $PSScriptRoot ('build\'+$name)
 $hash=(Get-FileHash -LiteralPath $file).Hash
 $existing=Join-Path $engine $name
 $exists=Test-Path -LiteralPath $existing
 if($exists -and (Get-FileHash -LiteralPath $existing).Hash -ne $hash){throw "Another application owns a different $name in the engine folder. Install aborted."}
 $previous=@($prior | Where-Object {$_.Name -eq $name -and $_.SHA256 -eq $hash})
 # Old manifests recorded only dependencies installed by this application.
 $owned=(-not $exists) -or ($previous.Count -eq 1 -and ($null -eq $previous[0].OwnedEngine -or $previous[0].OwnedEngine))
 [pscustomobject]@{Name=$name;SHA256=$hash;OwnedEngine=[bool]$owned}
}
if($CheckOnly){Write-Host 'Any2Odd package and Samsung engine compatibility checks passed. No changes made.';return}
$principal=New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if(-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Run install.ps1 in an administrator PowerShell window using the same Windows account.'}
if(Get-Process Any2Odd,Odyssey3DCompanion,OdysseyCompanionWorker -ErrorAction SilentlyContinue){throw 'Quit Any2Odd / Odyssey 3D Companion from its tray before installing.'}
New-Item -ItemType Directory -Force -Path $app | Out-Null
Copy-Item -LiteralPath $binary -Destination (Join-Path $app 'Any2Odd.exe') -Force
Copy-Item -LiteralPath $binary -Destination (Join-Path $engine 'OdysseyCompanionWorker.exe') -Force
foreach($name in @('USER-GUIDE.html','Uninstall.ps1','LICENSE','THIRD-PARTY-LICENSES.txt')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $app -Force}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'assets\Any2Odd.ico') -Destination (Join-Path $app 'Any2Odd.ico') -Force
foreach($dependency in $dependencies){
 $file=Join-Path $PSScriptRoot ('build\'+$dependency.Name)
 Copy-Item -LiteralPath $file -Destination $app -Force
 if($dependency.OwnedEngine){Copy-Item -LiteralPath $file -Destination $engine -Force}
}
$dependencies | ConvertTo-Json | Set-Content -LiteralPath $manifest -Encoding UTF8
$shell=New-Object -ComObject WScript.Shell
foreach($folder in @([Environment]::GetFolderPath('Desktop'),[Environment]::GetFolderPath('Programs'))){
 $link=$shell.CreateShortcut((Join-Path $folder 'Any2Odd.lnk'))
 $link.TargetPath=Join-Path $app 'Any2Odd.exe';$link.WorkingDirectory=$app;$link.IconLocation=(Join-Path $app 'Any2Odd.ico')+',0'
 $link.Description='Any2Odd - continuous Odyssey 3D; right-click the tray icon to toggle.';$link.Save()
 $old=Join-Path $folder 'Odyssey 3D Companion.lnk'
 if((Test-Path -LiteralPath $old) -and $shell.CreateShortcut($old).TargetPath -eq (Join-Path $app 'Odyssey3DCompanion.exe')){Remove-Item -LiteralPath $old}
}
$registration='HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\Odyssey3DCompanion'
New-Item -Path $registration -Force | Out-Null
$uninstall='powershell.exe -NoProfile -ExecutionPolicy Bypass -File "'+(Join-Path $app 'Uninstall.ps1')+'"'
$properties=@{DisplayName='Any2Odd';DisplayVersion='0.5.0';Publisher='Zhandragon';InstallLocation=$app;DisplayIcon=(Join-Path $app 'Any2Odd.ico')+',0';UninstallString=$uninstall;URLInfoAbout='https://github.com/Zhandragon/Any2Odd'}
foreach($entry in $properties.GetEnumerator()){New-ItemProperty -LiteralPath $registration -Name $entry.Key -Value $entry.Value -PropertyType String -Force | Out-Null}
foreach($name in @('NoModify','NoRepair')){New-ItemProperty -LiteralPath $registration -Name $name -Value 1 -PropertyType DWord -Force | Out-Null}
$data=Join-Path $env:LOCALAPPDATA 'Odyssey3DCompanion'
New-Item -ItemType Directory -Path $data -Force | Out-Null
[pscustomobject]@{Installed=(Get-Date).ToString('o');Version='0.5.0';App=$app;Worker=(Join-Path $engine 'OdysseyCompanionWorker.exe');Hash=(Get-FileHash -LiteralPath $binary).Hash} | ConvertTo-Json | Set-Content (Join-Path $data 'install-result.json') -Encoding UTF8
Write-Host 'Any2Odd installed. Open its desktop icon as your normal user. No reboot is needed.'