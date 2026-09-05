#!/usr/bin/env python3
"""Resolve the portable repository identities used by SkiaSharp automation."""

from __future__ import annotations

import argparse
import configparser
import json
import os
import re
import sys
from pathlib import Path
from typing import Mapping
from urllib.parse import urlsplit


THIS_DIR = Path(__file__).resolve().parent
DEFAULT_ROOT = THIS_DIR.parents[1]
CONFIG_PATH = THIS_DIR / "repository-identity.json"
_SLUG_RE = re.compile(r"[^/]+/[^/]+")
_OWNER_RE = re.compile(
    r"[A-Za-z0-9](?:[A-Za-z0-9-]{0,37}[A-Za-z0-9])?"
)
_REPOSITORY_RE = re.compile(r"[A-Za-z0-9._-]{1,100}")
_URL_PATTERNS = (
    re.compile(
        r"https://github\.com/(?P<slug>[^/]+/[^/]+?)(?:\.git)?/?",
        re.IGNORECASE,
    ),
    re.compile(
        r"git://github\.com/(?P<slug>[^/]+/[^/]+?)(?:\.git)?/?",
        re.IGNORECASE,
    ),
    re.compile(
        r"git@github\.com:(?P<slug>[^/]+/[^/]+?)(?:\.git)?",
        re.IGNORECASE,
    ),
    re.compile(
        r"ssh://git@github\.com/(?P<slug>[^/]+/[^/]+?)(?:\.git)?/?",
        re.IGNORECASE,
    ),
)
_REPOSITORY_BOUNDARY = (
    r"(?=(?:/|\.git(?:[/?#]|$)|[?#]|[\s\"'<>),;:]|$))"
)
_CURRENT_REPOSITORY_URL_RE = re.compile(
    r"https://github\.com/[^/]+/SkiaSharp" + _REPOSITORY_BOUNDARY,
    re.IGNORECASE,
)
_DOCS_REPOSITORY_URL_RE = re.compile(
    r"https://github\.com/[^/]+/SkiaSharp-API-docs"
    + _REPOSITORY_BOUNDARY,
    re.IGNORECASE,
)
_SITE_TEXT_SUFFIXES = {".html", ".js", ".json"}
_SITE_EXCLUDED_PREFIXES = {
    ("ai",),
    ("perf",),
    ("docs", "releases"),
}
_CONFIG_FIELDS = {
    "canonicalRepositoryId": int,
    "offlineRepository": str,
    "upstreamSkiaRepository": str,
    "publicSiteBaseUrl": str,
    "skiaRepositoryKey": str,
    "legacySkiaRepositoryKeys": list,
}


class IdentityError(RuntimeError):
    """A required repository identity could not be resolved safely."""


def _normalize_slug(value: str) -> str:
    slug = value.removesuffix(".git")
    if not _SLUG_RE.fullmatch(slug):
        raise IdentityError(f"Unsupported GitHub repository identity: {value!r}")

    owner, repository = slug.split("/", 1)
    if (
        not _OWNER_RE.fullmatch(owner)
        or "--" in owner
        or not _REPOSITORY_RE.fullmatch(repository)
        or repository in {".", ".."}
    ):
        raise IdentityError(f"Unsupported GitHub repository identity: {value!r}")
    return slug


def normalize_github_repository(value: str) -> str:
    """Return an owner/repository slug for a supported GitHub URL or slug."""

    if not isinstance(value, str):
        raise IdentityError(f"Unsupported GitHub repository identity: {value!r}")
    candidate = value
    if candidate != candidate.strip() or any(
        character.isspace()
        or ord(character) < 32
        or ord(character) == 127
        for character in candidate
    ):
        raise IdentityError(f"Unsupported GitHub repository identity: {value!r}")
    for pattern in _URL_PATTERNS:
        match = pattern.fullmatch(candidate)
        if match:
            return _normalize_slug(match.group("slug"))
    return _normalize_slug(candidate)


def github_url(repository: str, *, git: bool = False) -> str:
    suffix = ".git" if git else ""
    return f"https://github.com/{normalize_github_repository(repository)}{suffix}"


def _require_nonempty_string(config: dict, key: str) -> None:
    if not config[key].strip():
        raise IdentityError(
            f"Repository identity config field {key!r} must not be empty."
        )


