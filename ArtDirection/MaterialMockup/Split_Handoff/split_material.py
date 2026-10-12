"""Textless Material archive export, native 1672x941 coordinates."""
from pathlib import Path
import math,json,hashlib
import numpy as np
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parent.parent
OUT=ROOT/'Extracted';OUT.mkdir(exist_ok=True)
ENEMY='Material_Enemy_Mockup.png';COMP='Material_Companion_Mockup.png'
sources={n:Image.open(ROOT/n).convert('RGB') for n in (ENEMY,COMP)}
assert all(im.size==(1672,941) for im in sources.values())
E=[];INK='#16181B';EDGE='#6B7078';CYAN='#0DB8F2'
def rect(p):return [round(p[0]*1672/960),round(p[1]*941/540),round(p[2]*1672/960),round(p[3]*941/540)]
def xy(p):return rect((*p,0,0))[:2]
def wh(p):return tuple(rect(p)[2:])
def blank(s):return Image.new('RGBA',s,(0,0,0,0))
def save(n,im,p,source=ENEMY,border=None,**kw):
 im.save(OUT/(n+'.png'))
 E.append(dict(name=n,file=n+'.png',source=source,source_crop_xywh=rect(p),output_size=list(im.size),placement_1672_xy=rect(p)[:2],nine_slice_lbrt=border or [0]*4,method='canonical-guided contour reconstruction with sampled paper grain; lettering and art removed',**kw))
sample=(355,19,60,22);x,y,w,h=rect(sample)
a=np.asarray(sources[ENEMY].crop((x,y,x+w,y+h)),dtype=float)
grain=np.clip(a-a.mean(axis=(0,1)),-3,3)
grain=np.concatenate((grain,grain[:,::-1]),1);grain=np.concatenate((grain,grain[::-1]),0)
def paper(size,gray=False):
 w,h=size;gh,gw=grain.shape[:2];base=np.array([217,220,223] if gray else [238,240,242])
 a=base+np.tile(grain,(math.ceil(h/gh),math.ceil(w/gw),1))[:h,:w]
 return Image.fromarray(np.uint8(np.clip(a,0,255))).convert('RGBA')

# Retain the lobby beyond the actual panel contour; restore entire interior.
im=sources[ENEMY].convert('RGBA');m=Image.new('L',im.size)
points=[xy(p) for p in [(90,0),(960,0),(928,39),(928,478),(882,540),(0,540),(0,446),(50,390),(50,37)]]
ImageDraw.Draw(m).polygon(points,fill=255);im.paste(paper(im.size),(0,0),m);d=ImageDraw.Draw(im)
def rule(p,q,col='#B8BFC6'):d.line((*xy(p),*xy(q)),fill=col)
rule((120,14),(120,54),EDGE)
for k in (80,335,660):rule((k,69),(min(k+84,920),0),'#E1E5E9')
for x in (184,536):rule((x,73),(x,531))
rule((58,74),(173,74));rule((194,73),(530,73));rule((550,73),(920,73))
rule((61,440),(171,440));rule((21,520),(178,520))
for p in [(21,20),(21,473),(21,520),(939,20),(184,74),(184,521),(536,72),(536,531)]:
 x,y=xy(p);d.line((x-4,y,x+4,y),fill='#8A9099');d.line((x,y-4,x,y+4),fill='#8A9099')
save('Base_Material',im,(0,0,960,540),texture_sample_xywh=rect(sample),note='SubjectHeader, entry tabs and document all removed; original exterior lobby retained.')

def poly(w,h,c=12):return [(c,0),(w-c-1,0),(w-1,c),(w-1,h-c-1),(w-c-1,h-1),(0,h-1),(0,c)]
def panel(n,p,state='Normal',source=ENEMY,selected_dark=False,window=None,text=None):
 size=wh(p);w,h=size;im=blank(size);pts=poly(w,h);m=Image.new('L',size);ImageDraw.Draw(m).polygon(pts,fill=255)
 dark=selected_dark and state=='Selected'
 tex=Image.new('RGBA',size,INK) if dark else paper(size,state in ('Locked','LockedSelected','Unknown'))
 im.paste(tex,(0,0),m);d=ImageDraw.Draw(im)
 d.line(pts+[pts[0]],fill=CYAN if state in ('Hover','Selected') and not dark else '#A2A7AE',width=1)
 if dark:d.rectangle((0,1,5,h-2),fill=CYAN)
 if n.startswith('EntryTab') and state in ('Selected','LockedSelected'):d.line((12,h-4,w-13,h-4),fill=CYAN,width=3)
 if window:
  x,y,ww,hh=window;d.rectangle((x,y,x+ww-1,y+hh-1),fill=(0,0,0,0));d.rectangle((x-1,y-1,x+ww,y+hh),outline='#A2A7AE')
 save(n,im,p,source,[16]*4,state=state,portrait_window_xywh=window,text_slots_local=text or {})

