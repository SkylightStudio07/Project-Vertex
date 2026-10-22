Lobby Shop A - delivery notes

Coordinates: 1920x1080, top-left origin; rect = x,y,width,height.
Panels: SVG authored, exported as 2x RGBA PNG. Border values in layout.json are source PNG pixels, in left/bottom/right/top order.
Typography: UI words, numbers, quantity signs and state labels are NOT baked into the SVG/PNG panel assets. Render with TMP using the layout templates.
Currency icons: copied byte-for-byte from Currency_Icons; no redraw.
Products: extracted from the supplied draft; existing product branding belongs to the original illustration.
Tabs: existing right-side shop placement retained by explicit user clarification; armory shape and height matched.
Card examples: visual state previews only, not additions to the live game catalog.

Keeper_Full.png limitation:
The supplied draft is not an unobstructed full-body source. The dialogue panel and foreground cases cover parts of the keeper, and the image ends before the lower legs. The cutout contains visible original pixels only, not reconstructed anatomy. Its missing dialogue-covered area is transparent. Keep the dialogue at the original placement. A fully unobstructed standalone cutout requires the original character layer. See Extraction_Review.png and extraction_sources.json for the matte and exact source rectangle. The mockup itself preserves the original character and dialogue pixels.

Reference images are not bundled; build scripts expect them beside the scripts in the project. Renderer requires @resvg/resvg-js; extraction requires OpenCV and Pillow. Paths in scripts reflect this workstation.
