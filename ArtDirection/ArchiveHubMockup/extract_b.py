from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import json, subprocess, math, zipfile

P=Path(__file__).resolve().parent;O=P/'Extracted_Draft';S=O/'SVG';S.mkdir(parents=True,exist_ok=True)
INK='#16181B';PAPER='#EEF0F2';RED='#B3263A';HOVER='#D43A50'
assets={}
def export(n,w,h,body,scale=2,border=None):
    (S/(n+'.svg')).write_text(f'<svg xmlns="http://www.w3.org/2000/svg" width="{w*scale}" height="{h*scale}" viewBox="0 0 {w} {h}">{body}</svg>',encoding='utf-8')
    assets[n]={'file':n+'.png','svg':'SVG/'+n+'.svg','source_pixels':[w*scale,h*scale],'logical_size':[w,h],'scale':scale,'pivot':[.5,.5],'text_baked':False}
    if border:assets[n]['nine_slice']={'source_pixels_left_bottom_right_top':[v*scale for v in border],'logical_pixels_left_bottom_right_top':border,'mode':'stretch center; preserve corners','minimum_logical_size':[border[0]+border[2]+1,border[1]+border[3]+1]}
def panel(w,h,fill=INK,opacity=.85,stroke='#77717E'):
    return f'<path d="M.5 22L22 .5H{w-.5}V{h-22}L{w-22} {h-.5}H.5Z" fill="{fill}" fill-opacity="{opacity}" stroke="{stroke}" stroke-width="1"/>'
for n,fill,edge in [('Panel_Menu',INK,'#6B646F'),('Panel_Menu_Hover','#333039',HOVER),('Panel_Menu_Locked','#141318','#45404A')]:
    export(n,430,118,panel(430,118,fill,.85,edge),border=[24,24,24,24])
# Keep long text separator separate so menu panels remain genuinely stretchable.
export('Rule_Menu',317,1,f'<path d="M0 .5H317" stroke="{PAPER}" stroke-opacity=".13"/>')
for n,color,width in [('Deco_Diamond',RED,1),('Deco_Diamond_Hover',HOVER,2)]:
    export(n,90,90,f'<path d="M45 2L88 45L45 88L2 45Z" fill="{INK}" fill-opacity=".95" stroke="{color}" stroke-width="{width}"/><path d="M45 9L81 45L45 81L9 45Z" fill="none" stroke="{color}" stroke-opacity=".4"/>')
icons={
 'Icon_Enemy':'<circle cx="0" cy="0" r="14"/><circle cx="0" cy="0" r="7"/><path d="M-20 0H-10M10 0H20M0 -20V-10M0 10V20"/>',
 'Icon_Companion':'<circle cx="-6" cy="-7" r="6"/><circle cx="9" cy="-4" r="5"/><path d="M-19 16V11Q-19 3-6 3Q7 3 7 11V16ZM8 5Q20 5 20 13V16H11"/>',
 'Icon_Item':'<path d="M-17 -8L0 -16L17 -8V12L0 20L-17 12ZM-17 -8L0 0L17 -8M0 0V20M-8 -12L9 -4V5"/>',
 'Icon_Play':'<circle cx="0" cy="1" r="16"/><path d="M0 -10V1L9 6M-6 -20H6"/>',
 'Icon_Lock':'<path d="M-10 -2V-10a10 10 0 0120 0v8"/><rect x="-15" y="-2" width="30" height="23" rx="2"/><path d="M0 6V14"/>'}
for n,body in icons.items():export(n,48,48,f'<g transform="translate(24 24)" fill="none" stroke="{PAPER}" stroke-width="1.8" stroke-linejoin="round">{body}</g>')
export('Panel_Title',460,146,panel(460,146),border=[24,24,24,24])
export('Rule_Title',1,78,f'<path d="M.5 0V78" stroke="{PAPER}" stroke-opacity=".55"/>')
export('Button_Lobby',213,56,panel(213,56),border=[24,24,24,24])
export('Panel_Dialogue',500,112,panel(500,112),border=[24,24,24,24])
export('Rule_Dialogue',452,61,f'<path d="M7 0V61M0 7H14M36 13H448" stroke="{PAPER}" stroke-opacity=".35"/>')

