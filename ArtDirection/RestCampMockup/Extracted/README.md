# 휴식 야영지 UI 분절 납품

`SPLIT_REQUEST.md`와 `../UI_Mockup_Split_Workflow.md`의 4단계에 따른 UI 전용 분절입니다. 정본 세 장을 읽고 참조했습니다. **SVG는 생성·사용하지 않았습니다.** 표면, 그림, 별, 화살표, 자물쇠는 모두 내장 **imagegen**으로 개별 생성했습니다. 예외는 요청서에서 허용한 2px 세로 구분선 하나뿐입니다.

## 파일

| 파일 | PNG 크기(px) | 알파 | 크기 방식 |
|---|---|---|---|
| Menu_Panel.png | 1280×1510 | binary | 고정 |
| Menu_Star.png | 126×134 | soft | 고정 |
| Menu_TitleDivider.png | 4×130 | binary | 고정 |
| Row_Normal.png / Row_Hover.png / Row_Locked.png | 각 1236×348 | binary | 고정, 동일 알파 |
| Art_Rest.png | 400×260 | soft | 고정 |
| Art_Upgrade.png | 320×276 | soft | 고정 |
| Art_Return.png | 392×290 | soft | 고정 |
| Glyph_Arrow.png | 60×50 | soft | 고정, 흰색 |
| Glyph_Lock.png | 62×78 | soft | 고정, 흰색 |
| Tag_Fire.png | 400×136 | binary | 고정 |
| Tag_Name.png | 388×80 | binary | 가로 9-slice |
| Tag_Activity.png | 336×56 | binary | 가로 9-slice |

총 14개 스프라이트 PNG입니다. `layout.json`에는 1920×1080 좌상단 좌표, 실제 PNG 크기, 각 조각의 알파, PNG px 단위 9-slice 경계(L,B,R,T), 세 행동 칸의 위치와 패널 기준 오프셋, 칸 내부 그림/화살표/자물쇠 오프셋, 17개 런타임 텍스트 자리의 좌표·정렬·화면 px 크기·굵기·상태별 색·자간을 기록했습니다. 화살표와 자물쇠는 게임에서 색을 입히는 흰색 RGB입니다.

`Contact_Sheet.jpg`는 조각별 체커보드 확인, 세 정본과 같은 좌표의 재조립 비교, 두 이름표의 원래 너비/320px/420px 가로 늘림을 포함합니다. `QA/Reassembled_Default.png`, `QA/Reassembled_States.png`, `QA/Reassembled_Labels.png`는 비교용 1920×1080 이미지이며, 게임용 스프라이트가 아닙니다. QA 그림에만 예시 글자와 원본 야영지/HUD가 있습니다. 실제 14개 조각에는 글자·숫자·야영지·HUD·지시선이 없습니다.

## 처리와 조립

정본의 야영지를 잘라 붙이는 대신 UI만 독립적으로 재생성했습니다. 생성 래스터를 크롭/리샘플하고 투명도를 정리했으며, 코드로 판이나 일러스트를 새로 그리지 않았습니다. 패널과 태그의 주 알파 연결 성분을 추출하고 외곽은 0/255로 정리했습니다. 별과 세 일러스트는 모델의 부드러운 알파를 유지했습니다. 모든 조각은 화면 1px=PNG 2px, 회전 0°입니다. 패널 생성 원본은 1155×1362였고 최종 1280×1510으로 리샘플했습니다. 2배 PNG라는 표시는 최종 납품 치수입니다.

세 행에는 같은 `Row_Normal` 알파 마스크를 적용했습니다. 상태별 종이 밝기는 정본의 글자 없는 샘플 영역(x1580..1729, 각 행 y+154..164)을 기준으로 생성 래스터에 RGB 배율을 적용해 맞췄습니다: 기본 0.96228018, 올림 0.82282154, 잠김 1.08760539. 별도 런타임 보정 없이 PNG 자체로 사용합니다. 최초 흰 글리프 생성에 과한 후광이 있어 재생성했습니다. 최종 글리프는 모델 래스터의 고알파 코어(>=200)를 추출한 뒤 축소 안티앨리어싱과 흰색 정규화만 적용했습니다. 새 윤곽을 코드로 그리지 않았습니다.

