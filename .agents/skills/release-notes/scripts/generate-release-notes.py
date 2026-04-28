#!/usr/bin/env python3
"""
Fetch SkiaSharp release data and manage the website release notes structure.

This script collects raw data. AI does the formatting using TEMPLATE.md.

Commands:
    # Fetch raw release data for specific version(s) → temp dir
    python3 .agents/skills/release-notes/scripts/generate-release-notes.py --version 3.119.2

    # Fetch raw release data for the last N versions → temp dir
    python3 .agents/skills/release-notes/scripts/generate-release-notes.py --last 5

    # Fetch unreleased PRs (commits on main not in the last release tag) → temp dir or stdout
    python3 .agents/skills/release-notes/scripts/generate-release-notes.py --unreleased

    # Regenerate TOC.yml and index.md from files on disk + create upcoming version file
    python3 .agents/skills/release-notes/scripts/generate-release-notes.py --update-toc

Requirements: gh (GitHub CLI), git, Python 3.7+
"""

from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
import tempfile
from collections import defaultdict
from pathlib import Path
from typing import Optional


REPO = "mono/SkiaSharp"
RELEASES_DIR = Path("documentation/docfx/releases")

SKIA_REPO = "mono/skia"
SKIA_PR_PATTERNS = [
    re.compile(r"(?:companion|related)\s+(?:skia\s+)?pr[:\s]+https?://github\.com/mono/skia/pull/(\d+)", re.IGNORECASE),
    re.compile(r"https?://github\.com/mono/skia/pull/(\d+)"),
]




def run(args: list[str], check: bool = True) -> str:
    """Run a command and return stdout."""
    result = subprocess.run(args, capture_output=True, text=True, check=check)
    return result.stdout.strip()


def gh(args: list[str]) -> str:
    """Run a gh CLI command and return stdout."""
    return run(["gh"] + args)


def extract_base_version(tag: str) -> str:
    """v3.119.2-preview.2.3 -> 3.119.2"""
    return tag.lstrip("v").split("-")[0]


def minor_group(version: str) -> str:
    """3.119.2 -> 3.119"""
    parts = version.split(".")
    return f"{parts[0]}.{parts[1]}" if len(parts) >= 2 else parts[0]


def version_key(version: str) -> list[int]:
    """Sortable key from version string."""
    return [int(n) for n in re.findall(r"\d+", version)]


def get_upcoming_version() -> Optional[str]:
    """Read SKIASHARP_VERSION from azure-templates-variables.yml."""
    path = Path("scripts/azure-templates-variables.yml")
    if path.exists():
        for line in path.read_text().splitlines():
            m = re.match(r"\s*SKIASHARP_VERSION:\s*(\S+)", line)
            if m:
                return m.group(1)
    return None


# ── Released version data ────────────────────────────────────────────


def fetch_all_releases() -> list[dict]:
    """Fetch the release list from GitHub."""
    raw = gh(["release", "list", "--repo", REPO, "--limit", "300",
              "--json", "tagName,name,isPrerelease,publishedAt"])
    return json.loads(raw)


def fetch_release_body(tag: str) -> dict:
    """Fetch the full release body for a tag."""
    try:
        raw = gh(["release", "view", tag, "--repo", REPO,
                  "--json", "body,publishedAt,name,isPrerelease"])
        return json.loads(raw)
    except subprocess.CalledProcessError:
        return {"body": "", "publishedAt": "", "name": tag, "isPrerelease": False}


def group_by_base(releases: list[dict]) -> dict[str, list[dict]]:
    """Group releases by base version."""
    grouped = defaultdict(list)
    for rel in releases:
        base = extract_base_version(rel["tagName"])
        grouped[base].append(rel)
    return grouped


