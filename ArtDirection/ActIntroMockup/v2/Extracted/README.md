# Act Intro v2 — 게임용 분절 PNG

확정 목업 A / B / C와 `../SPLIT_REQUEST.md`, `../../../UI_Mockup_Split_Workflow.md`의 4단계를 기준으로, 각 조각을 **built-in imagegen**으로 단독 재생성했습니다. 목업에서 가린 부분을 잘라 쓴 결과물이 아닙니다. 원래 목업과 요청서는 보존했습니다.

## 납품 파일

| 파일 | 캔버스 | 알파 |
|---|---|---|
| Plate_Off.png | 1200×1000 | 0 / 255 |
| Plate_Floor01.png | 1200×1000 | 꽃 외곽만 soft, 그 외 0 / 255 |
| Marker_Vertex.png | 256×384 | 0 / 255 |
| Fragment_A.png | 320×240 | 0 / 255 |
| Fragment_B.png | 320×240 | 0 / 255 |
| Fragment_C.png | 320×240 | 0 / 255 |
| Backdrop.png | 1920×1080 | 전 픽셀 255 |

모든 PNG는 8비트 RGBA(PNG color type 6), 회전 0°입니다. 판·결정·파편은 화면 2배용, 바탕은 1배용입니다. 고정 크기이며 9-slice는 없습니다.

- `layout.json`: **최종 PNG에서 읽은 실제 꼭짓점**, 캔버스·알파·그림 범위·목표 좌표·검수용 조립 위치.
- `Contact_Sheet.jpg`: 3000×2400, #0F1826 바탕. 왼쪽은 조각 단독과 바탕 미리보기, 오른쪽은 판 3층 조립. 판 캔버스 원점 y=300 / 790 / 1280으로 **피치 490px**이며, 맨 위를 정원 판으로 교체했습니다. 결정과 파편도 실제 납품 PNG를 합성했습니다.
- `Working/`: 생성 원본 8장(정원 초기본 포함), 실제 프롬프트 JSON, 원본 경로 기록, 재처리 스크립트, 알파 검증 로그, 남색 합성본과 꽃 외곽 확대본.

## 실제 처리

1. 확정 목업 3장과 기존 README의 실측값, 원 요청서와 생성 기록을 읽었습니다. 목업 A를 판·결정·파편·바탕의 그림 참조, C를 정원 내용 참조로 사용했습니다.
2. 각 자산을 개별 imagegen 호출로 그렸습니다. 투명 조각은 요청대로 평평한 순마젠타 배경으로 생성했고, 바탕은 불투명 남색으로 생성했습니다. 정원은 처음 생성한 뒤, 빈 판을 추가 참조하여 기하를 한 번 수정했습니다.
3. 생성 래스터 전체를 최종 캔버스 크기로 bicubic 리샘플링했습니다. 별도의 다각형, 판 표면, 해칭, 꽃, 그라데이션, 노이즈를 코드로 그리지 않았습니다. 판 모서리 강제 정렬, 부분별 워핑, 목업 조각 붙이기, 다각형 외곽 마스킹도 하지 않았습니다.
4. 마젠타 거리 `d = max(abs(R-255), G, abs(B-255))`를 계산했습니다. 일반 조각은 `d >= 96`에서 알파 255, 아니면 0입니다.
5. 정원에서 배경으로 돌출하는 꽃 외곽만 soft 처리했습니다. 최종 캔버스 기준 후보 영역은 x=190..330/y=205..340, x=450..565/y=155..235, x=870..1000/y=235..340, x=590..690/y=145..205입니다. 이 중 마젠타 혼합이 남은 픽셀(`min(R,B)-G > 8`, `d < 160`)에만 `alpha = clamp((d-40)/120,0,1)`를 적용했습니다. 꽃이 아닌 석조·림·옆면은 이진 알파입니다.
6. soft 픽셀의 RGB는 `(RGB - (1-alpha)*(255,0,255))/alpha`로 복구해 straight alpha로 저장했습니다. 이진 외곽 1px의 RGB는 안쪽 불투명 이웃 평균으로 교체했습니다(깨끗한 이웃을 찾는 반경 2..6px). 추가 마젠타 오염도 같은 방식으로 제거했습니다. 이웃이 없는 극세 꽃술은 초과 마젠타 성분을 제거했습니다. 알파 0의 숨은 RGB는 0입니다.
7. 투명 자산의 바깥 4px는 알파 0으로 고정했습니다. 바탕은 예외로 전 픽셀 불투명입니다.
8. imagegen 바탕의 기존 결·그라데이션을 유지하면서 채널별 선형 색 보정만 했습니다: `R'=1.6R+0.6`, `G'=4G/3-4/3`, `B'=18B/13-64/13` (0..255 clamp). 최종 중앙 샘플은 상단 (960,20) = #070C14, 하단 (960,1060) = #0F1826입니다. 코드로 새 바탕이나 결을 만들지 않았습니다.
9. 최종 PNG를 #0F1826 위에 일반 source-over 합성하고, 단독·조립·꽃 확대 이미지를 다시 열어 검수했습니다.

