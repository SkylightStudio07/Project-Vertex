"""Restore textless dialogue layers, with an explicit 120px alpha ramp."""
from pathlib import Path
import json, math
import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT=Path(__file__).resolve().parent.parent
OUT=ROOT/'Extracted'; OUT.mkdir(exist_ok=True)
STORY='Dialogue_Story_Mockup.png'; OVERLAY='Dialogue_Overlay_Mockup.png'
sources={n:Image.open(ROOT/n).convert('RGB') for n in (STORY,OVERLAY)}
assert all(im.size==(1920,1080) for im in sources.values())
INK='#16181B'; EDGE='#2A2C30'; CYAN='#0DB8F2'
entries=[]
def rect(p): return [round(v*2) for v in p]
def crop(p,source=STORY):
    x,y,w,h=rect(p); return sources[source].crop((x,y,x+w,y+h))
def blank(s): return Image.new('RGBA',s,(0,0,0,0))
def save(name,im,p,border=None,source=STORY,method='canonical contour restoration; text and scene content removed',**kw):
    im.save(OUT/(name+'.png'))
    entries.append(dict(name=name,file=name+'.png',source=source,source_crop_xywh=rect(p),
        output_size=list(im.size),placement_1920_xy=rect(p)[:2],nine_slice_lbrt=border or [0]*4,method=method,**kw))

# Clean fully opaque lower-band whitespace, outside dialogue and compass.
sample=(455,500,100,26)
a=np.asarray(crop(sample),dtype=float)
grain=np.clip(a-a.mean(axis=(0,1)),-3,3)
grain=np.concatenate((grain,grain[:,::-1]),axis=1)
grain=np.concatenate((grain,grain[::-1]),axis=0)
def paper(size,disabled=False):
    w,h=size; gh,gw=grain.shape[:2]
    base=np.array([217,220,223] if disabled else [241,242,244])
    arr=np.tile(grain,(math.ceil(h/gh),math.ceil(w/gw),1))[:h,:w]
    return Image.fromarray(np.uint8(np.clip(base+arr,0,255))).convert('RGBA')

im=paper((1920,300)); d=ImageDraw.Draw(im)
# End-only decorative lines so horizontal slicing does not distort ornaments.
d.line((20,200,20,87,64,43,111,43),fill='#8A9099')
d.line((35,208,35,101,77,60,115,60),fill='#B5BBC1')
d.polygon([(92,80),(99,87),(92,94),(85,87)],outline=EDGE)
d.line((1776,279,1854,279,1901,232,1901,91,1870,60),fill='#8A9099')
d.line((1796,265,1844,265,1887,222),fill='#B5BBC1')
alpha=np.full((300,1920),255,dtype='uint8')
alpha[:120]=np.rint(np.linspace(0,255,120))[:,None].astype('uint8')
im.putalpha(Image.fromarray(alpha))
save('Dialogue_Box',im,(0,390,960,150),[128,0,160,0],
     method='paper grain restored from clean source; end ornaments reconstructed; linear alpha ramp',
     alpha_ramp={'rows':[0,119],'values':[0,255],'opaque_rows':[120,299]},
     texture_sample_xywh=rect(sample),height_fixed=300)

# Isolate compass inside its circular footprint; crop ends above advance-line overlay.
p=(838,412,106,94)
gray=np.asarray(crop(p).convert('L'),dtype=float)
bg=np.percentile(gray,85)
ink=np.clip((bg-gray-2)*5,0,255)
yy,xx=np.mgrid[:ink.shape[0],:ink.shape[1]]
# Outer panel corner strokes lie beyond this circle and are excluded.
circle=((xx-ink.shape[1]*.5)/(ink.shape[1]*.48))**2+((yy-ink.shape[0]*.5)/(ink.shape[0]*.49))**2<=1
ink[~circle]=0; ink[gray<185]=0; ink[ink<8]=0
mask=Image.fromarray(ink.astype('uint8')).resize((160,160),Image.Resampling.LANCZOS)
im=Image.new('RGBA',(160,160),EDGE); im.putalpha(mask)
save('Dialogue_Watermark',im,p,method='source compass luminance extraction within circular mask; scene and panel corner excluded',recommended_runtime_alpha=0.12)

im=Image.new('RGBA',(120,3),'white')
save('Name_AccentLine',im,(102,460,88,3),method='neutral white strip for runtime character-color tint')
im=blank((14,14)); ImageDraw.Draw(im).polygon([(7,0),(13,7),(7,13),(0,7)],fill=CYAN)
save('Advance_Diamond',im,(805,505,10,11))
im=blank((48,2)); ImageDraw.Draw(im).line((0,0,47,0),fill=CYAN)
save('Advance_Line',im,(821,510,96,1))

