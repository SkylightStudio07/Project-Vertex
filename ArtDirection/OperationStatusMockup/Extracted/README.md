# 작전 상황 화면 · 게임용 래스터 분절

`../SPLIT_REQUEST.md` 및 `../../UI_Mockup_Split_Workflow.md`의 분절 단계에 따라 제작했습니다. 확정 평상/호버 PNG 두 장을 참조하여 각 표면을 글자 없이 단독으로 다시 생성한 결과입니다.

## 납품 파일

- 상태별 파일을 포함한 PNG 21개. 모두 요청 표의 정확한 2배 캔버스, RGBA 8비트, 회전 0°입니다.
- `layout.json`: 각 파일의 PNG/화면 크기, 알파 종류, PNG 픽셀 L/B/R/T 9-slice 경계, 판 배치 21개, 글자 자리 68개, 상태 교체 및 게임이 그릴 요소의 참고 좌표.
- `Contact_Sheet.jpg`: 3880×5020. #0B111A 위에 PNG 21개 단독 표시, 평상/호버 정본과 같은 좌표로 재조립한 UI를 각각 나란히 표시, 7개 9-slice 계열의 늘림 예시.
- 이 README에 실제 처리, 확인 결과, 모든 생성 프롬프트를 기록했습니다. 원화·보조 확인본·재현 스크립트·검증 기록은 `../Working/`에 있습니다.

| 파일 | PNG px | 화면 px | 9-slice L,B,R,T (PNG px) | 알파 |
|---|---:|---:|---|---|
| `Title_Plate.png` | 1168×216 | 584×108 | 0, 0, 0, 0 | 이진 |
| `Panel_Body.png` | 976×840 | 488×420 | 60, 180, 64, 180 | 이진 |
| `Panel_Header.png` | 976×120 | 488×60 | 72, 12, 100, 12 | 이진 |
| `Panel_Header_Hover.png` | 976×120 | 488×60 | 72, 12, 100, 12 | 이진 |
| `Button_Back.png` | 468×126 | 234×63 | 0, 0, 0, 0 | 이진 |
| `Button_Back_Hover.png` | 468×126 | 234×63 | 0, 0, 0, 0 | 이진 |
| `Button_Back_Pressed.png` | 468×126 | 234×63 | 0, 0, 0, 0 | 이진 |
| `Row_Card.png` | 858×94 | 429×47 | 12, 12, 12, 12 | 이진 |
| `Op_Card.png` | 922×492 | 461×246 | 56, 56, 56, 56 | 이진 |
| `Op_Card_Hover.png` | 922×492 | 461×246 | 56, 56, 56, 56 | 불투명 면 + 시안 번짐만 soft |
| `Op_Slot_Empty.png` | 918×126 | 459×63 | 40, 16, 40, 16 | 이진 |
| `Button_Board.png` | 930×84 | 465×42 | 58, 16, 58, 16 | 이진 |
| `Button_Board_Hover.png` | 930×84 | 465×42 | 58, 16, 58, 16 | 이진 |
| `Button_Board_Pressed.png` | 930×84 | 465×42 | 58, 16, 58, 16 | 이진 |
| `Button_Deploy.png` | 958×282 | 479×141 | 0, 0, 0, 0 | 이진 |
| `Button_Deploy_Hover.png` | 958×282 | 479×141 | 0, 0, 0, 0 | 불투명 면 + 시안 번짐만 soft |
| `Button_Deploy_Pressed.png` | 958×282 | 479×141 | 0, 0, 0, 0 | 이진 |
| `Dialogue_Panel.png` | 1520×202 | 760×101 | 60, 28, 100, 28 | 이진 |
| `Name_Tag.png` | 416×62 | 208×31 | 0, 0, 0, 0 | 이진 |
| `Check_Off.png` | 44×44 | 22×22 | 0, 0, 0, 0 | 이진 |
| `Check_On.png` | 44×44 | 22×22 | 0, 0, 0, 0 | 이진 |

## 실제 제작 및 후처리

