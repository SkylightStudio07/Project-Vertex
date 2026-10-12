// 노상강도 못 박힌 몽둥이 풀스윙 궤적(스미어) 이펙트 시트 (6프레임) → Assets/Art/VFX/BatSwing_Sheet.png
// 사용: node gen-batswing.js [출력 폴더]
//
// 좌표는 전부 원본 공격 프레임(노상강도_attack, 832×1216) 픽셀 기준(왼쪽 위 원점)으로 잡고,
// 시트 한 칸은 그 프레임을 왼쪽·위로 넓힌 캔버스다(프레임 x -420~560, y -120~900).
// → 스프라이트 피벗을 프레임 픽셀 (0,560)에 두면 BatSwingOverlay의 anchorPixel(0,560)과 정확히 겹친다.
//
// 몽둥이 끝 궤적 (프레임 14~18에서 측정): 어깨 C=(330,340) 기준 반지름 약 430,
//   15: 머리 뒤 오른쪽(-26°) → 16: 머리 위 왼쪽(-135°) → 17: 왼쪽 수평(-217°) → 18: 왼쪽 아래(-233°)
// 공격 시트와 같은 14fps로 16번 프레임부터 한 칸씩 맞춰 재생한다.
//
// 스타일: 캐릭터의 흰 스티커 테두리와 같은 결 — 흰 코어 + 먹선(#16181b) 바깥 윤곽,
//         꼬리로 갈수록 가늘고 투명해지는 초승달 스미어, 끝에 충격 섬광·쇳조각 파편. 위험색(붉은색)은 충격점에만 조금.

const fs = require('fs');
const path = require('path');
const { Resvg } = require('@resvg/resvg-js');

const OUT = process.argv[2] || path.join(__dirname, '../../Assets/Art/VFX');
const SVG_OUT = path.join(__dirname, 'batswing');
fs.mkdirSync(OUT, { recursive: true });
fs.mkdirSync(SVG_OUT, { recursive: true });

const OX = 420, OY = 120;          // 프레임 좌표 → 캔버스 좌표 이동량
const FW = 980, FH = 1020;         // 칸 크기
const C = [330, 340];              // 어깨 (회전 중심)
const INK = '#16181b', WHITE = '#ffffff', SHADE = '#d5dde4', RED = '#e0463a';

const P = (r, deg) => {
  const a = (deg * Math.PI) / 180;
  return [C[0] + Math.cos(a) * r + OX, C[1] + Math.sin(a) * r + OY];
};
const f = (n) => n.toFixed(1);
const pts = (arr) => arr.map(([x, y]) => `${f(x)},${f(y)}`).join(' ');

