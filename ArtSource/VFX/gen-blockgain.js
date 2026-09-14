// 방어도 획득 시 캐릭터 위에 팝업되는 방패 실루엣.
// 기존 상태 아이콘(64px, 흰색+외곽선)과 달리 이건 그 자체로 이펙트라
// 파란 글로우가 배어나오는 느낌을 내려고 2겹(바깥 흐린 글로우 + 안쪽 선명한 방패)으로 그린다.
// 배경 없음(투명) — 스폰될 때 이미 파란 톤이라 별도 틴트 없이 흰 배경 위에서도 확인 가능해야 함.

const fs = require('fs');
const path = require('path');
const { Resvg } = require('@resvg/resvg-js');

const OUT = process.argv[2];
fs.mkdirSync(OUT, { recursive: true });

const BLUE = '#5aa7ff';
const BLUE_DARK = '#2f6fd6';

// 방패 실루엣 path (256 기준 좌표, 중심 128,128)
const shieldPath = (scale, opacity, stroke) => {
  const s = scale;
  const cx = 128, cy = 128;
  // 대략적인 방패 윤곽: 위는 넓고 아래로 갈수록 뾰족
  const pts = `
    M ${cx} ${cy - 78*s}
    L ${cx + 62*s} ${cy - 56*s}
    L ${cx + 62*s} ${cy - 4*s}
    C ${cx + 62*s} ${cy + 48*s} ${cx + 30*s} ${cy + 84*s} ${cx} ${cy + 96*s}
    C ${cx - 30*s} ${cy + 84*s} ${cx - 62*s} ${cy + 48*s} ${cx - 62*s} ${cy - 4*s}
    L ${cx - 62*s} ${cy - 56*s}
    Z`;
  return `<path d="${pts}" fill="${BLUE}" fill-opacity="${opacity}" stroke="${stroke ? BLUE_DARK : 'none'}" stroke-width="${stroke ? 5*s : 0}"/>`;
};

const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 256 256" width="256" height="256">
  <defs>
    <radialGradient id="glow" cx="50%" cy="50%" r="50%">
      <stop offset="0%" stop-color="${BLUE}" stop-opacity="0.55"/>
      <stop offset="100%" stop-color="${BLUE}" stop-opacity="0"/>
    </radialGradient>
  </defs>
  <circle cx="128" cy="128" r="128" fill="url(#glow)"/>
  ${shieldPath(1.35, 0.25, false)}
  ${shieldPath(1.0, 0.95, true)}
  <!-- 광택 하이라이트: 방패 왼쪽 위를 가로지르는 흰 사선 -->
  <path d="M 92 88 L 108 74" fill="none" stroke="#ffffff" stroke-opacity="0.85" stroke-width="8" stroke-linecap="round"/>
  <path d="M 92 108 L 128 74" fill="none" stroke="#ffffff" stroke-opacity="0.6" stroke-width="6" stroke-linecap="round"/>
</svg>`;

fs.writeFileSync(path.join(OUT, 'BlockGainShield.svg'), svg);
const png = new Resvg(svg, { fitTo: { mode: 'width', value: 256 } }).render().asPng();
fs.writeFileSync(path.join(OUT, 'BlockGainShield.png'), png);
console.log('generated BlockGainShield ->', OUT);
