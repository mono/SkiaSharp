#!/usr/bin/env python3

"""Update Skia version surfaces and exact source registrations as one operation."""

import argparse
import json
import re
import subprocess
from pathlib import Path


def run_git(cwd: Path, *args: str) -> str:
    result = subprocess.run(
        ["git", "-C", str(cwd), *args],
        check=True,
        capture_output=True,
        text=True,
    )
    return result.stdout.strip()


def replace_or_fail(content: str, pattern: str, replacement: str, description: str) -> str:
    updated, count = re.subn(pattern, replacement, content, flags=re.MULTILINE)
    if count == 0:
        raise RuntimeError(f"Could not update {description}.")
    return updated


def replace_transition(
    content: str,
    current_pattern: str,
    target_pattern: str,
    replacement: str,
    description: str,
) -> str:
    """Apply a transition once while allowing an already-updated target value."""
    if re.search(current_pattern, content, flags=re.MULTILINE):
        return replace_or_fail(content, current_pattern, replacement, description)
    if not re.search(target_pattern, content, flags=re.MULTILINE):
        raise RuntimeError(f"Could not find current or target {description}.")
    return content


def update_versions(repo_root: Path, current: int, target: int, upstream_ref: str) -> None:
    """Synchronize version files, C ABI increment, and component-governance SHAs."""
    versions_path = repo_root / "scripts" / "VERSIONS.txt"
    pipeline_path = repo_root / "scripts" / "azure-templates-variables.yml"
    cgmanifest_path = repo_root / "cgmanifest.json"
    sk_types_path = repo_root / "externals" / "skia" / "include" / "c" / "sk_types.h"
    skia_root = repo_root / "externals" / "skia"

    submodule_hash = run_git(skia_root, "rev-parse", "HEAD")
    upstream_hash = run_git(
        skia_root,
        "rev-parse",
        "--verify",
        f"upstream/{upstream_ref}^{{commit}}",
    )

    versions = versions_path.read_text(encoding="utf-8-sig")
    pipeline = pipeline_path.read_text(encoding="utf-8-sig")
    sk_types = sk_types_path.read_text(encoding="utf-8-sig")
    cgmanifest = json.loads(cgmanifest_path.read_text(encoding="utf-8-sig"))

    match = re.search(rf"\b(\d+)\.(?:{current}|{target})\.\d+\b", versions)
    if not match:
        raise RuntimeError(
            f"Could not find current or target NuGet version in {versions_path}."
        )
    major = match.group(1)

    if current != target:
        versions = replace_transition(
            versions,
            rf"(skia\s+release\s+)m{current}\b",
            rf"skia\s+release\s+m{target}\b",
            rf"\g<1>m{target}",
            "Skia release milestone",
        )
        versions = replace_transition(
            versions,
            rf"(libSkiaSharp\s+milestone\s+){current}\b",
            rf"libSkiaSharp\s+milestone\s+{target}\b",
            rf"\g<1>{target}",
            "libSkiaSharp milestone",
        )
        versions = replace_or_fail(
            versions,
            r"(libSkiaSharp\s+increment\s+)\d+\b",
            r"\g<1>0",
            "libSkiaSharp increment",
        )
        versions = versions.replace(f"{current}.0.0", f"{target}.0.0")
        versions = re.sub(
            rf"\b{major}\.{current}\.\d+\.0\b",
            f"{major}.{target}.0.0",
            versions,
        )
        versions = re.sub(
            rf"\b{major}\.{current}\.\d+\b",
            f"{major}.{target}.0",
            versions,
        )
        pipeline = re.sub(
            rf"(SKIASHARP_VERSION:\s*){major}\.{current}\.\d+\b",
            rf"\g<1>{major}.{target}.0",
            pipeline,
        )
        sk_types = replace_or_fail(
            sk_types,
            r"(#define\s+SK_C_INCREMENT\s+)\d+\b",
            r"\g<1>0",
            "SK_C_INCREMENT",
        )
    git_registration = None
    version_registration = None
    for registration in cgmanifest["registrations"]:
        component = registration.get("component", {})
        git_component = component.get("git", {})
        other_component = component.get("other", {})
        if git_component.get("repositoryUrl", "").endswith("/mono/skia.git"):
            git_component["commitHash"] = submodule_hash
            git_registration = registration
        if other_component.get("name") == "skia":
            other_component["version"] = f"chrome/m{target}"
            registration["chrome_milestone"] = target
            registration["upstream_merge_commit"] = upstream_hash
            version_registration = registration

    if git_registration is None or version_registration is None:
        raise RuntimeError("Could not find both Skia registrations in cgmanifest.json.")

    if not re.search(rf"libSkiaSharp\s+milestone\s+{target}\b", versions):
        raise RuntimeError("VERSIONS.txt does not contain the target milestone.")
    if current != target:
        expected_nuget = f"{major}.{target}.0"
        if not re.search(r"libSkiaSharp\s+increment\s+0\b", versions):
            raise RuntimeError("VERSIONS.txt increment is not zero.")
        if not re.search(r"#define\s+SK_C_INCREMENT\s+0\b", sk_types):
            raise RuntimeError("SK_C_INCREMENT is not zero.")
        if not re.search(
            rf"SKIASHARP_VERSION:\s*{re.escape(expected_nuget)}\b",
            pipeline,
        ):
            raise RuntimeError("SKIASHARP_VERSION does not match the target NuGet version.")

        versions_path.write_text(versions, encoding="utf-8", newline="")
        pipeline_path.write_text(pipeline, encoding="utf-8", newline="")
        sk_types_path.write_text(sk_types, encoding="utf-8", newline="")

    cgmanifest_path.write_text(
        json.dumps(cgmanifest, indent=2) + "\n",
        encoding="utf-8",
        newline="",
    )

    print(f"Updated SkiaSharp version files: m{current} -> m{target}")
    print(f"Submodule HEAD: {submodule_hash}")
    print(f"Upstream {upstream_ref}: {upstream_hash}")
    print("GATE PASSED: version files and Skia registrations are consistent.")


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Update all SkiaSharp version files for a Skia sync."
    )
    parser.add_argument("--current", required=True, type=int)
    parser.add_argument("--target", required=True, type=int)
    parser.add_argument("--upstream-ref")
    parser.add_argument("--repo-root", type=Path)
    args = parser.parse_args()

    repo_root = (
        args.repo_root.resolve()
        if args.repo_root
        else Path(__file__).resolve().parents[4]
    )
    upstream_ref = args.upstream_ref or f"chrome/m{args.target}"
    update_versions(repo_root, args.current, args.target, upstream_ref)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
