# 무기고 무기 그림 — 손으로 짠 SVG (측면도, 총구 오른쪽). python -I build_weapons.py
# 두 벌: Dark(회백 판 위, 먹색 몸통) / Light(먹색 칸 위, 회백 몸통). 강조색은 청록 하나.
import pathlib, subprocess

OUT = pathlib.Path(__file__).parent
RESVG = 'C:/Users/SKYLIGHT/.codex/tmp/currency-f-render/node_modules/@resvg/resvg-js'
PAL = {
    'Dark':  dict(body='#1D2125', part='#2F353B', line='#4E565E', hi='#3A4148', acc='#0DB8F2'),
    'Light': dict(body='#D9DEE2', part='#AEB6BD', line='#7E878F', hi='#EEF0F2', acc='#0DB8F2'),
}


def poly(pts, fill, extra=''):
    return f'<polygon points="{" ".join(f"{x},{y}" for x, y in pts)}" fill="{fill}" {extra}/>'


def rect(x, y, w, h, fill, rx=0, extra=''):
    return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" fill="{fill}" {extra}/>'


def line(x1, y1, x2, y2, c, w=3):
    return f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{c}" stroke-width="{w}" stroke-linecap="square"/>'


def path(d, fill='none', stroke='none', w=0):
    return f'<path d="{d}" fill="{fill}" stroke="{stroke}" stroke-width="{w}" stroke-linejoin="round" stroke-linecap="round"/>'


def circle(cx, cy, r, fill):
    return f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="{fill}"/>'


def pistol(p):
    s = []
    s.append(rect(46, 100, 18, 30, p['part']))                                                   # 공이치기
    s.append(poly([(226, 168), (548, 168), (548, 198), (470, 204), (300, 206), (232, 206)], p['part']))  # 프레임 앞
    s.append(path('M236 204 L250 262 Q256 276 272 276 L320 276 Q334 276 334 262 L334 204 L318 204 L318 256 '
                  'Q318 262 312 262 L276 262 Q268 262 266 254 L254 204 Z', p['part']))          # 방아쇠울
    s.append(path('M286 206 Q292 236 280 252 L292 252 Q304 234 298 206 Z', p['body']))           # 방아쇠
    s.append(poly([(80, 166), (236, 166), (224, 214), (170, 384), (158, 398), (60, 398), (46, 386), (94, 212)], p['body']))  # 손잡이
    s.append(poly([(104, 222), (208, 222), (162, 372), (78, 372)], p['part']))                    # 그립 패널
    for i in range(6):
        s.append(line(116 - i * 5, 240 + i * 22, 188 - i * 5, 240 + i * 22, p['line'], 3))        # 그립 결
    s.append(rect(56, 396, 108, 8, p['acc'], rx=2))                                               # 탄창 바닥
    s.append(poly([(70, 88), (540, 88), (554, 102), (554, 170), (70, 170), (56, 156), (56, 102)], p['body']))  # 슬라이드
    s.append(rect(70, 88, 470, 10, p['hi']))
    for i in range(7):
        s.append(line(86 + i * 11, 104, 86 + i * 11, 156, p['line'], 4))                         # 세레이션
    s.append(rect(330, 104, 96, 24, p['part'], rx=3))                                             # 배출구
    s.append(rect(554, 116, 14, 36, p['part']))                                                   # 총구
    s.append(rect(74, 74, 34, 16, p['body']))                                                     # 가늠자
    s.append(rect(516, 78, 18, 12, p['body']))                                                    # 가늠쇠
    s.append(rect(526, 80, 6, 6, p['acc']))
    s.append(line(300, 160, 530, 160, p['acc'], 3))
    return (600, 420), s


