"""Validate shared Help exports, local documentation links and screenshot placement."""
from pathlib import Path
import hashlib
from html.parser import HTMLParser
import json
import re
import subprocess
import sys
from urllib.parse import unquote

root=Path(__file__).resolve().parents[1]
topics=json.loads((root/'Documentation/help.json').read_text(encoding='utf-8'))
assert len(topics)==len({t['Id'] for t in topics})==13
before=[(root/p).read_bytes() for p in ('Documentation/README.md','USER-GUIDE.html')]
subprocess.run([sys.executable,str(root/'tools/export-guide.py')],check=True)
assert before==[(root/p).read_bytes() for p in ('Documentation/README.md','USER-GUIDE.html')], 'Regenerate guide exports'

class Guide(HTMLParser):
    def __init__(self):super().__init__();self.ids=set();self.images=[]
    def handle_starttag(self,tag,attrs):
        attrs=dict(attrs)
        if 'id' in attrs:self.ids.add(attrs['id'])
        if tag=='img':self.images.append(attrs['src'])
guide=Guide();guide.feed((root/'USER-GUIDE.html').read_text(encoding='utf-8'))
assert {t['Id'] for t in topics}<=guide.ids
assert len(guide.images)==2, 'Offline guide should contain branding/sponsor only, not screenshots'
assert all(i.startswith('data:image/') for i in guide.images)

for path in [root/'README.md',*list((root/'Documentation').rglob('*.md')),root/'portable/START-HERE.md']:
    text=path.read_text(encoding='utf-8')
    targets=re.findall(r'\]\(([^)]+)\)|(?:src|href)="([^"]+)"',text)
    for target in (a or b for a,b in targets):
        if target.startswith(('http:','https:','#','data:','mailto:')):continue
        file=target.split('#',1)[0]
        assert (path.parent/unquote(file)).exists(),f'{path.relative_to(root)}: broken link {target}'
for topic in topics:
    if topic.get('Screenshot'):
        for theme in ('light','dark'):assert (root/'Documentation/assets'/(topic['Screenshot']+'-'+theme+'.png')).is_file()
provenance=json.loads((root/'assets/donations/PROVENANCE.json').read_text(encoding='utf-8-sig'))
for asset in provenance['assets']:assert hashlib.sha256((root/'assets/donations'/asset['file']).read_bytes()).hexdigest()==asset['sha256']
assert 'github: Zhandragon' in (root/'.github/FUNDING.yml').read_text()
assert hashlib.sha256((root/'Documentation/assets/sponsor-button.svg').read_bytes()).hexdigest()=='f3240d1ba307e121e65c0afd7223e6d3b9056858870097e0bdce3a8e09b41bc4'
print('PASS: 13 synchronized topics, local links/images, text-only offline guide, exact donation/sponsor assets')
