#!/usr/bin/env python3
"""Tests for the PR artifact-size intake."""

from __future__ import annotations

import json
import os
import sys
import unittest

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import find_pr_builds as f  # noqa: E402
import pr_comment as pc  # noqa: E402
from _common import parse_iso_utc  # noqa: E402

T0 = "2026-01-01T00:00:00"
T1 = "2026-01-02T00:00:00"


def build(build_id, pr, *, status="completed", result="succeeded"):
    return {"id": build_id, "sourceBranch": f"refs/pull/{pr}/merge",
            "status": status, "result": result}


def feed(*builds):
    return lambda url, headers=None: {"value": list(builds)}


def select(*builds, stage=None, reported=None):
    return f.select("o/r", 2,
                    builds=lambda: f.newest_build_per_pr(2, fetch=feed(*builds)),
                    stage=stage or (lambda b: T1),
                    reported=reported or (lambda pr: None))


class ParseTimeTests(unittest.TestCase):
    """Timestamp parsing (shared, in _common) as the intake and the stamp reader use it."""

    def test_parses_every_precision_azure_emits(self):
        """Azure mixes 7-, 2-, 1-digit and absent fractions within a single timeline."""
        for value in ("2026-09-02T03:34:33Z", "2026-09-02T03:34:33.4Z",
                      "2026-09-02T03:34:33.83Z", "2026-09-02T03:34:33.3866667Z"):
            parsed = parse_iso_utc(value)
            self.assertIsNotNone(parsed, value)
            self.assertEqual("2026-09-02T03:34:33", parsed.strftime(pc.STAMP_FORMAT), value)

    def test_compares_chronologically_where_strings_do_not(self):
        """Raw: 'Z' (0x5A) > '.' (0x2E), so the same instant would rank as later."""
        bare, frac = "2026-09-02T03:34:33Z", "2026-09-02T03:34:33.3866667Z"
        self.assertGreater(bare, frac)                             # the trap
        self.assertLess(parse_iso_utc(bare), parse_iso_utc(frac))    # sub-second, ordered
        self.assertLess(parse_iso_utc(frac), parse_iso_utc("2026-09-02T05:10:46.04Z"))

    def test_our_own_stamp_round_trips(self):
        """A stored stamp is naive; an Azure value is aware. They must stay comparable."""
        stored, azure = parse_iso_utc("2026-09-02T03:34:33"), parse_iso_utc("2026-09-02T03:34:33Z")
        self.assertEqual(stored, azure)

    def test_missing_or_malformed_is_none(self):
        for value in (None, "", "not-a-time"):
            self.assertIsNone(parse_iso_utc(value))


class DiscoveryTests(unittest.TestCase):
    def test_one_query_covers_every_pr(self):
        calls = []

        def fetch(url, headers=None):
            calls.append(url)
            return {"value": [build(1, 10), build(2, 11), build(3, 12)]}

        self.assertEqual({10, 11, 12}, set(f.newest_build_per_pr(2, fetch=fetch)))
        self.assertEqual(1, len(calls), "must not scale with open PR count")

    def test_keeps_only_the_newest_build_per_pr(self):
        got = f.newest_build_per_pr(2, fetch=feed(build(100, 10), build(250, 10), build(180, 10)))
        self.assertEqual(250, got[10]["id"])

    def test_ignores_non_pr_builds(self):
        main = dict(build(1, 1), sourceBranch="refs/heads/main")
        self.assertEqual({42}, set(f.newest_build_per_pr(2, fetch=feed(main, build(2, 42)))))


class PackagedAtTests(unittest.TestCase):
    def stage(self, **kw):
        rec = {"type": "Stage", "name": f.PACKAGE_STAGE, "state": "completed",
               "result": "succeeded", "finishTime": T1}
        rec.update(kw)
        return lambda url, headers=None: {"records": [rec]}

    def test_returns_finish_time_when_succeeded(self):
        self.assertEqual(T1, f.packaged_at("1", fetch=self.stage()))

    def test_none_while_running(self):
        self.assertIsNone(f.packaged_at("1", fetch=self.stage(state="inProgress", result=None)))

    def test_none_when_failed(self):
        self.assertIsNone(f.packaged_at("1", fetch=self.stage(result="failed")))

    def test_none_when_stage_absent(self):
        self.assertIsNone(f.packaged_at("1", fetch=lambda url, headers=None: {"records": []}))

    def test_unreadable_timeline_is_contained(self):
        def boom(url, headers=None):
            raise RuntimeError("azdo 503")
        self.assertIsNone(f.packaged_at("1", fetch=boom))


