#!/usr/bin/env python3
"""Build caching tool — cache keys, change analysis, and validation.

Reads the job tree from repo-deps.json and provides three commands:

  cache-key   Compute a cache key for a job (hashes files + submodule SHAs)
  analyze     Determine which jobs need to run based on changed files
  validate    Check all tracked files are covered by a job or ignore pattern

Usage:
  python3 scripts/infra/caching/repo-deps.py cache-key --job native/ios --name native_ios_macos
  python3 scripts/infra/caching/repo-deps.py analyze --base HEAD~1
  python3 scripts/infra/caching/repo-deps.py validate
"""

import argparse
import fnmatch
import hashlib
import os
import re
import subprocess
import sys
from pathlib import Path

# ---------------------------------------------------------------------------
# Config loading
# ---------------------------------------------------------------------------
import json

def load_config(path):
    text = Path(path).read_text(encoding="utf-8")
    return json.loads(text)

# ---------------------------------------------------------------------------
# Tree walking
# ---------------------------------------------------------------------------

def walk_tree(node, prefix="", parent_paths=None, parent_subs=None):
    """Walk the job tree, yielding (full_path, accumulated_paths, accumulated_submodules) for each node."""
    paths = list(parent_paths or []) + list(node.get("include", []))
    subs = list(parent_subs or []) + list(node.get("submodules", []))
    name = prefix.lstrip("/")

    yield name, paths, subs

    for child_name, child_node in (node.get("children") or {}).items():
        child_prefix = f"{prefix}/{child_name}" if prefix else child_name
        yield from walk_tree(child_node, child_prefix, paths, subs)


def resolve_job(config, job_path):
    """Find a job by its slash-separated path and return (paths, submodules)."""
    # Start with root-level include
    accumulated_paths = list(config.get("include", []))
    accumulated_subs = []

    parts = job_path.strip("/").split("/")
    node = config["jobs"]

    for i, part in enumerate(parts):
        if part not in node:
            return None, None
        node = node[part]
        accumulated_paths.extend(node.get("include", []))
        accumulated_subs.extend(node.get("submodules", []))
        if i < len(parts) - 1:
            node = node.get("children", {})

    return accumulated_paths, accumulated_subs


def all_jobs(config):
    """Return dict of {job_path: (paths, submodules)} for all leaf and branch nodes."""
    root_paths = list(config.get("include", []))
    root_subs = []

    result = {}
    for root_name, root_node in config["jobs"].items():
        for path, paths, subs in walk_tree(root_node, root_name, root_paths, root_subs):
            result[path] = (paths, subs)
    return result

# ---------------------------------------------------------------------------
# Path matching
# ---------------------------------------------------------------------------

def match(filepath, pattern):
    regex = "^" + pattern.replace("**", "§§").replace("*", "[^/]*").replace("§§", ".*") + "$"
    return bool(re.match(regex, filepath))


def match_any(filepath, patterns):
    return any(match(filepath, p) for p in patterns)

# ---------------------------------------------------------------------------
# Git helpers
# ---------------------------------------------------------------------------

def git(*args):
    result = subprocess.run(["git"] + list(args), capture_output=True, text=True)
    return result.stdout.strip() if result.returncode == 0 else ""


def get_submodule_sha(path):
    # For PR builds, use the source branch commit (stable across re-merges).
    # The merge commit changes with every push even if content is identical,
    # but the source branch tip only changes when we actually push new code.
    # For non-PR builds, HEAD is the branch tip (already stable).
    source_sha = os.environ.get("SYSTEM_PULLREQUEST_SOURCECOMMITID", "")
    ref = source_sha if source_sha else "HEAD"
    line = git("ls-tree", ref, path)
    m = re.search(r"([0-9a-f]{40})", line)
    if m:
        return m.group(1)
    # Fallback to HEAD if source commit doesn't have the tree (shallow)
    if ref != "HEAD":
        line = git("ls-tree", "HEAD", path)
        m = re.search(r"([0-9a-f]{40})", line)
        return m.group(1) if m else "unknown"
    return "unknown"

# ---------------------------------------------------------------------------
# Commands
# ---------------------------------------------------------------------------

