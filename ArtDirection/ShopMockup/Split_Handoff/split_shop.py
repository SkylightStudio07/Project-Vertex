"""Text-free shop sprites from the canonical raster, without displaying source PNGs."""
from pathlib import Path
import json, math
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageOps

ROOT=Path(__file__).resolve().parent.parent
OUT=ROOT/'Extracted'; OUT.mkdir(exist_ok=True)
SRC=ROOT/'Shop_Mockup_v2_Canonical.png'
source=Image.open(SRC).convert('RGB'); assert source.size==(1672,941)
INK='#16181B'; EDGE='#2A2C30'; CYAN='#0DB8F2'; SHADOW='#C9CCD1'
assets=[]

def rect(p):
    x,y,w,h=p
    return [round(x*1672/960),round(y*941/540),round(w*1672/960),round(h*941/540)]
def crop(p):
    x,y,w,h=rect(p); return source.crop((x,y,x+w,y+h))
def rgba(size): return Image.new('RGBA',size,(0,0,0,0))
def save(name,im,p,border=None,method='source-guided contour restoration with clean source paper texture',**extra):
    im.save(OUT/(name+'.png'))
    assets.append(dict(name=name,file=name+'.png',source=SRC.name,source_crop_xywh=rect(p),
       output_size=list(im.size),placement_1920_xy=[round(p[0]*2),round(p[1]*2)],
       nine_slice_lbrt=border or [0,0,0,0],method=method,**extra))

# Clean card-panel margin: no labels, cards, border or watermark in this narrow sample.
PATCH=(397,191,8,50)
tile=np.asarray(crop(PATCH),dtype=np.int16)
base=np.array([241,240,236])
# Preserve sampled paper grain while rejecting dark artifacts and aligning paper palette.
grain=np.clip(tile-tile.mean(axis=(0,1)), -5, 5).astype(np.int16)
def paper(size):
    w,h=size; th,tw=grain.shape[:2]
    mirrored=np.concatenate([grain,grain[:,::-1]],axis=1)
    mirrored=np.concatenate([mirrored,mirrored[::-1]],axis=0)
    a=np.tile(mirrored,(math.ceil(h/(th*2)),math.ceil(w/(tw*2)),1))[:h,:w]
    return Image.fromarray(np.uint8(np.clip(base+a,0,255)),'RGB').convert('RGBA')

def polygon(w,h,corner,cut):
    if corner=='tr': return [(0,0),(w-cut-1,0),(w-1,cut),(w-1,h-1),(0,h-1)]
    if corner=='bl': return [(0,0),(w-1,0),(w-1,h-1),(cut,h-1),(0,h-cut-1)]
    if corner=='br': return [(0,0),(w-1,0),(w-1,h-cut-1),(w-cut-1,h-1),(0,h-1)]
    return [(cut,0),(w-1,0),(w-1,h-1),(0,h-1),(0,cut)]

