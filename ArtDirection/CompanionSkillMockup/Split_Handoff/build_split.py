"""Fresh 2x geometric sprites plus image_gen eye painting; no mockup crops."""
from pathlib import Path
import json, math, hashlib, shutil
import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT=Path(__file__).resolve().parent.parent; OUT=ROOT/'Extracted'
INK='#16181B';PAPER='#EEF0F2';CYAN='#0DB8F2';GRAY='#92999F';WHITE='#FFFFFF'
assets=[];sprites={}
def blank(w,h):return Image.new('RGBA',(w*2,h*2))
def poly(im,p,c):ImageDraw.Draw(im).polygon([(round(x*2),round(y*2)) for x,y in p],fill=c)
def line(im,p,c=INK,width=1):ImageDraw.Draw(im).line([(round(x*2),round(y*2)) for x,y in p],fill=c,width=round(width*2))
def rect(im,b,c):
    x,y,w,h=b;ImageDraw.Draw(im).rectangle((x*2,y*2,(x+w)*2-1,(y+h)*2-1),fill=c)
def diamond(im,x,y,r,c):poly(im,[(x,y-r),(x+r,y),(x,y+r),(x-r,y)],c)
def star(im,x,y,r,c):poly(im,[(x,y-r),(x+3,y-3),(x+r,y),(x+3,y+3),(x,y+r),(x-3,y+3),(x-r,y),(x-3,y-3)],c)
def cut(w,h,fill,outline=INK,cut=10):
    im=blank(w,h);p=[(0,0),(w-cut,0),(w-1,cut),(w-1,h-1),(0,h-1),(0,0)];poly(im,p,fill);line(im,p,outline);return im
def put(name,im,xy,border=None,**extra):
    im.save(OUT/(name+'.png'));sprites[name]=im
    assets.append(dict(id=name,file=name+'.png',scale=2,png_size_px=list(im.size),screen_rect=[*xy,im.width/2,im.height/2],
        alpha='binary',nine_slice_px=dict(zip(['left','bottom','right','top'],[v*2 for v in border])) if border else None,**extra))
