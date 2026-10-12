"""Draw five fresh 2x frame pieces; deterministically grade original background."""
from pathlib import Path
import json, hashlib
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT=Path(__file__).resolve().parent.parent
OUT=ROOT/'Extracted'
PAPER='#EEF0F2'; INK='#16181B'; GRAY='#92989E'; WHITE='#EEF0F2'
sprites={};assets=[]
def canvas(w,h):return Image.new('RGBA',(w*2,h*2))
def poly(im,pts,c):ImageDraw.Draw(im).polygon([(round(x*2),round(y*2)) for x,y in pts],fill=c)
def line(im,pts,c=GRAY,width=1):ImageDraw.Draw(im).line([(round(x*2),round(y*2)) for x,y in pts],fill=c,width=width*2)
def cross(im,x,y,r=12,c=GRAY):
    line(im,[(x-r,y),(x+r,y)],c);line(im,[(x,y-r),(x,y+r)],c)
def star(im,x,y,r,c=INK):
    poly(im,[(x,y-r),(x+3,y-3),(x+r,y),(x+3,y+3),(x,y+r),(x-3,y+3),(x-r,y),(x-3,y-3)],c)
def grain(im,seed):
    a=np.array(im);h,w=a.shape[:2];rng=np.random.default_rng(seed)
    broad=np.array(Image.fromarray(rng.integers(85,175,(10,14),dtype=np.uint8)).resize((w,h),Image.Resampling.BICUBIC),dtype=float)
    n=rng.normal(0,.55,(h,w))+(broad-128)*.024
    a[:,:,:3]=np.clip(a[:,:,:3].astype(float)+n[:,:,None],0,255).astype(np.uint8);a[a[:,:,3]==0,:3]=0
    return Image.fromarray(a)
