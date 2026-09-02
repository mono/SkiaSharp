#!/usr/bin/env python3
"""Tests for the PR size-diff intake (scripts/infra/perf/sizes/find_pr_builds.py).

These cover the correctness the old broad ``check_run`` subscription used to provide:
the intake must select exactly the successful package build for a PR, must not re-measure
a build that is already reported, and must survive a single failing Azure DevOps query.
"""

from __future__ import annotations

import contextlib
import re
import io
import json
import os
import sys
import tempfile
import unittest

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import find_pr_builds  # noqa: E402


def fake_fetch(mapping):
    """Return a fetch stub that serves canned payloads and records the URLs it saw."""
    seen: list[str] = []

    def _fetch(url: str) -> dict:
        seen.append(url)
        for needle, payload in mapping.items():
            if needle in url:
                if isinstance(payload, Exception):
                    raise payload
                return payload
        return {"count": 0, "value": []}

    _fetch.seen = seen  # type: ignore[attr-defined]
    return _fetch


def builds(*ids):
    return {"count": len(ids), "value": [{"id": i} for i in ids]}


class LatestSuccessfulBuildTests(unittest.TestCase):
    def test_returns_newest_build_id_as_string(self):
        fetch = fake_fetch({"refs/pull/4912/merge": builds(1576555)})
        self.assertEqual("1576555",
                         find_pr_builds.latest_successful_build(4912, fetch=fetch))

    def test_query_is_targeted(self):
        """The query must pin the pipeline, the PR merge ref, and success only."""
        fetch = fake_fetch({"refs/pull/4912/merge": builds(1)})
        find_pr_builds.latest_successful_build(4912, fetch=fetch)
        url = fetch.seen[0]
        self.assertIn("definitions=345", url)
        self.assertIn("branchName=refs/pull/4912/merge", url)
        self.assertIn("statusFilter=completed", url)
        self.assertIn("resultFilter=succeeded", url)
        self.assertIn("$top=1", url)

    def test_no_builds_returns_none(self):
        self.assertIsNone(
            find_pr_builds.latest_successful_build(1, fetch=fake_fetch({})))

    def test_query_failure_is_contained(self):
        fetch = fake_fetch({"refs/pull/7/merge": RuntimeError("azdo 503")})
        self.assertIsNone(find_pr_builds.latest_successful_build(7, fetch=fetch))


class BuildMatrixTests(unittest.TestCase):
    def test_emits_entry_per_pr_with_a_build(self):
        fetch = fake_fetch({
            "refs/pull/10/merge": builds(100),
            "refs/pull/11/merge": builds(111),
        })
        self.assertEqual(
            [{"pr": 10, "build": "100"}, {"pr": 11, "build": "111"}],
            find_pr_builds.build_matrix([10, 11], fetch=fetch))

    def test_skips_already_measured_build(self):
        fetch = fake_fetch({"refs/pull/10/merge": builds(100)})
        self.assertEqual([], find_pr_builds.build_matrix([10], {10: "100"}, fetch=fetch))

    def test_reprocesses_when_a_newer_build_exists(self):
        fetch = fake_fetch({"refs/pull/10/merge": builds(200)})
        self.assertEqual([{"pr": 10, "build": "200"}],
                         find_pr_builds.build_matrix([10], {10: "100"}, fetch=fetch))

    def test_one_broken_pr_does_not_stop_the_sweep(self):
        fetch = fake_fetch({
            "refs/pull/10/merge": RuntimeError("azdo down"),
            "refs/pull/11/merge": builds(111),
        })
        self.assertEqual([{"pr": 11, "build": "111"}],
                         find_pr_builds.build_matrix([10, 11], fetch=fetch))

    def test_no_open_prs_is_empty(self):
        self.assertEqual([], find_pr_builds.build_matrix([], fetch=fake_fetch({})))

    def test_widespread_failure_is_warned_about(self):
        """Mass throttling must not be indistinguishable from 'nobody has a build'."""
        fetch = fake_fetch({f"refs/pull/{n}/merge": RuntimeError("429") for n in range(1, 5)})
        buf = io.StringIO()
        with contextlib.redirect_stderr(buf):
            entries = find_pr_builds.build_matrix([1, 2, 3, 4], fetch=fetch)
        self.assertEqual([], entries)
        self.assertIn("::warning::", buf.getvalue())
        self.assertIn("4 of 4", buf.getvalue())

    def test_a_single_failure_is_not_warned_about(self):
        fetch = fake_fetch({
            "refs/pull/1/merge": RuntimeError("blip"),
            "refs/pull/2/merge": builds(22),
            "refs/pull/3/merge": builds(33),
            "refs/pull/4/merge": builds(44),
        })
        buf = io.StringIO()
        with contextlib.redirect_stderr(buf):
            find_pr_builds.build_matrix([1, 2, 3, 4], fetch=fetch)
        self.assertNotIn("::warning::", buf.getvalue())


class MeasuredParsingTests(unittest.TestCase):
    def test_parses_pairs(self):
        self.assertEqual({4912: "1576555", 7: "9"},
                         find_pr_builds.parse_measured(["4912=1576555", "7=9"]))

    def test_rejects_malformed_pair(self):
        with self.assertRaises(ValueError):
            find_pr_builds.parse_measured(["4912"])


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


def report(build_id):
    return comment(f"<!-- skiasharp-pr-artifact-sizes -->\n<!-- build={build_id} -->\nreport")


