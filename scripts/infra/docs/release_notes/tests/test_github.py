from __future__ import annotations

import json
import os
import sys
import unittest
from pathlib import Path
from unittest import mock

_DOCS_DIR = Path(__file__).resolve().parents[2]
if str(_DOCS_DIR) not in sys.path:
    sys.path.insert(0, str(_DOCS_DIR))

from release_notes import github


class _Response:
    def __init__(self, payload):
        self.payload = payload

    def __enter__(self):
        return self

    def __exit__(self, *_):
        return None

    def read(self):
        return json.dumps(self.payload).encode("utf-8")


class RestGitHubClientTests(unittest.TestCase):
    def test_reads_then_updates_release_by_id_without_gh(self):
        payload = {
            "id": 42,
            "tag_name": "v4.152.0",
            "name": "Version 4.152.0",
            "draft": False,
            "prerelease": False,
            "target_commitish": "a" * 40,
            "body": "old",
            "html_url": "https://github.com/mono/SkiaSharp/releases/tag/v4.152.0",
        }
        responses = [_Response(payload), _Response(payload)]
        with mock.patch.object(github.request, "urlopen", side_effect=responses) as urlopen:
            client = github.RestGitHubClient(
                "mono/SkiaSharp", token="secret", api_url="https://api.github.test"
            )
            release = client.get_release("v4.152.0")
            client.update_release_body(tag="v4.152.0", body="new")

        self.assertEqual(release.tag_name, "v4.152.0")
        self.assertEqual(urlopen.call_count, 2)
        get_request = urlopen.call_args_list[0].args[0]
        patch_request = urlopen.call_args_list[1].args[0]
        self.assertEqual(get_request.method, "GET")
        self.assertEqual(
            get_request.full_url,
            "https://api.github.test/repos/mono/SkiaSharp/releases/tags/v4.152.0",
        )
        self.assertEqual(patch_request.method, "PATCH")
        self.assertTrue(patch_request.full_url.endswith("/releases/42"))
        self.assertEqual(json.loads(patch_request.data), {"body": "new"})

    def test_requires_token_and_well_formed_repository(self):
        with mock.patch.dict(os.environ, {}, clear=True):
            with self.assertRaises(github.GitHubError):
                github.RestGitHubClient("mono/SkiaSharp")
        with self.assertRaises(github.GitHubError):
            github.RestGitHubClient("../SkiaSharp", token="secret")

    def test_marker_helpers_adopt_unmarked_and_replace_the_complete_body(self):
        body = github.build_managed_body("summary")
        self.assertTrue(github.has_managed_markers(body))
        self.assertNotIn(github.GENERATED_START_MARKER, body)
        replaced = github.replace_managed_summary(body, "new summary")
        self.assertIn("new summary", replaced)
        self.assertNotIn("\nsummary\n", replaced)
        adopted = github.replace_managed_summary("plain release notes", "summary")
        self.assertIn("summary", adopted)
        self.assertNotIn("plain release notes", adopted)
        self.assertEqual(replaced, github.replace_managed_summary(replaced, "new summary"))

    def test_marker_helpers_migrate_a_legacy_four_marker_body(self):
        legacy = "{}\nsummary\n{}\n\n{}\nold generated notes\n{}\n".format(
            github.SUMMARY_START_MARKER,
            github.SUMMARY_END_MARKER,
            github.GENERATED_START_MARKER,
            github.GENERATED_END_MARKER,
        )
        migrated = github.replace_managed_summary(legacy, "canonical")
        self.assertIn("canonical", migrated)
        self.assertNotIn("old generated notes", migrated)
        self.assertNotIn(github.GENERATED_START_MARKER, migrated)
        self.assertNotIn(github.GENERATED_END_MARKER, migrated)

    def test_marker_helpers_fail_closed_for_partial_duplicate_or_out_of_order_markers(self):
        valid = github.build_managed_body("summary")
        malformed_bodies = [
            valid + github.SUMMARY_START_MARKER,
            valid + github.GENERATED_START_MARKER,
            "{}\n{}\n{}\n{}".format(
                github.GENERATED_START_MARKER,
                github.SUMMARY_START_MARKER,
                github.SUMMARY_END_MARKER,
                github.GENERATED_END_MARKER,
            ),
        ]
        for body in malformed_bodies:
            with self.subTest(body=body):
                with self.assertRaises(github.GitHubError):
                    github.replace_managed_summary(body, "summary")

    def test_marker_helpers_reject_markers_embedded_in_canonical_content(self):
        with self.assertRaisesRegex(github.GitHubError, "reviewed summary contains a managed marker"):
            github.build_managed_body("summary " + github.GENERATED_END_MARKER)


if __name__ == "__main__":
    unittest.main()
