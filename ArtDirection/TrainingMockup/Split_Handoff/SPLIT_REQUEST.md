# 훈련장 화면 분절 요청

정본은 두 장이고, 둘 다 1672×941이다.
- 메인: `../Training_Main_Mockup.png`
- 덱 편집: `../Training_Deck_Mockup.png`

보기용 미리보기는 `Preview_Main.jpg`, `Preview_Deck.jpg`(960×540)이다.

## 이미지 규칙 (413 방지)

- 모델이 직접 보는 이미지는 위 미리보기 2장까지만이다. 원본 PNG는 스크립트로 디스크에서만 읽는다.
- 결과 확인은 1200폭 이하 확인용 시트 1장(`../Extracted/Contact_Sheet.jpg`)으로 한다.

## 이미 있는 조각 (다시 뽑지 않음)

같은 문법의 조각이 무기고·출정 준비 분절본에 있다. 아래는 **뽑지 말고** README에 "재사용"으로만 적는다.

| 쓰임 | 기존 조각 |
|---|---|
| ← 로비 / ← 돌아가기 버튼 | `LoadoutMockup/Extracted/Back_Normal(_Hover)` + `Icon_BackArrow` |
| 필터 체크박스 (덱 편집) | `ArmoryMockup/CardCatalogExtracted/Checkbox_*` |
| 카드 칸 틀·잠긴 카드 (덱 편집 격자) | `CardCatalogExtracted/Card_Attack_NoArt`, `Card_RewardLocked`, `Lock_Crop`, `Energy_Cyan/Gray` |

목업의 카드 칸이 무기고와 조금 다르면 차이만 README에 적는다(같으면 재사용).

## 공통 분절 규칙

- **글자·숫자는 전부 뺀다.**
  - 제목, 번호 06, 영문 라벨, 섹션 이름, "4 / 10", "15장"
  - 적·동료·카드 이름, "격퇴 1/3", "호감도 Lv.2 필요", "분석 완료 · 격퇴 3회"
  - 배지 글자(일반·보스·동행), ×n 숫자, 버튼 글자, 코스트 칩 숫자 0·1·2·3+
- **그림은 칸만 남긴다.** 적 썸네일, 차원의 공포 큰 그림, 치하라 쇼 초상, 잠긴 실루엣, 카드 아트는 넣지 않는다. 게임에서 실제 그림을 얹고, 실루엣도 게임에서 회색으로 칠한다.
- **덱 목록 줄의 아이콘(별·방패·탄·조준·삼지)은 빼고 칸만 남긴다.** 게임에서 카드 그림 크롭을 넣는다.
- 판과 칸의 알파는 0 또는 255로 하고, 종이 질감은 유지한다. 글자·그림을 지운 자리는 질감으로 복원한다.
- 기호(마름모 점, 자물쇠, →, 스크롤 손잡이)는 판과 따로 투명 PNG로 뽑는다.
- 상태별 조각은 **같은 크기·같은 위치**로 맞춘다.

## 1. 바탕

| 조각 | 설명 |
|---|---|
| `Base_Main` | 메인 1672×941 무문자 바탕. 바깥 로비 배경, 판, 세 구역 판(상대·가운데·덱), 머리 줄 장식, 구분선까지 포함한다. **목록 줄·버튼·동료 카드·그림·글자는 뺀다** |
| `Base_Deck` | 덱 편집 1672×941 무문자 바탕. 필터·격자·현재 덱 구역 판과 구분선만 남긴다. 카드 칸·목록 줄·버튼·글자는 뺀다 |
| `Target_Backdrop` | 메인 가운데 큰 그림 뒤의 원형 과녁·조준선 (투명 배경) |

## 2. 메인

| 조각 | 설명 | 상태 |
|---|---|---|
| `TargetRow` | 상대 목록 한 줄. 썸네일 칸의 바탕은 포함하고 그림은 뺀다 | Normal / Hover / Selected(청록 테두리 + 왼쪽 띠) / Locked(회색) |
| `TypeBadge` | 종류 배지 칸 | Normal(일반, 종이 테두리) / Elite(흑연) / Boss(먹색) |
| `DefeatPip` | 격퇴 마름모 점 | Filled(청록) / Empty(회색 테두리) |
| `Icon_Lock` | 잠긴 썸네일·동료 칸 가운데 자물쇠 | — |
| `Scrollbar` | 목록 스크롤 막대 | Track / Handle |
| `AnalysisBadge` | 큰 그림 아래 "보스" 배지 옆 표식 자리가 따로 판이면 뽑고, 글자뿐이면 생략 | — |
| `CompanionCard` | 동료 초상 카드 틀. 초상 칸은 투명 | Normal / Hover / Selected(청록 테두리 + 아래 선) / Locked |
| `CompanionTag` | 초상 아래쪽 청록 "동행" 태그 칸 | — |
| `DeckRow` | 덱 요약 한 줄. 아이콘 칸은 빈 칸으로 | Normal / Hover |
| `Button_DeckEdit` | "덱 편집 →" 종이 버튼 | Normal / Hover / Pressed |
| `Button_Sortie` | "SIMULATE 출격 →" 먹색 버튼 (아래 청록 선 포함) | Normal / Hover / Pressed / Disabled |
| `Icon_Arrow` | 버튼의 → | 먹색 버튼 위에서 흰색으로 tint하므로 **흰색** |

## 3. 덱 편집

| 조각 | 설명 | 상태 |
|---|---|---|
| `CostChip` | 코스트 필터 칩 칸 | Normal / Hover / Selected |
| `CountBadge` | 카드 칸 오른쪽 위 청록 "×n" 배지 칸 | — |
| `EditRow` | 현재 덱 한 줄. 아이콘 칸은 빈 칸, −/+ 자리는 비운다 | Normal / Hover |
| `Button_Minus`, `Button_Plus` | 줄 안의 −, + 작은 칸 (기호 포함) | Normal / Hover / Pressed / Disabled |
| `Button_Done` | "완료 →" 먹색 버튼 (아래 청록 선 포함) | Normal / Hover / Pressed |

## 결과물

`../Extracted/` 폴더에 아래를 넣는다.
- 조각 PNG
- `README.md`
- `Contact_Sheet.jpg`
- `layout.json`, 조각마다 아래를 적는다.
  - 원본 크롭 좌표
  - **1672×941 배치 좌표** (좌상단)
  - 9-slice (Left, Bottom, Right, Top)
  - 목록 줄·카드 칸은 **반복 간격**(줄 높이 + 간격, 격자 열 간격·행 간격)
  - **글자 자리 좌표**: 각 줄의 이름·배지·점·우측 수치, 동료 카드의 이름·Lv, 버튼 글자, 섹션 제목·개수