class SelectTests(unittest.TestCase):
    def test_reports_an_unreported_build(self):
        self.assertEqual([{"pr": 10, "build": "500", "packagedAt": T1}], select(build(500, 10)))

    def test_skips_when_not_newer_than_the_report(self):
        self.assertEqual([], select(build(500, 10), reported=lambda pr: parse_iso_utc(T1)))

    def test_reports_a_rerun_because_it_packages_later(self):
        """Ordering removes any dependence on a rerun/attempt counter."""
        got = select(build(500, 10), reported=lambda pr: parse_iso_utc(T0))
        self.assertEqual(1, len(got))

    def test_ignores_a_stale_out_of_order_result(self):
        got = select(build(500, 10), stage=lambda b: T0, reported=lambda pr: parse_iso_utc(T1))
        self.assertEqual([], got)

    def test_reports_a_failed_build_that_packaged_successfully(self):
        """Real case (build 1577663): failed overall, `Package NuGets` succeeded.

        The packages exist and their sizes are meaningful, so gating on the build's overall
        result would silently discard them. Only the stage's own result may gate this.
        """
        self.assertEqual(1, len(select(build(500, 10, result="failed"))))

    def test_skips_a_build_whose_packaging_failed(self):
        self.assertEqual([], select(build(500, 10, result="failed"), stage=lambda b: None))

    def test_skips_a_build_that_has_not_packaged(self):
        self.assertEqual([], select(build(500, 10), stage=lambda b: None))

    def test_reports_an_in_progress_build_once_packaged(self):
        self.assertEqual(1, len(select(build(500, 10, status="inProgress", result=None))))

    def test_oldest_first(self):
        times = {1: "2026-01-03T00:00:00", 2: "2026-01-01T00:00:00", 3: "2026-01-02T00:00:00"}
        got = select(build(1, 10), build(2, 11), build(3, 12), stage=lambda b: times[b])
        self.assertEqual([11, 12, 10], [e["pr"] for e in got])

    def test_unreadable_comment_skips_rather_than_measuring(self):
        """An unknown stamp must not cause a ~1.1 GB re-download."""
        def boom(pr):
            raise RuntimeError("429")
        self.assertEqual([], select(build(500, 10), reported=boom))

    def test_no_cap(self):
        self.assertEqual(20, len(select(*[build(i, i) for i in range(1, 21)])))

    def test_nothing_ready_is_empty(self):
        self.assertEqual([], select())


class StampTests(unittest.TestCase):
    """Reading the stamp back out of the PR comment."""

    def comments(self, *bodies):
        payload = json.dumps([{"id": i, "body": b} for i, b in enumerate(bodies)]).encode()
        return lambda url, headers=None, **kw: payload

    def test_reads_the_stamp(self):
        fetch = self.comments("hi", f"{pc.MARKER}\n<!-- build=7 packaged={T1} -->")
        self.assertEqual(parse_iso_utc(T1), pc.read_stamp("o/r", 1, request=fetch))

    def test_first_marker_wins_matching_the_writer(self):
        fetch = self.comments(f"{pc.MARKER}\n<!-- build=1 packaged={T0} -->",
                              f"{pc.MARKER}\n<!-- build=2 packaged={T1} -->")
        self.assertEqual(parse_iso_utc(T0), pc.read_stamp("o/r", 1, request=fetch))

    def test_no_report_is_none(self):
        self.assertIsNone(pc.read_stamp("o/r", 1, request=self.comments("nothing here")))

    def test_read_failure_propagates(self):
        def boom(url, headers=None, **kw):
            raise RuntimeError("403")
        with self.assertRaises(RuntimeError):
            pc.read_stamp("o/r", 1, request=boom)