1. 제공된 두 정본을 열고 요청서, 공통 작업 방식, 기존 좌표 실측/생성 기록을 읽었습니다. Unity 에디터나 게임 리소스를 변경하는 작업은 포함하지 않았습니다.
2. 기본 제공 `image_gen.imagegen` 도구를 사용했습니다. 표면·종이/먹색 질감·해칭·잘린 모서리·나침반·체크는 모두 이미지 모델이 그렸습니다. 자산마다 별도 생성 호출을 사용했으며 에셋 시트에서 표면을 조립하지 않았습니다. SVG 미사용. API/CLI 생성 우회도 사용하지 않았습니다.
3. 정본에 없는 상태는 `Working/Additional_States_Proof.png`(로비 호버, 게시판 호버, 출격 눌림, 체크 켜짐), `Working/Back_Pressed_Proof.png`(로비 눌림)으로 확인본을 남겼습니다. 이 두 장은 중간 검토용 전체 장면이며 최종 게임용 자산이 아닙니다.
4. 각 조각은 순마젠타 #FF00FF를 요청한 불투명 배경에 글자 없이 단독으로 생성했습니다. 원본은 `Working/*_magenta.png`입니다. 생성 결과의 캔버스 여백/크기는 일정하지 않아 실측한 유효 바운딩 박스로 정규화했습니다.
5. 채널 차이는 `d=max(abs(R-255),G,abs(B-255))`. 일반 조각은 `d>=96`에서 알파 255, 그 외 0으로 키잉했습니다. 시안 번짐 조각은 `clamp((d-40)/120,0,1)`로 키잉하고 `RGB=(observed-(1-alpha)*(255,0,255))/alpha`로 straight alpha를 복원했습니다. 범위는 0~255로 클램프했습니다.
6. 마젠타가 섞인 외곽 RGB는 깨끗한 불투명 내부 이웃의 평균으로 교정했습니다. 리샘플 후에도 같은 검사를 수행했습니다. 일반 조각은 마지막에 알파를 다시 0/255로 양자화했습니다. 모든 파일의 캔버스 바깥 4 PNG px는 알파 0이며 RGB도 0입니다.
7. 최종 크기로 고품질 Bicubic 리샘플했습니다. 먹색 내부는 알파 255입니다. 작전 카드/출격 계열은 공통 불투명 면을 8 PNG px 안쪽에 배치해 바깥 4 px 투명 안전영역과 최대 3 px 시안 번짐 공간을 확보했습니다. 상태별 불투명 외곽은 평상 상태 마스크로 등록했습니다. 시안 번짐만 그 외곽 근처의 soft alpha로 허용했습니다.
8. 요청에서 허용한 단색 얇은 선 보정만 코드로 했습니다. Row_Card는 생성기가 넣은 불필요한 끝 꺾임을 래스터 크롭으로 제거하고 1 화면 px 회색 테두리를 보정했습니다. Op_Card는 1 화면 px 시안 테두리를 보정했습니다. 상태 교체 시 기존 생성 원화의 시안 테두리/하단 띠 픽셀을 평상 마스크에 맞춰 등록했습니다. 표면이나 문양을 코드로 새로 그리지 않았습니다.
9. 9-slice는 모서리 패치를 고정하고 가운데를 늘려 실제 재조립했습니다. Panel_Body의 눈에 띄는 왼쪽 해칭은 위/아래 고정 모서리에 배치하여 세로 확장 시 해칭이 늘어나지 않게 했습니다. 원본의 연속 해칭 띠 전체를 그대로 유지한 것은 아닙니다.
10. 글자·숫자·화살표·아이콘·무기·진행/덱/비용 막대·구분선·판독문은 최종 PNG에 넣지 않았습니다. Check_On의 흰 체크와 출격 버튼의 나침반은 요청 표에 명시된 예외입니다. Contact_Sheet의 라벨/글자/막대는 검토용 별도 렌더이며 자산에 구워져 있지 않습니다.

## 배치 및 사용

원점은 1920×1080 화면의 왼쪽 위입니다. `screen_rect=[x,y,w,h]`는 투명 여백을 포함한 전체 PNG 캔버스의 화면 사각형입니다. 실제 불투명 면은 일반적으로 2 화면 px, 작전/출격 계열은 4 화면 px 안쪽에 있습니다. 원래 표의 화면 크기를 유지하면서 안전 알파 여백을 확보한 것이므로 정본 바깥선보다 약간 안쪽으로 보일 수 있습니다.