## 실제 꼭짓점과 남은 차이

좌표는 최종 PNG의 왼쪽 위 (0,0), x 오른쪽 / y 아래입니다. 윗면은 **면취가 아래로 꺾이기 전의 바깥 윗테두리**를 읽었습니다. 아래 모서리는 보이는 하단 윤곽입니다. 꽃·기둥 꼭대기는 판 꼭짓점에 포함하지 않습니다. 안티앨리어싱과 가는 외곽선 때문에 판독 허용 오차는 ±2px입니다. 정수 좌표는 실제 그림을 읽은 값이며 목표값 복사가 아닙니다.

| 판 | 뒤 | 왼쪽 | 오른쪽 | 앞 | 아래 왼쪽 / 앞 / 오른쪽 |
|---|---|---|---|---|---|
| Off | (600,124) | (68,411) | (1132,411) | (600,695) | (68,615) / (600,901) / (1132,615) |
| Floor01 | (600,120) | (49,417) | (1152,417) | (600,718) | (49,610) / (600,909) / (1152,611) |

**생성만으로 공통 기하를 완전히 일치시키지는 못했습니다.** Off 윗면은 약 1064×571px(1.86:1), 앞 두께 206px입니다. Floor01은 약 1103×598px(1.84:1), 앞 두께 191px입니다. 요청 목표 1080×560px / 두께 220px와 차이가 있고, 두 판 사이 크기·각도·두께도 다릅니다. 게임 정렬 시 `layout.json`의 각 판 좌표를 각각 사용해야 합니다. 정확한 2:1, 동일 실루엣, 픽셀 단위 공통 정렬은 보증하지 않습니다.

결정은 위 (128,35), 아래 (128,343), 두 점 중간 (128,189), 높이 308px입니다. 파편 윗면은 A 약 229px, B 약 185px, C 약 159px 너비입니다. 목표 약 240 / 200 / 160px보다 A/B가 조금 작고, 옆면 두께도 목표 25%보다 얇습니다. 각 실제 윗면 꼭짓점은 JSON에 별도 기록했습니다.

`screen_rect`는 null입니다. 이번 요청은 독립 조각 납품이며, 게임 화면 배치는 확정하지 않았습니다. 검수용 contact sheet 위치는 별도 객체에 기록했습니다. 텍스트 슬롯은 없고, 게임이 표시할 값은 그림에 넣지 않았습니다.

## 검증 결과

- PNG **7장**, 지정 이름·캔버스, PNG 헤더의 bit depth 8 / color type 6 확인.
- Off / 결정 / 파편 A·B·C: 중간 알파 픽셀 **0개**.
- Floor01: 지정 꽃 후보 영역의 중간 알파 **1867개**. 그 외 이진 알파.
- 투명 자산 6장의 4px 바깥 테두리: 비투명 픽셀 **0개**. 실제 그림 범위도 모두 캔버스 안쪽에 여유가 있으며 잘린 그림 없음.
- 바탕: 1920×1080, 불투명 픽셀 **2,073,600개**, 격자·오브젝트·문자·십자·해칭 띠 없음. 로그의 바탕 border 수는 불투명 바탕에 정상인 값이며 투명 조각 검사 대상이 아닙니다.
- 최종 보이는 픽셀에서 `min(R,B)-G > 8`인 초과 마젠타 픽셀: **모든 파일 0개**. 남색 합성 및 확대 시 분홍·보라 테두리를 관찰하지 못했습니다. 미세 꽃술은 배경 제거·이진 경계의 특성상 일부 가늘어지거나 끊어질 수 있습니다.
- 판은 밝은 윗면 / 중간 왼쪽 옆면 / 어두운 오른쪽 옆면, 면취와 사선 해칭 확인. Off의 내장 십자·가는 테두리·뒤쪽 검은 띠는 요청된 표면 무늬로 유지했습니다.
- 조각에 문자·숫자·떠 있는 꺾쇠 괄호·시안 선택선·스캔선·축·외부 그림자·다른 판 없음. 정원은 흰 꽃 / S자 길 / 각진 기둥 / 무너진 벽 / 계단 / 테라스 확인.
- SVG 미사용. 모든 실제 그림은 imagegen 생성물입니다. 코드는 래스터 후처리·측정·기존 PNG 합성에만 사용했습니다.
- 작업 파일은 이 v2 폴더 안에만 작성했습니다. Assets / Packages / ProjectSettings 변경, Unity 조작, git 사용, 게임 적용은 하지 않았습니다.

