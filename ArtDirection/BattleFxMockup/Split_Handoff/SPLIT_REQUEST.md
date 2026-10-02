# 전투 연출 · 결과 화면 분절 요청 (2배 해상도)

정본은 모두 1920×1080이다.

| 연출 | 정본 | 미리보기 (960×540) |
|---|---|---|
| 작전 개시 + 처치 기록 | `../BattleFx_Start_Mockup.png` | `Preview_Start.jpg` |
| 카드 컷인 (파워·유니크) | `../BattleFx_CutIn_Mockup.png` | `Preview_CutIn.jpg` |
| 작전 종료 | `../BattleFx_Victory_Mockup.png` | `Preview_Victory.jpg` |
| 작전 실패 | `../BattleFx_Defeat_Mockup.png` | `Preview_Defeat.jpg` |
| 결과 화면 (클리어 / 실패) | `../Result_Clear_Mockup.png`, `../Result_Defeat_Mockup.png` | `Preview_Result_Clear.jpg`, `Preview_Result_Defeat.jpg` |

## 이미지 규칙 (413 방지)

- 모델이 직접 보는 이미지는 미리보기 JPG뿐이다(한 번에 2장까지). 원본 PNG는 스크립트로 디스크에서만 읽는다.
- 결과 확인은 연출별 확인용 시트(1200폭 이하)로 한다.

## ★ 이번 분절의 핵심: 2배 해상도

지금까지 조각은 목업에서 1:1로 잘라서 1080p에서만 선명했고, 그보다 큰 화면에서는 흐려졌다. 이번에는 **모든 조각을 2배 크기로 다시 그려서 낸다.**

- **출력 크기:** 원본 영역의 2배. 예: 화면에서 600×80인 띠는 1200×160 PNG.
- **방식:** 띠·선·태그·판·칸 같은 도형은 목업을 참고해 **2배 캔버스에 새로 그린다**(벡터처럼 깨끗한 모서리·사선·헤어라인). 목업을 단순히 확대한 결과물은 받지 않는다.
  - 종이 질감은 2배에서도 자연스럽게 다시 입힌다.
  - 헤어라인은 2배에서 2px(화면 1px)이 되게 그린다.
- **좌표:** `layout.json`의 배치 좌표·글자 자리·창 좌표는 **1920×1080 화면 기준(1배)** 으로 적는다. 각 조각에 `"scale": 2`와 실제 PNG 크기를 함께 적는다.
- **9-slice:** 2배 PNG 기준 픽셀로 적는다(Unity에서 PPU 200으로 임포트해 화면 크기를 맞춘다).
- 카드 아트 창 같은 투명 창 좌표도 2배 PNG 기준과 1배 화면 기준을 둘 다 적는다.

## 연출별로 따로 낸다

결과는 연출마다 폴더를 나눈다. 폴더마다 조각 PNG, `layout.json`, `Contact_Sheet.jpg`을 넣고, README는 `../Extracted/README.md` 하나에 연출별 절로 나눠 쓴다.

```
../Extracted/Start/      작전 개시 배너
../Extracted/KillFeed/   적 처치 기록
../Extracted/CutIn/      카드 컷인
../Extracted/Victory/    작전 종료
../Extracted/Defeat/     작전 실패
../Extracted/Result/     결과 화면
```

## 공통 분절 규칙

- **글자·숫자는 전부 뺀다.**
  - 영문 라벨: OPERATION START, POWER ACTIVATED, ATTACK, UNIQUE, POWER, TARGET DOWN, BOSS, MISSION ACCOMPLISHED / FAILED, RESULT / OPERATION REPORT
  - 한글 제목, 카드 이름, 적 이름, 태그 글자("정예 조우 · 적 01", "무력화")
  - 기록 칸 항목명과 숫자, 버튼 글자, 번호 01
