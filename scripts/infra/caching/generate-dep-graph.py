from __future__ import annotations
#!/usr/bin/env python3
"""Generate a build dependency graph from file scanning and link rules.

Scans the repository for files matching configured globs, discovers
connections between them using regex link rules, then computes the
transitive dependency set for each target.

Configuration (repo-deps.config.json):

  scan:       Globs for files to index
  targets:    Globs for which files are build entry points
  global:     Globs for files that are deps of every target
  submodules: Git submodule paths (tracked by commit SHA)
  links:      Rules for how files reference each other:
              - pattern:  regex with capture group -> resolved as file path
              - siblings: true -> all files in same directory tree are linked
  exclude:    Globs for paths to skip

Usage:
    python3 scripts/generate-native-dep-graph.py [--root .] [--config ...]

Output:
    scripts/repo-deps.json   (machine-readable)
    scripts/native-build-deps.md     (human-readable)
"""

import argparse
import fnmatch
import json
import re
import sys
from collections import defaultdict
from pathlib import Path


# --- File discovery -----------------------------------------------------------

def match_any(rel_path, patterns):
    for p in patterns:
        if fnmatch.fnmatch(rel_path, p) or fnmatch.fnmatch(rel_path.replace("\\", "/"), p):
            return True
    return False


def discover_files(root, scan_globs, exclude_globs):
    files = set()
    for pattern in scan_globs:
        for match in root.glob(pattern):
            if match.is_file():
                rel = str(match.relative_to(root))
                if not match_any(rel, exclude_globs):
                    files.add(rel)
    return sorted(files)


def discover_targets(root, target_globs):
    targets = {}
    for pattern in target_globs:
        for match in sorted(root.glob(pattern)):
            if match.is_file():
                rel = str(match.relative_to(root))
                name = match.parent.name
                targets[name] = {
                    "directory": str(match.parent.relative_to(root)),
                    "entry_point": rel,
                }
    return targets


# --- Link resolution ----------------------------------------------------------

def resolve_relative(source_file, captured, root):
    captured = captured.strip().strip('"').strip("'")
    resolved = (source_file.parent / captured).resolve()
    try:
        return str(resolved.relative_to(root.resolve()))
    except ValueError:
        return None


def build_link_graph(root, all_files, link_rules):
    graph = defaultdict(set)
    files_set = set(all_files)

    for rule in link_rules:
        file_glob = rule.get("files", "**/*")

        if rule.get("siblings"):
            matching = [f for f in all_files if fnmatch.fnmatch(f, file_glob)]
            for anchor in matching:
                anchor_dir = str(Path(anchor).parent)
                parent_dir = str(Path(anchor_dir).parent)
                for f in all_files:
                    if f == anchor:
                        continue
                    f_dir = str(Path(f).parent)
                    if f_dir == anchor_dir or f_dir.startswith(anchor_dir + "/"):
                        graph[anchor].add(f)
                    elif f_dir == parent_dir and Path(f).suffix != "":
                        graph[anchor].add(f)

        elif "pattern" in rule:
            regex = rule["pattern"]
            resolve_mode = rule.get("resolve", "relative")
            prefix = rule.get("prefix", "")
            suffix = rule.get("suffix", "")
            matching = [f for f in all_files if fnmatch.fnmatch(f, file_glob)]
            for f in matching:
                try:
                    content = (root / f).read_text(encoding="utf-8", errors="replace")
                except OSError:
                    continue
                for m in re.finditer(regex, content, re.MULTILINE):
                    captured = m.group(1).strip()

                    if resolve_mode == "relative":
                        resolved = resolve_relative(root / f, captured, root)
                    elif resolve_mode == "prefix":
                        resolved = prefix + captured + suffix
                    elif resolve_mode == "root":
                        resolved = captured
                    else:
                        resolved = captured

                    if resolved and (resolved in files_set or (root / resolved).exists()):
                        graph[f].add(resolved)
                        # If resolved is a directory, also link to its Dockerfile
                        # (Docker context directories should link to all contents)
                        resolved_path = root / resolved
                        if resolved_path.is_dir():
                            dockerfile = resolved_path / "Dockerfile"
                            if dockerfile.exists():
                                df_rel = str(dockerfile.relative_to(root))
                                if df_rel in files_set:
                                    graph[f].add(df_rel)

    return graph


