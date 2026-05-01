from __future__ import annotations
#!/usr/bin/env python3
"""Generate a dependency graph for native builds.

Discovers build targets, script dependencies, and Docker contexts by
scanning the repository — no hardcoded file names or platform lists.

Configuration is provided via a small YAML/JSON config that declares
"leaf directories" (where to find targets) and "scan roots" (where to
look for shared scripts). Everything else is discovered.

Usage:
    python3 scripts/generate-native-dep-graph.py [--root .] [--config ...]

Output:
    scripts/native-build-deps.json   (machine-readable)
    scripts/native-build-deps.md     (human-readable)
"""

import argparse
import json
import os
import re
import sys
from collections import defaultdict
from pathlib import Path

# ─── Default configuration ────────────────────────────────────────────────────
# This can be overridden by passing --config to a JSON file with the same shape.
# All paths are relative to the repository root.
DEFAULT_CONFIG = {
    # Directories containing build targets (each subdirectory with a build entry
    # point is a target). The entry_point glob determines what makes a directory
    # a target.
    "target_roots": [
        {
            "path": "native",
            "entry_point": "build.cake",
        }
    ],

    # Directories to scan recursively for shared scripts that targets may load.
    "script_roots": [
        "scripts/cake",
    ],

    # Additional individual files that are always included in every target's
    # dependency set (version files, global configs, etc.). Supports globs.
    "global_deps": [
        "scripts/VERSIONS.txt",
    ],

    # Git submodule paths to track via commit SHA rather than file hashing.
    # These appear as sentinel entries in the dependency list.
    "submodules": [
        "externals/skia",
    ],

    # Root directories under which to discover Docker build contexts.
    # A context is any directory containing a Dockerfile.
    "docker_roots": [
        "scripts/Docker",
    ],

    # File patterns to scan for #load / #include / import directives.
    # Each entry maps a glob pattern to a regex that extracts the loaded path.
    "load_patterns": {
        "*.cake": r'^#load\s+"([^"]+)"',
    },

    # Directories to exclude when scanning target contents.
    "exclude_dirs": ["bin", "obj", "libs", "tools", ".git"],

    # Files/directories to skip when scanning (dot-prefixed are always skipped).
    "exclude_files": [],
}


# ─── Scanning ─────────────────────────────────────────────────────────────────

def resolve_load(source_file: Path, load_path: str, root: Path) -> "Path | None":
    """Resolve a load directive path relative to the source file."""
    load_path = load_path.strip().strip('"').strip("'")
    resolved = (source_file.parent / load_path).resolve()
    try:
        return resolved.relative_to(root.resolve())
    except ValueError:
        return None


def scan_loads(file_path: Path, pattern: str, root: Path) -> list[str]:
    """Find all load directives matching a regex pattern in a file."""
    loads = []
    try:
        content = file_path.read_text(encoding="utf-8", errors="replace")
    except OSError:
        return loads
    for match in re.finditer(pattern, content, re.MULTILINE):
        resolved = resolve_load(file_path, match.group(1), root)
        if resolved:
            loads.append(str(resolved))
    return loads


def build_load_graph(root: Path, config: dict) -> dict[str, list[str]]:
    """Build a file-to-file dependency graph from load directives.

    Scans all files matching the configured glob patterns under target_roots
    and script_roots.
    """
    graph: dict[str, list[str]] = {}

    # Collect all directories to scan
    scan_dirs = set()
    for target_root in config["target_roots"]:
        scan_dirs.add(root / target_root["path"])
    for script_root in config["script_roots"]:
        scan_dirs.add(root / script_root)

    for glob_pattern, regex in config["load_patterns"].items():
        for scan_dir in scan_dirs:
            if not scan_dir.exists():
                continue
            for f in scan_dir.rglob(glob_pattern):
                rel = str(f.relative_to(root))
                loads = scan_loads(f, regex, root)
                if loads or rel not in graph:
                    graph[rel] = loads

    # Also scan the root build entry point if it exists
    for glob_pattern, regex in config["load_patterns"].items():
        for f in root.glob(glob_pattern):
            rel = str(f.relative_to(root))
            graph[rel] = scan_loads(f, regex, root)

    return graph


