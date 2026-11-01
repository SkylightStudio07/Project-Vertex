# 작은 UI 여섯 개 · 조각 요청

정본:
- `Battle_SmallUI.png`: 적 처치 기록, 동료 지시 탭 · 목록, 맵 · 덱 버튼
- `DeckList_Close.png`: `CLOSE` 버튼만 (덱 목록 판 · 카드는 자르지 않는다)
- `CardDetail_Buttons_v2.png`: `제거` · `Cancel` 버튼. 첫 판 `CardDetail_Buttons.png`는 쓰지 않는다.

작업 방식은 `../UI_Mockup_Split_Workflow.md` 4번을 따른다.

## 공통 규칙

- 표면은 이미지 모델로 그린 그림. 정본의 무광 먹색 · 뼈흰색 종이 질감, 옅은 톤 면, 문양을 살린다. SVG · 도형 재구성 금지. 정본을 같은 프롬프트로 고해상도(또는 조각 단독)로 재생성해 잘라 내는 방식 권장.
- 1~2px 선 · 작은 십자 · 짧은 청록 막대 · 사선 같은 단색 장식만 단색으로 그려도 된다.
- **글자 · 숫자를 그림에 넣지 않는다**: `TARGET DOWN`, 적 이름, `BOSS`, `ORDER / 동료 지시`, `01`, 동료 이름 · 스킬, `READY`, `충전 중`, `지시`, 탭 숫자, `CLOSE`, `REMOVE` · `제거`, `RETURN` · `Cancel`, 버튼 번호 `01` · `02`, 큰 흐린 `02` 전부 게임이 쓴다.
- 아이콘은 넣는다: 맵 · 덱 버튼의 흰 선 아이콘, `제거` 버튼 오른쪽의 조준 문양.
- 동료 얼굴은 넣지 않는다 (얼굴 칸 테두리만).
- 2배 PNG(화면 1px = PNG 2px), 투명 배경, 회전 0°, 외곽 이진 알파.
- 상태가 여럿인 조각은 **같은 크기 · 같은 알파**로, 표면만 다르게.
- 늘어나야 하는 조각만 9-slice. 경계는 `layout.json`에 PNG px(왼 · 아래 · 오른 · 위)로 적는다.

## 조각

### `KillFeed/` — 적 처치 기록 (지금 `Assets/Resources/BattleFx/KillFeed/`와 같은 이름)

| 파일 | 내용 |
|---|---|
| `Row.png` | 먹색 사선 띠 한 줄 (가로 9-slice) |
| `Row_Accent.png` | 왼쪽 청록 짧은 사선 |
| `Separator.png` | 가운데 구분 사선 |
| `BossChip.png` | 오른쪽 끝 작은 `BOSS` 칩 테두리 (글자 없음) |

### `CompanionOrder/` — 동료 지시 (지금 `Assets/Resources/CompanionSkill/`와 같은 이름)

| 파일 | 내용 |
|---|---|
| `List_Panel.png` | 펼친 목록 판 (흰 종이, 세로 9-slice — 줄 수에 따라 늘어난다) |
| `List_HeaderRule.png` | 머리 아래 얇은 선 |
| `Row_Normal.png` · `Row_Hover.png` | 목록 한 줄 바탕 (평소 / 마우스 올림) |
| `Row_ReadyBar.png` | 준비된 줄 왼쪽 청록 세로 막대 |
| `Face_Frame.png` | 얼굴 칸 테두리 (가운데 투명) |
| `Ready_Tag.png` | `READY` 칩 (먹색) |
| `Charge_Bar_Back.png` · `Charge_Bar_Fill.png` | 충전 막대 바탕 · 채움 (가로 9-slice) |
| `Order_Button.png` · `_Active.png` · `_Disabled.png` | 접힌 탭 `지시 ORDER` 판: 평소 / 준비된 지시 있음 / 없음 |
| `Order_Diamond.png` | 탭 왼쪽 청록 마름모 |
| `Order_Badge.png` | 탭 오른쪽 숫자 칸 (청록) |

정본에 Active · Disabled · Row_Hover가 따로 안 보이면, 같은 장면에서 상태만 바꾼 확인용 목업을 한 장 같이 뽑아 둔다.

### `HudButtons/` — 맵 · 덱 버튼

| 파일 | 내용 |
|---|---|
| `Button_Map_Normal.png` · `_Hover.png` · `_Pressed.png` | 정사각 먹색 버튼 + 지도 아이콘. Hover는 아래 청록 막대(정본 덱 버튼처럼) |
| `Button_Deck_Normal.png` · `_Hover.png` · `_Pressed.png` | 같은 판 + 카드 아이콘 |

### `Buttons/` — 덱 목록 · 카드 상세 버튼

| 파일 | 내용 |
|---|---|
| `Button_Close_Normal.png` · `_Hover.png` · `_Pressed.png` | 흰 종이 `CLOSE` 버튼, 오른쪽 위 잘린 모서리 + 받침. Hover는 글자 밑 청록 막대 자리가 생기는 정도 — 막대는 따로 `Button_Close_Underline.png` |
| `Button_Remove_Normal.png` · `_Hover.png` · `_Pressed.png` | 먹색 `제거` 버튼: 왼쪽 흰 번호 칸, 세로 구분선, 오른쪽 조준 문양, 잘린 모서리 + 받침 |
| `Button_Cancel_Normal.png` · `_Hover.png` · `_Pressed.png` | 흰 종이 `Cancel` 버튼: 왼쪽 먹색 번호 칸, 세로 구분선, 잘린 모서리 + 받침 |
| `Button_Underline.png` | 버튼 문구 밑 짧은 청록 막대 (Hover 때 게임이 띄운다) |

세 버튼은 문구 길이에 따라 가로로 늘 수 있게 9-slice로. 번호 칸 · 문양은 고정 모서리 칸에.

## `layout.json` (폴더마다)

- 기준 1920×1080, 왼쪽 위 원점, `[x,y,w,h]` 화면 px (정본에서 환산).
- 조각마다 `file` · `png_size_px` · `screen_rect` · 9-slice면 `nine_slice_px`.
- **`text_slots`**: 글자 자리마다 조각 기준 `[x,y,w,h]` · 정렬 · 크기(px) · 굵기 · 색(hex) · 자간 · 불투명도 (상태별로 색이 바뀌면 상태별로).
- 동료 목록은 줄 간격(px)과 머리 높이.

## 결과물

`RasterRedo_v3/Extracted/{KillFeed,CompanionOrder,HudButtons,Buttons}/` — PNG + `layout.json` + `Contact_Sheet.jpg`(조각 단독 + 정본 좌표로 다시 조립해 정본 옆에 나란히 + 상태별 + 9-slice 늘린 모습) + `README.md`(SVG 미사용 확인, 생성 프롬프트).
