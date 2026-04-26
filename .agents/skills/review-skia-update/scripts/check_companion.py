#!/usr/bin/env python3
"""Check companion SkiaSharp PR: categorize changed files and produce diffs.

Compares the companion PR branch against the SkiaSharp main branch.
Filters out generated files (*Api.generated.cs) and produces the same
sourceFile structure used by upstream/interop integrity checks.
"""
import fnmatch
import os
import subprocess
import sys

# Files matching these patterns are auto-generated and skipped
GENERATED_PATTERNS = [
    "*Api.generated.cs",
]

# Files matching these patterns are skipped (not interesting for review)
SKIP_PATTERNS = [
    "*.generated.cs",
]


def eprint(*args, **kwargs):
    """Print to stderr (status messages)."""
    print(*args, file=sys.stderr, **kwargs)


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


def is_generated(path: str) -> bool:
    """Check if a file matches any generated/skip pattern."""
    basename = os.path.basename(path)
    for pat in SKIP_PATTERNS:
        if fnmatch.fnmatch(basename, pat):
            return True
    return False


def run_check(
    repo_root: str,
    base_ref: str,
    pr_ref: str,
    output_dir: str,
) -> dict:
    """Run companion PR file check.

    Args:
        repo_root: Path to the SkiaSharp repo root.
        base_ref: Base branch ref (e.g., 'origin/main').
        pr_ref: PR head ref (e.g., 'FETCH_HEAD' or a commit SHA).
        output_dir: Directory to save per-file diffs.

    Returns:
        dict with status, added, changed, unchanged matching sourceFile shape.
    """
    eprint("▸ Companion PR file analysis")

    # Find the merge base between main and the PR
    merge_base = git_run(
        ["merge-base", base_ref, pr_ref],
        cwd=repo_root,
    ).strip()
    eprint(f"   Merge base: {merge_base[:12]}")

    # Get all changed files between merge base and PR head
    diff_output = git_run(
        ["diff", "--name-status", f"{merge_base}..{pr_ref}"],
        cwd=repo_root,
    )

    # Parse into (status, path) tuples
    file_entries = []
    for line in diff_output.strip().split("\n"):
        if not line.strip():
            continue
        parts = line.split("\t", 1)
        if len(parts) == 2:
            status_code, path = parts[0].strip(), parts[1].strip()
            file_entries.append((status_code, path))

    # Filter and categorize
    added_files = []
    changed_files = []
    skipped_count = 0

    for status_code, path in file_entries:
        if is_generated(path):
            skipped_count += 1
            continue

        if status_code == "A":
            added_files.append(path)
        elif status_code in ("M", "R", "C", "T"):
            changed_files.append(path)
        elif status_code.startswith("R") or status_code.startswith("C"):
            # Renamed/copied with similarity — treat as changed
            changed_files.append(path)
        # D (deleted) is unusual for a companion PR but we skip it

    added_files.sort()
    changed_files.sort()

    # Build result arrays with diffs
    added = []
    for path in added_files:
        diff = git_run(
            ["diff", f"{merge_base}..{pr_ref}", "--", path],
            cwd=repo_root,
        ).strip()
        added.append({"path": path, "diff": diff})

    changed = []
    for path in changed_files:
        # Direct diff: merge_base → PR head (what actually changed)
        diff = git_run(
            ["diff", f"{merge_base}..{pr_ref}", "--", path],
            cwd=repo_root,
        ).strip()
        changed.append({"path": path, "diff": diff})

    # Save diffs to output dir
    if output_dir:
        out_path = os.path.join(output_dir, "companion-pr")
        os.makedirs(out_path, exist_ok=True)
        for item in added:
            safe = item["path"].replace("/", "_").replace("\\", "_")
            with open(os.path.join(out_path, f"added-{safe}.diff"), "w") as f:
                f.write(item["diff"])
        for item in changed:
            safe = item["path"].replace("/", "_").replace("\\", "_")
            with open(os.path.join(out_path, f"changed-{safe}.diff"), "w") as f:
                f.write(item["diff"])

    status = "REVIEW_REQUIRED" if (added or changed) else "PASS"

    eprint()
    eprint(f"  Total files in PR: {len(file_entries)}")
    eprint(f"  Skipped (generated): {skipped_count}")
    if status == "PASS":
        eprint(f"  ✅ Companion PR: PASS — no non-generated changes")
    else:
        eprint(f"  🔍 Companion PR: REVIEW_REQUIRED")
        if added:
            eprint(f"     Added ({len(added)}):")
            for a in added:
                eprint(f"       + {a['path']}")
        if changed:
            eprint(f"     Changed ({len(changed)}):")
            for c in changed:
                eprint(f"       ~ {c['path']}")
        eprint(f"     Unchanged (generated/skipped): {skipped_count}")

    return {
        "status": status,
        "added": added,
        "changed": changed,
        "unchanged": skipped_count,
    }


def main():
    import argparse
    import json

    parser = argparse.ArgumentParser(description="Check companion PR files.")
    parser.add_argument("--repo-root", required=True, help="Path to SkiaSharp repo root.")
    parser.add_argument("--base-ref", required=True, help="Base branch ref (e.g., origin/main).")
    parser.add_argument("--pr-ref", required=True, help="PR head ref or SHA.")
    parser.add_argument("--output-dir", default="", help="Directory to save per-file diffs.")
    args = parser.parse_args()

    result = run_check(
        repo_root=args.repo_root,
        base_ref=args.base_ref,
        pr_ref=args.pr_ref,
        output_dir=args.output_dir,
    )
    json.dump(result, sys.stdout, indent=2)
    print()


if __name__ == "__main__":
    main()
