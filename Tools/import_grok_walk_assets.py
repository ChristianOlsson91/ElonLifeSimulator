#!/usr/bin/env python3
"""Import grok-assets.zip walk sheets (4 frames x 5 eras) into Art + Resources.

Source JPGs are 1408x1408 with white / grey / black backgrounds. This pipeline:
  1. Flood-fills background from the corners to chroma-key magenta #FF00FF.
  2. Does not key black clothing / hair / shoes (navy blazer, black tee, EVA).
  3. Crops to content, nearest-neighbor downscales to existing sprite height,
     pads walk frames of an era to one size, pivot-at-feet (PAD=6).
  4. Orders walk frames by foot centroid into contact, pass, opp contact, opp pass.
  5. Writes Unity TextureImporter settings like import_elon_pixel_pack.py.

Idle = smallest foot-span (most planted) frame. Portrait = head crop of idle.
"""
from __future__ import annotations

import importlib.util
import shutil
import sys
from collections import deque
from pathlib import Path

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "Assets" / "Art" / "Characters" / "Elon"
RESOURCES = ROOT / "Assets" / "Resources" / "Characters" / "Elon"
SOURCE_DIR = ART / "source_pack" / "grok-assets-20260823"
ZIP_PATH = Path.home() / "Downloads" / "grok-assets.zip"

MAGENTA = (255, 0, 255, 255)
PAD = 6
TARGET_H = 232  # match existing era idle height so 64 PPU still fits the camera

_spec = importlib.util.spec_from_file_location(
    "import_elon_pixel_pack", Path(__file__).with_name("import_elon_pixel_pack.py")
)
pack = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(pack)

ERAS = [
    (
        "01_young_sa",
        "elon_young_sa",
        [
            "grok-1d0849bc-9e71-46d7-be94-143f092fa438.jpg",
            "grok-44b7d930-7456-41ca-9143-d597e33a517f.jpg",
            "grok-a7b50057-1861-4365-8c12-7fb3bf55d81c.jpg",
            "grok-ea0fc270-9bb1-411c-8aa1-90d564145702.jpg",
        ],
    ),
    (
        "02_young_adult_90s",
        "elon_young_adult",
        [
            "grok-117bcc97-41ca-420d-bec7-21ffbe904f14.jpg",
            "grok-99c0b894-b816-4c86-b601-d3302d104f76.jpg",
            "grok-db8c5720-2406-4808-9d61-a20885bac6c8.jpg",
            "grok-e3561cc5-5a5d-4093-86a6-89b8937066c6.jpg",
        ],
    ),
    (
        "03_early_2000s",
        "elon_early2000s",
        [
            "grok-0c3e2466-8407-4095-a3d1-d455a00d71b4.jpg",
            "grok-13123556-57df-4703-8166-5686cae02010.jpg",
            "grok-e8c1edc2-7474-40d2-81c0-a018397d6fc0.jpg",
            "grok-fe9933e8-ff06-4b81-806f-4281fd1c559c.jpg",
        ],
    ),
    (
        "04_modern",
        "elon_modern",
        [
            "grok-7f5d356b-b434-489e-b2aa-ed3300622178.jpg",
            "grok-b233bb01-abd6-4887-9696-05312a20d18f.jpg",
            "grok-cee25df1-827d-4162-a975-1992dd1d7d92.jpg",
            "grok-dd31400d-e501-44b2-a9d8-9c93ed0bb879.jpg",
        ],
    ),
    (
        "05_mars",
        "elon_mars",
        [
            "grok-af159ae0-eb97-4550-aabb-b78060d73f91.jpg",
            "grok-b3449559-4a0e-4a27-ab3a-89afd1521f3c.jpg",
            "grok-d8c5a154-b599-4090-9c53-c6eef38d5258.jpg",
            "grok-e43c924e-6ed5-4e41-90c1-e18c2b0d1d74.jpg",
        ],
    ),
]


def seed_from_corners(arr: np.ndarray) -> np.ndarray:
    patches = [arr[:8, :8], arr[:8, -8:], arr[-8:, :8], arr[-8:, -8:]]
    return np.mean([p.mean(axis=(0, 1)) for p in patches], axis=0)


def flood_true(mask: np.ndarray) -> np.ndarray:
    h, w = mask.shape
    vis = np.zeros((h, w), dtype=bool)
    q: deque[tuple[int, int]] = deque()
    for x in range(w):
        if mask[0, x]:
            q.append((0, x))
        if mask[h - 1, x]:
            q.append((h - 1, x))
    for y in range(h):
        if mask[y, 0]:
            q.append((y, 0))
        if mask[y, w - 1]:
            q.append((y, w - 1))
    while q:
        y, x = q.popleft()
        if vis[y, x]:
            continue
        vis[y, x] = True
        if y > 0 and mask[y - 1, x] and not vis[y - 1, x]:
            q.append((y - 1, x))
        if y < h - 1 and mask[y + 1, x] and not vis[y + 1, x]:
            q.append((y + 1, x))
        if x > 0 and mask[y, x - 1] and not vis[y, x - 1]:
            q.append((y, x - 1))
        if x < w - 1 and mask[y, x + 1] and not vis[y, x + 1]:
            q.append((y, x + 1))
    return vis


