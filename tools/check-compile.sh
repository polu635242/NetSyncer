#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT_DIR"

echo "[info] Project: $ROOT_DIR"
if [[ -f ProjectSettings/ProjectVersion.txt ]]; then
  echo "[info] Unity version:"
  cat ProjectSettings/ProjectVersion.txt
fi

if command -v unity >/dev/null 2>&1; then
  echo "[run] Unity batchmode compile check"
  unity -batchmode -nographics -quit -projectPath "$ROOT_DIR" -logFile "$ROOT_DIR/unity-compile.log"
  echo "[ok] Unity compile completed. Log: unity-compile.log"
  exit 0
fi

if command -v dotnet >/dev/null 2>&1; then
  echo "[warn] dotnet detected, but Unity APIs require Unity Editor assemblies for full compile validation."
  dotnet --version
  exit 0
fi

if command -v mcs >/dev/null 2>&1 || command -v csc >/dev/null 2>&1; then
  echo "[warn] C# compiler detected, but Unity APIs require Unity Editor assemblies for full compile validation."
  exit 0
fi

echo "[error] No usable compiler toolchain found (unity/dotnet/mcs/csc)."
echo "[hint] Install Unity Editor 6000.0.58f2 and rerun this script for authoritative compile results."
exit 2
