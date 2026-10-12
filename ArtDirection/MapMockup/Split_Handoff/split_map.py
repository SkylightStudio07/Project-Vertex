"""Map UI restoration and generated terrain export. No original screenshots displayed."""
from pathlib import Path
import json, math
import numpy as np
from PIL import Image, ImageDraw, ImageFont
ROOT=Path(__file__).resolve().parent.parent
OUT=ROOT/'Extracted'; OUT.mkdir(exist_ok=True)
SOURCE='Map_Mockup_v1.png'; BOSS='Map_Mockup_v1_Boss.png'
src=Image.open(ROOT/SOURCE).convert('RGB'); assert src.size==(1920,1080)
entries=[]; PAPER=(238,240,242); INK='#16181B'; EDGE='#2A2C30'; CYAN='#0DB8F2'
def rect(p): return [round(v*2) for v in p]
def save(name,im,p=None,border=None,source=SOURCE,method='source-guided restoration; typography, icons and terrain removed',**kw):
    im.save(OUT/(name+'.png'))
    entries.append(dict(name=name,file=name+'.png',source=source,source_crop_xywh=rect(p) if p else None,
        output_size=list(im.size),placement_1920_xy=rect(p)[:2] if p else None,
        nine_slice_lbrt=border or [0]*4,method=method,**kw))
def blank(s): return Image.new('RGBA',s,(0,0,0,0))
sample=[350,91,70,21]; x,y,w,h=rect(sample)
a=np.asarray(src.crop((x,y,x+w,y+h)),dtype=float)
g=np.clip(a-a.mean(axis=(0,1)),-4,4)
g=np.concatenate([g,g[:,::-1]],1); g=np.concatenate([g,g[::-1]],0)
def paper(s,gray=False):
    w,h=s; gh,gw=g.shape[:2]
    base=np.array([217,220,223] if gray else PAPER)
    a=np.tile(g,(math.ceil(h/gh),math.ceil(w/gw),1))[:h,:w]
    return Image.fromarray(np.uint8(np.clip(base+a,0,255))).convert('RGBA')
def points(w,h,cut=14): return [(cut,0),(w-1,0),(w-1,h-cut-1),(w-cut-1,h-1),(0,h-1),(0,cut)]
def plus(d,x,y,c=EDGE):
    d.line((x-5,y,x+5,y),fill=c); d.line((x,y-5,x,y+5),fill=c)

im=blank((1848,860)); pts=points(1844,856,32); d=ImageDraw.Draw(im)
d.polygon([(x+4,y+4) for x,y in pts],fill='#C9CCD1')
mask=Image.new('L',(1844,856)); ImageDraw.Draw(mask).polygon(pts,fill=255)
im.paste(paper((1844,856)),(0,0),mask); d=ImageDraw.Draw(im); d.line(pts+[pts[0]],fill=EDGE)
for yy in (100,728): d.line((36,yy,1810,yy),fill='#A2A7AE')
for xx,yy in [(46,18),(46,106),(1807,18),(1807,728)]: plus(d,xx,yy,'#8A9099')
save('Map_Panel',im,(16,77,930,450),[48,132,48,116],texture_sample_xywh=rect(sample),
     normalized_layout={'header_height':100,'scroll_top':128,'scroll_height':600,'legend_top':748})

for state,col in [('Past',EDGE),('Current',CYAN),('Future','#A2A7AE'),('Boss',EDGE)]:
    im=blank((200,28)); d=ImageDraw.Draw(im)
    if state=='Current': d.rectangle((34,0,165,21),fill=CYAN)
    d.line((0,21,199,21),fill=col); d.line((100,22,100,27),fill=col)
    save('Map_FloorTick_'+state,im,(366 if state=='Current' else 41,130,67,21),[0,0,0,0],state=state)
