#!/usr/bin/env python3
"""Regression tests for release-note pull-request safe-output tooling."""

from __future__ import annotations

from pathlib import Path
import unittest

import yaml


REPO_ROOT = Path(__file__).resolve().parents[3]
WORKFLOW_DIR = REPO_ROOT / ".github" / "workflows"
SOURCE = WORKFLOW_DIR / "update-release-notes.md"
LOCK = WORKFLOW_DIR / "update-release-notes.lock.yml"
PR_REQUEST_TOOLS = ("mkdir", "jq")


def load_frontmatter():
    lines = SOURCE.read_text(encoding="utf-8").splitlines()
    if not lines or lines[0] != "---":
        raise AssertionError("update-release-notes.md has malformed YAML frontmatter")
    try:
        closing_delimiter = lines.index("---", 1)
    except ValueError as error:
        raise AssertionError(
            "update-release-notes.md has malformed YAML frontmatter"
        ) from error
    return yaml.safe_load("\n".join(lines[1:closing_delimiter]))


class UpdateReleaseNotesWorkflowTests(unittest.TestCase):
    def test_pr_request_helpers_are_allowlisted(self):
        bash_tools = load_frontmatter()["tools"]["bash"]

        for tool in PR_REQUEST_TOOLS:
            with self.subTest(tool=tool):
                self.assertIn(tool, bash_tools)

    def test_compiled_agent_allows_pr_request_helpers(self):
        lock = LOCK.read_text(encoding="utf-8")

        for tool in PR_REQUEST_TOOLS:
            with self.subTest(tool=tool):
                self.assertIn(f"# --allow-tool shell({tool})", lock)


if __name__ == "__main__":
    unittest.main()
