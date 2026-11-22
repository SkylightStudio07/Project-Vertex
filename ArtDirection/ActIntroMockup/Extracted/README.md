# 막 개시 연출 — 개별 재생성 조각 납품

정본 A/B/C와 `SPLIT_REQUEST.md`, 기존 `README.md`, `PROMPTS.txt`, `../UI_Mockup_Split_Workflow.md`의 분절 단계를 읽고 제작했습니다. **표면과 모든 그림은 내장 `image_gen.imagegen`으로 생성했습니다.** SVG, 코드로 그린 판, 도형 조립은 사용하지 않았습니다.

## 납품

| 파일 | 크기 | 처리 |
|---|---|---|
| Plate_Off.png | 1200×800 | 가장 밝은 석고·종이 판, 작은 십자 5개, 이진 알파 |
| Plate_Boss.png | 1200×800 | Off를 참조한 같은 판, 중앙 조준 원과 십자 제도선, 이진 알파 |
| Plate_Floor01.png | 1200×800 | 백피안화·S자 돌길·석조 잔해, 꽃 외곽만 soft |
| Marker_Vertex.png | 256×384 | 흰·은회색 팔면체 결정, 이진 알파 |
| Backdrop.png | 1920×1080 | 판·축·표식·글자가 없는 청회색 밤공기, 완전 불투명 |
| layout.json | — | 최종 PNG에서 측정한 꼭짓점, 크기, 알파, 조립 좌표 |
| Contact_Sheet.jpg | 2800×2040 | #1A2636 위 단독 조각과 190px 피치 6층 조립 |
| README.md | — | 처리·검증·프롬프트 기록 |

판 스택은 Off 6장을 놓은 뒤 위·아래를 교체한 **총 6장(Boss 1 + Off 4 + Floor01 1)**입니다. 표식은 위에 얹었습니다. 조립은 납품 PNG 원본 크기, 피치 190px로 했습니다. 게임용 추가 빛·안개·감광은 넣지 않았습니다. 단독 조각 미리보기만 표시 공간에 맞춰 축소했습니다.

## 실제 처리

1. A의 밝은 아래쪽 판을 참조하여 Off를 단독 재생성했습니다. Boss는 생성된 Off를 편집 대상으로 삼아 중앙 제도선만 추가하도록 요청했습니다. Floor01은 C의 디오라마와 새 Off 몸체를 함께 참조했습니다. 결정과 바탕도 A를 참조하여 각각 생성했습니다.
2. 판 원본은 각각 1536×1024, 결정 원본은 1024×1536, 바탕 원본은 1672×941입니다. 투명 조각은 요청대로 **불투명 순마젠타 배경 위에서 생성**했습니다. 이미지 모델 원본의 미세한 배경 편차는 키잉으로 제거했습니다.
3. 판 전체를 **균일 배율 확대·축소와 평행 이동**으로 1200×800 캔버스에 배치했습니다. 꼭짓점별 워프, 표면 교체, 다각형 마스크로 외곽을 다시 그리는 작업은 하지 않았습니다. Off/Boss의 원본→납품 배율은 0.8095952, 이동은 (-20.15, 8.04)px입니다. Floor01은 0.8065721, (-19.45, 6.11)px입니다. 결정은 0.2553192, (-2.7234, -2.9362)px입니다. 바탕은 1920×1080으로 리샘플했습니다.
4. 요청한 거리 `d=max(|R−255|,G,|B−255|)`를 적용했습니다. 일반 부분은 `d≥96`에서 알파 255, 나머지 0입니다. 후방 왼쪽 경계 밖으로 솟은 흰 꽃·수술 가장자리에만 `clamp((d−40)/120,0,1)`을 적용했습니다. 돌·판·잎과 줄기는 이진입니다.
5. 꽃 가장자리 RGB는 `(observed−(1−alpha)×[255,0,255])/alpha`로 복원한 straight alpha입니다. 이진 경계 1px의 RGB는 안쪽 이웃 평균으로 교체했습니다. 얇은 돌출부는 가까운 내부 이웃까지 탐색했습니다. 잔여 마젠타 성분(`R>G+6 && B>G+6`)은 R을 G까지 낮춰 제거했습니다. 알파 0의 RGB도 0으로 정리했습니다.
6. 투명 조각의 바깥 4px는 알파 0입니다. 전체 캔버스가 배경인 Backdrop은 예외이며 모든 픽셀의 알파가 255입니다.
7. 코드 사용은 리샘플·키잉·RGB 정리·측정·검증·미리보기 합성 및 설명 라벨에 한정했습니다. 조각 그림 자체와 제도선·결정·구름은 모두 이미지 모델이 그렸습니다.