def transitive_deps(start, graph, seen=None):
    """Walk forward: start -> things it loads."""
    if seen is None:
        seen = set()
    if start in seen:
        return seen
    seen.add(start)
    for dep in graph.get(start, set()):
        transitive_deps(dep, graph, seen)
    return seen


def reverse_transitive(start, reverse_graph, seen=None):
    """Walk backward: start -> things that reference it."""
    if seen is None:
        seen = set()
    if start in seen:
        return seen
    seen.add(start)
    for dep in reverse_graph.get(start, set()):
        reverse_transitive(dep, reverse_graph, seen)
    return seen


def build_reverse_graph(graph):
    """Invert edge direction: {A -> B} becomes {B -> A}."""
    rev = defaultdict(set)
    for src, dsts in graph.items():
        for dst in dsts:
            rev[dst].add(src)
    return rev


# --- Target dependency computation --------------------------------------------

def compute_target_deps(target_info, all_files, link_graph, reverse_graph, global_files, submodules, exclude_globs):
    deps = set()

    entry = target_info["entry_point"]

    # 1. Forward: entry_point -> things it loads (cake #load chain)
    forward_deps = transitive_deps(entry, link_graph)
    deps.update(forward_deps)

    # 2. Backward: things that reference the entry_point (YAML templates that invoke this target)
    #    Only add the referencing files themselves — NOT their forward deps
    #    (otherwise stages-native.yml pulls in all other platforms' deps)
    reverse_deps = reverse_transitive(entry, reverse_graph)
    deps.update(reverse_deps)

    # 3. Global deps + their transitive forward deps (e.g., bootstrapper → install scripts)
    for gf in global_files:
        deps.update(transitive_deps(gf, link_graph))

    # 4. All files in the target directory
    target_dir = target_info["directory"]
    for f in all_files:
        if f.startswith(target_dir + "/") and not match_any(f, exclude_globs):
            deps.add(f)

    deps.update(global_files)
    deps.update(submodules)

    # 5. Expand forward ONLY from files found via the forward walk and globals
    expandable = set(forward_deps)
    for gf in global_files:
        expandable.update(transitive_deps(gf, link_graph))
    for f in list(deps):
        if f.startswith(target_dir + "/"):
            expandable.add(f)

    prev_size = 0
    while len(expandable) != prev_size:
        prev_size = len(expandable)
        expansion = set()
        for d in list(expandable):
            expansion.update(transitive_deps(d, link_graph))
        expandable.update(expansion)
    deps.update(expandable)

    # 6. Separate into directories (target dir, cake dir) and individual files
    #    This way new files in tracked dirs are caught, but we don't hash
    #    unrelated files in broad parent dirs like scripts/
    tracked_dirs = set()
    tracked_files = set()

    # The target directory is always a tracked dir
    tracked_dirs.add(target_dir)

    for f in deps:
        if f.startswith("externals/"):
            continue  # Submodule sentinel, not a real file
        parent = str(Path(f).parent)
        # If the file is in the target dir, it's covered by the dir
        if f.startswith(target_dir + "/"):
            continue
        # If the file is in a "leaf" dir (no other tracked files in sibling dirs),
        # track the dir. Otherwise track the file.
        # Heuristic: directories under native/ and scripts/cake/ and scripts/Docker/
        # are cohesive units. Individual files under scripts/ are standalone.
        if parent.startswith("native/") or parent.startswith("scripts/cake") or parent.startswith("scripts/Docker"):
            tracked_dirs.add(parent)
        else:
            tracked_files.add(f)

    return sorted(deps), sorted(tracked_dirs), sorted(tracked_files)


