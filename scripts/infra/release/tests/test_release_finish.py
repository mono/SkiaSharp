from __future__ import annotations

import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))
sys.path.insert(0, str(Path(__file__).resolve().parent))

import gitrepo_helpers as helpers
import release_common as common
from release_common import ConflictError, PlanError, with_digest
from release_git import GitRepository
import release_finish as finish
import release_github as gh
import release_milestones as milestones
import release_nuget as nuget
from release_github import PullRequestRef
from test_release_nuget import FakeNuGetClient, FakeSignatureVerifier, build_nupkg, catalog_entry_for


class FakeGitHubClient:
    def __init__(self):
        self.releases: dict[str, gh.ReleaseInfo] = {}
        self.generated_notes_calls: list[tuple[str, str, str | None]] = []
        self.dispatch_calls: list[dict] = []
        # A workflow name in this set causes the *next* dispatch_workflow
        # call for it to raise (and be removed from the set), simulating a
        # transient failure (crash/network blip/expired token) so tests can
        # exercise retry/recovery on a subsequent apply_closeout call.
        self.fail_next_dispatch_for: set[str] = set()

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
        if workflow in self.fail_next_dispatch_for:
            self.fail_next_dispatch_for.discard(workflow)
            raise gh.GitHubError(f"simulated transient dispatch failure for {workflow}")
        self.dispatch_calls.append({"workflow": workflow, "ref": ref, "inputs": dict(inputs)})


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
        self.assertEqual(result["draftStatus"], "done")
        self.assertEqual(result["tag"], "v3.119.0-preview.1")
        self.assertEqual(result["tagStatus"], "done")
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

    def test_migrates_existing_marker_less_draft_instead_of_oscillating(self):
        # Item 6 regression: an unpublished draft created by hand (or an
        # older tool version) before managed markers existed used to make
        # create-draft <-> plan-publication oscillate forever (create-draft
        # saw *any* existing draft as "already done" -> plan-publication,
        # but plan-publication saw no markers -> back to create-draft).
        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "file.txt").write_text("x", encoding="utf-8")
        sha = helpers.commit_all(worktree, "init")
        helpers.push(worktree)
        repo = GitRepository(root=worktree)
        repo.fetch()
        plan = make_finish_plan(source_commit=sha)
        github = FakeGitHubClient()
        legacy_body = "## What's Changed\n* a hand-written or pre-marker release body"
        github.releases["v3.119.0-preview.1"] = gh.ReleaseInfo(
            tag_name="v3.119.0-preview.1", name="Version 3.119.0 (Preview 1)", is_draft=True,
            is_prerelease=True, target_commitish=sha, body=legacy_body, url="https://example.invalid",
        )

        result = finish.create_draft(plan, repo=repo, github=github)

        self.assertTrue(result["migrated"])
        self.assertTrue(result["alreadyExists"])
        self.assertFalse(result["isPublished"])
        self.assertEqual(result["nextAction"], "plan-publication")
        migrated_body = github.releases["v3.119.0-preview.1"].body
        self.assertTrue(gh.has_managed_markers(migrated_body))
        self.assertIn(legacy_body, migrated_body)  # the original notes are preserved, not discarded

        # This must actually converge now: plan-publication reports ready,
        # not another bounce back to create-draft.
        publication = finish.plan_publication(plan, github=github)
        self.assertTrue(publication["hasManagedMarkers"])
        self.assertTrue(publication["readyToPublish"])
        self.assertEqual(publication["nextAction"], "publish")

        # Re-running create-draft again (idempotency) must not re-migrate.
        second = finish.create_draft(plan, repo=repo, github=github)
        self.assertFalse(second["migrated"])
        self.assertTrue(second["alreadyExists"])

    def test_never_rewrites_a_published_marker_less_release(self):
        # A legacy *published* release without managed markers must never
        # be rewritten by create-draft -- migration only ever applies to an
        # unpublished draft.
        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "file.txt").write_text("x", encoding="utf-8")
        sha = helpers.commit_all(worktree, "init")
        helpers.push(worktree)
        repo = GitRepository(root=worktree)
        repo.fetch()
        plan = make_finish_plan(source_commit=sha)
        github = FakeGitHubClient()
        published_body = "## What's Changed\n* already shipped, no markers"
        github.releases["v3.119.0-preview.1"] = gh.ReleaseInfo(
            tag_name="v3.119.0-preview.1", name="Version 3.119.0 (Preview 1)", is_draft=False,
            is_prerelease=True, target_commitish=sha, body=published_body, url="https://example.invalid",
        )

        result = finish.create_draft(plan, repo=repo, github=github)

        self.assertTrue(result["isPublished"])
        self.assertFalse(result["migrated"])
        self.assertEqual(result["nextAction"], "closeout")
        self.assertEqual(github.releases["v3.119.0-preview.1"].body, published_body)  # untouched


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

    def test_plan_publication_tolerates_published_release_with_legacy_branch_target(self):
        # Item 3 regression: an already-published release with a legacy
        # branch-name target_commitish (e.g. "main") must reconcile through
        # plan_publication just like it does through check_release_conflict
        # in create-draft -- the tag is authoritative once it exists, not
        # target_commitish. Must not be treated more strictly here.
        github = FakeGitHubClient()
        tag = "v3.119.0"
        source_commit = "a" * 40
        github.releases[tag] = gh.ReleaseInfo(
            tag_name=tag, name="Version 3.119.0", is_draft=False, is_prerelease=False,
            target_commitish="main", body=gh.build_initial_body("notes"), url="https://example.invalid",
        )
        plan = make_finish_plan(tag=tag, title="Version 3.119.0", stable=True, source_commit=source_commit)
        result = finish.plan_publication(plan, github=github)
        self.assertTrue(result["isPublished"])
        self.assertEqual(result["nextAction"], "closeout")

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

    def test_publish_tolerates_already_published_release_with_legacy_branch_target(self):
        # Item 3 regression: publish's already-published short-circuit must
        # use the same tolerant target_commitish rule as plan_publication
        # and create-draft, not a stricter open-coded equality check.
        github = FakeGitHubClient()
        tag = "v3.119.0"
        source_commit = "a" * 40
        github.releases[tag] = gh.ReleaseInfo(
            tag_name=tag, name="Version 3.119.0", is_draft=False, is_prerelease=False,
            target_commitish="main", body="already published", url="https://example.invalid",
        )
        plan = make_finish_plan(tag=tag, title="Version 3.119.0", stable=True, source_commit=source_commit)
        publication = {"bodySha256": gh.body_sha256("irrelevant")}
        result = finish.publish(plan, publication, github=github)
        self.assertEqual(result["status"], "already-published")
        self.assertEqual(result["nextAction"], "closeout")

    def test_publish_still_rejects_already_published_release_with_a_different_exact_sha(self):
        # A genuine SHA-vs-SHA disagreement on an already-published release
        # remains a hard conflict, unlike the legacy branch-name case above.
        github = FakeGitHubClient()
        tag = "v3.119.0"
        github.releases[tag] = gh.ReleaseInfo(
            tag_name=tag, name="Version 3.119.0", is_draft=False, is_prerelease=False,
            target_commitish="c" * 40, body="already published", url="https://example.invalid",
        )
        plan = make_finish_plan(tag=tag, title="Version 3.119.0", stable=True, source_commit="a" * 40)
        publication = {"bodySha256": gh.body_sha256("irrelevant")}
        with self.assertRaises(ConflictError):
            finish.publish(plan, publication, github=github)

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
        publication = finish.plan_publication(plan, github=github)
        # The draft body changes after the publication report was approved
        # -- publish must detect this even though the publication report
        # itself is otherwise valid and bound to this exact plan.
        github.update_release_body(tag=tag, body=gh.build_initial_body("different notes"))
        with self.assertRaisesRegex(ConflictError, "body changed"):
            finish.publish(plan, publication, github=github)

    def test_publish_rejects_publication_from_a_different_plan(self):
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
        # Same tag (so the draft lookup still succeeds) but a genuinely
        # different plan document -> a different planDigest.
        other_plan = make_finish_plan(source_commit=source_commit, title="Version 9.9.9 (Different)")
        with self.assertRaisesRegex(ConflictError, "different plan"):
            finish.publish(other_plan, publication, github=github)

    def test_publish_rejects_publication_for_a_different_tag(self):
        github = FakeGitHubClient()
        tag = "v3.119.0-preview.1"
        source_commit = "a" * 40
        body = gh.build_initial_body("notes")
        github.releases[tag] = gh.ReleaseInfo(
            tag_name=tag, name="Version 3.119.0 (Preview 1)", is_draft=True, is_prerelease=True,
            target_commitish=source_commit, body=body, url="https://example.invalid",
        )
        plan = make_finish_plan(source_commit=source_commit)
        publication = dict(finish.plan_publication(plan, github=github))
        publication["tag"] = "v9.9.9"
        with self.assertRaisesRegex(ConflictError, "for tag"):
            finish.publish(plan, publication, github=github)

    def test_publish_rejects_publication_without_managed_markers(self):
        github = FakeGitHubClient()
        tag = "v3.119.0-preview.1"
        source_commit = "a" * 40
        github.releases[tag] = gh.ReleaseInfo(
            tag_name=tag, name="Version 3.119.0 (Preview 1)", is_draft=True, is_prerelease=True,
            target_commitish=source_commit, body="legacy notes, no markers", url="https://example.invalid",
        )
        plan = make_finish_plan(source_commit=source_commit)
        publication = finish.plan_publication(plan, github=github)
        self.assertFalse(publication["hasManagedMarkers"])
        self.assertFalse(publication["readyToPublish"])
        self.assertEqual(publication["nextAction"], "create-draft")
        with self.assertRaisesRegex(ConflictError, "managed marker"):
            finish.publish(plan, publication, github=github)

    def test_publish_rejects_a_hand_built_publication_missing_required_fields(self):
        # A plausible-looking but incomplete stand-in (e.g. hand-built in a
        # test, or a corrupted file) must not be accepted just because it
        # happens to carry a matching bodySha256.
        github = FakeGitHubClient()
        tag = "v3.119.0-preview.1"
        source_commit = "a" * 40
        body = gh.build_initial_body("notes")
        github.releases[tag] = gh.ReleaseInfo(
            tag_name=tag, name="Version 3.119.0 (Preview 1)", is_draft=True, is_prerelease=True,
            target_commitish=source_commit, body=body, url="https://example.invalid",
        )
        plan = make_finish_plan(source_commit=source_commit)
        publication = {"bodySha256": gh.body_sha256(body)}
        with self.assertRaises(ConflictError):
            finish.publish(plan, publication, github=github)


