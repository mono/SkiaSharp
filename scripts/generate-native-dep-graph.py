from __future__ import annotations
#!/usr/bin/env python3
"""Generate a dependency graph for SkiaSharp native builds.

Scans Cake scripts (#load), YAML templates, Dockerfiles, and install scripts
to produce a JSON file mapping each native build target to the complete set
of files that affect its output. The cache key system uses this graph to
hash exactly the right files for each job.

Usage:
    python3 scripts/generate-native-dep-graph.py [--root .]

Output:
    scripts/native-build-deps.json
"""

import argparse
import json
import os
import re
import sys
from collections import defaultdict
from pathlib import Path


def resolve_cake_load(source_file: Path, load_path: str, root: Path) -> "Path | None":
    """Resolve a #load directive relative to the source file."""
    # Remove quotes
    load_path = load_path.strip().strip('"').strip("'")
    # Resolve relative to the source file's directory
    resolved = (source_file.parent / load_path).resolve()
    try:
        return resolved.relative_to(root.resolve())
    except ValueError:
        return None


def scan_cake_loads(cake_file: Path, root: Path) -> list[str]:
    """Find all #load directives in a .cake file."""
    loads = []
    try:
        content = cake_file.read_text(encoding="utf-8", errors="replace")
    except OSError:
        return loads
    for match in re.finditer(r'^#load\s+"([^"]+)"', content, re.MULTILINE):
        resolved = resolve_cake_load(cake_file, match.group(1), root)
        if resolved:
            loads.append(str(resolved))
    return loads


def build_cake_graph(root: Path) -> dict[str, list[str]]:
    """Build the full Cake #load dependency graph."""
    graph: dict[str, list[str]] = {}
    cake_dirs = [root / "scripts" / "cake", root / "native"]
    for cake_dir in cake_dirs:
        for cake_file in cake_dir.rglob("*.cake"):
            rel = str(cake_file.relative_to(root))
            graph[rel] = scan_cake_loads(cake_file, root)
    # Also scan root build.cake
    root_cake = root / "build.cake"
    if root_cake.exists():
        graph["build.cake"] = scan_cake_loads(root_cake, root)
    return graph


def transitive_cake_deps(start: str, graph: dict[str, list[str]], seen: set = None) -> set[str]:
    """Get transitive closure of Cake #load dependencies."""
    if seen is None:
        seen = set()
    if start in seen:
        return seen
    seen.add(start)
    for dep in graph.get(start, []):
        transitive_cake_deps(dep, graph, seen)
    return seen


def scan_native_platforms(root: Path) -> dict[str, dict]:
    """Discover native platform build directories and their properties."""
    platforms = {}
    native_dir = root / "native"
    for entry in sorted(native_dir.iterdir()):
        if entry.is_dir() and (entry / "build.cake").exists():
            name = entry.name
            platforms[name] = {
                "directory": f"native/{name}",
                "build_cake": f"native/{name}/build.cake",
            }
    return platforms


def scan_bootstrapper_install_conditions(root: Path) -> list[dict]:
    """Parse the bootstrapper template to extract install script conditions.

    Returns a list of {script, job_pattern, condition_description} dicts.
    """
    bootstrapper = root / "scripts" / "azure-templates-jobs-bootstrapper.yml"
    if not bootstrapper.exists():
        return []

    content = bootstrapper.read_text(encoding="utf-8", errors="replace")
    installs = []

    # Pattern: script references with their surrounding conditions
    # We look for install script invocations and the conditional blocks they're in
    install_patterns = [
        {"script": "scripts/install-ninja.ps1", "pattern": "native_", "desc": "all native jobs"},
        {"script": "scripts/install-android-ndk.ps1", "pattern": "_android_", "desc": "Android native jobs"},
        {"script": "scripts/install-tizen.ps1", "pattern": "_tizen_", "desc": "Tizen native jobs"},
        {"script": "scripts/install-7zip.ps1", "pattern": "_win32_", "desc": "Win32 native jobs"},
        {"script": "scripts/install-llvm.ps1", "pattern": "_win32_", "desc": "Win32 native jobs"},
        {"script": "scripts/install-emsdk.sh", "pattern": "native_wasm_", "desc": "WASM native jobs (via installEmsdk)"},
        {"script": "scripts/select-xcode.sh", "pattern": "native_", "desc": "native macOS jobs"},
        {"script": "scripts/select-vs.ps1", "pattern": "native_win", "desc": "native Windows jobs"},
        {"script": "scripts/install-openjdk.ps1", "pattern": "!native_", "desc": "managed jobs only"},
        {"script": "scripts/install-android-sdk.ps1", "pattern": "!native_", "desc": "managed jobs only"},
        {"script": "scripts/install-android-platform.ps1", "pattern": "!native_", "desc": "managed jobs only"},
        {"script": "scripts/install-dotnet-workloads.ps1", "pattern": "!native_", "desc": "managed jobs only"},
        {"script": "scripts/install-winsdk.ps1", "pattern": "native_win", "desc": "Windows native jobs (conditional)"},
    ]

    return install_patterns


