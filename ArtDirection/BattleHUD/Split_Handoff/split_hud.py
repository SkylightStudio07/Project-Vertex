"""Deterministic text-free HUD export. Original PNGs never need to be displayed."""
from pathlib import Path
import json, math
from PIL import Image, ImageDraw, ImageFont, ImageFilter

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / 'Extracted'
OUT.mkdir(exist_ok=True)
CAN = 'BattleHUD_Mockup_v3_Canonical.png'
BAN = 'BattleHUD_Mockup_v3_Oratio_TurnBanner.png'
ENE = 'BattleHUD_Mockup_v3_Oratio_EnemyTurn.png'
images = {n: Image.open(ROOT/n).convert('RGB') for n in [CAN, BAN, ENE]}
assert all(i.size == (1672,941) for i in images.values())
DARK='#16181B'; EDGE='#3A3E44'; WHITE='#F2F3F4'; CYAN='#0DB8F2'
entries=[]

def save(name, im, rect, source=CAN, method='geometry restored from source; text removed; exact palette', border=None, **extra):
    im.save(OUT/(name+'.png'))
    x,y,w,h=rect
    entry=dict(name=name,file=name+'.png',source=source,source_crop_xywh=rect,
        output_size=list(im.size),placement_1920_xy=[round(x*1920/1672),round(y*1080/941)],
        reference_display_size_1920=[round(w*1920/1672),round(h*1080/941)],method=method,
        nine_slice_lbrt=border or [0,0,0,0],**extra)
    entries.append(entry)

def blank(size): return Image.new('RGBA',size,(0,0,0,0))
def plate(name,size,rect,state='Normal',border=None,source=CAN):
    im=blank(size); d=ImageDraw.Draw(im); w,h=size
    color='#202328' if state=='Disabled' else DARK
    d.rectangle((0,0,w-1,h-1),fill=color)
    d.rectangle((1,1,w-2,h-2),outline=CYAN if state in ('Hover','Quest') else EDGE)
    if state=='Pressed': d.line((3,h-3,w-4,h-3),fill=CYAN)
    if state=='Quest':
        d.rectangle((2,h-17,17,h-2),fill=DARK,outline=CYAN)
        d.rectangle((6,h-12,13,h-6),outline=WHITE)
        d.line((6,h-10,13,h-10),fill=WHITE)
    save(name,im,rect,source,border=border,state=state)
    return im

# Preserve the canonical long shallow trapezoid and its three dividers.
im=blank((1300,72)); d=ImageDraw.Draw(im)
d.polygon([(0,0),(1299,0),(1252,71),(46,71)],fill=DARK)
for x in (211,411,846): d.line((x,60,x+24,16),fill=WHITE,width=1)
save('TopBar',im,[270,0,1115,64],border=[52,0,52,0])
for state in ('Empty','Hover','Quest'):
    plate('ItemSlot_'+state,(56,56),[1249,11,63,62],state,border=[18,18,3,3] if state=='Quest' else [3]*4)
for name,rect in [('Map',[1482,19,73,74]),('Deck',[1574,20,74,73])]:
    for state in ('Normal','Hover','Pressed'):
        plate('Button_'+name+'_'+state,(64,64),rect,state,border=[3]*4)
for state in ('Normal','Hover','Pressed','Disabled'):
    plate('Button_EndTurn_'+state,(280,70),[1364,817,284,63],state,border=[3]*4)

for state in ('Normal','Empty'):
    im=blank((264,225)); ImageDraw.Draw(im).rectangle((0,0,263,224),fill=DARK)
    save('Res_Panel_'+state,im,[1435,614,225,192],border=[2]*4,state=state,
         note='Empty keeps the canonical opaque plate. Render energy 0 and dim its icon/text at runtime.',
         state_reference=ENE if state=='Empty' else CAN)

def line(name,size,rect,points,color=WHITE,source=CAN,width=1):
    im=blank(size); ImageDraw.Draw(im).line(points,fill=color,width=width); save(name,im,rect,source)