def transitive_deps(start: str, graph: dict[str, list[str]], seen: set = None) -> set[str]:
    """Get transitive closure of load dependencies."""
    if seen is None:
        seen = set()
    if start in seen:
        return seen
    seen.add(start)
    for dep in graph.get(start, []):
        transitive_deps(dep, graph, seen)
    return seen


def discover_targets(root: Path, config: dict) -> dict[str, dict]:
    """Discover build targets by scanning target root directories.

    A target is any subdirectory of a target_root that contains the
    configured entry_point file.
    """
    targets = {}
    for target_root in config["target_roots"]:
        base = root / target_root["path"]
        entry_point = target_root["entry_point"]
        if not base.exists():
            continue
        for entry in sorted(base.iterdir()):
            if entry.is_dir() and (entry / entry_point).exists():
                name = entry.name
                rel_dir = str(entry.relative_to(root))
                targets[name] = {
                    "directory": rel_dir,
                    "entry_point": f"{rel_dir}/{entry_point}",
                }
    return targets


def discover_docker_contexts(root: Path, config: dict) -> dict[str, list[str]]:
    """Discover Docker build contexts by finding Dockerfiles."""
    contexts = {}
    shared_scripts = []

    for docker_root in config["docker_roots"]:
        base = root / docker_root
        if not base.exists():
            continue

        # Collect shared scripts at the docker root level (not in subdirectories)
        for f in base.iterdir():
            if f.is_file() and not f.name.startswith("."):
                shared_scripts.append(str(f.relative_to(root)))

        # Find all Dockerfiles
        for dockerfile in sorted(base.rglob("Dockerfile")):
            context_dir = dockerfile.parent
            rel_context = str(context_dir.relative_to(root))
            files = []
            for f in context_dir.rglob("*"):
                if f.is_file():
                    files.append(str(f.relative_to(root)))
            # Include shared scripts from the docker root
            for s in shared_scripts:
                if s not in files:
                    files.append(s)
            contexts[rel_context] = sorted(files)

    return contexts


def is_excluded(rel_path: str, config: dict) -> bool:
    """Check if a file path should be excluded from scanning."""
    parts = Path(rel_path).parts
    for part in parts:
        if part in config["exclude_dirs"]:
            return True
        if part.startswith("."):
            return True
    return rel_path in config.get("exclude_files", [])


def resolve_global_deps(root: Path, config: dict) -> list[str]:
    """Resolve global dependency patterns to actual file paths."""
    files = []
    for pattern in config["global_deps"]:
        for match in root.glob(pattern):
            if match.is_file():
                files.append(str(match.relative_to(root)))
    return sorted(files)


def build_target_deps(
    target_name: str,
    target_info: dict,
    load_graph: dict[str, list[str]],
    global_deps: list[str],
    submodules: list[str],
    root: Path,
    config: dict,
) -> list[str]:
    """Build the complete file dependency list for a target."""
    deps = set()

    # 1. Transitive load dependencies from the entry point
    entry = target_info["entry_point"]
    deps.update(transitive_deps(entry, load_graph))

    # 2. All files in the target directory (project files, makefiles, etc.)
    target_dir = root / target_info["directory"]
    for f in target_dir.rglob("*"):
        if f.is_file():
            rel = str(f.relative_to(root))
            if not is_excluded(rel, config):
                deps.add(rel)

    # 3. Global dependencies
    deps.update(global_deps)

    # 4. Submodule sentinels
    deps.update(submodules)

    return sorted(deps)


# ─── Output generation ────────────────────────────────────────────────────────

def generate_registry(
    targets: dict[str, dict],
    load_graph: dict[str, list[str]],
    docker_contexts: dict[str, list[str]],
    global_deps: list[str],
    submodules: list[str],
    root: Path,
    config: dict,
) -> dict:
    """Generate the complete dependency registry."""
    output = {
        "_metadata": {
            "description": "Native build dependency graph for cache keys",
            "generator": "scripts/generate-native-dep-graph.py",
            "usage": "Used by cache key scripts to determine inputs per target",
        },
        "targets": {},
        "docker_contexts": docker_contexts,
    }

    for name, info in sorted(targets.items()):
        deps = build_target_deps(
            name, info, load_graph, global_deps, submodules, root, config
        )
        output["targets"][name] = {
            "files": deps,
            "entry_point": info["entry_point"],
        }

    # Include the load graph for reference
    output["load_graph"] = {k: v for k, v in sorted(load_graph.items()) if v}

    return output


