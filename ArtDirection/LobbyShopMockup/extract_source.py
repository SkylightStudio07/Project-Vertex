from pathlib import Path
import sys,json
sys.path.insert(0,'C:/Users/SKYLIGHT/.codex/tmp/shop-seg')
import cv2
import numpy as np
from PIL import Image,ImageDraw,ImageFilter
P=Path(__file__).resolve().parent; O=P/'Extracted_Draft';O.mkdir(exist_ok=True)
src=Image.open(P/'Reference_Shop_Draft.png').convert('RGB')
items={'Item_PartsBundle':(814,224,1044,384),'Item_SupplyPackage':(1378,535,1596,704),'Item_Coffee':(1418,217,1590,384),'Item_Notebook':(835,528,1012,706)}
meta={}
for name,box in items.items():
    im=src.crop(box); a=np.array(im)
    mask=np.full(a.shape[:2],cv2.GC_PR_BGD,np.uint8)
    mask[3:-3,3:-3]=cv2.GC_PR_FGD
    mask[:2]=0;mask[-2:]=0;mask[:,:2]=0;mask[:,-2:]=0
    cv2.grabCut(a,mask,None,np.zeros((1,65)),np.zeros((1,65)),6,cv2.GC_INIT_WITH_MASK)
    alpha=np.where((mask==1)|(mask==3),255,0).astype('uint8')
    # Mild edge antialiasing; RGB pixels remain source-derived.
    matte=Image.fromarray(alpha).filter(ImageFilter.GaussianBlur(.35))
    rgba=im.convert('RGBA');rgba.putalpha(matte)
    rgba.save(O/(name+'.png'))
    meta[name]={'source_rect_ltrb':box,'pixel_size':list(rgba.size),'method':'GrabCut foreground extraction; original RGB retained'}
# Dense outer silhouette; uncertain boundary is refined by GrabCut.
poly=[(441,40),(482,43),(519,56),(548,82),(568,116),(573,139),(591,147),(606,169),(598,183),(625,209),(634,228),(617,251),(603,272),(617,297),(638,333),(656,386),(663,438),(656,474),(650,510),(664,551),(682,587),(691,630),(701,659),(710,678),(718,700),(735,719),(758,733),(773,755),(789,777),(786,798),(775,809),(753,805),(738,789),(728,777),(712,771),(708,808),(711,849),(720,895),(723,941),(400,941),(399,898),(380,872),(354,849),(319,837),(286,849),(263,869),(240,871),(244,824),(240,776),(247,725),(222,716),(202,712),(179,712),(153,715),(131,720),(103,722),(78,722),(50,721),(26,719),(32,699),(50,677),(70,667),(84,647),(91,619),(99,590),(108,565),(120,547),(141,535),(151,509),(169,485),(177,462),(193,441),(212,414),(232,383),(254,351),(275,329),(294,310),(303,291),(330,282),(356,274),(370,262),(362,242),(356,214),(352,187),(355,153),(360,120),(372,89),(389,64),(412,48)]
sil=Image.new('L',src.size);ImageDraw.Draw(sil).polygon(poly,fill=255)
arr=np.array(sil)
kernel=np.ones((11,11),np.uint8)
inside=cv2.erode(arr,kernel)>0;outside=cv2.dilate(arr,kernel)==0
mask=np.where(arr>0,cv2.GC_PR_FGD,cv2.GC_PR_BGD).astype('uint8')
mask[inside]=cv2.GC_FGD;mask[outside]=cv2.GC_BGD
cv2.grabCut(np.array(src),mask,None,np.zeros((1,65)),np.zeros((1,65)),4,cv2.GC_INIT_WITH_MASK)
alpha=Image.fromarray(np.where((mask==1)|(mask==3),255,0).astype('uint8'))
# Original dialogue occludes pixels here. Do not invent hidden anatomy or repaint.
occlusion=[(31,811),(67,778),(468,778),(468,850),(426,890),(31,890)]
ImageDraw.Draw(alpha).polygon(occlusion,fill=0)
alpha=alpha.filter(ImageFilter.GaussianBlur(.3))
keeper=src.convert('RGBA');keeper.putalpha(alpha)
bound=(16,34,795,941)
keeper.crop(bound).save(O/'Keeper_Full.png')
meta['Keeper_Full']={'source_rect_ltrb':bound,'pixel_size':[779,907],'method':'Original visible pixels only; boundary segmentation','occluded_by_dialogue_source_polygon':occlusion,'limitation':'Not an unseen full-body reconstruction. Source crops the legs and dialogue occludes part of lower-left body; those pixels remain transparent. Use with dialogue at source placement.'}
(O/'extraction_sources.json').write_text(json.dumps(meta,indent=2),encoding='utf-8')
# Contact sheet on checkerboard for alpha inspection.
sheet=Image.new('RGB',(1100,700),'#c5c8cc');d=ImageDraw.Draw(sheet)
for y in range(0,700,20):
 for x in range(0,1100,20):
  if (x//20+y//20)%2:d.rectangle((x,y,x+19,y+19),fill='#e4e6e8')
for i,n in enumerate(items):
 im=Image.open(O/(n+'.png'));im.thumbnail((240,190));sheet.paste(im,(20+(i%2)*260,30+(i//2)*250),im)
im=Image.open(O/'Keeper_Full.png');im.thumbnail((530,680));sheet.paste(im,(560,10),im)
sheet.save(P/'Extraction_Review.png')
print('Source extraction complete')
