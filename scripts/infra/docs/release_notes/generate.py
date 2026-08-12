#!/usr/bin/env python3
"""Generate canonical release facts and denormalized agent context atomically."""

from __future__ import annotations

import argparse
from pathlib import Path
import re
import subprocess
import sys

PACKAGE_PARENT = Path(__file__).resolve().parent.parent
if str(PACKAGE_PARENT) not in sys.path:
    sys.path.insert(0, str(PACKAGE_PARENT))

from release_notes import common, model, sources


def _valid_stable_base(branch: str) -> bool:
    version = common.version_from_branch(branch)
    if common.version_is_superseded(version):
        return False
    return sources.version_has_stable_tag(version)


def find_previous_stable_base(
    all_branches: list[str],
    major: int,
    minor: int,
    patch: int,
    subpatch: int = 0,
) -> str | None:
    prefix = "{}.{}".format(major, minor)
    if subpatch > 0:
        patch_base = "{}.{}".format(prefix, patch)
        for value in range(subpatch - 1, 0, -1):
            candidate = "release/{}.{}".format(patch_base, value)
            if candidate in all_branches:
                return candidate
        plain = "release/{}".format(patch_base)
        if plain in all_branches:
            return plain
        previews = [
            branch for branch in all_branches
            if branch.startswith("release/{}-preview.".format(patch_base))
        ]
        if previews:
            return max(previews, key=common.release_branch_sort_key)

    if patch > 0:
        for value in range(patch - 1, -1, -1):
            version = "{}.{}".format(prefix, value)
            if common.version_is_superseded(version):
                continue
            stable = "release/{}".format(version)
            if stable in all_branches:
                return stable
            if not sources.version_has_stable_tag(version):
                continue
            previews = [
                branch for branch in all_branches
                if branch.startswith("release/{}-preview.".format(version))
            ]
            if previews:
                return max(previews, key=common.release_branch_sort_key)

    versioned = sorted(
        (branch for branch in all_branches if not branch.endswith(".x")),
        key=common.release_branch_sort_key,
    )
    target = (major, minor, -2, 0, 0, 0)
    candidates = [
        branch for branch in versioned
        if common.release_branch_sort_key(branch) < target
    ]
    if not candidates:
        return None
    stable = [branch for branch in candidates if _valid_stable_base(branch)]
    return stable[-1] if stable else candidates[-1]


def _resolve_compare_to(
    compare_to: str,
    to_ref: str,
    version: str,
    all_branches: list[str],
) -> tuple[str, str, str] | None:
    branches = [
        branch for branch in all_branches
        if not branch.endswith(".x")
        and common.version_from_branch(branch) == compare_to
    ]
    if branches:
        latest = max(branches, key=common.release_branch_sort_key)
        return "origin/{}".format(latest), to_ref, version
    exact = "release/{}".format(compare_to)
    if exact in all_branches:
        return "origin/{}".format(exact), to_ref, version
    tag_sha = common.run(
        ["git", "rev-parse", "v{}".format(compare_to)],
        check=False,
    ).strip()
    return (tag_sha, to_ref, version) if tag_sha else None


