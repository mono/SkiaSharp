import importlib.util
import tempfile
import unittest
from pathlib import Path


SCRIPT = Path(__file__).with_name("fetch-approved-context.py")
SPEC = importlib.util.spec_from_file_location("fetch_approved_context", SCRIPT)
assert SPEC is not None and SPEC.loader is not None
MODULE = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(MODULE)


def issue(
    number,
    *,
    state="open",
    comments=0,
    body="body",
    labels=None,
    pull_request=False,
):
    value = {
        "number": number,
        "title": f"Issue {number}",
        "html_url": f"https://github.com/mono/SkiaSharp-API-docs/issues/{number}",
        "state": state,
        "user": {"login": f"author-{number}"},
        "labels": [
            {"name": label}
            for label in (labels or ["approved-for-context", "documentation"])
        ],
        "created_at": f"2026-01-{number:02d}T00:00:00Z",
        "updated_at": f"2026-02-{number:02d}T00:00:00Z",
        "closed_at": (
            f"2026-03-{number:02d}T00:00:00Z" if state == "closed" else None
        ),
        "body": body,
        "comments": comments,
    }
    if pull_request:
        value["pull_request"] = {"url": "https://api.github.test/pull/1"}
    return value


def comment(issue_number, suffix, created_at):
    return {
        "user": {"login": f"commenter-{suffix}"},
        "body": f"comment body {suffix}",
        "created_at": created_at,
        "updated_at": created_at,
        "html_url": (
            "https://github.com/mono/SkiaSharp-API-docs/issues/"
            f"{issue_number}#issuecomment-{suffix}"
        ),
    }


class FakeClient:
    def __init__(self, responses):
        self.responses = responses
        self.calls = []

    def get_pages(self, endpoint):
        self.calls.append(endpoint)
        return self.responses.get(endpoint, [])


class FetchApprovedContextTests(unittest.TestCase):
    def test_fetches_all_states_excludes_pull_requests_and_sorts(self):
        issue_endpoint = (
            "repos/mono/SkiaSharp-API-docs/issues?"
            "state=all&labels=approved-for-context&per_page=100"
        )
        comment_endpoint = (
            "repos/mono/SkiaSharp-API-docs/issues/2/comments?per_page=100"
        )
        client = FakeClient(
            {
                issue_endpoint: [
                    issue(2, state="closed", comments=2, labels=[
                        "documentation",
                        "approved-for-context",
                    ]),
                    issue(3, pull_request=True),
                    issue(1),
                ],
                comment_endpoint: [
                    comment(2, 20, "2026-04-02T00:00:00Z"),
                    comment(2, 10, "2026-04-01T00:00:00Z"),
                ],
            }
        )

        context = MODULE.fetch_context(client)

        self.assertEqual(1, context["schema_version"])
        self.assertEqual("mono/SkiaSharp-API-docs", context["repository"])
        self.assertEqual("approved-for-context", context["label"])
        self.assertEqual([1, 2], [item["number"] for item in context["issues"]])
        self.assertEqual("closed", context["issues"][1]["state"])
        self.assertEqual(
            [
                "https://github.com/mono/SkiaSharp-API-docs/issues/"
                "2#issuecomment-10",
                "https://github.com/mono/SkiaSharp-API-docs/issues/"
                "2#issuecomment-20",
            ],
            [item["url"] for item in context["issues"][1]["comments"]],
        )
        self.assertNotIn("fetched_at", context)
        self.assertNotIn("pull_request", str(context))

    def test_rejects_issue_count_over_bound_before_comment_fetch(self):
        issue_endpoint = (
            "repos/mono/SkiaSharp-API-docs/issues?"
            "state=all&labels=approved-for-context&per_page=100"
        )
        client = FakeClient({issue_endpoint: [issue(1), issue(2)]})

        with self.assertRaisesRegex(
            MODULE.ContextFetchError, "issue count 2 exceeds limit 1"
        ):
            MODULE.fetch_context(client, max_issues=1)

        self.assertEqual([issue_endpoint], client.calls)

    def test_rejects_incomplete_comment_pagination(self):
        issue_endpoint = (
            "repos/mono/SkiaSharp-API-docs/issues?"
            "state=all&labels=approved-for-context&per_page=100"
        )
        comment_endpoint = (
            "repos/mono/SkiaSharp-API-docs/issues/1/comments?per_page=100"
        )
        client = FakeClient(
            {
                issue_endpoint: [issue(1, comments=2)],
                comment_endpoint: [comment(1, 1, "2026-04-01T00:00:00Z")],
            }
        )

        with self.assertRaisesRegex(
            MODULE.ContextFetchError, "expected 2 comments but retrieved 1"
        ):
            MODULE.fetch_context(client)

    def test_canonical_bytes_are_stable_and_utf8(self):
        context = {
            "issues": [{"number": 1, "body": "Résumé"}],
            "label": "approved-for-context",
            "repository": "mono/SkiaSharp-API-docs",
            "schema_version": 1,
        }

        first = MODULE.canonical_json(context)
        second = MODULE.canonical_json(dict(reversed(list(context.items()))))

        self.assertEqual(first, second)
        self.assertIn("Résumé".encode("utf-8"), first)
        self.assertTrue(first.endswith(b"\n"))

    def test_atomic_write_preserves_existing_output_when_too_large(self):
        with tempfile.TemporaryDirectory() as temporary_directory:
            output = Path(temporary_directory) / "context.json"
            output.write_text("existing\n", encoding="utf-8")
            context = {
                "schema_version": 1,
                "repository": "mono/SkiaSharp-API-docs",
                "label": "approved-for-context",
                "issues": [{"body": "x" * 100}],
            }

            with self.assertRaisesRegex(
                MODULE.ContextFetchError, "exceeds limit"
            ):
                MODULE.write_context(context, output, max_bytes=10)

            self.assertEqual("existing\n", output.read_text(encoding="utf-8"))
            self.assertEqual([output], list(output.parent.iterdir()))


if __name__ == "__main__":
    unittest.main()
