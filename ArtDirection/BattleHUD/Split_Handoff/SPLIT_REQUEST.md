# 전투 HUD v3 분절 요청 (새 세션용)

이전 세션은 이미지를 너무 많이 불러와 **413 Payload Too Large**로 끊겼다. 이 세션은 아래 규칙을 지킨다.

## 이미지 다루는 규칙 (중요)

- **모델이 직접 보는 이미지는 이 폴더의 `Preview_*.jpg` 3장까지만.** 960×540 축소본이며 배치 파악용이다.
- 크롭·분절은 **원본 PNG를 스크립트(PowerShell/Python)로 디스크에서 직접 처리**한다. 원본 PNG를 대화에 첨부하거나 이미지로 열어 보지 않는다.
- 결과 확인도 전체 화면을 다시 불러오지 말고, 조각 몇 개를 한 장에 모은 **작은 확인용 시트(1장, 1280폭 이하 JPEG)**로 한다.
- 상위 폴더의 v1·v2 목업과 `Reference_Current.png`는 이번 작업에 필요 없다. 열지 않는다.

## 원본 (크롭 대상, 1672×941 — 1920×1080 기준 좌표 × 0.8708)

| 파일 (상위 폴더) | 쓰는 부분 | 보기용 |
|---|---|---|
| `../BattleHUD_Mockup_v3_Canonical.png` | **정본.** 거의 모든 조각 | `Preview_Canonical.jpg` |
| `../BattleHUD_Mockup_v3_Oratio_TurnBanner.png` | MAP 버튼의 **선 지도 기호**, 턴 배너(헤어라인 사선·줄표·청록 마름모) | `Preview_TurnBanner_MapIcon.jpg` |
| `../BattleHUD_Mockup_v3_Oratio_EnemyTurn.png` | 적 턴 상태 표현만 (END TURN 흐림, 에너지 0) | `Preview_EnemyTurnState.jpg` |
| `../Canonical_EmptyHUD_Model.png`, `../Canonical_PistolIcon_Model.png` | 이전 세션에서 만든 중간 결과. 쓸 수 있으면 이어서 쓴다 | — |

## 정본에서 고칠 것

1. MAP 버튼: 정본의 **✦ 별 기호는 쓰지 않는다.** TurnBanner 목업의 선 지도 기호로 교체한다.
2. 에너지 작은 "3" 위의 **청록 짧은 선을 뺀다.**
3. 적 HP "29 / 40": 글자는 게임에서 넣으므로 채움과 트랙만 분리한다.
4. 적 턴 상태는 EnemyTurn 목업의 표현을 **정본 구조에 맞춰** 만든다. 자원 모듈은 정본의 먹색 판 형태를 유지한다.

## 분절 규칙

- **글자는 전부 뺀다.** 숫자, 막 이름, 버튼 글자, 무기 이름, "WEAPON / ENERGY / AMMO" 라벨, "PLAYER TURN" 모두 게임에서 넣는다.
- 기호(번개·탄환·권총·지도·카드 묶음·방패)는 **판과 따로** 투명 PNG로 뗀다.
- 판은 알파 255, 바깥만 투명. 반투명 금지.
- 색: 판 #16181B, 안쪽 선 #3A3E44, 글자·기호 흰색, 청록 #0DB8F2는 선·점에만.

## 조각 목록 (1920×1080 기준 크기로 출력)

| 조각 | 크기 | 상태 |
|---|---|---|
| `TopBar` | 1300 × 72 (정본 비율에 맞춰 조정 가능) | — |
| `ItemSlot_Empty` · `ItemSlot_Hover` · `ItemSlot_Quest` | 56 × 56 | Quest는 청록 테두리 + 왼쪽 아래 유형 배지 |
| `Button_Map` · `Button_Deck` | 64 × 64 (판) | Normal / Hover / Pressed |
| `Icon_Map` · `Icon_Deck` | 32 × 32 | — |
| `Res_Panel` | 정본의 오른쪽 자원 모듈 판 | Normal / Empty(에너지 0) |
| `Res_Divider` | 판 폭 × 2 | 가로 구분선 |
| `Res_Slash` | 에너지 사선 | — |
| `Icon_Energy` · `Icon_Ammo` · `Icon_Weapon_Pistol` | 32 × 32 | — |
| `Button_EndTurn` | 280 × 70 | Normal / Hover / Pressed / Disabled |
| `HP_Frame` · `HP_Fill` · `HP_Track` | 360×39 · 340×27 · 340×27 | — |
| `Block_Badge` | 56 × 68 | — |
| `StatusSlot_Underline` | 44 × 2 | — |
| `EnemyHP_Frame` · `EnemyHP_Fill` · `EnemyHP_Track` | 240×26 · 227×18 · 227×18 | — |
| `EnemyBlock_Badge` | 40 × 48 | — |
| `EnemyName_Band` | 9-slice용 | — |
| `TextBand` | 120 × 40 (9-slice) | 틀 없는 글자 뒤 먹색 띠 |
| `Banner_Slash` · `Banner_Line` · `Banner_Diamond` | TurnBanner 목업 기준 | 턴 배너용 |

## 결과물

- 조각 PNG → `../Extracted/`
- `../Extracted/layout.json`: 각 조각의 원본 크롭 좌표, 출력 크기, 1920×1080 화면 배치 좌표
- `../Extracted/README.md`: 조각 설명, 9-slice 경계
- 확인용 시트 1장 (`../Extracted/Contact_Sheet.jpg`, 1280폭 이하)

전체 기준 문서는 `../LAYOUT_BRIEF.md`다. 필요한 절(★ 정본, 1. 색 체계)만 텍스트로 읽는다.
