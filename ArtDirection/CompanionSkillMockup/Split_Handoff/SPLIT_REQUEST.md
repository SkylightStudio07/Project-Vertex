# 동료 스킬 (지시 · 컷인) · 분절 요청 (2배 해상도)

목업은 확정이다. 이 디자인대로 게임 조각을 뽑는다. 컷인 구도는 `../USER_DIRECTION_OVERRIDE.md`(왼쪽 위 삼각 영역)를 따른다. 거기에 **대사 태그 하나만 새로 더한다**(3절).

| 장면 | 정본 | 미리보기 (960×540) |
|---|---|---|
| 지시 목록 | `../CompanionSkill_Order_Mockup.png` | `Preview_Order.jpg` |
| 컷인 기본 | `../CompanionSkill_CutIn_Mockup.png` | `Preview_CutIn.jpg` |
| 컷인 인연 강화 | `../CompanionSkill_CutIn_Enhanced_Mockup.png` | `Preview_CutIn_Enhanced.jpg` |

## 이미지 규칙 (413 방지)

- 모델이 직접 보는 이미지는 **미리보기 JPG뿐**이다(한 번에 2장까지).
- 원본 PNG는 스크립트로 디스크에서만 읽는다.
- 결과 확인은 확인용 시트(1200폭 이하)로 한다.

## ★ 핵심: 2배 해상도로 새로 그린다

- **출력:** 화면 크기의 2배 PNG. 예: 화면에서 460×60인 줄은 920×120.
- **새로 그릴 것:** 판, 줄, 칸, 선, 버튼, 막대, 태그는 목업을 참고해 2배 캔버스에 **벡터처럼 깨끗하게 다시 그린다.** 목업을 잘라 확대하지 않는다.
  - 헤어라인: 2배에서 2px (화면 1px)
  - 테두리·굵은 선: 2배에서 4px 이상
- **좌표:** `layout.json`의 좌표는 **1920×1080 화면 기준(1배)** 으로 적는다. 조각마다 `"scale": 2`와 실제 PNG 크기를 함께 적는다.
- **9-slice:** **2배 PNG 기준 픽셀**로 적는다. Unity에서는 PPU 200으로 임포트한다.
- **알파:** 판, 칸, 버튼은 0 또는 255만 쓴다. 컷인 눈매 그림만 예외(가장자리 부드럽게 가능).

## 1. 지시 버튼 · 목록

### 버튼

| 조각 | 설명 |
|---|---|
| `Order_Button` | 먹색 칸 + 사선 컷. **준비 없음** 상태 바탕. 가로 9-slice |
| `Order_Button_Active` | 같은 크기. 위쪽 가장자리 청록 선이 있는 **펼침 / 준비 있음** 상태 (목업 왼쪽 아래 버튼) |
| `Order_Button_Disabled` | 같은 크기. 적 턴 회색 톤 |
| `Order_Diamond` | 왼쪽 마름모. 흰색으로 뽑는다(게임에서 청록·회색으로 칠함) |
| `Order_Badge` | 오른쪽 숫자 배지 칸(청록 바탕). 숫자는 빼고 TMP로 얹는다 |

### 목록 판·줄

| 조각 | 설명 |
|---|---|
| `List_Panel` | 회백색 판 + 헤어라인 + 사선 모서리. 9-slice (동행 1~3명에 따라 세로 길이가 바뀐다) |
| `List_HeaderRule` | 머리("ORDER / 동료 지시 03") 아래 가로 선 |
| `Row_Normal` | 충전 중 줄 바탕 (회백색 + 먹색 1px 테두리). 가로 9-slice |
| `Row_Hover` | 먹색 반전 줄 (목업 첫 줄). 가로 9-slice |
| `Row_ReadyBar` | 준비됨 줄 왼쪽의 청록 세로선 단독 (호버 여부와 상관없이 얹는다) |
| `Face_Frame` | 얼굴 칸 먹색 테두리. 가운데는 투명(얼굴은 게임이 마스크로 넣는다) |
| `Face_Frame_Enhanced` | 같은 크기. 인연 강화 **이중선** 테두리 |
| `Face_Mask` | 얼굴 칸 안쪽 모양 흰색 단색 (Unity Mask용) |
| `Face_Placeholder` | 회색 실루엣 (얼굴 그림이 없는 동료용) |
| `Ready_Tag` | "READY" 태그 바탕(먹색 칸) + 청록 점. 글자는 빼고 TMP |
| `Charge_Bar_Back` | 진행 막대 빈 칸 하나 (먹색 테두리). 게임이 4칸을 늘어놓는다 |
| `Charge_Bar_Fill` | 진행 막대 채운 칸 하나 (청록) |
| `Mark_Enhanced` | 이름 옆 인연 강화 표식(4방향 별). 먹색 1장 + 흰색 1장(호버 줄용) |

목업 오른쪽의 상태표("준비 1 / 준비 없음 / 적 턴")는 견본이므로 따로 뽑지 않는다. 위 버튼 조각 셋으로 만든다.

## 2. 컷인

컷인은 **삼각 마스크 + 동료 눈매 그림 + 경계선**으로 나눠서, 동료마다 눈매 그림 한 장만 갈아 끼우게 한다.

