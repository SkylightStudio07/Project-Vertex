from pathlib import Path
from PIL import Image,ImageDraw,ImageFont,ImageOps,ImageChops
import subprocess,shutil,json
P=Path(__file__).resolve().parent
S=P/'Sources'; S.mkdir(exist_ok=True)
INK='#16181B'; PAPER='#EEF0F2'; CYAN='#0DB8F2'
shutil.copy2('C:/Users/SKYLIGHT/.codex/generated_images/01a0fc46-a6a9-77f0-8393-820b3b34e74e/exec-221edda7-1a84-4e8e-b00a-51e9e54134ac.png',S/'Thumb_Generated.png')
base=Image.open(P/'Reference_Lobby_Current.png').convert('RGBA').resize((1920,1080),Image.Resampling.LANCZOS)
thumb=ImageOps.fit(Image.open(S/'Thumb_Generated.png').convert('RGB'),(255,116))

def panel(x,y,w,h,fill):
 return f'<path d="M{x} {y}H{x+w-18}L{x+w} {y+18}V{y+h}H{x}Z" fill="{fill}" stroke="{INK}" stroke-width="1.5"/>'
archive='<g fill="none" stroke="#16181B" stroke-width="2.4"><path d="M4 8V34H29V8ZM10 3H35V29M9 16H24M9 22H24"/></g>'
people='<g fill="#16181B"><circle cx="15" cy="11" r="6"/><circle cx="28" cy="14" r="5"/><path d="M3 35V29Q3 20 15 20Q27 20 27 29V35ZM28 23Q37 23 37 31V35H30V29Q30 26 28 23Z"/></g>'
def render(v):
 shapes=panel(40,895,520,155,INK)+panel(575,895,185,72,PAPER)+panel(575,978,185,72,PAPER)
 if v=='A': shapes+=panel(40,760,720,120,PAPER)
 else: shapes+=panel(40,760,195,34,INK)
 for y,icon in [(911,archive),(994,people)]:shapes+=f'<g transform="translate(590 {y})">{icon}</g>'
 shapes+=f'<path fill="{CYAN}" d="M731 853L741 860L731 867Z"/>'
 shapes+=f'<path fill="{CYAN}" d="M57 1023L65 1028L57 1033Z"/>'
 for x,y,c in [(58,911,PAPER),(583,898,INK),(583,981,INK)]:
  shapes+=f'<path stroke="{c}" d="M{x} {y+14}H{x+9}"/>'
 shapes+=f'<path stroke="{INK}" d="M58 796H72"/>' if v=='A' else ''
 svg=f'<svg xmlns="http://www.w3.org/2000/svg" width="3840" height="2160" viewBox="0 0 1920 1080">{shapes}</svg>'
 path=S/f'Layout_{v}.svg';path.write_text(svg)
 js="const fs=require('fs');const {Resvg}=require('C:/Users/SKYLIGHT/.codex/tmp/currency-f-render/node_modules/@resvg/resvg-js');fs.writeFileSync(process.argv[2],new Resvg(fs.readFileSync(process.argv[1])).render().asPng());"
 subprocess.run(['node','-e',js,str(path),str(S/f'Overlay_{v}.png')],check=True)
 return Image.open(S/f'Overlay_{v}.png').resize((1920,1080),Image.Resampling.LANCZOS)

def font(size,bold=False,latin=False):return ImageFont.truetype('C:/Windows/Fonts/'+('bahnschrift.ttf' if latin else 'malgunbd.ttf' if bold else 'malgun.ttf'),size)
def text(im,xy,s,size,color=INK,bold=False,latin=False):ImageDraw.Draw(im).text(xy,s,font=font(size,bold,latin),fill=color,stroke_width=0)

for v in ['A','B']:
 im=base.copy()
 if v=='B':
  # Remove only the obsolete baked-in dialogue label/box using adjacent source ground.
  # No generated background or edits to character/menu; patch stops before leg silhouette.
  patch=base.crop((70,748,365,814)).resize((295,70),Image.Resampling.LANCZOS)
  im.paste(patch,(70,815))
 im.alpha_composite(render(v))
 im.paste(thumb,(304,897))
 # A quiet diagonal edge echoes the lobby kit, without framing the illustration.
 ImageDraw.Draw(im).polygon([(290,897),(322,897),(304,1013),(290,1013)],fill=INK)
 if v=='A':
  text(im,(58,775),'01',12,INK,latin=True)
  text(im,(87,770),'치하라 쇼',20,INK,True)
  text(im,(629,777),'DIALOGUE',10,INK,latin=True)
  text(im,(60,808),'비 오는 날엔 총기 손질이 두 배야.',23,INK)
  text(im,(60,840),'다음 여정을 준비할까요?',23,INK)
 else:
  text(im,(54,766),'01',12,PAPER,latin=True)
  text(im,(81,762),'치하라 쇼',20,PAPER,True)
  text(im,(251,770),'DIALOGUE',11,PAPER,latin=True)
  text(im,(44,805),'비 오는 날엔 총기 손질이 두 배야.',23,PAPER,True)
  text(im,(44,840),'다음 여정을 준비할까요?',23,PAPER,True)
 text(im,(58,906),'02  /  STORY',12,PAPER,latin=True)
 text(im,(58,929),'CH.2-1',16,PAPER,latin=True)
 text(im,(58,951),'영원하지 않기에',23,PAPER,True)
 text(im,(58,979),'빛나는 것들',23,PAPER,True)
 text(im,(73,1016),'다음 목표: 1막 보스 격파',17,PAPER)
 for y,label,eng,num in [(895,'아카이브','ARCHIVES','03'),(978,'동료','COMPANIONS','04')]:
  text(im,(639,y+11),label,20,INK,True)
  text(im,(639,y+42),eng,10,INK,latin=True)
  text(im,(583,y+3),num,8,INK,latin=True)
 im.convert('RGB').save(P/f'LobbyBottomLeft_{v}_Mockup.png')
 im.crop((24,744,776,1065)).convert('RGB').save(P/f'LobbyBottomLeft_{v}_Detail.png')
 diff=ImageChops.difference(base.convert('RGB'),im.convert('RGB'))
 ImageDraw.Draw(diff).rectangle((36,756,764,1054),fill=0)
 assert diff.getbbox() is None,'Changed pixels outside lower-left area'
(P/'BUILD_NOTES.json').write_text(json.dumps({'outputs':['LobbyBottomLeft_A_Mockup.png','LobbyBottomLeft_B_Mockup.png'],'canvas':[1920,1080],'thumbnail':'image_gen, original in Sources/Thumb_Generated.png','ui':'SVG rendered to PNG; typography composed with Pillow; no shadows','background_validation':'Exact resized original outside x36..764,y756..1054','B_note':'Old baked-in dialogue box removed using adjacent original ground sample only, rect [70,815,292,70].','delivery_scope':'A/B mockups only; extracted sprites/layout after selection as brief requests.'},indent=2),encoding='utf-8')
print('A/B created; original background preservation check passed.')
