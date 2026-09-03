#!/usr/bin/env python3
"""Tests for the PR artifact-size intake."""

from __future__ import annotations

import os
import sys
import unittest

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import find_pr_builds as f  # noqa: E402

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
    def test_parses_every_precision_azure_emits(self):
        """Azure mixes 7-, 2-, 1-digit and absent fractions within a single timeline."""
        for value in ("2026-09-02T03:34:33Z", "2026-09-02T03:34:33.4Z",
                      "2026-09-02T03:34:33.83Z", "2026-09-02T03:34:33.3866667Z"):
            parsed = f.parse_time(value)
            self.assertIsNotNone(parsed, value)
            self.assertEqual("2026-09-02T03:34:33", parsed.strftime(f.STAMP_FORMAT), value)

    def test_compares_chronologically_where_strings_do_not(self):
        """Raw: 'Z' (0x5A) > '.' (0x2E), so the same instant would rank as later."""
        bare, frac = "2026-09-02T03:34:33Z", "2026-09-02T03:34:33.3866667Z"
        self.assertGreater(bare, frac)                             # the trap
        self.assertLess(f.parse_time(bare), f.parse_time(frac))    # sub-second, ordered
        self.assertLess(f.parse_time(frac), f.parse_time("2026-09-02T05:10:46.04Z"))

    def test_our_own_stamp_round_trips(self):
        """A stored stamp is naive; an Azure value is aware. They must stay comparable."""
        stored, azure = f.parse_time("2026-09-02T03:34:33"), f.parse_time("2026-09-02T03:34:33Z")
        self.assertEqual(stored, azure)

    def test_missing_or_malformed_is_none(self):
        for value in (None, "", "not-a-time"):
            self.assertIsNone(f.parse_time(value))


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
        self.assertEqual([], select(build(500, 10), reported=lambda pr: f.parse_time(T1)))

    def test_reports_a_rerun_because_it_packages_later(self):
        """Ordering removes any dependence on a rerun/attempt counter."""
        got = select(build(500, 10), reported=lambda pr: f.parse_time(T0))
        self.assertEqual(1, len(got))

    def test_ignores_a_stale_out_of_order_result(self):
        got = select(build(500, 10), stage=lambda b: T0, reported=lambda pr: f.parse_time(T1))
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
    MARKER = "<!-- skiasharp-pr-artifact-sizes -->"

    def comments(self, *bodies):
        return lambda url, headers=None: [{"body": b} for b in bodies]

    def test_reads_the_stamp(self):
        fetch = self.comments("hi", f"{self.MARKER}\n<!-- build=7 packaged={T1} -->")
        self.assertEqual(f.parse_time(T1), f.reported_at("o/r", 1, fetch=fetch))

    def test_first_marker_wins_matching_the_writers(self):
        fetch = self.comments(f"{self.MARKER}\n<!-- build=1 packaged={T0} -->",
                              f"{self.MARKER}\n<!-- build=2 packaged={T1} -->")
        self.assertEqual(f.parse_time(T0), f.reported_at("o/r", 1, fetch=fetch))

    def test_no_report_is_none(self):
        self.assertIsNone(f.reported_at("o/r", 1, fetch=self.comments("nothing here")))

    def test_read_failure_propagates(self):
        def boom(url, headers=None):
            raise RuntimeError("403")
        with self.assertRaises(RuntimeError):
            f.reported_at("o/r", 1, fetch=boom)


class StampRoundTripTests(unittest.TestCase):
    """The rendered comment REPLACES the claim step's body, so it must carry the same stamp.

    If the renderer emitted only `build=<id>`, the intake could never match it and would
    re-download and re-measure the same ~1.1 GB artifact on every sweep.
    """

    def test_renderer_output_is_readable_by_the_intake(self):
        import json as _json
        import subprocess
        import tempfile
        render = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
                              "render_pr_md.py")
        with tempfile.TemporaryDirectory() as tmp:
            sizes = os.path.join(tmp, "pr.json")
            out = os.path.join(tmp, "comment.md")
            with open(sizes, "w", encoding="utf-8") as fh:
                _json.dump({"buildId": "1577884", "prNumber": 10,
                            "packages": {"SkiaSharp": {"version": "3.1.0", "nupkg": 1000,
                                                       "files": {}, "natives": {}}}}, fh)
            subprocess.run([sys.executable, render, "--pr-sizes", sizes,
                            "--packaged-at", "2026-09-02T03:34:33", "--output", out],
                           check=True, capture_output=True)
            with open(out, encoding="utf-8") as fh:
                body = fh.read()
        match = f.STAMP_RE.search(body)
        self.assertIsNotNone(match, f"intake cannot read the rendered stamp:\n{body[:200]}")
        self.assertEqual("1577884", match.group(1))
        self.assertEqual(f.parse_time("2026-09-02T03:34:33"), f.parse_time(match.group(2)))


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

    def test_renderer_receives_the_packaging_time(self):
        """Without it the renderer omits the stamp and dedupe silently stops working."""
        self.assertIn("--packaged-at", self.raw)

    def test_matrix_guarded_against_empty_string(self):
        """strategy.matrix expands BEFORE the job `if:`, so fromJSON('') fails the run."""
        self.assertIn("steps.builds.outputs.matrix || '[]'", self.raw)
        self.assertIn("needs.resolve.outputs.matrix || '[]'", self.raw)


if __name__ == "__main__":
    unittest.main()