# --- Output -------------------------------------------------------------------

def generate_registry(targets, all_files, link_graph, global_files, submodules, exclude_globs):
    reverse_graph = build_reverse_graph(link_graph)
    output = {
        "_metadata": {
            "description": "Build dependency graph for cache keys",
            "generator": "scripts/generate-native-dep-graph.py",
        },
        "targets": {},
        "load_graph": {k: sorted(v) for k, v in sorted(link_graph.items()) if v},
    }
    for name, info in sorted(targets.items()):
        files, dirs, individual_files = compute_target_deps(info, all_files, link_graph, reverse_graph, global_files, submodules, exclude_globs)
        output["targets"][name] = {
            "files": files,
            "dirs": dirs,
            "individual_files": individual_files,
            "entry_point": info["entry_point"],
        }
    return output


def _mid(path):
    return re.sub(r"[^a-zA-Z0-9]", "_", path)


def _short(path):
    return Path(path).name


def write_mermaid(config, registry, output_path):
    """Write a Mermaid diagram showing stages and native target breakdown."""
    lines = []
    stages = config.get("stages", {})
    targets = registry.get("targets", {})

    lines.append("# Build Dependency Graph")
    lines.append("")
    lines.append("> Auto-generated by `python3 scripts/infra/caching/generate-dep-graph.py`")
    lines.append("> Explore: paste the mermaid block into [mermaid.live](https://mermaid.live)")
    lines.append("")

    # --- Stage-level diagram ---
    lines.append("## Pipeline Stages")
    lines.append("")
    lines.append("```mermaid")
    lines.append("flowchart TB")
    lines.append("")

    # Stage nodes with their key paths
    for name, stage in stages.items():
        paths = stage.get("paths", [])
        # Summarize paths
        summary = []
        for p in paths[:3]:
            summary.append(p.replace("/**", "/").rstrip("/"))
        if len(paths) > 3:
            summary.append("+%d more" % (len(paths) - 3))
        label = name + "\\n" + ", ".join(summary)
        sid = _mid("stage_" + name)
        lines.append('  %s["%s"]' % (sid, label))

    lines.append("")

    # Stage dependency edges
    for name, stage in stages.items():
        sid = _mid("stage_" + name)
        for dep in (stage.get("depends_on") or []):
            lines.append("  %s --> %s" % (_mid("stage_" + dep), sid))

    lines.append("")
    lines.append("  style %s fill:#f9f,stroke:#333" % _mid("stage_native"))
    lines.append("```")
    lines.append("")

    # --- Native target breakdown ---
    if targets:
        lines.append("## Native Targets")
        lines.append("")
        lines.append("```mermaid")
        lines.append("flowchart TB")
        lines.append("")

        # Shared deps
        lines.append('  shared["scripts/infra/native/shared/\\n(affects all)"]')
        lines.append('  skia["externals/skia\\n(C++ source)"]')
        lines.append("")

        # Group targets by family
        families = defaultdict(list)
        for name in sorted(targets):
            if name in ("ios", "macos", "maccatalyst", "tvos"):
                families["Apple"].append(name)
            elif name in ("windows", "winui", "winui-angle", "nanoserver"):
                families["Windows"].append(name)
            elif name.startswith("linux"):
                families["Linux"].append(name)
            elif name in ("android", "tizen"):
                families["Mobile"].append(name)
            elif name == "wasm":
                families["Web"].append(name)
            else:
                families["Other"].append(name)

        for fam, members in families.items():
            fid = _mid("fam_" + fam)
            lines.append('  subgraph %s["%s"]' % (fid, fam))
            for name in members:
                tid = _mid("t_" + name)
                # Show unique infra deps
                infra = [f for f in targets[name].get("files", [])
                         if f.startswith("scripts/infra/native/") and "/shared/" not in f]
                infra_short = ", ".join(set(_short(f) for f in infra if not f.endswith("/")))
                label = name
                if infra_short:
                    label += "\\n" + infra_short
                lines.append('    %s["%s"]' % (tid, label))
            lines.append("  end")
            lines.append("")

        # Edges
        lines.append("  skia --> shared")
        for fam in families:
            fid = _mid("fam_" + fam)
            lines.append("  shared --> %s" % fid)

        lines.append("")
        lines.append("  style shared fill:#e8f4e8,stroke:#4a4")
        lines.append("  style skia fill:#f9f,stroke:#333")
        lines.append("```")

    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text("\n".join(lines) + "\n", encoding="utf-8")


