# Elon character sprites

## Source pack

Imported from `Downloads/elon_pixel_game_assets` (Grok pixel sheets) plus walk sheets from `Downloads/grok-assets.zip` (23 Aug 2026).

Re-run idle/portrait/walk from the original pack: `python Tools/import_elon_pixel_pack.py`

Re-run the 4-frame walk sheets: `python Tools/import_grok_walk_assets.py`

- **`source_pack/`** — original files (lineup, modern strip, singles). Background is transparent, not magenta.
- **`source_pack/grok-assets-20260823/`** — 20 unlabeled 1408x1408 walk JPGs (4 frames x 5 eras, facing right).
- **`01_young_sa` … `05_mars`** — processed idle / portrait / walk with chroma-key magenta `#FF00FF`
- **`Resources/Characters/Elon/`** — runtime copies for `Resources.Load`

Magenta is the only chroma-key. Do **not** key black or dark hair / shoes / clothing to alpha. Backgrounds are flood-filled from the corners so navy blazers, black tees, and EVA blacks stay.

## Era mapping in game

| Location | Era folder | Look | Spawn name |
|----------|------------|------|------------|
| Pretoria | `01_young_sa` | School blazer / shorts | `Player_Elon` |
| Toronto | `02_young_adult_90s` | Plaid shirt, jeans | `Player_Elon` |
| Palo Alto | `03_early_2000s` | Purple shirt (Zip2/X.com) | `Player_Elon` |
| (default) | `04_modern` | Black tee | `Player_Elon` |
| Mars assets | `05_mars` | White/orange EVA suit | `Player_Elon` |

`Player_YoungElon_PLACEHOLDER` is removed if present; Pretoria always uses the school idle.

## Runtime

- `ElonSpriteCatalog` loads textures from Resources and keys **magenta only** to alpha (Point filter, pivot at feet)
- `LoadWalkCycle` plays distinct same-facing walk frames (no mirrored copies, no synthesized stride when real frames exist)
- `GameplaySceneSetup` spawns `Player_Elon` with that location's idle + walk cycle
- `PixelPlayerController` animates walk while moving; facing is `SpriteRenderer.flipX`
- Dialogue panel shows era portrait

## Unity import

Textures under Art and Resources should use:

- Texture Type: **Sprite** (catalog also loads Texture2D + `Sprite.Create`)
- Filter Mode: **Point**
- Compression: **None**
- Pivot: **bottom center** (feet)
- Pixels Per Unit: **64**

`Tools/import_elon_pixel_pack.py` and `Tools/import_grok_walk_assets.py` write these into `.meta` files.

## Walk cycles

| Era | Walk source |
|-----|-------------|
| `01_young_sa` | Real 4-frame walk sheet from grok-assets.zip 23 Aug 2026 |
| `02_young_adult_90s` | Real 4-frame walk sheet from grok-assets.zip 23 Aug 2026 |
| `03_early_2000s` | Real 4-frame walk sheet from grok-assets.zip 23 Aug 2026 |
| `04_modern` | Real 4-frame walk sheet from grok-assets.zip 23 Aug 2026 |
| `05_mars` | Real 4-frame walk sheet from grok-assets.zip 23 Aug 2026 |

Frames are `walk_00` … `walk_03` (contact → pass → opposite contact → opposite pass). Idle is the most planted / smallest foot-span frame. Facing is flipX only — no mirrored `walk_04`.

Modern also keeps talk / side extras from the 4-pose strip (`elon_modern_talk.png`, `elon_modern_side.png`).
