#!/usr/bin/env python3
"""Build DocFX from temporary repository-aware inputs."""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
import tempfile
from pathlib import Path
from urllib.parse import urlsplit


ROOT = Path(__file__).resolve().parents[1]
DOCFX_DIR = ROOT / "documentation" / "docfx"
PUBLIC_SITE_METADATA = "_publicSiteBaseUrl"
PLACEHOLDER = "{{PublicSiteBaseUrl}}"
_TOC_HREF_RE = re.compile(
    r"^(?P<prefix>\s*href:\s*)(?P<quote>['\"]?)"
    r"(?P<href>[^'\"]+?)(?P=quote)(?P<ending>\s*)$"
)
_GITHUB_REMOTE_PATTERNS = (
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
_GITHUB_SLUG_RE = re.compile(
    r"[A-Za-z0-9](?:[A-Za-z0-9-]{0,37}[A-Za-z0-9])?"
    r"/[A-Za-z0-9._-]{1,100}"
)


def parse_args() -> tuple[argparse.Namespace, list[str]]:
    parser = argparse.ArgumentParser(
        description="Build DocFX without modifying tracked templates.",
    )
    parser.add_argument(
        "--config",
        type=Path,
        default=DOCFX_DIR / "docfx.json",
        help="DocFX config path, relative to the repository root.",
    )
    parser.add_argument(
        "--repository-url",
        help="Current GitHub repository URL; defaults to the validated origin remote.",
    )
    parser.add_argument(
        "--public-site-base-url",
        help="Override the configured public site base for a staging build.",
    )
    parser.add_argument(
        "--app-footer",
        help="Optional DocFX _appFooter value for CI builds.",
    )
    return parser.parse_known_args()


def load_docfx_config(path: Path) -> dict:
    try:
        config = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise ValueError(f"Unable to read DocFX config {path}: {exc}") from exc
    if not isinstance(config, dict) or not isinstance(config.get("build"), dict):
        raise ValueError(f"DocFX config {path} must contain a build object.")
    return config


def normalize_github_repository_url(value: str) -> str:
    if value != value.strip() or any(
        character.isspace()
        or ord(character) < 32
        or ord(character) == 127
        for character in value
    ):
        raise ValueError(f"Unsupported GitHub repository URL: {value!r}")
    for pattern in _GITHUB_REMOTE_PATTERNS:
        match = pattern.fullmatch(value)
        if not match:
            continue
        slug = match.group("slug").removesuffix(".git")
        if (
            _GITHUB_SLUG_RE.fullmatch(slug)
            and "--" not in slug.split("/", 1)[0]
            and slug.split("/", 1)[1] not in {".", ".."}
        ):
            return f"https://github.com/{slug}"
    raise ValueError(f"Unsupported GitHub repository URL: {value!r}")


def repository_url_from_remote(root: Path) -> str:
    result = subprocess.run(
        ["git", "remote", "get-url", "origin"],
        cwd=root,
        check=False,
        capture_output=True,
        text=True,
    )
    if result.returncode:
        message = result.stderr.strip() or "origin remote is unavailable"
        raise ValueError(f"Unable to resolve local repository URL: {message}")
    return normalize_github_repository_url(result.stdout.rstrip("\n"))


def normalize_public_site_base_url(value: str) -> str:
    if value != value.strip():
        raise ValueError("Public site base URL must not contain outer whitespace.")
    parsed = urlsplit(value)
    if (
        parsed.scheme != "https"
        or not parsed.netloc
        or parsed.query
        or parsed.fragment
    ):
        raise ValueError(
            "Public site base URL must be HTTPS without a query or fragment."
        )
    return value.rstrip("/")


def configured_public_site_base_url(config: dict) -> str:
    metadata = config["build"].get("globalMetadata")
    if not isinstance(metadata, dict):
        raise ValueError("DocFX build.globalMetadata must be an object.")
    value = metadata.get(PUBLIC_SITE_METADATA)
    if not isinstance(value, str):
        raise ValueError(
            f"DocFX global metadata must contain {PUBLIC_SITE_METADATA!r}."
        )
    return normalize_public_site_base_url(value)


def prepare_config(
    config: dict,
    temporary_source: Path,
    repository_url: str,
) -> None:
    build = config["build"]
    content = build.get("content")
    if (
        not isinstance(content, list)
        or not content
        or not isinstance(content[0], dict)
    ):
        raise ValueError("DocFX build.content must contain a primary source object.")

    exclusions = content[0].setdefault("exclude", [])
    if not isinstance(exclusions, list):
        raise ValueError("DocFX build.content[0].exclude must be a list.")
    exclusions.extend(["TOC.yml", ".docfx-preview.*/**"])
    content.append(
        {
            "src": temporary_source.name,
            "files": ["TOC.yml"],
        }
    )

    metadata = build.setdefault("globalMetadata", {})
    if not isinstance(metadata, dict):
        raise ValueError("DocFX build.globalMetadata must be an object.")
    contribution = metadata.setdefault("_gitContribute", {})
    if not isinstance(contribution, dict):
        raise ValueError(
            "DocFX build.globalMetadata._gitContribute must be an object."
        )
    contribution["repo"] = repository_url


def render_toc(path: Path, public_site_base_url: str) -> None:
    text = path.read_text(encoding="utf-8")
    path.write_text(
        text.replace(PLACEHOLDER, public_site_base_url),
        encoding="utf-8",
    )


def rebase_toc_relative_links(path: Path) -> None:
    lines = []
    for line in path.read_text(encoding="utf-8").splitlines(keepends=True):
        newline = "\n" if line.endswith("\n") else ""
        content = line.removesuffix("\n")
        match = _TOC_HREF_RE.fullmatch(content)
        if not match:
            lines.append(line)
            continue
        href = match.group("href")
        if "://" in href or href.startswith(("/", "~", "#")):
            lines.append(line)
            continue
        lines.append(
            f"{match.group('prefix')}{match.group('quote')}../{href}"
            f"{match.group('quote')}{match.group('ending')}{newline}"
        )
    path.write_text("".join(lines), encoding="utf-8")


def find_unresolved_placeholders(destination: Path) -> list[Path]:
    unresolved = []
    for path in destination.rglob("*"):
        if not path.is_file():
            continue
        try:
            text = path.read_text(encoding="utf-8")
        except (OSError, UnicodeDecodeError):
            continue
        if PLACEHOLDER in text:
            unresolved.append(path)
    return unresolved


def main() -> int:
    args, docfx_args = parse_args()
    config_path = args.config
    if not config_path.is_absolute():
        config_path = ROOT / config_path
    config_path = config_path.resolve()
    config_directory = config_path.parent
    toc_source = config_directory / "TOC.yml"

    try:
        config = load_docfx_config(config_path)
        repository_url = (
            normalize_github_repository_url(args.repository_url)
            if args.repository_url is not None
            else repository_url_from_remote(ROOT)
        )
        public_site_base_url = (
            normalize_public_site_base_url(args.public_site_base_url)
            if args.public_site_base_url is not None
            else configured_public_site_base_url(config)
        )
        destination = Path(config["build"]["dest"])
        if not destination.is_absolute():
            destination = (config_directory / destination).resolve()
        if not toc_source.is_file():
            raise ValueError(f"DocFX TOC not found: {toc_source}")
    except (KeyError, TypeError, ValueError) as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        return 2

    with tempfile.TemporaryDirectory(
        prefix=".docfx-preview.",
        dir=config_directory,
    ) as temporary_source:
        temporary_source_path = Path(temporary_source)
        temporary_toc = temporary_source_path / "TOC.yml"
        temporary_toc.write_bytes(toc_source.read_bytes())
        try:
            render_toc(temporary_toc, public_site_base_url)
            rebase_toc_relative_links(temporary_toc)
            prepare_config(config, temporary_source_path, repository_url)
            if args.app_footer is not None:
                config["build"]["globalMetadata"]["_appFooter"] = args.app_footer
        except (OSError, ValueError) as exc:
            print(f"ERROR: {exc}", file=sys.stderr)
            return 2

        descriptor, temporary_config = tempfile.mkstemp(
            prefix=".docfx-preview.",
            suffix=".json",
            dir=config_directory,
        )
        os.close(descriptor)
        temporary_config_path = Path(temporary_config)
        try:
            temporary_config_path.write_text(
                json.dumps(config, indent=2) + "\n",
                encoding="utf-8",
            )
            result = subprocess.run(
                ["dotnet", "docfx", str(temporary_config_path), *docfx_args],
                cwd=ROOT,
                check=False,
            )
        finally:
            temporary_config_path.unlink(missing_ok=True)

    if result.returncode:
        return result.returncode

    unresolved = find_unresolved_placeholders(destination)
    if unresolved:
        print(
            "ERROR: Unresolved public site placeholder in DocFX output:\n"
            + "\n".join(str(path) for path in unresolved),
            file=sys.stderr,
        )
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