def load_config(path: Path | None = None) -> dict:
    path = path or CONFIG_PATH
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, ValueError) as exc:
        raise IdentityError(
            f"Unable to read repository identity config {path}: {exc}"
        ) from exc
    if not isinstance(value, dict):
        raise IdentityError(
            f"Repository identity config {path} must contain an object."
        )

    for key, expected_type in _CONFIG_FIELDS.items():
        if type(value.get(key)) is not expected_type:
            raise IdentityError(
                f"Repository identity config field {key!r} must be "
                f"{expected_type.__name__}."
            )

    if value["canonicalRepositoryId"] <= 0:
        raise IdentityError(
            "Repository identity config field 'canonicalRepositoryId' "
            "must be positive."
        )
    for key in (
        "offlineRepository",
        "upstreamSkiaRepository",
        "publicSiteBaseUrl",
        "skiaRepositoryKey",
    ):
        _require_nonempty_string(value, key)
    entries = value["legacySkiaRepositoryKeys"]
    if not entries or any(
        not isinstance(entry, str) or not entry.strip() for entry in entries
    ):
        raise IdentityError(
            "Repository identity config field 'legacySkiaRepositoryKeys' "
            "must contain non-empty strings."
        )

    normalize_github_repository(value["offlineRepository"])
    normalize_github_repository(value["upstreamSkiaRepository"])
    site = urlsplit(value["publicSiteBaseUrl"])
    if (
        site.scheme != "https"
        or not site.netloc
        or site.query
        or site.fragment
    ):
        raise IdentityError(
            "Repository identity config field 'publicSiteBaseUrl' must be "
            "an HTTPS URL without a query or fragment."
        )
    return value


def resolve_current_repository(
    explicit: str | None = None,
    *,
    environ: Mapping[str, str] | None = None,
    config: dict | None = None,
) -> str:
    if explicit is not None:
        return normalize_github_repository(explicit)

    values = environ if environ is not None else os.environ
    runtime = values.get("GITHUB_REPOSITORY")
    if runtime:
        return normalize_github_repository(runtime)

    return normalize_github_repository(
        (config or load_config())["offlineRepository"]
    )


def read_submodule_repository(root: Path, path: str) -> str:
    gitmodules = root / ".gitmodules"
    parser = configparser.ConfigParser(interpolation=None)
    try:
        with gitmodules.open(encoding="utf-8") as stream:
            parser.read_file(stream)
    except (OSError, configparser.Error) as exc:
        raise IdentityError(f"Unable to read {gitmodules}: {exc}") from exc

    section = f'submodule "{path}"'
    if not parser.has_option(section, "url"):
        raise IdentityError(
            f"{gitmodules} has no URL for submodule {path!r}."
        )
    return normalize_github_repository(parser.get(section, "url"))


def resolve_identity(
    root: Path | None = None,
    *,
    repository: str | None = None,
    environ: Mapping[str, str] | None = None,
    config_path: Path | None = None,
) -> dict:
    root = (root or DEFAULT_ROOT).resolve()
    config = load_config(config_path)
    current = resolve_current_repository(
        repository,
        environ=environ,
        config=config,
    )
    skia = read_submodule_repository(root, "externals/skia")
    docs = read_submodule_repository(root, "docs")
    upstream = normalize_github_repository(config["upstreamSkiaRepository"])

    return {
        "canonicalRepositoryId": config["canonicalRepositoryId"],
        "repository": current,
        "repositoryUrl": github_url(current),
        "repositoryGitUrl": github_url(current, git=True),
        "skiaRepository": skia,
        "skiaUrl": github_url(skia),
        "skiaGitUrl": github_url(skia, git=True),
        "skiaRepositoryKey": config["skiaRepositoryKey"],
        "legacySkiaRepositoryKeys": list(config["legacySkiaRepositoryKeys"]),
        "docsRepository": docs,
        "docsUrl": github_url(docs),
        "docsGitUrl": github_url(docs, git=True),
        "upstreamSkiaRepository": upstream,
        "upstreamSkiaGitUrl": github_url(upstream, git=True),
        "publicSiteBaseUrl": config["publicSiteBaseUrl"].rstrip("/"),
    }


def _is_excluded_site_path(relative: Path) -> bool:
    return any(
        relative.parts[:len(prefix)] == prefix
        for prefix in _SITE_EXCLUDED_PREFIXES
    )


def render_site_identity(directory: Path, identity: dict) -> int:
    """Render current site links without changing historical or owned dashboards."""

    if not directory.is_dir():
        raise IdentityError(f"Site directory does not exist: {directory}")

    replacements = (
        (_DOCS_REPOSITORY_URL_RE, identity["docsUrl"]),
        (_CURRENT_REPOSITORY_URL_RE, identity["repositoryUrl"]),
        (re.compile(r"\{\{DocsRepository\}\}"), identity["docsRepository"]),
        (re.compile(r"\{\{SkiaRepository\}\}"), identity["skiaRepository"]),
        (re.compile(r"\{\{Repository\}\}"), identity["repository"]),
        (
            re.compile(r"\{\{PublicSiteBaseUrl\}\}"),
            identity["publicSiteBaseUrl"],
        ),
    )
    changed = 0
    for path in sorted(directory.rglob("*")):
        if (
            not path.is_file()
            or path.suffix.lower() not in _SITE_TEXT_SUFFIXES
        ):
            continue
        relative = path.relative_to(directory)
        if _is_excluded_site_path(relative):
            continue

        try:
            original = path.read_text(encoding="utf-8")
        except UnicodeDecodeError as exc:
            raise IdentityError(
                f"Site text file is not valid UTF-8: {path}"
            ) from exc
        rendered = original
        for pattern, replacement in replacements:
            rendered = pattern.sub(replacement, rendered)
        if rendered != original:
            path.write_text(rendered, encoding="utf-8")
            changed += 1
    return changed


