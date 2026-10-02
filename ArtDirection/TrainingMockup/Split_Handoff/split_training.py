from pathlib import Path
import json,math
import numpy as np
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parent.parent
OUT=ROOT/'Extracted';OUT.mkdir(exist_ok=True)
MAIN='Training_Main_Mockup.png';DECK='Training_Deck_Mockup.png'
src={n:Image.open(ROOT/n).convert('RGB') for n in (MAIN,DECK)}
assert all(i.size==(1672,941) for i in src.values())
entries=[];INK='#16181B';EDGE='#3A3E44';CYAN='#0DB8F2'
def r(p):return [round(p[0]*1672/960),round(p[1]*941/540),round(p[2]*1672/960),round(p[3]*941/540)]
def xy(p):return [round(p[0]*1672/960),round(p[1]*941/540)]
def blank(s):return Image.new('RGBA',s,(0,0,0,0))
def save(n,im,p,source=MAIN,border=None,**kw):
 im.save(OUT/(n+'.png'));entries.append(dict(name=n,file=n+'.png',source=source,source_crop_xywh=r(p),output_size=list(im.size),placement_1672_xy=r(p)[:2],nine_slice_lbrt=border or [0]*4,method='source-guided restoration; source paper texture; typography and artwork removed',**kw))
patch=(352,18,64,20);x,y,w,h=r(patch);a=np.asarray(src[MAIN].crop((x,y,x+w,y+h)),dtype=float)
g=np.clip(a-a.mean(axis=(0,1)),-4,4);g=np.concatenate((g,g[:,::-1]),1);g=np.concatenate((g,g[::-1]),0)
def paper(s,gray=False):
 w,h=s;gh,gw=g.shape[:2];base=np.array([217,220,223] if gray else [238,240,242])
 return Image.fromarray(np.uint8(np.clip(base+np.tile(g,(math.ceil(h/gh),math.ceil(w/gw),1))[:h,:w],0,255))).convert('RGBA')
def linep(d,p,q,col='#BBC1C7',width=1):d.line((*xy(p),*xy(q)),fill=col,width=width)
def plus(d,p):
 x,y=xy(p);d.line((x-5,y,x+5,y),fill='#8A9099');d.line((x,y-5,x,y+5),fill='#8A9099')
for name,source in [('Base_Main',MAIN),('Base_Deck',DECK)]:
 im=src[source].convert('RGBA');mask=Image.new('L',im.size)
 pts=[xy(p) for p in [(88,0),(960,0),(912,48),(912,482),(835,540),(0,540),(0,445),(48,397),(48,38)]]
 ImageDraw.Draw(mask).polygon(pts,fill=255);im.paste(paper(im.size),(0,0),mask);d=ImageDraw.Draw(im)
 for k in range(90,860,220):linep(d,(k,520),(min(k+390,910),5),'#E2E6E9')
 linep(d,(120,14),(120,54),'#6B7078')
 if source==MAIN:
  for x in (324,637):linep(d,(x,80),(x,520))
  for p,q in [((61,80),(912,80)),((62,116),(305,116)),((339,348),(633,348)),((340,376),(632,376)),((659,116),(897,116))]:linep(d,p,q)
 else:
  for x in (190,650):linep(d,(x,96),(x,520))
  for p,q in [((50,96),(913,96)),((65,140),(180,140)),((65,263),(180,263)),((65,334),(180,334)),((209,135),(633,135)),((670,138),(904,138))]:linep(d,p,q)
 for p in [(119,69),(21,20),(21,519),(939,20),(912,76),(324 if source==MAIN else 190,519)]:plus(d,p)
 save(name,im,(0,0,960,540),source,texture_sample_xywh=r(patch),note='Original outer lobby pixels retained; interior restored with no list rows/buttons/art.')
