from pathlib import Path
from PIL import Image,ImageDraw,ImageFont,ImageOps,ImageChops
import json,subprocess,math,zipfile,hashlib
import numpy as np
P=Path(__file__).resolve().parent;O=P/'Extracted_Dark';S=O/'SVG';S.mkdir(parents=True,exist_ok=True)
INK='#16181B';EDGE='#77717E';RED='#B3263A';HOVER='#D43A50';TEXT='#EEF0F2';MUTED='#B8B1BF'
meta=json.loads((P/'SOURCE_METADATA.json').read_text(encoding='utf-8'))
oldlayout=json.loads((P.parent/'MaterialMockup/Extracted/layout.json').read_text(encoding='utf-8'))
files=sorted((P/'Current_Sprites').glob('*.png'));assets=[]
def polygon(w,h,c=12):return f'M{c} .5H{w-c-1}L{w-.5} {c}V{h-c-1}L{w-c-1} {h-.5}H.5V{c}Z'
def plate(w,h,fill=INK,edge=EDGE,alpha=.85,c=12):return f'<path d="{polygon(w,h,c)}" fill="{fill}" fill-opacity="{alpha}" stroke="{edge}" stroke-width="1"/>'
def svg(n,w,h,b):
 (S/(n+'.svg')).write_text(f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}">{b}</svg>',encoding='utf-8')

for f in files:
 n=f.stem;w,h=Image.open(f).size;m=meta[n];selected='Selected' in n;hover='Hover' in n;locked='Locked' in n or 'Unknown' in n
 color=HOVER if hover else RED if selected else EDGE
 body=''
 if n=='Base_Material':
  body=plate(460,117,alpha=.88,c=22)
  for x,y,ww,hh in [(86,128,222,785),(332,128,596,785),(947,125,655,802)]:
   body+=f'<g transform="translate({x} {y})">{plate(ww,hh,alpha=.81,c=16)}</g>'
  body+=f'<path d="M209 24V94M320 128V927M935 125V927M105 768H298M105 907H298" stroke="{EDGE}" stroke-opacity=".6"/>'
  for x,y in [(36,35),(320,128),(935,125),(320,927),(935,927)]:body+=f'<path d="M{x-4} {y}H{x+4}M{x} {y-4}V{y+4}" stroke="{EDGE}"/>'
 elif n.startswith('Icon_Lock'):
  body=f'<g fill="none" stroke="{TEXT}" stroke-width="{max(1.5,w*.065)}" stroke-linejoin="round"><path d="M{w*.25} {h*.43}V{h*.24}a{w*.25} {h*.22} 0 0 1 {w*.5} 0V{h*.43}"/><rect x="{w*.12}" y="{h*.43}" width="{w*.76}" height="{h*.53}" rx="{w*.05}"/><path d="M{w*.5} {h*.59}V{h*.79}"/></g>'
 elif n.startswith('Checkbox'):
  fill=RED if 'Selected' in n else INK
  body=f'<rect x="1" y="1" width="{w-2}" height="{h-2}" rx="2" fill="{fill}" fill-opacity=".87" stroke="{HOVER if hover else EDGE}" stroke-width="2"/>'
  if 'Selected' in n:body+=f'<path d="M6 14L12 20L23 8" stroke="{TEXT}" stroke-width="2.5" fill="none"/>'
 elif n.startswith('DefeatPip'):
  body=f'<path d="M8 .7L15.3 8L8 15.3L.7 8Z" fill="{RED if n.endswith("Filled") else "none"}" stroke="{RED if n.endswith("Filled") else EDGE}" stroke-width="1"/>'
 elif n.startswith('Scrollbar'):
  body=f'<rect width="{w}" height="{h}" fill="{EDGE if n.endswith("Handle") else "#29262F"}"/>'
 elif n=='Document_Rule':body=f'<rect width="{w}" height="{h}" fill="{EDGE}"/>'
 elif n=='Document_Clip':
  body=f'<path d="M9 3H31V55L20 46L9 55Z" fill="{RED}"/><path d="M14 5V42M26 5V42" stroke="{HOVER}" stroke-width="1"/><path d="M6 3H34V9H6Z" fill="#6C1B2A"/>'
 elif n.startswith('TypeBadge') or n=='Badge_Affiliation':
  fill=RED if n in ['TypeBadge_Boss','Badge_Affiliation'] else INK
  edge=RED if n=='TypeBadge_Elite' else EDGE if n=='TypeBadge_Normal' else RED
  body=f'<rect x=".5" y=".5" width="{w-1}" height="{h-1}" fill="{fill}" fill-opacity=".87" stroke="{edge}"/>'
 elif n.startswith('ProgressCell'):
  filled=n.endswith('Filled');body=f'<rect x=".5" y=".5" width="{w-1}" height="{h-1}" fill="{RED if filled else INK}" fill-opacity=".87" stroke="{RED if filled else EDGE}"/>'
 else:
  fill='#111015' if locked else '#302B34' if hover else '#241E25' if n=='Document_Paper' else INK
  if n=='CategoryTab_Selected':fill=RED
  body=plate(w,h,fill,color,.85,c=12 if not n.startswith('Back_') else 14)
  if n.startswith('Back_'):body+=f'<path d="M{w*.41} {h*.5}H{w*.31}L{w*.35} {h*.34}M{w*.31} {h*.5}L{w*.35} {h*.66}" stroke="{TEXT}" stroke-width="2" fill="none"/>'
  if n.startswith('EntryTab') and selected:body+=f'<path d="M12 {h-4}H{w-13}" stroke="{RED}" stroke-width="3"/>'
  window=m.get('asset',{}).get('portrait_window_xywh')
  if window:
   x,y,ww,hh=window
   body=f'<defs><mask id="window"><rect width="{w}" height="{h}" fill="white"/><rect x="{x}" y="{y}" width="{ww}" height="{hh}" fill="black"/></mask></defs><g mask="url(#window)">{body}</g><rect x="{x-.5}" y="{y-.5}" width="{ww+1}" height="{hh+1}" fill="none" stroke="{color}"/>'
 svg(n,w,h,body)
 a={**m.get('asset',{}),'name':n,'file':f.name,'output_size':[w,h],'nine_slice_lbrt':m['nine_slice_lbrt'],'svg':'SVG/'+n+'.svg','method':'dark-theme SVG redraw, original dimensions/window/border metadata retained','border_source_meta':m['unity_metadata'][0]['meta'],'source_sha256':hashlib.sha256(f.read_bytes()).hexdigest()}
 assets.append(a)
