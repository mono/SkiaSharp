#!/usr/bin/env python3
"""Harvest golden images for the visual-regression matrix from test results (TRX).

Every cell of the visual matrix (tests/Tests/SkiaSharp/Visual) emits its rendered
PNG into the test output as a single line:

    ##SKIA-GOLDEN-IMAGE## path={renderer}.{platform}/{scene}.png size=WxH base64=<...>

That line lands in the TRX produced by every test host -- desktop Console, the
MAUI device hosts, and the WASM host alike -- which makes the TRX the one uniform
channel for seeding goldens, including on device/browser hosts whose filesystem is
sandboxed/embedded and cannot be written to in-process.

Seeding workflow:
    1. Run the matrix (locally or in CI). Cells with no committed golden FAIL, but
       still emit their PNG marker.
    2. Point this script at the TRX file(s) (the CI 'testlogs_*' artifacts, or a
       local output/logs/testlogs directory).
    3. Review the resulting git diff of tests/Content/Goldens/** and commit.
    4. Re-run -- the now-committed goldens are compared strictly and pass.

Usage:
    python3 scripts/infra/tests/extract-visual-goldens.py [PATH ...]
        [--out tests/Content/Goldens] [--dry-run]

PATH may be a .trx file or a directory (searched recursively for *.trx).
Defaults to output/logs/testlogs.
"""

import argparse
import base64
import os
import re
import sys

MARKER = "##SKIA-GOLDEN-IMAGE##"
# ##SKIA-GOLDEN-IMAGE## path=ganesh-gl.macos/DiagonalLines.png size=256x256 base64=AAAA...
LINE_RE = re.compile(
    r"##SKIA-GOLDEN-IMAGE##\s+path=(?P<path>[^\s]+)\s+size=(?P<size>\d+x\d+)\s+base64=(?P<b64>[A-Za-z0-9+/=]+)"
)

# Golden paths are always "{renderer}.{platform}/{scene}.png": exactly one
# subdirectory, a .png leaf, and no traversal. Reject anything else so a malformed
# or hostile marker can never write outside the goldens tree.
SAFE_PATH_RE = re.compile(r"^[A-Za-z0-9][\w.\-]*/[A-Za-z0-9][\w.\-]*\.png$")

# Splits "{renderer}.{platform}/{scene}.png" into its shared, platform-portable
# counterpart "{renderer}/{scene}.png" (renderer is everything before the last dot
# of the directory). Returns None when the directory has no platform suffix (the
# path is already the shared form).
PLATFORM_DIR_RE = re.compile(r"^(?P<renderer>.+)\.(?P<platform>[^.]+)/(?P<scene>.+\.png)$")


def shared_path_for(path):
    m = PLATFORM_DIR_RE.match(path)
    if not m:
        return None
    return f"{m.group('renderer')}/{m.group('scene')}"


def read_bytes(path):
    with open(path, "rb") as fh:
        return fh.read()


def find_trx_files(paths):
    files = []
    for p in paths:
        if os.path.isdir(p):
            for root, _, names in os.walk(p):
                for name in names:
                    if name.lower().endswith(".trx"):
                        files.append(os.path.join(root, name))
        elif os.path.isfile(p):
            files.append(p)
        else:
            print(f"warning: '{p}' does not exist, skipping", file=sys.stderr)
    return sorted(set(files))


def extract(trx_files):
    """Return {golden_path: png_bytes}. Later files win; conflicting bytes warn."""
    found = {}
    sources = {}
    for trx in trx_files:
        with open(trx, "r", encoding="utf-8", errors="replace") as fh:
            text = fh.read()
        for m in LINE_RE.finditer(text):
            path = m.group("path")
            if not SAFE_PATH_RE.match(path):
                print(f"warning: skipping unsafe golden path '{path}' in {trx}", file=sys.stderr)
                continue
            try:
                data = base64.b64decode(m.group("b64"), validate=True)
            except Exception as ex:  # noqa: BLE001
                print(f"warning: bad base64 for '{path}' in {trx}: {ex}", file=sys.stderr)
                continue
            if path in found and found[path] != data:
                print(
                    f"note: '{path}' differs between {sources[path]} and {trx}; "
                    f"using the latter (a tolerance-level difference between hosts is normal).",
                    file=sys.stderr,
                )
            found[path] = data
            sources[path] = trx
    return found


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("paths", nargs="*", default=["output/logs/testlogs"],
                        help="TRX files or directories to search (default: output/logs/testlogs)")
    parser.add_argument("--out", default=os.path.join("tests", "Content", "Goldens"),
                        help="Goldens root to write into (default: tests/Content/Goldens)")
    parser.add_argument("--dry-run", action="store_true",
                        help="List the goldens that would be written without writing them")
    args = parser.parse_args(argv)

    trx_files = find_trx_files(args.paths)
    if not trx_files:
        print("No .trx files found.", file=sys.stderr)
        return 1

    print(f"Scanning {len(trx_files)} TRX file(s) for {MARKER} markers...")
    goldens = extract(trx_files)
    if not goldens:
        print(f"No {MARKER} markers found. Did the visual matrix run and emit images?", file=sys.stderr)
        return 1

    written = 0
    covered = 0
    for path in sorted(goldens):
        data = goldens[path]
        # A promoted, platform-portable golden lives at {renderer}/{scene}.png.
        # If one already exists and is byte-identical to this capture, the shared
        # golden already covers this platform: don't re-create the per-platform
        # copy (that is what keeps a manual promotion from being clobbered on the
        # next harvest). A genuine per-platform divergence is byte-different, so it
        # is still written as an override.
        shared = shared_path_for(path)
        if shared is not None:
            shared_dest = os.path.join(args.out, *shared.split("/"))
            if os.path.isfile(shared_dest) and read_bytes(shared_dest) == data:
                print(f"  skip {path} (covered by shared {shared})")
                covered += 1
                continue

        dest = os.path.join(args.out, *path.split("/"))
        if args.dry_run:
            print(f"  would write {dest} ({len(data)} bytes)")
            continue
        os.makedirs(os.path.dirname(dest), exist_ok=True)
        with open(dest, "wb") as fh:
            fh.write(data)
        print(f"  wrote {dest} ({len(data)} bytes)")
        written += 1

    suffix = f" ({covered} already covered by a shared golden)" if covered else ""
    if args.dry_run:
        print(f"\n{len(goldens) - covered} golden(s) would be written under {args.out}{suffix}. "
              "Re-run without --dry-run to write them.")
    else:
        print(f"\nWrote {written} golden(s) under {args.out}{suffix}. Review the diff and commit.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