이름표만 가로로 늘립니다. `Tag_Name`의 경계는 **[96,0,70,0] PNG px**, `Tag_Activity`는 **[32,0,32,0] PNG px**입니다. 높이는 각각 화면 40px/28px로 유지합니다. 청록 막대는 왼쪽 고정 영역에 들어갑니다. 중앙에는 늘릴 수 있는 종이 질감과 가로 테두리만 남깁니다. 다른 조각은 9-slice를 사용하지 않습니다.

기본/올림에서 일러스트는 흰색 곱, 불투명도 1입니다. 잠김은 **곱할 색 #B8B8B8, 불투명도 0.43**으로 같은 그림을 잠김 행 위에 source-over 합성합니다. `layout.json`에 이 값과 화살표/자물쇠 상태별 색을 기록했습니다.

장면 어둡기는 원래 `PROMPTS.txt`에 명시된 **RGB ×0.70**입니다(검정 덮개 불투명도 0.30과 동등). 213,363개 비교 채널 표본의 평균 비율은 0.69921, 0.70 내림 계산과의 평균 절대 차이는 8비트 기준 0.41446입니다. 원본의 정수 반올림·합성 차이를 포함합니다. 상단 y0..109 HUD는 제외하고, UI를 얹기 전에 장면에 적용합니다.

## 검증 결과와 한계

- 요청된 14개 파일 모두 존재하고 PNG 크기는 `screen_rect`의 정확히 2배입니다.
- binary로 지정한 8개 파일은 중간 알파가 0개입니다. 전체 알파 값은 0/255 범위에만 있습니다.
- 세 행은 1236×348의 같은 캔버스이며, 기본 대비 올림/잠김 알파 불일치 픽셀은 각각 **0개**입니다.
- 두 글리프의 보이는 모든 픽셀 RGB는 **255,255,255**입니다.
- `validation.json`에 각 파일의 투명/불투명/soft 픽셀 수와 크기 검증을 기록했습니다.
- 최종 Contact Sheet, 상태 재조립, 태그 늘림을 시각 확인했습니다. PNG에는 런타임 글자를 넣지 않았습니다.
- 정본을 참조해 재생성한 그림이므로 픽셀 단위의 동일 복제는 아닙니다. 원본 글꼴 정보가 없어 텍스트의 크기·굵기·자간·색과 잠김 곱/불투명도는 시각 추정값입니다. QA 예시는 Malgun Gothic을 사용하며 프로젝트 글꼴 지정은 포함하지 않습니다.
- 파일 납품 중 미완료 항목은 없습니다. Unity 적용은 요청 범위에 포함되지 않습니다. `Assets/`, `Packages/`, `ProjectSettings/`를 수정하지 않았으며 git을 사용하지 않았습니다. 직접 작성한 파일은 모두 `ArtDirection/RestCampMockup/` 안에 있습니다. imagegen 자체의 자동 저장 원본만 도구 기본 저장 위치에 남습니다.

## 생성 프롬프트

아래는 내장 imagegen에 실제 제출한 프롬프트입니다. 각 호출은 단일 조각/상태를 생성했고 `transparent_background: true`를 사용했습니다. 패널·기본 행·세 일러스트·별·화살표는 기본 정본을, 태그는 Labels 정본을, 자물쇠는 States 정본을 참조했습니다. 행 상태 두 개는 생성된 기본 행을 편집 대상으로, States 정본을 색 참고로 사용했습니다. 재생성 표시는 최초 글리프 대신 채택한 호출입니다.

### 1. Menu_Panel.png

```text
Use case: ui-mockup. Asset type: Menu_Panel.png, standalone production raster game UI component.
Input reference: approved Rest_Menu_Default.png, ONLY the charcoal menu board at x1208 y150 w640 h755 is style/shape reference.
Redraw ONLY the OUTER CHARCOAL BOARD on a genuinely transparent background, front-on, rotation 0. Portrait aspect ratio exactly 640:755, tightly framed with a tiny transparent margin. High resolution at least 1280x1510. Preserve its matte charcoal ink surface with restrained fine tonal depth, clipped corners, double thin grey perimeter ink lines, four tiny registration crosses near the corners, sparse subtle diagonal drafting lines confined to upper-right and lower-right fixed corners. At y104/755 of board height a thin muted grey header underline runs from x160 to x545, with a short muted turquoise segment at its left. A similar thin footer progress line at y724 from x68 to x518 has a short teal initial segment. Match approved board's dim grey/charcoal luminosity.
The board interior MUST be EMPTY uninterrupted dark charcoal. Remove all three paper action rows entirely. Remove the star emblem and vertical title divider. Remove ALL text, letters, Korean, numbers, icons, illustrations. No row-shaped recesses or placeholder slots. No transparency INSIDE the board. Outside silhouette fully transparent. No camp pixels, scene, HUD, scenery, characters, glow, shadow extending beyond silhouette, SVG, vector substitute, grunge. This is one image-painted raster panel, not an asset sheet.
```

