"""Four authored monochrome weapon illustrations. Vector-only, right-facing side profiles."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,subprocess,zipfile,hashlib

P=Path(__file__).resolve().parent;S=P/'SVG';R=P/'PNG';S.mkdir(parents=True,exist_ok=True);R.mkdir(exist_ok=True)
INK='#16181B'

def path(d,cut=False,stroke=0):
    c='black' if cut else 'white'
    return f'<path d="{d}" fill="{c if not stroke else "none"}"'+(f' stroke="{c}" stroke-width="{stroke}" stroke-linecap="round" stroke-linejoin="round"' if stroke else '')+'/>'
def rect(x,y,w,h,r=0,cut=False):return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{r}" fill="{"black" if cut else "white"}"/>'
def circle(x,y,r,cut=False):return f'<circle cx="{x}" cy="{y}" r="{r}" fill="{"black" if cut else "white"}"/>'

def revolver():
    a=[]
    # Deliberate silhouette: forward heavy vent-rib barrel, fluted cylinder and curved grip.
    a+=[path('M238 160L295 149H470L489 171H983L1007 187V274L983 287H544L509 316H456L433 341H317L290 371L220 353L195 318V202Q195 179 213 173Z')]
    # Sculpted grip with distinct frontstrap scallops and broad heel.
    a+=[path('M217 317L305 336L330 368Q313 386 307 405Q283 408 285 431Q269 446 268 466Q252 480 256 500L260 552Q267 588 243 603Q215 617 155 610L105 599Q82 591 87 565L113 469Q132 414 165 362L185 333Z')]
    # Trigger guard and actual curved trigger; opening is genuinely transparent.
    a+=[path('M319 312H486L492 363Q494 400 466 417Q447 429 394 426L355 421Q328 416 321 388L309 353Z')]
    a+=[path('M344 335H470L473 363Q474 388 454 399Q440 406 398 404L365 400Q345 396 342 377L336 349Z',True)]
    a+=[path('M399 327L414 329Q421 352 410 373Q405 384 393 387L383 385Q398 367 399 350Z')]
    # Cocking hammer, raised rear sight and ramped front blade.
    a+=[path('M216 174L198 159L169 142L160 122L180 104L208 116L231 144L241 169Z'),path('M258 151L262 131H316L323 151Z'),path('M896 169L925 130H970L978 169Z')]
    # Full lower barrel shroud and ejector-rod boss.
    a+=[path('M496 267H1006V305L989 318H579L553 310H502Z'),path('M479 300L498 286H542L536 317H488Z')]
    # Sight serrations and rib windows. Broad negative shapes survive roster scale.
    a+=[path('M179 122L198 132M177 132L203 147',True,4),path('M932 142H963',True,4)]
    for x in [539,655,771,887]:a+=[path(f'M{x} 177H{x+82}L{x+77} 189H{x-5}Z',True)]
    a+=[path('M505 203H965',True,5),path('M511 259H965',True,5),path('M970 198V269',True,6),path('M548 291H971',True,5)]
    # Cylinder profile: convex end caps, long machined flutes, crisp perimeter break.
    a+=[path('M293 181Q278 195 278 218V277Q279 303 297 314H441Q457 298 457 275V216Q457 194 441 181Z',True)]
    a+=[path('M299 188Q286 201 286 219V276Q286 295 300 307H436Q449 294 449 274V218Q449 199 436 188Z')]
    for y in [210,241,272]:a+=[path(f'M318 {y}H418Q429 {y+7} 418 {y+14}H318Q305 {y+7} 318 {y}Z',True)]
    a+=[path('M299 202V288M436 202V291',True,4),path('M468 196V275',True,5)]
    # Thumb latch and sideplate joint make this unmistakably a revolver, not a pistol.
    a+=[path('M216 225L242 214H264V236L233 248H216Z',True),path('M225 228L242 222H256V231L233 239H225Z')]
    a+=[path('M212 264V302L249 320L280 315M267 322L302 334',True,4),circle(235,287,6,True),circle(479,301,6,True)]
    # Grip inset follows the curved outer contour; sparse broad grooves, two pins.
    a+=[path('M204 366L280 389Q264 425 248 461Q235 489 239 536L243 565Q242 583 225 588L139 582Q116 578 121 560L145 475Q164 420 204 366Z',True)]
    a+=[path('M207 377L268 397L237 466Q225 501 230 542L234 561Q234 575 222 578L144 573Q130 571 134 556L156 480Q174 428 207 377Z')]
    for x,y in [(184,420),(174,447),(163,474),(154,501),(147,528)]:a+=[path(f'M{x} {y}L{x+51} {y+14}',True,5)]
    a+=[circle(223,405,5,True),circle(177,554,7,True),path('M173 554H181',False,2),path('M105 583L149 595H226',True,4)]
    return (1080,650),a

def sniper():
    a=[]
    # Chassis, adjustable skeleton stock and recoil pad.
    a+=[path('M70 247L247 223L383 231L422 252L517 247V323L458 346L387 329L325 350L286 422L104 431L62 411Z')]
    a+=[path('M44 234L69 235L78 250L70 431L58 445L36 440L38 257Z'),path('M103 207L262 195L311 211L313 229L107 243Z')]
    a+=[path('M115 280L255 259L338 272L292 313L211 319L190 374L104 385Z',True)]
    a+=[path('M253 331L288 324L267 396L218 399Z',True),path('M316 250L351 253L371 283L341 291Z',True)]
    a+=[path('M60 258L55 408M112 220L255 209',True,5),path('M111 409H198M254 407L280 353',True,5),circle(309,329,7,True)]
    # Receiver and long, tapered handguard.
    a+=[path('M412 238L452 220H771L795 236L1092 243L1113 261V301L1094 319L794 339H460L421 322Z')]
    a+=[path('M782 229H1098V245H781Z')]
    # Exposed barrel with a short imposing ported brake.
    a+=[path('M1075 262H1391L1410 256H1467V288H1409L1390 283H1075Z'),path('M1454 247H1547L1564 260V289L1547 301H1454Z')]
    # Grip, trigger guard and magazine.
    a+=[path('M454 326L518 336L516 360L482 462L419 448L411 436L447 349Z')]
    a+=[path('M516 329H638L646 366Q645 394 618 405L554 404Q532 401 528 380Z')]
    a+=[path('M537 345H624L628 366Q628 385 612 390L558 389Q546 387 543 376Z',True),path('M587 335H598Q603 356 591 373L578 378Q591 357 587 335Z')]
    a+=[path('M670 333H756L747 409L731 425H663Z')]
    # Scope: eyepiece, narrow tube, wide objective, separate rings and elevation turret.
    a+=[path('M448 145L505 143L515 150H696L727 133L809 121L838 136V195L809 210L727 195L696 179H515L505 187L448 185L431 174V156Z')]
    a+=[rect(419,145,22,40,3),rect(508,137,24,50,3),rect(674,138,24,49,3),rect(605,113,33,40,4),rect(596,103,51,15,3),circle(619,164,18)]
    a+=[path('M543 182H562V221H530V208L543 205Z'),path('M705 181H724V218H692V207L705 203Z')]
    # Scope joints, turret index, and objective front ring.
    a+=[path('M453 152V178M519 146V177M683 146V177M816 140V192',True,4),path('M536 158H594M645 158H668',True,4),circle(619,164,9,True),circle(619,164,3),path('M606 107V114M618 107V114M631 107V114',True,3)]
    # Receiver bolt with downturned knob and readable ejection port.
    a+=[path('M465 245H651L667 257L651 274H463Z',True),path('M472 251H644L653 257L643 265H472Z')]
    a+=[path('M625 275L653 290L672 318',False,12),circle(677,325,15),circle(677,325,6,True)]
    a+=[path('M439 299H637M766 245V315',True,5),circle(446,280,6,True),circle(732,296,6,True),path('M682 351H739M679 373H737M676 395H733',True,5)]
    # Handguard cutouts cut through the silhouette, not filled fake gray panels.
    for x in [815,879,943,1007]:
        a+=[path(f'M{x} 258H{x+42}L{x+34} 278H{x-4}Z',True),path(f'M{x-5} 292H{x+35}L{x+30} 303H{x-7}Z',True)]
    # Fine barrel fluting with three broad brake ports.
    a+=[path('M1135 270H1380',True,4)]
    for x in [1470,1498,1526]:a+=[path(f'M{x} 258H{x+13}V290H{x-4}Z',True)]
    # Deployment feet on a compact bipod: distinct from the receiver grip and magazine.
    a+=[path('M1068 312L1085 315L1047 437L1034 433Z'),path('M1083 315L1095 313L1161 430L1147 438Z'),path('M1017 431H1053V443H1012Z'),path('M1144 432H1178L1183 445H1145Z')]
    a+=[circle(1082,317,12),circle(1082,317,5,True),path('M1055 368L1043 412M1110 362L1141 417',True,3)]
    # Grip panel and texture don't become a cluttered checkerboard.
    a+=[path('M462 354L496 360L468 439L431 430Z',True),path('M467 367L486 371L465 426L443 421Z')]
    for i in range(3):a+=[path(f'M{453-i*5} {393+i*13}L{474-i*5} {400+i*13}',True,3)]
    return (1600,500),a

def smg():
    a=[]
    # Telescoping shoulder stock: two struts and a curved rubber pad.
    a+=[path('M52 228Q48 210 63 202L92 196L101 207L93 347L80 364L51 356Z')]
    a+=[path('M92 221L302 204L344 217V240L91 251Z'),path('M90 294L306 277L337 290L334 312L84 322Z')]
    a+=[path('M111 229L293 215M109 301L294 287M67 223L61 339',True,5)]
    # Upper housing and handguard, compact proportions and forward-weighted receiver.
    a+=[path('M303 197L330 177H756L777 191L937 196L959 220V287L937 310H762L742 325H323L294 306V221Z')]
    a+=[path('M936 223H1016V282H941Z'),path('M1000 211H1057L1071 224V278L1056 291H1000Z')]
    # Lower receiver, swept grip, correctly placed trigger opening and curved box magazine.
    a+=[path('M327 314H484L488 348L460 385L431 388L403 501L378 520L315 503L303 487L344 362Z')]
    a+=[path('M477 317H613L626 353Q623 389 593 402H529L506 391Z')]
    a+=[path('M501 334H598L608 355Q605 376 587 384H536L522 377Z',True),path('M556 324H568Q579 344 561 367L547 371Q561 351 556 324Z')]
    a+=[path('M647 315H712L718 373Q727 451 774 508L766 527L713 548L695 538Q654 480 645 408L635 347Z')]
    # Robust lower magazine well and foregrip.
    a+=[path('M626 316H731V350L714 365H644L627 351Z'),path('M803 309L848 308L860 329L839 401L804 395L790 379Z')]
    # Rail and compact open reflex sight, rear aperture and front sight.
    a+=[path('M344 165H868V179H344Z')]
    for x in range(350,860,26):a+=[rect(x,158,15,9)]
    a+=[path('M479 108L495 94H576L592 112V155H479Z'),path('M497 111H570L577 119V141H491V119Z',True),rect(468,149,134,13,2)]
    a+=[path('M332 177L334 141H356L367 158V177Z'),path('M881 196L889 153H912L920 197Z')]
    a+=[path('M896 164H906V187H892Z',True)]
    # Ejection opening and charging handle.
    a+=[path('M393 212H551L564 222L555 240H391Z',True),path('M405 218H545L553 223L547 232H405Z')]
    a+=[path('M447 239L485 240L488 254L450 258Z'),path('M321 270H535L552 281H723',True,5),path('M758 210V294',True,5)]
    # Distinct handguard vent families: five oblique upper slots and three lower ports.
    for x in [781,815,849,883]:a+=[path(f'M{x} 215H{x+19}L{x+12} 241H{x-7}Z',True)]
    for x in [787,841,895]:a+=[rect(x,269,34,12,5,True)]
    for x in [1016,1037]:a+=[path(f'M{x} 224H{x+11}V277H{x-4}Z',True)]
    a+=[path('M946 246H986',True,5)]
    # Magazine grooves follow its curve and foot plate has a clear negative join.
    a+=[path('M661 368Q668 458 720 523M686 365Q695 452 743 513',True,6),path('M711 528L757 512',True,5)]
    # Grip contour, sparse traction cuts and fasteners.
    a+=[path('M357 374L414 391L391 484L378 497L328 482Z',True),path('M365 389L403 400L383 480L377 484L342 475Z')]
    for i in range(4):a+=[path(f'M{356-i*4} {416+i*16}L{391-i*4} {425+i*16}',True,4)]
    a+=[circle(365,302,7,True),circle(605,301,7,True),circle(747,274,5,True),path('M586 275L610 283L595 294',True,4),path('M809 339L832 344M805 354L829 360M802 369L825 375',True,4)]
    return (1120,590),a

def railgun():
    a=[]
    # A coherent VERTEX precision accelerator: skeleton stock, solid core, open parallel rails.
    a+=[path('M58 226L104 199L289 180L379 195L419 222V331L383 361L253 359L184 414L81 427L48 409L64 340L93 303Z')]
    a+=[path('M100 233L278 213L351 232V270L275 292L138 312L111 286Z',True)]
    a+=[path('M106 344L233 326L191 372L101 390Z',True),path('M63 247L75 299L48 383',True,5),path('M137 216L284 201M99 409L176 399',True,5)]
    # Accelerator core is faceted without being a stack of indistinguishable rectangles.
    a+=[path('M370 188L414 165H710L754 185L815 227V331L766 374L706 393H411L370 366L350 321V225Z')]
    # Long rails: asymmetric armor plates around a completely open central channel.
    a+=[path('M748 196L809 166H1299L1369 193L1426 229L1408 255L1335 243H805L774 257Z')]
    a+=[path('M777 313L812 325H1328L1411 313L1430 340L1365 378L1288 401H808L752 367Z')]
    # Parallel internal rails are kept attached to the core, separated by transparent space.
    a+=[path('M785 263H1336L1374 274L1356 284H792Z'),path('M792 296H1358L1377 306L1340 317H785Z')]
    # Rail side panels, long channels, triangular relieving windows.
    a+=[path('M836 184H1286L1331 200H835Z',True),path('M831 209H1305L1355 228H829Z',True),path('M837 345H1313L1276 368H832Z',True),path('M836 380H1244L1217 388H836Z',True)]
    for x in [906,1030,1154]:
        a+=[path(f'M{x} 170H{x+15}V241H{x-5}Z',True),path(f'M{x-2} 329H{x+16}V398H{x-3}Z',True)]
    # Rail-tip blocks have distinctive slanted slots, never glowing energy colors.
    a+=[path('M1353 202L1366 209L1392 228L1383 238L1362 229Z',True),path('M1364 346L1395 332L1403 340L1370 360Z',True)]
    # Core coils are stepped negative cuts following the angular case; central spindle stays connected.
    for x in [498,556,614,672]:
        a+=[path(f'M{x} 186H{x+14}L{x+35} 211V262L{x+22} 273L{x+35} 286V346L{x+14} 371H{x}L{x+20} 343V291L{x+7} 274L{x+20} 258V216Z',True)]
    # Rear side access port and broad shallow diagonal vent, with retained bolt heads.
    a+=[path('M392 222L423 201H476V245L447 270H389Z',True),path('M402 227L426 211H466V239L443 258H400Z')]
    a+=[path('M389 300H463L476 314V341H412L389 325Z',True),path('M401 307H454L466 317V333H417L401 322Z')]
    a+=[circle(408,282,6,True),circle(745,282,8,True),circle(745,282,3)]
    # Sight is low and geometric, different from SMG reflex or conventional sniper optic.
    a+=[path('M487 162V137L511 119H646L669 138V164Z'),path('M510 139H640L648 145V153H505Z',True),rect(537,110,54,11,2)]
    # Trigger guard with open interior, swept grip, broad detachable capacitor pack.
    a+=[path('M391 366L452 386L440 416L419 509L389 526L338 508L328 487L362 392Z')]
    a+=[path('M453 378H586L599 420Q598 446 569 459L512 457L487 438Z')]
    a+=[path('M474 395H571L582 421Q580 438 564 444H520L500 431Z',True),path('M533 385H548Q556 409 538 427L524 431Q540 410 533 385Z')]
    a+=[path('M620 389H744L765 414L750 474L726 493H621L598 474L604 416Z')]
    # Power pack and grip openings share the same chamfer language.
    a+=[path('M627 416H721L735 428L724 463L713 473H634L620 459Z',True)]
    for x in [636,663,690]:a+=[path(f'M{x} 426H{x+13}L{x+7} 461H{x-6}Z')]
    a+=[path('M377 404L422 418L403 493L389 506L349 491Z',True),path('M382 418L411 428L394 489L387 494L361 484Z')]
    for i in range(3):a+=[path(f'M{374-i*4} {444+i*16}L{399-i*4} {451+i*16}',True,4)]
    a+=[path('M640 482H714',True,4)]
    return (1480,570),a

items=[('Revolver','리볼버','Cylinder / heavy underlug',revolver),('Sniper','저격총','Precision chassis / long optic',sniper),('SMG','기관단총','Compact receiver / curved magazine',smg),('Railgun','레일건','Twin open rails / capacitor core',railgun)]
meta={}
for name,kr,desc,fn in items:
    (w,h),body=fn();mid=name.lower()+'_silhouette'
    s=f'''<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}" color="{INK}" role="img" aria-labelledby="title desc">
<title id="title">VERTEX — {kr}</title>
<desc id="desc">Single-ink right-facing side-profile illustration. True transparent negative space. Change the root color to recolor.</desc>
<defs><mask id="{mid}" maskUnits="userSpaceOnUse" maskContentUnits="userSpaceOnUse" x="0" y="0" width="{w}" height="{h}" style="mask-type:luminance">
<rect width="{w}" height="{h}" fill="black"/>
{''.join(body)}
</mask></defs>
<rect width="{w}" height="{h}" fill="currentColor" mask="url(#{mid})"/>
</svg>'''
    (S/f'Weapon_{name}_Mono.svg').write_text(s,encoding='utf-8')
    meta[name]={'name_ko':kr,'svg':f'SVG/Weapon_{name}_Mono.svg','png':f'PNG/Weapon_{name}_Mono.png','viewBox':[0,0,w,h],'png_scale':2,'facing':'right','ink':INK,'recolor':'root color attribute / currentColor','detail':desc}

renderer=P/'render.cjs'
renderer.write_text("const fs=require('fs'),p=require('path');const {Resvg}=require(process.env.VERTEX_RESVG_MODULE||'C:/Users/SKYLIGHT/.codex/tmp/currency-f-render/node_modules/@resvg/resvg-js');for(const f of fs.readdirSync(process.argv[2]))if(f.endsWith('.svg'))fs.writeFileSync(p.join(process.argv[3],f.replace('.svg','.png')),new Resvg(fs.readFileSync(p.join(process.argv[2],f)),{fitTo:{mode:'zoom',value:2}}).render().asPng());",encoding='utf-8')
subprocess.run(['node',str(renderer),str(S),str(R)],check=True)
# Resvg's premultiplied-alpha rounding can perturb RGB on nearly transparent edges.
# Keep its exact coverage, but encode one constant ink in the straight-alpha PNG.
for f in R.glob('Weapon_*_Mono.png'):
    im=Image.open(f).convert('RGBA');alpha=im.getchannel('A');out=Image.new('RGBA',im.size,INK);out.putalpha(alpha);out.save(f)

# Presentation board remains separate from the four transparent monochrome deliverables.
W,H=2000,1810
board=Image.new('RGB',(W,H),'#EDF0F3');d=ImageDraw.Draw(board)
bold='C:/Windows/Fonts/malgunbd.ttf';regular='C:/Windows/Fonts/malgun.ttf';latin='C:/Windows/Fonts/bahnschrift.ttf'
d.text((80,46),'VERTEX / WEAPON STUDIES',font=ImageFont.truetype(latin,34),fill=INK)
d.text((80,96),'MONOCHROME  ·  04 SIDE PROFILES',font=ImageFont.truetype(latin,17),fill='#6B737B')
d.line((80,142,1920,142),fill='#BDC5CD',width=2)
for i,(name,kr,desc,fn) in enumerate(items):
    y=171+i*395
    d.text((80,y+30),f'0{i+1}',font=ImageFont.truetype(latin,22),fill='#78838D')
    d.text((80,y+73),kr,font=ImageFont.truetype(bold,38),fill=INK)
    d.text((82,y+128),name.upper(),font=ImageFont.truetype(latin,20),fill='#65717B')
    im=Image.open(R/f'Weapon_{name}_Mono.png').convert('RGBA');bbox=im.getbbox();meta[name]['alpha_bounds_pixels']=list(bbox)
    im=im.crop(bbox)
    maxsize=(1320,342) if name!='Revolver' else (950,348)
    im.thumbnail(maxsize,Image.Resampling.LANCZOS)
    # Revolver can breathe at a larger silhouette height; long arms share rail baseline.
    x=520+(1320-im.width)//2;top=y+(358-im.height)//2
    board.paste(im,(x,top),im)
    d.line((80,y+378,1920,y+378),fill='#CBD2D9',width=1)
board.save(P/'Weapon_Monochrome_Overview.png')

# Actual small-size rendering, two contrasting backdrops; no display-only recoloring in deliverables.
test=Image.new('RGB',(1400,610),'#EDF0F3');draw=ImageDraw.Draw(test)
draw.rectangle((0,305,1400,610),fill=INK)
for i,(name,kr,_,_) in enumerate(items):
    im=Image.open(R/f'Weapon_{name}_Mono.png').convert('RGBA');im=im.crop(im.getbbox());im.thumbnail((300,210),Image.Resampling.LANCZOS)
    x=25+i*350+(300-im.width)//2;y=50+(210-im.height)//2
    test.paste(im,(x,y),im)
    white=Image.new('RGBA',im.size,'#EEF0F2');white.putalpha(im.getchannel('A'));test.paste(white,(x,y+305),white)
    draw.text((25+i*350,16),name.upper(),font=ImageFont.truetype(latin,17),fill='#596670')
    draw.text((25+i*350,321),name.upper(),font=ImageFont.truetype(latin,17),fill='#98A5B0')
test.save(P/'Weapon_Monochrome_SmallScale.png')

for name,a in meta.items():
    im=Image.open(P/a['png']).convert('RGBA');assert im.size==(a['viewBox'][2]*2,a['viewBox'][3]*2)
    bbox=im.getbbox();assert bbox[0]>0 and bbox[1]>0 and bbox[2]<im.width and bbox[3]<im.height
    colors=set()
    for rgba in im.get_flattened_data():
        if rgba[3]>0:colors.add(rgba[:3])
    # No shaded material, second ink or background fill: only alpha antialiasing.
    assert colors=={(22,24,27)},(name,colors)
    s=(P/a['svg']).read_text(encoding='utf-8');assert '<image' not in s and '<filter' not in s and 'Gradient' not in s
    a['png_sha256']=hashlib.sha256((P/a['png']).read_bytes()).hexdigest()
(P/'manifest.json').write_text(json.dumps({'style':'single ink / transparent cutouts','assets':meta},ensure_ascii=False,indent=2),encoding='utf-8')
(P/'README.txt').write_text('VERTEX — Monochrome weapon set v2\n\nFour authored SVG profiles: Revolver, Sniper, SMG, Railgun. All face right, without perspective distortion. Every visible pixel uses one ink (#16181B); interior grooves/windows are true transparent cutouts. No gradients, glow, shadows, embedded raster or secondary colored detail.\n\nSVG uses only vector geometry plus a vector luminance mask; black/white mask shapes encode cutouts, not visible colors. Change the root color attribute to recolor every part. Transparent PNG renders at 2x included for convenience. Overview and SmallScale are presentation sheets, not sprites.\n\nOriginal WeaponSVG files untouched. These new sibling files are not auto-wired into Unity or existing mockups. Manifest records viewBoxes, alpha bounds and checksums.\n\nRebuild: python build.py. Dependencies: Pillow, Node, @resvg/resvg-js (override VERTEX_RESVG_MODULE), Windows fonts for presentation sheets only.\n',encoding='utf-8')
with zipfile.ZipFile(P/'Weapon_Monochrome_v2.zip','w',zipfile.ZIP_DEFLATED) as z:
    for f in [*sorted(S.glob('*.svg')),*sorted(R.glob('*.png')),P/'manifest.json',P/'README.txt',Path(__file__),renderer,P/'Weapon_Monochrome_Overview.png',P/'Weapon_Monochrome_SmallScale.png']:z.write(f,f.relative_to(P))
print(json.dumps({'svg_count':len(meta),'png_count':len(meta),'visible_rgb':[22,24,27],'transparent':True,'all_edges_padded':True,'output':str(P)}))
