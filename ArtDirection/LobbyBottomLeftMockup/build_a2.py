from pathlib import Path
from PIL import Image, ImageOps, ImageDraw, ImageFont, ImageChops
import json, subprocess, zipfile

ROOT=Path(__file__).resolve().parent
OUT=ROOT/'Extracted_Draft'
SVG=OUT/'SVG'
SVG.mkdir(parents=True,exist_ok=True)
INK='#16181B'; PAPER='#EEF0F2'; CYAN='#0DB8F2'
assets={}

def shape(w,h):
    return f'M.75 .75H{w-18.75}L{w-.75} 18.75V{h-.75}H.75Z'

def asset(name,w,h,body,border=None):
    (SVG/f'{name}.svg').write_text(f'<svg xmlns="http://www.w3.org/2000/svg" width="{w*2}" height="{h*2}" viewBox="0 0 {w} {h}">{body}</svg>',encoding='utf-8')
    assets[name]={'file':name+'.png','svg':'SVG/'+name+'.svg','pixel_size':[w*2,h*2],'logical_size':[w,h],'scale':2}
    if border:
        assets[name]['nine_slice']={'source_pixels_left_bottom_right_top':border,'logical_pixels_left_bottom_right_top':[v/2 for v in border],'mode':'sliced; preserve corners; stretch center','minimum_logical_size':[40,40]}

for name,w,h,fill in [('Panel_Dialogue',480,90,PAPER),('Panel_Story',320,136,'none'),('Panel_Small',148,64,PAPER)]:
    asset(name,w,h,f'<path d="{shape(w,h)}" fill="{fill}" stroke="{INK}" stroke-width="1.5"/>',[4,4,40,40])
asset('Mask_StoryThumb',320,136,f'<path d="{shape(320,136)}" fill="white"/>',[4,4,40,40])
asset('Overlay_StoryGradient',320,136,f'<defs><linearGradient id="g"><stop offset="0" stop-color="{INK}" stop-opacity=".98"/><stop offset=".52" stop-color="{INK}" stop-opacity=".88"/><stop offset="1" stop-color="{INK}" stop-opacity=".05"/></linearGradient></defs><path d="{shape(320,136)}" fill="url(#g)"/>')
asset('Icon_Archive',40,40,f'<g fill="none" stroke="{INK}" stroke-width="2.4"><path d="M4 8V34H29V8ZM10 3H35V29M9 16H24M9 22H24"/></g>')
asset('Icon_Companions',40,40,f'<g fill="{INK}"><circle cx="15" cy="11" r="6"/><circle cx="28" cy="14" r="5"/><path d="M3 35V29Q3 20 15 20Q27 20 27 29V35ZM28 23Q37 23 37 31V35H30V29Q30 26 28 23Z"/></g>')
asset('Mark_Next',10,14,f'<path fill="{CYAN}" d="M0 0L10 7L0 14Z"/>')
asset('Mark_Underline',12,2,f'<path stroke="{INK}" stroke-width="1" d="M0 1H12"/>')
asset('Mark_UnderlineLight',12,2,f'<path stroke="{PAPER}" stroke-width="1" d="M0 1H12"/>')

js="""const fs=require('fs'),path=require('path');const {Resvg}=require('C:/Users/SKYLIGHT/.codex/tmp/currency-f-render/node_modules/@resvg/resvg-js');for(const f of fs.readdirSync(process.argv[1])){if(f.endsWith('.svg'))fs.writeFileSync(path.join(process.argv[2],f.replace('.svg','.png')),new Resvg(fs.readFileSync(path.join(process.argv[1],f))).render().asPng());}"""
subprocess.run(['node','-e',js,str(SVG),str(OUT)],check=True)
thumb=ImageOps.fit(Image.open(ROOT/'Sources/Thumb_Generated.png').convert('RGBA'),(640,272),method=Image.Resampling.LANCZOS)
thumb.save(OUT/'Thumb_Sample.png')
assets['Thumb_Sample']={'file':'Thumb_Sample.png','pixel_size':[640,272],'logical_size':[320,136],'scale':2,'source':'../Sources/Thumb_Generated.png','mask':'Mask_StoryThumb.png'}

base=Image.open(ROOT/'Reference_Lobby_Current_v2.png').convert('RGBA')
assert base.size==(1920,1080)
layer=Image.new('RGBA',(3840,2160))
elements=[]; texts=[]

def place(name,rect,id=None):
    x,y,w,h=rect
    im=Image.open(OUT/(name+'.png')).convert('RGBA')
    if im.size!=(w*2,h*2): im=im.resize((w*2,h*2),Image.Resampling.LANCZOS)
    layer.alpha_composite(im,(x*2,y*2))
    elements.append({'id':id or name,'asset':name,'rect':rect})

def text(id,value,rect,size,color=INK,bold=False,latin=False):
    fontname='bahnschrift.ttf' if latin else ('malgunbd.ttf' if bold else 'malgun.ttf')
    font=ImageFont.truetype('C:/Windows/Fonts/'+fontname,size*2)
    x,y,w,h=rect
    box=font.getbbox(value)
    assert box[2]-box[0]<=w*2,(id,'text exceeds width')
    assert box[3]-box[1]<=h*2,(id,'text exceeds height')
    ImageDraw.Draw(layer).text((x*2-box[0],y*2-box[1]),value,font=font,fill=color)
    texts.append({'id':id,'text':value,'rect':rect,'font_size_px':size,'font':fontname,'weight':700 if bold else 400,'color':color,'alignment':'top-left','auto_size':False,'baked_in_asset':False})