# Source-inspired pointed upper wings, lower lobes and long tails, common thorax pivot.
# Same tracing and vein topology in all frames; only wing spread changes.
defs='<defs><linearGradient id="wing" x1="0" y1="1" x2="1" y2="0"><stop stop-color="#EF304C"/><stop offset=".45" stop-color="#B3263A"/><stop offset="1" stop-color="#650E24"/></linearGradient></defs>'
wing='<path d="M63 62C53 54 34 24 17 11C15 30 24 55 51 65C33 61 21 65 12 70C27 83 48 82 60 68L41 111L64 73Z" fill="url(#wing)" stroke="#E73550" stroke-width=".9"/><g fill="none" stroke="#FF526A" stroke-opacity=".6" stroke-width=".65"><path d="M62 63L21 18M59 61C42 49 30 37 22 21M56 64L25 37M57 66L24 70M59 67L38 77M61 72L45 103"/></g>'
for i,spread in enumerate([1,.57,.17],1):
    wings=f'<g transform="translate(64 0) scale({spread} 1) translate(-64 0)">{wing}<g transform="translate(128 0) scale(-1 1)">{wing}</g></g>'
    body='<path d="M64 54C60 57 61 64 62 71L64 83L66 71C67 63 68 57 64 54Z" fill="#F23B55"/><path d="M63 56Q60 48 56 46M65 56Q68 48 72 46" fill="none" stroke="#ED3B53" stroke-width="1"/>'
    export(f'Deco_Butterfly_{i:02}',128,128,defs+f'<g transform="rotate(15 64 64)">{wings}{body}</g>',scale=1)
    assets[f'Deco_Butterfly_{i:02}']['pose']=['open','half-folded','nearly-folded'][i-1]
    assets[f'Deco_Butterfly_{i:02}']['method']='Vector interpretation of Source_Chara red pointed-wing butterflies; not an exact source-pixel extraction.'

export('Glow_Red',512,512,'<defs><radialGradient id="g"><stop offset="0" stop-color="white" stop-opacity="1"/><stop offset=".15" stop-color="white" stop-opacity=".82"/><stop offset=".35" stop-color="white" stop-opacity=".45"/><stop offset=".6" stop-color="white" stop-opacity=".13"/><stop offset=".82" stop-color="white" stop-opacity=".02"/><stop offset="1" stop-color="white" stop-opacity="0"/></radialGradient></defs><circle cx="256" cy="256" r="252" fill="url(#g)"/>',scale=1)
assets['Glow_Red']['tint']=RED
assets['Glow_Red']['blend']='straight-alpha RGBA; multiply RGB by tint at runtime; do not multiply alpha twice'
js="const fs=require('fs'),p=require('path');const {Resvg}=require('C:/Users/SKYLIGHT/.codex/tmp/currency-f-render/node_modules/@resvg/resvg-js');for(const f of fs.readdirSync(process.argv[1]))if(f.endsWith('.svg'))fs.writeFileSync(p.join(process.argv[2],f.replace('.svg','.png')),new Resvg(fs.readFileSync(p.join(process.argv[1],f))).render().asPng());"
subprocess.run(['node','-e',js,str(S),str(O)],check=True)
# Straight-alpha white RGB, including fully transparent pixels, avoids dark filter fringes.
glow=Image.open(O/'Glow_Red.png').convert('RGBA');alpha=glow.getchannel('A');glow=Image.new('RGBA',glow.size,'white');glow.putalpha(alpha);glow.save(O/'Glow_Red.png')
original=json.loads((P/'MOCKUP_LAYOUT.json').read_text(encoding='utf-8'))
placements=[];menus=[]
for i,rect in enumerate(original['menus']):
    x,y,w,h=rect;icon=list(icons)[i]
    menus.append({'id':['enemy','companion','item','play'][i],'rect':rect,'diamond_rect':[x-11,y+12,90,90],'icon_rect':[x+10,y+33,48,48],'rule_rect':[x+91,y+89,317,1],'state':'hover' if i==0 else 'normal'})
    placements += [{'asset':'Panel_Menu_Hover' if i==0 else 'Panel_Menu','rect':rect},{'asset':'Deco_Diamond_Hover' if i==0 else 'Deco_Diamond','rect':[x-11,y+12,90,90]},{'asset':icon,'rect':[x+10,y+33,48,48],'tint':HOVER if i==0 else PAPER},{'asset':'Rule_Menu','rect':[x+91,y+89,317,1]}]
