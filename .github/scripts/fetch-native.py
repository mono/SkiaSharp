#!/usr/bin/env python3
"""Stage the nightly libSkiaSharp native binary for this runner into output/native/.

Used by the "pr" (source-build) benchmark leg: the managed SkiaSharp is built from
the checkout, but the native binary comes from the published nightly package (a
from-scratch Skia build is too heavy for GitHub-hosted runners). The layout matches
binding/IncludeNativeAssets.SkiaSharp.targets so the source project picks it up.

Usage: fetch-native.py <os-label> <version>
    os-label : Linux | Windows | macOS
    version  : SkiaSharp package version (e.g. the resolved nightly)

Only the Python standard library is used.
"""

from __future__ import annotations

import io
import json
import os
import pathlib
import sys
import time
import urllib.error
import urllib.request
import zipfile

EAP_INDEX_URL = "https://aka.ms/skiasharp-eap/index.json"
USER_AGENT = "skiasharp-benchmark-native-fetch/1.0 (+https://github.com/mono/SkiaSharp)"
ROOT = pathlib.Path(__file__).resolve().parents[2]

# os-label -> (package id, entry inside the nupkg, output path under output/native/).
# The output paths mirror IncludeNativeAssets.SkiaSharp.targets (BuildArch = x64 here).
TARGETS = {
    "Linux": ("SkiaSharp.NativeAssets.Linux",
              "runtimes/linux-x64/native/libSkiaSharp.so",
              "linux/x64/libSkiaSharp.so"),
    "Windows": ("SkiaSharp.NativeAssets.Win32",
                "runtimes/win-x64/native/libSkiaSharp.dll",
                "windows/x64/libSkiaSharp.dll"),
    "macOS": ("SkiaSharp.NativeAssets.macOS",
              "runtimes/osx/native/libSkiaSharp.dylib",
              "osx/libSkiaSharp.dylib"),
}


def _log(msg: str) -> None:
    print(msg, file=sys.stderr, flush=True)


def _get(url: str, retries: int = 4, timeout: int = 120) -> bytes:
    last: Exception | None = None
    for attempt in range(1, retries + 1):
        try:
            req = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})
            with urllib.request.urlopen(req, timeout=timeout) as resp:
                return resp.read()
        except (urllib.error.URLError, TimeoutError, ConnectionError) as err:
            last = err
            time.sleep(min(30, 2 ** attempt))
    raise RuntimeError(f"GET failed after {retries} attempts: {url}\n  {last}")


def _flatcontainer_base() -> str:
    index = json.loads(_get(EAP_INDEX_URL))
    base = next((r["@id"] for r in index.get("resources", [])
                 if str(r.get("@type", "")).startswith("PackageBaseAddress/3.0.0")), None)
    if not base:
        raise RuntimeError("No PackageBaseAddress in the EAP service index")
    return base if base.endswith("/") else base + "/"


def main(argv: list[str]) -> int:
    if len(argv) != 2:
        print("usage: fetch-native.py <os-label> <version>", file=sys.stderr)
        return 2
    os_label, version = argv
    if os_label not in TARGETS:
        print(f"unknown os-label {os_label!r}; expected one of {list(TARGETS)}", file=sys.stderr)
        return 2

    package, entry, out_rel = TARGETS[os_label]
    base = _flatcontainer_base()
    url = f"{base}{package.lower()}/{version}/{package.lower()}.{version}.nupkg"
    _log(f"  downloading {package} {version}")
    zf = zipfile.ZipFile(io.BytesIO(_get(url)))

    if entry not in zf.namelist():
        # Fall back to any libSkiaSharp.* under runtimes/ (guards against layout drift).
        candidates = [n for n in zf.namelist()
                      if n.startswith("runtimes/") and "libSkiaSharp" in n and "/native/" in n]
        if not candidates:
            raise RuntimeError(f"{entry} not found in {package} {version}")
        entry = candidates[0]

    dest = ROOT / "output" / "native" / out_rel
    dest.parent.mkdir(parents=True, exist_ok=True)
    with zf.open(entry) as src, open(dest, "wb") as out:
        out.write(src.read())
    _log(f"  staged {entry} -> {dest} ({dest.stat().st_size} bytes)")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
