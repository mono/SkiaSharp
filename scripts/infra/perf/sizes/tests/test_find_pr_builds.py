#!/usr/bin/env python3
"""Tests for the PR artifact-size intake (scripts/infra/perf/sizes/find_pr_builds.py).

These cover the properties that replaced the old `check_run` subscription:

* discovery is ONE Azure DevOps query, not one per open pull request;
* only the newest build per PR is considered, since older ones are superseded;
* a build is selected on a durable comment stamp, never on the clock;
* the stamp carries the STAGE attempt, because Azure DevOps reuses the build id on a
  re-run and `buildNumberRevision` is a daily counter that cannot distinguish one;
* an unreadable comment SKIPS the PR rather than re-measuring it (~1 GB per measurement);
* selection is oldest-first so a backlog drains FIFO.
"""

from __future__ import annotations

import contextlib
import io
import os
import re
import sys
import unittest

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import find_pr_builds  # noqa: E402


# --------------------------------------------------------------------------- #
# Fixtures
# --------------------------------------------------------------------------- #

def azdo_build(build_id, pr, *, status="completed", result="succeeded", finish="2026-01-01T00:00:00Z"):
    return {
        "id": build_id,
        "sourceBranch": f"refs/pull/{pr}/merge",
        "status": status,
        "result": result,
        "finishTime": finish,
        "buildNumberRevision": 29,  # a daily counter; must NOT be used as the attempt
    }


def azdo_feed(*builds):
    def _fetch(url, headers=None):
        return {"count": len(builds), "value": list(builds)}
    return _fetch


def timeline(state="completed", result="succeeded", attempt=1):
    def _fetch(url, headers=None):
        return {"records": [
            {"type": "Stage", "name": "Native", "state": "completed", "result": "succeeded"},
            {"type": "Stage", "name": find_pr_builds.PACKAGE_STAGE, "state": state, "result": result, "attempt": attempt},
        ]}
    return _fetch


def paged_fetch(pages, *, fail_on_page=None):
    """Serve /comments pages in the API's real oldest-first order."""
    seen = []

    def _fetch(url, headers=None):
        seen.append(url)
        page = int(re.search(r"[?&]page=(\d+)", url).group(1))
        if fail_on_page is not None and page == fail_on_page:
            raise RuntimeError("403 rate limited")
        return pages[page - 1] if page - 1 < len(pages) else []

    _fetch.seen = seen
    return _fetch


def comment(body):
    return {"body": body}


def report(identity):
    return comment(f"<!-- skiasharp-pr-artifact-sizes -->\n<!-- build={identity} -->\nreport")


# --------------------------------------------------------------------------- #
# Discovery
# --------------------------------------------------------------------------- #

class RecentPrBuildsTests(unittest.TestCase):
    def test_one_query_covers_every_pr(self):
        seen = []

        def _fetch(url, headers=None):
            seen.append(url)
            return {"value": [azdo_build(1, 10), azdo_build(2, 11), azdo_build(3, 12)]}

        got = find_pr_builds.recent_pr_builds(fetch=_fetch)
        self.assertEqual({10, 11, 12}, set(got))
        self.assertEqual(1, len(seen), "discovery must not scale with open PR count")

    def test_query_is_scoped(self):
        seen = []

        def _fetch(url, headers=None):
            seen.append(url)
            return {"value": []}

        find_pr_builds.recent_pr_builds(fetch=_fetch, hours=2)
        url = seen[0]
        self.assertIn("definitions=345", url)
        self.assertIn("minTime=", url)
        self.assertIn("queryOrder=finishTimeDescending", url)

    def test_keeps_only_the_newest_build_per_pr(self):
        fetch = azdo_feed(azdo_build(100, 10), azdo_build(250, 10), azdo_build(180, 10))
        got = find_pr_builds.recent_pr_builds(fetch=fetch)
        self.assertEqual(250, got[10]["id"], "a superseded build must not be reported")

    def test_ignores_non_pr_builds(self):
        main_build = dict(azdo_build(1, 1), sourceBranch="refs/heads/main")
        fetch = azdo_feed(main_build, azdo_build(2, 42))
        self.assertEqual({42}, set(find_pr_builds.recent_pr_builds(fetch=fetch)))