`assets`의 screen_rect는 기본 사용 예시이고 실제 A~D/3개 카드 행 배치는 `placements`를 사용합니다. A~D의 측정 전체 판 경계는 `panel_bounds`에 따로 있습니다. 오른쪽 판의 머리 폭 505px처럼 공유 원본 폭 488px와 다른 경우도 배치에 기록했습니다. 고정 조각은 9-slice가 [0,0,0,0]입니다.

먹색 판은 불투명 PNG로 납품했습니다. 반투명 느낌은 게임에서 적용합니다. `runtime_opacity`와 placements의 opacity는 검토용 추천값입니다. 빈 작전 칸도 내부 알파는 255이며, 배경만 더 투명하게 하고 점선을 유지하려면 게임에서 fill/frame을 구분하는 처리가 필요합니다. 해당 값은 최종 PNG의 알파를 바꾸지 않습니다.

Op_Slot_Empty의 점선 간격을 확장 후에도 유지하려면 9-slice 경계와 함께 가로 위/아래 edge strip을 타일 샘플링하십시오. 단순 Stretch만 쓰면 점선 길이도 같이 늘어납니다. `layout.json`의 edge_sampling에 기록했습니다. Contact_Sheet는 기본 Stretch의 실제 모습을 보여줍니다.

## 검증 결과

- 최종 PNG 21개 모두 IHDR bit depth 8 / color type 6 (RGBA), 요청 크기 정확히 일치, 화면 크기 대비 정확히 2배.
- 최종 판 배치 21개, text_slots 68개, 9-slice 상태별 파일 11개.
- 바깥 4 px 알파 위반 0개.
- 마젠타/보라 오염 휴리스틱 검출 0 px. 검출식은 불투명/반투명 픽셀 중 R>G+18, B>G+18, R>45입니다.
- 이진 조각은 알파 값이 0/255 두 가지뿐입니다. soft 알파는 Op_Card_Hover와 Button_Deploy_Hover에만 있습니다.
- `Op_Card_Hover.png`: soft 알파 300 px.
- `Button_Deploy_Hover.png`: soft 알파 2956 px.
- 정본 옆 재조립 평상/호버 화면, 단독 조각 전체, 9-slice 늘림, 대표 조각 원본 크기를 실제로 열어 질감·문양·글자 미포함 및 #0B111A 위 프린지를 확인했습니다. OCR을 통한 글자 검사는 수행하지 않았습니다.
- `Working/validation.json`은 픽셀 검사와 생성 원화 실측 기록, `Working/final_validation.json`은 PNG 포맷/크기/좌표/메타데이터 검사 결과입니다. errors는 빈 배열입니다.
- 모든 프로젝트 쓰기는 ArtDirection/OperationStatusMockup/ 안에서만 했습니다. git은 사용하지 않았습니다.
- 도구 자체의 자동 원화 캐시는 예외입니다. imagegen은 C:/Users/SKYLIGHT/.codex/generated_images/에 원본을 자동 저장하며 이 저장 위치를 지정하는 인자를 제공하지 않습니다. 선택한 원본은 모두 Working/으로 복사했습니다. 따라서 도구가 만드는 캐시까지 지정 폴더 안으로 제한하지는 못했습니다. 캐시 원본은 삭제하거나 수정하지 않았습니다.

## 정본과의 차이 및 한계

- 이것은 이미지 모델이 새로 그린 표면입니다. 원본의 종이/먹색 질감, 모서리, 해칭, 나침반을 픽셀 단위로 동일하게 복원한 추출본은 아닙니다. 상태 간 종이 결/먹색 결/나침반 세부도 미세하게 달라집니다. 외곽 크기는 공통 마스크로 맞췄습니다.
- 표의 제목/머리판은 “오른쪽 위” 절단, 로비 버튼은 “왼쪽 아래” 절단이라고 적혀 있지만 확정 PNG는 주로 왼쪽 위와 오른쪽 아래 절단으로 보입니다. 이 불일치는 확정 이미지의 실루엣을 우선했습니다. 원문의 모서리 방향을 문자 그대로 재현한 결과는 아닙니다.
- Panel_Body의 연속 왼쪽 해칭은 9-slice 고정 모서리 규칙 때문에 위/아래 해칭으로 바꿨습니다. 가운데 세로 구간은 반복 가능한 먹색 질감입니다.
- 원본 글꼴을 특정할 수 없어 text_slots 크기/굵기/자간은 화면에서 추정했습니다. 위치는 대체로 ±2px 수준의 광학적 측정이며 글꼴/줄높이에 따라 추가 조정이 필요합니다. font_hint는 Malgun Gothic/Consolas 구현 참고값이며 정본의 확정 글꼴명이 아닙니다. 검토용 GDI 렌더는 자간 값을 별도 적용하지 않습니다.
- 재조립 화면은 #0B111A 위의 UI 배치 검토본입니다. 배경/캐릭터/무기/게임플레이 아이콘은 포함하지 않았습니다. 원본 전체 장면을 완전히 재현한 게임 플레이 캡처가 아닙니다.
- 정본에 없는 전체 상태 확인본도 imagegen 재합성 특성상 상태 밖 배경/캐릭터/문자 픽셀의 완전 보존을 보장하지 않습니다.

