#!/usr/bin/env python3
"""Structural check for Elon era sprite deliverables (shipped art paths)."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1] / "Assets" / "Art" / "Characters" / "Elon"
RESOURCES = Path(__file__).resolve().parents[1] / "Assets" / "Resources" / "Characters" / "Elon"

ERAS = [
    ("01_young_sa", "elon_young_sa"),
    ("02_young_adult_90s", "elon_young_adult"),
    ("03_early_2000s", "elon_early2000s"),
    ("04_modern", "elon_modern"),
    ("05_mars", "elon_mars"),
]


def _chroma_and_dark(path: Path) -> tuple[int, int] | None:
    try:
        from PIL import Image
    except ImportError:
        return None
    im = Image.open(path).convert("RGBA")
    mag = dark = 0
    for px in im.getdata():
        r, g, b, _a = px
        if r > 200 and g < 40 and b > 200:
            mag += 1
        elif r < 40 and g < 40 and b < 40:
            dark += 1
    return mag, dark


def main() -> int:
    if not ROOT.is_dir():
        print(f"FAIL missing root {ROOT}")
        return 1
    failed = 0
    for folder, prefix in ERAS:
        d = ROOT / folder
        idle = d / f"{prefix}_idle.png"
        portrait = d / f"{prefix}_portrait.png"
        walks = sorted((d / "walk").glob(f"{prefix}_walk_*.png")) if (d / "walk").is_dir() else []
        res_idle = RESOURCES / folder / f"{prefix}_idle.png"
        ok = idle.is_file() and portrait.is_file() and 4 <= len(walks) <= 6 and res_idle.is_file()
        print(
            f"{'PASS' if ok else 'FAIL'} {folder}: "
            f"idle={idle.is_file()} portrait={portrait.is_file()} walk={len(walks)} "
            f"resources={res_idle.is_file()}"
        )
        if not ok:
            failed += 1
            continue
        stats = _chroma_and_dark(idle)
        if stats is None:
            continue
        mag, dark = stats
        chroma_ok = mag > 100
        # Modern black tee / school shoes must survive magenta-only keying.
        dark_ok = dark > 50
        extra = f"  chroma_magenta={mag} dark_pixels={dark}"
        if chroma_ok and dark_ok:
            print(f"PASS {folder} pixels:{extra}")
        else:
            print(f"FAIL {folder} pixels:{extra}")
            failed += 1
    for folder, _ in ERAS[:4]:
        if not (ROOT / folder).is_dir():
            print(f"FAIL required era missing: {folder}")
            failed += 1
    print(f"Results: {len(ERAS) - failed} ok, {failed} failed")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
