#!/usr/bin/env python3
"""Check source integrity using diff-of-diffs for both upstream and interop layers.

Compares old fork patches (skiasharp vs old upstream) against new fork patches
(PR head vs new upstream). Produces two sections:
- upstream_integrity: files OUTSIDE interop dirs (should rarely change)
- interop_integrity: files INSIDE interop dirs (expected to change, still needs review)
Both use the same added/removed/changed/unchanged structure with diffs stored per-item.
"""
import argparse
import os
import subprocess
import sys
import tempfile

INTEROP_DIRS = [
    "include/c/",
    "include/xamarin/",
    "src/c/",
    "src/xamarin/",
]


def eprint(*args, **kwargs):
    """Print to stderr (status messages)."""
    print(*args, file=sys.stderr, **kwargs)


def get_patch_content(diff: str) -> str:
    """Keep only +/- content lines, strip diff headers to avoid false positives from line number shifts."""
    lines = []
    for line in diff.split("\n"):
        if (line.startswith("+") and not line.startswith("+++ ")) or \
           (line.startswith("-") and not line.startswith("--- ")):
            lines.append(line)
    return "\n".join(lines)


def git_run(args: list, cwd: str) -> str:
    """Run a git command and return stdout as string."""
    result = subprocess.run(
        ["git"] + args,
        cwd=cwd,
        capture_output=True,
        text=True,
    )
    if result.returncode != 0:
        raise RuntimeError(f"git {' '.join(args)} failed (exit {result.returncode}): {result.stderr.strip()}")
    return result.stdout


def get_diff_of_diffs(
    skia_root: str,
    old_upstream_branch: str,
    new_upstream_branch: str,
    base_sha: str,
    pr_head: str,
    path_args: list,
    label: str,
    output_dir: str,
    output_sub_dir: str,
) -> dict:
    """Compute diff-of-diffs for a set of path args."""

    # Old fork delta
    old_raw = git_run(
        ["diff", "--name-only", f"{old_upstream_branch}..{base_sha}", "--"] + path_args,
        cwd=skia_root,
    )
    old_files = [f.strip() for f in old_raw.strip().split("\n") if f.strip()]
    old_set = set(old_files)

    # New fork delta
    new_raw = git_run(
        ["diff", "--name-only", f"{new_upstream_branch}..{pr_head}", "--"] + path_args,
        cwd=skia_root,
    )
    new_files = [f.strip() for f in new_raw.strip().split("\n") if f.strip()]
    new_set = set(new_files)

    # Categorize
    added_files = sorted(f for f in new_files if f not in old_set)
    removed_files = sorted(f for f in old_files if f not in new_set)
    common_files = sorted(f for f in new_files if f in old_set)

    # Detect changed vs unchanged using patch content comparison
    changed_files = []
    unchanged_count = 0
    for file in common_files:
        old_patch = git_run(
            ["diff", f"{old_upstream_branch}..{base_sha}", "--", file],
            cwd=skia_root,
        )
        new_patch = git_run(
            ["diff", f"{new_upstream_branch}..{pr_head}", "--", file],
            cwd=skia_root,
        )
        if get_patch_content(old_patch) == get_patch_content(new_patch):
            unchanged_count += 1
        else:
            changed_files.append(file)

    # Build result arrays with diffs
    added = []
    for file in added_files:
        diff = git_run(
            ["diff", f"{new_upstream_branch}..{pr_head}", "--", file],
            cwd=skia_root,
        ).strip()
        added.append({"path": file, "diff": diff})

    removed = []
    for file in removed_files:
        diff = git_run(
            ["diff", f"{old_upstream_branch}..{base_sha}", "--", file],
            cwd=skia_root,
        ).strip()
        removed.append({"path": file, "diff": diff})

    changed = []
    for file in changed_files:
        old_diff = git_run(
            ["diff", f"{old_upstream_branch}..{base_sha}", "--", file],
            cwd=skia_root,
        ).strip()
        new_diff = git_run(
            ["diff", f"{new_upstream_branch}..{pr_head}", "--", file],
            cwd=skia_root,
        ).strip()

        # Compute patch_diff: diff between the two patches — shows exactly what changed
        old_tmp = tempfile.NamedTemporaryFile(mode="w", suffix=".diff", delete=False)
        new_tmp = tempfile.NamedTemporaryFile(mode="w", suffix=".diff", delete=False)
        try:
            old_tmp.write(old_diff)
            old_tmp.close()
            new_tmp.write(new_diff)
            new_tmp.close()
            patch_result = subprocess.run(
                ["git", "diff", "--no-index", old_tmp.name, new_tmp.name],
                capture_output=True,
                text=True,
            )
            # git diff --no-index returns 0 (no diff), 1 (diffs found), >1 (error)
            if patch_result.returncode > 1:
                raise RuntimeError(
                    f"'git diff --no-index' failed with exit code {patch_result.returncode}: "
                    f"{patch_result.stderr}"
                )
            patch_diff = patch_result.stdout.strip()
        finally:
            os.unlink(old_tmp.name)
            os.unlink(new_tmp.name)

        # Strip temp file header lines and replace with meaningful labels
        cleaned_lines = []
        for line in patch_diff.splitlines():
            if line.startswith("diff --git ") or line.startswith("index "):
                continue
            elif line.startswith("--- "):
                cleaned_lines.append("--- old-patch (skiasharp vs old-upstream)")
            elif line.startswith("+++ "):
                cleaned_lines.append("+++ new-patch (pr-head vs new-upstream)")
            else:
                cleaned_lines.append(line)
        patch_diff = "\n".join(cleaned_lines)

        # Direct branch-to-branch diff: the actual change between the two fork versions.
        # This is the simplest view — what actually changed in the file between base and PR.
        direct_diff = git_run(
            ["diff", f"{base_sha}..{pr_head}", "--", file],
            cwd=skia_root,
        ).strip()

        changed.append({
            "path": file,
            "diff": direct_diff,
            "oldDiff": old_diff,
            "newDiff": new_diff,
            "patchDiff": patch_diff,
        })

    # Save diffs to output dir
    if output_dir and output_sub_dir:
        out_path = os.path.join(output_dir, output_sub_dir)
        os.makedirs(out_path, exist_ok=True)
        for item in added:
            safe = item["path"].replace("/", "_")
            with open(os.path.join(out_path, f"added-{safe}.diff"), "w") as f:
                f.write(item["diff"])
        for item in removed:
            safe = item["path"].replace("/", "_")
            with open(os.path.join(out_path, f"removed-{safe}.diff"), "w") as f:
                f.write(item["diff"])
        for item in changed:
            safe = item["path"].replace("/", "_")
            with open(os.path.join(out_path, f"changed-{safe}.old.diff"), "w") as f:
                f.write(item["oldDiff"])
            with open(os.path.join(out_path, f"changed-{safe}.new.diff"), "w") as f:
                f.write(item["newDiff"])
            with open(os.path.join(out_path, f"changed-{safe}.patch.diff"), "w") as f:
                f.write(item["patchDiff"])

    status = "REVIEW_REQUIRED" if (added or removed or changed) else "PASS"

    eprint()
    eprint(f"  [{label}] Old delta: {len(old_set)} | New delta: {len(new_set)}")
    if status == "PASS":
        eprint(f"  ✅ {label}: PASS — {unchanged_count} unchanged, 0 added/removed/changed")
    else:
        eprint(f"  🔍 {label}: REVIEW_REQUIRED")
        if added:
            eprint(f"     Added ({len(added)}):")
            for a in added:
                eprint(f"       + {a['path']}")
        if removed:
            eprint(f"     Removed ({len(removed)}):")
            for r in removed:
                eprint(f"       - {r['path']}")
        if changed:
            eprint(f"     Changed ({len(changed)}):")
            for c in changed:
                eprint(f"       ~ {c['path']}")
        eprint(f"     Unchanged: {unchanged_count}")

    return {
        "status": status,
        "added": added,
        "removed": removed,
        "changed": changed,
        "unchanged": unchanged_count,
    }