class FakeMilestoneClient:
    def __init__(self):
        self._milestones: list = []
        self.open_items: dict[int, list] = {}
        self.moved: list[tuple[int, int]] = []
        self.closed: list[int] = []
        self.pr_milestones: dict[int, str | None] = {}
        self.issue_milestones: dict[int, str | None] = {}
        self.pr_closing_issues: dict[int, list[int]] = {}
        self.milestones_call_count = 0
        self.created: list[str] = []
        self.updated: list[tuple[int, str, str]] = []
        self._next_number = 1000

    def milestones(self):
        self.milestones_call_count += 1
        return list(self._milestones)

    def create_milestone(self, title: str, *, due_on: str | None, description: str | None):
        number = self._next_number
        self._next_number += 1
        milestone = milestones.Milestone(number=number, title=title, state="open", due_on=due_on, description=description)
        self._milestones.append(milestone)
        self.created.append(title)
        return milestone

    def update_milestone(self, number: int, *, due_on: str, description: str) -> None:
        self.updated.append((number, due_on, description))
        for index, m in enumerate(self._milestones):
            if m.number == number:
                self._milestones[index] = milestones.Milestone(
                    number=m.number, title=m.title, state=m.state, due_on=due_on, description=description
                )

    def open_milestone_items(self, milestone_number: int):
        return self.open_items.get(milestone_number, [])

    def _milestone_title(self, milestone_number: int) -> str | None:
        for m in self._milestones:
            if m.number == milestone_number:
                return m.title
        return None

    def update_item_milestone(self, item_number: int, milestone_number: int) -> None:
        self.moved.append((item_number, milestone_number))
        for number, items in list(self.open_items.items()):
            self.open_items[number] = [item for item in items if item.number != item_number]
        title = self._milestone_title(milestone_number)
        if item_number in self.pr_milestones:
            self.pr_milestones[item_number] = title
        if item_number in self.issue_milestones:
            self.issue_milestones[item_number] = title

    def close_milestone(self, milestone_number: int) -> None:
        self.closed.append(milestone_number)
        for m in self._milestones:
            if m.number == milestone_number:
                self._milestones[self._milestones.index(m)] = milestones.Milestone(
                    number=m.number, title=m.title, state="closed", due_on=m.due_on, description=m.description
                )

    def closing_issues(self, pull_request_number: int):
        return self.pr_closing_issues.get(pull_request_number, [])

    def pull_request_milestone(self, pull_request_number: int) -> str | None:
        return self.pr_milestones.get(pull_request_number)

    def issue_milestone(self, issue_number: int) -> str | None:
        return self.issue_milestones.get(issue_number)


