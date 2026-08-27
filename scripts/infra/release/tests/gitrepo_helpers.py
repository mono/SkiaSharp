"""Test-only helpers for building throwaway local Git repositories.

Prepare-planning tests exercise real ``git`` commands against temporary
on-disk repositories (created here) rather than mocking Git, so the same
code path is exercised as in production.
"""

from __future__ import annotations

import subprocess
import tempfile
from pathlib import Path


def _git(*args: str, cwd: Path) -> subprocess.CompletedProcess:
    result = subprocess.run(
        ["git", *args], cwd=cwd, capture_output=True, text=True, check=True
    )
    return result


def init_bare(path: Path) -> Path:
    path.mkdir(parents=True, exist_ok=True)
    _git("init", "--bare", "-b", "main", cwd=path)
    return path


def init_worktree(path: Path, *, origin: Path | None = None) -> Path:
    path.mkdir(parents=True, exist_ok=True)
    _git("init", "-b", "main", cwd=path)
    _git("config", "user.email", "release-bot@example.com", cwd=path)
    _git("config", "user.name", "Release Bot", cwd=path)
    if origin is not None:
        _git("remote", "add", "origin", str(origin), cwd=path)
    return path


def write_variables(path: Path, *, skiasharp_version: str, preview_label: str) -> None:
    (path / "scripts").mkdir(parents=True, exist_ok=True)
    (path / "scripts" / "azure-templates-variables.yml").write_text(
        "variables:\n"
        f"  SKIASHARP_VERSION: {skiasharp_version}\n"
        f"  PREVIEW_LABEL: '{preview_label}'\n",
        encoding="utf-8",
    )


def write_versions(path: Path, *, skiasharp_version: str, harfbuzzsharp_version: str) -> None:
    (path / "scripts").mkdir(parents=True, exist_ok=True)
    (path / "scripts" / "VERSIONS.txt").write_text(
        "# nuget versions\n"
        f"SkiaSharp                                       nuget       {skiasharp_version}\n"
        f"SkiaSharp.HarfBuzz                               nuget       {skiasharp_version}\n"
        "# HarfBuzzSharp\n"
        f"HarfBuzzSharp                                   nuget       {harfbuzzsharp_version}\n"
        "SkiaSharp               assembly    "
        + ".".join(skiasharp_version.split(".")[:2])
        + ".0.0\n"
        "SkiaSharp               file        " + skiasharp_version + "\n"
        "HarfBuzzSharp           assembly    1.0.0.0\n"
        "HarfBuzzSharp           file        " + harfbuzzsharp_version + "\n",
        encoding="utf-8",
    )


def stage(path: Path, *files: str) -> None:
    _git("add", "--", *files, cwd=path)


def commit_staged(path: Path, message: str) -> str:
    """Commit whatever is already staged, without an ``add -A`` pass.

    Unlike :func:`commit_all`, this never re-scans the working tree, so a
    gitlink staged with ``git update-index --cacheinfo`` (which has no
    corresponding path on disk) is not silently dropped by ``add -A``.
    """

    _git("commit", "-m", message, cwd=path)
    return _git("rev-parse", "HEAD", cwd=path).stdout.strip()


def commit_all(path: Path, message: str) -> str:
    _git("add", "-A", cwd=path)
    _git("commit", "-m", message, cwd=path)
    return _git("rev-parse", "HEAD", cwd=path).stdout.strip()


def add_gitlink(path: Path, *, submodule_path: str, sha: str, url: str = "https://example.invalid/skia.git") -> None:
    """Record a submodule gitlink entry without cloning anything."""

    _git(
        "update-index", "--add", "--cacheinfo", f"160000,{sha},{submodule_path}",
        cwd=path,
    )
    gitmodules = path / ".gitmodules"
    gitmodules.write_text(
        f'[submodule "{submodule_path}"]\n\tpath = {submodule_path}\n\turl = {url}\n',
        encoding="utf-8",
    )
    _git("add", ".gitmodules", cwd=path)


def push(path: Path, branch: str = "main") -> None:
    _git("push", "-u", "origin", branch, cwd=path)


def create_bare_and_worktree(root: Path, name: str) -> tuple[Path, Path]:
    bare = init_bare(root / f"{name}-origin.git")
    worktree = init_worktree(root / name, origin=bare)
    return bare, worktree
