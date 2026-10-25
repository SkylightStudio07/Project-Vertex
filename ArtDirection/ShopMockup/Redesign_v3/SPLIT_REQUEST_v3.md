# 전투 중 보급 상점 v3 · 조각 요청

`Shop_Mockup_v3.png` 방향 확정 (왼쪽 토냐 + 유적 배경, 오른쪽 진열 판, 우하단 하드 섀도우). 아래를 반영해 **조각과 `layout.json`** 을 만든다.
- **좌표는 1920×1080 기준**. 원본이 1672×941이므로 ×1.148로 옮긴다. 성소 v3(`../../SanctuaryMockup/Redesign_v2/Extracted_Draft/layout.json`)와 같은 형식.
- SVG 원본과 2배 PNG를 같이 준다. **글자 · 숫자는 굽지 않는다 (게임 TMP).**
- v2 조각(`../Extracted/`)과 모양이 같은 것은 다시 그리지 말고 `layout.json`에 외부 참조로 적는다 (예: `Icon_Arrow`, `Icon_Scissors`, `Icon_RemoveCard`, `Icon_Leave`, `Icon_Close`, `SoldOut_*`).

## 0. 조각에 넣지 않는 것

| 요소 | 처리 |
|---|---|
| 토냐 · 유적 배경 · 카운터 | 게임의 상점 주인 그림(막별 `ShopkeeperData`). 자리 rect만 |
| 카드 6장 | 게임 카드 프리팹. 카드 자리 rect만 (카드 크기 그대로, 받침 위 가운데) |
| 보급품 아이콘 | 게임 아이템 아이콘. 칸 안 아이콘 rect만 |
| 왼쪽 위 의뢰 추적 (`STORY · 블랙박스 회수`) | 전투 HUD(`QuestTrackerView`)가 그대로 뜬다. 자리만 비운다 |
| 상단 작전 바 · MAP · DECK | 게임 HUD. 상단 0~106px 비움 |
| 모든 글자 | TMP. 글자 상자만 `layout.json`에 |

## 1. 고칠 것

| # | 고칠 것 |
|---|---|
| 1 | **살 수 없는 상품 상태.** 크레딧이 모자라면 가격 글자는 게임이 붉게 칠한다(`ShopStockEntry.unaffordableColor`). 그림 쪽은 **가격표 `Unaffordable` 상태**(테두리 · 바탕 한 단계 흐림)와 **받침 · 아이템 칸 `Unaffordable` 상태**(살짝 어둡게)를 준다. 품절은 기존 `SoldOut_*` 그대로 |
| 2 | **보급품 이름.** 칸 아이콘만으로는 수류탄 둘이 구분되지 않는다. 아이템 칸 아래, 가격표 위에 **이름 한 줄 자리**를 둔다 (글자 상자, 15~16pt, 칸 폭 안에서 말줄임). 마우스 올림 툴팁은 게임에 이미 있다 |
| 3 | **재화 기호 통일.** 오른쪽 위 `CREDITS 33` 숫자 앞에도 가격표와 같은 마름모(`Icon_Credit`)를 둔다. 자리 rect만 |
| 4 | `REMOVE SERVICE` 사이에 점처럼 보이는 흔적이 있다. 글자는 TMP로 다시 넣으니 조각에 남지 않게만 |
| 5 | 그림자는 **오른쪽 아래 하드 섀도우만** (흐림 0). 큰 판 12px · 먹색 30% 안팎, 받침 · 칸 · 가격표 6~8px. 그림자는 판과 따로 떼거나, Unity `Shadow`로 처리할 수 있게 판 조각에서 빼고 `layout.json`에 오프셋 · 색만 적는다 (성소 v3과 같은 규칙: 둘 중 하나만) |

## 2. 조각 `Extracted_Draft/`

| 조각 | 내용 | 상태 |
|---|---|---|
| `Shop_BG` (1920×1080) | 오른쪽 백색 · 회청색 배경 면만 (판 · 토냐 · 글자 없이). 왼쪽 토냐 자리는 투명 | — |
| `Panel_Cards` | 01 카드 판 (사선 컷 모서리) | — |
| `Panel_Supply` | 02 보급품 판 (오른쪽 위 상자 선화는 `Watermark_Supply`로 따로) | — |
| `Panel_Service` | 03 카드 제거 판 (카드 · 가위 선화는 따로) | Normal / Hover / Disabled |
| `Panel_Credits` | 크레딧 판 + 아래 청록 막대 | — |
| `Button_Leave` | 나가기 판 | Normal / Hover / Pressed |
| `Plinth_Card` | 카드 한 장 받침 (카드 뒤 회백 판 + 아래 가격표 자리) | Normal / Hover / Unaffordable |
| `ItemCell` | 보급품 칸 | Normal / Hover / Unaffordable |
| `PriceTag` | 가격표 띠 (9-slice, 카드 · 보급품 · 카드 제거 공용) | Normal / Hover / Unaffordable |
| `Plate_Shopkeeper` | 카운터 아래 먹색 이름판 (`SHOPKEEPER` + 이름 + 대사 자리, 오른쪽 위 짧은 청록 표식) | — |
| `Shadow_*` | 위 판 · 받침 · 칸 · 가격표 그림자 (판과 따로 줄 경우) | — |
| `Watermark_Cards` · `Watermark_Supply` · `Watermark_Service` | 흐린 선화 (v2와 같으면 외부 참조) | — |
| `layout.json` | 아래 rect 전부 + 글자 상자(크기 · 색 · 정렬 · 자간) + 상태 전이 |

`layout.json`에 꼭 들어갈 rect:
- 상점 주인 그림 자리, 이름판 · `SHOPKEEPER` · 이름 · 대사
- 제목 줄 `보급 상점 / FIELD SUPPLY` (왼쪽 위, 의뢰 추적 아래로 밀리지 않게 높이 확인)
- 카드 판: 번호 · 제목 · 영문 라벨, 카드 6자리(3×2) 각각의 받침 · 카드 · 가격표 rect
- 보급품 판: 번호 · 제목 · 영문 라벨, 4칸(2×2) 각각의 칸 · 아이콘 · **이름 한 줄** · 가격표 rect
- 카드 제거 판: 번호 · 제목 · 영문 라벨 · 선화 · 가격표
- 크레딧 판: `CREDITS` 라벨 · 마름모 · 숫자 · × 버튼
- 나가기 버튼: 문 기호 · `나가기` · `LEAVE` · →

## 3. 상태

| 상태 | 모습 |
|---|---|
| 살 수 있음 | Normal. 가격 먹색 |
| 마우스 올림 | 받침 · 칸 · 가격표 Hover (청록 테두리) |
| 살 수 없음 | Unaffordable 조각 + 가격 붉은 글자 (게임) |
| 품절 | 기존 `SoldOut_Band` · `SoldOut_Hatching` · `SoldOut_Stamp` 겹침 (글자 `SOLD OUT`은 TMP) |
| 카드 제거 이미 사용 / 돈 부족 | `Panel_Service` Disabled |

결과물: `Shop_Mockup_v4.png` (1절 반영, 카드는 게임 카드 캡처, 토냐는 프로젝트 원본) + `Extracted_Draft/`.
