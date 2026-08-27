from __future__ import annotations

import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import release_milestones as milestones


class ShippedTagTests(unittest.TestCase):
    def test_shipped_when_exact_tag_present(self):
        self.assertEqual(milestones.shipped_tag("3.119.0", ["v3.119.0", "v3.118.0"]), "v3.119.0")

    def test_not_shipped_when_tag_absent(self):
        self.assertIsNone(milestones.shipped_tag("3.119.0", ["v3.118.0"]))


class PlanCloseoutTests(unittest.TestCase):
    def test_shipped_open_milestone_moves_items_and_closes(self):
        all_milestones = [
            milestones.Milestone(number=1, title="3.119.0", state="open"),
            milestones.Milestone(number=2, title="3.119.1", state="open"),
        ]
        tags = ["v3.119.0"]
        open_items = {1: [milestones.MilestoneItem(number=42, title="fix", url="u", kind="issue")]}
        operations, warnings = milestones.plan_closeout(
            milestones=all_milestones, tags=tags, open_items_for=lambda n: open_items.get(n, [])
        )
        self.assertEqual(len(operations), 1)
        op = operations[0]
        self.assertEqual(op.milestone_title, "3.119.0")
        self.assertEqual(op.status, "pending")
        self.assertEqual(op.move_to_title, "3.119.1")
        self.assertEqual(warnings, [])

    def test_already_closed_milestone_is_not_replanned(self):
        all_milestones = [
            milestones.Milestone(number=1, title="3.119.0", state="closed"),
            milestones.Milestone(number=2, title="3.119.1", state="open"),
        ]
        tags = ["v3.119.0"]
        operations, warnings = milestones.plan_closeout(
            milestones=all_milestones, tags=tags, open_items_for=lambda n: []
        )
        self.assertEqual(operations, [])
        self.assertEqual(warnings, [])

    def test_reapplying_after_close_is_idempotent(self):
        """Simulates rerunning closeout after a successful apply."""

        all_milestones = [
            milestones.Milestone(number=1, title="3.119.0", state="open"),
            milestones.Milestone(number=2, title="3.119.1", state="open"),
        ]
        tags = ["v3.119.0"]
        open_items = {1: []}
        first_operations, _ = milestones.plan_closeout(
            milestones=all_milestones, tags=tags, open_items_for=lambda n: open_items.get(n, [])
        )
        self.assertEqual(len(first_operations), 1)
        self.assertEqual(first_operations[0].status, "pending")

        # Now simulate the milestone actually being closed.
        closed_milestones = [
            milestones.Milestone(number=1, title="3.119.0", state="closed"),
            milestones.Milestone(number=2, title="3.119.1", state="open"),
        ]
        second_operations, _ = milestones.plan_closeout(
            milestones=closed_milestones, tags=tags, open_items_for=lambda n: open_items.get(n, [])
        )
        self.assertEqual(second_operations, [])

    def test_blocked_when_no_target_milestone_available(self):
        all_milestones = [milestones.Milestone(number=1, title="3.119.0", state="open")]
        tags = ["v3.119.0"]
        open_items = {1: [milestones.MilestoneItem(number=7, title="fix", url="u", kind="issue")]}
        operations, warnings = milestones.plan_closeout(
            milestones=all_milestones, tags=tags, open_items_for=lambda n: open_items.get(n, [])
        )
        self.assertEqual(len(operations), 1)
        self.assertEqual(operations[0].status, "blocked")
        self.assertEqual(len(warnings), 1)

    def test_skips_target_that_is_itself_shipped(self):
        all_milestones = [
            milestones.Milestone(number=1, title="3.119.0", state="open"),
            milestones.Milestone(number=2, title="3.119.1", state="open"),
            milestones.Milestone(number=3, title="3.119.2", state="open"),
        ]
        tags = ["v3.119.0", "v3.119.1"]  # 3.119.1 already shipped too
        operations, _ = milestones.plan_closeout(
            milestones=all_milestones, tags=tags, open_items_for=lambda n: []
        )
        # Both shipped-open milestones get a closure operation; 3.119.0 must
        # skip past the already-shipped 3.119.1 to land on 3.119.2.
        by_title = {op.milestone_title: op for op in operations}
        self.assertEqual(by_title["3.119.0"].move_to_title, "3.119.2")
        self.assertEqual(by_title["3.119.1"].move_to_title, "3.119.2")


