from pathlib import Path
import json, math
import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT=Path(__file__).resolve().parent.parent
OUT=ROOT/'Extracted'; OUT.mkdir(exist_ok=True)
SRC=ROOT/'Blessing_Mockup_v1_Canonical.png'
src=Image.open(SRC).convert('RGB'); assert src.size==(1672,941)
entries=[]
INK='#16181B'; EDGE='#2A2C30'; CYAN='#0DB8F2'
def rect(p): return [round(p[0]*1672/960),round(p[1]*941/540),round(p[2]*1672/960),round(p[3]*941/540)]
def crop(p):
    x,y,w,h=rect(p); return src.crop((x,y,x+w,y+h))
def blank(size): return Image.new('RGBA',size,(0,0,0,0))
def save(name,im,p,border=None,method='canonical contour restored with clean source paper grain; text and content removed',**kw):
    im.save(OUT/(name+'.png'))
    entries.append(dict(name=name,file=name+'.png',source=SRC.name,source_crop_xywh=rect(p),output_size=list(im.size),
        placement_1920_xy=[round(p[0]*2),round(p[1]*2)],nine_slice_lbrt=border or [0,0,0,0],method=method,**kw))

# Clean whitespace below dialogue text, above its lower border.
patch=(484,351,72,8)
a=np.asarray(crop(patch),dtype=np.int16)
grain=np.clip(a-a.mean(axis=(0,1)),-5,5)
grain=np.concatenate((grain,grain[:,::-1]),axis=1)
grain=np.concatenate((grain,grain[::-1]),axis=0)
def paper(size,disabled=False):
    w,h=size; gh,gw=grain.shape[:2]
    base=np.array([217,220,223] if disabled else [238,240,242])
    a=np.tile(grain,(math.ceil(h/gh),math.ceil(w/gw),1))[:h,:w]
    return Image.fromarray(np.uint8(np.clip(base+a,0,255))).convert('RGBA')
def poly(w,h,corner,c=18):
    if corner=='tl': return [(c,0),(w-1,0),(w-1,h-1),(0,h-1),(0,c)]
    if corner=='tr': return [(0,0),(w-c-1,0),(w-1,c),(w-1,h-1),(0,h-1)]
    return [(0,0),(w-1,0),(w-1,h-c-1),(w-c-1,h-1),(0,h-1)]
def plus(d,x,y,c=EDGE,r=7):
    d.line((x-r,y,x+r,y),fill=c); d.line((x,y-r,x,y+r),fill=c)
def panel(name,size,p,corner,state='Normal',dark=False,dialogue=False):
    w,h=size; fw,fh=w-4,h-4; pts=poly(fw,fh,corner)
    im=blank(size); d=ImageDraw.Draw(im)
    d.polygon([(x+4,y+4) for x,y in pts],fill='#9AA3AD')
    mask=Image.new('L',(fw,fh)); ImageDraw.Draw(mask).polygon(pts,fill=255)
    tex=Image.new('RGBA',(fw,fh),INK) if dark else paper((fw,fh),state=='Disabled')
    im.paste(tex,(0,0),mask); d=ImageDraw.Draw(im)
    d.line(pts+[pts[0]],fill=CYAN if state=='Hover' else '#9AA3AD' if state=='Disabled' else EDGE,width=1)
    if dialogue: d.line((36,31,fw-310,31),fill='#B6BBC0')
    if dark:
        plus(d,fw-35,26,'#8A9099'); plus(d,fw-20,fh-23,'#8A9099')
    save(name,im,p,[24,26,44 if dark else 26,36 if dialogue else 26],state=state,shadow_offset=[4,4],
         texture_sample_xywh=None if dark else rect(patch))
panel('Panel_Dialogue',(1158,140),(281,294,585,72),'tl',dialogue=True)
for shape,corner,p in [('A','tr',(282,450,580,36)),('B','br',(282,490,580,36))]:
    for state in ('Normal','Hover','Disabled'):
        panel('Panel_Option_'+shape+'_'+state,(1158,70),p,corner,state)
panel('Panel_Nameplate',(376,160),(24,443,188,80),'tr',dark=True)

for state in ('Normal','Hover','Disabled'):
    im=Image.new('RGBA',(56,56),INK) if state=='Hover' else paper((56,56),state=='Disabled')
    ImageDraw.Draw(im).rectangle((0,0,55,55),outline='#A2A7AE' if state=='Disabled' else EDGE)
    save('Option_IconCell_'+state,im,(325,372,30,31),[2]*4,state=state)
