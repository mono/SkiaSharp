#!/usr/bin/env python3
"""Tests for the PR size-diff intake (scripts/infra/perf/sizes/find_pr_builds.py).

These cover the correctness the old broad ``check_run`` subscription used to provide:
the intake must select exactly the successful package build for a PR, must not re-measure
a build that is already reported, and must survive a single failing Azure DevOps query.
"""

from __future__ import annotations

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


class MeasuredParsingTests(unittest.TestCase):
    def test_parses_pairs(self):
        self.assertEqual({4912: "1576555", 7: "9"},
                         find_pr_builds.parse_measured(["4912=1576555", "7=9"]))

    def test_rejects_malformed_pair(self):
        with self.assertRaises(ValueError):
            find_pr_builds.parse_measured(["4912"])


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
