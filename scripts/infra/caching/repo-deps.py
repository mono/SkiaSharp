#!/usr/bin/env python3
"""Build caching tool — cache keys, change analysis, and validation.

Reads the job tree from repo-deps.yaml and provides three commands:

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
# YAML loading (PyYAML or fallback)
# ---------------------------------------------------------------------------
try:
    import yaml
except ImportError:
    yaml = None

def load_yaml(path):
    text = Path(path).read_text(encoding="utf-8")
    if yaml:
        return yaml.safe_load(text)
    # Minimal fallback: try json (won't work for yaml, but gives a clear error)
    import json
    try:
        return json.loads(text)
    except Exception:
        print("ERROR: PyYAML not installed. Run: pip install pyyaml", file=sys.stderr)
        sys.exit(1)

# ---------------------------------------------------------------------------
# Tree walking
# ---------------------------------------------------------------------------

def walk_tree(node, prefix="", parent_paths=None, parent_subs=None):
    """Walk the job tree, yielding (full_path, accumulated_paths, accumulated_submodules) for each node."""
    paths = list(parent_paths or []) + list(node.get("paths", []))
    subs = list(parent_subs or []) + list(node.get("submodules", []))
    name = prefix.lstrip("/")

    yield name, paths, subs

    for child_name, child_node in (node.get("children") or {}).items():
        child_prefix = f"{prefix}/{child_name}" if prefix else child_name
        yield from walk_tree(child_node, child_prefix, paths, subs)


def resolve_job(config, job_path):
    """Find a job by its slash-separated path and return (paths, submodules)."""
    # Start with shared paths/subs from root
    shared = config.get("shared", {})
    accumulated_paths = list(shared.get("paths", []))
    accumulated_subs = list(shared.get("submodules", []))

    parts = job_path.strip("/").split("/")
    node = config["jobs"]

    for i, part in enumerate(parts):
        if part not in node:
            return None, None
        node = node[part]
        accumulated_paths.extend(node.get("paths", []))
        accumulated_subs.extend(node.get("submodules", []))
        if i < len(parts) - 1:
            node = node.get("children", {})

    return accumulated_paths, accumulated_subs


def all_jobs(config):
    """Return dict of {job_path: (paths, submodules)} for all leaf and branch nodes."""
    shared = config.get("shared", {})
    shared_paths = list(shared.get("paths", []))
    shared_subs = list(shared.get("submodules", []))

    result = {}
    for root_name, root_node in config["jobs"].items():
        for path, paths, subs in walk_tree(root_node, root_name, shared_paths, shared_subs):
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
    line = git("ls-tree", "HEAD", path)
    m = re.search(r"([0-9a-f]{40})", line)
    return m.group(1) if m else "unknown"

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
    sub_key = "|".join(sub_shas)
    cache_key = f"{args.job}|{args.name}|{sub_key}|{composite}"

    print()
    print("╔══════════════════════════════════════════════════════════════╗")
    print("║  Cache Key                                                   ║")
    print("╠══════════════════════════════════════════════════════════════╣")
    print(f"║  Name:     {args.name}")
    print(f"║  Job:      {args.job}")
    for s in sub_shas:
        print(f"║  Sub:      {s}")
    print(f"║  Files:    {composite} ({len(file_hashes)} files)")
    print("╠══════════════════════════════════════════════════════════════╣")
    for d in hashed_dirs:
        print(f"║  {d}")
    print("╠══════════════════════════════════════════════════════════════╣")
    print(f"║  KEY: {cache_key}")
    print("╚══════════════════════════════════════════════════════════════╝")

    if os.environ.get("BUILD_BUILDID"):
        print(f"##vso[task.setvariable variable=CACHE_KEY]{cache_key}")

    return 0


def cmd_analyze(config, args):
    base = args.base
    if not base:
        target = os.environ.get("SYSTEM_PULLREQUEST_TARGETBRANCH", "")
        if target:
            target = target.replace("refs/heads/", "origin/")
            base = git("merge-base", target, "HEAD") or "HEAD~1"
        else:
            base = "HEAD~1"

    # Protected branches always run everything
    branch = os.environ.get("BUILD_SOURCEBRANCH", "")
    is_protected = branch in ("refs/heads/main",) or branch.startswith("refs/heads/release/") or branch.startswith("refs/heads/develop")

    if is_protected:
        print(f"Protected branch '{branch}' — all jobs run")
        return 0

    changed = git("diff", "--name-only", base, "HEAD").splitlines()
    if not changed:
        print("No changed files detected")
        return 0

    print(f"Base: {base}")
    print(f"Changed: {len(changed)} files\n")

    jobs = all_jobs(config)
    ignore = [p for p in config.get("ignore", []) if not p.startswith("#")]

    # Match files to jobs
    results = {}
    for job_path, (paths, subs) in jobs.items():
        matched = any(match_any(f, paths) for f in changed)
        if not matched:
            matched = any(f == s or f.startswith(s + "/") for f in changed for s in subs)
        results[job_path] = matched

    # Propagate: if a parent is triggered, all children are too
    for job_path in sorted(results.keys(), key=len):
        if results[job_path]:
            for other in results:
                if other.startswith(job_path + "/"):
                    results[other] = True

    # Check unmatched
    all_patterns = []
    all_subs = []
    for paths, subs in jobs.values():
        all_patterns.extend(paths)
        all_subs.extend(subs)

    unmatched = []
    for f in changed:
        if match_any(f, all_patterns + ignore):
            continue
        if any(f == s or f.startswith(s + "/") for s in all_subs):
            continue
        unmatched.append(f)

    if unmatched:
        print("❌ UNMATCHED FILES:")
        for f in unmatched:
            print(f"  {f}")
        print(f"\nAdd to a job or ignore in repo-deps.yaml")
        return 1

    # Output
    print("╔══════════════════════════════════════════════════╗")
    print("║  Job Analysis                                    ║")
    print("╠══════════════════════════════════════════════════╣")
    for job_path in sorted(results.keys()):
        run = results[job_path]
        icon = "🔨" if run else "⏭️"
        label = "RUN" if run else "SKIP"
        print(f"║  {icon} {job_path:<30} {label}")
    print("╚══════════════════════════════════════════════════╝")
    return 0


def cmd_validate(config, args):
    tracked = git("ls-files", "-z").split("\0")
    tracked = [f.strip('"').encode().decode("unicode_escape") if f.startswith('"') else f
               for f in tracked if f]

    jobs = all_jobs(config)
    ignore = [p for p in config.get("ignore", []) if not p.startswith("#")]

    all_patterns = []
    all_subs = []
    for paths, subs in jobs.values():
        all_patterns.extend(paths)
        all_subs.extend(subs)

    uncovered = []
    covered = 0
    for f in tracked:
        if match_any(f, all_patterns + ignore):
            covered += 1
        elif any(f == s or f.startswith(s + "/") for s in all_subs):
            covered += 1
        else:
            uncovered.append(f)

    print(f"Total: {len(tracked)} files")
    print(f"Covered: {covered}")
    print(f"Uncovered: {len(uncovered)}")

    if uncovered:
        print(f"\n❌ UNCOVERED FILES:")
        for f in uncovered:
            print(f"  {f}")
        print(f"\nAdd to a job or ignore in repo-deps.yaml")
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

    config_path = args.config or str(Path(__file__).parent / "repo-deps.yaml")
    config = load_yaml(config_path)

    if args.command == "cache-key":
        return cmd_cache_key(config, args)
    elif args.command == "analyze":
        return cmd_analyze(config, args)
    elif args.command == "validate":
        return cmd_validate(config, args)


if __name__ == "__main__":
    sys.exit(main())