class FakeScheduleClient:
    """A ScheduleClient stand-in: seeded schedules by milestone number,
    raising release_milestones.MilestoneError (matching the real client's
    behavior on a network/data problem) for anything unseeded."""

    def __init__(self):
        self.schedules: dict[int, dict] = {}
        self.calls: list[int] = []

    def seed(
        self,
        milestone: int,
        *,
        branch_point: str,
        earliest_beta: str,
        early_stable_cut: str,
        early_stable: str,
        stable_cut: str,
        stable_date: str,
    ) -> None:
        self.schedules[milestone] = {
            "branch_point": branch_point,
            "earliest_beta": earliest_beta,
            "early_stable_cut": early_stable_cut,
            "early_stable": early_stable,
            "stable_cut": stable_cut,
            "stable_date": stable_date,
        }

    def fetch_schedule(self, milestone: int) -> dict:
        self.calls.append(milestone)
        if milestone not in self.schedules:
            raise milestones.MilestoneError(f"no schedule seeded for m{milestone} (simulated)")
        return self.schedules[milestone]


# Used by every closeout test that is not itself about schedule maintenance:
# paired with schedule_count=0, this is never actually called (the fetch
# loop's range is empty), so a single shared instance is safe to reuse.
_NULL_SCHEDULE_CLIENT = FakeScheduleClient()


class CloseoutTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self):
        self._tmp.cleanup()

    def _repo_with_history(self, *, tag_previous: bool = True):
        """A real repo: an initial commit (optionally tagged v3.118.0), then
        a PR-referencing commit that closes an issue, tagged as the shipped
        release commit (matching make_finish_plan()'s default source_commit
        is NOT used here -- callers read back the real SHA instead). Also
        seeds scripts/VERSIONS.txt with "SkiaSharp nuget 3.119.0" (major 3)
        and "libSkiaSharp milestone 119" so schedule-maintenance tests can
        read a current major/milestone from origin/main."""

        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "scripts").mkdir(parents=True, exist_ok=True)
        (worktree / "scripts" / "VERSIONS.txt").write_text(
            "# nuget versions\n"
            "# SkiaSharp\n"
            "SkiaSharp                nuget       3.119.0\n"
            "# HarfBuzzSharp\n"
            "HarfBuzzSharp            nuget       1.8.8\n"
            "libSkiaSharp             milestone   119\n",
            encoding="utf-8",
        )
        (worktree / "file.txt").write_text("v1", encoding="utf-8")
        first_sha = helpers.commit_all(worktree, "Initial commit")
        repo = GitRepository(root=worktree)
        if tag_previous:
            repo.push_tag("v3.118.0", first_sha)
        (worktree / "file.txt").write_text("v2", encoding="utf-8")
        second_sha = helpers.commit_all(worktree, "Fix the thing (#100)")
        helpers.push(worktree)
        repo.fetch()
        return repo, first_sha, second_sha

    def _ship_plan(self, repo: GitRepository, github: "FakeGitHubClient", plan: dict) -> None:
        """Satisfy _require_release_is_shipped for ``plan``: push its exact
        tag to its exact source commit, and register a matching, published
        (not draft) GitHub release for it. Every plan_closeout/apply_closeout
        test must call this (with a plan whose source_commit is a real
        commit in the fixture repo -- an arbitrary fake SHA can never be
        pushed as a real tag) before exercising closeout, since closeout now
        always reverifies live shipped state before touching milestones."""

        tag = plan["tag"]["name"]
        source_commit = plan["receipt"]["sourceCommit"]
        repo.push_tag(tag, source_commit)
        github.releases[tag] = gh.ReleaseInfo(
            tag_name=tag, name=plan["release"]["title"], is_draft=False,
            is_prerelease=not plan["release"]["stable"], target_commitish=source_commit,
            body="notes", url=f"https://example.invalid/{tag}",
        )

    def test_plan_closeout_reports_done_when_nothing_to_move(self):
        repo, _, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit)
        github = FakeGitHubClient()
        self._ship_plan(repo, github, plan)
        client = FakeMilestoneClient()
        client._milestones = [
            milestones.Milestone(number=1, title="3.119.0", state="open"),
            milestones.Milestone(number=2, title="3.119.1", state="open"),
        ]
        result = finish.plan_closeout(
            plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=["v3.119.0"]
        )
        self.assertEqual(result["nextAction"], "closeout")
        self.assertEqual(result["planDigest"], plan["planDigest"])
        self.assertEqual(result["release"]["branch"], "release/3.119.0-preview.1")
        self.assertEqual(len(result["operations"]), 1)
        self.assertEqual(result["operations"][0]["status"], "pending")
        # No milestone titled "3.119.0-preview.1" (the plan's release
        # identity) exists, so reconciliation is skipped with a warning
        # rather than failing.
        self.assertEqual(result["reconcileOperations"], [])
        self.assertTrue(any("3.119.0-preview.1" in w for w in result["warnings"]))

    def test_plan_closeout_next_action_done_when_nothing_shipped(self):
        repo, _, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit)
        github = FakeGitHubClient()
        self._ship_plan(repo, github, plan)
        client = FakeMilestoneClient()
        client._milestones = [milestones.Milestone(number=1, title="3.119.0", state="open")]
        result = finish.plan_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])
        self.assertEqual(result["operations"], [])
        self.assertEqual(result["nextAction"], "done")

    def test_plan_closeout_next_action_blocked(self):
        repo, _, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit)
        github = FakeGitHubClient()
        self._ship_plan(repo, github, plan)
        client = FakeMilestoneClient()
        client._milestones = [milestones.Milestone(number=1, title="3.119.0", state="open")]
        client.open_items[1] = [milestones.MilestoneItem(number=5, title="x", url="u", kind="issue")]
        result = finish.plan_closeout(
            plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=["v3.119.0"]
        )
        self.assertEqual(result["nextAction"], "blocked")
        self.assertEqual(result["operations"][0]["status"], "blocked")

    def test_plan_closeout_includes_release_notes_dispatch_inputs(self):
        repo, _, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit)
        github = FakeGitHubClient()
        self._ship_plan(repo, github, plan)
        client = FakeMilestoneClient()
        result = finish.plan_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])
        self.assertEqual(
            result["releaseNotesDispatch"],
            {"source_branch": "main", "min_version": "3.119.0", "max_version": "3.119.0", "force": "false"},
        )
        self.assertFalse(result["issueTemplateRefreshNeeded"])  # default plan is a preview, not stable

    def test_plan_closeout_issue_template_refresh_needed_for_stable(self):
        repo, _, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit, stable=True)
        github = FakeGitHubClient()
        self._ship_plan(repo, github, plan)
        client = FakeMilestoneClient()
        result = finish.plan_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])
        self.assertTrue(result["issueTemplateRefreshNeeded"])

    def test_apply_closeout_moves_items_and_closes(self):
        repo, _, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit)
        github = FakeGitHubClient()
        self._ship_plan(repo, github, plan)
        client = FakeMilestoneClient()
        client._milestones = [
            milestones.Milestone(number=1, title="3.119.0", state="open"),
            milestones.Milestone(number=2, title="3.119.1", state="open"),
        ]
        client.open_items[1] = [milestones.MilestoneItem(number=5, title="x", url="u", kind="issue")]
        result = finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=["v3.119.0"])
        self.assertEqual(client.moved, [(5, 2)])
        self.assertEqual(client.closed, [1])
        self.assertEqual(result["nextAction"], "done")
        self.assertEqual(result["planDigest"], plan["planDigest"])

    def test_apply_closeout_is_idempotent_when_rerun(self):
        repo, _, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit)
        github = FakeGitHubClient()
        self._ship_plan(repo, github, plan)
        client = FakeMilestoneClient()
        client._milestones = [
            milestones.Milestone(number=1, title="3.119.0", state="open"),
            milestones.Milestone(number=2, title="3.119.1", state="open"),
        ]
        first = finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=["v3.119.0"])
        self.assertEqual(first["nextAction"], "done")
        second = finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=["v3.119.0"])
        self.assertEqual(second["results"], [])
        self.assertEqual(second["nextAction"], "done")

    def test_apply_closeout_reports_blocked_without_raising(self):
        repo, _, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit)
        github = FakeGitHubClient()
        self._ship_plan(repo, github, plan)
        client = FakeMilestoneClient()
        client._milestones = [milestones.Milestone(number=1, title="3.119.0", state="open")]
        client.open_items[1] = [milestones.MilestoneItem(number=5, title="x", url="u", kind="issue")]
        result = finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=["v3.119.0"])
        self.assertEqual(result["nextAction"], "blocked")
        self.assertEqual(result["results"][0]["status"], "blocked")

    def test_apply_closeout_reconciles_merged_pr_and_linked_issue_before_advancing(self):
        repo, previous_sha, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit)
        github = FakeGitHubClient()
        self._ship_plan(repo, github, plan)
        client = FakeMilestoneClient()
        client._milestones = [milestones.Milestone(number=9, title="3.119.0-preview.1", state="open")]
        client.pr_milestones[100] = None
        client.pr_closing_issues[100] = [200]
        client.issue_milestones[200] = None

        result = finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])

        self.assertEqual(client.pr_milestones[100], "3.119.0-preview.1")
        self.assertEqual(client.issue_milestones[200], "3.119.0-preview.1")
        kinds = {(item["kind"], item["number"]) for item in result["reconcileResults"]}
        self.assertEqual(kinds, {("pull-request", 100), ("issue", 200)})

    def test_apply_closeout_dispatches_release_notes_and_issue_template_when_work_done(self):
        repo, previous_sha, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit, stable=True)
        github = FakeGitHubClient()
        self._ship_plan(repo, github, plan)
        client = FakeMilestoneClient()
        client._milestones = [milestones.Milestone(number=9, title="3.119.0-preview.1", state="open")]
        client.pr_milestones[100] = None

        result = finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])

        self.assertEqual(len(github.dispatch_calls), 2)
        notes_call = github.dispatch_calls[0]
        self.assertEqual(notes_call["workflow"], finish.UPDATE_RELEASE_NOTES_WORKFLOW)
        self.assertEqual(notes_call["ref"], "main")
        self.assertEqual(
            notes_call["inputs"],
            {"source_branch": "main", "min_version": "3.119.0", "max_version": "3.119.0", "force": "false"},
        )
        template_call = github.dispatch_calls[1]
        self.assertEqual(template_call["workflow"], finish.ISSUE_TEMPLATE_REFRESH_WORKFLOW)
        self.assertEqual(
            {d["status"] for d in result["dispatches"]}, {"dispatched"}
        )

    def test_apply_closeout_does_not_dispatch_issue_template_for_non_stable_release(self):
        repo, previous_sha, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit, stable=False)
        github = FakeGitHubClient()
        self._ship_plan(repo, github, plan)
        client = FakeMilestoneClient()
        client._milestones = [milestones.Milestone(number=9, title="3.119.0-preview.1", state="open")]
        client.pr_milestones[100] = None

        finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])

        self.assertEqual(len(github.dispatch_calls), 1)
        self.assertEqual(github.dispatch_calls[0]["workflow"], finish.UPDATE_RELEASE_NOTES_WORKFLOW)

    def test_apply_closeout_dispatches_release_notes_even_with_no_milestone_work(self):
        # A first closeout for a release with no milestone reconcile or
        # advance work at all (no matching milestone, nothing shipped) must
        # still generate its release notes -- the dispatch must never be
        # gated on "did any milestone activity happen".
        repo, _, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit, stable=True)
        github = FakeGitHubClient()
        self._ship_plan(repo, github, plan)
        client = FakeMilestoneClient()
        client._milestones = [milestones.Milestone(number=1, title="3.119.0", state="open")]

        result = finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])

        self.assertEqual(len(github.dispatch_calls), 2)
        self.assertEqual({d["status"] for d in result["dispatches"]}, {"dispatched"})
        self.assertEqual(github.dispatch_calls[0]["workflow"], finish.UPDATE_RELEASE_NOTES_WORKFLOW)
        self.assertEqual(github.dispatch_calls[1]["workflow"], finish.ISSUE_TEMPLATE_REFRESH_WORKFLOW)

    def test_apply_closeout_redispatches_on_rerun_with_no_new_work(self):
        # Rerunning after everything is already reconciled/advanced must
        # still redispatch both workflows on every successful invocation --
        # they are convergent/idempotent, so a repeated dispatch is safer
        # than hidden missing state.
        repo, previous_sha, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit, stable=True)
        github = FakeGitHubClient()
        self._ship_plan(repo, github, plan)
        client = FakeMilestoneClient()
        client._milestones = [milestones.Milestone(number=9, title="3.119.0-preview.1", state="open")]
        client.pr_milestones[100] = None

        first = finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])
        second = finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])

        self.assertEqual(len(github.dispatch_calls), 4)  # 2 workflows x 2 runs
        self.assertEqual({d["status"] for d in first["dispatches"]}, {"dispatched"})
        self.assertEqual({d["status"] for d in second["dispatches"]}, {"dispatched"})

    def test_apply_closeout_dispatch_recovers_after_a_transient_failure(self):
        # A dispatch failure that happens *after* the milestone writes for
        # this invocation already succeeded used to be unrecoverable: a
        # rerun would see no new reconcile/advance work left to do and
        # silently skip the dispatch too, leaving a published release with
        # no notes and no way to recover except by hand. Retrying now
        # always redispatches regardless of whether there is new milestone
        # work, so it converges.
        repo, previous_sha, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit, stable=True)
        github = FakeGitHubClient()
        self._ship_plan(repo, github, plan)
        client = FakeMilestoneClient()
        client._milestones = [milestones.Milestone(number=9, title="3.119.0-preview.1", state="open")]
        client.pr_milestones[100] = None
        github.fail_next_dispatch_for.add(finish.UPDATE_RELEASE_NOTES_WORKFLOW)

        with self.assertRaises(gh.GitHubError):
            finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])
        self.assertEqual(github.dispatch_calls, [])
        # The milestone reconciliation from the failed attempt still landed.
        self.assertEqual(client.pr_milestones[100], "3.119.0-preview.1")

        second = finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])

        self.assertEqual(len(github.dispatch_calls), 2)
        self.assertEqual({d["status"] for d in second["dispatches"]}, {"dispatched"})

    def test_reconciliation_skipped_with_warning_when_target_milestone_missing(self):
        repo, _, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit)
        github = FakeGitHubClient()
        self._ship_plan(repo, github, plan)
        client = FakeMilestoneClient()
        client._milestones = []  # no milestone at all

        result = finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])
        self.assertEqual(result["reconcileResults"], [])
        self.assertTrue(any("no milestone titled" in w for w in result["warnings"]))

    def test_apply_closeout_rejects_missing_tag(self):
        # The plan's tag was never pushed at all (e.g. create-draft/publish
        # never ran, or a caller hand-built/tampered a finish plan): closeout
        # must refuse before ever touching the milestone client.
        repo, _, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit)
        github = FakeGitHubClient()  # no release registered, and no tag pushed
        client = FakeMilestoneClient()
        client._milestones = [milestones.Milestone(number=9, title="3.119.0-preview.1", state="open")]

        with self.assertRaisesRegex(ConflictError, "does not exist on the remote"):
            finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])
        self.assertEqual(client.milestones_call_count, 0)
        self.assertEqual(client.moved, [])
        self.assertEqual(client.closed, [])

        with self.assertRaisesRegex(ConflictError, "does not exist on the remote"):
            finish.plan_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])
        self.assertEqual(client.milestones_call_count, 0)

    def test_apply_closeout_rejects_moved_tag(self):
        # The tag exists but points somewhere other than the exact package
        # source commit -- e.g. it was moved/re-tagged after publish, or the
        # plan is stale. This must never be silently tolerated the way an
        # already-published legacy branch-name target_commitish is (see
        # check_release_conflict): the tag is the one thing this tool never
        # force-updates, so a moved tag is always a hard conflict.
        repo, _, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit)
        github = FakeGitHubClient()
        tag = plan["tag"]["name"]
        # Push the tag to a *different* real commit than the plan's receipt.
        (repo.root / "file.txt").write_text("v3", encoding="utf-8")
        other_sha = helpers.commit_all(repo.root, "a different, later commit")
        repo.push_tag(tag, other_sha)
        github.releases[tag] = gh.ReleaseInfo(
            tag_name=tag, name=plan["release"]["title"], is_draft=False,
            is_prerelease=not plan["release"]["stable"], target_commitish=other_sha,
            body="notes", url=f"https://example.invalid/{tag}",
        )
        client = FakeMilestoneClient()
        client._milestones = [milestones.Milestone(number=9, title="3.119.0-preview.1", state="open")]

        with self.assertRaisesRegex(ConflictError, "expected the package source commit"):
            finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])
        self.assertEqual(client.milestones_call_count, 0)

    def test_apply_closeout_rejects_missing_release(self):
        # The tag is correct, but no GitHub release exists for it at all
        # (e.g. create-draft never ran even though the tag was somehow
        # pushed): closeout must refuse before touching milestones.
        repo, _, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit)
        tag = plan["tag"]["name"]
        repo.push_tag(tag, source_commit)
        github = FakeGitHubClient()  # no release registered for `tag`
        client = FakeMilestoneClient()
        client._milestones = [milestones.Milestone(number=9, title="3.119.0-preview.1", state="open")]

        with self.assertRaisesRegex(ConflictError, "no GitHub release exists"):
            finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])
        self.assertEqual(client.milestones_call_count, 0)

    def test_apply_closeout_rejects_draft_only_release(self):
        # A draft (not yet published) release must not let closeout run:
        # "finish closeout is a public CLI and must not close milestones
        # merely because a caller supplied a valid pre-publication finish
        # plan."
        repo, _, source_commit = self._repo_with_history()
        plan = make_finish_plan(source_commit=source_commit)
        tag = plan["tag"]["name"]
        repo.push_tag(tag, source_commit)
        github = FakeGitHubClient()
        github.releases[tag] = gh.ReleaseInfo(
            tag_name=tag, name=plan["release"]["title"], is_draft=True,
            is_prerelease=not plan["release"]["stable"], target_commitish=source_commit,
            body="notes", url=f"https://example.invalid/{tag}",
        )
        client = FakeMilestoneClient()
        client._milestones = [milestones.Milestone(number=9, title="3.119.0-preview.1", state="open")]

        with self.assertRaisesRegex(ConflictError, "unpublished draft"):
            finish.apply_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])
        self.assertEqual(client.milestones_call_count, 0)

        with self.assertRaisesRegex(ConflictError, "unpublished draft"):
            finish.plan_closeout(plan, repo=repo, milestone_client=client, github=github, schedule_client=_NULL_SCHEDULE_CLIENT, schedule_count=0, tags=[])
        self.assertEqual(client.milestones_call_count, 0)


    def test_resolve_reconciliation_range_returns_none_for_first_ever_release(self):
        repo, _, source_commit = self._repo_with_history(tag_previous=False)
        result = finish.resolve_reconciliation_range(repo, previous_tag=None, source_commit=source_commit)
        self.assertIsNone(result)

    def test_resolve_reconciliation_range_fails_explicitly_when_tag_unresolvable(self):
        repo, _, source_commit = self._repo_with_history()
        with self.assertRaises(ConflictError):
            finish.resolve_reconciliation_range(
                repo, previous_tag="v0.0.0-does-not-exist", source_commit=source_commit
            )

    def test_resolve_reconciliation_range_fails_explicitly_when_tag_not_an_ancestor(self):
        repo, previous_sha, source_commit = self._repo_with_history()
        # Tag a commit that is NOT an ancestor of source_commit: a sibling
        # branch off the very first commit.
        (repo.root / "other.txt").write_text("x", encoding="utf-8")
        repo.git("checkout", "--detach", previous_sha)
        sibling_sha = helpers.commit_all(repo.root, "unrelated sibling commit")
        repo.push_tag("v3.118.5-unrelated", sibling_sha)
        with self.assertRaises(ConflictError):
            finish.resolve_reconciliation_range(
                repo, previous_tag="v3.118.5-unrelated", source_commit=source_commit
            )