def dilate4(mask: np.ndarray) -> np.ndarray:
    g = mask.copy()
    g[1:] |= mask[:-1]
    g[:-1] |= mask[1:]
    g[:, 1:] |= mask[:, :-1]
    g[:, :-1] |= mask[:, 1:]
    return g


def fill_holes(mask: np.ndarray) -> np.ndarray:
    inv = ~mask
    from_border = flood_true(inv)
    return mask | (inv & ~from_border)


def character_mask(arr: np.ndarray) -> np.ndarray:
    """Corner flood-fill of the background; keep clothing that matches seed (black/white)."""
    seed = seed_from_corners(arr)
    lum = float(seed.mean())
    if lum > 200:
        t_hard, t_soft, t_core, grow = 14.0, 32.0, 50.0, 10
    elif lum < 40:
        t_hard, t_soft, t_core, grow = 10.0, 22.0, 40.0, 8
    else:
        t_hard, t_soft, t_core, grow = 16.0, 36.0, 55.0, 10

    diff = arr.astype(np.int32) - seed.astype(np.int32)
    dist = np.sqrt(
        diff[:, :, 0].astype(np.float64) ** 2
        + diff[:, :, 1].astype(np.float64) ** 2
        + diff[:, :, 2].astype(np.float64) ** 2
    )
    hard = dist <= t_hard
    soft = dist <= t_soft
    core = dist > t_core
    bg = flood_true(hard)
    for _ in range(grow):
        extra = dilate4(bg) & soft & ~core & ~bg
        if not extra.any():
            break
        bg |= extra
    return fill_holes(~bg)


def key_to_magenta(im: Image.Image) -> Image.Image:
    arr = np.array(im.convert("RGB"))
    char = character_mask(arr)
    rgba = np.zeros((arr.shape[0], arr.shape[1], 4), dtype=np.uint8)
    rgba[:, :, :3] = arr
    rgba[:, :, 3] = 255
    rgba[~char] = MAGENTA
    return Image.fromarray(rgba, "RGBA")


def crop_content(im: Image.Image) -> Image.Image:
    box = pack.content_bbox(im)
    if box is None:
        return im
    x0, y0, x1, y1 = box
    return im.crop((x0, y0, x1 + 1, y1 + 1))


