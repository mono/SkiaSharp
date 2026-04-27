#!/usr/bin/env python3
"""
Generate release notes markdown files for the SkiaSharp website.

Fetches all GitHub releases for mono/SkiaSharp, groups them by base version
(stable + previews), and writes DocFX-compatible markdown + TOC.yml into
documentation/docfx/releases/.

Usage:
    python3 scripts/generate-release-notes.py [--output DIR]

Requirements: gh (GitHub CLI), Python 3.7+
"""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
from collections import defaultdict
from datetime import datetime
from typing import Optional


REPO = "mono/SkiaSharp"


def run_gh(args: list[str]) -> str:
    """Run a gh CLI command and return stdout."""
    result = subprocess.run(
        ["gh"] + args,
        capture_output=True, text=True, check=True
    )
    return result.stdout


def extract_base_version(tag: str) -> str:
    """Extract base version from tag: v3.119.2-preview.2.3 -> 3.119.2"""
    ver = tag.lstrip("v")
    return ver.split("-")[0]


def parse_preview_number(tag: str) -> tuple:
    """Extract sort key from tag for ordering within a base version.

    Stable releases sort first (0,), then previews by descending preview number.
    """
    if "-" not in tag:
        return (0,)  # Stable comes first
    suffix = tag.split("-", 1)[1]
    # Try to extract numeric parts for sorting
    nums = re.findall(r"\d+", suffix)
    return (1,) + tuple(int(n) for n in nums)


def format_date(iso_date: str) -> str:
    """Format ISO date to human-readable: April 23, 2026"""
    if not iso_date:
        return ""
    try:
        dt = datetime.fromisoformat(iso_date.replace("Z", "+00:00"))
        return dt.strftime("%B %-d, %Y")
    except (ValueError, AttributeError):
        return iso_date[:10]


def version_sort_key(version: str) -> list:
    """Create a sortable key from a version string like 3.119.2"""
    parts = re.findall(r"\d+", version)
    return [int(p) for p in parts]


def fetch_releases() -> list[dict]:
    """Fetch all releases from GitHub."""
    print(f"Fetching releases from {REPO}...")
    raw = run_gh([
        "release", "list", "--repo", REPO,
        "--limit", "300",
        "--json", "tagName,name,isPrerelease,publishedAt,isDraft"
    ])
    releases = json.loads(raw)
    print(f"Found {len(releases)} releases")
    return releases


def fetch_release_body(tag: str) -> dict:
    """Fetch the full release details for a tag."""
    try:
        raw = run_gh([
            "release", "view", tag, "--repo", REPO,
            "--json", "body,publishedAt,name,isPrerelease"
        ])
        return json.loads(raw)
    except subprocess.CalledProcessError:
        return {"body": "", "publishedAt": "", "name": tag, "isPrerelease": False}


def generate_version_page(base_version: str, releases: list[dict]) -> str:
    """Generate markdown for a single base version page."""
    lines = [f"# Version {base_version}", ""]

    # Sort: stable first, then previews by preview number descending (latest first)
    releases.sort(key=lambda r: parse_preview_number(r["tag"]), reverse=True)
    # But stable (key starts with (0,)) should still be first, so re-sort:
    # stable=False sorts before stable=True when reversed, so we use a two-level key
    releases.sort(key=lambda r: (
        0 if "-" not in r["tag"] else 1,  # stable first
        [-n for n in parse_preview_number(r["tag"])]  # previews descending
    ))

    first = True
    for rel in releases:
        if not first:
            lines.extend(["", "---", ""])
        first = False

        date_str = format_date(rel.get("publishedAt", ""))
        is_pre = rel.get("isPrerelease", False)
        name = rel.get("name", "") or rel["tag"]
        body = rel.get("body", "") or ""

        if not is_pre:
            section_title = "Stable Release"
        else:
            section_title = name

        if date_str:
            lines.append(f"## {section_title} ({date_str})")
        else:
            lines.append(f"## {section_title}")
        lines.append("")

        if body.strip():
            lines.append(body.rstrip())
        else:
            lines.append("*No release notes available.*")
        lines.append("")

    return "\n".join(lines)


def generate_toc(versions_by_date: list[tuple[str, str]]) -> str:
    """Generate TOC.yml for DocFX."""
    lines = [
        "- name: Overview",
        "  href: index.md",
    ]
    for _date, base in versions_by_date:
        lines.append(f"- name: Version {base}")
        lines.append(f"  href: {base}.md")
    return "\n".join(lines) + "\n"