class ScheduleMaintenanceIntegrationTests(unittest.TestCase):
    """Integration-level tests for the closeout schedule step wired into
    plan_closeout/apply_closeout: creating/updating the upcoming preview/
    RC/stable milestones from the Chromium/Skia schedule, and never letting
    a schedule-fetch failure block the rest of closeout (reconciliation,
    milestone rollover, or either workflow dispatch)."""

    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self):
        self._tmp.cleanup()

    def _repo(self):
        """A real repo whose origin/main has scripts/VERSIONS.txt declaring
        major=3, current Skia milestone=119, and an already-shipped,
        published release/tag/GitHub-release for "3.119.0-preview.1"."""

        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "scripts").mkdir(parents=True, exist_ok=True)
        (worktree / "scripts" / "VERSIONS.txt").write_text(
            "# nuget versions\n"
            "# SkiaSharp\n"
            "SkiaSharp                nuget       3.119.0\n"
            "# HarfBuzzSharp\n"
            "HarfBuzzSharp            nuget       1.8.8\n"
            "libSkiaSharp             milestone   119\n",
            encoding="utf-8",
        )
        first_sha = helpers.commit_all(worktree, "Initial commit")
        repo = GitRepository(root=worktree)
        repo.push_tag("v3.118.0", first_sha)  # matches make_finish_plan()'s default previousTag
        (worktree / "file.txt").write_text("v2", encoding="utf-8")
        source_commit = helpers.commit_all(worktree, "Fix the thing (#100)")
        helpers.push(worktree)
        repo.fetch()
        return repo, source_commit

    def _ship(self, repo, github, plan):
        tag = plan["tag"]["name"]
        source_commit = plan["receipt"]["sourceCommit"]
        repo.push_tag(tag, source_commit)
        github.releases[tag] = gh.ReleaseInfo(
            tag_name=tag, name=plan["release"]["title"], is_draft=False,
            is_prerelease=not plan["release"]["stable"], target_commitish=source_commit,
            body="notes", url=f"https://example.invalid/{tag}",
        )

    SCHEDULE_119 = {
        "branch_point": "2026-07-27T00:00:00",
        "earliest_beta": "2026-07-29T00:00:00",
        "early_stable_cut": "2026-08-11T00:00:00",
        "early_stable": "2026-08-12T00:00:00",
        "stable_cut": "2026-08-18T00:00:00",
        "stable_date": "2026-08-25T00:00:00",
    }

    def test_apply_closeout_creates_missing_schedule_milestones(self):
        repo, source_commit = self._repo()
        plan = make_finish_plan(source_commit=source_commit)
        github = FakeGitHubClient()
        self._ship(repo, github, plan)
        milestone_client = FakeMilestoneClient()
        schedule_client = FakeScheduleClient()
        schedule_client.seed(119, **self.SCHEDULE_119)

        result = finish.apply_closeout(
            plan, repo=repo, milestone_client=milestone_client, github=github,
            schedule_client=schedule_client, schedule_count=1, tags=[],
        )

        self.assertEqual(schedule_client.calls, [119])
        self.assertEqual(
            sorted(milestone_client.created),
            ["3.119.0", "3.119.0-preview.1", "3.119.0-preview.2", "3.119.0-rc.1"],
        )
        schedule_titles = {r["title"]: r["status"] for r in result["scheduleResults"]}
        self.assertTrue(all(status == "done" for status in schedule_titles.values()))

    def test_apply_closeout_updates_schedule_milestone_with_wrong_due_date(self):
        repo, source_commit = self._repo()
        plan = make_finish_plan(source_commit=source_commit)
        github = FakeGitHubClient()
        self._ship(repo, github, plan)
        milestone_client = FakeMilestoneClient()
        # Pre-seed "3.119.0-preview.1" already existing but with a stale due date.
        milestone_client._milestones = [
            milestones.Milestone(
                number=42, title="3.119.0-preview.1", state="open",
                due_on="2020-01-01T00:00:00Z", description="stale description",
            )
        ]
        schedule_client = FakeScheduleClient()
        schedule_client.seed(119, **self.SCHEDULE_119)

        finish.apply_closeout(
            plan, repo=repo, milestone_client=milestone_client, github=github,
            schedule_client=schedule_client, schedule_count=1, tags=[],
        )

        self.assertEqual(len(milestone_client.updated), 1)
        number, due_on, description = milestone_client.updated[0]
        self.assertEqual(number, 42)
        self.assertEqual(due_on, "2026-07-29T00:00:00Z")
        self.assertNotEqual(description, "stale description")
        self.assertNotIn("3.119.0-preview.1", milestone_client.created)

    def test_apply_closeout_rerun_is_idempotent_for_schedule(self):
        repo, source_commit = self._repo()
        plan = make_finish_plan(source_commit=source_commit)
        github = FakeGitHubClient()
        self._ship(repo, github, plan)
        milestone_client = FakeMilestoneClient()
        schedule_client = FakeScheduleClient()
        schedule_client.seed(119, **self.SCHEDULE_119)

        finish.apply_closeout(
            plan, repo=repo, milestone_client=milestone_client, github=github,
            schedule_client=schedule_client, schedule_count=1, tags=[],
        )
        created_count = len(milestone_client.created)
        self.assertGreater(created_count, 0)

        second = finish.apply_closeout(
            plan, repo=repo, milestone_client=milestone_client, github=github,
            schedule_client=schedule_client, schedule_count=1, tags=[],
        )

        # No new creates/updates the second time: everything already matches.
        self.assertEqual(len(milestone_client.created), created_count)
        self.assertEqual(milestone_client.updated, [])
        self.assertTrue(all(r["status"] == "done" for r in second["scheduleResults"]))

    def test_schedule_fetch_failure_does_not_block_reconciliation_or_dispatch(self):
        # Do not couple docs dispatch to schedule success: an unreachable
        # Chromium schedule endpoint must not prevent PR/issue
        # reconciliation, milestone rollover, or either workflow dispatch --
        # it is recorded as an explicit warning only.
        repo, source_commit = self._repo()
        plan = make_finish_plan(source_commit=source_commit, stable=True)
        github = FakeGitHubClient()
        self._ship(repo, github, plan)
        milestone_client = FakeMilestoneClient()
        milestone_client._milestones = [milestones.Milestone(number=9, title="3.119.0-preview.1", state="open")]
        milestone_client.pr_milestones[100] = None
        schedule_client = FakeScheduleClient()  # nothing seeded -> every fetch fails

        result = finish.apply_closeout(
            plan, repo=repo, milestone_client=milestone_client, github=github,
            schedule_client=schedule_client, schedule_count=2, tags=[],
        )

        self.assertEqual(milestone_client.created, [])
        self.assertEqual(milestone_client.updated, [])
        self.assertTrue(any("could not fetch Chromium schedule" in w for w in result["warnings"]))
        # Reconciliation still happened.
        self.assertEqual(milestone_client.pr_milestones[100], "3.119.0-preview.1")
        # Both dispatches still fired.
        self.assertEqual(len(github.dispatch_calls), 2)
        self.assertEqual({d["status"] for d in result["dispatches"]}, {"dispatched"})

    def test_plan_closeout_previews_schedule_without_writing(self):
        repo, source_commit = self._repo()
        plan = make_finish_plan(source_commit=source_commit)
        github = FakeGitHubClient()
        self._ship(repo, github, plan)
        milestone_client = FakeMilestoneClient()
        schedule_client = FakeScheduleClient()
        schedule_client.seed(119, **self.SCHEDULE_119)

        result = finish.plan_closeout(
            plan, repo=repo, milestone_client=milestone_client, github=github,
            schedule_client=schedule_client, schedule_count=1, tags=[],
        )

        self.assertEqual(milestone_client.created, [])  # a preview never writes
        self.assertEqual(milestone_client.updated, [])
        actions = {op["title"]: op["action"] for op in result["scheduleOperations"]}
        self.assertEqual(
            actions,
            {
                "3.119.0-preview.1": "create",
                "3.119.0-preview.2": "create",
                "3.119.0-rc.1": "create",
                "3.119.0": "create",
            },
        )
        self.assertEqual(result["nextAction"], "closeout")


