# VERTEX 로비 UI 아트 원본 v1

2026-09-29. 승인된 로비 목업을 참조하여 내장 이미지 생성 모델로 제작. 씬 적용 전 검토용 개별 PNG 18장.

## 상태와 제약

- 버튼 6종 × Normal/Hover = 12장, 설정 버튼 2장, 상단/대사 패널 2장, 게이지 Track/Fill 2장.
- 모든 문구·수치·레벨·번역 대상 텍스트를 제외했다. 아이콘은 패널에 포함되어 있다.
- 투명 배경 요청 2회 모두 `background='transparent' is not supported by gpt-image-2` 오류. 사용자 허용에 따라 불투명 원본으로 생성했다. 흰 외곽은 투명 영역이 아니다.
- 원본에는 외곽 여백이 있다. 적용 시 스프라이트 영역과 절단 모서리 마스크를 맞추어야 한다. 전체 이미지를 그대로 Image에 넣으면 버튼 크기와 클릭 영역이 어긋날 수 있다.
- Hover는 Normal 원본을 참조하여 생성했다. 픽셀 단위 동일 형상은 보장되지 않으므로 실제 SpriteSwap 전에 정렬 및 외곽 확인이 필요하다.
- ProgressFill은 테두리가 포함된 전체 채움 그림이다. 적용 시 내부 채움 부분만 슬라이스하거나 마스크로 분리하고 Track 위에서 비율을 제어한다.
- 배경·캐릭터·실제 텍스트·화살표·성장 수치는 별도 런타임 요소. 현재 Unity 씬, 프리팹, 코드에는 적용하지 않았다.

## 파일과 생성 프롬프트

### Expedition_Normal.png

![Expedition](Expedition_Normal.png)

Extract/recreate a SINGLE standalone production raster UI asset from approved lobby reference. Expedition NORMAL state: wide 3:1 off-white expedition preparation tile, large faint four-point compass emblem on right third, left two thirds blank for title and description. Faithfully match reference's flat clean off-white printed paper, sparse black structural edge, diagonal cut upper-left and lower-right corners and tiny cyan lower edge line. No text whatsoever, no letters, no numbers, no labels, no arrow glyphs. Do not output the full screen. No character, landscape or other UI. Opaque full-bleed rectangular tile, NO exterior padding; the entire canvas is the button's paper surface. Suggest diagonal corner cuts with fine printed graphite lines inside corners. Straight orthographic, large clean blank text safe area, generous useful surface, zero margin, wide canvas matching tile. No bevel, metal, ornamental frame or glow.

### Expedition_Hover.png

![ExpeditionHover](Expedition_Hover.png)

Edit the attached SINGLE UI tile to make its subtle HOVER state. Preserve exact canvas dimensions, panel placement, outer contour, icon or illustration size and coordinates, all texture and blank text areas. Change ONLY neutral edge accents to crisp cyan and add a very subtle cool cyan tint just inside the lower edge. For dark tile brighten the existing fire illustration slightly; for light tile tint the existing compass softly cyan. No glow outside the silhouette, no movement, no scaling, no new ornaments. NO TEXT, numbers, letters, symbols resembling writing or labels. Preserve the existing background. Output one tile, not comparison or sprite sheet.

### Bonfire_Normal.png

![Bonfire](Bonfire_Normal.png)

Use case ui-mockup. Create ONE isolated production UI tile faithfully extracted from attached approved lobby design: wide 4:1 dark graphite social/bonfire tile, subtle monochrome campsite with campfire and folding chairs on right third, left two thirds blank for labels. NORMAL state. Flat printed matte graphic, sparse fine structural lines and understated diagonal corner cuts. NO text, digits, letters, labels, watermarks or arrows anywhere. Full-bleed opaque rectangular button surface, no exterior padding. Wide canvas 4:1. Keep the left two thirds dark blank for localized white text. Only right third has the reference's campfire, chairs and tents in subdued grayscale illustration. No character, other UI or surrounding environment. Tiny cyan accent only. Match mockup exactly in mood; no bevel, ornate border, neon or metallic gloss.

### Bonfire_Hover.png

![BonfireHover](Bonfire_Hover.png)