class PackageStageTests(unittest.TestCase):
    def test_reports_success_and_attempt(self):
        ok, attempt = find_pr_builds.package_stage_status("1", fetch=timeline(attempt=3))
        self.assertTrue(ok)
        self.assertEqual(3, attempt)

    def test_incomplete_stage_is_not_ready(self):
        ok, _ = find_pr_builds.package_stage_status("1", fetch=timeline(state="inProgress", result=None))
        self.assertFalse(ok)

    def test_failed_stage_is_not_ready(self):
        ok, _ = find_pr_builds.package_stage_status("1", fetch=timeline(result="failed"))
        self.assertFalse(ok)

    def test_missing_stage_is_not_ready(self):
        ok, attempt = find_pr_builds.package_stage_status("1", fetch=lambda url, headers=None: {"records": []})
        self.assertFalse(ok)
        self.assertEqual(1, attempt)

    def test_unreadable_timeline_is_contained(self):
        def _boom(url, headers=None):
            raise RuntimeError("azdo 503")
        ok, _ = find_pr_builds.package_stage_status("1", fetch=_boom)
        self.assertFalse(ok, "an unreadable timeline must not be treated as ready")


# --------------------------------------------------------------------------- #
# The stamp
# --------------------------------------------------------------------------- #

class ReportedBuildTests(unittest.TestCase):
    """The per-issue comments endpoint ignores sort/direction: always oldest-first."""

    def test_finds_stamp_on_the_first_page(self):
        fetch = paged_fetch([[comment("hi"), report("111.1")]])
        self.assertEqual("111.1", find_pr_builds.reported_build("o/r", 1, fetch=fetch))

    def test_finds_stamp_beyond_the_first_page(self):
        page1 = [comment(f"noise {i}") for i in range(100)]
        fetch = paged_fetch([page1, [comment("x"), report("222.1")]])
        self.assertEqual("222.1", find_pr_builds.reported_build("o/r", 1, fetch=fetch))
        self.assertTrue(any("page=2" in u for u in fetch.seen))

    def test_first_marker_comment_wins_like_the_writers(self):
        """Reader and writers must agree on WHICH comment is authoritative."""
        fetch = paged_fetch([[report("1.1")] + [comment("x")] * 99, [report("2.1")]])
        self.assertEqual("1.1", find_pr_builds.reported_build("o/r", 1, fetch=fetch))

    def test_no_report_is_none(self):
        self.assertIsNone(find_pr_builds.reported_build("o/r", 1, fetch=paged_fetch([[comment("none")]])))

    def test_api_failure_raises_rather_than_reporting_absent(self):
        fetch = paged_fetch([[comment("x")] * 100, []], fail_on_page=2)
        with self.assertRaises(find_pr_builds.GitHubReadError):
            find_pr_builds.reported_build("o/r", 1, fetch=fetch)


# --------------------------------------------------------------------------- #
# Selection
# --------------------------------------------------------------------------- #

