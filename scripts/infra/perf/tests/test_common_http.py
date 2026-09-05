#!/usr/bin/env python3
"""Tests for the HTTP contract in scripts/infra/perf/_common.py.

`http_get` raises `RuntimeError` for every failure, and `feed_versions`,
`track.enumerate_feed_packages` and `measure_pr.resolve_pr_number` all catch exactly that to
mean "absent". A fast-fail for non-retryable 4xx once raised `HTTPError` instead, which is an
`OSError` and escaped all three, turning an unpublished package into a crash. These pin both
halves of the contract: the type, and which codes are permanent.

Everything here is offline; `urllib.request.urlopen` and `time.sleep` are replaced.
"""
from __future__ import annotations

import io
import os
import sys
import unittest
import urllib.error

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import _common  # noqa: E402


class FakeUrlopen:
    """Serve a canned outcome and count attempts."""

    def __init__(self, outcome):
        self.outcome = outcome
        self.calls = 0

    def __call__(self, req, timeout=None):
        self.calls += 1
        if isinstance(self.outcome, Exception):
            raise self.outcome
        return _Body(self.outcome)


class _Body(io.BytesIO):
    def __enter__(self):
        return self

    def __exit__(self, *exc):
        self.close()
        return False


def http_error(code):
    return urllib.error.HTTPError("https://example.invalid/x", code, "boom", {}, None)


class HttpGetContractTests(unittest.TestCase):
    def setUp(self):
        self._urlopen = _common.urllib.request.urlopen
        self._sleep = _common.time.sleep
        self.slept: list[float] = []
        _common.time.sleep = self.slept.append

    def tearDown(self):
        _common.urllib.request.urlopen = self._urlopen
        _common.time.sleep = self._sleep

    def install(self, outcome):
        fake = FakeUrlopen(outcome)
        _common.urllib.request.urlopen = fake
        return fake

    def test_success_returns_bytes(self):
        self.install(b'{"ok":true}')
        self.assertEqual(b'{"ok":true}', _common.http_get("https://example.invalid/x"))

    def test_404_raises_runtime_error_not_http_error(self):
        """The type matters: callers catch RuntimeError to mean 'absent'."""
        self.install(http_error(404))
        with self.assertRaises(RuntimeError) as ctx:
            _common.http_get("https://example.invalid/x")
        self.assertNotIsInstance(ctx.exception, urllib.error.HTTPError)
        self.assertIn("404", str(ctx.exception))
        self.assertIn("example.invalid", str(ctx.exception))
        self.assertIsInstance(ctx.exception.__cause__, urllib.error.HTTPError)

    def test_non_retryable_4xx_fails_fast(self):
        for code in (400, 404, 410, 422):
            with self.subTest(code=code):
                self.slept.clear()
                fake = self.install(http_error(code))
                with self.assertRaises(RuntimeError):
                    _common.http_get("https://example.invalid/x")
                self.assertEqual(1, fake.calls, "a non-retryable 4xx must not be retried")
                self.assertEqual([], self.slept, "a non-retryable 4xx must not sleep")

    def test_transient_4xx_is_still_retried(self):
        """403 is GitHub's secondary rate limit and 408 a timeout — both recover.

        The sweep issues sequential authenticated GETs on a schedule, exactly the shape that
        trips a secondary rate limit, so fast-failing these would make it strictly less
        resilient than before the fast-fail existed.
        """
        for code in (403, 408, 429):
            with self.subTest(code=code):
                self.slept.clear()
                fake = self.install(http_error(code))
                with self.assertRaises(RuntimeError):
                    _common.http_get("https://example.invalid/x", retries=3)
                self.assertEqual(3, fake.calls, f"HTTP {code} must be retried")
                # Backoff happens between attempts only, never after the last one.
                self.assertEqual(2, len(self.slept))

    def test_no_credentials_leak_into_the_error(self):
        self.install(http_error(404))
        with self.assertRaises(RuntimeError) as ctx:
            _common.http_get("https://example.invalid/x",
                             headers={"Authorization": "Bearer s3cr3t-token-value"})
        message = str(ctx.exception)
        self.assertNotIn("s3cr3t-token-value", message)
        self.assertNotIn("Bearer", message)
        self.assertNotIn("Authorization", message)

    def test_5xx_is_still_retried(self):
        fake = self.install(http_error(503))
        with self.assertRaises(RuntimeError):
            _common.http_get("https://example.invalid/x", retries=2)
        self.assertEqual(2, fake.calls)

    def test_connection_error_is_still_retried(self):
        fake = self.install(urllib.error.URLError("dns"))
        with self.assertRaises(RuntimeError):
            _common.http_get("https://example.invalid/x", retries=2)
        self.assertEqual(2, fake.calls)


class AbsentPackageContractTests(unittest.TestCase):
    """The behaviour the callers actually rely on."""

    def setUp(self):
        self._urlopen = _common.urllib.request.urlopen
        self._sleep = _common.time.sleep
        _common.time.sleep = lambda _s: None

    def tearDown(self):
        _common.urllib.request.urlopen = self._urlopen
        _common.time.sleep = self._sleep

    def test_feed_versions_returns_empty_for_a_missing_package(self):
        _common.urllib.request.urlopen = FakeUrlopen(http_error(404))
        self.assertEqual([], _common.feed_versions("https://x.invalid/flat/", "Nope"))

    def test_feed_versions_returns_empty_when_the_feed_is_unavailable(self):
        _common.urllib.request.urlopen = FakeUrlopen(http_error(503))
        self.assertEqual([], _common.feed_versions("https://x.invalid/flat/", "Nope"))

    def test_feed_versions_passes_versions_through(self):
        _common.urllib.request.urlopen = FakeUrlopen(b'{"versions":["1.0.0","2.0.0"]}')
        self.assertEqual(["1.0.0", "2.0.0"],
                         _common.feed_versions("https://x.invalid/flat/", "Real"))


class EmptyBodyTests(unittest.TestCase):
    """Azure DevOps answers 204 No Content for a build with no timeline."""

    def decode(self, raw):
        original = _common.http_get
        _common.http_get = lambda url, **kw: raw
        try:
            return _common.http_get_json("https://example.invalid")
        finally:
            _common.http_get = original

    def test_no_content_is_an_empty_object(self):
        self.assertEqual({}, self.decode(b""))
        self.assertEqual({}, self.decode(b"   \n"))

    def test_real_json_still_decodes(self):
        self.assertEqual({"records": []}, self.decode(b'{"records": []}'))

    def test_malformed_json_still_raises(self):
        with self.assertRaises(ValueError):
            self.decode(b"<html>not json</html>")


if __name__ == "__main__":
    unittest.main()
