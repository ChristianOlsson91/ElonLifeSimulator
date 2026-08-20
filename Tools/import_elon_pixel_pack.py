#!/usr/bin/env python3
"""Import elon_pixel_game_assets into Art + Resources.

Source sheets use a transparent (not magenta) background. This pipeline:
  1. Turns only transparent / low-alpha pixels into chroma-key magenta #FF00FF.
  2. Leaves opaque dark pixels (hair, shoes, black tee) unchanged.
  3. Crops to content, pads walk frames of an era to one size, pivot-at-feet.
  4. Writes Unity TextureImporter settings: Point filter, no compression.

Do NOT key RGB-black. Runtime ElonSpriteCatalog keys magenta only.
"""
from __future__ import annotations

import hashlib
import re
import shutil
import sys
import uuid
from pathlib import Path

from PIL import Image, ImageOps

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "Assets" / "Art" / "Characters" / "Elon"
RESOURCES = ROOT / "Assets" / "Resources" / "Characters" / "Elon"
SOURCE = ART / "source_pack"
DOWNLOADS = Path.home() / "Downloads" / "elon_pixel_game_assets"

MAGENTA = (255, 0, 255, 255)
ALPHA_CUT = 128
PAD = 6

PACK = {
    "school": "grok-53a1f39d-ed07-463d-8c03-d7407f2a2c1a.png",
    "plaid": "grok-2422592a-15c8-490d-92a4-96eff1273a43.png",
    "purple": "grok-eb33df4a-79c0-4f08-992a-b03366a2db90.png",
    "modern_idle": "grok-555e49e9-e1bb-45b2-a994-313f37b6cc68.png",
    "modern_walk": "grok-65505f32-be32-400e-bf84-c796895d2696.png",
    "mars": "grok-5bb23bd7-b09f-46dd-9462-14a63c72a5a7.png",
    "strip": "grok-90aa58bd-cc24-410b-b360-2f5f135d6057.png",
    "lineup": "grok-98af9d6e-d501-426b-9d64-eee48e2eeb85.png",
}

ERAS = [
    ("01_young_sa", "elon_young_sa", "school", None),
    ("02_young_adult_90s", "elon_young_adult", "plaid", None),
    ("03_early_2000s", "elon_early2000s", "purple", None),
    ("04_modern", "elon_modern", "modern_idle", "modern_walk"),
    ("05_mars", "elon_mars", "mars", None),
]


def is_magenta(px: tuple[int, int, int, int]) -> bool:
    r, g, b, _a = px
    return r > 200 and g < 40 and b > 200


def to_magenta_bg(im: Image.Image) -> Image.Image:
    """Transparent / low-alpha → magenta. Keep all opaque RGB, including black."""
    im = im.convert("RGBA")
    pix = im.load()
    w, h = im.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = pix[x, y]
            if a < ALPHA_CUT:
                pix[x, y] = MAGENTA
            else:
                pix[x, y] = (r, g, b, 255)
    return im


def content_bbox(im: Image.Image) -> tuple[int, int, int, int] | None:
    pix = im.load()
    w, h = im.size
    min_x, min_y, max_x, max_y = w, h, -1, -1
    for y in range(h):
        for x in range(w):
            if not is_magenta(pix[x, y]):
                if x < min_x:
                    min_x = x
                if y < min_y:
                    min_y = y
                if x > max_x:
                    max_x = x
                if y > max_y:
                    max_y = y
    if max_x < 0:
        return None
    return min_x, min_y, max_x, max_y


def crop_padded(im: Image.Image, pad: int = PAD) -> Image.Image:
    box = content_bbox(im)
    if box is None:
        return im
    x0, y0, x1, y1 = box
    x0 = max(0, x0 - pad)
    y0 = max(0, y0 - pad)
    x1 = min(im.width - 1, x1 + pad)
    y1 = min(im.height - 1, y1 + pad)
    cropped = im.crop((x0, y0, x1 + 1, y1 + 1))
    # Guarantee a magenta frame so chroma-key has edge pixels.
    out = Image.new("RGBA", (cropped.width, cropped.height), MAGENTA)
    out.paste(cropped, (0, 0))
    return out


