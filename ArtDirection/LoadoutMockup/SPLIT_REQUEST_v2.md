# 출정 준비 v2 · 조각 요청

`Loadout_Mockup_v2.png` 확정 (회백·먹색 기하 면 + 그림자, 동료 탭과 같은 결). 아래를 반영해 조각으로 나눈다.
- **좌표는 1920×1080 기준**. 원본이 1672×941이므로 ×1.148로 옮긴다. 다른 시설 화면(`../CompanionTabMockup/Extracted_Draft/layout.json`)과 같은 규칙을 따른다.
- SVG 원본과 2배 PNG를 같이 준다. 판은 9-slice. **글자는 굽지 않는다(게임 TMP).**

## 1. 고칠 것

| # | 고칠 것 |
|---|---|
| 1 | **무기 그림은 조각에 넣지 않는다.** 게임에서 직접 만든 SVG 무기(`../WeaponSVG/Weapon_*_Dark.png`)를 넣는다. 큰 무기 자리와 아래 무기 칸 그림 자리는 rect만 `layout.json`에 적는다. 목업 확인용으로는 위 PNG를 그대로 얹는다 |
| 2 | 아래 무기 칸 4개: 실제 무기는 **권총 · 스나이퍼 · 레일건** + 빈 칸 1개. 잠긴 칸의 `해금 조건` → **아티팩트 아이콘 + 1** (`../LobbyShopMockup/Currency_Icons/Icon_Artifact.png`) |
| 3 | 무기 소개 판 수치: `탄창 / 사격 / 연사` → **`탄창 / 사격 추가 피해 / 강화 0/2`** (무기고 장비 강화 화면과 같은 항목). 아래 `무기고에서 강화 ↗`는 버튼이다 |
| 4 | 시작 덱 줄: 한 줄 = 카드 아이콘 칸 + 이름 + 효과 한 줄 + `×개수`. **줄 하나를 틀로 주고**, 게임에서 줄 수만큼 찍는다. 무기고에서 강화한 카드는 줄 오른쪽 위에 작은 **청록 `+` 표식**(`../ArmoryHubMockup/Extracted_Draft/Mark_Upgraded.png`과 같은 것) |
| 5 | 특전 카드: **잠김 / 해금** 두 상태. 해금은 카드 썸네일 1장 + 이름 + 효과가 들어가는 판 |
| 6 | 출정 버튼: **보통 / 마우스 올림 / 누를 수 없음(회색)** 세 상태 |
| 7 | 오른쪽 위 `진행 중 의뢰 1/2` 칩: 의뢰가 없으면 `진행 중 의뢰 없음` 한 줄. 의뢰 이름 줄은 최대 2줄, 틀 하나를 반복해서 쓴다 |

## 2. 조각 `Extracted_Draft/`

| 파일 | 내용 |
|---|---|
| `Loadout_BG.png` (1920×1080) | 바탕 기하 면 · 그림자 · 십자 표식만 (판·글자·무기 없이) |
| `Panel_Title`, `Button_Back` | `01 출정 준비` 제목 판, `← 로비` |
| `Tag_Selected` | 청록 사선 `선택 중` 띠 |
| `Panel_WeaponInfo` | 무기 이름 · 수치 판 (왼쪽 가운데) |
| `Rule_Vertical`, `Rule_Horizontal` | 수치 구분선 |
| `Button_ToArmory` | `무기고에서 강화 ↗` 글자 버튼 밑줄/화살표 |
| `WeaponSlot`, `WeaponSlot_Selected`, `WeaponSlot_Locked`, `WeaponSlot_Empty` | 아래 무기 칸 (이름 띠 포함, 선택 칸은 먹색 띠 + 청록 테두리) |
| `Icon_Lock` | 자물쇠 |
| `Panel_Section` | 오른쪽 판 (먹색 머리 띠 + 회백 몸통 + 왼쪽 청록 표식) — 시작 덱 · 특전 공용, 머리 높이 고정 9-slice |
| `Row_Deck` | 시작 덱 한 줄 틀 |
| `Icon_Card_Attack`, `Icon_Card_Defense`, `Icon_Card_Reload` | 줄 왼쪽 아이콘 (먹색 선화) |
| `Panel_Perk_Locked`, `Panel_Perk` | 특전 카드 잠김 / 해금 판 |
| `Button_Deploy`, `Button_Deploy_Hover`, `Button_Deploy_Disabled` | 출정 버튼 (오른쪽 아래 나침반 장식 포함, 아래 청록 줄) |
| `Chip_Quest`, `Mark_QuestDot` | 진행 중 의뢰 칩, 청록 점 |
| `Shadow_Panel`, `Shadow_Slot` | 판 아래 그림자 (판과 따로) |
| `layout.json` | 모든 rect, 큰 무기 자리 rect, 무기 칸 그림 자리 rect, 시작 덱 줄 간격, 글자 상자(크기·색·정렬·자간) |

결과물: `Loadout_Mockup_v3.png` (1·2절 반영, 무기는 WeaponSVG 그림) + `Extracted_Draft/`.