def determine_diff_range(branch: str) -> tuple[str, str, str]:
    branches = sources.list_remote_release_branches()
    if branch == "main":
        version = common.get_upcoming_version()
        if not version:
            raise RuntimeError("Cannot read SKIASHARP_VERSION")
        minor = common.minor_group(version)
        same_minor = sorted(
            (
                candidate for candidate in branches
                if candidate.startswith("release/{}.".format(minor))
                and not candidate.endswith(".x")
            ),
            key=common.release_branch_sort_key,
        )
        if same_minor:
            return "origin/{}".format(same_minor[-1]), "origin/main", version
        versioned = sorted(
            (
                candidate for candidate in branches
                if not candidate.endswith(".x")
            ),
            key=common.release_branch_sort_key,
        )
        if not versioned:
            raise RuntimeError("No release branches found")
        below = [
            candidate for candidate in versioned
            if common.version_key(common.version_from_branch(candidate))
            < common.version_key(version)
        ]
        latest = below[-1] if below else versioned[-1]
        return "origin/{}".format(latest), "origin/main", version

    servicing = re.fullmatch(r"release/(\d+)\.(\d+)\.x", branch)
    if servicing:
        major, minor = (int(value) for value in servicing.groups())
        prefix = "{}.{}".format(major, minor)
        version = (
            sources.get_version_from_remote_branch(branch)
            or "{}.0".format(prefix)
        )
        candidates = sorted(
            (
                candidate for candidate in branches
                if candidate.startswith("release/{}.".format(prefix))
                and candidate != branch
                and not candidate.endswith(".x")
            ),
            key=common.release_branch_sort_key,
        )
        if candidates:
            return (
                "origin/{}".format(candidates[-1]),
                "origin/{}".format(branch),
                version,
            )
        base = find_previous_stable_base(branches, major, minor, 0)
        if base:
            return (
                "origin/{}".format(base),
                "origin/{}".format(branch),
                version,
            )
        merge_base = common.run([
            "git", "merge-base", "origin/{}".format(branch), "origin/main",
        ])
        return merge_base, "origin/{}".format(branch), version

    versioned = re.match(
        r"release/(\d+)\.(\d+)\.(\d+)(?:\.(\d+))?",
        branch,
    )
    if not versioned:
        raise RuntimeError("Cannot parse branch: {}".format(branch))
    major = int(versioned.group(1))
    minor = int(versioned.group(2))
    patch = int(versioned.group(3))
    subpatch = int(versioned.group(4) or 0)
    version = common.version_from_branch(branch)
    entry = common.versions_config_lookup(version)
    if entry and entry.get("compare_to"):
        resolved = _resolve_compare_to(
            entry["compare_to"],
            "origin/{}".format(branch),
            version,
            branches,
        )
        if resolved:
            return resolved
    base = find_previous_stable_base(
        branches, major, minor, patch, subpatch
    )
    if base:
        return (
            "origin/{}".format(base),
            "origin/{}".format(branch),
            version,
        )
    merge_base = common.run([
        "git", "merge-base", "origin/{}".format(branch), "origin/main",
    ])
    return merge_base, "origin/{}".format(branch), version


def _page_status(branch: str, version: str) -> tuple[str, str | None, list[str]]:
    unreleased = branch == "main" or branch.endswith(".x")
    status = "unreleased"
    superseded_by = None
    if not unreleased:
        tags = common.run(
            ["git", "tag", "-l", "v{}*".format(version)],
            check=False,
        ).splitlines()
        status = (
            "stable"
            if any("-preview" not in tag and "-rc" not in tag for tag in tags)
            else "preview"
        )
        superseded_by = common.resolve_superseded_by(version)
        if superseded_by:
            status = "preview"
    return status, superseded_by, common.detect_supersedes(version)


def _page_filename(branch: str, version: str) -> str:
    return (
        "{}-unreleased.md".format(version)
        if branch == "main" or branch.endswith(".x")
        else "{}.md".format(version)
    )


def _canonical_branches(branches: list[str]) -> dict[str, str]:
    canonical = {}
    for branch in branches:
        if branch.endswith(".x"):
            continue
        version = common.version_from_branch(branch)
        current = canonical.get(version)
        if (
            current is None
            or common.release_branch_sort_key(branch)
            > common.release_branch_sort_key(current)
        ):
            canonical[version] = branch
    return canonical


def _base_version(from_ref: str, version: str) -> str | None:
    display = common.removeprefix(from_ref, "origin/")
    if display.startswith("release/"):
        return common.version_from_branch(display)
    if re.match(r"^\d+\.\d+\.\d+", display):
        return display
    entry = common.versions_config_lookup(version)
    return entry.get("compare_to") if entry else None


def _previous_co_release_version(
    base_version: str | None,
    supersedes: list[str],
    co_releases: dict[str, str],
) -> str | None:
    candidates = [
        candidate
        for candidate in [base_version, *supersedes]
        if candidate and co_releases.get(candidate)
    ]
    if not candidates:
        return None
    previous_line = max(candidates, key=common.version_key)
    return co_releases[previous_line]


