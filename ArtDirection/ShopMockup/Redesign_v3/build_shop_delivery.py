"""Build shop v4 preview and text-free SVG/PNG delivery; never changes Unity assets."""
from pathlib import Path
import json, subprocess, re, hashlib, zipfile, math
from PIL import Image, ImageDraw, ImageFont

HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
OUT=HERE/'Extracted_Draft'; SVG=OUT/'SVG'; SVG.mkdir(parents=True,exist_ok=True)
INK='#16181B'; CYAN='#0DB8F2'; GRAY='#68737D'; RED='#FF0000'
assets={}; jobs=[]; places=[]; texts=[]; sources=[]
def asset(name,w,h,body,scale=2,**extra):
    p=SVG/(name+'.svg')
    p.write_text(f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}">{body}</svg>',encoding='utf-8')
    assets[name]=dict(file=name+'.png',svg='SVG/'+name+'.svg',logical_size=[w,h],source_pixels=[w*scale,h*scale],scale=scale,pivot=[.5,.5],text_baked=False,shadow_baked=False,method='SVG reconstruction from approved v3',**extra)
    jobs.append(dict(input=str(p),output=str(OUT/(name+'.png')),width=w*scale))
def shape(w,h,cut=16,fill='#F2F4F5',stroke='#B7C1CA'):
    return f'<path d="M{cut} 1H{w-cut}L{w-1} {cut}V{h-cut}L{w-cut} {h-1}H{cut}L1 {h-cut}V{cut}Z" fill="{fill}" stroke="{stroke}" stroke-width="1.5"/>'
bg='<path d="M820 106H1920V1080H690V920L736 864V210Z" fill="#DEE4EA"/><path d="M1516 106H1920V1080H1650L1460 836Z" fill="#BBC6D1"/><path d="M820 106H1480L1360 270H736V210Z" fill="#EDF1F4"/><path d="M690 1015L1460 836L1650 1080H748Z" fill="#CFD7DF"/>'
asset('Shop_BG',1920,1080,bg,scale=1)
asset('Panel_Cards',774,770,shape(774,770,28,'#F2F4F6','#C7D0D7'))
asset('Panel_Supply',360,630,shape(360,630,24,'#E9EEF2','#C0CAD3'))
asset('Panel_Credits',360,112,shape(360,112,12)+'<path d="M20 102H300" stroke="#0DB8F2" stroke-width="4"/>')
asset('Plate_Shopkeeper',590,148,shape(590,148,20,INK,'#4E5C67')+'<path d="M516 8H550" stroke="#0DB8F2" stroke-width="3"/>')
for state in ['Normal','Hover','Disabled']:
    fill='#F2F4F5' if state=='Normal' else '#FAFCFD' if state=='Hover' else '#D4DCE3'
    stroke=CYAN if state=='Hover' else '#B7C1CA' if state=='Normal' else '#C2CAD1'
    asset('Panel_Service_'+state,774,130,shape(774,130,24,fill,stroke))
for state in ['Normal','Hover','Pressed']:
    asset('Button_Leave_'+state,360,128,shape(360,128,24,{'Normal':'#F2F4F5','Hover':'#FFFFFF','Pressed':'#D9E2E9'}[state],CYAN if state!='Normal' else '#B7C1CA'))
for family,w,h,cut in [('Plinth_Card',222,326,18),('ItemCell',138,132,12),('PriceTag',176,38,9)]:
    for state in ['Normal','Hover','Unaffordable']:
        fill={'Normal':'#F1F4F6','Hover':'#FFFFFF','Unaffordable':'#DDE3E8' if family!='PriceTag' else '#E4E8EC'}[state]
        stroke={'Normal':'#AEBBC5','Hover':CYAN,'Unaffordable':'#CDD5DB'}[state]
        extra={}
        if family=='PriceTag': extra['nine_slice']={'logical_pixels_left_bottom_right_top':[12,12,12,12],'source_pixels_left_bottom_right_top':[24,24,24,24],'minimum_logical_size':[25,25],'unity_image_type':'Sliced','fill_center':True}
        asset(f'{family}_{state}',w,h,shape(w,h,cut,fill,stroke),**extra)