// 초승달 스미어: 꼬리(tail)→머리(head) 방향으로 조각을 나눠 폭·불투명도를 키운다.
// 안쪽 반지름이 머리 쪽에서 크게 벌어지고 꼬리 쪽은 바깥 가장자리 한 줄만 남는다.
let _uid = 0;
function smear(head, tail, { alpha = 1, rOut = 458, rInHead = 235, rInTail = 425, steps = 48 } = {}) {
  let s = '';
  const span = head - tail;
  const ease = (t) => t * t * (3 - 2 * t);
  // 한 덩어리 도형으로 그리고(조각 이음새 줄무늬 방지), 꼬리→머리 선형 그라데이션 마스크로 흐려지게 한다
  const outer = [], inner = [], mid = [];
  for (let i = 0; i <= steps; i++) {
    const t = i / steps, a = tail + span * t;
    const rin = rInTail + (rInHead - rInTail) * ease(t);
    outer.push(P(rOut, a));
    inner.push(P(rin, a));
    mid.push(P((rOut + rin) / 2 + (rOut - rin) * 0.08, a)); // 흰 면이 바깥쪽 절반보다 조금 좁게
  }
  const id = `m${_uid++}`;
  const [tx, ty] = P((rOut + rInTail) / 2, tail), [hx, hy] = P((rOut + rInHead) / 2, head);
  s += `<defs><linearGradient id="g${id}" gradientUnits="userSpaceOnUse" x1="${f(tx)}" y1="${f(ty)}" x2="${f(hx)}" y2="${f(hy)}">` +
       `<stop offset="0" stop-color="#fff" stop-opacity="0"/><stop offset="0.45" stop-color="#fff" stop-opacity="0.25"/>` +
       `<stop offset="0.8" stop-color="#fff" stop-opacity="0.8"/><stop offset="1" stop-color="#fff" stop-opacity="1"/></linearGradient>` +
       `<mask id="${id}" maskUnits="userSpaceOnUse" x="0" y="0" width="${FW}" height="${FH}"><rect x="0" y="0" width="${FW}" height="${FH}" fill="url(#g${id})"/></mask></defs>`;
  s += `<g mask="url(#${id})" opacity="${alpha}">`;
  s += `<polygon points="${pts([...outer, ...inner.reverse()])}" fill="${SHADE}"/>`;
  s += `<polygon points="${pts([...outer, ...mid.reverse()])}" fill="${WHITE}"/>`;
  s += `</g>`;
  // 바깥 먹선: 머리 쪽 60%에만, 머리로 갈수록 굵게
  const n = 16;
  for (let i = 0; i < n; i++) {
    const t0 = 0.4 + 0.6 * (i / n), t1 = 0.4 + 0.6 * ((i + 1) / n);
    const [x0, y0] = P(rOut + 3, tail + span * t0), [x1, y1] = P(rOut + 3, tail + span * t1);
    s += `<line x1="${f(x0)}" y1="${f(y0)}" x2="${f(x1)}" y2="${f(y1)}" stroke="${INK}" stroke-width="${(1.5 + 4 * t1).toFixed(2)}" stroke-linecap="round" opacity="${alpha}"/>`;
  }
  // 머리 가장자리 (몽둥이가 지나가는 선) — 흰 선 + 먹선
  const [hx0, hy0] = P(rInHead, head), [hx1, hy1] = P(rOut + 4, head);
  s += `<line x1="${f(hx0)}" y1="${f(hy0)}" x2="${f(hx1)}" y2="${f(hy1)}" stroke="${INK}" stroke-width="9" stroke-linecap="round" opacity="${alpha}"/>`;
  s += `<line x1="${f(hx0)}" y1="${f(hy0)}" x2="${f(hx1)}" y2="${f(hy1)}" stroke="${WHITE}" stroke-width="4" stroke-linecap="round" opacity="${alpha}"/>`;
  return s;
}

// 바깥쪽 속도선 (짧은 호)
function speedLines(head, tail, alpha, radii = [482, 506, 530]) {
  let s = '';
  radii.forEach((r, k) => {
    const a0 = head - (head - tail) * (0.15 + 0.12 * k), a1 = a0 - (head - tail) * (0.35 - 0.08 * k) * -1;
    const seg = [];
    for (let i = 0; i <= 10; i++) seg.push(P(r, a0 + (a1 - a0) * (i / 10)));
    s += `<polyline points="${pts(seg)}" fill="none" stroke="${INK}" stroke-width="${3 - k * 0.7}" stroke-linecap="round" opacity="${(alpha * (0.8 - k * 0.2)).toFixed(2)}"/>`;
  });
  return s;
}

// 충격 섬광: 가시가 불규칙한 별 (흰 코어 + 먹선) + 붉은 속
function burst(cx, cy, r, alpha, rot = 0) {
  const spikes = [1, 0.55, 0.85, 0.5, 1.15, 0.6, 0.8, 0.45, 1.0, 0.55];
  const outer = [], inner = [];
  spikes.forEach((k, i) => {
    const a = rot + (i / spikes.length) * Math.PI * 2;
    const rr = r * (i % 2 === 0 ? k : k * 0.45);
    outer.push([cx + Math.cos(a) * rr, cy + Math.sin(a) * rr]);
    inner.push([cx + Math.cos(a) * rr * 0.55, cy + Math.sin(a) * rr * 0.55]);
  });
  return `<polygon points="${pts(outer)}" fill="${WHITE}" stroke="${INK}" stroke-width="5" stroke-linejoin="round" opacity="${alpha}"/>` +
         `<polygon points="${pts(inner)}" fill="${RED}" opacity="${(alpha * 0.85).toFixed(2)}"/>` +
         `<circle cx="${f(cx)}" cy="${f(cy)}" r="${f(r * 0.16)}" fill="${WHITE}" opacity="${alpha}"/>`;
}

