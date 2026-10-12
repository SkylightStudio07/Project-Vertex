# 축복 화면 v1 분절 요청

정본: `../Blessing_Mockup_v1_Canonical.png` (1672×941 — 1920×1080 좌표 × 0.8708). 보기용: `Preview_Canonical.jpg` (960×540).

## 이미지 규칙 (413 방지)

- 모델이 직접 보는 이미지는 `Preview_Canonical.jpg` 한 장만. 원본 PNG는 스크립트로 디스크에서만 읽는다.
- 확인은 1200폭 이하 확인용 시트 1장(`../Extracted/Contact_Sheet.jpg`)으로.

## 분절 전 수정

1. 위쪽 검은 사각 3개와 MAP·DECK 버튼은 **게임 HUD**다. 조각에 넣지 않는다.
2. 선택지 판 01(호버)·02(비활성)·03·04는 **같은 판의 상태 차이**로 정리한다. 판 모양이 줄마다 달라 보이면 두 종류(A·B, 사선 컷 위치만 다름)로 통일한다.
3. 대사 판 오른쪽의 나침반 워터마크는 판과 따로 뗀다 (게임에서 투명도 조정).

## 분절 규칙

- **글자·숫자 전부 제외.** 번호(01~04), 제목(정화·연마·보급·교감), 영문 라벨, 설명, 대사, "VERTEX / ENCOUNTER LOG", "ENTITY / MACHINA", 이름, 소개, 왼쪽 위 "BLESSING / BETWEEN REBIRTH AND DEATH" 모두 게임에서 TMP로 넣는다.
- **선택지 아이콘(정화·연마·보급·교감)과 대사 초상은 빼고 칸만** 남긴다 (기존 `Assets/Art/Blessing/UI` 아이콘을 게임에서 얹는다).
- 판은 알파 0/255 (반투명 금지). 종이 질감 유지, 글자 지운 자리는 질감으로 채운다. 판 바깥 그림자 띠는 판 조각에 포함.
- 기호(→, 번호 밑줄, 세로 구분선, 이름판 붉은 선, "+" 표시)는 판과 따로 투명 PNG.

## 조각 목록

| 조각 | 설명 | 상태 |
|---|---|---|
| `Panel_Dialogue` | 대사 판 (왼쪽 위 사선 컷, 위쪽 가는 선 포함) | — |
| `Portrait_Cell` | 대사 판 왼쪽 초상 칸·세로선 | — |
| `Watermark_Compass` | 대사 판 오른쪽 나침반 선화 | — |
| `Panel_Option_A` · `Panel_Option_B` | 선택지 판 (9-slice) | Normal / Hover(청록 테두리) / Disabled(회색) |
| `Option_IconCell` | 아이콘 칸 | Normal / Hover(먹색 반전) / Disabled |
| `Option_Divider` | 제목·설명 사이 세로선 | — |
| `Panel_Nameplate` | 마키나 먹색 이름판 ("+" 포함) | — |
| `Nameplate_AccentLine` | 이름 아래 붉은 선 | — |
| `Corner_Marks` | 화면 가장자리 선·"+" (왼쪽 위·오른쪽 아래) 투명 오버레이 1920×1080 | — |
| `Icon_Arrow` · `NumberUnderline` | 기호 | — |

## 결과물

- `../Extracted/` 조각 PNG, `layout.json` (원본 크롭 좌표 · 1920×1080 배치 좌표 · 9-slice), `README.md`, `Contact_Sheet.jpg`
