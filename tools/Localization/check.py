"""Fail on missing translations, corrupt Unicode or changed format placeholders."""
from pathlib import Path
import json,re
root=Path(__file__).resolve().parents[2]
source=json.loads((root/'locales/source.json').read_text(encoding='utf-8'))
for code in ['de','ru','fr','es']:
 data=json.loads((root/'locales'/f'{code}.json').read_text(encoding='utf-8'))
 for key in source:
  assert key in data and data[key].strip(),(code,'missing',key)
  assert '\ufffd' not in data[key],(code,'invalid Unicode',key)
  placeholders=lambda s:sorted(re.findall(r'\{\d+(?:[^}]*)\}|%[+\-]?(?:\.\d+)?[dfs]',s))
  assert placeholders(key)==placeholders(data[key]),(code,'format',key,data[key])
 print(code,len(source),'complete templates')