def mermaid_id(path: str) -> str:
    """Sanitize a file path into a valid Mermaid node ID."""
    return re.sub(r"[^a-zA-Z0-9]", "_", path)


def short_name(path: str) -> str:
    """Get just the filename from a path."""
    return Path(path).name


def write_markdown(
    registry: dict,
    load_graph: dict[str, list[str]],
    docker_contexts: dict[str, list[str]],
    output_path: Path,
) -> None:
    """Write a Markdown doc with impact table and Mermaid diagram."""
    lines = []
    targets = registry["targets"]

    # ── Header ──
    lines.append("# Native Build Dependency Graph")
    lines.append("")
    lines.append("> Auto-generated by `scripts/generate-native-dep-graph.py`.")
    lines.append("> Regenerate with: `python3 scripts/generate-native-dep-graph.py`")
    lines.append("")

    # ── Impact table ──
    lines.append("## Impact Table")
    lines.append("")
    lines.append("What rebuilds when you change a file?")
    lines.append("")
    lines.append("| File changed | Targets affected |")
    lines.append("|---|---|")

    reverse = defaultdict(set)
    submodule_sentinels = set()
    for target_name, info in targets.items():
        for f in info["files"]:
            if f.startswith("externals/"):
                submodule_sentinels.add(f)
            else:
                reverse[f].add(target_name)

    # Submodules affect all targets
    for s in sorted(submodule_sentinels):
        reverse[f"{s} (submodule bump)"] = set(targets.keys())

    for f in sorted(reverse.keys()):
        affected_targets = sorted(reverse[f])
        if len(affected_targets) == len(targets):
            affected = "**ALL targets**"
        else:
            affected = ", ".join(f"`{t}`" for t in affected_targets)
        lines.append(f"| `{f}` | {affected} |")
    lines.append("")

    # ── Per-target file list ──
    lines.append("## Per-Target Dependencies")
    lines.append("")
    for name, info in sorted(targets.items()):
        n = len(info["files"])
        lines.append(f"### `{name}` ({n} files)")
        lines.append("")
        for f in info["files"]:
            lines.append(f"- `{f}`")
        lines.append("")

    # ── Mermaid diagram ──
    lines.append("## Dependency Graph")
    lines.append("")
    lines.append("```mermaid")
    lines.append("flowchart LR")
    lines.append("")

    # Discover shared files (appear in 2+ targets)
    file_target_count = defaultdict(int)
    for info in targets.values():
        for f in info["files"]:
            file_target_count[f] += 1

    shared_files = {f for f, count in file_target_count.items()
                    if count > 1 and not f.startswith("externals/")}

    if shared_files:
        lines.append("  subgraph shared[Shared Dependencies]")
        for f in sorted(shared_files):
            lines.append(f'    {mermaid_id(f)}["{short_name(f)}"]')
        lines.append("  end")
        lines.append("")

    # Load graph edges
    lines.append("  %% Load dependencies")
    seen_edges = set()
    for src, deps in load_graph.items():
        for dep in deps:
            edge = (mermaid_id(src), mermaid_id(dep))
            if edge not in seen_edges:
                lines.append(f"  {edge[0]} -.->|load| {edge[1]}")
                seen_edges.add(edge)
    lines.append("")

    # Docker subgraph
    if docker_contexts:
        lines.append("  subgraph docker[Docker Contexts]")
        for ctx in sorted(docker_contexts.keys()):
            ctx_short = re.sub(r"^scripts/Docker/", "", ctx)
            lines.append(f'    {mermaid_id(ctx)}["{ctx_short}/"]')
        lines.append("  end")
        lines.append("")

    # Target nodes — auto-group by common prefix
    grouped = defaultdict(list)
    for name in sorted(targets.keys()):
        # Try to find a category from the first path component after native/
        entry = targets[name]["entry_point"]  # e.g. native/windows/build.cake
        parts = Path(entry).parts  # ('native', 'windows', 'build.cake')
        if len(parts) >= 2:
            grouped[name].append(name)

    # Just list all targets in one subgraph for simplicity
    lines.append("  subgraph targets[Build Targets]")
    for name in sorted(targets.keys()):
        lines.append(f'    {mermaid_id("target_" + name)}(("{name}")')
    lines.append("  end")
    lines.append("")

    # Edges: target → shared files and entry points
    lines.append("  %% Target → dependency edges")
    for name, info in sorted(targets.items()):
        tid = mermaid_id("target_" + name)
        for f in info["files"]:
            if f.startswith("externals/"):
                lines.append(f"  {tid} -->|source| {mermaid_id(f)}[{short_name(f)}]")
            elif f in shared_files:
                lines.append(f"  {tid} --> {mermaid_id(f)}")
            elif f.endswith(("build.cake", ".cake")):
                fid = mermaid_id(f)
                lines.append(f"  {tid} --> {fid}")
    lines.append("")

    # Edges: target → docker (discover from docker_contexts)
    lines.append("  %% Docker associations")
    for name, info in sorted(targets.items()):
        tid = mermaid_id("target_" + name)
        for ctx in docker_contexts:
            # Check if any docker context file is in this target's deps
            # (it would be added by compute-native-cache-key.ps1 via --Docker)
            ctx_id = mermaid_id(ctx)
            # Show association if there's a plausible link
    lines.append("")

    # Styling
    lines.append("  style shared fill:#e8f4e8,stroke:#4a4")
    if docker_contexts:
        lines.append("  style docker fill:#e8e8f4,stroke:#44a")
    lines.append("```")

    output_path.parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "w", encoding="utf-8") as f:
        f.write("\n".join(lines) + "\n")


