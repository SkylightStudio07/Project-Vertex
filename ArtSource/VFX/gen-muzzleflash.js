// 플레이어 권총 사격 시 총구에 덮어씌우는 섬광 스프라이트 시트 (6프레임, 가로 1줄).
// 원본 공격 시트(player_f_attack_handgun)의 섬광은 프레임 오른쪽 경계에서 잘려 있어서,
// 총구를 원점으로 오른쪽으로 뻗는 섬광을 따로 그려 그 위에 재생한다(MuzzleFlashOverlay).
//
// 아트 디렉션: 흰 코어 + 얇은 먹선 + 시안은 테두리 포인트로만. 그라데이션 대신 겹친 단색 레이어.
// 프레임 흐름: 점화 → 최대 섬광 → 갈라짐 → 파편 이탈 + 연기 → 연기 확산 → 잔연.
//
// 사용: NODE_PATH=<@resvg/resvg-js 설치 경로>/node_modules node gen-muzzleflash.js <출력 폴더>

const fs = require('fs');
const path = require('path');
const { Resvg } = require('@resvg/resvg-js');

const OUT = process.argv[2];
fs.mkdirSync(OUT, { recursive: true });

const FW = 512, FH = 256;       // 프레임 크기
const MX = 40, MY = 128;        // 총구 원점 (프레임 좌표)
const WHITE = '#ffffff';
const PAPER = '#f4f4f0';
const INK = '#16181b';
const CYAN = '#7fd6f0';
const SMOKE = '#c9ccd2';

// 총구에서 오른쪽으로 뻗는 원뿔. len: 길이, half: 총구 쪽 반폭, jag: 가장자리 톱니 깊이
function cone(len, half, jag, opacity = 1) {
  const pts = [[MX, MY - half]];
  const steps = 5;
  for (let i = 1; i <= steps; i++) {
    const x = MX + (len * i) / steps;
    const w = half * (1 - i / steps) + 2;
    pts.push([x - (i % 2 ? jag : 0), MY - w - (i % 2 ? jag * 0.4 : 0)]);
  }
  pts.push([MX + len + 18, MY]);
  for (let i = steps; i >= 1; i--) {
    const x = MX + (len * i) / steps;
    const w = half * (1 - i / steps) + 2;
    pts.push([x - (i % 2 ? 0 : jag), MY + w + (i % 2 ? 0 : jag * 0.4)]);
  }
  pts.push([MX, MY + half]);
  const d = 'M ' + pts.map(p => p.map(v => v.toFixed(1)).join(' ')).join(' L ') + ' Z';
  return `<path d="${d}" fill="${WHITE}" fill-opacity="${opacity}" stroke="${INK}" stroke-width="3" stroke-linejoin="round"/>`;
}

// 총구 중심의 4갈래 별 섬광
function star(r, thin, rot = 0, opacity = 1) {
  const pts = [];
  for (let i = 0; i < 8; i++) {
    const a = (Math.PI / 4) * i + rot;
    const rr = i % 2 === 0 ? r : thin;
    pts.push([MX + 18 + Math.cos(a) * rr, MY + Math.sin(a) * rr]);
  }
  const d = 'M ' + pts.map(p => p.map(v => v.toFixed(1)).join(' ')).join(' L ') + ' Z';
  return `<path d="${d}" fill="${WHITE}" fill-opacity="${opacity}" stroke="${INK}" stroke-width="2.5" stroke-linejoin="round"/>`;
}

// 떨어져 나가는 삼각 파편
function shard(x, y, s, rot, opacity = 1) {
  const pts = [[0, -s], [s * 0.9, s * 0.7], [-s * 0.6, s * 0.5]].map(([px, py]) => {
    const c = Math.cos(rot), sn = Math.sin(rot);
    return [x + px * c - py * sn, y + px * sn + py * c];
  });
  const d = 'M ' + pts.map(p => p.map(v => v.toFixed(1)).join(' ')).join(' L ') + ' Z';
  return `<path d="${d}" fill="${WHITE}" fill-opacity="${opacity}" stroke="${INK}" stroke-width="2" stroke-linejoin="round"/>`;
}

const streak = (x0, y0, x1, y1, w, color = CYAN, opacity = 1) =>
  `<line x1="${x0}" y1="${y0}" x2="${x1}" y2="${y1}" stroke="${color}" stroke-opacity="${opacity}" stroke-width="${w}" stroke-linecap="round"/>`;