| 조각 | 설명 |
|---|---|
| `CutIn_Mask` | 왼쪽 위 삼각 영역 흰색 단색 (Unity Mask용). 화면 왼쪽 위 모서리에서 사선 경계까지. 목업과 같은 각도 |
| `CutIn_Edge` | 기본판 사선 경계선 (얇은 회백색·먹색 선). 화면 밖까지 조금 넉넉하게 길게 |
| `CutIn_Edge_Enhanced` | 강화판 청록 이중선 |
| `CutIn_Watermark` | 강화판 삼각 안의 Vertex Mark 워터마크 단독 (투명 배경, 눈매 위에 얹는다) |
| `CutIn_Eye_Cp01` | **치하라 쇼 눈매**. 삼각 영역보다 사방 10% 넉넉한 **직사각형** 그림(마스크 밖으로 밀려 들어오는 움직임이 있어서). 목업의 눈 위치·크기 그대로 |

- 눈매 그림 안의 붉은 눈·피 표현은 원화 색 그대로 둔다.
- 다른 동료 눈매도 같은 크기·같은 눈 위치로 받을 수 있게, README에 **눈 중심 위치와 크기 규격**을 적는다.

## 3. ★ 새로 그릴 것: 대사 태그

목업에 없는 요소다. 확정 방향은 다음과 같다.

- **위치:** 사선 경계선 **바로 바깥**(전투 화면 쪽), 선을 따라 **같은 각도로 기울여** 붙인다. 경계선 가운데보다 약간 아래쪽(왼쪽)에서 시작한다.
- **내용 (글자는 TMP, 예시):** `ORDER · 흡혈  ─  "…쓴 만큼, 채울게."`
  - 작은 영문 "ORDER" + 스킬 이름(굵게) + 짧은 청록 구분선 + 대사.
- **모양:** 먹색 가로 띠 + 왼쪽 끝 청록 짧은 띠. 사선 양 끝 컷. 높이는 화면 약 44.
- **강화판:** 같은 태그에 위쪽 청록 헤어라인 하나를 더한다.

| 조각 | 설명 |
|---|---|
| `Tag_Back` | 먹색 띠. **기울이지 않은 수평 상태**로 뽑는다(게임에서 회전). 가로 9-slice (대사 길이가 달라서) |
| `Tag_Back_Enhanced` | 같은 크기. 위 청록 헤어라인 추가 |
| `Tag_Divider` | 스킬 이름과 대사 사이 짧은 청록 구분선 |

- 확인용으로 `Assembly_CutIn.jpg`에 태그를 얹은 모습을 같이 그려 준다(글자 포함 가능).

## 4. 빼는 것

- **글자 전부:** 지시, ORDER, ORDER / 동료 지시, 03, 동료 이름, 스킬 이름, READY, 1턴, 1/3, 숫자 배지 숫자, 대사
- 전투 화면 배경, 상단 바, HUD, 캐릭터, 손패, 적
- 목업 오른쪽 상태표 견본

## 5. layout.json에 적을 것

좌표는 모두 1배 화면 rect다.

- **지시 버튼:** rect, 마름모·라벨("지시", "ORDER")·배지 자리, 배지 숫자 자리
- **목록:**
  - 판 rect (동행 3명 기준), 판이 버튼 위로 붙는 간격
  - 머리 글자 자리(영문·한글·동행 수), 머리 선 y
  - 첫 줄 rect, 줄 간격
  - 줄 안 요소 오프셋: 준비 세로선, 얼굴 칸, 강화 표식, 동료 이름, 스킬 이름, READY 태그, 진행 막대 칸 4개, 남은 수 글자
  - 글자마다 박스, 정렬, 대략 글자 크기, 색 hex (보통 / 호버 반전 두 가지)
- **컷인:**
  - 삼각 영역 꼭짓점 3개 (화면 좌표), 경계선 각도
  - 눈매 그림 rect (마스크 안에서 멈춘 위치)
  - 워터마크 위치
  - 대사 태그: 기준점, 회전 각도, 기본 길이, 글자 자리(ORDER / 스킬 이름 / 구분선 / 대사)
- **9-slice 값**

## 6. 결과물

```
../Extracted/
  Order_Button.png  Order_Button_Active.png  Order_Button_Disabled.png  Order_Diamond.png  Order_Badge.png
  List_Panel.png  List_HeaderRule.png  Row_Normal.png  Row_Hover.png  Row_ReadyBar.png
  Face_Frame.png  Face_Frame_Enhanced.png  Face_Mask.png  Face_Placeholder.png
  Ready_Tag.png  Charge_Bar_Back.png  Charge_Bar_Fill.png  Mark_Enhanced.png  Mark_Enhanced_White.png
  CutIn_Mask.png  CutIn_Edge.png  CutIn_Edge_Enhanced.png  CutIn_Watermark.png  CutIn_Eye_Cp01.png
  Tag_Back.png  Tag_Back_Enhanced.png  Tag_Divider.png
  layout.json
  README.md
  Contact_Sheet.jpg          (조각 모음, 1200폭 이하)
  Assembly_Order.jpg         (조각만으로 조립한 지시 목록, 1200폭 이하)
  Assembly_CutIn.jpg         (조각만으로 조립한 기본 컷인 + 대사 태그, 1200폭 이하)
  Assembly_CutIn_Enhanced.jpg
```
