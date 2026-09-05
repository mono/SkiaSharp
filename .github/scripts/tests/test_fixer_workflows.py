#!/usr/bin/env python3
"""Regression tests for fail-closed fixer workflow dry runs."""

from __future__ import annotations

from pathlib import Path
import unittest

import yaml


REPO_ROOT = Path(__file__).resolve().parents[3]
WORKFLOW_DIR = REPO_ROOT / ".github" / "workflows"
FIXER_WORKFLOWS = ("memory-leak-fixer", "performance-fixer")
STAGED_EXPRESSION = (
    "${{ github.event_name == 'pull_request' || "
    "(github.event_name == 'workflow_dispatch' && inputs.dry_run) }}"
)


def load_frontmatter(workflow: str):
    text = (WORKFLOW_DIR / f"{workflow}.md").read_text(encoding="utf-8")
    lines = text.splitlines()
    if not lines or lines[0] != "---":
        raise AssertionError(f"{workflow}.md has malformed YAML frontmatter")
    try:
        closing_delimiter = lines.index("---", 1)
    except ValueError as error:
        raise AssertionError(
            f"{workflow}.md has malformed YAML frontmatter"
        ) from error
    return yaml.safe_load("\n".join(lines[1:closing_delimiter]))


def load_lock(workflow: str):
    with (WORKFLOW_DIR / f"{workflow}.lock.yml").open(encoding="utf-8") as stream:
        return yaml.safe_load(stream)


def find_mapping_values(value, key):
    found = []
    if isinstance(value, dict):
        for child_key, child_value in value.items():
            if child_key == key:
                found.append(child_value)
            found.extend(find_mapping_values(child_value, key))
    elif isinstance(value, list):
        for child in value:
            found.extend(find_mapping_values(child, key))
    return found


class FixerWorkflowSafetyTests(unittest.TestCase):
    def test_source_stages_all_dry_run_triggers(self):
        for workflow in FIXER_WORKFLOWS:
            with self.subTest(workflow=workflow):
                safe_outputs = load_frontmatter(workflow)["safe-outputs"]
                self.assertEqual(STAGED_EXPRESSION, safe_outputs["staged"])
                self.assertIn("create-issue", safe_outputs)
                self.assertIn("create-pull-request", safe_outputs)

    def test_compiled_jobs_receive_dynamic_staged_mode(self):
        for workflow in FIXER_WORKFLOWS:
            with self.subTest(workflow=workflow):
                lock = load_lock(workflow)
                staged_values = find_mapping_values(
                    lock["jobs"], "GH_AW_SAFE_OUTPUTS_STAGED"
                )
                self.assertEqual(
                    [STAGED_EXPRESSION] * 3,
                    staged_values,
                    "The agent tool and privileged handler must share the guard.",
                )
                self.assertEqual(
                    STAGED_EXPRESSION,
                    find_mapping_values(lock["jobs"], "GH_AW_INFO_STAGED")[0],
                )

    def test_compilation_preserves_disabled_failure_issue_expiry(self):
        for workflow in FIXER_WORKFLOWS:
            with self.subTest(workflow=workflow):
                lock = load_lock(workflow)
                self.assertEqual(
                    ["0"],
                    find_mapping_values(
                        lock["jobs"], "GH_AW_ACTION_FAILURE_ISSUE_EXPIRES_HOURS"
                    ),
                )


if __name__ == "__main__":
    unittest.main()
