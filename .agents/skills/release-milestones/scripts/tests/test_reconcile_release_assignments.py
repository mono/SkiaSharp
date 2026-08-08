#!/usr/bin/env python3

import importlib.util
from pathlib import Path
import sys
from types import SimpleNamespace
import unittest


SCRIPTS = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(SCRIPTS))
SCRIPT_PATH = SCRIPTS / "reconcile-release-assignments.py"
SPEC = importlib.util.spec_from_file_location(
    "reconcile_release_assignments",
    SCRIPT_PATH,
)
reconcile = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = reconcile
SPEC.loader.exec_module(reconcile)


class ReconcileReleaseAssignmentsTests(unittest.TestCase):
    def test_release_branches_sort_in_shipping_order(self):
        branches = [
            reconcile.ReleaseBranch.parse("release/4.152.0"),
            reconcile.ReleaseBranch.parse("release/4.152.0-rc.1"),
            reconcile.ReleaseBranch.parse("release/4.152.0-preview.2"),
            reconcile.ReleaseBranch.parse("release/4.152.0.1-preview.1"),
            reconcile.ReleaseBranch.parse("release/4.152.0.1"),
        ]
        ordered = sorted(branches, key=lambda item: item.sort_key)
        self.assertEqual(
            [item.title for item in ordered],
            [
                "4.152.0-preview.2",
                "4.152.0-rc.1",
                "4.152.0",
                "4.152.0.1-preview.1",
                "4.152.0.1",
            ],
        )

    def test_unshipped_release_rolls_forward(self):
        branches = [
            reconcile.ReleaseBranch.parse("release/4.152.0-preview.1"),
            reconcile.ReleaseBranch.parse("release/4.152.0-preview.2"),
            reconcile.ReleaseBranch.parse("release/4.152.0-rc.1"),
        ]
        tags = [
            "v4.152.0-preview.2.1",
            "v4.152.0-rc.1.1",
        ]
        self.assertEqual(
            reconcile.effective_titles(branches, tags),
            [
                "4.152.0-preview.2",
                "4.152.0-preview.2",
                "4.152.0-rc.1",
            ],
        )

    def test_previous_boundary_prefers_latest_stable(self):
        branches = [
            reconcile.ReleaseBranch.parse("release/4.150.0"),
            reconcile.ReleaseBranch.parse("release/4.150.1"),
            reconcile.ReleaseBranch.parse("release/4.151.0-preview.1"),
        ]
        previous = reconcile.previous_stable_branch(branches, "4.152.0")
        self.assertEqual(previous.name, "release/4.150.1")

    def test_closing_keywords_are_detected(self):
        body = "Fixes #12, resolves: #34 and closed #56"
        self.assertEqual(
            [
                int(match.group(1))
                for match in reconcile.CLOSING_RE.finditer(body)
            ],
            [12, 34, 56],
        )

    def test_execution_command_runs_assignment_reconciliation(self):
        args = SimpleNamespace(
            repo="mono/SkiaSharp",
        )
        command = reconcile.execution_command(args, "4.152.0")
        self.assertIn("reconcile-release-assignments.py", command)
        self.assertIn("--version 4.152.0", command)
        self.assertNotIn("--dry-run", command)

    def test_default_executes_and_dry_run_is_explicit(self):
        parser = reconcile.create_parser()
        self.assertFalse(parser.parse_args([]).dry_run)
        self.assertTrue(parser.parse_args(["--dry-run"]).dry_run)

    def test_scripts_are_ascii_only(self):
        SCRIPT_PATH.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
