# Block gain shield repaint

Generated using the built-in image generation tool. The first geometric version was rejected as too vector-like; this repaint uses illustrated crystalline brushwork.

Repaint output: `Assets/Art/VFX/BlockGainShield_Painted.png`.

## Prompt

Use case: stylized-concept. Production transparent 2D combat VFX sprite for VERTEX, a cold white ruined-world dark fantasy game. Draw a hand-painted spectral ward forming in the recognizable silhouette of a tall shield, front-facing and centered, isolated on genuinely transparent background. NOT a logo, NOT vector art, NOT an SVG icon. Painterly chalk-white and ash-gray broken crystalline strokes, subtle charcoal brush texture and chipped irregular edges, restrained pale icy cyan energy inside cracks. A luminous four-point ward mark brushed into the center, organic wisps and a few floating fragments close to the shield. Interior mostly translucent so the character behind remains readable. Beautiful intentional illustrated brushwork, slight asymmetric wear, strong readable outer silhouette at small size, not a detailed metal equipment shield. No thick uniform geometric outlines, no flat fill clipart, no bevels or glossy 3D, no saturated neon, no text, no checkerboard, no scenery, no UI frame. Single asset with 10 percent transparent margins, all fragments contained. Palette ivory, cold gray, muted icy cyan.

## Integration status

- EnemyView block amount subscription and badge prefab are implemented.
- This painted ice/crystal repaint was rejected for not matching the art direction and was never assigned.
- BlockGainShield.prefab now uses BlockGainShield_ArtDirection.png with an unlit material; see BlockGainShield_ArtDirection_Prompt.md.
- The persistent numerical HUD uses UI_EnemyBlockBadge_WhiteVertex.png; see EnemyBlockBadge_Prompt.md for successful play verification. Tooltip wiring and play verification are also complete.