line('Res_Divider',(264,2),[1435,743,225,2],[(0,0),(263,0)],EDGE)
line('Res_Slash',(40,80),[1553,663,31,70],[(1,79),(39,0)])
line('EndTurn_CornerSlash',(48,70),[1364,817,47,63],[(0,69),(47,0)])
line('StatusSlot_Underline',(44,2),[44,808,39,2],[(0,0),(43,0)])

def icon(name,rect,source=CAN):
    x,y,w,h=rect
    crop=images[source].crop((x,y,x+w,y+h))
    mask=crop.convert('L').point(lambda v: 255 if v>=145 else 0)
    bbox=mask.getbbox(); assert bbox, name
    mask=mask.crop(bbox); mask.thumbnail((28,28),Image.Resampling.LANCZOS)
    mask=mask.point(lambda v:255 if v>=100 else 0)
    im=blank((32,32)); ink=Image.new('RGBA',mask.size,WHITE); ink.putalpha(mask)
    im.paste(ink,((32-mask.width)//2,(32-mask.height)//2))
    save(name,im,rect,source,method='source luminance mask; binary alpha; aspect preserved; centered in 32px')
for name,rect,source in [
    ('Icon_Map',[1493,31,51,52],BAN),('Icon_Deck',[1588,38,46,39],CAN),
    ('Icon_Energy',[1450,680,29,47],CAN),('Icon_Ammo',[1451,768,24,34],CAN),
    ('Icon_Arrow',[1598,838,33,21],CAN)]: icon(name,rect,source)

# A pistol is absent from the canonical weapon row. Recover the prior isolated model.
pistol=Image.open(ROOT/'Canonical_PistolIcon_Model.png').convert('RGBA')
alpha=pistol.getchannel('A')
if alpha.getextrema()==(255,255):
    alpha=pistol.convert('L').point(lambda v:255 if v>100 else 0)
else: alpha=alpha.point(lambda v:255 if v>=128 else 0)
bbox=alpha.getbbox(); assert bbox
alpha=alpha.crop(bbox).filter(ImageFilter.MaxFilter(15))
alpha.thumbnail((28,28),Image.Resampling.LANCZOS)
alpha=alpha.point(lambda v:255 if v>=40 else 0)
im=blank((32,32)); ink=Image.new('RGBA',alpha.size,WHITE); ink.putalpha(alpha)
im.paste(ink,((32-alpha.width)//2,(32-alpha.height)//2))
save('Icon_Weapon_Pistol',im,[0,0,pistol.width,pistol.height],'Canonical_PistolIcon_Model.png',
     method='isolated model foreground mask; stroke dilation before downsampling; binary alpha; aspect preserved')
entries[-1]['placement_1920_xy']=[1660,715]
entries[-1]['reference_display_size_1920']=[32,32]

for name in ('Energy','Arrow'):
    im=Image.open(OUT/('Icon_'+name+'.png')).convert('RGBA')
    ink=Image.new('RGBA',im.size,'#5A6069'); ink.putalpha(im.getchannel('A'))
    base=next(e for e in entries if e['name']=='Icon_'+name)
    save('Icon_'+name+'_Disabled',ink,base['source_crop_xywh'],state_reference=ENE)

for prefix,size,inner,rect in [('HP',(360,39),(340,27),[89,826,335,42]),
                              ('EnemyHP',(240,26),(227,18),[1108,637,266,24])]:
    w,h=size; iw,ih=inner; ox=(w-iw)//2; oy=(h-ih)//2
    im=Image.new('RGBA',size,DARK); d=ImageDraw.Draw(im)
    d.rectangle((1,1,w-2,h-2),outline=EDGE)
    d.rectangle((ox,oy,ox+iw-1,oy+ih-1),fill=(0,0,0,0))
    save(prefix+'_Frame',im,rect,border=[ox,oy,w-iw-ox,h-ih-oy],fill_offset=[ox,oy])
    subrect=[rect[0]+round(ox/w*rect[2]),rect[1]+round(oy/h*rect[3]),round(iw/w*rect[2]),round(ih/h*rect[3])]
    for layer,color in [('Fill','#C9CED4'),('Track',DARK)]:
        im=Image.new('RGBA',inner,color)
        if layer=='Fill' and prefix=='HP': ImageDraw.Draw(im).line((iw-2,0,iw-2,ih-1),fill=CYAN,width=2)
        save(prefix+'_'+layer,im,subrect,border=[2,0,2,0],note='Full-width fill; apply gameplay HP ratio at runtime; no numbers.')

for name,size,rect in [('Block_Badge',(56,68),[38,823,54,61]),('EnemyBlock_Badge',(40,48),[1110,599,25,33])]:
    w,h=size; im=blank(size); d=ImageDraw.Draw(im)
    d.polygon([(0,0),(w-1,0),(w-1,round(h*.73)),(w//2,h-1),(0,round(h*.73))],fill=DARK)
    d.line([(3,3),(w-4,3),(w-4,round(h*.71)),(w//2,h-5),(3,round(h*.71)),(3,3)],fill=WHITE,width=1)
    save(name,im,rect,note='Shield symbol silhouette only; number is a separate runtime label.')
for name,size,rect in [('EnemyName_Band',(154,26),[1109,665,133,22]),('TextBand',(120,40),[1109,665,133,22])]:
    save(name,Image.new('RGBA',size,DARK),rect,border=[4,4,4,4])
line('Banner_Slash',(138,206),[758,190,120,179],[(0,205),(137,0)],DARK,BAN)
line('Banner_Line',(234,2),[815,309,204,2],[(0,0),(233,0)],DARK,BAN)
im=blank((14,14)); ImageDraw.Draw(im).polygon([(7,0),(13,7),(7,13),(0,7)],fill=CYAN)
save('Banner_Diamond',im,[1023,302,12,12],BAN)

manifest=dict(version=1,reference_canvas=[1920,1080],source_canvas=[1672,941],
    coordinate_convention='top-left origin; xywh in original PNG pixels; output pixels at 1920x1080 design resolution',
    placement_note='Native output sizes are normative. reference_display_size records mockup footprint, which differs from requested sizes.',
    palette=dict(panel=DARK,inner_line=EDGE,icon=WHITE,accent=CYAN,disabled_panel='#202328',disabled_icon='#5A6069'),
    assets=entries,resource_divider_instances_1920=[[1648,745],[1648,853]],
    item_slot_instances_1920=[[1240,11],[1338,13],[1434,13]],
    runtime_states={'Normal':{'resource':'Res_Panel_Normal','end_turn':'Button_EndTurn_Normal','energy_text':'3'},
                    'EnemyTurn':{'resource':'Res_Panel_Empty','end_turn':'Button_EndTurn_Disabled','energy_text':'0','energy_icon':'Icon_Energy_Disabled','arrow_icon':'Icon_Arrow_Disabled'}},
    removed=['all typography and numbers','canonical star MAP symbol','short cyan line above small energy 3'],
    button_icon_layers={'Button_Map':'Icon_Map','Button_Deck':'Icon_Deck','Button_EndTurn':['EndTurn_CornerSlash','Icon_Arrow']})
(OUT/'layout.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')

# One small sheet, including all pieces; no full-screen source images.
cols=4; cw=300; ch=124; rows=math.ceil(len(entries)/cols)
sheet=Image.new('RGB',(cols*cw,rows*ch),(77,81,87)); sd=ImageDraw.Draw(sheet)
font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',13)
for i,e in enumerate(entries):
    x=i%cols*cw; y=i//cols*ch
    for yy in range(y+26,y+ch,12):
        for xx in range(x,x+cw,12):
            sd.rectangle((xx,yy,xx+11,yy+11),fill=(66,70,76) if ((xx-x)//12+(yy-y-26)//12)%2 else (85,89,95))
    sd.text((x+7,y+5),e['name'],font=font,fill='white')
    im=Image.open(OUT/e['file']); im.thumbnail((cw-20,ch-40),Image.Resampling.NEAREST)
    sheet.paste(im,(x+(cw-im.width)//2,y+30+(ch-36-im.height)//2),im)
sheet.save(OUT/'Contact_Sheet.jpg',quality=90)

readme='''# Battle HUD v3 분절 결과

`Split_Handoff/SPLIT_REQUEST.md` 기준. 원본 PNG는 디스크에서만 읽었고, 모든 조각에서 글자와 숫자를 제외했습니다.

## 추출 방식

기호(Map/Deck/Energy/Ammo/Arrow)는 지정된 원본의 밝기 마스크로 분리했습니다. MAP은 TurnBanner의 접힌 지도입니다. 권총은 기존 `Canonical_PistolIcon_Model.png`에서 분리했습니다. 판·HP·방패·헤어라인은 원본 위치와 형태를 기준으로 지정 색상으로 재구성했습니다. 따라서 배경이나 글자가 남는 단순 사각 크롭이 아닙니다. 생성형 이미지 도구는 사용하지 않았습니다.

모든 PNG의 알파는 0 또는 255입니다. 판은 #16181B, 안쪽 선은 #3A3E44, 기호는 #F2F3F4입니다. 비활성 END TURN은 명세의 #202328, 비활성 기호는 #5A6069입니다. HP 채움은 원본의 연회색 #C9CED4입니다. 청록은 테두리·선·마름모에만 사용했습니다.

## 조립

- `layout.json`의 `source_crop_xywh`는 원본 픽셀 좌표, `output_size`는 요청 출력 크기, `placement_1920_xy`는 좌상단 기준 배치입니다. `reference_display_size_1920`는 목업에서 차지하던 크기입니다. 지정된 출력 크기와 목업 비율은 일부 다르므로 두 값을 구분했습니다.
- 버튼 판 위에 Icon_Map/Deck 또는 Icon_Arrow와 EndTurn_CornerSlash를 올리고 글자는 게임에서 넣습니다.
- Res_Panel_Normal/Empty는 같은 먹색 판입니다. 적 턴은 에너지 숫자 0, Icon_Energy_Disabled, Button_EndTurn_Disabled, Icon_Arrow_Disabled로 표현합니다. 작은 3 위 청록 선은 없습니다.
- Res_Divider는 판 폭과 같은 264×2입니다. 구분선 2개의 위치는 JSON에 기록했습니다. 에너지 사선은 Res_Slash입니다.
- HP는 Track → Fill → Frame → 런타임 숫자 순서입니다. Frame의 `fill_offset`에 Track/Fill을 배치합니다. Fill은 전체 폭이므로 체력 비율을 적용합니다. 플레이어 청록 끝선은 UV 크롭보다 가로 크기 조절로 유지하십시오.
- Block_Badge/EnemyBlock_Badge는 숫자 없는 방패입니다. Quest의 왼쪽 아래 유형 배지는 별도의 인벤토리 그림과 구분되는 작은 상자 기호입니다.
- EnemyName_Band/TextBand 위에 런타임 글자를 올립니다. Banner_Slash/Line/Diamond는 TurnBanner의 세 장식이며 PLAYER TURN은 포함하지 않습니다.

## 9-slice

JSON `nine_slice_lbrt` 순서는 Unity와 같은 Left, Bottom, Right, Top이며 출력 픽셀 단위입니다. TextBand/EnemyName_Band는 4,4,4,4; 버튼과 기본 슬롯은 3,3,3,3; Quest는 배지 보존을 위해 18,18,3,3입니다. HP_Frame은 실제 창 여백이며 Fill은 2,0,2,0입니다. TopBar는 양 끝 컷 보존을 위해 52,0,52,0이며 높이 72 고정 사용을 권장합니다. 기호와 배너 조각은 Simple로 사용합니다.

이 작업은 ArtDirection의 PNG 납품이며 Unity importer/.meta 설정은 변경하지 않았습니다.

## 재생성 및 확인

`python ArtDirection/BattleHUD/Split_Handoff/split_hud.py`

확인은 `Contact_Sheet.jpg` 한 장(1200px 폭)으로 합니다. 원본이나 전체화면 조립 이미지는 추가로 열 필요가 없습니다.
'''
(OUT/'README.md').write_text(readme,encoding='utf-8')
for e in entries:
    im=Image.open(OUT/e['file']); assert list(im.size)==e['output_size']
    assert set(im.getchannel('A').get_flattened_data()) <= {0,255},e['name']
    x,y,w,h=e['source_crop_xywh']; src=Image.open(ROOT/e['source'])
    assert 0<=x<x+w<=src.width and 0<=y<y+h<=src.height,e['name']
print(f'Exported and validated {len(entries)} RGBA sprites; contact sheet {sheet.size}.')