jobpath=HERE/'render_jobs.json'; jobpath.write_text(json.dumps(jobs),encoding='utf-8')
runner=HERE/'render_shop_assets.cjs'
runner.write_text("const fs=require('fs');const{Resvg}=require('C:/Users/SKYLIGHT/.codex/tmp/currency-f-render/node_modules/@resvg/resvg-js');for(const j of JSON.parse(fs.readFileSync(process.argv[2],'utf8')))fs.writeFileSync(j.output,new Resvg(fs.readFileSync(j.input),{fitTo:{mode:'width',value:j.width}}).render().asPng());",encoding='utf-8')
subprocess.run(['node',str(runner),str(jobpath)],check=True); jobpath.unlink()

external={}
for name in ['Icon_Arrow','Icon_Scissors','Icon_RemoveCard','Icon_Leave','Icon_Close','Icon_Credit','SoldOut_Band','SoldOut_Hatching','SoldOut_Stamp','Watermark_Cards','Watermark_Supply','Watermark_Service','Button_Close_Normal','Button_Close_Hover','Button_Close_Pressed']:
    file='../../Extracted/'+name+'.png'; p=OUT/file
    external[name]=dict(file=file,included=False,source_pixels=list(Image.open(p).size),reuse='unchanged v2')

def place(id,asset,rect,z=10,shadow=0,opacity=1,external_asset=False):
    p=dict(id=id,rect=rect,z=z,opacity=opacity)
    p['external_asset' if external_asset else 'asset']=asset
    if shadow: p['shadow']={'offset_canvas':[shadow,shadow],'unity_effect_distance':[shadow,-shadow],'color':INK,'opacity':.30 if shadow==12 else .24,'blur':0,'implementation':'UnityEngine.UI.Shadow; do not add a shadow PNG'}
    places.append(p);return p
def txt(id,string,rect,size=22,color=INK,weight=400,align='left',spacing=0,**kw):
    texts.append(dict(id=id,text=string,rect=rect,font_size=size,color=color,font_weight=weight,alignment=align,character_spacing_px=spacing,**kw))
place('background','Shop_BG',[0,0,1920,1080],0)
place('cards','Panel_Cards',[736,116,774,770],10,12)
place('supply','Panel_Supply',[1532,256,360,630],10,12)
place('credits','Panel_Credits',[1532,124,360,112],10,12)
place('keeper_plate','Plate_Shopkeeper',[82,894,590,148],40,12)
place('service','Panel_Service_Disabled',[736,914,774,130],20,12)
place('leave','Button_Leave_Normal',[1532,916,360,128],20,12)
place('watermark_cards','Watermark_Cards',[1378,138,92,58],11,opacity=.18,external_asset=True)
place('watermark_supply','Watermark_Supply',[1762,282,100,96],11,opacity=.45,external_asset=True)
place('watermark_service','Watermark_Service',[1070,932,110,86],21,opacity=.45,external_asset=True)
place('remove_icon','Icon_RemoveCard',[1204,949,42,52],22,opacity=.6,external_asset=True)
place('scissors','Icon_Scissors',[1250,959,22,32],22,opacity=.6,external_asset=True)
place('leave_icon','Icon_Leave',[1570,949,40,56],22,external_asset=True)
place('leave_arrow','Icon_Arrow',[1842,965,16,24],22,external_asset=True)
place('credit_diamond','Icon_Credit',[1682,165,22,22],22,external_asset=True)
place('close','Button_Close_Normal',[1836,147,36,36],22,external_asset=True)
place('close_icon','Icon_Close',[1846,157,16,16],23,external_asset=True)
txt('title','보급 상점',[86,118,500,64],52,weight=700)
txt('title_en','FIELD SUPPLY',[88,185,400,25],17,GRAY,500,spacing=2)
txt('keeper_en','SHOPKEEPER',[112,910,410,22],14,'#BCC7CE',500,spacing=3)
txt('keeper_name','토냐',[112,941,350,45],40,'#FFFFFF',700,binding='ShopkeeperData.entityName')
txt('dialogue','필요한 게 있는 거예요?',[112,995,510,32],25,'#FFFFFF',500,binding='ShopkeeperData.greetingLine',overflow='ellipsis',max_lines=1)
for id,n,title,en,rects in [
    ('cards','01','카드','CARD MODULE',[[774,140,40,28],[844,136,160,58],[1020,160,260,24]]),
    ('supply','02','보급품','SUPPLY',[[1562,284,40,28],[1562,330,196,54],[1758,355,110,23]]),
    ('service','03','카드 제거','REMOVE SERVICE',[[774,940,40,28],[832,941,242,52],[834,1001,245,22]])]:
    txt(id+'_number',n,rects[0],20,GRAY)
    txt(id+'_title',title,rects[1],44,weight=700)
    txt(id+'_en',en,rects[2],14,GRAY,500,spacing=2)
