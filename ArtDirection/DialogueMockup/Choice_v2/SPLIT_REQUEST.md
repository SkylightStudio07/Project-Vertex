# 대화 선택지 v2 · 조각 요청 (꼬리)

정본: `Choice_Mockup_v2_Default.png` · `_Hover.png` · `_Selected.png`.
판은 **v1 조각(`../Choice_v1/Extracted_v2/`)을 그대로 쓴다.** 이번에 새로 필요한 것은 판 오른쪽 끝에 붙는 **꼬리**뿐이다.
v1 v2 때와 같이 **이미지 모델로 그린 그림**으로 만든다 (SVG로 다시 그린 단색 도형 금지). 질감 · 색은 `../Choice_v1/Extracted_v2/Choice_Panel_*.png`와 이어져 보여야 한다.

## 조각 `Extracted/` (2배 PNG, 투명 배경, 0°)

| 파일 | 내용 |
|---|---|
| `Choice_Tail_Normal.png` | 흰 종이 꼬리 + 가장자리 회색 얇은 선 (`Choice_Panel_Normal`과 같은 종이) |
| `Choice_Tail_Hover.png` | 먹색 꼬리 + 청록 2px 테두리 (`Choice_Panel_Hover`와 같은 잉크) |
| `Choice_Tail_Selected.png` | 먹색 + 청록 3px 테두리 (`Choice_Panel_Selected`) |
| `Choice_Tail_Shadow.png` | 꼬리 모양 먹색 단색 (흐림 0) — `Choice_Shadow`와 같이 깐다 |

- 모양: 목업처럼 **각진 삼각 꼬리**, 오른쪽(주인공 쪽)을 가리킨다. 판 높이 96(화면 px) 기준, 판 오른쪽 변의 세로 가운데보다 약간 위에서 나온다.
- **판과 이어 붙는 쪽 변은 판 테두리와 겹쳐 이음매가 안 보이게**: 꼬리의 붙는 쪽에는 테두리 선을 그리지 않고, 판 테두리 선이 꼬리 입구에서 끊긴 것처럼 보이도록 판 위에 살짝(4~6px) 겹쳐 얹는 구조로. 겹치는 부분의 색은 판 바탕과 같게.
- 글자 · 기울기 · 그림자를 그림에 넣지 않는다.

## `layout.json`

- 꼬리 크기 (화면 px) · 판 기준 위치 — **판 오른쪽 위 모서리 기준 오프셋**으로 (판 폭이 736 → 960으로 늘어도 오른쪽에 붙어 따라간다)
- 그림자 꼬리 오프셋 (판 그림자와 같은 규칙이면 그렇다고만)
- Hover · Selected 때 꼬리가 판과 같이 움직이는 것 외에 따로 바뀌는 게 있으면 (예: 끝이 더 뻗음)

## 결과물

`Extracted/` — PNG 4개 + `layout.json` + `Contact_Sheet.jpg` (꼬리 단독 + v1 판(736/960px)에 붙인 Normal / Hover / Selected 조립).