Edit the attached SINGLE UI tile to make its subtle HOVER state. Preserve exact canvas dimensions, panel placement, outer contour, icon or illustration size and coordinates, all texture and blank text areas. Change ONLY neutral edge accents to crisp cyan and add a very subtle cool cyan tint just inside the lower edge. For dark tile brighten the existing fire illustration slightly; for light tile tint the existing compass softly cyan. No glow outside the silhouette, no movement, no scaling, no new ornaments. NO TEXT, numbers, letters, symbols resembling writing or labels. Preserve the existing background. Output one tile, not comparison or sprite sheet.

### Armory_Normal.png

![Armory](Armory_Normal.png)

Use case ui-mockup. ONE production game lobby NORMAL button asset from attached approved screenshot. 2:1 off-white facility tile, bold flat black single rifle silhouette on left quarter, right side blank for localized name. Match small facility tiles: matte off-white paper, very thin gray border, subtle clipped corners, minimal detail. 2:1 landscape image filled by tile. Keep outside silhouette pure white only if transparency unavailable, no cast shadows. Icon centered left quarter; right 65% clean blank for localized UI text. No text, digits, letters, pseudo-writing, level labels or arrows ANYWHERE. No scenery, character or other panels. Flat monochrome editorial graphic, restrained cyan short lower-edge tick. No bevel, metal or fantasy ornament.

### Armory_Hover.png

![ArmoryHover](Armory_Hover.png)

Edit this exact UI button into its HOVER state. Preserve EXACT same canvas size, white exterior padding, silhouette coordinates, icon shape/size/position and blank label safe areas. ONLY change thin gray perimeter outline to cyan and add a subtle pale cyan tint to the panel interior; keep black pictogram black. Preserve all geometry and texture. No text, digits, letters, labels, arrows, new ornaments, glow, movement, zoom, resizing or comparison layout. One output.

### QuestBoard_Normal.png

![QuestBoard](QuestBoard_Normal.png)

Use case ui-mockup. ONE production game lobby NORMAL button asset from attached approved screenshot. 2:1 off-white facility tile, bold flat black clipboard/document pictogram on left quarter, right side blank for localized name. Match small facility tiles: matte off-white paper, very thin gray border, subtle clipped corners, minimal detail. 2:1 landscape image filled by tile. Keep outside silhouette pure white only if transparency unavailable, no cast shadows. Icon centered left quarter; right 65% clean blank for localized UI text. No text, digits, letters, pseudo-writing, level labels or arrows ANYWHERE. No scenery, character or other panels. Flat monochrome editorial graphic, restrained cyan short lower-edge tick. No bevel, metal or fantasy ornament.

### QuestBoard_Hover.png

![QuestBoardHover](QuestBoard_Hover.png)

Edit this exact UI button into its HOVER state. Preserve EXACT same canvas size, white exterior padding, silhouette coordinates, icon shape/size/position and blank label safe areas. ONLY change thin gray perimeter outline to cyan and add a subtle pale cyan tint to the panel interior; keep black pictogram black. Preserve all geometry and texture. No text, digits, letters, labels, arrows, new ornaments, glow, movement, zoom, resizing or comparison layout. One output.

### InformationBroker_Normal.png

![InformationBroker](InformationBroker_Normal.png)

Use case ui-mockup. ONE production game lobby NORMAL button asset from approved screenshot. 2:1 off-white facility tile, bold flat black eye inside archive pictogram on left quarter, right side blank for localized name. Match small facility tiles: matte off-white paper, thin gray outline, clipped upper-left and lower-right corners. 2:1 landscape canvas filled by tile; minimal exterior white padding, no cast shadow. Icon centered left quarter, right 65% blank for localized text. NO text, digits, letters, pseudo-writing, levels or arrows. No scenery, characters or other panels. Flat monochrome editorial graphic, tiny cyan lower-edge accent. No bevel, metal or ornament.

### InformationBroker_Hover.png

![InformationBrokerHover](InformationBroker_Hover.png)

Edit this exact UI button into its HOVER state. Preserve EXACT same canvas size, white exterior padding, silhouette coordinates, icon shape/size/position and blank label safe areas. ONLY change thin gray perimeter outline to cyan and add a subtle pale cyan tint to the panel interior; keep black pictogram black. Preserve all geometry and texture. No text, digits, letters, labels, arrows, new ornaments, glow, movement, zoom, resizing or comparison layout. One output.

### TrainingGround_Normal.png

