from pathlib import Path
from PIL import Image, ImageDraw, ImageChops
import json, math, subprocess

ROOT = Path(__file__).resolve().parent
OUT = ROOT / 'Extracted_Draft'
SVG = OUT / 'SVG'
SVG.mkdir(parents=True, exist_ok=True)
INK, PAPER, CYAN = '#16181B', '#EEF0F2', '#0DB8F2'

def svg(name, body, w=76, h=76, view='0 0 38 38'):
    s=f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="{view}">{body}</svg>'
    (SVG / (name+'.svg')).write_text(s, encoding='utf-8')

svg('Icon_Talin', f'<path fill="{INK}" d="M4 6H27L34 13V32H4Z"/><path fill="{PAPER}" d="M19 12L25 19L19 26L13 19Z"/><path fill="{INK}" d="M19 16L21.5 19L19 22L16.5 19Z"/>')
# Hexagonal nut and diagonal bolt; no gear teeth.
svg('Icon_Parts', f'<path fill="{INK}" fill-rule="evenodd" d="M14 3L26 10V24L14 31L2 24V10Z M14 11L9 14V20L14 23L19 20V14Z"/><path stroke="{PAPER}" stroke-width="2.4" fill="{INK}" d="M21 17L35 30L30 35L16 22Z"/><path fill="{INK}" d="M18 13L25 20L20 25L13 18Z"/><path stroke="{PAPER}" stroke-width="1.4" d="M26 24L22 28M30 28L26 32"/>')
svg('Icon_Artifact', f'<g fill="none" stroke="{INK}" stroke-width="1.05"><path d="M19 1L37 19L19 37L1 19Z"/><path d="M19 4.5L33.5 19L19 33.5L4.5 19Z"/></g><path fill="{INK}" d="M19 8L22.5 15.5L30 19L22.5 22.5L19 30L15.5 22.5L8 19L15.5 15.5Z"/>')
svg('Button_Plus', f'<circle cx="12" cy="12" r="12" fill="{CYAN}"/><path stroke="#FFFFFF" stroke-width="2.5" d="M6 12H18M12 6V18"/>',48,48,'0 0 24 24')
pts=[]
for i in range(48):
    a=2*math.pi*i/48-math.pi/2
    r=17 if i%6 in (1,2,3,4) else 13.8
    pts.append(f'{19+r*math.cos(a):.3f},{19+r*math.sin(a):.3f}')
svg('Icon_Settings', f'<polygon points="{" ".join(pts)}" fill="{INK}"/><circle cx="19" cy="19" r="6.5" fill="{PAPER}"/>')
svg('Dev_Track','<rect width="16" height="4" rx="2" fill="#D4D6D8"/>',16,4,'0 0 16 4')
svg('Dev_Fill',f'<rect width="16" height="4" rx="2" fill="{CYAN}"/>',16,4,'0 0 16 4')
render = '''const fs=require('fs'), path=require('path');
const {Resvg}=require('C:/Users/SKYLIGHT/.codex/tmp/currency-f-render/node_modules/@resvg/resvg-js');
for(const file of fs.readdirSync(process.argv[1])) { if(!file.endsWith('.svg'))continue;
 const r=new Resvg(fs.readFileSync(path.join(process.argv[1],file)));
 fs.writeFileSync(path.join(process.argv[2],file.replace('.svg','.png')),r.render().asPng()); }
'''
subprocess.run(['node','-e',render,str(SVG),str(OUT)],check=True)

base=Image.open(ROOT/'Reference_Lobby_Current.png').convert('RGB')
source_size=base.size
base=base.resize((1920,1080),Image.Resampling.LANCZOS).convert('RGBA')
e=Image.open(ROOT/'CurrencyBar_E_Mockup.png').convert('RGB')
canvas=base.copy()
# Replace only old HUD with neighboring original paper pixels, no generated scene.
erase=(1312,34,1718,69)
for x in range(erase[0],erase[2]):
    for y in range(erase[1],erase[3]):
        canvas.putpixel((x,y),base.getpixel((x,22)))
hud=Image.new('RGBA',(1920,1080))

def retain_e(rect):
    """Keep E glyph silhouettes and antialiasing; remove its paper backdrop."""
    crop=e.crop(rect)
    result=Image.new('RGBA',crop.size)
    for y in range(crop.height):
        # Background is locally sampled along top edge of each tight text crop.
        for x in range(crop.width):
            r,g,b=crop.getpixel((x,y))
            if b-r>45 and g-r>45:
                result.putpixel((x,y),(r,g,b,255))
            else:
                a=max(0,min(255,round((235-min(r,g,b))*255/214)))
                if a<20:a=0
                result.putpixel((x,y),(22,24,27,a))
    hud.alpha_composite(result,rect[:2])

# E's number raster shapes, sizes, and positions are reused, not re-typeset.
number_rects={'Talin':[1353,49,1449,88], 'Parts':[1544,49,1590,87], 'Artifact':[1660,49,1689,88]}
for rect in number_rects.values(): retain_e(tuple(rect))
retain_e((1313,17,1436,40))
retain_e((1660,17,1711,40))