def render_identity_file(path: Path, identity: dict) -> bool:
    """Render repository identity placeholders in one UTF-8 text file."""

    try:
        original = path.read_text(encoding="utf-8")
    except (OSError, UnicodeDecodeError) as exc:
        raise IdentityError(f"Unable to read identity template {path}: {exc}") from exc

    rendered = original
    for placeholder, replacement in (
        ("{{Repository}}", identity["repository"]),
        ("{{SkiaRepository}}", identity["skiaRepository"]),
        ("{{DocsRepository}}", identity["docsRepository"]),
        ("{{PublicSiteBaseUrl}}", identity["publicSiteBaseUrl"]),
    ):
        rendered = rendered.replace(placeholder, replacement)

    if rendered == original:
        return False
    try:
        path.write_text(rendered, encoding="utf-8")
    except OSError as exc:
        raise IdentityError(
            f"Unable to write rendered identity template {path}: {exc}"
        ) from exc
    return True


def validate_manifest(root: Path, identity: dict) -> None:
    manifest_path = root / "cgmanifest.json"
    try:
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    except (OSError, ValueError) as exc:
        raise IdentityError(f"Unable to read {manifest_path}: {exc}") from exc
    if not isinstance(manifest, dict):
        raise IdentityError(f"{manifest_path} must contain an object.")

    registrations = manifest.get("registrations")
    if not isinstance(registrations, list):
        raise IdentityError(
            f"{manifest_path} field 'registrations' must contain a list."
        )

    expected_url = identity["skiaGitUrl"]
    matches = []
    for index, registration in enumerate(registrations):
        if not isinstance(registration, dict):
            raise IdentityError(
                f"{manifest_path} registration {index} must contain an object."
            )
        component = registration.get("component")
        if not isinstance(component, dict):
            raise IdentityError(
                f"{manifest_path} registration {index} component must "
                "contain an object."
            )
        component_type = component.get("type")
        if not isinstance(component_type, str) or not component_type:
            raise IdentityError(
                f"{manifest_path} registration {index} component type must "
                "contain a string."
            )
        if component_type != "git":
            continue

        git = component.get("git")
        if not isinstance(git, dict):
            raise IdentityError(
                f"{manifest_path} registration {index} git component must "
                "contain an object."
            )
        repository_url = git.get("repositoryUrl")
        if not isinstance(repository_url, str) or not repository_url:
            raise IdentityError(
                f"{manifest_path} registration {index} repositoryUrl must "
                "contain a string."
            )
        if repository_url == expected_url:
            matches.append(registration)

    if len(matches) != 1:
        raise IdentityError(
            f"{manifest_path} must contain exactly one git registration for "
            f"{expected_url}; found {len(matches)}."
        )


def _lookup(value: object, dotted_key: str) -> object:
    current = value
    for part in dotted_key.split("."):
        if not isinstance(current, dict) or part not in current:
            raise IdentityError(
                f"Unknown repository identity field: {dotted_key}"
            )
        current = current[part]
    return current


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path)
    parser.add_argument("--config", type=Path)
    parser.add_argument("--repository")
    subparsers = parser.add_subparsers(dest="command", required=True)
    subparsers.add_parser("json")
    get_parser = subparsers.add_parser("get")
    get_parser.add_argument("field")
    subparsers.add_parser("validate")
    render_site_parser = subparsers.add_parser("render-site")
    render_site_parser.add_argument("directory", type=Path)
    render_file_parser = subparsers.add_parser("render-file")
    render_file_parser.add_argument("path", type=Path)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = create_parser().parse_args(argv)
    root = (args.root or DEFAULT_ROOT).resolve()
    try:
        identity = resolve_identity(
            root,
            repository=args.repository,
            config_path=args.config,
        )
        if args.command == "json":
            print(json.dumps(identity, sort_keys=True))
        elif args.command == "get":
            value = _lookup(identity, args.field)
            if isinstance(value, (dict, list)):
                print(json.dumps(value, sort_keys=True))
            else:
                print(value)
        elif args.command == "validate":
            validate_manifest(root, identity)
            print(
                f"Repository identity is valid: {identity['repository']} "
                f"(ID {identity['canonicalRepositoryId']}), "
                f"Skia {identity['skiaRepository']}, "
                f"docs {identity['docsRepository']}."
            )
        elif args.command == "render-site":
            count = render_site_identity(args.directory.resolve(), identity)
            print(f"Rendered repository identity in {count} site file(s).")
        elif args.command == "render-file":
            changed = render_identity_file(args.path.resolve(), identity)
            state = "Rendered" if changed else "Already current"
            print(f"{state}: {args.path}")
        return 0
    except IdentityError as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
