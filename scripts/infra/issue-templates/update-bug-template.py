#!/usr/bin/env python3
"""Update the version dropdowns in the bug-report issue template.

The GitHub issue form ``.github/ISSUE_TEMPLATE/bug-report.yml`` has two version
dropdowns whose options need to track the shipped SkiaSharp releases:

* ``version``      — "Version of SkiaSharp" (the version the reporter is on).
                     Only the currently-supported major (from VERSIONS.txt) is
                     listed as concrete builds; every older major collapses to a
                     single ``N.x (Obsolete)`` entry because those lines are no
                     longer maintained and the triage response is simply "update".
* ``goodversion``  — "Last Known Good Version of SkiaSharp". The last-known-good
                     version matters for triage even on retired lines, so this
                     keeps the supported major's builds *and* lists every stable
                     release of the previous major individually, then collapses
                     the rest to ``N.x (Obsolete)``.

Both option lists are generated from the published GitHub Releases (the source
of truth for what a user can actually install), so running this weekly keeps the
"Current Pre-release / Previous Pre-release / Current / Previous / Deprecated"
labels accurate as new packages ship. Pre-release covers both preview and rc
builds, since the in-flight release moves through preview -> rc -> stable.

Usage::

    python3 scripts/infra/issue-templates/update-bug-template.py [--dry-run]
        [--repo mono/SkiaSharp] [--file .github/ISSUE_TEMPLATE/bug-report.yml]

Requires the ``gh`` CLI (authenticated) to read releases. PyYAML is used only to
validate the result when available.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys

# ---------------------------------------------------------------------------
# Version parsing
# ---------------------------------------------------------------------------

# Tags look like: v4.150.1, v4.151.0-preview.2.1, v4.150.0-rc.1.1
# The trailing ".1" on prereleases is the build counter and is dropped from the
# user-facing (NuGet) version.
_TAG_RE = re.compile(
    r"^v?(\d+)\.(\d+)\.(\d+)(?:-(alpha|beta|preview|rc)\.(\d+))?(?:\.(\d+))?$"
)
_PRE_RANK = {"alpha": 0, "beta": 1, "preview": 2, "rc": 3}


def parse_version(tag: str):
    """Parse a git tag into a structured version, or ``None`` if it doesn't fit."""
    m = _TAG_RE.match(tag.strip())
    if not m:
        return None
    major, minor, patch = int(m.group(1)), int(m.group(2)), int(m.group(3))
    label, prenum, build = m.group(4), m.group(5), m.group(6)
    is_pre = label is not None

    if is_pre:
        display = f"{major}.{minor}.{patch}-{label}.{prenum}"
    else:
        display = f"{major}.{minor}.{patch}"

    # Sort key: newest last-> we sort reverse. Stable sorts *after* its own
    # prereleases (4th element 1 vs 0). Prereleases order by kind then number.
    sort_key = (
        major,
        minor,
        patch,
        1 if not is_pre else 0,
        _PRE_RANK.get(label, -1) if is_pre else 99,
        int(prenum) if prenum else 0,
        int(build) if build else 0,
    )
    return {
        "tag": tag,
        "display": display,
        "major": major,
        "minor": minor,
        "patch": patch,
        "is_pre": is_pre,
        "base": (major, minor, patch),
        "sort_key": sort_key,
    }


# ---------------------------------------------------------------------------
# Data sources
# ---------------------------------------------------------------------------


def fetch_releases(repo: str):
    """Return parsed, de-duplicated, newest-first versions from GitHub Releases."""
    out = subprocess.check_output(
        [
            "gh", "release", "list",
            "--repo", repo,
            "--limit", "300",
            "--json", "tagName,isDraft",
        ],
        text=True,
    )
    raw = json.loads(out)

    by_display: dict[str, dict] = {}
    for entry in raw:
        if entry.get("isDraft"):
            continue
        v = parse_version(entry.get("tagName", ""))
        if not v:
            continue
        # Keep the highest build number for a given display version.
        existing = by_display.get(v["display"])
        if existing is None or v["sort_key"] > existing["sort_key"]:
            by_display[v["display"]] = v

    return sorted(by_display.values(), key=lambda v: v["sort_key"], reverse=True)