def _harfbuzz_pr_numbers(prs: list[dict], path_prs: list[dict]) -> list[int]:
    path_numbers = {
        pr.get("number")
        for pr in path_prs
        if pr.get("number")
    }
    return [
        pr["number"]
        for pr in prs
        if pr.get("number")
        and (
            pr["number"] in path_numbers
            or common.has_harfbuzz_evidence(
                pr.get("title") or "",
                "",
            )
        )
    ]


def write_page(
    branch: str,
    all_branches: list[str],
    *,
    force: bool = False,
    min_core: tuple | None = None,
    max_core: tuple | None = None,
) -> str | None:
    try:
        from_ref, to_ref, version = determine_diff_range(branch)
    except (RuntimeError, subprocess.CalledProcessError) as error:
        common.log(
            "  WARNING: Could not determine diff range for {}: {}"
            .format(branch, error)
        )
        return None
    version_tuple = common.core_tuple(version)
    if min_core is not None and version_tuple < min_core:
        common.log("  Skipping {} (below --min-version).".format(version))
        return None
    if max_core is not None and version_tuple > max_core:
        common.log("  Skipping {} (above --max-version).".format(version))
        return None
    if common.is_below_history_floor(version):
        common.log(
            "  Skipping {} (below history floor {}).".format(
                version, common.history_floor()
            )
        )
        return None

    from_display = common.removeprefix(from_ref, "origin/")
    to_display = common.removeprefix(to_ref, "origin/")
    if re.fullmatch(r"[0-9a-f]{7,}", from_display):
        from_display = from_display[:12]
    status, superseded_by, supersedes = _page_status(branch, version)
    prs = sources.get_prs_from_diff(from_ref, to_ref)
    common.log(
        "  Found {} PR(s), diff: {}..{}".format(
            len(prs), from_display, to_display
        )
    )
    page_path = common.RELEASES_DIR / _page_filename(branch, version)
    is_head = branch == "main" or branch.endswith(".x")
    if is_head and not prs:
        model.prune_page_and_sources(page_path)
        return None

    api_diff_link = (
        "{}/index.md".format(version)
        if not is_head and (common.RELEASES_DIR / version).is_dir()
        else None
    )
    co_releases = common.load_co_release_map()
    harfbuzz = None
    harfbuzz_version = co_releases.get(version)
    if not is_head and harfbuzz_version:
        hb_prs = sources.get_prs_from_diff(
            from_ref, to_ref, paths=sources.HARFBUZZ_PATHSPECS
        )
        harfbuzz = {
            "version": harfbuzz_version,
            "api_diff_link": "harfbuzzsharp/{}/index.md".format(
                harfbuzz_version
            ),
            "prs": _harfbuzz_pr_numbers(prs, hb_prs),
        }

    base_version = _base_version(from_ref, version)
    previous_harfbuzz = _previous_co_release_version(
        base_version,
        supersedes,
        co_releases,
    )
    if harfbuzz and previous_harfbuzz:
        harfbuzz["previous_version"] = previous_harfbuzz
    milestones = model.collect_preview_milestones(version, base_version)
    shipments, exact_prs = (
        model.collect_shipments(version, base_version)
        if not is_head else ([], [])
    )
    numbers = {pr.get("number") for pr in prs}
    for pr in exact_prs:
        if pr.get("number") not in numbers:
            prs.append(pr)
            numbers.add(pr.get("number"))

    sources.resolve_pr_authors(prs)
    sources.resolve_skia_links(prs)
    sources.resolve_fixed_issues(prs)
    metadata = {
        "branch": branch,
        "version": version,
        "status": status,
        "from": from_display,
        "to": to_display,
        "base_version": base_version,
        "shipments": shipments,
    }
    if superseded_by:
        metadata["superseded_by"] = superseded_by
    if supersedes:
        metadata["supersedes"] = supersedes
    if milestones:
        metadata["pr_buckets"] = model.bucket_prs_by_milestone(
            prs, milestones, from_ref
        )
    if api_diff_link:
        metadata["api_diff_link"] = api_diff_link
    if harfbuzz:
        metadata["harfbuzz"] = harfbuzz

    stem = page_path.stem
    notes = common.load_notes_sidecar(stem, common.RELEASES_DIR)
    breaking = (
        common.load_breaking_companions(version, common.RELEASES_DIR)
        if not is_head else None
    )
    companions = {}
    if notes:
        companions["notes"] = notes
    if api_diff_link:
        companions["apidiff"] = {"path": api_diff_link}
    if breaking:
        companions["breaking"] = breaking
    if companions:
        metadata["companions"] = companions

    data = model.build_data_json(prs, metadata)
    return model.write_page_outputs(
        page_path, data, len(prs), force=force
    )