# --- Config -------------------------------------------------------------------

def load_config(args_config, output_path):
    defaults = {
        "scan": ["**/*"],
        "targets": [],
        "global": [],
        "submodules": [],
        "links": [],
        "exclude": ["**/bin/**", "**/obj/**", "**/.git/**"],
    }

    config_path = None
    if args_config:
        config_path = Path(args_config)
    else:
        for candidate in [
            output_path.with_name("repo-deps.config.json"),
            Path(__file__).resolve().parent / "repo-deps.config.json",
        ]:
            if candidate.exists():
                config_path = candidate
                break

    if config_path and config_path.exists():
        raw = json.loads(config_path.read_text(encoding="utf-8"))
        clean = {k: v for k, v in raw.items() if not k.startswith("_")}
        for rule in clean.get("links", []):
            for k in list(rule):
                if k.startswith("_"):
                    del rule[k]
        defaults.update(clean)
        print("Config: %s" % config_path)
    else:
        print("Config: built-in defaults")

    return defaults


# --- Main ---------------------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(description="Generate build dependency graph")
    parser.add_argument("--root", default=".", help="Repository root")
    parser.add_argument("--output", default=None, help="Output JSON path")
    parser.add_argument("--config", default=None, help="Config JSON path")
    args = parser.parse_args()

    root = Path(args.root).resolve()
    output_path = Path(args.output) if args.output else root / "scripts" / "infra" / "caching" / "repo-deps.json"
    config = load_config(args.config, output_path)

    print("Root: %s\n" % root)

    print("1. Scanning files...")
    all_files = discover_files(root, config["scan"], config["exclude"])
    print("   %d files indexed" % len(all_files))

    print("2. Discovering targets...")
    # Get native targets from stages config or top-level
    target_globs = config.get("targets", [])
    if not target_globs and "stages" in config:
        native_stage = config["stages"].get("native", {})
        target_globs = native_stage.get("targets", [])
    targets = discover_targets(root, target_globs)
    print("   %d targets: %s" % (len(targets), ", ".join(sorted(targets))))

    print("3. Resolving links...")
    link_graph = build_link_graph(root, all_files, config["links"])
    total = sum(len(v) for v in link_graph.values())
    print("   %d links across %d files" % (total, len(link_graph)))

    global_files = []
    for p in config["global"]:
        for m in root.glob(p):
            if m.is_file():
                global_files.append(str(m.relative_to(root)))
    submodules = [s for s in config["submodules"] if (root / s).exists()]

    print("4. Computing dependencies...")
    registry = generate_registry(targets, all_files, link_graph, global_files, submodules, config["exclude"])

    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps(registry, indent=2, sort_keys=False) + "\n", encoding="utf-8")
    print("\n   -> %s" % output_path)

    md_path = output_path.with_suffix(".md")
    write_mermaid(config, registry, md_path)
    print("   -> %s" % md_path)

    print("\n=== Summary ===")
    for name, info in sorted(registry["targets"].items()):
        print("  %-25s %3d files" % (name, len(info["files"])))

    return 0


if __name__ == "__main__":
    sys.exit(main())
