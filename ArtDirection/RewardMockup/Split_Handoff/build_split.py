"""Reward mockup: fresh 2x geometric redraw, no source image pixel extraction."""
from pathlib import Path
import json, hashlib, math
import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT=Path(__file__).resolve().parent.parent
OUT=ROOT/'Extracted'
INK='#16181B'; PAPER='#EEF0F2'; CYAN='#0DB8F2'; WHITE='#FFFFFF'; GRAY='#747B81'
sprites={}; assets=[]

def canvas(w,h):return Image.new('RGBA',(w*2,h*2))
def line(im,pts,color=INK,width=1):ImageDraw.Draw(im).line([(round(x*2),round(y*2)) for x,y in pts],fill=color,width=width*2)
def poly(im,pts,color):ImageDraw.Draw(im).polygon([(round(x*2),round(y*2)) for x,y in pts],fill=color)
def rect(im,b,color):
    x,y,w,h=b;ImageDraw.Draw(im).rectangle((x*2,y*2,(x+w)*2-1,(y+h)*2-1),fill=color)
def ellipse(im,b,color=WHITE,width=2):ImageDraw.Draw(im).ellipse(tuple(round(v*2) for v in b),outline=color,width=width*2)
def grain(im,seed):
    a=np.array(im);rng=np.random.default_rng(seed);h,w=a.shape[:2]
    broad=np.asarray(Image.fromarray(rng.integers(90,165,(12,16),dtype=np.uint8)).resize((w,h),Image.Resampling.BICUBIC),dtype=float)
    noise=rng.normal(0,.4,(h,w))+(broad-128)*.018
    a[:,:,:3]=np.clip(a[:,:,:3].astype(float)+noise[:,:,None],0,255).astype(np.uint8)
    a[a[:,:,3]==0,:3]=0
    return Image.fromarray(a)
