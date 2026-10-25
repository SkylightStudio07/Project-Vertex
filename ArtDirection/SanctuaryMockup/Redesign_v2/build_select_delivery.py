"""Build editable UI geometry, 2x sprites, layout, and source-art preview. No Unity edits."""
from pathlib import Path
import json, subprocess, hashlib, zipfile
from PIL import Image, ImageDraw, ImageFont

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
OUT = HERE / 'Extracted_Draft'
SVG = OUT / 'SVG'
SVG.mkdir(parents=True, exist_ok=True)
INK = '#16181B'
CYAN = '#0DB8F2'
CRIMSON = '#AC173C'
assets = {}
jobs = []

def svg(w, h, body):
    return f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}">{body}</svg>'

def poly(points, color):
    return '<polygon points="' + ' '.join(f'{x},{y}' for x,y in points) + f'" fill="{color}"/>'

def asset(name, w, h, body, scale=2, **extra):
    path = SVG / (name+'.svg')
    path.write_text(svg(w,h,body), encoding='utf-8')
    jobs.append({'input':str(path), 'output':str(OUT/(name+'.png')), 'width':w*scale})
    assets[name] = dict(file=name+'.png',svg='SVG/'+name+'.svg',logical_size=[w,h],source_pixels=[w*scale,h*scale],scale=scale,pivot=[.5,.5],text_baked=False,method='Editable SVG reconstruction',**extra)

active = [(0,0),(975,0),(665,974),(0,974)]
locked = [(310,0),(780,0),(470,974),(0,974)]
right = [(310,0),(785,0),(785,974),(0,974)]
asset('Panel_Active',975,974,poly(active,'#F2F4F5'))
asset('Panel_Active_Hover',975,974,poly(active,'#FAFBFC'))
asset('Panel_Locked',780,974,poly(locked,'#D2DAE1'))
asset('Panel_Locked_Right',785,974,poly(right,'#E0E5EA'))
base = '<rect width="1920" height="1080" fill="#EEF0F2"/>'
planes = f'<g transform="translate(0 106)">{poly(active,"#F2F4F5")}</g><g transform="translate(665 106)">{poly(locked,"#D2DAE1")}</g><g transform="translate(1135 106)">{poly(right,"#E0E5EA")}</g>'
guides = '<g fill="none" stroke="#71818F" stroke-width="1" opacity=".10">'
for x in range(40,1920,160): guides += f'<path d="M{x} 110V1050"/>'
for y in range(170,1080,160): guides += f'<path d="M40 {y}H1880"/>'
guides += '</g><g stroke="#71818F" stroke-width="1" fill="none" opacity=".4"><path d="M40 110V1030H1880M40 110H1880V1030"/>'
for x,y in [(40,110),(1880,110),(40,1030),(1880,1030),(40,570),(1880,570)]: guides += f'<path d="M{x-10} {y}H{x+10}M{x} {y-10}V{y+10}"/>'
guides += '<path d="M975 106L665 1080M1445 106L1135 1080"/></g>'
asset('Sanctuary_BG',1920,1080,base+planes+guides,scale=1,opaque=True)
asset('Sanctuary_Base',1920,1080,base,scale=1,opaque=True)
asset('Overlay_Guides',1920,1080,guides,scale=1)
edge = '<path d="M312 0L2 974" fill="none" stroke="#0DB8F2" stroke-width="2"/>'
asset('Line_ActiveEdge',314,974,edge)
asset('Line_ActiveEdge_Hover',314,974,edge.replace('width="2"','width="4"'))
asset('Mark_Selected',12,12,'<circle cx="6" cy="6" r="5" fill="#0DB8F2"/>')
asset('Rule_Name',92,4,'<rect width="92" height="4" fill="white"/>',tint='candidate.theme_color')
button = [(24,0),(432,0),(432,84),(408,108),(0,108),(0,24)]
for name,color in [('Button_Detail',INK),('Button_Detail_Hover','#303C47'),('Button_Detail_Disabled','#A3ADB6')]:
    asset(name,432,108,poly(button,color)+'<path d="M0 106H408" stroke="'+(CYAN if 'Disabled' not in name else '#C5CDD3')+'" stroke-width="4"/>')
asset('Shadow_Button',432,108,'<g opacity=".35">'+poly(button,INK)+'</g>',offset=[12,12])