im=blank((48,64)); d=ImageDraw.Draw(im)
d.rectangle((0,0,41,63),outline='#A2A7AE'); d.line((47,0,47,63),fill='#8A9099')
save('Portrait_Cell',im,(298,319,32,35),[2]*4,method='empty portrait border and right divider; portrait omitted')

def line(name,size,p,points,color=EDGE,width=1):
    im=blank(size); ImageDraw.Draw(im).line(points,fill=color,width=width)
    save(name,im,p,method='source-guided clean line reconstruction')
line('Option_Divider',(2,40),(446,379,1,20),[(0,0),(0,39)],'#8A9099')
line('Nameplate_AccentLine',(60,2),(39,491,104,2),[(0,0),(59,0)],'#B3242B',2)
line('NumberUnderline',(18,2),(296,389,8,1),[(0,0),(17,0)])
im=blank((20,20)); d=ImageDraw.Draw(im)
d.line((2,10,17,10),fill=EDGE); d.line((11,4,17,10,11,16),fill=EDGE)
save('Icon_Arrow',im,(832,462,15,12),method='arrow outline normalized to shared 20px size')
im=blank((16,16)); plus(ImageDraw.Draw(im),8,8,'#8A9099',7)
save('Icon_Plus',im,(182,449,13,14),method='optional detached registration mark; nameplate already contains its marks')

# Luminance subtraction removes paper from the actual compass, retaining linework and shading.
p=(796,302,62,58); arr=np.asarray(crop(p).convert('L'),dtype=float)
bg=np.percentile(arr,90); alpha=np.clip((bg-arr-3)*3.5,0,255).astype('uint8')
alpha[alpha<12]=0
mask=Image.fromarray(alpha).resize((120,120),Image.Resampling.LANCZOS)
im=Image.new('RGBA',(120,120),EDGE); im.putalpha(mask)
save('Watermark_Compass',im,p,method='canonical compass luminance mask; paper removed; no replacement star',recommended_runtime_alpha=0.18)

im=blank((1920,1080)); d=ImageDraw.Draw(im)
plus(d,40,40,'#C9CED4',13); d.line((40,57,40,134),fill='#8A9099')
d.line((52,40,68,40),fill='#8A9099')
plus(d,1880,1040,'#C9CED4',10); d.line((1832,1040,1865,1040),fill='#8A9099')
d.line((1880,994,1880,1024),fill='#8A9099')
save('Corner_Marks',im,(0,0,960,540),method='edge registration lines restored; all microcopy omitted',source_regions_xywh=[rect((12,12,25,58)),rect((915,497,31,30))])

manifest=dict(version=1,source=SRC.name,source_canvas=[1672,941],design_canvas=[1920,1080],
    coordinates='top-left origin; source_crop_xywh in original PNG pixels; output_size and placement in design pixels',
    dimensions_note='Dialogue and nameplate heights follow canonical footprints; option panels normalized to1158x70; accent normalized to60x2.',
    assets=entries,
    option_instances=[dict(index=i+1,panel='Panel_Option_'+('A' if i%2==0 else 'B')+'_'+s,position=[564,740+80*i],
                           icon_cell='Option_IconCell_'+s) for i,s in enumerate(['Hover','Disabled','Normal','Normal'])],
    hover_runtime_offset=[-8,0],
    excluded=['top HUD slots','MAP/DECK','all text and numbers','option artwork','dialogue portrait'],
    nameplate_plus_note='Included per explicit asset list. Icon_Plus is supplied separately; avoid double placement.',
    states='Disabled uses gray paper; tint runtime text/icons separately. Hover moves left8px in-game.')
(OUT/'layout.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')

