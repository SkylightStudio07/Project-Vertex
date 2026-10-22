from pathlib import Path
from PIL import Image,ImageDraw,ImageFont,ImageOps,ImageChops
import json,subprocess,shutil,zipfile,hashlib,sys
sys.path.insert(0,'C:/Users/SKYLIGHT/.codex/tmp/shop-seg')
import cv2,numpy as np
P=Path(__file__).resolve().parent;O=P/'Extracted_Draft';S=O/'SVG';S.mkdir(parents=True,exist_ok=True)
INK='#16181B';PAPER='#EEF0F2';CYAN='#0DB8F2';GRAY='#94999D'
assets={};elements=[];texts=[]
def path(w,h,c=16):return f'M.75 .75H{w-c}L{w-.75} {c}V{h-c}L{w-c} {h-.75}H.75Z'
def svg(n,w,h,body,border=None):
 (S/(n+'.svg')).write_text(f'<svg xmlns="http://www.w3.org/2000/svg" width="{w*2}" height="{h*2}" viewBox="0 0 {w} {h}">{body}</svg>',encoding='utf-8')
 assets[n]={'file':n+'.png','svg':'SVG/'+n+'.svg','pixel_size':[w*2,h*2],'logical_size':[w,h],'asset_scale':2}
 if border:assets[n]['nine_slice']={'source_pixels_left_bottom_right_top':border,'logical_pixels_left_bottom_right_top':[a/2 for a in border]}
def panel(n,w,h,fill=PAPER,stroke='#A3A7AA',selected=False):
 b=f'<path d="{path(w,h)}" fill="{fill}" stroke="{stroke}" stroke-width="{2 if selected else 1}"/>'
 if n.startswith('Panel_Item'):
  b+=f'<path d="M7 17L17 7H{w-18}L{w-7} 18V{h-18}L{w-18} {h-7}H7Z" fill="none" stroke="#C4C7C9" stroke-width=".65"/>'
 svg(n,w,h,b,[36,36,36,36])
panel('Panel_Item',304,354)
panel('Panel_Item_Selected',304,354,stroke=CYAN,selected=True)
for n,fill in [('Band_SoldOut',INK),('Band_Unlocked','#D9DCDE')]:svg(n,304,44,f'<rect width="304" height="44" fill="{fill}"/>',[2,2,2,2])
panel('Panel_BuyBar',930,78)
for n,fill in [('Button_Buy',INK),('Button_Buy_Disabled','#D1D4D6')]:
 svg(n,215,78,f'<path d="M.75 18L18 .75H214.25V60L197 77.25H.75Z" fill="{fill}" stroke="#A3A7AA"/>',[40,40,40,40])
for n in ['Button_Qty_Minus','Button_Qty_Plus']:
 svg(n,34,34,f'<rect x=".5" y=".5" width="33" height="33" fill="{PAPER}" stroke="#A3A7AA"/>',[2,2,2,2])
for n,fill in [('Tab_Normal','#E7E9EB'),('Tab_Selected',CYAN)]:
 svg(n,231,42,f'<path d="M1 41L34 1H230V41Z" fill="{fill}" stroke="#ABB0B3" stroke-width="1"/>',[70,4,4,4])
svg('Panel_Dialogue',502,129,f'<path d="M.75 36L38 .75H501.25V83L455 128.25H.75Z" fill="{PAPER}" stroke="#A3A7AA"/><path d="M25 40V119M17 49H33M49 49H462" stroke="{INK}" stroke-width="1"/><path d="M466 46L469 49L466 52L463 49Z" fill="{INK}"/>',[104,48,90,108])
panel('Button_Lobby',212,56)
panel('Panel_CardFace',130,202)
svg('Mark_CardLock',20,25,f'<path d="M5 10V6a5 5 0 0110 0v4" fill="none" stroke="{PAPER}" stroke-width="2"/><rect x="2" y="10" width="16" height="14" rx="2" fill="{PAPER}"/><circle cx="10" cy="16" r="2" fill="{INK}"/>')
js="const fs=require('fs'),p=require('path');const {Resvg}=require('C:/Users/SKYLIGHT/.codex/tmp/currency-f-render/node_modules/@resvg/resvg-js');for(const f of fs.readdirSync(process.argv[1]))if(f.endsWith('.svg'))fs.writeFileSync(p.join(process.argv[2],f.replace('.svg','.png')),new Resvg(fs.readFileSync(p.join(process.argv[1],f))).render().asPng());"
subprocess.run(['node','-e',js,str(S),str(O)],check=True)
for f in (P/'Currency_Icons').glob('*.png'):
 shutil.copy2(f,O/f.name)
 assets[f.stem]={'file':f.name,'pixel_size':list(Image.open(f).size),'source':'../Currency_Icons/'+f.name,'unaltered_sha256':hashlib.sha256(f.read_bytes()).hexdigest()}
