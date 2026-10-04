# 로비 2.5D 메뉴 · 틀 조각 분절 요청 (2배 해상도)

목업 구성은 확정이다. 로비 틀을 조각으로 다시 그린다.

| 장면 | 정본 | 미리보기 |
|---|---|---|
| 로비 전체 | `../LobbyTilt_Mockup.png` | `Preview_Lobby.jpg` (960×540) |
| 훈련장 호버 | `../LobbyTilt_Mockup_Hover.png` | `Preview_Lobby_Hover.jpg` |
| 부분 확대 | — | `Preview_Crop_TopLeft.jpg`, `Preview_Crop_TopRight.jpg`, `Preview_Crop_BottomCenter.jpg`, `Preview_Crop_BottomRight.jpg` |

## 이미지 규칙 (413 방지)

- 모델이 직접 보는 이미지는 **미리보기 JPG뿐**이다. 한 번에 2장까지 본다.
- 원본 PNG는 스크립트로 디스크에서만 읽는다.
- 결과 확인은 확인용 시트로 한다. 폭은 1200 이하로 만든다.

## 이번 분절 범위

**뽑는 것은 틀 조각 5개와 배경 변형 1장뿐이다.** 메뉴 칸, 캐릭터, 글자는 게임에 이미 있다.

| 목업에 보이는 것 | 처리 |
|---|---|
| 메뉴 6칸 그림 | 기존 그림을 그대로 쓴다. 뽑지 않는다 |
| 칸 옆면 두께·그림자 (먹색) | **게임 코드가 칸 그림 모양 그대로 만든다**(실루엣 셰이더). 뽑지 않는다 |
| 훈련장 호버 (칸 확대 + 그림자 길어짐) | 코드가 처리한다. 뽑지 않는다 |
| 캐릭터, 말풍선 | 기존 것을 쓴다. 뽑지 않는다 |
| 글자 전부 (VERTEX, EXPEDITION…, SHELTER…, OBSERVE…, SOMEWHERE…, 50 / 100, 말풍선 글자) | TMP다. 뽑지 않는다 |
| 진행 막대, 설정 톱니 | 게임 요소다. 뽑지 않는다(자리만 비워 둔다) |

## ★ 2배 해상도

- **출력:** 화면 크기의 2배 PNG. 예: 화면에서 380×110인 조각은 760×220이다.
- **다시 그리기:** 목업은 1672×941로 생성해 늘린 그림이다(`GENERATION_MANIFEST.json`). 그래서 **잘라 확대하지 말고**, 2배 캔버스에 다시 그린다.
  - 종이 면, 사선, 헤어라인, 십자, 먹색 삼각형을 벡터처럼 깨끗하게 그린다.
  - 헤어라인은 2배 기준 2px(화면 1px)로 그린다.
- **종이 질감:** 지금 로비 종이(`../Reference_UI_Static_Current.png`)와 같은 결로 2배에서 새로 입힌다. 바탕색은 #EEF0F2 기준이다.
- **좌표:** `layout.json`에 **1920×1080 화면 기준(1배)** 으로 적는다. 조각마다 `"scale": 2`와 PNG 크기를 함께 적는다.
- **알파:** 0 또는 255만 쓴다.

## 1. 틀 조각

| 조각 | 내용 | 목업 위치 (1920×1080, 대략) |
|---|---|---|
| `Frame_TopLeft` | 왼쪽 위 종이 모서리. 오른쪽 끝이 사선이고 가장자리 헤어라인이 있다. 로고 별·십자 장식을 넣는다. VERTEX, EXPEDITION 글자 자리는 비운다 | x 0~400, y 0~120 |
| `Frame_TopBar` | 오른쪽 위 종이 띠. 왼쪽 끝이 사선이고 오른쪽 끝에 세로 헤어라인과 십자가 있다. 진행 막대, 50 / 100, 톱니, OBSERVE 글자 자리는 비운다 | x 1180~1920, y 0~110 |
| `Frame_BottomLeft` | 왼쪽 아래 종이 모서리. 위쪽이 사선이고 헤어라인과 십자가 있다. SHELTER 글자 자리는 비운다 | x 0~320, y 920~1080 |
| `Frame_BottomCenter` | 캐릭터 발 오른쪽, 메뉴 왼쪽 아래 밑의 종이 삼각 쐐기. 사선 가장자리가 있고 십자와 헤어라인 장식을 넣는다(`Preview_Crop_BottomCenter.jpg`) | x 740~1000, y 840~1080 |
| `Frame_BottomRight` | 오른쪽 아래 먹색 삼각형, 흰 사선, 십자. SOMEWHERE 글자 자리는 비운다(`Preview_Crop_BottomRight.jpg`) | x 1780~1920, y 930~1080 |

- 왼쪽 가장자리의 세로 헤어라인, 작은 십자 같은 자잘한 장식은 가장 가까운 조각 안에 넣는다.
- 조각 바깥은 투명이다. **메뉴 자리(x 960~1880, y 100~1010)에는 어떤 조각도 들어가지 않는다.** 단 `Frame_BottomCenter`의 오른쪽 끝은 메뉴 왼쪽 아래 밑으로 조금 들어가도 된다. 메뉴 뒤에 깔린다.
- `layout.json`에는 다음을 적는다.
  - 각 조각의 화면 rect
  - 비워 둔 글자 자리 (박스, 정렬, 대략 글자 크기, 색)
  - 진행 막대·톱니 자리

## 2. 배경 변형

| 파일 | 내용 |
|---|---|
| `Background_RightDim.png` | 지금 배경(`../Reference_Background_Current.png`, 1672×941) **원본 픽셀에 보정만** 한다. 오른쪽 절반(메뉴 쪽)을 목업처럼 어둡고 푸른 회색으로 낮추고, 아주 약하게 흐리게 한다. 왼쪽으로 갈수록 자연스럽게 원본으로 돌아온다. **다시 생성하지 않는다** (천막·다리·산 형태가 원본과 같아야 한다). 크기는 원본과 같은 1672×941, 불투명 |

보정값(밝기, 채도, 흐림 반경, 그라데이션 시작 x)을 README에 적는다. 나중에 같은 보정을 다시 걸 수 있어야 한다.

## 3. 결과물

```
../Extracted/
  Frame_TopLeft.png  Frame_TopBar.png  Frame_BottomLeft.png  Frame_BottomCenter.png  Frame_BottomRight.png
  Background_RightDim.png
  layout.json
  README.md
  Contact_Sheet.jpg       (조각 모음, 1200폭 이하)
  Assembly_Lobby.jpg      (Background_RightDim + 틀 조각만으로 조립, 메뉴·캐릭터·글자 없이, 1200폭 이하)
```
