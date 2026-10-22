from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import sys,json,base64,subprocess,zipfile
sys.path.insert(0,'C:/Users/SKYLIGHT/.codex/tmp/shop-seg')
import cv2,numpy as np
P=Path(__file__).resolve().parent;O=P/'Extracted_Draft';S=O/'SVG';D=O/'2x'
S.mkdir(parents=True,exist_ok=True);D.mkdir(exist_ok=True)
INK='#16181B';CYAN='#0DB8F2'
src=Image.open(P/'Loading_A_Mockup.png').convert('RGB');a=np.array(src);mask=np.zeros(a.shape[:2],np.uint8)
gray=cv2.cvtColor(a,cv2.COLOR_RGB2GRAY)
# Restrict removal to runtime elements; keep registration crosses/diagonals untouched.
glyph_boxes=[(760,150,1162,531),(812,536,1108,583),(829,597,1097,639),(1698,752,1772,792),(120,947,250,972),(120,977,552,1008),(1490,976,1768,1003)]
for x0,y0,x1,y1 in glyph_boxes:
    sub=(gray[y0:y1,x0:x1]<216).astype('uint8')*255
    mask[y0:y1,x0:x1]=cv2.dilate(sub,np.ones((11,11),np.uint8))
mask[374:388,760:1163]=255
mask[156:204,954:967]=255
mask[727:756,151:1770]=255
clean=cv2.inpaint(a,mask,9,cv2.INPAINT_NS)
# Smooth the inferred low-frequency surface, retaining source pixels outside repair masks.
smooth=cv2.GaussianBlur(clean,(0,0),8)
dist=cv2.distanceTransform(mask,cv2.DIST_L2,5)
blend=np.clip(dist/12,0,1)[:,:,None]
clean=np.round(clean*(1-blend)+smooth*blend).astype('uint8')
clean_im=Image.fromarray(clean).convert('RGBA');clean_im.save(O/'Loading_BG.png')
Image.fromarray(mask).save(P/'Loading_RemovalMask.png')

assets={}
def svg(n,w,h,body,border=None):
    doc=f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}">{body}</svg>'
    (S/(n+'.svg')).write_text(doc,encoding='utf-8')
    assets[n]={'file':n+'.png','pixel_size':[w,h],'svg':'SVG/'+n+'.svg','file_2x':'2x/'+n+'.png','pixel_size_2x':[w*2,h*2],'pivot':[.5,.5]}
    if border:assets[n]['nine_slice']={'base_pixels_left_bottom_right_top':border,'source_2x_pixels_left_bottom_right_top':[v*2 for v in border],'mode':'horizontal stretch; preserve end caps; retain height'}

# Background SVG embeds cleaned source raster: lossless source texture, not fake vector grain.
encoded=base64.b64encode((O/'Loading_BG.png').read_bytes()).decode()
svg('Loading_BG',1920,1080,f'<image width="1920" height="1080" href="data:image/png;base64,{encoded}"/>')
assets['Loading_BG']['source_method']='Original raster with masked runtime elements removed; SVG contains embedded raster, not pure vector reconstruction.'
# Shared 512 canvas maps to screen [704,124,512,512]; center is [960,380].
# Four pointed quadrants separated by negative space, preserving the confirmed silhouette.
quadrant='<path d="M256 126L246 235L243 239L237 234L232 239L145 256L249 253L253 249Z"/>'
star=f'<g fill="{INK}">{quadrant}<g transform="rotate(90 256 256)">{quadrant}</g><g transform="rotate(180 256 256)">{quadrant}</g><g transform="rotate(270 256 256)">{quadrant}</g></g>'
# Vertical points are longer than lateral ones in the reference.
star=f'<g transform="translate(256 256) scale(.84 1) translate(-256 -256)">{star}</g>'
svg('Emblem_Star',512,512,star)
# Broken diamond consists of four diagonal bands, each with one small seam.
frame=f'<g fill="{INK}"><path d="M232 147V160L148 246H133Z"/><path d="M280 147V160L364 246H379Z"/><path d="M232 363V350L148 266H133Z"/><path d="M280 363V350L364 266H379Z"/></g>'
# Interruptions are real transparency, not background-colored strokes.
frame=f'<defs><mask id="seams"><rect width="512" height="512" fill="white"/><path d="M186 188L197 199M315 199L326 188M186 324L197 313M315 313L326 324" stroke="black" stroke-width="1.8"/></mask></defs><g mask="url(#seams)">{frame}</g>'
frame+=f'<path d="M256 37V75M64 256H124M388 256H448" fill="none" stroke="#777D83" stroke-opacity=".6" stroke-width="1.2"/><rect x="252" y="100" width="8" height="8" fill="{INK}"/><rect x="105" y="254" width="5" height="4" fill="{INK}"/><rect x="400" y="254" width="5" height="4" fill="{INK}"/>'
svg('Emblem_Frame',512,512,frame)
svg('Track',16,6,'<rect x="0" y="1" width="16" height="4" rx="2" fill="#B3B7BA"/>',[3,0,3,0])
svg('Fill',16,6,f'<rect width="16" height="6" rx="3" fill="{CYAN}"/>',[3,0,3,0])
svg('Head_Diamond',26,26,f'<path d="M13 0L26 13L13 26L0 13Z" fill="{CYAN}"/>')
js="""const fs=require('fs'),p=require('path');const {Resvg}=require('C:/Users/SKYLIGHT/.codex/tmp/currency-f-render/node_modules/@resvg/resvg-js');for(const f of fs.readdirSync(process.argv[1])){if(!f.endsWith('.svg'))continue;const s=fs.readFileSync(p.join(process.argv[1],f));for(const scale of [1,2]){let opts=scale===2?{fitTo:{mode:'zoom',value:2}}:{};fs.writeFileSync(p.join(process.argv[2],scale===2?'2x':'',f.replace('.svg','.png')),new Resvg(s,opts).render().asPng());}}"""
subprocess.run(['node','-e',js,str(S),str(O)],check=True)