js="const fs=require('fs'),p=require('path');const {Resvg}=require('C:/Users/SKYLIGHT/.codex/tmp/currency-f-render/node_modules/@resvg/resvg-js');for(const f of fs.readdirSync(process.argv[1]))if(f.endsWith('.svg'))fs.writeFileSync(p.join(process.argv[2],f.replace('.svg','.png')),new Resvg(fs.readFileSync(p.join(process.argv[1],f))).render().asPng());"
subprocess.run(['node','-e',js,str(S),str(O)],check=True)
# Subtle, deterministic one-level grain, never the old paper pattern.
rng=np.random.default_rng(20261022)
for a in assets:
 n=a['name'];im=Image.open(O/a['file']).convert('RGBA')
 if n.startswith(('CategoryTab','Subject','EntryTab','Document_Paper')):
  ar=np.array(im);grain=rng.integers(-1,2,size=ar.shape[:2]+(1,));ar[:,:,:3]=np.clip(ar[:,:,:3].astype(int)+grain,0,255).astype('uint8');im=Image.fromarray(ar)
 if n=='Base_Material':
  bg=Image.open(P.parent/'ArchiveHubMockup/Source_BG.png').convert('RGBA').resize((1672,941),Image.Resampling.LANCZOS)
  bg=Image.alpha_composite(bg,Image.new('RGBA',bg.size,(0,0,0,115)))
  im=Image.alpha_composite(bg,im)
 im.save(O/a['file'])
layout={**oldlayout,'version':2,'theme':'Archive B dark crimson','assets':assets,'reuse':{},'palette':{'panel':INK,'edge':EDGE,'text':TEXT,'secondary':MUTED,'accent':RED,'hover':HOVER},'base_background_darken_alpha':.45,'metadata_policy':'Borders copied from hash-matched runtime .meta files; no runtime asset modified.','source_layout':'../MaterialMockup/Extracted/layout.json','notes':['All 36 assets supplied locally; no dependency on old light sprites.','Windows remain transparent; portraits and all text belong to runtime layers.','Mockups use actual current screenshots for subject portraits and current unlock counts.']}
(O/'layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf-8')

def load(n):return Image.open(O/(n+'.png')).convert('RGBA')
def place(im,n,x,y,w=None,h=None):
 a=load(n)
 if w is not None:a=a.resize((w,h),Image.Resampling.LANCZOS)
 im.alpha_composite(a,(x,y))
def txt(im,t,x,y,size=20,col=TEXT,bold=False,latin=False):
 f=ImageFont.truetype('C:/Windows/Fonts/'+('bahnschrift.ttf' if latin else 'malgunbd.ttf' if bold else 'malgun.ttf'),size)
 bb=f.getbbox(t);ImageDraw.Draw(im).text((x-bb[0],y-bb[1]),t,font=f,fill=col)