extraction=json.loads((O/'extraction_sources.json').read_text())
for n,meta in extraction.items():assets[n]={'file':n+'.png',**meta}

src=Image.open(P/'Reference_Shop_Draft.png').convert('RGBA').resize((1920,1080),Image.Resampling.LANCZOS)
arm=Image.open(P/'Reference_Armory.png').convert('RGBA').resize((1920,1080),Image.Resampling.LANCZOS)
# Reuse the existing armory card illustration, not a newly invented card front.
art=arm.crop((438,286,624,465));art.save(O/'Card_Art_Reference.png')
unknown=arm.crop((657,644,831,816))
ua=np.array(unknown.convert('RGB'));um=np.zeros(ua.shape[:2],np.uint8)
um[64:172,48:124]=255
ua=cv2.inpaint(ua,um,5,cv2.INPAINT_TELEA)
unknown=Image.fromarray(ua).convert('RGBA');unknown.save(O/'Card_Locked_Reference.png')
assets['Card_Art_Reference']={'file':'Card_Art_Reference.png','source':'../Reference_Armory.png','illustration_only':True}
assets['Card_Locked_Reference']={'file':'Card_Locked_Reference.png','source':'../Reference_Armory.png','note':'Reference silhouette artwork; mockup question marks and lock are separate.'}

def paste(im,n,box):
 x,y,w,h=box;a=Image.open(O/(n+'.png')).convert('RGBA').resize((w,h),Image.Resampling.LANCZOS);im.alpha_composite(a,(x,y))