// 쇳조각 파편 (흰 마름모 + 먹선), 충격점에서 왼쪽으로 튄다
const SHARDS = [
  { a: 200, d: 1.0, s: 16 }, { a: 170, d: 0.8, s: 11 }, { a: 228, d: 0.9, s: 13 },
  { a: 150, d: 0.6, s: 9 }, { a: 245, d: 0.7, s: 10 }, { a: 190, d: 1.25, s: 8 }, { a: 215, d: 1.4, s: 7 },
];
function shards(cx, cy, dist, alpha) {
  return SHARDS.map(({ a, d, s }) => {
    const r = (a * Math.PI) / 180, x = cx + Math.cos(r) * dist * d, y = cy + Math.sin(r) * dist * d;
    const ux = Math.cos(r), uy = Math.sin(r);
    const p = [[x + ux * s * 1.8, y + uy * s * 1.8], [x - uy * s * 0.5, y + ux * s * 0.5], [x - ux * s, y - uy * s], [x + uy * s * 0.5, y - ux * s * 0.5]];
    return `<polygon points="${pts(p)}" fill="${WHITE}" stroke="${INK}" stroke-width="2.5" stroke-linejoin="round" opacity="${alpha}"/>`;
  }).join('');
}

// 흙먼지 (옅은 회색 원, 먹선 없음)
function dust(cx, cy, spread, alpha) {
  const puffs = [[0, 0, 1], [-0.8, 0.3, 0.8], [-0.4, -0.5, 0.7], [-1.3, 0.1, 0.6], [0.4, 0.5, 0.55]];
  return puffs.map(([dx, dy, k]) => `<circle cx="${f(cx + dx * spread)}" cy="${f(cy + dy * spread)}" r="${f(spread * 0.45 * k)}" fill="${SHADE}" opacity="${(alpha * 0.55).toFixed(2)}"/>`).join('');
}

const IMPACT = P(470, -226); // 몽둥이 끝이 왼쪽 아래로 내려오며 부딪히는 지점

const frames = [
  // 0 (공격 16): 머리 위를 넘어오는 궤적
  () => smear(-140, -40, { alpha: 0.95, rInHead: 260 }) + speedLines(-140, -40, 0.7),
  // 1 (17): 왼쪽 수평까지 크게 펼쳐짐
  () => smear(-217, -95, { alpha: 1 }) + speedLines(-217, -95, 0.9),
  // 2 (18): 왼쪽 아래 끝 + 충격 섬광·파편
  () => smear(-234, -150, { alpha: 1, rInHead: 250 }) + speedLines(-234, -150, 0.7) +
        burst(IMPACT[0], IMPACT[1], 95, 1, 0.3) + shards(IMPACT[0], IMPACT[1], 70, 1),
  // 3: 궤적이 가늘어지며 사라짐, 파편 비산, 먼지
  () => smear(-238, -200, { alpha: 0.6, rInHead: 380, rInTail: 440 }) +
        burst(IMPACT[0], IMPACT[1], 60, 0.55, 0.9) + shards(IMPACT[0], IMPACT[1], 140, 0.85) + dust(IMPACT[0] + 10, IMPACT[1] + 20, 60, 0.8),
  // 4: 파편 멀리, 먼지 퍼짐
  () => smear(-240, -222, { alpha: 0.25, rInHead: 430, rInTail: 448 }) +
        shards(IMPACT[0], IMPACT[1], 210, 0.45) + dust(IMPACT[0] - 10, IMPACT[1] + 10, 85, 0.55),
  // 5: 잔연
  () => dust(IMPACT[0] - 25, IMPACT[1], 105, 0.25),
];

const pngs = frames.map((draw, i) => {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="${FW}" height="${FH}" viewBox="0 0 ${FW} ${FH}">${draw()}</svg>`;
  fs.writeFileSync(path.join(SVG_OUT, `BatSwing_${i}.svg`), svg);
  return new Resvg(svg, { fitTo: { mode: 'original' } }).render();
});

// 가로 한 줄 시트 — 프레임 크기 그대로 (Unity에서 Grid By Cell Size 980×1020, 피벗 (420,680 위에서) = 정규화 (0.4286, 0.3333))
const sheetSvg = `<svg xmlns="http://www.w3.org/2000/svg" width="${FW * frames.length}" height="${FH}">` +
  frames.map((draw, i) => `<g transform="translate(${FW * i},0)">${draw()}</g>`).join('') + `</svg>`;
fs.writeFileSync(path.join(OUT, 'BatSwing_Sheet.png'), new Resvg(sheetSvg, { fitTo: { mode: 'original' } }).render().asPng());
console.log(`BatSwing_Sheet.png ${FW * frames.length}×${FH}, ${frames.length} frames, pivot px (${OX}, ${560 + OY}) from top-left`);
