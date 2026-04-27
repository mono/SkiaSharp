#!/usr/bin/env python3
"""
Fetch raw release notes from GitHub for the SkiaSharp website.

Downloads GitHub release data and writes raw markdown files grouped by base
version. These raw files are then reformatted by AI using the template in
documentation/docfx/releases/TEMPLATE.md.

Usage:
    # Fetch the last 5 base versions to a temp directory
    python3 scripts/generate-release-notes.py --last 5

    # Fetch a specific version
    python3 scripts/generate-release-notes.py --version 3.119.2

    # Fetch a range of versions
    python3 scripts/generate-release-notes.py --version 3.119.0 --version 3.119.2

    # Fetch all versions and write directly to the releases directory
    python3 scripts/generate-release-notes.py --all --output documentation/docfx/releases

    # Regenerate TOC and index (no fetching)
    python3 scripts/generate-release-notes.py --update-toc

Requirements: gh (GitHub CLI), Python 3.7+
"""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
import tempfile
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
    print(f"Fetching release list from {REPO}...")
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


def group_releases_by_base(releases: list[dict]) -> dict:
    """Group releases by base version. Returns {base_version: [release_info]}."""
    grouped = defaultdict(list)
    for rel in releases:
        tag = rel["tagName"]
        base = extract_base_version(tag)
        grouped[base].append(rel)
    return grouped


