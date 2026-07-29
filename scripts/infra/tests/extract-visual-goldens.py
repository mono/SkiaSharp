#!/usr/bin/env python3
"""Harvest golden images for the visual-regression tests from test results (TRX).

Every visual test (tests/Tests/SkiaSharp/Visual) emits its images into the test
output, one line each, all sharing one path -- the golden key:

    ##SKIA-VISUAL-ACTUAL## path={renderer}.{platform}/{scene}.png size=WxH base64=<...>
    ##SKIA-VISUAL-GOLDEN## path=... size=WxH base64=<...>
    ##SKIA-VISUAL-DIFF##   path=... size=WxH base64=<...>

No golden means none was committed, and a diff only exists when there was
something to diff against.

Those lines land in the TRX produced by every test host -- desktop Console, the
MAUI device hosts, and the WASM host alike -- which makes the TRX the one uniform
channel for seeding goldens, including on device/browser hosts whose filesystem is
sandboxed/embedded and cannot be written to in-process.

Seeding workflow:
    1. Run the suite (locally or in CI). Tests with no committed golden FAIL and
       emit their rendered PNG.
    2. Point this script at the TRX file(s) (the CI 'testlogs_*' artifacts, or a
       local output/logs/testlogs directory).
    3. Review the resulting git diff of tests/Content/Goldens/** and commit.
    4. Re-run -- the now-committed goldens are compared and pass.

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


def _marker_re(role):
    return re.compile(
        rf"##SKIA-VISUAL-{role}##\s+path=(?P<path>[^\s]+)\s+"
        r"size=(?P<size>\d+x\d+)\s+base64=(?P<b64>[A-Za-z0-9+/=]+)"
    )


ACTUAL_MARKER = "##SKIA-VISUAL-ACTUAL##"
MARKERS = {role: _marker_re(role.upper()) for role in ("actual", "golden", "diff")}

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
        for m in MARKERS["actual"].finditer(text):
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


def extract_images(trx_files, out_dir):
    """Write every emitted image as a browsable PNG, mirroring the goldens tree.

    Produces {out_dir}/{renderer}.{platform}/{scene}.{actual,golden,diff}.png. A
    scene with only an .actual.png had no golden committed; one with all three was
    compared, and the test result says whether it passed. Returns the file count.
    """
    written = 0
    for trx in trx_files:
        with open(trx, "r", encoding="utf-8", errors="replace") as fh:
            text = fh.read()
        for role, regex in MARKERS.items():
            for m in regex.finditer(text):
                path = m.group("path")
                if not SAFE_PATH_RE.match(path):
                    print(f"warning: skipping unsafe path '{path}' in {trx}", file=sys.stderr)
                    continue
                try:
                    data = base64.b64decode(m.group("b64"), validate=True)
                except Exception as ex:  # noqa: BLE001
                    print(f"warning: bad base64 for '{path}' in {trx}: {ex}", file=sys.stderr)
                    continue
                rel_dir, leaf = path.split("/", 1)
                dest_dir = os.path.join(out_dir, *rel_dir.split("/"))
                os.makedirs(dest_dir, exist_ok=True)
                with open(os.path.join(dest_dir, f"{leaf[:-4]}.{role}.png"), "wb") as out:
                    out.write(data)
                written += 1

    print(f"Wrote {written} image(s) under {out_dir}.")
    return written


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("paths", nargs="*", default=["output/logs/testlogs"],
                        help="TRX files or directories to search (default: output/logs/testlogs)")
    parser.add_argument("--out", default=os.path.join("tests", "Content", "Goldens"),
                        help="Goldens root to write into (default: tests/Content/Goldens)")
    parser.add_argument("--dry-run", action="store_true",
                        help="List the goldens that would be written without writing them")
    parser.add_argument("--images-out", metavar="DIR", default=None,
                        help="Instead of seeding goldens, write every emitted actual/golden/diff "
                             "image into DIR as browsable PNGs. Intended as an always() CI step "
                             "writing into the published test-logs artifact.")
    args = parser.parse_args(argv)

    trx_files = find_trx_files(args.paths)
    if not trx_files:
        print("No .trx files found.", file=sys.stderr)
        # A missing TRX is not an error when extracting images (the lane may have
        # no visual tests); never fail the CI step over it.
        return 0 if args.images_out else 1

    if args.images_out:
        print(f"Scanning {len(trx_files)} TRX file(s) for visual-regression images...")
        extract_images(trx_files, args.images_out)
        return 0

    print(f"Scanning {len(trx_files)} TRX file(s) for {ACTUAL_MARKER} markers...")
    goldens = extract(trx_files)
    if not goldens:
        print(f"No {ACTUAL_MARKER} markers found. Did the visual tests run and emit images?", file=sys.stderr)
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