def current_major(repo_root: str) -> int:
    """Read the supported SkiaSharp major version from scripts/VERSIONS.txt."""
    versions_path = os.path.join(repo_root, "scripts", "VERSIONS.txt")
    with open(versions_path, encoding="utf-8") as f:
        for line in f:
            parts = line.split()
            if len(parts) >= 3 and parts[0] == "SkiaSharp" and parts[1] == "nuget":
                return int(parts[2].split(".")[0])
    raise SystemExit(f"Could not find 'SkiaSharp nuget' line in {versions_path}")

# ---------------------------------------------------------------------------
# Option-list generation
# ---------------------------------------------------------------------------

_OTHER = "Other (Please indicate in the description)"
_NIGHTLY = "Nightly / CI build"


def build_supported_block(versions: list[dict], major: int):
    """Build the shared 4.x option lines and the index of the Current entry."""
    supported = [v for v in versions if v["major"] == major]
    stables = [v for v in supported if not v["is_pre"]]
    prereleases = [v for v in supported if v["is_pre"]]

    current = stables[0] if stables else None
    previous = stables[1] if len(stables) > 1 else None
    deprecated = stables[2:]

    current_base = current["base"] if current else (0, 0, 0)
    # Pre-releases (previews *and* rcs) for a version newer than the current
    # stable — i.e. the in-flight release moving preview -> rc -> stable. The
    # newest published one is the "current" pre-release, not the "next": it has
    # already shipped. Newest first.
    upcoming = [p for p in prereleases if p["base"] > current_base]
    current_pre = upcoming[0] if upcoming else None
    previous_pre = upcoming[1] if len(upcoming) > 1 else None

    block: list[str] = []
    if current_pre:
        block.append(f"{current_pre['display']} (Current Pre-release)")
    if previous_pre:
        block.append(f"{previous_pre['display']} (Previous Pre-release)")

    current_index = 0
    if current:
        current_index = len(block)
        block.append(f"{current['display']} (Current)")
    if previous:
        block.append(f"{previous['display']} (Previous)")
    for d in deprecated:
        block.append(f"{d['display']} (Deprecated)")

    return block, current_index


def obsolete_majors(versions: list[dict], major: int, exclude: set[int]):
    """Collapsed ``N.x (Obsolete)`` lines for every older major, newest first."""
    majors = sorted(
        {v["major"] for v in versions if v["major"] < major and v["major"] not in exclude},
        reverse=True,
    )
    return [f"{m}.x (Obsolete)" for m in majors]


def build_options(versions: list[dict], major: int):
    """Return (version_options, goodversion_options, version_default, goodversion_default)."""
    block, current_index = build_supported_block(versions, major)

    # "Version" dropdown: a Nightly / CI option (people testing the CI feed),
    # the supported builds, then every older major collapsed to N.x (Obsolete).
    version_options = (
        [_NIGHTLY]
        + block
        + obsolete_majors(versions, major, exclude=set())
        + [_OTHER]
    )
    version_default = current_index + 1  # +1 for the leading Nightly entry

    # "Last known good" dropdown: supported builds, then each stable of the
    # previous major individually, then remaining older majors collapsed.
    prev_major = major - 1
    prev_major_stables = [
        v["display"]
        for v in versions
        if v["major"] == prev_major and not v["is_pre"]
    ]
    goodversion_options = (
        block
        + prev_major_stables
        + obsolete_majors(versions, major, exclude={prev_major})
        + [_OTHER]
    )
    goodversion_default = current_index

    return version_options, goodversion_options, version_default, goodversion_default


