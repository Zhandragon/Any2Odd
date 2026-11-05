param([Parameter(Mandatory=$true)][string]$LegacyZip)
$ErrorActionPreference='Stop'
$repo=Split-Path -Parent $PSScriptRoot
$root=Join-Path ([IO.Path]::GetTempPath()) ('Any2Odd-flow-test-'+[Guid]::NewGuid().ToString('N'))
$package=Join-Path $root 'package';$legacy=Join-Path $root 'legacy';$local=Join-Path $root 'local';$engine=Join-Path $root 'engine';$desktop=Join-Path $root 'desktop';$programs=Join-Path $root 'programs'
$registration='HKCU:\Software\Any2OddInstallerTests\'+[Guid]::NewGuid().ToString('N')
New-Item -ItemType Directory -Path $package,$local,$engine,$desktop,$programs | Out-Null
Expand-Archive -LiteralPath $LegacyZip -DestinationPath $legacy
New-Item -ItemType Directory -Path (Join-Path $package 'build'),(Join-Path $package 'assets') | Out-Null
foreach($file in @('Any2Odd.exe','SharpDX.dll','SharpDX.DXGI.dll','SharpDX.Direct3D11.dll','SharpDX.D3DCompiler.dll')){Copy-Item -LiteralPath (Join-Path $repo ('build\'+$file)) -Destination (Join-Path $package 'build')}
foreach($file in @('VERSION','InstallSupport.ps1','Uninstall.ps1','USER-GUIDE.html','LICENSE','THIRD-PARTY-LICENSES.txt')){Copy-Item -LiteralPath (Join-Path $repo $file) -Destination $package}
Copy-Item -LiteralPath (Join-Path $repo 'assets\Any2Odd.ico') -Destination (Join-Path $package 'assets')
# Route the actual installer to isolated destinations. Only Samsung compatibility,
# elevation and running-process discovery are simulated; copy/registration/shortcuts run normally.
$script=Get-Content -LiteralPath (Join-Path $repo 'install.ps1') -Raw
$script=$script.Replace('$env:LOCALAPPDATA',("'"+$local+"'"))
$script=$script.Replace('C:\Program Files\Odyssey 3D Hub\Conversion',$engine)
$script=$script.Replace('HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\Odyssey3DCompanion',$registration)
$script=$script.Replace("[Environment]::GetFolderPath('Desktop')",("'"+$desktop+"'")).Replace("[Environment]::GetFolderPath('Programs')",("'"+$programs+"'"))
$script=$script.Replace('if(-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator))','if($false)')
if($script.Contains('C:\Program Files\Odyssey 3D Hub') -or $script.Contains('HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall') -or $script.Contains('$env:LOCALAPPDATA') -or $script.Contains('[Environment]::GetFolderPath')){throw 'Fixture routing incomplete; refusing to run.'}
$testInstall=Join-Path $package 'install.ps1';Set-Content -LiteralPath $testInstall -Value $script -Encoding UTF8
function Get-Process {param($Name,$ErrorAction) return @()}
function Get-FileHash {
 param([string]$LiteralPath,$Algorithm)
 if($LiteralPath -eq (Join-Path $engine 'ConversionWrapper.dll')){return [pscustomobject]@{Hash='0116B33697CA0192FBE26DA96CB9E2592599C0CACFBB1129AE03E445CB3A840B'}}
 Microsoft.PowerShell.Utility\Get-FileHash -LiteralPath $LiteralPath -Algorithm SHA256
}
function Assert($condition,[string]$message){if(-not $condition){throw $message};Write-Host "PASS: $message"}
$app=Join-Path $local 'Programs\Odyssey3DCompanion';$data=Join-Path $local 'Odyssey3DCompanion'
New-Item -ItemType Directory -Path $app,$data | Out-Null
Copy-Item -LiteralPath (Join-Path $legacy 'build\Any2Odd.exe') -Destination $app
Copy-Item -LiteralPath (Join-Path $legacy 'build\Any2Odd.exe') -Destination (Join-Path $engine 'OdysseyCompanionWorker.exe')
$deps=foreach($name in @('SharpDX.dll','SharpDX.DXGI.dll','SharpDX.Direct3D11.dll','SharpDX.D3DCompiler.dll')){Copy-Item -LiteralPath (Join-Path $legacy ('build\'+$name)) -Destination $app;Copy-Item -LiteralPath (Join-Path $legacy ('build\'+$name)) -Destination $engine;[pscustomobject]@{Name=$name;SHA256=(Get-FileHash -LiteralPath (Join-Path $engine $name)).Hash;OwnedEngine=($name -ne 'SharpDX.dll')}}
$deps | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $app 'dependencies.json') -Encoding UTF8
Set-Content -LiteralPath (Join-Path $engine 'ConversionWrapper.dll') -Value 'Simulated Samsung engine; never loaded'
Set-Content -LiteralPath (Join-Path $data 'settings.json') -Value '{"Mode":"Active","Depth":1.4,"MinimizeToTray":false}'
$settingsHash=(Get-FileHash -LiteralPath (Join-Path $data 'settings.json')).Hash
$oldHash=(Get-FileHash -LiteralPath (Join-Path $app 'Any2Odd.exe')).Hash
try{
 & $testInstall -CheckOnly
 Assert ((Get-FileHash -LiteralPath (Join-Path $app 'Any2Odd.exe')).Hash -eq $oldHash -and -not (Test-Path -LiteralPath $registration)) 'CheckOnly leaves old installation and registration untouched'
 & $testInstall
 Assert ((Get-ItemProperty -LiteralPath $registration).DisplayVersion -eq (Get-Content -LiteralPath (Join-Path $repo 'VERSION') -Raw).Trim()) 'Full upgrade registers the packaged version'
 Assert ((Get-Content -LiteralPath (Join-Path $data 'install-result.json') -Raw | ConvertFrom-Json).Action -eq 'Upgrade') 'Install record reports an upgrade from the published version'
 Assert ((Get-FileHash -LiteralPath (Join-Path $data 'settings.json')).Hash -eq $settingsHash) 'Full installer preserves existing user preferences'
 $installedDependencies=Get-Content -LiteralPath (Join-Path $app 'dependencies.json') -Raw | ConvertFrom-Json
 Assert (($installedDependencies | Where-Object Name -eq 'SharpDX.dll').OwnedEngine -eq $false) 'Pre-existing shared dependency ownership stays unowned'
 $shell=New-Object -ComObject WScript.Shell
 Assert ($shell.CreateShortcut((Join-Path $desktop 'Any2Odd.lnk')).TargetPath -eq (Join-Path $app 'Any2Odd.exe')) 'Desktop shortcut targets updated executable'
 Assert ($shell.CreateShortcut((Join-Path $programs 'Any2Odd.lnk')).TargetPath -eq (Join-Path $app 'Any2Odd.exe')) 'Start menu shortcut targets updated executable'
 & $testInstall
 Assert ((Get-Content -LiteralPath (Join-Path $data 'install-result.json') -Raw | ConvertFrom-Json).Action -eq 'Repair') 'Repeated installer repairs the same release'
 Write-Host "Isolated integration files: $root"
}finally{if(Test-Path -LiteralPath $registration){Remove-Item -LiteralPath $registration}}