def generate_raw_version_page(base_version: str, releases: list[dict]) -> str:
    """Generate raw markdown for a version — stable first, then previews descending."""

    def sort_key(r):
        tag = r["tag"]
        if "-" not in tag:
            return (0,)
        nums = re.findall(r"\d+", tag.split("-", 1)[1])
        return (1,) + tuple(-int(n) for n in nums)

    releases.sort(key=sort_key)
    lines = [f"# Version {base_version}", ""]

    for i, rel in enumerate(releases):
        if i > 0:
            lines.extend(["", "---", ""])

        name = rel.get("name", "") or rel["tag"]
        date = rel.get("publishedAt", "")
        is_pre = rel.get("isPrerelease", False)
        body = rel.get("body", "") or ""

        title = name if is_pre else "Stable Release"
        date_part = f" ({date[:10]})" if date else ""
        lines.append(f"## {title}{date_part}")
        lines.append("")
        lines.append(body.rstrip() if body.strip() else "*No release notes available.*")
        lines.append("")

    return "\n".join(lines)


def compute_pr_effort(pr: dict) -> dict:
    """Compute commit count and unique working days from PR commit data.

    If the PR body references a companion mono/skia PR, fetches that PR's
    commits too and merges the effort (only counting authors from the
    SkiaSharp PR to exclude unrelated upstream Skia committers).
    """
    commits = pr.get("commits", [])
    commit_count = len(commits)
    unique_days = set()

    # Collect SkiaSharp PR author names for filtering skia commits
    pr_author_names = set()
    for c in commits:
        for a in c.get("authors", []):
            name = a.get("name", "")
            if name:
                pr_author_names.add(name)

    for c in commits:
        date_str = c.get("committedDate") or c.get("authoredDate", "")
        if date_str:
            unique_days.add(date_str[:10])  # YYYY-MM-DD

    # Look for companion skia PR in the body
    skia_pr_num = None
    body = pr.get("body") or ""
    for pattern in SKIA_PR_PATTERNS:
        m = pattern.search(body)
        if m:
            skia_pr_num = m.group(1)
            break

    skia_commits = 0
    if skia_pr_num:
        skia_commits, skia_days = _fetch_skia_pr_effort(
            skia_pr_num, pr_author_names)
        unique_days |= skia_days

    return {
        "commitCount": commit_count + skia_commits,
        "workingDays": len(unique_days),
        "skiaPr": int(skia_pr_num) if skia_pr_num else None,
    }


def _fetch_skia_pr_effort(
    pr_num: str, author_names: set[str]
) -> tuple[int, set[str]]:
    """Fetch effort from a mono/skia PR using git log on the submodule.

    Uses the skia submodule directly to avoid GitHub API commit truncation
    (the API caps at 100-250 commits, but skia merge PRs can have thousands).
    Filters commits by author name to count only work by the SkiaSharp PR
    contributors, excluding unrelated upstream Skia committers.

    Returns (commit_count, set_of_date_strings).
    """
    skia_dir = Path("externals/skia")
    if not (skia_dir / ".git").exists():
        return 0, set()

    try:
        meta = json.loads(
            gh(["api", f"repos/{SKIA_REPO}/pulls/{pr_num}",
                "--jq", "{base_sha: .base.sha, head_sha: .head.sha}"]))
        base_sha = meta["base_sha"]
        head_sha = meta["head_sha"]
    except (subprocess.CalledProcessError, json.JSONDecodeError, KeyError):
        return 0, set()

    # Fetch the head commit into the submodule
    run(["git", "-C", str(skia_dir), "fetch", "origin", head_sha, "--quiet"],
        check=False)

    # Verify we have both commits
    try:
        run(["git", "-C", str(skia_dir), "cat-file", "-e", head_sha])
        run(["git", "-C", str(skia_dir), "cat-file", "-e", base_sha])
    except subprocess.CalledProcessError:
        return 0, set()

    # Get all commits in the PR range with author and date
    log_output = run([
        "git", "-C", str(skia_dir), "log",
        "--format=%ad\t%an", "--date=short",
        f"{base_sha}..{head_sha}",
    ], check=False)

    if not log_output:
        return 0, set()

    count = 0
    days = set()
    for line in log_output.splitlines():
        parts = line.split("\t", 1)
        if len(parts) != 2:
            continue
        date_str, author_name = parts
        if author_name in author_names:
            count += 1
            days.add(date_str)

    return count, days


