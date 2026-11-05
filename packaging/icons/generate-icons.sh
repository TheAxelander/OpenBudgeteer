#!/usr/bin/env bash
# Generates all platform-specific icon files from assets/icon.png
# Requires ImageMagick (magick) and Python 3 with Pillow
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOURCE="${SCRIPT_DIR}/../../assets/icon.png"
APP_ID="io.github.TheAxelander.OpenBudgeteer"

cd "${SCRIPT_DIR}"

# Windows (Executable, Window and Velopack Setup)
magick "${SOURCE}" -background none -define icon:auto-resize=256,128,64,48,32,24,16 openbudgeteer.ico

# macOS (App Bundle)
python3 -I - "${SOURCE}" openbudgeteer.icns <<'EOF'
import sys
from PIL import Image
Image.open(sys.argv[1]).save(sys.argv[2], format="ICNS")
EOF

# Linux (Window, hicolor icon theme for AppImage and Flatpak)
magick "${SOURCE}" -resize 256x256 openbudgeteer.png
for size in 16 24 32 48 64 128 256 512; do
    mkdir -p "hicolor/${size}x${size}/apps"
    magick "${SOURCE}" -resize "${size}x${size}" "hicolor/${size}x${size}/apps/${APP_ID}.png"
done