def downscale_era(frames: list[Image.Image], target_h: int = TARGET_H) -> list[Image.Image]:
    max_h = max(im.height for im in frames)
    factor = max(1, int(round(max_h / target_h)))
    if factor <= 1:
        return frames
    out = []
    for im in frames:
        nw = max(1, im.width // factor)
        nh = max(1, im.height // factor)
        out.append(im.resize((nw, nh), Image.Resampling.NEAREST))
    return out


def foot_metrics(im: Image.Image) -> tuple[float, float, float]:
    """Return (signed_stride, foot_span, foot_cx) from the lowest ~10% (actual feet)."""
    pix = np.array(im)
    mag = (pix[:, :, 0] > 200) & (pix[:, :, 1] < 40) & (pix[:, :, 2] > 200)
    content = ~mag
    ys, xs = np.where(content)
    if len(xs) == 0:
        return 0.0, 0.0, im.width / 2.0
    min_y, max_y = int(ys.min()), int(ys.max())
    body_cx = float(xs.mean())
    band = max(4, int((max_y - min_y + 1) * 0.10))
    split = max_y - band + 1
    lower = content.copy()
    lower[:split, :] = False
    ly, lx = np.where(lower)
    if len(lx) == 0:
        ly, lx = ys, xs
    foot_cx = float(lx.mean())
    foot_span = float(lx.max() - lx.min() + 1)
    return foot_cx - body_cx, foot_span, foot_cx


def order_walk(frames: list[Image.Image]) -> tuple[list[Image.Image], int]:
    """contact → pass → opposite contact → opposite pass. Idle index = smallest span."""
    scored = [(foot_metrics(im), i, im) for i, im in enumerate(frames)]
    idle_i = min(range(len(scored)), key=lambda k: scored[k][0][1])
    by_span = sorted(scored, key=lambda t: t[0][1])
    passes = by_span[:2]
    contacts = by_span[2:]
    passes.sort(key=lambda t: t[0][0])
    contacts.sort(key=lambda t: t[0][0])
    if len(contacts) == 2 and len(passes) == 2:
        ordered = [contacts[0][2], passes[0][2], contacts[1][2], passes[1][2]]
    else:
        scored.sort(key=lambda t: t[0][0])
        ordered = [t[2] for t in scored]
    # idle index in the *ordered* list
    idle_im = frames[idle_i]
    ordered_idle = next((k for k, im in enumerate(ordered) if im is idle_im), 0)
    return ordered, ordered_idle


def unpack_zip() -> None:
    SOURCE_DIR.mkdir(parents=True, exist_ok=True)
    if ZIP_PATH.is_file():
        import zipfile

        with zipfile.ZipFile(ZIP_PATH) as zf:
            zf.extractall(SOURCE_DIR)
        print(f"unpacked {ZIP_PATH} -> {SOURCE_DIR}")
    else:
        print(f"zip not found at {ZIP_PATH}; using existing {SOURCE_DIR}")


def remove_old_walk04(folder: str, prefix: str) -> None:
    for root in (ART, RESOURCES):
        png = root / folder / "walk" / f"{prefix}_walk_04.png"
        meta = Path(str(png) + ".meta")
        if png.exists():
            png.unlink()
            print(f"removed leftover {png.relative_to(ROOT)}")
        if meta.exists():
            meta.unlink()


def process_era(folder: str, prefix: str, names: list[str]) -> dict:
    keyed: list[Image.Image] = []
    for name in names:
        path = SOURCE_DIR / name
        if not path.is_file():
            raise FileNotFoundError(path)
        im = key_to_magenta(Image.open(path))
        keyed.append(crop_content(im))

    print(f"{folder} full-res metrics before scale:")
    for i, im in enumerate(keyed):
        stride, span, cx = foot_metrics(im)
        print(f"  src{i} {im.size} stride={stride:+.1f} span={span:.1f} foot_cx={cx:.1f}")
    ordered, idle_i = order_walk(keyed)
    ordered = downscale_era(ordered)
    ordered = [pack.crop_padded(im, PAD) for im in ordered]

    max_w = max(im.width for im in ordered)
    max_h = max(im.height for im in ordered)
    ordered = [pack.pad_to(im, max_w, max_h) for im in ordered]
    idle = ordered[idle_i]
    portrait = pack.make_portrait(idle)

    era_art = ART / folder
    era_art.mkdir(parents=True, exist_ok=True)
    walk_dir = era_art / "walk"
    walk_dir.mkdir(parents=True, exist_ok=True)

    pack.save_png(idle, era_art / f"{prefix}_idle.png")
    pack.save_png(portrait, era_art / f"{prefix}_portrait.png")
    for i, frame in enumerate(ordered):
        pack.save_png(frame, walk_dir / f"{prefix}_walk_{i:02d}.png")
    remove_old_walk04(folder, prefix)

    mag, dark, other = pack.count_stats(idle)
    metrics = [foot_metrics(im) for im in ordered]
    info = {
        "folder": folder,
        "size": idle.size,
        "mag": mag,
        "dark": dark,
        "other": other,
        "idle_i": idle_i,
        "metrics": metrics,
    }
    print(
        f"{folder}: idle={idle.size} from walk_{idle_i:02d} "
        f"mag={mag} dark={dark} other={other}"
    )
    for i, (stride, span, cx) in enumerate(metrics):
        print(f"  walk_{i:02d} stride={stride:+.1f} span={span:.1f} foot_cx={cx:.1f}")
    if dark < 50:
        print("  WARN few dark pixels — black clothing may have been keyed")
    return info


def sync_processed() -> None:
    pack.ensure_folder_metas()
    pack.folder_meta(SOURCE_DIR)
    for jpg in SOURCE_DIR.glob("*.jpg"):
        pack.write_texture_meta(jpg)

    for folder, prefix, _ in ERAS:
        pack.folder_meta(ART / folder)
        pack.folder_meta(ART / folder / "walk")
        pack.folder_meta(RESOURCES / folder)
        pack.folder_meta(RESOURCES / folder / "walk")
        rels = [
            Path(folder) / f"{prefix}_idle.png",
            Path(folder) / f"{prefix}_portrait.png",
        ]
        for i in range(4):
            rels.append(Path(folder) / "walk" / f"{prefix}_walk_{i:02d}.png")
        for rel in rels:
            src = ART / rel
            dst = RESOURCES / rel
            dst.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(src, dst)
            pack.write_texture_meta(src)
            pack.write_texture_meta(dst)


def main() -> int:
    unpack_zip()
    missing = []
    for _folder, _prefix, names in ERAS:
        for name in names:
            if not (SOURCE_DIR / name).is_file():
                missing.append(name)
    if missing:
        print("FAIL missing source files:", missing)
        return 1

    for folder, prefix, names in ERAS:
        process_era(folder, prefix, names)
    sync_processed()
    print("import complete")
    return 0


if __name__ == "__main__":
    sys.exit(main())
