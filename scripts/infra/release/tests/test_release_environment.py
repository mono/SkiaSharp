from __future__ import annotations

import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import release_environment as environment
from release_common import CommandResult, ReleaseToolError


def _snapshot(
    *,
    name: str = "release-branching",
    reviewer_count: int = 1,
    prevent_self_review: bool = True,
    protected_branches: bool = False,
    custom_branch_policies: bool = True,
    branch_policies: tuple[environment.BranchPolicy, ...] = (),
    protection_rule_types: tuple[str, ...] = ("required_reviewers", "branch_policy"),
    no_required_reviewers: bool = False,
) -> environment.EnvironmentSnapshot:
    return environment.EnvironmentSnapshot(
        name=name,
        protection_rule_types=protection_rule_types,
        required_reviewers=(
            None
            if no_required_reviewers
            else environment.RequiredReviewersRule(
                reviewer_count=reviewer_count, prevent_self_review=prevent_self_review
            )
        ),
        protected_branches=protected_branches,
        custom_branch_policies=custom_branch_policies,
        branch_policies=branch_policies,
    )


class CheckEnvironmentTests(unittest.TestCase):
    def test_missing_environment_fails_with_exists_false(self):
        result = environment.check_environment(None, name="release-tag", default_branch="main")
        self.assertFalse(result.exists)
        self.assertFalse(result.ok)
        self.assertEqual(len(result.reasons), 1)
        self.assertIn("does not exist", result.reasons[0])

    def test_well_formed_environment_passes(self):
        snapshot = _snapshot(
            branch_policies=(environment.BranchPolicy(name="main", kind="branch"),),
        )
        result = environment.check_environment(snapshot, name="release-tag", default_branch="main")
        self.assertTrue(result.exists)
        self.assertTrue(result.ok, result.reasons)
        self.assertEqual(result.reasons, ())
        self.assertEqual(result.allowed_branches, ("main",))
        self.assertEqual(result.reviewer_count, 1)
        self.assertTrue(result.prevent_self_review)
        self.assertTrue(result.custom_branch_policies)

    def test_missing_required_reviewers_rule_fails(self):
        snapshot = _snapshot(
            no_required_reviewers=True,
            branch_policies=(environment.BranchPolicy(name="main", kind="branch"),),
        )
        result = environment.check_environment(snapshot, name="x", default_branch="main")
        self.assertFalse(result.ok)
        self.assertTrue(any("required_reviewers" in reason for reason in result.reasons))

    def test_required_reviewers_rule_with_zero_reviewers_fails(self):
        snapshot = _snapshot(
            reviewer_count=0,
            branch_policies=(environment.BranchPolicy(name="main", kind="branch"),),
        )
        result = environment.check_environment(snapshot, name="x", default_branch="main")
        self.assertFalse(result.ok)
        self.assertTrue(any("no reviewers configured" in reason for reason in result.reasons))

    def test_prevent_self_review_disabled_fails(self):
        snapshot = _snapshot(
            prevent_self_review=False,
            branch_policies=(environment.BranchPolicy(name="main", kind="branch"),),
        )
        result = environment.check_environment(snapshot, name="x", default_branch="main")
        self.assertFalse(result.ok)
        self.assertTrue(any("prevent_self_review" in reason for reason in result.reasons))

    def test_protected_branches_mode_instead_of_custom_fails(self):
        snapshot = _snapshot(
            protected_branches=True,
            custom_branch_policies=False,
            branch_policies=(),
        )
        result = environment.check_environment(snapshot, name="x", default_branch="main")
        self.assertFalse(result.ok)
        self.assertTrue(any("custom deployment branch policies" in reason for reason in result.reasons))

    def test_unrestricted_deployment_branch_policy_fails(self):
        # deployment_branch_policy is entirely null: any branch may deploy.
        snapshot = _snapshot(protected_branches=False, custom_branch_policies=False, branch_policies=())
        result = environment.check_environment(snapshot, name="x", default_branch="main")
        self.assertFalse(result.ok)
        self.assertTrue(any("custom deployment branch policies" in reason for reason in result.reasons))

    def test_extra_allowed_branch_fails(self):
        snapshot = _snapshot(
            branch_policies=(
                environment.BranchPolicy(name="main", kind="branch"),
                environment.BranchPolicy(name="release/*", kind="branch"),
            ),
        )
        result = environment.check_environment(snapshot, name="x", default_branch="main")
        self.assertFalse(result.ok)
        self.assertTrue(any("allowed deployment branches" in reason for reason in result.reasons))

    def test_wrong_single_branch_fails(self):
        snapshot = _snapshot(branch_policies=(environment.BranchPolicy(name="master", kind="branch"),))
        result = environment.check_environment(snapshot, name="x", default_branch="main")
        self.assertFalse(result.ok)
        self.assertIn("master", result.reasons[-1])

    def test_no_branch_policies_configured_fails(self):
        snapshot = _snapshot(branch_policies=())
        result = environment.check_environment(snapshot, name="x", default_branch="main")
        self.assertFalse(result.ok)
        self.assertTrue(any("allowed deployment branches are []" in reason for reason in result.reasons))

    def test_tag_policy_present_fails_even_with_correct_branch(self):
        snapshot = _snapshot(
            branch_policies=(
                environment.BranchPolicy(name="main", kind="branch"),
                environment.BranchPolicy(name="v*", kind="tag"),
            ),
        )
        result = environment.check_environment(snapshot, name="x", default_branch="main")
        self.assertFalse(result.ok)
        self.assertTrue(any("tag deployment policies" in reason for reason in result.reasons))

    def test_multiple_failures_are_all_reported(self):
        snapshot = _snapshot(
            no_required_reviewers=True,
            custom_branch_policies=False,
            branch_policies=(),
        )
        result = environment.check_environment(snapshot, name="x", default_branch="main")
        self.assertFalse(result.ok)
        self.assertGreaterEqual(len(result.reasons), 3)

    def test_empty_default_branch_raises(self):
        with self.assertRaises(environment.EnvironmentError):
            environment.check_environment(_snapshot(), name="x", default_branch="")


