#!/usr/bin/env python3
"""Update the version dropdowns in the bug-report issue template.

The GitHub issue form ``.github/ISSUE_TEMPLATE/bug-report.yml`` has two version
dropdowns whose options need to track the shipped SkiaSharp releases:

* ``version``      — "Version of SkiaSharp" (the version the reporter is on).
                     Only the currently-supported major (from VERSIONS.txt) is
                     listed as concrete builds; every older major collapses to a
                     single ``N.x (Obsolete)`` entry because those lines are no
                     longer maintained and the triage response is simply "update".
                     A single "Pre-release" entry covers the in-flight release —
                     only the newest build matters when asking what you're on now.
* ``goodversion``  — "Last Known Good Version of SkiaSharp". The last-known-good
                     version matters for triage even on retired lines, so this
                     lists *every* in-flight pre-release build individually
                     (preview.1, preview.2, rc.1, …): someone may have been fine
                     on preview.1 but hit a regression in preview.2, and that is
                     exactly what last-known-good is meant to capture. Older
                     majors still collapse to ``N.x (Obsolete)`` — those lines
                     are retired, so "somewhere in 3.x" is a good enough answer.

Both option lists are generated from the published GitHub Releases (the source
of truth for what a user can actually install), so running this weekly keeps the
"Pre-release / Current / Previous / Deprecated" labels accurate as new packages
ship.

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
    """Build the supported-major option lines.

    Returns ``(stable_lines, upcoming)`` where ``stable_lines`` are the shared
    ``(Current)`` / ``(Previous)`` / ``(Deprecated)`` entries and ``upcoming`` is
    the list of in-flight pre-release versions (previews *and* rcs of a base
    newer than the current stable), newest first. The two dropdowns treat the
    pre-releases differently, so they are returned separately rather than baked
    into a single block.
    """
    supported = [v for v in versions if v["major"] == major]
    stables = [v for v in supported if not v["is_pre"]]
    prereleases = [v for v in supported if v["is_pre"]]

    current = stables[0] if stables else None
    previous = stables[1] if len(stables) > 1 else None
    deprecated = stables[2:]

    current_base = current["base"] if current else (0, 0, 0)
    # Pre-releases (previews *and* rcs) for a version newer than the current
    # stable — i.e. the in-flight release moving preview -> rc -> stable.
    # Newest first.
    upcoming = [p for p in prereleases if p["base"] > current_base]

    stable_lines: list[str] = []
    if current:
        stable_lines.append(f"{current['display']} (Current)")
    if previous:
        stable_lines.append(f"{previous['display']} (Previous)")
    for d in deprecated:
        stable_lines.append(f"{d['display']} (Deprecated)")

    return stable_lines, upcoming


def obsolete_majors(versions: list[dict], major: int):
    """Collapsed ``N.x (Obsolete)`` lines for every older major, newest first."""
    majors = sorted(
        {v["major"] for v in versions if v["major"] < major},
        reverse=True,
    )
    return [f"{m}.x (Obsolete)" for m in majors]


def build_options(versions: list[dict], major: int):
    """Return (version_options, goodversion_options, version_default, goodversion_default)."""
    stable_lines, upcoming = build_supported_block(versions, major)

    # "Version" dropdown: a Nightly / CI option (people testing the CI feed),
    # a single "Pre-release" entry for the newest in-flight build (only the
    # newest matters when asking "what are you on now" — the triage answer for
    # an older pre-release build is always "update to the latest"), the
    # supported stables, then every older major collapsed to N.x (Obsolete).
    version_pre = [f"{upcoming[0]['display']} (Pre-release)"] if upcoming else []
    version_options = (
        [_NIGHTLY]
        + version_pre
        + stable_lines
        + obsolete_majors(versions, major)
        + [_OTHER]
    )
    # Default lands on (Current): after Nightly + the pre-release entries.
    version_default = 1 + len(version_pre)

    # "Last known good" dropdown: every in-flight pre-release listed
    # individually — a reporter may have been fine on preview.1 but hit a
    # regression in preview.2, and that distinction is exactly what
    # last-known-good captures. Then the supported stables, then every older
    # major collapsed to N.x (Obsolete): those lines are retired, so knowing it
    # last worked "somewhere in 3.x" is enough — the exact build doesn't matter.
    good_pre = [f"{p['display']} (Pre-release)" for p in upcoming]
    goodversion_options = (
        good_pre
        + stable_lines
        + obsolete_majors(versions, major)
        + [_OTHER]
    )
    # Default lands on (Current): after the pre-release entries.
    goodversion_default = len(good_pre)

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