# ── Unreleased data ─────────────────────────────────────────────────


def find_baseline_tag(upcoming_version: Optional[str]) -> str:
    """Find the correct baseline tag for unreleased diff.

    Gets all release tags, parses as semver, and finds the highest tag
    whose base version is <= the upcoming version. This correctly handles
    the case where a v3 hotfix is tagged after a v4 preview — we still
    pick the v4 preview as the baseline for v4 unreleased work.

    The tag may be on a release/* branch (not main), but git log
    {tag}..origin/main still works correctly because release branches
    fork from main.
    """
    raw = gh(["release", "list", "--repo", REPO, "--limit", "100",
              "--json", "tagName"])
    tags = [r["tagName"] for r in json.loads(raw)]

    if not tags:
        raise RuntimeError("No releases found")

    if not upcoming_version:
        tags.sort(key=lambda t: version_key(t.lstrip("v")), reverse=True)
        return tags[0]

    upcoming_key = version_key(upcoming_version)

    # Filter to tags whose base version is <= upcoming, then pick the highest
    candidates = []
    for tag in tags:
        key = version_key(tag.lstrip("v"))
        if key <= upcoming_key:
            candidates.append((key, tag))

    if not candidates:
        tags.sort(key=lambda t: version_key(t.lstrip("v")), reverse=True)
        return tags[0]

    candidates.sort(key=lambda x: x[0], reverse=True)
    return candidates[0][1]


def get_unreleased_prs(tag: str) -> list[dict]:
    """Find PRs from commits on main that are not in the release tag."""
    run(["git", "fetch", "origin", "main", "--quiet"], check=False)
    run(["git", "fetch", "origin", "tag", tag, "--quiet"], check=False)

    log = run(["git", "log", "--oneline", f"{tag}..origin/main"])

    pr_numbers = []
    for line in log.splitlines():
        m = re.search(r"\(#(\d+)\)\s*$", line)
        if m:
            pr_numbers.append(int(m.group(1)))

    if not pr_numbers:
        return []

    prs = []
    total = len(pr_numbers)
    for i, num in enumerate(pr_numbers, 1):
        try:
            raw = gh(["pr", "view", str(num), "--repo", REPO,
                      "--json", "title,author,url,number,labels,mergedAt,commits,body"])
            pr = json.loads(raw)
            pr.update(compute_pr_effort(pr))
            prs.append(pr)
        except (subprocess.CalledProcessError, json.JSONDecodeError):
            continue
        if i % 20 == 0:
            print(f"  Fetched {i}/{total} PRs...", file=sys.stderr)

    return prs


def generate_raw_unreleased(prs: list[dict], tag: str) -> str:
    """Generate raw unreleased PR list — just data, no formatting."""
    if not prs:
        return "*No unreleased changes.*\n"

    lines = [f"Unreleased changes on `main` since `{tag}`:", ""]
    for pr in prs:
        title = pr.get("title", "")
        author = pr.get("author", {}).get("login", "unknown")
        url = pr.get("url", "")
        number = pr.get("number", "")
        label_names = [l.get("name", "") for l in pr.get("labels", [])]
        labels_str = f" [{', '.join(label_names)}]" if label_names else ""
        commits = pr.get("commitCount", 0)
        days = pr.get("workingDays", 0)
        effort = f" ({commits} commit{'s' if commits != 1 else ''}, {days} day{'s' if days != 1 else ''})"
        skia_pr = pr.get("skiaPr")
        skia_str = f" (skia: mono/skia#{skia_pr})" if skia_pr else ""

        lines.append(f"- {title} by @{author} in {url}{labels_str}{effort}{skia_str}")

    lines.append("")
    return "\n".join(lines)


# ── TOC and index generation ────────────────────────────────────────


def get_version_files() -> list[str]:
    """List version strings from existing markdown files."""
    versions = []
    for f in RELEASES_DIR.iterdir():
        if f.suffix == ".md" and f.name not in ("index.md", "TEMPLATE.md"):
            versions.append(f.stem)
    versions.sort(key=version_key, reverse=True)
    return versions