im=blank((500,500));d=ImageDraw.Draw(im)
for rr in (160,206):d.ellipse((250-rr,250-rr,250+rr,250+rr),outline='#A2A7AE',width=2)
for t in range(0,360,15):
 a=math.radians(t);d.line((250+math.cos(a)*212,250+math.sin(a)*212,250+math.cos(a)*228,250+math.sin(a)*228),fill='#A2A7AE')
d.line((0,250,499,250),fill='#BBC1C7');d.line((250,0,250,499),fill='#BBC1C7')
save('Target_Backdrop',im,(341,44,283,259),recommended_runtime_alpha=.18)

def size(p):return tuple(r(p)[2:])
def panel(n,p,state='Normal',source=MAIN,dark=False,accent=False,thumb=None,portrait=None,text=None,repeat=None):
 w,h=size(p);im=blank((w,h));pts=[(12,0),(w-1,0),(w-1,h-14),(w-15,h-1),(0,h-1),(0,12)]
 mask=Image.new('L',(w,h));ImageDraw.Draw(mask).polygon(pts,fill=255)
 tex=Image.new('RGBA',(w,h),'#4A5058' if state=='Disabled' else INK) if dark else paper((w,h),state in ('Locked','Disabled'))
 im.paste(tex,(0,0),mask);d=ImageDraw.Draw(im)
 d.line(pts+[pts[0]],fill=CYAN if state in ('Hover','Selected') else '#B6BDC4',width=1)
 if accent:d.line((1,h-4,w-17,h-4),fill='#A2A7AE' if state=='Disabled' else CYAN,width=3)
 if state=='Selected':d.rectangle((0,1,4,h-2),fill=CYAN)
 if state=='Pressed':d.line((12,h-8,w-21,h-8),fill='#8A9099',width=2)
 if thumb:
  x,y,tw,th=thumb;d.rectangle((x,y,x+tw-1,y+th-1),fill='#DDE1E5',outline='#B6BDC4')
 if portrait:
  x,y,pw,ph=portrait;d.rectangle((x,y,x+pw-1,y+ph-1),fill=(0,0,0,0));d.rectangle((x-1,y-1,x+pw,y+ph),outline='#A2A7AE')
 save(n,im,p,source,[16]*4,state=state,text_slots_local=text or {},repeat_step=repeat,portrait_window=portrait)

tp=(60,178,246,66);tw,th=size(tp)
for s in ('Normal','Hover','Selected','Locked'):
 panel('TargetRow_'+s,tp,s,thumb=[10,10,140,94],text={'name':[174,14],'type_badge':[174,51],'defeat_pips':[251,65],'right_progress':[tw-92,th-25]},repeat=[0,round(67*941/540)])
for s,col in [('Normal',None),('Elite','#3A3E44'),('Boss',INK)]:
 im=Image.new('RGBA',(56,26),col) if col else paper((56,26));ImageDraw.Draw(im).rectangle((0,0,55,25),outline='#8A9099')
 save('TypeBadge_'+s,im,(162,210,30,13),border=[2]*4,text_slots_local={'label':[7,4]})
for s in ('Filled','Empty'):
 im=blank((16,16));d=ImageDraw.Draw(im);pts=[(8,1),(15,8),(8,15),(1,8)]
 d.polygon(pts,fill=CYAN if s=='Filled' else None,outline=CYAN if s=='Filled' else '#8A9099')
 save('DefeatPip_'+s,im,(202,147,9,9),repeat_step=[24,0])
