from pathlib import Path
import json,math
import numpy as np
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parent.parent
OUT=ROOT/'Extracted_v2';OUT.mkdir(exist_ok=True)
COMP='Training_Companion_Mockup.png';MAIN='Training_Main_Mockup_v2.png'
src={n:Image.open(ROOT/n).convert('RGB') for n in (COMP,MAIN)}
assert all(i.size==(1672,941) for i in src.values())
E=[];CYAN='#0DB8F2';EDGE='#6B7078'
def rect(p):return [round(p[0]*1672/960),round(p[1]*941/540),round(p[2]*1672/960),round(p[3]*941/540)]
def xy(p):return rect((*p,0,0))[:2]
def blank(s):return Image.new('RGBA',s,(0,0,0,0))
def save(n,im,p,source=COMP,border=None,**kw):
 im.save(OUT/(n+'.png'));E.append(dict(name=n,file=n+'.png',source=source,source_crop_xywh=rect(p),output_size=list(im.size),placement_1672_xy=rect(p)[:2],nine_slice_lbrt=border or [0]*4,method='source-guided geometry and source paper texture restoration; text/art removed',**kw))
sample=(365,20,58,20);x,y,w,h=rect(sample);a=np.asarray(src[COMP].crop((x,y,x+w,y+h)),dtype=float)
g=np.clip(a-a.mean(axis=(0,1)),-4,4);g=np.concatenate((g,g[:,::-1]),1);g=np.concatenate((g,g[::-1]),0)
def paper(s,gray=False):
 w,h=s;gh,gw=g.shape[:2];base=np.array([217,220,223] if gray else [238,240,242])
 return Image.fromarray(np.uint8(np.clip(base+np.tile(g,(math.ceil(h/gh),math.ceil(w/gw),1))[:h,:w],0,255))).convert('RGBA')
im=src[COMP].convert('RGBA');m=Image.new('L',im.size)
pts=[xy(p) for p in [(88,0),(960,0),(916,49),(916,480),(875,522),(0,540),(0,445),(50,396),(50,37)]]
ImageDraw.Draw(m).polygon(pts,fill=255);im.paste(paper(im.size),(0,0),m);d=ImageDraw.Draw(im)
def rule(p,q,c='#B6BDC4'):d.line((*xy(p),*xy(q)),fill=c)
rule((120,14),(120,54),EDGE)
for k in (80,330,630):rule((k,520),(min(k+390,915),4),'#E2E6E9')
for x in (180,650):rule((x,96),(x,520))
for p,q in [((50,96),(918,96)),((64,140),(170,140)),((64,218),(170,218)),((657,312),(917,312)),((669,135),(907,135))]:rule(p,q)
for x,y in [(21,20),(21,519),(939,20),(180,96),(650,96),(650,519)]:
 xx,yy=xy((x,y));d.line((xx-5,yy,xx+5,yy),fill=EDGE);d.line((xx,yy-5,xx,yy+5),fill=EDGE)
save('Base_Companion',im,(0,0,960,540),texture_sample_xywh=rect(sample),note='Original outer lobby scenery retained; all interior content restored.')

def dashed(d,box,col):
 x0,y0,x1,y1=box
 for x in range(x0,x1,10):
  d.line((x,y0,min(x+5,x1),y0),fill=col);d.line((x,y1,min(x+5,x1),y1),fill=col)
 for y in range(y0,y1,10):
  d.line((x0,y,x0,min(y+5,y1)),fill=col);d.line((x1,y,x1,min(y+5,y1)),fill=col)
def cell(n,size,p,state,window=None,source=COMP,text=None):
 w,h=size;im=paper(size,state=='Locked');d=ImageDraw.Draw(im)
 color=CYAN if state in ('Hover','Party') else '#B6BDC4'
 if state=='Empty' or n.startswith('CompanionSlot_Empty'):dashed(d,(2,2,w-3,h-3),color)
 else:d.rectangle((0,0,w-1,h-1),outline=color)
 if window:
  x,y,ww,hh=window;d.rectangle((x,y,x+ww-1,y+hh-1),fill=(0,0,0,0));d.rectangle((x-1,y-1,x+ww,y+hh),outline='#B6BDC4')
 save(n,im,p,source,[5]*4,state=state,portrait_window_xywh=window,text_slots_local=text or {})