def panel(name,p,corner='tr',state='Normal',dark=False,credits=False,marks=False):
    w,h=round(p[2]*2),round(p[3]*2); shadow=4; fw,fh=w-shadow,h-shadow
    cut=min(32,fh//4); pts=polygon(fw,fh,corner,cut)
    im=rgba((w,h)); d=ImageDraw.Draw(im)
    d.polygon([(x+shadow,y+shadow) for x,y in pts],fill=SHADOW)
    texture=Image.new('RGBA',(fw,fh),INK) if dark else paper((fw,fh))
    mask=Image.new('L',(fw,fh)); ImageDraw.Draw(mask).polygon(pts,fill=255)
    im.paste(texture,(0,0),mask); d=ImageDraw.Draw(im)
    outline=CYAN if state=='Hover' else '#A2A7AE' if state=='Disabled' else EDGE
    d.line(pts+[pts[0]],fill=outline,width=1)
    if state=='Pressed':
        d.line((8,fh-6,fw-10,fh-6),fill=EDGE,width=2)
    if marks:
        for x,y in [(14,fh-18),(fw-18,fh-18)]:
            d.line((x-5,y,x+5,y),fill='#C9CED4' if dark else '#6B7078')
            d.line((x,y-5,x,y+5),fill='#C9CED4' if dark else '#6B7078')
    if credits: d.line((12,fh-9,fw-58,fh-9),fill=CYAN,width=4)
    save(name,im,p,[cut+6,cut+8,cut+8,cut+6],state=state,shadow_offset=[4,4],
         texture_sample_xywh=None if dark else rect(PATCH))

panel('Panel_Cards',(382,67,366,375),'tr',marks=True)
panel('Panel_Supply',(753,121,199,326),'bl',marks=True)
for s in ('Normal','Hover','Disabled'): panel('Panel_Service_'+s,(381,450,366,81),'br',s,marks=True)
panel('Panel_Credits',(748,53,204,57),'tl',credits=True)
panel('Panel_Shopkeeper',(16,119,277,102),'br',dark=True,marks=True)
for s in ('Normal','Hover','Pressed'): panel('Button_Leave_'+s,(769,467,181,66),'tl',s)

def cell(name,size,p,state='Normal',chamfer=False):
    w,h=size; im=paper(size); d=ImageDraw.Draw(im)
    color=CYAN if state=='Hover' else '#A2A7AE' if state=='Pressed' else '#B6B8B7'
    if chamfer:
        mask=Image.new('L',size); pts=[(5,0),(w-6,0),(w-1,5),(w-1,h-6),(w-6,h-1),(5,h-1),(0,h-6),(0,5)]
        ImageDraw.Draw(mask).polygon(pts,fill=255); im.putalpha(mask)
        ImageDraw.Draw(im).line(pts+[pts[0]],fill=color,width=1)
    else: d.rectangle((0,0,w-1,h-1),outline=color,width=1)
    if state=='Pressed': ImageDraw.Draw(im).line((2,h-3,w-3,h-3),fill=EDGE)
    save(name,im,p,[7]*4,state=state)
for s in ('Normal','Hover','Pressed'): cell('Button_Close_'+s,(40,40),(922,65,23,25),s)
for s in ('Normal','Hover'):
    cell('PriceTag_'+s,(150,28),(414,253,87,20),s,True)
    cell('ItemCell_'+s,(130,130),(770,208,80,72),s,True)

def extracted(name,p,size,watermark=False):
    img=crop(p).convert('L'); a=np.asarray(img,dtype=float)
    # Background is local paper. Separate line ink from the upper paper luminance percentile.
    bg=float(np.percentile(a,85))
    alpha=np.clip((bg-a-5)*(3 if watermark else 1.4),0,255).astype('uint8')
    alpha[alpha<12]=0
    if name=='Watermark_Cards':
        # The underlying first card's top border touches the watermark crop.
        alpha[round(alpha.shape[0]*.88):,:round(alpha.shape[1]*.35)]=0
    if name=='Watermark_Supply':
        # SUPPLY typography overlaps the lower-left of the original watermark.
        alpha[round(alpha.shape[0]*.63):,:round(alpha.shape[1]*.48)]=0
        alpha[a<175]=0
    if not watermark: alpha=np.where(alpha>55,255,0).astype('uint8')
    mask=Image.fromarray(alpha,'L'); mask.thumbnail(size,Image.Resampling.LANCZOS)
    if not watermark: mask=mask.point(lambda v:255 if v>=65 else 0)
    im=rgba(size); ink=Image.new('RGBA',mask.size,EDGE); ink.putalpha(mask)
    im.paste(ink,((size[0]-mask.width)//2,(size[1]-mask.height)//2))
    save(name,im,p,method='local paper luminance removed from canonical crop; aspect-preserving ink extraction',
        recommended_runtime_alpha=0.08 if watermark else 1)

extracted('Watermark_Cards',(635,79,85,50),(170,100),True)
extracted('Watermark_Supply',(865,135,72,66),(144,132),True)
extracted('Watermark_Service',(541,461,44,53),(88,106),True)
extracted('Icon_Credit',(421,257,11,13),(20,20))
extracted('Icon_Arrow',(488,257,9,13),(20,20))
extracted('Icon_Close',(925,70,15,14),(20,20))
extracted('Icon_Leave',(785,477,29,37),(40,48))
extracted('Icon_Scissors',(591,484,20,28),(32,40))
im=rgba((40,40)); d=ImageDraw.Draw(im)
d.line([(8,9),(3,11),(7,36),(28,33),(28,31)],fill=EDGE,width=1)
d.rectangle((12,3,33,30),outline=EDGE,width=1)
d.line((18,11,27,21),fill=EDGE,width=1); d.line((27,11,18,21),fill=EDGE,width=1)
save('Icon_RemoveCard',im,(541,461,44,53),method='source-guided outline restored at 40px to preserve fine strokes')

im=rgba((18,2)); ImageDraw.Draw(im).line((0,0,17,0),fill=EDGE)
save('NumberUnderline',im,(405,94,9,1),method='restored graphite hairline')

# Separate textless band and hatching also supplied so game can independently fade them.
size=(150,230)
hatch=rgba(size); hd=ImageDraw.Draw(hatch)
for k in range(-230,150,9): hd.line((k,0,k+230,229),fill='#6B7078',width=1)
band=rgba(size); bd=ImageDraw.Draw(band)
bd.polygon([(0,130),(149,92),(149,125),(0,163)],fill=INK)
stamp=Image.alpha_composite(hatch,band)
save('SoldOut_Stamp',stamp,(524,284,90,118),method='restored diagonal hatch and empty angled ink band; SOLD OUT omitted')
save('SoldOut_Hatching',hatch,(524,284,90,118),method='separate optional hatch overlay')
save('SoldOut_Band',band,(524,284,90,118),method='separate optional empty angled stamp band')

manifest=dict(version=2,source=SRC.name,source_canvas=[1672,941],design_canvas=[1920,1080],
    coordinates='source_crop_xywh: original pixels. placement_1920_xy: top-left origin. output_size: design pixels.',
    geometry_note='Panels follow canonical screenshot footprint, not superseded layout brief dimensions. Shared PriceTag 150x28; ItemCell130x130.',
    assets=assets,shopkeeper_label='SHOPKEEPER',shopkeeper_label_excludes_number=True,
    price_tag_shared_for=['cards','items'],price_tag_widths=[150,130],
    source_art_preserved='No source files, card illustrations, item art, counter, characters or Unity importers modified.',
    runtime_watermark_alpha=0.08,
    card_price_tag_instances_1920=[[828,506],[1052,506],[1276,506],[828,808],[1052,808],[1276,808]],
    item_price_tag_instances_1920=[[1540,562],[1712,562],[1540,764],[1712,764]],
    item_cell_instances_1920=[[1540,416],[1712,416],[1540,620],[1712,620]],
    state_notes={'Disabled':'Panel_Service uses muted border; disable interaction and gray runtime labels/icons.',
                 'Hover':'Cyan 1px outline; runtime may raise the panel 2px.',
                 'Pressed':'Inset graphite lower rule; runtime may translate content 2px.'})
(OUT/'layout.json').write_text(json.dumps(manifest,indent=2,ensure_ascii=False),encoding='utf-8')

cols=4; cw=300; ch=150; sheet=Image.new('RGB',(1200,math.ceil(len(assets)/cols)*ch),'#555B63')
d=ImageDraw.Draw(sheet); font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',13)
for idx,e in enumerate(assets):
    x=idx%cols*cw; y=idx//cols*ch
    for yy in range(y+25,y+ch,12):
        for xx in range(x,x+cw,12):
            d.rectangle((xx,yy,xx+11,yy+11),fill='#CDD0D4' if ((xx-x)//12+(yy-y-25)//12)%2 else '#E4E5E7')
    d.text((x+7,y+5),e['name'],font=font,fill='white')
    im=Image.open(OUT/e['file']); im.thumbnail((280,116),Image.Resampling.LANCZOS)
    sheet.paste(im,(x+(cw-im.width)//2,y+28+(116-im.height)//2),im)
sheet.save(OUT/'Contact_Sheet.jpg',quality=91)

readme='''# 상점 v2 분절

정본: Shop_Mockup_v2_Canonical.png (1672×941). 출력 좌표계: 1920×1080, 좌상단 원점.

## 제작 방식과 범위

원본은 스크립트에서만 읽었습니다. 판은 정본의 외형을 기준으로 복원하고, 글자·번호·카드·아이템·워터마크가 없는 깨끗한 여백에서 추출한 종이 질감을 채웠습니다. 따라서 원본을 사각형으로 자른 이미지가 아니라 **질감 복원판**입니다. 질감은 원본 샘플을 반전 반복하고 #F1F0EC에 맞춰 보정했습니다. 회색 불투명 그림자 띠(4px)는 각 큰 판에 포함되어 있습니다. 원본 파일은 변경하지 않았습니다.

워터마크와 기호는 원본 크롭의 종이 밝기를 제거해 분리했습니다. 아주 옅은 선화 특성상 미세한 종이 잡음이 남을 수 있습니다. 판과 셀, 가격표 알파는 0/255입니다. 워터마크는 원본 선 가장자리를 살리는 가변 알파를 사용하며 게임에서 전체 알파를 약 0.08부터 조절하십시오.

## 조립

- Panel_Cards/Supply/Service/Credits/Shopkeeper와 Button_Leave/Close에는 제목·숫자·가격·대사·기호가 없습니다. Panel_Cards의 등록 +, Credits의 청록 막대는 포함합니다.
- 이름판의 런타임 영문은 **SHOPKEEPER**입니다. 02 번호는 넣지 마십시오. 번호·제목·미세 영문·SOLD OUT 등은 모두 TMP로 넣습니다.
- Watermark_Cards/Supply/Service는 해당 판 위에 별도 레이어로 올립니다. 원본 위치는 JSON에 있습니다. Watermark_Service는 정본의 카드 삭제 선화입니다.
- PriceTag_Normal/Hover를 카드·아이템에 공용으로 사용합니다. 150×28을 기준으로 아이템에는 폭130으로 9-slice합니다. Icon_Credit/Arrow와 숫자는 별도입니다.
- ItemCell_Normal/Hover는 상품 아이콘 아래에 배치합니다. 상품 그림은 포함하지 않습니다.
- Icon_Close는 Button_Close, Icon_Leave와 Arrow는 Button_Leave, Icon_Scissors/RemoveCard는 서비스에 사용합니다.
- SoldOut_Stamp는 글자 없는 사선 먹색 띠와 해칭을 합친150×230 오버레이입니다. 더 세밀하게 제어하려면 별도 SoldOut_Hatching과 SoldOut_Band를 사용하십시오. 두 방식은 중복해서 올리지 않습니다.
- Disabled 서비스는 흐린 테두리이며, 글자와 기호도 게임에서 회색으로 조절하고 입력을 차단합니다.

## 좌표와 9-slice

layout.json의 source_crop_xywh는 원본 PNG 픽셀, placement_1920_xy는 화면 배치, output_size는 실제 PNG 크기입니다. 원본 크롭은 복원/추출에 사용한 참고 영역을 뜻합니다. 큰 판은 정본의 실제 비율로 출력했으며 이전 LAYOUT_BRIEF의 구역 치수와 다릅니다. 가격표·셀은 재사용 규격으로 정규화했습니다.

nine_slice_lbrt 순서는 Left, Bottom, Right, Top(출력 픽셀)입니다. 모서리 컷·그림자·등록 마크를 보존할 여백을 지정했습니다. PriceTag/ItemCell/Close는7px 경계입니다. 기호·워터마크·품절 해칭은 Simple로 사용합니다. 9-slice는 최소한 좌우/상하 경계 합보다 크게 사용하십시오.

## 재생성·검증

python ArtDirection/ShopMockup/Split_Handoff/split_shop.py

Contact_Sheet.jpg 한 장(1200px 폭)에 전 조각이 있습니다. PNG 규격, 원본 크롭 범위와 판 알파를 자동 확인합니다. Unity importer 및 .meta는 변경하지 않았습니다.
'''
(OUT/'README.md').write_text(readme,encoding='utf-8')
for e in assets:
    im=Image.open(OUT/e['file']); assert list(im.size)==e['output_size']
    x,y,w,h=e['source_crop_xywh']; assert x>=0 and y>=0 and x+w<=1672 and y+h<=941,e['name']
    if not e['name'].startswith('Watermark'):
        assert set(im.getchannel('A').get_flattened_data())<={0,255},e['name']
    assert im.getchannel('A').getbbox(),e['name']
print(f'Validated {len(assets)} sprites; contact sheet {sheet.size}.')

if __name__=='__main__': pass
