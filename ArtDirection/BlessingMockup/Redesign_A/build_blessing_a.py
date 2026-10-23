"""Native SVG UI delivery. No changes to scenes, importers or game art."""
from pathlib import Path
import json, subprocess, zipfile, math, hashlib
import numpy as np
from PIL import Image, ImageDraw, ImageFont

HERE=Path(__file__).resolve().parent; ROOT=HERE.parents[2]
OUT=HERE/'Extracted_Draft'; SD=OUT/'SVG'; SD.mkdir(parents=True,exist_ok=True)
assets=[]; jobs=[]; placements=[]; texts=[]
def asset(name,w,h,body,border=None,state='Normal',color=None):
    p=SD/(name+'.svg');p.write_text(f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}">{body}</svg>',encoding='utf-8')
    a=dict(name=name,file=name+'.png',svg='SVG/'+name+'.svg',source='../Blessing_A_Canonical.png',output_size=[w*2,h*2],logical_size=[w,h],png_scale=2,placement_1920_xy=None,nine_slice_lbrt=[v*2 for v in (border or [0,0,0,0])],nine_slice_logical_lbrt=border or [0,0,0,0],state=state,text_baked=False,shadow_baked=False,method='Editable SVG reconstruction; uniform face alpha; no raster art')
    if color:a['face_color']=color
    assets.append(a);jobs.append(dict(input=str(p),output=str(OUT/(name+'.png')),width=w*2))
def panel(w,h,fill,stroke,alpha=.9,dialogue=False):
    path=f'M24 1H{w-1}V{h-25}L{w-25} {h-1}H1V24Z' if dialogue else f'M1 1H{w-1}V{h-25}L{w-25} {h-1}H1Z'
    # The overall group alpha is uniform; line strokes do not stack translucent fills.
    return f'<g opacity="{alpha}"><path d="{path}" fill="{fill}" stroke="{stroke}" stroke-width="{2 if stroke=="#49C6E5" else 1.2}"/></g>'
normal='#0D1420';hover='#192A3B';disabled='#121923'
asset('Panel_Dialogue',920,174,panel(920,174,normal,'#8995A3',.9,True),[42,32,34,36],color={'srgb_hex':normal,'alpha':.9})
for state,fill,edge in [('Normal',normal,'#7D8C9C'),('Hover',hover,'#49C6E5'),('Disabled',disabled,'#52606F')]:
    asset('Panel_Option_'+state,920,136,panel(920,136,fill,edge),[30,32,34,24],state,color={'srgb_hex':fill,'alpha':.9})
    diamond=f'<g opacity=".9"><path d="M56 3L109 56L56 109L3 56Z" fill="{fill}" stroke="{edge}" stroke-width="{2 if state=="Hover" else 1.4}"/><path d="M30 85L28 112L45 99" fill="none" stroke="{edge}" stroke-width="1.2"/></g>'
    asset('Option_IconDiamond_'+state,112,114,diamond,state=state)
for state,col in [('Normal','#E8EDF2'),('Hover','#49C6E5')]:
    asset('Icon_Chevron_'+state,18,28,f'<path d="M3 3L14 14L3 25" fill="none" stroke="{col}" stroke-width="2"/>',state=state)
asset('Panel_Nameplate',470,190,'<g opacity=".9"><path d="M1 1H445L469 25V189H1Z" fill="#0D1420" stroke="#8995A3" stroke-width="1.2"/></g>',[24,24,36,36],color={'srgb_hex':normal,'alpha':.9})
runner=HERE/'render_blessing_assets.cjs';runner.write_text("const fs=require('fs');const{Resvg}=require('C:/Users/SKYLIGHT/.codex/tmp/currency-f-render/node_modules/@resvg/resvg-js');for(const j of JSON.parse(fs.readFileSync(process.argv[2],'utf8')))fs.writeFileSync(j.output,new Resvg(fs.readFileSync(j.input),{fitTo:{mode:'width',value:j.width}}).render().asPng());",encoding='utf-8')
jp=HERE/'render_jobs.json';jp.write_text(json.dumps(jobs),encoding='utf-8');subprocess.run(['node',str(runner),str(jp)],check=True);jp.unlink()
byname={a['name']:a for a in assets}
old=json.loads((HERE.parent/'Extracted/layout.json').read_text(encoding='utf-8'));oldmap={a['name']:a for a in old['assets']}
external={}
for name in ['Corner_Marks','Option_Divider','Nameplate_AccentLine','Icon_Plus']:
    external[name]=dict(file='../../Extracted/'+name+'.png',output_size=oldmap[name]['output_size'],included=False,method='unchanged v1; optional runtime tint')