class CheckResultToDictTests(unittest.TestCase):
    def test_shape_uses_camel_case_keys(self):
        snapshot = _snapshot(branch_policies=(environment.BranchPolicy(name="main", kind="branch"),))
        result = environment.check_environment(snapshot, name="release-tag", default_branch="main")
        payload = environment.check_result_to_dict(result)
        self.assertEqual(
            set(payload),
            {
                "name", "exists", "ok", "reasons", "defaultBranch", "protectionRuleTypes",
                "allowedBranches", "reviewerCount", "preventSelfReview", "customBranchPolicies",
            },
        )
        self.assertEqual(payload["name"], "release-tag")
        self.assertTrue(payload["ok"])
        self.assertEqual(payload["allowedBranches"], ["main"])
        self.assertEqual(payload["reasons"], [])


class ParseEnvironmentPayloadTests(unittest.TestCase):
    """Exercises release_environment._parse_environment against the exact
    JSON shape GitHub's REST API returns (verified against the real
    "environment" OpenAPI schema: protection_rules is a list of
    {type: "wait_timer"|"required_reviewers"|"branch_policy", ...})."""

    def test_parses_required_reviewers_rule_and_branch_policy_settings(self):
        payload = {
            "id": 56780428,
            "name": "release-tag",
            "protection_rules": [
                {"id": 1, "node_id": "a", "type": "wait_timer", "wait_timer": 30},
                {
                    "id": 2,
                    "node_id": "b",
                    "type": "required_reviewers",
                    "prevent_self_review": True,
                    "reviewers": [
                        {"type": "User", "reviewer": {"login": "octocat"}},
                        {"type": "Team", "reviewer": {"name": "release-approvers"}},
                    ],
                },
                {"id": 3, "node_id": "c", "type": "branch_policy"},
            ],
            "deployment_branch_policy": {"protected_branches": False, "custom_branch_policies": True},
        }
        snapshot = environment._parse_environment(
            "release-tag", payload, (environment.BranchPolicy(name="main", kind="branch"),)
        )
        self.assertEqual(snapshot.protection_rule_types, ("wait_timer", "required_reviewers", "branch_policy"))
        self.assertIsNotNone(snapshot.required_reviewers)
        self.assertEqual(snapshot.required_reviewers.reviewer_count, 2)
        self.assertTrue(snapshot.required_reviewers.prevent_self_review)
        self.assertFalse(snapshot.protected_branches)
        self.assertTrue(snapshot.custom_branch_policies)
        self.assertEqual(snapshot.branch_policies, (environment.BranchPolicy(name="main", kind="branch"),))

    def test_null_deployment_branch_policy_means_unrestricted(self):
        payload = {"id": 1, "name": "x", "protection_rules": [], "deployment_branch_policy": None}
        snapshot = environment._parse_environment("x", payload, ())
        self.assertFalse(snapshot.protected_branches)
        self.assertFalse(snapshot.custom_branch_policies)
        self.assertIsNone(snapshot.required_reviewers)

    def test_required_reviewers_defaults_prevent_self_review_to_false_when_absent(self):
        payload = {
            "id": 1,
            "name": "x",
            "protection_rules": [{"id": 1, "node_id": "a", "type": "required_reviewers", "reviewers": []}],
            "deployment_branch_policy": None,
        }
        snapshot = environment._parse_environment("x", payload, ())
        self.assertFalse(snapshot.required_reviewers.prevent_self_review)
        self.assertEqual(snapshot.required_reviewers.reviewer_count, 0)