place('Panel_Dialogue',[40,800,480,90])
thumb.putalpha(Image.open(OUT/'Mask_StoryThumb.png').getchannel('A'))
layer.alpha_composite(thumb,(80,1810))
elements.append({'id':'StoryThumbnail','asset':'Thumb_Sample','mask':'Mask_StoryThumb','rect':[40,905,320,136]})
place('Overlay_StoryGradient',[40,905,320,136])
place('Panel_Story',[40,905,320,136])
place('Panel_Small',[372,905,148,64],'ArchivePanel')
place('Panel_Small',[372,977,148,64],'CompanionPanel')
place('Icon_Archive',[382,918,34,34])
place('Icon_Companions',[382,990,34,34])
place('Mark_Next',[495,868,9,12],'DialogueNext')
place('Mark_Next',[54,1022,6,8],'ObjectiveNext')
for name,rect,id in [('Mark_Underline',[54,822,12,2],'DialogueUnderline'),('Mark_UnderlineLight',[54,927,12,2],'StoryUnderline'),('Mark_Underline',[380,919,9,2],'ArchiveUnderline'),('Mark_Underline',[380,991,9,2],'CompanionUnderline')]:place(name,rect,id)

text('DialogueNumber','01',[54,809,16,12],10,latin=True)
text('Speaker','치하라 쇼',[78,807,200,20],18,bold=True)
text('DialogueCaption','DIALOGUE',[426,811,78,12],10,latin=True)
text('DialogueLine1','비 오는 날엔 총기 손질이 두 배야.',[54,837,440,20],19)
text('DialogueLine2','다음 여정을 준비할까요?',[54,862,430,20],19)
text('StoryHeader','02 / STORY CH.2-1',[54,914,275,12],11,PAPER,latin=True)
text('StoryTitle1','영원하지 않기에',[54,938,282,26],24,PAPER,True)
text('StoryTitle2','빛나는 것들',[54,969,282,26],24,PAPER,True)
text('StoryObjective','다음 목표: 1막 보스 격파',[66,1019,274,16],14,PAPER)
for y,label,caption,num,key in [(905,'아카이브','ARCHIVES','03','Archive'),(977,'동료','COMPANIONS','04','Companion')]:
    text(key+'Number',num,[380,y+5,14,9],8,latin=True)
    text(key+'Name',label,[423,y+17,91,22],20,bold=True)
    text(key+'Caption',caption,[423,y+44,91,10],9,latin=True)

overlay=layer.resize((1920,1080),Image.Resampling.LANCZOS)
# Clip antialiasing to prescribed rects so absolutely all other pixels are untouched.
allowed=Image.new('L',(1920,1080));draw=ImageDraw.Draw(allowed)
rects=[[40,800,480,90],[40,905,320,136],[372,905,148,64],[372,977,148,64]]
for x,y,w,h in rects:draw.rectangle((x,y,x+w-1,y+h-1),fill=255)
overlay.putalpha(ImageChops.multiply(overlay.getchannel('A'),allowed))
final=Image.alpha_composite(base,overlay)
final.convert('RGB').save(ROOT/'LobbyBottomLeft_A2_Mockup.png')
final.crop((32,792,528,1049)).resize((992,514),Image.Resampling.LANCZOS).convert('RGB').save(ROOT/'LobbyBottomLeft_A2_Detail.png')
diff=ImageChops.difference(base.convert('RGB'),final.convert('RGB'))
outside=ImageChops.multiply(diff,Image.merge('RGB',(ImageOps.invert(allowed),)*3))
assert outside.getbbox() is None
for name,a in assets.items():
    im=Image.open(OUT/a['file'])
    assert list(im.size)==a['pixel_size'] and im.mode=='RGBA'
for f in SVG.glob('*.svg'):assert '<text' not in f.read_text()
layout={'version':'A2','reference':'../Reference_Lobby_Current_v2.png','canvas':[1920,1080],'origin':'top-left','rect_format':'x,y,width,height','asset_scale':2,'unity_pixels_per_unit':200,'shadows':False,'assets':assets,'elements_in_draw_order':elements,'text_boxes':texts,'story_composition':['Thumb_Sample clipped with Mask_StoryThumb alpha','Overlay_StoryGradient','Panel_Story transparent-center border','TMP labels and next marker'],'notes':['Text and numbers only appear in mockup; sprites contain no glyphs.','Use Simple full-rect thumbnail and gradient fitted to story rect; sliced mask and border preserve cut corner.','Thumbnail unchanged in tone from A, cropped to full-card 2.35:1 per section 4.','4-sided slice values use source PNG pixels; divide by 2 for design pixels.']}
(OUT/'layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf-8')
report={'canvas':[1920,1080],'rects':rects,'background_pixels_outside_rects_unchanged':True,'all_text_fits':True,'svg_no_text':True,'png_assets':len(assets),'shadows':False}
(OUT/'validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
with zipfile.ZipFile(ROOT/'LobbyBottomLeft_A2_Package.zip','w',zipfile.ZIP_DEFLATED) as z:
    for f in [ROOT/'LobbyBottomLeft_A2_Mockup.png',ROOT/'LobbyBottomLeft_A2_Detail.png',Path(__file__),*sorted(OUT.rglob('*'))]:
        if f.is_file():z.write(f,f.relative_to(ROOT))
print(json.dumps(report))