for key,file in [('purify','UI_Blessing_Cleanse_White.png'),('refine','UI_Blessing_Refine_White.png'),('supply','UI_Blessing_Supply_White.png'),('commune','UI_Blessing_Communion_White.png')]:
    external[key]=dict(file='../../../../Assets/Art/Blessing/UI/'+file,included=False,logical_size=[52,52],preserve_aspect=True)
def place(id,key,rect,z=10,tint=None,opacity=1):
    placements.append(dict(id=id,asset=key,rect=rect,z=z,tint=tint,opacity=opacity,external=key in external))
def text(id,string,rect,size=22,color='#F1F4F7',weight=400,spacing=0,**kw):
    texts.append(dict(id=id,text=string,rect=rect,font_size=size,color=color,font_weight=weight,alignment='left',character_spacing_px=spacing,**kw))
place('corners','Corner_Marks',[0,0,1920,1080],2,tint='#B1BCC6',opacity=.55)
place('dialogue','Panel_Dialogue',[966,214,920,174])
place('dialogue_plus','Icon_Plus',[989,236,28,28],11,tint='#AFBBC8',opacity=.65)
place('dialogue_rule','Option_Divider',[1040,257,790,1],11,tint='#A3B2C2',opacity=.4)
place('nameplate','Panel_Nameplate',[36,842,470,190])
place('name_accent','Nameplate_AccentLine',[64,957,224,2],11)
place('name_plus_top','Icon_Plus',[460,860,20,20],11,tint='#AABACA',opacity=.7)
place('name_plus_bottom','Icon_Plus',[460,1001,20,20],11,tint='#AABACA',opacity=.7)
text('title','BLESSING',[94,96,280,25],20,weight=600,spacing=5)
text('subtitle','BETWEEN REBIRTH AND DEATH',[94,133,360,22],12,weight=500,spacing=1.5)
text('encounter_label','VERTEX / ENCOUNTER LOG',[1040,235,760,22],13,'#A9B7C7',500,2)
dialogue_lines=['재회를 축복할지, 아니면 첫 대면을 환영해야 할지 알 수 없는 일이야.','버텍스에 온 것을 환영해, 탐색자. 후후, 여기는 버텍스가 아니지만.']
text('dialogue_text','\n'.join(dialogue_lines),[1046,282,792,90],22,weight=500,max_lines=3,line_height=29,binding='BlessingView.dialogueText',overflow='page; do not resize panel')
text('entity_label','ENTITY / MACHINA',[64,862,360,23],15,weight=500,spacing=2)
text('name','마키나',[64,904,360,47],44,weight=700)
text('intro','재생과 죽음의 경계에서',[64,979,375,31],23,weight=500)
option_defs=[('purify','정화','PURIFY','덱에서 불필요한 카드 1장을 완전히 제거합니다.'),('refine','연마','REFINE','기본 카드 2장을 선택하여 강화합니다.'),('supply','보급','SUPPLY','모험에 도움이 되는 소모품 아이템 2개를\n가방에 획득합니다.'),('commune','교감','COMMUNE','마키나와 대화를 나누어 친밀도를 쌓습니다.')]
instances=[]
for i,(key,title,en,desc) in enumerate(option_defs):
    x,y=966,410+158*i
    r=dict(index=i+1,id=key,panel='Panel_Option_Normal',position=[x,y],panel_rect=[x,y,920,136],icon_cell='Option_IconDiamond_Normal',icon_diamond_rect=[x+12,y+10,112,114],icon_rect=[x+42,y+40,52,52],number_rect=[x+148,y+16,60,21],title_rect=[x+148,y+42,144,53],english_rect=[x+148,y+102,175,22],divider_rect=[x+302,y+32,2,70],description_rect=[x+338,y+42,526,75],chevron_rect=[x+870,y+54,18,28],default_state='Normal',content_group_includes_panel=True)
    instances.append(r)
    place(key+'_panel','Panel_Option_Normal',r['panel_rect'])
    place(key+'_diamond','Option_IconDiamond_Normal',r['icon_diamond_rect'],11)
    place(key+'_icon',key,r['icon_rect'],12)
    place(key+'_divider','Option_Divider',r['divider_rect'],11,tint='#A5B6C5',opacity=.5)
    place(key+'_chevron','Icon_Chevron_Normal',r['chevron_rect'],12)
    text(key+'_number',f'{i+1:02}',r['number_rect'],15,weight=600,spacing=1)
    text(key+'_title',title,r['title_rect'],42,weight=700,first_syllable_emphasis=False)
    text(key+'_english',en,r['english_rect'],14,'#A7B8CA',600,2)
    text(key+'_description',desc,r['description_rect'],21,weight=500,max_lines=2,line_height=28)