placements += [{'asset':'Panel_Title','rect':[0,0,460,146]},{'asset':'Rule_Title','rect':[238,30,1,78]},{'asset':'Button_Lobby','rect':[1641,30,213,56]},{'asset':'Panel_Dialogue','rect':[40,909,500,112]},{'asset':'Rule_Dialogue','rect':[54,938,452,61]}]
butterflies=[{'rect':[1310,168,64,64],'phase_offset_seconds':0},{'rect':[1801,513,48,48],'phase_offset_seconds':.12},{'rect':[1311,850,56,56],'phase_offset_seconds':.24}]
layout={'variant':'B','canvas':[1920,1080],'origin':'top-left','rect_format':'x,y,width,height','accent':RED,'hover_accent':HOVER,'assets':assets,'background':{'file':'../Source_BG.png','source_crop_ltrb':original['bg_crop_ltrb'],'rect':[0,0,1920,1080]},'character':{'file':'../Source_Chara.png','rect':original['character_rect'],'unchanged_source':True},'menus':menus,'placements':placements,'text_boxes':original['variants']['B']['text_boxes'],'states':{'normal':{'panel':'Panel_Menu','diamond':'Deco_Diamond','icon_tint':PAPER},'hover':{'panel':'Panel_Menu_Hover','diamond':'Deco_Diamond_Hover','icon_tint':HOVER},'locked':{'panel':'Panel_Menu_Locked','diamond':'Deco_Diamond','replace_icon':'Icon_Lock','icon_tint':'#77717E','text_tint':'#77717E','interactable':False}},'butterfly_animation':{'frames':['Deco_Butterfly_01','Deco_Butterfly_02','Deco_Butterfly_03'],'loop_sequence':[0,1,2,1],'frame_duration_seconds':.09,'cycle_seconds':.36,'pivot':[.5,.5],'trim':False,'filter':'bilinear','placements_suggested':butterflies},'glow':{'asset':'Glow_Red','suggested_rect':[1190,475,300,300],'tint':RED,'opacity':.16,'order':'after background and character, before menu and butterflies'},'notes':['Original B positions preserved. Hover edge intentionally brighter per section 5.','Panel glyphs are not baked; text is TMP. Icons use white RGB and runtime tint.','All panel and icon PNGs generated from included SVG sources. Butterfly frames are source-inspired vector art.','Background and character are referenced, never modified.','9-slice values refer to source PNG pixels in left,bottom,right,top order; logical sizes are half for scale=2.']}
(O/'layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf-8')

def load(n):return Image.open(O/(n+'.png')).convert('RGBA')
def tinted(im,color):
    out=Image.new('RGBA',im.size,color);out.putalpha(im.getchannel('A'));return out
base=Image.open(P/'Source_BG.png').convert('RGBA').crop(tuple(original['bg_crop_ltrb']))
base.alpha_composite(Image.open(P/'Source_Chara.png').convert('RGBA').resize((1305,870),Image.Resampling.LANCZOS),(70,155))
g=tinted(glow,RED).resize((300,300),Image.Resampling.LANCZOS);g.putalpha(g.getchannel('A').point(lambda a:round(a*.16)));base.alpha_composite(g,(1190,475))
for el in placements:
    im=load(el['asset']);x,y,w,h=el['rect']
    if 'tint' in el:im=tinted(im,el['tint'])
    base.alpha_composite(im.resize((w,h),Image.Resampling.LANCZOS),(x,y))
for i,b in enumerate(butterflies):
    x,y,w,h=b['rect'];base.alpha_composite(load(f'Deco_Butterfly_{i+1:02}').resize((w,h),Image.Resampling.LANCZOS),(x,y))
