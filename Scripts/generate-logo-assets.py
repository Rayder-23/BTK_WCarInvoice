"""
Derive CarInvoice logo assets from Design/logo-master.png.

Usage (repo root):
  python scripts/generate-logo-assets.py

Writes:
  CarInvoice.Desktop/Assets/logo-512.png
  CarInvoice.Desktop/Assets/logo-128.png
  CarInvoice.Desktop/Assets/app.ico  (16,24,32,48,64,128,256)
"""

from __future__ import annotations

import struct
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
MASTER = ROOT / "Design" / "logo-master.png"
ASSETS = ROOT / "CarInvoice.Desktop" / "Assets"
ICO_SIZES = [(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]


def main() -> None:
    if not MASTER.is_file():
        raise SystemExit(f"Missing master: {MASTER}")

    ASSETS.mkdir(parents=True, exist_ok=True)
    m = Image.open(MASTER).convert("RGBA")

    m.resize((512, 512), Image.Resampling.LANCZOS).save(ASSETS / "logo-512.png", optimize=True)
    m.resize((128, 128), Image.Resampling.LANCZOS).save(ASSETS / "logo-128.png", optimize=True)
    m.resize((256, 256), Image.Resampling.LANCZOS).save(
        ASSETS / "app.ico", format="ICO", sizes=ICO_SIZES
    )

    data = (ASSETS / "app.ico").read_bytes()
    n = struct.unpack("<H", data[4:6])[0]
    print(f"Wrote assets under {ASSETS}")
    print(f"ICO frames: {n}")
    for i in range(n):
        w, h, _, _, _, bpp, sz, _ = struct.unpack("<BBBBHHII", data[6 + 16 * i : 22 + 16 * i])
        print(f"  {w or 256}x{h or 256} bpp={bpp} bytes={sz}")


if __name__ == "__main__":
    main()
