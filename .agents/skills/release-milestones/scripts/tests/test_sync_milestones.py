#!/usr/bin/env python3

import importlib.util
from pathlib import Path
import sys
from types import SimpleNamespace
import unittest
from unittest import mock


SCRIPTS = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(SCRIPTS))
SCRIPT_PATH = SCRIPTS / "sync-milestones.py"
SPEC = importlib.util.spec_from_file_location("sync_milestones", SCRIPT_PATH)
sync = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = sync
SPEC.loader.exec_module(sync)


class SyncMilestonesTests(unittest.TestCase):
    def test_schedule_maps_to_release_cadence(self):
        schedule = {
            "branch_point": "2026-07-27T00:00:00Z",
            "earliest_beta": "2026-08-04T00:00:00Z",
            "early_stable_cut": "2026-08-11T00:00:00Z",
            "early_stable": "2026-08-12T00:00:00Z",
            "stable_cut": "2026-08-18T00:00:00Z",
            "stable_date": "2026-08-25T00:00:00Z",
        }
        milestones = sync.desired_milestones(
            schedule,
            milestone=152,
            major=4,
        )
        self.assertEqual(
            [item.title for item in milestones],
            [
                "4.152.0-preview.1",
                "4.152.0-preview.2",
                "4.152.0-rc.1",
                "4.152.0",
            ],
        )
        self.assertEqual(
            [item.due.isoformat() for item in milestones],
            [
                "2026-08-04",
                "2026-08-12",
                "2026-08-18",
                "2026-08-25",
            ],
        )
        self.assertIn("\u00b7", milestones[0].description)

    def test_default_executes_and_dry_run_is_explicit(self):
        parser = sync.create_parser()
        defaults = parser.parse_args([])
        self.assertEqual(defaults.count, 3)
        self.assertFalse(defaults.dry_run)
        self.assertTrue(parser.parse_args(["--dry-run"]).dry_run)

    def test_execution_command_drops_dry_run(self):
        args = sync.create_parser().parse_args(
            ["--count", "3", "--repo", "mono/SkiaSharp", "--dry-run"]
        )
        command = sync.execution_command(args)
        self.assertNotIn("--dry-run", command)
        self.assertIn("--count 3", command)

    def test_tag_detection_matches_release_milestone(self):
        tags = [
            "v4.152.0-preview.1.10",
            "v4.152.0-preview.1.2",
            "v4.152.0-preview.1.invalid",
            "v4.152.0-preview.2.1",
            "v4.152.0",
        ]
        self.assertEqual(
            sync.common.shipped_tag("4.152.0-preview.1", tags),
            "v4.152.0-preview.1.10",
        )
        self.assertEqual(
            sync.common.shipped_tag("4.152.0", tags),
            "v4.152.0",
        )
        self.assertIsNone(
            sync.common.shipped_tag("4.152.0-rc.1", tags),
        )

    def test_tagged_preview_moves_issues_to_next_unshipped_preview(self):
        milestones = [
            sync.ReleaseMilestone.parse("4.152.0-preview.1"),
            sync.ReleaseMilestone.parse("4.152.0-preview.2"),
            sync.ReleaseMilestone.parse("4.152.0-rc.1"),
            sync.ReleaseMilestone.parse("4.152.0"),
        ]
        existing = {
            "4.152.0-preview.1": {"number": 1, "state": "open"},
            "4.152.0-preview.2": {"number": 2, "state": "open"},
        }

        operations, warnings = sync.plan_closures(
            existing,
            milestones,
            ["v4.152.0-preview.1.2"],
            lambda number: [
                {
                    "number": 99,
                    "title": "Bug",
                    "url": "url",
                    "kind": "issue",
                }
            ],
        )

        self.assertEqual(warnings, [])
        self.assertEqual(len(operations), 1)
        self.assertEqual(operations[0]["title"], "4.152.0-preview.1")
        self.assertEqual(operations[0]["moveTo"], "4.152.0-preview.2")
        self.assertEqual(operations[0]["status"], "pending")

    def test_tagged_stable_moves_issues_to_next_release_preview(self):
        milestones = [
            sync.ReleaseMilestone.parse("4.152.0"),
            sync.ReleaseMilestone.parse("4.153.0-preview.1"),
            sync.ReleaseMilestone.parse("4.153.0-preview.2"),
        ]
        existing = {
            "4.152.0": {"number": 4, "state": "open"},
            "4.153.0-preview.1": {"number": 5, "state": "open"},
        }

        operations, warnings = sync.plan_closures(
            existing,
            milestones,
            ["v4.152.0"],
            lambda number: [
                {
                    "number": 100,
                    "title": "Bug",
                    "url": "url",
                    "kind": "issue",
                }
            ],
        )

        self.assertEqual(warnings, [])
        self.assertEqual(operations[0]["moveTo"], "4.153.0-preview.1")

    def test_rollover_can_target_milestone_created_by_same_sync(self):
        milestones = [
            sync.ReleaseMilestone.parse("4.152.0"),
            sync.ReleaseMilestone.parse("4.153.0-preview.1"),
        ]

        operations, warnings = sync.plan_closures(
            {"4.152.0": {"number": 4, "state": "open"}},
            milestones,
            ["v4.152.0"],
            lambda number: [
                {
                    "number": 100,
                    "title": "Bug",
                    "url": "url",
                    "kind": "issue",
                }
            ],
            creatable_titles={"4.153.0-preview.1"},
        )

        self.assertEqual(warnings, [])
        self.assertEqual(operations[0]["moveTo"], "4.153.0-preview.1")

    def test_tagged_milestone_with_issues_requires_future_milestone(self):
        milestones = [
            sync.ReleaseMilestone.parse("4.152.0"),
            sync.ReleaseMilestone.parse("4.153.0-preview.1"),
        ]

        operations, warnings = sync.plan_closures(
            {"4.152.0": {"number": 4, "state": "open"}},
            milestones,
            ["v4.152.0"],
            lambda number: [
                {
                    "number": 100,
                    "title": "Bug",
                    "url": "url",
                    "kind": "issue",
                }
            ],
        )

        self.assertEqual(operations[0]["status"], "blocked")
        self.assertIsNone(operations[0]["moveTo"])
        self.assertEqual(len(warnings), 1)
        self.assertIn("no future milestone", warnings[0])

    def test_tagged_empty_milestone_closes_without_future_milestone(self):
        milestone = sync.ReleaseMilestone.parse("4.152.0")

        operations, warnings = sync.plan_closures(
            {"4.152.0": {"number": 4, "state": "open"}},
            [milestone],
            ["v4.152.0"],
            lambda number: [],
        )

        self.assertEqual(warnings, [])
        self.assertEqual(operations[0]["status"], "pending")
        self.assertIsNone(operations[0]["moveTo"])

    def test_execution_moves_open_issues_then_closes(self):
        events = []

        class FakeGitHub:
            def __init__(self, repo):
                self.repo = repo

            def milestone_map(self):
                return {
                    "4.152.0-preview.2": {
                        "number": 2,
                        "state": "open",
                    }
                }

            def update_issue_milestone(self, issue, milestone):
                events.append(("move", issue, milestone))

            def open_milestone_items(self, number):
                return []

            def close_milestone(self, number):
                events.append(("close", number))

        plan = {
            "schedule": [],
            "closures": [
                {
                    "title": "4.152.0-preview.1",
                    "number": 1,
                    "tag": "v4.152.0-preview.1.2",
                    "status": "pending",
                    "openItems": [
                        {"number": 99, "kind": "issue"},
                        {"number": 100, "kind": "pull-request"},
                    ],
                    "moveTo": "4.152.0-preview.2",
                }
            ],
        }
        with mock.patch.object(sync.common, "GitHub", FakeGitHub):
            sync.execute(
                SimpleNamespace(repo="mono/SkiaSharp"),
                plan,
            )
        self.assertEqual(
            events,
            [("move", 99, 2), ("move", 100, 2), ("close", 1)],
        )

    def test_scripts_are_ascii_only(self):
        SCRIPT_PATH.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
