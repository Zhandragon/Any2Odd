param([string]$LegacyExecutable)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot '..\InstallSupport.ps1')
function Assert($condition,[string]$message){if(-not $condition){throw $message};Write-Host "PASS: $message"}
$root=Join-Path ([IO.Path]::GetTempPath()) ('Any2Odd-installer-test-'+[Guid]::NewGuid().ToString('N'))
$app=Join-Path $root 'app';$data=Join-Path $root 'data';$engine=Join-Path $root 'engine'
New-Item -ItemType Directory -Path $app,$data,$engine | Out-Null
$current=Join-Path $PSScriptRoot '..\build\Any2Odd.exe'
Assert ((Get-Any2OddInstallAction ([version]'0.6.0') $null) -eq 'Install') 'Fresh installation recognized'
Assert ((Get-Any2OddInstallAction ([version]'0.6.0') ([version]'0.5.0.0')) -eq 'Upgrade') '0.5.0 is upgraded in place'
Assert ((Get-Any2OddInstallAction ([version]'0.6.0') ([version]'0.6.0.0')) -eq 'Repair') 'Same version is a repair'
$blocked=$false;try{Get-Any2OddInstallAction ([version]'0.6.0') ([version]'0.7.0')}catch{$blocked=$true};Assert $blocked 'Downgrade blocked by default'
Assert ((Get-Any2OddInstallAction ([version]'0.6.0') ([version]'0.7.0') -AllowDowngrade) -eq 'Downgrade') 'Explicit downgrade is supported'
Assert ($null -eq (Get-Any2OddInstalledVersion $app '' $data)) 'Empty installation has no version'
if($LegacyExecutable){
 Copy-Item -LiteralPath $LegacyExecutable -Destination (Join-Path $app 'Any2Odd.exe')
 $legacy=Get-Any2OddInstalledVersion $app '' $data
 Assert ($legacy.Version -eq [version]'0.5.0.0') 'Actual 0.5.0 release binary detected without new metadata'
}else{Copy-Item -LiteralPath $current -Destination (Join-Path $app 'Any2Odd.exe')}
Set-Content -LiteralPath (Join-Path $data 'settings.json') -Value '{"Depth":1.37,"Popout":-0.25,"Mode":"Window","MinimizeToTray":false}'
Set-Content -LiteralPath (Join-Path $data 'Samsung-OverlaySettings-before.json') -Value 'original Samsung settings'
Set-Content -LiteralPath (Join-Path $engine 'ConversionWrapper.dll') -Value 'Samsung-owned file'
$settingsHash=(Get-FileHash -LiteralPath (Join-Path $data 'settings.json')).Hash
$backupHash=(Get-FileHash -LiteralPath (Join-Path $data 'Samsung-OverlaySettings-before.json')).Hash
$samsungHash=(Get-FileHash -LiteralPath (Join-Path $engine 'ConversionWrapper.dll')).Hash
$oldHash=(Get-FileHash -LiteralPath (Join-Path $app 'Any2Odd.exe')).Hash
$operations=@([pscustomobject]@{Source=$current;Destination=Join-Path $app 'Any2Odd.exe'},[pscustomobject]@{Source=$current;Destination=Join-Path $engine 'OdysseyCompanionWorker.exe'})
$rollback=[pscustomobject]@{Called=$false};$failed=$false
try{Invoke-Any2OddFileTransaction $operations -AfterCopy {throw 'Simulated registration failure'} -RollbackState {$rollback.Called=$true}}catch{$failed=$true}
Assert ($failed -and $rollback.Called) 'Failure invokes registration rollback'
Assert ((Get-FileHash -LiteralPath (Join-Path $app 'Any2Odd.exe')).Hash -eq $oldHash) 'Failed upgrade restores old executable byte-for-byte'
Assert (-not (Test-Path -LiteralPath (Join-Path $engine 'OdysseyCompanionWorker.exe'))) 'Failed upgrade removes newly introduced worker'
Invoke-Any2OddFileTransaction $operations
Assert ((Get-Any2OddInstalledVersion $app '' $data).Version -eq [version]'0.6.0.0') 'Successful upgrade installs new executable version'
Assert ((Get-FileHash -LiteralPath (Join-Path $engine 'OdysseyCompanionWorker.exe')).Hash -eq (Get-FileHash -LiteralPath $current).Hash) 'App and worker match after upgrade'
Assert ((Get-FileHash -LiteralPath (Join-Path $data 'settings.json')).Hash -eq $settingsHash) 'Preferences preserved byte-for-byte'
Assert ((Get-FileHash -LiteralPath (Join-Path $data 'Samsung-OverlaySettings-before.json')).Hash -eq $backupHash) 'Samsung settings backup preserved'
Assert ((Get-FileHash -LiteralPath (Join-Path $engine 'ConversionWrapper.dll')).Hash -eq $samsungHash) 'Samsung engine untouched'
Write-Host "Isolated test files: $root"
