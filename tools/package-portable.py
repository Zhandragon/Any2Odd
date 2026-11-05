"""Build source launch bundles; this does not claim native macOS/Linux testing."""
from pathlib import Path
import hashlib
import io
import tarfile
import zipfile

root=Path(__file__).resolve().parents[1]
version=(root/'VERSION').read_text().strip()
dist=root/'dist';dist.mkdir(exist_ok=True)
files={}
for name in ('app.py','stereo.py','requirements.txt','run.sh','START-HERE.md'):
    files['portable/'+name]=root/'portable'/name
for name in ('VERSION','LICENSE','THIRD-PARTY-LICENSES.txt','USER-GUIDE.html'):
    files[name]=root/name
files['START-HERE.md']=root/'portable/START-HERE.md'
for folder in ('Documentation','assets'):
    for path in (root/folder).rglob('*'):
        if path.is_file():files[path.relative_to(root).as_posix()]=path
for platform,launcher,extension in [('macos','Launch-macOS.command','.zip'),('linux','launch-linux.sh','.tar.gz')]:
    entries=dict(files);entries[launcher]=root/'portable'/launcher
    artifact=dist/f'Any2Odd-{version}-{platform}-experimental{extension}'
    prefix=f'Any2Odd-{version}-{platform}-experimental/'
    if extension=='.zip':
        with zipfile.ZipFile(artifact,'w',zipfile.ZIP_DEFLATED) as archive:
            for name,path in sorted(entries.items()):
                info=zipfile.ZipInfo(prefix+name);info.create_system=3;info.compress_type=zipfile.ZIP_DEFLATED
                info.external_attr=((0o100755 if name.endswith(('.sh','.command')) else 0o100644)<<16)
                archive.writestr(info,path.read_bytes())
    else:
        with tarfile.open(artifact,'w:gz') as archive:
            for name,path in sorted(entries.items()):
                content=path.read_bytes();info=tarfile.TarInfo(prefix+name);info.size=len(content)
                info.mode=0o755 if name.endswith(('.sh','.command')) else 0o644;info.mtime=0
                archive.addfile(info,io.BytesIO(content))
    digest=hashlib.sha256(artifact.read_bytes()).hexdigest()
    Path(str(artifact)+'.sha256').write_text(f'{digest}  {artifact.name}\n',encoding='utf-8')
    print(artifact.name,artifact.stat().st_size,'bytes',digest)
