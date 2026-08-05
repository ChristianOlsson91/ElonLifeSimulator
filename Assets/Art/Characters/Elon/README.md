# Elon character sprites

## Source pack

Imported from `Downloads/elon_pixel_game_assets` (Grok pixel sheets).

- **`source_pack/`** — original files (lineup, modern strip, singles)
- **`01_young_sa` … `05_mars`** — processed idle / portrait / walk (magenta key)
- **`Resources/Characters/Elon/`** — runtime copies for `Resources.Load`

## Era mapping in game

| Location | Era folder | Look |
|----------|------------|------|
| Pretoria | `01_young_sa` | School blazer / shorts |
| Toronto | `02_young_adult_90s` | Plaid shirt, jeans |
| Palo Alto | `03_early_2000s` | Purple shirt (Zip2/X.com) |
| (default) | `04_modern` | Black tee |
| Mars assets | `05_mars` | White/orange EVA suit |

## Runtime

- `ElonSpriteCatalog` loads textures from Resources, keys magenta/black to alpha
- `GameplaySceneSetup` spawns `Player_Elon` with idle + walk cycle
- `PixelPlayerController` animates walk while moving
- Dialogue panel shows era portrait

## Unity import

Textures under Resources should use:

- Texture Type: **Default** or **Sprite** (catalog uses Texture2D + Sprite.Create)
- Filter Mode: **Point**
- Compression: **None** (crisp pixels)

If sprites look blurry, select all under `Resources/Characters/Elon` and set Point filter.

## Notes

- Walk for non-modern eras currently reuses idle/flip (source pack only had full walk for modern). Replace when more walk sheets exist.
- Modern has real walk + talk + side variants from the 4-pose strip.
