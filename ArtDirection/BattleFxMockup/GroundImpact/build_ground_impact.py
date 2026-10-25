"""Extract generated keyed artwork into RGBA VFX; art handoff only, no Unity edits."""
from pathlib import Path
import json, math, shutil, hashlib, zipfile
import numpy as np
from PIL import Image, ImageDraw, ImageFont
from scipy import ndimage

HERE=Path(__file__).resolve().parent; BASE=HERE.parent; ROOT=BASE.parents[1]
OUT=BASE/'Extracted_Draft'; OUT.mkdir(exist_ok=True)
SOURCES=HERE/'Sources'; SOURCES.mkdir(exist_ok=True)
manifest=json.loads((HERE/'sources.json').read_text(encoding='utf-8'))
for name,source in manifest.items():
    dest=SOURCES/(name+'.png')
    if not dest.exists():shutil.copy2(source,dest)

def keyed(name):
    rgb=np.asarray(Image.open(SOURCES/(name+'.png')).convert('RGB'),dtype=np.float32)/255
    # For grayscale foreground over magenta: R-G = 1-alpha; remove key spill then unpremultiply.
    alpha=1-np.clip(np.minimum(rgb[:,:,0],rgb[:,:,2])-rgb[:,:,1],0,1)
    alpha=np.where(alpha<.025,0,alpha)
    gray=np.clip(rgb[:,:,1]/np.maximum(alpha,.001),0,1)
    out=np.empty((*alpha.shape,4),np.uint8);out[:,:,:3]=np.uint8(gray[:,:,None]*255+.5);out[:,:,3]=np.uint8(alpha*255+.5)
    out[alpha==0,:3]=0
    return Image.fromarray(out)

def black_alpha(name):
    rgb=np.asarray(Image.open(SOURCES/(name+'.png')).convert('RGB'),dtype=np.float32)
    lum=np.max(rgb,axis=2)
    alpha=np.clip((lum-3)/252,0,1)
    out=np.full((*alpha.shape,4),255,np.uint8);out[:,:,3]=np.uint8(alpha*255+.5)
    return Image.fromarray(out)

