# 상점 v2 분절 요청

정본: `../Shop_Mockup_v2_Canonical.png` (1672×941 — 1920×1080 좌표 × 0.8708). 보기용: `Preview_Canonical.jpg` (960×540).

## 이미지 규칙 (413 방지)

- 모델이 직접 보는 이미지는 `Preview_Canonical.jpg` 한 장만. 원본 PNG는 스크립트로 디스크에서만 읽는다.
- 확인은 1200폭 이하 확인용 시트 1장(`../Extracted/Contact_Sheet.jpg`)으로.

## 분절 전 수정

1. 상점 주인 이름판 라벨 "02 · SHOPKEEPER" → **번호 없이 "SHOPKEEPER"**. 02는 보급품 판 번호와 겹친다.
2. 카드 판·보급품 판·서비스 판의 **워터마크 선화는 판과 따로** 뗀다 (나중에 투명도 조정).
3. 카드 아래 가격표와 아이템 가격표는 같은 조각(9-slice)으로 통일한다.

## 분절 규칙

- **글자·숫자 전부 제외.** 번호(01/02/03), 제목(카드·보급품·카드 제거·나가기·토냐), 영문 라벨, 마이크로 카피, 대사, 가격, 크레딧 숫자, "SOLD OUT" 모두 게임에서 TMP로 넣는다.
- 판은 알파 0/255만 (반투명 금지). 종이 질감은 유지하되 **글자 흔적이 남지 않게** 지운 자리를 질감으로 채운다.
- 판 바깥 회색 그림자 띠는 판 조각에 포함한다.
- 기호(크레딧 마름모, →, ×, 나가기 문 기호, 가위)는 판과 따로 투명 PNG.

## 조각 목록

| 조각 | 설명 | 상태 |
|---|---|---|
| `Panel_Cards` | 01 카드 판 (오른쪽 위 사선 컷, "+" 표시 포함) | — |
| `Panel_Supply` | 02 보급품 판 | — |
| `Panel_Service` | 03 카드 제거 판 | Normal / Hover / Disabled |
| `Panel_Credits` | 크레딧 판 + 아래 청록 막대 | — |
| `Panel_Shopkeeper` | 먹색 이름판 | — |
| `Button_Leave` | 나가기 판 | Normal / Hover / Pressed |
| `Button_Close` | × 버튼 칸 | Normal / Hover / Pressed |
| `PriceTag` | 가격표 띠 (9-slice) | Normal / Hover(청록 테두리) |
| `ItemCell` | 아이템 칸 | Normal / Hover(청록 테두리) |
| `SoldOut_Stamp` | 사선 먹색 띠 (글자 없이) + 카드 위 해칭 오버레이 | — |
| `Watermark_Cards` · `Watermark_Supply` · `Watermark_Service` | 흐린 선화 | — |
| `Icon_Credit` · `Icon_Arrow` · `Icon_Close` · `Icon_Leave` · `Icon_Scissors` · `Icon_RemoveCard` | 기호 | — |
| `NumberUnderline` | 번호 아래 짧은 밑줄 | — |

## 결과물

- `../Extracted/` 조각 PNG, `layout.json` (원본 크롭 좌표 · 1920×1080 배치 좌표 · 9-slice), `README.md`, `Contact_Sheet.jpg`