## 재현 스크립트

Working/build_layout.cjs → layout.json, Working/export.ps1 + ExportRaster.cs → 키잉/리샘플/등록, Working/contact_sheet.ps1 → Contact_Sheet, Working/final_verify.cjs → 검증, Working/build_readme.cjs → 이 문서. Node 및 Windows PowerShell/.NET System.Drawing으로 실행했습니다. 추가 패키지 설치는 하지 않았습니다.

## 실제 생성 프롬프트

도구는 모든 호출에서 `transparent_background:false`였습니다. 명시한 마젠타 배경으로 생성한 다음 알파를 뽑았습니다. 아래 프롬프트는 요약이 아닌 실제 전달 문자열입니다. 참조 경로는 이 문서 기준으로 표시했습니다.

### 추가 상태 확인본 1

참조: `../OpStatus_Mockup.png`

```text
Use case: precise-object-edit / ui-mockup. Input is the approved 1920x1080 Operation Status screen; retain all scene composition, text, character, panels and exact positions. Create ONE interaction-state proof screenshot for the states absent from the approved normal/hover references, before extracting their textless game assets. Change only these: lobby paper button at [1620,42,234,63] is HOVER, slightly brighter paper and a thin cyan bottom edge; operation-board button at [1394,654,465,42] is HOVER, slightly brighter ink with thin cyan perimeter; deploy button at [1382,861,479,141] is PRESSED, darker matte ink with the identical cyan edge and faint gray compass emblem; first objective checkbox at [1437,434,22,22] is ON, cyan filled with a white check. Same frame and silhouettes as input. No redesign. Raster image model rendering only. This proof is for extracting states; keep exact original text.
```

### 추가 상태 확인본 2 · 로비 눌림

참조: `../OpStatus_Mockup.png`, `../Working/Button_Back_Pressed_magenta.png`

```text
Use case: precise-object-edit / ui-mockup. Create a missing-state review proof of this approved Operation Status scene at the same 1920x1080 composition. Input1 is approved normal scene. Input2 is the already drawn isolated blank pressed lobby-button surface: use its dim cool gray paper appearance as the exact state reference. Change ONLY the lobby button at [1620,42,234,63] into PRESSED state: dim cool-gray fibrous paper, subtle inset top shadow, same silhouette/size/position, same black arrow and Korean 로비 label shifted down 2 screen px. Keep every other original UI, text, character, hangar, weapon and layout exactly unchanged. No redesign. One full screenshot state proof, not a sprite sheet. Raster imagegen only. This complements the existing additional-states proof.
```

### Title_Plate.png

참조: `../OpStatus_Mockup.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. Asset: Title_Plate.png only, screen dimensions 584x108 (wide 5.407:1), export will be 1168x216. Extract the TOP LEFT TITLE PAPER surface of reference, [60,37,584,108]. Off-white lightly fibrous paper with delicate crumpled grain, angled clipped top-left corner and the reference diagonally clipped right lower corner, narrow gray diagonal hatch band only at extreme right edge. Long horizontal title plaque with completely empty center. Remove ALL title/number/English text and the vertical divider; leave their places empty. No thin inset frame. The paper and hatch are the ONLY visible content. Make its exact reference silhouette, not a new design. Large empty center.
```

### Panel_Body.png