class ScriptedRunner:
    """Returns one canned :class:`CommandResult` per call, in call order.
    Mirrors ``FakeCommandRunnerForVerify`` in tests/test_release_nuget.py:
    a real :class:`release_common.SubprocessCommandRunner` raises on a
    failed result only when ``check`` is left at its default ``True``, so
    this fake replicates that so ``GhCliEnvironmentClient``'s own explicit
    ``check=False`` on the 404-tolerant first call is exercised faithfully.
    """

    def __init__(self, results: list[CommandResult]):
        self._results = list(results)
        self.calls: list[list[str]] = []

    def run(self, args, *, cwd, check=True, timeout=120, input=None):
        self.calls.append(list(args))
        result = self._results.pop(0)
        if check and not result.ok:
            raise ReleaseToolError(f"scripted failure for {args}")
        return result


def _assert_explicit_get(test: unittest.TestCase, argv: list[str]) -> None:
    test.assertIn("-X", argv)
    test.assertEqual(argv[argv.index("-X") + 1], "GET")


class GhCliEnvironmentClientTests(unittest.TestCase):
    def test_full_round_trip_builds_snapshot(self):
        env_payload = (
            '{"id": 1, "name": "release-tag", "protection_rules": ['
            '{"id": 1, "node_id": "a", "type": "required_reviewers", '
            '"prevent_self_review": true, "reviewers": [{"type": "User", "reviewer": {"login": "octocat"}}]}, '
            '{"id": 2, "node_id": "b", "type": "branch_policy"}], '
            '"deployment_branch_policy": {"protected_branches": false, "custom_branch_policies": true}}'
        )
        # Two pages: --slurp wraps each page's own {"branch_policies": [...]}
        # object in a list; the merge logic must concatenate across pages.
        branch_policies_pages = (
            '[{"total_count": 2, "branch_policies": [{"id": 1, "node_id": "x", "name": "main", "type": "branch"}]}, '
            '{"total_count": 2, "branch_policies": [{"id": 2, "node_id": "y", "name": "main-2", "type": "branch"}]}]'
        )
        runner = ScriptedRunner(
            [
                CommandResult(args=(), returncode=0, stdout=env_payload, stderr=""),
                CommandResult(args=(), returncode=0, stdout=branch_policies_pages, stderr=""),
            ]
        )
        client = environment.GhCliEnvironmentClient(repository="mono/SkiaSharp", runner=runner)
        snapshot = client.get_environment("release-tag")

        self.assertIsNotNone(snapshot)
        self.assertEqual(snapshot.required_reviewers.reviewer_count, 1)
        self.assertTrue(snapshot.required_reviewers.prevent_self_review)
        self.assertTrue(snapshot.custom_branch_policies)
        self.assertEqual(
            snapshot.branch_policies,
            (
                environment.BranchPolicy(name="main", kind="branch"),
                environment.BranchPolicy(name="main-2", kind="branch"),
            ),
        )

        self.assertEqual(len(runner.calls), 2)
        env_call, branch_call = runner.calls
        self.assertEqual(env_call[:2], ["gh", "api"])
        self.assertIn("repos/mono/SkiaSharp/environments/release-tag", env_call)
        _assert_explicit_get(self, env_call)
        self.assertIn("repos/mono/SkiaSharp/environments/release-tag/deployment-branch-policies", branch_call)
        _assert_explicit_get(self, branch_call)
        self.assertIn("--paginate", branch_call)
        self.assertIn("--slurp", branch_call)
        self.assertIn("per_page=100", branch_call)

    def test_url_encodes_slashes_in_environment_name(self):
        runner = ScriptedRunner(
            [
                CommandResult(
                    args=(), returncode=0,
                    stdout='{"id": 1, "name": "x", "protection_rules": [], "deployment_branch_policy": null}',
                    stderr="",
                ),
                CommandResult(args=(), returncode=0, stdout="[]", stderr=""),
            ]
        )
        client = environment.GhCliEnvironmentClient(repository="mono/SkiaSharp", runner=runner)
        client.get_environment("team/sub-environment")
        env_call = runner.calls[0]
        self.assertIn("repos/mono/SkiaSharp/environments/team%2Fsub-environment", env_call)

    def test_returns_none_for_404(self):
        runner = ScriptedRunner(
            [CommandResult(args=(), returncode=1, stdout="", stderr="gh: Not Found (HTTP 404)")]
        )
        client = environment.GhCliEnvironmentClient(repository="mono/SkiaSharp", runner=runner)
        self.assertIsNone(client.get_environment("release-publish"))
        # A 404 on the environment itself must not attempt the second call.
        self.assertEqual(len(runner.calls), 1)

    def test_raises_on_a_genuine_query_failure(self):
        runner = ScriptedRunner(
            [CommandResult(args=(), returncode=1, stdout="", stderr="gh: Bad credentials (HTTP 401)")]
        )
        client = environment.GhCliEnvironmentClient(repository="mono/SkiaSharp", runner=runner)
        with self.assertRaises(environment.EnvironmentError):
            client.get_environment("release-publish")


if __name__ == "__main__":
    unittest.main()
