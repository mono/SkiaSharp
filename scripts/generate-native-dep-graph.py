from __future__ import annotations
#!/usr/bin/env python3
"""Generate a build dependency graph from file scanning and link rules.

Scans the repository for files matching configured globs, discovers
connections between them using regex link rules, then computes the
transitive dependency set for each target.

Configuration (native-build-deps.config.json):

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
    scripts/native-build-deps.json   (machine-readable)
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
            matching = [f for f in all_files if fnmatch.fnmatch(f, file_glob)]
            for f in matching:
                try:
                    content = (root / f).read_text(encoding="utf-8", errors="replace")
                except OSError:
                    continue
                for m in re.finditer(regex, content, re.MULTILINE):
                    resolved = resolve_relative(root / f, m.group(1), root)
                    if resolved and (resolved in all_files or (root / resolved).exists()):
                        graph[f].add(resolved)

    return graph


def transitive_deps(start, graph, seen=None):
    if seen is None:
        seen = set()
    if start in seen:
        return seen
    seen.add(start)
    for dep in graph.get(start, set()):
        transitive_deps(dep, graph, seen)
    return seen


# --- Target dependency computation --------------------------------------------

def compute_target_deps(target_info, all_files, link_graph, global_files, submodules, exclude_globs):
    deps = set()
    deps.update(transitive_deps(target_info["entry_point"], link_graph))

    target_dir = target_info["directory"]
    for f in all_files:
        if f.startswith(target_dir + "/") and not match_any(f, exclude_globs):
            deps.add(f)

    deps.update(global_files)
    deps.update(submodules)
    return sorted(deps)


# --- Output -------------------------------------------------------------------

def generate_registry(targets, all_files, link_graph, global_files, submodules, exclude_globs):
    output = {
        "_metadata": {
            "description": "Build dependency graph for cache keys",
            "generator": "scripts/generate-native-dep-graph.py",
        },
        "targets": {},
        "load_graph": {k: sorted(v) for k, v in sorted(link_graph.items()) if v},
    }
    for name, info in sorted(targets.items()):
        output["targets"][name] = {
            "files": compute_target_deps(info, all_files, link_graph, global_files, submodules, exclude_globs),
            "entry_point": info["entry_point"],
        }
    return output


def _mid(path):
    return re.sub(r"[^a-zA-Z0-9]", "_", path)


def _short(path):
    return Path(path).name


def write_markdown(registry, output_path):
    lines = []
    targets = registry["targets"]
    load_graph = registry.get("load_graph", {})

    lines.append("# Build Dependency Graph")
    lines.append("")
    lines.append("> Auto-generated by `scripts/generate-native-dep-graph.py`.")
    lines.append("> Regenerate: `python3 scripts/generate-native-dep-graph.py`")
    lines.append("")

    # Impact table
    lines.append("## Impact Table")
    lines.append("")
    lines.append("What rebuilds when you change a file?")
    lines.append("")
    lines.append("| File changed | Targets affected |")
    lines.append("|---|---|")

    reverse = defaultdict(set)
    sentinels = set()
    for name, info in targets.items():
        for f in info["files"]:
            if f.startswith("externals/"):
                sentinels.add(f)
            else:
                reverse[f].add(name)
    for s in sorted(sentinels):
        reverse[s + " (submodule bump)"] = set(targets.keys())

    for f in sorted(reverse):
        affected = sorted(reverse[f])
        text = "**ALL targets**" if len(affected) == len(targets) else ", ".join("`%s`" % t for t in affected)
        lines.append("| `%s` | %s |" % (f, text))
    lines.append("")

    # Per-target
    lines.append("## Per-Target Dependencies")
    lines.append("")
    for name, info in sorted(targets.items()):
        lines.append("### `%s` (%d files)" % (name, len(info["files"])))
        lines.append("")
        for f in info["files"]:
            lines.append("- `%s`" % f)
        lines.append("")

    # Mermaid
    lines.append("## Dependency Graph")
    lines.append("")
    lines.append("```mermaid")
    lines.append("flowchart LR")
    lines.append("")

    fc = defaultdict(int)
    for info in targets.values():
        for f in info["files"]:
            fc[f] += 1
    shared = {f for f, c in fc.items() if c > 1 and not f.startswith("externals/")}

    if shared:
        lines.append("  subgraph shared[Shared Dependencies]")
        for f in sorted(shared):
            lines.append('    %s["%s"]' % (_mid(f), _short(f)))
        lines.append("  end")
        lines.append("")

    if load_graph:
        lines.append("  %% Link edges")
        seen = set()
        for src, deps in load_graph.items():
            for dep in deps:
                e = (_mid(src), _mid(dep))
                if e not in seen:
                    lines.append("  %s -.->|load| %s" % e)
                    seen.add(e)
        lines.append("")

    lines.append("  subgraph targets[Build Targets]")
    for name in sorted(targets):
        lines.append('    %s(("%s")' % (_mid("target_" + name), name))
    lines.append("  end")
    lines.append("")

    lines.append("  %% Target edges")
    for name, info in sorted(targets.items()):
        tid = _mid("target_" + name)
        for f in info["files"]:
            if f.startswith("externals/"):
                lines.append("  %s -->|source| %s[%s]" % (tid, _mid(f), _short(f)))
            elif f in shared:
                lines.append("  %s --> %s" % (tid, _mid(f)))
            elif f == info["entry_point"]:
                lines.append("  %s --> %s" % (tid, _mid(f)))
    lines.append("")
    lines.append("  style shared fill:#e8f4e8,stroke:#4a4")
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
            output_path.with_name("native-build-deps.config.json"),
            Path(__file__).resolve().parent / "native-build-deps.config.json",
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
    output_path = Path(args.output) if args.output else root / "scripts" / "native-build-deps.json"
    config = load_config(args.config, output_path)

    print("Root: %s\n" % root)

    print("1. Scanning files...")
    all_files = discover_files(root, config["scan"], config["exclude"])
    print("   %d files indexed" % len(all_files))

    print("2. Discovering targets...")
    targets = discover_targets(root, config["targets"])
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
    write_markdown(registry, md_path)
    print("   -> %s" % md_path)

    print("\n=== Summary ===")
    for name, info in sorted(registry["targets"].items()):
        print("  %-25s %3d files" % (name, len(info["files"])))

    return 0


if __name__ == "__main__":
    sys.exit(main())