참조: `../OpStatus_Mockup.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. The shared panel BODY only (no paper heading). 488:420 aspect ratio. Thin cool-gray 1-screen-pixel edge around opaque near-black #0B111A ink, subtle matte mottled ink texture, square top, clipped diagonal lower-right corner ~22 screen px. Narrow vertical gray diagonal hatching on LEFT outer margin, as in A/B/C/D reference. IMPORTANT for vertical 9-slice: concentrate visible hatch into fixed top-left and bottom-left corner bands, each 44 screen px tall; middle of left margin only quiet ink so stretching does not deform hatching. Middle and center absolutely blank except subtle seamless ink grain. No internal separator, no content. Reference panel B dark body. Output a single isolated sprite, not an asset sheet. Intended final canvas 976x840; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Panel_Header.png

참조: `../OpStatus_Mockup.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. The common off-white PAPER HEADER strip only from A at [56,197,488,60]. Aspect ratio 8.133:1. Very light subtle fibrous off-white paper grain, reference clipped upper-left and lower-right corners about20 screen px, narrow diagonal gray hatch band at extreme right within fixed 40 screen px. Center blank, long continuous paper. All letters/numbers and divider removed. Horizontal 9-slice: every distinctive cut/hatch confined to first/last44 screen px. Normal state, NO cyan. Output a single isolated sprite, not an asset sheet. Intended final canvas 976x120; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Panel_Header_Hover.png

참조: `../OpStatus_Hover.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. Same common off-white PAPER HEADER strip, aspect 8.133:1, identical to normal paper strip from A [56,197,488,60], same clipped corners and narrow right diagonal hatching. HOVER: add ONLY a thin crisp cyan #00CBE8 stripe along its bottom edge following lower-right cut, reference Hover equipment header. Paper lightly fibrous, no text/divider. Every cut/hatch within outer44 screen px. No broad glow. Output a single isolated sprite, not an asset sheet. Intended final canvas 976x120; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Button_Back.png

참조: `../OpStatus_Mockup.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. LOBBY BACK BUTTON paper surface ONLY [1620,42,234,63]. Aspect234:63. Off-white fine paper grain, reference angular diagonal lower-left cut and subtle right-edge cut, matching approved reference silhouette. Empty center. Remove arrow and Korean letters entirely. Normal slightly warm-white paper, no cyan. No hatching unless visible in reference (plain paper button). Output a single isolated sprite, not an asset sheet. Intended final canvas 468x126; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Button_Back_Hover.png

참조: `../Working/Button_Back_magenta.png`, `../OpStatus_Mockup.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. LOBBY BACK BUTTON paper surface ONLY, same234:63 proportions, identical cut corners and silhouette as normal. HOVER: slightly brighter off-white fine paper texture, thin crisp cyan line along bottom edge. No arrow, letters, numbers or decorations. Same design as the lobby button of the additional-states proof. Output a single isolated sprite, not an asset sheet. Intended final canvas 468x126; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Button_Back_Pressed.png

참조: `../Working/Button_Back_magenta.png`, `../OpStatus_Mockup.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. LOBBY BACK BUTTON paper surface ONLY, same234:63 proportions and identical cut corners as normal. PRESSED: dimmer cool-gray paper, subtle inset top-edge shadow, no cyan highlight, no cast shadow. Absolutely no arrow/text/content. Retain paper texture. Output a single isolated sprite, not an asset sheet. Intended final canvas 468x126; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Row_Card.png

참조: `../OpStatus_Mockup.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. ONE DECK CARD ROW SURFACE ONLY, [93,820,429,47], aspect429:47. Opaque near-black matte ink rectangle, very thin neutral cool-gray 1px frame. Subtle fine ink grain, quiet empty center. NO burst, shield, bullet, text, numbers, arrows or divider. Pure surface/frame only. Horizontal9slice with quiet center. Output a single isolated sprite, not an asset sheet. Intended final canvas 858x94; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Op_Card.png

참조: `../OpStatus_Mockup.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. ACTIVE OPERATION CARD normal surface ONLY [1399,313,461,246], aspect461:246. Opaque near-black #0B111A subtly textured ink, EXACT thin 1screenpx cyan perimeter. Four small L corner brackets inset12screenpx, NORMAL brackets slim pale gray (not cyan). Absolutely remove cube, all text, checkboxes, separator lines, reward, content. Blank middle. Brackets confined within24 screenpx corner patches for vertical9slice. Square corners as reference, no big bevels, no glow. Output a single isolated sprite, not an asset sheet. Intended final canvas 922x492; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Op_Card_Hover.png

