#!/usr/bin/env python3
"""Build caching tool — cache keys and validation.

Reads the job tree from repo-deps.json and provides two commands:

  cache-key   Compute a cache key for a job (hashes files + submodule SHAs)
  validate    Check all tracked files are covered by a job or exclude pattern

Usage:
  python3 scripts/infra/caching/repo-deps.py cache-key --job managed/package --name package_normal_windows
  python3 scripts/infra/caching/repo-deps.py cache-key --job managed/package --name test --base HEAD~1
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

def collect_all(node):
    """Collect ALL include paths and submodules from a node and ALL its children (recursively)."""
    paths = list(node.get("include", []))
    subs = list(node.get("submodules", []))
    for child_node in (node.get("children") or {}).values():
        cp, cs = collect_all(child_node)
        paths.extend(cp)
        subs.extend(cs)
    return paths, subs


def resolve_depends_on(config, node):
    """Resolve dependsOn references, collecting all paths+submodules from referenced jobs and their children.
    Returns (paths, subs, provenance) where provenance maps path → 'dep_name' or 'dep_name/child'."""
    dep_paths = []
    dep_subs = []
    dep_provenance = {}
    for dep_name in node.get("dependsOn", []):
        dep_node = config["jobs"].get(dep_name)
        if dep_node:
            # Root level of the dependency
            for p in dep_node.get("include", []):
                dep_paths.append(p)
                dep_provenance[p] = dep_name
            for s in dep_node.get("submodules", []):
                dep_subs.append(s)
                dep_provenance[s] = dep_name
            # Walk children
            for child_name, child_node in (dep_node.get("children") or {}).items():
                _collect_with_provenance(child_node, f"{dep_name}/{child_name}", dep_paths, dep_subs, dep_provenance)
    return dep_paths, dep_subs, dep_provenance


def _collect_with_provenance(node, prefix, paths, subs, provenance):
    """Recursively collect paths/subs with provenance labels."""
    for p in node.get("include", []):
        paths.append(p)
        provenance[p] = prefix
    for s in node.get("submodules", []):
        subs.append(s)
        provenance[s] = prefix
    for child_name, child_node in (node.get("children") or {}).items():
        _collect_with_provenance(child_node, f"{prefix}/{child_name}", paths, subs, provenance)


def _provenance_tree(node, prefix, provenance):
    """Build provenance labels for a tree (for virtual 'all' job)."""
    for p in node.get("include", []):
        provenance.setdefault(p, prefix)
    for s in node.get("submodules", []):
        provenance.setdefault(s, prefix)
    for child_name, child_node in (node.get("children") or {}).items():
        _provenance_tree(child_node, f"{prefix}/{child_name}", provenance)


def walk_tree(config, node, prefix="", parent_paths=None, parent_subs=None):
    """Walk the job tree, yielding (full_path, accumulated_paths, accumulated_submodules) for each node."""
    paths = list(parent_paths or []) + list(node.get("include", []))
    subs = list(parent_subs or []) + list(node.get("submodules", []))

    # Resolve dependsOn (accumulates all files from referenced jobs + their children)
    dep_paths, dep_subs, _ = resolve_depends_on(config, node)
    paths.extend(dep_paths)
    subs.extend(dep_subs)

    name = prefix.lstrip("/")

    yield name, paths, subs

    for child_name, child_node in (node.get("children") or {}).items():
        child_prefix = f"{prefix}/{child_name}" if prefix else child_name
        yield from walk_tree(config, child_node, child_prefix, paths, subs)


def resolve_job(config, job_path):
    """Find a job by its slash-separated path and return (paths, submodules, provenance).
    Special job 'all' creates a virtual node that depends on all root jobs."""
    accumulated_paths = list(config.get("include", []))
    accumulated_subs = []
    provenance = {}

    for p in accumulated_paths:
        provenance[p] = "global"

    # Virtual "all" job — depends on every root job
    if job_path == "all":
        for root_name, root_node in config["jobs"].items():
            dp, ds = collect_all(root_node)
            accumulated_paths.extend(dp)
            accumulated_subs.extend(ds)
            # Build provenance with child tree
            for p in root_node.get("include", []):
                provenance.setdefault(p, root_name)
            for s in root_node.get("submodules", []):
                provenance.setdefault(s, root_name)
            for child_name, child_node in (root_node.get("children") or {}).items():
                _provenance_tree(child_node, f"{root_name}/{child_name}", provenance)
        return accumulated_paths, accumulated_subs, provenance

    parts = job_path.strip("/").split("/")
    node = config["jobs"]
    current_path = ""

    for i, part in enumerate(parts):
        if part not in node:
            return None, None, None
        node = node[part]
        current_path = f"{current_path}/{part}" if current_path else part

        for p in node.get("include", []):
            accumulated_paths.append(p)
            provenance[p] = current_path
        for s in node.get("submodules", []):
            accumulated_subs.append(s)
            provenance[s] = current_path

        # Resolve dependsOn at each level
        dep_paths, dep_subs, dep_prov = resolve_depends_on(config, node)
        for p in dep_paths:
            accumulated_paths.append(p)
            if p not in provenance:
                dep_source = dep_prov.get(p, "?")
                provenance[p] = f"{current_path} → {dep_source}"
        for s in dep_subs:
            accumulated_subs.append(s)
            if s not in provenance:
                dep_source = dep_prov.get(s, "?")
                provenance[s] = f"{current_path} → {dep_source}"

        if i < len(parts) - 1:
            node = node.get("children", {})

    return accumulated_paths, accumulated_subs, provenance


def all_jobs(config):
    """Return dict of {job_path: (paths, submodules)} for all leaf and branch nodes."""
    root_paths = list(config.get("include", []))
    root_subs = []

    result = {}
    for root_name, root_node in config["jobs"].items():
        for path, paths, subs in walk_tree(config, root_node, root_name, root_paths, root_subs):
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
    result = resolve_job(config, args.job)
    if result[0] is None:
        print(f"ERROR: Job '{args.job}' not found in config", file=sys.stderr)
        return 1
    paths, subs, provenance = result

    # Get changed files if base ref provided
    changed_files = set()
    if args.base:
        diff_output = git("diff", "--name-only", args.base, "HEAD")
        if diff_output:
            changed_files = set(diff_output.splitlines())

    # Submodule SHAs grouped by source
    sub_shas = []
    sub_by_source = {}
    for sub in sorted(set(subs)):
        sha = os.environ.get(sub.replace("/", "_").replace("-", "_").upper() + "_SHA") or get_submodule_sha(sub)
        sub_shas.append(f"{sub}:{sha}")
        source = provenance.get(sub, "?")
        sub_by_source.setdefault(source, []).append(f"{sub}:{sha[:12]}")

    # Hash files grouped by source
    file_hashes = []
    groups = {}  # source -> [(display, dirty)]
    for pattern in sorted(set(paths)):
        dir_path = re.sub(r"/?\*\*$", "", pattern)
        source = provenance.get(pattern, "?")
        p = Path(dir_path)
        if p.is_dir():
            files = sorted(f for f in p.rglob("*") if f.is_file()
                           and not any(x in f.parts for x in ("bin", "obj", "libs", "tools", ".git")))
            for f in files:
                file_hashes.append(hashlib.sha256(f.read_bytes()).hexdigest())
            dirty = any(c.startswith(dir_path + "/") for c in changed_files)
            groups.setdefault(source, []).append((f"{dir_path}/ ({len(files)} files)", dirty))
        elif p.is_file():
            file_hashes.append(hashlib.sha256(p.read_bytes()).hexdigest())
            dirty = dir_path in changed_files
            groups.setdefault(source, []).append((dir_path, dirty))

    composite = hashlib.sha256("|".join(file_hashes).encode()).hexdigest()[:24]
    sub_parts = "_".join(s.replace(":", "-").replace("/", "-") for s in sub_shas)
    job_part = args.job.replace("/", "-")
    cache_key = f"{job_part}_{args.name}_{sub_parts}_{composite}"

    print(f"\n=== Cache Key ===")
    print(f"Name:  {args.name}")
    print(f"Job:   {args.job}")
    print(f"Files: {composite} ({len(file_hashes)} files)")
    print(f"Key:   {cache_key}")

    print(f"\nSources:")

    def is_dirty(source):
        items = groups.get(source, [])
        slines = sub_by_source.get(source, [])
        return (any(d for _, d in items) or
                any(any(c.startswith(s.split(":")[0]) for c in changed_files) for s in slines))

    def print_items(source, indent):
        pre = "  " * indent
        for sub in sub_by_source.get(source, []):
            sub_path = sub.split(":")[0]
            dirty = any(c.startswith(sub_path) for c in changed_files)
            flag = "* " if dirty else "  "
            print(f"{pre}{flag}sub: {sub}")
        for display, dirty in groups.get(source, []):
            flag = "* " if dirty else "  "
            print(f"{pre}{flag}{display}")

    # Parse sources into direct nodes vs dependency nodes
    direct = {}   # source_key -> source_key
    deps = {}     # (owner, dep_root) -> [source_keys]
    all_sources = set(groups.keys()) | set(sub_by_source.keys())
    for source in all_sources:
        if " → " in source:
            owner, dep_path = source.split(" → ", 1)
            dep_root = dep_path.split("/")[0]
            deps.setdefault((owner, dep_root), []).append(source)
        else:
            direct[source] = source

    # Print global
    if "global" in all_sources:
        flag = "* " if is_dirty("global") else "  "
        print(f"  {flag}global")
        print_items("global", 2)

    if args.job == "all":
        # For "all" job, render all direct sources as a tree based on / nesting
        rendered = {"global"}
        for source in sorted(direct.keys()):
            if source in rendered:
                continue
            parts = source.split("/")
            indent = len(parts)
            flag = "* " if is_dirty(source) else "  "
            print(f"{'  ' * indent}{flag}{parts[-1]}")
            print_items(source, indent + 1)
            rendered.add(source)
    else:
        # Print job path hierarchy
        job_parts = args.job.strip("/").split("/")
        current = ""
        for i, part in enumerate(job_parts):
            current = f"{current}/{part}" if current else part
            if current not in all_sources and not any(o == current for o, _ in deps):
                continue
            indent = i + 1
            flag = "* " if is_dirty(current) else "  "
            print(f"{'  ' * indent}{flag}{part}")
            print_items(current, indent + 1)

            # Print dependsOn trees nested under this node
            for (owner, dep_root), dep_list in sorted(deps.items()):
                if owner != current:
                    continue
                any_dirty = any(is_dirty(s) for s in dep_list)
                flag = "* " if any_dirty else "  "
                print(f"{'  ' * (indent + 1)}{flag}{dep_root} (dependsOn)")

                root_src = f"{owner} → {dep_root}"
                if root_src in all_sources:
                    print_items(root_src, indent + 2)

                for dep_src in sorted(dep_list):
                    if dep_src == root_src:
                        continue
                    dep_path = dep_src.split(" → ", 1)[1]
                    child = dep_path.split("/", 1)[1] if "/" in dep_path else None
                    if child:
                        flag = "* " if is_dirty(dep_src) else "  "
                        print(f"{'  ' * (indent + 2)}{flag}{child}")
                        print_items(dep_src, indent + 3)

    if os.environ.get("BUILD_BUILDID"):
        print(f"##vso[task.setvariable variable=CACHE_KEY]{cache_key}")

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
        print(f"\n[info] {len(overlaps)} files match both include and exclude (include wins):")
        for f in overlaps[:10]:
            print(f"  {f}")
        if len(overlaps) > 10:
            print(f"  ... +{len(overlaps) - 10} more")

    if uncovered:
        print(f"\n[FAIL] UNCOVERED FILES:")
        for f in uncovered:
            print(f"{f}")
        print(f"\nAdd to a job or exclude list in repo-deps.json")
        return 1

    print("\n[PASS] All files covered!")
    return 0

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(description="Build caching tool")
    parser.add_argument("--config", default=None, help="Config file path")
    sub = parser.add_subparsers(dest="command")

    ck = sub.add_parser("cache-key", help="Compute cache key for a job")
    ck.add_argument("--job", required=True, help="Job path (e.g. managed/package)")
    ck.add_argument("--name", required=True, help="ADO job name")
    ck.add_argument("--docker", default="", help="Docker context directory")
    ck.add_argument("--base", default="", help="Base ref to diff — marks changed paths with *")

    sub.add_parser("validate", help="Check all files are covered")

    args = parser.parse_args()
    if not args.command:
        parser.print_help()
        return 1

    config_path = args.config or str(Path(__file__).parent / "repo-deps.json")
    config = load_config(config_path)

    if args.command == "cache-key":
        return cmd_cache_key(config, args)
    elif args.command == "validate":
        return cmd_validate(config, args)


if __name__ == "__main__":
    sys.exit(main())
