#!/usr/bin/env python3
"""Export unversioned ##SKIA-GOLDEN## records from TRX.

Stage actual images in output/golden-proposals for review, or export all roles
with --images-out. Neither operation approves a new reference automatically.
Keys are relative to the consumer's configured golden root.
"""

import argparse
import base64
import hashlib
from pathlib import Path
import re
import struct
import sys
import zlib

PNG = b"\x89PNG\r\n\x1a\n"
SLUG = re.compile(r"[a-z0-9]+(?:-[a-z0-9]+)*\Z")
CASE_SEGMENT = re.compile(r"[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*\Z")
RESERVED = re.compile(r"(?:con|prn|aux|nul|com[1-9]|lpt[1-9])\Z", re.IGNORECASE)
ARCHITECTURES = {"x86", "x64", "arm", "arm64", "armv6", "wasm", "s390x", "loongarch64", "ppc64le", "riscv64"}
MARKER = re.compile(
    r"##SKIA-GOLDEN## role=(actual|reference|diff) key=(\S+) "
    r"size=(\d+)x(\d+) base64=([A-Za-z0-9+/=]+)"
)


def validate_key(key):
    parts = key.split("/")
    if len(parts) < 2:
        raise ValueError(f"Expected platform/[version/]case[.variant][.architecture].png: {key}")
    platform, *case = parts
    if len(case) >= 2 and re.fullmatch(r"(0|[1-9][0-9]*)(\.(0|[1-9][0-9]*)){2,3}", case[0]):
        if any(int(component) > 2147483647 for component in case[0].split(".")):
            raise ValueError(f"Version component is out of range: {key}")
        case.pop(0)
    elif len(case) >= 2 and "." in case[0] and case[0][0].isdigit():
        raise ValueError(f"Expected a normalized three- or four-component System.Version: {key}")
    filename = case[-1]
    if not filename.endswith(".png"):
        raise ValueError(f"Expected a PNG filename: {key}")
    names = filename[:-4].split(".")
    if not 1 <= len(names) <= 3:
        raise ValueError(f"Too many filename suffixes: {key}")
    if len(names) == 3 and (names[1] in ARCHITECTURES or names[2] not in ARCHITECTURES):
        raise ValueError(f"Variant must precede a recognized architecture: {key}")
    for name in [platform, *names[1:]]:
        if not SLUG.fullmatch(name) or RESERVED.fullmatch(name):
            raise ValueError(f"Unsafe golden path: {key}")
    for name in [*case[:-1], names[0]]:
        if not CASE_SEGMENT.fullmatch(name) or RESERVED.fullmatch(name):
            raise ValueError(f"Unsafe golden case path: {key}")


def png_size(data):
    if not data.startswith(PNG):
        raise ValueError("Capture is not a PNG")
    offset = len(PNG)
    size = None
    has_pixels = False
    while offset + 12 <= len(data):
        length = struct.unpack_from(">I", data, offset)[0]
        kind = data[offset + 4:offset + 8]
        end = offset + 12 + length
        if end > len(data):
            raise ValueError("Truncated PNG chunk")
        payload = data[offset + 8:end - 4]
        if zlib.crc32(kind + payload) != struct.unpack_from(">I", data, end - 4)[0]:
            raise ValueError("PNG chunk checksum does not match")
        if size is None:
            if kind != b"IHDR" or length != 13:
                raise ValueError("PNG must begin with IHDR")
            size = struct.unpack_from(">II", payload)
            if not all(size):
                raise ValueError("PNG dimensions must be positive")
        elif kind == b"IHDR":
            raise ValueError("PNG has multiple IHDR chunks")
        has_pixels |= kind == b"IDAT"
        if kind == b"IEND":
            if length or end != len(data) or not has_pixels:
                raise ValueError("Invalid PNG ending or missing pixels")
            return size
        offset = end
    raise ValueError("PNG is missing IEND")


def contained(root, key):
    validate_key(key)
    target = root.joinpath(*key.split("/"))
    if not target.resolve().is_relative_to(root.resolve()):
        raise ValueError(f"Output escapes destination: {key}")
    return target


def trx_files(paths):
    result = set()
    for name in paths:
        path = Path(name)
        if path.is_dir():
            result.update(path.rglob("*.trx"))
        elif path.is_file():
            result.add(path)
        else:
            raise ValueError(f"Results do not exist: {path}")
    return sorted(result)


def records(path):
    text = path.read_text(encoding="utf-8")
    for line in text.splitlines():
        if line.lstrip().startswith("##SKIA-GOLDEN##") and MARKER.search(line) is None:
            raise ValueError(f"Malformed golden record in {path}")
    for match in MARKER.finditer(text):
        role, key, width, height, encoded = match.groups()
        validate_key(key)
        data = base64.b64decode(encoded, validate=True)
        if png_size(data) != (int(width), int(height)):
            raise ValueError(f"PNG dimensions disagree with marker: {key}")
        yield role, key, data


def proposals(files):
    result = {}
    for path in files:
        for role, key, data in records(path):
            if role != "actual":
                continue
            if key in result and result[key] != data:
                raise ValueError(f"Conflicting captures for {key}; review each run instead of choosing a winner")
            result[key] = data
    return result


def export_images(files, destination):
    count = 0
    for path in files:
        run = path.stem + "-" + hashlib.sha256(str(path).encode()).hexdigest()[:8]
        occurrences = {}
        for role, key, data in records(path):
            number = occurrences.get((role, key), 0) + 1
            occurrences[role, key] = number
            output = destination.joinpath(run, *key.split("/"))
            output = output.with_name(f"{output.stem}.{number}.{role}.png")
            if not output.resolve().is_relative_to(destination.resolve()):
                raise ValueError(f"Output escapes destination: {key}")
            output.parent.mkdir(parents=True, exist_ok=True)
            output.write_bytes(data)
            count += 1
    return count


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("paths", nargs="*", default=["output/logs/testlogs"])
    parser.add_argument("--out", default="output/golden-proposals")
    parser.add_argument("--images-out")
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args(argv)
    try:
        if args.images_out and not any(Path(name).exists() for name in args.paths):
            print("No test results were produced; no diagnostic images to export.")
            return 0
        files = trx_files(args.paths)
        if args.images_out:
            print(f"Exported {export_images(files, Path(args.images_out))} diagnostic images.")
            return 0
        captures = proposals(files)
        if not captures:
            raise ValueError("No actual golden captures found")
        for key, data in sorted(captures.items()):
            destination = contained(Path(args.out), key)
            print(f"{'Would stage' if args.dry_run else 'Staging'} {destination}")
            if not args.dry_run:
                if destination.exists() and destination.read_bytes() != data:
                    raise ValueError(f"Conflicting proposal already exists: {destination}")
                destination.parent.mkdir(parents=True, exist_ok=True)
                destination.write_bytes(data)
        print("Review proposals and rerun the supported profiles before committing references.")
        return 0
    except (ValueError, OSError) as error:
        print(f"error: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