def save(name,im,xy):
    OUT.mkdir(parents=True,exist_ok=True);im.save(OUT/(name+'.png'));sprites[name]=im
    assets.append(dict(id=name,file=name+'.png',screen_rect=[*xy,im.width//2,im.height//2],scale=2,png_size_px=list(im.size),alpha='binary',nine_slice_px=None))
def frames():
    im=canvas(400,180);poly(im,[(0,0),(400,0),(282,114),(0,178)],PAPER);im=grain(im,1)
    line(im,[(0,174),(281,110),(389,4)],'#BDC1C5');cross(im,40,42,15)
    line(im,[(40,68),(40,160)]);line(im,[(42,68),(42,160)],'#D9DCDF')
    star(im,94,42,29);line(im,[(78,26),(110,58)],INK);line(im,[(78,58),(110,26)],INK)
    save('Frame_TopLeft',im,(0,0))
    im=canvas(740,100);poly(im,[(98,0),(740,0),(740,99),(0,99)],PAPER);im=grain(im,2)
    line(im,[(7,96),(739,96)],'#BDC1C5');line(im,[(570,24),(570,75)])
    cross(im,704,36,17);line(im,[(732,5),(732,88)],'#BDC1C5')
    save('Frame_TopBar',im,(1180,0))
    im=canvas(320,160);poly(im,[(0,44),(244,0),(308,126),(248,147),(0,159)],PAPER);im=grain(im,3)
    line(im,[(0,49),(238,6)],'#BDC1C5');line(im,[(0,154),(246,142),(299,124)],'#BDC1C5')
    cross(im,40,50,5);line(im,[(40,58),(40,126)]);cross(im,40,132,7)
    save('Frame_BottomLeft',im,(0,920))
    im=canvas(260,240);poly(im,[(260,0),(260,240),(0,240)],PAPER);im=grain(im,4)
    line(im,[(8,237),(255,9)],'#BDC1C5');line(im,[(152,134),(152,226)])
    cross(im,152,128,8);line(im,[(54,208),(259,208)],'#B6BCC1');star(im,152,208,11)
    save('Frame_BottomCenter',im,(740,840))
    # Small enough that every visible pixel above y1010 lies to the right of x1880.
    im=canvas(140,150);poly(im,[(140,54),(140,150),(0,150)],INK)
    line(im,[(10,148),(136,61)],WHITE);cross(im,114,100,9,WHITE)
    save('Frame_BottomRight',im,(1780,930))

CORRECTION=dict(color_space='sRGB values',gradient_start_x=640,gradient_end_x=1300,
    gradient='smoothstep t*t*(3-2*t)',brightness_factor=.78,saturation_factor=.78,
    blue_gray_tint_rgb=[47,64,79],tint_mix=.10,gaussian_blur_radius_px=1.15,
    operations=['Gaussian blur original','Rec709 grayscale blend, saturation .78','multiply brightness .78','mix10% blue-gray','blend with original by horizontal smoothstep','round nearest uint8'])
def background():
    p=ROOT/'Reference_Background_Current.png';src=Image.open(p).convert('RGB');assert src.size==(1672,941)
    original=np.asarray(src,dtype=np.float32);blur=np.asarray(src.filter(ImageFilter.GaussianBlur(1.15)),dtype=np.float32)
    lum=np.sum(blur*np.array([.2126,.7152,.0722]),axis=2,keepdims=True)
    graded=(lum+(blur-lum)*.78)*.78
    graded=graded*.9+np.array([47,64,79])*.1
    t=np.clip((np.arange(1672)-640)/660,0,1);weight=(t*t*(3-2*t))[None,:,None]
    out=np.rint(np.clip(original*(1-weight)+graded*weight,0,255)).astype(np.uint8)
    assert np.array_equal(out[:,:641],np.array(src)[:,:641])
    im=Image.fromarray(out).convert('RGBA');im.save(OUT/'Background_RightDim.png')
    return im,dict(file=p.name,sha256=hashlib.sha256(p.read_bytes()).hexdigest())
def slot(id,box,size,color=INK,align='left'):
    return dict(id=id,screen_rect=box,font_size_screen_px=size,color_hex=color,align=align)
def checker(w,h):
    yy,xx=np.indices((h,w));v=np.where((xx//16+yy//16)%2,72,85).astype(np.uint8)
    return Image.fromarray(np.dstack([v,v,v,np.full_like(v,255)]))
def previews(bg):
    full=bg.resize((3840,2160),Image.Resampling.LANCZOS)
    for a in assets:full.alpha_composite(sprites[a['id']],tuple(v*2 for v in a['screen_rect'][:2]))
    full.convert('RGB').resize((1120,630),Image.Resampling.LANCZOS).save(OUT/'Assembly_Lobby.jpg',quality=94)
    sheet=Image.new('RGB',(1120,1230),(29,33,38));d=ImageDraw.Draw(sheet);f=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
    d.text((20,12),'LOBBY TILT / 5 REDRAWN FRAME PIECES + ORIGINAL-PIXEL BACKGROUND GRADE',font=f,fill='white')
    for i,a in enumerate(assets):
        x=20+(i%2)*550;y=52+(i//2)*190;p=sprites[a['id']].copy();p.thumbnail((510,145),Image.Resampling.LANCZOS)
        tile=checker(530,150);tile.alpha_composite(p,((530-p.width)//2,(150-p.height)//2));sheet.paste(tile.convert('RGB'),(x,y))
        d.text((x,y+155),f"{a['id']}  {a['png_size_px'][0]} x {a['png_size_px'][1]}",font=f,fill='white')
    d.text((20,630),'Background_RightDim / native1672x941 / opaque / no scene regeneration',font=f,fill='white')
    p=bg.convert('RGB');p.thumbnail((1000,563),Image.Resampling.LANCZOS);sheet.paste(p,(60,662))
    sheet.save(OUT/'Contact_Sheet.jpg',quality=93)
def validate(bg):
    checks=[];mask=np.zeros((2160,3840),dtype=bool)
    for a in assets:
        im=Image.open(OUT/a['file']);ar=np.array(im)
        assert im.mode=='RGBA' and list(im.size)==a['png_size_px'] and set(np.unique(ar[:,:,3]))<={0,255}
        assert im.width==a['screen_rect'][2]*2 and im.height==a['screen_rect'][3]*2
        if a['id']!='Frame_BottomCenter':
            x,y,w,h=a['screen_rect'];mask[y*2:(y+h)*2,x*2:(x+w)*2]|=ar[:,:,3]>0
        checks.append(dict(file=a['file'],sha256=hashlib.sha256((OUT/a['file']).read_bytes()).hexdigest(),size=list(im.size)))
    assert not mask[200:2020,1920:3760].any(),'frame intrudes into reserved menu region'
    assert bg.size==(1672,941) and np.all(np.array(bg)[:,:,3]==255)
    src=np.array(Image.open(ROOT/'Reference_Background_Current.png').convert('RGB'))
    assert np.array_equal(np.array(bg)[:,:641,:3],src[:,:641])
    for n in ['Assembly_Lobby','Contact_Sheet']:assert Image.open(OUT/(n+'.jpg')).width<=1200
    return dict(status='passed',frame_count=5,background_count=1,frame_alpha_binary=True,
        menu_region_clear_except_allowed_bottom_center=True,background_left_pixels_unchanged_through_x640=True,
        background_opaque=True,files=checks)
def main():
    frames();bg,source=background();previews(bg);report=validate(bg)
    data=dict(schema_version=1,reference_resolution=[1920,1080],coordinate_system='top-left; +x right,+y down; rect [x,y,width,height]',
        assets=assets,background=dict(file='Background_RightDim.png',png_size_px=[1672,941],screen_rect=[0,0,1920,1080],
            resolution_mode='native source size; exempt from2x frame rule',screen_to_source_scale=[1920/1672,1080/941],source=source,correction=CORRECTION),
        draw_order=['Background_RightDim']+[a['id'] for a in assets]+['existing perspective menu','existing character and text'],
        menu_reserved_rect=[960,100,920,910],exception=dict(asset='Frame_BottomCenter',max_x=1000,overlap_allowed=True),
        text_slots=[slot('VERTEX',[136,30,190,28],23),slot('EXPEDITION_3_LINES',[72,82,160,43],9,'#949A9F'),
            slot('SHELTER_3_LINES',[72,992,196,46],10,'#949A9F'),slot('OBSERVE_3_LINES',[1766,26,108,51],11,'#62686F'),
            slot('progress_value',[1570,38,102,28],17,'#62686F'),slot('SOMEWHERE_3_LINES',[1850,1052,62,24],7,WHITE)],
        runtime_element_slots=dict(progress_bar=[1320,42,226,14],settings_gear=[1680,34,34,34]),
        import_settings=dict(frame_ppu=200,texture_type='Sprite',sprite_mode='Single',mesh_type='FullRect',mipmaps=False,compression='None',filter_mode='Bilinear',wrap_mode='Clamp',canvas_reference_pixels_per_unit=100),
        excluded=['menu artwork','menu shadows','hover effects','character','speech bubble','all text','progress bar','settings gear'])
    for n,obj in [('layout',data),('validation',report)]:
        (OUT/(n+'.json')).write_text(json.dumps(obj,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(status='passed',frames=5,backgrounds=1)))
if __name__=='__main__':main()