portrait_size=tuple(rect((0,0,69,122))[2:]);pw,ph=portrait_size
for s in ('Normal','Hover','Party','Locked'):
 cell('PortraitCell_'+s,portrait_size,(198,140,69,122),s,[4,4,pw-8,137],text={'name':[8,154],'status':[8,181]})
save('PartyBadge',Image.new('RGBA',(54,34),CYAN),(236,141,31,19),border=[2]*4,text_slots_local={'label':[9,9]})
im=blank(tuple(rect((0,0,125,163))[2:]));d=ImageDraw.Draw(im);d.rectangle((0,0,im.width-1,im.height-1),outline='#A2A7AE')
save('DetailPortrait_Frame',im,(668,139,125,163),border=[2]*4,portrait_window_xywh=[1,1,im.width-2,im.height-2])
size=tuple(rect((0,0,107,46))[2:]);im=paper(size);d=ImageDraw.Draw(im);d.rectangle((0,0,im.width-1,im.height-1),outline='#BCC3CA');window=[4,4,66,70];d.rectangle((4,4,69,73),fill=(0,0,0,0));d.rectangle((3,3,70,74),outline='#A2A7AE')
save('DetailCardSlot',im,(800,256,107,46),border=[5]*4,art_window_xywh=window,text_slots_local={'card_name':[80,13],'energy_marker':[84,47]})
party_size=tuple(rect((0,0,76,94))[2:]);w,h=party_size
for s in ('Filled','Hover','Empty'):
 cell('PartySlot_'+s,party_size,(669,350,76,94),s,[6,6,w-12,h-38] if s!='Empty' else None,text={'name':[w//2,h-25],'plus':[w//2,h//2]})
for s in ('Normal','Hover','Pressed'):
 im=paper((28,28),s=='Pressed');d=ImageDraw.Draw(im);d.rectangle((0,0,27,27),outline=CYAN if s=='Hover' else '#8A9099');d.line((8,8,19,19),fill='#16181B',width=2);d.line((19,8,8,19),fill='#16181B',width=2)
 save('Button_Remove_'+s,im,(725,355,16,16),border=[3]*4,state=s)
im=blank((28,28));d=ImageDraw.Draw(im);d.line((3,14,25,14),fill=EDGE,width=2);d.line((14,3,14,25),fill=EDGE,width=2);save('Icon_Plus',im,(857,382,24,26))
main_size=Image.open(ROOT/'Extracted/CompanionCard_Normal.png').size
for s in ('Normal','Hover'):
 cell('CompanionSlot_Empty_'+s,main_size,(536,382,88,124),s,source=MAIN,text={'label':[main_size[0]//2,139],'plus':[main_size[0]//2,81]})
for s in ('Normal','Hover','Pressed'):
 size=tuple(rect((0,0,54,22))[2:]);im=paper(size,s=='Pressed');ImageDraw.Draw(im).rectangle((0,0,im.width-1,im.height-1),outline=CYAN if s=='Hover' else '#A2A7AE')
 save('Button_Formation_'+s,im,(573,355,54,22),MAIN,[3]*4,state=s,text_slots_local={'label':[16,8],'arrow':[70,12]})

reuse={
 'main_background':['../Extracted/Base_Main.png'],
 'main_companions':['../Extracted/CompanionCard_'+s+'.png' for s in ('Normal','Hover','Selected','Locked')]+['../Extracted/CompanionTag.png'],
 'done':['../Extracted/Button_Done_'+s+'.png' for s in ('Normal','Hover','Pressed')],
 'scroll':['../Extracted/Scrollbar_Track.png','../Extracted/Scrollbar_Handle.png'],
 'lock':['../Extracted/Icon_Lock.png'],
 'diamond':['../Extracted/DefeatPip_Filled.png'],
 'affinity':['../../SanctuaryMockup/Extracted/Detail_AffinityCell_'+s+'.png' for s in ('Empty','Filled')],
 'name_accent':['../../SanctuaryMockup/Extracted/Name_AccentLine.png'],
 'checkbox':['../../ArmoryMockup/CardCatalogExtracted/Checkbox_'+s+'.png' for s in ('Unchecked','Selected','Unchecked_Hover','Selected_Hover')],
 'back':['../../LoadoutMockup/Extracted/Back_Normal.png','../../LoadoutMockup/Extracted/Back_Normal_Hover.png','../../LoadoutMockup/Extracted/Icon_BackArrow.png'],
 'arrow':['../Extracted/Icon_Arrow.png']}
for paths in reuse.values():
 for p in paths:assert (OUT/p).exists(),p
manifest=dict(version=2,canvas=[1672,941],assets=E,reuse=reuse,
 coordinates='Top-left. Original crop/placement native1672x941. Windows/text slots local to output PNG unless marked screen.',
 repeats={'portrait_grid':{'origin':xy((198,140)),'cell_size':list(portrait_size),'columns':6,'column_step':round(73*1672/960),'row_step':round(128*941/540),'count':14},
 'party_slots':{'origin':xy((669,350)),'size':list(party_size),'column_step':round(81*1672/960),'count':3},
 'main_party_slots':{'origin':xy((338,380)),'size':list(main_size),'column_step':round(98*1672/960),'count':3}},
 text_screen_companion={k:xy(v) for k,v in {'title':(134,17),'filter':(67,111),'grid_title':(199,111),'grid_count':(512,113),'selected_title':(670,112),'name':(800,143),'affiliation':(817,167),'affinity':(801,201),'card_label':(801,245),'party_title':(670,327),'party_count':(875,328),'done':(685,469)}.items()},
 text_screen_main={k:xy(v) for k,v in {'party_title':(340,360),'party_count':(531,361),'formation':(584,361),'empty_label':(559,458)}.items()},
 detail_windows_screen={'portrait':rect((668,139,125,163)),'card_art':rect((802,258,38,40))},
 affinity_instances={'origin':xy((802,216)),'size':size if False else [33,26],'step':[38,0],'count':5},
 main_tag_policy='Use existing CompanionTag at lower-left on both filled cards; grid uses PartyBadge at upper-right only.',
 analysis={'Base_Main_v2':'not emitted: original main column geometry unchanged','AffinityCell':'reuse rectangular Sanctuary cells, displayed33x26','DetailPortrait_Frame':'separate visible border, transparent center'})
(OUT/'layout.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
sheet=Image.new('RGB',(1200,math.ceil(len(E)/4)*145),'#535D67');d=ImageDraw.Draw(sheet);font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',12)
for i,e in enumerate(E):
 x=i%4*300;y=i//4*145
 for yy in range(y+24,y+145,12):
  for xx in range(x,x+300,12):d.rectangle((xx,yy,xx+11,yy+11),fill='#BDC5CD' if ((xx-x)//12+(yy-y-24)//12)%2 else '#E3E7EB')
 d.text((x+5,y+5),e['name'],font=font,fill='white');im=Image.open(OUT/e['file']);im.thumbnail((280,112),Image.Resampling.LANCZOS);sheet.paste(im,(x+(300-im.width)//2,y+27+(112-im.height)//2),im)
sheet.save(OUT/'Contact_Sheet.jpg',quality=93)
readme='''# 훈련장 동료 편성 v2 분절

이번 결과는 Extracted_v2에만 저장했습니다. 1차 Extracted는 수정하거나 재생성하지 않았습니다. 판은 원본의 깨끗한 종이 여백 질감으로 복원하고 윤곽·상태를 재구성했습니다. 글자·초상·실루엣·카드 그림은 제외했습니다. 모든 알파는0/255입니다.

## 새 조각

Base_Companion은 오른쪽 선택/동행 구역과 필터 구성이 달라 새로 만들었습니다. 바깥 로비 배경은 원본을 유지하고 내부는 무문자 종이로 복원했습니다.

PortraitCell4상태는 초상 창이 투명하고 이름/상태 띠는 종이입니다. PartyBadge는 오른쪽 위에 얹는 글자 없는 청록 칸입니다. 큰 초상은 DetailPortrait_Frame과 기존 그림을 합칩니다. DetailCardSlot은 카드 아트 창이 투명하며 카드 이름/에너지 표시는 게임에서 얹습니다.

PartySlot Filled/Hover는 초상 창이 투명, Empty는 빈 종이+점선입니다. Button_Remove3상태는 요청대로×기호를 포함합니다. Icon_Plus는 별도입니다.

메인 CompanionSlot_Empty2상태는 기존 CompanionCard와 정확히 같은162×221입니다. +와 동료 편성 글자는 별도로 넣습니다. Button_Formation3상태에는 글자/화살표가 없습니다.

## 재사용 — 새로 출력하지 않음

- Extracted/Base_Main: 메인 동료 구역 경계가 동일하므로 재사용. Base_Main_v2 없음.
- Extracted/CompanionCard_* + CompanionTag: 메인의 두 동료 모두 왼쪽 아래 동일 위치에 동행 태그를 놓습니다. 목업의 이카루스 오른쪽 위 태그는 사용하지 않습니다.
- Extracted/Button_Done_*, Scrollbar_*, Icon_Lock, DefeatPip_Filled, Icon_Arrow.
- ArmoryMockup/CardCatalogExtracted/Checkbox_*.
- LoadoutMockup/Extracted/Back_Normal, Back_Normal_Hover, Icon_BackArrow.
- SanctuaryMockup/Extracted/Detail_AffinityCell_Empty/Filled: 같은 직사각 문법이라 재사용, 화면상33×26으로 표시합니다.
- SanctuaryMockup/Extracted/Name_AccentLine: 흰색을 캐릭터 색으로tint합니다.

layout.json의 reuse 경로는 이 Extracted_v2 폴더 기준이며 모두 존재를 확인했습니다. 기존 완료 버튼은 편성 화면 완료 위치(약1158,789)에 맞춰 배치하며 실제 목업과 작은 폭 차이는9-slice로 조절합니다.

## 조립·좌표

모든 배치는1672×941 좌상단 기준입니다. source_crop_xywh는 참고한 원본 영역, placement_1672_xy는 정본 배치, output_size는 출력 PNG 규격입니다. repeats에는6열14칸의 격자 간격과3개 동행 칸 간격을 기록했습니다. 초상 격자는14칸만 생성하고 나머지4자리는 빈 배경입니다.

portrait_window_xywh/art_window_xywh는 조각 내부의 투명 창입니다. text_slots_local는 이름/상태/동행 이름/버튼 글자 위치이며 text_screen_companion/main에는 섹션 제목·수치·상세 정보·완료 위치가 있습니다. 글자는 모두 런타임입니다. 큰 초상과 카드 창의 화면 영역도 별도로 기록했습니다.

nine_slice_lbrt는Left,Bottom,Right,Top입니다. 내부 투명 창이 있는 판은지정 크기 사용을 권장합니다. 9-slice로 전체를 늘리면 초상 창도 변하므로 필요하면 그림과 창 좌표를 함께 조절하십시오. 기호·배경은Simple로 사용합니다.

## 확인·재생성

python ArtDirection/TrainingMockup/Split_Handoff/split_training_v2.py

Contact_Sheet.jpg는1200px 폭 한 장입니다. 크기·크롭 범위·알파·투명 창·상태 동일 크기·재사용 파일 존재를 자동 확인합니다. Unity importer/.meta는 수정하지 않았습니다.
'''
(OUT/'README.md').write_text(readme,encoding='utf-8')
for e in E:
 im=Image.open(OUT/e['file']);assert list(im.size)==e['output_size'];assert set(im.getchannel('A').get_flattened_data())<={0,255},e['name']
 x,y,w,h=e['source_crop_xywh'];assert 0<=x<x+w<=1672 and 0<=y<y+h<=941,e['name']
 for field in ('portrait_window_xywh','art_window_xywh'):
  if e.get(field):
   x,y,w,h=e[field];assert np.all(np.asarray(im)[y:y+h,x:x+w,3]==0),e['name']
for prefix in ('PortraitCell','PartySlot','Button_Remove','CompanionSlot_Empty','Button_Formation'):
 assert len({tuple(e['output_size']) for e in E if e['name'].startswith(prefix+'_')})==1
print(f'Validated {len(E)} PNGs; transparent windows, alpha, state sizes and reuse files verified. Sheet{sheet.size}.')