def fit(im,n,box):
 a=Image.open(O/(n+'.png')).convert('RGBA');a.thumbnail((box[2],box[3]),Image.Resampling.LANCZOS);im.alpha_composite(a,(box[0]+(box[2]-a.width)//2,box[1]+(box[3]-a.height)//2))
def font(s,bold=False,latin=False):return ImageFont.truetype('C:/Windows/Fonts/'+('bahnschrift.ttf' if latin else 'malgunbd.ttf' if bold else 'malgun.ttf'),s)
def txt(im,s,box,size=20,color=INK,bold=False,latin=False,align='left'):
 f=font(size,bold,latin);bb=f.getbbox(s);w=bb[2]-bb[0];h=bb[3]-bb[1]
 assert w<=box[2] and h<=box[3],(s,w,h,box)
 x=box[0]+((box[2]-w)//2 if align=='center' else 0);y=box[1]+(box[3]-h)//2
 ImageDraw.Draw(im).text((x-bb[0],y-bb[1]),s,font=f,fill=color)
 return {'text':s,'rect':box,'font_size_px':size,'color':color,'weight':700 if bold else 400,'font':('Bahnschrift' if latin else 'Malgun Gothic'),'alignment':align+'-middle','baked_in_asset':False}

def card(kind='parts',state='normal',price=None):
 im=Image.new('RGBA',(304,354));paste(im,'Panel_Item_Selected' if state=='selected' else 'Panel_Item',[0,0,304,354])
 data={'parts':('파츠 묶음','파츠 ×10','Item_PartsBundle',120),'supply':('보급 패키지','파츠 ×20','Item_SupplyPackage',220),'coffee':('따뜻한 커피','선물 · 1개','Item_Coffee',60),'note':('기록 노트','선물 · 1개','Item_Notebook',90),'card':('무색 카드','카드 · 해금',None,240),'unknown':('???','미발견 카드',None,None)}[kind]
 name,desc,asset,default=data;cost=default if price is None else price
 txt(im,'01',[17,15,35,18],15,GRAY,latin=True)
 if asset:fit(im,asset,[23,38,258,176])
 else:
  paste(im,'Panel_CardFace',[87,31,130,202])
  if kind=='unknown':
   # Pure silhouette from the same armory artwork, preserve its frame proportions.
   crop=Image.open(O/'Card_Locked_Reference.png').convert('RGBA');crop=ImageOps.fit(crop,(118,126));im.alpha_composite(crop,(93,37))
   txt(im,'?',[98,81,108,39],36,PAPER,True,align='center')
   paste(im,'Mark_CardLock',[142,132,20,25])
   txt(im,'???',[98,165,108,29],25,INK,True,align='center')
   txt(im,'미발견',[98,199,108,16],11,GRAY,align='center')
  else:
   a=Image.open(O/'Card_Art_Reference.png');a=ImageOps.fit(a,(118,116));im.alpha_composite(a,(93,37))
   txt(im,'무색 카드',[96,161,112,24],18,INK,True,align='center')
   txt(im,'VERTEX',[96,199,112,13],9,GRAY,latin=True,align='center')
 txt(im,name,[23,237,258,31],26,INK,True)
 txt(im,desc,[23,274,258,23],18,INK)
 ImageDraw.Draw(im).line((18,306,286,306),fill='#C6C9CB',width=1)
 if cost is not None and state!='unlocked':
  paste(im,'Icon_Talin',[25,319,25,25]);txt(im,f'{cost:,}',[64,313,204,35],30,GRAY if state=='insufficient' else INK,True)
 if kind in ['parts','supply'] and state in ['stock','selected','normal']:
  txt(im,'재고 2/3',[184,12,105,25],15,INK,True)
 if state=='soldout':
  dull=Image.new('RGBA',im.size,'#D6D9DB');dull.putalpha(155);im=Image.alpha_composite(im,dull)
  paste(im,'Band_SoldOut',[0,140,304,44]);txt(im,'품절',[0,140,304,44],25,PAPER,True,align='center')
 if state=='unlocked':
  paste(im,'Band_Unlocked',[0,140,304,44]);txt(im,'해금 완료',[0,140,304,44],23,INK,True,align='center')
 return im

def buybar(disabled=False):
 im=Image.new('RGBA',(930,78));paste(im,'Panel_BuyBar',[0,0,930,78]);arr=[]
 balance='보유 60 → 구매 후 —' if disabled else '보유 1,240 → 구매 후 1,120'
 arr.append(txt(im,balance,[20,21,350,37],20,INK,True))
 for n,x,char in [('Button_Qty_Minus',379,'-'),('Button_Qty_Plus',455,'+')]:
  paste(im,n,[x,22,34,34]);arr.append(txt(im,char,[x,22,34,34],24,GRAY if disabled else INK,align='center'))
 arr.append(txt(im,'1',[415,22,38,34],23,INK,True,align='center'))
 paste(im,'Icon_Talin',[513,26,26,26]);arr.append(txt(im,'120',[550,16,144,46],35,CYAN if not disabled else GRAY,True))
 paste(im,'Button_Buy_Disabled' if disabled else 'Button_Buy',[715,0,215,78]);arr.append(txt(im,'탈린 부족' if disabled else '구매하기 →',[725,10,195,58],25,GRAY if disabled else PAPER,True,align='center'))
 return im,arr

main=src.copy();draw=ImageDraw.Draw(main)
# Retain base screenshot everywhere except explicit UI correction rectangles.
allowed=[]
def cover(box,color=PAPER):
 x,y,w,h=box;draw.rectangle((x,y,x+w-1,y+h-1),fill=color);allowed.append(box)
# Armory header position and typography, with original shopkeeper layered unchanged over overlap.
# Reuse clean armory paper for the existing diagonal header language; no square plate.
paper=arm.crop((650,0,1110,125)).resize((460,164),Image.Resampling.LANCZOS).convert('RGBA')
paper_alpha=Image.new('L',(460,164))
ImageDraw.Draw(paper_alpha).polygon([(0,164),(0,125),(125,0),(460,0),(460,125),(421,145),(80,145)],fill=255)
paper.putalpha(paper_alpha);main.alpha_composite(paper,(0,0));draw=ImageDraw.Draw(main)
allowed.append([0,0,460,164])
texts.append(txt(main,'05',[176,42,46,35],28,INK,latin=True))
draw.line((238,30,238,107),fill=INK,width=1)
texts.append(txt(main,'상점',[265,21,176,66],59,INK,True))
texts.append(txt(main,'SUPPLY EXCHANGE',[266,94,150,19],13,GRAY,latin=True))
allowed.append([266,94,150,19])
# Exact armory back button image and placement.
backbox=(1641,30,1854,86);main.paste(arm.crop(backbox),backbox[:2]);allowed.append([1641,30,213,56])
elements.append({'id':'LobbyBack','asset':'Button_Lobby','rect':[1641,30,213,56],'text':'← 로비','reference':'Exact armory button crop in mockup; editable SVG in assets'})
cover([1193,28,407,74])
for n,x,val in [('Icon_Talin',1201,'1,240'),('Icon_Parts',1374,'36'),('Icon_Artifact',1504,'2')]:
 paste(main,n,[x,47,38,38]);elements.append({'id':n+'Top','asset':n,'rect':[x,47,38,38]});texts.append(txt(main,val,[x+49,45,92 if n=='Icon_Talin' else 48,42],25,INK,True))
# User approved existing right-side tab anchors; use armory shape and height.
cover([930,140,925,60])
for i,t in enumerate(['전체','강화 재료','선물','무색 카드']):
 x=930+i*231;paste(main,'Tab_Selected' if i==0 else 'Tab_Normal',[x,140,231,42]);elements.append({'id':'Tab'+str(i),'asset':'Tab_Selected' if i==0 else 'Tab_Normal','rect':[x,140,231,42]});texts.append(txt(main,t,[x+30,144,192,34],23,INK,True,align='center'))
grid=[('parts','selected'),('supply','stock'),('coffee','normal'),('note','normal'),('card','normal'),('unknown','normal')]
for i,(kind,state) in enumerate(grid):
 x=[916,1236,1556][i%3];y=[222,590][i//3]
 cover([x-1,y-1,307,356]);im=card(kind,state);main.alpha_composite(im,(x,y))
 # Replace mockup-only card indices without altering text-free panel assets.
 ImageDraw.Draw(main).rectangle((x+12,y+10,x+54,y+34),fill=PAPER)
 txt(main,f'{i+1:02}',[x+17,y+15,35,18],15,GRAY,latin=True)
 elements.append({'id':'Item'+str(i+1),'asset':'Panel_Item_Selected' if state=='selected' else 'Panel_Item','rect':[x,y,304,354],'product':kind,'state':state})
bar,bartexts=buybar();cover([944,960,930,78]);main.alpha_composite(bar,(944,960));elements.append({'id':'BuyBar','asset':'Panel_BuyBar','rect':[944,960,930,78]})
for t in bartexts:t['rect'][0]+=944;t['rect'][1]+=960;texts.append(t)
# Preserve every original shopkeeper RGB pixel in header overlap using extracted alpha.
keeper=Image.open(O/'Keeper_Full.png').convert('RGBA').resize((895,1041),Image.Resampling.LANCZOS)
keepmask=Image.new('L',main.size);keepmask.paste(keeper.getchannel('A'),(18,39))
header=Image.new('L',main.size);ImageDraw.Draw(header).rectangle((70,20,496,132),fill=255)
main=Image.composite(src,main,ImageChops.multiply(keepmask,header))
main.convert('RGB').save(P/'Shop_A_Mockup.png')

# Eight specified states and both purchase bars on one annotated review sheet.
sheet=Image.new('RGBA',(1920,1080),PAPER)
txt(sheet,'상점 · 상품 상태',[42,20,900,45],34,INK,True)
txt(sheet,'SUPPLY EXCHANGE / COMPONENT STATES',[1135,29,740,30],18,GRAY,latin=True)
states=[('보통','coffee','normal'),('선택','parts','selected'),('탈린 부족','parts','insufficient'),('재고 2/3','supply','stock'),('품절','parts','soldout'),('무색 카드 · 해금 전','card','normal'),('해금 완료','card','unlocked'),('미발견 · ???','unknown','normal')]
for i,(label,kind,state) in enumerate(states):
 x=42+(i%4)*474;y=77+(i//4)*421
 txt(sheet,label,[x,y,430,30],22,INK,True)
 im=card(kind,state);sheet.alpha_composite(im,(x+64,y+39))
txt(sheet,'구매 가능',[42,919,900,23],17,INK,True)
txt(sheet,'탈린 부족 · 구매 비활성',[1000,919,875,23],17,INK,True)
normal,_=buybar();disabled,_=buybar(True)
sheet.alpha_composite(normal.resize((900,76),Image.Resampling.LANCZOS),(42,953))
sheet.alpha_composite(disabled.resize((900,76),Image.Resampling.LANCZOS),(978,953))
sheet.convert('RGB').save(P/'Shop_A_States.png')

# Text placement templates; all labels remain runtime TMP text.
itemtext=[{'id':'index','rect':[17,15,35,18],'size':15,'color':GRAY},{'id':'stock','rect':[184,12,105,25],'size':15,'color':INK},{'id':'name','rect':[23,237,258,31],'size':26,'color':INK},{'id':'description','rect':[23,274,258,23],'size':18,'color':INK},{'id':'price','rect':[64,313,204,35],'size':30,'color':INK,'insufficient_color':GRAY},{'id':'state_band','rect':[0,140,304,44],'size':25,'alignment':'center'}]
dialogue={'rect':[36,893,502,129],'name_box':[95,915,410,25],'dialogue_box':[95,955,410,53],'name_size':18,'dialogue_size':22,'name':'보급 담당','lines':{'entry':'필요한 물건 있어? 천천히 둘러봐.','purchased':'준비해 뒀어. 잘 써.','insufficient':'탈린이 조금 모자라네.','sold_out':'지금은 재고가 없어.'},'mockup':'Original dialogue pixels unchanged'}
layout={'canvas':[1920,1080],'origin':'top-left','rect_format':'x,y,width,height','assets':assets,'elements':elements,'text_boxes':texts,'item_template':{'size':[304,354],'text_boxes':itemtext,'product_art_rect':[23,38,258,176],'price_icon_rect':[25,319,25,25],'card_front_rect':[87,31,130,202]},'dialogue':dialogue,'keeper':{'file':'Keeper_Full.png','rect':[18,39,895,1041],'source_rect_ltrb':[16,34,795,941],'occlusion_notice':extraction['Keeper_Full']['limitation']},'buybar':{'rect':[944,960,930,78],'quantity_minus_rect':[1323,982,34,34],'quantity_text_rect':[1359,982,38,34],'quantity_plus_rect':[1399,982,34,34],'button_rect':[1659,960,215,78],'formula':'remaining = balance - unitPrice * quantity','quantity_max':'min(sharedPartsStock, floor(balance / unitPrice), 99); unlock cards fixed at 1','example':{'balance':1240,'unitPrice':120,'quantity':1,'remaining':1120}},'state_rules':{'priority':['already_unlocked','sold_out','insufficient_currency'],'shared_parts_stock':'2/3 applies to both parts products','sold_out':'Dim entire card; centered SoldOut band; no purchase','unlocked':'Unlocked band; price and currency icon hidden','unknown':'Armory silhouette + ???; no purchase','insufficient':'Gray price digits and disabled buy button'},'notes':['All panel/icon SVG assets are text-free; quantity symbols and labels belong to TMP.','Tab placement kept at shop right per user clarification; armory tab shape/height used.','The two card slots are visual previews; current game catalog contains the four physical products only.','Keeper is visible original pixels, with missing occluded pixels transparent; do not treat as a completed full-body illustration.']}
(O/'layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf-8')
allowedmask=Image.new('L',main.size);d=ImageDraw.Draw(allowedmask)
for x,y,w,h in allowed:d.rectangle((x,y,x+w-1,y+h-1),fill=255)
diff=ImageChops.difference(src.convert('RGB'),main.convert('RGB'))
outside=ImageChops.multiply(diff,Image.merge('RGB',(ImageOps.invert(allowedmask),)*3))
assert outside.getbbox() is None,'Unintended screenshot modification'
assert all('<text' not in f.read_text() for f in S.glob('*.svg'))
for f in (P/'Currency_Icons').glob('*.png'):assert f.read_bytes()==(O/f.name).read_bytes()
validation={'mockup_size':[1920,1080],'state_sheet_size':[1920,1080],'states':len(states),'source_preserved_outside_UI_rects':True,'currency_icons_byte_identical':True,'vector_assets_text_free':True,'extraction_limitation':extraction['Keeper_Full']['limitation']}
(O/'validation.json').write_text(json.dumps(validation,ensure_ascii=False,indent=2),encoding='utf-8')
with zipfile.ZipFile(P/'Shop_A_Package.zip','w',zipfile.ZIP_DEFLATED) as z:
 for f in [P/'Shop_A_Mockup.png',P/'Shop_A_States.png',P/'Extraction_Review.png',P/'extract_source.py',Path(__file__),*sorted(O.rglob('*'))]:
  if f.is_file():z.write(f,f.relative_to(P))
print(json.dumps(validation))
