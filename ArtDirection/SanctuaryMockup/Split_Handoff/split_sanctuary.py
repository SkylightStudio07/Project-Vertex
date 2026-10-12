from pathlib import Path
import math,json
import numpy as np
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parent.parent
OUT=ROOT/'Extracted'; OUT.mkdir(exist_ok=True)
SEL='Sanctuary_Select_Mockup.png'; DET='Sanctuary_Detail_Mockup.png'
src={n:Image.open(ROOT/n).convert('RGB') for n in (SEL,DET)}
assert all(i.size==(1920,1080) for i in src.values())
INK='#16181B'; EDGE='#2A2C30'; CYAN='#0DB8F2'; entries=[]
def rect(p): return [round(v*2) for v in p]
def blank(s): return Image.new('RGBA',s,(0,0,0,0))
def save(n,im,p,border=None,source=SEL,method='source-guided geometry and paper restoration; text and artwork removed',**kw):
 im.save(OUT/(n+'.png')); entries.append(dict(name=n,file=n+'.png',source=source,source_crop_xywh=rect(p),output_size=list(im.size),placement_1920_xy=rect(p)[:2],nine_slice_lbrt=border or [0]*4,method=method,**kw))
patch=(366,22,80,24); x,y,w,h=rect(patch)
a=np.asarray(src[SEL].crop((x,y,x+w,y+h)),dtype=float); g=np.clip(a-a.mean(axis=(0,1)),-4,4)
g=np.concatenate((g,g[:,::-1]),1);g=np.concatenate((g,g[::-1]),0)
def paper(s,dark=False):
 w,h=s; gh,gw=g.shape[:2]; base=np.array([218,221,224] if dark else [238,240,242])
 return Image.fromarray(np.uint8(np.clip(base+np.tile(g,(math.ceil(h/gh),math.ceil(w/gw),1))[:h,:w],0,255))).convert('RGBA')
def plus(d,x,y,col='#A2A7AE',r=6):
 d.line((x-r,y,x+r,y),fill=col);d.line((x,y-r,x,y+r),fill=col)
im=paper((1920,1080));d=ImageDraw.Draw(im)
for x in range(52,1920,48): d.line((x,108,x,1055),fill='#E5E8EB')
for y in range(120,1080,48):d.line((30,y,1890,y),fill='#E5E8EB')
for k in range(-1000,1920,350):d.line((k,1080,k+800,108),fill='#E1E5E8')
for x in (52,1872):
 d.line((x,106,x,1050),fill='#BFC5CB')
 for y in (120,256,528,800,992):plus(d,x,y)
for y in (120,992):d.line((30,y,1890,y),fill='#BFC5CB')
for x in range(105,1820,135):d.line((x,990,x,998),fill='#B5BCC3')
save('Sanctuary_Background',im,(0,0,960,540),texture_sample_xywh=rect(patch),note='Top106px paper only; reconstructed clean grid, no UI/typography.')
im=blank((820,820));d=ImageDraw.Draw(im);cx=cy=410
for r in (338,380):d.ellipse((cx-r,cy-r,cx+r,cy+r),outline='#ADB5BD')
for t in range(0,360,2):
 a=math.radians(t);r=354;l=17 if t%10==0 else 7
 d.line((cx+math.cos(a)*r,cy+math.sin(a)*r,cx+math.cos(a)*(r+l),cy+math.sin(a)*(r+l)),fill='#ADB5BD')
for t in (35,140):
 a=math.radians(t);d.line((cx-math.cos(a)*405,cy-math.sin(a)*405,cx+math.cos(a)*405,cy+math.sin(a)*405),fill='#CBD0D5')
save('Detail_ArtBackdrop',im,(443,76,410,384),source=DET,recommended_runtime_alpha=.22)

# Canonical second strip: top x497..722, bottom x314..544, y130..470.
size=(816,680);pts=[(366,0),(815,0),(460,679),(0,679)]
mask=Image.new('L',size);ImageDraw.Draw(mask).polygon(pts,fill=255)
for state in ('Normal','Hover','Selected','Locked'):
 im=blank(size);d=ImageDraw.Draw(im);color=CYAN if state in ('Hover','Selected') else '#9299A1' if state=='Locked' else EDGE
 d.line(pts+[pts[0]],fill=color,width=2 if state in ('Hover','Selected') else 1)
 if state=='Selected':d.line((1,677,460,677),fill=CYAN,width=5)
 save('Strip_Frame_'+state,im,(314,130,408,340),state=state,note='Border only; use common mask beneath.')
