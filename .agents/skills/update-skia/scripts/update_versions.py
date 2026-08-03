#!/usr/bin/env python3

"""Synchronize Skia version and source metadata during Phases 06 and 10.

Milestone, ABI, package, pipeline, submodule, and Skia component-governance
values must describe the same tested update. This helper changes them together
and is rerun after final native adaptations so manifests record the exact
tested SHAs. Semantic versions for tracked third-party dependencies whose
revision or enabled state changed and whose final state is enabled remain a
Phase 07 reconciliation because they come from each dependency's own metadata.
"""

import argparse
import ast
import json
import os
import re
import subprocess
from pathlib import Path


# Keys are the final path segments used in Skia DEPS. A path rename must fail loudly
# here so its Component Governance mapping is reviewed rather than guessed.
TRACKED_SKIA_DEPENDENCIES = {
    "libpng": "libpng",
    "zlib": "zlib",
    "libjpeg-turbo": "libjpeg-turbo",
    "libwebp": "libwebp",
    "freetype": "freetype",
    "harfbuzz": "harfbuzz",
    "expat": "libexpat",
    "brotli": "brotli",
    "wuffs": "wuffs",
    "dng_sdk": "dng_sdk",
    "vulkanmemoryallocator": "VulkanMemoryAllocator",
    "spirv-cross": "SPIRV-Cross",
    "d3d12allocator": "D3D12MemoryAllocator",
    "vulkan-headers": "vulkan-headers",
    "piex": "piex",
}


class DependencyReviewRequired(RuntimeError):
    pass


def run_git(cwd: Path, *args: str) -> str:
    result = subprocess.run(
        ["git", "-C", str(cwd), *args],
        check=True,
        capture_output=True,
        text=True,
    )
    return result.stdout.strip()


def git_show(cwd: Path, ref: str, path: str) -> str:
    try:
        return run_git(cwd, "show", f"{ref}:{path}")
    except subprocess.CalledProcessError as error:
        raise RuntimeError(
            f"Could not read {path} at {ref} in {cwd}: {error.stderr.strip()}"
        ) from error


def _resolve_deps_node(node: ast.AST, variables: dict[str, object]) -> object:
    if isinstance(node, ast.Call):
        if isinstance(node.func, ast.Name) and node.func.id == "Var" and len(node.args) == 1:
            key = ast.literal_eval(node.args[0])
            return variables.get(key, key)
        raise ValueError(f"Unsupported DEPS call: {ast.dump(node)}")
    if isinstance(node, ast.BinOp) and isinstance(node.op, ast.Add):
        return _resolve_deps_node(node.left, variables) + _resolve_deps_node(
            node.right, variables
        )
    if isinstance(node, ast.Dict):
        return {
            _resolve_deps_node(key, variables): _resolve_deps_node(value, variables)
            for key, value in zip(node.keys, node.values)
        }
    return ast.literal_eval(node)


def _extract_deps_dict(tree: ast.Module, name: str, variables: dict[str, object]) -> dict:
    for node in ast.iter_child_nodes(tree):
        if not isinstance(node, ast.Assign):
            continue
        if not any(
            isinstance(target, ast.Name) and target.id == name for target in node.targets
        ):
            continue
        if not isinstance(node.value, ast.Dict):
            return {}
        result = {}
        for key_node, value_node in zip(node.value.keys, node.value.values):
            try:
                key = _resolve_deps_node(key_node, variables)
                value = _resolve_deps_node(value_node, variables)
            except (ValueError, TypeError, SyntaxError):
                continue
            result[key] = value
        return result
    return {}


def parse_deps(content: str) -> dict[str, dict[str, str]]:
    """Parse enabled DEPS entries without executing the file."""
    tree = ast.parse(content)
    variables = _extract_deps_dict(tree, "vars", {})
    raw_deps = _extract_deps_dict(tree, "deps", variables)
    result = {}
    for path, value in raw_deps.items():
        if isinstance(value, str):
            locator = value
        elif isinstance(value, dict):
            locator = value.get("url", "")
        else:
            continue
        if "@" in locator:
            url, revision = locator.rsplit("@", 1)
        else:
            url, revision = locator, ""
        dependency_name = path.rstrip("/").split("/")[-1]
        result[dependency_name] = {
            "path": path,
            "url": url.strip(),
            "revision": revision.strip(),
        }
    return result


