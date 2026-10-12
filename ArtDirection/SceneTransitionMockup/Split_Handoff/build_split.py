"""Scene-transition v2: draw full 2x curtains, then losslessly partition.

Requires Pillow and NumPy. No mockup pixels are used in any output sprite.
"""
from pathlib import Path
import hashlib
import json
import math
import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / 'Extracted'
INK = '#16181B'
PAPER = '#EEF0F2'
CYAN = '#0DB8F2'
GRAY = '#93999F'
PALE = '#B7BDC2'
WHITE = '#FFFFFF'
FULL_W, H, S = 2640, 548, 2
sprites = {}
curtains = {}
layout = dict(schema_version=1, reference_resolution=[1920,1080],
    coordinate_system='top-left origin; +x right, +y down; rect [x,y,width,height]',
    assets=[], text_slots=[],
    import_settings=dict(texture_type='Sprite',sprite_mode='Single',pixels_per_unit=200,
        mesh_type='FullRect',mipmaps=False,compression='None',filter_mode='Bilinear',
        wrap_mode='Clamp',max_texture_size=4096,canvas_reference_pixels_per_unit=100))

def blank(w,h): return Image.new('RGBA',(w*S,h*S))

def polygon(im,points,color):
    ImageDraw.Draw(im).polygon([(round(x*S),round(y*S)) for x,y in points], fill=color)

def line(im,points,color=INK,width=1):
    ImageDraw.Draw(im).line([(round(x*S),round(y*S)) for x,y in points],fill=color,width=round(width*S))

def rect(im,box,color):
    x,y,w,h=box
    ImageDraw.Draw(im).rectangle((round(x*S),round(y*S),round((x+w)*S)-1,round((y+h)*S)-1),fill=color)

def cross(im,x,y,span=16,color=GRAY):
    line(im,[(x-span,y),(x+span,y)],color)
    line(im,[(x,y-span),(x,y+span)],color)

def diamond(im,x,y,r,color):
    polygon(im,[(x,y-r),(x+r,y),(x,y+r),(x-r,y)],color)

def paper(seed):
    """Synthesize at final resolution; never texture each cut piece separately."""
    rng=np.random.default_rng(seed)
    w,h=FULL_W*S,H*S
    fine=rng.normal(0,0.38,(h,w)).astype(np.float32)
    broad=np.asarray(Image.fromarray(rng.integers(70,190,(12,48),dtype=np.uint8)).resize((w,h),Image.Resampling.BICUBIC),dtype=np.float32)
    variation=fine+(broad-128)*0.018
    rgb=np.clip(np.array([238,240,242],dtype=np.float32)+variation[:,:,None],0,255).astype(np.uint8)
    a=np.full((h,w,1),255,dtype=np.uint8)
    im=Image.fromarray(np.concatenate([rgb,a],axis=2))
    # Discrete faint fibers are newly generated and stay sparse.
    d=ImageDraw.Draw(im)
    for _ in range(2600):
        x=int(rng.integers(0,w)); y=int(rng.integers(0,h))
        d.line((x,y,x+int(rng.integers(3,14)),y+int(rng.integers(-1,2))),fill=(235,237,239,255),width=1)
    return im

def save(name,im,xy,**metadata):
    assert im.width<=4096 and im.height<=4096
    OUT.mkdir(parents=True,exist_ok=True)
    im.save(OUT/(name+'.png'))
    sprites[name]=im
    e=dict(id=name,file=name+'.png',screen_rect=[*xy,im.width/S,im.height/S],scale=S,
        png_size_px=list(im.size),alpha='binary',nine_slice_px=None,**metadata)
    layout['assets'].append(e)
    return e

