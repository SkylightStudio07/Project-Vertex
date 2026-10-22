from pathlib import Path
from PIL import Image,ImageDraw,ImageFont,ImageChops
import subprocess,json
P=Path(__file__).resolve().parent;S=P/'Sources';S.mkdir(exist_ok=True)
INK='#16181B';PAPER='#EEF0F2'
bg=Image.open(P/'Source_BG.png').convert('RGBA').crop((0,4,1920,1084))
chara=Image.open(P/'Source_Chara.png').convert('RGBA').resize((1305,870),Image.Resampling.LANCZOS)
base=bg.copy();base.alpha_composite(chara,(70,155))
menus=[(1390,225,430,118),(1420,389,430,118),(1390,553,430,118),(1420,717,430,118)]
icons=[
 '<circle cx="0" cy="0" r="14"/><circle cx="0" cy="0" r="7"/><path d="M-20 0H-10M10 0H20M0 -20V-10M0 10V20"/>',
 '<circle cx="-6" cy="-7" r="6"/><circle cx="9" cy="-4" r="5"/><path d="M-19 16V11Q-19 3-6 3Q7 3 7 11V16ZM8 5Q20 5 20 13V16H11"/>',
 '<path d="M-17 -8L0 -16L17 -8V12L0 20L-17 12ZM-17 -8L0 0L17 -8M0 0V20M-8 -12L9 -4V5"/>',
 '<circle cx="0" cy="1" r="16"/><path d="M0 -10V1L9 6M-6 -20H6"/>'
]
data=[('적','기록','ENEMY RECORDS','수집 12 / 40'),('동','료 기록','COMPANION RECORDS','수집 3 / 15'),('아','이템 도감','ITEM CODEX','수집 8 / 30'),('플','레이 기록','PLAY RECORDS','런 14회')]
def panel(x,y,w,h,fill,opacity=.85,stroke='#77717E'):
 return f'<path d="M{x} {y+22}L{x+22} {y}H{x+w}V{y+h-22}L{x+w-22} {y+h}H{x}Z" fill="{fill}" fill-opacity="{opacity}" stroke="{stroke}" stroke-width="1"/>'
def font(s,b=False,latin=False):return ImageFont.truetype('C:/Windows/Fonts/'+('bahnschrift.ttf' if latin else 'malgunbd.ttf' if b else 'malgun.ttf'),s*2)
records={}
for v,accent in [('A','#0DB8F2'),('B','#B3263A')]:
 shapes=panel(0,0,460,146,INK,.85)
 shapes+=f'<path d="M238 30V108" stroke="{PAPER}" stroke-opacity=".55"/>'
 shapes+=panel(1641,30,213,56,INK,.85)
 shapes+=panel(40,909,500,112,INK,.85)
 shapes+=f'<path d="M61 938V999M54 945H68M90 951H502" stroke="{PAPER}" stroke-opacity=".35"/>'
 for i,(x,y,w,h) in enumerate(menus):
  hover=i==0
  shapes+=panel(x,y,w,h,'#333039' if hover else INK,.85,accent if hover else '#6B646F')
  cx,cy=x+34,y+57
  shapes+=f'<path d="M{cx} {cy-43}L{cx+43} {cy}L{cx} {cy+43}L{cx-43} {cy}Z" fill="{INK}" fill-opacity=".95" stroke="{accent}" stroke-width="{2 if hover else 1}"/>'
  shapes+=f'<path d="M{cx} {cy-36}L{cx+36} {cy}L{cx} {cy+36}L{cx-36} {cy}Z" fill="none" stroke="{accent}" stroke-opacity=".4"/>'
  shapes+=f'<g transform="translate({cx} {cy})" fill="none" stroke="{accent if hover else PAPER}" stroke-width="1.8" stroke-linejoin="round">{icons[i]}</g>'
  shapes+=f'<path d="M{x+91} {y+89}H{x+w-22}" stroke="{PAPER}" stroke-opacity=".13"/>'
 svg=f'<svg xmlns="http://www.w3.org/2000/svg" width="3840" height="2160" viewBox="0 0 1920 1080">{shapes}</svg>'
 (S/f'Archive_{v}_UI.svg').write_text(svg)
 js="const fs=require('fs');const {Resvg}=require('C:/Users/SKYLIGHT/.codex/tmp/currency-f-render/node_modules/@resvg/resvg-js');fs.writeFileSync(process.argv[2],new Resvg(fs.readFileSync(process.argv[1])).render().asPng());"
 subprocess.run(['node','-e',js,str(S/f'Archive_{v}_UI.svg'),str(S/f'Archive_{v}_UI.png')],check=True)
 layer=Image.open(S/f'Archive_{v}_UI.png').convert('RGBA');d=ImageDraw.Draw(layer);texts=[]
 def text(t,box,s,c=PAPER,b=False,latin=False):
  x,y,w,h=box;f=font(s,b,latin);bb=f.getbbox(t)
  assert bb[2]-bb[0]<=w*2 and bb[3]-bb[1]<=h*2,(t,box)
  d.text((x*2-bb[0],y*2+(h*2-(bb[3]-bb[1]))//2-bb[1]),t,font=f,fill=c)
  texts.append({'text':t,'rect':box,'font_size':s,'color':c})
 text('03',[176,43,45,34],28,latin=True)
 text('아카이브',[265,29,190,51],43,b=True)
 text('ARCHIVES',[267,95,180,18],14,'#B8B1BF',latin=True)
 text('← 로비',[1698,42,146,31],25,b=True)
 for i,((x,y,w,h),(first,rest,eng,count)) in enumerate(zip(menus,data)):
  text(f'{i+1:02}',[x+91,y+9,28,13],10,'#B8B1BF',latin=True)
  text(first,[x+91,y+21,62,61],54,accent,True)
  text(rest,[x+151,y+40,250,38],28,PAPER,True)
  text(eng,[x+94,y+79,290,13],10,'#BCB6C2',latin=True)
  text(count,[x+w-143,y+96,128,17],13,'#BCB6C2')
 text('기록관 담당',[90,922,310,24],18,b=True)
 text('어떤 이야기를 펼쳐 볼까요?',[90,965,428,29],22)
 overlay=layer.resize((1920,1080),Image.Resampling.LANCZOS)
 final=Image.alpha_composite(base,overlay);final.convert('RGB').save(P/f'Archive_{v}_Mockup.png')
 # No pixel modifications beyond compositing these two source assets and UI.
 diff=ImageChops.difference(base.convert('RGB'),final.convert('RGB'))
 inverse=overlay.getchannel('A').point(lambda a:255 if a==0 else 0)
 assert ImageChops.multiply(diff,Image.merge('RGB',(inverse,)*3)).getbbox() is None
 records[v]={'accent':accent,'text_boxes':texts}
(P/'MOCKUP_LAYOUT.json').write_text(json.dumps({'canvas':[1920,1080],'bg':'Source_BG.png','bg_crop_ltrb':[0,4,1920,1084],'character':'Source_Chara.png','character_rect':[70,155,1305,870],'menus':menus,'variants':records,'mode':'Original PNG alpha composition + SVG UI; no generative redrawing','note':'First menu shown in hover state. Sample collection counts/dialogue per layout brief. Sprite extraction deferred until variant selection.'},ensure_ascii=False,indent=2),encoding='utf-8')
print('Archive A/B mockups created; source-preservation checks passed.')
