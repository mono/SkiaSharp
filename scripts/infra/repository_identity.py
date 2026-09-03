#!/usr/bin/env python3
"""Resolve the portable repository identities used by SkiaSharp automation."""

from __future__ import annotations

import argparse
import configparser
import fnmatch
import json
import os
import re
import subprocess
import sys
from pathlib import Path
from typing import Mapping


THIS_DIR = Path(__file__).resolve().parent
DEFAULT_ROOT = THIS_DIR.parents[1]
CONFIG_PATH = THIS_DIR / "repository-identity.json"
_SLUG_RE = re.compile(r"^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$")
_URL_PATTERNS = (
    re.compile(r"^https://github\.com/(?P<slug>[^/]+/[^/]+?)(?:\.git)?/?$"),
    re.compile(r"^git://github\.com/(?P<slug>[^/]+/[^/]+?)(?:\.git)?/?$"),
    re.compile(r"^git@github\.com:(?P<slug>[^/]+/[^/]+?)(?:\.git)?$"),
    re.compile(r"^ssh://git@github\.com/(?P<slug>[^/]+/[^/]+?)(?:\.git)?/?$"),
)
_LEGACY_IDENTITY_RE = re.compile(
    r"mono/(?:SkiaSharp(?:-API-docs)?|skia)|"
    r"\bmono-(?:SkiaSharp|skia)\b|"
    r"github\.com/orgs/mono/|"
    r"repository_owners\s*:\s*mono(?:,|\b)|"
    r"github\.repository_owner\s*==\s*['\"]mono['\"]|"
    r"github\.repository\s*==\s*['\"]mono/SkiaSharp['\"]"
)
_SCANNED_SUFFIXES = {
    ".cake",
    ".md",
    ".props",
    ".ps1",
    ".psm1",
    ".py",
    ".sh",
    ".targets",
    ".yaml",
    ".yml",
}
_TRANSITION_WORKFLOWS = {
    ".github/workflows/auto-skia-sync.md",
    ".github/workflows/auto-triage.md",
    ".github/workflows/memory-leak-fixer.md",
    ".github/workflows/merge-message.md",
    ".github/workflows/performance-fixer.md",
}
_LEGACY_LITERAL_ALLOWLIST = (
    (
        "scripts/infra/repository-identity.json",
        re.compile(r"mono/(?:SkiaSharp|skia)|mono-(?:SkiaSharp|skia)"),
        "central offline fallback and retained legacy cache aliases",
    ),
    (
        ".github/workflows/auto-triage.md",
        re.compile(r"github\.com/orgs/mono/projects/1"),
        "current backlog project remains live until transfer infrastructure is ready",
    ),
    (
        ".github/workflows/auto-triage.lock.yml",
        re.compile(r"github\.com/orgs/mono/projects/1"),
        "generated form of the current backlog project transition value",
    ),
    (
        ".github/workflows/backport.yml",
        re.compile(r"repository_owners\s*:\s*mono,dotnet"),
        "temporary trusted-owner transition allowlist",
    ),
    (
        ".agents/skills/ci-status/scripts/ci-status.py",
        re.compile(r"mono-SkiaSharp"),
        "Azure pipeline 345 name is intentionally unchanged",
    ),
    (
        ".agents/skills/ci-status/SKILL.md",
        re.compile(r"mono-SkiaSharp"),
        "documents the intentionally unchanged Azure pipeline 345 name",
    ),
    (
        ".github/workflows/track-artifact-sizes.yml",
        re.compile(r"mono-SkiaSharp"),
        "Azure Pipelines check name is intentionally unchanged",
    ),
    (
        "scripts/get-skiasharp-pr.*",
        re.compile(r"mono-SkiaSharp"),
        "Azure pipeline 345 name is intentionally unchanged",
    ),
    (
        ".github/workflows/*.lock.yml",
        re.compile(r"mono/(?:SkiaSharp|skia)|mono-(?:SkiaSharp|skia)"),
        "generated workflow; its source transition value is reviewed separately",
    ),
    (
        ".github/scripts/tests/**",
        re.compile(r"mono/(?:SkiaSharp|skia)|mono-(?:SkiaSharp|skia)"),
        "migration fixture exercises the legacy identity",
    ),
    (
        "scripts/infra/**/tests/**",
        re.compile(r"mono/(?:SkiaSharp|skia)|mono-(?:SkiaSharp|skia)"),
        "migration or historical fixture",
    ),
    (
        ".agents/skills/release-notes/samples/**",
        re.compile(r"mono/(?:SkiaSharp|skia)"),
        "immutable historical release sample",
    ),
)


