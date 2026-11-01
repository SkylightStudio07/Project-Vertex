# 전투 UI v3 · 조각 요청 (승리 띠 · 결과 판 · 카테고리 컷인)

정본 목업 3장 (이 그림대로 게임에 넣는다):

| UI | 정본 |
|---|---|
| 승리 띠 | `Victory.png` (`작전 종료`) |
| 런 결과 판 | `Result_Clear.png` (`런 클리어`) |
| 스킬 컷인 | `../../BattleFxMockup/CutIn_Category_v2/Category_Mockup.png` (POWER / SKILL / ATTACK 세 줄) + 유니크 이중 테두리는 `Skill_Banners.png` 두 번째 띠 |

지난 `Extracted_v2`(질감만 입힌 판)는 **쓰지 않는다.** 이번은 모양 · 내부 구성이 바뀐 새 디자인이라, 크기 · 알파 · 9-slice도 새로 잡는다. 지난 `Extracted/`와 맞출 필요 없다.

## 공통 규칙

1. **표면은 이미지 모델로 그린 그림.** 목업에 보이는 종이 · 잉크 질감, 옅은 기하 면, 사선 제도선, 나침반 문양을 살린다. SVG · 도형 재구성으로 단색 판을 만들지 않는다. 목업을 같은 프롬프트로 고해상도(또는 조각별) 재생성해서 잘라 내는 방식 권장.
2. 1~2px 선 · 작은 십자 · 짧은 청록 막대 같은 단색 장식만 깨끗한 단색으로 그려도 된다.
3. **글자 · 숫자 · 화살표 · 캐릭터 · 카드 아트 · 전장 배경을 조각에 넣지 않는다.** 글자 자리는 비워 둔다 (판 질감만).
4. 2배 PNG(화면 1px = PNG 2px), 투명 배경, 회전 0°. 외곽은 깔끔한 이진 알파. **문양 · 그라데이션 덮개만** 부드러운 알파 허용 (아래 표에 "soft" 표시).
5. 판은 **고정 크기**(늘리지 않음). 9-slice가 필요한 것만 표에 경계를 적었다.
6. 움직임을 따로 주려고 판 · 그림자 · 문양 · 덮개를 나눴다. 각 조각을 목업 좌표에 겹치면 목업과 같은 그림이 되어야 한다.

## 1. 승리 띠 `Victory/`

화면 가운데 띠. 판이 왼쪽에서 밀려 들어오고, 나침반 문양이 살짝 늦게 떠오르고, 청록 막대가 왼쪽부터 그어진다.

| 파일 | 내용 | 알파 |
|---|---|---|
| `Panel.png` | 흰 종이 판: 비대칭 잘린 모서리, 안쪽 옅은 회청 기하 면, 사선 제도선, 오른쪽 위 · 아래 작은 십자 | 이진 |
| `Panel_Shadow.png` | 판 모양 먹색 단색 그림자 (목업의 딱딱한 오프셋 그림자, 흐림 0) | 이진 |
| `Emblem_Compass.png` | 오른쪽의 크고 옅은 나침반 별 — **판 외곽으로 이미 잘라 둔 상태** (판 밖으로 안 넘침) | soft |
| `Corner_Ink.png` | 판 왼쪽 위의 먹색 잘린 모서리 덩어리 | 이진 |
| `Accent_Bar.png` | 아래 청록 막대 (가로로 늘림, 9-slice 좌우 4) | 이진 |
| `Tag.png` | 오른쪽 아래 먹색 태그 판 (`ALL HOSTILES NEUTRALIZED` 자리), 9-slice 좌우 경계는 사선 끝을 포함하게 | 이진 |
| `Tag_Mark.png` | 태그 안 왼쪽 짧은 흰 선 | 이진 |

**패배 `Defeat/`**: 정본 목업이 없다. 같은 프롬프트로 패배판 목업 `Defeat.png`(제목 `작전 실패` · `MISSION FAILED`, 배경을 어둡게 누른 상태)을 먼저 한 장 뽑고, 위 7개를 같은 이름 · 같은 알파로 만든다. 판은 먹색 잉크, 문양은 옅은 회색, 청록 대신 회색 막대(붉은색 금지).

## 2. 런 결과 판 `Result/`

