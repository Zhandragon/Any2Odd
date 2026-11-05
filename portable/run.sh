#!/bin/sh
set -eu
app_root=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
cd "$app_root"
echo 'Any2Odd — EXPERIMENTAL macOS/Linux build. Not tested on these operating systems.'
echo 'Samsung AI conversion and calibrated Odyssey output are not available in this build.'
case "$(uname -s)" in
  Darwin|Linux) ;;
  *) echo 'Use the native Any2Odd installer on Windows.' >&2; exit 1 ;;
esac
if ! command -v python3 >/dev/null 2>&1; then
  echo 'Python 3.10–3.13 is required. Install Python for your OS, then run this launcher again.' >&2
  exit 1
fi
python3 -c 'import sys; assert (3,10) <= sys.version_info[:2] <= (3,13), "Use Python 3.10–3.13 for this experimental package."'
if [ ! -x .venv/bin/python ]; then
  echo 'Creating an application-local Python environment. Linux may require python3-venv.'
  python3 -m venv .venv
fi
if ! .venv/bin/python -c 'import PySide6; assert PySide6.__version__ == "6.10.1"' >/dev/null 2>&1; then
  echo 'Downloading pinned Qt/PySide6 wheels from PyPI (first launch requires internet).'
  .venv/bin/python -m pip install --only-binary=:all: --requirement portable/requirements.txt
fi
exec .venv/bin/python portable/app.py "$@"