원본, 키잉 직전 중간본, 실제 프롬프트 JSON, 재현용 처리 스크립트, 확대 모서리 검수 이미지와 검증 JSON은 `../Working/Split/`에 있습니다. 기존 목업과 Working 파일은 보존했습니다.

## 실제 측정값

아래 값은 목표값을 복사한 것이 아니라 **최종 PNG 픽셀에서 측정한 좌표**입니다. 원점은 왼쪽 위입니다. 뒤쪽은 윗면 실루엣의 중앙 꼭짓점, 좌·우는 가장 바깥 실루엣 열의 위·아래 끝, 앞쪽 윗면은 중앙의 명암 전환, 앞쪽 아래는 실루엣 최하단을 기준으로 했습니다. 래스터 꼭짓점은 약 ±1px, 좌·우 끝 열은 1~2px 판단 차이가 가능합니다.

| 판 | 뒤 | 왼쪽 | 오른쪽 | 앞 | 아래 왼쪽 | 아래 앞 | 아래 오른쪽 |
|---|---|---|---|---|---|---|---|
| Off | (600,139) | (60,410) | (1140,411) | (600,678) | (60,441) | (600,711) | (1140,441) |
| Boss | (600,139) | (61,410) | (1140,411) | (600,678) | (61,442) | (600,711) | (1140,440) |
| Floor01 | (600,139) | (61,408) | (1140,408) | (600,678) | (61,440) | (600,709) | (1140,440) |

결정의 위·아래 꼭짓점은 **(127,42), (127,341)**이며 그 중점은 **(127,191.5)**입니다. 원래 목표 중심 (128,192)에서 약 1px 차이가 납니다.

## 검증 결과와 남은 차이

- PNG 5장 모두 **8비트 RGBA**, 지정 캔버스 크기, 회전 0°입니다.
- Off/Boss/Marker의 알파는 0/255뿐입니다. Floor01의 중간 알파 픽셀은 **2,898개**로 꽃 가장자리에 한정했습니다. Backdrop은 전 픽셀 알파 255입니다.
- 투명 조각 4장의 바깥 4px에서 알파가 0이 아닌 픽셀은 **0개**입니다. 실루엣은 캔버스에 잘리지 않았습니다.
- #1A2636 위 단독·조립 그림 및 5배 모서리 확대를 확인했습니다. 분홍·보라 테두리는 보이지 않았고, 위의 마젠타 성분 검사 후보도 **0개**입니다. 이 수치는 해당 RGB 판정 기준의 검사 결과입니다.
- 조각에 글자·숫자·라벨·다른 판·축·시안 테두리 빛·섬광·후광·안개가 없습니다. 바탕에만 요청된 낮은 안개가 있습니다.
- 세 판은 폭 약 1080px, 윗면 세로 약 539px로 2:1에 근접합니다. **정확한 2:1, 완전히 같은 픽셀 외곽·질감·명암은 달성하지 못했습니다.** 모델의 개별 재생성으로 실루엣이 1~3px 다르고, 중앙 두께는 Off/Boss 약 33px, Floor01 약 31px입니다. 게임에서 정렬할 때는 layout.json의 실제 측정값을 사용하세요.
- Boss의 조준 원·선과 디오라마의 세부 배치는 정본을 참조한 재생성입니다. 원본 목업에서 잘라낸 픽셀과 동일하지 않습니다.
- 모든 사용자 작업 파일 쓰기는 `ArtDirection/ActIntroMockup/` 안에서만 했습니다. Assets/, Packages/, ProjectSettings/는 수정하지 않았고 git을 사용하지 않았습니다. 내장 이미지 도구 자체의 기본 생성 경로에서 나온 파일은 Working/Split로 복사했습니다. Unity 임포트·게임 적용은 이 요청 범위에 포함하지 않았습니다.