| 파일 | 내용 | 알파 |
|---|---|---|
| `Panel.png` | 흰 종이 보고서 판 전체 (왼쪽 사진 띠 자리 포함, 그 자리는 판 질감만), 안쪽 회청 기하 면 · 오른쪽 아래 옅은 하늘색 사선 면 · 제도선 · 십자 | 이진 |
| `Panel_Shadow.png` | 판 모양 먹색 그림자 | 이진 |
| `Emblem_Compass.png` | 오른쪽 위 나침반 문양, 판 외곽으로 잘라 둠 | soft |
| `Strip_Mask.png` | 왼쪽 세로 사진 띠의 **모양만** (흰색 단색). 게임이 이 모양으로 실제 전장 배경을 잘라 넣는다 | 이진 |
| `Strip_Overlay.png` | 사진 띠 위에 얹는 먹색 반투명 덮개 — 왼쪽은 어둡고, 오른쪽 끝에서 판 종이색으로 녹아드는 그라데이션. 이걸 얹으면 아무 배경이 들어가도 목업처럼 보여야 한다 | soft |
| `Strip_Frame.png` | 사진 띠의 얇은 테두리 · 잘린 모서리 · `FIELD RECORD` 아래 짧은 선 (글자 제외) | 이진 |
| `Button_Return_Normal.png` · `_Hover.png` · `_Pressed.png` | 먹색 잘린 버튼 + 왼쪽 사선 + 아래 얇은 청록 선. Hover는 약간 밝게, Pressed는 더 어둡게. 같은 알파 | 이진 |
| `Title_Accent.png` | 제목 아래 짧은 청록 막대 | 이진 |
| `Stat_Accent.png` | 통계 항목 이름 아래 짧은 청록 막대 | 이진 |
| `Stat_Rule_V.png` · `Stat_Rule_H.png` | 통계 열 사이 세로선 · 숫자 아래 가로선 (끝 점 포함) | 이진 |

**패배판 `Result/` 안에 `_Defeat` 접미사로**: `Panel_Defeat` · `Emblem_Compass_Defeat` · `Strip_Overlay_Defeat` · 버튼 3종 · 두 청록 막대의 회색판. 판은 짙은 회색 종이 또는 먹색, 붉은색 금지. 패배판 목업 `Result_Defeat.png`도 한 장 같이.

## 3. 스킬 컷인 `CutIn/`

한 장짜리 공용 판 + 카테고리 문양 3종 + 유니크 덧테두리. 카드마다 다른 것은 왼쪽 카드 아트(게임이 넣음)와 글자뿐.

| 파일 | 내용 | 알파 |
|---|---|---|
| `Frame.png` | 먹색 공용 판: 잘린 양 끝, 겹친 짙은 회색 면, 얇은 무채색 테두리. **왼쪽 카드 아트 창은 투명**(목업처럼 사선으로 잘린 다각형) | 이진 |
| `Frame_Shadow.png` | 판 모양 그림자 | 이진 |
| `Art_Fade.png` | 아트 창 오른쪽에 얹어 카드 아트가 글자 뒤로 어둡게 녹아들게 하는 먹색 그라데이션 덮개 | soft |
| `Emblem_Power.png` · `Emblem_Skill.png` · `Emblem_Attack.png` | 오른쪽 끝 카테고리 문양 (파워 = 에너지 코어 지느러미, 스킬 = 연결된 육각 모듈, 공격 = 수렴 타격 문양). 세 장 **같은 크기 · 같은 위치 · 같은 농도**, 판 외곽으로 잘라 둠 | soft |
| `Unique_Border.png` | 유니크용 이중 테두리 — `Frame` 위에 겹쳐 얹는 선만 (판 바탕 없음) | 이진 |
| `Index_Line.png` | 번호(`01`) 아래 짧은 청록 막대 | 이진 |
| `Label_Line.png` | `POWER` 오른쪽으로 뻗는 얇은 회색 선 | 이진 |
| `Cross.png` | 오른쪽 위 작은 십자 | 이진 |
| `Name_Slash.png` | 카드 이름 왼쪽 아래 짧은 흰 사선 | 이진 |

- 띠 하나만 쓴다 (세 줄은 비교용). 위치는 목업 첫 줄 기준.
- 카드 아트 창 다각형 꼭짓점을 `layout.json`에 PNG px · 화면 px 둘 다 적는다 (게임이 같은 모양으로 마스크).

## `layout.json` (UI 폴더마다)

- 기준 1920×1080, 왼쪽 위 원점, `[x,y,w,h]` 화면 px. 목업(1672×941)에서 잰 값을 1920 기준으로 환산.
- 조각마다 `file` · `png_size_px` · `screen_rect` · `alpha`(binary / soft) · 9-slice면 `nine_slice_px`.
- **`text_slots`**: 글자 자리마다 `[x,y,w,h]` · 정렬 · 글자 크기(px) · 굵기 · 색(hex) · 자간. 예: 승리 `serial`(01), `title`(작전 종료), `eng`(MISSION ACCOMPLISHED), `caption`(VERTEX // ...), `tag`. 결과 `serial`, `eyebrow`(OPERATION REPORT), `title`, `subtitle`, `caption`, `strip_caption`(FIELD RECORD / 04), `stat_label` ×4, `stat_value` ×4, `button`. 컷인 `index`, `label`(POWER), `name`, `category_ko`(파워).
- 컷인은 `art_window` 다각형.

## 결과물

`../Direction_Split/{Victory,Defeat,Result,CutIn}/` — PNG + `layout.json` + `Contact_Sheet.jpg`(조각 단독 + 목업 좌표로 다시 조립해 정본 옆에 나란히, 글자는 조립 화면에만 얹어도 됨) + `README.md`(SVG 미사용 확인, 생성 프롬프트).
