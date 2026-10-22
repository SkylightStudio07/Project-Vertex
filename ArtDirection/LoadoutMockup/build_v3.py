"""Loadout v3: SVG UI kit, external weapon references, exact assembly proof."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,subprocess,shutil,hashlib,copy,math,zipfile
P=Path(__file__).resolve().parent;O=P/'Extracted_Draft';S=O/'SVG';S.mkdir(parents=True,exist_ok=True)
INK='#16181B';PAPER='#EEF0F2';WHITE=PAPER;CYAN='#0DB8F2';GRAY='#68737D';A={};placements=[];texts=[]

def export(n,w,h,body,border=None,scale=2,**kw):
    (S/(n+'.svg')).write_text(f'<svg xmlns="http://www.w3.org/2000/svg" width="{w*scale}" height="{h*scale}" viewBox="0 0 {w} {h}">{body}</svg>',encoding='utf-8')
    A[n]={'file':n+'.png','svg':'SVG/'+n+'.svg','logical_size':[w,h],'source_pixels':[w*scale,h*scale],'scale':scale,'pivot':[.5,.5],'text_baked':False,'method':'SVG reconstruction from approved v2',**kw}
    if border:A[n]['nine_slice']={'logical_pixels_left_bottom_right_top':border,'source_pixels_left_bottom_right_top':[v*scale for v in border],'minimum_logical_size':[border[0]+border[2]+1,border[1]+border[3]+1]}

def plate(w,h,fill=PAPER,edge='#B6BEC7',cut=16):
    return f'<path d="M{cut} 1H{w-1}V{h-cut}L{w-cut} {h-1}H1V{cut}Z" fill="{fill}" stroke="{edge}"/>'
def cross(x,y,r=15,c='#89939D'):
    return f'<path d="M{x-r} {y}H{x+r}M{x} {y-r}V{y+r}" fill="none" stroke="{c}"/>'
def compass(x,y,r,c='#535D66'):
    return f'<circle cx="{x}" cy="{y}" r="{r*.72}" fill="none" stroke="{c}" stroke-width="3"/><path d="M{x} {y-r}L{x+10} {y-10}L{x+r} {y}L{x+10} {y+10}L{x} {y+r}L{x-10} {y+10}L{x-r} {y}L{x-10} {y-10}Z" fill="none" stroke="{c}" stroke-width="3"/>'

bg='<defs><linearGradient id="base" x2="1" y2="1"><stop stop-color="#F1F4F7"/><stop offset="1" stop-color="#CBD2D9"/></linearGradient><linearGradient id="light" x2="1" y2="1"><stop stop-color="#F7F9FB"/><stop offset="1" stop-color="#C5CED8"/></linearGradient><filter id="s" x="-30%" y="-30%" width="180%" height="180%"><feDropShadow dx="16" dy="26" stdDeviation="18" flood-color="#15202B" flood-opacity=".28"/></filter></defs>'
bg+='<rect width="1920" height="1080" fill="url(#base)"/><path d="M960 0H1192V746H960Z" fill="#424950"/><path d="M960 60L1192 322V536L960 415Z" fill="#555E67"/><path d="M0 182L237 16L390 161L0 512Z" fill="#A9B3BF"/><g filter="url(#s)"><path d="M0 274L91 117L1069 150V957L202 891L0 664Z" fill="url(#light)"/><path d="M0 842L104 743L1165 946L1242 1080H0Z" fill="#DDE4EB"/></g><path d="M813 152L1069 378V957L893 786Z" fill="#F5F7FA" fill-opacity=".45"/><path d="M0 413L345 783L446 1080H132L0 900Z" fill="#B6C1CC" fill-opacity=".6"/><path d="M1192 0H1473L1192 289Z" fill="#E2E7ED"/><path d="M1920 121V282L1709 495L1644 408Z" fill="#B2BCC7"/><path d="M1180 1080L1362 884H1860L1920 967V1080Z" fill="#BCC6D0"/>'
bg+=''.join(cross(x,y) for x,y in [(42,52),(42,438),(42,1028),(1215,166),(1570,57),(1878,52),(1878,819),(1878,1028),(1218,625)])
export('Loadout_BG',1920,1080,bg,scale=1,opaque=True)
export('Panel_Title',500,122,'<path d="M25 1H499L405 121H1V26Z" fill="#F4F7F9"/><path d="M116 23V99" stroke="#56616A"/>',[125,10,100,10])
export('Button_Back',248,60,plate(248,60,'#F1F4F6','#89939D',16),[20,20,20,20])
export('Tag_Selected',202,48,'<path d="M43 0H202L159 48H0Z" fill="#0DB8F2"/>',[45,3,45,3])
export('Panel_WeaponInfo',590,330,'<path d="M31 1H330L390 66H589V329H1V32Z" fill="#F1F4F7" stroke="#D9DFE5"/>',[35,20,205,72])
export('Rule_Vertical',2,80,'<path d="M1 0V80" stroke="#7C8791"/>',[0,1,0,1])
export('Rule_Horizontal',510,2,'<path d="M0 1H510" stroke="#9AA5AF"/>',[1,0,1,0])
export('Button_ToArmory',350,38,'<path d="M0 37H350M318 8H334V24M334 8L314 28" fill="none" stroke="#697580" stroke-width="1.5"/>',[1,4,40,4])
for n,fill,edge in [('WeaponSlot',PAPER,'#A5AFB9'),('WeaponSlot_Selected',PAPER,CYAN),('WeaponSlot_Locked','#DBE0E5','#ACB5BF'),('WeaponSlot_Empty','#E2E7EC','#B6BFC9')]:
    body=plate(250,172,fill,edge,3)
    band=INK if n.endswith('Selected') else '#C6CED6' if n.endswith('Locked') else '#E8ECF0'
    body+=f'<path d="M20 117H249V155L233 171H1V137Z" fill="{band}"/>'
    if n.endswith('Selected'):body+='<path d="M1 1H249V154L232 171H1Z" fill="none" stroke="#0DB8F2" stroke-width="3"/>'
    export(n,250,172,body,[24,57,24,8])
export('Mark_Empty',38,38,cross(19,19,17))
lock='<path d="M10 20V12a12 12 0 0124 0v8" fill="none" stroke="#64707B" stroke-width="4"/><rect x="4" y="19" width="36" height="32" rx="2" fill="#64707B"/><circle cx="22" cy="30" r="3" fill="#EEF0F2"/><path d="M22 31V39" stroke="#EEF0F2" stroke-width="3"/>'
export('Icon_Lock',44,54,lock)
section='<path d="M1 1H355L373 17H609V479H1Z" fill="#F1F4F7" stroke="#CFD6DD"/><path d="M1 1H355L373 17H609V84H1Z" fill="#16181B"/><path d="M18 18V51" stroke="#0DB8F2" stroke-width="6"/>'
export('Panel_Section',610,480,section,[28,18,260,90],fixed_header_logical_height=84)
export('Row_Deck',562,110,plate(562,110,'#F3F5F7','#CDD5DD',1),[4,4,4,4])
export('Slot_CardIcon',80,80,plate(80,80,'#E8EDF1','#D1D8DF',2),[4,4,4,4])
export('Icon_Card_Attack',64,64,'<path d="M32 1L38 24L59 7L44 28L63 32L43 38L58 60L36 44L32 63L26 43L5 57L22 35L1 32L23 26L7 5L28 23Z" fill="#16181B"/>')
export('Icon_Card_Defense',64,64,'<path d="M7 9L32 1L57 9V35Q55 49 32 63Q9 49 7 35ZM14 15L32 8L50 15V34Q47 45 32 54Q17 45 14 34Z" fill="none" stroke="#16181B" stroke-width="3"/>')
export('Icon_Card_Reload',64,64,'<g fill="#16181B"><path d="M5 25L10 1L15 25ZM5 28H15V59H5ZM27 25L32 1L37 25ZM27 28H37V59H27ZM49 25L54 1L59 25ZM49 28H59V59H49Z"/></g>')
for n,fill in [('Panel_Perk_Locked','#CDD4DC'),('Panel_Perk','#F1F4F7')]:export(n,548,128,plate(548,128,fill,'#BEC7D0',20),[24,24,24,24])
export('Perk_Thumbnail',83,100,plate(83,100,INK,'#8A97A2',8)+compass(41,50,30,'#0DB8F2'),[10,10,10,10],preserve_aspect=True)
for n,c,edge in [('Button_Deploy',INK,CYAN),('Button_Deploy_Hover','#263B47','#4AD7FF'),('Button_Deploy_Disabled','#818B95','#ABB5BE')]:
    body=plate(610,145,c,edge,27)+f'<path d="M1 141H555L607 88" stroke="{edge}" stroke-width="6" fill="none"/>'+'<defs><clipPath id="plate"><path d="M27 1H609V118L583 144H1V27Z"/></clipPath></defs><g clip-path="url(#plate)">'+compass(533,121,112,'#36414B' if n!='Button_Deploy_Disabled' else '#A1ABB4')+'</g>'
    export(n,610,145,body,[30,30,228,30])
export('Chip_Quest',300,40,plate(300,40,'#F2F5F8','#CAD1D9',9),[12,12,12,12])
export('Mark_QuestDot',14,14,'<circle cx="7" cy="7" r="6" fill="#0DB8F2"/>')
export('Icon_Quest',28,28,'<path d="M2 7L14 1L26 7V21L14 27L2 21ZM2 7L14 13L26 7M14 13V27" fill="#16181B" stroke="#EEF0F2" stroke-width="1.2"/>')
for n,w,h in [('Shadow_Panel',650,390),('Shadow_Slot',310,232)]:
    body=f'<defs><filter id="s" x="-50%" y="-50%" width="200%" height="200%"><feGaussianBlur stdDeviation="11"/></filter></defs><rect x="25" y="30" width="{w-60}" height="{h-60}" fill="#17212A" opacity=".28" filter="url(#s)"/>'
    export(n,w,h,body,[48,48,48,48],transparent_padding=30)

renderer=P/'render_v3.cjs';renderer.write_text("const fs=require('fs'),p=require('path');const {Resvg}=require(process.env.VERTEX_RESVG_MODULE||'C:/Users/SKYLIGHT/.codex/tmp/currency-f-render/node_modules/@resvg/resvg-js');for(const f of fs.readdirSync(process.argv[2]))if(f.endsWith('.svg'))fs.writeFileSync(p.join(process.argv[3],f.replace('.svg','.png')),new Resvg(fs.readFileSync(p.join(process.argv[2],f))).render().asPng());",encoding='utf-8')
subprocess.run(['node',str(renderer),str(S),str(O)],check=True)
for n,source in [('Icon_Artifact',P.parent/'LobbyShopMockup/Currency_Icons/Icon_Artifact.png'),('Mark_Upgraded',P.parent/'ArmoryHubMockup/Extracted_Draft/Mark_Upgraded.png')]:
    shutil.copyfile(source,O/(n+'.png'));im=Image.open(source)
    A[n]={'file':n+'.png','source_pixels':list(im.size),'scale':2,'logical_size':[v/2 for v in im.size],'text_baked':False,'method':'byte-identical supplied asset','source':str(source.relative_to(P.parent)),'sha256':hashlib.sha256(source.read_bytes()).hexdigest()}
    if n=='Mark_Upgraded':
        shutil.copyfile(source.parent/'SVG/Mark_Upgraded.svg',S/'Mark_Upgraded.svg');A[n]['svg']='SVG/Mark_Upgraded.svg'

def put(n,r,id=None,**kw):placements.append({'asset':n,'rect':r,'id':id or n,**kw})
def label(id,value,r,size=23,color=INK,bold=False,align='left',**kw):texts.append({'id':id,'text':value,'rect':r,'font_size':size,'color':color,'alignment':align,'vertical_alignment':'middle','font_family':'Malgun Gothic','font_weight':700 if bold else 400,'character_spacing_px':0,'runtime':'TMP',**kw})
def shadow(r,slot=False):
    x,y,w,h=r;put('Shadow_Slot' if slot else 'Shadow_Panel',[x-16,y-10,w+60,h+60])
def panel(n,r,id=None):shadow(r);put(n,r,id)

put('Loadout_BG',[0,0,1920,1080]);panel('Panel_Title',[72,17,500,122]);label('header.number','01',[120,45,65,50],30,bold=True);label('header.title','출정 준비',[214,31,340,65],52,bold=True);label('header.en','EXPEDITION / LOADOUT',[216,106,330,23],16,GRAY,spacing_note='runtime may apply 1.5px tracking')
panel('Button_Back',[1605,37,248,60]);label('back','←  로비',[1615,43,228,47],28,bold=True,align='center')
quest_origin=[1248,24];put('Chip_Quest',[1248,24,300,40],id='quest.header');label('quest.heading','진행 중 의뢰 1 / 2',[1262,28,273,32],19,bold=True)
put('Chip_Quest',[1248,66,300,40],id='quest.row.0');put('Icon_Quest',[1263,71,28,28],id='quest.icon.0');put('Mark_QuestDot',[1500,78,14,14],id='quest.dot.0');label('quest.name.0','블랙박스 회수',[1298,70,192,31],18,bold=True)
put('Tag_Selected',[85,186,202,48]);label('selected','선택 중',[115,193,139,33],22,bold=True,align='center')

weapon_sources={n:{'source':'../../WeaponSVG/Weapon_'+en+'_Dark.png','source_from_project':'ArtDirection/WeaponSVG/Weapon_'+en+'_Dark.png','source_pixels':list(Image.open(P.parent/'WeaponSVG'/('Weapon_'+en+'_Dark.png')).size),'sha256':hashlib.sha256((P.parent/'WeaponSVG'/('Weapon_'+en+'_Dark.png')).read_bytes()).hexdigest(),'included':False} for n,en in [('pistol','Pistol'),('sniper','Sniper'),('railgun','Railgun')]}
# Original weapon points right. Keep its original aspect, direction and source pixels.
# A slightly higher wide hero area avoids covering the left-facing grip with the info plate.
hero_rect=[335,175,855,395]
put('__weapon__',hero_rect,id='weapon.hero',external='pistol',fit='contain_alpha_bounds')
info=[60,570,590,230];panel('Panel_WeaponInfo',info)
label('weapon.name','권총',[108,584,290,67],53,bold=True);label('weapon.subtitle','기본 시작 무기',[110,651,400,29],24,GRAY)
put('Rule_Horizontal',[108,691,505,2]);put('Rule_Vertical',[250,705,2,54]);put('Rule_Vertical',[460,705,2,54])
for i,(title,val,x,w) in enumerate([('탄창','3',110,120),('사격 추가 피해','+0',280,165),('강화','0 / 2',491,122)]):
    label(f'stat.{i}.label',title,[x,701,w,26],19,GRAY);label(f'stat.{i}.value',val,[x,729,w,42],31,bold=True)
put('Button_ToArmory',[108,765,350,28],id='button.armory');label('button.armory.label','무기고에서 강화',[108,765,275,26],19,GRAY)

slots=[]
for i,(id,name,state) in enumerate([('pistol','권총','selected'),('sniper','스나이퍼','locked'),('railgun','레일건','locked'),(None,'','empty')]):
    x=65+i*276;y=833;r=[x,y,250,172];shadow(r,True);put({'selected':'WeaponSlot_Selected','locked':'WeaponSlot_Locked','empty':'WeaponSlot_Empty'}[state],r,id='weapon.slot.'+str(i))
    art=[x+15,y+10,220,100];slots.append({'index':i,'weapon_id':id,'state':state,'rect':r,'art_rect':art,'fit':'contain_alpha_bounds','external_source':weapon_sources[id]['source'] if id else None,'unlock_cost':{'artifact':1} if state=='locked' else None})
    if id:put('__weapon__',art,id='weapon.slot.art.'+str(i),external=id,fit='contain_alpha_bounds')
    if state=='locked':
        put('Icon_Lock',[x+109,y+45,32,40]);put('Icon_Artifact',[x+183,y+129,25,25]);label('weapon.slot.cost.'+str(i),'1',[x+218,y+127,25,29],21)
    if state=='empty':put('Mark_Empty',[x+106,y+45,38,38])
    else:label('weapon.slot.name.'+str(i),name,[x+12,y+126,165 if state=='locked' else 226,34],25,WHITE if state=='selected' else INK,bold=True,align='left' if state=='locked' else 'center')

deck_rect=[1252,153,610,479];panel('Panel_Section',deck_rect,id='deck.panel');label('deck.title','시작 덱 (10)',[1298,167,390,40],31,WHITE,bold=True);label('deck.en','STARTING DECK',[1300,211,390,24],17,'#ABB5BD')
deck_rows=[]
for i,(name,effect,count,icon,upgraded) in enumerate([('사격','피해 6 → 7',5,'Attack',True),('방어','방어 5 → 6',4,'Defense',True),('재장전','탄약 3',1,'Reload',False)]):
    x=1276;y=262+i*110;r=[x,y,562,110];put('Row_Deck',r,id=f'deck.row.{i}');put('Slot_CardIcon',[x+13,y+15,80,80]);put('Icon_Card_'+icon,[x+21,y+23,64,64]);label(f'deck.{i}.name',name,[x+128,y+16,305,35],27,bold=True);label(f'deck.{i}.effect',effect,[x+128,y+57,315,31],23,GRAY);label(f'deck.{i}.count','×'+str(count),[x+460,y+36,80,45],33,bold=True,align='right')
    if upgraded:put('Mark_Upgraded',[x+519,y+6,28,28],id=f'deck.upgraded.{i}')
    deck_rows.append({'rect':r,'name':name,'effect':effect,'count':count,'icon':'Icon_Card_'+icon,'upgraded':upgraded})
label('deck.hint','강화 수치 반영',[1590,597,246,26],18,GRAY,align='right')
panel('Panel_Section',[1252,650,610,212],id='perk.section');label('perk.title','특전 카드',[1298,667,480,43],32,WHITE,bold=True)
put('Panel_Perk_Locked',[1284,742,548,101],id='perk.body');put('Icon_Lock',[1430,766,38,47],id='perk.lock');label('perk.locked','올클리어 시 해금',[1498,777,290,36],25,GRAY)
panel('Button_Deploy',[1252,883,610,145],id='deploy.button');label('deploy.en','DEPLOY',[1298,903,360,26],20,'#AAB4BD');label('deploy.label','출정 →',[1295,932,390,78],56,WHITE,bold=True)

templates={
 'deck_row':{'asset':'Row_Deck','size':[562,110],'origin':[1276,262],'row_stride':110,'gap':0,'dynamic_panel_height':'84 + 25 + row_count * 110 + 35','placements':[{'asset':'Slot_CardIcon','rect':[13,15,80,80]},{'asset':'{card_icon}','rect':[21,23,64,64]},{'asset':'Mark_Upgraded','rect':[519,6,28,28],'visible_if':'upgraded'}],'text_boxes':[{'binding':'name','rect':[128,16,305,35],'font_size':27,'color':INK,'font_weight':700,'alignment':'left','character_spacing_px':0},{'binding':'effect','rect':[128,57,315,31],'font_size':23,'color':GRAY,'alignment':'left','character_spacing_px':0},{'binding':'count','format':'×{count}','rect':[460,36,80,45],'font_size':33,'color':INK,'font_weight':700,'alignment':'right','character_spacing_px':0}],'overflow':'scroll deck body if growth would overlap perk section; header fixed'},
 'quest_row':{'asset':'Chip_Quest','size':[300,40],'origin':[1248,66],'row_stride':42,'max_rows':2,'placements':[{'asset':'Icon_Quest','rect':[15,5,28,28]},{'asset':'Mark_QuestDot','rect':[252,12,14,14]}],'text_boxes':[{'binding':'quest_name','rect':[50,4,192,31],'font_size':18,'color':INK,'font_weight':700,'alignment':'left','character_spacing_px':0}],'overflow':'truncate with ellipsis, no third row'},
 'perk_unlocked':{'asset':'Panel_Perk','rect':[1284,742,548,101],'thumbnail_rect':[1294,748,70,89],'name_rect':[1382,752,428,33],'effect_rect':[1382,791,428,41],'thumbnail_source':'runtime perk card artwork','example_name':'특전 카드','example_effect':'선택한 특전의 효과가 표시됩니다.'}
}
states={'deploy':{'normal':{'asset':'Button_Deploy','interactable':True},'hover':{'asset':'Button_Deploy_Hover','interactable':True},'disabled':{'asset':'Button_Deploy_Disabled','text_color':'#D5DCE2','interactable':False}},'perk':{'locked':{'asset':'Panel_Perk_Locked','label':'올클리어 시 해금','icon':'Icon_Lock'},'unlocked':{'asset':'Panel_Perk','template':'perk_unlocked','hide':['perk.lock','perk.locked']}},'quests':{'empty':{'header':'진행 중 의뢰 없음','row_count':0},'active':{'header':'진행 중 의뢰 {count} / 2','template':'quest_row','max_rows':2}}}
layout={'version':3,'canvas':[1920,1080],'origin':'top-left','rect_format':'x,y,width,height','source_mockup':{'file':'../Loadout_Mockup_v2.png','size':[1672,941],'reference_scale_xy':[1920/1672,1080/941],'note':'Scaled reference; weapon info moved slightly down to fit unchanged right-facing WeaponSVG pistol without occlusion.'},'assets':A,'external_weapons':weapon_sources,'hero_weapon':{'rect':hero_rect,'external':'pistol','fit':'contain_alpha_bounds','preserve_aspect':True,'flip':False,'rotation':0},'weapon_slots':slots,'placements':placements,'text_boxes':texts,'deck_rows':deck_rows,'templates':templates,'states':states,'rendering':{'alpha':'straight','filter':'bilinear','ui_png_scale':2,'background_png_scale':1,'nine_slice_order':'left,bottom,right,top SOURCE PNG pixels','font':'project Korean TMP font; preview uses Windows Malgun Gothic'},'notes':['All UI labels/numbers are separate TMP text.','Weapon rasters are EXTERNAL and intentionally absent from delivery. Source pixels preserved, alpha bbox used only for preview fitting.','Deck and quest rows instantiate templates; max quest count 2.','Perk preview thumbnail is a geometric placeholder, replace with actual perk art.','Geometric background/panels rebuilt as SVG, not pixel-identical image crop.']}
(O/'layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf-8')

def slice9(im,size,b):
    w,h=size;sw,sh=im.size;l,bt,r,t=b;dl,db,dr,dt=[v//2 for v in b];assert w>=dl+dr and h>=dt+db,(size,b)
    out=Image.new('RGBA',size);xs=[0,l,sw-r,sw];ys=[0,t,sh-bt,sh];dx=[0,dl,w-dr,w];dy=[0,dt,h-db,h]
    for j in range(3):
        for i in range(3):
            tw=dx[i+1]-dx[i];th=dy[j+1]-dy[j]
            if tw>0 and th>0:out.alpha_composite(im.crop((xs[i],ys[j],xs[i+1],ys[j+1])).resize((tw,th),Image.Resampling.LANCZOS),(dx[i],dy[j]))
    return out
def drawtext(layer,t):
    d=ImageDraw.Draw(layer);x,y,w,h=t['rect'];font=ImageFont.truetype('C:/Windows/Fonts/'+('malgunbd.ttf' if t.get('font_weight',400)==700 else 'malgun.ttf'),t['font_size']*2);v=t['text'];b=font.getbbox(v);tw=d.textlength(v,font=font)
    assert tw<=w*2+2,('text width',t['id'],tw/2,w);assert b[3]-b[1]<=h*2+2,('text height',t['id'])
    px=x*2+(w*2-tw)/2 if t['alignment']=='center' else x*2+w*2-tw if t['alignment']=='right' else x*2
    d.text((px,y*2+(h*2-(b[3]-b[1]))/2-b[1]),v,font=font,fill=t['color'])
def render(ps,ts):
    im=Image.new('RGBA',(1920,1080))
    for el in ps:
        n=el['asset'];x,y,w,h=el['rect']
        if n=='__weapon__':
            tile=Image.open(O/weapon_sources[el['external']]['source']).convert('RGBA');tile=tile.crop(tile.getbbox());ratio=min(w/tile.width,h/tile.height);tile=tile.resize((round(tile.width*ratio),round(tile.height*ratio)),Image.Resampling.LANCZOS);im.alpha_composite(tile,(x+(w-tile.width)//2,y+(h-tile.height)//2));continue
        a=A[n];tile=Image.open(O/a['file']).convert('RGBA');tile=slice9(tile,(w,h),a['nine_slice']['source_pixels_left_bottom_right_top']) if 'nine_slice' in a else tile.resize((w,h),Image.Resampling.LANCZOS)
        im.alpha_composite(tile,(x,y))
    layer=Image.new('RGBA',(3840,2160))
    for t in ts:drawtext(layer,t)
    im.alpha_composite(layer.resize(im.size,Image.Resampling.LANCZOS));return im

render(placements,texts).convert('RGB').save(P/'Loadout_Mockup_v3.png')
# Complete variant: unlocked perk, two quests, hover button.
ps=copy.deepcopy(placements);ts=copy.deepcopy(texts)
for el in ps:
    if el['id']=='perk.body':el['asset']='Panel_Perk'
    if el['id']=='deploy.button':el['asset']='Button_Deploy_Hover'
ps=[e for e in ps if e['id']!='perk.lock'];ts=[t for t in ts if t['id']!='perk.locked']
ps.append({'asset':'Perk_Thumbnail','rect':[1294,748,70,89],'id':'perk.thumbnail'})
def tx(id,v,r,s=20):return {'id':id,'text':v,'rect':r,'font_size':s,'color':INK,'font_weight':400,'alignment':'left','character_spacing_px':0}
ts+=[tx('perk.name','특전 카드',[1382,752,428,33],25),tx('perk.effect','선택한 특전의 효과가 표시됩니다.',[1382,791,428,41],20)]
ps+=[{'asset':'Chip_Quest','rect':[1248,108,300,40],'id':'quest.row.1'},{'asset':'Icon_Quest','rect':[1263,113,28,28],'id':'quest.icon.1'},{'asset':'Mark_QuestDot','rect':[1500,120,14,14],'id':'quest.dot.1'}]
ts.append(tx('quest.name.1','두 번째 의뢰',[1298,112,192,31],18))
for t in ts:
    if t['id']=='quest.heading':t['text']='진행 중 의뢰 2 / 2'
render(ps,ts).convert('RGB').save(P/'Loadout_Unlocked_Hover_v3.png')
# Empty quests and disabled deployment.
ps=[copy.deepcopy(e) for e in placements if not e['id'].startswith(('quest.row.','quest.icon.','quest.dot.'))]
ts=[copy.deepcopy(t) for t in texts if not t['id'].startswith('quest.name.')]
for el in ps:
    if el['id']=='deploy.button':el['asset']='Button_Deploy_Disabled'
for t in ts:
    if t['id']=='quest.heading':t['text']='진행 중 의뢰 없음'
    if t['id']=='deploy.label':t['color']='#D5DCE2'
render(ps,ts).convert('RGB').save(P/'Loadout_EmptyQuest_Disabled_v3.png')

sheet=Image.new('RGB',(1440,math.ceil((len(A)-1)/4)*190),'#3D4852');d=ImageDraw.Draw(sheet)
for i,n in enumerate(n for n in A if n!='Loadout_BG'):
    x=i%4*360;y=i//4*190;d.rectangle([x+6,y+29,x+354,y+183],fill='#C4CDD5');d.text((x+10,y+5),n,font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',17),fill=WHITE)
    tile=Image.open(O/A[n]['file']).convert('RGBA');tile.thumbnail((320,138),Image.Resampling.LANCZOS);sheet.paste(tile,(x+180-tile.width//2,y+103-tile.height//2),tile)
sheet.save(P/'Loadout_AssetSheet_v3.png')
sheet=Image.new('RGB',(1920,1080),'#CCD4DC')
for i,f in enumerate(['Loadout_Mockup_v3.png','Loadout_Unlocked_Hover_v3.png','Loadout_EmptyQuest_Disabled_v3.png']):
    tile=Image.open(P/f).resize((960,540),Image.Resampling.LANCZOS);sheet.paste(tile,((i%2)*960,(i//2)*540))
d=ImageDraw.Draw(sheet);font=ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',26)
for i,v in enumerate(['좌상: 보통 · 특전 잠김 · 의뢰 1개','우상: 호버 · 특전 해금 · 의뢰 2개','좌하: 출정 불가 · 의뢰 없음','무기 PNG는 외부 참조, 패키지 제외']):d.text((1000,650+i*60),v,font=font,fill=INK)
sheet.save(P/'Loadout_States_v3.png')

required='Loadout_BG Panel_Title Button_Back Tag_Selected Panel_WeaponInfo Rule_Vertical Rule_Horizontal Button_ToArmory WeaponSlot WeaponSlot_Selected WeaponSlot_Locked WeaponSlot_Empty Icon_Lock Panel_Section Row_Deck Icon_Card_Attack Icon_Card_Defense Icon_Card_Reload Panel_Perk_Locked Panel_Perk Button_Deploy Button_Deploy_Hover Button_Deploy_Disabled Chip_Quest Mark_QuestDot Shadow_Panel Shadow_Slot'.split()
assert all(n in A for n in required)
for n,a in A.items():
    im=Image.open(O/a['file']);assert im.mode=='RGBA' and list(im.size)==a['source_pixels'],n
    if a.get('svg'):assert '<text' not in (O/a['svg']).read_text(),n
    if a.get('sha256'):assert hashlib.sha256((O/a['file']).read_bytes()).hexdigest()==a['sha256'],n
    if 'nine_slice' in a:
        l,b,r,t=a['nine_slice']['source_pixels_left_bottom_right_top'];assert l+r<im.width and b+t<im.height,n
for a in weapon_sources.values():assert hashlib.sha256((O/a['source']).read_bytes()).hexdigest()==a['sha256']
assert not list(O.rglob('Weapon_*'))
for t in texts:
    x,y,w,h=t['rect'];assert x>=0 and y>=0 and x+w<=1920 and y+h<=1080,t['id']
report={'png_assets':len(A),'svg_assets':len(list(S.glob('*.svg'))),'required_assets_present':len(required),'weapons_excluded':True,'weapon_sources_unchanged':True,'source_artifact_and_upgrade_mark_hash_match':True,'ui_scale':2,'background_size':[1920,1080],'text_not_baked':True,'text_bounds_verified':True,'nine_slice_verified':True,'deploy_states':3,'perk_states':2,'quest_examples':[0,1,2],'deck_row_template':True,'quest_row_template':True}
(O/'validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
(O/'README.txt').write_text('Loadout v3 UI kit\n\nAll UI PNGs are 2x, except Loadout_BG (1920x1080). SVG originals included for authored geometry. Icon_Artifact is copied byte-for-byte from supplied raster; Mark_Upgraded PNG/SVG copied from ArmoryHub. Text is separate in layout.json for TMP.\n\nWeapon PNGs are NOT included. External paths in layout.json resolve from this Extracted_Draft folder to ../../WeaponSVG. Preview fits the original PNG alpha bounds, preserves aspect and direction, does not tint or flip. Do not import __weapon__ as a sprite name.\n\n9-slice borders use source PNG pixels, left/bottom/right/top. Panel_Section top 90 logical pixels preserve its fixed 84-pixel header. Use independent shadow sprites behind panels. Repeat deck_row and quest_row templates; maximum two quests. Empty quest state suppresses all rows. Unlocked perk preview is a placeholder graphic/text, bind actual perk data.\n\nGeometric background and panel kit are SVG reconstructions rather than pixel-exact crops. The v3 mockup is assembled from the kit and original weapon PNGs. No new image generation or Unity asset editing performed. Run python build_v3.py; requires Pillow, Node, @resvg/resvg-js (VERTEX_RESVG_MODULE override), Windows Malgun Gothic preview font and project external weapon PNGs.\n',encoding='utf-8')
with zipfile.ZipFile(P/'Loadout_v3_Package.zip','w',zipfile.ZIP_DEFLATED) as z:
    files=[Path(__file__),renderer,P/'SPLIT_REQUEST_v2.md',P/'Loadout_Mockup_v3.png',P/'Loadout_Unlocked_Hover_v3.png',P/'Loadout_EmptyQuest_Disabled_v3.png',P/'Loadout_States_v3.png',P/'Loadout_AssetSheet_v3.png',*sorted(O.rglob('*'))]
    for f in files:
        if f.is_file():z.write(f,f.relative_to(P))
with zipfile.ZipFile(P/'Loadout_v3_Package.zip') as z:assert z.testzip() is None;assert not any(Path(n).name.startswith('Weapon_') for n in z.namelist())
print(json.dumps(report))