def scan_docker_contexts(root: Path) -> dict[str, list[str]]:
    """Scan Docker directories for all files that are part of the build context."""
    docker_base = root / "scripts" / "Docker"
    contexts = {}
    if not docker_base.exists():
        return contexts

    for docker_dir in sorted(docker_base.rglob("Dockerfile")):
        context_dir = docker_dir.parent
        rel_context = str(context_dir.relative_to(root))
        files = []
        for f in context_dir.rglob("*"):
            if f.is_file():
                files.append(str(f.relative_to(root)))
        # Also include the common cross-compile script if it exists
        common_script = docker_base / "_clang-cross-common.sh"
        if common_script.exists():
            common_rel = str(common_script.relative_to(root))
            if common_rel not in files:
                files.append(common_rel)
        contexts[rel_context] = sorted(files)

    return contexts


def map_target_to_platform(target: str) -> str:
    """Map a Cake target name to a native platform directory."""
    # externals-windows → windows
    # externals-linux-clang-cross → linux-clang-cross
    # externals-winui-angle → winui-angle
    platform = re.sub(r"^externals-", "", target)
    return platform


def map_job_to_docker(job_name: str) -> "str | None":
    """Infer Docker context from job name patterns."""
    # Linux matrix jobs encode variant in the name
    if "_alpine_" in job_name:
        return "scripts/Docker/alpine"
    if "_bionic_" in job_name:
        return "scripts/Docker/bionic"
    if "native_wasm_" in job_name:
        return "scripts/Docker/wasm"
    # Standard Linux jobs use debian
    if "native_linux_" in job_name:
        # Check for loongarch64 which uses debian/13
        if "_loongarch64_" in job_name and "_alpine" not in job_name:
            return "scripts/Docker/debian/13"
        if "_alpine" not in job_name and "_bionic" not in job_name:
            return "scripts/Docker/debian/11"
    return None


def map_job_to_install_scripts(job_name: str, install_conditions: list[dict]) -> list[str]:
    """Determine which install scripts apply to a given job."""
    scripts = []
    for cond in install_conditions:
        pattern = cond["pattern"]
        if pattern.startswith("!"):
            # Negative pattern — applies when name does NOT start with the pattern
            if not job_name.startswith(pattern[1:]):
                scripts.append(cond["script"])
        elif pattern in job_name:
            scripts.append(cond["script"])
    return scripts


def build_target_deps(
    platform: str,
    platform_info: dict,
    cake_graph: dict[str, list[str]],
    docker_contexts: dict[str, list[str]],
    root: Path,
) -> list[str]:
    """Build the complete file dependency list for a native target."""
    deps = set()

    # 1. Transitive Cake dependencies from the platform's build.cake
    build_cake = platform_info["build_cake"]
    cake_deps = transitive_cake_deps(build_cake, cake_graph)
    deps.update(cake_deps)

    # 2. Other files in the native platform directory (xcodeproj, sln, etc.)
    platform_dir = root / platform_info["directory"]
    for f in platform_dir.rglob("*"):
        if f.is_file() and not f.name.startswith("."):
            rel = str(f.relative_to(root))
            # Skip build output directories
            if "/bin/" not in rel and "/obj/" not in rel and "/libs/" not in rel:
                deps.add(rel)

    # 3. VERSIONS.txt — always relevant
    deps.add("scripts/VERSIONS.txt")

    # 4. Skia submodule marker — we use a special token for this
    deps.add("externals/skia")  # Sentinel: cache key uses git rev-parse HEAD

    return sorted(deps)


