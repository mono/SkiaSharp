#!/usr/bin/env python3
"""Restore Git execute bits lost when gh-aw transports trusted config artifacts."""

from contextlib import ExitStack
import os
from pathlib import Path
import stat
import subprocess


# Match the pinned runtime's GH_AW_AGENT_FOLDERS / GH_AW_AGENT_FILES.
RESTORED_PATHS = (".agents", ".github", "AGENTS.md")


def restore_modes(workspace, snapshot_sha):
    entries = subprocess.check_output([
        "git", "-C", str(workspace), "ls-tree", "-rz", "--full-tree",
        snapshot_sha, "--", *RESTORED_PATHS,
    ])
    with ExitStack() as root_stack:
        root = os.open(workspace, os.O_RDONLY | os.O_DIRECTORY | os.O_NOFOLLOW)
        root_stack.callback(os.close, root)
        for entry in entries.split(b"\0"):
            if not entry:
                continue
            metadata, path = entry.split(b"\t", 1)
            mode = metadata.split(b" ", 1)[0]
            if mode not in (b"100644", b"100755"):
                continue
            # Open each component relative to a verified directory descriptor:
            # neither the file nor a parent symlink may escape the restored tree.
            with ExitStack() as stack:
                parent = root
                parts = path.split(b"/")
                for part in parts[:-1]:
                    parent = os.open(
                        part, os.O_RDONLY | os.O_DIRECTORY | os.O_NOFOLLOW,
                        dir_fd=parent,
                    )
                    stack.callback(os.close, parent)
                descriptor = os.open(
                    parts[-1], os.O_RDONLY | os.O_NOFOLLOW | os.O_NONBLOCK,
                    dir_fd=parent,
                )
                stack.callback(os.close, descriptor)
                current = os.fstat(descriptor).st_mode
                if not stat.S_ISREG(current):
                    raise ValueError(f"Expected a regular restored file: {os.fsdecode(path)}")
                expected = 0o111 if mode == b"100755" else 0
                permissions = (stat.S_IMODE(current) & ~0o111) | expected
                if permissions != stat.S_IMODE(current):
                    os.fchmod(descriptor, permissions)


if __name__ == "__main__":
    restore_modes(Path(os.environ["GITHUB_WORKSPACE"]), os.environ["GITHUB_SHA"])
