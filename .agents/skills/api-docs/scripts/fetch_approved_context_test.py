import argparse
import importlib.util
import io
import json
import subprocess
import tempfile
import unittest
from pathlib import Path


SCRIPT = Path(__file__).with_name("fetch-approved-context.py")
SPEC = importlib.util.spec_from_file_location("fetch_approved_context", SCRIPT)
assert SPEC is not None and SPEC.loader is not None
MODULE = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(MODULE)

REPOSITORY = "mono/SkiaSharp-API-docs"
LABEL = "approved-for-context"
ISSUE_ENDPOINT = (
    f"repos/{REPOSITORY}/issues?state=all&labels={LABEL}&per_page=100"
)


def issue(
    number,
    *,
    state="open",
    comments=0,
    body="issue secret body",
    labels=None,
    pull_request=False,
):
    value = {
        "number": number,
        "title": f"Issue {number}\nwith title",
        "html_url": f"https://github.com/{REPOSITORY}/issues/{number}",
        "state": state,
        "user": {"login": f"author-{number}"},
        "labels": [
            {"name": name}
            for name in (labels or [LABEL, "documentation"])
        ],
        "created_at": f"2026-01-{number:02d}T00:00:00Z",
        "updated_at": f"2026-02-{number:02d}T00:00:00Z",
        "body": body,
        "comments": comments,
    }
    if pull_request:
        value["pull_request"] = {"url": "https://api.github.test/pull/1"}
    return value


def comment(issue_number, identifier, created_at):
    return {
        "id": identifier,
        "user": {"login": f"commenter-{identifier}"},
        "body": f"comment secret body {identifier}",
        "created_at": created_at,
        "updated_at": created_at,
        "html_url": (
            f"https://github.com/{REPOSITORY}/issues/{issue_number}"
            f"#issuecomment-{identifier}"
        ),
    }


class FakeClient:
    def __init__(self, responses=None, error=None):
        self.responses = responses or {}
        self.error = error
        self.calls = []

    def get_pages(self, endpoint):
        self.calls.append(endpoint)
        if self.error is not None:
            raise self.error
        return self.responses.get(endpoint, [])


def args(output, *, max_issues=50, max_bytes=1048576):
    return argparse.Namespace(
        repository=REPOSITORY,
        label=LABEL,
        output=output,
        max_issues=max_issues,
        max_bytes=max_bytes,
    )


