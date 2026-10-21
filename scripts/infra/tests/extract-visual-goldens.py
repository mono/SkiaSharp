#!/usr/bin/env python3
"""Validate canonical goldens or export reviewed-reference proposals from TRX.

Default export stages exact-environment captures in output/golden-proposals.
It never updates a selected shared baseline. Review proposals before promoting
them to tests/Goldens/v1. Historical v1 markers support diagnostics only.
"""

import argparse
import base64
import hashlib
from pathlib import Path
import re
import struct
import sys

PROFILES = {
    "portable", "mac", "windows", "linux", "ios", "android", "maccatalyst",
    "tvos", "browser", "windows-nanoserver", "windows-client", "windows-server", "windows-servercore",
    "ubuntu", "alpine", "azurelinux", "debian", "iphone-16-pro", "pixel-9",
}
ARCHITECTURES = {"arm", "arm64", "x86", "x64", "wasm", "loongarch64", "riscv64"}
BACKENDS = {
    "raster", "ganesh-gl", "ganesh-metal", "ganesh-vulkan",
    "ganesh-vulkan-sharpvk", "ganesh-direct3d", "graphite-metal",
    "graphite-vulkan", "graphite-dawn",
}
ENGINES = {"chromium", "firefox", "webkit"}
NEUTRAL = {"portable", "windows", "linux", "browser"}
SLUG = re.compile(r"[a-z][a-z0-9]*(?:-[a-z0-9]+)*\Z")
RESERVED = re.compile(r"(?:con|prn|aux|nul|com[1-9]|lpt[1-9])\Z")
V2 = re.compile(
    r"##SKIA-GOLDEN-V2## role=(actual|reference|diff) key=(\S+) selected=(\S+) "
    r"size=(\d+)x(\d+) base64=([A-Za-z0-9+/=]+)"
)
LEGACY = re.compile(
    r"##SKIA-VISUAL-(ACTUAL|GOLDEN|DIFF)## path=([A-Za-z0-9][\w.-]*/[A-Za-z0-9][\w.-]*\.png) "
    r"size=\d+x\d+ base64=([A-Za-z0-9+/=]+)"
)
PNG = b"\x89PNG\r\n\x1a\n"


def validate_key(key):
    """Parse the same strict grammar as GoldenKey; never infer driver identity."""
    parts = key.split("/")
    if len(parts) != 3 or parts[0] not in PROFILES or "\\" in key:
        raise ValueError(f"Invalid canonical golden key: {key}")
    profile, version, filename = parts
    if version != "base":
        if profile in NEUTRAL:
            raise ValueError(f"Profile {profile} permits only base references")
        values = version.split(".")
        if profile in {"android", "pixel-9"} and values[-1].startswith("r"):
            revision = values.pop()[1:]
            if not re.fullmatch(r"[1-9][0-9]*", revision):
                raise ValueError(f"Invalid Android revision: {version}")
        for i, value in enumerate(values):
            pattern = r"[0-9]{2}" if profile == "ubuntu" and i == 1 else r"(?:0|[1-9][0-9]*)"
            if not re.fullmatch(pattern, value) or int(value) > 2147483647:
                raise ValueError(f"Noncanonical version: {version}")
        minimum, maximum = 1, 3
        if profile in {"android", "pixel-9", "debian"}:
            maximum = 2
        if profile.startswith("windows-"):
            maximum = 2
            if not values or int(values[0]) < 10000:
                raise ValueError("Windows selectors use build trains, not NT versions")
        if profile in {"ubuntu", "alpine", "azurelinux"} or profile == "mac" and values[0] == "10":
            minimum = 2
        if not minimum <= len(values) <= maximum or int(values[0]) == 0:
            raise ValueError(f"Invalid selector precision: {version}")
    if not filename.endswith(".png"):
        raise ValueError(f"Expected a PNG leaf: {key}")
    tokens = filename[:-4].split(".")
    architecture = tokens.pop() if len(tokens) > 1 and tokens[-1] in ARCHITECTURES else None
    if not 1 <= len(tokens) <= 2:
        raise ValueError(f"Variant must precede architecture: {key}")
    case = tokens[0]
    variant = tokens[1] if len(tokens) == 2 else None
    if not SLUG.fullmatch(case) or RESERVED.fullmatch(case) or case in ARCHITECTURES | BACKENDS | ENGINES | {"noto-sans"}:
        raise ValueError(f"Invalid case: {key}")
    if case.startswith("visual-"):
        if variant not in BACKENDS:
            raise ValueError(f"Visual cases require an exact renderer: {key}")
    elif variant is not None:
        browser = profile == "browser" or case.startswith(("blazor-", "browser-", "uno-")) or case == "web-page"
        if not (browser and variant in ENGINES or case == "basic-console" and variant == "noto-sans"):
            raise ValueError(f"Unknown functional variant: {key}")
    return profile, version, case, variant, architecture


