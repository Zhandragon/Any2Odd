$ErrorActionPreference='Stop'
[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12
Add-Type -AssemblyName System.IO.Compression.FileSystem
$cache=Join-Path $PSScriptRoot 'packages'
$out=Join-Path $PSScriptRoot 'build'
New-Item -ItemType Directory -Path $cache,$out -Force | Out-Null
# Exact NuGet archives, verified before reading any DLL from them.
$packages=@{
 'SharpDX'='117126040200FAAA118F1312AD5A5B1E7D067C90ABF6E726F18B6D1635216FC3'
 'SharpDX.DXGI'='CB4A31BD0137EC8845B234621A690E5AB2B45830BDE0A3D27CAABC5BCED5FC97'
 'SharpDX.Direct3D11'='21DEDCF8B1DDB50EA3FF81F61D45511AEC79D727D15BDA9AE45C05D5058EE219'
 'SharpDX.D3DCompiler'='CA86D6A315FDEA7C0CC7C25299B66FA4F8DC8BAED5B7C3ED918FEC9BBABFC726'
}
foreach($name in ($packages.Keys | Sort-Object)){
 $file=Join-Path $cache ($name+'.4.2.0.nupkg')
 if(-not (Test-Path -LiteralPath $file)){
  $id=$name.ToLowerInvariant()
  Invoke-WebRequest -UseBasicParsing -Uri "https://api.nuget.org/v3-flatcontainer/$id/4.2.0/$id.4.2.0.nupkg" -OutFile $file
 }
 if((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -ne $packages[$name]){throw "Package checksum mismatch: $name"}
 $archive=[IO.Compression.ZipFile]::OpenRead($file)
 try{
  $entry=$archive.GetEntry('lib/net45/'+$name+'.dll')
  if($null -eq $entry){throw "Missing net45 assembly: $name"}
  [IO.Compression.ZipFileExtensions]::ExtractToFile($entry,(Join-Path $out ($name+'.dll')),$true)
 }finally{$archive.Dispose()}
 Write-Host "Verified $name 4.2.0"
}
