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

# Triage markers, emitted by VisualMatrixTestsBase alongside the captured image.
# ##SKIA-VISUAL-CELL## path=ganesh-gl.linux/DiagonalLines.png outcome=mismatch
CELL_RE = re.compile(
    r"##SKIA-VISUAL-CELL##\s+path=(?P<path>[^\s]+)\s+outcome=(?P<outcome>\w+)"
)
# ##SKIA-VISUAL-IMAGE## path=ganesh-gl.linux/DiagonalLines.golden.png size=256x256 base64=AAAA...
IMAGE_RE = re.compile(
    r"##SKIA-VISUAL-IMAGE##\s+path=(?P<path>[^\s]+)\s+size=(?P<size>\d+x\d+)\s+base64=(?P<b64>[A-Za-z0-9+/=]+)"
)


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


def _decode_markers(text, regex, trx, into):
    """Decode {path: png_bytes} from one TRX's markers, skipping unsafe/bad ones."""
    for m in regex.finditer(text):
        path = m.group("path")
        if not SAFE_PATH_RE.match(path):
            print(f"warning: skipping unsafe path '{path}' in {trx}", file=sys.stderr)
            continue
        try:
            into[path] = base64.b64decode(m.group("b64"), validate=True)
        except Exception as ex:  # noqa: BLE001
            print(f"warning: bad base64 for '{path}' in {trx}: {ex}", file=sys.stderr)


def extract_failures(trx_files, out_dir):
    """Write browsable triage images for every failing cell, grouped by outcome.

    Reads the captured (actual) image, the per-cell outcome, and the golden/diff
    images a mismatch emits, then writes:
        {out_dir}/unseeded/{renderer}.{platform}/{scene}.actual.png
        {out_dir}/mismatch/{renderer}.{platform}/{scene}.{actual,golden,diff}.png
    so a red cell is reviewable as PNGs straight from the published TRX, and an
    unseeded cell (harvest it) is told apart from a regression (investigate it).
    Passing cells are skipped. Returns (mismatch_count, unseeded_count).
    """
    actuals, images, outcomes = {}, {}, {}
    for trx in trx_files:
        with open(trx, "r", encoding="utf-8", errors="replace") as fh:
            text = fh.read()
        _decode_markers(text, LINE_RE, trx, actuals)
        _decode_markers(text, IMAGE_RE, trx, images)
        for m in CELL_RE.finditer(text):
            if SAFE_PATH_RE.match(m.group("path")):
                outcomes[m.group("path")] = m.group("outcome")

    mismatch = unseeded = 0
    for cell_path in sorted(outcomes):
        outcome = outcomes[cell_path]
        if outcome not in ("mismatch", "unseeded"):
            continue
        rel_dir, leaf = cell_path.split("/", 1)
        base = leaf[:-4]  # strip ".png"
        dest_dir = os.path.join(out_dir, outcome, *rel_dir.split("/"))
        os.makedirs(dest_dir, exist_ok=True)

        wrote_any = False
        if cell_path in actuals:
            with open(os.path.join(dest_dir, base + ".actual.png"), "wb") as fh:
                fh.write(actuals[cell_path])
            wrote_any = True
        if outcome == "mismatch":
            for kind in ("golden", "diff"):
                key = f"{rel_dir}/{base}.{kind}.png"
                if key in images:
                    with open(os.path.join(dest_dir, f"{base}.{kind}.png"), "wb") as fh:
                        fh.write(images[key])
        if wrote_any:
            print(f"  {outcome}: {cell_path}")
        mismatch += outcome == "mismatch"
        unseeded += outcome == "unseeded"

    print(f"\nWrote {mismatch} mismatch + {unseeded} unseeded cell(s) under {out_dir}.")
    return mismatch, unseeded


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("paths", nargs="*", default=["output/logs/testlogs"],
                        help="TRX files or directories to search (default: output/logs/testlogs)")
    parser.add_argument("--out", default=os.path.join("tests", "Content", "Goldens"),
                        help="Goldens root to write into (default: tests/Content/Goldens)")
    parser.add_argument("--dry-run", action="store_true",
                        help="List the goldens that would be written without writing them")
    parser.add_argument("--failures-out", metavar="DIR", default=None,
                        help="Triage mode: instead of seeding goldens, extract actual/golden/diff "
                             "images for failing cells into DIR (grouped by outcome). Intended as an "
                             "always() CI step writing into the published test-logs artifact.")
    args = parser.parse_args(argv)

    trx_files = find_trx_files(args.paths)
    if not trx_files:
        print("No .trx files found.", file=sys.stderr)
        # In triage mode a missing TRX is not an error (the lane may have no
        # visual cells); never fail the CI step over it.
        return 0 if args.failures_out else 1

    # Triage mode: dump failure images for review; do not touch the goldens tree.
    if args.failures_out:
        print(f"Scanning {len(trx_files)} TRX file(s) for visual-regression failures...")
        extract_failures(trx_files, args.failures_out)
        return 0

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