for s in ('Normal','Hover','Selected'):
 panel('CategoryTab_'+s,(57,85,120,57),s,selected_dark=True,text={'title':[24,18],'english':[24,59]})
p=(201,111,80,110);w,h=wh(p);portrait=[4,4,w-8,132]
for s in ('Normal','Hover','Selected','Unknown'):
 panel('SubjectCell_'+s,p,s,window=portrait,text={'name':[12,143],'progress_origin':[22,172]})
p=(544,72,376,143);w,h=wh(p)
panel('SubjectHeader',p,window=[24,23,190,208],text={'name':[246,26],'badge':[246,91],'progress_text':[246,151],'progress_cells':[246,186]})
im=Image.new('RGBA',(198,35),CYAN)
save('Badge_Affiliation',im,(684,125,114,20),COMP,[4]*4,text_slots_local={'label':[13,8]})
for s in ('Filled','Empty'):
 im=Image.new('RGBA',(50,32),CYAN) if s=='Filled' else paper((50,32))
 ImageDraw.Draw(im).rectangle((0,0,49,31),outline=CYAN if s=='Filled' else '#A2A7AE')
 save('ProgressCell_'+s,im,(685,180,28,18),border=[2]*4)
for s in ('Normal','Hover','Selected','Locked','LockedSelected'):
 panel('EntryTab_'+s,(544,223,69,33),s,text={'number':[58,18],'lock':[20,16]})