def make_curtain(upper):
    name='U' if upper else 'L'
    im=paper(2201 if upper else 2202)
    mask=Image.new('L',im.size)
    d=ImageDraw.Draw(mask)
    # Preview-led chevron: / upper left edge and \ lower right edge.
    slope=310
    if upper:
        boundary=[(slope,0),(FULL_W,0),(FULL_W,H),(0,H)]
        band=[(slope,0),(slope+80,0),(80,H),(0,H)]
    else:
        boundary=[(0,0),(FULL_W-slope,0),(FULL_W,H),(0,H)]
        band=[(FULL_W-slope-80,0),(FULL_W-slope,0),(FULL_W,H),(FULL_W-80,H)]
    d.polygon([(x*S,y*S) for x,y in boundary],fill=255)
    polygon(im,band,INK)
    # Offset cyan edge stroke and short perpendicular white ruler marks.
    if upper:
        line(im,[(slope+7,0),(7,H)],CYAN,2)
        line(im,[(slope+13,0),(13,H)],CYAN,1)
    else:
        line(im,[(FULL_W-slope-80+7,0),(FULL_W-80+7,H)],CYAN,2)
        line(im,[(FULL_W-slope-80+13,0),(FULL_W-80+13,H)],CYAN,1)
    tick_y=list(range(22,H-20,40))
    if not upper:tick_y=[H-y for y in tick_y]
    for y in tick_y:
        outer=(slope*(1-y/H)) if upper else (FULL_W-slope+slope*y/H)
        x=outer+36 if upper else outer-80+36
        line(im,[(x,y),(x+12,y+(7 if upper else -7))],WHITE,1)
    if upper:
        # Leading decoration corresponds to screen x54; all is in Lead.
        cross(im,574,50)
        line(im,[(574,70),(574,188)],PALE)
        # Tail decoration corresponds to screen x1870.
        cross(im,2390,50)
        line(im,[(2390,68),(2390,228)],PALE)
        diamond(im,2390,188,4,INK)
        line(im,[(2330,72),(2348,72)],GRAY)
        line(im,[(FULL_W-1,0),(FULL_W-1,H-1)],GRAY)
    else:
        cross(im,254,328)
        line(im,[(254,348),(254,502)],PALE)
        diamond(im,254,482,4,INK)
        line(im,[(254,482),(560,482)],PALE)
        line(im,[(1790,482),(2070,482)],PALE)
        cross(im,2070,328)
        line(im,[(2070,248),(2070,506)],PALE)
        diamond(im,2070,328,4,INK)
        diamond(im,2070,482,4,INK)
        line(im,[(2020,382),(2038,382)],GRAY)
        line(im,[(0,0),(0,H-1)],GRAY)
    # Keep seam-facing 12px paper-only. The separately layered rail covers it.
    # Diagonal leading band is the sole structural exception at the outer edge.
    im.putalpha(mask)
    arr=np.array(im);arr[arr[:,:,3]==0,:3]=0;im=Image.fromarray(arr)
    curtains[name]=im
    cuts=[('Lead',0,920,-520),('Body',920,1120,400),('Tail',2040,600,1520)] if upper else [('Tail',0,600,-200),('Body',600,1120,400),('Lead',1720,920,1520)]
    for part,start,width,x in cuts:
        piece=im.crop((start*S,0,(start+width)*S,H*S))
        save('Curtain'+name+'_'+part,piece,(x,0 if upper else 532),
            group='Curtain'+name,whole_canvas_crop_px=[start*S,0,width*S,H*S],
            stretch=False,draw_order=0 if upper else 1)
    return cuts

