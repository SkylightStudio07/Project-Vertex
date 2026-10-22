VERTEX Companion v2 sprite delivery

SVG reconstruction of approved mockups; not pixel-identical raster extraction. All UI sprite PNGs are 2x; the two backgrounds are exactly 1920x1080. Character is a byte-identical native-resolution project original.

layout.json uses top-left canvas coordinates. Nine-slice borders use SOURCE PNG pixels ordered left/bottom/right/top. Panel_Section has a fixed 48 logical-pixel header and 54-pixel top slice. Keep shadows on separate Images behind plates. Preserve aspect for frames containing centered symbols.

No text is baked in sprites. TMP must render layout text. Mockup previews use Malgun Gothic and sample additional unlock data. Every level grants one record. No list pager; overflow scrolls vertically. LINE controls cycle text only.

__portrait__ is an external runtime image layer. Do not load it as a sprite filename; use the companion portrait and clip to the provided portrait rect. Preview locked silhouettes reuse the original Sho portrait as an anonymous placeholder. Original illustration, not mockup-redrawn Sho, is supplied.

Run python build_split_v2.py to regenerate. Requires Pillow, Node, @resvg/resvg-js. Set VERTEX_RESVG_MODULE to override the renderer module path. Preview fonts use Windows Malgun Gothic. No Unity assets or scenes were modified.