class IdentityError(RuntimeError):
    """A required repository identity could not be resolved safely."""


def load_config(path: Path = CONFIG_PATH) -> dict:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, ValueError) as exc:
        raise IdentityError(f"Unable to read repository identity config {path}: {exc}") from exc
    if not isinstance(value, dict):
        raise IdentityError(f"Repository identity config {path} must contain an object.")
    required = {
        "canonicalRepositoryId": int,
        "offlineRepository": str,
        "upstreamSkiaRepository": str,
        "publicSiteBaseUrl": str,
        "repositoryKey": str,
        "legacyRepositoryKeys": list,
        "skiaRepositoryKey": str,
        "legacySkiaRepositoryKeys": list,
    }
    for key, expected_type in required.items():
        if not isinstance(value.get(key), expected_type):
            raise IdentityError(
                f"Repository identity config field {key!r} must be {expected_type.__name__}."
            )
    return value


def normalize_github_repository(value: str) -> str:
    """Return an owner/repository slug for a supported GitHub URL or slug."""

    candidate = (value or "").strip()
    if _SLUG_RE.fullmatch(candidate):
        return candidate.removesuffix(".git")
    for pattern in _URL_PATTERNS:
        match = pattern.fullmatch(candidate)
        if match:
            slug = match.group("slug").removesuffix(".git")
            if _SLUG_RE.fullmatch(slug):
                return slug
    raise IdentityError(f"Unsupported GitHub repository identity: {value!r}")


def github_url(repository: str, *, git: bool = False) -> str:
    suffix = ".git" if git else ""
    return f"https://github.com/{normalize_github_repository(repository)}{suffix}"


def resolve_current_repository(
    explicit: str | None = None,
    *,
    environ: Mapping[str, str] | None = None,
    config: dict | None = None,
) -> str:
    values = environ if environ is not None else os.environ
    identity = explicit or values.get("GITHUB_REPOSITORY")
    if not identity:
        identity = (config or load_config())["offlineRepository"]
    return normalize_github_repository(identity)


def read_submodule_repository(root: Path, path: str) -> str:
    gitmodules = root / ".gitmodules"
    parser = configparser.ConfigParser(interpolation=None)
    parser.optionxform = str
    try:
        with gitmodules.open(encoding="utf-8") as stream:
            parser.read_file(stream)
    except OSError as exc:
        raise IdentityError(f"Unable to read {gitmodules}: {exc}") from exc
    section = f'submodule "{path}"'
    if not parser.has_option(section, "url"):
        raise IdentityError(f"{gitmodules} has no URL for submodule {path!r}.")
    return normalize_github_repository(parser.get(section, "url"))


def resolve_identity(
    root: Path = DEFAULT_ROOT,
    *,
    repository: str | None = None,
    environ: Mapping[str, str] | None = None,
    config_path: Path = CONFIG_PATH,
) -> dict:
    root = root.resolve()
    config = load_config(config_path)
    current = resolve_current_repository(repository, environ=environ, config=config)
    skia = read_submodule_repository(root, "externals/skia")
    docs = read_submodule_repository(root, "docs")
    upstream = normalize_github_repository(config["upstreamSkiaRepository"])
    return {
        "canonicalRepositoryId": config["canonicalRepositoryId"],
        "repository": current,
        "repositoryUrl": github_url(current),
        "repositoryGitUrl": github_url(current, git=True),
        "repositoryKey": config["repositoryKey"],
        "legacyRepositoryKeys": list(config["legacyRepositoryKeys"]),
        "skiaRepositoryKey": config["skiaRepositoryKey"],
        "legacySkiaRepositoryKeys": list(config["legacySkiaRepositoryKeys"]),
        "skiaRepository": skia,
        "skiaUrl": github_url(skia),
        "skiaGitUrl": github_url(skia, git=True),
        "docsRepository": docs,
        "docsUrl": github_url(docs),
        "docsGitUrl": github_url(docs, git=True),
        "upstreamSkiaRepository": upstream,
        "upstreamSkiaGitUrl": github_url(upstream, git=True),
        "publicSiteBaseUrl": config["publicSiteBaseUrl"].rstrip("/"),
    }


