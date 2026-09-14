// 상태(버프/디버프) 아이콘 생성기.
// 64x64 SVG를 그린 뒤 resvg로 PNG(128px)로 변환한다.
// 흰 글리프 + 어두운 외곽선 → 칩 배경색(버프 초록 / 디버프 빨강)이 뒤에서 비친다.

const fs = require('fs');
const path = require('path');
const { Resvg } = require('@resvg/resvg-js');

const OUT = process.argv[2];
if (!OUT) { console.error('usage: node gen-icons.js <outDir>'); process.exit(1); }
fs.mkdirSync(OUT, { recursive: true });

// 공통 스타일: 굵은 실루엣이라 34px로 줄여도 형태가 남는다.
const S = `
  .f { fill:#ffffff; stroke:#1a1a20; stroke-width:3; stroke-linejoin:round; stroke-linecap:round; }
  .l { fill:none; stroke:#1a1a20; stroke-width:3; stroke-linejoin:round; stroke-linecap:round; }
  .w { fill:none; stroke:#ffffff; stroke-width:5; stroke-linejoin:round; stroke-linecap:round; }
  .d { fill:#1a1a20; }
`;

const svg = (body) =>
  `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" width="64" height="64">` +
  `<style>${S}</style>${body}</svg>`;

// 아래로 갈수록 좁아지는 물방울. 독/피 계열에서 재사용.
const droplet = (cx, cy, s) =>
  `<path class="f" d="M${cx} ${cy - 18 * s} C${cx + 13 * s} ${cy - 2 * s} ${cx + 12 * s} ${cy + 14 * s} ${cx} ${cy + 14 * s} C${cx - 12 * s} ${cy + 14 * s} ${cx - 13 * s} ${cy - 2 * s} ${cx} ${cy - 18 * s} Z"/>`;

const shield = (extra = '') =>
  `<path class="f" d="M32 6 L54 14 V32 C54 46 44 55 32 59 C20 55 10 46 10 32 V14 Z"/>${extra}`;