def make_parts():
    im=blank(1920,24)
    line(im,[(0,5),(1920,5)],INK)
    line(im,[(0,19),(1920,19)],INK)
    for x in [56,420,960,1140,1504,1868]:
        diamond(im,x,12,9,PAPER);diamond(im,x,12,7,INK);diamond(im,x,12,3,WHITE)
    save('Seam_Base',im,(0,528),draw_order=4,groove_local_screen_rect=[0,9,1920,6],groove_local_png_rect=[0,18,3840,12])
    im=blank(1920,6);rect(im,(0,0,1920,6),CYAN)
    # White sheen is a color interpolation, not semi-transparent alpha.
    a=np.array(im)
    for i in range(80):
        t=i/79;a[:,-80+i,:3]=np.round(np.array([13,184,242])*(1-t)+np.array([255,255,255])*t)
    save('Seam_Fill',Image.fromarray(a),(0,537),draw_order=5,pivot=[0,0.5],scale_x_anchor='left',highlight_width_screen=40)
    im=blank(32,32)
    diamond(im,16,16,15,INK);diamond(im,16,16,11,CYAN);diamond(im,16,16,2,WHITE)
    save('Seam_Head',im,(1136,524),draw_order=6,pivot=[0.5,0.5],default_progress=0.6)
    im=blank(984,364)
    for right in [False,True]:
        for bottom in [False,True]:
            x=983 if right else 1;y=363 if bottom else 1
            dx=-1 if right else 1;dy=-1 if bottom else 1
            line(im,[(x+dx*27,y),(x,y),(x,y+dy*27)],INK,2)
            line(im,[(x+dx*29,y),(x+dx*53,y)],CYAN)
            line(im,[(x,y+dy*29),(x,y+dy*53)],CYAN)
    save('TitleFrame',im,(468,306),draw_order=2,closed_mockup_rect=[468,344,984,364],shift_y_from_mockup=-38)
    # Pixel-centered fourfold symmetry: create one quadrant motif, combine rotations.
    im=blank(180,180)
    d=ImageDraw.Draw(im)
    d.arc((47,47,312,312),196,254,fill=INK,width=4)
    d.polygon([(179,0),(179,179),(150,150)],fill=INK)
    d.polygon([(181,47),(197,139),(181,158)],fill=CYAN)
    original=im.copy()
    for rotation in [Image.Transpose.ROTATE_90,Image.Transpose.ROTATE_180,Image.Transpose.ROTATE_270]:
        im=Image.alpha_composite(im,original.transpose(rotation))
    d=ImageDraw.Draw(im);d.ellipse((165,165,194,194),fill=INK);d.ellipse((175,175,184,184),fill=WHITE)
    save('Emblem',im,(604,340),draw_order=3,pivot=[0.5,0.5],center_png_px=[180,180],pixel_center_index=[179.5,179.5],closed_mockup_rect=[604,378,180,180])
    im=blank(150,34);polygon(im,[(0,0),(150,0),(132,34),(0,34)],INK)
    polygon(im,[(138,0),(150,0),(132,34),(120,34)],CYAN)
    save('Tag_Sim',im,(474,274),draw_order=3,variant='Training')
    im=blank(364,48)
    polygon(im,[(30,0),(364,0),(364,24),(340,48),(0,48),(0,30)],INK)
    polygon(im,[(30,0),(46,0),(12,34),(12,48),(0,48),(0,30)],CYAN)
    save('LoadingChip',im,(1432,946),draw_order=7)
    im=blank(8,8);ImageDraw.Draw(im).ellipse((0,0,15,15),fill=WHITE)
    save('Chip_Dot',im,(1682,966),draw_order=8,pivot=[0.5,0.5])

def slot(id,box,align,size,color,**extra):
    layout['text_slots'].append(dict(id=id,closed_mockup_rect=box,align=align,
        font_size_screen_px=size,color_hex=color,**extra))

