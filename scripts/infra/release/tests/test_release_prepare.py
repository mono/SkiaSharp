from __future__ import annotations

import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))
sys.path.insert(0, str(Path(__file__).resolve().parent))

import gitrepo_helpers as helpers
import release_common as common
from release_common import PlanError, with_digest
from release_git import GitRepository
from release_github import GitHubClient, GitHubError, PullRequestRef
import release_prepare as prepare


SKIA_SHA = "b" * 40


class FakeGitHubClient:
    """An in-memory stand-in for :class:`release_github.GitHubClient`."""

    def __init__(self):
        self.skia_refs: dict[str, str] = {}
        self.pull_requests: dict[tuple[str, str], PullRequestRef] = {}
        self._next_pr_number = 1

    def find_open_pull_request(self, *, head: str, base: str):
        return self.pull_requests.get((head, base))

    def create_pull_request(self, *, head: str, base: str, title: str, body: str) -> PullRequestRef:
        pr = PullRequestRef(number=self._next_pr_number, url=f"https://example.invalid/pr/{self._next_pr_number}")
        self._next_pr_number += 1
        self.pull_requests[(head, base)] = pr
        return pr

    def create_ref(self, *, repository: str, ref: str, sha: str) -> None:
        key = f"{repository}:{ref}"
        if key in self.skia_refs and self.skia_refs[key] != sha:
            raise GitHubError(f"ref {key} already exists at a different sha")
        self.skia_refs[key] = sha

    def ref_sha(self, *, repository: str, ref: str):
        return self.skia_refs.get(f"{repository}:{ref}")

    def get_release(self, tag: str):
        return None

    def generate_notes(self, *, tag, target_commitish, previous_tag):
        return {"body": "generated"}

    def create_draft(self, **kwargs):
        raise NotImplementedError

    def update_release_body(self, **kwargs):
        raise NotImplementedError

    def publish_release(self, **kwargs):
        raise NotImplementedError

    def dispatch_workflow(self, **kwargs):
        raise NotImplementedError


class PrepareFixture:
    """Builds a throwaway SkiaSharp-shaped repository for prepare tests."""

    def __init__(self, root: Path):
        self.root = root
        self.bare, self.worktree = helpers.create_bare_and_worktree(root, "skiasharp")
        self.repo = GitRepository(root=self.worktree)

    def seed_main(self, *, skiasharp_version: str, harfbuzzsharp_version: str, preview_label: str = "preview.0") -> str:
        helpers.write_variables(
            self.worktree, skiasharp_version=skiasharp_version, preview_label=preview_label
        )
        helpers.write_versions(
            self.worktree, skiasharp_version=skiasharp_version, harfbuzzsharp_version=harfbuzzsharp_version
        )
        helpers.add_gitlink(self.worktree, submodule_path="externals/skia", sha=SKIA_SHA)
        helpers.stage(self.worktree, "scripts", ".gitmodules")
        sha = helpers.commit_staged(self.worktree, "seed main")
        helpers.push(self.worktree, "main")
        return sha

    def create_remote_branch(self, name: str, at: str) -> None:
        self.repo.git("branch", name, at)
        self.repo.push_branch(name)

    def create_advanced_release_branch(
        self,
        name: str,
        at: str,
        *,
        skiasharp_version: str,
        preview_label: str,
    ) -> str:
        self.repo.git("switch", "-c", name, at)
        helpers.write_variables(
            self.worktree,
            skiasharp_version=skiasharp_version,
            preview_label=preview_label,
        )
        helpers.stage(self.worktree, "scripts/azure-templates-variables.yml")
        helpers.commit_staged(self.worktree, f"Bump the version to {skiasharp_version}-{preview_label}")
        (self.worktree / "ci-pool-fix.txt").write_text("follow-up\n", encoding="utf-8")
        helpers.stage(self.worktree, "ci-pool-fix.txt")
        tip = helpers.commit_staged(self.worktree, "Merge main and fix the CI pool")
        self.repo.push_branch(name)
        self.repo.git("switch", "main")
        return tip

    def fetch(self) -> None:
        self.repo.fetch()


class PrepareFirstPrereleaseTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self):
        self._tmp.cleanup()

    def test_first_preview_creates_maintenance_branch_from_main(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.fetch()
        github = FakeGitHubClient()
        plan = prepare.build_prepare_plan(
            fixture.repo,
            integration_target="main",
            requested_version=None,
            tooling_sha=main_sha,
            github=github,
        )
        self.assertEqual(plan["release"]["version"], "3.119.0-preview.1")
        self.assertEqual(plan["release"]["identity"], "3.119.0-preview.1")
        self.assertEqual(plan["release"]["branch"], "release/3.119.0-preview.1")
        self.assertEqual(plan["release"]["integrationBranch"], "release/3.119.x")
        self.assertEqual(plan["base"]["sha"], main_sha)
        self.assertEqual(plan["maintenanceBranch"]["action"], "create")
        self.assertEqual(plan["maintenanceBranch"]["baseSha"], main_sha)
        self.assertFalse(plan["maintenanceBranch"]["exists"])
        self.assertEqual(plan["skia"]["sha"], SKIA_SHA)
        self.assertTrue(any("maintenance branch" in w for w in plan["warnings"]))
        self.assertIsNone(plan["stableBump"])
        self.assertEqual(plan["nextAction"], "apply")
        statuses = {op["id"]: op["status"] for op in plan["operations"]}
        self.assertEqual(statuses["create-maintenance-branch"], "pending")
        self.assertEqual(statuses["create-skia-ref"], "pending")
        self.assertEqual(statuses["create-release-branch"], "pending")

    def test_apply_creates_maintenance_branch_skia_ref_and_release_branch(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.fetch()
        github = FakeGitHubClient()
        plan = prepare.build_prepare_plan(
            fixture.repo,
            integration_target="main",
            requested_version=None,
            tooling_sha=main_sha,
            github=github,
        )
        plan = with_digest(plan)
        report = prepare.apply_prepare_plan(
            plan, repo=fixture.repo, github=github
        )
        self.assertEqual(report["release"]["branch"], "release/3.119.0-preview.1")
        self.assertTrue(fixture.repo.ref_exists("refs/remotes/origin/release/3.119.x"))
        self.assertEqual(fixture.repo.remote_sha("release/3.119.x"), main_sha)
        self.assertEqual(
            github.ref_sha(repository="mono/skia", ref="refs/heads/release/3.119.0-preview.1"),
            SKIA_SHA,
        )
        self.assertTrue(fixture.repo.ref_exists("refs/remotes/origin/release/3.119.0-preview.1"))

        # Re-planning after apply must see everything as already done.
        fixture.fetch()
        replanned = prepare.build_prepare_plan(
            fixture.repo,
            integration_target="main",
            requested_version="3.119.0-preview.1",
            tooling_sha=main_sha,
            github=github,
        )
        statuses = {op["id"]: op["status"] for op in replanned["operations"]}
        self.assertEqual(statuses["create-maintenance-branch"], "done")
        self.assertEqual(statuses["create-skia-ref"], "done")
        self.assertEqual(statuses["create-release-branch"], "done")
        self.assertEqual(replanned["nextAction"], "done")

        # Re-applying is idempotent: no exceptions, no forced updates.
        second_report = prepare.apply_prepare_plan(
            with_digest(replanned), repo=fixture.repo, github=github
        )
        self.assertEqual(second_report["release"]["branch"], "release/3.119.0-preview.1")
        self.assertEqual(second_report["nextAction"], "done")

    def test_apply_rejects_existing_release_branch_not_descended_from_approved_base(self):
        # Item 4 regression: a release branch with the right name but whose
        # head is not a descendant of the approved base (e.g. force-pushed,
        # or created by something else entirely between "prepare plan" and
        # "prepare apply") must fail loudly, never be silently accepted as
        # "already done".
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.fetch()
        github = FakeGitHubClient()
        plan = prepare.build_prepare_plan(
            fixture.repo, integration_target="main", requested_version=None,
            tooling_sha=main_sha, github=github,
        )
        release_branch = plan["release"]["branch"]

        # An orphan commit (no parents) sharing the same tree as main_sha:
        # its content matches, but it is not reachable from main_sha's
        # history, so is_ancestor(main_sha, orphan_sha) is false.
        tree = fixture.repo.git("rev-parse", f"{main_sha}^{{tree}}").stdout.strip()
        orphan_sha = fixture.repo.git("commit-tree", tree, "-m", "unrelated history").stdout.strip()
        fixture.create_remote_branch(release_branch, orphan_sha)
        fixture.fetch()

        with self.assertRaisesRegex(GitHubError, "not a descendant"):
            prepare.apply_prepare_plan(with_digest(plan), repo=fixture.repo, github=github)

    def test_apply_rejects_existing_release_branch_with_wrong_version_state(self):
        # Item 4 regression: a release branch that IS a proper descendant of
        # the approved base but whose version-state files disagree with the
        # plan (e.g. a concurrent/earlier partial apply bumped to the wrong
        # label) must also fail loudly rather than being treated as done.
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.fetch()
        github = FakeGitHubClient()
        plan = prepare.build_prepare_plan(
            fixture.repo, integration_target="main", requested_version=None,
            tooling_sha=main_sha, github=github,
        )
        release_branch = plan["release"]["branch"]

        # A real descendant commit of main_sha, but left at PREVIEW_LABEL
        # preview.5 instead of the plan's expected preview.1.
        fixture.repo.git("branch", release_branch, main_sha)
        fixture.repo.git("checkout", release_branch)
        helpers.write_variables(fixture.worktree, skiasharp_version="3.119.0", preview_label="preview.5")
        helpers.stage(fixture.worktree, "scripts")
        helpers.commit_staged(fixture.worktree, "wrong label")
        fixture.repo.push_branch(release_branch)
        fixture.repo.git("checkout", "main")
        fixture.fetch()

        with self.assertRaisesRegex(GitHubError, "PREVIEW_LABEL"):
            prepare.apply_prepare_plan(with_digest(plan), repo=fixture.repo, github=github)


class PrepareSecondPrereleaseTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self):
        self._tmp.cleanup()

    def test_second_preview_bases_on_existing_maintenance_branch(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.create_remote_branch("release/3.119.x", main_sha)
        fixture.create_remote_branch("release/3.119.0-preview.1", main_sha)
        fixture.fetch()
        github = FakeGitHubClient()
        github.create_ref(repository="mono/skia", ref="refs/heads/release/3.119.0-preview.1", sha=SKIA_SHA)
        plan = prepare.build_prepare_plan(
            fixture.repo,
            integration_target="main",
            requested_version="3.119.0-preview.2",
            tooling_sha=main_sha,
            github=github,
        )
        self.assertEqual(plan["base"]["ref"], "refs/remotes/origin/release/3.119.x")
        self.assertEqual(plan["maintenanceBranch"]["action"], "none")
        self.assertTrue(plan["maintenanceBranch"]["exists"])
        self.assertEqual(plan["warnings"], [])


class PrepareStableTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self):
        self._tmp.cleanup()

    def test_stable_uses_existing_maintenance_branch(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.create_remote_branch("release/3.119.x", main_sha)
        fixture.fetch()
        github = FakeGitHubClient()
        plan = prepare.build_prepare_plan(
            fixture.repo,
            integration_target="main",
            requested_version="3.119.0",
            tooling_sha=main_sha,
            github=github,
        )
        self.assertEqual(plan["base"]["ref"], "refs/remotes/origin/release/3.119.x")
        self.assertEqual(plan["maintenanceBranch"]["action"], "none")
        self.assertIsNotNone(plan["stableBump"])
        self.assertEqual(plan["stableBump"]["skiaSharpVersion"], "3.119.1")
        self.assertEqual(plan["stableBump"]["harfBuzzSharpVersion"], "1.8.8.1")
        self.assertEqual(plan["stableBump"]["status"], "pending")

    def test_stable_recovers_from_matching_prerelease_branch_when_maintenance_missing(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        # The maintenance branch was never created (simulating recovery from a
        # broken run), but an RC branch with later legitimate commits exists.
        rc_tip = fixture.create_advanced_release_branch(
            "release/3.119.0-rc.1",
            main_sha,
            skiasharp_version="3.119.0",
            preview_label="rc.1",
        )
        fixture.fetch()
        github = FakeGitHubClient()
        plan = prepare.build_prepare_plan(
            fixture.repo,
            integration_target="main",
            requested_version="3.119.0",
            tooling_sha=main_sha,
            github=github,
        )
        self.assertEqual(plan["base"]["ref"], "refs/remotes/origin/release/3.119.0-rc.1")
        self.assertEqual(plan["base"]["sha"], rc_tip)
        self.assertEqual(plan["maintenanceBranch"]["action"], "create")
        self.assertEqual(plan["maintenanceBranch"]["baseSha"], main_sha)
        self.assertEqual(plan["stableBump"]["status"], "pending")

    def test_existing_advanced_rc_uses_its_tip_and_creates_maintenance_from_main(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        rc_tip = fixture.create_advanced_release_branch(
            "release/3.119.0-rc.1",
            main_sha,
            skiasharp_version="3.119.0",
            preview_label="rc.1",
        )
        fixture.fetch()
        github = FakeGitHubClient()
        plan = prepare.build_prepare_plan(
            fixture.repo,
            integration_target="main",
            requested_version="3.119.0-rc.1",
            tooling_sha=main_sha,
            github=github,
        )
        self.assertEqual(plan["base"]["sha"], rc_tip)
        self.assertEqual(plan["maintenanceBranch"]["baseSha"], main_sha)
        self.assertEqual(plan["skiaSharpRemoteState"], "matching")
        self.assertEqual(plan["nextAction"], "apply")

    def test_stable_uses_safe_main_when_no_recovery_candidate(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.fetch()
        plan = prepare.build_prepare_plan(
            fixture.repo,
            integration_target="main",
            requested_version="3.119.0",
            tooling_sha=main_sha,
            github=FakeGitHubClient(),
        )
        self.assertEqual(plan["base"]["sha"], main_sha)
        self.assertEqual(plan["maintenanceBranch"]["baseSha"], main_sha)

    def test_unsafe_main_requires_an_approved_preview_zero_base(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(
            skiasharp_version="3.119.0",
            harfbuzzsharp_version="1.8.8",
            preview_label="rc.1",
        )
        fixture.fetch()
        with self.assertRaisesRegex(PlanError, "not a safe maintenance base"):
            prepare.build_prepare_plan(
                fixture.repo,
                integration_target="main",
                requested_version="3.119.0",
                tooling_sha=main_sha,
                github=FakeGitHubClient(),
            )

    def test_approved_base_must_be_same_numeric_preview_zero(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(
            skiasharp_version="3.119.0",
            harfbuzzsharp_version="1.8.8",
            preview_label="rc.1",
        )
        fixture.fetch()
        with self.assertRaisesRegex(PlanError, "approved base.*not a safe maintenance base"):
            prepare.build_prepare_plan(
                fixture.repo,
                integration_target="main",
                requested_version="3.119.0",
                tooling_sha=main_sha,
                github=FakeGitHubClient(),
                approved_base="refs/remotes/origin/main",
            )

    def test_stable_accepts_explicitly_approved_base(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.fetch()
        github = FakeGitHubClient()
        plan = prepare.build_prepare_plan(
            fixture.repo,
            integration_target="main",
            requested_version="3.119.0",
            tooling_sha=main_sha,
            github=github,
            approved_base="refs/remotes/origin/main",
        )
        self.assertEqual(plan["base"]["sha"], main_sha)
        self.assertEqual(plan["maintenanceBranch"]["action"], "create")

    def test_apply_stable_release_returns_await_merge_while_bump_pr_is_open(self):
        # Item 8 regression: apply must not hardcode nextAction="done" once
        # it has opened (or found) the stable-bump PR -- the release
        # process is not finished until a human merges it.
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.create_remote_branch("release/3.119.x", main_sha)
        fixture.fetch()
        github = FakeGitHubClient()
        plan = prepare.build_prepare_plan(
            fixture.repo,
            integration_target="main",
            requested_version="3.119.0",
            tooling_sha=main_sha,
            github=github,
        )
        self.assertEqual(plan["stableBump"]["status"], "pending")
        # Regression: an "awaiting-user" operation status (emitted below,
        # once the bump PR is open) must be schema-valid -- writing a plan
        # is the boundary that actually enforces the schema, unlike
        # constructing the in-memory dict directly.
        common.write_plan(self.root / "prepare-plan.json", plan, schema_name=prepare.PREPARE_SCHEMA)
        report = prepare.apply_prepare_plan(with_digest(plan), repo=fixture.repo, github=github)

        self.assertEqual(report["nextAction"], "await-merge")
        self.assertIsNotNone(report["stableBumpPullRequestUrl"])
        bump_op = next(op for op in report["operations"] if op["id"] == "open-stable-bump-pr")
        self.assertEqual(bump_op["status"], "done")  # the act of opening it is complete

        # Re-planning while the PR is still open must also report
        # await-merge, not done.
        fixture.fetch()
        replanned = prepare.build_prepare_plan(
            fixture.repo, integration_target="main", requested_version="3.119.0",
            tooling_sha=main_sha, github=github,
        )
        self.assertEqual(replanned["nextAction"], "await-merge")
        self.assertEqual(replanned["stableBump"]["status"], "awaiting-user")
        awaiting_op = next(op for op in replanned["operations"] if op["id"] == "open-stable-bump-pr")
        self.assertEqual(awaiting_op["status"], "awaiting-user")
        # This is the exact plan CLI callers persist and later re-read: it
        # must round-trip through schema validation without raising, since
        # the schema is the source of truth for the enum, not just the code
        # path that produces the value.
        common.write_plan(
            self.root / "prepare-plan-awaiting-user.json", replanned, schema_name=prepare.PREPARE_SCHEMA
        )

        # Re-applying while still open is idempotent and still await-merge,
        # not done.
        second_report = prepare.apply_prepare_plan(with_digest(replanned), repo=fixture.repo, github=github)
        self.assertEqual(second_report["nextAction"], "await-merge")

    def test_apply_reuses_pr_found_immediately_before_create_on_retry(self):
        # Opus 5 must-fix 1: the original approved plan's stableBump has a
        # null pullRequestUrl (recorded at plan time, before any PR
        # existed). Retrying apply with that exact same plan after a PR
        # has since appeared -- e.g. a previous apply attempt crashed after
        # opening it but before returning its URL, or something else
        # opened it out-of-band/racily -- must reuse the existing open PR
        # rather than creating a duplicate. _create_stable_bump_pr must
        # call find_open_pull_request immediately before create, not rely
        # on a value cached at plan time.
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.create_remote_branch("release/3.119.x", main_sha)
        fixture.fetch()
        github = FakeGitHubClient()
        plan = prepare.build_prepare_plan(
            fixture.repo, integration_target="main", requested_version="3.119.0",
            tooling_sha=main_sha, github=github,
        )
        self.assertIsNone(plan["stableBump"]["pullRequestUrl"])

        first_report = prepare.apply_prepare_plan(with_digest(plan), repo=fixture.repo, github=github)
        first_url = first_report["stableBumpPullRequestUrl"]
        self.assertIsNotNone(first_url)
        self.assertEqual(github._next_pr_number, 2)  # exactly one PR was created

        # Retry with the exact original plan (still recording a null
        # pullRequestUrl, as approved) -- must not create a second PR.
        second_report = prepare.apply_prepare_plan(with_digest(plan), repo=fixture.repo, github=github)
        self.assertEqual(second_report["stableBumpPullRequestUrl"], first_url)
        self.assertEqual(github._next_pr_number, 2)  # still exactly one PR

    def test_apply_reuses_existing_valid_bump_branch_without_repushing(self):
        # Opus 5 must-fix 2 (happy path): an already-pushed bump branch
        # that correctly descends from the integration branch and carries
        # the expected preview.0/SkiaSharp/HarfBuzzSharp version state
        # (e.g. left over from a prior apply attempt that pushed the
        # branch but crashed before opening the PR) must be reused, not
        # rejected or re-committed.
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.create_remote_branch("release/3.119.x", main_sha)
        fixture.fetch()
        github = FakeGitHubClient()
        plan = prepare.build_prepare_plan(
            fixture.repo, integration_target="main", requested_version="3.119.0",
            tooling_sha=main_sha, github=github,
        )
        bump_branch = plan["stableBump"]["bumpBranch"]
        expected_skia = plan["stableBump"]["skiaSharpVersion"]
        expected_harfbuzz = plan["stableBump"]["harfBuzzSharpVersion"]

        # Simulate a prior partial apply: the bump branch was pushed with
        # the correct version state, but the process crashed before ever
        # opening a PR.
        fixture.repo.git("switch", "-c", bump_branch, "refs/remotes/origin/main")
        helpers.write_variables(fixture.worktree, skiasharp_version=expected_skia, preview_label="preview.0")
        helpers.write_versions(fixture.worktree, skiasharp_version=expected_skia, harfbuzzsharp_version=expected_harfbuzz)
        helpers.stage(fixture.worktree, "scripts")
        helpers.commit_staged(fixture.worktree, "Bump to the next version")
        fixture.repo.push_branch(bump_branch)
        pushed_sha = fixture.repo.remote_sha(bump_branch)

        report = prepare.apply_prepare_plan(with_digest(plan), repo=fixture.repo, github=github)

        self.assertIsNotNone(report["stableBumpPullRequestUrl"])
        # The branch was reused, not re-created/re-committed.
        self.assertEqual(fixture.repo.remote_sha(bump_branch), pushed_sha)

    def test_apply_rejects_existing_bump_branch_not_descended_from_integration(self):
        # Opus 5 must-fix 2: an existing bump branch that is not a
        # descendant of the live integration branch is stale/conflicting
        # and must block rather than being silently reused.
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.create_remote_branch("release/3.119.x", main_sha)
        fixture.fetch()
        github = FakeGitHubClient()
        plan = prepare.build_prepare_plan(
            fixture.repo, integration_target="main", requested_version="3.119.0",
            tooling_sha=main_sha, github=github,
        )
        bump_branch = plan["stableBump"]["bumpBranch"]

        # Push the bump branch from unrelated history (an orphan commit),
        # not descended from the integration branch at all.
        fixture.repo.git("switch", "--orphan", "unrelated-history")
        (fixture.worktree / "unrelated.txt").write_text("stale", encoding="utf-8")
        fixture.repo.git("add", "unrelated.txt")
        fixture.repo.git("commit", "-m", "unrelated commit")
        fixture.repo.git("branch", bump_branch, "HEAD")
        fixture.repo.push_branch(bump_branch)

        with self.assertRaisesRegex(GitHubError, "not a descendant"):
            prepare.apply_prepare_plan(with_digest(plan), repo=fixture.repo, github=github)

    def test_apply_rejects_existing_bump_branch_with_wrong_version_state(self):
        # Opus 5 must-fix 2: an existing bump branch that descends from
        # the integration branch but carries the wrong version state
        # (stale SkiaSharp version, in this case) must also block.
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.create_remote_branch("release/3.119.x", main_sha)
        fixture.fetch()
        github = FakeGitHubClient()
        plan = prepare.build_prepare_plan(
            fixture.repo, integration_target="main", requested_version="3.119.0",
            tooling_sha=main_sha, github=github,
        )
        bump_branch = plan["stableBump"]["bumpBranch"]

        fixture.repo.git("switch", "-c", bump_branch, "refs/remotes/origin/main")
        # Wrong: some other version entirely, not the next preview.0 bump
        # this plan expects.
        helpers.write_variables(fixture.worktree, skiasharp_version="9.9.9", preview_label="preview.0")
        helpers.write_versions(fixture.worktree, skiasharp_version="9.9.9", harfbuzzsharp_version="1.8.8")
        helpers.stage(fixture.worktree, "scripts")
        helpers.commit_staged(fixture.worktree, "wrong bump content")
        fixture.repo.push_branch(bump_branch)

        with self.assertRaisesRegex(GitHubError, "SkiaSharp version"):
            prepare.apply_prepare_plan(with_digest(plan), repo=fixture.repo, github=github)


class PrepareHotfixTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self):
        self._tmp.cleanup()

    def test_hotfix_preview_bases_on_stable_tag(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.repo.push_tag("v3.119.0", main_sha)
        fixture.fetch()
        github = FakeGitHubClient()
        plan = prepare.build_prepare_plan(
            fixture.repo,
            integration_target="main",
            requested_version="3.119.0.1-preview.1",
            tooling_sha=main_sha,
            github=github,
        )
        self.assertEqual(plan["base"]["ref"], "refs/tags/v3.119.0")
        self.assertEqual(plan["base"]["sha"], main_sha)
        self.assertTrue(plan["release"]["isHotfix"])

    def test_hotfix_stable_bases_on_latest_prerelease_branch(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.repo.push_tag("v3.119.0", main_sha)
        fixture.create_remote_branch("release/3.119.0.1-preview.1", main_sha)
        fixture.create_remote_branch("release/3.119.0.1-rc.1", main_sha)
        fixture.fetch()
        github = FakeGitHubClient()
        plan = prepare.build_prepare_plan(
            fixture.repo,
            integration_target="main",
            requested_version="3.119.0.1",
            tooling_sha=main_sha,
            github=github,
        )
        self.assertEqual(plan["base"]["ref"], "refs/remotes/origin/release/3.119.0.1-rc.1")


class PrepareStableBumpTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self):
        self._tmp.cleanup()

    def test_bump_status_done_when_already_advanced(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.create_remote_branch("release/3.119.x", main_sha)
        # Simulate the bump PR having already merged: the integration branch
        # now reads the *next* version at preview.0.
        helpers.write_variables(fixture.worktree, skiasharp_version="3.119.1", preview_label="preview.0")
        helpers.write_versions(fixture.worktree, skiasharp_version="3.119.1", harfbuzzsharp_version="1.8.8.1")
        helpers.stage(fixture.worktree, "scripts")
        bumped_sha = helpers.commit_staged(fixture.worktree, "bump")
        fixture.repo.git("push", "origin", f"HEAD:release/3.119.x")
        fixture.fetch()
        github = FakeGitHubClient()
        bump_plan = prepare.plan_stable_bump(fixture.repo, __import__("release_model").parse_release_version("3.119.0"), "1.8.8", github=github)
        self.assertEqual(bump_plan.status, "done")

    def test_bump_status_awaiting_user_when_pr_already_open(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.create_remote_branch("release/3.119.x", main_sha)
        fixture.fetch()
        github = FakeGitHubClient()
        github.pull_requests[("bump-version-3.119.1", "release/3.119.x")] = PullRequestRef(
            number=7, url="https://example.invalid/pr/7"
        )
        import release_model as model

        bump_plan = prepare.plan_stable_bump(
            fixture.repo, model.parse_release_version("3.119.0"), "1.8.8", github=github
        )
        self.assertEqual(bump_plan.status, "awaiting-user")
        self.assertEqual(bump_plan.pull_request_url, "https://example.invalid/pr/7")

    def test_bump_rejects_unexpected_integration_state(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.create_remote_branch("release/3.119.x", main_sha)
        fixture.fetch()
        github = FakeGitHubClient()
        import release_model as model

        with self.assertRaises(PlanError):
            prepare.plan_stable_bump(
                fixture.repo, model.parse_release_version("3.119.5"), "1.8.8", github=github
            )

    def test_full_plan_next_action_is_await_merge_when_only_bump_pr_pending(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.create_remote_branch("release/3.119.x", main_sha)
        fixture.create_remote_branch("release/3.119.0", main_sha)
        fixture.fetch()
        github = FakeGitHubClient()
        github.create_ref(repository="mono/skia", ref="refs/heads/release/3.119.0", sha=SKIA_SHA)
        github.pull_requests[("bump-version-3.119.1", "release/3.119.x")] = PullRequestRef(
            number=9, url="https://example.invalid/pr/9"
        )
        plan = prepare.build_prepare_plan(
            fixture.repo,
            integration_target="main",
            requested_version="3.119.0",
            tooling_sha=main_sha,
            github=github,
        )
        statuses = {op["id"]: op["status"] for op in plan["operations"]}
        self.assertEqual(statuses["create-maintenance-branch"], "done")
        self.assertEqual(statuses["create-skia-ref"], "done")
        self.assertEqual(statuses["create-release-branch"], "done")
        self.assertEqual(statuses["open-stable-bump-pr"], "awaiting-user")
        self.assertEqual(plan["nextAction"], "await-merge")


class VersionFileUpdateTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self):
        self._tmp.cleanup()

    def test_update_version_files_bumps_label_only(self):
        helpers.write_variables(self.root, skiasharp_version="3.119.0", preview_label="preview.0")
        helpers.write_versions(self.root, skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        changed = prepare.update_version_files(self.root, preview_label="preview.1")
        self.assertEqual(changed, [prepare.VARIABLES_PATH])
        state = prepare.read_worktree_version_state(self.root)
        self.assertEqual(state.label, "preview.1")
        self.assertEqual(state.skia, "3.119.0")

    def test_update_version_files_bumps_versions_and_label(self):
        helpers.write_variables(self.root, skiasharp_version="3.119.0", preview_label="preview.0")
        helpers.write_versions(self.root, skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        changed = prepare.update_version_files(
            self.root, preview_label="preview.1", skia_version="3.119.1", harfbuzz_version="1.8.8.1"
        )
        self.assertEqual(sorted(changed), sorted([prepare.VARIABLES_PATH, prepare.VERSIONS_PATH]))
        state = prepare.read_worktree_version_state(self.root)
        self.assertEqual(state.label, "preview.1")
        self.assertEqual(state.skia, "3.119.1")
        self.assertEqual(state.harfbuzz, "1.8.8.1")

    def test_update_version_files_rejects_no_op(self):
        helpers.write_variables(self.root, skiasharp_version="3.119.0", preview_label="preview.0")
        helpers.write_versions(self.root, skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        with self.assertRaises(PlanError):
            prepare.update_version_files(self.root, preview_label="preview.0")


if __name__ == "__main__":
    unittest.main()
