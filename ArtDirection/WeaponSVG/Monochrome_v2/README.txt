VERTEX — Monochrome weapon set v2

Four authored SVG profiles: Revolver, Sniper, SMG, Railgun. All face right, without perspective distortion. Every visible pixel uses one ink (#16181B); interior grooves/windows are true transparent cutouts. No gradients, glow, shadows, embedded raster or secondary colored detail.

SVG uses only vector geometry plus a vector luminance mask; black/white mask shapes encode cutouts, not visible colors. Change the root color attribute to recolor every part. Transparent PNG renders at 2x included for convenience. Overview and SmallScale are presentation sheets, not sprites.

Original WeaponSVG files untouched. These new sibling files are not auto-wired into Unity or existing mockups. Manifest records viewBoxes, alpha bounds and checksums.

Rebuild: python build.py. Dependencies: Pillow, Node, @resvg/resvg-js (override VERTEX_RESVG_MODULE), Windows fonts for presentation sheets only.