def metadata():
    slot('english_label',[824,366,380,26],'center',20,CYAN,game_rect=[824,328,380,26])
    slot('title',[842,406,306,136],'center',124,INK,game_rect=[842,368,306,136])
    slot('subtitle',[768,612,458,42],'center',30,'#72797F',game_rect=[768,574,458,42])
    slot('vertex_transit',[96,40,304,24],'left',15,INK,curtain_parent='CurtainU_Lead',local_screen_rect=[616,40,304,24])
    slot('document_01',[1806,40,34,22],'center',15,GRAY,curtain_parent='CurtainU_Tail',local_screen_rect=[286,40,34,22])
    slot('document_02',[1812,884,34,22],'center',15,GRAY,curtain_parent='CurtainL_Lead',local_screen_rect=[292,352,34,22])
    slot('expedition_3_lines',[96,82,302,46],'left',10,GRAY,line_height=14,curtain_parent='CurtainU_Lead',local_screen_rect=[616,82,302,46])
    slot('shelter_3_lines',[96,956,324,40],'left',10,GRAY,line_height=14,curtain_parent='CurtainL_Tail',local_screen_rect=[296,424,324,40])
    slot('now_loading',[1500,961,170,22],'center',16,WHITE,game_rect=[1500,961,170,22])
    slot('sim',[482,277,116,26],'center',20,WHITE,game_rect=[482,277,116,26],variant='Training')
    layout['curtains']=dict(full_screen_size=[2640,548],working_canvas_px=[5280,1096],overlap_screen_px=16,
        closed_origins=dict(upper=[-520,0],lower=[-200,532]),
        cuts_upper_screen_x=[0,920,2040,2640],cuts_lower_screen_x=[0,600,1720,2640],
        leading_edge=dict(priority='Preview_Moving_Edge.jpg and requested < silhouette',horizontal_slope_screen=310,
            ink_band_horizontal_width=80,total_decorated_extent=390,
            upper_outer_edge_local=[[310,0],[0,548]],lower_outer_edge_local=[[2330,0],[2640,548]],
            upper_band_left_edge_local=[[310,0],[0,548]],lower_band_left_edge_local=[[2250,0],[2560,548]],
            note='User-confirmed < geometry. Lower band mirrors upper vertically, with cyan on its left edge, while lower paper stays left of band.310px slope plus80px band needs390px, still completely offscreen when closed.'),
        animation=dict(close_upper='right to center',close_lower='left to center',open_upper='center to left',open_lower='center to right',lower_delay_seconds=0.06))
    layout['seam']=dict(mockup_center_y=578,game_center_y=540,base_rect=[0,528,1920,24],
        fill_rect=[0,537,1920,6],groove_offset_y_screen=9,groove_offset_y_png=18,
        layering=['Seam_Base','Seam_Fill','Seam_Head'],head_center_formula='[1920*progress,540]',
        fill_width_formula='1920*progress',zero_progress='hide Fill and Head',highlight_scaled_width_formula='40*progress')
    layout['title_alignment']=dict(group_shift_y=-38,mockup_title_bottom=542,mockup_seam_center=578,
        title_bottom_to_seam_center=36,title_bottom_to_rail_top=24,
        mockup_subtitle_top=612,seam_center_to_subtitle_top=34,rail_bottom_to_subtitle_top=22,
        game_title_bottom=504,game_subtitle_top=574)
    layout['loading_chip']=dict(screen_rect=[1432,946,364,48],dot_centers_screen=[[1686,970],[1708,970],[1730,970]])
    layout['assembly_moving']=dict(upper_root=[600,0],lower_root=[-1650,532],view_rect=[-40,0,2240,1080],
        description='Transparent checkerboard replaces game backdrop; leading edges join around x605,y540 into <.')