def generate_job_registry(
    platforms: dict[str, dict],
    cake_graph: dict[str, list[str]],
    docker_contexts: dict[str, list[str]],
    install_conditions: list[dict],
    root: Path,
) -> dict:
    """Generate the complete job dependency registry."""

    # Build per-target (platform) dependency sets
    target_deps = {}
    for platform, info in platforms.items():
        target_deps[platform] = build_target_deps(
            platform, info, cake_graph, docker_contexts, root
        )

    # Define known job patterns and their targets + docker contexts
    # This maps the Azure DevOps job naming convention to targets
    job_patterns = []

    # macOS stage jobs
    for arch in ["x86", "x64", "arm", "arm64"]:
        job_patterns.append({
            "pattern": f"native_android_{arch}_",
            "target": "android",
            "docker": None,
            "install_match": f"native_android_{arch}_macos",
        })
    job_patterns.extend([
        {"pattern": "native_ios_", "target": "ios", "docker": None, "install_match": "native_ios_macos"},
        {"pattern": "native_maccatalyst_", "target": "maccatalyst", "docker": None, "install_match": "native_maccatalyst_macos"},
        {"pattern": "native_macos_", "target": "macos", "docker": None, "install_match": "native_macos_macos"},
        {"pattern": "native_tvos_", "target": "tvos", "docker": None, "install_match": "native_tvos_macos"},
    ])

    # Windows stage jobs
    for arch in ["x86", "x64", "arm64"]:
        job_patterns.append({
            "pattern": f"native_win32_{arch}_",
            "target": "windows",
            "docker": None,
            "install_match": f"native_win32_{arch}_windows",
        })
        job_patterns.append({
            "pattern": f"native_winui_{arch}_",
            "target": "winui",
            "docker": None,
            "install_match": f"native_winui_{arch}_windows",
        })
        job_patterns.append({
            "pattern": f"native_winui_angle_{arch}_",
            "target": "winui-angle",
            "docker": None,
            "install_match": f"native_winui_angle_{arch}_windows",
        })

    job_patterns.append({
        "pattern": "native_win32_x64_nanoserver_",
        "target": "nanoserver",
        "docker": None,
        "install_match": "native_win32_x64_nanoserver_windows",
    })

    # Build the output structure
    output = {
        "_metadata": {
            "description": "Native build dependency graph for SkiaSharp cache keys",
            "generator": "scripts/generate-native-dep-graph.py",
            "usage": "Used by scripts/compute-native-cache-key.ps1 to determine cache key inputs",
        },
        "targets": {},
        "docker_contexts": docker_contexts,
        "install_scripts": {},
    }

    # Per-target dependencies (the core of the cache key)
    for platform, deps in target_deps.items():
        output["targets"][platform] = {
            "files": deps,
            "cake_target": f"externals-{platform}",
        }

    # Install script mapping (for documentation/validation)
    for cond in install_conditions:
        script = cond["script"]
        if script not in output["install_scripts"]:
            output["install_scripts"][script] = {
                "job_pattern": cond["pattern"],
                "description": cond["desc"],
                "affects_output": script
                in [
                    "scripts/install-android-ndk.ps1",
                    "scripts/install-tizen.ps1",
                    "scripts/install-llvm.ps1",
                    "scripts/install-emsdk.sh",
                    "scripts/select-xcode.sh",
                ],
            }

    return output