class BuildPendingReportTests(unittest.TestCase):
    """Opus 5 must-fix/operability item 5: the machine-readable pending
    report a ``NotReadyError`` produces for ``finish plan --output``."""

    def test_pending_report_carries_missing_packages_and_deadline_context(self):
        from release_common import NotReadyError

        error = NotReadyError(
            "2 package(s) not yet visible/listed on NuGet.org after 1200s "
            "(deadline 1200s): SkiaSharp.Extra 3.119.0, HarfBuzzSharp 1.8.8.1; "
            "rerun once indexing completes",
            missing=(
                {"id": "SkiaSharp.Extra", "version": "3.119.0"},
                {"id": "HarfBuzzSharp", "version": "1.8.8.1"},
            ),
            elapsed_seconds=1200.0,
            deadline_seconds=1200.0,
        )
        report = finish.build_pending_report(
            requested_version="3.119.0", tooling_sha="a" * 40, error=error
        )
        self.assertEqual(report["nextAction"], "pending")
        self.assertEqual(report["requestedVersion"], "3.119.0")
        self.assertEqual(report["toolingSha"], "a" * 40)
        self.assertEqual(
            report["missingPackages"],
            [
                {"id": "SkiaSharp.Extra", "version": "3.119.0"},
                {"id": "HarfBuzzSharp", "version": "1.8.8.1"},
            ],
        )
        self.assertEqual(report["elapsedSeconds"], 1200.0)
        self.assertEqual(report["deadlineSeconds"], 1200.0)
        self.assertIn("SkiaSharp.Extra", report["message"])
        common.validate_against_schema(report, finish.FINISH_PENDING_SCHEMA)