## 실제 생성 프롬프트

도구는 모두 내장 `image_gen.imagegen`, `transparent_background=false`입니다. 호출 5회 결과를 모두 채택했습니다. 영문 프롬프트에 적은 크기는 생성 지시이며, 도구 반환 원본 크기와 최종 리샘플 처리는 위에 따로 기록했습니다.

### Plate_Off

입력: ActIntro_A_Stack.png (reference)

```text
Use case: stylized-concept. Create ONE isolated game sprite Plate_Off, a standalone redraw of ONE BRIGHT LOWEST PLASTER FLOOR SLAB from input reference ActIntro_A_Stack.png. The input is visual reference only, NOT a scene to reproduce. SINGLE PLATE ONLY, NO STACK, NO background atmosphere.
Canvas intended 1200x800, landscape 3:2. Perfectly flat solid pure chroma magenta #FF00FF background with NO gradient, shadow, reflected magenta or magenta in the object. Fully opaque artwork for later chroma keying.
Orthographic parallel projection, no perspective. The complete top face is a 2:1 diamond. On the intended 1200x800 canvas top-face vertices: REAR (600,140), LEFT (60,410), RIGHT (1140,410), FRONT (600,680). Side wall is 32px vertically tall: lower LEFT (60,442), lower FRONT (600,712), lower RIGHT (1140,442). Preserve plentiful margins. These are spatial instructions, DO NOT PRINT coordinates.
One bright matte off-white plaster / thick paper slab, subtle realistic fine paper-plaster grain like lower slabs in reference, thin graphite perimeter and delicate inset diamond border. Exactly FIVE tiny pale graphite registration crosses: one toward each of the four corners and one at center. No other symbols. Strongest brightness on top, left-front sidewall slightly darker, right-front sidewall darkest, restrained realistic overhead lighting; bright un-fogged un-dimmed state. Real tangible solid thickness. Very fine detail, quiet architectural miniature raster painting.
No text, numbers, labels, cyan, colored edge light, glow, flare, fog, axis, crystal, other plates, cast shadow outside slab, large target circle, grids, glass, metal, ornamental motifs. Render the surface with image-model art, not a vector diagram. Only one isolated fully visible slab on uniform #FF00FF.
```

### Plate_Boss

입력: Working/Split/Plate_Off_source.png (edit target); ActIntro_A_Stack.png (reference)

```text
Use case: precise-object-edit. Create Plate_Boss from image1 (Plate_Off_source.png, EDIT TARGET). Image2 (ActIntro_A_Stack.png) is REFERENCE only for the subtle target drafting motif on its highest slab.
Change ONLY the graphite marks in the central area of image1's TOP SURFACE: add a large thin faint graphite TARGET CIRCLE projected onto the slab plane (therefore a 2:1 ellipse on screen), with fine horizontal and front-to-back drafting crosshair lines intersecting exactly at the existing central registration cross. Circle diameter about 300px horizontally and150px vertically on intended1200x800 canvas. Lines thin and pale, like reference. Retain exactly five tiny registration crosses, delicate inset diamond border, fine paper-plaster grain.
KEEP the entire silhouette, position, dimensions, outer edges, slab thickness, lighting, bright offwhite top, left/right sidewall shading and pure magenta #FF00FF background IDENTICAL to image1. DO NOT change camera or crop. Same landscape3:2 framing. One complete isolated plate only. No cyan light, glow, mist, flare, text, numbers, coordinates, labels, additional plate or shadow outside object. This is a very minimal central drafting-mark edit of image1, not a redesign.
```

### Plate_Floor01

입력: ActIntro_C_Floor01.png (reference); Working/Split/Plate_Off_source.png (body reference)