참조: `../Working/Op_Card_magenta.png`, `../OpStatus_Hover.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. ACTIVE OPERATION CARD HOVER surface ONLY, same461:246 size/silhouette as normal. Slightly brighter opaque slate ink (#172531), crisp thin cyan perimeter and FOUR thicker cyan L brackets inset12px into corners with restrained tiny local cyan glow. All glow confined close to brackets/frame. NO cube/check boxes/text/content or separator. Center uniformly quiet matte ink grain. Brackets remain in24screenpx corner patches for9slice. Same rectangle, no cast shadow outside, no neon broad bloom. Output a single isolated sprite, not an asset sheet. Intended final canvas 922x492; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Op_Slot_Empty.png

참조: `../OpStatus_Mockup.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. EMPTY OPERATION SLOT surface ONLY [1400,576,459,63], aspect459:63. Quiet opaque near-black ink surface (game applies near-transparent opacity later) with thin dashed neutral cool-gray perimeter. Fine restrained grain, no letters/content. Horizontal9slice: visible dashed accents in outer fixed corner strips only, center top/bottom lines very subdued so it stretches quietly. No cyan, no glow. Full rectangular slot. Output a single isolated sprite, not an asset sheet. Intended final canvas 918x126; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Button_Board.png

참조: `../OpStatus_Mockup.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. OPERATION BOARD AUXILIARY BUTTON surface ONLY [1394,654,465,42], very wide465:42. Opaque dark matte ink textured panel, very slim off-white1screenpx outline, top-left tiny angular chamfer and lower-right diagonal cut as in approved reference. Center empty, remove entire Korean label and arrow. Normal state, no cyan. Every cut in fixed30screenpx ends for horizontal9slice. Output a single isolated sprite, not an asset sheet. Intended final canvas 930x84; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Button_Board_Hover.png

참조: `../Working/Button_Board_magenta.png`, `../OpStatus_Hover.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. OPERATION BOARD AUXILIARY BUTTON HOVER surface ONLY. Identical465:42 aspect and corners as normal, reference additional-states proof. Slightly brighter opaque slate ink surface, thin cyan1screenpx perimeter, restrained and clean, no glow. Top-left tiny chamfer and lower-right diagonal cut, fixed30screenpx ends for9slice. NO text or arrow. Preserve quiet ink texture. Output a single isolated sprite, not an asset sheet. Intended final canvas 930x84; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Button_Board_Pressed.png

참조: `../Working/Button_Board_magenta.png`, `../OpStatus_Hover.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. OPERATION BOARD AUXILIARY BUTTON PRESSED surface ONLY, same465:42 and same top-left tiny chamfer/lower-right clipped corner as normal. Approved Hover screenshot board button is pressed: darker opaque matte ink, thin dim off-white perimeter, subtle inset upper-edge shadow. No cyan. NO Korean label or arrow. Horizontal9slice30screenpx ends, empty middle. Output a single isolated sprite, not an asset sheet. Intended final canvas 930x84; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Button_Deploy.png

참조: `../OpStatus_Mockup.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. DEPLOY BUTTON normal surface ONLY [1382,861,479,141], aspect479:141. Opaque matte near-black subtle ink grain, crisp cyan #00CBE8 2screenpx perimeter, thick cyan bottom stripe about9screenpx. Top-left small diagonal chamfer, large lower-right cut ~22screenpx exactly reference. RIGHT SIDE includes a LARGE FAINT GRAY FOUR-POINT COMPASS STAR in a circular arc, lower-contrast engraving, occupying last170screenpx, partly clipped by right/bottom panel boundary just as reference. This compass is essential painted ornament, not text or gameplay icon. Leave LEFT TWO THIRDS blank: remove Korean deploy label AND arrow. Normal no glow. No other ornaments. Output a single isolated sprite, not an asset sheet. Intended final canvas 958x282; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Button_Deploy_Hover.png

