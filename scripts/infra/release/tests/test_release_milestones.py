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


class PlanCloseoutCreatableTargetTests(unittest.TestCase):
    """creatable_titles lets a read-only preview treat an upcoming schedule
    milestone that does not exist yet (but has a pending "create" schedule
    operation applied before this runs for real) as a valid move target."""

    def test_creatable_title_is_accepted_as_a_move_target(self):
        all_milestones = [milestones.Milestone(number=1, title="3.119.0", state="open")]
        tags = ["v3.119.0"]
        open_items = {1: [milestones.MilestoneItem(number=7, title="fix", url="u", kind="issue")]}
        operations, warnings = milestones.plan_closeout(
            milestones=all_milestones, tags=tags, open_items_for=lambda n: open_items.get(n, []),
            creatable_titles=frozenset({"3.119.1"}),
        )
        self.assertEqual(len(operations), 1)
        self.assertEqual(operations[0].status, "pending")
        self.assertEqual(operations[0].move_to_title, "3.119.1")
        self.assertIsNone(operations[0].move_to_number)  # not a real milestone yet
        self.assertEqual(warnings, [])

    def test_shipped_creatable_title_is_not_a_valid_target(self):
        all_milestones = [milestones.Milestone(number=1, title="3.119.0", state="open")]
        tags = ["v3.119.0", "v3.119.1"]  # 3.119.1 already shipped too
        open_items = {1: [milestones.MilestoneItem(number=7, title="fix", url="u", kind="issue")]}
        operations, warnings = milestones.plan_closeout(
            milestones=all_milestones, tags=tags, open_items_for=lambda n: open_items.get(n, []),
            creatable_titles=frozenset({"3.119.1"}),
        )
        self.assertEqual(operations[0].status, "blocked")
        self.assertEqual(len(warnings), 1)

    def test_existing_open_milestone_is_still_preferred_over_creatable(self):
        all_milestones = [
            milestones.Milestone(number=1, title="3.119.0", state="open"),
            milestones.Milestone(number=2, title="3.119.1", state="open"),
        ]
        tags = ["v3.119.0"]
        operations, _ = milestones.plan_closeout(
            milestones=all_milestones, tags=tags, open_items_for=lambda n: [],
            creatable_titles=frozenset({"3.119.2"}),
        )
        self.assertEqual(operations[0].move_to_title, "3.119.1")
        self.assertEqual(operations[0].move_to_number, 2)


class ParseCurrentMajorAndMilestoneTests(unittest.TestCase):
    def test_parses_major_and_milestone(self):
        text = (
            "# nuget versions\n"
            "# SkiaSharp\n"
            "SkiaSharp                nuget       4.152.0\n"
            "# HarfBuzzSharp\n"
            "HarfBuzzSharp            nuget       14.2.1\n"
            "libSkiaSharp             milestone   152\n"
        )
        major, current = milestones.parse_current_major_and_milestone(text)
        self.assertEqual(major, 4)
        self.assertEqual(current, 152)

    def test_rejects_missing_skiasharp_nuget_line(self):
        with self.assertRaises(milestones.MilestoneError):
            milestones.parse_current_major_and_milestone("libSkiaSharp    milestone   152\n")

    def test_rejects_missing_libskiasharp_milestone_line(self):
        with self.assertRaises(milestones.MilestoneError):
            milestones.parse_current_major_and_milestone("SkiaSharp    nuget   4.152.0\n")