def generate_version_page(base_version: str, releases: list[dict]) -> str:
    """Generate raw markdown for a single base version page."""
    lines = [f"# Version {base_version}", ""]

    # Sort: stable first, then previews descending
    releases.sort(key=lambda r: (
        0 if "-" not in r["tag"] else 1,
        [-n for n in parse_preview_number(r["tag"])]
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


def minor_group_key(version: str) -> str:
    """Extract major.minor group: 3.119.2 -> 3.119, 1.68.0 -> 1.68"""
    parts = version.split(".")
    if len(parts) >= 2:
        return f"{parts[0]}.{parts[1]}"
    return parts[0]


def generate_toc(releases_dir: str) -> str:
    """Generate TOC.yml from existing markdown files, grouped by major.minor."""
    versions = []
    for f in os.listdir(releases_dir):
        if f.endswith(".md") and f not in ("index.md", "TEMPLATE.md"):
            versions.append(f[:-3])

    versions.sort(key=version_sort_key, reverse=True)

    # Group by major.minor
    minor_groups = defaultdict(list)
    for base in versions:
        group = minor_group_key(base)
        minor_groups[group].append(base)

    lines = [
        "- name: Overview",
        "  href: index.md",
    ]

    # Sort groups by version descending
    for group in sorted(minor_groups.keys(), key=lambda g: version_sort_key(g), reverse=True):
        members = minor_groups[group]
        major = group.split(".")[0]
        obsolete = " (Obsolete)" if int(major) < 3 else ""

        lines.append(f"- name: Version {group}.x{obsolete}")
        lines.append(f"  href: {members[0]}.md")
        lines.append(f"  items:")
        for base in members:
            lines.append(f"    - name: Version {base}")
            lines.append(f"      href: {base}.md")

    return "\n".join(lines) + "\n"


def get_upcoming_version(releases_dir: str) -> Optional[str]:
    """Read SKIASHARP_VERSION from azure-templates-variables.yml."""
    variables_path = os.path.join(os.path.dirname(releases_dir),
                                  "..", "..", "scripts", "azure-templates-variables.yml")
    if os.path.exists(variables_path):
        with open(variables_path) as f:
            for line in f:
                m = re.match(r"\s*SKIASHARP_VERSION:\s*(\S+)", line)
                if m:
                    return m.group(1)
    return None


def ensure_upcoming_version_file(releases_dir: str, version: str) -> None:
    """Create the upcoming version file if it doesn't exist."""
    filepath = os.path.join(releases_dir, f"{version}.md")
    if os.path.exists(filepath):
        return

    content = f"""# Version {version}

> **Upcoming release** · In development · Not yet available on NuGet

<!-- UNRELEASED_BEGIN -->

*No changes yet.*

<!-- UNRELEASED_END -->
"""
    with open(filepath, "w") as f:
        f.write(content)
    print(f"  Created upcoming version file: {filepath}")


def generate_index(releases_dir: str) -> str:
    """Generate index.md from existing markdown files."""
    versions = []
    for f in os.listdir(releases_dir):
        if f.endswith(".md") and f not in ("index.md", "TEMPLATE.md"):
            versions.append(f[:-3])

    versions.sort(key=version_sort_key, reverse=True)

    upcoming_version = get_upcoming_version(releases_dir)

    lines = [
        "# Release Notes",
        "",
        "Release notes for all SkiaSharp versions. Each page includes the stable release and all associated preview releases.",
        "",
    ]

    # Group by major version, then by minor
    major_groups = defaultdict(list)
    for base in versions:
        major = base.split(".")[0]
        major_groups[major].append(base)

    def render_major_group(major: str, version_list: list[str]) -> list[str]:
        """Render a major version group's version list."""
        result = []
        minor_groups = defaultdict(list)
        for base in version_list:
            group = minor_group_key(base)
            minor_groups[group].append(base)

        for group in sorted(minor_groups.keys(), key=lambda g: version_sort_key(g), reverse=True):
            members = minor_groups[group]
            result.append(f"- **Version {group}.x**")
            for base in members:
                upcoming = " (Upcoming)" if base == upcoming_version else ""
                result.append(f"  - [Version {base}{upcoming}]({base}.md)")
        return result

    for major in sorted(major_groups.keys(), key=int, reverse=True):
        is_obsolete = int(major) < 3

        if is_obsolete:
            lines.append(f"<details>")
            lines.append(f"<summary><h3>SkiaSharp {major}.x (Obsolete)</h3></summary>")
            lines.append("")
            lines.extend(render_major_group(major, major_groups[major]))
            lines.append("")
            lines.append("</details>")
            lines.append("")
        else:
            lines.extend([f"### SkiaSharp {major}.x", ""])
            lines.extend(render_major_group(major, major_groups[major]))
            lines.append("")

    return "\n".join(lines)


def main():
    parser = argparse.ArgumentParser(
        description="Fetch raw SkiaSharp release notes from GitHub",
        epilog="Examples:\n"
               "  %(prog)s --last 5              # Last 5 base versions → temp dir\n"
               "  %(prog)s --version 3.119.2      # Single version → temp dir\n"
               "  %(prog)s --all --output DIR      # All versions → specified dir\n"
               "  %(prog)s --update-toc            # Regenerate TOC + index only\n",
        formatter_class=argparse.RawDescriptionHelpFormatter
    )
    parser.add_argument("--output", default=None,
                        help="Output directory (default: temp directory)")
    parser.add_argument("--version", action="append", dest="versions",
                        help="Fetch specific version(s). Can be repeated.")
    parser.add_argument("--last", type=int, default=None,
                        help="Fetch the last N base versions (by date)")
    parser.add_argument("--all", action="store_true",
                        help="Fetch all versions")
    parser.add_argument("--update-toc", action="store_true",
                        help="Regenerate TOC.yml and index.md from existing files")
    args = parser.parse_args()

    releases_dir = "documentation/docfx/releases"

    # --update-toc: just regenerate TOC and index from existing files
    if args.update_toc:
        if not os.path.isdir(releases_dir):
            print(f"Error: {releases_dir} does not exist")
            sys.exit(1)

        # Create upcoming version file if needed
        upcoming = get_upcoming_version(releases_dir)
        if upcoming:
            ensure_upcoming_version_file(releases_dir, upcoming)

        toc_path = os.path.join(releases_dir, "TOC.yml")
        with open(toc_path, "w") as f:
            f.write(generate_toc(releases_dir))
        print(f"Updated {toc_path}")

        index_path = os.path.join(releases_dir, "index.md")
        with open(index_path, "w") as f:
            f.write(generate_index(releases_dir))
        print(f"Updated {index_path}")
        return

    # Determine output directory
    if args.output:
        output_dir = args.output
    else:
        output_dir = tempfile.mkdtemp(prefix="skiasharp-releases-")
    os.makedirs(output_dir, exist_ok=True)

    # Fetch all releases (we need the full list to group by base version)
    releases = fetch_releases()
    grouped = group_releases_by_base(releases)

    # Determine which base versions to fetch
    if args.versions:
        target_versions = set(args.versions)
        missing = target_versions - set(grouped.keys())
        if missing:
            print(f"Warning: versions not found: {', '.join(missing)}")
    elif args.last:
        # Sort all base versions by latest release date, take last N
        base_dates = {}
        for base, rels in grouped.items():
            dates = [r.get("publishedAt", "") for r in rels]
            base_dates[base] = max(dates) if dates else ""
        sorted_bases = sorted(base_dates.keys(), key=lambda b: base_dates[b], reverse=True)
        target_versions = set(sorted_bases[:args.last])
    elif args.all:
        target_versions = set(grouped.keys())
    else:
        # Default: last 5
        base_dates = {}
        for base, rels in grouped.items():
            dates = [r.get("publishedAt", "") for r in rels]
            base_dates[base] = max(dates) if dates else ""
        sorted_bases = sorted(base_dates.keys(), key=lambda b: base_dates[b], reverse=True)
        target_versions = set(sorted_bases[:5])

    print(f"Fetching {len(target_versions)} base version(s): {', '.join(sorted(target_versions, key=version_sort_key, reverse=True))}")

    # Fetch release bodies for target versions
    tags_to_fetch = []
    for base in target_versions:
        for rel in grouped[base]:
            tags_to_fetch.append((base, rel["tagName"]))

    fetched = defaultdict(list)
    total = len(tags_to_fetch)
    for i, (base, tag) in enumerate(tags_to_fetch, 1):
        details = fetch_release_body(tag)
        details["tag"] = tag
        fetched[base].append(details)
        if i % 10 == 0:
            print(f"  Fetched {i} / {total}...")
    print(f"  Fetched {total} / {total} (done)")

    # Generate raw version pages
    for base, rels in fetched.items():
        filepath = os.path.join(output_dir, f"{base}.md")
        content = generate_version_page(base, rels)
        with open(filepath, "w") as f:
            f.write(content)
        print(f"  Generated {filepath}")

    # If writing to the releases dir, also update TOC and index
    if args.output and os.path.samefile(args.output, releases_dir):
        toc_path = os.path.join(output_dir, "TOC.yml")
        with open(toc_path, "w") as f:
            f.write(generate_toc(output_dir))
        print(f"  Updated {toc_path}")

        index_path = os.path.join(output_dir, "index.md")
        with open(index_path, "w") as f:
            f.write(generate_index(output_dir))
        print(f"  Updated {index_path}")

    # Summary
    md_count = len([f for f in os.listdir(output_dir) if f.endswith(".md")])
    print(f"\nDone! Raw release notes in {output_dir}/")
    print(f"  - {md_count} markdown file(s)")
    if not args.output:
        print(f"\n  (temp directory — read these files, then reformat using TEMPLATE.md)")


if __name__ == "__main__":
    main()