def linearize(a):return np.where(a<=.04045,a/12.92,((a+.055)/1.055)**2.4)
def srgb(a):return np.where(a<=.0031308,a*12.92,1.055*np.maximum(a,0)**(1/2.4)-.055)
def blend_linear(canvas,im,xy):
    x,y=xy;w,h=im.size;bg=np.asarray(canvas.crop((x,y,x+w,y+h)).convert('RGB'),dtype=np.float32)/255
    src=np.asarray(im,dtype=np.float32)/255;alpha=src[:,:,3:4]
    out=srgb(linearize(src[:,:,:3])*alpha+linearize(bg)*(1-alpha))
    canvas.paste(Image.fromarray(np.uint8(np.clip(out*255+.5,0,255))).convert('RGBA'),(x,y))

# Reassemble existing game art continuously: the capture hides torso/background under old UI.
capture=Image.open(HERE.parent/'Alternatives_v2/Reference_Current.png').convert('RGBA').resize((1920,1080),Image.Resampling.LANCZOS)
background=Image.open(ROOT/'Assets/Art/Blessing/Machina/Backgrounds.png').convert('RGBA').resize((1920,1080),Image.Resampling.LANCZOS)
frame=Image.open(ROOT/'Assets/Art/Blessing/Machina/machina_idle_sprite_64_sheet.png').convert('RGBA').crop((0,0,768,1344)).resize((1229,2150),Image.Resampling.LANCZOS)
clean=background.copy();clean.alpha_composite(frame,(-100,-169))
scene=clean.copy()
# Keep the actual capture's runtime HUD. Left blessing title is supplied separately by TMP.
hudmask=Image.new('L',(1920,1080));hd=ImageDraw.Draw(hudmask)
hd.polygon([(306,0),(1618,0),(1560,73),(354,73)],fill=255)
for rect in [(1704,22,1789,108),(1807,22,1895,108),(1835,118,1896,178)]:hd.rectangle(rect,fill=255)
scene=Image.composite(capture,scene,hudmask)
base_scene=scene.copy()
def paint(p,canvas):
    a=external[p['asset']] if p['external'] else byname[p['asset']]
    im=Image.open(OUT/a['file']).convert('RGBA').resize(tuple(p['rect'][2:]),Image.Resampling.LANCZOS)
    if p['tint']:
        tinted=Image.new('RGBA',im.size,p['tint']);tinted.putalpha(im.getchannel('A'));im=tinted
    if p['opacity']!=1:im.putalpha(im.getchannel('A').point(lambda v:round(v*p['opacity'])))
    blend_linear(canvas,im,tuple(p['rect'][:2]))
fontdir=ROOT/'Assets/Font/Pretendard'
def draw_text(t,canvas,dy=0):
    draw=ImageDraw.Draw(canvas);x,y,w,h=t['rect'];y+=dy
    font=ImageFont.truetype(str(fontdir/('Pretendard-Bold.otf' if t['font_weight']>=700 else 'Pretendard-SemiBold.otf' if t['font_weight']>=600 else 'Pretendard-Medium.otf' if t['font_weight']>=500 else 'Pretendard-Regular.otf')),t['font_size'])
    widths=[]
    for line in t['text'].split('\n'):
        xx=x;sp=t['character_spacing_px'];tw=sum(draw.textlength(ch,font=font) for ch in line)+max(0,len(line)-1)*sp
        assert tw<=w,(t['id'],tw,w);widths.append(round(tw,2))
        for ch in line:draw.text((xx,y),ch,font=font,fill=t['color'],anchor='lt');xx+=draw.textlength(ch,font=font)+sp
        y+=t.get('line_height',t['font_size']+6)
    t['preview_line_widths']=widths