def portrait(ref,box,unknown=False):
 a=np.array(ref.crop(box).convert('RGBA'))
 rgb=a[:,:,:3].astype(int);spread=rgb.max(2)-rgb.min(2);light=rgb.mean(2)
 if unknown:
  # Reference silhouettes are flat gray; remove paper and old lock, then place new lock.
  keep=(light>95)&(light<205)&(spread<25)
  # Remove old baked lock pixels before placing the new line-art lock.
  surround=np.concatenate([keep[39:43,43:89].ravel(),keep[89:93,43:89].ravel(),keep[43:89,41:45].ravel(),keep[43:89,87:91].ravel()])
  keep[43:89,45:87]=surround.mean()>.4
  a[:,:,:3]=[90,83,101];a[:,:,3]=np.where(keep,255,0)
 else:
  # Remove edge-connected near-neutral light backing only; preserve enclosed art whites.
  candidate=(light>216)&(spread<18)
  from collections import deque
  seen=np.zeros(candidate.shape,bool);q=deque();hh,ww=candidate.shape
  for x in range(ww):
   for y in [0,hh-1]:
    if candidate[y,x]:seen[y,x]=1;q.append((y,x))
  for y in range(hh):
   for x in [0,ww-1]:
    if candidate[y,x] and not seen[y,x]:seen[y,x]=1;q.append((y,x))
  while q:
   y,x=q.popleft()
   for yy,xx in [(y-1,x),(y+1,x),(y,x-1),(y,x+1)]:
    if 0<=yy<hh and 0<=xx<ww and candidate[yy,xx] and not seen[yy,xx]:seen[yy,xx]=1;q.append((yy,xx))
  a[:,:,3]=np.where(seen,0,255)
 return Image.fromarray(a)