### 2. Tag_Fire.png

```text
Use case: ui-mockup. Asset type: isolated painted raster game UI sprite. Use approved reference ONLY for matching shape/style; redraw just the requested component on genuinely transparent background with no camp/scenery contamination. Zero rotation, front-on, one tightly framed object, high resolution. ALL text, letters, numbers, Korean, English must be absent. No SVG, neon, glow, grunge, new ornaments, drop shadow outside contour, extra objects or asset sheet. Paint ONLY the EMPTY two-line campfire guidance plaque from reference around x858 y332, aspect 200:68. Matte nearly-black charcoal clipped-corner horizontal panel, subtle cool ink-paper tonal texture, thin dark grey perimeter, tiny subdued registration corner flecks, thin muted teal horizontal divider through middle from x26 to174 of width200. Top half blank for a title, bottom half blank for subtitle. The center teal rule is included. Fully opaque inside, alpha outside. No pointer, no star, no upright divider.
```

### 3. Tag_Name.png

```text
Use case: ui-mockup. Asset type: isolated painted raster game UI sprite. Use approved reference ONLY for matching shape/style; redraw just the requested component on genuinely transparent background with no camp/scenery contamination. Zero rotation, front-on, one tightly framed object, high resolution. ALL text, letters, numbers, Korean, English must be absent. No SVG, neon, glow, grunge, new ornaments, drop shadow outside contour, extra objects or asset sheet. Paint ONLY the EMPTY upper companion NAME band from reference at x143 y350 w194 h40, aspect194:40. Matte ink charcoal with thin restrained grey outline, symmetrically clipped short corners, tiny muted teal horizontal accent at top-left, approx x18..48. Opaque inside. Designed for HORIZONTAL 9-slice: ALL distinct details in fixed left/right ends <=35/194 width, middle only uniform subtle charcoal paper texture with continuous horizontal borders. No middle divider, no star, no lower activity plaque, NO downward pointer or connector. Blank center.
```

### 4. Tag_Activity.png

```text
Use case: ui-mockup. Asset type: isolated painted raster game UI sprite. Use approved reference ONLY for matching shape/style; redraw just the requested component on genuinely transparent background with no camp/scenery contamination. Zero rotation, front-on, one tightly framed object, high resolution. ALL text, letters, numbers, Korean, English must be absent. No SVG, neon, glow, grunge, new ornaments, drop shadow outside contour, extra objects or asset sheet. Paint ONLY the EMPTY lower companion activity band from reference at x157 y390 w168 h28, aspect168:28. Quiet matte charcoal grey surface, thin dim grey outline, short clipped bottom corners, top corners square, opaque inside. Designed for HORIZONTAL 9-slice, distinctive details ONLY in left/right fixed end zones <=16/168 width; middle uniform subtle paper texture. No teal bar, no text, no name plaque, no downward pointer, leader or connector.
```

### 5. Menu_Star.png

```text
Use case: ui-mockup. Asset type: isolated painted raster game UI sprite. Use approved reference ONLY for matching shape/style; redraw just the requested component on genuinely transparent background with no camp/scenery contamination. Zero rotation, front-on, one tightly framed object, high resolution. ALL text, letters, numbers, Korean, English must be absent. No SVG, neon, glow, grunge, new ornaments, drop shadow outside contour, extra objects or asset sheet. Paint ONLY the slim four-point compass STAR beside approved menu title. Shape matches reference x1257 y190 w63 h67: elongated vertical needle, slightly shorter horizontal needle, four main needle points, small secondary thin diagonal rays near center. Soft grey ink edges with brighter grey center, very restrained pencil-like material, dark fine facets. No surrounding circle, no board, no letters. Transparent between rays.
```

### 6. Glyph_Arrow.png

