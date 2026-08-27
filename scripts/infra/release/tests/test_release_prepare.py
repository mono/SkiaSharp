from __future__ import annotations

import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))
sys.path.insert(0, str(Path(__file__).resolve().parent))

import gitrepo_helpers as helpers
from release_common import PlanError
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
        self.assertEqual(plan["release"]["integrationBranch"], "release/3.119.x")
        self.assertEqual(plan["base"]["sha"], main_sha)
        self.assertEqual(plan["maintenanceBranch"]["action"], "create")
        self.assertEqual(plan["maintenanceBranch"]["baseSha"], main_sha)
        self.assertFalse(plan["maintenanceBranch"]["exists"])
        self.assertEqual(plan["skia"]["sha"], SKIA_SHA)
        self.assertTrue(any("maintenance branch" in w for w in plan["warnings"]))
        self.assertIsNone(plan["stableBump"])
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
        report = prepare.apply_prepare_plan(
            plan, repo=fixture.repo, skia_repo=fixture.repo, github=github
        )
        self.assertEqual(report["releaseBranch"], "release/3.119.0-preview.1")
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

        # Re-applying is idempotent: no exceptions, no forced updates.
        second_report = prepare.apply_prepare_plan(
            replanned, repo=fixture.repo, skia_repo=fixture.repo, github=github
        )
        self.assertEqual(second_report["releaseBranch"], "release/3.119.0-preview.1")


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
        # broken run), but the RC branch for this exact line still exists.
        fixture.create_remote_branch("release/3.119.0-rc.1", main_sha)
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
        self.assertEqual(plan["maintenanceBranch"]["action"], "create")
        self.assertEqual(plan["maintenanceBranch"]["baseSha"], main_sha)

    def test_stable_requires_approved_base_when_no_recovery_candidate(self):
        fixture = PrepareFixture(self.root)
        main_sha = fixture.seed_main(skiasharp_version="3.119.0", harfbuzzsharp_version="1.8.8")
        fixture.fetch()
        github = FakeGitHubClient()
        with self.assertRaisesRegex(PlanError, "explicitly approved base"):
            prepare.build_prepare_plan(
                fixture.repo,
                integration_target="main",
                requested_version="3.119.0",
                tooling_sha=main_sha,
                github=github,
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