# ---------------------------------------------------------------------------
# YAML text surgery (preserves the rest of the file byte-for-byte)
# ---------------------------------------------------------------------------


def replace_dropdown(lines: list[str], dropdown_id: str, options: list[str], default: int):
    """Replace the ``options:`` list and ``default:`` of one dropdown in-place."""
    id_re = re.compile(r"^(\s*)id:\s*" + re.escape(dropdown_id) + r"\s*$")
    next_item_re = re.compile(r"^  - type:")
    options_re = re.compile(r"^(\s*)options:\s*$")
    default_re = re.compile(r"^(\s*)default:\s*\d+\s*$")
    option_item_re = re.compile(r"^\s*- ")

    start = next((i for i, ln in enumerate(lines) if id_re.match(ln)), None)
    if start is None:
        raise SystemExit(f"Could not find dropdown id: {dropdown_id}")

    # Determine the block's end (next top-level list item).
    end = len(lines)
    for i in range(start + 1, len(lines)):
        if next_item_re.match(lines[i]):
            end = i
            break

    # Locate the options: line within the block.
    opt_idx = next((i for i in range(start, end) if options_re.match(lines[i])), None)
    if opt_idx is None:
        raise SystemExit(f"Could not find options for dropdown id: {dropdown_id}")
    indent = options_re.match(lines[opt_idx]).group(1)
    item_indent = indent + "  "

    # Collect the contiguous run of existing option items.
    first = opt_idx + 1
    last = first
    while last < end and option_item_re.match(lines[last]):
        last += 1

    new_items = [f"{item_indent}- {opt}\n" for opt in options]
    lines[first:last] = new_items

    # Update default: within the (possibly shifted) block.
    new_end = end + (len(new_items) - (last - first))
    for i in range(start, new_end):
        if default_re.match(lines[i]):
            dindent = default_re.match(lines[i]).group(1)
            lines[i] = f"{dindent}default: {default}\n"
            break

    return lines


def validate_yaml(path: str):
    try:
        import yaml  # type: ignore
    except ImportError:
        print("  (PyYAML not installed — skipping YAML validation)")
        return
    with open(path, encoding="utf-8") as f:
        yaml.safe_load(f)
    print("  YAML is well-formed.")


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo", default="mono/SkiaSharp", help="owner/repo to read releases from")
    parser.add_argument("--file", default=None, help="path to bug-report.yml")
    parser.add_argument("--dry-run", action="store_true", help="print the result without writing")
    args = parser.parse_args(argv)

    repo_root = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".."))
    path = args.file or os.path.join(repo_root, ".github", "ISSUE_TEMPLATE", "bug-report.yml")

    major = current_major(repo_root)
    versions = fetch_releases(args.repo)
    if not versions:
        raise SystemExit("No releases found — is the gh CLI authenticated?")

    version_options, goodversion_options, version_default, goodversion_default = build_options(
        versions, major
    )

    print(f"Supported major: {major}.x")
    print("version options:")
    for o in version_options:
        print(f"  - {o}")
    print(f"  default index: {version_default}")
    print("goodversion options:")
    for o in goodversion_options:
        print(f"  - {o}")
    print(f"  default index: {goodversion_default}")

    with open(path, encoding="utf-8") as f:
        lines = f.readlines()
    original = list(lines)

    lines = replace_dropdown(lines, "version", version_options, version_default)
    lines = replace_dropdown(lines, "goodversion", goodversion_options, goodversion_default)

    if lines == original:
        print("\nNo changes needed — dropdowns already up to date.")
        return 0

    if args.dry_run:
        print("\n--dry-run: not writing changes.")
        return 0

    with open(path, "w", encoding="utf-8") as f:
        f.writelines(lines)
    print(f"\nUpdated {os.path.relpath(path, repo_root)}")
    validate_yaml(path)
    return 0


if __name__ == "__main__":
    sys.exit(main())
