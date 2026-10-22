"""Armory v2: deterministic SVG sprites, exact source currency icons, layouts and proofs."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,subprocess,base64,shutil,hashlib,zipfile,math,copy

P=Path(__file__).resolve().parent; O=P/'Extracted_Draft'; S=O/'SVG'; S.mkdir(parents=True,exist_ok=True)
CYAN='#0DB8F2'; INK='#16181B'; WHITE='#EEF0F2'; MUTED='#929AA3'
A={}; screens={k:{'placements':[],'text_boxes':[]} for k in ['hub','equipment','card']}

def svg(n,w,h,body,border=None,**kw):
    (S/(n+'.svg')).write_text(f'<svg xmlns="http://www.w3.org/2000/svg" width="{w*2}" height="{h*2}" viewBox="0 0 {w} {h}">{body}</svg>',encoding='utf-8')
    A[n]={'file':n+'.png','svg':'SVG/'+n+'.svg','logical_size':[w,h],'source_pixels':[w*2,h*2],'scale':2,'text_baked':False,'pivot':[.5,.5],'method':'SVG reconstruction of approved design',**kw}
    if border:A[n]['nine_slice']={'source_pixels_left_bottom_right_top':[v*2 for v in border],'logical_pixels_left_bottom_right_top':border,'minimum_logical_size':[border[0]+border[2]+1,border[1]+border[3]+1]}

def plate(w,h,fill=INK,edge='#727D87',opacity=.92,cut=16):
    return f'<path d="M{cut} 1H{w-1}V{h-cut}L{w-cut} {h-1}H1V{cut}Z" fill="{fill}" fill-opacity="{opacity}" stroke="{edge}" stroke-width="1.3"/>'

def cross(x,y,r=8,c='#A4AFB7'):
    return f'<path d="M{x-r} {y}H{x+r}M{x} {y-r}V{y+r}" stroke="{c}" fill="none"/>'

# Raster painting stays raster; self-contained SVG wrapper is not a vector trace.
bg=base64.b64encode((P/'Workshop_BG_Source.png').read_bytes()).decode()
svg('Workshop_BG',1920,1080,f'<image width="1920" height="1080" preserveAspectRatio="none" href="data:image/png;base64,{bg}"/>',method='image_gen clean plate; embedded raster SVG wrapper, not vectorized')
for n,w,h in [('Panel_Title',450,130),('Panel_TitlePath',650,118),('Button_Back',240,58),('Panel_Dark',500,500),('Menu_Panel',710,215),('Menu_Panel_Hover',710,215),('Panel_Dialogue',560,126),('Panel_CardDetail',560,830),('Currency_Bar',450,58)]:
    edge=CYAN if n.endswith('Hover') else '#77828C'
    body=plate(w,h,edge=edge,cut=24 if n.startswith('Menu') else 16)
    if n.startswith('Menu'):body+=cross(37,h-31)+cross(w-15,18)
    svg(n,w,h,body,[30,42,30,30] if n.startswith('Menu') else [20,20,20,20])
svg('Panel_Paper',794,830,plate(794,830,WHITE,'#BCC3C9',1,24),[28,28,28,28])
for n,c in [('Tab_Normal',INK),('Tab_Selected',CYAN)]:
    svg(n,260,48,f'<path d="M28 1H259L232 47H1Z" fill="{c}" fill-opacity=".94" stroke="#7C8A94"/>',[30,6,30,6])
for n,c in [('Menu_Diamond','#0DB8F2'),('Menu_Diamond_Hover','#42D1FF')]:
    svg(n,156,156,f'<path d="M78 2L154 78L78 154L2 78Z M78 12L144 78L78 144L12 78Z" fill="none" stroke="{c}" stroke-width="2"/>')
equipment_icon='<path d="M16 10L32 26L26 35L14 31L8 19C2 36 13 50 30 46L68 87L85 69L46 30C50 14 35 3 20 9L31 20L26 26Z" fill="#EEF0F2"/><path d="M70 12L82 4L92 14L84 27L73 30L33 73L32 83L19 93L7 80L17 68L28 67L69 26Z" fill="#EEF0F2" stroke="#16181B" stroke-width="2"/>'
svg('Icon_Equipment',100,100,equipment_icon)
svg('Icon_CardUpgrade',100,100,'<g fill="none" stroke="#EEF0F2" stroke-width="3"><path d="M32 11L83 16V61M21 19L72 24V76M9 30H59V94H9Z"/></g><path d="M74 41L97 67H82V91H65V67H51Z" fill="#EEF0F2"/>')
for n,c,edge in [('WeaponSlot',INK,'#646F78'),('WeaponSlot_Selected',INK,CYAN),('WeaponSlot_Locked','#22282D','#606A73')]:svg(n,310,138,plate(310,138,c,edge,.86,6),[10,10,10,10])
svg('Weapon_Sniper_Silhouette',240,85,'<path d="M5 35H87V27H130V31H155L161 37H203L237 28V57L205 50H160L142 58L129 50L124 77H110L114 50H87V44H5ZM105 16H143V24H105Z M113 24H118V29H113Z M134 24H139V29H134Z" fill="#717D88"/>',method='Vector placeholder rifle silhouette for locked equipment')
svg('Weapon_Shotgun_Silhouette',240,85,'<path d="M3 30H134V35H169L191 46L233 39V67L193 59L167 49H147L136 68H125L127 48H62V44H3ZM35 42H98V55H35Z" fill="#717D88"/>',method='Vector placeholder shotgun silhouette for locked equipment')
lock='<path d="M10 16V10a10 10 0 0120 0v6" fill="none" stroke="#EEF0F2" stroke-width="3"/><rect x="5" y="15" width="30" height="28" rx="2" fill="none" stroke="#EEF0F2" stroke-width="3"/><circle cx="20" cy="26" r="3" fill="#EEF0F2"/><path d="M20 28V35" stroke="#EEF0F2" stroke-width="3"/>'
svg('Icon_Lock',40,46,lock)
for n,c,inner in [('Step_Done',WHITE,'<path d="M19 36L30 47L51 23" fill="none" stroke="#EEF0F2" stroke-width="5"/>'),('Step_Current',CYAN,'<circle cx="35" cy="35" r="26" fill="#0DB8F2"/>'),('Step_Locked','#9CA4AB','<g transform="translate(19 16) scale(.8)">'+lock+'</g>')]:svg(n,70,70,f'<circle cx="35" cy="35" r="33" fill="#222A30" stroke="{c}" stroke-width="3"/>'+inner)
svg('Step_Line',4,150,'<path d="M2 0V150" stroke="#CAD1D8" stroke-width="2"/>',[1,1,1,1])
for n,w,h,c,edge in [('Button_Upgrade',514,115,INK,CYAN),('Button_Upgrade_Disabled',514,115,'#525B64','#78838C'),('Button_Small',290,54,INK,'#7F8992')]:
    body=plate(w,h,c,edge,1,24 if h>100 else 6)
    if n=='Button_Upgrade':body+=f'<path d="M3 {h-4}H{w-26}L{w-3} {h-27}" fill="none" stroke="{CYAN}" stroke-width="4"/>'
    svg(n,w,h,body,[28,28,28,28] if h>100 else [10,10,10,10])
svg('Row_BaseCard',710,64,plate(710,64,'#E0E4E8','#A6AFB8',.9,1),[4,4,4,4])
for n,on in [('Checkbox_Off',False),('Checkbox_On',True)]:svg(n,28,28,f'<rect x="1" y="1" width="26" height="26" rx="2" fill="{CYAN if on else "none"}" stroke="{CYAN if on else "#97A1AA"}" stroke-width="2"/>'+('<path d="M6 14L12 20L23 7" fill="none" stroke="#16181B" stroke-width="2"/>' if on else ''))
for n,c in [('CardCell','#B7C0C8'),('CardCell_Selected',CYAN)]:svg(n,194,348,plate(194,348,WHITE,c,1,7),[11,11,11,11])
svg('Mark_Upgraded',34,34,'<rect x="1" y="1" width="32" height="32" rx="4" fill="#16181B" stroke="#0DB8F2"/><path d="M17 7V27M7 17H27" stroke="#0DB8F2" stroke-width="4"/>')
svg('Rule',700,2,'<path d="M0 1H700" stroke="#7E8A94" stroke-opacity=".6"/>')
svg('Icon_Attack',48,48,'<path d="M24 0L29 16L45 5L34 21L48 24L32 30L42 44L27 34L24 48L19 32L3 43L14 27L0 24L16 19L5 3L21 14Z" fill="#16181B"/>')
svg('Icon_Defense',48,48,'<path d="M3 5L24 0L45 5V25Q43 39 24 47Q5 39 3 25Z M9 10L24 7L39 10V25Q36 35 24 40Q12 35 9 25Z" fill="none" stroke="#16181B" stroke-width="3"/>')
svg('Icon_Check',32,32,'<path d="M4 16L12 24L29 5" fill="none" stroke="#0DB8F2" stroke-width="4"/>')
svg('Compass',500,500,'<g fill="none" stroke="#A7B0B9" stroke-opacity=".22" stroke-width="3"><circle cx="250" cy="250" r="164"/><path d="M250 10V490M10 250H490"/></g><path d="M250 0L277 221L500 250L277 277L250 500L221 277L0 250L221 221Z" fill="#A7B0B9" fill-opacity=".18"/>')

renderer=P/'render_delivery_svg.cjs'
renderer.write_text("const fs=require('fs'),p=require('path');const {Resvg}=require(process.env.VERTEX_RESVG_MODULE||'C:/Users/SKYLIGHT/.codex/tmp/currency-f-render/node_modules/@resvg/resvg-js');for(const f of fs.readdirSync(process.argv[2]))if(f.endsWith('.svg'))fs.writeFileSync(p.join(process.argv[3],f.replace('.svg','.png')),new Resvg(fs.readFileSync(p.join(process.argv[2],f))).render().asPng());",encoding='utf-8')
subprocess.run(['node',str(renderer),str(S),str(O)],check=True)

currency=P.parent/'LobbyShopMockup/Currency_Icons'
for f in currency.glob('*.png'):
    shutil.copyfile(f,O/f.name);im=Image.open(f)
    A[f.stem]={'file':f.name,'source_pixels':list(im.size),'logical_size':[v/2 for v in im.size],'scale':2,'text_baked':False,'method':'byte-identical supplied currency icon','source':'../../LobbyShopMockup/Currency_Icons/'+f.name,'sha256':hashlib.sha256(f.read_bytes()).hexdigest()}

# Reuse already-separated weapon illustration. New card art is cropped from generated v2.
for name,source in [('Weapon_Pistol',P.parent/'ArmoryMockup/Extracted/WeaponArt_Pistol.png'),('Weapon_SMG',P.parent/'ArmoryMockup/Extracted/SMG_Thumbnail.png')]:
    shutil.copyfile(source,O/(name+'.png'));im=Image.open(source)
    A[name]={'file':name+'.png','source_pixels':list(im.size),'logical_size':list(im.size),'scale':1,'text_baked':False,'method':'existing project-separated raster art; native resolution preserved','source':str(source.relative_to(P.parent))}

rawcard=Image.open(P/'Armory_Card_Generated_v2.png').convert('RGBA')
artnames=['Shot','Defense','Reload','StockStrike','QED','Advance','EmptyMagazine','Vest']
for i,n in enumerate(artnames):
    x=[368,550,733,912][i%4];y=255 if i<4 else 570
    crop=[x,y,x+153,y+150]
    # Defense's generated + badge is excluded by choosing a slightly narrower clean art crop.
    if i==1:crop=[550,290,699,405]
    im=rawcard.crop(crop);im.save(O/('Art_'+n+'.png'))
    A['Art_'+n]={'file':'Art_'+n+'.png','source_pixels':list(im.size),'logical_size':list(im.size),'scale':1,'text_baked':False,'method':'art-only crop from image_gen v2','source':'../Armory_Card_Generated_v2.png','source_crop_ltrb':crop}
im=rawcard.crop((1162,255,1568,442));im.save(O/'Art_Shot_Detail.png')
A['Art_Shot_Detail']={'file':'Art_Shot_Detail.png','source_pixels':list(im.size),'logical_size':list(im.size),'scale':1,'text_baked':False,'method':'art-only crop','source':'../Armory_Card_Generated_v2.png','source_crop_ltrb':[1162,255,1568,442]}

def put(k,n,r,id=None,**kw):screens[k]['placements'].append({'asset':n,'rect':r,'id':id or n,**kw})
def text(k,id,v,r,size=22,color=WHITE,bold=False,align='left',**kw):screens[k]['text_boxes'].append({'id':id,'text':v,'rect':r,'font_size':size,'color':color,'font_weight':700 if bold else 400,'alignment':align,'vertical_alignment':'middle','character_spacing_px':0,'runtime':'TMP',**kw})
def rule(k,x,y,w):put(k,'Rule',[x,y,w,2])
def money(k,kind,value,x,y,size=30,color=WHITE,id='',dark=False):
    put(k,'Icon_'+kind+('' if dark else '_White'),[x,y,size,size],id=id+'.icon')
    text(k,id+'.value',str(value),[x+size+9,y,90,size],size-5,color,bold=True)

def header(k,title):
    put(k,'Workshop_BG',[0,0,1920,1080])
    rect=[0,0,450,130] if k=='hub' else [80,24,650,110]
    put(k,'Panel_Title' if k=='hub' else 'Panel_TitlePath',rect,id='header.plate')
    if k=='hub':
        text(k,'header.number','03',[35,26,85,65],48,bold=True);rule(k,140,32,2)
        text(k,'header.title','무기고',[170,22,240,65],46,bold=True);text(k,'header.en','ARMORY / UPGRADE',[172,91,255,24],17,MUTED)
    else:
        text(k,'header.path','03 무기고 › '+title,[108,37,600,56],36,bold=True)
        text(k,'header.en','ARMORY / '+('EQUIPMENT UPGRADE' if k=='equipment' else 'CARD UPGRADE'),[110,98,560,23],16,MUTED)
    put(k,'Currency_Bar',[1135,40,450,58]);put(k,'Button_Back',[1610,40,240,58])
    money(k,'Talin','1,240',1150,50,38,id='currency.talin')
    money(k,'Parts',36,1310,50,38,id='currency.parts')
    money(k,'Artifact',2,1455,50,38,id='currency.artifact')
    text(k,'header.back','←  '+('로비' if k=='hub' else '무기고'),[1622,44,216,48],27,bold=True,align='center')
    if k!='hub':
        for i,label in enumerate(['장비 강화','카드 강화']):
            selected=(i==0)==(k=='equipment');put(k,'Tab_Selected' if selected else 'Tab_Normal',[108+i*260,144,260,48])
            text(k,'tab.'+str(i),label,[130+i*260,149,210,35],24,INK if selected else WHITE,bold=selected,align='center')

for k,t in [('hub',''),('equipment','장비 강화'),('card','카드 강화')]:header(k,t)

# Hub original character is referenced externally and never exported or included in ZIP.
put('hub','__mechanic__',[485,32,670,1005],external_source='../Source_Mechanic.png',preserve_aspect=True,package_excluded=True)
screens['hub']['character']={'external_source':'../Source_Mechanic.png','rect':[485,32,670,1005],'pivot_normalized_bottom_left':[.5,.02],'package_excluded':True,'runtime':'use original Source_Mechanic, not generated character'}
for i,(title,rest,eng,desc,icon) in enumerate([('장','비 강화','EQUIPMENT UPGRADE','무기 성능을 강화합니다','Icon_Equipment'),('카','드 강화','CARD UPGRADE','카드 효과를 강화합니다','Icon_CardUpgrade')]):
    y=245+i*270;put('hub','Menu_Panel_Hover' if i==0 else 'Menu_Panel',[1125,y,710,215]);put('hub','Menu_Diamond_Hover' if i==0 else 'Menu_Diamond',[1140,y+27,156,156]);put('hub',icon,[1168,y+55,100,100])
    text('hub',f'menu.{i}.index',f'{i+1:02}',[1315,y+13,65,24],16,MUTED)
    text('hub',f'menu.{i}.initial',title,[1312,y+29,88,86],74,CYAN,bold=True)
    text('hub',f'menu.{i}.rest',rest,[1398,y+60,300,62],43,bold=True)
    text('hub',f'menu.{i}.english',eng,[1317,y+129,460,26],17,MUTED)
    rule('hub',1317,y+158,460);text('hub',f'menu.{i}.description',desc,[1317,y+170,460,31],24,'#BEC6CD')
    text('hub',f'menu.{i}.arrow','›',[1780,y+79,35,53],48,align='center')
put('hub','Panel_Dialogue',[30,892,560,126]);text('hub','dialogue.name','정비 담당',[80,902,430,31],22,bold=True);rule('hub',80,942,480)
text('hub','dialogue.line','어떤 장비를 손봐 줄까?',[80,953,470,42],25,bold=True)

# Equipment layout.
put('equipment','Panel_Dark',[80,198,350,830]);put('equipment','Panel_Paper',[460,198,794,830]);put('equipment','Panel_Dark',[1272,198,576,830])
text('equipment','weapons.heading','무기 목록',[118,218,270,60],37,bold=True)
for i,name in enumerate(['권총','SMG','저격총','산탄총']):
    y=282+i*149;put('equipment','WeaponSlot_Selected' if i==0 else 'WeaponSlot' if i==1 else 'WeaponSlot_Locked',[108,y,298,138])
    if i<2:put('equipment','Weapon_Pistol' if i==0 else 'Weapon_SMG',[168,y+10,196,90],fit='contain',**({'tint':'#A5AFB8'} if i==1 else {}))
    else:
        put('equipment','Weapon_Sniper_Silhouette' if i==2 else 'Weapon_Shotgun_Silhouette',[139,y+15,240,75],fit='contain',opacity=.7,placeholder=True)
        put('equipment','Icon_Lock',[240,y+31,35,40]);money('equipment','Artifact',1,267,y+96,28,id=f'weapon.{i}.unlock')
    text('equipment',f'weapon.{i}.name',name,[125,y+99,135,30],25,bold=True)
    if i==0:put('equipment','Icon_Check',[360,y+10,32,32])
text('equipment','unlock.title','무기 해금',[124,920,255,36],27,bold=True)
text('equipment','unlock.hint','아티팩트로 새 무기 해금',[124,963,275,31],20,MUTED)
put('equipment','Compass',[650,208,530,530]);text('equipment','weapon.title','권총',[500,214,300,80],62,INK,bold=True)
put('equipment','Weapon_Pistol',[680,294,515,420],fit='contain')
text('equipment','weapon.level','Lv.1',[510,305,112,43],30,INK,bold=True);put('equipment','Icon_Check',[510,366,27,27]);text('equipment','weapon.unlocked','해금 완료',[546,360,155,40],23,'#65717A')
for i,(label,val) in enumerate([('탄창','3'),('사격','6 → 7'),('연사','3 × 3')]):
    x=510+i*218;text('equipment',f'stat.{i}.label',label,[x,648,185,30],23,INK);text('equipment',f'stat.{i}.value',val,[x,686,200,54],43,CYAN if i==1 else INK,bold=True)
rule('equipment',510,750,700);text('equipment','next','다음 강화: 사격 피해 +1',[515,754,680,37],25,INK)
text('equipment','base.title','기본 카드 강화',[510,798,650,45],32,INK,bold=True)
for i,name in enumerate(['사격','방어']):
    y=852+i*70;put('equipment','Row_BaseCard',[510,y,710,64]);put('equipment','Icon_Attack' if i==0 else 'Icon_Defense',[524,y+9,45,45]);text('equipment',f'base.{i}.name',name,[585,y+10,85,43],25,INK,bold=True)
    text('equipment',f'base.{i}.effect','피해 6 → 7' if i==0 else '방어 5 → 6',[675,y+13,225,37],23,'#626D77')
    put('equipment','Button_Small',[919,y+5,290,54]);money('equipment','Talin',80,932,y+15,31,id=f'base.{i}.talin');money('equipment','Parts',3,1038,y+15,31,id=f'base.{i}.parts');text('equipment',f'base.{i}.action','강화',[1136,y+13,65,37],23,bold=True)
text('equipment','base.hint','출정 준비의 시작 덱에 반영',[800,992,415,27],19,'#626D77',align='right')
text('equipment','progress.title','강화 단계',[1305,221,240,49],37,bold=True)
text('equipment','facility','시설 Lv.1  |  최대 강화 Lv.2',[1510,239,310,30],19,MUTED)
rule('equipment',1305,283,510);put('equipment','Step_Line',[1442,350,4,294])
for i,(n,y,lv,desc) in enumerate([('Step_Done',316,'Lv.1','기본 성능'),('Step_Current',470,'Lv.2','사격 피해 +1'),('Step_Locked',624,'Lv.3','시설 Lv.2 필요')]):
    put('equipment',n,[1408,y,70,70]);text('equipment',f'step.{i}.level',lv,[1520,y+2,130,43],33,CYAN if i==1 else WHITE,bold=True)
    text('equipment',f'step.{i}.description',desc,[1520,y+49,300,37],24,WHITE if i<2 else MUTED)
    if i:money('equipment','Parts',10 if i==1 else 20,1660,y+10,31,id=f'step.{i}.cost')
rule('equipment',1305,740,510);text('equipment','cost.label','강화 비용',[1305,759,230,34],25)
money('equipment','Parts',10,1314,808,64,id='cost.parts')
text('equipment','balance','보유 36 → 강화 후 26',[1515,819,307,40],25,bold=True)
put('equipment','Button_Upgrade',[1299,902,514,115],id='upgrade.button');text('equipment','upgrade.label','강화 →',[1340,920,425,75],56,bold=True)
screens['equipment']['costs']={'level2':{'parts':10},'level3':{'parts':20,'requires_facility_level':2},'base_card':{'talin':80,'parts':3},'weapon_unlock':{'artifact':1},'wallet_example':{'talin':1240,'parts':36,'artifact':2},'after_example':{'parts':26}}

# Card screen: all body copy lives in text_boxes, separated from art and panels.
put('card','Panel_Dark',[80,198,292,830]);put('card','Panel_Dark',[387,198,884,830]);put('card','Panel_CardDetail',[1288,198,560,830])
text('card','filter.title','필터',[112,222,215,50],37,bold=True)
groups=[('소속',303,['전체','주인공','치하라 쇼','무색'],[41,39,2,0]),('종류',567,['공격','스킬','파워'],[]),('등급',773,['일반','특별','희귀'],[])]
for g,y,labels,counts in groups:
    rule('card',112,y-16,224);text('card','filter.'+g,g,[112,y,220,40],27,bold=True)
    for i,v in enumerate(labels):
        yy=y+52+i*47;put('card','Checkbox_On' if g=='소속' and i==0 else 'Checkbox_Off',[119,yy,28,28]);text('card',f'filter.{g}.{i}',v,[165,yy-2,140,32],23)
        if counts:text('card',f'filter.{g}.{i}.count',str(counts[i]),[310,yy-2,40,32],22,align='right')
text('card','collection','수집 40 / 41',[119,974,225,35],24,bold=True)
text('card','grid.title','카드 목록',[419,220,360,50],37,bold=True);text('card','grid.count','표시 41 / 41',[1000,232,222,36],22,MUTED,align='right')
names=['사격','방어','재장전','개머리판 타격','Q.E.D','전진사격','엠티 매거진','방탄복']
descs=['적에게 6 피해를\n줍니다.','방어도 5만큼\n증가합니다.','최대 탄약으로\n재장전합니다.','적에게 8 피해를\n줍니다.','3턴 동안 방어도를\n얻습니다.','적에게 6 피해를\n줍니다.','남은 탄약을 모두\n소비합니다.','방어도 1만큼\n증가합니다.']
card_rects=[]
for i,n in enumerate(names):
    x=419+(i%4)*207;y=290+(i//4)*362;r=[x,y,194,348];card_rects.append(r)
    put('card','CardCell_Selected' if i==0 else 'CardCell',r);put('card','Art_'+artnames[i],[x+7,y+7,180,174],fit='cover')
    text('card',f'cell.{i}.name',n,[x+11,y+184,173,39],22 if len(n)>5 else 27,INK,bold=True)
    text('card',f'cell.{i}.description',descs[i],[x+12,y+239,168,65],20,'#5D6973',line_height=1.35)
    text('card',f'cell.{i}.footer','VERTEX',[x+12,y+316,168,20],12,'#7E8992',align='center')
    if i==1:put('card','Mark_Upgraded',[x+151,y+12,34,34],id='card.upgraded.mark')
screens['card']['grid']={'columns':4,'cells':card_rects,'overflow':'vertical_scroll','viewport_rect':[410,282,850,726],'upgraded_example_index':1}
text('card','detail.title','카드 상세',[1325,222,400,50],37,bold=True)
put('card','Art_Shot_Detail',[1325,290,485,214],fit='cover')
text('card','detail.name','사격',[1325,514,460,53],38,bold=True);text('card','detail.type','공격 · 일반',[1325,572,460,35],24)
for id,y,title,line in [('current',625,'현재 효과','적에게 6 피해를 줍니다.'),('next',713,'강화 시','적에게 9 피해를 줍니다.')]:
    rule('card',1325,y-12,485);text('card',id+'.title',title,[1325,y,470,32],25,bold=True);text('card',id+'.body',line,[1325,y+34,470,35],24)
rule('card',1325,800,485);text('card','unlock','시작 덱 포함',[1325,807,470,26],18,MUTED)
text('card','cost.label','강화 비용',[1325,841,470,32],25,bold=True)
money('card','Talin',80,1335,882,37,id='cost.talin');money('card','Parts',3,1575,882,37,id='cost.parts')
text('card','balance','보유 탈린 1,240 · 파츠 36',[1325,925,480,32],22)
put('card','Button_Upgrade',[1315,965,514,60],id='upgrade.button');text('card','upgrade.label','강화하기 →',[1340,971,460,45],34,bold=True)
screens['card']['costs']={'upgrade':{'talin':80,'parts':3},'wallet_example':{'talin':1240,'parts':36,'artifact':2},'once_only':True}

states={
 'equipment_insufficient':{'screen':'equipment','reason':'parts < 10','button_asset':'Button_Upgrade_Disabled','button_text':'강화','price_text_ids':['cost.parts.value'],'price_color':MUTED,'interactable':False},
 'card_insufficient':{'screen':'card','reason':'talin < 80 OR parts < 3','button_asset':'Button_Upgrade_Disabled','button_text':'강화하기','price_text_ids':['cost.talin.value','cost.parts.value'],'price_color':MUTED,'interactable':False},
 'card_upgraded':{'screen':'card','reason':'already upgraded','button_asset':'Button_Upgrade_Disabled','button_text':'강화 완료','price_text_ids':[],'interactable':False,'selected_card_mark':'Mark_Upgraded'},
 'card_ineligible':{'screen':'card','reason':'card cannot upgrade (e.g. status)','button_asset':'Button_Upgrade_Disabled','button_text':'강화 불가','price_text_ids':[],'interactable':False},
}
layout={'version':2,'canvas':[1920,1080],'origin':'top-left','rect_format':'x,y,width,height','assets':A,'screens':screens,'states':states,'rendering':{'ui_scale':2,'nine_slice_border_order':'left,bottom,right,top SOURCE PNG pixels','alpha':'straight','filter':'bilinear','font':'Malgun Gothic (preview), bind project Korean TMP font at runtime'},'notes':['SVG panels reconstructed; detailed painted background remains an image_gen raster embedded in an SVG wrapper.','Currency PNGs are byte-identical supplied icons; their SVG source was not supplied and was not invented.','Character intentionally excluded. External Source_Mechanic is referenced only in hub preview.','Raster weapon/card art preserved at native source resolution; SVG panels and background exported at 2x logical resolution.','All labels, numbers, costs and state captions are TMP text_boxes, not sprite pixels.','Generated_v2 files are image_gen working images; final Mockup_v2 files are exact-icon corrections, and Assembly_v2 files prove sprite/layout reconstruction.']}
(O/'layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf-8')

def sliced(im,size,b):
    w,h=size;sw,sh=im.size;l,bt,r,t=b;dl,db,dr,dt=[round(v/2) for v in b]
    if w<dl+dr or h<dt+db:raise ValueError(('slice destination too small',size,b))
    out=Image.new('RGBA',size);xs=[0,l,sw-r,sw];ys=[0,t,sh-bt,sh];dx=[0,dl,w-dr,w];dy=[0,dt,h-db,h]
    for j in range(3):
        for i in range(3):
            tw=dx[i+1]-dx[i];th=dy[j+1]-dy[j]
            if tw>0 and th>0:out.alpha_composite(im.crop((xs[i],ys[j],xs[i+1],ys[j+1])).resize((tw,th),Image.Resampling.LANCZOS),(dx[i],dy[j]))
    return out

def tile(el):
    n=el['asset'];w,h=el['rect'][2:]
    if n=='__mechanic__':im=Image.open(P/'Source_Mechanic.png').convert('RGBA')
    else:im=Image.open(O/A[n]['file']).convert('RGBA')
    if n in A and 'nine_slice' in A[n]:im=sliced(im,(w,h),A[n]['nine_slice']['source_pixels_left_bottom_right_top'])
    elif el.get('fit') in ['contain','cover']:
        ratio=(min if el['fit']=='contain' else max)(w/im.width,h/im.height);im=im.resize((round(im.width*ratio),round(im.height*ratio)),Image.Resampling.LANCZOS)
        out=Image.new('RGBA',(w,h));out.alpha_composite(im,((w-im.width)//2,(h-im.height)//2));im=out
    else:im=im.resize((w,h),Image.Resampling.LANCZOS)
    if 'tint' in el:
        alpha=im.getchannel('A');im=Image.new('RGBA',im.size,el['tint']);im.putalpha(alpha)
    if 'opacity' in el:im.putalpha(im.getchannel('A').point(lambda a:round(a*el['opacity'])))
    return im

def drawtext(layer,t):
    d=ImageDraw.Draw(layer);x,y,w,h=t['rect'];font=ImageFont.truetype('C:/Windows/Fonts/'+('malgunbd.ttf' if t['font_weight']==700 else 'malgun.ttf'),t['font_size']*2)
    lines=t['text'].split('\n');lineh=round(t['font_size']*2*t.get('line_height',1.2))
    if len(lines)==1:
        bounds=font.getbbox(lines[0]);lineh=bounds[3]-bounds[1]
    total=lineh*len(lines)
    if total>h*2+4:raise ValueError(('text height overflow',t['id'],total/2,h))
    for i,line in enumerate(lines):
        width=d.textlength(line,font=font)
        if width>w*2+2:raise ValueError(('text width overflow',t['id'],width/2,w))
        b=font.getbbox(line);px=x*2+(w*2-width)/2 if t['alignment']=='center' else x*2+w*2-width if t['alignment']=='right' else x*2
        py=y*2+(h*2-total)/2+i*lineh+(lineh-(b[3]-b[1]))/2-b[1]
        d.text((px,py),line,font=font,fill=t['color'])

def render(data,base=None):
    im=Image.new('RGBA',(1920,1080)) if base is None else base.copy().convert('RGBA')
    for el in data['placements']:im.alpha_composite(tile(el),tuple(el['rect'][:2]))
    ty=Image.new('RGBA',(3840,2160))
    for t in data['text_boxes']:drawtext(ty,t)
    im.alpha_composite(ty.resize(im.size,Image.Resampling.LANCZOS));return im

for k,data in screens.items():render(data).convert('RGB').save(P/f'Armory_{k.title()}_Assembly_v2.png')

# Exact-icon mockups: retain model's painted content and composite only revised header/cost UI.
for k in ['hub','equipment','card']:
    source=P/('Armory_Hub_Mockup_v1.png' if k=='hub' else f'Armory_{k.title()}_Generated_v2.png')
    im=Image.open(source).convert('RGBA').resize((1920,1080),Image.Resampling.LANCZOS)
    clean=Image.open(O/'Workshop_BG.png').convert('RGBA').resize((1920,1080),Image.Resampling.LANCZOS)
    # Remove the entire old header footprints before adding the new header.
    # Keep the hub character untouched by restricting its cleanup to the two corners.
    cleanup=[(0,0,470,135),(1120,15,1900,125)] if k=='hub' else [(0,0,1920,145)]
    for box in cleanup:im.paste(clean.crop(box),box[:2])
    data={'placements':[],'text_boxes':[]}
    def patch_plate(n,r):data['placements'].append({'asset':n,'rect':r})
    # Expanded opaque title paths cover old header variants completely.
    for el in screens[k]['placements']:
        if el['id']=='header.plate' or el['asset'] in ['Currency_Bar','Button_Back'] or el['id'].startswith('currency.'):data['placements'].append(copy.deepcopy(el))
    for t in screens[k]['text_boxes']:
        if t['id'].startswith(('header.','currency.')):data['text_boxes'].append(copy.deepcopy(t))
    if k=='equipment':
        patch_plate('Panel_Dark',[1304,735,520,145])
        for el in screens[k]['placements']:
            if el['id'].startswith('cost.parts.'):data['placements'].append(copy.deepcopy(el))
        for t in screens[k]['text_boxes']:
            if t['id'] in ['cost.label','cost.parts.value','balance']:data['text_boxes'].append(copy.deepcopy(t))
        # Correct step icon+cost, base-card dual prices and locked-weapon artifact prices.
        for r in [[1627,453,163,47],[1627,596,163,47],[237,674,175,47],[237,828,175,47]]:patch_plate('Panel_Dark',r)
        for kind,val,x,y,id in [('Parts',10,1640,461,'step2'),('Parts',20,1640,604,'step3'),('Artifact',1,246,682,'unlock1'),('Artifact',1,246,836,'unlock2')]:
            data['placements'].append({'asset':'Icon_'+kind+'_White','rect':[x,y,31,31]});data['text_boxes'].append({'id':id,'text':str(val),'rect':[x+40,y,95,31],'font_size':25,'color':WHITE,'font_weight':700,'alignment':'left'})
        for i,y in enumerate([842,910]):
            patch_plate('Button_Small',[902,y,305,54])
            for kind,val,x in [('Talin',80,915),('Parts',3,1023)]:
                data['placements'].append({'asset':'Icon_'+kind+'_White','rect':[x,y+11,32,32]});data['text_boxes'].append({'id':f'base{i}{kind}','text':str(val),'rect':[x+40,y+11,63,32],'font_size':25,'color':WHITE,'font_weight':700,'alignment':'left'})
            data['text_boxes'].append({'id':f'action{i}','text':'강화','rect':[1125,y+9,75,36],'font_size':25,'color':WHITE,'font_weight':700,'alignment':'left'})
    elif k=='card':
        patch_plate('Panel_Dark',[1318,794,500,143])
        for el in screens[k]['placements']:
            if el['id'].startswith(('cost.talin.','cost.parts.')):
                e=copy.deepcopy(el);e['rect'][1]-=37;data['placements'].append(e)
        for t in screens[k]['text_boxes']:
            if t['id'] in ['cost.label','cost.talin.value','cost.parts.value','balance']:
                t=copy.deepcopy(t);t['rect'][1]-=37;data['text_boxes'].append(t)
    # Make correction plates opaque so old model text never bleeds through.
    for el in data['placements']:
        if el['asset'] in ['Panel_Title','Panel_TitlePath','Panel_Dark','Currency_Bar']:
            x,y,w,h=el['rect'];ImageDraw.Draw(im).rectangle([x,y,x+w-1,y+h-1],fill=INK)
    render(data,im).convert('RGB').save(P/f'Armory_{k.title()}_Mockup_v2.png')

# Full-frame state proofs and a compact state overview use exactly the same layout.
state_previews=[]
for name,st in states.items():
    data=copy.deepcopy(screens[st['screen']])
    for el in data['placements']:
        if el['id']=='upgrade.button':el['asset']=st['button_asset']
    for t in data['text_boxes']:
        if t['id']=='upgrade.label':t['text']=st['button_text'];t['color']='#BEC5CC'
        if t['id'] in st['price_text_ids']:t['color']=MUTED
    if name=='card_upgraded':data['placements'].append({'asset':'Mark_Upgraded','rect':[570,302,34,34]})
    if name=='equipment_insufficient':
        for t in data['text_boxes']:
            if t['id']=='balance':t['text']='보유 6 · 파츠 부족'
            if t['id']=='currency.parts.value':t['text']='6'
    if name=='card_insufficient':
        for t in data['text_boxes']:
            if t['id']=='balance':t['text']='보유 탈린 40 · 파츠 2'
            if t['id']=='currency.talin.value':t['text']='40'
            if t['id']=='currency.parts.value':t['text']='2'
    im=render(data);im.convert('RGB').save(P/('State_'+name+'.png'));state_previews.append((name,im))
sheet=Image.new('RGB',(1600,980),'#26313A');d=ImageDraw.Draw(sheet);font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',20)
for i,(n,im) in enumerate(state_previews):
    x=(i%2)*800;y=(i//2)*490;d.text((x+15,y+10),n,font=font,fill=WHITE);sheet.paste(im.resize((768,432),Image.Resampling.LANCZOS),(x+16,y+44))
sheet.save(P/'Armory_States_v2.png')
names=[n for n in A if n!='Workshop_BG' and not n.startswith(('Art_','Weapon_P'))];rows=math.ceil(len(names)/5)
sheet=Image.new('RGB',(1500,rows*180),'#34404B');d=ImageDraw.Draw(sheet)
for i,n in enumerate(names):
    x=(i%5)*300;y=(i//5)*180;d.rectangle([x+5,y+29,x+295,y+175],fill='#AEB8C1');d.text((x+8,y+7),n,font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',15),fill=WHITE)
    im=Image.open(O/A[n]['file']).convert('RGBA');im.thumbnail((270,135),Image.Resampling.LANCZOS);sheet.paste(im,(x+150-im.width//2,y+100-im.height//2),im)
sheet.save(P/'Armory_AssetSheet_v2.png')

required='Workshop_BG Panel_Title Panel_TitlePath Button_Back Tab_Normal Tab_Selected Panel_Dark Panel_Paper Menu_Panel Menu_Panel_Hover Menu_Diamond Menu_Diamond_Hover Icon_Equipment Icon_CardUpgrade Panel_Dialogue WeaponSlot WeaponSlot_Selected WeaponSlot_Locked Step_Done Step_Current Step_Locked Step_Line Button_Upgrade Button_Upgrade_Disabled Button_Small Row_BaseCard Checkbox_Off Checkbox_On CardCell CardCell_Selected Mark_Upgraded Panel_CardDetail'.split()
assert all(n in A for n in required)
for n,a in A.items():
    im=Image.open(O/a['file']);assert list(im.size)==a['source_pixels'],n;assert im.mode=='RGBA',n
    if a.get('svg'):assert '<text' not in (O/a['svg']).read_text(),n
    if a.get('sha256'):assert hashlib.sha256((O/a['file']).read_bytes()).hexdigest()==a['sha256'],n
    if 'nine_slice' in a:
        l,b,r,t=a['nine_slice']['source_pixels_left_bottom_right_top'];assert l+r<im.width and b+t<im.height,n
assert not any('Mechanic' in f.name or 'Chara' in f.name for f in O.rglob('*'))
for data in screens.values():
    assert all('EXP' not in t['text'] and '경험치' not in t['text'] for t in data['text_boxes'])
    for t in data['text_boxes']:
        x,y,w,h=t['rect'];assert 0<=x and 0<=y and x+w<=1920 and y+h<=1080,t['id']
report={'assets':len(A),'svg_assets':len(list(S.glob('*.svg'))),'required_assets_present':len(required),'exact_currency_hashes':6,'character_excluded':True,'text_not_baked_in_ui':True,'nine_slice_valid':True,'text_bounds_checked':True,'no_exp':True,'three_layouts':list(screens),'states':list(states),'workshop_png':[3840,2160],'preview_canvas':[1920,1080]}
(O/'validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
(O/'README.txt').write_text('Armory v2 delivery\n\nSVG panels + 2x PNG; text is in layout.json for TMP. Border order is SOURCE pixels left/bottom/right/top. Workshop_BG is a generated clean raster embedded in a self-contained SVG; it is not vector art.\nCurrency files are copied byte-for-byte from supplied originals. Raster card/weapon art retains native resolution. Character is intentionally NOT INCLUDED; bind ../Source_Mechanic.png from the parent project.\nMockup_v2 preserves image_gen artwork with exact source-icon/header corrections. Assembly_v2 proves reconstructed sprites/layout. Panel contours, texture and shadows may differ from generated reference.\nNormal, insufficient currency, upgraded, and ineligible states are defined in layout.json. Button_Upgrade_Disabled is shared, state labels remain TMP. No EXP.\nRebuild: python build_delivery_v2.py. Requires Pillow, Node and @resvg/resvg-js (override module path with VERTEX_RESVG_MODULE). Preview font: Windows Malgun Gothic. External input art/source files remain in parent project.\n',encoding='utf-8')
with zipfile.ZipFile(P/'Armory_v2_Package.zip','w',zipfile.ZIP_DEFLATED) as z:
    files=[Path(__file__),renderer,P/'REVISION_v2.md',P/'Armory_States_v2.png',P/'Armory_AssetSheet_v2.png',*P.glob('Armory_*_Mockup_v2.png'),*P.glob('Armory_*_Assembly_v2.png'),*P.glob('GENERATION_PROMPT_*_v2.txt'),*sorted(O.rglob('*'))]
    for f in files:
        if f.is_file():z.write(f,f.relative_to(P))
print(json.dumps(report))