def cmd_cache_key(config, args):
    paths, subs = resolve_job(config, args.job)
    if paths is None:
        print(f"ERROR: Job '{args.job}' not found in config", file=sys.stderr)
        return 1

    # Submodule SHAs
    sub_shas = []
    for sub in sorted(set(subs)):
        sha = os.environ.get(sub.replace("/", "_").replace("-", "_").upper() + "_SHA") or get_submodule_sha(sub)
        sub_shas.append(f"{sub}:{sha}")

    # Hash files
    file_hashes = []
    hashed_dirs = []
    for pattern in sorted(set(paths)):
        dir_path = re.sub(r"/?\*\*$", "", pattern)
        p = Path(dir_path)
        if p.is_dir():
            files = sorted(f for f in p.rglob("*") if f.is_file()
                           and not any(x in f.parts for x in ("bin", "obj", "libs", "tools", ".git")))
            for f in files:
                file_hashes.append(hashlib.sha256(f.read_bytes()).hexdigest())
            hashed_dirs.append(f"{dir_path}/ ({len(files)} files)")
        elif p.is_file():
            file_hashes.append(hashlib.sha256(p.read_bytes()).hexdigest())
            hashed_dirs.append(dir_path)

    composite = hashlib.sha256("|".join(file_hashes).encode()).hexdigest()[:24]
    sub_parts = "_".join(s.replace(":", "-").replace("/", "-") for s in sub_shas)
    job_part = args.job.replace("/", "-")
    cache_key = f"{job_part}_{args.name}_{sub_parts}_{composite}"

    print()
    print("\n=== Cache Key ===")
    print(f"Name:     {args.name}")
    print(f"Job:      {args.job}")
    for s in sub_shas:
        print(f"Sub:      {s}")
    print(f"Files:    {composite} ({len(file_hashes)} files)")
    print(f"Key:      {cache_key}")
    print("\nHashed:")
    for d in hashed_dirs:
        print(f"  - {d}")

    if os.environ.get("BUILD_BUILDID"):
        print(f"##vso[task.setvariable variable=CACHE_KEY]{cache_key}")

    return 0


def cmd_analyze(config, args):
    base = args.base
    if not base:
        target = os.environ.get("SYSTEM_PULLREQUEST_TARGETBRANCH", "")
        if target:
            target = target.replace("refs/heads/", "origin/")
            base = git("merge-base", target, "HEAD")
            if not base:
                print(f"Cannot determine merge-base with {target} (shallow checkout?)")
                print("Run with --base <ref> or use a deeper fetch")
                return 0
        else:
            base = "HEAD~1"

    # Protected branches always run everything
    branch = os.environ.get("BUILD_SOURCEBRANCH", "")
    is_protected = branch in ("refs/heads/main",) or branch.startswith("refs/heads/release/") or branch.startswith("refs/heads/develop")

    if is_protected:
        print(f"Protected branch '{branch}' — all jobs run")
        return 0

    all_changed = git("diff", "--name-only", base, "HEAD").splitlines()
    if not all_changed:
        print(f"No changed files between {base[:10]} and HEAD")
        return 0

    # Separate existing files from deleted ones
    # Deleted files still trigger their job but shouldn't cause "unmatched" errors
    changed = [f for f in all_changed if os.path.exists(f)]
    deleted = [f for f in all_changed if not os.path.exists(f)]

    print(f"Base: {base}")
    print(f"Changed: {len(changed)} files, {len(deleted)} deleted\n")

    jobs = all_jobs(config)
    ignore = [p for p in config.get("exclude", []) if not p.startswith("#")]

    # Match ALL changed files (including deleted) to jobs
    # A deleted file in native/ios/ should still trigger native/ios
    results = {}
    for job_path, (paths, subs) in jobs.items():
        matched = any(match_any(f, paths) for f in all_changed)
        if not matched:
            matched = any(f == s or f.startswith(s + "/") for f in changed for s in subs)
        results[job_path] = matched

    # Propagate: if a parent is triggered, all children are too
    for job_path in sorted(results.keys(), key=len):
        if results[job_path]:
            for other in results:
                if other.startswith(job_path + "/"):
                    results[other] = True

    # Check unmatched — only for files that still exist (deleted files are fine)
    all_patterns = []
    all_subs = []
    for paths, subs in jobs.values():
        all_patterns.extend(paths)
        all_subs.extend(subs)

    unmatched = []
    for f in changed:  # only existing files, not deleted
        if match_any(f, all_patterns + ignore):
            continue
        if any(f == s or f.startswith(s + "/") for s in all_subs):
            continue
        unmatched.append(f)

    if unmatched:
        print("❌ UNMATCHED FILES (not covered by any job or exclude):")
        for f in unmatched:
            print(f"{f}")
        print(f"\nAdd to a job or exclude list in repo-deps.json")
        return 1

    # Output
    print("\n=== Job Analysis ===")
    for job_path in sorted(results.keys()):
        run = results[job_path]
        icon = "🔨" if run else "⏭️"
        label = "RUN" if run else "SKIP"
        print(f"{icon} {job_path:<30} {label}")
    return 0


