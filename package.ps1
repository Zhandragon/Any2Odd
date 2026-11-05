$ErrorActionPreference='Stop'
$version=(Get-Content -LiteralPath (Join-Path $PSScriptRoot 'VERSION') -Raw).Trim()
if($version -notmatch '^\d+\.\d+\.\d+$' -or [Reflection.AssemblyName]::GetAssemblyName((Join-Path $PSScriptRoot 'build\Any2Odd.exe')).Version -ne [version]($version+'.0')){throw 'Rebuild before packaging: VERSION and executable must match.'}
$dist=Join-Path $PSScriptRoot 'dist'
New-Item -ItemType Directory -Path $dist -Force | Out-Null
# Fresh staging directory: never pick up logs, diagnostics, or Samsung assets.
$stage=Join-Path $dist ('stage-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path (Join-Path $stage 'build'),(Join-Path $stage 'assets') -Force | Out-Null
foreach($name in @('Any2Odd.exe','SharpDX.dll','SharpDX.DXGI.dll','SharpDX.Direct3D11.dll','SharpDX.D3DCompiler.dll')){
 Copy-Item -LiteralPath (Join-Path $PSScriptRoot ('build\'+$name)) -Destination (Join-Path $stage 'build')
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'assets\Any2Odd.ico') -Destination (Join-Path $stage 'assets')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'assets\Any2Odd.png') -Destination (Join-Path $stage 'assets')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'assets\Any2Odd-wordmark.png') -Destination (Join-Path $stage 'assets')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Documentation') -Destination $stage -Recurse
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'assets\donations') -Destination (Join-Path $stage 'assets') -Recurse
New-Item -ItemType Directory -Path (Join-Path $stage 'portable') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'portable\START-HERE.md') -Destination (Join-Path $stage 'portable')
foreach($name in @('README.md','USER-GUIDE.html','CHANGELOG.md','TESTING.md','LICENSE','THIRD-PARTY-LICENSES.txt','install.ps1','InstallSupport.ps1','Install.cmd','VERSION','Uninstall.ps1')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $stage}
$zip=Join-Path $dist ('Any2Odd-'+$version+'-windows-x64.zip')
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -Force
$hash=(Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText(($zip+'.sha256'),$hash+'  '+[IO.Path]::GetFileName($zip)+[Environment]::NewLine,[Text.UTF8Encoding]::new($false))
Write-Host $zip
Write-Host ('SHA256 '+$hash)
