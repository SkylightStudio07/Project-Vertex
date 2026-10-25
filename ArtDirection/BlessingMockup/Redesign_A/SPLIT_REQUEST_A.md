# 축복 화면 A안 · 조각 요청

정본: `Blessing_A_Canonical.png` (1672×941). `../Alternatives_v2/Blessing_A_Atmospheric.png`에서 첫 글자 강조를 뺀 판.
**UI 레이아웃만 가져온다.** 마키나 그림, 배경(달 · 꽃밭 · 꽃잎 · 빛 연출)은 지금 게임 것을 그대로 쓴다.

- **좌표는 1920×1080 기준.** 원본 좌표에 ×1.148을 곱한다. 형식은 v1 `../Extracted/layout.json`과 같게.
- SVG 원본과 2배 PNG를 같이 준다. **글자와 숫자는 굽지 않는다 (게임 TMP).**
- v1 조각(`../Extracted/`)과 모양이 같은 것은 다시 그리지 않는다. `layout.json`에 외부 참조로 적는다.

## 0. 조각에 넣지 않는 것

| 요소 | 처리 |
|---|---|
| 마키나 · 배경 · 달 · 꽃잎 | 게임 원본(`BlessingView.characterImage` · `backgroundImage` · `BlessingAmbienceFx`). 자리 rect만 |
| 상단 작전 바(FLOOR · TURN · BATTLE FIELD · 빈 칸 3개) · MAP · DECK · 눈 버튼 | 게임 HUD. 상단 0~80px 비움 |
| 왼쪽 위 `BLESSING / BETWEEN REBIRTH AND DEATH` · 모서리 선 | v1 `Corner_Marks`와 TMP 그대로 (외부 참조) |
| 선택지 아이콘(정화 · 연마 · 보급 · 교감) | 기존 `Assets/Art/Blessing/UI` 아이콘. 마름모 칸 안 아이콘 rect만 |
| 모든 글자 | TMP. 글자 상자만 `layout.json`에 |

## 1. 고칠 것

| # | 고칠 것 |
|---|---|
| 1 | **기본 상태에 하이라이트 없음.** 시안의 01 줄(밝은 청록 테두리 · 바깥 빛)은 **마우스를 올렸을 때만** 켜진다. 기본은 4줄 모두 02~04와 같은 Normal로 그린다 |
| 2 | **Hover는 빛 번짐 없이.** 청록 테두리 1.5~2px와 판 바탕을 한 단계 밝게 하는 정도로 끝낸다. 바깥 글로우 · 블룸 금지 |
| 3 | **제목은 단색.** 정화 · 연마 · 보급 · 교감은 흰색 한 가지 크기 (첫 글자 강조 없음) |
| 4 | **대사 판에 초상 칸이 없다.** 게임은 `speakerAvatarImage`를 끈다. 판 왼쪽 여백만 시안대로 둔다 |
| 5 | **판은 어두운 남색 반투명.** 알파를 써도 되지만 판 전체에 한 값(약 85~90%)으로 균일하게. 게임이 linear 색 공간이라 반투명 어두운 판이 시안보다 밝게 보이니, `layout.json`에 의도한 색 · 알파를 숫자로 같이 적는다 |
| 6 | 선택지 판 4장은 **같은 판의 상태 차이**다. 줄마다 모양이 다르면 하나로 통일한다 (오른쪽 아래 사선 컷 한 종류) |

## 2. 조각 `Extracted_Draft/`

| 조각 | 내용 | 상태 |
|---|---|---|
| `Panel_Dialogue` | 대사 판 (왼쪽 위 `+` 표식, 위쪽 가는 선, 오른쪽 아래 사선 컷). 9-slice | — |
| `Panel_Option` | 선택지 판 (오른쪽 아래 사선 컷). 9-slice | Normal / Hover / Disabled |
| `Option_IconDiamond` | 마름모 아이콘 칸 (왼쪽 아래 작은 꼬리 포함) | Normal / Hover / Disabled |
| `Option_Divider` | 제목과 설명 사이 세로선 (v1과 같으면 외부 참조) | — |
| `Icon_Chevron` | 오른쪽 `>` 기호 | Normal / Hover |
| `Panel_Nameplate` · `Nameplate_AccentLine` | 마키나 이름판 (v1과 같으면 외부 참조) | — |
| `layout.json` | 아래 rect 전부, 글자 상자(크기 · 색 · 정렬 · 자간), 상태 전이 | — |

`layout.json`에 꼭 들어갈 rect:
- 대사 판: `VERTEX / ENCOUNTER LOG` 라벨, 라벨 오른쪽 가는 선, 대사 2줄 상자 (3줄까지 늘어날 때 판이 아래로 늘어나는지 고정인지 적어 줄 것)
- 선택지 4줄 각각: 판, 마름모 칸, 아이콘, 번호(`01`), 제목, 영문 라벨(`PURIFY`), 세로선, 설명, `>`
- 줄 간격, 대사 판과 첫 줄 사이 간격
- 이름판: `ENTITY / MACHINA`, 이름, 붉은 선, 소개, `+` 두 개
- 마키나 그림 자리 (지금 게임 위치와 겹치지 않는지 확인용)

## 3. 상태

| 상태 | 모습 |
|---|---|
| 기본 | 4줄 모두 Normal |
| 마우스 올림 | `Panel_Option` · `Option_IconDiamond` Hover. 줄 전체가 왼쪽으로 8px 밀림(게임 `hoverShift`) |
| 선택 불가 (예: 강화할 카드 없음) | Disabled. 글자는 게임이 흐리게 |
| 대사 진행 중 | 선택지 숨김, 대사 판만 |

결과물: `Blessing_A_Mockup_v2.png` (1절 반영. 마키나 · 배경은 게임 캡처 `../Alternatives_v2/Reference_Current.png` 사용) + `Extracted_Draft/`.