def checker(w,h):
    yy,xx=np.indices((h,w));v=np.where((xx//24+yy//24)%2==0,87,99).astype(np.uint8)
    return Image.fromarray(np.dstack([v,v,v,np.full_like(v,255)]))

def draw_sprite(scene,name,x,y,width=None):
    im=sprites[name]
    im=im.resize((width or im.width//2,im.height//2),Image.Resampling.LANCZOS)
    scene.alpha_composite(im,(int(x),int(y)))

def assembly(closed=True):
    if closed:
        scene=checker(1920,1080)
        for a in layout['assets']:
            if a['id'] in ['Tag_Sim','Seam_Fill','Seam_Head','Chip_Dot']:continue
            draw_sprite(scene,a['id'],*a['screen_rect'][:2])
        for x,y in layout['loading_chip']['dot_centers_screen']:draw_sprite(scene,'Chip_Dot',x-4,y-4)
    else:
        scene=checker(2240,1080)
        # Reconstruct from saved pieces, never use source mockup or full curtain render.
        for a in layout['assets']:
            if not a['id'].startswith('Curtain'):continue
            origin=600 if a['id'].startswith('CurtainU') else -1650
            draw_sprite(scene,a['id'],origin+a['whole_canvas_crop_px'][0]/2+40,0 if a['id'].startswith('CurtainU') else 532)
    scene=scene.convert('RGB');scene.thumbnail((1120,630),Image.Resampling.LANCZOS)
    scene.save(OUT/('Assembly_Closed.jpg' if closed else 'Assembly_Moving.jpg'),quality=93)

def contact_sheet():
    entries=layout['assets'];w=1120
    sheet=Image.new('RGB',(w,1740),(30,35,40));d=ImageDraw.Draw(sheet)
    font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
    d.text((20,10),'SCENE TRANSITION V2 / 2X REDRAW / 14 TEXT-FREE SPRITES',font=font,fill='white')
    for i,a in enumerate(entries):
        x=20+(i%2)*550;y=48+(i//2)*152
        tile=checker(530,112);im=sprites[a['id']].copy();im.thumbnail((512,106),Image.Resampling.LANCZOS)
        tile.alpha_composite(im,((530-im.width)//2,(112-im.height)//2))
        sheet.paste(tile.convert('RGB'),(x,y))
        d.text((x,y+116),f"{a['id']}  {a['png_size_px'][0]} x {a['png_size_px'][1]}",font=font,fill=(225,230,235))
    y=1120
    d.text((20,y),'JOIN CHECK / Lead + Body + Tail (lossless full-canvas partition)',font=font,fill='white')
    y+=32
    for group,order in [('U',['Lead','Body','Tail']),('L',['Tail','Body','Lead'])]:
        joined=Image.new('RGBA',(5280,1096));offset=0
        for part in order:
            im=sprites['Curtain'+group+'_'+part];joined.paste(im,(offset,0));offset+=im.width
        joined.thumbnail((1060,220),Image.Resampling.LANCZOS)
        tile=checker(joined.width,joined.height);tile.alpha_composite(joined);sheet.paste(tile.convert('RGB'),(30,y))
        y+=joined.height+12
    d.text((20,y),'SEAM FIT / 60% cyan fill / center y540',font=font,fill='white')
    y+=26
    rail=checker(1920,48)
    draw_sprite(rail,'Seam_Base',0,12)
    draw_sprite(rail,'Seam_Fill',0,21,width=1152)
    draw_sprite(rail,'Seam_Head',1136,8)
    rail=rail.convert('RGB').resize((1060,27),Image.Resampling.LANCZOS)
    sheet.paste(rail,(30,y));y+=27
    # Keep all strips visible.
    if y>sheet.height:
        # This branch is precluded by layout below; fail instead of silently clipping.
        raise AssertionError('contact sheet height too short')
    sheet.save(OUT/'Contact_Sheet.jpg',quality=93)

def validate():
    checks=[]
    for a in layout['assets']:
        p=OUT/a['file'];im=Image.open(p);ar=np.array(im)
        assert im.mode=='RGBA' and list(im.size)==a['png_size_px']
        assert im.width==a['screen_rect'][2]*2 and im.height==a['screen_rect'][3]*2
        assert max(im.size)<=4096
        assert set(np.unique(ar[:,:,3]))<={0,255}
        checks.append(dict(file=a['file'],size=list(im.size),sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
    joins=[]
    for name,order in [('U',['Lead','Body','Tail']),('L',['Tail','Body','Lead'])]:
        arr=np.concatenate([np.array(sprites['Curtain'+name+'_'+p]) for p in order],axis=1)
        assert np.array_equal(arr,np.array(curtains[name]))
        joins.append(dict(curtain=name,reassembled_equals_full_canvas=True))
    # Assert curtain-only closed coverage at full2x resolution.
    coverage=Image.new('RGBA',(3840,2160))
    for a in layout['assets']:
        if a['id'].startswith('Curtain'):coverage.alpha_composite(sprites[a['id']],tuple(int(v*2) for v in a['screen_rect'][:2]))
    assert np.all(np.array(coverage)[:,:,3]==255)
    emblem=np.array(sprites['Emblem']);assert np.array_equal(emblem,np.rot90(emblem))
    # At all non-tick x positions, groove must be transparent beneath the fill.
    base=np.array(sprites['Seam_Base']);assert np.all(base[18:30,500:700,3]==0)
    for name in ['Contact_Sheet.jpg','Assembly_Closed.jpg','Assembly_Moving.jpg']:assert Image.open(OUT/name).width<=1200
    return dict(status='passed',asset_count=len(checks),checks=checks,curtain_joins=joins,
        closed_curtains_fully_opaque=True,emblem_fourfold_pixel_symmetry=True,
        note='No Unity importer or runtime changes; geometry validation only.')

def main():
    OUT.mkdir(parents=True,exist_ok=True)
    source=[]
    for p in sorted(ROOT.glob('SceneTransition_v2_*.png')):
        with Image.open(p) as im:assert im.size==(1920,1080)
        source.append(dict(file=p.name,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
    make_curtain(True);make_curtain(False);make_parts();metadata()
    assembly(True);assembly(False);contact_sheet()
    report=validate();report['mockup_sources']=source
    (OUT/'layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    (OUT/'validation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(status='passed',assets=len(sprites),path=str(OUT)),ensure_ascii=False))

if __name__=='__main__':main()