참조: `../Working/Button_Deploy_magenta.png`, `../OpStatus_Hover.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. DEPLOY BUTTON HOVER surface only, same479:141 proportions and IDENTICAL top-left and lower-right cuts as normal. Brighter opaque slate-blue ink (#1B303E), crisp cyan2px perimeter and thick cyan bottom stripe about9screenpx, modest localized cyan light bloom along lower edge ONLY over magenta. RIGHT includes exact same LARGE FAINT GRAY FOUR-POINT COMPASS STAR inside circular arc, occupying last170screenpx and partly clipped at right/bottom border. Left two thirds completely blank (no Korean label or arrow). No added decoration, preserve quiet ink grain. Same surface silhouette. Output a single isolated sprite, not an asset sheet. Intended final canvas 958x282; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Button_Deploy_Pressed.png

참조: `../Working/Button_Deploy_magenta.png`, `../OpStatus_Mockup.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. DEPLOY BUTTON PRESSED surface only, same479:141 proportions, identical top-left and lower-right cuts as normal. Darker opaque matte #080E15 ink with subtle inset upper shadow, cyan2px perimeter and thick bottom stripe retained but slightly dimmed. RIGHT includes exact same LARGE FAINT GRAY FOUR-POINT COMPASS STAR in circular arc occupying last170screenpx and clipped at edge. No glow. Left two thirds empty: absolutely NO text or arrow. Same compass/corners as normal. Output a single isolated sprite, not an asset sheet. Intended final canvas 958x282; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Dialogue_Panel.png

참조: `../OpStatus_Mockup.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. DIALOGUE PANEL surface ONLY [581,931,760,101], very wide760:101. Opaque matte ink fine grain, thin cool-gray1screenpx outline, reference short top-left chamfer and lower-right clipped diagonal corner. Extreme right narrow diagonal gray hatch band confined last32screenpx, corners within32screenpx for horizontal9slice. Left top needs EMPTY SPACE where short cyan vertical speaker bar will be rendered by game; do NOT paint the bar. Absolutely NO name/text/dialogue/registration brackets. Center quiet seamless subtle ink grain. No glow. Output a single isolated sprite, not an asset sheet. Intended final canvas 1520x202; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Name_Tag.png

참조: `../OpStatus_Mockup.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. NAME TAG paper strip surface ONLY [586,883,208,31], wide208:31. Off-white fine paper texture, horizontal thin paper ribbon, plain left edge and diagonal cut at right tip, very subtle lower paper shadow painted within silhouette. Empty blank strip, absolutely NO Korean/English text, slash, rule or bracket. Match approved reference narrow tag, not a chunky badge. Output a single isolated sprite, not an asset sheet. Intended final canvas 416x62; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Check_Off.png

참조: `../OpStatus_Mockup.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. ONE UNCHECKED CHECKBOX sprite ONLY, square22:22. Thin off-white/light-gray1screenpx square outline and opaque #101820 ink square interior. Crisp simple tiny square, quiet matte fill, no bevels, no glow, NO check or letters. Isolated on flat magenta. Match the two unchecked objective boxes in reference. No other content. Output a single isolated sprite, not an asset sheet. Intended final canvas 44x44; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

### Check_On.png

참조: `../Working/Additional_States_Proof.png`, `../OpStatus_Mockup.png`

```text
Use case: ui-mockup / precise-object-edit. Create ONE standalone textless raster UI game sprite redrawn from the approved reference image, same shape, aspect ratio, fine painted paper/ink texture and cut corners. Front-on orthographic, zero rotation. The requested piece alone centered, filling 96% of image width with small magenta margin, no cast shadow unless requested. Background MUST be perfectly flat pure #FF00FF magenta, no lighting/gradient/noise outside the sprite. Never use magenta or purple inside the sprite. No text, numbers, letters, arrow glyphs, characters, weapons, background scene, gameplay icons, bars, graphs, separator rules, registration crosses or readouts. Surface painting by image model, no SVG, no geometric substitute. All dark panel interiors fully OPAQUE matte ink near #0B111A; game controls opacity later. Exact slim edges, subtle texture, no grunge, no holograms. Preserve the reference design; do not add ornaments. ONE CHECKED CHECKBOX sprite ONLY, square22:22. Thin off-white1screenpx square outline, opaque restrained cyan #00CBE8 filled interior and ONE crisp WHITE check mark. The check mark is allowed for this asset; no letters or numbers. Match unchecked checkbox proportions, same square silhouette. No bevel or broad glow. Isolated on flat magenta. Reference additional-states proof cyan checked objective. Output a single isolated sprite, not an asset sheet. Intended final canvas 44x44; preserve stated visible aspect ratio even if you use a larger magenta canvas.
```