def main():
    parser = argparse.ArgumentParser(description="Generate native build dependency graph")
    parser.add_argument("--root", default=".", help="Repository root directory")
    parser.add_argument("--output", default=None, help="Output JSON file path")
    args = parser.parse_args()

    root = Path(args.root).resolve()
    output_path = Path(args.output) if args.output else root / "scripts" / "native-build-deps.json"

    print(f"Scanning repository: {root}")
    print()

    # 1. Build Cake dependency graph
    print("1. Scanning Cake #load dependencies...")
    cake_graph = build_cake_graph(root)
    total_loads = sum(len(v) for v in cake_graph.values())
    print(f"   Found {len(cake_graph)} .cake files with {total_loads} #load directives")

    # 2. Discover native platforms
    print("2. Scanning native platform directories...")
    platforms = scan_native_platforms(root)
    print(f"   Found {len(platforms)} platforms: {', '.join(sorted(platforms.keys()))}")

    # 3. Scan bootstrapper install conditions
    print("3. Parsing bootstrapper install conditions...")
    install_conditions = scan_bootstrapper_install_conditions(root)
    print(f"   Found {len(install_conditions)} install script mappings")

    # 4. Scan Docker contexts
    print("4. Scanning Docker build contexts...")
    docker_contexts = scan_docker_contexts(root)
    print(f"   Found {len(docker_contexts)} Docker contexts")

    # 5. Generate the registry
    print("5. Generating dependency graph...")
    registry = generate_job_registry(
        platforms, cake_graph, docker_contexts, install_conditions, root
    )

    # Add Cake graph for reference
    registry["cake_graph"] = {k: v for k, v in sorted(cake_graph.items()) if v}

    # 6. Write output
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "w", encoding="utf-8") as f:
        json.dump(registry, f, indent=2, sort_keys=False)
    print(f"\nWrote dependency graph to: {output_path}")

    # Summary
    print("\n=== Summary ===")
    for target, info in sorted(registry["targets"].items()):
        file_count = len(info["files"])
        print(f"  {target:<25} {file_count:>3} files")

    # 7. Mermaid diagram (always alongside JSON)
    mermaid_path = output_path.with_suffix(".md")
    write_mermaid(registry, cake_graph, docker_contexts, mermaid_path)
    print(f"Wrote Mermaid diagram to: {mermaid_path}")

    return 0


def mermaid_id(path: str) -> str:
    """Sanitize a file path into a valid Mermaid node ID."""
    return path.replace("/", "_").replace(".", "_").replace("-", "_").replace(" ", "_")


def short_name(path: str) -> str:
    """Get just the filename from a path."""
    return Path(path).name