def generate_index(
    versions_by_date: list[tuple[str, str]],
    latest_stable: Optional[tuple[str, str]],
    latest_preview: Optional[tuple[str, str, str]],
) -> str:
    """Generate the index.md overview page."""
    lines = [
        "# Release Notes",
        "",
        "Release notes for all SkiaSharp versions. Each page includes the stable release and all associated preview releases.",
        "",
    ]

    # Latest stable
    lines.append("## Latest Stable")
    lines.append("")
    if latest_stable:
        base, date = latest_stable
        lines.append(f"**[Version {base}]({base}.md)** — released {format_date(date)}")
    lines.append("")

    # Latest preview
    lines.append("## Latest Preview")
    lines.append("")
    if latest_preview:
        base, date, name = latest_preview
        lines.append(f"**[{name}]({base}.md)** — released {format_date(date)}")
    lines.append("")

    # Unreleased placeholder
    lines.extend([
        "## What's Coming Next",
        "",
        "<!-- UNRELEASED_PLACEHOLDER -->",
        "<!-- This section is populated at build time by the CI workflow. -->",
        "",
        "*Build the site with CI to see merged PRs since the last release.*",
        "",
    ])

    # All versions grouped by major
    lines.append("## All Versions")
    lines.append("")

    # Group by major version first, then list within each group by date descending
    major_groups: dict[str, list[tuple[str, str]]] = defaultdict(list)
    for _date, base in versions_by_date:
        major = base.split(".")[0]
        major_groups[major].append((_date, base))

    # Sort major versions descending
    for major in sorted(major_groups.keys(), key=int, reverse=True):
        lines.extend([f"### SkiaSharp {major}.x", ""])
        for _date, base in major_groups[major]:
            date_str = format_date(_date)
            lines.append(f"- [Version {base}]({base}.md) — {date_str}")
        lines.append("")

    lines.append("")
    return "\n".join(lines)


def main():
    parser = argparse.ArgumentParser(description="Generate SkiaSharp release notes for the website")
    parser.add_argument("--output", default="documentation/docfx/releases",
                        help="Output directory for release notes")
    args = parser.parse_args()

    output_dir = args.output
    os.makedirs(output_dir, exist_ok=True)

    # Fetch all releases
    releases = fetch_releases()

    # Fetch bodies and group by base version
    grouped: dict[str, list[dict]] = defaultdict(list)
    base_dates: dict[str, str] = {}  # base_version -> latest date

    total = len(releases)
    for i, rel in enumerate(releases, 1):
        tag = rel["tagName"]
        base = extract_base_version(tag)

        # Fetch full release details
        details = fetch_release_body(tag)
        details["tag"] = tag

        grouped[base].append(details)

        # Track latest date per base version
        pub_date = details.get("publishedAt", "")
        if pub_date and (base not in base_dates or pub_date > base_dates[base]):
            base_dates[base] = pub_date

        if i % 10 == 0:
            print(f"  Fetched {i} / {total}...")

    print(f"  Fetched {total} / {total} (done)")
    print(f"Found {len(grouped)} unique base versions")

    # Generate version pages
    for base, rels in grouped.items():
        filepath = os.path.join(output_dir, f"{base}.md")
        content = generate_version_page(base, rels)
        with open(filepath, "w") as f:
            f.write(content)
        print(f"  Generated {filepath}")

    # Sort versions by date descending for TOC and index
    versions_by_date = sorted(
        [(base_dates.get(b, ""), b) for b in grouped],
        key=lambda x: x[0],
        reverse=True
    )

    # Find latest stable and preview
    latest_stable = None
    latest_preview = None

    for base, rels in grouped.items():
        for rel in rels:
            pub = rel.get("publishedAt", "")
            is_pre = rel.get("isPrerelease", False)
            name = rel.get("name", "") or rel["tag"]

            if not is_pre:
                if latest_stable is None or pub > latest_stable[1]:
                    latest_stable = (base, pub)
            else:
                if latest_preview is None or pub > latest_preview[1]:
                    latest_preview = (base, pub, name)

    # Generate TOC
    toc_path = os.path.join(output_dir, "TOC.yml")
    with open(toc_path, "w") as f:
        f.write(generate_toc(versions_by_date))
    print(f"  Generated {toc_path}")

    # Generate index
    index_path = os.path.join(output_dir, "index.md")
    with open(index_path, "w") as f:
        f.write(generate_index(versions_by_date, latest_stable, latest_preview))
    print(f"  Generated {index_path}")

    # Summary
    md_count = len([f for f in os.listdir(output_dir) if f.endswith(".md")])
    print(f"\nDone! Generated release notes in {output_dir}/")
    print(f"  - {md_count} markdown files (including index.md)")
    print(f"  - TOC.yml")


if __name__ == "__main__":
    main()