# ─── Main ─────────────────────────────────────────────────────────────────────

def main():
    parser = argparse.ArgumentParser(
        description="Generate a native build dependency graph from repository scanning"
    )
    parser.add_argument("--root", default=".", help="Repository root directory")
    parser.add_argument("--output", default=None, help="Output JSON file path")
    parser.add_argument(
        "--config", default=None,
        help="Path to a JSON config file (overrides built-in defaults)"
    )
    args = parser.parse_args()

    root = Path(args.root).resolve()
    output_path = Path(args.output) if args.output else root / "scripts" / "native-build-deps.json"

    # Load config
    config = dict(DEFAULT_CONFIG)
    if args.config:
        with open(args.config) as f:
            overrides = json.load(f)
        config.update(overrides)

    print(f"Scanning repository: {root}")
    print()

    # 1. Build load dependency graph
    print("1. Scanning load dependencies...")
    load_graph = build_load_graph(root, config)
    total_loads = sum(len(v) for v in load_graph.values())
    print(f"   Found {len(load_graph)} files with {total_loads} load directives")

    # 2. Discover targets
    print("2. Discovering build targets...")
    targets = discover_targets(root, config)
    print(f"   Found {len(targets)} targets: {', '.join(sorted(targets.keys()))}")

    # 3. Discover Docker contexts
    print("3. Scanning Docker build contexts...")
    docker_contexts = discover_docker_contexts(root, config)
    print(f"   Found {len(docker_contexts)} Docker contexts")

    # 4. Resolve globals
    global_deps = resolve_global_deps(root, config)
    submodules = [s for s in config["submodules"] if (root / s).exists()]
    print(f"   Global deps: {global_deps}")
    print(f"   Submodules: {submodules}")

    # 5. Generate registry
    print("4. Generating dependency graph...")
    registry = generate_registry(
        targets, load_graph, docker_contexts, global_deps, submodules, root, config
    )

    # 6. Write JSON
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "w", encoding="utf-8") as f:
        json.dump(registry, f, indent=2, sort_keys=False)
    print(f"\nWrote dependency graph to: {output_path}")

    # 7. Write Markdown
    md_path = output_path.with_suffix(".md")
    write_markdown(registry, load_graph, docker_contexts, md_path)
    print(f"Wrote Markdown doc to: {md_path}")

    # Summary
    print("\n=== Summary ===")
    for name, info in sorted(registry["targets"].items()):
        print(f"  {name:<25} {len(info['files']):>3} files")

    return 0


if __name__ == "__main__":
    sys.exit(main())