runner = HERE/'render_select_assets.cjs'
runner.write_text("const fs=require('fs'); const {Resvg}=require('C:/Users/SKYLIGHT/.codex/tmp/currency-f-render/node_modules/@resvg/resvg-js'); for(const j of JSON.parse(fs.readFileSync(process.argv[2],'utf8'))){fs.writeFileSync(j.output,new Resvg(fs.readFileSync(j.input),{fitTo:{mode:'width',value:j.width}}).render().asPng());}\n",encoding='utf-8')
(OUT/'render_jobs.json').write_text(json.dumps(jobs),encoding='utf-8')
subprocess.run(['node',str(runner),str(OUT/'render_jobs.json')],check=True)
(OUT/'render_jobs.json').unlink()

characters = [
    {'slot':'01','source':'Assets/Art/Characters/Joining Characters/CP_01/CP_01_Chihara.png','rect':[280,118,660,930],'silhouette':False},
    {'slot':'02','source':'Assets/Art/Characters/Joining Characters/CP_02/알레시아_Standing.png','rect':[800,118,600,930],'silhouette':True},
    {'slot':'03','source':'Assets/Art/Characters/Joining Characters/CP_03/2058009_image.png','rect':[1280,118,600,930],'silhouette':True},
]
placements = [
    {'id':'background','asset':'Sanctuary_BG','rect':[0,0,1920,1080],'z':0,'mode':'flattened'},
    {'id':'active_edge','asset':'Line_ActiveEdge','rect':[663,106,314,974],'z':3},
    {'id':'selected_dot','asset':'Mark_Selected','rect':[492,153,12,12],'z':20},
    {'id':'name_rule','asset':'Rule_Name','rect':[96,812,92,4],'tint':CRIMSON,'tint_binding':'candidate.theme_color','z':20},
    {'id':'lock_02','external_asset':'Icon_Lock','rect':[1078,538,44,54],'z':20},
    {'id':'lock_03','external_asset':'Icon_Lock','rect':[1558,538,44,54],'z':20},
    {'id':'detail_shadow','asset':'Shadow_Button','rect':[1460,940,432,108],'z':30},
    {'id':'detail','asset':'Button_Detail','rect':[1448,928,432,108],'z':31},
]
texts=[]
def text_box(id, text, rect, size, color=INK, weight=400, alignment='left', spacing=0, **extra):
    texts.append(dict(id=id,text=text,rect=rect,font_size=size,color=color,font_weight=weight,alignment=alignment,character_spacing_px=spacing,**extra))
text_box('title','성소',[82,158,300,72],60,weight=700)
text_box('subtitle','SANCTUARY / COMPANION SELECT',[84,247,350,26],16,'#626F7B',spacing=.4)
text_box('instruction','동료를 한 명 고르십시오.',[84,282,335,32],23)
for n,x in [('01',438),('02',980),('03',1462)]:
    text_box('candidate_'+n,n,[x,143,45,32],24,'#596571')
text_box('selected','선택 중',[438,184,100,28],18,CYAN,600)
text_box('name','치하라 쇼',[96,733,310,72],56,weight=700,binding='candidate.display_name')
text_box('faction','VERTEX / 협력자',[96,835,310,32],22,CRIMSON,600,binding='candidate.faction',color_binding='candidate.theme_color')
text_box('availability','합류 가능',[96,879,330,60],21,'#66717C',binding='party.capacity_message',max_lines=2)
for n,cx in [('02',1100),('03',1580)]:
    text_box('locked_'+n,'잠긴 후보',[cx-160,612,320,50],36,INK,700,'center',opacity=.7)
    text_box('condition_'+n,'구출 의뢰 · 구조 신호',[cx-180,687,360,60],20,INK,400,'center',opacity=.55,max_lines=2,binding=f'candidates.{n}.unlock_condition')
text_box('detail_en','DETAIL',[1488,945,240,22],14,'#C1CAD1',500,spacing=2)
text_box('detail_label','후보 확인',[1488,974,280,47],36,'#FFFFFF',700)
text_box('detail_arrow','→',[1810,963,48,54],44,'#FFFFFF',400,'center')

canvas=Image.open(OUT/'Sanctuary_BG.png').convert('RGBA')
def sprite(name,rect,tint=None):
    im=Image.open(OUT/(name+'.png')).convert('RGBA')
    im=im.resize(tuple(rect[2:]),Image.Resampling.LANCZOS)
    if tint:
        tinted=Image.new('RGBA',im.size,tint); tinted.putalpha(im.getchannel('A')); im=tinted
    canvas.alpha_composite(im,tuple(rect[:2]))

