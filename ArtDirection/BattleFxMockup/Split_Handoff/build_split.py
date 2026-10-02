"""Deterministic 2x geometric redraw; originals are never cropped or resized into assets."""
from pathlib import Path
import json
import hashlib
import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / 'Extracted'
INK = (22, 24, 27, 255)
PAPER = (238, 240, 242, 255)
CYAN = (13, 184, 242, 255)
GRAY = (158, 164, 170, 255)
WHITE = (235, 240, 243, 255)
layouts = {}
images = {}

def canvas(w, h):
    return Image.new('RGBA', (int(w*2), int(h*2)))

def poly(im, points, color):
    ImageDraw.Draw(im).polygon([(round(x*2), round(y*2)) for x,y in points], fill=color)

def rect(im, box, color):
    x,y,w,h = box
    ImageDraw.Draw(im).rectangle((round(x*2),round(y*2),round((x+w)*2)-1,round((y+h)*2)-1),fill=color)

def line(im, points, color, width=1):
    ImageDraw.Draw(im).line([(round(x*2),round(y*2)) for x,y in points],fill=color,width=round(width*2))

def grain(im, seed, strength=0.65):
    a = np.array(im)
    rng = np.random.default_rng(seed)
    h,w = a.shape[:2]
    noise = rng.normal(0, strength, (h,w))
    # Newly synthesized fine grain and broad paper variation at output resolution.
    small = rng.integers(0,256,(max(2,h//80),max(2,w//80)),dtype=np.uint8)
    broad = np.asarray(Image.fromarray(small).resize((w,h),Image.Resampling.BICUBIC),dtype=float)/255-0.5
    a[:,:,:3] = np.clip(a[:,:,:3].astype(float)+noise[:,:,None]+broad[:,:,None]*1.5,0,255).astype(np.uint8)
    a[a[:,:,3]==0,:3] = 0
    return Image.fromarray(a)

def put(group, name, im, xy, border=None, **extra):
    folder = OUT/group
    folder.mkdir(parents=True,exist_ok=True)
    im.save(folder/(name+'.png'))
    entry = dict(id=name,file=name+'.png',scale=2,png_size_px=list(im.size),
                 screen_rect=[*xy,im.width/2,im.height/2],pivot=[0,1],alpha='binary',**extra)
    if border is not None:
        entry['nine_slice_px'] = dict(zip(['left','bottom','right','top'],[int(v*2) for v in border]))
        entry['resize_axes'] = 'horizontal'
    layouts.setdefault(group,dict(schema_version=1,reference_resolution=[1920,1080],
        coordinate_system='top-left origin; x right, y down; rectangles [x,y,width,height]',
        import_settings=dict(texture_type='Sprite',sprite_mode='Single',pixels_per_unit=200,
            mesh_type='FullRect',filter_mode='Bilinear',mipmaps=False,compression='None',max_texture_size=4096,
            canvas_reference_pixels_per_unit=100),assets=[],instances=[],text_slots=[]))['assets'].append(entry)
    images[(group,name)] = im
    return entry

def strip(group,name,xy,w,color):
    im=canvas(w,1); rect(im,(0,0,w,1),color)
    return put(group,name,im,xy,[1,0,1,0])

def parallelogram(w,h,s,color):
    im=canvas(w,h); poly(im,[(s,0),(w,0),(w-s,h),(0,h)],color); return im

def diamond(group,name,xy,color,size=14):
    im=canvas(size,size); poly(im,[(size/2,0),(size,size/2),(size/2,size),(0,size/2)],color)
    put(group,name,im,xy)

def slot(group,id,box,**extra):
    layouts[group]['text_slots'].append(dict(id=id,screen_rect=box,**extra))

def instance(group,id,asset,xy,**extra):
    layouts[group]['instances'].append(dict(id=id,asset=asset,screen_position=xy,**extra))

# START: shape extents measured from 960x540 reference preview, converted to 1080p.
im=parallelogram(1880,194,152,INK)
put('Start','Band',grain(im,1),(2,174),[154,0,154,0])
strip('Start','Line_Top',(96,154),1764,CYAN)
strip('Start','Line_Bottom',(66,368),1668,WHITE)
im=canvas(106,100)
poly(im,[(74,0),(86,0),(20,100),(8,100)],CYAN)
poly(im,[(42,28),(50,28),(8,78),(0,78)],CYAN)
put('Start','Slash_Accent',im,(1680,250))
put('Start','Tag',parallelogram(488,46,32,INK),(718,356),[34,0,34,0])
put('Start','Tag_EliteMark',parallelogram(126,38,24,(21,41,48,255)),(742,360),[26,0,26,0])
diamond('Start','Tick',(1134,374),CYAN)
strip('Start','Label_Line',(558,210),170,GRAY)
instance('Start','label_line_right','Label_Line',(1192,210))
im=canvas(82,118);line(im,[(0,117),(80,0)],WHITE)
put('Start','Title_Slash',im,(1228,228))
slot('Start','english_label',[754,194,430,28],align='center')
slot('Start','title',[706,224,518,122],align='center')
slot('Start','elite_label',[768,366,80,28],align='center')
slot('Start','encounter',[916,365,205,29],align='center')

# KILL FEED: a uniform row accommodating the longest boss entry.
im=parallelogram(590,50,30,INK)
line(im,[(30,0),(589,0),(560,49),(0,49),(30,0)],(108,119,123,255))
put('KillFeed','Row',im,(64,258),[34,0,34,0])
put('KillFeed','Row_Accent',parallelogram(22,34,16,CYAN),(88,266))
put('KillFeed','BossChip',parallelogram(60,22,4,PAPER),(562,274),[6,0,6,0])
im=canvas(18,34);line(im,[(0,33),(17,0)],WHITE)
put('KillFeed','Separator',im,(268,266))
layouts['KillFeed']['row_repeat_step_screen_px']=58
layouts['KillFeed']['row_repeat_step_png_px']=116
layouts['KillFeed']['row_height_screen_px']=50
layouts['KillFeed']['row_gap_screen_px']=8
for i in range(3):
    for part,xy in [('Row',(64,258)),('Row_Accent',(88,266)),('Separator',(268,266))]:
        instance('KillFeed',f'row_{i}_{part}',part,(xy[0],xy[1]+58*i))
instance('KillFeed','boss_chip','BossChip',(562,390))
slot('KillFeed','english_label',[120,270,142,26],repeat_y=58)
slot('KillFeed','enemy_and_status',[310,268,244,32],repeat_y=58)
slot('KillFeed','boss_label',[566,390,52,22])

# CUT-IN: normalized variant dimensions, transparent polygon windows.
for variant,xy in [('Power',(0,102)),('Unique',(0,310))]:
    w,h=1140,200
    im=canvas(w,h)
    right,bottom=(1140,1020) if variant=='Power' else (1100,964)
    shape=[(0,0),(right,0),(bottom,200),(0,200)]
    poly(im,shape,INK)
    window=[(8,8),(614,8),(504,186),(8,186)] if variant=='Power' else [(8,8),(658,8),(566,186),(8,186)]
    poly(im,window,(0,0,0,0))
    line(im,[(0,1),(right-3,1),(bottom-1,198),(0,198)],WHITE)
    line(im,[(window[1][0]+12,8),(window[2][0]+10,187)],GRAY)
    if variant=='Unique':
        line(im,[(0,5),(right-10,5),(bottom-5,193),(0,193)],GRAY)
    entry=put('CutIn','Frame_'+variant,im,xy,variant=variant)
    entry['art_window'] = dict(polygon_png_px=[[x*2,y*2] for x,y in window],
        polygon_screen=[[x+xy[0],y+xy[1]] for x,y in window],
        bounds_png_px=[16,16,(window[1][0]-8)*2,356],bounds_screen=[xy[0]+8,xy[1]+8,window[1][0]-8,178],
        masking='Polygon mask required; rectangular bounds alone include the diagonal name plate.')
    label_x=630 if variant=='Power' else 686
    label_w=414 if variant=='Power' else 318
    im=canvas(424,32);line(im,[(0,30),(label_w,30)],GRAY)
    put('CutIn','Label_Plate_'+variant,im,(xy[0]+label_x,xy[1]+26),variant=variant)
    im=parallelogram(136,24,12,CYAN if variant=='Unique' else (31,37,41,255))
    put('CutIn','Chip_'+variant,im,(xy[0]+694,xy[1]+148),variant=variant)
    slot('CutIn','label_'+variant,[648,xy[1]+22,392,28],variant=variant)
    slot('CutIn','name_'+variant,[646,xy[1]+66,392,74],variant=variant)
    slot('CutIn','chip_'+variant,[714,xy[1]+148,100,24],variant=variant)
strip('CutIn','Line',(630,158),414,CYAN)
instance('CutIn','line_unique','Line',(686,366),screen_size=[318,1])
diamond('CutIn','Diamond',(1040,151),CYAN,14)
instance('CutIn','diamond_unique','Diamond',(998,359))
im=canvas(250,8)
for x in range(0,248,12): line(im,[(x,7),(x+3,0)],GRAY)
put('CutIn','Ticks_Unique',im,(712,496),variant='Unique')

# VICTORY / DEFEAT share dimensions and all local positions.
for group,base,accent,tag in [('Victory',PAPER,CYAN,INK),('Defeat',INK,GRAY,GRAY)]:
    im=canvas(1920,220);rect(im,(0,0,1920,220),base)
    put(group,'Band',grain(im,11 if group=='Victory' else 12),(0,500),[96,0,96,0])
    strip(group,'Line_Top',(96,504),1762,accent)
    strip(group,'Line_Bottom',(44,714),696,accent)
    instance(group,'bottom_right','Line_Bottom',(1194,714),screen_size=[656,1])
    im=canvas(214,284)
    line(im,[(0,283),(212,0)],INK if group=='Victory' else GRAY)
    put(group,'Slash',im,(814,480))
    im=canvas(82,110);line(im,[(0,109),(80,0)],INK if group=='Victory' else GRAY)
    put(group,'Slash_Edge',im,(12,502))
    instance(group,'slash_right','Slash_Edge',(1838,608))
    im=canvas(42,56);line(im,[(0,55),(40,0)],INK if group=='Victory' else GRAY)
    put(group,'Slash_Short',im,(1840,502))
    instance(group,'slash_short_left','Slash_Short',(32,676))
    put(group,'Tag',parallelogram(392,28,22,tag),(760,688),[24,0,24,0])
    diamond(group,'Diamond',(1154,694),accent,18)
    strip(group,'Label_Line',(350,548),268,GRAY)
    instance(group,'label_line_right','Label_Line',(1300,548))
    strip(group,'Title_Line',(412,630),206,GRAY)
    instance(group,'title_line_right','Title_Line',(1300,630))
    slot(group,'english_label',[652,532,600,32],align='center')
    slot(group,'title',[670,568,590,106],align='center')
    slot(group,'tag_label',[782,690,340,22],align='center')
yy,xx=np.mgrid[-1:1:440j,-1:1:3840j]
alpha=np.round(235*np.exp(-(xx*xx*5+yy*yy*16))).astype(np.uint8)
flash=np.full((440,3840,4),255,dtype=np.uint8);flash[:,:,3]=alpha
e=put('Victory','Flash',Image.fromarray(flash),(0,500));e['alpha']='gradient'

# RESULT: common paper panel, state accents remain separate.
im=canvas(1114,656)
poly(im,[(64,12),(1113,12),(1113,604),(1056,655),(12,655),(12,64)],(8,10,12,255))
poly(im,[(52,0),(1100,0),(1100,590),(1048,642),(0,642),(0,52)],PAPER)
im=grain(im,42,0.8)
for x,y in [(44,596),(1064,42),(1056,596)]:
    line(im,[(x-12,y),(x+12,y)],GRAY)
    line(im,[(x,y-12),(x,y+12)],GRAY)
line(im,[(60,52),(80,52)],INK)
put('Result','Panel',im,(410,218),[84,80,84,80])['resize_axes']='both'
strip('Result','Rule_Top',(494,526),940,(178,183,185,255))
strip('Result','Rule_Bottom',(484,814),576,(178,183,185,255))
im=canvas(1,126);rect(im,(0,0,1,126),(174,180,184,255))
put('Result','Stat_Divider',im,(698,566))
instance('Result','divider_2','Stat_Divider',(960,566))
instance('Result','divider_3','Stat_Divider',(1222,566))
for state,color in [('Normal',INK),('Hover',(36,48,55,255)),('Pressed',(10,15,19,255))]:
    im=canvas(390,86)
    poly(im,[(28,0),(390,0),(390,56),(362,84),(0,84),(0,28)],color)
    rect(im,(0,81,362,3),CYAN);poly(im,[(362,81),(390,53),(390,57),(362,85)],CYAN)
    put('Result','Button_Return_'+state,im,(1078,734),[30,0,32,0],variant=state)
for state,color in [('Clear',CYAN),('Defeat',GRAY)]:
    im=canvas(56,4);rect(im,(0,0,56,4),color)
    put('Result','Accent_'+state,im,(494,498),variant=state)
    im=canvas(26,2);rect(im,(0,0,26,2),color)
    put('Result','Accent_Stat_'+state,im,(558,602),variant=state)
    for i,x in enumerate([816,1080,1340]): instance('Result',f'stat_accent_{state}_{i}',f'Accent_Stat_{state}',(x,602),variant=state)
    im=canvas(26,26);line(im,[(0,25),(25,25),(25,0)],color)
    put('Result','Accent_Corner_'+state,im,(1440,792),variant=state)
slot('Result','english_label',[494,306,600,24])
slot('Result','title',[490,338,880,88])
slot('Result','subtitle',[494,444,870,40])
for i,x in enumerate([494,752,1014,1276]):
    slot('Result',f'stat_{i+1}_label',[x,562,154,30],align='center')
    slot('Result',f'stat_{i+1}_value',[x,622,154,68],align='center')
slot('Result','button_label',[1120,746,284,42],align='center')
slot('Result','button_sublabel',[1152,790,220,14],align='center')
slot('Result','report_number',[470,244,36,20])

def font(size):
    return ImageFont.truetype('C:/Windows/Fonts/arial.ttf',size)

def checker(w,h):
    yy,xx=np.indices((h,w));v=np.where((xx//12+yy//12)%2==0,65,76).astype(np.uint8)
    return Image.fromarray(np.dstack([v,v,v,np.full_like(v,255)]))

def render_group(group,variant=None):
    scene=checker(1920,1080)
    layout=layouts[group]
    # Explicit instances replace the default placement when they exist without state sizing.
    replaced={i['asset'] for i in layout['instances'] if i['id'].startswith('row_') or i['id']=='boss_chip'}
    for e in layout['assets']:
        if e['id']=='Flash' or e['id'] in replaced:continue
        if group=='Result' and (e.get('variant') not in [None,variant,'Normal']):continue
        im=images[group,e['id']].resize((int(e['screen_rect'][2]),int(e['screen_rect'][3])),Image.Resampling.LANCZOS)
        scene.alpha_composite(im,tuple(e['screen_rect'][:2]))
    for inst in layout['instances']:
        if group=='Result' and inst.get('variant') not in [None,variant]:continue
        im=images[group,inst['asset']]
        size=inst.get('screen_size',[im.width//2,im.height//2])
        scene.alpha_composite(im.resize(tuple(size),Image.Resampling.LANCZOS),tuple(inst['screen_position']))
    return scene.convert('RGB').resize((960,540),Image.Resampling.LANCZOS)

def contact(group):
    entries=layouts[group]['assets']
    header=580 if group!='Result' else 1140
    rows=(len(entries)+1)//2
    sheet=Image.new('RGB',(1120,header+rows*150+20),(28,32,37));d=ImageDraw.Draw(sheet)
    d.text((24,12),group+' / 2x geometric redraw / text-free assets',font=font(22),fill='white')
    sheet.paste(render_group(group,'Clear'),(80,40))
    if group=='Result':sheet.paste(render_group(group,'Defeat'),(80,600))
    for k,e in enumerate(entries):
        x=20+(k%2)*550;y=header+(k//2)*150
        tile=checker(530,106)
        im=images[group,e['id']].copy();im.thumbnail((508,86),Image.Resampling.LANCZOS)
        tile.alpha_composite(im,((530-im.width)//2,(106-im.height)//2))
        sheet.paste(tile.convert('RGB'),(x,y))
        d.text((x,y+110),f"{e['id']}  {e['png_size_px'][0]}x{e['png_size_px'][1]}",font=font(16),fill=(222,230,235))
    sheet.save(OUT/group/'Contact_Sheet.jpg',quality=88)

def validate():
    checks=[]
    for group,layout in layouts.items():
        for e in layout['assets']:
            im=Image.open(OUT/group/e['file']);a=np.array(im)
            assert im.mode=='RGBA' and list(im.size)==e['png_size_px']
            assert im.width==e['screen_rect'][2]*2 and im.height==e['screen_rect'][3]*2
            assert e['scale']==2
            if e['alpha']=='binary':assert set(np.unique(a[:,:,3])).issubset({0,255}),e['id']
            if 'nine_slice_px' in e:
                b=e['nine_slice_px'];assert b['left']+b['right']<im.width and b['top']+b['bottom']<im.height
            if 'art_window' in e:
                mask=Image.new('1',im.size);pts=e['art_window']['polygon_png_px'];ImageDraw.Draw(mask).polygon(pts,fill=1)
                assert np.all(a[:,:,3][np.array(mask)]==0)
            checks.append(dict(file=f'{group}/{e["file"]}',size=list(im.size),alpha=e['alpha'],sha256=hashlib.sha256((OUT/group/e['file']).read_bytes()).hexdigest()))
        assert Image.open(OUT/group/'Contact_Sheet.jpg').width<=1200
    for group,names in [('CutIn',['Frame_Power','Frame_Unique']),('CutIn',['Label_Plate_Power','Label_Plate_Unique']),('CutIn',['Chip_Power','Chip_Unique']),('Result',['Button_Return_Normal','Button_Return_Hover','Button_Return_Pressed'])]:
        assert len({images[group,n].size for n in names})==1
    for name in ['Band','Line_Top','Line_Bottom','Slash','Tag','Diamond']:
        assert images['Victory',name].size==images['Defeat',name].size
    return checks

if __name__=='__main__':
    sources=[]
    for p in ROOT.glob('*_Mockup.png'):
        with Image.open(p) as im:assert im.size==(1920,1080)
        sources.append(dict(file=p.name,size=[1920,1080],sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
    for group,layout in layouts.items():
        (OUT/group/'layout.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
        contact(group)
    checks=validate()
    (OUT/'validation.json').write_text(json.dumps(dict(status='passed',asset_count=len(checks),source_images=sources,checks=checks),indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(status='passed',assets=len(checks),groups=list(layouts)),ensure_ascii=False))