def smg(p):
    s = []
    s.append(poly([(206, 118), (70, 126), (46, 206), (78, 214), (98, 154), (206, 150)], p['part']))  # 접이식 개머리
    s.append(poly([(86, 150), (196, 146), (196, 134), (90, 140)], p['body']))
    s.append(rect(640, 128, 170, 44, p['part']))                                                  # 총열 덮개
    for i in range(5):
        s.append(rect(660 + i * 28, 140, 16, 20, p['body'], rx=3))
    s.append(rect(810, 136, 34, 28, p['body']))                                                   # 소염기
    s.append(rect(812, 142, 30, 4, p['line']))
    s.append(poly([(390, 186), (456, 186), (436, 306), (372, 306)], p['body']))                   # 손잡이
    s.append(poly([(488, 186), (538, 186), (556, 322), (504, 322)], p['part']))                   # 탄창
    s.append(rect(500, 312, 58, 10, p['acc'], rx=2))
    s.append(path('M330 190 L340 236 Q344 248 356 248 L386 248 L386 236 L360 236 Q354 236 352 230 L346 190 Z', p['part']))
    s.append(poly([(206, 108), (640, 108), (652, 120), (652, 192), (206, 192), (194, 180), (194, 120)], p['body']))  # 리시버
    s.append(rect(206, 108, 434, 9, p['hi']))
    s.append(rect(240, 94, 360, 14, p['part']))                                                   # 레일
    for i in range(12):
        s.append(rect(246 + i * 30, 90, 16, 8, p['part']))
    s.append(poly([(380, 56), (476, 56), (486, 66), (486, 94), (380, 94)], p['body']))            # 도트 사이트
    s.append(rect(474, 64, 10, 22, p['acc'], rx=2))
    s.append(rect(300, 130, 110, 22, p['part'], rx=3))
    s.append(line(430, 176, 630, 176, p['acc'], 3))
    return (880, 340), s


def sniper(p):
    s = []
    s.append(poly([(30, 150), (300, 136), (384, 148), (384, 204), (300, 212), (230, 264), (60, 272), (26, 252)], p['body']))  # 개머리판
    s.append(path('M160 186 L246 180 L222 222 L148 228 Z', p['part']))                           # 엄지 구멍 판
    s.append(rect(26, 148, 14, 124, p['part'], rx=3))                                             # 패드
    s.append(poly([(392, 198), (446, 198), (430, 262), (384, 262)], p['body']))                   # 손잡이
    s.append(path('M450 198 L458 236 Q462 246 472 246 L500 246 L500 234 L476 234 Q470 234 468 228 L462 198 Z', p['part']))
    s.append(rect(500, 190, 60, 46, p['part']))                                                   # 탄창
    s.append(rect(500, 232, 60, 6, p['acc']))
    s.append(poly([(560, 152), (860, 158), (860, 186), (560, 196)], p['part']))                   # 총열 덮개
    s.append(rect(760, 150, 400, 14, p['body']))                                                  # 총열
    s.append(rect(1150, 140, 48, 34, p['body']))                                                  # 소염기
    for i in range(3):
        s.append(rect(1158 + i * 13, 146, 6, 22, p['part']))
    s.append(poly([(384, 130), (720, 130), (732, 142), (732, 198), (384, 198)], p['body']))       # 리시버
    s.append(rect(384, 130, 336, 8, p['hi']))
    s.append(path('M560 196 L572 222 L590 222', 'none', p['body'], 8))                           # 볼트
    s.append(circle(596, 224, 12, p['body']))
    s.append(rect(478, 100, 26, 32, p['part']))                                                   # 마운트
    s.append(rect(632, 100, 26, 32, p['part']))
    s.append(rect(430, 78, 280, 24, p['body'], rx=4))                                             # 스코프
    s.append(poly([(700, 64), (770, 56), (770, 124), (700, 114)], p['body']))
    s.append(poly([(384, 72), (434, 76), (434, 104), (384, 108)], p['body']))
    s.append(rect(540, 66, 26, 12, p['part'], rx=2))
    s.append(rect(764, 62, 8, 56, p['acc'], rx=2))                                                # 렌즈
    s.append(line(450, 90, 690, 90, p['line'], 2))
    s.append(path('M860 186 L900 238 M880 186 L920 238', 'none', p['part'], 6))                 # 양각대
    return (1210, 290), s


def shotgun(p):
    s = []
    s.append(poly([(30, 150), (300, 124), (320, 130), (320, 186), (300, 196), (60, 262), (26, 246)], p['body']))
    s.append(rect(26, 146, 14, 104, p['part'], rx=3))
    s.append(poly([(330, 184), (390, 184), (372, 252), (326, 252)], p['body']))
    s.append(path('M394 184 L402 220 Q406 230 416 230 L444 230 L444 218 L420 218 Q414 218 412 212 L406 184 Z', p['part']))
    s.append(rect(480, 150, 520, 22, p['part']))                                                  # 탄창관
    s.append(rect(480, 122, 560, 22, p['body']))                                                  # 총열
    s.append(rect(1030, 118, 22, 30, p['body']))
    s.append(rect(1034, 112, 10, 8, p['acc']))
    s.append(poly([(640, 144), (830, 144), (840, 154), (840, 180), (830, 190), (640, 190), (630, 180), (630, 154)], p['part']))  # 펌프
    for i in range(7):
        s.append(line(654 + i * 24, 154, 654 + i * 24, 180, p['line'], 4))
    s.append(poly([(320, 114), (480, 114), (492, 126), (492, 188), (320, 188)], p['body']))       # 리시버
    s.append(rect(320, 114, 160, 8, p['hi']))
    s.append(rect(372, 134, 80, 24, p['part'], rx=3))
    s.append(line(330, 178, 480, 178, p['acc'], 3))
    return (1070, 280), s