def warn_orphan_notes_sidecars() -> list[str]:
    orphans = []
    for base_dir in (
        common.RELEASES_DIR,
        common.RELEASES_DIR / "harfbuzzsharp",
    ):
        source = base_dir / "_sources"
        if not source.is_dir():
            continue
        for file in sorted(source.glob("*.notes.md")):
            stem = file.name[:-len(".notes.md")]
            if not (base_dir / "{}.md".format(stem)).is_file():
                common.log(
                    "WARNING: orphan manual notes sidecar {} has no matching page"
                    .format(file)
                )
                orphans.append(str(file))
    return orphans


def generate(
    *,
    force: bool = False,
    polish_list_path=None,
    min_core: tuple | None = None,
    max_core: tuple | None = None,
) -> None:
    common.log("Fetching remote branches...")
    common.run(["git", "fetch", "origin", "--unshallow", "--quiet"], check=False)
    try:
        common.run([
            "git", "fetch", "origin",
            "refs/heads/release/*:refs/remotes/origin/release/*",
            "refs/heads/main:refs/remotes/origin/main",
            "--quiet",
        ])
    except subprocess.CalledProcessError:
        common.log("ERROR: git fetch failed.")
        raise
    branches = sources.list_remote_release_branches()
    if not branches:
        raise RuntimeError("No release branches found after fetch")
    servicing = [branch for branch in branches if branch.endswith(".x")]
    canonical = sorted(
        _canonical_branches(branches).values(),
        key=common.release_branch_sort_key,
    )
    contexts = []
    skipped = 0
    for branch in ["main", *servicing, *canonical]:
        if branch == "main":
            version_hint = common.get_upcoming_version()
        elif branch.endswith(".x"):
            version_hint = sources.get_version_from_remote_branch(branch)
        else:
            version_hint = common.version_from_branch(branch)
        if not version_hint:
            skipped += 1
            continue
        hint = common.core_tuple(version_hint)
        if (
            (min_core is not None and hint < min_core)
            or (max_core is not None and hint > max_core)
            or common.is_below_history_floor(version_hint)
        ):
            skipped += 1
            continue
        common.log("\n--- Processing: {} ---".format(branch))
        context = write_page(
            branch,
            branches,
            force=force,
            min_core=min_core,
            max_core=max_core,
        )
        if context:
            contexts.append(context)
        else:
            skipped += 1
    warn_orphan_notes_sidecars()
    common.log("")
    common.log(
        "Processed: {}, Skipped/unchanged: {}".format(len(contexts), skipped)
    )
    common.write_polish_list(contexts, polish_list_path)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--force", action="store_true")
    parser.add_argument("--polish-list")
    parser.add_argument("--min-version")
    parser.add_argument("--max-version")
    args = parser.parse_args()
    generate(
        force=args.force,
        polish_list_path=args.polish_list,
        min_core=(
            common.core_tuple(args.min_version)
            if args.min_version else None
        ),
        max_core=(
            common.core_tuple(args.max_version)
            if args.max_version else None
        ),
    )


if __name__ == "__main__":
    try:
        main()
    except (RuntimeError, subprocess.CalledProcessError) as error:
        common.log("ERROR: {}".format(error))
        raise SystemExit(1)