class SelectTests(unittest.TestCase):
    def select(self, *builds, reported=None, **kw):
        return find_pr_builds.select(
            "o/r",
            azdo_fetch=azdo_feed(*builds),
            timeline_fetch=timeline(),
            read_reported=reported or (lambda pr: None),
            **kw)

    def test_selects_an_unreported_build(self):
        got = self.select(azdo_build(500, 10))
        self.assertEqual([{"pr": 10, "build": "500", "identity": "500.1"}], got)

    def test_skips_a_build_already_reported(self):
        self.assertEqual([], self.select(azdo_build(500, 10), reported=lambda pr: "500.1"))

    def test_reports_again_after_a_stage_rerun(self):
        """The id is reused on a re-run, so the attempt is what distinguishes it."""
        got = find_pr_builds.select(
            "o/r",
            azdo_fetch=azdo_feed(azdo_build(500, 10)),
            timeline_fetch=timeline(attempt=2),
            read_reported=lambda pr: "500.1")
        self.assertEqual("500.2", got[0]["identity"])

    def test_daily_revision_is_not_used_as_the_attempt(self):
        got = self.select(azdo_build(500, 10))
        self.assertEqual("500.1", got[0]["identity"], "buildNumberRevision (29) must never appear in the stamp")

    def test_failed_build_is_skipped(self):
        self.assertEqual([], self.select(azdo_build(500, 10, result="failed")))

    def test_unpackaged_build_is_skipped(self):
        got = find_pr_builds.select(
            "o/r",
            azdo_fetch=azdo_feed(azdo_build(500, 10, status="inProgress", result=None)),
            timeline_fetch=timeline(state="inProgress", result=None),
            read_reported=lambda pr: None)
        self.assertEqual([], got)

    def test_in_progress_build_reports_once_packaged(self):
        """Watching the package stage reports well before the build finishes."""
        got = find_pr_builds.select(
            "o/r",
            azdo_fetch=azdo_feed(azdo_build(500, 10, status="inProgress", result=None)),
            timeline_fetch=timeline(),
            read_reported=lambda pr: None)
        self.assertEqual(1, len(got))

    def test_oldest_first(self):
        got = self.select(
            azdo_build(3, 12, finish="2026-01-01T03:00:00Z"),
            azdo_build(1, 10, finish="2026-01-01T01:00:00Z"),
            azdo_build(2, 11, finish="2026-01-01T02:00:00Z"))
        self.assertEqual([10, 11, 12], [e["pr"] for e in got])

    def test_no_cap_every_ready_build_is_returned(self):
        """Measured worst case is 7 PRs per 2h against a 256-job matrix limit."""
        got = self.select(*[azdo_build(i, i) for i in range(1, 21)])
        self.assertEqual(20, len(got))

    def test_unreadable_comment_skips_rather_than_measuring(self):
        def _boom(pr):
            raise find_pr_builds.GitHubReadError("429")
        self.assertEqual([], self.select(azdo_build(500, 10), reported=_boom))

    def test_only_pr_restricts_to_one_pull_request(self):
        got = self.select(azdo_build(1, 10), azdo_build(2, 11), only_pr=11)
        self.assertEqual([11], [e["pr"] for e in got])

    def test_ignore_reported_reports_on_demand(self):
        """Kept for the manual dispatch/backfill route, which must be able to re-report."""
        got = self.select(azdo_build(500, 10), reported=lambda pr: "500.1", ignore_reported=True)
        self.assertEqual(1, len(got))

    def test_nothing_ready_is_empty(self):
        self.assertEqual([], self.select())

    def test_widespread_read_failure_is_warned_about(self):
        def _boom(pr):
            raise find_pr_builds.GitHubReadError("429")
        buf = io.StringIO()
        with contextlib.redirect_stderr(buf):
            self.select(*[azdo_build(i, i) for i in range(1, 6)], reported=_boom)
        self.assertIn("::warning::", buf.getvalue())


# --------------------------------------------------------------------------- #
# Workflow wiring
# --------------------------------------------------------------------------- #

class WorkflowTests(unittest.TestCase):
    """Guards on the workflow itself: no loop-prone trigger, and a safe comment command."""

    WORKFLOW = os.path.abspath(os.path.join(
        os.path.dirname(os.path.abspath(__file__)),
        "..", "..", "..", "..", "..",
        ".github", "workflows", "track-artifact-sizes.yml"))

    def setUp(self):
        import yaml
        with open(self.WORKFLOW, encoding="utf-8") as fh:
            self.raw = fh.read()
        self.wf = yaml.safe_load(self.raw)
        self.on = self.wf.get("on", self.wf.get(True))

    def test_never_subscribes_to_check_run_or_check_suite(self):
        """The loop that produced 40,000 runs. A skipped job still publishes a check run."""
        self.assertNotIn("check_run", self.on)
        self.assertNotIn("check_suite", self.on)

    def test_sweep_is_hourly(self):
        crons = [s["cron"] for s in self.on["schedule"]]
        self.assertIn("0 7 * * *", crons, "the nightly measurement must survive")
        self.assertTrue(any(c.endswith("* * * *") and not c.startswith("0 7") for c in crons))

    def test_only_subscribes_to_events_actions_cannot_emit(self):
        """This workflow posts PR comments, so an `issue_comment` trigger could re-trigger it."""
        self.assertEqual({"schedule", "workflow_dispatch"}, set(self.on))

    def test_matrix_inputs_are_guarded_against_empty_string(self):
        """`strategy.matrix` expands BEFORE the job `if:`, so fromJSON('') fails the run."""
        self.assertIn("steps.builds.outputs.matrix || '[]'", self.raw)
        self.assertIn("needs.resolve.outputs.matrix || '[]'", self.raw)


if __name__ == "__main__":
    unittest.main()