def generate_toc(versions: list[str]) -> str:
    """Generate TOC.yml grouped by major.minor, obsolete under one node."""
    groups = defaultdict(list)
    for v in versions:
        groups[minor_group(v)].append(v)

    current = []
    obsolete = []
    for g in sorted(groups.keys(), key=lambda x: version_key(x), reverse=True):
        if int(g.split(".")[0]) < 3:
            obsolete.append(g)
        else:
            current.append(g)

    lines = ["- name: Overview", "  href: index.md"]

    for g in current:
        members = groups[g]
        lines.append(f"- name: Version {g}.x")
        lines.append(f"  href: {members[0]}.md")
        lines.append(f"  items:")
        for v in members:
            lines.append(f"    - name: Version {v}")
            lines.append(f"      href: {v}.md")

    if obsolete:
        lines.append(f"- name: Obsolete Versions")
        lines.append(f"  href: {groups[obsolete[0]][0]}.md")
        lines.append(f"  items:")
        for g in obsolete:
            members = groups[g]
            lines.append(f"    - name: Version {g}.x")
            lines.append(f"      href: {members[0]}.md")
            if len(members) > 1:
                lines.append(f"      items:")
                for v in members:
                    lines.append(f"        - name: Version {v}")
                    lines.append(f"          href: {v}.md")

    return "\n".join(lines) + "\n"


def generate_index(versions: list[str], upcoming: Optional[str]) -> str:
    """Generate index.md with version list grouped by major."""
    lines = [
        "# Release Notes",
        "",
        "Release notes for all SkiaSharp versions.",
        "",
    ]

    major_groups = defaultdict(list)
    for v in versions:
        major_groups[v.split(".")[0]].append(v)

    for major in sorted(major_groups.keys(), key=int, reverse=True):
        lines.extend([f"### SkiaSharp {major}.x", ""])

        minor_groups = defaultdict(list)
        for v in major_groups[major]:
            minor_groups[minor_group(v)].append(v)

        for g in sorted(minor_groups.keys(), key=lambda x: version_key(x), reverse=True):
            members = minor_groups[g]
            lines.append(f"- **Version {g}.x**")
            for v in members:
                tag = " (Upcoming)" if v == upcoming else ""
                lines.append(f"  - [Version {v}{tag}]({v}.md)")
        lines.append("")

    return "\n".join(lines)


def ensure_upcoming_file(version: str) -> bool:
    """Create upcoming version file if it doesn't exist. Returns True if created."""
    path = RELEASES_DIR / f"{version}.md"
    if path.exists():
        return False

    path.write_text(
        f"# Version {version}\n\n"
        f"> **Upcoming release** · In development · Not yet available on NuGet\n\n"
        f"*No changes yet.*\n"
    )
    print(f"  Created {path}")
    return True


# ── Main ─────────────────────────────────────────────────────────────


def cmd_update_toc():
    """Regenerate TOC.yml and index.md, create upcoming version file if needed."""
    if not RELEASES_DIR.is_dir():
        print(f"Error: {RELEASES_DIR} does not exist", file=sys.stderr)
        sys.exit(1)

    upcoming = get_upcoming_version()
    if upcoming:
        ensure_upcoming_file(upcoming)

    versions = get_version_files()

    (RELEASES_DIR / "TOC.yml").write_text(generate_toc(versions))
    print(f"Updated {RELEASES_DIR / 'TOC.yml'}")

    (RELEASES_DIR / "index.md").write_text(generate_index(versions, upcoming))
    print(f"Updated {RELEASES_DIR / 'index.md'}")