def run_check(
    skia_root: str,
    old_upstream_branch: str,
    new_upstream_branch: str,
    base_sha: str,
    pr_head: str,
    output_dir: str,
) -> dict:
    """Run source integrity checks. Returns dict with upstreamIntegrity and interopIntegrity."""

    # Upstream integrity: files OUTSIDE interop dirs
    upstream_exclude = ['.'] + [f":(exclude){d}" for d in INTEROP_DIRS]
    eprint("▸ Upstream integrity (outside interop dirs)")
    upstream = get_diff_of_diffs(
        skia_root=skia_root,
        old_upstream_branch=old_upstream_branch,
        new_upstream_branch=new_upstream_branch,
        base_sha=base_sha,
        pr_head=pr_head,
        path_args=upstream_exclude,
        label="upstream",
        output_dir=output_dir,
        output_sub_dir="upstream-integrity",
    )

    # Interop integrity: files INSIDE interop dirs
    eprint()
    eprint("▸ Interop integrity (inside interop dirs)")
    interop = get_diff_of_diffs(
        skia_root=skia_root,
        old_upstream_branch=old_upstream_branch,
        new_upstream_branch=new_upstream_branch,
        base_sha=base_sha,
        pr_head=pr_head,
        path_args=list(INTEROP_DIRS),
        label="interop",
        output_dir=output_dir,
        output_sub_dir="interop-integrity",
    )

    return {
        "upstreamIntegrity": upstream,
        "interopIntegrity": interop,
    }


def main():
    parser = argparse.ArgumentParser(description="Check source integrity using diff-of-diffs.")
    parser.add_argument("--skia-root", required=True, help="Path to the skia submodule.")
    parser.add_argument("--old-upstream-branch", required=True, help="Old upstream branch ref.")
    parser.add_argument("--new-upstream-branch", required=True, help="New upstream branch ref.")
    parser.add_argument("--base-sha", required=True, help="Base (skiasharp) branch SHA.")
    parser.add_argument("--pr-head", required=True, help="PR head SHA or ref.")
    parser.add_argument("--output-dir", default="", help="Directory to save per-file diffs.")
    args = parser.parse_args()

    import json
    result = run_check(
        skia_root=args.skia_root,
        old_upstream_branch=args.old_upstream_branch,
        new_upstream_branch=args.new_upstream_branch,
        base_sha=args.base_sha,
        pr_head=args.pr_head,
        output_dir=args.output_dir,
    )
    json.dump(result, sys.stdout, indent=2)
    print()


if __name__ == "__main__":
    main()
