# 대화 선택지 v3 · 조각 요청

정본: `Choice_v3.png` (토냐 상점, `RESPONSE / 03` 선택지 3줄).
**선택지 판만 자른다.** 하단 대화창 · 스탠딩 · 어두운 덮개는 자르지 않는다(대화창은 지금 게임 것을 그대로 쓰고, 덮개 · 스탠딩은 게임이 만든다).

## 공통 규칙

- 표면은 이미지 모델로 그린 그림. 정본의 종이 질감, 오른쪽의 옅은 회청 사선 면, 작은 십자, 왼쪽 위 먹색 잘린 모서리를 살린다. SVG · 도형 재구성으로 단색 판을 만들지 않는다. 정본을 같은 프롬프트로 고해상도(또는 판 단독)로 재생성해서 잘라 내는 방식 권장.
- 1~2px 선 · 작은 화살표 · 밑줄 같은 단색 장식만 깨끗한 단색으로 그려도 된다.
- 글자 · 번호를 그림에 넣지 않는다.
- 2배 PNG, 투명 배경, **회전 0°**(정본은 판이 살짝 기울어 있다. 게임이 돌린다). 외곽은 이진 알파.
- **파일 이름은 지금 게임 조각과 같게** 한다(`Assets/Resources/DialogueChoice/`). 폴더째 바꿔 끼운다.

## 조각 `Extracted/`

| 파일 | 내용 |
|---|---|
| `Choice_Panel_Normal.png` | 정본 판 그대로: 흰 종이, 왼쪽 위 먹색 잘린 모서리, 오른쪽 옅은 회청 사선 면 · 십자. 오른쪽 변은 꼬리가 붙을 자리 |
| `Choice_Panel_Hover.png` | 마우스를 올린 판: **먹색 잉크 판으로 뒤집힘**, 청록 2px 테두리, 사선 면은 짙은 회색으로. 알파는 Normal과 같게 |
| `Choice_Panel_Selected.png` | 고른 판: Hover와 같은 먹색 + 청록 3px 테두리, 조금 더 밝은 사선 면. 알파 같게 |
| `Choice_Shadow.png` | 판 모양 먹색 단색 그림자(흐림 0) — 정본의 딱딱한 오프셋 그림자 |
| `Choice_Tail_Normal.png` · `_Hover.png` · `_Selected.png` | 판 오른쪽에서 주인공 쪽으로 뻗는 말풍선 꼬리. 판과 같은 종이 / 잉크, 판과 이어 붙는 쪽에 이음매가 안 보이게(판 위로 4~6px 겹침) |
| `Choice_Tail_Shadow.png` | 꼬리 모양 그림자 |
| `Choice_NumberLine.png` | 번호(01) 아래 짧은 선 |
| `Choice_Divider.png` | 문구 아래 짧은 선(정본 `오늘 추천은?` 밑) |
| `Choice_Arrow.png` | 오른쪽 `→` 화살표. **흰색**으로 — 게임이 Normal은 먹색, Hover · Selected는 흰색으로 칠한다 |
| `Choice_SelectBand.png` | 고른 판 아래에 그어지는 청록 띠(가로로 늘림) |

Hover · Selected는 정본에 없다. 같은 장면에서 두 번째 줄만 Hover, 첫 줄만 Selected로 바꾼 확인용 목업 `Choice_v3_Hover.png` · `Choice_v3_Selected.png`도 같이 뽑아 둔다.

## 9-slice

- 판은 문구 길이에 따라 **가로로 늘어난다**(화면 기준 너비 720 ~ 960). 세로는 고정.
- 왼쪽 경계에 먹색 모서리, 오른쪽 경계에 사선 면 · 십자를 다 넣고, 가운데는 **가로로 반복 가능한 종이 질감만**.
- 경계값(PNG px, 왼 · 아래 · 오른 · 위)을 `layout.json`에 적는다. Normal · Hover · Selected · Shadow 경계는 같게.

## `layout.json`

- 판 화면 크기(1920 기준, 회전 전), 9-slice 경계.
- 꼬리 크기와 위치: **판 오른쪽 위 모서리 기준 오프셋**(판 너비가 바뀌어도 오른쪽에 붙어 따라가게).
- 그림자 오프셋(판 기준 x, y).
- 판 안 글자 자리(회전 전, 판 왼쪽 위 기준): 번호 · 문구 · 화살표의 `[x,y,w,h]`, 글자 크기, 색(hex). 헤더 `RESPONSE / 03`의 크기 · 색도.
- 정본의 세 줄 위치와 기울기(도) — 참고용.

## 결과물

`Redesign_v3/Extracted/` — PNG + `layout.json` + `Contact_Sheet.jpg`(조각 단독 + 너비 720 / 960으로 늘려 조립한 Normal · Hover · Selected) + `README.md`(SVG 미사용 확인, 생성 프롬프트).