## 실제 생성 프롬프트

아래는 실행 순서 그대로입니다. 구조화 원문은 `Working/generation_prompts.json`, 선택 원본 기록은 `Working/generation_manifest.json`입니다. 각 입력은 스타일/내용 참조로 사용했으며, 정원 수정 호출만 앞서 생성한 정원 이미지를 편집 대상으로 사용했습니다.

### 1. Plate_Off.png

```text
Use case: stylized-concept. Asset Plate_Off for Project VERTEX. Input image is design/material reference ONLY, not an edit target. Draw ONE isolated complete chunky slab from the top block of reference A, independently, no other objects. Raster illustration, orthographic no perspective. Canvas intended 1200x1000, pure perfectly flat chroma magenta #FF00FF everywhere outside object. Slab top face outer vertices BACK(600,140), LEFT(60,420), RIGHT(1140,420), FRONT(600,700); vertical extrusion220px to LEFT BOTTOM(60,640), FRONT BOTTOM(600,920), RIGHT BOTTOM(1140,640). Top rhombus width1080 height560 approximately2:1, exact parallel edges slopes about26.565 degrees. Object entirely within canvas, generous margin above and below. Preserve reference matte offwhite top at brightest state, fine graphite outer contour and inset perimeter, small surface survey registration crosses near four corners and center, short bold charcoal diagonal stripe inset along back-right edge. These subtle INTRINSIC surface marks must match reference; no floating brackets or UI. Thin chamfered bevel rim. Left front wall mediumgrey, right front wall darkest graphite, very fine regular diagonal hatching on both walls matching reference, shallow matte paper/stone texture. Light upperleft. Fully expose the back tip, all4 top tips and3 bottom corners. NO text/numbers, cyan, glow, external shadow, floor/receiving plane, axes, rulers, detached brackets, other slabs, gradient background. Chroma magenta must be perfectly flat with no shadow. Draw with image model, not vector. Output one image.
```

### 2. Plate_Floor01.png

```text
Use case: stylized-concept. Draw ONE independently redrawn isolated Plate_Floor01 diorama asset. Input reference C is design reference ONLY: preserve its white spider-lily garden, S-shaped recessed stone path, angular stacked square stone pillars, crumbled walls, terraces and short stairs. This is not a finished screen. Flat PURE MAGENTA #FF00FF background, no gradient, no shadows outside slab. Intended canvas1200x1000. Orthographic no perspective. Base slab top rim outer corners BACK(600,140), LEFT(60,420), RIGHT(1140,420), FRONT(600,700). Vertical extrusion220px: bottom left(60,640), front(600,920), right(1140,640). Wide2:1 top rhombus, edges26.565degrees. All base corners clearly visible, keep offwhite fine bevel rim. EXACT same base material as reference A: matte mediumgrey left-front side with fine diagonal hatch, darkest graphite right-front side with fine diagonal hatch, no terrain on side walls. Moonlit garden restricted to upper diamond. Natural delicate WHITE Lycoris spider-lilies with fine curled petals and stamens, muted bluegreen leaves, cool slate ground. S-shaped stone walkway carved as shallow channel from front to back, terraces and short stairs; square ashlar ruined pillars and angular broken low walls, no round columns. Plants/columns can rise into top margin y20..140 but must not clip. Make scene balanced like reference C, preserve rich miniature diorama detail. Upperleft light, matte fine textures, crisp three-tone block. Entire object fully visible and alone on chroma magenta. NO cyan outline/lighting/brackets, NO external cast shadow/receiving plane, NO crystals/fragments/other slabs/axes, NO registration crosses on garden, NO UI, text, numbers, detached brackets or labels. No magenta inside art.
```

### 3. Marker_Vertex.png

```text
Use case: stylized-concept. Independently draw ONE isolated VERTEX marker crystal matching the elongated faceted octahedron in top center of input reference A. Reference is style only. Tall sharp matte low-poly OCTAHEDRON, symmetrical vertical long axis, pointed apex and bottom, maximum width at middle-height facet belt, clear triangular facets white and silvergrey with distinct planar tones, upperleft illumination. NOT hexagonal quartz, no extra tips. Canvas intended256x384. Crystal centered(128,192), TOP(128,42), BOTTOM(128,342), leftmost(40,192), rightmost(216,192), height300. Whole crystal visible surrounded by ample perfectly FLAT PURE MAGENTA #FF00FF chroma-key background. No magenta on object, no reflection of backdrop. Crisp fine edges, matte subtle paperlike texture, topface brightest, bottom right darkest. ONLY the single crystal: no bracket underneath, no string, axis, line, cross, pedestal, shadow, text, glow, other objects.
```

### 4. Fragment_A.png