def pad_to(im: Image.Image, tw: int, th: int) -> Image.Image:
    """Pad with magenta, character bottom-center (feet on the bottom edge)."""
    if im.width == tw and im.height == th:
        return im
    out = Image.new("RGBA", (tw, th), MAGENTA)
    x = (tw - im.width) // 2
    y = th - im.height
    out.paste(im, (x, max(0, y)))
    return out


def make_portrait(idle: Image.Image) -> Image.Image:
    w, h = idle.size
    crop_h = min(h, max(int(h * 0.50), w + 10))
    head = idle.crop((0, 0, w, crop_h))
    side = max(head.size)
    out = Image.new("RGBA", (side, side), MAGENTA)
    out.paste(head, ((side - head.width) // 2, (side - head.height) // 2))
    return out


def slice_cells(im: Image.Image, n: int) -> list[Image.Image]:
    cell_w = im.width // n
    return [im.crop((i * cell_w, 0, (i + 1) * cell_w, im.height)) for i in range(n)]


def load_pack(name: str) -> Image.Image:
    path = SOURCE / PACK[name]
    if not path.is_file():
        raise FileNotFoundError(path)
    return Image.open(path).convert("RGBA")


def walk_cycle(idle: Image.Image, walk: Image.Image | None) -> list[Image.Image]:
    """Five frames. Real walk pose when the pack has one; otherwise idle + flip."""
    if walk is not None:
        return [idle, walk, walk, ImageOps.mirror(walk), ImageOps.mirror(walk)]
    flipped = ImageOps.mirror(idle)
    return [idle, idle, flipped, flipped, idle]


def save_png(im: Image.Image, path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    im.save(path, format="PNG")


def copy_to_resources(rel: Path) -> None:
    src = ART / rel
    dst = RESOURCES / rel
    dst.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(src, dst)


def folder_meta(path: Path) -> None:
    if not path.is_dir():
        return
    meta_path = Path(str(path) + ".meta")
    if meta_path.exists():
        return
    guid = uuid.uuid4().hex
    meta_path.write_text(
        "fileFormatVersion: 2\n"
        f"guid: {guid}\n"
        "folderAsset: yes\n"
        "DefaultImporter:\n"
        "  externalObjects: {}\n"
        "  userData: \n"
        "  assetBundleName: \n"
        "  assetBundleVariant: \n",
        encoding="utf-8",
    )


TEXTURE_META_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 1
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 0
  spriteMeshType: 0
  alignment: 9
  spritePivot: {{x: 0.5, y: 0}}
  spritePixelsToUnits: 64
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  - serializedVersion: 4
    buildTarget: Standalone
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 1
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData: 
    physicsShape: []
    bones: []
    spriteID: 
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def stable_guid(path: Path) -> str:
    rel = str(path).replace("\\", "/")
    return hashlib.md5(rel.encode("utf-8")).hexdigest()


def write_texture_meta(png: Path) -> None:
    meta = Path(str(png) + ".meta")
    if meta.exists():
        patch_texture_meta(meta)
        return
    meta.write_text(TEXTURE_META_TEMPLATE.format(guid=stable_guid(png)), encoding="utf-8")


def patch_texture_meta(meta: Path) -> None:
    text = meta.read_text(encoding="utf-8")
    text = re.sub(r"filterMode: \d+", "filterMode: 0", text)
    text = re.sub(r"textureCompression: \d+", "textureCompression: 0", text)
    text = re.sub(r"spritePivot: \{x: [0-9.]+, y: [0-9.]+\}", "spritePivot: {x: 0.5, y: 0}", text)
    text = re.sub(r"^  alignment: \d+", "  alignment: 9", text, flags=re.M)
    text = re.sub(r"spritePixelsToUnits: \d+", "spritePixelsToUnits: 64", text)
    text = re.sub(r"spriteExtrude: \d+", "spriteExtrude: 0", text)
    text = re.sub(r"spriteMeshType: \d+", "spriteMeshType: 0", text)
    text = re.sub(r"^  isReadable: \d+", "  isReadable: 1", text, flags=re.M)
    meta.write_text(text, encoding="utf-8")


def count_stats(im: Image.Image) -> tuple[int, int, int]:
    pix = im.load()
    mag = dark = other = 0
    for y in range(im.height):
        for x in range(im.width):
            r, g, b, a = pix[x, y]
            if is_magenta((r, g, b, a)):
                mag += 1
            elif r < 40 and g < 40 and b < 40:
                dark += 1
            else:
                other += 1
    return mag, dark, other


def sync_source_pack() -> None:
    SOURCE.mkdir(parents=True, exist_ok=True)
    if not DOWNLOADS.is_dir():
        print(f"Downloads pack not found at {DOWNLOADS}; using existing source_pack")
        return
    for src in DOWNLOADS.glob("*.png"):
        dst = SOURCE / src.name
        shutil.copy2(src, dst)
        print(f"copied {src.name} -> source_pack")


def process_era(folder: str, prefix: str, idle_key: str, walk_key: str | None) -> None:
    idle = crop_padded(to_magenta_bg(load_pack(idle_key)))
    walk_src = crop_padded(to_magenta_bg(load_pack(walk_key))) if walk_key else None

    frames = walk_cycle(idle, walk_src)
    max_w = max(im.width for im in frames)
    max_h = max(im.height for im in frames)
    idle = pad_to(idle, max_w, max_h)
    frames = [pad_to(im, max_w, max_h) for im in frames]
    portrait = make_portrait(idle)

    era_art = ART / folder
    era_art.mkdir(parents=True, exist_ok=True)
    (era_art / "walk").mkdir(parents=True, exist_ok=True)

    save_png(idle, era_art / f"{prefix}_idle.png")
    save_png(portrait, era_art / f"{prefix}_portrait.png")
    for i, frame in enumerate(frames):
        save_png(frame, era_art / "walk" / f"{prefix}_walk_{i:02d}.png")

    mag, dark, other = count_stats(idle)
    print(f"{folder}: idle {idle.size} mag={mag} dark={dark} other={other} walk_real={walk_src is not None}")
    if dark < 50:
        print(f"  WARN few dark pixels — black clothing may have been keyed")


def process_modern_extras() -> None:
    strip = to_magenta_bg(load_pack("strip"))
    cells = [crop_padded(c) for c in slice_cells(strip, 4)]
    # strip: idle, talk, side, walk
    era = ART / "04_modern"
    save_png(cells[1], era / "elon_modern_talk.png")
    save_png(cells[2], era / "elon_modern_side.png")
    save_png(cells[3], era / "elon_modern_walk_frame.png")
    save_png(cells[3], era / "elon_modern_walk_01.png")
    save_png(Image.open(era / "elon_modern_idle.png"), era / "elon_modern_idle_strip.png")


def process_lineup() -> None:
    lineup = to_magenta_bg(load_pack("lineup"))
    save_png(lineup, ART / "elon_lineup_all_eras.png")


def ensure_folder_metas() -> None:
    folders = [
        ROOT / "Assets" / "Resources",
        ROOT / "Assets" / "Resources" / "Characters",
        RESOURCES,
        ART / "source_pack",
    ]
    for era, _, _, _ in ERAS:
        folders.append(ART / era)
        folders.append(ART / era / "walk")
        folders.append(RESOURCES / era)
        folders.append(RESOURCES / era / "walk")
    for f in folders:
        f.mkdir(parents=True, exist_ok=True)
        folder_meta(f)


def sync_resources_and_metas() -> None:
    ensure_folder_metas()
    pngs = list(ART.rglob("*.png"))
    for png in pngs:
        rel = png.relative_to(ART)
        if any(part.startswith("_") for part in rel.parts):
            write_texture_meta(png)
            continue
        if rel.parts[0] == "source_pack":
            write_texture_meta(png)
            continue
        copy_to_resources(rel)
        write_texture_meta(png)
        write_texture_meta(RESOURCES / rel)


def main() -> int:
    sync_source_pack()
    missing = [PACK[k] for k in PACK if not (SOURCE / PACK[k]).is_file()]
    if missing:
        print("FAIL missing source files:", missing)
        return 1

    for folder, prefix, idle_key, walk_key in ERAS:
        process_era(folder, prefix, idle_key, walk_key)
    process_modern_extras()
    process_lineup()
    sync_resources_and_metas()
    print("import complete")
    return 0


if __name__ == "__main__":
    sys.exit(main())
