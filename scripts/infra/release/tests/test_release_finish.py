from __future__ import annotations

import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))
sys.path.insert(0, str(Path(__file__).resolve().parent))

import gitrepo_helpers as helpers
from release_common import ConflictError, PlanError
from release_git import GitRepository
import release_finish as finish
import release_github as gh
from release_github import PullRequestRef


class FakeGitHubClient:
    def __init__(self):
        self.releases: dict[str, gh.ReleaseInfo] = {}
        self.generated_notes_calls: list[tuple[str, str, str | None]] = []

    def find_open_pull_request(self, *, head, base):
        return None

    def create_pull_request(self, *, head, base, title, body):
        raise NotImplementedError

    def create_ref(self, *, repository, ref, sha):
        raise NotImplementedError

    def ref_sha(self, *, repository, ref):
        return None

    def get_release(self, tag):
        return self.releases.get(tag)

    def generate_notes(self, *, tag, target_commitish, previous_tag):
        self.generated_notes_calls.append((tag, target_commitish, previous_tag))
        return {"body": "## What's Changed\n* did stuff"}

    def create_draft(self, *, tag, title, target_commitish, body, prerelease):
        self.releases[tag] = gh.ReleaseInfo(
            tag_name=tag, name=title, is_draft=True, is_prerelease=prerelease,
            target_commitish=target_commitish, body=body, url=f"https://example.invalid/{tag}",
        )

    def update_release_body(self, *, tag, body):
        existing = self.releases[tag]
        self.releases[tag] = gh.ReleaseInfo(
            tag_name=existing.tag_name, name=existing.name, is_draft=existing.is_draft,
            is_prerelease=existing.is_prerelease, target_commitish=existing.target_commitish,
            body=body, url=existing.url,
        )

    def publish_release(self, *, tag, title, body):
        existing = self.releases[tag]
        self.releases[tag] = gh.ReleaseInfo(
            tag_name=existing.tag_name, name=title, is_draft=False,
            is_prerelease=existing.is_prerelease, target_commitish=existing.target_commitish,
            body=body, url=existing.url,
        )

    def dispatch_workflow(self, *, workflow, ref, inputs):
        raise NotImplementedError


def make_finish_plan(*, tag="v3.119.0-preview.1", title="Version 3.119.0 (Preview 1)", stable=False, source_commit="a" * 40):
    return {
        "schemaVersion": 1,
        "operation": "finish",
        "generatedAt": "2024-01-01T00:00:00Z",
        "toolingSha": "b" * 40,
        "input": {"requestedVersion": "3.119.0-preview.1.42"},
        "receipt": {
            "skiaSharpVersion": "3.119.0-preview.1.42",
            "base": "3.119.0",
            "label": "preview.1",
            "buildRevision": "42",
            "sourceCommit": source_commit,
            "sourceBranch": "release/3.119.0-preview.1",
            "harfBuzzSharpVersion": "1.8.8.1-preview.1.42",
            "packages": [],
        },
        "release": {
            "raw": "3.119.0-preview.1", "numeric": "3.119.0", "label": "preview.1",
            "releaseType": "preview", "stable": stable, "title": title, "tag": tag,
        },
        "tag": {"name": tag, "targetCommit": source_commit, "existingSha": None, "status": "pending"},
        "previousTag": "v3.118.0",
        "draft": {"exists": False, "isPublished": False, "status": "pending"},
        "warnings": [],
    }


class CreateDraftTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self):
        self._tmp.cleanup()

    def test_pushes_tag_and_creates_draft(self):
        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "file.txt").write_text("x", encoding="utf-8")
        sha = helpers.commit_all(worktree, "init")
        helpers.push(worktree)
        repo = GitRepository(root=worktree)
        repo.fetch()
        plan = make_finish_plan(source_commit=sha)
        github = FakeGitHubClient()
        result = finish.create_draft(plan, repo=repo, github=github)
        self.assertEqual(result["draft"], "done")
        self.assertFalse(result["alreadyExists"])
        self.assertEqual(repo.remote_tags().get("v3.119.0-preview.1"), sha)
        self.assertIn("v3.119.0-preview.1", github.releases)

    def test_reapplying_after_draft_exists_is_idempotent(self):
        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "file.txt").write_text("x", encoding="utf-8")
        sha = helpers.commit_all(worktree, "init")
        helpers.push(worktree)
        repo = GitRepository(root=worktree)
        repo.fetch()
        plan = make_finish_plan(source_commit=sha)
        github = FakeGitHubClient()
        finish.create_draft(plan, repo=repo, github=github)
        second = finish.create_draft(plan, repo=repo, github=github)
        self.assertTrue(second["alreadyExists"])

    def test_conflicting_tag_blocks(self):
        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "file.txt").write_text("x", encoding="utf-8")
        sha = helpers.commit_all(worktree, "init")
        (worktree / "file.txt").write_text("y", encoding="utf-8")
        other_sha = helpers.commit_all(worktree, "second")
        helpers.push(worktree)
        repo = GitRepository(root=worktree)
        repo.push_tag("v3.119.0-preview.1", other_sha)
        plan = make_finish_plan(source_commit=sha)
        github = FakeGitHubClient()
        with self.assertRaises(gh.GitHubError):
            finish.create_draft(plan, repo=repo, github=github)

    def test_conflicting_existing_release_blocks(self):
        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "file.txt").write_text("x", encoding="utf-8")
        sha = helpers.commit_all(worktree, "init")
        helpers.push(worktree)
        repo = GitRepository(root=worktree)
        repo.fetch()
        github = FakeGitHubClient()
        github.releases["v3.119.0-preview.1"] = gh.ReleaseInfo(
            tag_name="v3.119.0-preview.1", name="Wrong Title", is_draft=True, is_prerelease=True,
            target_commitish=sha, body="", url="https://example.invalid",
        )
        plan = make_finish_plan(source_commit=sha)
        with self.assertRaises(gh.GitHubError):
            finish.create_draft(plan, repo=repo, github=github)


