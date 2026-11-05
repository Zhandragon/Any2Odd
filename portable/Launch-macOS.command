#!/bin/sh
app_root=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
sh "$app_root/portable/run.sh" "$@"
status=$?
if [ "$status" -ne 0 ]; then
  printf '\nLauncher stopped (exit %s). Read the error above and START-HERE.md. Press Return to close.\n' "$status"
  read -r reply
fi
exit "$status"