def fit_art(im,size,margin=(12,12),stretch=False):
    a=im.getchannel('A');bbox=a.point(lambda v:255 if v>3 else 0).getbbox()
    assert bbox
    im=im.crop(bbox);w,h=size;mw,mh=margin
    if stretch:new=(w-2*mw,h-2*mh)
    else:
        scale=min((w-2*mw)/im.width,(h-2*mh)/im.height);new=(round(im.width*scale),round(im.height*scale))
    im=im.resize(new,Image.Resampling.LANCZOS)
    result=Image.new('RGBA',size);result.alpha_composite(im,((w-new[0])//2,(h-new[1])//2));return result,bbox

assets=[]
def save(name,im,pivot=(.5,.5),**kw):
    im.save(OUT/(name+'.png'))
    a=dict(name=name,file=name+'.png',size=list(im.size),pivot=list(pivot),pivot_origin='bottom-left (Unity normalized)',frame_count=1,columns=1,rows=1,alpha='straight',text_baked=False);a.update(kw);assets.append(a);return a

for name in ['Crater_A','Crater_B','Crater_C']:
    im,bbox=fit_art(keyed(name),(380,190),(12,12),True)
    rgba=np.array(im);rgba[:,:,3]=np.minimum(rgba[:,:,3],230)
    # Subpixel feather softens decal edges without blurring painted concrete detail.
    alpha=rgba[:,:,3].astype(float)/255;edge=ndimage.gaussian_filter(alpha,.42);rgba[:,:,3]=np.uint8(np.clip(edge,0,.902)*255+.5)
    save(name,Image.fromarray(rgba),source_crop_ltrb=bbox,center_alpha_target=.9,semantic='dark neutral concrete crater and radiating fractures',tint='retain dark painted RGB; do not tint dark twice')

dust=black_alpha('Dust_Puff'); cw=dust.width/8
# All cells use same vertical region and same scale so rising cloud remains stable.
arr=np.array(dust.getchannel('A'));ys=np.where(np.max(arr,axis=1)>4)[0]; top=max(0,int(ys.min())-8);bottom=min(dust.height,int(ys.max())+9)
frames=[];fades=[1,1,1,.84,.64,.4,.18,.025]
for i in range(8):
    im=dust.crop((round(i*cw),top,round((i+1)*cw),bottom))
    scale=min(224/cw,224/(bottom-top));size=(round(cw*scale),round((bottom-top)*scale))
    im=im.resize(size,Image.Resampling.LANCZOS);im.putalpha(im.getchannel('A').point(lambda v:round(v*fades[i])))
    frame=Image.new('RGBA',(256,256));frame.alpha_composite(im,((256-size[0])//2,232-size[1]));frames.append(frame)
sheet=Image.new('RGBA',(2048,256))
for i,im in enumerate(frames):sheet.alpha_composite(im,(i*256,0))
save('Dust_Puff',sheet,pivot=(.5,.5),frame_count=8,columns=8,frame_size=[256,256],padding=0,extrude=0,reading_order='left-to-right',frame_duration_seconds=.1,preview_fps=10,playback='one-shot',last_frame_alpha_max=int(np.max(np.array(frames[-1])[:,:,3])),runtime_tint='multiply white RGB by background-matched dust color',source_cells=[{'rect':[i*cw,top,cw,bottom-top]} for i in range(8)],unity_frame_rects=[{'index':i,'rect':[i*256,0,256,256]} for i in range(8)])
frame_dir=OUT/'Dust_Frames';frame_dir.mkdir(exist_ok=True)
for i,im in enumerate(frames):im.save(frame_dir/f'Dust_Puff_{i:02}.png')

debris=keyed('Debris')
labels,count=ndimage.label(np.asarray(debris.getchannel('A'))>32)
objects=ndimage.find_objects(labels)
components=[]
for label,sl in enumerate(objects,1):
    if sl is None:continue
    if np.sum(labels[sl]==label)>800:components.append((sl[1].start,sl[0].start,sl[1].stop,sl[0].stop))
components.sort();assert len(components)==6,components
sizes=[24,28,32,16,30,22]
for i,size in enumerate(sizes):
    x0,y0,x1,y1=components[i]
    part=debris.crop((max(0,x0-4),max(0,y0-4),min(debris.width,x1+4),min(debris.height,y1+4)))
    im,bbox=fit_art(part,(size,size),(2,2))
    save(f'Debris_{i+1:02}',im,semantic='angular concrete fragment',source_cell=i,source_crop_in_cell_ltrb=bbox,runtime_tint='retain dark gray RGB; avoid double darkening')

ring,bbox=fit_art(black_alpha('Shock_Ring'),(600,300),(22,11),True)
# Absolutely clear center required for a ring; no residual black/gray disk alpha.
aa=np.array(ring);yy,xx=np.mgrid[:300,:600];inner=((xx-299.5)/278)**2+((yy-149.5)/139)**2<.68**2;aa[inner,3]=0
save('Shock_Ring',Image.fromarray(aa),semantic='thin elliptical white pressure ring; center transparent',source_crop_ltrb=bbox,footprint_aspect=2,pre_flattened=True,runtime_tint='white RGB supports multiply tint')

layout=dict(version=1,canvas_reference=[1920,1080],request='../GROUND_IMPACT_REQUEST.md',assets=assets,
 rendering={'alpha':'straight','texture_srgb':True,'alpha_is_transparency':True,'filter_mode':'Bilinear','wrap_mode':'Clamp','compression':'None during review; choose quality after in-game inspection','mipmaps':False,'sprite_pixels_per_unit':100,'raycast_target':False,'blend':'standard alpha; no additive required'},
 impact_frames_24fps=[15,21,27,33,42,45],impact_seconds=[round(f/24,6) for f in [15,21,27,33,42,45]],
 source_method='Built-in imagegen on magenta/black key; deterministic RGBA matte extraction, despill, fit and frame slicing. Native transparent option returned unsupported error.',
 runtime_handoff={'changes_made':'none; art files only','timing_position_scale':'existing GroundImpactFx.cs remains authoritative','pivots':'All sprites and each dust frame centered, matching existing Spawn pivot .5,.5. Dust art has a stable near-bottom origin within frame.','dust_animation':'8-frame deliverable; current code animates a single Soft sprite and has no sheet reader. This package does not silently add animation playback. Individual frame PNGs also supplied.','ring_aspect':'600x300 already flat. Existing code also scales Y to .38 and uses square size; do not blindly apply both flattenings when binding this art.','dark_rgb':'Existing crater/chip tint was authored for white masks. New crater/debris preserve dark RGB as requested; assess tint multiplication when binding.','suggested_binding_review':'Sprite selection/animation wiring and any material/geometry mapping are integration work; this delivery does not modify code, timings or positions.'},
 optional={'blue_purple_dust':'not duplicated; tint white source at runtime','Crater_Glow':'not included (optional; avoids introducing fire flash)'},
 notes=['Exact native export sizes; no invented 2x detail.','Generated source images preserved separately in GroundImpact/Sources; not import sprites.','Dust is a generated sequence with alignment and alpha fade normalization, not a simulation.','Preview timings illustrate supplied attack keyframes; they do not replace runtime timing.'])
(OUT/'layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf-8')

# Contact sheet on a neutral checker, plus bunker context preview.
font=ImageFont.truetype(str(ROOT/'Assets/Font/Pretendard/Pretendard-Medium.otf'),20)
contact=Image.new('RGBA',(1280,950),'#75818E');dr=ImageDraw.Draw(contact)
for y in range(0,950,20):
    for x in range(0,1280,20):dr.rectangle((x,y,x+19,y+19),fill='#CBD2D9' if (x//20+y//20)%2 else '#9AA6B3')
for i,name in enumerate(['Crater_A','Crater_B','Crater_C']):
    x=20+i*420;dr.text((x,10),name,font=font,fill='#141820');contact.alpha_composite(Image.open(OUT/(name+'.png')),(x,50))
dr.text((20,250),'Shock_Ring',font=font,fill='#141820');contact.alpha_composite(Image.open(OUT/'Shock_Ring.png'),(20,290))
for i in range(6):
    x=665+(i%3)*195;y=300+(i//3)*160;name=f'Debris_{i+1:02}';dr.text((x,y),name,font=font,fill='#141820');im=Image.open(OUT/(name+'.png'));contact.alpha_composite(im,(x+45,y+45))
dr.text((20,605),'Dust_Puff: 8 frames / 256x256 (displayed at 50%)',font=font,fill='#141820')
for i,im in enumerate(frames):contact.alpha_composite(im.resize((150,150),Image.Resampling.LANCZOS),(20+i*155,660))
contact.convert('RGB').save(HERE/'GroundImpact_Asset_Overview.png')

bg=Image.open(ROOT/'Assets/Art/Characters/Enemy/헌터킬러/헌터킬러_BG.png').convert('RGBA').resize((1280,725),Image.Resampling.LANCZOS)
events=[(15/24,(360,560),0),(21/24,(470,575),1),(27/24,(590,555),2),(33/24,(700,580),0),(42/24,(810,553),1),(45/24,(890,580),2)]
craters=[Image.open(OUT/f'Crater_{s}.png').convert('RGBA') for s in 'ABC'];ring=Image.open(OUT/'Shock_Ring.png').convert('RGBA')
chips=[Image.open(OUT/f'Debris_{i:02}.png').convert('RGBA') for i in range(1,7)]
def comp(im,xywh,canvas,opacity=1):
    x,y,w,h=xywh
    if w<1 or h<1:return
    im=im.resize((round(w),round(h)),Image.Resampling.LANCZOS)
    if opacity!=1:im.putalpha(im.getchannel('A').point(lambda a:round(a*max(0,min(1,opacity)))))
    canvas.alpha_composite(im,(round(x-w/2),round(y-h/2)))
def render(t):
    canvas=bg.copy()
    for start,(x,y),kind in events:
        age=t-start
        if age<0 or age>2.1:continue
        opacity=min(1,age/.05)*max(0,min(1,(2.1-age)/.9))
        comp(craters[kind],(x,y,190,95),canvas,opacity*.85)
        if age<.34:
            w=60+240*(1-(1-age/.34)**3);comp(ring,(x,y,w,w*.5),canvas,.55*(1-(age/.34)**2))
        if age<.8:
            idx=min(7,int(age/.1));comp(frames[idx],(x,y-32-age*28,155,155),canvas,.62)
        if age<.75:
            for j in range(6):
                dur=.5+j*.04;u=age/dur
                if u>1:continue
                dx=(j-2.5)*38*u;up=85*(1-((u-.5)*2)**2)
                comp(chips[j],(x+dx,y-up,10+j,10+j),canvas,min(1,(1-u)*3))
    return canvas
render(1.95).convert('RGB').save(HERE/'GroundImpact_Bunker_Preview.png')
gifframes=[render(i/24).convert('RGB') for i in range(96)]
gifframes[0].save(HERE/'GroundImpact_Sequence_Preview.gif',save_all=True,append_images=gifframes[1:],duration=[40,40,50]*32,loop=0,optimize=False)
dustgif=[]
for im in frames:
    c=Image.new('RGBA',(256,256),'#354150');c.alpha_composite(im);dustgif.append(c.convert('RGB'))
dustgif[0].save(HERE/'Dust_Puff_Preview.gif',save_all=True,append_images=dustgif[1:]+[Image.new('RGB',(256,256),'#354150')]*3,duration=100,loop=0,optimize=False)

checks=[]
for a in assets:
    im=Image.open(OUT/a['file']).convert('RGBA');assert list(im.size)==a['size'];alpha=np.array(im)[:,:,3]
    assert alpha.min()==0 and alpha.max()>0,a['name']
    edges=np.concatenate([alpha[0],alpha[-1],alpha[:,0],alpha[:,-1]]);assert edges.max()<=2,(a['name'],edges.max())
    rgb=np.array(im)[:,:,:3].astype(int);visible=alpha>5
    assert (np.max(rgb,axis=2)-np.min(rgb,axis=2))[visible].max()==0,'non-neutral key spill'
    checks.append(dict(name=a['name'],size=a['size'],transparent_edges=True,grayscale=True,alpha_max=int(alpha.max())))
assert frames[-1].getchannel('A').getextrema()[1]<=7
assert ring.getpixel((300,150))[3]==0
validation={'passed':True,'asset_count':len(assets),'dust_frames':8,'dust_sheet_size':[2048,256],'checks':checks,'last_dust_frame_nearly_transparent':True,'shock_center_transparent':True,'source_sha256':{n:hashlib.sha256((SOURCES/(n+'.png')).read_bytes()).hexdigest() for n in manifest}}
(OUT/'validation.json').write_text(json.dumps(validation,ensure_ascii=False,indent=2),encoding='utf-8')
(OUT/'README.txt').write_text('''헌터킬러 바닥 타격 VFX / GroundImpact

Crater_A/B/C: 380x190, center alpha max230(약.90), dark grayscale.
Dust_Puff: 2048x256, 8열1행, 프레임256x256, 여백0. 개별8프레임 Dust_Frames/ 제공.
Debris_01~06: 24,28,32,16,30,22px 정사각.
Shock_Ring:600x300, 흰색 RGB / alpha, 중앙 완전투명.
모든 pivot(.5,.5), PNG RGBA straight alpha, 필터 Bilinear, wrap Clamp, mipmaps off.
크기/피벗/프레임 rect/layout.json 참조. 2배판은 업스케일만 될 것이므로 추가하지 않음.

먼지·고리는 백색RGB에 알파로 농도 표현하므로 게임에서 색 곱하기 가능.
구덩이·돌은 요청대로 어두운RGB 유지. 기존의 매우 어두운 tint를 다시 곱하면 지나치게 어두워짐.
내장 imagegen 투명출력이 지원 오류여서 마젠타/검정 키로 생성 후 알파 추출·중성화·디스필.
원화는 별도 GroundImpact/Sources. 최종 PNG에는 단색 배경/키색/글자/불꽃 없음.

코드·타이밍·위치·씬·meta 변경 없음.
현재 GroundImpactFx.cs는 단일 Soft 스프라이트를 움직이며 시트 애니메이터가 없다. 이8프레임 시트의 실제 재생 연결은 후속 통합 작업.
Shock_Ring은 이미 납작하므로 기존 사각 Ring의 Y축.38 스케일과 이중 압축 주의.
현재 Sprite 자체가 코드 생성 private 프로퍼티이므로 이 산출물만으로 게임 자동 교체가 된 것은 아님.
타이밍/위치 동작은 원래 코드가 기준. GIF는 요청서의 24fps 타격프레임 15,21,27,33,42,45를 보여주는 검수용.
선택사항인 푸른/보라 먼지는 런타임 tint로 처리, Crater_Glow는 생략.
검증: 크기, 투명 가장자리, 키색 없음, 고리 중심투명, 마지막 먼지 거의 투명.
''',encoding='utf-8')
with zipfile.ZipFile(HERE/'GroundImpact_Package.zip','w',zipfile.ZIP_DEFLATED) as z:
    for p in OUT.rglob('*'):
        if p.is_file():z.write(p,'Extracted_Draft/'+p.relative_to(OUT).as_posix())
    for name in ['GroundImpact_Asset_Overview.png','GroundImpact_Bunker_Preview.png','GroundImpact_Sequence_Preview.gif','Dust_Puff_Preview.gif']:z.write(HERE/name,name)
print(json.dumps({'passed':True,'assets':len(assets),'frames':8,'zip':str(HERE/'GroundImpact_Package.zip')},ensure_ascii=False))