for p in sorted(placements,key=lambda a:a['z']):paint(p,scene)
for t in texts:draw_text(t,scene)
scene.convert('RGB').save(HERE/'Blessing_A_Mockup_v2.png')

# Separate state sheet makes normal/hover/disabled behavior reviewable without changing default mockup.
statesheet=Image.open(HERE/'Blessing_A_Mockup_v2.png').convert('RGBA')
for idx,state in [(0,'Hover'),(1,'Disabled')]:
    r=instances[idx];x,y,w,h=r['panel_rect'];rect=(x-8,y,x+w,y+h)
    # Restore the same capture/clean scene underneath before replacing a full row.
    underneath=base_scene.crop(rect);statesheet.paste(underneath,rect[:2])
    for p in [a for a in placements if a['id'].startswith(r['id']+'_')]:
        q=dict(p,rect=list(p['rect']));q['rect'][0]-=8 if state=='Hover' else 0
        if q['asset']=='Panel_Option_Normal':q['asset']='Panel_Option_'+state
        if q['asset']=='Option_IconDiamond_Normal':q['asset']='Option_IconDiamond_'+state
        if q['asset']=='Icon_Chevron_Normal' and state=='Hover':q['asset']='Icon_Chevron_Hover'
        if state=='Disabled' and q['id'].endswith('_icon'):q['opacity']=.5
        paint(q,statesheet)
    for t in [a for a in texts if a['id'].startswith(r['id']+'_')]:
        tt=dict(t,rect=list(t['rect']));tt['rect'][0]-=8 if state=='Hover' else 0
        if state=='Disabled':tt['color']='#8C949E'
        draw_text(tt,statesheet)
statesheet.convert('RGB').save(OUT/'States_Preview.png')

color_info={}
for name,fill in [('Normal',normal),('Hover',hover),('Disabled',disabled)]:
    rgb=np.array([int(fill[i:i+2],16) for i in (1,3,5)])/255
    color_info[name]={'srgb_hex':fill,'rgba_srgb':[round(float(x),6) for x in rgb]+[.9],'rgb_linear':[round(float(x),7) for x in linearize(rgb)],'alpha':.9,'alpha_byte':230,'image_color':[1,1,1,1],'texture_srgb':True,'alpha_is_transparency':True,'note':'RGB already in PNG. Use white Image tint at alpha 1; do not apply .9 a second time.'}
