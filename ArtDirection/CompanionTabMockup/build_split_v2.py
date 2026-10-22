"""Reproducible SVG sprite delivery and assembly proof; no Unity assets modified."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageFilter
import json, subprocess, shutil, hashlib, zipfile, math

P=Path(__file__).resolve().parent
ROOT=P.parents[1]
O=P/'Extracted_Draft'; S=O/'SVG'; S.mkdir(parents=True,exist_ok=True)
INK='#16181B'; PAPER='#EEF0F2'; CYAN='#0DB8F2'; GRAY='#9299A2'
assets={}; screens={k:{'placements':[], 'text_boxes':[]} for k in ['list','detail']}

def export(name,w,h,body,border=None,scale=2,**extra):
    svg=f'<svg xmlns="http://www.w3.org/2000/svg" width="{w*scale}" height="{h*scale}" viewBox="0 0 {w} {h}">{body}</svg>'
    (S/(name+'.svg')).write_text(svg,encoding='utf-8')
    a={'file':name+'.png','svg':'SVG/'+name+'.svg','logical_size':[w,h],'source_pixels':[w*scale,h*scale],'scale':scale,'pivot':[.5,.5],'text_baked':False,'method':'SVG reconstruction from approved v2 visual reference',**extra}
    if border:
        a['nine_slice']={'source_pixels_left_bottom_right_top':[v*scale for v in border], 'logical_pixels_left_bottom_right_top':border,'minimum_logical_size':[border[0]+border[2]+1,border[1]+border[3]+1]}
    assets[name]=a

def shape(w,h,fill=PAPER,stroke='#B6BDC4',cut=12):
    return f'<path d="M{cut} 1H{w-cut}L{w-1} {cut}V{h-cut}L{w-cut} {h-1}H{cut}L1 {h-cut}V{cut}Z" fill="{fill}" stroke="{stroke}"/>'

def cross(x,y,r=12,color='#7A838D'):
    return f'<path d="M{x-r} {y}H{x+r}M{x} {y-r}V{y+r}" stroke="{color}" fill="none"/>'

def compass(x,y,r=9,color='#A5ABB2'):
    return f'<path d="M{x} {y-r}L{x+1.6} {y-1.6}L{x+r} {y}L{x+1.6} {y+1.6}L{x} {y+r}L{x-1.6} {y+1.6}L{x-r} {y}L{x-1.6} {y-1.6}Z" fill="{color}"/>'

# Backgrounds contain geometry only, never character silhouettes, plates or type.
defs='<defs><linearGradient id="paper" x1="0" y1="0" x2="1" y2="1"><stop stop-color="#F7F9FA"/><stop offset="1" stop-color="#DDE2E7"/></linearGradient><linearGradient id="slab"><stop stop-color="#CBD0D6"/><stop offset="1" stop-color="#ADB4BC"/></linearGradient><filter id="cast" x="-30%" y="-30%" width="180%" height="180%"><feDropShadow dx="18" dy="22" stdDeviation="16" flood-color="#1C232A" flood-opacity=".23"/></filter></defs>'
list_bg=defs+'<path fill="url(#paper)" d="M0 0H1920V1080H0Z"/>'
list_bg+='<g filter="url(#cast)"><path d="M590 -50H1075V185L590 105Z" fill="#D4D9DF"/><path d="M650 -20H860V125L650 35Z" fill="#E8EBEF"/><path d="M0 190L164 298V1080H0Z" fill="#42484E"/><path d="M-45 12H34V230L-45 180Z" fill="#343A40"/><path d="M-60 322L84 397V1110H-60Z" fill="#292F34"/><path d="M150 950L440 1080H270L150 1025Z" fill="#BCC3CB"/></g>'
list_bg+='<path d="M0 162L104 215V514L0 452Z" fill="#555C63" stroke="#E8ECEF"/><path d="M0 660L170 756V1080H132V995L0 920Z" fill="#4F565D"/><path d="M1590 815H1920V898H1590Z" fill="#D9DEE3" opacity=".6"/>'
list_bg+=''.join(cross(x,y) for x,y in [(50,267),(59,902),(1610,58),(62,115)])
export('List_BG',1920,1080,list_bg,scale=1,opaque=True)
detail_bg=defs+'<path fill="url(#paper)" d="M0 0H1920V1080H0Z"/>'
detail_bg+='<g filter="url(#cast)"><path d="M540 -20H948V950H540Z" fill="#30363B"/><path d="M430 70L870 104V947L430 917Z" fill="url(#slab)" stroke="#F8FAFC"/><path d="M945 112L1091 132V970L945 951Z" fill="#C7CDD3"/><path d="M855 449L1117 495V946H855Z" fill="#3B4146"/><path d="M360 296L464 393V958H360Z" fill="#BAC1C9"/><path d="M718 858H1052L1086 955H718Z" fill="#AEB6BF"/></g>'
detail_bg+='<path d="M0 180L438 606V958H0Z" fill="#C3CAD2"/><path d="M0 410L415 797V958H0Z" fill="#B4BDC7" opacity=".45"/><path d="M0 969L641 943L1270 1080H0Z" fill="#F0F3F6"/><path d="M32 1050L556 960L895 974L405 1080H0Z" fill="#DDE3E9" opacity=".5"/>'
detail_bg+=''.join(cross(x,y,17) for x,y in [(1077,87),(1583,50),(46,358),(1884,28)])
export('Detail_BG',1920,1080,detail_bg,scale=1,opaque=True)

# Frames have independent shadow sprites, so their 9-slice borders never distort the shadows.
for name,fill,edge in [('Card_Frame','#E4E8ED','#AEB7C0'),('Card_Frame_Hover','#E4E8ED',CYAN),('Card_Frame_Locked','#D3D8DE','#AEB7C0'),('Card_Frame_Empty','#E7EBEF','#C0C8D0')]:
    body=shape(258,386,fill,edge,14)
    body+='<path d="M18 5H240L253 18V368L240 381H18L5 368V18Z" fill="none" stroke="#FFFFFF" stroke-opacity=".8"/>'
    if name.endswith('Hover'):body+='<path d="M14 2H244L256 14V372L244 384H14L2 372V14Z" fill="none" stroke="#0DB8F2" stroke-width="3"/>'
    if name.endswith('Empty'):
        body+='<path d="M31 20H225L240 35V350L225 366H31L18 350V35Z" fill="none" stroke="#C9D0D7"/>'+compass(224,351)
    export(name,258,386,body,border=[45,45,45,45] if name.endswith('Empty') else [18,18,18,18])
export('Empty_Plus',48,48,cross(24,24,23),method='Separate centered overlay; prevents symbol stretching during 9-slice.')
export('Card_NameBand',250,98,'<path d="M0 0H250V86L238 98H12L0 86Z" fill="#16181B"/>'+compass(222,69),border=[16,20,40,6])
for n,c in [('Pip_Empty','#AAB4BE'),('Pip_Filled',CYAN)]:
    export(n,18,18,f'<path d="M9 1L17 9L9 17L1 9Z" fill="{c if n.endswith("Filled") else "none"}" stroke="{c}"/>'+('<path d="M9 5L13 9L9 13L5 9Z" fill="white"/>' if n.endswith('Filled') else ''))
export('SortTab_Underline',150,4,f'<path d="M0 0H150V4H0Z" fill="{CYAN}"/>',border=[1,1,1,1])
export('Panel_Title',425,136,'<path d="M1 1H424L306 135H1Z" fill="#F5F7F9" stroke="#D3D9DF"/><path d="M101 31V100" stroke="#16181B" stroke-width="1.5"/>',border=[110,8,124,8])
export('Button_Lobby',230,64,shape(230,64,'#F1F4F6','#737D87',13),border=[16,16,16,16])
section=shape(694,170,'#F0F2F4','#CED3D8',1)+'<path d="M1 1H419L435 16H693V48H1Z" fill="#16181B"/><path d="M13 14H18V38H13Z" fill="#0DB8F2"/>'
export('Panel_Section',694,170,section,border=[24,10,276,54],fixed_header_height=48)
for n,c in [('Bond_Seg_Empty','#BEC5CB'),('Bond_Seg_Filled',CYAN)]:export(n,57,26,f'<path d="M10 0H57L47 26H0Z" fill="{c}"/>')
export('Skill_Slot',116,112,shape(116,112,INK,'#8D979F',1)+'<path d="M5 5H111V107H5Z" fill="none" stroke="#F1F4F6"/>',border=[7,7,7,7])
export('Skill_Icon',90,90,'<path d="M8 83L43 17L35 51L78 5L57 51L87 34L43 73L51 52Z" fill="#FFFFFF"/>')
export('Tag_Small',198,44,shape(198,44,'#DADEE3','#B1B9C1',12),border=[15,15,15,15])
export('CardThumb_Frame',142,174,shape(142,174,'#F5F7F9','#A4ADB5',1)+cross(123,156,7),border=[5,27,23,5])
lock='<path d="M8 13V8a8 8 0 0116 0v5" fill="none" stroke="currentColor" stroke-width="3"/><rect x="4" y="12" width="24" height="22" rx="2" fill="currentColor"/><circle cx="16" cy="21" r="2.3" fill="#EEF0F2"/><path d="M16 22V27" stroke="#EEF0F2" stroke-width="2"/>'
export('CardThumb_Locked',142,174,shape(142,174,'#CFD5DB','#A4ADB5',1)+cross(123,156,7),border=[5,27,23,5],symbol_overlay='Unlock_Lock centered separately')
export('Unlock_Mark',24,24,'<path d="M12 1L23 12L12 23L1 12Z" fill="none" stroke="#0DB8F2"/><path d="M12 5L19 12L12 19L5 12Z" fill="#0DB8F2"/>')
export('Unlock_Lock',32,36,f'<g color="#8B949E">{lock}</g>')
export('Icon_Lock_White',32,36,f'<g color="#F4F7F9">{lock}</g>')
export('Panel_NameBlock',598,250,'<path d="M1 21L40 1L597 50V249H1Z" fill="#F0F3F6" stroke="#CDD4DB"/><path d="M98 65V230M119 230H425" stroke="#A1ABB5"/><path d="M119 230H203" stroke="#0DB8F2" stroke-width="4"/>'+cross(40,80,17)+cross(555,210,17),border=[108,30,58,57])
for n,pts in [('Arrow_Prev','29 2 5 30 29 58'),('Arrow_Next','5 2 29 30 5 58')]:export(n,34,60,f'<polyline points="{pts}" fill="none" stroke="#16181B" stroke-width="4"/>')
export('Button_Record',248,66,shape(248,66,'#F0F3F6','#69747E',14),border=[17,17,17,17])
export('Button_Camp',328,66,shape(328,66,INK,INK,14),border=[17,17,17,17])
export('Rule_Horizontal',694,1,'<path d="M0 .5H694" stroke="#CCD2D8"/>',border=None)
export('Icon_Record',30,32,'<path d="M3 2H27V30H3Z M8 9H22 M8 15H22 M8 21H17" fill="none" stroke="#16181B" stroke-width="2"/>')
# Soft shadows are expandable sprites with transparent padding and independent rects.
for n,w,h in [('Shadow_Card',318,446),('Shadow_Panel',754,230),('Shadow_Title',485,196),('Shadow_Name',658,310)]:
    export(n,w,h,f'<defs><filter id="s" x="-50%" y="-50%" width="200%" height="200%"><feGaussianBlur stdDeviation="10"/></filter></defs><rect x="26" y="28" width="{w-60}" height="{h-60}" fill="#131D27" opacity=".26" filter="url(#s)"/>',border=[46,46,46,46],padding=30)
export('Shadow_Character',630,90,'<defs><radialGradient id="s"><stop stop-color="#131B22" stop-opacity=".3"/><stop offset="1" stop-color="#131B22" stop-opacity="0"/></radialGradient></defs><ellipse cx="315" cy="45" rx="315" ry="45" fill="url(#s)"/>')

renderer=P/'render_split_svg.cjs'
renderer.write_text("const fs=require('fs'),p=require('path');const {Resvg}=require(process.env.VERTEX_RESVG_MODULE||'C:/Users/SKYLIGHT/.codex/tmp/currency-f-render/node_modules/@resvg/resvg-js');for(const f of fs.readdirSync(process.argv[2]))if(f.endsWith('.svg'))fs.writeFileSync(p.join(process.argv[3],f.replace('.svg','.png')),new Resvg(fs.readFileSync(p.join(process.argv[2],f))).render().asPng());",encoding='utf-8')
subprocess.run(['node',str(renderer),str(S),str(O)],check=True)

chara=ROOT/'Assets/Art/Characters/Joining Characters/CP01 복사.png'
icarus=ROOT/'Assets/Art/Characters/Joining Characters/Events/CP_Event_01_Icarus_Revised.png'
shutil.copyfile(chara,O/'Chara_ChiharaSho_Full.png')
assets['Chara_ChiharaSho_Full']={'file':'Chara_ChiharaSho_Full.png','source_pixels':[832,1216],'logical_size':[832,1216],'scale':1,'pivot':[.50,.02],'text_baked':False,'method':'byte-identical original project character, not generated mockup cutout','source':str(chara.relative_to(ROOT))}

def put(screen,asset,rect,**kw):
    screens[screen]['placements'].append({'asset':asset,'rect':rect,**kw})
def label(screen,id,text,rect,size=20,color=INK,align='left',bold=False,spacing=0,**kw):
    screens[screen]['text_boxes'].append({'id':id,'text':text,'rect':rect,'font_size':size,'color':color,'alignment':align,'vertical_alignment':'middle','character_spacing_px':spacing,'font_family':'Malgun Gothic','font_weight':700 if bold else 400,'runtime':'TMP','wrap':False,**kw})
def shadow(screen,rect,name='Shadow_Panel'):
    x,y,w,h=rect;put(screen,name,[x-18,y-12,w+60,h+60])
def common(screen):
    put(screen,'Panel_Title',[0,0,425,136]);label(screen,'header.number','04',[36,34,48,44],27,bold=True)
    label(screen,'header.title','동료',[133,20,180,65],55,bold=True)
    label(screen,'header.caption','COMPANIONS',[136,92,180,22],15,GRAY)
    shadow(screen,[1630,24,230,64]);put(screen,'Button_Lobby',[1630,24,230,64])
    label(screen,'header.back','←  로비',[1640,28,210,52],25,align='center',bold=True)

put('list','List_BG',[0,0,1920,1080]);shadow('list',[0,0,425,136],'Shadow_Title');common('list')
label('list','section.title','동료 목록',[496,107,130,42],26,bold=True)
label('list','section.english','COMPANION ROSTER',[638,111,275,30],17,GRAY)
put('list','SortTab_Underline',[467,115,6,28])
for i,text in enumerate(['합류순','호감도','막']):label('list','sort.'+str(i),text,[1308+i*178,112,156,35],24,bold=i==0,align='center')
put('list','SortTab_Underline',[1308,151,156,4])
cards=[]
for i in range(12):
    x=185+(i%6)*281;y=186+(i//6)*410;rect=[x,y,258,386]
    state='hover' if i==0 else 'normal' if i==1 else 'locked' if i<9 else 'empty'
    frame={'hover':'Card_Frame_Hover','normal':'Card_Frame','locked':'Card_Frame_Locked','empty':'Card_Frame_Empty'}[state]
    shadow('list',rect,'Shadow_Card');put('list',frame,rect)
    card={'index':i+1,'state':state,'rect':rect,'portrait_rect':[x+5,y+6,248,281],'name_band_rect':[x+4,y+284,250,98],'mask':'clipped card corners; portrait clipped before overlaying name band'}
    cards.append(card)
    if i<9:
        # External portrait layer is composed here before the name band.
        put('list','__portrait__',card['portrait_rect'],preview_only=True,source='sho' if i==0 else 'icarus' if i==1 else 'silhouette',slot=i)
        put('list','Card_NameBand',card['name_band_rect'])
    label('list',f'card.{i}.number',f'{i+1:02}',[x+15,y+13,38,25],17,GRAY)
    if i==0:
        label('list','card.0.name','치하라 쇼',[x+16,y+293,178,34],26,PAPER,bold=True)
        label('list','card.0.level','Lv.3',[x+195,y+295,50,32],18,PAPER)
        label('list','card.0.faction','VERTEX / 협력자',[x+16,y+332,210,24],16,'#B7BFC7')
        for j in range(5):put('list','Pip_Filled' if j<3 else 'Pip_Empty',[x+17+j*24,y+359,18,18])
    elif i==1:
        label('list','card.1.name','이카루스 군',[x+16,y+293,225,34],26,PAPER,bold=True)
        label('list','card.1.faction','이벤트 동료',[x+16,y+335,220,24],18,'#B7BFC7')
    elif i<9:
        put('list','Icon_Lock_White',[x+113,y+176,32,36]);label('list',f'card.{i}.name','???',[x+16,y+293,210,35],27,PAPER,bold=True)
        label('list',f'card.{i}.condition',['2막 구출 의뢰','3막 진입','특수 의뢰 완료'][(i-2)%3],[x+16,y+335,220,26],18,PAPER)
    else:put('list','Empty_Plus',[x+105,y+166,48,48])
screens['list']['cards']=cards
screens['list']['overflow']={'mode':'vertical_scroll','viewport_rect':[175,178,1690,809],'columns':6,'pagination':False,'spacing':[23,24]}
label('list','footer.hint','ⓘ  동료를 선택해 전력 정보를 확인하세요.',[350,997,1000,43],22,bold=True)

put('detail','Detail_BG',[0,0,1920,1080]);shadow('detail',[0,0,425,136],'Shadow_Title');common('detail')
put('detail','Shadow_Character',[410,982,660,70])
character_rect=[350,58,650,950]
put('detail','Chara_ChiharaSho_Full',character_rect)
screens['detail']['character']={'asset':'Chara_ChiharaSho_Full','rect':character_rect,'preserve_aspect':True,'pivot_normalized_bottom_left':[.50,.02],'foot_anchor_canvas_top_left':[675,989],'source_foot_anchor_pixels_top_left':[416,1192],'runtime_source':'CoopCharData.sanctuaryFullArt','note':'Pivot at front shoe sole near source x416/y1192; preserve source aspect ratio.'}
label('detail','quote','「쓴 만큼 채워.」',[98,214,420,48],32)
put('detail','Arrow_Prev',[116,279,10,18]);label('detail','quote.index','LINE 01 / 03',[140,275,170,25],16,GRAY);put('detail','Arrow_Next',[318,279,10,18])
screens['detail']['dialogue']={'mode':'text_line_cycle','audio':False,'previous_rect':[110,271,26,34],'next_rect':[310,271,26,34],'label_id':'quote.index','sample_count':3}
for name,y,h,title,eng in [('bond',86,147,'호감도','FAVOR'),('skill',253,190,'지시 스킬','ORDER SKILL'),('cards',465,245,'동료 카드','COMPANION CARDS'),('unlocks',727,235,'단계별 해금','UNLOCK LIST')]:
    rect=[1172,y,694,h];shadow('detail',rect);put('detail','Panel_Section',rect,id='section.'+name)
    label('detail',name+'.heading',title,[1216,y+6,210,35],25,PAPER,bold=True)
    label('detail',name+'.caption',eng,[1430,y+12,250,27],15,'#A5ADB5')
label('detail','bond.lv','Lv.',[1205,153,54,43],28,bold=True);label('detail','bond.value','3',[1263,132,62,80],66,bold=True);label('detail','bond.max','/ 5',[1327,161,64,35],27)
for i in range(5):put('detail','Bond_Seg_Filled' if i<3 else 'Bond_Seg_Empty',[1404+i*60,165,57,26])
label('detail','bond.remaining','다음 단계까지 2 PT',[1540,200,305,25],18,align='right')
put('detail','Skill_Slot',[1206,318,116,112]);put('detail','Skill_Icon',[1219,328,90,90])
label('detail','skill.name','흡혈',[1347,310,210,43],32,bold=True)
label('detail','skill.description','적 하나에게 흡혈 6',[1347,359,275,32],23)
label('detail','skill.cooldown','3전투마다 사용 가능',[1347,395,310,28],21)
put('detail','Tag_Small',[1640,347,198,44]);label('detail','skill.upgrade','최대 호감도 시 강화',[1646,350,186,36],18,align='center',bold=True)
for i in range(4):
    x=1206+158*i;put('detail','CardThumb_Frame' if i<2 else 'CardThumb_Locked',[x,527,142,174])
    if i<2:put('detail','Skill_Icon',[x+18,541,100,110],tint=INK,preview_art_placeholder=True)
    else:put('detail','Unlock_Lock',[x+55,595,32,36])
    label('detail',f'thumb.{i}.index',f'{i+1:02}',[x+104,534,30,22],16,GRAY)
    label('detail',f'thumb.{i}.name',['재장전','흡혈','???','???'][i],[x+12,664,115,30],19,INK if i<2 else GRAY,bold=i<2)
unlock_rows=[]
for i in range(5):
    y=787+i*32;opened=i<3;record=['I','II','III','IV','V'][i];extra={0:'합류 카드',2:'동료 카드',4:'지시 스킬 강화'}.get(i,'')
    value='동료 기록 '+record+(' · '+extra if extra else '')
    put('detail','Unlock_Mark' if opened else 'Unlock_Lock',[1212,y+2,20,23]);label('detail',f'unlock.{i}.level',f'Lv.{i+1}',[1255,y,70,28],20,INK if opened else GRAY)
    label('detail',f'unlock.{i}.content',value,[1338,y,500,28],21,INK if opened else GRAY,bold=opened)
    if i<4:put('detail','Rule_Horizontal',[1255,y+30,580,1])
    unlock_rows.append({'level':i+1,'record':i+1,'additional_unlock_sample':extra,'rect':[1204,y,640,31],'unlocked_sample':opened})
screens['detail']['unlock_rows']=unlock_rows
shadow('detail',[0,700,598,250],'Shadow_Name');put('detail','Panel_NameBlock',[0,700,598,250])
label('detail','identity.faction','VERTEX / 협력자',[119,763,375,34],21)
label('detail','identity.name','치하라 쇼',[116,809,440,86],66,bold=True)
label('detail','identity.english','CHIHARA SHO',[119,900,390,26],18,GRAY,spacing=7)
put('detail','Arrow_Prev',[29,474,34,60]);put('detail','Arrow_Next',[1867,474,34,60])
for asset,rect in [('Button_Record',[1267,980,248,66]),('Button_Camp',[1538,980,328,66])]:shadow('detail',rect);put('detail',asset,rect)
put('detail','Icon_Record',[1314,997,27,31]);label('detail','button.record','기록 보기',[1351,985,144,55],26,bold=True)
label('detail','button.camp','베이스캠프에서 만나기  →',[1551,986,302,53],22,PAPER,align='center',bold=True)

layout={'version':2,'canvas':[1920,1080],'origin':'top-left','rect_format':'x,y,width,height','unity_rect_transform_note':'Use top-left anchors, invert canvas y for anchoredPosition; explicit character pivot uses Unity bottom-left convention.','palette':{'ink':INK,'paper':PAPER,'accent':CYAN},'assets':assets,'screens':screens,'rendering':{'sprite_scale':2,'background_scale':1,'color_space':'sRGB','alpha':'straight','filter':'bilinear','mipmaps':False,'shadow_order':'behind corresponding plate; see ordered placements','nine_slice_border_order':'left,bottom,right,top','tmp_font_recommendation':'project Korean TMP font, Malgun Gothic used only in assembly proof'},'notes':['SVG reconstruction, not pixel-identical raster extraction.','Texts exist only in layout.json and assembly previews, never baked into delivered sprites.','Backgrounds fixed at 1920x1080; UI sprite PNGs 2x SVG logical dimensions. Original character copied at native dimensions.','No pagination; list uses vertical scroll on overflow. No voice controls; text line navigation only.','Every affection level grants the corresponding record. Additional unlocks shown are layout samples, bind actual game data.','__portrait__ entries are external runtime layers and preview-only source crops, not delivered frame sprites.','Empty_Plus and Unlock_Lock are centered overlay sprites, separate from their 9-sliced frames to prevent symbol distortion.','Preview card art uses a tinted Skill_Icon placeholder; replace with real card artwork at runtime.']}
(O/'layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf-8')

def nine(im,size,border):
    w,h=size;sw,sh=im.size;l,b,r,t=border
    # Border inputs in source pixels; target borders are logical pixels (2x source assets).
    dl,db,dr,dt=[round(v/2) for v in border]
    out=Image.new('RGBA',size)
    xs=[0,l,sw-r,sw];ys=[0,t,sh-b,sh];dx=[0,dl,w-dr,w];dy=[0,dt,h-db,h]
    for j in range(3):
        for i in range(3):
            tw=dx[i+1]-dx[i];th=dy[j+1]-dy[j]
            if tw>0 and th>0:out.alpha_composite(im.crop((xs[i],ys[j],xs[i+1],ys[j+1])).resize((tw,th),Image.Resampling.LANCZOS),(dx[i],dy[j]))
    return out

def portrait(source,size,slot):
    im=Image.open(icarus if source=='icarus' else chara).convert('RGBA')
    # Explicit source face/upper-body crop; original source pixels only.
    crop=(170,0,740,650) if source=='icarus' else (250,25,670,500)
    im=im.crop(crop).resize(size,Image.Resampling.LANCZOS)
    if source=='silhouette':
        a=im.getchannel('A');im=Image.new('RGBA',size,'#30363C');im.putalpha(a)
        if slot%2:im=im.transpose(Image.Transpose.FLIP_LEFT_RIGHT)
    return im

def draw_text(layer,t):
    d=ImageDraw.Draw(layer);x,y,w,h=t['rect'];s=t['font_size']*2
    font=ImageFont.truetype('C:/Windows/Fonts/'+('malgunbd.ttf' if t['font_weight']==700 else 'malgun.ttf'),s)
    value=t['text'];gap=t['character_spacing_px']*2
    width=sum(d.textlength(c,font=font) for c in value)+gap*max(0,len(value)-1)
    if gap==0:width=d.textlength(value,font=font)
    if width>w*2+1:raise ValueError(f'Text overflow: {t["id"]}: {width/2} > {w}')
    b=font.getbbox(value);px=x*2+(w*2-width)/2 if t['alignment']=='center' else x*2+w*2-width if t['alignment']=='right' else x*2
    py=y*2+(h*2-(b[3]-b[1]))/2-b[1]
    if not gap:d.text((px,py),value,font=font,fill=t['color'])
    else:
        for c in value:d.text((px,py),c,font=font,fill=t['color']);px+=d.textlength(c,font=font)+gap

for screen,data in screens.items():
    im=Image.new('RGBA',(1920,1080))
    for el in data['placements']:
        name=el['asset'];x,y,w,h=el['rect']
        if name=='__portrait__':tile=portrait(el['source'],(w,h),el['slot'])
        else:
            a=assets[name];tile=Image.open(O/a['file']).convert('RGBA')
            tile=nine(tile,(w,h),a['nine_slice']['source_pixels_left_bottom_right_top']) if 'nine_slice' in a else tile.resize((w,h),Image.Resampling.LANCZOS)
        if 'tint' in el:
            alpha=tile.getchannel('A');tile=Image.new('RGBA',tile.size,el['tint']);tile.putalpha(alpha)
        im.alpha_composite(tile,(x,y))
    type_layer=Image.new('RGBA',(3840,2160))
    for t in data['text_boxes']:draw_text(type_layer,t)
    im.alpha_composite(type_layer.resize(im.size,Image.Resampling.LANCZOS))
    im.convert('RGB').save(P/f'Companion_{screen.title()}_Assembly_v2.png')

# Inspectable contact sheet of every panel/icon/shadow on a checkerboard.
names=[n for n in assets if not n.endswith('_BG') and n!='Chara_ChiharaSho_Full']
rows=math.ceil(len(names)/4);sheet=Image.new('RGB',(1440,rows*215),'#C9CFD5');d=ImageDraw.Draw(sheet)
font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',16)
for idx,n in enumerate(names):
    x=(idx%4)*360;y=(idx//4)*215
    for yy in range(y+27,y+202,14):
        for xx in range(x+9,x+350,14):d.rectangle([xx,yy,xx+13,yy+13],fill='#DDE2E6' if ((xx-x)//14+(yy-y)//14)%2 else '#C3CBD2')
    tile=Image.open(O/assets[n]['file']).convert('RGBA');tile.thumbnail((320,155),Image.Resampling.LANCZOS)
    sheet.paste(tile,(x+180-tile.width//2,y+110-tile.height//2),tile);d.text((x+10,y+5),n,font=font,fill=INK)
sheet.save(P/'Companion_AssetSheet_v2.png')

for n,a in assets.items():
    im=Image.open(O/a['file']);assert list(im.size)==a['source_pixels'],n
    assert im.mode=='RGBA',n
    if a.get('opaque'):assert im.getchannel('A').getextrema()==(255,255),n
    if a.get('svg'):assert '<text' not in (O/a['svg']).read_text(),n
    if 'nine_slice' in a:
        l,b,r,t=a['nine_slice']['source_pixels_left_bottom_right_top'];assert l+r<im.width and b+t<im.height,n
assert hashlib.sha256(chara.read_bytes()).digest()==hashlib.sha256((O/'Chara_ChiharaSho_Full.png').read_bytes()).digest()
assert len(cards)==12 and not screens['list']['overflow']['pagination']
assert all(r['record']==r['level'] for r in unlock_rows)
for screen in screens.values():
    for t in screen['text_boxes']:
        x,y,w,h=t['rect'];assert x>=0 and y>=0 and x+w<=1920 and y+h<=1080,t['id']
report={'assets':len(assets),'svg_assets':len(list(S.glob('*.svg'))),'backgrounds':'1920x1080 RGBA opaque','ui_png_scale':2,'character_sha256_matches_original':True,'svg_contains_no_text':True,'text_boxes_fit':True,'nine_slice_borders_valid':True,'records_at_every_level':True,'list_pagination_removed':True,'dialogue_has_no_audio':True,'assembly_previews':['Companion_List_Assembly_v2.png','Companion_Detail_Assembly_v2.png']}
(O/'validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
(O/'README.txt').write_text('VERTEX Companion v2 sprite delivery\n\nSVG reconstruction of approved mockups; not pixel-identical raster extraction. All UI sprite PNGs are 2x; the two backgrounds are exactly 1920x1080. Character is a byte-identical native-resolution project original.\n\nlayout.json uses top-left canvas coordinates. Nine-slice borders use SOURCE PNG pixels ordered left/bottom/right/top. Panel_Section has a fixed 48 logical-pixel header and 54-pixel top slice. Keep shadows on separate Images behind plates. Preserve aspect for frames containing centered symbols.\n\nNo text is baked in sprites. TMP must render layout text. Mockup previews use Malgun Gothic and sample additional unlock data. Every level grants one record. No list pager; overflow scrolls vertically. LINE controls cycle text only.\n\n__portrait__ is an external runtime image layer. Do not load it as a sprite filename; use the companion portrait and clip to the provided portrait rect. Preview locked silhouettes reuse the original Sho portrait as an anonymous placeholder. Original illustration, not mockup-redrawn Sho, is supplied.\n\nRun python build_split_v2.py to regenerate. Requires Pillow, Node, @resvg/resvg-js. Set VERTEX_RESVG_MODULE to override the renderer module path. Preview fonts use Windows Malgun Gothic. No Unity assets or scenes were modified.\n',encoding='utf-8')
with zipfile.ZipFile(P/'Companion_v2_Package.zip','w',zipfile.ZIP_DEFLATED) as z:
    files=[Path(__file__),renderer,P/'SPLIT_REQUEST.md',P/'Companion_List_Assembly_v2.png',P/'Companion_Detail_Assembly_v2.png',P/'Companion_AssetSheet_v2.png',*sorted(O.rglob('*'))]
    for f in files:
        if f.is_file():z.write(f,f.relative_to(P))
print(json.dumps(report))
