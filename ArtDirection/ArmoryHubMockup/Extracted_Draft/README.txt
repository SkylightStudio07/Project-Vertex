Armory v2 delivery

SVG panels + 2x PNG; text is in layout.json for TMP. Border order is SOURCE pixels left/bottom/right/top. Workshop_BG is a generated clean raster embedded in a self-contained SVG; it is not vector art.
Currency files are copied byte-for-byte from supplied originals. Raster card/weapon art retains native resolution. Character is intentionally NOT INCLUDED; bind ../Source_Mechanic.png from the parent project.
Mockup_v2 preserves image_gen artwork with exact source-icon/header corrections. Assembly_v2 proves reconstructed sprites/layout. Panel contours, texture and shadows may differ from generated reference.
Normal, insufficient currency, upgraded, and ineligible states are defined in layout.json. Button_Upgrade_Disabled is shared, state labels remain TMP. No EXP.
Rebuild: python build_delivery_v2.py. Requires Pillow, Node and @resvg/resvg-js (override module path with VERTEX_RESVG_MODULE). Preview font: Windows Malgun Gothic. External input art/source files remain in parent project.