layout=dict(version=2,source='Blessing_A_Canonical.png',source_canvas=[1672,941],design_canvas=[1920,1080],canvas=[1920,1080],coordinates='top-left origin; rect=x,y,width,height; PNG output_size and nine_slice_lbrt in 2x SOURCE pixels',origin='top-left',assets=assets,external_assets=external,placements=placements,text_boxes=texts,option_instances=instances,
 hover_runtime_offset=[-8,0],hud_reserved_rect=[0,0,1920,80],portrait={'speakerAvatarImage_enabled':False},
 spacing={'row_height':136,'row_gap':22,'row_pitch':158,'dialogue_to_first_row_gap':22},
 dialogue={'panel_rect':[966,214,920,174],'text_rect':[1046,282,792,90],'max_lines':3,'line_height':29,'height_mode':'fixed','three_lines':'same panel and row positions; text box supports 3 lines','overflow':'paginate at 3 lines; next page uses existing advance behavior','header_rule_rect':[1040,257,790,1]},
 character={'rect':[-100,-169,1229,2150],'visible_bounds_approx':[60,65,920,1015],'runtime':'BlessingView.characterImage; preserve current placement/rig','included':False,'overlap_check':'Face and hand outside x966 menu/dialogue boundary. Nameplate intentionally overlays lower torso as canonical.','preview':'Original game background and sprite first frame, positioned using supplied capture. Runtime HUD comes from the capture. No generative repainting.'},
 color_intent=color_info,
 states={'default':'Normal for all four rows','Normal':{'panel':'Panel_Option_Normal','diamond':'Option_IconDiamond_Normal','chevron':'Icon_Chevron_Normal','text_color':'#F1F4F7','title_font_size':42,'title_first_syllable_emphasis':False},'Hover':{'panel':'Panel_Option_Hover','diamond':'Option_IconDiamond_Hover','chevron':'Icon_Chevron_Hover','outline_logical_px':2,'outline_color':'#49C6E5','offset':[-8,0],'move':'Entire row: panel, diamond, icon, number, title, English, divider, description, chevron','glow':False,'bloom':False},'Disabled':{'panel':'Panel_Option_Disabled','diamond':'Option_IconDiamond_Disabled','chevron':'Icon_Chevron_Normal','text_color':'#8C949E','icon_alpha':.5,'interactable':False,'offset':[0,0]},'DialogueRunning':{'show':['dialogue','nameplate','existing scenery','HUD'],'hide':['all option row groups'],'portrait_enabled':False}},
 rendering={'alpha':'straight; RGB stored sRGB, alpha linear','preview_compositing':'linear-light source-over to approximate Unity Linear UI','texture_srgb':True,'image_tint':[1,1,1,1],'ui_png_scale':2,'nine_slice_order':'left,bottom,right,top SOURCE pixels','material':'default UI material, no additive passes, no glow, no shadow','raycast':'row button receives; decorative elements raycastTarget=false'},
 preview_sources={'capture':'../../Alternatives_v2/Reference_Current.png','background':'Assets/Art/Blessing/Machina/Backgrounds.png','frame':'Assets/Art/Blessing/Machina/machina_idle_sprite_64_sheet.png; first sprite 768x1344 top-left; metadata Unity rect [0,9408,768,1344]','limitation':'Capture contains old UI over torso/background. For a continuous clean scene, preview rebuilds from original game background/frame rather than retaining occluding pixels. Capture supplies placement reference and HUD. Animated pose is not pixel-identical.'},
 excluded=['character','background','moon','petals','HUD','icons','text','numbers','portrait'],notes=['Nameplate A differs from v1 opaque plate: rebuilt with uniform .9 alpha. Accent line/plus/divider/corners reuse existing sprites.','No first-syllable styling. No permanently highlighted row.','Hover shift must target whole row container, not only content; existing component requires placement integration by game implementer.','No Unity scene, importer, prefab or meta changes.'])
for a in assets:
    matches=[p for p in placements if p['asset']==a['name']]
    a['placement_1920_xy']=matches[0]['rect'][:2] if matches else None
(OUT/'layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf-8')