# Reuse original mockup typography at 2x for sharp reconstruction.
type_layer=Image.new('RGBA',(3840,2160));d=ImageDraw.Draw(type_layer)
for t in layout['text_boxes']:
    value=t['text'];size=t['font_size'];x,y,w,h=t['rect']
    latin=value in ['03','01','02','04','ARCHIVES'] or 'RECORDS' in value or value=='ITEM CODEX'
    bold=size in [54,28,43,25,18] and not latin
    font=ImageFont.truetype('C:/Windows/Fonts/'+('bahnschrift.ttf' if latin else 'malgunbd.ttf' if bold else 'malgun.ttf'),size*2)
    bb=font.getbbox(value);d.text((x*2-bb[0],y*2+(h*2-(bb[3]-bb[1]))//2-bb[1]),value,font=font,fill=t['color'])
base.alpha_composite(type_layer.resize((1920,1080),Image.Resampling.LANCZOS));base.convert('RGB').save(P/'Archive_B_Assembly.png')

sheet=Image.new('RGBA',(1200,720),'#28232D');d=ImageDraw.Draw(sheet)
for idx,n in enumerate(['Panel_Menu','Panel_Menu_Hover','Panel_Menu_Locked']):
    sheet.alpha_composite(load(n).resize((430,118)),(28,35+idx*155));d.text((28,17+idx*155),n,fill=PAPER)
for i in range(3):
    sheet.alpha_composite(load(f'Deco_Butterfly_{i+1:02}'),(540+i*200,70));d.text((540+i*200,215),['OPEN','HALF','CLOSED'][i],fill=PAPER)
sheet.alpha_composite(tinted(glow,RED).resize((260,260)),(590,310))
sheet.alpha_composite(glow.resize((260,260)),(880,310))
for i,n in enumerate(icons):sheet.alpha_composite(load(n).resize((48,48)),(40+i*90,570))
sheet.convert('RGB').save(P/'Archive_B_AssetSheet.png')
# Animated proof of common-center wing motion; preview only, source PNGs retain alpha.
frames=[]
for idx in [1,2,3,2]:
    f=Image.new('RGBA',(256,256),'#28232D');f.alpha_composite(load(f'Deco_Butterfly_{idx:02}').resize((256,256),Image.Resampling.LANCZOS));frames.append(f.convert('RGB'))
frames[0].save(P/'Butterfly_Flap_Preview.gif',save_all=True,append_images=frames[1:],duration=90,loop=0)
for n,a in assets.items():
    im=load(n);assert list(im.size)==a['source_pixels'],n
    if not n.startswith('Rule_'):assert im.getchannel('A').getextrema()[0]==0,n
assert all('<text' not in f.read_text() for f in S.glob('*.svg'))
assert glow.getpixel((256,256))[3]>250 and glow.getpixel((0,256))[3]==0
assert all(glow.getpixel((x,256))[3]>=glow.getpixel((x+1,256))[3] for x in range(256,511))
assert all(glow.getchannel(c).getextrema()==(255,255) for c in ['R','G','B'])
report={'rgba_assets':len(assets),'png_dimensions_verified':True,'no_baked_text':True,'butterflies':'128x128; identical canvas and pivot; 3 poses','glow':'512x512 white RGB; alpha center bright, monotonic to zero edge','menu_rects_match_B':True}
(O/'validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
(O/'README.txt').write_text('Archive B sprites. See layout.json for all rects and source-pixel 9-slice borders. PNGs are straight-alpha RGBA. Text is runtime TMP. Icons are white for runtime tint. Butterfly frames are vector interpretations of the source character butterflies, with unchanged center pivot, not extracted source pixels. Glow_Red RGB is white; tint crimson at runtime. Sources remain in parent folder. Assembly preview includes optional effect placements; original Archive_B_Mockup.png is untouched.',encoding='utf-8')
with zipfile.ZipFile(P/'Archive_B_Package.zip','w',zipfile.ZIP_DEFLATED) as z:
    for f in [P/'Archive_B_Assembly.png',P/'Archive_B_AssetSheet.png',P/'Butterfly_Flap_Preview.gif',Path(__file__),*sorted(O.rglob('*'))]:
        if f.is_file():z.write(f,f.relative_to(P))
print(json.dumps(report))