class DesiredMilestonesTests(unittest.TestCase):
    """Ported wording/date arithmetic from the retired advance-release-
    milestones.py, verified against a real Chromium schedule shape (fetched
    live from chromiumdash for m152 during development)."""

    SCHEDULE = {
        "branch_point": "2026-07-27T00:00:00",
        "earliest_beta": "2026-07-29T00:00:00",
        "early_stable_cut": "2026-08-11T00:00:00",
        "early_stable": "2026-08-12T00:00:00",
        "stable_cut": "2026-08-18T00:00:00",
        "stable_date": "2026-08-25T00:00:00",
    }

    def test_computes_four_milestones_with_expected_titles_and_dates(self):
        items = milestones.desired_milestones(self.SCHEDULE, milestone=152, major=4)
        titles = [item.title for item in items]
        self.assertEqual(titles, ["4.152.0-preview.1", "4.152.0-preview.2", "4.152.0-rc.1", "4.152.0"])
        due_dates = [item.due.isoformat() for item in items]
        self.assertEqual(due_dates, ["2026-07-29", "2026-08-12", "2026-08-18", "2026-08-25"])

    def test_descriptions_mention_the_milestone_and_start_date(self):
        items = milestones.desired_milestones(self.SCHEDULE, milestone=152, major=4)
        stable = next(item for item in items if item.title == "4.152.0")
        self.assertIn("m152", stable.description)
        self.assertIn("stable", stable.description)
        self.assertIn("Aug 18, 2026", stable.description)

    def test_due_on_is_midnight_utc_iso8601(self):
        items = milestones.desired_milestones(self.SCHEDULE, milestone=152, major=4)
        self.assertEqual(items[0].due_on, "2026-07-29T00:00:00Z")


class HttpChromiumScheduleClientTests(unittest.TestCase):
    """Real local-HTTP-server transport tests (mirrors
    test_release_nuget.py's HttpNuGetClientRealTransportTests): a hand-built
    fake transport could trivially "pass" a broken implementation, so these
    run a genuine socket round trip through urllib.request."""

    def setUp(self):
        import http.server
        import threading

        class _Handler(http.server.BaseHTTPRequestHandler):
            routes: dict[str, tuple[int, bytes]] = {}

            def do_GET(self):  # noqa: N802
                status, body = self.routes.get(self.path, (404, b"{}"))
                self.send_response(status)
                self.send_header("Content-Type", "application/json")
                self.send_header("Content-Length", str(len(body)))
                self.end_headers()
                self.wfile.write(body)

            def log_message(self, format, *args):  # noqa: A002
                pass

        self.handler = type("_H", (_Handler,), {"routes": {}})
        self.httpd = http.server.ThreadingHTTPServer(("127.0.0.1", 0), self.handler)
        self.thread = threading.Thread(target=self.httpd.serve_forever, daemon=True)
        self.thread.start()
        self.client = milestones.HttpChromiumScheduleClient()
        self.client_url_base = f"http://127.0.0.1:{self.httpd.server_port}"

    def tearDown(self):
        self.httpd.shutdown()
        self.httpd.server_close()

    def _fetch(self, milestone: int) -> dict:
        # Point the module-level URL template at the local server for the
        # duration of this call, then restore it -- avoids depending on
        # network access or monkeypatching urlopen itself.
        original = milestones.CHROMIUM_SCHEDULE_URL
        milestones.CHROMIUM_SCHEDULE_URL = f"{self.client_url_base}/fetch_milestone_schedule?mstone={{milestone}}"
        try:
            return self.client.fetch_schedule(milestone)
        finally:
            milestones.CHROMIUM_SCHEDULE_URL = original

    def test_fetches_and_parses_a_real_response(self):
        import json

        payload = json.dumps(
            {
                "mstones": [
                    {
                        "branch_point": "2026-07-27T00:00:00",
                        "earliest_beta": "2026-07-29T00:00:00",
                        "early_stable_cut": "2026-08-11T00:00:00",
                        "early_stable": "2026-08-12T00:00:00",
                        "stable_cut": "2026-08-18T00:00:00",
                        "stable_date": "2026-08-25T00:00:00",
                    }
                ]
            }
        ).encode("utf-8")
        self.handler.routes["/fetch_milestone_schedule?mstone=152"] = (200, payload)
        schedule = self._fetch(152)
        self.assertEqual(schedule["stable_date"], "2026-08-25T00:00:00")

    def test_http_error_raises_milestone_error(self):
        self.handler.routes["/fetch_milestone_schedule?mstone=999"] = (503, b"{}")
        with self.assertRaises(milestones.MilestoneError):
            self._fetch(999)

    def test_empty_mstones_raises_milestone_error(self):
        import json

        self.handler.routes["/fetch_milestone_schedule?mstone=153"] = (
            200, json.dumps({"mstones": []}).encode("utf-8")
        )
        with self.assertRaisesRegex(milestones.MilestoneError, "no schedule"):
            self._fetch(153)

    def test_missing_required_field_raises_milestone_error(self):
        import json

        incomplete = {"mstones": [{"branch_point": "2026-07-27T00:00:00"}]}
        self.handler.routes["/fetch_milestone_schedule?mstone=154"] = (
            200, json.dumps(incomplete).encode("utf-8")
        )
        with self.assertRaisesRegex(milestones.MilestoneError, "missing"):
            self._fetch(154)

    def test_malformed_json_raises_milestone_error(self):
        self.handler.routes["/fetch_milestone_schedule?mstone=155"] = (200, b"not json at all")
        with self.assertRaisesRegex(milestones.MilestoneError, "not valid JSON"):
            self._fetch(155)