```text
Use case: ui-mockup. Asset type: isolated painted raster game UI sprite. Use approved reference ONLY for matching shape/style; redraw just the requested component on genuinely transparent background with no camp/scenery contamination. Zero rotation, front-on, one tightly framed object, high resolution. ALL text, letters, numbers, Korean, English must be absent. No SVG, neon, glow, grunge, new ornaments, drop shadow outside contour, extra objects or asset sheet. Draw ONLY a simple thin right-pointing arrow glyph matching approved row arrow around x1780 y370 w30 h25. Horizontal shaft and two diagonals open arrow head. Monochrome pure WHITE RGB255,255,255, with alpha antialiasing. No grey shading, texture, black stroke, backdrop or glow. Tight framing with margin.
```

### 7. Glyph_Lock.png

```text
Use case: ui-mockup. Asset type: isolated painted raster game UI sprite. Use approved reference ONLY for matching shape/style; redraw just the requested component on genuinely transparent background with no camp/scenery contamination. Zero rotation, front-on, one tightly framed object, high resolution. ALL text, letters, numbers, Korean, English must be absent. No SVG, neon, glow, grunge, new ornaments, drop shadow outside contour, extra objects or asset sheet. Draw ONLY the small CLOSED PADLOCK glyph from locked bottom row x1776 y725 w31 h39. Rounded shackle with hollow transparent hole, compact squarish body, tiny transparent keyhole. Pure WHITE RGB255,255,255 everywhere the glyph exists, antialiased alpha only. No grey shading, black stroke, outline box, background or glow.
```

### 8. Row_Normal.png

```text
Use case: ui-mockup. Draw ONE standalone horizontal production raster UI paper action band from the approved Rest_Menu_Default.png. Exact aspect 618:174, high resolution at least 1236x348, front-on zero rotation. Genuinely transparent outside silhouette, fully opaque inside. Matte low-luminance grey paper surface matching the approved x1218 y294 w618 h174 row, faint tactile paper grain, clean top-left clipped corner approx20px screen and bottom-right clipped corner approx20px. Top-right and bottom-left square. Thin grey perimeter rule. Fine dark vertical divider at x257/618, from y28/174 to146/174, separating empty illustration area and empty text area. One very faint diagonal drafting line in right part from upper right toward x450,y150. EMPTY otherwise. No text, numbers, letters, arrows, illustrations, icons, star, mug, tools, menu board, scene, camp pixels, glow, SVG. Single painted image, not an asset sheet.
```

### 9. Art_Rest.png

```text
Use case: background-extraction / ui-mockup. Production raster game UI asset, genuinely transparent background, zero rotation, front-on orthographic. Approved Rest_Menu_Default.png is a STYLE AND SUBJECT reference only. Paint the requested isolated component at high resolution. Match the approved muted greyscale pencil/ink illustration with soft painterly shading and controlled low-luminance grey paper tones. Absolutely NO text, letters, numbers, labels, HUD, camp scenery, characters, menu board, paper card rectangle, frame or backdrop. Keep the subject and all faint pencil contact-shadow strokes on true transparent alpha, no white/grey matte or opaque rectangle. Tight bounding box with a small clear margin. No SVG or procedural vector. Subject: the approved left-row illustration of a folded thick grey checkered wool camping blanket and an enamel metal camping mug placed in front left, mug handle right, tiny four-point compass emblem on the mug but no text. Exactly their approved arrangement, 200:130 overall aspect. Illustrated graphite contact shadows may be soft, no broad background swatch. Same medium grey tonal range as the original artwork on its grey card.
```

### 10. Art_Upgrade.png

```text
Use case: background-extraction / ui-mockup. Production raster game UI asset, genuinely transparent background, zero rotation, front-on orthographic. Approved Rest_Menu_Default.png is a STYLE AND SUBJECT reference only. Paint the requested isolated component at high resolution. Match the approved muted greyscale pencil/ink illustration with soft painterly shading and controlled low-luminance grey paper tones. Absolutely NO text, letters, numbers, labels, HUD, camp scenery, characters, menu board, paper card rectangle, frame or backdrop. Keep the subject and all faint pencil contact-shadow strokes on true transparent alpha, no white/grey matte or opaque rectangle. Tight bounding box with a small clear margin. No SVG or procedural vector. Subject: the approved crossed camping hammer and open-ended wrench, diagonal X arrangement, hammer head upper-left, wrench jaw upper-right, handles crossed below, dark graphite shading, metal highlights, exact overall arrangement from middle row. Approx overall aspect 160:138. No extra tool.
```