txt('credits_label','CREDITS',[1560,148,200,24],16,GRAY,500,spacing=2)
txt('credits_value','33',[1714,150,110,65],56,INK,700,binding='wallet.credits')
txt('leave_label','나가기',[1638,946,190,51],38,weight=700)
txt('leave_en','LEAVE',[1640,1001,160,22],15,GRAY,500,spacing=3)

def price(id,rect,value):
    x,y,w,h=rect
    place(id+'_tag','PriceTag_Unaffordable',rect,24,6)
    place(id+'_diamond','Icon_Credit',[x+12,y+11,16,16],25,external_asset=True)
    place(id+'_arrow','Icon_Arrow',[x+w-21,y+12,9,14],25,external_asset=True)
    txt(id+'_value',str(value),[x+35,y+5,w-64,30],24,RED,600,'center',binding=id+'.price',color_binding='ShopStockEntry.unaffordableColor')

cards=[]
capture=Image.open(HERE/'Reference_Current.png').convert('RGBA')
# Exact game screenshot, not generated v3 card art. Coordinates are native screenshot pixels.
capture_boxes=[(665,187,801,395),(848,187,984,395),(1030,187,1166,395),(665,435,801,643),(848,435,984,643),(1030,435,1166,643)]
for i,(name,value) in enumerate(zip(['견제 초탄','전진사격','임시 재장전','Q.E.D','빠른 후퇴','후퇴'],[46,47,53,49,50,46])):
    x=774+(i%3)*244; y=210+(i//3)*334
    plinth=[x,y,222,326]; card=[x+16,y+8,190,276]; tag=[x+23,y+284,176,38]
    place(f'card_{i}_plinth','Plinth_Card_Unaffordable',plinth,20,8)
    price(f'card_{i}',tag,value)
    cards.append(dict(id=f'card_{i}',name=name,plinth_rect=plinth,card_rect=card,price_rect=tag,price=value,state='Unaffordable',runtime='existing game card prefab; preserve aspect and prefab content; center in card_rect',capture_source='../Reference_Current.png',capture_rect_ltrb=list(capture_boxes[i]),capture_included=False))

items=[]
item_defs=[('전투 부스터','전투 부스터',140),('최루탄','최루탄',162),('열화 수류탄','열화 수류탄',243),('가스 마스크','가스 마스크',153)]
for i,(file,name,value) in enumerate(item_defs):
    data=(ROOT/'Assets/Data/Items'/f'{file}.asset').read_text(encoding='utf-8-sig')
    guid=re.search(r'itemIcon:.*guid: (\w+)',data).group(1)
    match=[p for p in (ROOT/'Assets/Art/Items').glob('*.meta') if f'guid: {guid}' in p.read_text(encoding='utf-8-sig')]
    assert len(match)==1
    source=match[0].with_suffix(''); relative=source.relative_to(ROOT).as_posix()
    x=1562+(i%2)*164; y=412+(i//2)*244
    cell=[x,y,138,132]; icon=[x+16,y+12,106,108]; namerect=[x,y+142,138,24]; tag=[x,y+174,138,38]
    place(f'item_{i}_cell','ItemCell_Unaffordable',cell,20,8)
    txt(f'item_{i}_name',name,namerect,16,INK,500,'center',binding=f'items.{i}.itemName',overflow='ellipsis',max_lines=1)
    price(f'item_{i}',tag,value)
    items.append(dict(id=f'item_{i}',name=name,source=relative,cell_rect=cell,icon_rect=icon,name_rect=namerect,price_rect=tag,price=value,state='Unaffordable',asset_included=False,preserve_aspect=True,tooltip='existing game tooltip'))
price('service',[1286,957,198,44],75)

external_art={
 'scene':{'source':'Assets/Art/Backgrounds/Act1_BattleBackground_Clean.png','rect':[0,0,1920,1080],'fit':'cover'},
 'keeper':{'source':'Assets/Art/Shop/1막 상점주인 복사 2.png','rect':[250,322,430,548],'source_crop_ltrb':[0,190,1088,1578],'fit':'contain','binding':'ShopkeeperData / current chapter standing'},
 'counter':{'source':'Assets/Art/Shop/ShopCounter_WhiteVertex.png','rect':[0,804,1460,321],'source_crop_ltrb':[13,337,1955,764],'fit':'contain main alpha bounds; clipped by viewport/right panels'}
}
for a in external_art.values():
    a['asset_included']=False;a['sha256']=hashlib.sha256((ROOT/a['source']).read_bytes()).hexdigest()

canvas=Image.open(ROOT/external_art['scene']['source']).convert('RGBA').resize((1920,1080),Image.Resampling.LANCZOS)
def contain(im,rect):
    x,y,w,h=rect;s=min(w/im.width,h/im.height);size=(round(im.width*s),round(im.height*s))
    return im.resize(size,Image.Resampling.LANCZOS),(x+(w-size[0])//2,y+(h-size[1])//2)
counter=Image.open(ROOT/external_art['counter']['source']).convert('RGBA'); counter=counter.crop(tuple(external_art['counter']['source_crop_ltrb']))
im,pos=contain(counter,external_art['counter']['rect']);canvas.alpha_composite(im,pos)
keeper=Image.open(ROOT/external_art['keeper']['source']).convert('RGBA').crop(tuple(external_art['keeper']['source_crop_ltrb']))
im,pos=contain(keeper,external_art['keeper']['rect']);canvas.alpha_composite(im,pos)

def slice9(im,size,border=24):
    w,h=size; sw,sh=im.size;b=border
    dst=Image.new('RGBA',size)
    xs=[0,b,sw-b,sw];ys=[0,b,sh-b,sh];dx=[0,b,w-b,w];dy=[0,b,h-b,h]
    for iy in range(3):
        for ix in range(3):
            region=im.crop((xs[ix],ys[iy],xs[ix+1],ys[iy+1])); region=region.resize((dx[ix+1]-dx[ix],dy[iy+1]-dy[iy]),Image.Resampling.LANCZOS);dst.alpha_composite(region,(dx[ix],dy[iy]))
    return dst
def render_place(p):
    key=p.get('asset',p.get('external_asset'));a=(assets if 'asset' in p else external)[key]
    im=Image.open(OUT/a['file']).convert('RGBA');x,y,w,h=p['rect']
    if key.startswith('PriceTag'):
        im=slice9(im,(w*2,h*2)).resize((w,h),Image.Resampling.LANCZOS)
    else: im=im.resize((w,h),Image.Resampling.LANCZOS)
    if p['opacity']!=1: im.putalpha(im.getchannel('A').point(lambda v:round(v*p['opacity'])))
    if 'shadow' in p:
        sh=p['shadow'];shadow=Image.new('RGBA',im.size,INK);shadow.putalpha(im.getchannel('A').point(lambda v:round(v*sh['opacity'])));canvas.alpha_composite(shadow,(x+sh['offset_canvas'][0],y+sh['offset_canvas'][1]))
    canvas.alpha_composite(im,(x,y))
for p in sorted(places,key=lambda p:p['z']):render_place(p)
for c in cards:
    im=capture.crop(c['capture_rect_ltrb']);im,pos=contain(im,c['card_rect']);canvas.alpha_composite(im,pos)
for i in items:
    im=Image.open(ROOT/i['source']).convert('RGBA');im=im.crop(im.getchannel('A').getbbox());im,pos=contain(im,i['icon_rect']);canvas.alpha_composite(im,pos)

fontdir=ROOT/'Assets/Font/Pretendard'
def draw_text(t):
    draw=ImageDraw.Draw(canvas); weight='Bold' if t['font_weight']>=700 else 'SemiBold' if t['font_weight']>=600 else 'Medium' if t['font_weight']>=500 else 'Regular'
    font=ImageFont.truetype(str(fontdir/f'Pretendard-{weight}.otf'),t['font_size'])
    string=t['text'];x,y,w,h=t['rect'];sp=t['character_spacing_px']
    width=sum(draw.textlength(ch,font=font) for ch in string)+sp*max(0,len(string)-1)
    assert width<=w,(t['id'],width,w)
    if t['alignment']=='center':x+=(w-width)/2
    for ch in string:draw.text((x,y),ch,font=font,fill=t['color'],anchor='lt');x+=draw.textlength(ch,font=font)+sp
    t['preview_text_width']=round(width,2)
for t in texts:draw_text(t)
# HUD and quest are screenshot-only context; neither enters sprite delivery.
hud=capture.crop((0,0,capture.width,85)).resize((1920,104),Image.Resampling.LANCZOS)
canvas.alpha_composite(hud,(0,0))
quest=capture.crop((68,89,419,180));quest=quest.resize((425,110),Image.Resampling.LANCZOS);canvas.alpha_composite(quest,(82,222))
canvas.convert('RGB').save(HERE/'Shop_Mockup_v4.png')

layout=dict(version=4,canvas=[1920,1080],origin='top-left',rect_format='x,y,width,height',
 source_mockup={'file':'../Shop_Mockup_v3.png','size':[1672,941],'reference_scale_xy':[1920/1672,1080/941],'note':'Reconstructed geometry with SPLIT_REQUEST_v3 changes. Native character and real game card captures in preview.'},delivery_mockup='../Shop_Mockup_v4.png',
 assets=assets,external_assets=external,external_art=external_art,reserved_regions={'hud':[0,0,1920,106],'quest_tracker':[82,222,580,110]},
 placements=places,text_boxes=texts,card_slots=cards,item_slots=items,
 templates={'card':{'count':6,'grid':[3,2],'hover_preserves_card_prefab':True},'item':{'count':4,'grid':[2,2],'name_font_size':16,'name_max_lines':1,'name_overflow':'ellipsis'},'sold_out':{'applies_to':'card and item content rect; price remains separate','overlays':[{'external_asset':'SoldOut_Hatching','rect_normalized':[0,0,1,1]},{'external_asset':'SoldOut_Band','rect_normalized':[0,.38,1,.24]},{'external_asset':'SoldOut_Stamp','rect_normalized':[.12,.42,.76,.16]}],'text_box':{'text':'SOLD OUT','rect_normalized':[0,.40,1,.20],'font_size':20,'color':INK,'alignment':'center','character_spacing_px':1,'font_weight':700},'interactable':False}},
 states={'goods':{'precedence':['SoldOut','Unaffordable','Hover','Normal'],'Normal':{'plinth':'Plinth_Card_Normal','cell':'ItemCell_Normal','price_tag':'PriceTag_Normal','price_color':INK,'interactable':True},'Hover':{'plinth':'Plinth_Card_Hover','cell':'ItemCell_Hover','price_tag':'PriceTag_Hover','price_color':INK,'interactable':True},'Unaffordable':{'plinth':'Plinth_Card_Unaffordable','cell':'ItemCell_Unaffordable','price_tag':'PriceTag_Unaffordable','price_color':RED,'price_color_binding':'ShopStockEntry.unaffordableColor','purchase_enabled':False,'tooltip_enabled':True,'pointer_hover_overrides_state':False},'SoldOut':{'template':'sold_out','purchase_enabled':False}},'service':{'Normal':'Panel_Service_Normal','Hover':'Panel_Service_Hover','Disabled':'Panel_Service_Disabled','disabled_if':'already used OR credits < cost','disabled_price_tag':'PriceTag_Unaffordable','insufficient_funds_price_color_binding':'ShopStockEntry.unaffordableColor','interactable_when_disabled':False},'leave':{'Normal':'Button_Leave_Normal','Hover':'Button_Leave_Hover','Pressed':'Button_Leave_Pressed','pressed_shadow_offset':[6,6]},'close':{'Normal':'Button_Close_Normal','Hover':'Button_Close_Hover','Pressed':'Button_Close_Pressed'}},
 rendering={'alpha':'straight','filter':'bilinear','ui_png_scale':2,'background_png_scale':1,'font':'Pretendard; actual text uses TMP','nine_slice_order':'left,bottom,right,top in SOURCE PNG pixels','shadow_policy':'Unity Shadow only. No shadow baked into exported PNG/SVG. useGraphicAlpha=true. No second shadow sprite.','sort':'external scene -> counter -> keeper -> Shop_BG -> panels/shadows -> watermarks -> goods -> prices/icons -> TMP -> runtime HUD/QuestTracker','background_left_transparent_until_x':690,'background_top_transparent_px':106},
 notes=['Only new UI geometry included. Existing icons, sold-out masks, watermarks are external v2 references.','All shop labels and prices are TMP; game card prefab contents are unchanged external runtime visuals.','PriceTag uses real 9-slice; keep 24 source-pixel borders at 2x, 12 logical pixels.','Credit value 33 makes all displayed goods and service unaffordable. Source component default is Color.red; serialized game override remains authoritative.','Mockup uses source screenshot card captures, so their sharpness is limited by supplied screenshot resolution.','No scene, prefab, importer, or .meta file edits.'])
(OUT/'layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf-8')

# Contact sheet includes all new state variants, not imported into the game.
rows=math.ceil(len(assets)/4);sheet=Image.new('RGB',(1600,rows*290),'#DAE0E5');draw=ImageDraw.Draw(sheet);font=ImageFont.truetype(str(fontdir/'Pretendard-Medium.otf'),18)
for idx,(name,a) in enumerate(assets.items()):
    x=(idx%4)*400;y=(idx//4)*290
    for yy in range(y+32,y+280,16):
        for xx in range(x+8,x+392,16):draw.rectangle((xx,yy,xx+15,yy+15),fill='#F5F7F9' if (xx//16+yy//16)%2 else '#CDD5DC')
    im=Image.open(OUT/a['file']).convert('RGBA');im.thumbnail((360,226),Image.Resampling.LANCZOS);sheet.paste(im,(x+200-im.width//2,y+155-im.height//2),im);draw.text((x+8,y+5),name,font=font,fill=INK)
sheet.save(OUT/'Asset_Overview.png')
checks=[]
for name,a in assets.items():
    p=OUT/a['file'];im=Image.open(p).convert('RGBA');s=(OUT/a['svg']).read_text(encoding='utf-8')
    assert list(im.size)==a['source_pixels'];assert im.getchannel('A').getextrema()[0]==0
    assert all(token not in s for token in ['<text','<image','<filter'])
    if 'nine_slice' in a:
        b=a['nine_slice']['source_pixels_left_bottom_right_top'];assert sum(b[::2])<im.width and sum(b[1::2])<im.height
    checks.append(dict(asset=name,size=list(im.size),alpha=True,text_free=True,shadow_free=True))
bgim=Image.open(OUT/'Shop_BG.png').convert('RGBA');assert bgim.crop((0,0,690,1080)).getchannel('A').getbbox() is None;assert bgim.crop((0,0,1920,106)).getchannel('A').getbbox() is None
for a in external.values():assert (OUT/a['file']).exists()
for t in texts:
    x,y,w,h=t['rect'];assert x>=0 and y>=106 and x+w<=1920 and y+h<=1080
for p in places:
    x,y,w,h=p['rect'];assert x>=0 and y>=0 and x+w<=1920 and y+h<=1080
validation=dict(passed=True,assets=len(assets),external_reuses=len(external),card_capture_source_size=list(capture.size),checks=checks,bg_left_and_hud_transparent=True,source_refs_exist=True,text_widths_fit=True,nine_slice_valid=True,original_source_hashes={k:v['sha256'] for k,v in external_art.items()},card_count=len(cards),item_count=len(items))
(OUT/'validation.json').write_text(json.dumps(validation,ensure_ascii=False,indent=2),encoding='utf-8')
(OUT/'README.txt').write_text('''상점 v4 / SPLIT_REQUEST_v3 반영

1920×1080, 좌상단 원점, rect=x,y,width,height. SVG 논리 크기, PNG UI 2배 / Shop_BG 1배.
신규 조각에는 글자·숫자·토냐·배경 그림·카운터·카드·보급품 그림·그림자가 없다.
Shop_BG는 오른쪽 기하 면만 포함하며 왼쪽 690px 및 상단 106px는 투명.
가격표 PriceTag 9-slice: source PNG L/B/R/T=24/24/24/24; 논리값은12px.
Panel/Plinth/ItemCell은 고정 크기 또는 비율 유지; 임의 9-slice 금지.

외부 재사용: ../../Extracted/의 아이콘 6종, 품절 3종, 워터마크 3종, 닫기 상태 3종.
외부 PNG는 기존 크기 그대로 재사용한다. 신규 SVG/2배 PNG 요구는 새로 만든 조각에 적용.
ZIP 단독으로 모든 런타임 자산이 포함되는 것은 아니다. 원래 Redesign_v3 위치에 풀어 외부 상대경로를 유지한다.

그림자는 UnityEngine.UI.Shadow만 사용. 큰 판 offset(12,-12), 먹색30%; 받침/칸8px, 가격표6px 먹색24%; Use Graphic Alpha. PNG 그림자를 더하지 않는다.
상태 우선순위: SoldOut > Unaffordable > Hover > Normal. 구매 불가 상태에서도 툴팁 사용 가능.
돈 부족 시 받침/칸/가격표 모두 Unaffordable. 가격은 ShopStockEntry.unaffordableColor, 미리보기는 코드 기본값 Color.red.
카드 제거는 이미 사용했거나 돈 부족이면 Disabled. 나가기 Pressed는 그림자6px.
품절은 기존 마스크 3종과 TMP SOLD OUT을 겹친다. 실제 rect는 layout templates.sold_out의 정규화 좌표를 content rect에 적용.

보급품 이름은 실제 ItemData 명칭, 16px, 한 줄 말줄임. 가격표와 분리.
크레딧 숫자 앞에도 기존 Icon_Credit을 사용한다.
TMP 폰트 Pretendard, text_boxes에 크기·색·정렬·자간·바인딩 기록.
제목 y118~210, 퀘스트 자리 y222~332. 상단 HUD는 y0~106.

Shop_Mockup_v4.png: 프로젝트 토냐 원본/배경/카운터/아이템 + 제공 게임 캡처 카드 + 신규 조각 조립본.
카드는 AI 목업에서 잘라내지 않았다. 원본 게임 캡처의 해상도 한계는 존재한다.
목업에만 HUD/퀘스트 캡처가 있으며 해당 요소는 조각에 포함되지 않는다.
Asset_Overview.png는 검수용으로 임포트하지 않는다.
Unity 적용 및 meta 수정은 하지 않았다.
''',encoding='utf-8')
with zipfile.ZipFile(HERE/'Shop_v4_Package.zip','w',zipfile.ZIP_DEFLATED) as z:
    z.write(HERE/'Shop_Mockup_v4.png','Shop_Mockup_v4.png')
    for p in OUT.rglob('*'):
        if p.is_file():z.write(p,'Extracted_Draft/'+p.relative_to(OUT).as_posix())
print(json.dumps({'new_pairs':len(assets),'external_reuses':len(external),'validated':True},ensure_ascii=False))