im=blank((200,2)); ImageDraw.Draw(im).line((0,0,199,0),fill='#8A9099')
save('Map_FloorRuler',im,(42,149,100,1),[1,0,1,0])
im=paper((1760,54)); d=ImageDraw.Draw(im)
for xx in range(220,1760,220): d.line((xx,7,xx,46),fill='#A2A7AE')
save('Map_LegendBar',im,(40,468,883,28),[8,8,8,8],note='8 equal legend sections; stretching also moves internal separators.')
for name,size,col,p in [('Map_ScrollBar_Track',(250,6),'#B5BBC1',(399,509,125,3)),('Map_ScrollBar_Handle',(52,6),'#5A6069',(450,509,26,3))]:
    im=Image.new('RGBA',size,col); save(name,im,p,[2,2,2,2])

def node(kind,state,sz,p):
    boss=kind=='Boss'; w=sz; h=sz+24 if boss else sz
    im=blank((w,h)); d=ImageDraw.Draw(im)
    dark=kind in ('Elite','Boss')
    pts=points(w-8 if boss else w,sz-8 if boss else sz,7)
    off=4 if boss else 0; pts=[(x+off,y+off) for x,y in pts]
    color=('#292D32' if state in ('Locked','Visited') else INK) if dark else None
    mask=Image.new('L',(w,h)); ImageDraw.Draw(mask).polygon(pts,fill=255)
    tex=Image.new('RGBA',(w,h),color) if dark else paper((w,h),state=='Locked')
    im.paste(tex,(0,0),mask); d=ImageDraw.Draw(im)
    d.line(pts+[pts[0]],fill='#9299A1' if state in ('Locked','Visited') else EDGE)
    if boss:
        d.rectangle((0,0,w-1,sz-1),outline=EDGE)
        d.rectangle((8,sz+4,w-9,h-1),fill=INK)
    if kind=='Rest': d.rectangle((7,sz-4,w-8,sz-3),fill=CYAN)
    if state in ('Accessible','Current'):
        for x,y,sx,sy in [(1,1,1,1),(w-2,1,-1,1),(1,sz-2,1,-1),(w-2,sz-2,-1,-1)]:
            d.line((x+sx*14,y,x,y,x,y+sy*14),fill=CYAN,width=2)
    save('Node_Frame_'+kind+'_'+state,im,p,[10,10 if not boss else 28,10,10],source=BOSS if boss else SOURCE,
         state=state,node_frame_size=[sz,sz],label_band_height=24 if boss else 0,
         visited_note='Add separate Node_VisitedCheck at lower-right.' if state=='Visited' else None)
for kind,sz,p,states in [
    ('Normal',76,(171,247,40,39),['Locked','Accessible','Visited','Current']),
    ('Rest',76,(776,188,42,41),['Locked','Accessible','Visited']),
    ('Elite',84,(775,405,43,43),['Locked','Accessible','Visited']),
    ('Boss',128,(790,264,66,83),['Locked','Accessible'])]:
    for state in states: node(kind,state,sz,p)
im=blank((76,76)); ImageDraw.Draw(im).ellipse((1,1,74,74),outline=EDGE,width=1)
save('Node_Frame_Blessing',im,(62,296,47,47),node_frame_size=[76,76])
im=blank((24,24)); ImageDraw.Draw(im).polygon([(12,1),(23,12),(12,23),(1,12)],fill=CYAN)
save('Node_HereMarker',im,(394,165,11,12))
im=blank((16,16)); d=ImageDraw.Draw(im); d.ellipse((0,0,15,15),fill=INK); d.line((4,8,7,11,12,5),fill=CYAN)
save('Node_VisitedCheck',im,(202,277,12,12))
im=blank((48,20)); ImageDraw.Draw(im).polygon([(5,0),(47,0),(47,16),(42,19),(0,19),(0,5)],fill=INK)
save('Node_QuestTag',im,(503,246,25,10),[6,5,6,5])
im=blank((92,92)); d=ImageDraw.Draw(im)
for start,end in [(5,78),(100,170),(190,260),(280,350)]: d.arc((0,0,91,91),start,end,fill=CYAN,width=1)
save('Node_AccessibleRing',im,(477,185,49,48),method='optional detached open arc from accessible node; resize per node frame')
for name in ('Line_Solid','Line_Dashed'):
    im=blank((64,4)); d=ImageDraw.Draw(im)
    if name.endswith('Solid'): d.line((0,1,63,1),fill=EDGE,width=2)
    else:
        # 16px repeat: 10px dash,6px gap; half coverage on second row approximates1.5px.
        for xx in range(0,64,16):
            d.line((xx,1,xx+9,1),fill=EDGE)
            d.line((xx,2,xx+9,2),fill=(42,44,48,128))
    save(name,im,(400,208,32,2),[1,0,1,0] if name.endswith('Solid') else [0]*4,
         note='Tint cyan at runtime. Dashed: tile; do not stretch dash spacing.')
