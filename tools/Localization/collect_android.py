"""Collect literal PL/EN pairs; no translation service or network is used."""
from pathlib import Path
import json,re
root=Path(__file__).resolve().parents[2]
def arguments(text, start):
    parts=[];at=start;depth=0;quoted=False;escape=False;begin=at
    for at in range(start,len(text)):
        c=text[at]
        if quoted:
            if escape:escape=False
            elif c=='\\':escape=True
            elif c=='"':quoted=False
        elif c=='"':quoted=True
        elif c=='(':depth+=1
        elif c==')':
            if depth==0:return parts+[text[begin:at]]
            depth-=1
        elif c==',' and depth==0:parts.append(text[begin:at]);begin=at+1
    return []
def literal(text):
    if not re.fullmatch(r'\s*"(?:\\.|[^"\\])*"(?:\s*\+\s*"(?:\\.|[^"\\])*")*\s*',text):return None
    return ''.join(json.loads(s) for s in re.findall(r'"(?:\\.|[^"\\])*"',text))
pairs={}
for file in (root/'android/app/src/main/java').rglob('*.java'):
    if file.name=='Translations.java':continue
    text=file.read_text(encoding='utf-8')
    for call in re.finditer(r'\bt\s*\(',text):
        args=arguments(text,call.end())
        if len(args)!=2:continue
        pl,en=map(literal,args)
        if pl is not None and en is not None:pairs[en]=pl
(root/'locales/source-android.json').write_text(json.dumps(pairs,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
base=json.loads((root/'locales/source-csharp.json').read_text(encoding='utf-8'))
base.update(pairs)
# Canonical physical-key descriptions are shared by both platforms.
text=(root/'TuningPlan.cs').read_text(encoding='utf-8')
arrays=[]
for name in ['Polish','English']:
    contents=re.search(r'\b'+name+r'\s*=\s*\[(.*?)\];',text,re.S).group(1)
    arrays.append([json.loads(x) for x in re.findall(r'"(?:\\.|[^"\\])*"',contents)])
base.update(zip(arrays[1],arrays[0]))
(root/'locales/source.json').write_text(json.dumps(base,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
existing=json.loads((root/'locales/de.json').read_text(encoding='utf-8'))
missing=[s for s in base if s not in existing]
for i,key in enumerate(missing):print(i,key.replace('\n','\\n'))
print('Android pairs:',len(pairs),'Total:',len(base),'Missing:',len(missing))
