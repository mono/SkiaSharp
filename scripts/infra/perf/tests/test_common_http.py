#!/usr/bin/env python3
"""Tests for the HTTP contract in scripts/infra/perf/_common.py.

`http_get` raises `RuntimeError` for every failure, and several callers depend on exactly that: `feed_versions`,
`track.enumerate_feed_packages`, and `measure_pr.resolve_pr_number` catch `RuntimeError` to mean "absent / unavailable" and return
an empty result.

Fast-failing a non-retryable 4xx must therefore not leak the raw `urllib.error.HTTPError` — it is an `OSError`, not a
`RuntimeError`, so it would escape those handlers and turn a package that is merely not published yet into a crash in the nightly
size tracker. These tests pin both halves of that contract: the speed and the type.

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

        The size sweep issues sequential authenticated GETs on a schedule, which is exactly the shape that trips a secondary rate
        limit; fast-failing those would make the sweep strictly less resilient than before the fast-fail existed.
        """
        for code in (403, 408, 429):
            with self.subTest(code=code):
                fake = self.install(http_error(code))
                with self.assertRaises(RuntimeError):
                    _common.http_get("https://example.invalid/x", retries=3)
                self.assertEqual(3, fake.calls, f"HTTP {code} must be retried")

    def test_no_credentials_leak_into_the_error(self):
        self.install(http_error(404))
        with self.assertRaises(RuntimeError) as ctx:
            _common.http_get("https://example.invalid/x",
                             headers={"Authorization": "Bearer s3cr3t-token-value"})
        message = str(ctx.exception)
        self.assertNotIn("s3cr3t-token-value", message)
        self.assertNotIn("Bearer", message)
        self.assertNotIn("Authorization", message)

    def test_429_is_still_retried(self):
        fake = self.install(http_error(429))
        with self.assertRaises(RuntimeError):
            _common.http_get("https://example.invalid/x", retries=3)
        self.assertEqual(3, fake.calls)
        # Backoff happens between attempts only, never after the last one.
        self.assertEqual(2, len(self.slept))

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
        self.assertEqual([], _common.feed_versions("https://example.invalid/flat/", "Nope.Missing"))

    def test_feed_versions_returns_empty_when_the_feed_is_unavailable(self):
        _common.urllib.request.urlopen = FakeUrlopen(http_error(503))
        self.assertEqual([], _common.feed_versions("https://example.invalid/flat/", "Nope.Missing"))

    def test_feed_versions_passes_versions_through(self):
        _common.urllib.request.urlopen = FakeUrlopen(b'{"versions":["1.0.0","2.0.0"]}')
        self.assertEqual(["1.0.0", "2.0.0"], _common.feed_versions("https://example.invalid/flat/", "Real.Package"))


if __name__ == "__main__":
    unittest.main()