for state in ('Normal','Locked'):
 im=paper(size,state=='Locked');im.putalpha(mask)
 save('Strip_Fill_'+state,im,(314,130,408,340),state=state)
im=Image.new('RGBA',size,'white');im.putalpha(mask);save('Strip_Mask',im,(314,130,408,340))
for state in ('Normal','Locked'):
 im=blank(size);d=ImageDraw.Draw(im)
 # Same full canvas as mask eliminates alignment ambiguity.
 d.polygon([(81,530),(538,530),(460,679),(0,679)],fill=INK if state=='Normal' else '#303338')
 im.putalpha(Image.fromarray(np.minimum(np.asarray(im.getchannel('A')),np.asarray(mask))))
 save('Strip_NameBand_'+state,im,(314,130,408,340),state=state,note='Full strip canvas; name band begins local y530.')
im=blank((18,2));ImageDraw.Draw(im).line((0,0,17,0),fill=EDGE);save('Strip_NumberUnderline',im,(502,149,9,1))
im=blank((40,52));d=ImageDraw.Draw(im)
d.arc((9,1,31,31),180,360,fill='#6B7078',width=4);d.line((9,15,9,24),fill='#6B7078',width=4);d.line((31,15,31,24),fill='#6B7078',width=4)
d.rectangle((3,22,37,50),fill='#6B7078');d.ellipse((17,31,23,37),fill='#EEF0F2');d.rectangle((19,35,21,42),fill='#EEF0F2')
save('Icon_Lock',im,(511,269,28,40))

def panel(n,s,p,state='Normal',dark=False,source=DET,accent=False):
 w,h=s;fw,fh=w-4,h-4;cut=28 if h>100 else 22
 pts=[(cut,0),(fw-1,0),(fw-1,fh-cut-1),(fw-cut-1,fh-1),(0,fh-1),(0,cut)]
 im=blank(s);d=ImageDraw.Draw(im);d.polygon([(x+4,y+4) for x,y in pts],fill='#C9CCD1')
 m=Image.new('L',(fw,fh));ImageDraw.Draw(m).polygon(pts,fill=255)
 tex=Image.new('RGBA',(fw,fh),'#34383D' if state=='Disabled' else INK) if dark else paper((fw,fh),state=='Disabled')
 im.paste(tex,(0,0),m);d=ImageDraw.Draw(im)
 d.line(pts+[pts[0]],fill=CYAN if state=='Hover' else '#A2A7AE' if state=='Disabled' else EDGE,width=1)
 if accent:d.line((1,fh-2,fw-cut-1,fh-2),fill='#A2A7AE' if state=='Disabled' else CYAN,width=2)
 if state=='Pressed':d.line((12,fh-6,fw-cut-8,fh-6),fill='#8A9099',width=2)
 save(n,im,p,[cut+6]*4,source=source,state=state,shadow_offset=[4,4])
for s in ('Normal','Hover','Pressed','Disabled'):panel('Button_Detail_'+s,(416,114),(737,462,208,57),s,source=SEL,accent=True)
panel('Detail_InfoPanel',(806,604),(45,159,403,302))
im=blank((710,2));d=ImageDraw.Draw(im);d.line((0,0,15,0),fill='#6B7078');d.line((150,0,709,0),fill='#A2A7AE')
save('Detail_SectionRule',im,(72,181,354,2),source=DET,label_gap=[20,145])
im=paper((732,156));d=ImageDraw.Draw(im)
pts=[(18,0),(713,0),(731,17),(731,137),(713,155),(0,155),(0,18)]
m=Image.new('L',im.size);ImageDraw.Draw(m).polygon(pts,fill=255);im.putalpha(m);d.line(pts+[pts[0]],fill='#A2A7AE')
d.rectangle((20,18,225,129),fill=(0,0,0,0))
save('Detail_CardSlot',im,(62,324,366,78),[20]*4,source=DET,art_window_xywh=[20,18,206,112],note='Fixed size recommended: internal transparent art opening must keep its shape.')
for state in ('Empty','Filled'):
 im=blank((42,36));d=ImageDraw.Draw(im);d.rectangle((0,0,41,35),outline='#A2A7AE')
 if state=='Filled':d.rectangle((3,3,38,32),fill='#5A6069')
 save('Detail_AffinityCell_'+state,im,(145,258,21,18),source=DET)
