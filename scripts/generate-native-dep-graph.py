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

    # Forward: entry_point -> things it loads (cake #load chain)
    deps.update(transitive_deps(entry, link_graph))

    # Backward: things that reference the entry_point (YAML templates that invoke this target)
    deps.update(reverse_transitive(entry, reverse_graph))

    # All files in the target directory
    target_dir = target_info["directory"]
    for f in all_files:
        if f.startswith(target_dir + "/") and not match_any(f, exclude_globs):
            deps.add(f)

    deps.update(global_files)
    deps.update(submodules)
    return sorted(deps)


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
        output["targets"][name] = {
            "files": compute_target_deps(info, all_files, link_graph, reverse_graph, global_files, submodules, exclude_globs),
            "entry_point": info["entry_point"],
        }
    return output


def _mid(path):
    return re.sub(r"[^a-zA-Z0-9]", "_", path)


def _short(path):
    return Path(path).name


def write_markdown(registry, output_path):
    """Write a clean overview Mermaid diagram showing target → dependency groups."""
    targets = registry["targets"]

    # Categorize files into groups for readability
    def categorize(f):
        if f.startswith("externals/"):
            return "submodule"
        if f.startswith("scripts/Docker/"):
            return "docker"
        if f.startswith("scripts/azure-"):
            return "yaml"
        if f.startswith("scripts/cake/"):
            return "cake"
        if f.startswith("scripts/") and (f.endswith(".ps1") or f.endswith(".sh")):
            return "scripts"
        if f.startswith("scripts/"):
            return "config"
        if f.startswith("native/"):
            return "native"
        return "other"

    # Build per-target category sets with actual file lists
    target_cats = {}
    for name, info in targets.items():
        cats = defaultdict(list)
        for f in info["files"]:
            cats[categorize(f)].append(f)
        target_cats[name] = cats

    # Find which categories are shared vs unique
    cat_per_target = defaultdict(lambda: defaultdict(set))  # cat -> file -> set of targets
    for name, cats in target_cats.items():
        for cat, files in cats.items():
            for f in files:
                cat_per_target[cat][f].add(name)

    lines = []
    lines.append("# Build Dependency Graph")
    lines.append("")
    lines.append("> Auto-generated by `scripts/generate-native-dep-graph.py`.")
    lines.append("> Regenerate: `python3 scripts/generate-native-dep-graph.py`")
    lines.append("> Explore interactively: paste the mermaid block into [mermaid.live](https://mermaid.live)")
    lines.append("")
    lines.append("```mermaid")
    lines.append("flowchart TB")
    lines.append("")

    # Submodules
    lines.append("  submodule[\"🔗 externals/skia\n(C++ source)\"]")
    lines.append("")

    # Shared layer — files that appear in ALL or MOST targets
    all_count = len(targets)
    global_cake = sorted(f for f, ts in cat_per_target["cake"].items() if len(ts) > all_count * 0.7)
    global_yaml = sorted(f for f, ts in cat_per_target["yaml"].items() if len(ts) > all_count * 0.7)
    global_scripts = sorted(f for f, ts in cat_per_target["scripts"].items() if len(ts) > all_count * 0.7)
    global_config = sorted(f for f, ts in cat_per_target["config"].items() if len(ts) > all_count * 0.7)

    lines.append("  subgraph global[\"🌍 Global — affects ALL targets\"]")
    lines.append("    direction LR")
    for f in global_cake + global_config:
        lines.append('    %s["%s"]' % (_mid(f), _short(f)))
    for f in global_yaml:
        lines.append('    %s["%s"]' % (_mid(f), _short(f)))
    for f in global_scripts:
        lines.append('    %s["%s"]' % (_mid(f), _short(f)))
    lines.append("  end")
    lines.append("")

    # Per-target unique deps — group by platform family
    families = {
        "🍎 Apple":    ["ios", "macos", "maccatalyst", "tvos"],
        "🪟 Windows":  ["windows", "winui", "winui-angle", "nanoserver"],
        "🐧 Linux":    ["linux", "linux-clang-cross", "linuxnodeps"],
        "📱 Mobile":   ["android", "tizen"],
        "🌐 Web":      ["wasm"],
    }

    global_files = set(global_cake + global_yaml + global_scripts + global_config)

    for family_name, family_targets in families.items():
        present = [t for t in family_targets if t in targets]
        if not present:
            continue

        fam_id = _mid(family_name)
        lines.append('  subgraph %s["%s"]' % (fam_id, family_name))

        for name in present:
            tid = _mid("t_" + name)
            # Collect unique (non-global) deps for this target
            unique = []
            for f in targets[name]["files"]:
                if f not in global_files and not f.startswith("externals/"):
                    unique.append(f)

            # Group unique deps by type
            unique_cake = [f for f in unique if categorize(f) == "cake"]
            unique_yaml = [f for f in unique if categorize(f) == "yaml"]
            unique_docker = [f for f in unique if categorize(f) == "docker"]
            unique_native = [f for f in unique if categorize(f) == "native"]
            unique_scripts = [f for f in unique if categorize(f) == "scripts"]

            # Build a compact label
            parts = [name]
            if unique_cake:
                parts.append("cake: " + ", ".join(_short(f) for f in unique_cake))
            if unique_yaml:
                parts.append("yaml: " + ", ".join(_short(f) for f in unique_yaml[:3]))
                if len(unique_yaml) > 3:
                    parts[-1] += " +%d" % (len(unique_yaml) - 3)
            if unique_docker:
                docker_names = set(str(Path(f).parent).replace("scripts/Docker/", "") for f in unique_docker)
                parts.append("docker: " + ", ".join(sorted(docker_names)))
            if unique_scripts:
                parts.append("scripts: " + ", ".join(_short(f) for f in unique_scripts[:3]))
                if len(unique_scripts) > 3:
                    parts[-1] += " +%d" % (len(unique_scripts) - 3)

            label = "\\n".join(parts)
            lines.append('    %s["%s"]' % (tid, label))

        lines.append("  end")
        lines.append("")

    # Edges: submodule → global → families
    lines.append("  submodule --> global")
    for family_name in families:
        fam_id = _mid(family_name)
        lines.append("  global --> %s" % fam_id)
    lines.append("")

    # Styling
    lines.append("  style submodule fill:#f9f,stroke:#333")
    lines.append("  style global fill:#e8f4e8,stroke:#4a4")
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
