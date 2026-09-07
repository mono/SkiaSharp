#!/usr/bin/env python3
"""Regression tests for Pages staging workflow safeguards."""

from __future__ import annotations

from pathlib import Path
import unittest

import yaml


REPO_ROOT = Path(__file__).resolve().parents[3]
WORKFLOW_DIR = REPO_ROOT / ".github" / "workflows"


def load_workflow(name: str):
    return yaml.load(
        (WORKFLOW_DIR / name).read_text(encoding="utf-8"),
        Loader=yaml.BaseLoader,
    )


class SiteWorkflowTests(unittest.TestCase):
    def test_staging_build_requires_same_repository_opt_in_label(self):
        workflow = load_workflow("build-site.yml")
        self.assertIn("labeled", workflow["on"]["pull_request"]["types"])
        self.assertNotIn("paths-ignore", workflow["on"]["pull_request"])

        build = workflow["jobs"]["build"]
        self.assertIn(
            "github.event.pull_request.head.repo.full_name == github.repository",
            build["if"],
        )
        self.assertIn(
            "contains(github.event.pull_request.labels.*.name, 'automation/staging')",
            build["if"],
        )
        self.assertEqual("read", build["permissions"]["pull-requests"])

        setup = build["steps"][0]["run"]
        self.assertIn("--json headRepository,labels", setup)
        self.assertIn('any(.labels[].name; . == "automation/staging")', setup)
        self.assertIn("^[1-9][0-9]*$", setup)

    def test_removing_opt_in_label_uses_existing_safe_cleanup(self):
        workflow = load_workflow("build-site-cleanup.yml")
        self.assertIn("unlabeled", workflow["on"]["pull_request"]["types"])

        cleanup = workflow["jobs"]["cleanup"]
        self.assertIn(
            "github.event.pull_request.head.repo.full_name == github.repository",
            cleanup["if"],
        )
        self.assertIn("github.event.action == 'closed'", cleanup["if"])
        self.assertIn("github.event.label.name == 'automation/staging'", cleanup["if"])

        remove_staging = cleanup["steps"][1]["run"]
        self.assertIn("^[1-9][0-9]*$", remove_staging)
        self.assertIn("git push --force-with-lease origin docs-live", remove_staging)

    def test_public_pages_artifact_excludes_staging(self):
        workflow = load_workflow("build-site-go-live.yml")
        steps = workflow["jobs"]["deploy"]["steps"]
        prepare = next(
            step for step in steps if step["name"] == "Prepare public Pages artifact"
        )
        self.assertIn("-not -name 'staging'", prepare["run"])
        self.assertEqual(
            "/tmp/pages-public",
            next(step for step in steps if step["name"] == "Upload artifact")["with"]["path"],
        )


if __name__ == "__main__":
    unittest.main()