def other_registrations(manifest: dict) -> dict[str, dict]:
    registrations = {}
    for registration in manifest["registrations"]:
        component = registration.get("component", {}).get("other", {})
        name = component.get("name")
        if name:
            registrations[name] = registration
    return registrations


def dependency_changed(base: dict | None, final: dict | None) -> bool:
    return base != final


def reconcile_dependency_metadata(
    cgmanifest: dict,
    base_cgmanifest: dict,
    base_deps: dict[str, dict[str, str]],
    final_deps: dict[str, dict[str, str]],
) -> tuple[list[dict], list[str]]:
    """Synchronize DEPS identities and require explicit semantic-version verification."""
    registrations = other_registrations(cgmanifest)
    base_registrations = other_registrations(base_cgmanifest)
    manifest_to_deps = {
        manifest_name: deps_name
        for deps_name, manifest_name in TRACKED_SKIA_DEPENDENCIES.items()
    }
    changes = []
    errors = []

    for dependency_name in sorted(set(base_deps) | set(final_deps)):
        base = base_deps.get(dependency_name)
        final = final_deps.get(dependency_name)
        if not dependency_changed(base, final):
            continue
        manifest_name = TRACKED_SKIA_DEPENDENCIES.get(dependency_name)
        changes.append(
            {
                "name": dependency_name,
                "base": base,
                "final": final,
                "manifestName": manifest_name,
                "tracked": manifest_name is not None,
            }
        )

    changed_names = {change["name"] for change in changes}
    changes_by_name = {change["name"]: change for change in changes}

    for dependency_name, manifest_name in TRACKED_SKIA_DEPENDENCIES.items():
        registration = registrations.get(manifest_name)
        base_registration = base_registrations.get(manifest_name)
        base_dependency = base_deps.get(dependency_name)
        final_dependency = final_deps.get(dependency_name)
        change = changes_by_name.get(dependency_name)

        if final_dependency is None:
            if change:
                change["manifestPresent"] = registration is not None
                change["reviewRequired"] = registration is not None
            if registration is not None:
                errors.append(
                    f"{dependency_name} is disabled or removed from final DEPS, but "
                    f"the {manifest_name!r} cgmanifest registration remains."
                )
            continue
        if registration is None:
            if change:
                change["manifestPresent"] = False
                change["reviewRequired"] = True
            errors.append(
                f"Enabled tracked dependency {dependency_name} requires the "
                f"{manifest_name!r} cgmanifest registration."
            )
            continue
        if base_dependency is not None and base_registration is None:
            errors.append(
                f"Base DEPS enables {dependency_name}, but the base cgmanifest does not contain "
                f"the {manifest_name!r} registration."
            )
            continue

        base_metadata = (
            base_registration.get("skia_dependency") if base_registration else None
        )
        if base_metadata:
            if (
                base_dependency is None
                or base_metadata.get("name") != dependency_name
                or base_metadata.get("revision") != base_dependency["revision"]
            ):
                errors.append(
                    f"Base cgmanifest metadata for {dependency_name} does not match base DEPS."
                )

        metadata = registration.setdefault("skia_dependency", {})
        base_identity = (
            f"{base_dependency['url']}@{base_dependency['revision']}"
            if base_dependency
            else ""
        )
        final_identity = f"{final_dependency['url']}@{final_dependency['revision']}"
        reviewed_identity = metadata.get("version_reviewed_identity", base_identity)
        metadata["name"] = dependency_name
        metadata["revision"] = final_dependency["revision"]
        metadata.setdefault("version_reviewed_identity", reviewed_identity)
        metadata.pop("version_verified_identity", None)

        base_version = (
            base_registration["component"]["other"]["version"]
            if base_registration
            else None
        )
        final_version = registration["component"]["other"]["version"]
        changed = dependency_name in changed_names
        if changed:
            review_required = (
                reviewed_identity != final_identity
                or not metadata.get("version_source")
            )
            change.update(
                {
                    "manifestVersion": final_version,
                    "versionReviewedIdentity": metadata[
                        "version_reviewed_identity"
                    ],
                    "versionSource": metadata.get("version_source"),
                    "reviewRequired": review_required,
                }
            )
            if reviewed_identity != final_identity:
                base_revision = (base_dependency or {}).get("revision", "<disabled>")
                errors.append(
                    f"{dependency_name} changed from {base_revision} to "
                    f"{final_dependency['revision']}; derive its semantic version from checked-out "
                    "source, update cgmanifest, then set version_reviewed_identity to the final "
                    "DEPS URL@revision."
                )
            elif not metadata.get("version_source"):
                errors.append(
                    f"{dependency_name} changed but skia_dependency.version_source is empty."
                )
        else:
            metadata["version_reviewed_identity"] = final_identity
            if base_metadata and base_metadata.get("version_source"):
                metadata["version_source"] = base_metadata["version_source"]
            elif not metadata.get("version_source"):
                errors.append(
                    f"{dependency_name} lacks skia_dependency.version_source for its "
                    "current semantic version."
                )
            if final_version != base_version:
                errors.append(
                    f"{manifest_name} version changed from {base_version} to {final_version} "
                    f"without a {dependency_name} DEPS change."
                )

    for manifest_name, registration in registrations.items():
        if manifest_name == "skia":
            continue
        base_registration = base_registrations.get(manifest_name)
        if base_registration is None:
            continue
        base_version = base_registration["component"]["other"]["version"]
        final_version = registration["component"]["other"]["version"]
        if base_version == final_version:
            continue
        dependency_name = manifest_to_deps.get(manifest_name)
        if dependency_name is None:
            errors.append(
                f"{manifest_name} version changed from {base_version} to {final_version} "
                "but the component is not mapped to a Skia DEPS dependency."
            )

    return changes, errors


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


