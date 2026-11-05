"""Export the shared Help catalog to GitHub Markdown and a text-only offline HTML guide."""
import base64
from html import escape
import json
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
TOPICS=json.loads((ROOT/'Documentation/help.json').read_text(encoding='utf-8'))
VERSION=(ROOT/'VERSION').read_text().strip()

def data(path):
    mime='image/svg+xml' if path.suffix=='.svg' else 'image/png'
    return 'data:'+mime+';base64,'+base64.b64encode(path.read_bytes()).decode('ascii')

def paragraphs(body):
    result=[]
    for paragraph in body.split('\n\n'):
        lines=paragraph.split('\n')
        if len(lines)>1 and lines[0].isupper():
            result.append('<h3>'+escape(lines.pop(0))+'</h3>')
        result.append('<p>'+escape('\n'.join(lines)).replace('\n','<br>')+'</p>')
    return ''.join(result)

markdown=['<p align="center"><img src="../assets/Any2Odd-wordmark.png" width="520" alt="Any2Odd mathematical wordmark" /></p>',
          '# Illustrated Help',f'Any2Odd {VERSION} · Windows edition reference, with a separate experimental macOS/Linux topic.',
          '[Repository](../README.md) · [Platform support](PLATFORMS.md) · [Donations](DONATIONS.md)',
          '<p align="center"><a href="https://github.com/sponsors/Zhandragon"><img src="assets/sponsor-button.svg" width="252" height="48" alt="Sponsor on GitHub" /></a></p>',
          'The same written topics are included inside the app and in `USER-GUIDE.html`. Screenshots are provided here and in the repository README only. Screenshots show real Windows controls in an isolated preview; conversion was not started. They do not demonstrate optical 3D or macOS/Linux operation.',
          ' · '.join(f'[{t["Title"]}](#{t["Id"]})' for t in TOPICS)]
sections=[]
for topic in TOPICS:
    id,title,body=topic['Id'],topic['Title'],topic['Body']
    content=f'<section id="{id}"><h2>{escape(title)}</h2>'
    markdown.extend([f'<a id="{id}"></a>',f'## {title}'])
    if topic.get('Screenshot'):
        stem=topic['Screenshot'];caption=topic['Caption']
        markdown.append(f'[![{caption}](assets/{stem}-light.png)](assets/{stem}-light.png)')
    content+=paragraphs(body)
    for paragraph in body.split('\n\n'):
        lines=paragraph.split('\n')
        if len(lines)>1 and lines[0].isupper():markdown.append('**'+lines.pop(0)+'**')
        markdown.append('\n'.join(lines) if lines and lines[0].startswith('1. ') else '<br>\n'.join(lines))
    for link in topic.get('Links',[]):
        content+=f'<p><a href="{escape(link["Url"],quote=True)}">{escape(link["Label"])}</a></p>'
        markdown.append(f'[{link["Label"]}]({link["Url"]})')
    sections.append(content+'</section>')

css='''
:root{color-scheme:light dark;--bg:#eee7dc;--panel:#f8f3ea;--text:#202224;--muted:#625e59;--accent:#1f6f82;--gold:#a87d32}
@media(prefers-color-scheme:dark){:root{--bg:#111315;--panel:#1c1e20;--text:#f2ece2;--muted:#b7afa4;--accent:#7cc4d3;--gold:#b0863a}}
*{box-sizing:border-box}html{scroll-behavior:smooth;scrollbar-color:var(--gold) var(--bg)}body{margin:0;background:var(--bg);color:var(--text);font:16px/1.7 "Segoe UI",Arial,sans-serif}header,main,footer{max-width:1080px;margin:auto;padding:26px}header{text-align:center}header>img{width:min(520px,100%)}h1,h2,h3{font-family:Constantia,Georgia,serif;line-height:1.3}h2{color:var(--accent)}h3{font-size:16px;letter-spacing:.03em;margin-top:26px}a{color:var(--accent)}nav{display:flex;flex-wrap:wrap;justify-content:center;gap:8px;margin:24px 0}nav a{padding:8px 12px;text-decoration:none;background:var(--panel);border:1px solid var(--gold);border-radius:10px}section{position:relative;background:var(--panel);border:3px double var(--gold);border-radius:20px;padding:28px 34px;margin-bottom:28px;scroll-margin-top:20px}section:before{content:'◇ ─── ◈ ─── ◇';position:absolute;top:-17px;left:50%;transform:translateX(-50%);background:var(--panel);padding:0 15px;color:var(--gold)}figure{margin:22px auto;max-width:850px}figure img{display:block;max-width:100%;height:auto;max-height:620px;margin:auto;border-radius:14px}figcaption,footer,.note{font-size:14px;color:var(--muted)}figcaption{text-align:center;margin-top:10px}.sponsor{display:inline-block;margin:14px}footer{text-align:center}@media(max-width:650px){header,main,footer{padding:16px}section{padding:22px 18px}h1{font-size:28px}}
'''
nav=''.join(f'<a href="#{t["Id"]}">{escape(t["Title"])}</a>' for t in TOPICS)
html=f'''<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Any2Odd {VERSION} · User guide</title><style>{css}</style></head><body>
<header><img src="{data(ROOT/'assets/Any2Odd-wordmark.png')}" alt="Any2Odd: 2n maps to 2n plus 1"><h1>User guide</h1><p>Version {VERSION} · Continuous desktop 3D for Samsung Odyssey G90XF</p><p>Windows edition reference. macOS/Linux bundles are experimental, untested and have limited stereo-format conversion.</p><a class="sponsor" href="https://github.com/sponsors/Zhandragon"><img src="{data(ROOT/'Documentation/assets/sponsor-button.svg')}" width="252" height="48" alt="Sponsor on GitHub"></a><p class="note">This offline guide contains written documentation; screenshots are on the GitHub documentation page. External links need internet.</p><nav>{nav}</nav></header><main>{''.join(sections)}</main>
<footer><a href="https://github.com/Zhandragon/Any2Odd">Repository</a> · <a href="https://github.com/Zhandragon/Any2Odd/releases">Downloads</a> · <a href="https://github.com/Zhandragon/Any2Odd/issues">Report a problem</a><p>Independent GPL-3.0-only application by Zhandragon. CubeVi C1 / Any2C1 inspired the workflow; PortalAI inspired the presentation. Samsung/Leia software is separately installed and licensed.</p></footer></body></html>'''
(ROOT/'USER-GUIDE.html').write_text(html,encoding='utf-8')
(ROOT/'Documentation/README.md').write_text('\n\n'.join(markdown)+'\n',encoding='utf-8')
print(f'Exported {len(TOPICS)} topics; screenshots are GitHub-only and offline Help is text-only.')
