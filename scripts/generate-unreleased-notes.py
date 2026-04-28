#!/usr/bin/env python3
"""
Fetch unreleased changes (merged PRs since the last release) for SkiaSharp.

Outputs a raw markdown file with emoji-annotated PR lists, grouped by type.
This file is then read by the agentic workflow which uses AI to polish it
into the template format and update index.md.

Usage:
    # Output to stdout
    python3 scripts/generate-unreleased-notes.py

    # Output to a file
    python3 scripts/generate-unreleased-notes.py --output /tmp/unreleased.md

Requirements: gh (GitHub CLI), Python 3.7+
"""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
from typing import Optional


REPO = "mono/SkiaSharp"

# Platform emoji mapping from PR labels
LABEL_PLATFORM_MAP = {
    "os/Windows": "🪟",
    "os/macOS": "🍎",
    "os/iOS": "🍎",
    "os/tvOS": "🍎",
    "os/Linux": "🐧",
    "os/Android": "🤖",
    "backend/SkiaSharp": "🎨",
    "area/Build": "🏗️",
}

# Title keyword to platform emoji
TITLE_PLATFORM_MAP = [
    (r"(?i)(iOS|macOS|tvOS|Apple|Metal|Catalyst)", "🍎"),
    (r"(?i)(Windows|Win|UWP|WinUI|Direct3D|D3D)", "🪟"),
    (r"(?i)(Linux|Alpine|riscv|LoongArch|Bionic)", "🐧"),
    (r"(?i)(Android|NDK)", "🤖"),
    (r"(?i)(WebAssembly|Wasm|Blazor)", "🌐"),
    (r"(?i)(SK[A-Z])", "🎨"),
    (r"(?i)(Build|CI|Pipeline|Docker)", "🏗️"),
]

OWNER = "mattleibow"


def run(args: list[str], check: bool = True) -> str:
    result = subprocess.run(args, capture_output=True, text=True, check=check)
    return result.stdout.strip()


def get_latest_release_tag() -> str:
    """Get the most recent release tag."""
    raw = run(["gh", "release", "list", "--repo", REPO, "--limit", "1",
               "--json", "tagName", "-q", ".[0].tagName"])
    return raw.strip()


def get_unreleased_pr_numbers(tag: str) -> list[int]:
    """Find PR numbers from commits on main that are not in the release tag.

    Uses git log to compare the tag with origin/main, extracting PR numbers
    from merge commit messages (format: "... (#NNN)").
    """
    # Ensure we have the tag and main locally
    run(["git", "fetch", "origin", "main", "--quiet"], check=False)
    run(["git", "fetch", "origin", "tag", tag, "--quiet"], check=False)

    log_output = run([
        "git", "log", "--oneline", f"{tag}..origin/main"
    ])

    pr_numbers = []
    for line in log_output.splitlines():
        match = re.search(r"\(#(\d+)\)\s*$", line)
        if match:
            pr_numbers.append(int(match.group(1)))
    return pr_numbers


def fetch_pr_details(pr_numbers: list[int]) -> list[dict]:
    """Fetch PR details for a list of PR numbers."""
    if not pr_numbers:
        return []

    # Batch fetch using gh api
    prs = []
    for num in pr_numbers:
        try:
            raw = run([
                "gh", "pr", "view", str(num), "--repo", REPO,
                "--json", "title,author,url,number,labels,mergedAt,baseRefName"
            ])
            pr = json.loads(raw)
            # Only include PRs merged to main
            if pr.get("baseRefName") == "main":
                prs.append(pr)
        except (subprocess.CalledProcessError, json.JSONDecodeError):
            continue
    return prs


def get_platform_emoji(title: str, labels: list[dict]) -> str:
    """Determine platform emoji from labels and title."""
    label_names = [l.get("name", "") for l in labels]
    for label_prefix, emoji in LABEL_PLATFORM_MAP.items():
        for label in label_names:
            if label.startswith(label_prefix):
                return emoji

    for pattern, emoji in TITLE_PLATFORM_MAP:
        if re.search(pattern, title):
            return emoji

    return "📦"


def is_community(author: dict) -> bool:
    """Check if author is a community contributor."""
    login = author.get("login", "")
    return login != OWNER and login != "github-actions[bot]"


def classify_pr(pr: dict) -> Optional[str]:
    """Classify PR as 'feature', 'breaking', or None (regular)."""
    title = pr.get("title", "")
    labels = [l.get("name", "") for l in pr.get("labels", [])]

    if any("breaking" in l.lower() for l in labels) or "BREAKING" in title:
        return "breaking"

    feature_patterns = [
        r"(?i)^Add\b", r"(?i)^Support\b", r"(?i)^Enable\b",
        r"(?i)^Implement\b", r"(?i)Bump skia",
    ]
    if any(re.search(p, title) for p in feature_patterns):
        return "feature"

    return None


def generate_unreleased_section(prs: list[dict], latest_tag: str) -> str:
    """Generate the raw unreleased section markdown."""
    if not prs:
        return "*No changes since the last release.*\n"

    breaking = []
    features = []
    all_prs = []

    for pr in prs:
        title = pr.get("title", "")
        author = pr.get("author", {})
        url = pr.get("url", "")
        labels = pr.get("labels", [])

        emoji = get_platform_emoji(title, labels)
        community = "❤️ " if is_community(author) else ""
        login = author.get("login", "unknown")
        line = f"* {emoji}{community}{title} by @{login} in {url}"

        classification = classify_pr(pr)
        if classification == "breaking":
            breaking.append(line)
        elif classification == "feature":
            features.append(line)
        all_prs.append(line)

    lines = []
    lines.append(f"Changes merged to `main` since [{latest_tag}](https://github.com/{REPO}/releases/tag/{latest_tag}):\n")

    if breaking:
        lines.append("### Breaking Changes\n")
        lines.extend(breaking)
        lines.append("")

    if features:
        lines.append("### New Features\n")
        lines.extend(features)
        lines.append("")

    lines.append("### All Changes\n")
    lines.extend(all_prs)
    lines.append("")

    return "\n".join(lines)


def main():
    parser = argparse.ArgumentParser(description="Fetch unreleased SkiaSharp changes")
    parser.add_argument("--output", default=None,
                        help="Output file path (default: stdout)")
    args = parser.parse_args()

    # Find latest release tag
    print("Finding latest release tag...", file=sys.stderr)
    latest_tag = get_latest_release_tag()
    print(f"Latest release: {latest_tag}", file=sys.stderr)

    # Find PRs from commits not in the release tag
    print("Finding commits since release tag...", file=sys.stderr)
    pr_numbers = get_unreleased_pr_numbers(latest_tag)
    print(f"Found {len(pr_numbers)} PR(s) in commit log", file=sys.stderr)

    # Fetch PR details
    print("Fetching PR details...", file=sys.stderr)
    prs = fetch_pr_details(pr_numbers)
    print(f"Fetched {len(prs)} PR(s)", file=sys.stderr)

    # Generate the unreleased section
    unreleased_md = generate_unreleased_section(prs, latest_tag)

    # Output
    if args.output:
        with open(args.output, "w") as f:
            f.write(unreleased_md)
        print(f"Wrote {args.output}", file=sys.stderr)
    else:
        print(unreleased_md)


if __name__ == "__main__":
    main()