sheet=Image.new('RGB',(1200,math.ceil(len(entries)/4)*150),'#555B63'); d=ImageDraw.Draw(sheet)
font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',13)
for i,e in enumerate(entries):
    x=i%4*300; y=i//4*150
    for yy in range(y+25,y+150,12):
        for xx in range(x,x+300,12):
            d.rectangle((xx,yy,xx+11,yy+11),fill='#C2C6CC' if ((xx-x)//12+(yy-y-25)//12)%2 else '#E5E7EA')
    d.text((x+7,y+5),e['name'],font=font,fill='white')
    im=Image.open(OUT/e['file']); im.thumbnail((280,116),Image.Resampling.LANCZOS)
    sheet.paste(im,(x+(300-im.width)//2,y+28+(116-im.height)//2),im)
sheet.save(OUT/'Contact_Sheet.jpg',quality=92)

readme='''# 축복 v1 분절 결과

정본 Blessing_Mockup_v1_Canonical.png에서 제작했습니다. 원본을 화면에 열지 않고 축소본과 스크립트로 작업했습니다.

## 조각 및 조립

- Panel_Dialogue: 초상·대사·라벨·워터마크 없는 종이 판. 위쪽 가는 선 포함. Portrait_Cell과 Watermark_Compass를 별도 배치합니다.
- Panel_Option_A/B: 오른쪽 위/아래 사선 컷만 다른 공통 판. 각각 Normal/Hover/Disabled. 번호·제목·설명·선택지 아이콘·구분선·화살표는 없습니다.
- Option_IconCell: 56×56 빈 칸, Normal/Hover(먹색)/Disabled. 기존 게임 아이콘을 위에 올립니다. 대사 초상도 기존 게임 자산을 사용합니다.
- Option_Divider/NumberUnderline/Icon_Arrow: 별도 레이어. 상태에 따른 기호/글자 색은 게임에서 처리합니다.
- Panel_Nameplate: 먹색 판과 회색 그림자, 조각 목록에서 요구한 + 표시 포함. 이름·소개·라벨·붉은 선은 제외했습니다. Icon_Plus는 별도 활용용이며 이미 포함된 자리에 중복 배치하지 마십시오.
- Nameplate_AccentLine: 60×2 붉은 선. 원본 선보다 짧게 정규화했습니다.
- Corner_Marks: 1920×1080 투명 오버레이, 좌상단·우하단 선과 +만 포함. 마이크로 카피는 게임 글자입니다.
- HUD 아이템 칸, MAP/DECK, 캐릭터, 배경, 선택지 아이콘, 대사 초상은 결과에 포함하지 않았습니다.

## 복원 방식

단순 사각 크롭이 아닌 글자 없는 복원판입니다. 대사 판의 깨끗한 여백에서 추출한 종이 질감을 반전 반복해 글자·아이콘·장식 영역을 채우고 원본 윤곽에 맞춰 판과 불투명 그림자 띠를 복원했습니다. 종이는 #EEF0F2, 비활성은 #D9DCDF입니다. 판 알파는 0/255입니다. 상태 파생판과 작은 기호는 규격에 맞춰 재구성했습니다.

Watermark_Compass는 원본 나침반을 밝기 마스크로 추출했습니다. 종이 바탕을 제거하고 선화만 남겼으며 가변 알파입니다. 게임에서 전체 알파0.18부터 조절하십시오. 나침반 주변의 미세한 질감이 일부 남을 수 있습니다.

## 좌표·9-slice

layout.json: source_crop_xywh는 원본1672×941 픽셀 기준 참고/추출 영역, output_size는 PNG 크기, placement_1920_xy는 좌상단 원점 배치입니다. 옵션 판은1158×70으로 통일했습니다. 대사 판1158×140과 이름판376×160은 정본 외형에 맞춰 이전 레이아웃 문서와 치수가 다릅니다.

nine_slice_lbrt는 Left, Bottom, Right, Top 순서이며 출력 픽셀 단위입니다. 사선 컷·그림자·+를 보존하도록 경계를 잡았습니다. 칸은2px 경계이며, 기호·워터마크·오버레이는 Simple로 사용합니다. 옵션 Hover는 게임에서 x=-8px 이동합니다. JSON option_instances는 정본 기준이며 초안 레이아웃의 y값과 다를 수 있습니다.

## 검증·재생성

python ArtDirection/BlessingMockup/Split_Handoff/split_blessing.py

PNG 크기·원본 크롭 범위·판 알파를 자동 확인했습니다. Contact_Sheet.jpg 한 장(1200px 폭)에 전 조각이 있습니다. Unity importer/.meta 및 원본 파일은 수정하지 않았습니다.
'''
(OUT/'README.md').write_text(readme,encoding='utf-8')
for e in entries:
    im=Image.open(OUT/e['file']); assert list(im.size)==e['output_size']
    x,y,w,h=e['source_crop_xywh']; assert 0<=x<x+w<=1672 and 0<=y<y+h<=941,e['name']
    assert im.getchannel('A').getbbox(),e['name']
    if e['name']!='Watermark_Compass': assert set(im.getchannel('A').get_flattened_data())<={0,255},e['name']
print(f'Validated {len(entries)} sprites; contact sheet {sheet.size}.')