def lock(n,size,p,source=ENEMY):
 im=blank(size);w,h=size;d=ImageDraw.Draw(im)
 x0=round(w*.19);x1=round(w*.81);top=1;bot=round(h*.57)
 d.arc((x0,top,x1,bot),180,360,fill=EDGE,width=max(2,round(w*.14)))
 d.line((x0,round(h*.25),x0,round(h*.48)),fill=EDGE,width=max(2,round(w*.14)))
 d.line((x1,round(h*.25),x1,round(h*.48)),fill=EDGE,width=max(2,round(w*.14)))
 d.rounded_rectangle((1,round(h*.4),w-2,h-1),radius=max(1,w//14),fill=EDGE)
 cx=w//2;cy=round(h*.66);rr=max(2,round(w*.11))
 d.ellipse((cx-rr,cy-rr,cx+rr,cy+rr),fill=(0,0,0,0));d.rectangle((cx-rr//2,cy,cx+rr//2,round(h*.85)),fill=(0,0,0,0))
 save(n,im,p,source)
lock('Icon_LockTab',(18,24),(710,231,11,14))
lock('Icon_LockLarge',(62,80),(709,360,36,46),COMP)

p=(544,262,376,270);w,h=wh(p);im=blank((w,h));d=ImageDraw.Draw(im);pts=[(22,0),(w-28,0),(w-5,24),(w-5,h-27),(w-28,h-5),(22,h-5),(0,h-27),(0,22)]
d.polygon([(x+4,y+4) for x,y in pts],fill='#C9CCD1')
m=Image.new('L',(w,h));ImageDraw.Draw(m).polygon(pts,fill=255);im.paste(paper((w,h)),(0,0),m);d=ImageDraw.Draw(im);d.line(pts+[pts[0]],fill='#A2A7AE')
d.polygon([(w-28,0),(w-28,24),(w-5,24)],fill='#E0E4E8',outline='#BBC2C9')
save('Document_Paper',im,p,border=[28]*4,shadow_offset=[4,4],document_regions_local={
 'document_number':[44,23,w-95,20],'title':[44,50,w-95,35],'title_rule':[44,96,w-91,2],
 'body_scroll':[44,121,w-94,h-173],'scrollbar':[w-19,82,5,h-154],
 'source_footer':[w-300,h-39,254,18],'locked_icon':[w//2-31,172,62,80],
 'locked_message':[60,274,w-120,28],'locked_current_level':[60,310,w-120,22]})
im=blank((40,58));d=ImageDraw.Draw(im)
d.line([(24,4),(31,3),(37,7),(37,13),(18,50),(13,54),(7,52),(3,47),(3,42),(21,8),(25,7),(28,10),(27,15),(12,43)],fill='#8A9099',width=2)
save('Document_Clip',im,(882,258,23,33))
im=blank((566,2));ImageDraw.Draw(im).line((0,0,565,0),fill='#8A9099')
save('Document_Rule',im,(570,317,324,1),border=[1,0,1,0])

reuse={
 'back':['../../LoadoutMockup/Extracted/Back_Normal.png','../../LoadoutMockup/Extracted/Back_Normal_Hover.png','../../LoadoutMockup/Extracted/Icon_BackArrow.png'],
 'checkbox':['../../ArmoryMockup/CardCatalogExtracted/Checkbox_'+s+'.png' for s in ('Unchecked','Selected','Unchecked_Hover','Selected_Hover')],
 'type_badges':['../../TrainingMockup/Extracted/TypeBadge_'+s+'.png' for s in ('Normal','Elite','Boss')],
 'record_dots':['../../TrainingMockup/Extracted/DefeatPip_'+s+'.png' for s in ('Filled','Empty')],
 'scroll':['../../TrainingMockup/Extracted/Scrollbar_'+s+'.png' for s in ('Track','Handle')],
 'small_lock':['../../TrainingMockup/Extracted/Icon_Lock.png']}
for paths in reuse.values():
 for p in paths:assert (OUT/p).exists(),p
document=next(e for e in E if e['name']=='Document_Paper')
manifest=dict(version=1,canvas=[1672,941],assets=E,reuse=reuse,
 coordinates='Top-left native1672x941. source_crop_xywh are reference areas; text/windows local to sprite unless screen is specified.',
 repeats={'subject_grid':{'origin':xy((201,111)),'cell_size':list(wh((0,0,80,110))),'columns':4,'column_step':round(83*1672/960),'row_step':round(112*941/540),'count':14},
 'entry_tabs':{'origin':xy((544,223)),'step':[round(75*1672/960),0],'count':5},
 'progress_cells':{'origin':xy((685,180)),'step':[round(33*1672/960),0],'count':5},
 'record_dots':{'local_origin':[22,172],'step':[23,0],'count':5},
 'category_tabs':{'origin':xy((57,85)),'step':[0,round(64*941/540)],'count':2}},
 text_screen={k:xy(v) for k,v in {'title':(134,16),'subtitle':(134,47),'grid_title':(201,86),'grid_count':(423,88),
 'filter_heading':(69,235),'filter_1':(99,264),'filter_2':(99,295),'filter_3':(99,324),'filter_event':(99,354),
 'unlock_rules':(63,454),'rule_footer':(63,490),'subject_name':(685,92),'subject_badge':(685,126),'subject_progress':(685,160)}.items()},
 document_regions_screen={k:[document['placement_1672_xy'][0]+v[0],document['placement_1672_xy'][1]+v[1],v[2],v[3]] for k,v in document['document_regions_local'].items()},
 locked_state={'selected_tab':'EntryTab_LockedSelected','lock_tab':'Icon_LockTab','body_lock':'Icon_LockLarge','runtime_message':'호감도 Lv.3에 열립니다','runtime_subtext':'(현재 Lv.2)'},
 decisions={'SubjectHeader':'Separate bordered plate in both mockups, exported with transparent art window.',
 'Icon_LockTab':'Smaller glyph than reused grid lock; exported18x24.',
 'Icon_LockLarge':'Distinct large body lock62x80, exported separately.'},
 source_differences='Enemy screenshot has bottom grid framing irregularities; shared four-column/14-cell layout follows companion regular grid.',
 layering=['Base_Material','category tabs','subject cells','existing game art clipped to windows','record dots','SubjectHeader','EntryTab states','Document_Paper','Document_Rule','Document_Clip','runtime text/locks'])
(OUT/'layout.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')

sheet=Image.new('RGB',(1200,math.ceil(len(E)/4)*150),'#535D67');d=ImageDraw.Draw(sheet);font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',12)
for i,e in enumerate(E):
 x=i%4*300;y=i//4*150
 for yy in range(y+24,y+150,12):
  for xx in range(x,x+300,12):d.rectangle((xx,yy,xx+11,yy+11),fill='#BAC3CC' if ((xx-x)//12+(yy-y-24)//12)%2 else '#E3E7EB')
 d.text((x+5,y+5),e['name'],font=font,fill='white');im=Image.open(OUT/e['file']);im.thumbnail((280,118),Image.Resampling.LANCZOS);sheet.paste(im,(x+(300-im.width)//2,y+27+(118-im.height)//2),im)
sheet.save(OUT/'Contact_Sheet.jpg',quality=93)

readme='''# 마테리얼 UI 분절

정본1672×941 두 장을 기준으로 분절·복원했습니다. 글자·숫자·초상·실루엣·적 그림은 포함하지 않았습니다. 판 내부는 원본의 깨끗한 종이 여백에서 추출한 질감으로 채웠고, 테두리·그림자·상태·기호는 정본 형태로 재구성했습니다. 단순 사각 크롭이 아닙니다. Base의 바깥 로비 배경은 원본 그대로 유지했습니다. 모든 PNG 알파는0/255입니다.

## 새 조각과 이유

- Base_Material: 분류/4열 격자/읽기 영역 구조가 훈련장과 달라 전용 바탕. 머리/구역 구분선만 포함하며 탭·칸·서류·대상 머리는 분리했습니다.
- CategoryTab3상태: 두 언어를 넣는 큰 잘린 분류 판. Selected는먹색+왼쪽청록띠입니다.
- SubjectCell4상태: 초상 창은 투명, 이름·점 자리 종이는 포함. Hover/Selected 청록 테두리이며 표시점은 별도입니다.
- SubjectHeader: 원본에 독립 테두리가 있어 별도 출력했습니다. 큰 그림 창 투명, 정보 자리 빈 종이입니다.
- Badge_Affiliation: 동료 소속 청록 칸, 가로9-slice 가능. ProgressCell Filled/Empty: 원본의 직사각 진행 칸으로 마름모 기록 점과 구분합니다.
- EntryTab5상태: 글자·잠금 기호 제외. LockedSelected는회색+청록밑줄입니다.
- Icon_LockTab18×24, Icon_LockLarge62×80: 기존 작은 격자 잠금과 용도·규격이 달라 새로 복원했습니다.
- Document_Paper: 접힌 모서리·불투명 그림자 포함. 제목/본문/괘선/클립은 없습니다. Document_Rule와 Document_Clip은별도입니다.

## 재사용 (다시 출력하지 않음)

- LoadoutMockup/Extracted/Back_Normal, Back_Normal_Hover, Icon_BackArrow.
- ArmoryMockup/CardCatalogExtracted/Checkbox_Unchecked/Selected 및_Hover.
- TrainingMockup/Extracted/TypeBadge_Normal/Elite/Boss.
- TrainingMockup/Extracted/DefeatPip_Filled/Empty: 칸 아래 기록점5개.
- TrainingMockup/Extracted/Scrollbar_Track/Handle: 격자와본문용으로길이조절.
- TrainingMockup/Extracted/Icon_Lock: 미확인격자칸용. 밝은색으로tint합니다.

기존 파일은 같은 문법이므로 재사용하며 존재를 확인했습니다. reuse 경로는 이 Extracted 폴더 기준입니다. 기존 분절본을 수정하지 않았습니다.

## 조립·좌표

layout.json 좌표계는1672×941 좌상단 원점입니다. source_crop_xywh는원본 참고 영역, placement_1672_xy는배치, output_size는PNG규격입니다. portrait_window_xywh는각 칸 내부의 완전투명영역이고 text_slots_local는글자위치입니다. 창 안에 게임 그림을 마스크해 넣습니다.

격자는4열14칸, 마지막줄2칸입니다. 두 정본 하단틀에 미세한 차이가 있어 동료 화면의 정규 간격으로 통일했습니다. repeats에는격자/편탭/진행칸/기록점/분류탭 간격을 기록했습니다. text_screen에는섹션명·개수·필터·대상정보·해금안내 자리가 있습니다.

Document_Paper의 document_regions_local와 최상위 document_regions_screen에는문서번호/제목/괘선/본문스크롤/스크롤바/출처/잠김문구 영역이 있습니다. 잠긴3편은 EntryTab_LockedSelected+Icon_LockTab을 쓰고본문 대신 Icon_LockLarge와런타임조건을 표시합니다. 기록2/5와선택3편을혼동하지 않습니다.

nine_slice_lbrt는Left,Bottom,Right,Top입니다. 초상 창을 가진 칸과대상머리는지정크기를권장합니다. 크게늘리면창도변하므로게임의그림마스크를함께맞추십시오. 서류는28px경계로접힘/그림자를보존하며클립은별도배치합니다. 기호/배경은Simple입니다.

## 확인·재생성

python ArtDirection/MaterialMockup/Split_Handoff/split_material.py

Contact_Sheet.jpg는1200px폭한장입니다. 크기·알파·크롭범위·투명창·상태동일크기·재사용파일존재를자동검증했습니다. 원본목업과Unity importer/.meta는수정하지않았습니다.
'''
(OUT/'README.md').write_text(readme,encoding='utf-8')
for e in E:
 im=Image.open(OUT/e['file']);assert list(im.size)==e['output_size'];assert set(im.getchannel('A').get_flattened_data())<={0,255},e['name']
 x,y,w,h=e['source_crop_xywh'];assert 0<=x<x+w<=1672 and 0<=y<y+h<=941,e['name']
 if e.get('portrait_window_xywh'):
  x,y,w,h=e['portrait_window_xywh'];assert np.all(np.asarray(im)[y:y+h,x:x+w,3]==0),e['name']
for pre in ('CategoryTab','SubjectCell','EntryTab','ProgressCell'):
 assert len({tuple(e['output_size']) for e in E if e['name'].startswith(pre+'_')})==1
print(f'Validated {len(E)} PNGs; alpha, windows, state sizes and reuse verified; sheet{sheet.size}.')
