# 대화 선택지 v1 · 조각 요청 (카리나 합류 선택지)

정본: `Choice_Mockup_v1_Default.png` · `_Hover.png` · `_Selected.png` (1920×1080). 연출 수치: `MOTION_SPEC.txt`.
쓰는 곳: 격퇴 후 대화(`DefeatedTalkSession`)의 「데려간다 / 두고 간다」 — 지금은 먹색 버튼 두 개를 코드로 만들고 있다. 이 조각으로 바꾼다.
나중에 NPC 대화 선택지(v2, 꼬리 달린 띠)도 같은 판을 기반으로 만들 예정이니 **판은 꼬리 없이 단독으로 쓸 수 있게** 자른다.

## 0. 조각에 넣지 않는 것

| 요소 | 처리 |
|---|---|
| 배경 · 전투 HUD · 주인공 · 카리나 스탠딩 · 하단 대사창 | 게임 화면 그대로. 자리 rect도 필요 없음 |
| 모든 글자 (`RESPONSE / 03`, 번호, 선택지 문구, `RECRUIT`) | TMP. 글자 상자만 `layout.json`에 |
| 줄의 기울기 · 위치 · 확대 · 반투명 | 코드가 RectTransform 회전 · 이동 · 크기 · CanvasGroup으로 처리. **조각은 기울이지 않은 0° 상태로** 준다 |

## 1. 조각 `Extracted_Draft/` (SVG 원본 + 2배 PNG)

| 조각 | 내용 | 상태 |
|---|---|---|
| `Choice_Panel` | 선택지 판 한 장. 왼쪽 아래 · 오른쪽 위 사선 컷 (목업 그대로). **9-slice** — 가로로 늘려도 사선 컷 모양 유지 | `Normal`(흰 종이) / `Hover`(먹색 + 청록 1.5~2px 테두리) / `Selected`(Hover와 같은 바탕, 테두리 조금 굵게) |
| `Choice_Shadow` | 판과 같은 모양의 하드 섀도우 (흐림 0, 먹색). 판 뒤에 오프셋으로 깐다 | 한 장 (코드가 10px → 14px로 오프셋만 바꿈) |
| `Choice_Ghost` | 등장 잔상용 — 판 외곽선만 가는 선 (속은 투명) | 한 장 |
| `Choice_NumberLine` | 번호 아래 짧은 밑줄 | `Normal`(먹색) / `Hover`(흰색) — 또는 흰색 한 장 + 코드 틴트 |
| `Choice_Divider` | 번호와 문구 사이, 태그와 화살표 사이의 사선 칸막이 | 흰색 한 장 (코드 틴트) |
| `Choice_Arrow` | 오른쪽 끝 → | 흰색 한 장 (코드 틴트) |
| `Choice_SelectBand` | 선택 시 판 아래로 그어지는 청록 6px 띠 (끝이 사선) | 한 장 (가로로 늘림, 9-slice 또는 좌우 끝만 고정) |
| `Header_Rule` | `RESPONSE / 03` 오른쪽 가는 선 | 흰색 한 장 |

흰색 한 장으로 주고 색을 코드에서 곱하는 조각은, 의도한 색을 `layout.json`에 같이 적어 줄 것.

## 2. `layout.json`에 꼭 들어갈 것 (1920×1080, 좌상단 원점, 0° 기준 좌표 + 회전값 따로)

- 머리글: `RESPONSE / 03` 글자 상자, `Header_Rule` rect
- 줄 3개 각각: 판 rect · 회전(°) · 기준점(pivot), 그림자 오프셋
- 줄 안 (판 기준 로컬 좌표): 번호 · 밑줄 · 칸막이 · 문구 · 태그 · 칸막이 · 화살표 rect
- 글자 상자마다: 크기 · 굵기 · 색(Normal / Hover) · 정렬 · 자간
- 줄이 2개 / 1개일 때의 배치 (3개 기준에서 위쪽 줄부터 쓰는지, 가운데 정렬인지)
- 문구가 길 때: 판 최대 폭과 줄임 규칙 (말줄임 or 판이 늘어남)
- 9-slice 경계 (2배 PNG 기준 L/B/R/T)

## 3. 상태 전이 (목업 · MOTION_SPEC 기준, 확인용)

| 상태 | 판 | 글자 | 그 밖 |
|---|---|---|---|
| 기본 | `Normal` | 먹색 | 그림자 10px |
| 마우스 올림 | `Hover` | 흰색 | 오른쪽 18px, 그림자 14px |
| 선택 | `Selected` | 흰색 | 1.2배 · 오른쪽 50 / 위 35px, `Choice_SelectBand` |
| 선택 안 된 줄 | `Normal` | 먹색 | 왼쪽 95~145px · 투명도 .25~.30 |

## 결과물

`Extracted_Draft/` (SVG + 2배 PNG) · `layout.json` · `README.md` · 조각 확인 시트 1장 (`Contact_Sheet.jpg`, 1200폭 이하).
