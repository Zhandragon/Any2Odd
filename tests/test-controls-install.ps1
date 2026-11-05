$ErrorActionPreference='Stop'
$repo=Split-Path -Parent $PSScriptRoot
$root=Join-Path ([IO.Path]::GetTempPath()) ('Any2Odd-controls-test-'+[Guid]::NewGuid().ToString('N'))
$package=Join-Path $root 'package';$local=Join-Path $root 'local';$engine=Join-Path $root 'engine';$desktop=Join-Path $root 'desktop';$programs=Join-Path $root 'programs'
$registration='HKCU:\Software\Any2OddInstallerTests\'+[Guid]::NewGuid().ToString('N')
New-Item -ItemType Directory -Path $package,$desktop,$programs,(Join-Path $package 'build'),(Join-Path $package 'assets') | Out-Null
foreach($file in @('Any2Odd.exe','SharpDX.dll','SharpDX.DXGI.dll','SharpDX.Direct3D11.dll','SharpDX.D3DCompiler.dll')){Copy-Item -LiteralPath (Join-Path $repo ('build\'+$file)) -Destination (Join-Path $package 'build')}
foreach($file in @('VERSION','InstallSupport.ps1','USER-GUIDE.html','LICENSE','THIRD-PARTY-LICENSES.txt')){Copy-Item -LiteralPath (Join-Path $repo $file) -Destination $package}
Copy-Item -LiteralPath (Join-Path $repo 'assets\Any2Odd.ico') -Destination (Join-Path $package 'assets')
foreach($file in @('install.ps1','Uninstall.ps1')){
 $script=Get-Content -LiteralPath (Join-Path $repo $file) -Raw
 $script=$script.Replace('$env:LOCALAPPDATA',("'"+$local+"'")).Replace('C:\Program Files\Odyssey 3D Hub\Conversion',$engine).Replace('HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\Odyssey3DCompanion',$registration)
 $script=$script.Replace("[Environment]::GetFolderPath('Desktop')",("'"+$desktop+"'")).Replace("[Environment]::GetFolderPath('Programs')",("'"+$programs+"'"))
 if($file -eq 'install.ps1'){$script=$script.Replace('if(-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator))','if($false)')}
 if($script.Contains('C:\Program Files\Odyssey 3D Hub') -or $script.Contains('HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall') -or $script.Contains('$env:LOCALAPPDATA') -or $script.Contains('[Environment]::GetFolderPath')){throw 'Fixture routing incomplete; refusing to run.'}
 Set-Content -LiteralPath (Join-Path $package $file) -Value $script -Encoding UTF8
}
function Assert($condition,[string]$message){if(-not $condition){throw $message};Write-Host "PASS: $message"}
function Get-Process {param($Name,$ErrorAction)return @()}
function Start-Process {param($FilePath,$ArgumentList,$WindowStyle,[switch]$Wait,$Verb,[switch]$PassThru)if($Verb){throw 'Controls-only uninstall must not elevate.'};return [pscustomobject]@{ExitCode=0}}
$installer=Join-Path $package 'install.ps1';$app=Join-Path $local 'Programs\Odyssey3DCompanion';$data=Join-Path $local 'Odyssey3DCompanion'
New-Item -ItemType Directory -Path $data -Force | Out-Null
Set-Content -LiteralPath (Join-Path $data 'settings.json') -Value '{"AutoConvert":false,"Depth":1.2}'
$settingsHash=(Get-FileHash -LiteralPath (Join-Path $data 'settings.json')).Hash
try{
 & $installer -CheckOnly -ControlsOnly
 Assert (-not (Test-Path -LiteralPath $app) -and -not (Test-Path -LiteralPath $registration) -and -not (Test-Path -LiteralPath $engine)) 'Controls-only preflight makes no installation changes'
 $failed=$false;try{& $installer -CheckOnly}catch{$failed=$_.Exception.Message.Contains('Install-GUI.cmd')};Assert $failed 'Full preflight explains missing Hub and points to controls-only installer'
 & $installer -ControlsOnly
 $record=Get-Content -LiteralPath (Join-Path $app 'version.json') -Raw | ConvertFrom-Json
 Assert ($record.ControlsOnly -eq $true -and $null -eq $record.Worker) 'Controls-only mode records no conversion worker'
 Assert (-not (Test-Path -LiteralPath $engine)) 'Controls-only install does not create or modify the Samsung directory'
 Assert ((Get-ItemProperty -LiteralPath $registration).DisplayName -eq 'Any2Odd (controls only)') 'Installed apps identifies controls-only mode'
 $deps=Get-Content -LiteralPath (Join-Path $app 'dependencies.json') -Raw | ConvertFrom-Json
 Assert (@($deps | Where-Object OwnedEngine).Count -eq 0) 'Controls-only dependencies have no Samsung-folder ownership'
 & $installer -ControlsOnly
 Assert ((Get-Content -LiteralPath (Join-Path $app 'version.json') -Raw | ConvertFrom-Json).Action -eq 'Repair') 'Controls-only reinstall repairs in place'
 New-Item -ItemType Directory -Path $engine | Out-Null
 Set-Content -LiteralPath (Join-Path $engine 'OdysseyCompanionWorker.exe') -Value 'Unrelated sentinel created after controls-only install'
 Copy-Item -LiteralPath (Join-Path $app 'SharpDX.dll') -Destination $engine
 $engineHash=(Get-FileHash -LiteralPath (Join-Path $engine 'SharpDX.dll')).Hash
 foreach($dependency in $deps){$dependency.OwnedEngine=$true};$deps | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $app 'dependencies.json') -Encoding UTF8
 & (Join-Path $app 'Uninstall.ps1')
 Assert (-not (Test-Path -LiteralPath (Join-Path $app 'Any2Odd.exe')) -and -not (Test-Path -LiteralPath $registration)) 'Controls-only uninstall removes the app and registration without elevation'
 Assert ((Test-Path -LiteralPath (Join-Path $engine 'OdysseyCompanionWorker.exe')) -and (Get-FileHash -LiteralPath (Join-Path $engine 'SharpDX.dll')).Hash -eq $engineHash) 'Controls-only uninstall leaves all Samsung-directory files untouched'
 Assert ((Get-FileHash -LiteralPath (Join-Path $data 'settings.json')).Hash -eq $settingsHash) 'Uninstall preserves user preferences'
 Remove-Item -LiteralPath (Join-Path $engine 'OdysseyCompanionWorker.exe')
 & $installer -ControlsOnly
 Set-Content -LiteralPath (Join-Path $engine 'ConversionWrapper.dll') -Value 'Simulated Samsung engine; never loaded'
 $failed=$false;try{& $installer -CheckOnly}catch{$failed=$_.Exception.Message.Contains('engine changed')};Assert $failed 'Full setup still rejects an incompatible engine'
 function Get-FileHash {param([string]$LiteralPath,$Algorithm)if($LiteralPath -eq (Join-Path $engine 'ConversionWrapper.dll')){return [pscustomobject]@{Hash='0116B33697CA0192FBE26DA96CB9E2592599C0CACFBB1129AE03E445CB3A840B'}};Microsoft.PowerShell.Utility\Get-FileHash -LiteralPath $LiteralPath -Algorithm SHA256}
 & $installer
 $record=Get-Content -LiteralPath (Join-Path $app 'version.json') -Raw | ConvertFrom-Json
 Assert ($record.ControlsOnly -eq $false -and (Test-Path -LiteralPath $record.Worker)) 'Full setup promotes a controls-only installation and adds its renderer'
 Assert ((Get-ItemProperty -LiteralPath $registration).DisplayName -eq 'Any2Odd') 'Full setup clears the controls-only registration label'
 Assert ((Get-FileHash -LiteralPath (Join-Path $data 'settings.json')).Hash -eq $settingsHash) 'Promotion preserves user preferences'
 $failed=$false;try{& $installer -ControlsOnly}catch{$failed=$_.Exception.Message.Contains('full installation')};Assert $failed 'Controls-only setup refuses to split an existing full installation'
 Write-Host "Isolated test files: $root"
}finally{if(Test-Path -LiteralPath $registration){Remove-Item -LiteralPath $registration}}