def put(name,im,xy,border=None,**more):
    OUT.mkdir(parents=True,exist_ok=True);im.save(OUT/(name+'.png'));sprites[name]=im
    assets.append(dict(id=name,file=name+'.png',scale=2,png_size_px=list(im.size),
        screen_rect=[*xy,im.width//2,im.height//2],alpha='binary',
        nine_slice_px=dict(zip(['left','bottom','right','top'],[v*2 for v in border])) if border else None,**more))
def cut_panel(w,h,cut,fill,border,width=1):
    im=canvas(w,h);pts=[(1,1),(w-cut-1,1),(w-1,cut),(w-1,h-1),(1,h-1),(1,1)]
    poly(im,pts,fill);line(im,pts,border,width);return im

def make_assets():
    im=cut_panel(1000,828,72,PAPER,'#747B81',1);im=grain(im,3)
    # Marks are all inside fixed corner patches, including top-right chamfer.
    for x,y in [(32,28),(924,28),(32,800),(976,800)]:
        line(im,[(x-10,y),(x+10,y)],GRAY);line(im,[(x,y-10),(x,y+10)],GRAY)
    put('Panel',im,(482,154),[108,52,108,108],resize_axes='both')
    im=canvas(192,192);color='#E7E9EB'
    ellipse(im,(9,9,183,183),color,1);ellipse(im,(17,17,175,175),color,1)
    for k in range(4):
        ang=k*math.pi/2
        def tr(x,y):return(96+x*math.cos(ang)-y*math.sin(ang),96+x*math.sin(ang)+y*math.cos(ang))
        poly(im,[tr(0,-84),tr(10,-10),tr(0,0),tr(-10,-10)],color)
    put('Panel_Watermark',im,(1262,208),pivot=[.5,.5],opaque_ink_hex=color)
    im=canvas(908,1);rect(im,(0,0,908,1),'#92989C');put('Rule',im,(536,362),[1,0,1,0],resize_axes='horizontal')
    im=canvas(32,1);rect(im,(0,0,32,1),INK);put('Header_NoTick',im,(1198,220))
    for state,c in [('Normal',INK),('Hover',CYAN)]:
        im=cut_panel(908,144,24,PAPER,c,1 if state=='Normal' else 2);im=grain(im,9)
        if state=='Hover':rect(im,(0,0,8,144),CYAN)
        put('Row_'+state,im,(536,390 if state=='Normal' else 544),[28,4,30,4],resize_axes='horizontal')
    im=canvas(30,1);rect(im,(0,0,30,1),INK);put('Row_NumTick',im,(572,464))
    im=canvas(110,110);rect(im,(0,0,110,110),INK);put('Icon_Slot',im,(648,406))
    im=canvas(80,80)
    # Stacked coins: rear stack first, front stack second; white outlines only.
    for x,y,w,h in [(37,15,33,43),(7,35,35,31)]:
        line(im,[(x,y+6),(x,y+h-5)],WHITE,2);line(im,[(x+w,y+6),(x+w,y+h-5)],WHITE,2)
        for yy in range(y,y+h,8):
            ImageDraw.Draw(im).arc((x*2,yy*2,(x+w)*2,(yy+12)*2),0,180,fill=WHITE,width=4)
        ellipse(im,(x,y,x+w,y+12),WHITE,2)
    put('Icon_Gold',im,(663,421))
    im=canvas(80,80)
    for pts in [[(8,22),(29,15),(43,62),(21,68),(8,22)],[(48,16),(72,24),(60,69),(38,63),(48,16)],[(29,11),(53,11),(53,64),(29,64),(29,11)]]:line(im,pts,WHITE,2)
    put('Icon_Card',im,(663,575))
    im=canvas(40,30);line(im,[(1,15),(37,15)],INK);line(im,[(25,3),(37,15),(25,27)],INK)
    put('Icon_Arrow',im,(1362,445))
    for primary in [True,False]:
        for hover in [False,True]:
            name='Button_'+('Primary' if primary else 'Secondary')+('_Hover' if hover else '')
            w,h=(278,70) if primary else (246,68)
            im=canvas(w,h)
            fill=('#25272A' if hover else INK) if primary else PAPER
            rect(im,(0,0,w,h),fill)
            if primary:rect(im,(0,h-(5 if hover else 3),w,5 if hover else 3),CYAN)
            else:line(im,[(1,1),(w-1,1),(w-1,h-1),(1,h-1),(1,1)],CYAN if hover else INK,2)
            put(name,im,(1164,886) if primary else (1246,856),[8,8,8,8],resize_axes='both')
    im=canvas(164,4);rect(im,(0,1,49,1),INK);rect(im,(115,1,49,1),INK)
    put('Card_Plinth',im,(552,802))

def text_slot(id,box,size,color=INK,align='left'):
    return dict(id=id,screen_rect=box,font_size_screen_px=size,color_hex=color,align=align)
def inst(asset,box):return dict(asset=asset,screen_rect=box)

def make_layout():
    reward=dict(panel_rect=[482,154,1000,828],watermark=dict(screen_rect=[1262,208,192,192],anchor='panel top-right',offset_from_top_right=[-220,54]),
        header=dict(english=text_slot('english',[562,198,570,24],16),title=text_slot('title',[556,244,570,92],76),number=text_slot('number',[1198,194,244,18],12,GRAY),number_tick_rect=[1198,220,32,1],rule_rect=[536,362,908,1]),
        rows=dict(first_rect=[536,390,908,144],repeat_step_y=154,gap_y=10,
            offsets=dict(number=[36,34,40,30],number_tick=[36,74,30,1],icon_slot=[112,16,110,110],
                icon=[127,31,80,80],item_art=[120,24,94,94],name_with_description=[256,24,530,52],
                name_without_description=[256,43,530,56],description=[256,84,530,28],arrow=[826,55,40,30]),
            item_art_padding_in_slot=[8,8,8,8],name_font_size=44,description_font_size=24,number_font_size=24,
            name_color_hex=INK,description_color_hex=GRAY,number_color_hex=INK,align='left',
            examples=[dict(index=1,state='Normal',icon='Icon_Gold',has_description=False),dict(index=2,state='Hover',icon='Icon_Card',has_description=True),dict(index=3,state='Normal',icon='runtime item artwork',has_description=True)]),
        footer=dict(rule_rect=[536,868,908,1],status=text_slot('remaining',[568,908,490,30],20,GRAY),button_rect=[1164,886,278,70],button_text=text_slot('continue',[1200,902,204,40],28,WHITE,'center')))
    card=dict(panel_rect=[424,152,1110,800],watermark=dict(screen_rect=[1300,204,192,192],anchor='panel top-right',offset_from_top_right=[-234,52]),
        header=dict(english=text_slot('english',[502,196,650,24],16),title=text_slot('title',[496,242,690,88],76),number=text_slot('number',[1228,192,248,18],12,GRAY),number_tick_rect=[1228,218,32,1],rule_rect=[470,350,1022,1]),
        cards=[dict(card_rect=[x,378,294,400],plinth_rect=[x+65,802,164,4],number=text_slot('card_'+str(i+1),[x+124,786,46,32],20,INK,'center')) for i,x in enumerate([486,832,1180])],
        footer=dict(rule_rect=[470,836,1022,1],status=text_slot('add_one',[504,874,630,30],20,GRAY),button_rect=[1246,856,246,68],button_text=text_slot('skip',[1270,872,198,36],24,INK,'center')))
    for name,scene in [('reward',reward),('card',card)]:
        items=[inst('Panel',scene['panel_rect']),inst('Panel_Watermark',scene['watermark']['screen_rect']),inst('Rule',scene['header']['rule_rect']),inst('Header_NoTick',scene['header']['number_tick_rect']),inst('Rule',scene['footer']['rule_rect'])]
        if name=='reward':
            for i in range(3):
                x,y,w,h=scene['rows']['first_rect'];y+=154*i
                items.append(inst('Row_Hover' if i==1 else 'Row_Normal',[x,y,w,h]))
                for asset,key in [('Row_NumTick','number_tick'),('Icon_Slot','icon_slot'),('Icon_Arrow','arrow')]:
                    ox,oy,ww,hh=scene['rows']['offsets'][key];items.append(inst(asset,[x+ox,y+oy,ww,hh]))
                if i<2:
                    ox,oy,ww,hh=scene['rows']['offsets']['icon'];items.append(inst('Icon_Gold' if i==0 else 'Icon_Card',[x+ox,y+oy,ww,hh]))
        else:
            items += [inst('Card_Plinth',c['plinth_rect']) for c in scene['cards']]
        items.append(inst('Button_Primary' if name=='reward' else 'Button_Secondary',scene['footer']['button_rect']))
        scene['instances']=items
    return dict(schema_version=1,reference_resolution=[1920,1080],coordinate_system='top-left origin; rect [x,y,width,height]; +y down',
        assets=assets,scenes=dict(reward=reward,card=card),import_settings=dict(pixels_per_unit=200,texture_type='Sprite',sprite_mode='Single',mesh_type='FullRect',filter_mode='Bilinear',mipmaps=False,compression='None',max_texture_size=4096,canvas_reference_pixels_per_unit=100))

def sliced(im,size,border):
    """Actual nine-patch raster assembly at 2x, preserving corner pixels exactly."""
    l,b,r,t=[border[k] for k in ['left','bottom','right','top']];w,h=size
    assert w>l+r and h>t+b
    xs=[0,l,im.width-r,im.width];ys=[0,t,im.height-b,im.height]
    xd=[0,l,w-r,w];yd=[0,t,h-b,h]
    out=Image.new('RGBA',size)
    for yy in range(3):
        for xx in range(3):
            if xs[xx]==xs[xx+1] or ys[yy]==ys[yy+1]:continue
            p=im.crop((xs[xx],ys[yy],xs[xx+1],ys[yy+1]))
            out.paste(p.resize((xd[xx+1]-xd[xx],yd[yy+1]-yd[yy]),Image.Resampling.NEAREST),(xd[xx],yd[yy]))
    return out
def checker(w,h):
    yy,xx=np.indices((h,w));v=np.where((xx//24+yy//24)%2,80,91).astype(np.uint8)
    return Image.fromarray(np.dstack([v,v,v,np.full_like(v,255)]))
def assembly(layout,name):
    im=checker(3840,2160);byid={a['id']:a for a in assets}
    for entry in layout['scenes'][name]['instances']:
        a=byid[entry['asset']];part=sprites[a['id']];x,y,w,h=entry['screen_rect'];size=(w*2,h*2)
        if part.size!=size:
            part=sliced(part,size,a['nine_slice_px']) if a['nine_slice_px'] else part.resize(size,Image.Resampling.NEAREST)
        im.alpha_composite(part,(x*2,y*2))
    im.convert('RGB').resize((1120,630),Image.Resampling.LANCZOS).save(OUT/('Assembly_'+name.title()+'.jpg'),quality=94)
def contact():
    out=Image.new('RGB',(1120,1480),(28,32,37));d=ImageDraw.Draw(out);font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
    d.text((20,12),'REWARD / 2X REDRAW / 16 SPRITES / TEXT-FREE',font=font,fill='white')
    for i,a in enumerate(assets):
        x=20+(i%2)*550;y=52+(i//2)*174
        tile=checker(530,130);p=sprites[a['id']].copy();p.thumbnail((510,120),Image.Resampling.LANCZOS)
        tile.alpha_composite(p,((530-p.width)//2,(130-p.height)//2));out.paste(tile.convert('RGB'),(x,y))
        d.text((x,y+136),f"{a['id']}  {a['png_size_px'][0]} x {a['png_size_px'][1]}",font=font,fill='white')
    out.save(OUT/'Contact_Sheet.jpg',quality=94)
def validate(layout):
    checks=[]
    for a in assets:
        p=OUT/a['file'];im=Image.open(p);ar=np.array(im)
        assert im.mode=='RGBA' and list(im.size)==a['png_size_px'] and max(im.size)<=4096
        assert im.width==a['screen_rect'][2]*2 and im.height==a['screen_rect'][3]*2
        assert set(np.unique(ar[:,:,3]))<={0,255}
        b=a['nine_slice_px']
        if b:assert b['left']+b['right']<im.width and b['top']+b['bottom']<im.height
        checks.append(dict(file=a['file'],size=list(im.size),sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
    for a,b in [('Row_Normal','Row_Hover'),('Button_Primary','Button_Primary_Hover'),('Button_Secondary','Button_Secondary_Hover')]:assert sprites[a].size==sprites[b].size
    a=assets[0];src=np.array(sprites['Panel']);b=a['nine_slice_px']
    for scene in layout['scenes'].values():
        size=tuple(v*2 for v in scene['panel_rect'][2:]);out=np.array(sliced(sprites['Panel'],size,b))
        l,r,t,bt=[b[k] for k in ['left','right','top','bottom']]
        for sx,sy,dx,dy,ww,hh in [(0,0,0,0,l,t),(src.shape[1]-r,0,size[0]-r,0,r,t),(0,src.shape[0]-bt,0,size[1]-bt,l,bt),(src.shape[1]-r,src.shape[0]-bt,size[0]-r,size[1]-bt,r,bt)]:
            assert np.array_equal(src[sy:sy+hh,sx:sx+ww],out[dy:dy+hh,dx:dx+ww])
    for n in ['Contact_Sheet','Assembly_Reward','Assembly_Card']:assert Image.open(OUT/(n+'.jpg')).width<=1200
    return dict(status='passed',asset_count=len(assets),checks=checks,panel_corners_preserved_in_both_sizes=True,alpha_binary=True)
def main():
    make_assets();layout=make_layout();assembly(layout,'reward');assembly(layout,'card');contact();report=validate(layout)
    report['source_files']=[]
    for name in ['Reward_Mockup.png','Reward_Card_Mockup.png']:
        p=ROOT/name
        with Image.open(p) as im:assert im.size==(1920,1080)
        report['source_files'].append(dict(file=name,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
    for name,data in [('layout',layout),('validation',report)]:
        (OUT/(name+'.json')).write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(status='passed',assets=len(assets)),ensure_ascii=False))
if __name__=='__main__':main()
