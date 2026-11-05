"""Regenerate the standalone offline guide from the in-app Help catalog."""
from html import escape
from pathlib import Path
import json
import re

root = Path(__file__).resolve().parents[1]
source = (root / 'HelpContent.cs').read_text(encoding='utf-8-sig')
topics = re.findall(r'new HelpTopic\("([^"\\]+)","([^"\\]+)",\s*"((?:\\.|[^"\\])*)"\)', source)
assert len(topics) == 8, 'Help catalog changed; check exporter before publishing'
nav = ''.join(f'<a href="#{escape(id)}">{escape(title)}</a>' for id, title, _ in topics)
articles = []
for id, title, encoded in topics:
    body = json.loads('"' + encoded + '"')
    paragraphs = ''.join('<p>' + escape(p).replace('\n', '<br>') + '</p>' for p in body.split('\n\n'))
    articles.append(f'<section id="{escape(id)}"><h2>{escape(title)}</h2>{paragraphs}</section>')
html = '''<!doctype html>
<html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Any2Odd · User guide</title>
<style>
:root{color-scheme:light}*{box-sizing:border-box}body{margin:0;background:#eee7dc;color:#202224;font:16px/1.65 "Segoe UI",sans-serif}header,main,footer{max-width:1120px;margin:auto;padding:26px}header{border-bottom:3px double #a87d32}h1,h2{font-family:Constantia,Georgia,serif;color:#1f6f82;line-height:1.2}h1{font-size:36px;margin-bottom:10px}nav{display:flex;flex-wrap:wrap;gap:8px;margin-top:22px}a{color:#1f6f82}nav a{padding:7px 12px;border:1px solid #a87d32;background:#f8f3ea;text-decoration:none}section{padding:22px 28px;margin:0 0 20px;background:#f8f3ea;border:1px solid #a87d32;scroll-margin-top:18px}h2{margin-top:0}footer{color:#625e59;font-size:14px}@media(max-width:650px){header,main,footer{padding:18px}section{padding:20px}}
</style>
<header><h1>Any2Odd · User guide</h1><p>Continuous desktop 3D for Samsung Odyssey G90XF. Version VERSION.</p><nav>NAV</nav></header>
<main>ARTICLES</main>
<footer><a href="https://github.com/Zhandragon/Any2Odd">Source &amp; README</a> · <a href="https://github.com/Zhandragon/Any2Odd/releases">Releases</a> · <a href="https://github.com/Zhandragon/Any2Odd/issues">Report an issue</a><p>Independent GPL-3.0-only application. Samsung/Leia software is separately installed and licensed. Not affiliated with Samsung, Leia or Any2C1. PortalAI provides the visual reference and maintainer donation assets; the applications are independent.</p></footer></html>
'''.replace('VERSION', (root / 'VERSION').read_text().strip()).replace('NAV', nav).replace('ARTICLES', '\n'.join(articles))
(root / 'USER-GUIDE.html').write_text(html, encoding='utf-8')
print('Exported all eight Help topics to USER-GUIDE.html')