sprite('Line_ActiveEdge',[663,106,314,974])
for c in characters:
    path=ROOT/c['source']; im=Image.open(path).convert('RGBA')
    c['sha256']=hashlib.sha256(path.read_bytes()).hexdigest()
    bbox=im.getchannel('A').getbbox(); c['alpha_bounds_source']=list(bbox)
    im=im.crop(bbox)
    x,y,w,h=c['rect']; s=min(w/im.width,h/im.height)
    size=(round(im.width*s),round(im.height*s)); im=im.resize(size,Image.Resampling.LANCZOS)
    if c['silhouette']:
        alpha=im.getchannel('A').point(lambda p:round(p*.16))
        im=Image.new('RGBA',im.size,'#687785'); im.putalpha(alpha)
    pos=(x+(w-size[0])//2,y+(h-size[1])//2)
    c['preview_rendered_rect']=[*pos,*size]
    c.update(fit='contain',preserve_aspect=True,rotation=0,clip_to_panel=False,asset_included=False)
    canvas.alpha_composite(im,pos)

lockpath=ROOT/'ArtDirection/LoadoutMockup/Extracted_Draft/Icon_Lock.png'
for p in placements:
    if p['id'] in ['background','active_edge']: continue
    if 'external_asset' in p:
        im=Image.open(lockpath).convert('RGBA').resize(tuple(p['rect'][2:]),Image.Resampling.LANCZOS)
        canvas.alpha_composite(im,tuple(p['rect'][:2]))
    else: sprite(p['asset'],p['rect'],p.get('tint'))

fontdir=ROOT/'Assets/Font/Pretendard'
def draw_text(t):
    layer=Image.new('RGBA',canvas.size); draw=ImageDraw.Draw(layer)
    weight='Bold' if t['font_weight']>=700 else 'SemiBold' if t['font_weight']>=600 else 'Medium' if t['font_weight']>=500 else 'Regular'
    font=ImageFont.truetype(str(fontdir/f'Pretendard-{weight}.otf'),t['font_size'])
    x,y,w,h=t['rect']; string=t['text']; spacing=t['character_spacing_px']
    tw=sum(draw.textlength(ch,font=font) for ch in string)+spacing*max(0,len(string)-1)
    if t['alignment']=='center': x+=(w-tw)/2
    fill=t['color']
    for ch in string:
        draw.text((x,y),ch,font=font,fill=fill,anchor='lt')
        x+=draw.textlength(ch,font=font)+spacing
    if t.get('opacity') is not None: layer.putalpha(layer.getchannel('A').point(lambda p:round(p*t['opacity'])))
    canvas.alpha_composite(layer)
    t['preview_text_width']=round(tw,2)
    assert tw<=w, (t['id'],tw,w)
for t in texts: draw_text(t)
canvas.convert('RGB').save(HERE/'Sanctuary_Select_Mockup_v3.png')

layout=dict(version=3,canvas=[1920,1080],origin='top-left',rect_format='x,y,width,height',
    source_mockup={'file':'../Sanctuary_Select_Mockup_v2.png','size':[1672,941],'reference_scale_xy':[1920/1672,1080/941],'note':'Reference scaled, then corrected per SPLIT_REQUEST_v2; deterministic SVG reconstruction, not pixel crop.'},
    delivery_mockup='../Sanctuary_Select_Mockup_v3.png',hud_reserved_rect=[0,0,1920,106],assets=assets,
    external_assets={'Icon_Lock':{'file':'../../../LoadoutMockup/Extracted_Draft/Icon_Lock.png','svg':'../../../LoadoutMockup/Extracted_Draft/SVG/Icon_Lock.svg','logical_size':[44,54],'included':False}},
    external_characters=characters,
    candidates=[
        {'slot':'01','state':'active','panel_asset':'Panel_Active','panel_rect':[0,106,975,974],'polygon':[[x,y+106] for x,y in active],'character_rect':characters[0]['rect'],'theme_color':CRIMSON,'text_ids':['candidate_01','selected','name','faction','availability']},
        {'slot':'02','state':'locked','panel_asset':'Panel_Locked','panel_rect':[665,106,780,974],'polygon':[[x+665,y+106] for x,y in locked],'character_rect':characters[1]['rect'],'silhouette':True,'theme_color':'#AD8C54','text_ids':['candidate_02','locked_02','condition_02'],'art_requirement':'Alesia: shield 1.5x character height + short spear. Current standing source has different weapon proportions; preview uses source unchanged.'},
        {'slot':'03','state':'locked','panel_asset':'Panel_Locked_Right','panel_rect':[1135,106,785,974],'polygon':[[x+1135,y+106] for x,y in right],'character_rect':characters[2]['rect'],'silhouette':True,'text_ids':['candidate_03','locked_03','condition_03']}
    ],placements=placements,text_boxes=texts,
    states={'candidate':{'active':{'asset':'Panel_Active','show':['name','name_rule','faction','availability','selected_dot','selected']},'hover':{'asset':'Panel_Active_Hover','edge_asset':'Line_ActiveEdge_Hover','edge_width_px':4},'locked':{'asset':'Panel_Locked','silhouette_color':'#687785','silhouette_opacity':.16,'lock_label_opacity':.7,'condition_opacity':.55},'party_full':{'availability_text':'동행 인원이 가득 찼습니다','detail_state':'disabled','interactable':False}},'detail':{'normal':{'asset':'Button_Detail','interactable':True},'hover':{'asset':'Button_Detail_Hover','interactable':True},'disabled':{'asset':'Button_Detail_Disabled','interactable':False,'text_color':'#E5E9ED'}}},
    rendering={'alpha':'straight','filter':'bilinear','ui_png_scale':2,'background_png_scale':1,'font':'Pretendard; runtime TMP','shadow':{'offset_canvas':[12,12],'unity_effect_distance':[12,-12],'color':INK,'opacity':.35,'blur':0,'use_either_shadow_sprite_or_unity_shadow':True},'background_modes':{'flattened':['Sanctuary_BG'],'modular':['Sanctuary_Base','candidate panel assets','Overlay_Guides'],'rule':'Choose ONE background mode. Common overlays, characters and text follow; never draw flattened and modular background together.'},'edge_note':'Cyan line follows approved mockup active/right boundary (left edge of slot 02). Hover replaces normal edge, not additive.'},
    notes=['UI geometry is rebuilt as SVG; characters, silhouettes, lock icon and HUD are not included in sprites.','All character slots have identical 930px height, contain fit, no tilt or panel clipping. Wide art may render shorter due to preserved aspect ratio.','Tiny cyan button edge retained as explicitly requested in asset table; faction and name rule use character theme color.','Background top 106px remains plain for runtime HUD.','No nine-slice: use authored size or proportional scale. Panel polygon hit areas must follow candidate polygon, not bounding rect.'])
(OUT/'layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf-8')

# Asset sheet: display alpha pieces on light checkerboard, separate from import sprites.
sheet=Image.new('RGB',(1600,1320),'#E6EAEE'); d=ImageDraw.Draw(sheet)
label_font=ImageFont.truetype(str(fontdir/'Pretendard-Medium.otf'),18)
for i,(name,a) in enumerate(assets.items()):
    col=i%4; row=i//4; x=col*400; y=row*330
    for yy in range(y+34,y+310,16):
        for xx in range(x+12,x+388,16):
            d.rectangle([xx,yy,xx+15,yy+15],fill='#F7F8FA' if ((xx-x)//16+(yy-y)//16)%2 else '#D8DEE4')
    im=Image.open(OUT/a['file']).convert('RGBA'); im.thumbnail((350,260),Image.Resampling.LANCZOS)
    sheet.paste(im,(x+200-im.width//2,y+170-im.height//2),im)
    d.text((x+12,y+8),name,font=label_font,fill=INK)
sheet.save(OUT/'Asset_Overview.png')

checks=[]
for name,a in assets.items():
    im=Image.open(OUT/a['file'])
    assert list(im.size)==a['source_pixels']
    s=(OUT/a['svg']).read_text(encoding='utf-8')
    assert '<text' not in s and '<image' not in s and '<filter' not in s
    if name not in ['Sanctuary_BG','Sanctuary_Base']: assert im.convert('RGBA').getchannel('A').getextrema()[0]==0 or name=='Rule_Name'
    checks.append({'asset':name,'size':list(im.size),'text_free':True,'embedded_art':False,'blur_filter':False})
assert len({c['rect'][3] for c in characters})==1
assert all(Image.open(OUT/'Sanctuary_BG.png').getpixel((x,50))==Image.open(OUT/'Sanctuary_BG.png').getpixel((0,50)) for x in range(1920))
for t in texts:
    x,y,w,h=t['rect']; assert 0<=x<x+w<=1920 and 106<=y<y+h<=1080
for item in layout['external_assets'].values(): assert (OUT/item['file']).exists()
validation={'passed':True,'asset_count':len(assets),'checks':checks,'same_character_slot_height':930,'hud_top_plain':True,'text_widths_fit':True,'external_references_exist':True,'character_source_sha256_verified':True,'limitations':['Current Alesia source differs from requested oversized shield / short spear design. Source art preserved; runtime art replacement remains external.']}
(OUT/'validation.json').write_text(json.dumps(validation,ensure_ascii=False,indent=2),encoding='utf-8')
(OUT/'README.txt').write_text('''성소 후보 선택 v3 / SVG 분절 패키지

범위: 후보 선택 화면만. 상세 화면은 포함하지 않음.
좌표: 1920×1080, 좌상단 원점, rect = x,y,width,height.
PNG: UI 2배, 배경/전체 가이드 1배. SVG는 논리 크기 viewBox.
SVG/PNG에는 글자, 캐릭터, 잠긴 실루엣, HUD를 포함하지 않음.
TMP 글자: layout.json의 text_boxes. Pretendard 기준 크기, 색, 정렬, 자간.
자물쇠: 기존 LoadoutMockup/Extracted_Draft/Icon_Lock 재사용. 외부 참조.

배경 사용법:
  A. 고정: Sanctuary_BG 한 장. Hover 때 해당 면만 교체/겹침.
  B. 동적: Sanctuary_Base → 후보별 Panel → Overlay_Guides.
  A와 B를 중복으로 그리지 않음. 선택 경계선과 캐릭터는 그 위에 배치.
Panel_Locked_Right는 화면 오른쪽에서 잘린 모양을 맞춘 별도 조각.
Line_ActiveEdge는 원본 목업의 활성 면 오른쪽 경계(02 면 왼쪽 경계).

캐릭터: 프로젝트 원본 외부 참조, preserve aspect / contain / no mask.
세 후보 rect 높이 930px 동일. 넓은 무기는 contain 결과 표시 높이가 작아질 수 있음.
02는 현재 알레시아_Standing.png 실루엣으로 미리보기. 요청서의 1.5배 방패/짧은 창과 현재 원본의 무기 비율은 다르므로 원본을 변경하지 않았다.
잠금 문구 70%, 조건 55%, 조건은 두 줄까지. 조건 문자열은 런타임 바인딩.
Rule_Name은 흰색 tint용. 이름 선/소속에는 후보 테마색 적용.
동행 가득 상태는 상태 문구 변경 및 확인 버튼 disabled.

그림자: Shadow_Button을 버튼보다 오른쪽 아래 12px에 배치. 먹색 35%, blur 없음.
Unity Shadow를 쓰면 그림자 PNG는 끈다. 둘을 중복하지 않음.
상단 106px는 HUD 자리이며 배경 단색만 둠.
9-slice 없음. 면 클릭 판정은 bounding rect 대신 polygon 사용.

Sanctuary_Select_Mockup_v3.png는 제공 조각과 원본 아트로 조립한 검수 이미지.
Asset_Overview.png는 체크무늬 검수 시트로 게임에 넣지 않음.
validation.json: PNG 크기, 외부 경로, 텍스트 박스, 글자/원본 그림 미포함 검증.
Unity 씬, 임포터, meta 파일은 변경하지 않음.
''',encoding='utf-8')

zip_path=HERE/'Sanctuary_Select_v3_Package.zip'
with zipfile.ZipFile(zip_path,'w',zipfile.ZIP_DEFLATED) as z:
    z.write(HERE/'Sanctuary_Select_Mockup_v3.png','Sanctuary_Select_Mockup_v3.png')
    for path in OUT.rglob('*'):
        if path.is_file(): z.write(path,'Extracted_Draft/'+path.relative_to(OUT).as_posix())
print(json.dumps({'assets':len(assets),'mockup':str(HERE/'Sanctuary_Select_Mockup_v3.png'),'zip':str(zip_path),'validated':True},ensure_ascii=False))
