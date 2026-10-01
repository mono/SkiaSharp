#!/usr/bin/env python3
"""Structural safety tests for the direct Skia sync PR repin workflow."""

from __future__ import annotations

from pathlib import Path
import unittest

import yaml


REPO_ROOT = Path(__file__).resolve().parents[3]
WORKFLOW_PATH = (
    REPO_ROOT / ".github" / "workflows" / "auto-skia-submodule-sync.yml"
)


class AutoSkiaSubmoduleSyncWorkflowTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.text = WORKFLOW_PATH.read_text(encoding="utf-8")
        cls.workflow = yaml.load(cls.text, Loader=yaml.BaseLoader)

    def test_preserves_manual_and_reusable_dry_run_surfaces(self):
        triggers = self.workflow["on"]
        self.assertIn("workflow_dispatch", triggers)
        self.assertIn("workflow_call", triggers)
        self.assertNotIn("schedule", triggers)
        for trigger in ("workflow_dispatch", "workflow_call"):
            inputs = triggers[trigger]["inputs"]
            for name in (
                "target_branch",
                "skia_branch",
                "reviewed_skia_sha",
                "expected_target_sha",
                "expected_skia_sha",
            ):
                with self.subTest(trigger=trigger, input=name):
                    self.assertEqual("true", inputs[name]["required"])
            self.assertEqual("boolean", inputs["push"]["type"])
            self.assertEqual("false", inputs["push"]["default"])

    def test_reusable_outputs_expose_final_target_and_native_shas(self):
        outputs = self.workflow["on"]["workflow_call"]["outputs"]
        self.assertIn("target_sha", outputs)
        self.assertIn("skia_sha", outputs)
        self.assertIn("pushed", outputs)
        self.assertIn("pr_number", outputs)
        self.assertIn("${{ jobs.repin.outputs.target_sha }}", self.text)
        self.assertIn("${{ jobs.repin.outputs.skia_sha }}", self.text)

    def test_resolves_exact_immutable_refs_before_work(self):
        self.assertIn(
            'git -C target fetch --no-tags origin \\\n'
            '            "+refs/heads/$TARGET_BRANCH:refs/remotes/origin/$TARGET_BRANCH"',
            self.text,
        )
        self.assertIn(
            'git ls-remote --exit-code --heads "$EXPECTED_NATIVE_URL" \\\n'
            '            "refs/heads/$SKIA_BRANCH"',
            self.text,
        )
        self.assertIn(
            'TARGET_SHA=$(git -C target rev-parse "refs/remotes/origin/$TARGET_BRANCH^{commit}")',
            self.text,
        )
        self.assertIn('[ "$TARGET_SHA" = "$EXPECTED_TARGET_SHA" ]', self.text)
        self.assertIn('[ "$SKIA_SHA" = "$EXPECTED_SKIA_SHA" ]', self.text)
        self.assertIn('EXPECTED_NATIVE_URL: https://github.com/mono/skia.git', self.text)
        self.assertIn('must use $EXPECTED_NATIVE_URL', self.text)

    def test_push_is_restricted_to_one_open_same_repository_sync_pr(self):
        self.assertIn('if: inputs.push', self.text)
        self.assertIn('[[ "$TARGET_BRANCH" == skia-sync/*', self.text)
        self.assertIn('.head.repo.full_name == env.GITHUB_REPOSITORY', self.text)
        self.assertIn('.head.ref == env.TARGET_BRANCH', self.text)
        self.assertIn('.head.sha == env.EXPECTED_TARGET_SHA', self.text)
        self.assertIn('.base.repo.full_name == env.GITHUB_REPOSITORY', self.text)
        self.assertIn('.base.ref == "main" or (.base.ref | test("^release/', self.text)
        self.assertIn('startswith("[skia-sync]")', self.text)
        self.assertIn('jq \'length\' <<<"$matches"', self.text)
        self.assertEqual(2, self.text.count("raw_matches=$(gh api"))
        self.assertEqual(
            2,
            self.text.count(
                '-f head="${GITHUB_REPOSITORY_OWNER}:${TARGET_BRANCH}"'
            ),
        )
        self.assertIn('echo "pr_number=$(jq -r \'.[0].number\' <<<"$matches")"', self.text)
        self.assertIn('steps.pr.outputs.pr_number', self.text)

    def test_reuses_repin_script_and_never_creates_dependent_prs(self):
        self.assertEqual(
            2,
            self.text.count(
                ".agents/skills/merge-skia-update/scripts/"
                "Update-SkiaSharpSkiaCommit.ps1"
            ),
        )
        for argument in (
            '-ExpectedTargetSha "$EXPECTED_TARGET_SHA"',
            '-ExpectedSkiaSha "$EXPECTED_SKIA_SHA"',
            '-ReviewedSkiaSha "$REVIEWED_SKIA_SHA"',
        ):
            with self.subTest(argument=argument):
                self.assertEqual(2, self.text.count(argument))
        for forbidden in (
            "gh pr create",
            "gh pr edit",
            "automation/",
            "checkout -B",
            "git -C target push --force ",
        ):
            with self.subTest(forbidden=forbidden):
                self.assertNotIn(forbidden, self.text)

    def test_uses_trusted_tooling_and_scopes_write_credential_to_push(self):
        self.assertIn("repository: ${{ job.workflow_repository }}", self.text)
        self.assertIn("ref: ${{ job.workflow_sha }}", self.text)
        self.assertIn("path: trusted-tooling", self.text)
        self.assertIn("path: target", self.text)
        self.assertEqual(2, self.text.count("persist-credentials: false"))
        self.assertEqual(2, self.text.count("token: ${{ github.token }}"))
        self.assertIn("contents: read", self.text)
        self.assertNotIn("contents: write", self.text)
        self.assertIn("pull-requests: read", self.text)
        self.assertIn("SKIASHARP_AUTOBUMP_TOKEN", self.workflow["on"]["workflow_call"]["secrets"])
        self.assertIn('push=true requires the SKIASHARP_AUTOBUMP_TOKEN secret.', self.text)
        self.assertEqual(1, self.text.count("SKIASHARP_AUTOBUMP_TOKEN: ${{ secrets.SKIASHARP_AUTOBUMP_TOKEN }}"))
        self.assertEqual(
                2,
                self.text.count(
                    "../trusted-tooling/.agents/skills/merge-skia-update/scripts/"
                    "Update-SkiaSharpSkiaCommit.ps1"
                ),
        )
        self.assertEqual(1, self.text.count("-Apply"))
        self.assertEqual(0, self.text.count("-Push"))
        self.assertIn(
            '"--force-with-lease=refs/heads/$TARGET_BRANCH:$EXPECTED_TARGET_SHA"',
            self.text,
        )
        self.assertIn('origin "HEAD:refs/heads/$TARGET_BRANCH"', self.text)

    def test_push_requires_trusted_main_workflow_and_uncontrolled_branch(self):
        self.assertIn("WORKFLOW_REF: ${{ job.workflow_ref }}", self.text)
        self.assertIn(
            'mono/SkiaSharp/.github/workflows/auto-skia-submodule-sync.yml@refs/heads/main',
            self.text,
        )
        self.assertEqual(
            2,
            self.text.count("repos/$GITHUB_REPOSITORY/branches/$encoded_branch"),
        )
        self.assertEqual(
            2,
            self.text.count("repos/$GITHUB_REPOSITORY/rules/branches/$encoded_branch"),
        )
        self.assertIn('Target branch is protected.', self.text)
        self.assertIn('Target branch is controlled by a ruleset.', self.text)

    def test_checkouts_are_pinned_to_the_reviewed_revision(self):
        checkout = "actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1"
        self.assertEqual(2, self.text.count(checkout))
        self.assertNotIn("actions/checkout@v4", self.text)

    def test_final_remote_verification_distinguishes_dry_run_from_push(self):
        self.assertIn(
            'FINAL_TARGET_SHA=$(git -C target rev-parse "refs/remotes/origin/$TARGET_BRANCH^{commit}")',
            self.text,
        )
        self.assertIn('[ "$FINAL_SKIA_SHA" = "$EXPECTED_SKIA_SHA" ]', self.text)
        self.assertIn('[ "$PUSH" = true ]', self.text)
        self.assertIn('[ "$FINAL_TARGET_SHA" = "$EXPECTED_TARGET_SHA" ]', self.text)


if __name__ == "__main__":
    unittest.main()