im=blank((96,96)); d=ImageDraw.Draw(im)
pts=[(16,0),(95,0),(95,79),(79,95),(0,95),(0,16)]
d.line(pts+[pts[0]],fill=EDGE,width=1)
d.line((3,35,3,18,18,3,36,3),fill='#A2A7AE')
d.line((59,92,77,92,92,77,92,58),fill='#A2A7AE')
save('Portrait_Cell',im,(55,428,102,98),[19]*4,source=OVERLAY,
     method='empty clipped portrait border restored; transparent interior; portrait excluded')

def panel(name,size,p,state,source=STORY):
    w,h=size; fw,fh=w-4,h-4
    pts=[(16,0),(fw-12,0),(fw-1,11),(fw-1,fh-16),(fw-17,fh-1),(0,fh-1),(0,16)]
    im=blank(size); d=ImageDraw.Draw(im)
    d.polygon([(x+4,y+4) for x,y in pts],fill='#9AA3AD')
    mask=Image.new('L',(fw,fh)); ImageDraw.Draw(mask).polygon(pts,fill=255)
    im.paste(paper((fw,fh),state=='Disabled'),(0,0),mask)
    ImageDraw.Draw(im).line(pts+[pts[0]],fill=CYAN if state=='Hover' else '#A2A7AE' if state=='Disabled' else '#6B7078',width=1)
    save(name,im,p,[22]*4,source=source,state=state,shadow_offset=[4,4])
for state in ('Normal','Hover','Disabled'):
    panel('Choice_Panel_'+state,(760,72),(290,205 if state=='Hover' else 254,396,39),state)
for state in ('Normal','Hover'):
    panel('Answer_Panel_'+state,(640,60),(586,395,329,38),state,OVERLAY)
im=blank((20,20)); d=ImageDraw.Draw(im)
d.line((2,10,17,10),fill=EDGE); d.line((11,4,17,10,11,16),fill=EDGE)
save('Icon_Arrow',im,(664,267,10,15),method='shared20px arrow restored from source direction indicator')
im=blank((18,2)); ImageDraw.Draw(im).line((0,0,17,0),fill=EDGE)
save('NumberUnderline',im,(317,279,12,1),method='shared18px underline')

manifest=dict(version=1,source_canvas=[1920,1080],design_canvas=[1920,1080],assets=entries,
    coordinates='Top-left origin. source_crop_xywh: screenshot reference/texture extraction area; output_size: actual normalized PNG dimensions.',
    placements={'Dialogue_Box':[0,780],'Choice_1':[580,420],'Choice_2':[580,504],
                'Answer_Panel':[1180,712],'Advance_Diamond':[1700,1010],'Advance_Line':[1722,1016],
                'Dialogue_Watermark':[1740,900],'Portrait_Cell':[120,830]},
    dialogue_alpha='RGBA straight alpha: rows0..119 linearly0..255, rows120..299 fully255; RGB paper remains independent of source background.',
    states='Disabled uses gray paper; gray runtime text/icon separately. Hover uses cyan1px outline.',
    excluded=['all text/numbers','characters','portrait art','scene/shop/background pixels'],
    dialogue_slicing='Horizontal only, height300 fixed. L128 B0 R160 T0. Do not vertically stretch the120px ramp.')
for e in entries:
    if e['name'] in manifest['placements']: e['placement_1920_xy']=manifest['placements'][e['name']]
(OUT/'layout.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')