for s in ('Normal','Hover','Pressed'):panel('Button_Back_'+s,(326,94),(25,470,163,47),s)
for s in ('Normal','Hover','Pressed','Disabled'):panel('Button_Join_'+s,(332,100),(771,469,166,50),s,True)
im=Image.new('RGBA',(120,3),'white');save('Name_AccentLine',im,(60,143,84,3),source=DET)
for n,back in [('Icon_Arrow',False),('Icon_ArrowBack',True)]:
 im=blank((20,20));d=ImageDraw.Draw(im);d.line((2,10,17,10),fill=EDGE);d.line((11,4,17,10,11,16) if not back else (8,4,2,10,8,16),fill=EDGE)
 save(n,im,(875,484,19,15) if not back else (55,487,20,14),source=DET,note='Tint light on ink button.')

strips=[]
for i,x in enumerate((142,626,1106)):
 strips.append(dict(index=i+1,position_xy=[x,260],size=list(size),state='Hover' if i==0 else 'Locked',
  text_local={'number':[376,16],'name':[94,554],'affiliation_or_unlock':[94,620],'name_accent':[94,605]},
  text_screen={'number':[x+376,276],'name':[x+94,814],'affiliation_or_unlock':[x+94,880]},lock_local=[398,278]))
manifest=dict(version=1,source_canvas=[1920,1080],design_canvas=[1920,1080],assets=entries,strip_instances=strips,
 coordinate_convention='Top-left origin; source_crop_xywh in canonical screenshot pixels; output_size is native PNG size.',
 strip_geometry={'vertices':pts if False else [[366,0],[815,0],[460,679],[0,679]],'reference':'Second canonical strip; common normalized shape replaces inconsistent first/third shapes.',
 'original_strip_reference_xywh':[[64,260,894,680],[628,260,816,680],[1112,260,780,680]]},
 detail_text_anchors={'number':[126,168],'name':[118,206],'affiliation':[440,246],'profile_label':[176,356],'profile_body':[142,396],
 'affinity_label':[176,478],'affinity_level':[158,518],'card_label':[176,608],'card_name':[382,668],'card_cost':[382,716],'card_description':[382,752],'reward_label':[176,818],'reward_summary':[144,866]},
 detail_affinity_instances=[[290+54*i,516] for i in range(5)],
 excluded=['all typography','characters','locked silhouettes','card art'],
 layering=['background','strip fill','game character masked by Strip_Mask','strip name band','strip frame','lock and runtime text'])
