#!/usr/bin/env python3
"""Resolve repository identities used only by Skia synchronization."""

from __future__ import annotations

import argparse
import configparser
import json
import os
import re
import sys
from pathlib import Path


_REPOSITORY_RE = re.compile(
    r"[A-Za-z0-9](?:[A-Za-z0-9-]{0,37}[A-Za-z0-9])?/[A-Za-z0-9._-]{1,100}"
)
_GITHUB_URL_RE = re.compile(
    r"(?:https://github\.com/|git@github\.com:)"
    r"(?P<repository>[A-Za-z0-9][A-Za-z0-9-]*/[A-Za-z0-9._-]+?)(?:\.git)?/?",
    re.IGNORECASE,
)
_TRANSITION_SKIA_URLS = {
    "https://github.com/mono/skia.git",
    "https://github.com/dotnet/skia.git",
}


class SyncIdentityError(RuntimeError):
    pass


def normalize_repository(value: str) -> str:
    if not isinstance(value, str) or value != value.strip():
        raise SyncIdentityError(f"Unsupported GitHub repository identity: {value!r}")
    match = _GITHUB_URL_RE.fullmatch(value)
    repository = match.group("repository") if match else value.removesuffix(".git")
    if not _REPOSITORY_RE.fullmatch(repository):
        raise SyncIdentityError(f"Unsupported GitHub repository identity: {value!r}")
    owner, name = repository.split("/", 1)
    if "--" in owner or name in {".", ".."}:
        raise SyncIdentityError(f"Unsupported GitHub repository identity: {value!r}")
    return repository


def git_url(repository: str) -> str:
    return f"https://github.com/{normalize_repository(repository)}.git"


def read_skia_repository(root: Path) -> str:
    gitmodules = root / ".gitmodules"
    parser = configparser.ConfigParser(interpolation=None)
    try:
        with gitmodules.open(encoding="utf-8") as stream:
            parser.read_file(stream)
    except (OSError, configparser.Error) as exc:
        raise SyncIdentityError(f"Unable to read {gitmodules}: {exc}") from exc

    section = 'submodule "externals/skia"'
    if not parser.has_option(section, "url"):
        raise SyncIdentityError(
            f"{gitmodules} has no URL for submodule 'externals/skia'."
        )
    return normalize_repository(parser.get(section, "url"))


def resolve_identity(root: Path, repository: str | None = None) -> dict[str, str]:
    current = repository if repository is not None else os.environ.get("GITHUB_REPOSITORY")
    if not current:
        raise SyncIdentityError("GITHUB_REPOSITORY is required.")
    current = normalize_repository(current)
    skia = read_skia_repository(root)
    return {
        "repository": current,
        "repositoryGitUrl": git_url(current),
        "skiaRepository": skia,
        "skiaGitUrl": git_url(skia),
    }


def validate_manifest(root: Path, skia_git_url: str) -> None:
    manifest_path = root / "cgmanifest.json"
    try:
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        registrations = manifest["registrations"]
    except (OSError, ValueError, KeyError, TypeError) as exc:
        raise SyncIdentityError(f"Unable to read {manifest_path}: {exc}") from exc
    if not isinstance(registrations, list):
        raise SyncIdentityError(f"{manifest_path} registrations must contain a list.")

    transition_urls = []
    for registration in registrations:
        component = registration.get("component", {}) if isinstance(registration, dict) else {}
        git_component = component.get("git", {}) if isinstance(component, dict) else {}
        repository_url = git_component.get("repositoryUrl")
        if repository_url in _TRANSITION_SKIA_URLS:
            transition_urls.append(repository_url)

    if transition_urls != [skia_git_url]:
        raise SyncIdentityError(
            f"{manifest_path} must contain exactly one paired Skia git registration "
            f"for {skia_git_url}; found {transition_urls!r}."
        )


def create_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--repository")
    subparsers = parser.add_subparsers(dest="command", required=True)
    subparsers.add_parser("json")
    subparsers.add_parser("validate-manifest")
    return parser


def main(argv: list[str] | None = None) -> int:
    args = create_parser().parse_args(argv)
    root = args.root.resolve()
    try:
        identity = resolve_identity(root, args.repository)
        if args.command == "json":
            print(json.dumps(identity, sort_keys=True))
        else:
            validate_manifest(root, identity["skiaGitUrl"])
            print(
                f"Skia sync identity is valid: {identity['repository']} with "
                f"{identity['skiaRepository']}."
            )
        return 0
    except SyncIdentityError as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