def make():
    OUT.mkdir(parents=True,exist_ok=True)
    for suffix,color in [('',INK),('_Active',INK),('_Disabled','#53585E')]:
        im=cut(270,44,color,color)
        if suffix=='_Active':rect(im,(12,0,244,2),CYAN)
        put('Order_Button'+suffix,im,(90,848),[14,4,14,4],resize_axes='horizontal')
    im=blank(22,22);diamond(im,11,11,10,WHITE);put('Order_Diamond',im,(106,859))
    im=blank(34,32);rect(im,(0,0,34,32),CYAN);put('Order_Badge',im,(316,854))
    im=blank(390,216);p=[(14,0),(389,0),(389,215),(0,215),(0,14),(14,0)]
    poly(im,p,PAPER);line(im,p,INK);put('List_Panel',im,(86,628),[18,8,8,18],resize_axes='vertical')
    im=blank(370,1);rect(im,(0,0,370,1),INK);put('List_HeaderRule',im,(96,658),[1,0,1,0])
    for name,color in [('Row_Normal',PAPER),('Row_Hover',INK)]:put(name,cut(370,60,color,INK,2),(96,660),[4,2,4,2],resize_axes='horizontal')
    im=blank(5,60);rect(im,(0,0,5,60),CYAN);put('Row_ReadyBar',im,(96,660))
    for name,enh in [('Face_Frame',False),('Face_Frame_Enhanced',True)]:
        im=blank(60,52);line(im,[(1,1),(59,1),(59,51),(1,51),(1,1)],INK,2)
        if enh:line(im,[(5,5),(55,5),(55,47),(5,47),(5,5)],INK,1)
        put(name,im,(116,664))
    im=blank(48,40);rect(im,(0,0,48,40),WHITE);put('Face_Mask',im,(122,670))
    im=blank(48,40);rect(im,(0,0,48,40),'#B2B7BC');d=ImageDraw.Draw(im)
    d.ellipse((29,3,67,43),fill='#777F86');d.ellipse((7,37,89,106),fill='#777F86');put('Face_Placeholder',im,(122,732))
    im=blank(86,30);rect(im,(0,0,86,30),INK);diamond(im,10,15,4,CYAN);put('Ready_Tag',im,(372,676))
    im=blank(18,16);rect(im,(0,0,18,16),INK);line(im,[(0,0),(17,0),(17,15),(0,15),(0,0)],'#6D777F');put('Charge_Bar_Back',im,(324,744))
    im=blank(14,12);rect(im,(0,0,14,12),CYAN);put('Charge_Bar_Fill',im,(326,746))
    for name,color in [('Mark_Enhanced',INK),('Mark_Enhanced_White',WHITE)]:
        im=blank(14,14);star(im,7,7,7,color);put(name,im,(244,792))
    im=blank(800,380);poly(im,[(0,0),(800,0),(0,380)],WHITE);put('CutIn_Mask',im,(0,0))
    # Extra16px padding extends boundary beyond viewport at either end.
    for name,enh in [('CutIn_Edge',False),('CutIn_Edge_Enhanced',True)]:
        im=blank(832,412)
        line(im,[(0,403.6),(832,8.4)],INK,3)
        line(im,[(0,401.6),(832,6.4)],CYAN if enh else PAPER,1)
        if enh:line(im,[(0,395.6),(832,.4)],CYAN,1)
        put(name,im,(-16,-16))
    im=blank(54,54)
    p=[(27,1),(31,23),(53,27),(31,31),(27,53),(23,31),(1,27),(23,23),(27,1)]
    line(im,p,'#DDE0E3',1);put('CutIn_Watermark',im,(566,4))
    source=ROOT/'Split_Handoff'/'Eye_Generated_Final.png'
    if not source.exists():raise FileNotFoundError('Restore Split_Handoff/Eye_Generated_Final.png before regenerating.')
    eye=Image.open(source).convert('RGBA').resize((1920,912),Image.Resampling.LANCZOS)
    put('CutIn_Eye_Cp01',eye,(-80,-38),source_native_px=list(Image.open(source).size),method='New image_gen illustration; normalized to1920x912, not cropped/upscaled from mockup')
    for name,enh in [('Tag_Back',False),('Tag_Back_Enhanced',True)]:
        im=blank(570,44);poly(im,[(18,0),(570,0),(552,44),(0,44)],INK)
        poly(im,[(18,0),(24,0),(6,44),(0,44)],CYAN)
        if enh:line(im,[(25,1),(566,1)],CYAN)
        put(name,im,(120,342),[26,3,26,3],resize_axes='horizontal')
    im=blank(22,1);rect(im,(0,0,22,1),CYAN);put('Tag_Divider',im,(120,342),placement='tag-local rect [154,22,22,1]')