### 11. Art_Return.png

```text
Use case: background-extraction / ui-mockup. Production raster game UI asset, genuinely transparent background, zero rotation, front-on orthographic. Approved Rest_Menu_Default.png is a STYLE AND SUBJECT reference only. Paint the requested isolated component at high resolution. Match the approved muted greyscale pencil/ink illustration with soft painterly shading and controlled low-luminance grey paper tones. Absolutely NO text, letters, numbers, labels, HUD, camp scenery, characters, menu board, paper card rectangle, frame or backdrop. Keep the subject and all faint pencil contact-shadow strokes on true transparent alpha, no white/grey matte or opaque rectangle. Tight bounding box with a small clear margin. No SVG or procedural vector. Subject: the approved simple weathered wooden direction sign with a short grey arrow painted on it, on a single post with a tiny grass tuft, and a trail of five receding boot prints curving from bottom-left to sign. Match third row arrangement, approx aspect 196:145. No words on sign. Use normal enabled artwork tones, not locked state. No ground plane, just the individual illustration strokes and tiny soft contact shadows on alpha.
```

### 12. Row_Hover.png

```text
Use case: precise-object-edit. Input image 1 is the textless Row_Normal raster edit target. Image2 approved state mockup is state color reference only. Change ONLY the row surface state. Preserve EXACT same canvas dimensions, exact outer silhouette, clipped top-left and bottom-right corners, thin perimeter, divider position x257/618 and length, subtle upper-right diagonal drafting line. Same paper texture and layout, front-on 0 rotation. True transparent exterior, opaque interior. No text, numbers, art, icons, glyphs, scenery, background, glow, SVG. One empty row, tight frame. Make a HOVER state like first row in reference2: slightly lighter cool blue-grey matte paper, narrow muted teal vertical strip on left edge and restrained thin teal inset outline along all borders. No other change. The center and right of row remain completely empty.
```

### 13. Row_Locked.png

```text
Use case: precise-object-edit. Input image 1 is the textless Row_Normal raster edit target. Image2 approved state mockup is state color reference only. Change ONLY the row surface state. Preserve EXACT same canvas dimensions, exact outer silhouette, clipped top-left and bottom-right corners, thin perimeter, divider position x257/618 and length, subtle upper-right diagonal drafting line. Same paper texture and layout, front-on 0 rotation. True transparent exterior, opaque interior. No text, numbers, art, icons, glyphs, scenery, background, glow, SVG. One empty row, tight frame. Make a LOCKED state like bottom row in reference2: dark opaque matte ink-charcoal/graphite paper, subtle grey perimeter, reduced-contrast vertical divider and faint diagonal. No teal accent. The center and right of row remain completely empty. No lock icon (separate asset).
```

### 14. Glyph_Arrow.png (재생성, 최종 채택)

```text
Create one crisp flat raster UI glyph: thin right-pointing arrow with straight horizontal shaft and two diagonal arrowhead strokes, matching the small arrow in reference's action row. Pure white ink silhouette on genuinely transparent background. This is a technical monochrome ALPHA MASK, not a light source: absolutely no glow, halo, blur, shadow, bloom, shading, grey cloud, 3D or material. Uniform solid white interiors with only one-pixel antialiasing at contour. No luminous effect. No other visible pixels. Thin squared-ended strokes, no round glowing tubes. Tight isolated shape, arrow aspect30:25, straight front-on. No text. Do not include menu or scene.
```

### 15. Glyph_Lock.png (재생성, 최종 채택)

```text
Create one crisp flat raster UI CLOSED PADLOCK icon matching the tiny locked bottom-row icon in reference. Pure white ink on genuinely transparent background. Compact rounded rectangular body, thick arch shackle with completely TRANSPARENT empty space beneath arch, and completely TRANSPARENT small keyhole in body. This is a technical monochrome ALPHA MASK, not a light source: absolutely no glow, halo, blur, shadow, bloom, shading, grey cloud, 3D or material. Uniform solid white interior, only one-pixel antialiasing at contour. No luminous effect. No other visible pixels. Tight isolated shape, lock aspect31:39, front-on. No text. Do not include menu or scene.
```