for mode in ['Enemy','Companion']:
 ref=Image.open(P/f'Reference_Material_{mode}.png').convert('RGBA').resize((1672,941),Image.Resampling.LANCZOS)
 im=load('Base_Material');comp=mode=='Companion';known=2 if comp else 4;count=4 if comp else 14;progress=1 if comp else 4
 txt(im,'03',151,43,24,latin=True);txt(im,'동료 기록' if comp else '적 기록',233,25,44,bold=True);txt(im,'COMPANION RECORDS' if comp else 'ENEMY RECORDS',233,82,16,MUTED,latin=True)
 place(im,'Back_Normal',1429,27);txt(im,'아카이브',1510,43,16)
 for j,(label,en) in enumerate([('적','ENEMY'),('동료','COMPANION')]):
  y=148+j*112;place(im,'CategoryTab_Selected' if j==int(comp) else 'CategoryTab_Normal',99,y);txt(im,label,123,y+18,28,bold=True);txt(im,en,123,y+59,14,latin=True)
 txt(im,'동료' if comp else '적',350,149,28,bold=True);txt(im,'/ COMPANION' if comp else '/ ENEMY',410 if comp else 389,158,16,MUTED,latin=True);txt(im,f'확인 {known} · 전체 {count}',795 if comp else 790,152,16)
 txt(im,'소속' if comp else '등급',120,410,23,bold=True)
 for j,s in enumerate(['1막','2막','3막','이벤트'] if comp else ['일반','엘리트','보스']):place(im,'Checkbox_Unchecked',124,457+j*53);txt(im,s,172,464+j*53,20)
 for j,line in enumerate(['동료 호감도 레벨마다','기록 1편 해금'] if comp else ['일반 3회 · 엘리트 2회 ·','보스 1회마다 기록 1편 해금']):txt(im,line,110,791+j*19,14,MUTED)
 txt(im,'실전 격퇴 기록만 반영',110,854,13,MUTED)
 names=['치하라 쇼','이카루스 군'] if comp else ['노상강도','제파러','탈주 용병 사격수','탈주 용병 습격수']
 # The subject grid has a vertical viewport; clip the fourth row exactly like reference.
 grid=Image.new('RGBA',(580,716))
 for i in range(count):
  x=(i%4)*145;y=(i//4)*195
  place(grid,'SubjectCell_Selected' if i==0 else 'SubjectCell_Normal' if i<known else 'SubjectCell_Unknown',x,y)
  rx,ry=350+x+4,193+y+4
  art=portrait(ref,(rx,ry,rx+131,ry+132),i>=known)
  grid.alpha_composite(art,(x+4,y+4))
  if i>=known:place(grid,'Icon_Lock',x+55,y+50)
  txt(grid,names[i] if i<known else '???',x+12,y+143,13,MUTED if i>=known else TEXT)
  dots=(progress if i==0 else 1 if comp and i==1 else 2 if not comp and i==1 else 0)
  for j in range(5):place(grid,'DefeatPip_Filled' if j<dots else 'DefeatPip_Empty',x+22+j*23,y+172)
 im.alpha_composite(grid,(350,193))
 place(im,'Scrollbar_Track',931,193);place(im,'Scrollbar_Handle',931,193)
 place(im,'SubjectHeader',947,125);im.alpha_composite(portrait(ref,(971,148,1161,356)),(971,148))
 txt(im,'치하라 쇼' if comp else '노상강도',1193,155,38,bold=True)
 place(im,'Badge_Affiliation' if comp else 'TypeBadge_Normal',1193,218)
 txt(im,'VERTEX / 협력자' if comp else '일반',1205,225,16)
 txt(im,'호감도 Lv.1 · 기록 1 / 5' if comp else '격퇴 12회 · 기록 4 / 5',1193,279,20)
 for j in range(5):place(im,'ProgressCell_Filled' if j<progress else 'ProgressCell_Empty',1193+j*57,314)
 for j in range(5):
  x=947+j*131;selected=j==progress-1;locked=j>=progress
  place(im,'EntryTab_Selected' if selected else 'EntryTab_Locked' if locked else 'EntryTab_Normal',x,389)
  if locked:place(im,'Icon_LockTab',x+20,405)
  txt(im,f'{j+1:02}',x+58 if locked else x+49,407,22,MUTED if locked else TEXT,True,True)
 place(im,'Document_Paper',947,457);place(im,'Document_Rule',991,553);place(im,'Document_Clip',1536,450)
 txt(im,f'ARCHIVE-{progress:02}',991,480,12,MUTED,latin=True)
 txt(im,'인물 기록 1' if comp else '관측 기록 4',991,508,27,bold=True)
 txt(im,'(작성 예정)' if comp else '아직 기록이 작성되지 않았습니다.',991,581,18)
 txt(im,'관측 자료 / 전투 기록 기반',1430,893,11,MUTED)
 place(im,'Scrollbar_Track',1583,539,4,316);place(im,'Scrollbar_Handle',1583,539,4,88)
 im.save(P/f'Material_Dark_{mode}.png')

# Compact contact sheet; no labels baked into any deliverable sprite.
sheet=Image.new('RGB',(1440,1080),'#37313D');d=ImageDraw.Draw(sheet)
for i,a in enumerate(assets):
 x=(i%6)*240;y=(i//6)*180;im=load(a['name']);im.thumbnail((218,140));sheet.paste(im,(x+10,y+30),im);d.text((x+9,y+8),a['name'],fill=TEXT)
sheet.save(P/'Dark_Sprites_Review.png')
assert len(files)==36 and {f.name for f in files}=={f.name for f in O.glob('*.png')}
for a in assets:
 assert list(load(a['name']).size)==list(Image.open(P/'Current_Sprites'/a['file']).size)
 assert a['nine_slice_lbrt']==meta[a['name']]['nine_slice_lbrt']
 window=meta[a['name']].get('asset',{}).get('portrait_window_xywh')
 if window:
  x,y,w,h=window;assert load(a['name']).crop((x,y,x+w,y+h)).getchannel('A').getextrema()[1]==0,a['name']
assert all('<text' not in f.read_text() for f in S.glob('*.svg'))
report={'sprite_count':36,'names_match_exactly':True,'sizes_match_exactly':True,'nine_slice_matches_runtime_metadata':True,'portrait_windows_transparent':True,'vector_assets_text_free':True,'mockup_size':[1672,941]}
(O/'validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
(O/'README.txt').write_text('36 dark-theme sprites. Keep existing filenames and importer metadata when replacing PNGs. layout.json records unchanged runtime 9-slice values, transparent portrait windows and original layout data. Text colors require runtime TMP updates to #EEF0F2 / #B8B1BF; headings become enemy/companion records. Mockups use current reference unlock state. ArtDirection only: no live Assets files or .meta files were changed. Base_Material embeds the resized library background with 45% black overlay and translucent partition panels. Remaining sprite PNGs are rendered from SVG; selected panels use crimson. Source SVG is supplied for editing; tiny grain is added during raster export.',encoding='utf-8')
with zipfile.ZipFile(P/'Material_Dark_Package.zip','w',zipfile.ZIP_DEFLATED) as z:
 for f in [P/'Material_Dark_Enemy.png',P/'Material_Dark_Companion.png',P/'Dark_Sprites_Review.png',P/'SOURCE_METADATA.json',Path(__file__),*sorted(O.rglob('*'))]:
  if f.is_file():z.write(f,f.relative_to(P))
print(json.dumps(report))
