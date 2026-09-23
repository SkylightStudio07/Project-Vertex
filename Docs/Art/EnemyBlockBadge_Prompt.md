# Enemy block HUD badge

Generated with the built-in image tool, referencing the user's shield/HP sample for function and the existing White Vertex event UI for style.

Asset: `Assets/Art/UI/UI_EnemyBlockBadge_WhiteVertex.png`.
Consumer: `Assets/Data/Enemy/Prefabs/EnemyView.prefab`, HP Bar/BlockBadge/ShieldIcon. The Amount TMP label displays live EnemyInstance.Block, independently of HP.

## Prompt

Use case: stylized-concept. Generate exactly ONE production UI shield badge sprite with a BLANK center, for displaying a live numeric block counter at the left end of an enemy HP bar in WHITE VERTEX. Image 1 is a functional layout reference ONLY: shield with a number at the bar end. Do not draw the number or bar. Image 2 is the authoritative art-direction reference: off-white and charcoal editorial UI, thin observation lines, clipped layered planes, faint paper grain. Draw a small sturdy upright shield with a matte charcoal blue-gray filled center, finely illustrated off-white angular folded border and a restrained desaturated pale blue edge accent. Broad uncluttered central face reserved for 1-3 white digits rendered by the game. Subtle paper/ink texture like reference 2, not a generic SVG logo, no 3D bevels, no metal or ice or glow. At most a tiny four-point mark at bottom tip OUTSIDE the central number area. Strong simple readable silhouette at 50 pixels height. Single centered badge in square transparent canvas, 12 percent empty margins. Absolutely NO text, digits, bars, background, checkerboard, scenery, extra icons, particles, or mockup. Actual transparent background.

## Verification

- Actual 전술 회피 EnemyAction: 8 block; damage 3 -> 5 block; HP remains 42/66.
- ResetBlock hides the badge; three-digit value 125 fits.
- Coroutine action, rebind initial state, unsubscribe from previous enemy, and Bind(null) verified.
- In-game 1920x1080 capture reviewed. Existing zero-height fill/legacy Slider layout conflict corrected.
- New art-direction shield VFX visible at enemy center with 0 screen-space offset.
- Tooltip hover enter/exit works, sorting order 200, no raycast interception, all current status descriptions fit.
- Play mode stopped and original blessing PlayerPrefs restored. No commits or pushes.

