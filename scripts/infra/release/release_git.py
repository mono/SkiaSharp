"""Git repository access shared by prepare planning/apply.

Everything here operates on an actual on-disk Git working copy through
plain ``git`` argv invocations -- never shell strings, never network calls
beyond ``git fetch``/``git push``/``git ls-remote``. Unit tests exercise this
module against real temporary repositories created with ``git init``, so the
behaviour under test is the same code path used in production.
"""

from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path

from release_common import CommandResult, CommandRunner, DEFAULT_RUNNER, ReleaseToolError


class GitError(ReleaseToolError):
    """A Git operation failed or returned an unexpected result."""


@dataclass
class GitRepository:
    """A single Git working copy (either the SkiaSharp checkout or the
    ``externals/skia`` submodule checkout)."""

    root: Path
    runner: CommandRunner = DEFAULT_RUNNER

    @classmethod
    def discover(cls, start: Path, *, runner: CommandRunner = DEFAULT_RUNNER) -> "GitRepository":
        result = runner.run(["git", "rev-parse", "--show-toplevel"], cwd=start)
        return cls(root=Path(result.stdout.strip()), runner=runner)

    def git(self, *args: str, check: bool = True, timeout: int = 120) -> CommandResult:
        return self.runner.run(["git", *args], cwd=self.root, check=check, timeout=timeout)

    def fetch(self, remote: str = "origin") -> None:
        self.git("fetch", remote, "--prune", "--tags")

    def ref_exists(self, ref: str) -> bool:
        return self.git("show-ref", "--verify", "--quiet", ref, check=False).ok

    def resolve(self, ref: str) -> str:
        result = self.git("rev-parse", "--verify", f"{ref}^{{commit}}")
        return result.stdout.strip()

    def read_ref_file(self, ref: str, path: str) -> str:
        return self.git("show", f"{ref}:{path}").stdout

    def read_gitlink(self, ref: str, submodule_path: str) -> str:
        """Return the commit SHA a gitlink (submodule pointer) records at ``ref``."""

        result = self.git("ls-tree", ref, "--", submodule_path)
        line = result.stdout.strip()
        if not line:
            raise GitError(f"{submodule_path} is not a gitlink at {ref}")
        # "<mode> commit <sha>\t<path>"
        fields = line.split()
        if len(fields) < 3 or fields[1] != "commit":
            raise GitError(f"{submodule_path} at {ref} is not a submodule gitlink")
        return fields[2]

    def remote_sha(self, branch: str, *, remote: str = "origin") -> str | None:
        result = self.git("ls-remote", "--heads", remote, f"refs/heads/{branch}")
        line = result.stdout.strip()
        if not line:
            return None
        return line.split()[0]

    def remote_tags(self, *, remote: str = "origin", pattern: str = "refs/tags/*") -> dict[str, str]:
        result = self.git("ls-remote", "--tags", remote, pattern)
        tags: dict[str, str] = {}
        for line in result.stdout.splitlines():
            if not line.strip():
                continue
            sha, ref = line.split(None, 1)
            ref = ref.strip()
            if ref.endswith("^{}"):
                ref = ref[: -len("^{}")]
                tags[ref[len("refs/tags/"):]] = sha
            elif ref[len("refs/tags/"):] not in tags:
                tags[ref[len("refs/tags/"):]] = sha
        return tags

    def release_branches(self, *, remote: str = "origin") -> list[str]:
        result = self.git(
            "for-each-ref",
            "--format=%(refname:strip=3)",
            f"refs/remotes/{remote}/release/",
        )
        return [line for line in result.stdout.splitlines() if line]

    def merge_base(self, a: str, b: str) -> str:
        return self.git("merge-base", a, b).stdout.strip()

    def is_ancestor(self, ancestor: str, descendant: str) -> bool:
        return self.git("merge-base", "--is-ancestor", ancestor, descendant, check=False).ok

    def require_clean(self) -> None:
        status = self.git("status", "--porcelain", "--ignore-submodules").stdout
        if status.strip():
            raise GitError(f"working tree at {self.root} is not clean:\n{status}")

    def current_branch(self) -> str:
        return self.git("rev-parse", "--abbrev-ref", "HEAD").stdout.strip()

    def create_branch(self, branch: str, start_point: str) -> None:
        self.git("branch", branch, start_point)

    def switch(self, branch: str) -> None:
        self.git("switch", branch)

    def switch_create(self, branch: str, start_point: str) -> None:
        self.git("switch", "-c", branch, start_point)

    def commit(self, message: str, *, paths: list[str] | None = None) -> str:
        if paths:
            self.git("add", "--", *paths)
        self.git("commit", "-m", message)
        return self.resolve("HEAD")

    def push_branch(self, branch: str, *, remote: str = "origin", set_upstream: bool = True) -> None:
        args = ["push"]
        if set_upstream:
            args.append("-u")
        args.extend([remote, branch])
        self.git(*args)

    def push_tag(self, tag: str, sha: str, *, remote: str = "origin") -> None:
        self.git("push", remote, f"{sha}:refs/tags/{tag}")

    def contains_commit(self, branch_ref: str, commit: str) -> bool:
        """True if ``commit`` is reachable from ``branch_ref`` (e.g. an
        ``origin/release/...`` ref)."""

        return self.is_ancestor(commit, branch_ref)
