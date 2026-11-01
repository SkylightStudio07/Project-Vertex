# 작은 UI 여섯 개 · 방향 목업 요청

아래 여섯 UI는 지난번에 **SVG / 도형으로 그린 단색 조각**이라 새 화면들(작전 종료 · 결과 판 · 컷인 · 작전 개시 · 선택지) 옆에서 납작해 보인다.
같은 디자인 언어로 다시 그린다. 이번은 **방향 목업만** 받고, 분절은 목업이 확정된 뒤 따로 요청한다.
작업 방식은 `../UI_Mockup_Split_Workflow.md`.

## 디자인 기준 (이 그림들과 한 식구로 보여야 한다)

- 먹색 판: `../BattleFxMockup/CutIn_Category_v2/Category_Mockup.png`(컷인), `../BattleFxMockup/StartEnd_Redesign_v2/Operation_Start_Mockup_v7.png`(작전 개시)
- 흰 종이 판: `../RasterRedo_v2/Direction_Mockups/Victory.png` · `Result_Clear.png`, `../DialogueMockup/Redesign_v3/Choice_v3.png`
- 공통: 무광 먹색 / 뼈흰색 종이, 아주 옅은 톤 면, 잘린 모서리, 딱딱한 오프셋 그림자, 얇은 무채색 선, **청록은 작은 강조에만**(짧은 막대 · 점 · 번호).
- 금지: 네온 글로 · 금색 · 그런지 · 빽빽한 회로무늬 · 하늘색 큰 다각형 · SVG · 도형 조립 · 에셋 시트. 이미지 모델로 그린 **완성 화면 1920×1080**.
- 지금 모양 · 크기 · 위치 · 정보 구성은 유지하고 표면과 안쪽 디테일을 다시 그린다. 지금 캡처는 `Reference/`.

## 목업 3장

### 1. `Battle_SmallUI.png` — 전투 화면 (`Reference/Current_Battle_Full.png` 장면)

한 화면에 아래 셋을 같이 넣는다.

| UI | 지금 | 요점 |
|---|---|---|
| 적 처치 기록 | `Current_KillFeed.png` — 화면 왼쪽 위 | 먹색 사선 띠 한 줄: 왼쪽 청록 짧은 사선 + `TARGET DOWN` + 구분 사선 + `제파러  무력화`. 두 줄이 쌓인 모습으로 그리고, 둘째 줄은 보스 처치(오른쪽 끝 작은 `BOSS` 칩) |
| 동료 지시 | `Current_OrderTab.png`(접힌 탭) · `Current_OrderList.png`(펼친 목록) | 탭: 마름모 + `지시 ORDER` + 오른쪽 숫자 칸(준비된 지시 수). 목록: 머리 `ORDER / 동료 지시  01` + 줄(동료 얼굴 칸 · 이름 `알레시아` · 스킬 `선봉 투창` · 오른쪽 `READY` 칩). 목록은 펼친 상태로, 줄 둘(하나는 READY, 하나는 충전 중 — 얇은 충전 막대) |
| 맵 · 덱 버튼 | `Current_MapDeckButtons.png` — 오른쪽 위 | 정사각 먹색 버튼 둘(아이콘은 지금 것 그대로 흰 선 아이콘). 하나는 마우스 올린 상태로 |

### 2. `DeckList_Close.png` — 덱 목록 화면 (CardListUI)

- 카드 목록이 깔린 화면 아래쪽의 `CLOSE` 버튼 (`Current_CloseButton.png`: 흰 종이 버튼, 오른쪽 위 잘린 모서리).
- 버튼은 평소 / 마우스 올림 두 상태를 나란히 비교할 수 있게, 화면 한쪽에 작은 비교 칸으로 같이 그려도 된다.

### 3. `CardDetail_Buttons.png` — 카드 상세 (CardDetailUI, 상점 카드 제거)

- 카드 한 장을 크게 띄운 화면, 오른쪽에 버튼 두 개 (`Current_CardDetailButtons.png`).
- 주 버튼 `제거`: 먹색 판 + 흰 글자(확정 동작 — 청록 짧은 막대 하나 정도).
- 보조 버튼 `Cancel`: 흰 종이 판 + 먹색 글자.
- 두 버튼 모두 평소 / 마우스 올림 상태를 비교 칸으로.

## 결과물

`RasterRedo_v3/` — PNG 3장 + `PROMPTS.txt`(실제 생성 프롬프트) + `README.md`.
상태 비교는 한 장 안에 같이 넣되 **같은 버튼 모양 · 같은 크기**로, 상태만 달라야 한다.