def validate_manifest(root: Path, identity: dict) -> None:
    manifest_path = root / "cgmanifest.json"
    try:
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    except (OSError, ValueError) as exc:
        raise IdentityError(f"Unable to read {manifest_path}: {exc}") from exc
    matches = [
        registration
        for registration in manifest.get("registrations", [])
        if registration.get("component", {}).get("git", {}).get("repositoryUrl")
        == identity["skiaGitUrl"]
    ]
    if len(matches) != 1:
        raise IdentityError(
            f"{manifest_path} must contain exactly one git registration for "
            f"{identity['skiaGitUrl']}; found {len(matches)}."
        )


def render_site_identity(directory: Path, identity: dict) -> int:
    """Rewrite current-site repository links without touching historical release data."""

    if not directory.is_dir():
        raise IdentityError(f"Site directory does not exist: {directory}")
    replacements = (
        (
            re.compile(r"https://github\.com/[^/]+/SkiaSharp-API-docs"),
            identity["docsUrl"],
        ),
        (
            re.compile(r"https://github\.com/[^/]+/SkiaSharp(?!-API-docs)"),
            identity["repositoryUrl"],
        ),
        (
            re.compile(
                r"https://raw\.githubusercontent\.com/[^/]+/SkiaSharp/aw-data/"
            ),
            f"https://raw.githubusercontent.com/{identity['repository']}/aw-data/",
        ),
        (
            re.compile(
                r"(?<![/A-Za-z0-9_.-])"
                r"[A-Za-z0-9_.-]+/SkiaSharp-API-docs\b"
            ),
            identity["docsRepository"],
        ),
        (
            re.compile(
                r"(?<![/A-Za-z0-9_.-])"
                r"[A-Za-z0-9_.-]+/SkiaSharp(?!-API-docs)\b"
            ),
            identity["repository"],
        ),
    )
    changed = 0
    for path in sorted(directory.rglob("*")):
        if not path.is_file() or path.suffix.lower() not in {".html", ".json", ".js"}:
            continue
        relative = path.relative_to(directory)
        if relative.parts and relative.parts[0] == "docs":
            continue
        original = path.read_text(encoding="utf-8")
        rendered = original
        for pattern, replacement in replacements:
            rendered = pattern.sub(replacement, rendered)
        if rendered != original:
            path.write_text(rendered, encoding="utf-8")
            changed += 1
    return changed


def _is_scanned_path(relative: str) -> bool:
    path = Path(relative)
    if path.suffix.lower() not in _SCANNED_SUFFIXES:
        return False
    return (
        relative.startswith(".github/scripts/")
        or relative.startswith(".github/workflows/")
        or relative.startswith(".agents/skills/")
        or relative.startswith("scripts/")
        or relative.startswith("binding/")
        or relative.startswith("source/")
        or relative.startswith("documentation/site/")
    )