![TrainingGround](TrainingGround_Normal.png)

Use case ui-mockup. ONE production game lobby NORMAL button asset from approved screenshot. 2:1 off-white facility tile, bold flat black target pictogram on left quarter, right side blank for localized name. Match small facility tiles: matte off-white paper, thin gray outline, clipped upper-left and lower-right corners. 2:1 landscape canvas filled by tile; minimal exterior white padding, no cast shadow. Icon centered left quarter, right 65% blank for localized text. NO text, digits, letters, pseudo-writing, levels or arrows. No scenery, characters or other panels. Flat monochrome editorial graphic, tiny cyan lower-edge accent. No bevel, metal or ornament.

### TrainingGround_Hover.png

![TrainingGroundHover](TrainingGround_Hover.png)

Edit this exact UI button into its HOVER state. Preserve EXACT same canvas size, white exterior padding, silhouette coordinates, icon shape/size/position and blank label safe areas. ONLY change thin gray perimeter outline to cyan and add a subtle pale cyan tint to the panel interior; keep black pictogram black. Preserve all geometry and texture. No text, digits, letters, labels, arrows, new ornaments, glow, movement, zoom, resizing or comparison layout. One output.

### Header.png

![Header](Header.png)

Use case ui-mockup. Generate standalone production raster UI element extracted from approved reference: wide off-white upper-left lobby brand panel, small black four-point compass near left end and the rest blank for separate logo text; fine gray lower edge with diagonal lower-right cut. Full-bleed paper rectangle, 3:1. Same clean flat matte off-white paper / black observation-system art style, minimal cyan accent. No letters, text, numbers, pseudo-writing or logos except specified compass pictogram. NO character, scenery, full-screen mockup, shadows or bevel. Solid white exterior if any, as small as possible. Preserve useful empty text space.

### Dialogue.png

![Dialogue](Dialogue.png)

Use case ui-mockup. Generate standalone production raster UI element extracted from approved reference: wide character dialogue nameplate with short graphite tab at upper-left for separate name and wide off-white lower strip for dialogue. Very sparse coordinate ticks at edges, no text at all. One panel only, landscape 3:1. Same clean flat matte off-white paper / black observation-system art style, minimal cyan accent. No letters, text, numbers, pseudo-writing or logos except specified compass pictogram. NO character, scenery, full-screen mockup, shadows or bevel. Solid white exterior if any, as small as possible. Preserve useful empty text space.

### Settings_Normal.png

![Settings](Settings_Normal.png)

Production VERTEX lobby standalone raster UI element, matching reference. One square compact off-white settings button with single centered black solid gear pictogram, fine gray clipped-corner perimeter, tiny cyan corner tick, no other symbols. Minimal outer margin. Flat clean monochrome observational system design. NO text, digits, letters or pseudo-writing anywhere. No bevel, glow, metal or other UI. Plain white exterior.

### Settings_Hover.png

![SettingsHover](Settings_Hover.png)

Edit this exact UI button into its HOVER state. Preserve EXACT same canvas size, white exterior padding, silhouette coordinates, icon shape/size/position and blank label safe areas. ONLY change thin gray perimeter outline to cyan and add a subtle pale cyan tint to the panel interior; keep black pictogram black. Preserve all geometry and texture. No text, digits, letters, labels, arrows, new ornaments, glow, movement, zoom, resizing or comparison layout. One output.

### ProgressTrack.png

![ProgressTrack](ProgressTrack.png)

Production VERTEX lobby standalone raster UI element, matching reference. One long narrow EMPTY horizontal growth progress track on off-white matte surface: thin graphite rectangular outline around flat pale gray empty bar, squared ends, no tick numbers or labels. Bar almost full image width, 6:1 silhouette centered in wide canvas. No filled portion. Flat clean monochrome observational system design. NO text, digits, letters or pseudo-writing anywhere. No bevel, glow, metal or other UI. Plain white exterior.

### ProgressFill.png

![ProgressFill](ProgressFill.png)

Edit the attached progress bar asset. Preserve exact canvas dimensions and the bar coordinates and width/height. Change ONLY the empty pale gray interior to a uniform solid cyan fill, completely full. Keep outline and white exterior unchanged. No text, digits, letters, labels, ticks, glow or ornament. This is a standalone filled gauge image for later clipping in Unity.