class ApplyCloseoutTests(unittest.TestCase):
    def test_apply_moves_items_then_closes_milestone(self):
        class FakeClient:
            def __init__(self):
                self.moved = []
                self.closed = []
                self._remaining = {1: []}

            def open_milestone_items(self, number):
                return self._remaining.get(number, [])

            def update_item_milestone(self, item_number, milestone_number):
                self.moved.append((item_number, milestone_number))

            def close_milestone(self, number):
                self.closed.append(number)

        client = FakeClient()
        operation = milestones.ClosureOperation(
            milestone_title="3.119.0", milestone_number=1, tag="v3.119.0", status="pending",
            open_items=(milestones.MilestoneItem(number=9, title="x", url="u", kind="issue"),),
            move_to_title="3.119.1", move_to_number=2,
        )
        results = milestones.apply_closeout([operation], client)
        self.assertEqual(client.moved, [(9, 2)])
        self.assertEqual(client.closed, [1])
        self.assertEqual(results[0]["status"], "done")

    def test_apply_raises_if_items_remain_after_move(self):
        class StuckClient:
            def open_milestone_items(self, number):
                return [milestones.MilestoneItem(number=9, title="x", url="u", kind="issue")]

            def update_item_milestone(self, item_number, milestone_number):
                pass

            def close_milestone(self, number):
                raise AssertionError("must not close while items remain")

        operation = milestones.ClosureOperation(
            milestone_title="3.119.0", milestone_number=1, tag="v3.119.0", status="pending",
            open_items=(milestones.MilestoneItem(number=9, title="x", url="u", kind="issue"),),
            move_to_title="3.119.1", move_to_number=2,
        )
        with self.assertRaises(milestones.MilestoneError):
            milestones.apply_closeout([operation], StuckClient())


class ReconcileTests(unittest.TestCase):
    def test_extracts_pr_numbers_from_commit_subjects(self):
        subjects = ["Fix the thing (#123)", "Not a PR commit", "Another fix (#456)"]
        self.assertEqual(milestones.extract_merged_pull_requests(subjects), [123, 456])

    def test_plan_reconcile_moves_pr_and_linked_issue(self):
        target = milestones.Milestone(number=5, title="3.119.0", state="open")
        operations = milestones.plan_reconcile(
            pull_request_numbers=[100],
            target_milestone=target,
            get_pull_request_milestone=lambda n: None,
            get_closing_issues=lambda n: [200],
            get_issue_milestone=lambda n: None,
        )
        kinds = {(op.kind, op.number) for op in operations}
        self.assertEqual(kinds, {("pull-request", 100), ("issue", 200)})

    def test_plan_reconcile_skips_already_correct_assignment(self):
        target = milestones.Milestone(number=5, title="3.119.0", state="open")
        operations = milestones.plan_reconcile(
            pull_request_numbers=[100],
            target_milestone=target,
            get_pull_request_milestone=lambda n: "3.119.0",
            get_closing_issues=lambda n: [200],
            get_issue_milestone=lambda n: "3.119.0",
        )
        self.assertEqual(operations, [])

    def test_apply_reconcile_updates_each_operation(self):
        class FakeClient:
            def __init__(self):
                self.calls = []

            def update_item_milestone(self, item_number, milestone_number):
                self.calls.append((item_number, milestone_number))

        client = FakeClient()
        ops = [
            milestones.ReconcileOperation(
                kind="pull-request", number=100, via_pull_request=None, from_milestone=None,
                to_milestone="3.119.0", to_milestone_number=5,
            )
        ]
        milestones.apply_reconcile(ops, client)
        self.assertEqual(client.calls, [(100, 5)])


if __name__ == "__main__":
    unittest.main()