class BuildFinishPlanNextActionTests(unittest.TestCase):
    """Plan-level regressions for ``build_finish_plan`` itself (the actual
    ``finish plan`` entry point), not just ``plan_publication``'s own
    internal ``next_action`` rule: ``finish plan`` previously emitted
    ``plan-publication`` for *any* existing draft without inspecting its
    body for managed markers, so a workflow driven purely by ``finish
    plan``'s ``nextAction`` would skip ``create-draft`` entirely and could
    never reach the marker-less-draft migration path there. Uses a real,
    throwaway git repository (exercising the actual ``GitVersionsFileReader``
    ``build_finish_plan`` hard-codes) plus a fake in-memory NuGet client, so
    this is a genuine end-to-end plan-generation test, not just a unit test
    of an extracted helper.
    """

    SOURCE_BRANCH = "release/3.119.0-preview.1"
    REQUESTED_VERSION = "3.119.0-preview.1.42"
    TAG = "v3.119.0-preview.1"
    TITLE = "Version 3.119.0 (Preview 1)"
    MANIFEST = {"anchorPackages": ["SkiaSharp", "SkiaSharp.HarfBuzz", "HarfBuzzSharp"]}

    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self):
        self._tmp.cleanup()

    def _repo(self) -> tuple[GitRepository, str]:
        _, worktree = helpers.create_bare_and_worktree(self.root, "finish-plan")
        (worktree / "scripts").mkdir(parents=True, exist_ok=True)
        (worktree / "scripts" / "VERSIONS.txt").write_text(
            "# nuget versions\n"
            "# SkiaSharp\n"
            "SkiaSharp                nuget       3.119.0\n"
            "SkiaSharp.HarfBuzz       nuget       3.119.0\n"
            "# HarfBuzzSharp\n"
            "HarfBuzzSharp            nuget       1.8.8\n",
            encoding="utf-8",
        )
        (worktree / "scripts" / "azure-templates-variables.yml").write_text(
            "variables:\n"
            "  SKIASHARP_VERSION: 3.119.0\n"
            "  PREVIEW_LABEL: 'preview.1'\n",
            encoding="utf-8",
        )
        source_commit = helpers.commit_all(worktree, "Initial commit")
        repo = GitRepository(root=worktree)
        repo.git("branch", self.SOURCE_BRANCH, source_commit)
        repo.push_branch(self.SOURCE_BRANCH)
        repo.fetch()
        return repo, source_commit

    def _nuget_client(self, *, source_commit: str) -> FakeNuGetClient:
        client = FakeNuGetClient()
        for package_id, version, dependency_groups in (
            ("SkiaSharp", self.REQUESTED_VERSION, None),
            (
                "SkiaSharp.HarfBuzz", self.REQUESTED_VERSION,
                [("net8.0", [("HarfBuzzSharp", "1.8.8-preview.1.42")])],
            ),
            ("HarfBuzzSharp", "1.8.8-preview.1.42", None),
        ):
            nupkg = build_nupkg(
                package_id, version, commit=source_commit, branch=self.SOURCE_BRANCH,
                dependency_groups=dependency_groups,
            )
            # catalog_entry_for() derives its own repository field from the
            # nupkg's embedded nuspec by default, so the two stay
            # consistent without needing to specify it again here.
            client.add(package_id, version, nupkg, entry=catalog_entry_for(
                nupkg, package_id=package_id, version=version,
            ))
        return client

    def _build_plan(self, *, github_factory) -> dict:
        repo, source_commit = self._repo()
        client = self._nuget_client(source_commit=source_commit)
        github = github_factory(source_commit)
        return finish.build_finish_plan(
            requested_version=self.REQUESTED_VERSION,
            nuget_client=client,
            repo=repo,
            github=github,
            manifest=self.MANIFEST,
            fingerprints=("dummy-fingerprint",),
            signature_verifier=FakeSignatureVerifier(),
            download_dir=self.root / "downloads",
            tooling_sha="b" * 40,
        )

    def test_no_release_yet_routes_to_create_draft(self):
        plan = self._build_plan(github_factory=lambda source_commit: FakeGitHubClient())
        self.assertEqual(plan["nextAction"], "create-draft")
        self.assertFalse(plan["draft"]["exists"])
        self.assertFalse(plan["draft"]["hasManagedMarkers"])

    def test_unpublished_draft_without_managed_markers_routes_to_create_draft(self):
        # Item regression: this is the exact case that previously fell
        # through to plan-publication, even though only create_draft knows
        # how to migrate a marker-less draft in place.
        def _github_factory(source_commit):
            github = FakeGitHubClient()
            github.releases[self.TAG] = gh.ReleaseInfo(
                tag_name=self.TAG, name=self.TITLE, is_draft=True, is_prerelease=True,
                target_commitish=source_commit, body="legacy notes, no markers", url="https://example.invalid",
            )
            return github

        plan = self._build_plan(github_factory=_github_factory)
        self.assertEqual(plan["nextAction"], "create-draft")
        self.assertTrue(plan["draft"]["exists"])
        self.assertFalse(plan["draft"]["isPublished"])
        self.assertFalse(plan["draft"]["hasManagedMarkers"])

    def test_unpublished_draft_with_managed_markers_routes_to_plan_publication(self):
        def _github_factory(source_commit):
            github = FakeGitHubClient()
            github.releases[self.TAG] = gh.ReleaseInfo(
                tag_name=self.TAG, name=self.TITLE, is_draft=True, is_prerelease=True,
                target_commitish=source_commit, body=gh.build_initial_body("notes"),
                url="https://example.invalid",
            )
            return github

        plan = self._build_plan(github_factory=_github_factory)
        self.assertEqual(plan["nextAction"], "plan-publication")
        self.assertTrue(plan["draft"]["exists"])
        self.assertFalse(plan["draft"]["isPublished"])
        self.assertTrue(plan["draft"]["hasManagedMarkers"])

    def test_published_release_routes_to_closeout(self):
        def _github_factory(source_commit):
            github = FakeGitHubClient()
            github.releases[self.TAG] = gh.ReleaseInfo(
                tag_name=self.TAG, name=self.TITLE, is_draft=False, is_prerelease=True,
                target_commitish=source_commit, body=gh.build_initial_body("notes"),
                url="https://example.invalid",
            )
            return github

        plan = self._build_plan(github_factory=_github_factory)
        self.assertEqual(plan["nextAction"], "closeout")
        self.assertTrue(plan["draft"]["exists"])
        self.assertTrue(plan["draft"]["isPublished"])


if __name__ == "__main__":
    unittest.main()
