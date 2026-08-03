# Elon character sprites (SNES-style pixel art)

Path: `Assets/Art/Characters/Elon/`

## Style

- Clean **16-bit / SNES-inspired** pixel art
- Limited cohesive palette; **magenta `#FF00FF`** flat key background
- Isolated character, no environment
- Stylized game proportions; biographical game art of a public figure (not photoreal)

## Eras

| Folder | Era | Costume cues |
|--------|-----|----------------|
| `01_young_sa` | Young (SA school years) | Navy blazer, red tie, grey pants, book; awkward/youth posture |
| `02_young_adult_90s` | Canada / early Zip2 | Grey hoodie, white tee, jeans, sneakers |
| `03_early_2000s` | PayPal / early SpaceX & Tesla | Black turtleneck, khaki pants |
| `04_modern` | Modern | Black t-shirt, dark pants, confident stance |
| `05_mars` | Future Mars | Orange/white space suit (settler / EVA look) |

## Per era files

- `elon_*_idle.png` — standing idle
- `elon_*_portrait.png` — dialogue headshot / bust
- `walk/elon_*_walk_00.png` … `walk_04.png` — **5** walk frames (idle-adjacent + 4 stride poses; use `01`–`04` for cycle or all five)

## Unity import (recommended)

1. Select PNGs → Texture Type **Sprite (2D and UI)**
2. **Filter Mode: Point (no filter)**
3. Compression: None or low for crisp pixels
4. Pixels Per Unit: try **32–64** depending on scene scale
5. Magenta can be made transparent in a Sprite Editor / shader, or re-export with alpha later

## Notes

- Walk cycles were built as keyframe edits (video walk pipeline unavailable in this environment). Footing may need polish for production.
- Young SA is a **stylized school-era** design in the same pack language (not a photographic minor likeness).
- Reference base for modern look: Wikimedia Commons Royal Society portrait (edit-chained into pixel art).

## Likeness pass (update)

Sprites were re-edited **reference-first** from a public adult photo (Wikimedia Royal Society portrait) so face/hair read more like Elon Musk while staying SNES pixel art.

- Modern idle/portrait locked first, then face language propagated to other eras.
- Walk frames rebuilt from improved idles (5 frames: walk_00..walk_04).
- Young SA remains stylized school-era game art (not a photographic minor likeness).