def text(id,box,size=14,c=INK,align='left',hover=PAPER):return dict(id=id,rect=box,font_size=size,color=c,hover_color=hover,align=align)
ANGLE=math.degrees(math.atan2(-380,800))
def metadata():
    return dict(schema_version=1,reference_resolution=[1920,1080],coordinate_system='top-left +y down; rect [x,y,width,height]; offsets marked local',assets=assets,
        order=dict(button_rect=[90,848,270,44],diamond_rect=[106,859,22,22],diamond_tints=dict(ready=CYAN,not_ready=GRAY),badge_rect=[316,854,34,32],
            text=[text('label',[154,856,44,30],24,PAPER),text('english',[216,864,60,18],12,PAPER),text('badge',[316,854,34,32],24,INK,'center')]),
        list=dict(panel_rect=[86,628,390,216],gap_above_button=4,header_rule_y=658,
            height_formula='30 + companion_count*62',panel_top_formula='848-4-(30+companion_count*62)',
            header=[text('ORDER',[112,634,64,20],14),text('companion_orders',[184,634,176,20],14),text('count',[430,634,28,20],14,INK,'right')],
            first_row_rect=[96,660,370,60],repeat_step_y=62,gap_y=2,
            offsets=dict(ready_bar=[0,0,5,60],face_frame=[20,4,60,52],face_mask=[26,10,48,40],enhanced_mark=[148,8,14,14],
                name=text('companion_name',[104,5,120,21],12),skill=text('skill_name',[104,26,120,28],20),ready_tag=[276,16,86,30],ready_label=text('READY',[294,21,62,20],12,PAPER),
                charge_cells=[[228+21*i,22,18,16] for i in range(4)],charge_fill_inset=[2,2,14,12],remaining=text('remaining',[322,20,42,22],14,INK,'center')),
            face_note='Same48x40 inner mask for both frames. Runtime face art beneath mask/frame; no Sho portrait extracted. Placeholder used in assembly.',
            examples=['hover+ready','3/4 turn charge','1/4 battle charge+enhanced']),
        cutin=dict(triangle_screen=[[0,0],[800,0],[0,380]],edge_angle_screen_degrees=ANGLE,unity_z_degrees=-ANGLE,
            eye_rect=[-80,-38,960,456],eye_overscan_each_side_percent=10,
            eye_template=dict(png_size=[1920,912],target_centers_png=[[557,447],[1114,264]],target_centers_screen=[[198.5,185.5],[477,94]],
                target_eye_width_screen=154,description='Approximate drawing targets, not measured landmark detections; same mask and placement for future companions.'),
            watermark_rect=[566,4,54,54],draw_order=['masked eye','edge variant','watermark if enhanced','tag'],
            tag=dict(anchor_screen=[120,342],pivot=[0,0],screen_rotation_degrees=ANGLE,unity_z_degrees=-ANGLE,base_size=[570,44],
                placement='top-left anchor; tag extends right/up along edge. Its height extends down/right into battle side.',
                text_local=[text('ORDER',[30,9,52,24],13,PAPER),text('skill',[88,7,58,28],20,PAPER),text('dialogue',[190,8,350,26],18,PAPER)],
                divider_local=[154,22,22,1],examples=dict(base='…쓴 만큼, 채울게.',enhanced='…우리 둘 몫이야. 남김없이.'))),
        import_settings=dict(ppu=200,canvas_reference_pixels_per_unit=100,texture_type='Sprite',sprite_mode='Single',mesh_type='FullRect',mipmaps=False,compression='None',filter_mode='Bilinear',wrap_mode='Clamp',max_texture_size=4096))