def railgun(p):
    s = []
    s.append('<defs><filter id="g" x="-20%" y="-200%" width="140%" height="500%"><feGaussianBlur stdDeviation="7"/></filter></defs>')
    s.append(poly([(60, 130), (250, 112), (262, 124), (262, 196), (250, 206), (130, 206), (96, 240), (50, 240), (40, 226), (78, 184), (46, 152)], p['body']))
    s.append(poly([(110, 140), (232, 130), (232, 180), (140, 184)], p['part']))
    s.append(poly([(330, 224), (392, 224), (372, 300), (322, 300)], p['body']))
    s.append(path('M398 224 L406 262 Q410 272 420 272 L448 272 L448 260 L424 260 Q418 260 416 254 L410 224 Z', p['part']))
    s.append(poly([(470, 224), (600, 224), (590, 268), (480, 268)], p['part']))                   # 전지 팩
    for i in range(4):
        s.append(rect(490 + i * 24, 240, 14, 10, p['acc'], rx=2))
    s.append(rect(720, 166, 430, 28, p['acc'], extra='filter="url(#g)" opacity="0.75"'))           # 레일 사이 빛
    s.append(rect(720, 174, 430, 12, p['acc']))
    s.append(poly([(700, 118), (1150, 126), (1176, 140), (1150, 154), (700, 160)], p['body']))    # 위 레일
    s.append(poly([(700, 200), (1150, 206), (1176, 220), (1150, 234), (700, 242)], p['body']))    # 아래 레일
    s.append(rect(700, 152, 440, 6, p['part']))
    s.append(rect(700, 202, 440, 6, p['part']))
    s.append(poly([(262, 100), (690, 100), (722, 132), (722, 228), (690, 246), (262, 246), (250, 232), (250, 114)], p['body']))  # 본체
    s.append(rect(262, 100, 428, 9, p['hi']))
    for i in range(5):                                                                            # 코일
        x = 420 + i * 52
        s.append(rect(x, 92, 30, 162, p['part'], rx=6))
        s.append(rect(x + 8, 98, 14, 150, p['body'], rx=4))
        s.append(rect(x + 12, 104, 6, 138, p['acc'], rx=3))
    s.append(rect(300, 128, 92, 60, p['part'], rx=6))
    s.append(rect(312, 140, 68, 8, p['acc']))
    s.append(rect(312, 156, 46, 6, p['line']))
    s.append(poly([(470, 60), (600, 60), (612, 72), (612, 92), (470, 92)], p['body']))            # 조준기
    s.append(rect(600, 66, 10, 20, p['acc'], rx=2))
    return (1200, 320), s


DEFS = dict(Pistol=pistol, SMG=smg, Sniper=sniper, Shotgun=shotgun, Railgun=railgun)

if __name__ == '__main__':
    svgdir = OUT / 'SVG'
    svgdir.mkdir(exist_ok=True)
    for name, fn in DEFS.items():
        for variant, p in PAL.items():
            (w, h), body = fn(p)
            pad = 12
            svg = (f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="{-pad} {-pad} {w + 2 * pad} {h + 2 * pad}" '
                   f'width="{w + 2 * pad}" height="{h + 2 * pad}">' + ''.join(body) + '</svg>')
            (svgdir / f'Weapon_{name}_{variant}.svg').write_text(svg, encoding='utf-8')
    renderer = OUT / 'render.cjs'
    renderer.write_text(
        "const fs=require('fs'),p=require('path');const {Resvg}=require('" + RESVG + "');"
        "for(const f of fs.readdirSync(process.argv[2]))if(f.endsWith('.svg')){"
        "const r=new Resvg(fs.readFileSync(p.join(process.argv[2],f)),{fitTo:{mode:'zoom',value:2}});"
        "fs.writeFileSync(p.join(process.argv[3],f.replace('.svg','.png')),r.render().asPng());}",
        encoding='utf-8')
    subprocess.run(['node', str(renderer), str(svgdir), str(OUT)], check=True)
    print('ok', len(DEFS) * 2)