(OUT/'layout.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
sheet=Image.new('RGB',(1200,math.ceil(len(entries)/4)*140),'#535D67');d=ImageDraw.Draw(sheet);font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',12)
for i,e in enumerate(entries):
 x=i%4*300;y=i//4*140
 for yy in range(y+24,y+140,12):
  for xx in range(x,x+300,12):d.rectangle((xx,yy,xx+11,yy+11),fill='#BCC3CA' if ((xx-x)//12+(yy-y-24)//12)%2 else '#E3E7EA')
 d.text((x+5,y+5),e['name'],font=font,fill='white');im=Image.open(OUT/e['file']);im.thumbnail((280,108),Image.Resampling.LANCZOS)
 sheet.paste(im,(x+(300-im.width)//2,y+27+(108-im.height)//2),im)
sheet.save(OUT/'Contact_Sheet.jpg',quality=93)
readme='''# 성소 분절

정본 선택/상세 목업을 기준으로 글자·캐릭터·실루엣·카드 아트를 제외한 조각을 복원했습니다. 배경과 판은 깨끗한 종이 여백 질감으로 채웠으며 격자·눈금·모서리·기호는 원본 형태를 따라 재구성했습니다. 단순 사각 크롭은 아닙니다. 원본 파일은 변경하지 않았습니다.

## 후보 띠

Strip_Frame은 테두리만, Strip_Fill은 종이 면, Strip_Mask는 흰색 마스크입니다. 세 조각은816×680 동일 캔버스·꼭짓점을 사용합니다. Strip_NameBand도 같은 캔버스이며 위530px은 투명, 아래 이름 띠만 남겼습니다. 동일 위치에 겹치면 맞습니다. 프레임 테두리 때문에 Fill/Mask의 외곽과 프레임 외곽은 일치합니다. 마스크 적용은 게임에서 합니다.

생성 목업의01띠는02·03과 폭·기울기가 서로 다릅니다. 요청한 공통 조각을 위해 **02번 정본 띠**의 윤곽을 기준으로 통일했습니다. 따라서01 원본 윤곽과는 차이가 있습니다. layout.json에 원본 참고 영역과 통일한3개 배치, 번호·이름·소속/해금 조건의 로컬/화면 좌표를 모두 기록했습니다. 우측 마지막2px은 화면 경계에서 클리핑됩니다. 실제 구현에서는 끝 띠를2px 왼쪽으로 이동해도 됩니다.

Locked에도 실루엣을 넣지 않았습니다. 게임의 캐릭터 그림을 회색으로 바꿔 동일 마스크로 표시합니다. 잠금 기호는 별도 Icon_Lock입니다. Selected는 청록 테두리와 아래 굵은 선, Hover는 청록 테두리입니다.

## 상세·공통

Sanctuary_Background는1920×1080 공용 종이 격자 배경이며 상단106px은 장식 없이 비웠습니다. Detail_ArtBackdrop은 투명 원형 눈금, 게임에서 알파0.22 정도부터 조절합니다.

Detail_InfoPanel에는 라벨·내용·구분선이 없습니다. Detail_SectionRule은 글자 자리를 비운 줄표와 긴 선입니다. Detail_CardSlot은 아트 창이 투명하며 art_window_xywh에 영역을 기록했습니다. 카드 칸은 지정 크기 사용을 권장합니다. AffinityCell은 Empty/Filled 분리입니다.

Button_Detail은 아래 청록 선 포함4상태, Back3상태, Join4상태입니다. 화살표는 별도이며 먹색 합류 버튼 위에서는 흰색 tint합니다. Name_AccentLine은120×3 흰색입니다.

## 좌표·9-slice

source_crop_xywh는1920×1080 정본 참고 영역, placement_1920_xy는 좌상단 배치, output_size는 실제 크기입니다. 이번 요청의 정본 크기를 따라 상세 정보판806×604, Detail버튼416×114, Back326×94, Join332×100으로 출력했습니다. 이전 초안 문서 치수와 다릅니다.

nine_slice_lbrt 순서는Left,Bottom,Right,Top입니다. 사선 후보 띠·마스크·이름 띠는 **Simple, 비율 고정**으로 사용하고9-slice하지 않습니다. 정보판·버튼은 모서리 여백이 지정되어 있습니다. 원형 눈금과 기호도Simple로 사용합니다.

## 확인·재생성

python ArtDirection/SanctuaryMockup/Split_Handoff/split_sanctuary.py

Contact_Sheet.jpg는1200px 폭 한 장입니다. 파일 크기·원본 참고 범위·알파0/255·공통 띠 마스크 일치·아트 창 투명을 자동 검증합니다. Unity importer/.meta는 변경하지 않았습니다.
'''
(OUT/'README.md').write_text(readme,encoding='utf-8')
for e in entries:
 im=Image.open(OUT/e['file']);assert list(im.size)==e['output_size'];assert set(im.getchannel('A').get_flattened_data())<={0,255},e['name']
 x,y,w,h=e['source_crop_xywh'];assert 0<=x<x+w<=1920 and 0<=y<y+h<=1080,e['name']
for n in ('Strip_Fill_Normal','Strip_Fill_Locked'):
 assert np.array_equal(np.asarray(Image.open(OUT/(n+'.png')))[:,:,3],np.asarray(mask))
assert np.all(np.asarray(Image.open(OUT/'Detail_CardSlot.png'))[18:130,20:226,3]==0)
print(f'Validated {len(entries)} PNGs; sheet {sheet.size}; masks and artwork window verified.')