const icons = {
  // ── 버프 ──────────────────────────────────────────────
  // 죄와 벌 — 탄약 공격 피해 증가. 탄환 + 상승 화살표.
  AmmoAttackBonus: svg(
    `<path class="f" d="M26 52 V26 C26 18 29 11 32 7 C35 11 38 18 38 26 V52 Z"/>` +
    `<path class="l" d="M26 38 H38"/>` +
    `<path class="w" d="M50 30 V14 M43 21 L50 14 L57 21"/>` +
    `<path class="l" d="M50 30 V14 M43 21 L50 14 L57 21" stroke-width="9" stroke-opacity="0.35"/>` +
    `<path class="w" d="M50 30 V14 M43 21 L50 14 L57 21"/>`),

  // 스피드로더 — 턴마다 탄약 획득. 리볼버 실린더.
  AmmoPerTurn: svg(
    `<circle class="f" cx="32" cy="32" r="24"/>` +
    `<circle class="d" cx="32" cy="32" r="6"/>` +
    [0, 60, 120, 180, 240, 300].map(a => {
      const r = (a * Math.PI) / 180;
      return `<circle class="d" cx="${(32 + 14 * Math.cos(r)).toFixed(1)}" cy="${(32 + 14 * Math.sin(r)).toFixed(1)}" r="4.5"/>`;
    }).join('')),

  // 점혈 — 피해 시 약화 부여. 혈자리를 찌르는 지점 + 파동.
  ApplyWeakOnHit: svg(
    `<circle class="f" cx="32" cy="32" r="8"/>` +
    `<path class="w" d="M32 14 A18 18 0 0 1 50 32"/>` +
    `<path class="w" d="M32 50 A18 18 0 0 1 14 32"/>` +
    `<path class="w" d="M32 6 A26 26 0 0 1 58 32" stroke-width="3.5"/>` +
    `<path class="w" d="M32 58 A26 26 0 0 1 6 32" stroke-width="3.5"/>`),

  // 버퍼 — 다음 피해 무효화. 방패 안의 차단 표식.
  DamageNullified: shield_icon(),

  // 스피드/지속 방어 — 턴마다 방어도. 방패 + 시계바늘.
  TimedBlock: svg(
    shield(`<circle class="l" cx="32" cy="31" r="12" fill="none"/>` +
           `<path class="l" d="M32 31 V22 M32 31 L39 35"/>`)),

  // 시베리안 로망스 — 턴마다 힘↑ 민첩↓. 눈 결정.
  SiberianRomance: svg(
    [0, 60, 120].map(a =>
      `<g transform="rotate(${a} 32 32)">` +
      `<path class="w" d="M32 8 V56"/>` +
      `<path class="w" d="M32 16 L25 22 M32 16 L39 22 M32 48 L25 42 M32 48 L39 42" stroke-width="4"/>` +
      `</g>`).join('')),

  // 전술보행 — 매 턴 첫 피해에 보너스. 발바닥 자국(앞꿈치 + 뒤꿈치 분리).
  TacticalFirstAttack: svg(
    `<path class="f" d="M20 26 C20 16 26 8 33 8 C41 8 46 16 45 26 C44 33 39 36 32 36 C25 36 20 33 20 26 Z"/>` +
    `<path class="f" d="M24 44 C24 40 28 38 33 38 C38 38 42 40 42 45 C42 52 38 57 33 57 C27 57 24 51 24 44 Z"/>` +
    `<circle class="f" cx="48" cy="16" r="4.5"/>` +
    `<circle class="f" cx="51" cy="26" r="4"/>`),

  // ── 디버프 ────────────────────────────────────────────
  // 화상 — 턴 시작 관통 피해. 불꽃.
  Burn: svg(
    `<path class="f" d="M32 4 C38 16 48 22 48 34 C48 46 41 56 32 60 C23 56 16 46 16 34 C16 26 21 22 24 18 C26 26 30 28 32 24 C34 20 32 12 32 4 Z"/>` +
    `<path class="l" d="M32 60 C27 55 25 48 27 42 C29 37 33 35 34 31 C37 36 39 42 38 48 C37 54 35 58 32 60 Z" fill="#1a1a20" fill-opacity="0.55"/>`),

  // 독 — 턴 시작 관통 피해. 독액 방울 + 해골.
  Poison: svg(
    droplet(32, 34, 1.25) +
    `<circle class="d" cx="27" cy="34" r="3.2"/>` +
    `<circle class="d" cx="37" cy="34" r="3.2"/>` +
    `<path class="l" d="M28 44 H36" stroke-width="3.5"/>` +
    `<path class="l" d="M30 41 V47 M34 41 V47" stroke-width="2.5"/>`),

  // 응시 — 심판이 소비하는 지속 디버프. 눈.
  Pressured: svg(
    `<path class="f" d="M4 32 C14 18 24 12 32 12 C40 12 50 18 60 32 C50 46 40 52 32 52 C24 52 14 46 4 32 Z"/>` +
    `<circle class="d" cx="32" cy="32" r="10"/>` +
    `<circle class="f" cx="35" cy="28" r="3" stroke="none"/>`),

  // 취약 — 받는 피해 +50%. 금이 간 방패.
  Vulnerable: svg(
    shield(`<path class="l" d="M32 10 L26 28 L36 34 L28 54" stroke-width="4.5"/>`)),

  // 약화 — 가하는 피해 −25%. 힘없이 아래로 휘어진 검.
  // 부러진 검은 가드/자루가 십자로 뭉쳐 보여 실패했다. 축 늘어진 칼날이 "약해짐"을 더 직관적으로 전달한다.
  Weak: svg(
    // 자루 (왼쪽 위에서 시작)
    `<path class="f" d="M10 10 L20 6 L24 16 L14 20 Z"/>` +
    // 가드
    `<path class="f" d="M17 17 L28 13 L31 21 L20 25 Z"/>` +
    // 아래로 축 처진 칼날
    `<path class="f" d="M24 20 C36 26 46 34 50 50 L42 54 C38 40 32 32 20 27 Z"/>` +
    // 처지는 방향 표시
    `<path class="w" d="M52 40 V54 H38" stroke-width="3.5" stroke-dasharray="4 4"/>`),

  // 혈액치환 — 이번 턴 카드 비용을 HP로 지불. 핏방울을 감싸는 순환 화살표.
  BloodCost: svg(
    // 방울을 감싸는 고리를 먼저 깔고 그 위에 방울을 올린다(겹침 순서로 "감싼다"가 읽힌다).
    `<circle class="l" cx="32" cy="32" r="25" fill="none" stroke-width="6" stroke-dasharray="98 60"/>` +
    `<circle class="w" cx="32" cy="32" r="25" stroke-width="4" stroke-dasharray="98 60"/>` +
    // 고리 끝의 화살촉
    `<path class="l" d="M46 48 L49 57 L40 56" stroke-width="6"/>` +
    `<path class="w" d="M46 48 L49 57 L40 56" stroke-width="4"/>` +
    droplet(32, 30, 1.0)),

  // 타격 취약 — 타격 시 적에게 취약 부여. 조준점 + 균열.
  VulnerableOnHitMarker: svg(
    `<circle class="f" cx="32" cy="32" r="20"/>` +
    `<circle class="d" cx="32" cy="32" r="5"/>` +
    `<path class="l" d="M32 4 V16 M32 48 V60 M4 32 H16 M48 32 H60" stroke-width="4"/>` +
    `<path class="l" d="M24 24 L30 31 L26 36" stroke-width="3"/>`),

  // ── 중립 / 가변 ───────────────────────────────────────
  // 힘 — 가하는 피해가 스택만큼 변함. 덤벨.
  // 주먹/팔은 절차적으로 그리면 "펼친 손"이나 "줄무늬 상자"로 읽혀 실패했다.
  // 덤벨은 순수 기하 형태라 34px로 줄여도 형태가 무너지지 않는다.
  Strength: svg(
    // 바
    `<path class="f" d="M20 28 H44 V36 H20 Z"/>` +
    // 안쪽 원판
    `<path class="f" d="M14 20 H23 V44 H14 Z"/>` +
    `<path class="f" d="M41 20 H50 V44 H41 Z"/>` +
    // 바깥 원판 (작게)
    `<path class="f" d="M6 26 H14 V38 H6 Z"/>` +
    `<path class="f" d="M50 26 H58 V38 H50 Z"/>`),

  // 민첩 — 얻는 방어도가 스택만큼 변함. 깃털.
  Dexterity: svg(
    `<path class="f" d="M50 8 C30 12 16 26 14 44 L10 56 L22 52 C40 50 54 36 56 16 Z"/>` +
    `<path class="l" d="M50 14 L18 48" stroke-width="3.5"/>` +
    `<path class="l" d="M44 16 L30 22 M48 24 L34 30 M42 32 L28 38" stroke-width="2.5"/>`),

  // 착혈 — HP 피해 시 공격자를 표식. 송곳니 한 쌍 + 떨어지는 핏방울.
  BloodsuckingMarker: svg(
    // 잇몸 (위턱)
    `<path class="f" d="M10 10 H54 V20 C54 24 50 26 44 26 H20 C14 26 10 24 10 20 Z"/>` +
    // 송곳니 두 개 — 길고 뾰족하게, 간격을 벌려 확실히 분리
    `<path class="f" d="M20 24 L27 24 L24 44 Z"/>` +
    `<path class="f" d="M37 24 L44 24 L41 44 Z"/>` +
    // 핏방울
    droplet(32, 52, 0.55)),

  // 체력감소 공격 — 체력이 줄 때마다 공격력 증가. 하트 + 하강 화살표.
  LostBloodAttack: svg(
    `<path class="f" d="M32 56 C14 44 6 34 6 24 C6 15 13 9 20 9 C26 9 30 13 32 17 C34 13 38 9 44 9 C51 9 58 15 58 24 C58 34 50 44 32 56 Z"/>` +
    `<path class="l" d="M32 24 V42 M24 34 L32 42 L40 34" stroke-width="4.5"/>`),
};

// 버퍼(피해 무효화) — 방패 + 차단 사선. shield()를 쓰려고 함수로 뺐다.
function shield_icon() {
  return svg(shield(`<path class="l" d="M22 22 L42 42" stroke-width="5"/>` +
                    `<circle class="l" cx="32" cy="32" r="13" fill="none" stroke-width="4"/>`));
}

let n = 0;
for (const [name, source] of Object.entries(icons)) {
  fs.writeFileSync(path.join(OUT, `${name}.svg`), source);

  // 128px로 렌더해 두면 Unity에서 축소되며 안티에일리어싱이 깔끔하게 먹는다.
  const png = new Resvg(source, { fitTo: { mode: 'width', value: 128 } }).render().asPng();
  fs.writeFileSync(path.join(OUT, `${name}.png`), png);
  n++;
}
console.log(`generated ${n} icons -> ${OUT}`);