for state in ('Normal','Hover','Pressed'):
    im=paper((40,40),state=='Pressed'); ImageDraw.Draw(im).rectangle((0,0,39,39),outline=CYAN if state=='Hover' else '#A2A7AE')
    save('Button_Close_'+state,im,(910,81,27,27),[3]*4,state=state)
im=blank((20,20)); d=ImageDraw.Draw(im); d.line((4,4,15,15),fill=EDGE); d.line((15,4,4,15),fill=EDGE)
save('Icon_Close',im,(918,88,12,12))
im=blank((12,16)); ImageDraw.Draw(im).polygon([(1,1),(10,8),(1,14)],fill=EDGE)
save('Icon_Triangle',im,(891,108,6,9))

# Generated terrain: preserve circular landmark scale instead of stretching it with the panorama.
GEN=Path('C:/Users/SKYLIGHT/.codex/generated_images/01a0f18c-4ca7-7b52-aa4a-7f5c739283c4/exec-075634a7-093d-403e-a314-617c08ac13ea.png')
raw=Image.open(GEN).convert('RGB'); rw,rh=raw.size
terrain=Image.new('RGB',(3320,600),PAPER)
body=raw.crop((round(rw*.055),round(rh*.14),round(rw*.77),round(rh*.86))).resize((2900,600),Image.Resampling.LANCZOS)
terrain.paste(body,(160,0))
landmark=raw.crop((round(rw*.77),round(rh*.14),rw,round(rh*.86)))
landmark.thumbnail((260,300),Image.Resampling.LANCZOS)
terrain.paste(landmark,(3060+(260-landmark.width)//2,(600-landmark.height)//2))
a=np.asarray(terrain,dtype=float); base=np.array(PAPER,dtype=float)
lum=a.mean(axis=2); darkness=np.clip((238-lum)/70,0,1)
strength=np.full((600,3320),.08); strength[:,3060:]=.12
# Fade each pasted region to paper at joins; keep start160px completely empty.
fade=np.ones((600,3320)); fade[:,:160]=0
fade[:,160:210]=np.linspace(0,1,50); fade[:,3010:3060]=np.linspace(1,0,50)
fade[:,3060:3080]=np.linspace(0,1,20); fade[:,3300:]=np.linspace(1,0,20)
darkness*=fade
out=base[None,None,:]*(1-darkness[:,:,None]*strength[:,:,None])
water=np.clip((a[:,:,2]-a[:,:,0]-3)/12,0,1)*fade
watercolor=np.array([220,230,238],dtype=float)
out=out*(1-water[:,:,None]) + watercolor*water[:,:,None]
terrain=Image.fromarray(np.uint8(np.clip(out,0,255)),'RGB').convert('RGBA')
save('Terrain_Act1',terrain,None,source=str(GEN),method='new image_gen terrain; panoramic body and aspect-preserved boss landmark composed; palette/contrast normalized',
     placement_1920_xy_override=[36,236],terrain_size=[3320,600],background='#EEF0F2',start_blank_x=[0,160],boss_zone_x=[3060,3320])
entries[-1]['placement_1920_xy']=[36,236]
manifest=dict(version=1,source_canvas=[1920,1080],design_canvas=[1920,1080],assets=entries,
    coordinate_convention='Top-left origin; source_crop_xywh are screenshot reference rectangles; output sizes normalized to layout spec.',
    map_panel_placement=[36,120],scroll_viewport=[36,236,1848,600],terrain_scroll_size=[3320,600],
    terrain_background='#EEF0F2 opaque',floor_spacing=200,
    node_sizes={'Normal':[76,76],'Rest':[76,76],'Elite':[84,84],'Boss':[128,128],'Boss_with_label':[128,152],'Blessing':[76,76]},
    excluded=['all typography/numbers','all node pictograms','HUD','source terrain from UI sprites'],
    layering=['Map_Panel','Terrain_Act1 clipped to scroll viewport','connections','node frames','existing game icons','state markers','runtime labels','legend','close'])
next(e for e in entries if e['name']=='Map_Panel')['placement_1920_xy']=[36,120]
(OUT/'layout.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')

cols=4; ch=116; rows=math.ceil((len(entries)-1)/cols)
sheet=Image.new('RGB',(1200,rows*ch+260),'#59616B'); d=ImageDraw.Draw(sheet)
font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',12)
for i,e in enumerate(entries[:-1]):
    x=i%4*300; y=i//4*ch
    for yy in range(y+22,y+ch,12):
        for xx in range(x,x+300,12): d.rectangle((xx,yy,xx+11,yy+11),fill='#CCD0D4' if ((xx-x)//12+(yy-y-22)//12)%2 else '#ECEEF0')
    d.text((x+5,y+4),e['name'],font=font,fill='white')
    im=Image.open(OUT/e['file']); im.thumbnail((280,84),Image.Resampling.LANCZOS)
    sheet.paste(im,(x+(300-im.width)//2,y+25+(84-im.height)//2),im)
d.text((10,rows*ch+5),'Terrain_Act1 / 3320 x 600 / opaque #EEF0F2',font=font,fill='white')
preview=terrain.resize((1180,213),Image.Resampling.LANCZOS); sheet.paste(preview,(10,rows*ch+30),preview)
sheet.save(OUT/'Contact_Sheet.jpg',quality=93)

readme='''# 맵 v1 분절 + 1막 지형

Map_Mockup_v1.png / Map_Mockup_v1_Boss.png를 기준으로 UI를 복원했습니다. 원본 전체 PNG를 화면에 띄우지 않고 미리보기2장과 스크립트로 작업했습니다.

## 제작 방식

판은 깨끗한 제목 줄 여백의 종이 질감을 추출해 반전 반복하고 #EEF0F2로 보정한 복원판입니다. 단순 사각 크롭이 아니며 글자·노드 아이콘·지형 흔적은 없습니다. 원본의 윤곽과 표시를 기준으로 노드 상태·선·작은 기호를 규격에 맞게 재구성했습니다. 판/노드 알파는0/255이며 바깥 그림자는 불투명입니다.

Terrain_Act1은 내장 imagegen으로 새로 생성했습니다. **3320×600, 불투명 종이색 #EEF0F2 배경**을 선택했습니다. 생성 결과의 본문을 가로형으로 편집하고, 오른쪽 원형 광장은 종횡비를 보존해 보스 구역에 배치했습니다. x0~160은 비어 있고, 보스 광장은x3060~3320에 있습니다. 본문 명도차 최대8%, 보스 구역 최대12%로 보정했으며 물색은 #DCE6EE로 제한했습니다. 노드·연결선·글자는 없습니다. 게임에서는 가로로 늘리지 말고 원래 크기로 스크롤 영역에 클리핑합니다.

## 조립

- Map_Panel: 제목 글자 없는 판, 가로선·모서리+ 포함. 지도 영역은 빈 종이입니다.
- FloorTick: 200×28, Current만 가운데 청록 칸. 번호는 게임에서 올립니다. FloorRuler는 반복 가로선입니다.
- LegendBar: 아이콘·글자 없이 구분선7개. 공통 폭1760 기준8개 항목입니다. 심하게 늘리면 구분선 위치도 움직이므로 지정 폭을 권장합니다.
- ScrollBar_Track/Handle: 분리된 트랙과 손잡이. 스크롤에 맞춰 손잡이를 이동합니다.
- Normal76×76: Locked/Accessible/Visited/Current. Rest76×76 및 Elite84×84: Locked/Accessible/Visited. Boss는128×128 칸+24px 아래 이름띠(파일128×152)입니다.
- Accessible은 청록 꺾쇠입니다. Node_AccessibleRing을 추가 레이어로 맥동할 수 있습니다. Rest 아래 청록 띠는 상태와 관계없이 안전 지점을 나타냅니다.
- Visited에는 Node_VisitedCheck를 별도로 얹습니다. Current에는 Node_HereMarker와 런타임 HERE 글자를 얹습니다. Node_QuestTag는 글자 없는 먹색 꼬리표입니다.
- 기존 Assets/Art/Maps/Nodes의 아이콘은 프레임 안에 게임에서 배치합니다. 이번 작업에서 해당 자산을 수정하지 않았습니다.
- Line_Solid는2px 실선입니다. Line_Dashed는10px 대시+6px 공백을 반복하며 두 번째 픽셀 행을50% 알파로 하여1.5px를 근사합니다. 점선은 늘리지 말고 타일링합니다. 지나온 길은 게임에서 청록으로 tint합니다.
- Button_Close 판과 Icon_Close를 따로 제공합니다. Icon_Triangle은 남은 층 라벨 옆에 사용합니다.

## 좌표·9-slice

layout.json의 source_crop_xywh는1920×1080 정본의 참고 영역, placement_1920_xy는 화면 배치, output_size는 실제 파일 크기입니다. 정본과 요청 규격 사이 차이가 있어 출력은 요청 규격으로 정규화했습니다. Map_Panel은(36,120), 지형 뷰포트는(36,236)1848×600입니다. 개별 프레임의 source_crop은 형태 참고이며 아이콘을 실제로 복사한 영역은 아닙니다.

nine_slice_lbrt 순서는 Left, Bottom, Right, Top(출력 픽셀)입니다. 패널 내부 가로선·범례 구분선 때문에 정해진 크기 사용을 권장합니다. 프레임·기호·층 눈금·지형은 지정 크기를 사용하고 크기 변경 시 모서리/선 두께를 확인하십시오. 지형 source_crop은null이며 출처는 생성 파일입니다.

## 재생성·확인

python ArtDirection/MapMockup/Split_Handoff/split_map.py

Contact_Sheet.jpg는1200px 폭 한 장입니다. 규격·알파·크롭 범위·지형 시작 여백을 검증했습니다. 원본 목업 및 Unity importer/.meta는 변경하지 않았습니다.
'''
(OUT/'README.md').write_text(readme,encoding='utf-8')
for e in entries:
    im=Image.open(OUT/e['file']); assert list(im.size)==e['output_size']
    assert im.getchannel('A').getbbox(),e['name']
    if e['name']!='Line_Dashed': assert set(im.getchannel('A').get_flattened_data())<={0,255},e['name']
    if e['source_crop_xywh']:
        x,y,w,h=e['source_crop_xywh']; assert 0<=x<x+w<=1920 and 0<=y<y+h<=1080,e['name']
assert np.all(np.asarray(terrain)[:,:160,:3]==np.array(PAPER))
print(f'Validated {len(entries)} PNGs including terrain; sheet {sheet.size}.')
