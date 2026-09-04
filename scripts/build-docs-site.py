#!/usr/bin/env python3
"""Build DocFX with repository identity rendered from an untracked temporary TOC."""

from __future__ import annotations

import argparse
import json
import os
import subprocess
import sys
import tempfile
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
DOCFX_DIR = ROOT / "documentation" / "docfx"
IDENTITY_DIR = ROOT / "scripts" / "infra"
PLACEHOLDERS = (
    "{{Repository}}",
    "{{SkiaRepository}}",
    "{{DocsRepository}}",
    "{{PublicSiteBaseUrl}}",
)


def parse_args() -> tuple[argparse.Namespace, list[str]]:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--config",
        type=Path,
        default=DOCFX_DIR / "docfx.json",
        help="DocFX config path relative to the repository root.",
    )
    return parser.parse_known_args()


def main() -> int:
    args, docfx_args = parse_args()
    config_path = args.config
    if not config_path.is_absolute():
        config_path = ROOT / config_path
    if not config_path.is_file():
        print(f"ERROR: DocFX config not found: {config_path}", file=sys.stderr)
        return 2

    sys.path.insert(0, str(IDENTITY_DIR))
    import repository_identity

    config = json.loads(config_path.read_text(encoding="utf-8"))
    destination = Path(config["build"]["dest"])
    if not destination.is_absolute():
        destination = (DOCFX_DIR / destination).resolve()

    with tempfile.TemporaryDirectory(
        prefix=".identity-preview.",
        dir=DOCFX_DIR,
    ) as temporary_source:
        temporary_source_path = Path(temporary_source)
        toc_path = temporary_source_path / "TOC.yml"
        toc_path.write_bytes((DOCFX_DIR / "TOC.yml").read_bytes())
        repository_identity.render_identity_file(
            toc_path,
            repository_identity.resolve_identity(ROOT),
        )

        content = config["build"]["content"]
        content[0]["exclude"] = list(content[0].get("exclude", [])) + [
            "TOC.yml",
            ".identity-preview.*/**",
        ]
        content.append(
            {
                "src": temporary_source_path.name,
                "files": ["TOC.yml"],
            }
        )

        descriptor, temporary_config = tempfile.mkstemp(
            prefix=".docfx-preview.",
            suffix=".json",
            dir=DOCFX_DIR,
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
            )
        finally:
            temporary_config_path.unlink(missing_ok=True)

    if result.returncode:
        return result.returncode

    unresolved = []
    for path in destination.rglob("*"):
        if not path.is_file():
            continue
        try:
            text = path.read_text(encoding="utf-8")
        except (OSError, UnicodeDecodeError):
            continue
        if any(placeholder in text for placeholder in PLACEHOLDERS):
            unresolved.append(path)
    if unresolved:
        print(
            "ERROR: Unresolved repository identity placeholder in DocFX output:\n"
            + "\n".join(str(path) for path in unresolved),
            file=sys.stderr,
        )
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