- **전투 화면 배경·HUD·캐릭터·카드 아트는 넣지 않는다.** 화면을 어둡게 덮는 것(실패·결과 화면의 어두운 막)도 뽑지 않는다. 게임에서 따로 깐다.
- 판과 칸의 알파는 0 또는 255. 단 **흰 섬광**처럼 원래 부드러운 빛만 알파 그라데이션을 허용한다.
- 움직이는 부분은 따로 뽑는다. 띠 본체 / 위·아래 헤어라인 / 사선 끝 조각 / 태그 판 / 짧은 강조선 / 장식 눈금은 각각 따로 움직일 수 있어야 한다.
- 상태·변형별 조각은 같은 크기로 맞춘다.

## 1. Start — 작전 개시 배너

| 조각 | 설명 |
|---|---|
| `Band` | 먹색 사선 띠 본체 (가로로 늘어나게 9-slice, 사선 끝 보존) |
| `Line_Top`, `Line_Bottom` | 띠 위·아래 청록·흰 헤어라인 (가로 9-slice) |
| `Slash_Accent` | 띠 오른쪽 끝 청록 이중 사선 |
| `Tag` | 띠 아래 태그 판 (글자 없음) |
| `Tag_EliteMark` | 태그 왼쪽 청록 "ELITE" 칸 (글자 없음) |
| `Tick` | 태그 끝 작은 청록 마름모·점 |

## 2. KillFeed — 적 처치 기록

| 조각 | 설명 |
|---|---|
| `Row` | 먹색 기록 줄 한 칸 (왼쪽 사선 포함, 가로 9-slice) |
| `Row_Accent` | 줄 왼쪽 청록 짧은 띠 |
| `BossChip` | 보스 줄 끝 작은 칸 (글자 없음) |

줄 간격(반복 간격)을 적는다.

## 3. CutIn — 카드 컷인

| 조각 | 설명 | 변형 |
|---|---|---|
| `Frame` | 카드 아트 창(투명) + 이름 칸이 붙은 사선 띠 | Power / Unique (유니크는 이중 테두리·눈금 장식) |
| `Label_Plate` | 위 영문 라벨 자리 판·선 (있으면) | Power / Unique |
| `Chip` | 아래 작은 칸 ("POWER" / "UNIQUE" 자리, 글자 없음) | Power / Unique |
| `Line` | 띠를 따라가는 청록 헤어라인 | — |

카드 아트 창 좌표(투명 창)를 2배 PNG 기준과 1배 화면 기준으로 둘 다 적는다.

## 4. Victory — 작전 종료

| 조각 | 설명 |
|---|---|
| `Band` | 종이색 넓은 띠 (가로 9-slice) |
| `Line_Top`, `Line_Bottom` | 청록 헤어라인 |
| `Slash` | 띠 가운데·끝의 사선 장식 |
| `Tag` | 아래 먹색 태그 판 (글자 없음) |
| `Diamond` | 태그 끝 청록 마름모 |
| `Flash` | 흰 섬광 한 장 (부드러운 알파 허용) |

## 5. Defeat — 작전 실패

Victory와 같은 구성의 먹색 버전이다. 회색 헤어라인과 회색 태그로 낸다. 섬광은 없다.

## 6. Result — 결과 화면

| 조각 | 설명 | 변형 |
|---|---|---|
| `Panel` | 종이 보고서 판 (사선 컷·그림자·모서리 장식 포함, 9-slice) | — |
| `Stat_Divider` | 기록 칸 사이 세로 구분선 | — |
| `Button_Return` | "기지로 귀환 →" 먹색 버튼 (아래 청록 선 포함, 글자·화살표 없음) | Normal / Hover / Pressed |
| `Accent` | 제목 위·판 모서리의 상태 강조 선 | Clear(청록) / Defeat(회색) |

판 안쪽의 영문 라벨, 제목, 부제, 기록 칸 4개(항목명·숫자), 버튼 글자 자리를 글자 자리 좌표로 적는다.