class PublicationPlanAndPublishTests(unittest.TestCase):
    def test_plan_publication_reports_readiness(self):
        github = FakeGitHubClient()
        tag = "v3.119.0-preview.1"
        github.releases[tag] = gh.ReleaseInfo(
            tag_name=tag, name="Version 3.119.0 (Preview 1)", is_draft=True, is_prerelease=True,
            target_commitish="a" * 40, body=gh.build_initial_body("notes"), url="https://example.invalid",
        )
        plan = make_finish_plan()
        result = finish.plan_publication(plan, github=github)
        self.assertTrue(result["readyToPublish"])
        self.assertTrue(result["hasManagedMarkers"])

    def test_plan_publication_requires_existing_draft(self):
        github = FakeGitHubClient()
        plan = make_finish_plan()
        with self.assertRaises(PlanError):
            finish.plan_publication(plan, github=github)

    def test_plan_publication_rejects_target_mismatch(self):
        github = FakeGitHubClient()
        tag = "v3.119.0-preview.1"
        github.releases[tag] = gh.ReleaseInfo(
            tag_name=tag, name="Version 3.119.0 (Preview 1)", is_draft=True, is_prerelease=True,
            target_commitish="c" * 40, body=gh.build_initial_body("notes"), url="https://example.invalid",
        )
        plan = make_finish_plan()
        with self.assertRaises(ConflictError):
            finish.plan_publication(plan, github=github)

    def test_publish_publishes_matching_draft(self):
        github = FakeGitHubClient()
        tag = "v3.119.0-preview.1"
        source_commit = "a" * 40
        body = gh.build_initial_body("notes")
        github.releases[tag] = gh.ReleaseInfo(
            tag_name=tag, name="Version 3.119.0 (Preview 1)", is_draft=True, is_prerelease=True,
            target_commitish=source_commit, body=body, url="https://example.invalid",
        )
        plan = make_finish_plan(source_commit=source_commit)
        publication = finish.plan_publication(plan, github=github)
        result = finish.publish(plan, publication, github=github)
        self.assertEqual(result["status"], "published")
        self.assertFalse(github.releases[tag].is_draft)

    def test_publish_already_published_matching_release_is_success(self):
        github = FakeGitHubClient()
        tag = "v3.119.0-preview.1"
        source_commit = "a" * 40
        github.releases[tag] = gh.ReleaseInfo(
            tag_name=tag, name="Version 3.119.0 (Preview 1)", is_draft=False, is_prerelease=True,
            target_commitish=source_commit, body="already published", url="https://example.invalid",
        )
        plan = make_finish_plan(source_commit=source_commit)
        publication = {"bodySha256": gh.body_sha256("irrelevant")}
        result = finish.publish(plan, publication, github=github)
        self.assertEqual(result["status"], "already-published")

    def test_publish_rejects_body_changed_since_plan(self):
        github = FakeGitHubClient()
        tag = "v3.119.0-preview.1"
        source_commit = "a" * 40
        body = gh.build_initial_body("notes")
        github.releases[tag] = gh.ReleaseInfo(
            tag_name=tag, name="Version 3.119.0 (Preview 1)", is_draft=True, is_prerelease=True,
            target_commitish=source_commit, body=body, url="https://example.invalid",
        )
        plan = make_finish_plan(source_commit=source_commit)
        publication = {"bodySha256": "stale-hash"}
        with self.assertRaises(ConflictError):
            finish.publish(plan, publication, github=github)


if __name__ == "__main__":
    unittest.main()