class CommentContractTests(unittest.TestCase):
    """The writers and the reader must agree, in both directions.

    Every historical failure here was silent and cost ~1.1 GB an hour: a writer that emitted
    `build=<id>` without the packaging time, and a writer that paginated to find the comment
    while the reader did not — which on a PR past 100 comments hides the stamp from the
    reader alone. Both are gone because there is now one implementation of each.
    """

    def call(self, *, body="## report", packaged="2026-09-02T03:34:33", existing=None):
        sent = []

        def request(url, method="GET", data=None, headers=None, **kw):
            if method == "GET":
                return json.dumps(existing or []).encode()
            sent.append((method, url, json.loads(data.decode())["body"]))
            return b"{}"

        pc.upsert("o/r", 7, pc.compose("1577884", packaged, body), request=request)
        return sent[0]

    def test_posted_body_is_readable_by_the_reader(self):
        _, _, body = self.call()
        stamp = pc.STAMP_RE.search(body)
        self.assertIsNotNone(stamp, f"the reader cannot parse what we post:\n{body}")
        self.assertEqual("1577884", stamp.group(1))
        self.assertEqual(parse_iso_utc("2026-09-02T03:34:33"), parse_iso_utc(stamp.group(2)))
        self.assertIn(pc.MARKER, body)

    def test_a_rendered_report_round_trips(self):
        """Drive the real renderer, post its output, read the stamp back."""
        import subprocess
        import tempfile
        render = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
                              "render_pr_md.py")
        with tempfile.TemporaryDirectory() as tmp:
            sizes, out = os.path.join(tmp, "pr.json"), os.path.join(tmp, "c.md")
            with open(sizes, "w", encoding="utf-8") as fh:
                json.dump({"buildId": "1577884", "prNumber": 10,
                           "packages": {"SkiaSharp": {"version": "3.1.0", "nupkg": 1000,
                                                      "files": {}, "natives": {}}}}, fh)
            subprocess.run([sys.executable, render, "--pr-sizes", sizes, "--output", out],
                           check=True, capture_output=True)
            with open(out, encoding="utf-8") as fh:
                rendered = fh.read()
        self.assertNotIn(pc.MARKER, rendered, "the renderer must not emit a second marker")
        _, _, body = self.call(body=rendered)
        self.assertEqual(1, body.count(pc.MARKER))
        self.assertEqual(parse_iso_utc("2026-09-02T03:34:33"),
                         parse_iso_utc(pc.STAMP_RE.search(body).group(2)))

    def test_missing_packaging_time_still_stamps(self):
        """A stampless body is indistinguishable from an unmeasured PR, so never emit one.

        Assert the stamp *parses*, not merely that it matches: STAMP_RE's `\\S+` happily
        matches `packaged=None`, so a writer that interpolated a missing time would satisfy
        the regex and still be unreadable — the reader would see no usable time, call the PR
        unmeasured, and re-download every hour. Presence is not the property that matters.
        """
        _, _, body = self.call(packaged=None)
        match = pc.STAMP_RE.search(body)
        self.assertIsNotNone(match)
        self.assertIsNotNone(parse_iso_utc(match.group(2)), f"unparseable stamp: {body!r}")

    def test_updates_in_place_when_the_comment_exists(self):
        method, url, _ = self.call(existing=[{"id": 42, "body": pc.MARKER}])
        self.assertEqual("PATCH", method)
        self.assertTrue(url.endswith("/issues/comments/42"))

    def test_creates_when_absent(self):
        method, url, _ = self.call(existing=[{"id": 1, "body": "unrelated"}])
        self.assertEqual("POST", method)
        self.assertTrue(url.endswith("/issues/7/comments"))


class WorkflowTests(unittest.TestCase):
    PATH = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)),
                                        "..", "..", "..", "..", "..",
                                        ".github", "workflows", "track-artifact-sizes.yml"))

    def setUp(self):
        import yaml
        with open(self.PATH, encoding="utf-8") as fh:
            self.raw = fh.read()
        self.wf = yaml.safe_load(self.raw)
        self.on = self.wf.get("on", self.wf.get(True))

    def test_only_events_actions_cannot_emit(self):
        """check_run looped to 40,000 runs; this workflow also posts comments."""
        self.assertEqual({"schedule", "workflow_dispatch"}, set(self.on))

    def test_nightly_and_hourly_sweep(self):
        crons = [s["cron"] for s in self.on["schedule"]]
        self.assertIn("0 7 * * *", crons)
        self.assertTrue(any(c != "0 7 * * *" for c in crons))

    def test_the_comment_steps_all_route_through_one_writer(self):
        """Three steps write this comment; hand-rolling any of them is how they drift."""
        self.assertEqual(3, self.raw.count("python3 scripts/infra/perf/sizes/pr_comment.py"))
        self.assertNotIn("skiasharp-pr-artifact-sizes", self.raw)

    def test_matrix_guarded_against_empty_string(self):
        """strategy.matrix expands BEFORE the job `if:`, so fromJSON('') fails the run."""
        self.assertIn("steps.builds.outputs.matrix || '[]'", self.raw)
        self.assertIn("needs.resolve.outputs.matrix || '[]'", self.raw)


if __name__ == "__main__":
    unittest.main()
