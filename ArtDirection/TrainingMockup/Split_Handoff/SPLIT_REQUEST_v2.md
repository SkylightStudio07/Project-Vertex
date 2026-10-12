# 훈련장 v2 분절 요청 (동료 편성 페이지 + 메인 동행 칸)

정본은 둘 다 1672×941이다.
- `../Training_Companion_Mockup.png` (동료 편성 페이지)
- `../Training_Main_Mockup_v2.png` (메인 — 동행 3칸 고정)

보기용은 `Preview_Companion.jpg`, `Preview_Main_v2.jpg`(960×540)다.

1차 분절(`../Extracted/`)은 그대로 두고, **이번 조각은 `../Extracted_v2/`에 따로 낸다.** 1차 조각과 같은 것은 다시 뽑지 않는다.

## 이미지 규칙 (413 방지)

- 모델이 직접 보는 이미지는 위 미리보기 2장까지만이다. 원본 PNG는 스크립트로 디스크에서만 읽는다.
- 결과 확인은 1200폭 이하 확인용 시트 1장(`../Extracted_v2/Contact_Sheet.jpg`)으로 한다.

## 재사용 (다시 뽑지 않음, README에 "재사용"으로만 적기)

| 쓰임 | 기존 조각 |
|---|---|
| 편성 페이지 바깥 판·머리·필터 구역 | `Extracted/Base_Deck` 문법. 단 **구역 구성이 다르면 아래 `Base_Companion`을 새로 뽑는다** |
| 메인 바탕 | `Extracted/Base_Main` (동료 구역이 1차와 같으면 재사용, 다르면 `Base_Main_v2`) |
| 동행 중인 동료 카드 (메인) | `Extracted/CompanionCard_*` + `CompanionTag` |
| 완료 버튼 | `Extracted/Button_Done_*` |
| 스크롤 막대 | `Extracted/Scrollbar_*` |
| 필터 체크박스 | `ArmoryMockup/CardCatalogExtracted/Checkbox_*` |
| ← 돌아가기 | `LoadoutMockup/Extracted/Back_Normal(_Hover)` + `Icon_BackArrow` |
| 호감도 단계 칸 | `SanctuaryMockup/Extracted/Detail_AffinityCell_Empty/Filled` (목업 칸과 모양이 같으면 재사용. 다르면 `AffinityCell_Empty/Filled` 새로 뽑기) |
| 소속 앞 청록 마름모 | `Extracted/DefeatPip_Filled` |
| 이름 아래 고유색 선 | `SanctuaryMockup/Extracted/Name_AccentLine` (흰색, 게임에서 tint) |

## 공통 분절 규칙

- **글자·숫자는 전부 뺀다.**
  - 제목, 섹션 이름, "동행 가능 5 · 전체 14", "2 / 3"
  - 이름, 소속, "동행 가능", "잠긴 동료", "호감도 Lv.2 필요", "호감도 Lv.2", "보유 카드"
  - 카드 이름, "동행" 배지 글자, "편성 →", "+ 동료 편성", "완료"
- **그림은 칸만 남긴다.** 치하라 쇼·이카루스 초상, 큰 초상, 실루엣, 카드 그림은 넣지 않는다. 게임에서 실제 그림을 얹고, 잠김은 게임에서 회색 실루엣으로 칠한다.
- **"동행" 표시는 하나로 통일한다.** 메인 목업에서 카드마다 위치가 다르다(왼쪽 아래 / 오른쪽 위). 메인은 기존 `CompanionTag`(왼쪽 아래)를 쓰고, 편성 격자는 이번에 뽑는 `PartyBadge`(오른쪽 위) 하나로 한다.
- 판과 칸의 알파는 0 또는 255, 종이 질감은 유지한다. 지운 자리는 질감으로 복원한다.
- 상태별 조각은 같은 크기·같은 위치로 맞춘다.
- 기호(+, ×)는 따로 투명 PNG로 뽑는다.

## 1. 동료 편성 페이지

| 조각 | 설명 | 상태 |
|---|---|---|
| `Base_Companion` | 1672×941 무문자 바탕. 필터·격자·선택한 동료·동행 구역 판과 구분선만 남긴다. 칸·버튼·글자·그림은 뺀다 | — |
| `PortraitCell` | 격자 초상 칸. 초상 창은 투명, 아래 이름 띠 칸은 포함 | Normal / Hover / Party(청록 테두리) / Locked(회색) |
| `PartyBadge` | 칸 오른쪽 위 청록 "동행" 배지 칸 (글자 없음) | — |
| `Icon_Lock` | 잠긴 초상 가운데 자물쇠 (1차 `Icon_Lock`과 같으면 재사용) | — |
| `DetailPortrait_Frame` | 선택한 동료 큰 초상 틀. 초상 창은 투명. 바탕에 포함돼 있으면 생략하고 창 좌표만 적는다 | — |
| `DetailCardSlot` | "보유 카드" 칸 (카드 그림 창은 투명, 에너지 점 칸 포함 가능) | — |
| `PartySlot` | 오른쪽 아래 동행 큰 칸. 초상 창은 투명 | Filled / Hover / Empty(점선 테두리) |
| `Button_Remove` | 동행 칸 오른쪽 위 × 작은 버튼 (기호 포함) | Normal / Hover / Pressed |
| `Icon_Plus` | 빈 동행 칸 가운데 + (흑연, 투명 배경) | — |

## 2. 메인 v2 (동행 3칸)

| 조각 | 설명 | 상태 |
|---|---|---|
| `CompanionSlot_Empty` | 빈 동행 칸 (점선 테두리). 동료 카드와 같은 크기 | Normal / Hover |
| `Button_Formation` | 동료 구역 오른쪽 위 "편성 →" 작은 종이 버튼 칸 (글자·화살표 없음) | Normal / Hover / Pressed |

## 결과물

`../Extracted_v2/` 폴더에 아래를 넣는다.
- 조각 PNG
- `README.md` (재사용 목록 포함)
- `Contact_Sheet.jpg`
- `layout.json`, 조각마다 아래를 적는다.
  - 원본 크롭 좌표
  - 1672×941 배치 좌표 (좌상단)
  - 9-slice (Left, Bottom, Right, Top)
  - 반복 간격: 초상 격자 열·행 간격과 열 수, 동행 칸 간격
  - 창 좌표: 초상 창, 큰 초상 창, 카드 그림 창, 동행 칸 초상 창
  - 글자 자리 좌표
    - 편성 페이지: 섹션 제목·개수, 초상 칸 이름·상태 줄, 선택한 동료의 이름·소속·호감도·보유 카드 이름, 동행 칸 이름, 완료
    - 메인 v2: 동료 구역 제목·개수·편성 버튼, 빈 칸 "동료 편성"
