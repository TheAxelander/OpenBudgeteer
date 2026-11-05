#!/usr/bin/env bash
# Installs the .desktop file and icons for the current user, so the Window and Task Bar icon also show up when
# running OpenBudgeteer.Desktop from source (e.g. dotnet run). Packaged builds (AppImage, Flatpak) bring their own.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DATA_HOME="${XDG_DATA_HOME:-${HOME}/.local/share}"
APP_ID="io.github.TheAxelander.OpenBudgeteer"

install -Dm644 "${SCRIPT_DIR}/${APP_ID}.desktop" "${DATA_HOME}/applications/${APP_ID}.desktop"
for icon in "${SCRIPT_DIR}"/../icons/hicolor/*/apps/"${APP_ID}.png"; do
    size_dir="$(basename "$(dirname "$(dirname "${icon}")")")"
    install -Dm644 "${icon}" "${DATA_HOME}/icons/hicolor/${size_dir}/apps/${APP_ID}.png"
done

update-desktop-database "${DATA_HOME}/applications" 2>/dev/null || true
gtk-update-icon-cache -f -t "${DATA_HOME}/icons/hicolor" 2>/dev/null || true
echo "Installed ${APP_ID}.desktop and icons to ${DATA_HOME}"
