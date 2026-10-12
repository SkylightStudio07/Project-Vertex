# Extraction generation prompts

Built-in image generation tool / imagegen skill. Input for both edits: `Armory_Equipment_Mockup_v2_Sniper.png`. Opaque model output; transparency on sliced contours is supplied by deterministic masks. No new independently designed button assets.

## Text removal

Use case: precise-object-edit. Edit target: attached approved 1672x941 armory UI mockup. Produce EXACT SAME composition, dimensions, panel positions, weapon illustrations, symbols, cyan accents and paper texture. Remove ALL text and numbers everywhere including Korean, English, LV, EXP, costs, stats, headings, button labels. Restore only paper or dark button surfaces under removed lettering. Preserve pistol, SMG, scoped sniper rifle and shotgun images, checkmarks, padlocks, arrows, star emblem, stat dividers, progress bar and upgrade tree circles/connector. Do NOT redesign, move, resize or add anything. This is a textless master for precise slicing, not a new design. No typography anywhere.

## Empty backing reconstruction

Use case: precise-object-edit. Edit target: approved armory equipment screen. Produce clean EMPTY UI BACKING master for slicing at identical 1672x941 coordinates. Remove ALL text/numbers, ALL guns including large central pistol and all four thumbnails, all checkmarks/locks/icons/arrows, all upgrade circles/connectors, and central watermark compass behind pistol. Preserve exactly all panel boundaries, paper textures, black large bottom-right button with cyan bottom rule and faint compass decoration, selected cyan tab, inactive tab, left four weapon tile outlines, card upgrade button outlines, column dividers, outer snowy scenery. Replace removed objects with coherent blank underlying surfaces. Preserve dark level badge as SOLID dark rectangle without text. Preserve top progress track but remove cyan fill. No redesign or moved geometry; no new symbols, typography or elements.