const puff = (x, y, r, opacity) =>
  `<circle cx="${x}" cy="${y}" r="${r}" fill="${SMOKE}" fill-opacity="${opacity}" stroke="${INK}" stroke-opacity="${opacity * 0.5}" stroke-width="1.5"/>`;

const bloom = (r, opacity) =>
  `<circle cx="${MX + 22}" cy="${MY}" r="${r}" fill="#dff3fa" fill-opacity="${opacity}"/>` +
  `<circle cx="${MX + 22}" cy="${MY}" r="${r}" fill="none" stroke="${CYAN}" stroke-opacity="${opacity}" stroke-width="3"/>`;

const frames = [
  // 0 점화: 별 + 짧은 원뿔
  [bloom(36, 0.8), cone(150, 24, 5), star(46, 13)],
  // 1 최대 섬광: 굵고 긴 원뿔 + 대각 분출 + 시안 궤적
  [bloom(66, 0.9),
   streak(MX + 30, MY, MX + 450, MY, 5), streak(MX + 60, MY - 16, MX + 360, MY - 34, 3),
   streak(MX + 60, MY + 16, MX + 360, MY + 34, 3),
   `<path d="M ${MX + 18} ${MY - 12} L ${MX + 150} ${MY - 98} L ${MX + 60} ${MY - 6} Z" fill="${WHITE}" stroke="${INK}" stroke-width="3" stroke-linejoin="round"/>`,
   `<path d="M ${MX + 18} ${MY + 12} L ${MX + 150} ${MY + 98} L ${MX + 60} ${MY + 6} Z" fill="${WHITE}" stroke="${INK}" stroke-width="3" stroke-linejoin="round"/>`,
   cone(340, 44, 12), star(66, 17, Math.PI / 8)],
  // 2 갈라짐: 짧고 톱니진 원뿔, 앞쪽 파편 분리
  [bloom(48, 0.6), streak(MX + 60, MY, MX + 400, MY, 3, CYAN, 0.8),
   cone(250, 34, 18, 0.95), star(44, 13, Math.PI / 4, 0.9),
   shard(MX + 320, MY - 22, 18, 0.4), shard(MX + 360, MY + 22, 14, 1.8), shard(MX + 395, MY - 2, 11, 3.0)],
  // 3 파편 이탈 + 연기 시작
  [puff(MX + 60, MY - 8, 26, 0.6), puff(MX + 104, MY + 10, 20, 0.55),
   cone(110, 18, 10, 0.85),
   shard(MX + 380, MY - 40, 14, 0.9, 0.9), shard(MX + 420, MY + 34, 12, 2.2, 0.85), shard(MX + 450, MY - 8, 9, 3.4, 0.8),
   streak(MX + 396, MY - 48, MX + 420, MY - 58, 2, CYAN, 0.7)],
  // 4 연기 확산 (위로 흩어짐)
  [puff(MX + 70, MY - 24, 32, 0.5), puff(MX + 124, MY - 10, 26, 0.45), puff(MX + 164, MY - 38, 20, 0.4),
   shard(MX + 455, MY - 56, 9, 1.2, 0.6), shard(MX + 470, MY + 42, 8, 2.6, 0.5)],
  // 5 잔연
  [puff(MX + 86, MY - 40, 38, 0.25), puff(MX + 146, MY - 28, 30, 0.2), puff(MX + 190, MY - 52, 24, 0.15)],
];

const pngs = frames.map((layers, i) => {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${FW} ${FH}" width="${FW}" height="${FH}">${layers.join('')}</svg>`;
  fs.writeFileSync(path.join(OUT, `MuzzleFlash_${i}.svg`), svg);
  return svg;
});

// 가로 1줄 시트로 합친다 (Unity에서 Grid By Cell Size 512x256으로 슬라이스)
const sheet = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${FW * frames.length} ${FH}" width="${FW * frames.length}" height="${FH}">` +
  pngs.map((svg, i) => `<g transform="translate(${FW * i} 0)">${svg.replace(/^<svg[^>]*>/, '').replace(/<\/svg>$/, '')}</g>`).join('') +
  `</svg>`;
fs.writeFileSync(path.join(OUT, 'MuzzleFlash_Sheet.svg'), sheet);
const png = new Resvg(sheet, { fitTo: { mode: 'original' } }).render().asPng();
fs.writeFileSync(path.join(OUT, 'MuzzleFlash_Sheet.png'), png);
console.log(`generated MuzzleFlash_Sheet (${frames.length} frames, ${FW}x${FH}) -> ${OUT}`);