```text
Use case: stylized-concept. Create ONE isolated game sprite Plate_Floor01. Image1 ActIntro_C_Floor01.png is DESIGN REFERENCE for its white spider-lily garden miniature, S-curved stone path and small stone ruins; NOT a scene to reproduce. Image2 Plate_Off_source.png is BODY/GEOMETRY REFERENCE: use its same standalone slab shape, dimensions and plaster sidewalls.
Canvas intended1200x800, landscape3:2. Background MUST be flat pure chroma magenta #FF00FF, no gradient, shadow or magenta spill. Single fully visible isolated diorama slab, no other plates, typography, axis or background atmosphere.
Orthographic no perspective. Complete TOP DIAMOND vertices intended rear(600,140), left(60,410), right(1140,410), front(600,680). 2:1 ratio1080wide540high, edge slope26.565deg. Sidewall32px high lower corners left(60,442), front(600,712), right(1140,442). Same offwhite thick paper-plaster material as image2. Top brightest, left-front sidewall darker, right-front sidewall darkest. No outside shadow.
On the top face a delicate cold moonlit miniature landscape matching image1: dense clusters of WHITE SPIDER LILIES / white Lycoris with recurved thin petals and long curling filament stamens on short dark green stems. Small winding S-shaped grey paved stone path leading from front edge through middle to rear. Tiny low broken block wall on left and modest ruined stone posts toward back right. Loose tiny rubble and petals. Maintain the reference distribution and refined miniature scale, restrained blue-grey cold moonlight. All plant roots and ruins stand strictly within diamond footprint; raised flowers/ruins may extend upward into y20-140 but stay inside canvas with margins. Use all full1200x800 framing without cropping. Diorama fits on the slab, not floating separately.
Absolutely NO cyan border illumination, NO central star glint or flare, NO glow, no halo, no fog, no neon, no text or numbers, no crystal, no other plates. Perimeter is unlit plain graphite/material edge. Bright solid plaster sidewalls remain visible; dark slate-blue garden top. Beautiful detailed raster miniature painting on pure solid magenta background for later keying, no magenta in the object.
```

### Marker_Vertex

입력: ActIntro_A_Stack.png (reference)

```text
Use case: stylized-concept. Create ONE isolated Marker_Vertex game sprite. Input ActIntro_A_Stack.png is DESIGN REFERENCE ONLY for its very small floating crystal above the stack.
Only a vertically elongated OCTAHEDRON crystal, white and silver-grey matte faceted surfaces, differing light/dark planar faces clearly showing solid3D form. Faithfully match that simple elegant geometric crystal shape: sharp upper and lower tips, broad equatorial left/right corners, tapered elongated upper and lower pyramids. NOT a cube, gemstone cluster, ornate object or outline symbol. Cool overhead lighting, top-facing facets pale white, lower facets soft steel silver-grey, no colored glow, no cyan, no transparency or refractive glass. The crystal itself is fully opaque.
Intended final canvas256x384 portrait2:3. Crystal CENTER at(128,192), upper tip(128,42), lower tip(128,342), height300px, equatorial left(42,192), right(214,192), about172px wide. Preserve clean magenta margin all around; these are composition coordinates, do not print them.
Flat solid pure chroma magenta #FF00FF background ONLY, no gradient, no shadows, no reflected magenta or magenta on crystal. Absolutely no cross-shaped light, compass lines, sparkle, flare, halo, mist, text, numbers, labels, slab, stack, axis, additional object or decoration. One solitary silver-white faceted elongated octahedron, detailed tasteful raster model rendering for chroma key removal.
```

### Backdrop

입력: ActIntro_A_Stack.png (reference)

```text
Use case: precise-object-edit / stylized-concept. Create Backdrop.png, ONLY THE BACKGROUND AIR from input ActIntro_A_Stack.png. Reference image is the approved mood, color and atmospheric texture. Remove the entire slab tower, floating crystal, dotted vertical axis, all crosses, all text including upper-left label and ruler, and every hard-edged symbol. Reconstruct continuous empty blue-grey night air across the whole frame.
Opaque landscape16:9 intended1920x1080. Deep desaturated blue-grey and navy night atmosphere matching reference (#0D1520 to #1A2636 with subtle lighter blue-grey air). Very faint fine grain and delicate softly varied atmospheric texture. Lower quarter has thin pale drifting clouds and fog like reference, subtle and restrained, not bright white or dramatic storm. Quiet, open, sparse, usable as reusable game cinematic background. No central spotlight or hidden silhouette where plates used to be. No plate, object, axis, crystal, symbol, star, text, numbers, labels, grid, border, cyan light, flare or neon. Fill entire canvas; no transparency and no magenta. Raster painted atmospheric background faithfully matching reference.
```