text_boxes=[
 {'id':'wordmark','text':'VERTEX','rect':[810,540,300,39],'font_size_px':30,'weight':500,'color':INK,'alignment':'center','letter_spacing_px':22,'font':'Bahnschrift'},
 {'id':'status','text':'기록을 불러오는 중...','rect':[800,601,320,38],'font_size_px':26,'weight':500,'color':'#60676D','alignment':'center','letter_spacing_px':0,'font':'Malgun Gothic'},
 {'id':'percent','text':'68%','rect':[1688,759,77,32],'font_size_px':28,'weight':400,'color':'#454C53','alignment':'right','letter_spacing_px':0,'font':'Bahnschrift'},
 {'id':'tip_label','text':'FIELD GUIDE','rect':[129,949,126,20],'font_size_px':13,'weight':400,'color':'#7D838A','alignment':'left','letter_spacing_px':3,'font':'Bahnschrift'},
 {'id':'tip','text':'동료의 호감도가 오르면 새로운 기록이 열립니다.','rect':[129,978,650,29],'font_size_px':21,'weight':500,'color':'#555D65','alignment':'left','letter_spacing_px':0,'font':'Malgun Gothic'},
 {'id':'footer','text':'PROJECT VERTEX / ARCHIVE','rect':[1495,980,268,20],'font_size_px':12,'weight':400,'color':'#7D838A','alignment':'right','letter_spacing_px':3,'font':'Bahnschrift'}]
layout={'canvas':[1920,1080],'origin':'top-left','rect_format':'x,y,width,height','assets':assets,'resolution_policy':'Main files follow explicitly requested sizes: BG 1920x1080, emblem 512x512. Additional 2x folder contains 3840x2160 / 1024x1024 and double-size small sprites. Use same logical rects with either set.','emblem':{'center':[960,380],'star_rect':[704,124,512,512],'frame_rect':[704,124,512,512],'pivot':[.5,.5],'animation':'Rotate/flip star only; keep frame static. Do not trim transparent margins.'},'progress':{'track_rect':[159,738,1602,6],'fill_rect_at_1':[159,738,1602,6],'head_size':[26,26],'head_center_y':741,'value_range':[0,1],'fill_width_formula':'1602 * progress','head_center_x_formula':'159 + 1602 * progress','reference_visual_progress':.696,'reference_label':'68%','preview_progress':.68,'note':'Generated reference endpoint is about 69.6% despite label 68%; assembly uses true 68%. Fill/head advance together. Hide head at progress=0 if required.'},'text_boxes':text_boxes,'letter_spacing_units':'Design pixels per character gap, not font-dependent TMP spacing units; convert after choosing TMP font.','layer_order':['Loading_BG','Track','Fill','Head_Diamond','Emblem_Frame','Emblem_Star','TMP text'],'text_baked':False,'notes':['Foreground geometry traced from approved image using SVG; not extracted photographic pixels.','Background uses source raster inpainting only inside runtime-element removal mask.','Background SVG embeds cleaned raster to preserve original grain and polygon facets.']}
(O/'layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf-8')

def slice_bar(name,width):
    src=Image.open(O/'2x'/(name+'.png')).convert('RGBA');width=max(12,round(width*2));res=Image.new('RGBA',(width,12))
    res.alpha_composite(src.crop((0,0,6,12)),(0,0));res.alpha_composite(src.crop((6,0,26,12)).resize((width-12,12)),(6,0));res.alpha_composite(src.crop((26,0,32,12)),(width-6,0));return res