def png_size(data):
    if len(data) < 33 or data[:8] != PNG or data[12:16] != b"IHDR":
        raise ValueError("Capture is not a PNG with an IHDR")
    size = struct.unpack(">II", data[16:24])
    if not all(size):
        raise ValueError("PNG dimensions must be positive")
    return size


def validate_assets(root, packaged=None):
    root = Path(root)
    if not root.is_dir():
        raise ValueError(f"Golden root does not exist: {root}")
    keys = set()
    for path in sorted(root.rglob("*.png")):
        key = path.relative_to(root).as_posix()
        validate_key(key)
        if key.lower() in keys:
            raise ValueError(f"Duplicate canonical key: {key}")
        keys.add(key.lower())
        data = path.read_bytes()
        png_size(data)
        if packaged is not None:
            copy = contained(Path(packaged), key)
            if not copy.is_file() or copy.read_bytes() != data:
                raise ValueError(f"Packaged asset differs or is missing: {key}")
    if not keys:
        raise ValueError(f"No canonical PNGs under {root}")
    return len(keys)


def contained(root, key):
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
        if line.lstrip().startswith("##SKIA-GOLDEN-V2##") and V2.search(line) is None:
            raise ValueError(f"Malformed v2 golden record in {path}")
    for match in V2.finditer(text):
        role, key, selected, width, height, encoded = match.groups()
        validate_key(key)
        if selected != "-":
            validate_key(selected)
        data = base64.b64decode(encoded, validate=True)
        if png_size(data) != (int(width), int(height)):
            raise ValueError(f"Capture dimensions disagree with record: {key}")
        yield role, key, selected, data


def export_images(files, destination):
    count = 0
    for path in files:
        run = path.stem + "-" + hashlib.sha256(str(path).encode()).hexdigest()[:8]
        occurrences = {}
        for role, key, _, data in records(path):
            number = occurrences.get((role, key), 0) + 1
            occurrences[role, key] = number
            leaf = key[:-4] + f".{number}.{role}.png"
            output = contained(destination / run, leaf)
            output.parent.mkdir(parents=True, exist_ok=True)
            output.write_bytes(data)
            count += 1
        # Legacy records are diagnostic-only, never reference proposals.
        for number, match in enumerate(LEGACY.finditer(path.read_text(encoding="utf-8")), 1):
            role, key, encoded = match.groups()
            data = base64.b64decode(encoded, validate=True)
            png_size(data)
            output = contained(destination / run / "legacy", key[:-4] + f".{number}.{role.lower()}.png")
            output.parent.mkdir(parents=True, exist_ok=True)
            output.write_bytes(data)
            count += 1
    return count


def proposals(files):
    result = {}
    for path in files:
        for role, key, _, data in records(path):
            if role != "actual":
                continue
            if key in result and result[key] != data:
                raise ValueError(f"Conflicting captures for {key}; retain and review each run instead of choosing a winner")
            result[key] = data
    return result


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("paths", nargs="*", default=["output/logs/testlogs"])
    parser.add_argument("--out", default="output/golden-proposals")
    parser.add_argument("--images-out")
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--check", metavar="GOLDEN_ROOT")
    parser.add_argument("--packaged", help="Also compare each source key against this copied golden root")
    args = parser.parse_args(argv)
    try:
        if args.check:
            print(f"Validated {validate_assets(args.check, args.packaged)} canonical PNGs.")
            return 0
        if args.images_out and not any(Path(name).exists() for name in args.paths):
            print("No test results were produced; no diagnostic images to export.")
            return 0
        files = trx_files(args.paths)
        if not files:
            if args.images_out:
                print("No TRX files to export.")
                return 0
            raise ValueError("No test results found")
        if args.images_out:
            print(f"Exported {export_images(files, Path(args.images_out))} diagnostic images.")
            return 0
        captures = proposals(files)
        if not captures:
            raise ValueError("No v2 actual captures; historical markers are diagnostics only")
        for key, data in sorted(captures.items()):
            destination = contained(Path(args.out), key)
            print(f"{'Would stage' if args.dry_run else 'Staging'} {destination}")
            if not args.dry_run:
                if destination.exists() and destination.read_bytes() != data:
                    raise ValueError(f"Proposal already exists with conflicting bytes: {destination}")
                destination.parent.mkdir(parents=True, exist_ok=True)
                destination.write_bytes(data)
        print("Proposals are not approvals. Review pixels and rerun the supported profiles before committing.")
        return 0
    except (ValueError, OSError) as error:
        print(f"error: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