def cmd_fetch_versions(target_versions: set[str], output_dir: Path):
    """Fetch raw release data for specific versions."""
    print(f"Fetching release list from {REPO}...", file=sys.stderr)
    releases = fetch_all_releases()
    grouped = group_by_base(releases)

    missing = target_versions - set(grouped.keys())
    if missing:
        print(f"Warning: not found on GitHub: {', '.join(sorted(missing))}", file=sys.stderr)

    tags_to_fetch = []
    for base in target_versions & set(grouped.keys()):
        for rel in grouped[base]:
            tags_to_fetch.append((base, rel["tagName"]))

    print(f"Fetching {len(tags_to_fetch)} release(s)...", file=sys.stderr)
    fetched = defaultdict(list)
    for i, (base, tag) in enumerate(tags_to_fetch, 1):
        details = fetch_release_body(tag)
        details["tag"] = tag
        fetched[base].append(details)
        if i % 10 == 0:
            print(f"  {i}/{len(tags_to_fetch)}...", file=sys.stderr)

    output_dir.mkdir(parents=True, exist_ok=True)
    for base, rels in fetched.items():
        path = output_dir / f"{base}.md"
        path.write_text(generate_raw_version_page(base, rels))
        print(f"  {path}")

    print(f"\nDone: {len(fetched)} version(s) in {output_dir}/", file=sys.stderr)


def cmd_unreleased(output_path: Optional[Path]):
    """Fetch unreleased PRs and output raw list."""
    upcoming = get_upcoming_version()
    print(f"Upcoming version: {upcoming or 'unknown'}", file=sys.stderr)

    print("Finding baseline tag...", file=sys.stderr)
    tag = find_baseline_tag(upcoming)
    print(f"Baseline: {tag}", file=sys.stderr)

    print("Finding unreleased PRs via commit ancestry...", file=sys.stderr)
    prs = get_unreleased_prs(tag)
    print(f"Found {len(prs)} PR(s)", file=sys.stderr)

    content = generate_raw_unreleased(prs, tag)

    if output_path:
        output_path.parent.mkdir(parents=True, exist_ok=True)
        output_path.write_text(content)
        print(f"Wrote {output_path}", file=sys.stderr)
    else:
        print(content)


def main():
    parser = argparse.ArgumentParser(
        description="Fetch SkiaSharp release data for the website",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=(
            "Examples:\n"
            "  %(prog)s --version 3.119.2        Fetch one version\n"
            "  %(prog)s --last 5                  Fetch 5 most recent versions\n"
            "  %(prog)s --unreleased              Unreleased PRs to stdout\n"
            "  %(prog)s --unreleased -o /tmp/u.md  Unreleased PRs to file\n"
            "  %(prog)s --update-toc              Regenerate TOC + index\n"
        ),
    )
    parser.add_argument("--version", action="append", dest="versions",
                        help="Fetch specific version(s). Repeatable.")
    parser.add_argument("--last", type=int, help="Fetch the N most recent versions")
    parser.add_argument("--unreleased", action="store_true",
                        help="Fetch unreleased PRs since last tag")
    parser.add_argument("--update-toc", action="store_true",
                        help="Regenerate TOC.yml + index.md")
    parser.add_argument("-o", "--output", help="Output directory (versions) or file (unreleased)")
    args = parser.parse_args()

    # Must specify exactly one mode
    modes = sum([bool(args.versions), bool(args.last), args.unreleased, args.update_toc])
    if modes == 0:
        parser.print_help()
        sys.exit(1)
    if modes > 1:
        parser.error("Specify only one of --version, --last, --unreleased, --update-toc")

    if args.update_toc:
        cmd_update_toc()

    elif args.unreleased:
        cmd_unreleased(Path(args.output) if args.output else None)

    elif args.versions:
        output = Path(args.output) if args.output else Path(tempfile.mkdtemp(prefix="skiasharp-releases-"))
        cmd_fetch_versions(set(args.versions), output)

    elif args.last:
        releases = fetch_all_releases()
        grouped = group_by_base(releases)
        base_dates = {}
        for base, rels in grouped.items():
            dates = [r.get("publishedAt", "") for r in rels if r.get("publishedAt")]
            base_dates[base] = max(dates) if dates else ""
        recent = sorted(base_dates, key=lambda b: base_dates[b], reverse=True)[:args.last]

        output = Path(args.output) if args.output else Path(tempfile.mkdtemp(prefix="skiasharp-releases-"))
        cmd_fetch_versions(set(recent), output)


if __name__ == "__main__":
    main()
