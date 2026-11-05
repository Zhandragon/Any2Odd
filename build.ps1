$ErrorActionPreference='Stop'
$out=Join-Path $PSScriptRoot 'build'
if(-not [Environment]::Is64BitProcess){throw 'Build from 64-bit PowerShell.'}
if([Environment]::OSVersion.Version.Build -lt 26100){throw 'Windows 11 24H2 (build 26100) or newer is required.'}
New-Item -ItemType Directory -Path $out -Force | Out-Null
$version=(Get-Content -LiteralPath (Join-Path $PSScriptRoot 'VERSION') -Raw).Trim()
if($version -notmatch '^\d+\.\d+\.\d+$'){throw 'VERSION must contain major.minor.patch.'}
$versionSource=Join-Path $out 'Version.cs'
[IO.File]::WriteAllText($versionSource,('[assembly: System.Reflection.AssemblyVersion("'+$version+'.0")][assembly: System.Reflection.AssemblyFileVersion("'+$version+'.0")]'))
& (Join-Path $PSScriptRoot 'restore.ps1')
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:exe /r:System.Drawing.dll ("/out:"+(Join-Path $out 'IconMaker.exe')) (Join-Path $PSScriptRoot 'IconMaker.cs')
if($LASTEXITCODE){throw 'Icon tool compilation failed.'}
& (Join-Path $out 'IconMaker.exe') (Join-Path $PSScriptRoot 'assets')
if($LASTEXITCODE){throw 'Brand asset generation failed.'}
$refs=@('SharpDX','SharpDX.DXGI','SharpDX.Direct3D11','SharpDX.D3DCompiler') | ForEach-Object {'/r:'+(Join-Path $out ($_+'.dll'))}
$sources=@('Companion.cs','MainWindow.cs','DesktopTheme.cs','DesktopControls.cs','HelpContent.cs','DonationWindow.cs','OverlayController.cs','RendererTaskbar.cs','Projection.cs','DesktopCapture.cs','GpuDesktopCapture.cs','GpuSurface.cs','DisplayColor.cs','TaskbarMenu.cs','WgcFrameSource.cs') | ForEach-Object {Join-Path $PSScriptRoot $_}
$sources+=$versionSource
$resources=@('paypal.png','ethereum.png','polygon.png','bitcoin.png') | ForEach-Object {'/resource:'+(Join-Path $PSScriptRoot ('assets\donations\'+$_))+',Any2Odd.Donations.'+$_}
$resources+='/resource:'+(Join-Path $PSScriptRoot 'assets\Any2Odd-wordmark.png')+',Any2Odd.Wordmark.png'
$resources+='/resource:'+(Join-Path $PSScriptRoot 'Documentation\help.json')+',Any2Odd.Help.json'
$wpf=@('PresentationFramework','PresentationCore','WindowsBase') | ForEach-Object {'/r:'+(Join-Path "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\WPF" ($_+'.dll'))}
$winrt=@('Windows.Graphics','Windows.Foundation','Windows.Security') | ForEach-Object {'/r:'+(Join-Path "$env:WINDIR\System32\WinMetadata" ($_+'.winmd'))}
$winrt+= '/r:'+(Join-Path "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319" 'System.Runtime.WindowsRuntime.dll')
$winrt+= '/r:'+(Join-Path "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319" 'System.Runtime.dll')
$winrt+= '/r:'+(Join-Path "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319" 'System.Runtime.InteropServices.WindowsRuntime.dll')
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:winexe /platform:x64 /optimize+ /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll /r:System.Management.dll /r:System.Xaml.dll @refs @wpf @winrt @resources ("/win32icon:"+(Join-Path $PSScriptRoot 'assets\Any2Odd.ico')) ("/out:"+(Join-Path $out 'Any2Odd.exe')) @sources
if($LASTEXITCODE){throw 'Compilation failed.'}
