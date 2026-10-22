Loadout v3 UI kit

All UI PNGs are 2x, except Loadout_BG (1920x1080). SVG originals included for authored geometry. Icon_Artifact is copied byte-for-byte from supplied raster; Mark_Upgraded PNG/SVG copied from ArmoryHub. Text is separate in layout.json for TMP.

Weapon PNGs are NOT included. External paths in layout.json resolve from this Extracted_Draft folder to ../../WeaponSVG. Preview fits the original PNG alpha bounds, preserves aspect and direction, does not tint or flip. Do not import __weapon__ as a sprite name.

9-slice borders use source PNG pixels, left/bottom/right/top. Panel_Section top 90 logical pixels preserve its fixed 84-pixel header. Use independent shadow sprites behind panels. Repeat deck_row and quest_row templates; maximum two quests. Empty quest state suppresses all rows. Unlocked perk preview is a placeholder graphic/text, bind actual perk data.

Geometric background and panel kit are SVG reconstructions rather than pixel-exact crops. The v3 mockup is assembled from the kit and original weapon PNGs. No new image generation or Unity asset editing performed. Run python build_v3.py; requires Pillow, Node, @resvg/resvg-js (VERTEX_RESVG_MODULE override), Windows Malgun Gothic preview font and project external weapon PNGs.