class PlanScheduleOperationsTests(unittest.TestCase):
    def test_creates_a_milestone_that_does_not_exist_yet(self):
        import datetime as dt

        desired = [milestones.DesiredMilestone("4.152.0-preview.1", dt.date(2026, 7, 29), "desc")]
        operations = milestones.plan_schedule_operations(desired, {}, today=dt.date(2026, 7, 1))
        self.assertEqual(len(operations), 1)
        self.assertEqual(operations[0].action, "create")
        self.assertEqual(operations[0].status, "pending")
        self.assertIsNone(operations[0].number)

    def test_updates_a_milestone_with_the_wrong_due_date(self):
        import datetime as dt

        desired = [milestones.DesiredMilestone("4.152.0-preview.1", dt.date(2026, 7, 29), "desc")]
        existing = {
            "4.152.0-preview.1": milestones.Milestone(
                number=9, title="4.152.0-preview.1", state="open",
                due_on="2026-07-01T00:00:00Z", description="desc",
            )
        }
        operations = milestones.plan_schedule_operations(desired, existing, today=dt.date(2026, 7, 1))
        self.assertEqual(operations[0].action, "update")
        self.assertEqual(operations[0].status, "pending")
        self.assertEqual(operations[0].number, 9)
        self.assertEqual(
            operations[0].changes,
            ({"field": "dueOn", "from": "2026-07-01", "to": "2026-07-29"},),
        )

    def test_updates_a_milestone_with_the_wrong_description(self):
        import datetime as dt

        desired = [milestones.DesiredMilestone("4.152.0-preview.1", dt.date(2026, 7, 29), "new desc")]
        existing = {
            "4.152.0-preview.1": milestones.Milestone(
                number=9, title="4.152.0-preview.1", state="open",
                due_on="2026-07-29T00:00:00Z", description="old desc",
            )
        }
        operations = milestones.plan_schedule_operations(desired, existing, today=dt.date(2026, 7, 1))
        self.assertEqual(operations[0].action, "update")
        self.assertEqual(
            operations[0].changes,
            ({"field": "description", "from": "old desc", "to": "new desc"},),
        )

    def test_matching_milestone_is_left_alone(self):
        import datetime as dt

        desired = [milestones.DesiredMilestone("4.152.0-preview.1", dt.date(2026, 7, 29), "desc")]
        existing = {
            "4.152.0-preview.1": milestones.Milestone(
                number=9, title="4.152.0-preview.1", state="open",
                due_on="2026-07-29T00:00:00Z", description="desc",
            )
        }
        operations = milestones.plan_schedule_operations(desired, existing, today=dt.date(2026, 7, 1))
        self.assertEqual(operations[0].action, "none")
        self.assertEqual(operations[0].status, "done")
        self.assertEqual(operations[0].changes, ())

    def test_milestone_more_than_30_days_in_the_past_is_skipped_not_created(self):
        import datetime as dt

        desired = [milestones.DesiredMilestone("4.150.0-preview.1", dt.date(2026, 1, 1), "desc")]
        operations = milestones.plan_schedule_operations(desired, {}, today=dt.date(2026, 7, 1))
        self.assertEqual(operations[0].action, "none")
        self.assertEqual(operations[0].status, "skipped")
        self.assertIsNone(operations[0].number)

    def test_milestone_within_30_days_in_the_past_is_still_created(self):
        import datetime as dt

        desired = [milestones.DesiredMilestone("4.152.0-preview.1", dt.date(2026, 6, 15), "desc")]
        operations = milestones.plan_schedule_operations(desired, {}, today=dt.date(2026, 7, 1))
        self.assertEqual(operations[0].action, "create")
        self.assertEqual(operations[0].status, "pending")