```text
Use case: stylized-concept. Single game asset Fragment_A. Draw just ONE small chunky grey matte metallic slab fragment from the scattered small fragments of reference A, reference for material/angle only. Canvas intended320x240, centered complete block. Orthographic no perspective, top is2:1 rhombus WIDTH240 HEIGHT120 with back(160,30), left(40,90), right(280,90), front(160,150), extrude STRAIGHT DOWN60 to bottomfront(160,210) and bottomsidecorners(40,150),(280,150). Upperleft light: smooth matte warmgrey top brightest, mediumgrey left side, darkest right side; fine subtle horizontal edge lamination, very small bevels, same cool fine-grained material as reference fragments, no top markings or surface line. ONE object only on perfectly FLAT PURE MAGENTA #FF00FF. No magenta reflections. No external shadow, plane, UI, axis, grid, text, symbols, other slabs, glow.
```

### 5. Fragment_B.png

```text
Use case: stylized-concept. Single raster game asset Fragment_B. Independently draw ONE isolated small grey matte metal-like fragment slab matching scattered fragments in reference A. Canvas320x240 intended; centered, same orthographic angle as main slab, top2:1 diamond WIDTH200 HEIGHT100, back(160,45), left(60,95), right(260,95), front(160,145). Vertical thickness50, bottomfront(160,195). Modest thin bevel edge, slightly cooler darker grey top than Fragment A, subtle fine horizontal layered side texture, upperleft light: top lightgrey brightest, left midgrey, right darkestgrey. Pure flat MAGENTA #FF00FF backdrop, no gradient/shadow/reflection/magenta on artwork. ONE complete object ONLY, all vertices visible, ample margins, no text, markings, glow, external shadow, floor, grid, axis, other slabs. Preserve restrained matte illustrated finish in reference.
```

### 6. Fragment_C.png

```text
Use case: stylized-concept. Raster asset Fragment_C, ONE small chunky mattegrey slab fragment matching the reference's scattered little blocks. Independently drawn on flat PURE MAGENTA #FF00FF, no backdrop variation/shadow. Canvas intended320x240, centered with large margins. Orthographic top2:1 diamond WIDTH160 HEIGHT80, back(160,60), left(80,100), right(240,100), front(160,140), vertical extrusion40px to bottomfront(160,180). Same upperleft light, top bright silvergrey, left mediumgrey, right darkgrey, very thin bevel around edges, delicate matte grain and subtle layered sidewall texture. No polished reflections. ONLY one complete block, no markings/text/crosses/axis/grid, no external shadows or receiving floor, no other objects, no glow.
```

### 7. Backdrop.png

```text
Use case: stylized-concept. Draw ONLY a raster backdrop texture for the reference Project VERTEX briefing scene, target1920x1080 landscape16:9. Almost black navy, upper area #070C14, bottom slightly brighter #0F1826, extremely subtle short vertical gradient. Very faint fine matte paper/noise grain, uniform understated texture across full frame. All pixels opaque. NO magenta. Absolutely NO objects, slabs, crystal, fragments, grid, lines, axes, hatch strip, crosses, labels, text, decorations, vignette spotlight, clouds, smoke, horizon, stars. Quiet nearly-black navy blank backdrop ONLY, matching reference background minus every overlay and grid.
```

### 8. Plate_Floor01.png — revision 2

```text
Use case: precise-object-edit. Two inputs: Image1 is EDIT TARGET isolated garden on magenta; Image2 is geometry/material master isolated blank slab. Redraw Image1 with ONLY the base geometry and framing corrected to match Image2. Preserve Image1's lovely exact garden design: S-curve recessed path, delicate white spiderlily clusters, square ruined pillars, broken low walls, stairs and terraces. Make the garden sit on exactly the same slab silhouette, top-face angle, base width and220px thickness as Image2. Target1200x1000canvas, coordinates BACK(600,140), LEFT(60,420), RIGHT(1140,420), FRONT(600,700), BOTTOM LEFT(60,640), BOTTOM FRONT(600,920), BOTTOM RIGHT(1140,640). Top rhombus WIDTH1080 HEIGHT560; do NOT use the current garden's too-deep 1.68:1 rhombus. Flatten garden's top plane to approximately2:1 like Image2. Make corner framing identical to Image2: there is60px empty margin at left and right,140px over backtip and80px under bottomfront. Tall pillar allowed y20..140. Keep complete slab and flowers well within canvas, no clipped artwork. Base colors/textures and fine diagonal hatch the same as Image2: mediumgrey left, darkest graphite right, matte offwhite bevel. Garden occupies only top; no top survey marks. EXACT flat solid PURE MAGENTA#FF00FF background outside object. No cyan edge, brackets, glow, external castshadow, receivingplane, UI, text, otherobjects.
```
