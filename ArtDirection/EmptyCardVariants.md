# 아트 없는 카드 추가본

첨부된 두 카드에 대해 내장 imagegen 모델로 배경 삽화를 제거했다. 원본 보존. PNG 크기는 원본과 동일하며 바깥 4픽셀 테두리는 원본을 재사용했다. 내부는 모델 복원본으로 픽셀 단위 원본 레이어가 아니다.

- QuestBoardMockup/Extracted/Card_Accepted_NoArt.png: 367×289. 서류/가방/도시 제거, 종이 바탕·구분선·봉투·수주 도장 유지.
- ArmoryMockup/CardCatalogExtracted/Card_Attack_NoArt.png: 172×301. 총/손/발포/도시 제거, 빈 아트 칸·카드 테두리·속성·에너지 표시 유지.

배경 삽화가 없는 불투명 종이 템플릿이며, 투명 아트 창은 아니다. 기존 layout.json에서 원본과 같은 좌표에 대체 배치하면 된다. 다른 카드 및 원본 파일은 변경하지 않았다. Unity 배선 없음.

## 프롬프트

### Quest

Precise-object-edit. Edit this exact landscape UI card into a reusable EMPTY card backing. Remove ALL illustrative content: document case, papers, city, towers, landscape, all shadows and ghost silhouettes. Restore continuous subtly textured off-white paper everywhere inside existing border. Preserve precisely the thin metallic/graphite outer frame, corner cuts and accents, two horizontal divider lines, small black envelope top-left, and cyan 수주 stamp top-right. No new artwork, text or icons. Preserve aspect ratio and edge-to-edge framing. Empty WHITE PAPER, no scenery.

### Attack

Precise-object-edit. Edit this exact portrait UI card into reusable EMPTY card frame. Remove pistol, muzzle flash, hand, all city buildings and all snow scenery from the upper artwork window. Replace with clean empty pale cool-gray paper art placeholder, no objects, no silhouettes, no scenery. Preserve exact cyan outer selected border, clipped corners, white starburst attack-type symbol at top-left (give it its existing dark gray local backing if necessary), all three cyan/gray energy diamonds, white lower description area, fine frame details. Same proportions edge-to-edge, no added text or artwork.