class MeasuredBuildTests(unittest.TestCase):
    """The per-issue comments endpoint ignores sort/direction and returns oldest-first."""

    def test_finds_stamp_on_the_first_page(self):
        fetch = paged_fetch([[comment("hi"), report(111)]])
        self.assertEqual("111", find_pr_builds.measured_build("o/r", 1, fetch=fetch))

    def test_finds_stamp_beyond_the_first_page(self):
        # 100 unrelated comments, then the report — exactly the case a single
        # non-paginated read misses, causing a repeated ~1 GB measurement.
        page1 = [comment(f"noise {i}") for i in range(100)]
        page2 = [comment("noise 100"), report(222)]
        fetch = paged_fetch([page1, page2])
        self.assertEqual("222", find_pr_builds.measured_build("o/r", 1, fetch=fetch))
        self.assertTrue(any("page=2" in u for u in fetch.seen))

    def test_uses_the_newest_stamp_when_several_exist(self):
        fetch = paged_fetch([[report(1)] + [comment("x")] * 99, [report(2)]])
        self.assertEqual("2", find_pr_builds.measured_build("o/r", 1, fetch=fetch))

    def test_no_report_is_none(self):
        fetch = paged_fetch([[comment("nothing here")]])
        self.assertIsNone(find_pr_builds.measured_build("o/r", 1, fetch=fetch))

    def test_api_failure_raises_rather_than_reporting_unmeasured(self):
        fetch = paged_fetch([[comment("x")] * 100, []], fail_on_page=2)
        with self.assertRaises(find_pr_builds.GitHubReadError):
            find_pr_builds.measured_build("o/r", 1, fetch=fetch)

    def test_unexpected_shape_raises(self):
        def _fetch(url, headers=None):
            return {"message": "Not Found"}
        with self.assertRaises(find_pr_builds.GitHubReadError):
            find_pr_builds.measured_build("o/r", 1, fetch=_fetch)


class FailClosedSweepTests(unittest.TestCase):
    def test_unreadable_comments_skip_the_pr_instead_of_measuring_it(self):
        azdo = fake_fetch({"refs/pull/10/merge": builds(100)})

        def read_measured(pr):
            raise find_pr_builds.GitHubReadError("429")

        self.assertEqual(
            [],
            find_pr_builds.build_matrix([10], read_measured=read_measured, fetch=azdo))

    def test_live_stamp_dedupes(self):
        azdo = fake_fetch({"refs/pull/10/merge": builds(100)})
        self.assertEqual(
            [],
            find_pr_builds.build_matrix([10], read_measured=lambda pr: "100", fetch=azdo))

    def test_live_stamp_allows_a_newer_build(self):
        azdo = fake_fetch({"refs/pull/10/merge": builds(200)})
        self.assertEqual(
            [{"pr": 10, "build": "200"}],
            find_pr_builds.build_matrix([10], read_measured=lambda pr: "100", fetch=azdo))


class ClaimOrderingTests(unittest.TestCase):
    """Every selected build must be stamped before any expensive work begins.

    `if: failure()` cannot cover a job hard-timeout, so the stamp has to be written by an
    unconditional step that runs before the download.
    """

    WORKFLOW = os.path.abspath(os.path.join(
        os.path.dirname(os.path.abspath(__file__)),  # .../scripts/infra/perf/sizes/tests
        "..", "..", "..", "..", "..",                # -> repository root
        ".github", "workflows", "track-artifact-sizes.yml"))

    def setUp(self):
        import yaml
        with open(self.WORKFLOW, encoding="utf-8") as fh:
            self.steps = yaml.safe_load(fh)["jobs"]["pr"]["steps"]

    def names(self):
        return [s.get("name") or s.get("uses") for s in self.steps]

    def index_of(self, needle):
        for i, name in enumerate(self.names()):
            if needle in (name or ""):
                return i
        raise AssertionError(f"no step named {needle!r} in {self.names()}")

    def test_claim_runs_before_the_download(self):
        self.assertLess(self.index_of("Claim the build"),
                        self.index_of("Download + measure"))

    def test_claim_is_unconditional(self):
        self.assertIsNone(self.steps[self.index_of("Claim the build")].get("if"),
                          "the claim step must not be conditional")

    def test_claim_writes_the_dedupe_stamp(self):
        script = self.steps[self.index_of("Claim the build")]["with"]["script"]
        self.assertIn("build=${buildId}", script)
        self.assertIn("skiasharp-pr-artifact-sizes", script)

    def test_no_step_before_the_claim_downloads_anything(self):
        before = self.steps[: self.index_of("Claim the build")]
        for step in before:
            rendered = json.dumps(step)
            self.assertNotIn("measure_pr.py", rendered)


class CliTests(unittest.TestCase):
    def setUp(self):
        self._original = find_pr_builds.http_get_json

    def tearDown(self):
        find_pr_builds.http_get_json = self._original

    def test_emits_json_matrix_and_honours_limit(self):
        find_pr_builds.http_get_json = fake_fetch({
            "refs/pull/1/merge": builds(11),
            "refs/pull/2/merge": builds(22),
        })
        with tempfile.TemporaryDirectory() as tmp:
            out = os.path.join(tmp, "matrix.json")
            rc = find_pr_builds.main(
                ["--pr", "1", "--pr", "2", "--limit", "1", "--output", out])
            self.assertEqual(0, rc)
            with open(out, encoding="utf-8") as fh:
                self.assertEqual([{"pr": 1, "build": "11"}], json.load(fh))

    def test_emits_empty_matrix_without_prs(self):
        find_pr_builds.http_get_json = fake_fetch({})
        with tempfile.TemporaryDirectory() as tmp:
            out = os.path.join(tmp, "matrix.json")
            self.assertEqual(0, find_pr_builds.main(["--output", out]))
            with open(out, encoding="utf-8") as fh:
                self.assertEqual([], json.load(fh))


if __name__ == "__main__":
    unittest.main()