im=blank((28,34));d=ImageDraw.Draw(im);d.arc((6,0,22,21),180,360,fill=INK,width=3);d.line((6,10,6,16),fill=INK,width=3);d.line((22,10,22,16),fill=INK,width=3);d.rectangle((2,14,25,32),fill=INK);d.ellipse((11,19,17,25),fill='#EEF0F2')
save('Icon_Lock',im,(473,431,15,21),note='Tint light on dark silhouette.')
for n,s,col in [('Track',(4,674),'#D0D5DA'),('Handle',(4,88),'#9299A1')]:save('Scrollbar_'+n,Image.new('RGBA',s,col),(312,122,3,386 if n=='Track' else 51),border=[1]*4)
cp=(338,380,93,127);cw,ch=size(cp)
for s in ('Normal','Hover','Selected','Locked'):
 panel('CompanionCard_'+s,cp,s,accent=s=='Selected',portrait=[6,5,cw-12,150],text={'name':[cw//2,ch-46],'level':[cw//2,ch-23],'lock_condition':[cw//2,ch-40]},repeat=[round(98*1672/960),0])
save('CompanionTag',Image.new('RGBA',(62,22),CYAN),(340,460,34,14),border=[2]*4,text_slots_local={'label':[8,3]})
for s in ('Normal','Hover'):
 panel('DeckRow_'+s,(660,116,237,49),s,thumb=[15,12,58,58],text={'name':[126,30],'count':[358,30]},repeat=[0,round(49*941/540)])
for s in ('Normal','Hover','Pressed'):panel('Button_DeckEdit_'+s,(659,367,238,43),s,text={'label':[144,23],'arrow':[278,29]})
for s in ('Normal','Hover','Pressed','Disabled'):panel('Button_Sortie_'+s,(648,435,263,73),s,dark=True,accent=True,text={'small_label':[47,24],'label':[47,54],'arrow':[157,67]})
im=blank((24,24));d=ImageDraw.Draw(im);d.line((2,12,21,12),fill='white',width=2);d.line((14,5,21,12,14,19),fill='white',width=2);save('Icon_Arrow',im,(739,469,24,16))
for s in ('Normal','Hover','Selected'):panel('CostChip_'+s,(66,294,24,26),s,DECK,text={'value':[12,10]},repeat=[round(29*1672/960),0])
save('CountBadge',Image.new('RGBA',(57,38),CYAN),(276,143,33,22),DECK,[3]*4,text_slots_local={'quantity':[12,9]})
for s in ('Normal','Hover'):panel('EditRow_'+s,(670,139,235,50),s,DECK,thumb=[10,12,60,60],text={'name':[100,30],'count':[327,31],'minus_position':[254,23],'plus_position':[362,23]},repeat=[0,round(53*941/540)])
for op in ('Minus','Plus'):
 for s in ('Normal','Hover','Pressed','Disabled'):
  im=paper((42,42),s in ('Pressed','Disabled'));d=ImageDraw.Draw(im);d.rectangle((0,0,41,41),outline=CYAN if s=='Hover' else '#A2A7AE');col='#A2A7AE' if s=='Disabled' else EDGE
  d.line((12,21,30,21),fill=col,width=2)
  if op=='Plus':d.line((21,12,21,30),fill=col,width=2)
  save('Button_'+op+'_'+s,im,(817 if op=='Minus' else 879,151,24,25),DECK,[3]*4,state=s)
for s in ('Normal','Hover','Pressed'):panel('Button_Done_'+s,(663,440,249,70),s,DECK,dark=True,accent=True,text={'label':[35,36],'arrow':[150,49]})

reuse={
'Back':['../LoadoutMockup/Extracted/Back_Normal.png','../LoadoutMockup/Extracted/Back_Normal_Hover.png','../LoadoutMockup/Extracted/Icon_BackArrow.png'],
'Checkbox':['../ArmoryMockup/CardCatalogExtracted/Checkbox_Unchecked.png','../ArmoryMockup/CardCatalogExtracted/Checkbox_Selected.png','../ArmoryMockup/CardCatalogExtracted/Checkbox_Unchecked_Hover.png','../ArmoryMockup/CardCatalogExtracted/Checkbox_Selected_Hover.png'],
'Cards':['../ArmoryMockup/CardCatalogExtracted/'+n+'.png' for n in ['Card_Attack_NoArt','Card_RewardLocked','Lock_Crop','Energy_Cyan','Energy_Gray']]}
for paths in reuse.values():
 for path in paths:assert (ROOT/path).exists(),path
manifest=dict(version=1,canvas=[1672,941],assets=entries,reuse=reuse,coordinate_convention='Top-left, native1672x941; local text slots relative to sprite top-left.',
 sections_main={k:xy(v) for k,v in {'title':(134,18),'subtitle':(134,50),'target_title':(64,94),'target_count':(274,94),'target_name':(339,292),'analysis_text':(398,325),'companion_title':(340,360),'companion_count':(592,360),'deck_title':(660,94),'deck_count':(871,94)}.items()},
 sections_deck={k:xy(v) for k,v in {'title':(135,17),'filter':(67,113),'library':(211,113),'library_count':(539,115),'deck':(671,113),'deck_count':(872,115)}.items()},
 repeats={'target_rows':{'origin':xy((60,115)),'step':[0,round(67*941/540)],'count':6},'companions':{'origin':xy((338,380)),'step':[round(98*1672/960),0],'count':3},
 'deck_summary':{'origin':xy((660,116)),'step':[0,round(49*941/540)],'count':5},'deck_edit':{'origin':xy((670,139)),'step':[0,round(53*941/540)],'count':5},
 'library_grid':{'origin':xy((211,140)),'cell_size':size((0,0,101,172)),'column_step':round(107*1672/960),'row_step':round(178*941/540),'columns':4},
 'cost_chips':{'origin':xy((66,294)),'step':[round(29*1672/960),0],'count':4}},
 analysis_badge='omitted: analysis completed/defeats is text only; boss badge uses TypeBadge_Boss',
 excluded=['all text/numbers','enemies','companions','silhouettes','card artwork','deck row pictograms'],
 text_alignment='Companion name/level anchors are centered; other local slots are top-left. Name/count text is runtime only.')
(OUT/'layout.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
sheet=Image.new('RGB',(1200,math.ceil(len(entries)/4)*125),'#535D67');d=ImageDraw.Draw(sheet);font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',12)
for i,e in enumerate(entries):
 x=i%4*300;y=i//4*125
 for yy in range(y+24,y+125,12):
  for xx in range(x,x+300,12):d.rectangle((xx,yy,xx+11,yy+11),fill='#BBC3CB' if ((xx-x)//12+(yy-y-24)//12)%2 else '#E2E7EB')
 d.text((x+5,y+5),e['name'],font=font,fill='white');im=Image.open(OUT/e['file']);im.thumbnail((280,95),Image.Resampling.LANCZOS);sheet.paste(im,(x+(300-im.width)//2,y+27+(95-im.height)//2),im)
sheet.save(OUT/'Contact_Sheet.jpg',quality=93)
readme='''# 훈련장 분절

정본 두 장1672×941 기준입니다. 글자·숫자·썸네일·큰 적 그림·초상·실루엣·카드 아트·덱 아이콘은 제외했습니다. 판 내부는 원본의 깨끗한 머리 여백 종이 질감으로 복원했으며, 상태/기호/테두리는 원본을 참고해 재구성했습니다. 단순 크롭이 아닙니다. Base는 바깥 로비 배경을 보존하고 내부 판을 복원했습니다. 모든 PNG 알파는0/255입니다.

## 재사용 (이번에 출력하지 않음)

- 뒤로 버튼: ArtDirection/LoadoutMockup/Extracted/Back_Normal.png, Back_Normal_Hover.png, Icon_BackArrow.png.
- 필터 체크박스: ArtDirection/ArmoryMockup/CardCatalogExtracted/Checkbox_Unchecked, Checkbox_Selected 및 각_Hover.png.
- 카드 틀·잠긴 카드·에너지: 같은 CardCatalogExtracted의 Card_Attack_NoArt.png, Card_RewardLocked.png, Lock_Crop.png, Energy_Cyan.png, Energy_Gray.png.

파일 존재를 확인했습니다. 목업 카드에는 위 에너지 표기보다 오른쪽 수량 배지가 강조되어 있으며 기존 카드 틀과 세부 테두리/크기 차이가 있습니다. 기존 틀을 재사용하고 CountBadge를 추가합니다. 카드4열 배치 간격과 출력 칸 크기는 JSON library_grid에 기록했습니다. 그림은 기존 카드 아트입니다.

## 조각

Base_Main/Deck에는 구역 판·머리 장식·구분선만 있고 목록/버튼/초상/카드는 없습니다. Target_Backdrop은 분리된 과녁이며 전체 알파0.18 정도부터 조절합니다.

TargetRow는 빈 썸네일 바탕 포함4상태, TypeBadge는3종류, DefeatPip은Filled/Empty입니다. 상태별 크기와 로컬 좌표는 동일합니다. Selected는청록 테두리+왼쪽 띠입니다. 자물쇠와 스크롤은 별도입니다. AnalysisBadge는 원본에서 별도 판 없이 글자뿐이므로 생략했습니다. 옆 보스 칸은 TypeBadge_Boss를 사용합니다.

CompanionCard는4상태이며 초상 창은 완전 투명입니다. 선택 시 아래 청록 선을 포함하고 CompanionTag는 별도로 올립니다. 잠긴 실루엣은 게임에서 실제 그림을 회색으로 표시합니다.

DeckRow/EditRow는 카드 그림을 넣을 빈 아이콘 바탕 포함2상태입니다. EditRow의−/+기호·버튼은 포함하지 않았으며 Button_Minus/Plus를 별도로 올립니다. 이 작은 버튼만 기호를 포함하고 각각4상태입니다.

Button_DeckEdit3상태, Sortie4상태, Done3상태입니다. Sortie/Done은먹색+아래 청록 선입니다. Icon_Arrow는**흰색**이며 종이 버튼 위에서는 먹색 tint합니다. CostChip3상태와 CountBadge는숫자 없는 칸입니다.

## 좌표·크기

layout.json은 **1672×941, 좌상단 기준**입니다. source_crop_xywh는 원본 참고 영역, placement_1672_xy는 배치, output_size는 PNG크기입니다. text_slots_local는각 조각 안 글자/배지/점/수치/버튼 위치이며 sections_main/deck에는섹션 제목·개수, repeats에는목록·동료·카드 격자·코스트 칩 반복 간격이 있습니다. 줄의 실제 게임 텍스트 크기에 맞춰 마지막 정렬을 조정하십시오.

nine_slice_lbrt 순서는Left,Bottom,Right,Top입니다. 내부 썸네일/초상 창이 있는 줄·카드는지정 크기 사용을 권장합니다. 틀을 크게 늘리면 내부 창도 늘어날 수 있습니다. 과녁·기호·Base는Simple입니다. 원본 목업 치수에 맞춘 출력이며 초안 레이아웃 문서 치수와 다를 수 있습니다.

## 검증·재생성

python ArtDirection/TrainingMockup/Split_Handoff/split_training.py

Contact_Sheet.jpg 한 장(1200px 폭)에 모든 새 조각이 있습니다. 규격·알파·원본참고범위·초상투명·상태별크기를 검사합니다. Unity importer/.meta 및원본은 변경하지 않았습니다.
'''
(OUT/'README.md').write_text(readme,encoding='utf-8')
for e in entries:
 im=Image.open(OUT/e['file']);assert list(im.size)==e['output_size'];assert set(im.getchannel('A').get_flattened_data())<={0,255},e['name']
 x,y,w,h=e['source_crop_xywh'];assert 0<=x<x+w<=1672 and 0<=y<y+h<=941,e['name']
 if e.get('portrait_window'):
  x,y,w,h=e['portrait_window'];assert np.all(np.asarray(im)[y:y+h,x:x+w,3]==0)
for prefix in ['TargetRow','CompanionCard','DeckRow','EditRow','Button_Sortie','Button_Done','Button_Minus','Button_Plus','CostChip']:
 assert len({tuple(e['output_size']) for e in entries if e['name'].startswith(prefix+'_')})==1
print(f'Validated {len(entries)} PNGs, reused files, portrait alpha and state sizes; sheet {sheet.size}.')
