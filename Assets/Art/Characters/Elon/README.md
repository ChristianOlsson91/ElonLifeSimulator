# Elon character sprites

## Source pack

Imported from `Downloads/elon_pixel_game_assets` (Grok pixel sheets).

Re-run: `python Tools/import_elon_pixel_pack.py`

- **`source_pack/`** — original files (lineup, modern strip, singles). Background is transparent, not magenta.
- **`01_young_sa` … `05_mars`** — processed idle / portrait / walk with chroma-key magenta `#FF00FF`
- **`Resources/Characters/Elon/`** — runtime copies for `Resources.Load`

Magenta is the only chroma-key. Do **not** key black or dark hair / shoes / clothing to alpha.

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
- `GameplaySceneSetup` spawns `Player_Elon` with that location's idle + walk cycle
- `PixelPlayerController` animates walk while moving
- Dialogue panel shows era portrait

## Unity import

Textures under Art and Resources should use:

- Texture Type: **Sprite** (catalog also loads Texture2D + `Sprite.Create`)
- Filter Mode: **Point**
- Compression: **None**
- Pivot: **bottom center** (feet)
- Pixels Per Unit: **64**

`Tools/import_elon_pixel_pack.py` writes these into `.meta` files.

## Walk cycles

| Era | Walk source |
|-----|-------------|
| `04_modern` | Real walk pose from the pack. Runtime drops the mirrored frames (facing is `flipX`). |
| `01_young_sa`, `02_young_adult_90s`, `03_early_2000s`, `05_mars` | Pack has no walk sheet (idle copies + one mirrored idle). `ElonSpriteCatalog` synthesizes a 5-frame SNES stride from the idle likeness at runtime. Replace with real walk sheets when they exist. |

Do **not** play the mirrored idle as a walk frame — that strobes the whole body. Facing is `SpriteRenderer.flipX` only.

Modern also keeps talk / side extras from the 4-pose strip (`elon_modern_talk.png`, `elon_modern_side.png`).