assembly=Image.open(O/'2x/Loading_BG.png').convert('RGBA')
assembly.alpha_composite(slice_bar('Track',1602),(318,1476));assembly.alpha_composite(slice_bar('Fill',1602*.68),(318,1476))
head=Image.open(O/'2x/Head_Diamond.png').convert('RGBA');assembly.alpha_composite(head,(round((159+1602*.68-13)*2),1456))
for n in ['Emblem_Frame','Emblem_Star']:assembly.alpha_composite(Image.open(O/'2x'/(n+'.png')).convert('RGBA'),(1408,248))
draw=ImageDraw.Draw(assembly)
for t in text_boxes:
    font=ImageFont.truetype('C:/Windows/Fonts/'+('bahnschrift.ttf' if t['font']=='Bahnschrift' else 'malgun.ttf'),t['font_size_px']*2)
    s=t['text'];spacing=t['letter_spacing_px']*2;length=sum(font.getlength(c) for c in s)+max(0,len(s)-1)*spacing
    x,y,w,h=[v*2 for v in t['rect']];bb=font.getbbox(s)
    assert length<=w+2,(t['id'],length,w)
    xx=x+(w-length)/2 if t['alignment']=='center' else x+w-length if t['alignment']=='right' else x
    yy=y+(h-(bb[3]-bb[1]))/2-bb[1]
    if spacing:
        for c in s:draw.text((xx,yy),c,font=font,fill=t['color']);xx+=font.getlength(c)+spacing
    else:draw.text((xx,yy),s,font=font,fill=t['color'])
assembly.resize((1920,1080),Image.Resampling.LANCZOS).convert('RGB').save(P/'Loading_A_Assembly.png')

sheet=Image.new('RGBA',(1200,780),'#D4D6D9');dr=ImageDraw.Draw(sheet)
for y in range(0,780,20):
 for x in range(0,1200,20):
  if (x//20+y//20)%2:dr.rectangle((x,y,x+19,y+19),fill='#E9EBED')
for i,n in enumerate(['Emblem_Star','Emblem_Frame']):sheet.alpha_composite(Image.open(O/(n+'.png')).convert('RGBA'),(20+i*600,35));dr.text((25+i*600,15),n,fill=INK)
for i,n in enumerate(['Track','Fill','Head_Diamond']):
    im=Image.open(O/(n+'.png')).convert('RGBA');im=im.resize((400,12)) if n!='Head_Diamond' else im.resize((52,52));sheet.alpha_composite(im,(60,590+i*60));dr.text((490,590+i*60),n,fill=INK)
sheet.convert('RGB').save(P/'Loading_A_Sprites.png')
for n,a in assets.items():
    assert list(Image.open(O/a['file']).size)==a['pixel_size']
    assert list(Image.open(O/a['file_2x']).size)==a['pixel_size_2x']
    if n!='Loading_BG':assert Image.open(O/a['file']).getchannel('A').getextrema()[0]==0
assert np.array_equal(np.array(Image.open(O/'Loading_BG.png').convert('RGB'))[mask==0],np.array(src)[mask==0])
assert all('<text' not in f.read_text() for f in S.glob('*.svg'))
report={'asset_count':6,'sizes_verified':True,'two_resolution_sets':True,'transparent_foregrounds':True,'no_baked_text':True,'shared_emblem_center':[960,380],'background_method':'source-preserving masked inpainting, embedded raster SVG','progress_preview':.68}
(O/'validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
(O/'README.txt').write_text('Named PNGs follow the explicit table sizes. 2x/ contains twice those pixel dimensions for high-density use with unchanged logical rects. SVG foregrounds are editable vectors. Loading_BG.svg embeds the cleaned original raster to preserve texture; it is not a pure-vector replacement. All text is separate in layout.json. Letter spacing is measured in design pixels and requires conversion for the selected TMP font. Both emblem PNGs have the same 512 square canvas and centered pivot. Keep frame static and animate star independently. The generated reference visually filled ~69.6% while displaying 68%; assembly uses an accurate 68%. No runtime files were changed.',encoding='utf-8')
with zipfile.ZipFile(P/'Loading_A_Package.zip','w',zipfile.ZIP_DEFLATED) as z:
    for f in [P/'Loading_A_Assembly.png',P/'Loading_A_Sprites.png',P/'Loading_RemovalMask.png',Path(__file__),*sorted(O.rglob('*'))]:
        if f.is_file():z.write(f,f.relative_to(P))
print(json.dumps(report))
