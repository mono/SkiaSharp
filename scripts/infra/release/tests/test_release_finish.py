from __future__ import annotations

import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))
sys.path.insert(0, str(Path(__file__).resolve().parent))

import gitrepo_helpers as helpers
from release_common import ConflictError, PlanError, with_digest
from release_git import GitRepository
import release_finish as finish
import release_github as gh
import release_milestones as milestones
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
    plan = {
        "schemaVersion": 1,
        "operation": "finish",
        "generatedAt": "2024-01-01T00:00:00Z",
        "toolingSha": "b" * 40,
        "nextAction": "create-draft",
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
            "identity": "3.119.0-preview.1",
            "version": "3.119.0-preview.1.42",
            "branch": "release/3.119.0-preview.1",
            "raw": "3.119.0-preview.1", "numeric": "3.119.0", "label": "preview.1",
            "releaseType": "preview", "stable": stable, "title": title, "tag": tag,
        },
        "tag": {"name": tag, "targetCommit": source_commit, "existingSha": None, "status": "pending"},
        "previousTag": "v3.118.0",
        "draft": {"exists": False, "isPublished": False, "status": "pending"},
        "warnings": [],
    }
    return with_digest(plan)


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
        self.assertEqual(result["nextAction"], "plan-publication")
        self.assertEqual(result["planDigest"], plan["planDigest"])
        self.assertEqual(result["release"]["branch"], "release/3.119.0-preview.1")
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
        self.assertEqual(publication["nextAction"], "publish")
        result = finish.publish(plan, publication, github=github)
        self.assertEqual(result["status"], "published")
        self.assertEqual(result["nextAction"], "closeout")
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


class FakeMilestoneClient:
    def __init__(self):
        self._milestones: list = []
        self.open_items: dict[int, list] = {}
        self.moved: list[tuple[int, int]] = []
        self.closed: list[int] = []

    def milestones(self):
        return list(self._milestones)

    def open_milestone_items(self, milestone_number: int):
        return self.open_items.get(milestone_number, [])

    def update_item_milestone(self, item_number: int, milestone_number: int) -> None:
        self.moved.append((item_number, milestone_number))
        for number, items in list(self.open_items.items()):
            self.open_items[number] = [item for item in items if item.number != item_number]

    def close_milestone(self, milestone_number: int) -> None:
        self.closed.append(milestone_number)
        for m in self._milestones:
            if m.number == milestone_number:
                self._milestones[self._milestones.index(m)] = milestones.Milestone(
                    number=m.number, title=m.title, state="closed"
                )

    def closing_issues(self, pull_request_number: int):
        return []


class CloseoutTests(unittest.TestCase):
    def test_plan_closeout_reports_done_when_nothing_to_move(self):
        plan = make_finish_plan()
        client = FakeMilestoneClient()
        client._milestones = [
            milestones.Milestone(number=1, title="3.119.0", state="open"),
            milestones.Milestone(number=2, title="3.119.1", state="open"),
        ]
        result = finish.plan_closeout(plan, milestone_client=client, tags=["v3.119.0"])
        self.assertEqual(result["nextAction"], "closeout")
        self.assertEqual(result["planDigest"], plan["planDigest"])
        self.assertEqual(result["release"]["branch"], "release/3.119.0-preview.1")
        self.assertEqual(len(result["operations"]), 1)
        self.assertEqual(result["operations"][0]["status"], "pending")

    def test_plan_closeout_next_action_done_when_nothing_shipped(self):
        plan = make_finish_plan()
        client = FakeMilestoneClient()
        client._milestones = [milestones.Milestone(number=1, title="3.119.0", state="open")]
        result = finish.plan_closeout(plan, milestone_client=client, tags=[])
        self.assertEqual(result["operations"], [])
        self.assertEqual(result["nextAction"], "done")

    def test_plan_closeout_next_action_blocked(self):
        plan = make_finish_plan()
        client = FakeMilestoneClient()
        client._milestones = [milestones.Milestone(number=1, title="3.119.0", state="open")]
        client.open_items[1] = [milestones.MilestoneItem(number=5, title="x", url="u", kind="issue")]
        result = finish.plan_closeout(plan, milestone_client=client, tags=["v3.119.0"])
        self.assertEqual(result["nextAction"], "blocked")
        self.assertEqual(result["operations"][0]["status"], "blocked")

    def test_apply_closeout_moves_items_and_closes(self):
        plan = make_finish_plan()
        client = FakeMilestoneClient()
        client._milestones = [
            milestones.Milestone(number=1, title="3.119.0", state="open"),
            milestones.Milestone(number=2, title="3.119.1", state="open"),
        ]
        client.open_items[1] = [milestones.MilestoneItem(number=5, title="x", url="u", kind="issue")]
        result = finish.apply_closeout(plan, milestone_client=client, tags=["v3.119.0"])
        self.assertEqual(client.moved, [(5, 2)])
        self.assertEqual(client.closed, [1])
        self.assertEqual(result["nextAction"], "done")
        self.assertEqual(result["planDigest"], plan["planDigest"])

    def test_apply_closeout_is_idempotent_when_rerun(self):
        plan = make_finish_plan()
        client = FakeMilestoneClient()
        client._milestones = [
            milestones.Milestone(number=1, title="3.119.0", state="open"),
            milestones.Milestone(number=2, title="3.119.1", state="open"),
        ]
        first = finish.apply_closeout(plan, milestone_client=client, tags=["v3.119.0"])
        self.assertEqual(first["nextAction"], "done")
        second = finish.apply_closeout(plan, milestone_client=client, tags=["v3.119.0"])
        self.assertEqual(second["results"], [])
        self.assertEqual(second["nextAction"], "done")

    def test_apply_closeout_reports_blocked_without_raising(self):
        plan = make_finish_plan()
        client = FakeMilestoneClient()
        client._milestones = [milestones.Milestone(number=1, title="3.119.0", state="open")]
        client.open_items[1] = [milestones.MilestoneItem(number=5, title="x", url="u", kind="issue")]
        result = finish.apply_closeout(plan, milestone_client=client, tags=["v3.119.0"])
        self.assertEqual(result["nextAction"], "blocked")
        self.assertEqual(result["results"][0]["status"], "blocked")


if __name__ == "__main__":
    unittest.main()
