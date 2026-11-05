#!/bin/sh
app_root=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
exec sh "$app_root/portable/run.sh" "$@"
