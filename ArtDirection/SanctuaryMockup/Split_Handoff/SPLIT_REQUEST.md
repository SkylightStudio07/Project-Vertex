# 성소 화면 분절 요청

정본: `../Sanctuary_Select_Mockup.png` (후보 선택), `../Sanctuary_Detail_Mockup.png` (후보 상세). 둘 다 1920×1080. 보기용: `Preview_Select.jpg`, `Preview_Detail.jpg` (960×540).

## 이미지 규칙 (413 방지)

- 모델이 직접 보는 이미지는 위 미리보기 2장까지만. 원본 PNG는 스크립트로 디스크에서만 읽는다.
- 확인은 1200폭 이하 확인용 시트 1장(`../Extracted/Contact_Sheet.jpg`)으로.

## 공통 분절 규칙

- **글자·숫자 전부 제외:** 번호(01~03, 02), "성소", 영문 라벨, 안내 문장, 이름, 소속, "잠긴 후보", 해금 조건, PROFILE·호감도·합류 카드·보상 풀 라벨과 내용, "Lv.2", 카드 이름·코스트·설명, 버튼 글자.
- **캐릭터 그림·카드 아트·자물쇠 외 그림 요소는 칸만.** 치하라 쇼 스탠딩, 전신 아트, 카드 아트, 회색 실루엣은 넣지 않는다 (게임에서 실제 그림을 얹고, 잠긴 후보의 실루엣도 게임에서 캐릭터 그림을 회색으로 칠해 만든다).
- 판·칸은 알파 0/255, 종이 질감 유지. 글자·그림을 지운 자리는 질감으로 복원.
- 기호(자물쇠, →, ←, 번호 밑줄, 줄표, 호감도 칸)는 판과 따로 투명 PNG.
- 이름 아래 고유색 선은 **흰색**으로 (게임에서 캐릭터 색으로 tint).

## 1. 배경

| 조각 | 설명 |
|---|---|
| `Sanctuary_Background` | 1920×1080. 종이 바탕 + 격자·눈금·모서리 "+" 장식. **띠·판·버튼·글자 없이** 바탕만. 상단 0~106px은 HUD 자리라 장식 최소. 선택·상세 공용 |
| `Detail_ArtBackdrop` | 상세 화면 전신 아트 뒤 원형 눈금 장식 (투명 배경) |

## 2. 후보 선택

| 조각 | 설명 | 상태 |
|---|---|---|
| `Strip_Frame` | 사선 띠 하나의 **테두리만** (가운데 투명, 캐릭터를 아래에 깐다). 정본 띠 크기·기울기 그대로 | Normal / Hover / Selected(청록 테두리) / Locked |
| `Strip_Fill` | 띠 안쪽 종이 면 (캐릭터 뒤 바탕) | Normal / Locked(조금 어둡게) |
| `Strip_Mask` | 띠 모양 **흰색 마스크** (캐릭터 크롭용, Strip_Fill과 같은 모양) | — |
| `Strip_NameBand` | 띠 아래 먹색 이름 띠 (띠 기울기 따라 잘린 모양) | Normal / Locked |
| `Strip_NumberUnderline` | 번호 아래 짧은 밑줄 | — |
| `Icon_Lock` | 자물쇠 | — |
| `Button_Detail` | "후보 확인" 버튼 판 (아래 청록 선 포함) | Normal / Hover / Pressed / Disabled |

- 띠 3장은 같은 조각을 쓴다. `layout.json`에 **띠 3장 각각의 배치 좌표**와 띠 안 이름·소속·번호 글자 위치를 적는다.

## 3. 후보 상세

| 조각 | 설명 | 상태 |
|---|---|---|
| `Detail_InfoPanel` | 정보 판 (사선 컷 + 그림자, 안쪽 줄표 라벨·구분선은 제외) 9-slice | — |
| `Detail_SectionRule` | "— PROFILE ———" 같은 구역 머리 줄 (줄표 + 긴 헤어라인, 글자 자리 비움) | — |
| `Detail_CardSlot` | 합류 카드 칸 (카드 아트 자리 투명) | — |
| `Detail_AffinityCell` | 호감도 단계 칸 | Empty / Filled |
| `Button_Back` (종이) · `Button_Join` (먹색) | 정본 크기 | Normal / Hover / Pressed (합류는 Disabled도) |
| `Name_AccentLine` · `Icon_Arrow` · `Icon_ArrowBack` | 기호 | — |

## 결과물

- `../Extracted/` 조각 PNG, `layout.json` (원본 크롭 좌표 · 1920×1080 배치 좌표 · 9-slice · 띠 3장 좌표 · 글자 자리 좌표), `README.md`, `Contact_Sheet.jpg`