class ApplyScheduleOperationsTests(unittest.TestCase):
    class FakeClient:
        def __init__(self):
            self.created: list[tuple[str, str, str]] = []
            self.updated: list[tuple[int, str, str]] = []
            self._next_number = 500

        def create_milestone(self, title, *, due_on, description):
            self.created.append((title, due_on, description))
            number = self._next_number
            self._next_number += 1
            return milestones.Milestone(number=number, title=title, state="open", due_on=due_on, description=description)

        def update_milestone(self, number, *, due_on, description):
            self.updated.append((number, due_on, description))

    def test_applies_only_create_and_update_actions(self):
        client = self.FakeClient()
        operations = [
            milestones.ScheduleOperation(
                title="4.152.0-preview.1", number=None, status="pending", action="create",
                due_on="2026-07-29T00:00:00Z", description="desc", changes=(),
            ),
            milestones.ScheduleOperation(
                title="4.152.0-preview.2", number=9, status="pending", action="update",
                due_on="2026-08-12T00:00:00Z", description="desc2",
                changes=({"field": "dueOn", "from": "2026-08-01", "to": "2026-08-12"},),
            ),
            milestones.ScheduleOperation(
                title="4.152.0-rc.1", number=10, status="done", action="none",
                due_on="2026-08-18T00:00:00Z", description="desc3", changes=(),
            ),
            milestones.ScheduleOperation(
                title="4.150.0", number=None, status="skipped", action="none",
                due_on="2026-01-01T00:00:00Z", description="desc4", changes=(),
            ),
        ]
        results = milestones.apply_schedule_operations(operations, client)

        self.assertEqual(client.created, [("4.152.0-preview.1", "2026-07-29T00:00:00Z", "desc")])
        self.assertEqual(client.updated, [(9, "2026-08-12T00:00:00Z", "desc2")])
        statuses = {r["title"]: r["status"] for r in results}
        self.assertEqual(statuses["4.152.0-preview.1"], "done")
        self.assertEqual(statuses["4.152.0-preview.2"], "done")
        self.assertEqual(statuses["4.152.0-rc.1"], "done")  # already-matching passthrough status
        self.assertEqual(statuses["4.150.0"], "skipped")

    def test_idempotent_rerun_performs_no_further_writes(self):
        # Simulates a rerun where every desired milestone already matches:
        # apply_schedule_operations is only ever given "none"/"skipped"
        # operations by plan_schedule_operations in that case, so it must
        # not call create/update at all.
        client = self.FakeClient()
        operations = [
            milestones.ScheduleOperation(
                title="4.152.0-preview.1", number=9, status="done", action="none",
                due_on="2026-07-29T00:00:00Z", description="desc", changes=(),
            ),
        ]
        milestones.apply_schedule_operations(operations, client)
        self.assertEqual(client.created, [])
        self.assertEqual(client.updated, [])


if __name__ == "__main__":
    unittest.main()