def write_mermaid(
    registry: dict,
    cake_graph: dict[str, list[str]],
    docker_contexts: dict[str, list[str]],
    output_path: Path,
) -> None:
    """Write a Markdown doc with Mermaid diagrams and a summary table."""
    lines = []

    # --- Header ---
    lines.append("# Native Build Dependency Graph")
    lines.append("")
    lines.append("> Auto-generated by `scripts/generate-native-dep-graph.py`.")
    lines.append("> Used by `scripts/compute-native-cache-key.ps1` to determine cache key inputs.")
    lines.append("> Regenerate with: `python3 scripts/generate-native-dep-graph.py`")
    lines.append("")

    # --- Impact table: "if I touch X, what rebuilds?" ---
    lines.append("## Impact Table")
    lines.append("")
    lines.append("What rebuilds when you change a file?")
    lines.append("")
    lines.append("| File changed | Targets affected |")
    lines.append("|---|---|")

    # Build reverse index: file → set of targets
    reverse = defaultdict(set)
    for target, info in registry["targets"].items():
        for f in info["files"]:
            if f != "externals/skia":
                reverse[f].add(target)
    reverse["externals/skia (submodule bump)"] = set(registry["targets"].keys())

    for f in sorted(reverse.keys()):
        targets = sorted(reverse[f])
        if len(targets) == len(registry["targets"]):
            affected = "**ALL targets**"
        else:
            affected = ", ".join(f"`{t}`" for t in targets)
        lines.append(f"| `{f}` | {affected} |")
    lines.append("")

    # --- Per-target file list ---
    lines.append("## Per-Target Dependencies")
    lines.append("")
    for target, info in sorted(registry["targets"].items()):
        file_count = len(info["files"])
        lines.append(f"### `{target}` ({file_count} files)")
        lines.append("")
        for f in info["files"]:
            lines.append(f"- `{f}`")
        lines.append("")

    # --- Mermaid: target → files graph ---
    lines.append("## Dependency Graph")
    lines.append("")
    lines.append("```mermaid")
    lines.append("flowchart LR")
    lines.append("")

    # Shared scripts subgraph
    lines.append("  subgraph shared_scripts[Shared Scripts]")
    shared_files = set()
    for target_info in registry["targets"].values():
        for f in target_info["files"]:
            if f.startswith("scripts/cake/") or f == "scripts/VERSIONS.txt":
                shared_files.add(f)
    for f in sorted(shared_files):
        fid = mermaid_id(f)
        lines.append(f'    {fid}["{short_name(f)}"]')
    lines.append("  end")
    lines.append("")

    # Cake #load edges
    lines.append("  %% Cake #load dependencies")
    seen_edges = set()
    for src, deps in cake_graph.items():
        if not deps:
            continue
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
            ctx_id = mermaid_id(ctx)
            ctx_short = ctx.replace("scripts/Docker/", "")
            lines.append(f'    {ctx_id}["{ctx_short}/"]')
        lines.append("  end")
        lines.append("")

    # Native target subgraphs by category
    categories = {
        "Apple": ["ios", "macos", "maccatalyst", "tvos"],
        "Windows": ["windows", "winui", "winui-angle", "nanoserver"],
        "Linux": ["linux", "linux-clang-cross", "linuxnodeps"],
        "Mobile": ["android", "tizen"],
        "Web": ["wasm"],
    }
    for cat_name, cat_targets in categories.items():
        present = [t for t in cat_targets if t in registry["targets"]]
        if not present:
            continue
        cat_id = mermaid_id(cat_name)
        lines.append(f"  subgraph {cat_id}[{cat_name} Targets]")
        for target in present:
            tid = mermaid_id(f"target_{target}")
            lines.append(f'    {tid}(("{target}")')
        lines.append("  end")
        lines.append("")

    # Edges: target → shared scripts and build.cake
    lines.append("  %% Target dependencies")
    for target, info in sorted(registry["targets"].items()):
        tid = mermaid_id(f"target_{target}")
        for f in info["files"]:
            if f == "externals/skia":
                lines.append(f"  {tid} -->|C++ source| skia_submodule[externals/skia]")
                continue
            fid = mermaid_id(f)
            if f.startswith("scripts/"):
                lines.append(f"  {tid} --> {fid}")
            elif f.startswith("native/") and f.endswith("build.cake"):
                lines.append(f"  {tid} --> {fid}")
    lines.append("")

    # Edges: targets → docker
    lines.append("  %% Docker context associations")
    docker_targets = {
        "linux-clang-cross": ["scripts/Docker/alpine", "scripts/Docker/debian/11", "scripts/Docker/debian/13"],
        "linux": ["scripts/Docker/debian/11"],
        "wasm": ["scripts/Docker/wasm"],
    }
    for target, contexts in docker_targets.items():
        if target in registry["targets"]:
            tid = mermaid_id(f"target_{target}")
            for ctx in contexts:
                if ctx in docker_contexts:
                    ctx_id = mermaid_id(ctx)
                    lines.append(f"  {tid} -.->|Docker| {ctx_id}")
    lines.append("")

    # Styling
    lines.append("  style skia_submodule fill:#f9f,stroke:#333")
    lines.append("  style shared_scripts fill:#e8f4e8,stroke:#4a4")
    lines.append("  style docker fill:#e8e8f4,stroke:#44a")
    lines.append("```")

    output_path.parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "w", encoding="utf-8") as f:
        f.write("\n".join(lines) + "\n")


if __name__ == "__main__":
    sys.exit(main())