elements={}
def place(name,box):
    im=Image.open(OUT/(name+'.png')).convert('RGBA')
    if name.startswith('Dev_'):
        rendered=Image.new('RGBA',(box[2],box[3]))
        cap=min(box[3]//2,box[2]//2)
        rendered.alpha_composite(im.crop((0,0,2,4)).resize((cap,box[3]),Image.Resampling.LANCZOS))
        rendered.alpha_composite(im.crop((2,0,14,4)).resize((box[2]-2*cap,box[3]),Image.Resampling.LANCZOS),(cap,0))
        rendered.alpha_composite(im.crop((14,0,16,4)).resize((cap,box[3]),Image.Resampling.LANCZOS),(box[2]-cap,0))
    else:
        rendered=im.resize((box[2],box[3]),Image.Resampling.LANCZOS)
    hud.alpha_composite(rendered,box[:2])
    elements[name]={'file':name+'.png','rect':box,'source_size':list(im.size),'pivot':[0.5,0.5]}

place('Icon_Talin',[1310,50,38,38])
place('Icon_Parts',[1501,50,38,38])
place('Icon_Artifact',[1609,49,38,38])
place('Icon_Settings',[1720,50,38,38])
place('Button_Plus',[1451,56,28,28])
# Retain E's visible gauge and plus dimensions in the comparison mockup.
# Exported assets still follow the specified 48px button and 4px-high tracks.
place('Dev_Track',[1449,25,200,8])
place('Dev_Fill',[1449,25,12,8])
for n in ['Dev_Track','Dev_Fill']:
    elements[n]['border_source_pixels_lbrt']=[2,0,2,0]
    elements[n]['slicing']='horizontal; preserve 4px source height at 2x; stretch center'
elements['Dev_Fill']['note']='E cyan start cap retained; zero value, not a proportional fill.'
elements['Dev_Track']['specified_runtime_height']=2
elements['Dev_Fill']['specified_runtime_height']=2
elements['Button_Plus']['specified_runtime_size']=[24,24]
canvas.alpha_composite(hud)
clean=canvas.copy()
clean.save(ROOT/'CurrencyBar_F_Runtime.png')
hud_box=(1306,14,1764,92)
detail=hud.crop(hud_box).resize((916,156),Image.Resampling.LANCZOS)
inset=(984,900,1918,1078)
ImageDraw.Draw(canvas).rectangle(inset,fill=PAPER)
canvas.alpha_composite(detail,(994,917))
canvas.save(ROOT/'CurrencyBar_F_Mockup.png')

sheet=Image.new('RGBA',(456,152))
for i,n in enumerate(['Icon_Talin','Icon_Parts','Icon_Artifact']):
    sheet.alpha_composite(Image.open(OUT/(n+'.png')).convert('RGBA'),(i*152+38,38))
sheet.save(ROOT/'Icons_Sheet.png')
layout={'variant':'F','canvas':[1920,1080],'origin':'top-left','asset_scale':2,'colors':{'ink':INK,'paper':PAPER,'cyan':CYAN},'shadows':False,
 'reference':{'file':'../Reference_Lobby_Current.png','original_size':list(source_size),'resampling':'Lanczos to 1920x1080; no generative repaint'},
 'elements':elements,'numbers':{},
 'development':{'label':'BASE DEV  Lv.3','label_box':[1313,17,123,23],'value':'0 / 30','value_box':[1660,17,51,23],'font_size':14,'color':INK,'alignment':'left','mockup_typography':'E raster glyphs retained'},
 'mockup':{'file':'../CurrencyBar_F_Mockup.png','clean_runtime':'../CurrencyBar_F_Runtime.png','hud_source_box':list(hud_box),'detail_position':[994,917],'detail_scale':2,'inset_box':list(inset),'old_hud_cover_box':list(erase)},
 'notes':['No digits baked into extracted PNG assets.','E number glyphs reused in mockup; TMP font is an implementation recommendation, not the source of mockup glyphs.','All rect values are x,y,width,height except explicitly named boxes, which are left,top,right,bottom.']}
for name,rect in number_rects.items():
    x,y,r,b=rect
    layout['numbers'][name]={'sample':{'Talin':'1,240','Parts':'36','Artifact':'2'}[name],'box':[x,y,r-x,b-y],'alignment':'left-middle','font_size':34,'font_weight':900,'color':INK,'recommended_font':'Arial Black','overflow':'expand left within group or use TMP autosize for five digits','mockup_source_E_box':rect}
(OUT/'layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf-8')
# Verify exact preservation outside declared top HUD and detail overlays.
diff=ImageChops.difference(base.convert('RGB'),canvas.convert('RGB'))
d=ImageDraw.Draw(diff)
d.rectangle((1306,14,1764,92),fill=0)
d.rectangle(inset,fill=0)
assert diff.getbbox() is None, 'Unexpected background change outside HUD/inset'
for n,el in elements.items():
    im=Image.open(OUT/(n+'.png'))
    assert im.mode=='RGBA' and im.size==tuple(el['source_size'])
assert all(Image.open(OUT/(n+'.png')).getextrema()[3][0]==0 for n in ['Icon_Talin','Icon_Parts','Icon_Artifact','Icon_Settings','Button_Plus'])
(OUT/'validation.json').write_text(json.dumps({'background_unchanged_outside_declared_overlays':True,'mockup_size':list(canvas.size),'svg_rendered_png_count':len(elements),'icon_alpha_verified':True,'detail_scale':2},indent=2),encoding='utf-8')
print('F mockup, runtime, icon sheet, 7 SVG/PNG assets and layout.json created; background comparison passed.')
