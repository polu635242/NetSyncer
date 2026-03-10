#!/usr/bin/env python3
"""Download and extract a portable .NET SDK.

Examples:
  python tools/download_dotnet.py --channel 8.0 --install-dir third_party/dotnet
  python tools/download_dotnet.py --channel 8.0 --os windows --arch x64 --install-dir third_party/dotnet
"""

from __future__ import annotations

import argparse
import os
import platform
import shutil
import sys
import tarfile
import urllib.request
import zipfile
from pathlib import Path


def detect_defaults() -> tuple[str, str]:
    sysname = platform.system().lower()
    machine = platform.machine().lower()

    if sysname.startswith("linux"):
        os_name = "linux"
    elif sysname.startswith("darwin"):
        os_name = "osx"
    elif sysname.startswith("windows"):
        os_name = "windows"
    else:
        raise RuntimeError(f"Unsupported OS: {sysname}")

    arch_map = {
        "x86_64": "x64",
        "amd64": "x64",
        "aarch64": "arm64",
        "arm64": "arm64",
    }
    arch = arch_map.get(machine)
    if not arch:
        raise RuntimeError(f"Unsupported architecture: {machine}")

    return os_name, arch


def build_url(channel: str, os_name: str, arch: str) -> tuple[str, str]:
    ext = "zip" if os_name == "windows" else "tar.gz"
    filename = f"dotnet-sdk-{os_name}-{arch}.{ext}"
    # aka.ms returns a redirect to the latest SDK in the selected channel.
    url = f"https://aka.ms/dotnet/{channel}/{filename}"
    return url, filename


def download_file(url: str, target: Path) -> None:
    with urllib.request.urlopen(url) as response, open(target, "wb") as out:
        shutil.copyfileobj(response, out)


def extract_archive(archive: Path, install_dir: Path) -> None:
    install_dir.mkdir(parents=True, exist_ok=True)
    name = archive.name.lower()
    if name.endswith(".zip"):
        with zipfile.ZipFile(archive, "r") as zf:
            zf.extractall(install_dir)
    elif name.endswith(".tar.gz"):
        with tarfile.open(archive, "r:gz") as tf:
            tf.extractall(install_dir)
    else:
        raise RuntimeError(f"Unsupported archive format: {archive}")


def main() -> int:
    parser = argparse.ArgumentParser(description="Download and extract a portable .NET SDK")
    parser.add_argument("--channel", default="8.0", help="SDK channel, e.g. 8.0, 9.0")
    parser.add_argument("--os", dest="os_name", choices=["linux", "osx", "windows"], help="Target OS")
    parser.add_argument("--arch", choices=["x64", "arm64"], help="Target architecture")
    parser.add_argument("--install-dir", default="third_party/dotnet", help="Install directory")
    parser.add_argument("--keep-archive", action="store_true", help="Keep downloaded archive")
    parser.add_argument("--dry-run", action="store_true", help="Print URL and exit")
    args = parser.parse_args()

    default_os, default_arch = detect_defaults()
    os_name = args.os_name or default_os
    arch = args.arch or default_arch

    url, filename = build_url(args.channel, os_name, arch)
    install_dir = Path(args.install_dir).resolve()
    archive = install_dir.parent / filename

    print(f"[info] Channel: {args.channel}")
    print(f"[info] Target:  {os_name}-{arch}")
    print(f"[info] URL:     {url}")
    print(f"[info] Install: {install_dir}")

    if args.dry_run:
        return 0

    archive.parent.mkdir(parents=True, exist_ok=True)
    print("[run] Downloading .NET SDK archive...")
    download_file(url, archive)

    print("[run] Extracting archive...")
    extract_archive(archive, install_dir)

    dotnet_bin = install_dir / ("dotnet.exe" if os_name == "windows" else "dotnet")
    if dotnet_bin.exists():
        print(f"[ok] Done. dotnet binary: {dotnet_bin}")
        if os_name != "windows":
            os.chmod(dotnet_bin, os.stat(dotnet_bin).st_mode | 0o111)
    else:
        print("[warn] Extraction finished, but dotnet binary was not found in expected location.")

    if not args.keep_archive and archive.exists():
        archive.unlink()
        print(f"[info] Removed archive: {archive}")

    print("[hint] Add to PATH before build:")
    if os_name == "windows":
        print(f"       set DOTNET_ROOT={install_dir}")
        print(f"       set PATH=%DOTNET_ROOT%;%PATH%")
    else:
        print(f"       export DOTNET_ROOT='{install_dir}'")
        print("       export PATH=\"$DOTNET_ROOT:$PATH\"")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
