$ErrorActionPreference='Stop'
$out=Join-Path $PSScriptRoot 'build'
if(-not [Environment]::Is64BitProcess){throw 'Build from 64-bit PowerShell.'}
if([Environment]::OSVersion.Version.Build -lt 26100){throw 'Windows 11 24H2 (build 26100) or newer is required.'}
New-Item -ItemType Directory -Path $out -Force | Out-Null
& (Join-Path $PSScriptRoot 'restore.ps1')
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:exe /r:System.Drawing.dll ("/out:"+(Join-Path $out 'IconMaker.exe')) (Join-Path $PSScriptRoot 'IconMaker.cs')
if($LASTEXITCODE){throw 'Icon tool compilation failed.'}
& (Join-Path $out 'IconMaker.exe') (Join-Path $PSScriptRoot 'assets')
$refs=@('SharpDX','SharpDX.DXGI','SharpDX.Direct3D11','SharpDX.D3DCompiler') | ForEach-Object {'/r:'+(Join-Path $out ($_+'.dll'))}
$sources=@('Companion.cs','OverlayController.cs','RendererTaskbar.cs','Projection.cs','DesktopCapture.cs','GpuDesktopCapture.cs','GpuSurface.cs','DisplayColor.cs','TaskbarMenu.cs','WgcFrameSource.cs') | ForEach-Object {Join-Path $PSScriptRoot $_}
$wpf=@('PresentationFramework','PresentationCore','WindowsBase') | ForEach-Object {'/r:'+(Join-Path "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\WPF" ($_+'.dll'))}
$winrt=@('Windows.Graphics','Windows.Foundation','Windows.Security') | ForEach-Object {'/r:'+(Join-Path "$env:WINDIR\System32\WinMetadata" ($_+'.winmd'))}
$winrt+= '/r:'+(Join-Path "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319" 'System.Runtime.WindowsRuntime.dll')
$winrt+= '/r:'+(Join-Path "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319" 'System.Runtime.dll')
$winrt+= '/r:'+(Join-Path "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319" 'System.Runtime.InteropServices.WindowsRuntime.dll')
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:winexe /platform:x64 /optimize+ /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Web.Extensions.dll /r:System.Management.dll /r:System.Xaml.dll @refs @wpf @winrt ("/win32icon:"+(Join-Path $PSScriptRoot 'assets\Any2Odd.ico')) ("/out:"+(Join-Path $out 'Any2Odd.exe')) @sources
if($LASTEXITCODE){throw 'Compilation failed.'}