sheet=Image.new('RGB',(1500,math.ceil(len(assets)/3)*270),'#6E7D8C');dr=ImageDraw.Draw(sheet);font=ImageFont.truetype(str(fontdir/'Pretendard-Medium.otf'),18)
for i,a in enumerate(assets):
    x=(i%3)*500;y=(i//3)*270
    for yy in range(y+32,y+260,16):
        for xx in range(x+10,x+490,16):dr.rectangle((xx,yy,xx+15,yy+15),fill='#E3E8EE' if (xx//16+yy//16)%2 else '#ABB7C4')
    im=Image.open(OUT/a['file']).convert('RGBA');im.thumbnail((460,208),Image.Resampling.LANCZOS);sheet.paste(im,(x+250-im.width//2,y+146-im.height//2),im);dr.text((x+10,y+5),a['name'],font=font,fill='white')
sheet.save(OUT/'Asset_Overview.png')
checks=[]
for a in assets:
    im=Image.open(OUT/a['file']).convert('RGBA');assert list(im.size)==a['output_size']
    src=(OUT/a['svg']).read_text();assert not any(t in src for t in ['<text','<image','<filter'])
    if a['name'].startswith('Panel_'):
        alpha=im.getchannel('A'); w,h=im.size; assert alpha.crop((80,80,w-80,h-80)).getextrema()==(230,230),(a['name'],alpha.crop((80,80,w-80,h-80)).getextrema())
    b=a['nine_slice_lbrt'];assert b[0]+b[2]<im.width and b[1]+b[3]<im.height
    checks.append({'name':a['name'],'text_free':True,'uniform_face_alpha':True,'size_ok':True,'nine_slice_ok':True})
for k,a in external.items():assert (OUT/a['file']).exists(),k
for t in texts:
    x,y,w,h=t['rect'];assert x>=0 and y>=80 and x+w<=1920 and y+h<=1080,t['id']
assert len(instances)==4 and all(i['panel']=='Panel_Option_Normal' for i in instances)
validation={'passed':True,'new_svg_png_pairs':len(assets),'external_reuses':len(external),'checks':checks,'all_rows_normal':True,'title_size_uniform':42,'capture_sha256':hashlib.sha256((HERE.parent/'Alternatives_v2/Reference_Current.png').read_bytes()).hexdigest(),'text_widths_fit':True,'external_refs_exist':True,'preview_only_restoration_disclosed':True}
(OUT/'validation.json').write_text(json.dumps(validation,ensure_ascii=False,indent=2),encoding='utf-8')
(OUT/'README.txt').write_text('''축복 A / SPLIT_REQUEST_A

1920×1080, 좌상단 원점. rect=x,y,width,height. SVG 논리 크기, PNG는2배.
신규10종. 모든 조각에서 글자·숫자·인물·배경·아이콘·빛·그림자 제외.
layout은 v1 호환 assets 배열, output_size, placement_1920_xy, nine_slice_lbrt, option_instances와 TMP text_boxes 등을 포함.

기본은 네 줄 모두 Normal. 제목은 모두 흰색 42px, 첫 글자 강조 없음.
Hover는 2px 청록선과 밝은 바탕, 줄 전체 왼쪽 8px 이동. glow/bloom/Shadow 없음.
Disabled는 판/마름모 교체 + 글자 #8C949E, 아이콘 alpha .5. Hover 이동 해제.
대사 진행 중 네 선택지 그룹 숨김. speakerAvatarImage는 꺼 둠.
대사 판은 174px 고정, 본문 90px / line-height29로 최대3줄. 4줄부터 페이지 분리, 아래 줄 위치 불변.

알파: 판 면 전체 0.90(바이트230). PNG RGB가 이미 의도색이므로 Image tint=(1,1,1,1).
Image 색에 다시 .9 알파를 곱하면 의도보다 밝아진다. Texture sRGB=true, straight alpha.
layout color_intent에 sRGB float와 linear RGB 값을 모두 제공.
미리보기는 Linear 색 공간의 source-over 계산 사용. 실제 프로젝트 머티리얼/톤매핑에 따라 차이는 남는다.

9-slice: assets[].nine_slice_lbrt는 2배 PNG SOURCE 기준 L/B/R/T. 논리값은 별도 nine_slice_logical_lbrt.
마름모/chevron은 고정 크기, sliced 금지.
외부 참조 8종: v1 Corner_Marks/Option_Divider/Nameplate_AccentLine/Icon_Plus 및 게임 흰색 아이콘4종.
외부 파일 중복 포함하지 않음. 원래 Redesign_A 폴더 아래에 풀어 상대경로 유지.
Divider/Plus는 기존 알파를 보존하면서 layout tint 적용. 마키나 이름판은 새 반투명 판이며 v1과 달라 재작성.

검수 목업은 캡처를 배치 기준으로 사용하고 상단 HUD를 캡처에서 재사용.
구 UI가 마키나 몸과 배경을 가리므로, 이음매 없이 게임 배경/리깅 첫프레임 원본으로 전체 장면을 조립.
당시 게임 애니메이션 캡처와 완전히 동일한 픽셀은 아님. 생성 모델로 원본을 새로 그리지 않았다.
States_Preview: 01 Hover / 02 Disabled 상태 확인용. 기본 목업은 전부 Normal.
Asset_Overview는 임포트용 아님. 캐릭터/배경/아이콘/모서리 원본은 패키지 제외.
Unity 적용 및 importer/meta 편집 없음.
''',encoding='utf-8')
with zipfile.ZipFile(HERE/'Blessing_A_v2_Package.zip','w',zipfile.ZIP_DEFLATED) as z:
    z.write(HERE/'Blessing_A_Mockup_v2.png','Blessing_A_Mockup_v2.png')
    for p in OUT.rglob('*'):
        if p.is_file():z.write(p,'Extracted_Draft/'+p.relative_to(OUT).as_posix())
print(json.dumps({'pairs':len(assets),'external':len(external),'validation':'passed'},ensure_ascii=False))