def _legacy_allowlist_reason(
    relative: str, line: str, content: str
) -> str | None:
    for path_pattern, line_pattern, reason in _LEGACY_LITERAL_ALLOWLIST:
        if fnmatch.fnmatch(relative, path_pattern) and line_pattern.search(line):
            return reason
    if (
        relative.endswith(".md")
        and not relative.startswith(".github/workflows/")
        and not re.search(
            r"(?:--repo|repo:|repos/|https://github\.com/|CACHE=|REPOSITORY=|"
            r"\b(?:gh|git|curl)\s)",
            line,
        )
    ):
        return "non-executable narrative terminology"
    if relative in {
        "scripts/infra/repository-identity.json",
        "scripts/infra/repository_identity.py",
        "scripts/infra/docs/release-notes-data.py",
        "scripts/get-skiasharp-pr.sh",
        "scripts/get-skiasharp-pr.ps1",
    }:
        return "central identity implementation or current bootstrap URL"
    if relative.startswith((
        ".github/ISSUE_TEMPLATE/",
        ".agents/skills/release-notes/samples/",
    )):
        return "current public redirect or historical sample"
    if relative == ".github/pull_request_template.md":
        return "contributor template retains live redirect URLs until cutover"
    if "/tests/" in relative or relative.endswith(("_test.py", ".test.sh")):
        return "test fixture"
    if "/references/" in relative and any(
        marker in relative for marker in ("examples", "schema")
    ):
        return "historical example or schema identifier"
    if relative.endswith(".lock.yml"):
        return "generated workflow"
    if relative in _TRANSITION_WORKFLOWS:
        pairs = (
            ("mono/skiasharp", "dotnet/skiasharp"),
            ("mono/skia", "dotnet/skia"),
        )
        lowered = content.lower()
        if any(old in line.lower() and new in lowered for old, new in pairs):
            return "temporary dual-owner gh-aw transition allowlist"
    if relative == ".agents/skills/ci-status/scripts/ci-status.py" and "mono-SkiaSharp" in line:
        return "Azure pipeline 345 name is intentionally unchanged"
    if relative == ".github/workflows/track-artifact-sizes.yml" and "mono-SkiaSharp" in line:
        return "Azure Pipelines check name is intentionally unchanged"
    if line.lstrip().startswith("#") and "https://github.com/" not in line:
        return "non-executable comment"
    return None


def scan_identity_drift(root: Path) -> list[str]:
    """Return executable old-owner literals not covered by migration exceptions."""

    root = root.resolve()
    try:
        tracked = subprocess.run(
            ["git", "ls-files", "--cached", "--others", "--exclude-standard"],
            cwd=root,
            check=True,
            capture_output=True,
            text=True,
        ).stdout.splitlines()
    except subprocess.CalledProcessError as exc:
        raise IdentityError(f"Unable to enumerate repository files: {exc}") from exc
    violations = []
    for relative in sorted(set(tracked)):
        if not _is_scanned_path(relative):
            continue
        path = root / relative
        try:
            content = path.read_text(encoding="utf-8")
        except (OSError, UnicodeDecodeError):
            continue
        if (
            "tibdex/backport" in content
            and relative != "scripts/infra/repository_identity.py"
            and "/tests/" not in relative
        ):
            violations.append(f"{relative}: forbidden executable tibdex/backport reference")
        for number, line in enumerate(content.splitlines(), 1):
            if not _LEGACY_IDENTITY_RE.search(line):
                continue
            if _legacy_allowlist_reason(relative, line, content):
                continue
            violations.append(f"{relative}:{number}: {line.strip()}")
    return violations


def _lookup(value: object, dotted_key: str) -> object:
    current = value
    for part in dotted_key.split("."):
        if not isinstance(current, dict) or part not in current:
            raise IdentityError(f"Unknown repository identity field: {dotted_key}")
        current = current[part]
    return current


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=DEFAULT_ROOT)
    parser.add_argument("--repository")
    subparsers = parser.add_subparsers(dest="command", required=True)
    subparsers.add_parser("json")
    get_parser = subparsers.add_parser("get")
    get_parser.add_argument("field")
    subparsers.add_parser("validate")
    subparsers.add_parser("scan")
    render_parser = subparsers.add_parser("render-site")
    render_parser.add_argument("directory", type=Path)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = create_parser().parse_args(argv)
    try:
        identity = resolve_identity(args.root, repository=args.repository)
        if args.command == "json":
            print(json.dumps(identity, sort_keys=True))
        elif args.command == "get":
            value = _lookup(identity, args.field)
            if isinstance(value, (dict, list)):
                print(json.dumps(value, sort_keys=True))
            else:
                print(value)
        elif args.command == "validate":
            validate_manifest(args.root.resolve(), identity)
            print(
                f"Repository identity is valid: {identity['repository']} "
                f"(ID {identity['canonicalRepositoryId']}), "
                f"Skia {identity['skiaRepository']}, docs {identity['docsRepository']}."
            )
        elif args.command == "render-site":
            count = render_site_identity(args.directory, identity)
            print(f"Rendered repository identity in {count} site file(s).")
        elif args.command == "scan":
            violations = scan_identity_drift(args.root)
            if violations:
                raise IdentityError(
                    "Executable old-owner literals require migration review:\n"
                    + "\n".join(violations)
                )
            print("Executable repository identity scan passed.")
        return 0
    except IdentityError as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
