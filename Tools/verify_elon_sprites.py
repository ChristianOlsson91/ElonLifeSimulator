#!/usr/bin/env python3
"""Structural check for Elon era sprite deliverables (shipped art paths)."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1] / "Assets" / "Art" / "Characters" / "Elon"

ERAS = [
    ("01_young_sa", "elon_young_sa"),
    ("02_young_adult_90s", "elon_young_adult"),
    ("03_early_2000s", "elon_early2000s"),
    ("04_modern", "elon_modern"),
    ("05_mars", "elon_mars"),
]


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
        ok = idle.is_file() and portrait.is_file() and 4 <= len(walks) <= 6
        print(
            f"{'PASS' if ok else 'FAIL'} {folder}: "
            f"idle={idle.is_file()} portrait={portrait.is_file()} walk={len(walks)}"
        )
        if not ok:
            failed += 1
    # Required four eras (Mars optional for pass in plan, but present here)
    for folder, _ in ERAS[:4]:
        if not (ROOT / folder).is_dir():
            print(f"FAIL required era missing: {folder}")
            failed += 1
    print(f"Results: {len(ERAS) - failed} ok, {failed} failed")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