def checker(w,h):
    yy,xx=np.indices((h,w));v=np.where((xx//24+yy//24)%2,75,88).astype(np.uint8)
    return Image.fromarray(np.dstack([v,v,v,np.full_like(v,255)]))
def draw(im,name,x,y):im.alpha_composite(sprites[name],(round(x*2),round(y*2)))
def font(size):return ImageFont.truetype('C:/Windows/Fonts/malgun.ttf',size)
def order_sheet():
    im=checker(3840,2160)
    for name,x,y in [('List_Panel',86,628),('List_HeaderRule',96,658),('Order_Button_Active',90,848),('Order_Diamond',106,859),('Order_Badge',316,854)]:draw(im,name,x,y)
    for i in range(3):
        y=660+62*i;draw(im,'Row_Hover' if i==0 else 'Row_Normal',96,y)
        draw(im,'Face_Placeholder',122,y+10);draw(im,'Face_Frame_Enhanced' if i==2 else 'Face_Frame',116,y+4)
        if i==0:draw(im,'Row_ReadyBar',96,y);draw(im,'Ready_Tag',372,y+16)
        else:
            for j in range(4):
                draw(im,'Charge_Bar_Back',324+j*21,y+22)
                if j<(3 if i==1 else 1):draw(im,'Charge_Bar_Fill',326+j*21,y+24)
        if i==2:draw(im,'Mark_Enhanced',244,y+8)
    im.convert('RGB').resize((1120,630),Image.Resampling.LANCZOS).save(OUT/'Assembly_Order.jpg',quality=94)

def tag_picture(enh):
    im=sprites['Tag_Back_Enhanced' if enh else 'Tag_Back'].copy();im.alpha_composite(sprites['Tag_Divider'],(308,44))
    d=ImageDraw.Draw(im)
    for txt,xy,size in [('ORDER',(60,18),26),('흡혈',(176,14),40),('…우리 둘 몫이야. 남김없이.' if enh else '…쓴 만큼, 채울게.',(380,16),36)]:d.text(xy,txt,font=font(size),fill=PAPER)
    # Affine map local pixels into screen, rotation about top-left.
    theta=math.radians(ANGLE);c,s=math.cos(theta),math.sin(theta);ax,ay=240,684
    return im.transform((3840,2160),Image.Transform.AFFINE,(c,s,-c*ax-s*ay,-s,c,s*ax-c*ay),resample=Image.Resampling.BICUBIC)
def cutin_sheet(enh):
    im=checker(3840,2160);eye=Image.new('RGBA',im.size);draw(eye,'CutIn_Eye_Cp01',-80,-38)
    mask=Image.new('L',im.size);mask.paste(sprites['CutIn_Mask'].getchannel('A'),(0,0));eye.putalpha(mask);im=Image.alpha_composite(im,eye)
    draw(im,'CutIn_Edge_Enhanced' if enh else 'CutIn_Edge',-16,-16)
    if enh:draw(im,'CutIn_Watermark',566,4)
    im=Image.alpha_composite(im,tag_picture(enh))
    im.convert('RGB').resize((1120,630),Image.Resampling.LANCZOS).save(OUT/('Assembly_CutIn_Enhanced.jpg' if enh else 'Assembly_CutIn.jpg'),quality=94)
def contact():
    sh=Image.new('RGB',(1120,1800),(28,32,37));d=ImageDraw.Draw(sh);f=font(16)
    d.text((20,8),'COMPANION SKILL / 2X / 27 PIECES',font=f,fill='white')
    for i,a in enumerate(assets):
        x=15+i%3*370;y=40+i//3*152;p=sprites[a['id']].copy();p.thumbnail((340,106),Image.Resampling.LANCZOS)
        tile=checker(350,110);tile.alpha_composite(p,((350-p.width)//2,(110-p.height)//2));sh.paste(tile.convert('RGB'),(x,y))
        d.text((x,y+113),a['id'],font=f,fill='white');d.text((x,y+131),str(a['png_size_px']),font=font(12),fill='white')
    p=sprites['CutIn_Eye_Cp01'].convert('RGB').resize((760,361),Image.Resampling.LANCZOS)
    sh.paste(p,(180,1420));sh.save(OUT/'Contact_Sheet.jpg',quality=93)
def validate(data):
    report=[]
    for a in assets:
        im=Image.open(OUT/a['file']);arr=np.array(im);assert im.mode=='RGBA' and max(im.size)<=4096
        assert list(im.size)==a['png_size_px'] and im.width==a['screen_rect'][2]*2 and im.height==a['screen_rect'][3]*2
        assert set(np.unique(arr[:,:,3]))<={0,255}
        b=a['nine_slice_px']
        if b:assert b['left']+b['right']<im.width and b['bottom']+b['top']<im.height
        report.append(dict(file=a['file'],size=list(im.size),sha256=hashlib.sha256((OUT/a['file']).read_bytes()).hexdigest()))
    for names in [('Order_Button','Order_Button_Active','Order_Button_Disabled'),('Row_Normal','Row_Hover'),('Face_Frame','Face_Frame_Enhanced'),('CutIn_Edge','CutIn_Edge_Enhanced'),('Tag_Back','Tag_Back_Enhanced')]:assert len({sprites[n].size for n in names})==1
    for n in ['Face_Frame','Face_Frame_Enhanced']:assert np.all(np.array(sprites[n])[12:92,12:108,3]==0)
    for n in ['Assembly_Order','Assembly_CutIn','Assembly_CutIn_Enhanced','Contact_Sheet']:assert Image.open(OUT/(n+'.jpg')).width<=1200
    # Tag rectangle stays on game side y >=380-.475*x (all four transformed corners).
    c=math.cos(math.radians(ANGLE));s=math.sin(math.radians(ANGLE))
    for x,y in [(0,0),(570,0),(570,44),(0,44)]:
        sx=120+c*x-s*y;sy=342+s*x+c*y;assert sy>=380-.475*sx
    return dict(status='passed',asset_count=len(assets),checks=report,face_centers_transparent=True,tag_outside_triangle=True)
def main():
    make();data=metadata();order_sheet();cutin_sheet(False);cutin_sheet(True);contact();report=validate(data)
    for n,obj in [('layout',data),('validation',report)]: (OUT/(n+'.json')).write_text(json.dumps(obj,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(status='passed',assets=len(assets))))
if __name__=='__main__':main()