def cmd_validate(config, args):
    tracked = git("ls-files", "-z").split("\0")
    tracked = [f.strip('"').encode().decode("unicode_escape") if f.startswith('"') else f
               for f in tracked if f]

    jobs = all_jobs(config)
    exclude = [p for p in config.get("exclude", []) if not p.startswith("#")]

    all_include = list(config.get("include", []))
    all_subs = []
    for paths, subs in jobs.values():
        all_include.extend(paths)
        all_subs.extend(subs)

    # Categorize every file
    job_files = {}     # job_path → count
    excluded_count = 0
    uncovered = []
    overlaps = []

    for f in tracked:
        in_exclude = match_any(f, exclude)

        # Find which job(s) include this file
        matched_jobs = []
        for job_path, (paths, subs) in jobs.items():
            if match_any(f, paths) or any(f == s or f.startswith(s + "/") for s in subs):
                matched_jobs.append(job_path)

        # Also check root include
        in_root = match_any(f, config.get("include", []))

        if in_exclude and (matched_jobs or in_root):
            overlaps.append(f)
        elif matched_jobs or in_root:
            for j in matched_jobs:
                job_files[j] = job_files.get(j, 0) + 1
        elif in_exclude:
            excluded_count += 1
        else:
            uncovered.append(f)

    # Output
    print("\n=== Validation ===")
    print(f"Total tracked files: {len(tracked)}")
    print(f"Excluded:            {excluded_count}")
    print(f"Uncovered:           {len(uncovered)}")
    print("\nJobs:")

    # Show jobs with file counts
    for job_path in sorted(jobs.keys()):
        count = job_files.get(job_path, 0)
        _, subs = jobs[job_path]
        sub_str = ""
        if subs:
            sub_shas = [f"{s}:{get_submodule_sha(s)[:12]}" for s in sorted(set(subs))]
            sub_str = "  " + " ".join(sub_shas)
        print(f"{job_path:<30} {count:>4} files{sub_str}")

    if overlaps:
        print(f"\nℹ️  {len(overlaps)} files match both include and exclude (include wins):")
        for f in overlaps[:10]:
            print(f"  {f}")
        if len(overlaps) > 10:
            print(f"  ... +{len(overlaps) - 10} more")

    if uncovered:
        print(f"\n❌ UNCOVERED FILES:")
        for f in uncovered:
            print(f"{f}")
        print(f"\nAdd to a job or exclude list in repo-deps.json")
        return 1

    print("\n✅ All files covered!")
    return 0

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(description="Build caching tool")
    parser.add_argument("--config", default=None, help="Config file path")
    sub = parser.add_subparsers(dest="command")

    ck = sub.add_parser("cache-key", help="Compute cache key for a job")
    ck.add_argument("--job", required=True, help="Job path (e.g. shared/native/ios)")
    ck.add_argument("--name", required=True, help="ADO job name")
    ck.add_argument("--docker", default="", help="Docker context directory")

    an = sub.add_parser("analyze", help="Analyze which jobs to run")
    an.add_argument("--base", default="", help="Base SHA for diff")

    sub.add_parser("validate", help="Check all files are covered")

    args = parser.parse_args()
    if not args.command:
        parser.print_help()
        return 1

    config_path = args.config or str(Path(__file__).parent / "repo-deps.json")
    config = load_config(config_path)

    if args.command == "cache-key":
        return cmd_cache_key(config, args)
    elif args.command == "analyze":
        return cmd_analyze(config, args)
    elif args.command == "validate":
        return cmd_validate(config, args)


if __name__ == "__main__":
    sys.exit(main())