def update_versions(
    repo_root: Path,
    current: int,
    target: int,
    upstream_ref: str,
    upstream_sha: str | None = None,
    parent_base_sha: str | None = None,
    skia_base_sha: str | None = None,
    artifact_dir: Path | None = None,
) -> None:
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
        f"{upstream_sha}^{{commit}}"
        if upstream_sha
        else f"upstream/{upstream_ref}^{{commit}}",
    )

    versions = versions_path.read_text(encoding="utf-8-sig")
    pipeline = pipeline_path.read_text(encoding="utf-8-sig")
    sk_types = sk_types_path.read_text(encoding="utf-8-sig")
    cgmanifest = json.loads(cgmanifest_path.read_text(encoding="utf-8-sig"))
    dependency_changes = []
    dependency_errors = []

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
            registration["upstream_ref"] = upstream_ref
            registration["upstream_merge_commit"] = upstream_hash
            version_registration = registration

    if git_registration is None or version_registration is None:
        raise RuntimeError("Could not find both Skia registrations in cgmanifest.json.")

    if bool(parent_base_sha) != bool(skia_base_sha):
        raise RuntimeError(
            "parent_base_sha and skia_base_sha must either both be provided or both omitted."
        )
    if parent_base_sha and skia_base_sha:
        base_cgmanifest = json.loads(
            git_show(repo_root, parent_base_sha, "cgmanifest.json")
        )
        base_deps = parse_deps(git_show(skia_root, skia_base_sha, "DEPS"))
        final_deps = parse_deps((skia_root / "DEPS").read_text(encoding="utf-8-sig"))
        dependency_changes, dependency_errors = reconcile_dependency_metadata(
            cgmanifest,
            base_cgmanifest,
            base_deps,
            final_deps,
        )

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
    if artifact_dir:
        artifact_dir.mkdir(parents=True, exist_ok=True)
        (artifact_dir / "skia-dependency-changes.json").write_text(
            json.dumps(
                {
                    "baseSkiaSha": skia_base_sha,
                    "finalSkiaSha": submodule_hash,
                    "changes": dependency_changes,
                },
                indent=2,
            )
            + "\n",
            encoding="utf-8",
            newline="",
        )

    if dependency_changes:
        print("DEPS changes:")
        for change in dependency_changes:
            base_revision = (change["base"] or {}).get("revision", "<disabled>")
            final_revision = (change["final"] or {}).get("revision", "<disabled>")
            tracked = change["manifestName"] or "not tracked"
            print(
                f"  {change['name']}: {base_revision} -> {final_revision} "
                f"(cgmanifest: {tracked})"
            )
    if dependency_errors:
        raise DependencyReviewRequired(
            "Dependency manifest review required:\n- " + "\n- ".join(dependency_errors)
        )

    print(f"Updated SkiaSharp version files: m{current} -> m{target}")
    print(f"Submodule HEAD: {submodule_hash}")
    print(f"Upstream {upstream_ref}: {upstream_hash}")
    print("GATE PASSED: version files and Skia registrations are consistent.")


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Update all SkiaSharp version files for a Skia sync."
    )
    parser.add_argument("--current", type=int)
    parser.add_argument("--target", type=int)
    parser.add_argument("--upstream-ref")
    parser.add_argument("--upstream-sha")
    parser.add_argument("--parent-base-sha")
    parser.add_argument("--skia-base-sha")
    parser.add_argument("--artifact-dir", type=Path)
    parser.add_argument("--repo-root", type=Path)
    args = parser.parse_args()

    repo_root = (
        args.repo_root.resolve()
        if args.repo_root
        else Path(__file__).resolve().parents[4]
    )
    if os.environ.get("SKIA_SYNC_AUTOMATION") == "1":
        required = {
            "SKIA_SYNC_CURRENT": os.environ.get("SKIA_SYNC_CURRENT"),
            "SKIA_SYNC_TARGET": os.environ.get("SKIA_SYNC_TARGET"),
            "SKIA_SYNC_UPSTREAM_REF": os.environ.get("SKIA_SYNC_UPSTREAM_REF"),
            "SKIA_SYNC_TARGET_UPSTREAM_SHA": os.environ.get(
                "SKIA_SYNC_TARGET_UPSTREAM_SHA"
            ),
            "SKIA_SYNC_PARENT_BASE_SHA": os.environ.get("SKIA_SYNC_PARENT_BASE_SHA"),
            "SKIA_SYNC_SKIA_BASE_SHA": os.environ.get("SKIA_SYNC_SKIA_BASE_SHA"),
        }
        missing = [name for name, value in required.items() if not value]
        if missing:
            raise RuntimeError(
                "Missing required automation values: " + ", ".join(missing)
            )
        current = int(required["SKIA_SYNC_CURRENT"])
        target = int(required["SKIA_SYNC_TARGET"])
        upstream_ref = required["SKIA_SYNC_UPSTREAM_REF"]
        upstream_sha = required["SKIA_SYNC_TARGET_UPSTREAM_SHA"]
        parent_base_sha = required["SKIA_SYNC_PARENT_BASE_SHA"]
        skia_base_sha = required["SKIA_SYNC_SKIA_BASE_SHA"]
        artifact_dir = Path(
            os.environ.get(
                "SKIA_SYNC_ARTIFACT_DIR",
                os.environ.get("TMPDIR", "/tmp") + "/skia-sync-agent",
            )
        )
    else:
        if args.current is None or args.target is None:
            parser.error("--current and --target are required outside automation.")
        current = args.current
        target = args.target
        upstream_ref = args.upstream_ref or f"chrome/m{target}"
        upstream_sha = args.upstream_sha
        parent_base_sha = args.parent_base_sha
        skia_base_sha = args.skia_base_sha
        artifact_dir = args.artifact_dir
    update_versions(
        repo_root,
        current,
        target,
        upstream_ref,
        upstream_sha,
        parent_base_sha,
        skia_base_sha,
        artifact_dir,
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