sheet=Image.new('RGB',(1200,math.ceil(len(entries)/4)*160),'#515963'); d=ImageDraw.Draw(sheet)
font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',13)
for i,e in enumerate(entries):
    x=i%4*300; y=i//4*160
    for yy in range(y+25,y+160,12):
        for xx in range(x,x+300,12): d.rectangle((xx,yy,xx+11,yy+11),fill='#747E88' if ((xx-x)//12+(yy-y-25)//12)%2 else '#A2ABB4')
    d.text((x+6,y+5),e['name'],font=font,fill='white')
    im=Image.open(OUT/e['file']); im.thumbnail((280,120),Image.Resampling.LANCZOS)
    sheet.paste(im,(x+(300-im.width)//2,y+30+(120-im.height)//2),im)
sheet.save(OUT/'Contact_Sheet.jpg',quality=93)

readme='''# 대화 UI 분절

정본: Dialogue_Story_Mockup.png / Dialogue_Overlay_Mockup.png. 글자·번호·캐릭터·초상·배경·상점 화면은 포함하지 않았습니다.

## 제작 방식

판은 정본 하단의 깨끗한 여백에서 추출한 종이 질감을 반전 반복하고 #F1F2F4로 보정해 복원했습니다. 단순 사각 크롭이 아니며 원본 배경이 보이던 부분도 종이 RGB로 채웠습니다. 판 윤곽·끝 장식·칸은 요청 규격으로 재구성했습니다. 워터마크는 원본 나침반 밝기 마스크로 추출했습니다.

## 알파

Dialogue_Box는1920×300 RGBA입니다. y0 알파0에서 y119 알파255까지 선형 변화하고, y120~299는 전부255입니다. 장식에도 같은 알파를 적용했습니다. RGB를 배경과 섞어 굽지 않은 straight alpha PNG이며 런타임에서 실제 장면과 합성합니다. 8비트 PNG의 정상적인256단계 정밀도입니다. 하단은 불투명 종이입니다.

나침반은 선 가장자리를 살리는 가변 알파입니다. 전체 알파0.12부터 조절하십시오. 나머지 판·칸·기호 알파는0/255입니다. Portrait_Cell의 가운데는 초상을 얹기 위한 투명 영역입니다.

## 조립

- Dialogue_Box: 왼쪽 마름모·양 끝 모서리 장식 포함, 나침반 제외.
- Dialogue_Watermark:160×160 독립 나침반. 원본의 미세한 질감이 일부 남을 수 있습니다.
- Name_AccentLine:120×3 **흰색**, 캐릭터 색으로 tint합니다.
- Advance_Diamond14×14 + Advance_Line48×2: 청록 넘기기 표시. 깜빡임은 게임에서 처리합니다.
- Portrait_Cell96×96: 원본의 초상 크기와 달리 요청 규격으로 정규화했습니다. 초상 그림은 별도로 올립니다.
- Choice_Panel760×72: Normal/Hover/Disabled. Answer_Panel640×60: Normal/Hover. 모든 글자·숫자·화살표를 제외했습니다.
- Icon_Arrow20×20, NumberUnderline18×2: 선택지·답변 공용. Disabled에서는 게임에서 회색으로 tint합니다.

## 좌표·9-slice

layout.json의 source_crop_xywh는1920×1080 원본에서 참고한 영역이고 output_size는 출력 크기입니다. 원본 목업과 요청 규격이 다르므로 placements에 구현용 요청 좌표를 별도로 기록했습니다. 모든 좌표는 좌상단 기준입니다.

nine_slice_lbrt는 Left, Bottom, Right, Top 순서입니다. Dialogue_Box는128,0,160,0이며 **높이300 고정, 가로만** 늘립니다. 위120px 그라데이션을 세로로 늘리지 마십시오. Choice/Answer는22px, 초상 칸은19px 경계입니다. 나침반·밑줄·기호는 Simple로 사용합니다.

## 재생성·검증

python ArtDirection/DialogueMockup/Split_Handoff/split_dialogue.py

PNG 크기, 크롭 범위, 그라데이션 행별 알파·단조성, 하단 불투명, 다른 판의0/255 알파를 자동 검증합니다. Contact_Sheet.jpg는1200px 폭 한 장입니다. 원본 및 Unity importer/.meta는 수정하지 않았습니다.
'''
(OUT/'README.md').write_text(readme,encoding='utf-8')
for e in entries:
    im=Image.open(OUT/e['file']); assert list(im.size)==e['output_size']
    x,y,w,h=e['source_crop_xywh']; assert 0<=x<x+w<=1920 and 0<=y<y+h<=1080,e['name']
    assert im.getchannel('A').getbbox(),e['name']
    if e['name'] not in ('Dialogue_Box','Dialogue_Watermark'):
        assert set(im.getchannel('A').get_flattened_data())<={0,255},e['name']
aa=np.asarray(Image.open(OUT/'Dialogue_Box.png'))[:,:,3]
assert np.all(aa[0]==0) and np.all(aa[119:]==255)
assert np.all(np.diff(aa[:,0].astype(int))>=0)
assert np.all(aa[:120]==np.rint(np.linspace(0,255,120)).astype('uint8')[:,None])
print(f'Validated {len(entries)} sprites,120px alpha ramp and opaque lower band; sheet {sheet.size}.')