class FetchApprovedContextTests(unittest.TestCase):
    def test_all_states_pr_filtering_comment_order_and_stable_bytes(self):
        comments_endpoint = f"repos/{REPOSITORY}/issues/2/comments?per_page=100"
        client = FakeClient(
            {
                ISSUE_ENDPOINT: [
                    issue(2, state="closed", comments=2),
                    issue(3, pull_request=True),
                    issue(1),
                ],
                comments_endpoint: [
                    comment(2, 20, "2026-04-02T00:00:00Z"),
                    comment(2, 11, "2026-04-01T00:00:00Z"),
                    comment(2, 10, "2026-04-01T00:00:00Z"),
                ][0:2],
            }
        )
        client.responses[comments_endpoint] = [
            comment(2, 20, "2026-04-02T00:00:00Z"),
            comment(2, 11, "2026-04-01T00:00:00Z"),
        ]

        context = MODULE.fetch_context(
            client, REPOSITORY, LABEL, max_issues=50
        )
        first = MODULE.canonical_json(context)
        second = MODULE.canonical_json(json.loads(first))

        self.assertEqual(1, context["schemaVersion"])
        self.assertEqual([1, 2], [item["number"] for item in context["issues"]])
        self.assertEqual("closed", context["issues"][1]["state"])
        self.assertEqual(
            [11, 20],
            [item["id"] for item in context["issues"][1]["comments"]],
        )
        self.assertEqual(first, second)
        self.assertTrue(first.endswith(b"\n"))

    def test_gh_client_flattens_paginated_pages(self):
        payload = json.dumps([[{"number": 1}], [{"number": 2}]])

        def runner(*_args, **_kwargs):
            return subprocess.CompletedProcess([], 0, payload, "")

        client = MODULE.GhApiClient(runner)
        self.assertEqual(
            [{"number": 1}, {"number": 2}],
            client.get_pages("endpoint"),
        )

    def test_issue_and_byte_bounds_fail_without_output(self):
        with tempfile.TemporaryDirectory() as temporary_directory:
            output = Path(temporary_directory) / "context.json"
            output.write_text("stale\n", encoding="utf-8")
            stdout = io.StringIO()
            stderr = io.StringIO()
            client = FakeClient(
                {ISSUE_ENDPOINT: [issue(1), issue(2)]}
            )

            result = MODULE.run(
                args(output, max_issues=1),
                client=client,
                stdout=stdout,
                stderr=stderr,
            )

            self.assertEqual(1, result)
            self.assertFalse(output.exists())
            self.assertEqual("", stdout.getvalue())
            self.assertIn("exceeds limit", stderr.getvalue())

            result = MODULE.run(
                args(output, max_bytes=10),
                client=FakeClient({ISSUE_ENDPOINT: [issue(1)]}),
                stdout=stdout,
                stderr=stderr,
            )
            self.assertEqual(1, result)
            self.assertFalse(output.exists())

    def test_api_and_pagination_failures_remove_stale_output(self):
        with tempfile.TemporaryDirectory() as temporary_directory:
            output = Path(temporary_directory) / "context.json"
            for client, expected in (
                (
                    FakeClient(error=MODULE.ContextFetchError("API failed")),
                    "API failed",
                ),
                (
                    FakeClient(
                        {
                            ISSUE_ENDPOINT: [issue(1, comments=1)],
                            (
                                f"repos/{REPOSITORY}/issues/1/comments"
                                "?per_page=100"
                            ): [],
                        }
                    ),
                    "pagination may be incomplete",
                ),
            ):
                output.write_text("stale\n", encoding="utf-8")
                stderr = io.StringIO()
                result = MODULE.run(
                    args(output),
                    client=client,
                    stdout=io.StringIO(),
                    stderr=stderr,
                )
                self.assertEqual(1, result)
                self.assertFalse(output.exists())
                self.assertIn(expected, stderr.getvalue())
                self.assertEqual([], list(output.parent.glob("*.tmp")))

    def test_empty_result_and_manifest_redact_bodies(self):
        with tempfile.TemporaryDirectory() as temporary_directory:
            output = Path(temporary_directory) / "context.json"
            stdout = io.StringIO()
            stderr = io.StringIO()

            result = MODULE.run(
                args(output),
                client=FakeClient({ISSUE_ENDPOINT: []}),
                stdout=stdout,
                stderr=stderr,
            )

            self.assertEqual(0, result)
            self.assertEqual("", stderr.getvalue())
            self.assertTrue(output.exists())
            self.assertIn(
                f"CONTEXT | {REPOSITORY} | {LABEL} | 0 | ",
                stdout.getvalue(),
            )

    def test_manifest_has_sanitized_rows_and_no_bodies(self):
        with tempfile.TemporaryDirectory() as temporary_directory:
            output = Path(temporary_directory) / "context.json"
            stdout = io.StringIO()
            stderr = io.StringIO()
            client = FakeClient({ISSUE_ENDPOINT: [issue(1)]})

            result = MODULE.run(
                args(output),
                client=client,
                stdout=stdout,
                stderr=stderr,
            )

            log = stdout.getvalue()
            self.assertEqual(0, result)
            self.assertEqual(2, len(log.splitlines()))
            self.assertIn("ISSUE | 1 | open |", log)
            self.assertIn("Issue 1 with title", log)
            self.assertNotIn("issue secret body", log)
            self.assertNotIn("comment secret body", log)


if __name__ == "__main__":
    unittest.main()
