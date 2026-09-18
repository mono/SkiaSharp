#!/usr/bin/env python3
"""Structural safety contracts for the Skia sync command workflows."""

from __future__ import annotations

import json
from pathlib import Path
import unittest

import yaml
from jsonschema import Draft202012Validator


ROOT = Path(__file__).resolve().parents[3]
WORKFLOWS = ROOT / ".github" / "workflows"


def load_yaml(path: Path):
    return yaml.safe_load(path.read_text(encoding="utf-8"))


class SkiaSyncWorkflowTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.review_agent = (WORKFLOWS / "skia-sync-review.md").read_text(encoding="utf-8")
        cls.review_entry = (WORKFLOWS / "skia-sync-review.yml").read_text(encoding="utf-8")
        cls.review_lock = (WORKFLOWS / "skia-sync-review.lock.yml").read_text(encoding="utf-8")
        cls.merge_message = (WORKFLOWS / "merge-message.md").read_text(encoding="utf-8")
        cls.merge_message_lock = (WORKFLOWS / "merge-message.lock.yml").read_text(encoding="utf-8")
        cls.merge_message_command = (WORKFLOWS / "merge-message-command.yml").read_text(encoding="utf-8")
        cls.merge = (WORKFLOWS / "skia-sync-merge.yml").read_text(encoding="utf-8")
        cls.merge_yaml = load_yaml(WORKFLOWS / "skia-sync-merge.yml")
        cls.persist = (WORKFLOWS / "persist-aw-data.yml").read_text(encoding="utf-8")
        cls.schema = json.loads((ROOT / ".agents/skills/review-skia-update/references/skia-review-schema.json").read_text(encoding="utf-8"))

    def test_review_isolated_preparation_and_deterministic_publication(self):
        self.assertIn("name: Review - Skia Sync", self.review_entry)
        self.assertIn("github.event.comment.body == '/skia-sync-review'", self.review_entry)
        for association in ("OWNER", "MEMBER", "COLLABORATOR"):
            self.assertIn(f"github.event.comment.author_association == '{association}'", self.review_entry)
        self.assertIn("default: true", self.review_entry)
        self.assertEqual(1, self.review_entry.count("scripts/run_review.py"))
        self.assertIn("skia-sync-review-raw-${{ github.run_id }}", self.review_entry)
        self.assertIn("uses: ./.github/workflows/skia-sync-review.lock.yml", self.review_entry)
        self.assertIn("resolve:", self.review_entry)
        self.assertIn("mechanical-review:", self.review_entry)
        mechanical = self.review_entry.split("mechanical-review:", 1)[1].split("\n  review:", 1)[0]
        self.assertNotIn("GH_TOKEN:", mechanical)
        self.assertNotIn("actions/checkout", mechanical)
        self.assertNotIn("token:", mechanical)
        self.assertIn("git init .", mechanical)
        self.assertIn("git fetch --depth=1 origin \"$GITHUB_SHA\"", mechanical)
        self.assertIn("docker run --rm", mechanical)
        self.assertIn("$GITHUB_WORKSPACE:/trusted:ro", mechanical)
        self.assertIn("$PAIR_JSON:/control/pair.json:ro", mechanical)
        self.assertNotIn("$RAW_DIR:", mechanical)
        self.assertIn("--prepared-generated-root \"$export_root/generated\"", mechanical)
        self.assertIn("-u GITHUB_ENV -u GITHUB_OUTPUT -u GITHUB_PATH -u GITHUB_STEP_SUMMARY", mechanical)
        self.assertIn("--pair-json \"$PAIR_JSON\"", mechanical)
        self.assertNotIn("gh-aw-agents", self.review_entry)
        self.assertNotIn("SKIASHARP_AUTOBUMP_TOKEN", self.review_entry)
        self.assertIn("model: gpt-5.6-terra", self.review_agent)
        self.assertIn("publish-skia-review:", self.review_agent)
        self.assertIn("Call `publish_skia_review` exactly once", self.review_agent)
        self.assertNotIn("add-comment:", self.review_agent)
        self.assertNotIn('name: agent\n            path: /tmp/gh-aw/agent', self.review_agent)
        self.assertIn("skia-sync-review:v1 native-pr=", self.review_agent)
        self.assertIn("artifact=$EVIDENCE_ARTIFACT_ID", self.review_agent)
        self.assertIn("skia-review-evidence", self.review_agent)
        self.assertIn("Generated Files", self.review_agent)
        self.assertIn("HTML preview after persistence", self.review_agent)
        self.assertIn("find \"$root/$RAW_ARTIFACT\" -type f -name '*.log'", self.review_agent)
        self.assertIn("${{ needs.agent.outputs.artifact_prefix }}agent", self.review_agent)
        self.assertLess(self.review_agent.index("name: skia-review-evidence"), self.review_agent.index("skia-sync-review:v1 native-pr="))
        self.assertIn("companionPr.headSha", self.review_agent)
        self.assertNotIn("environment:", self.review_agent)
        self.assertIn("workflow_call:", self.review_lock)
        self.assertIn("publish_skia_review", self.review_lock)
        self.assertNotIn("SKIASHARP_AUTOBUMP_TOKEN", self.review_lock)
        self.assertNotIn("environment: gh-aw-agents", self.review_lock)
        companion = self.schema["properties"]["companionPr"]
        self.assertIn("headSha", companion["properties"])
        self.assertNotIn("headSha", companion["required"])
        legacy = {
            "meta": {"schemaVersion": "1.0", "skiaPrNumber": 1, "skiasharpPrNumber": 2,
                     "repo": "mono/skia", "upstreamBranch": "chrome/m1",
                     "oldUpstreamBranch": "chrome/m1", "analyzedAt": "2026-01-01T00:00:00Z",
                     "shas": {"prHead": "a" * 40, "base": "b" * 40, "upstream": "c" * 40}},
            "summary": "x" * 50, "recommendations": ["x"], "riskAssessment": "LOW",
            "generatedFiles": {"status": "PASS", "checked": ["x"], "summary": "x", "recommendations": []},
            "upstreamIntegrity": {"status": "PASS", "unchanged": 0, "summary": "x", "recommendations": []},
            "interopIntegrity": {"status": "PASS", "unchanged": 0, "summary": "x", "recommendations": []},
            "depsAudit": {"status": "PASS", "unchanged": 0, "summary": "x", "recommendations": []},
            "companionPr": {"prNumber": 2, "status": "PASS", "summary": "x", "recommendations": [], "unchanged": 0},
        }
        self.assertEqual([], list(Draft202012Validator(self.schema).iter_errors(legacy)))
        self.assertIn('artifact_name=skia-review-evidence', self.persist)
        self.assertIn('WF_NAME="$WR_NAME"', self.persist)
        self.assertIn('WF_NAME=$(gh api', self.persist)

    def test_merge_message_uses_exact_input_target_and_output_contract(self):
        for required in (
            "name: Merge Message Agent",
            "workflow_call:",
            "expected_head_sha:",
            "artifact_name:",
            "post_comment:",
            "jobs:",
            "resolve:",
            "publish-merge-message:",
            "merge-message.json",
            "Upload structured merge message",
            "Post direct merge-message comment",
            "if: steps.validate.outputs.post_comment == 'true'",
            "Call `publish_merge_message` exactly once",
        ):
            self.assertIn(required, self.merge_message)
        self.assertNotIn("add-comment:", self.merge_message)
        self.assertNotIn("SKIASHARP_AUTOBUMP_TOKEN", self.merge_message)
        self.assertNotIn("github.event_name == 'workflow_call'", self.merge_message)
        self.assertIn("name: Merge Message", self.merge_message_command)
        self.assertIn("github.event.comment.body == '/merge-message'", self.merge_message_command)
        for association in ("OWNER", "MEMBER", "COLLABORATOR"):
            self.assertIn(
                f"github.event.comment.author_association == '{association}'",
                self.merge_message_command,
            )
        self.assertIn("uses: ./.github/workflows/merge-message.lock.yml", self.merge_message_command)
        self.assertIn("post_comment: true", self.merge_message_command)
        self.assertIn("workflow_call:", self.merge_message_lock)
        self.assertIn("publish_merge_message", self.merge_message_lock)
        self.assertIn("Upload structured merge message", self.merge_message_lock)
        self.assertNotIn("SKIASHARP_AUTOBUMP_TOKEN", self.merge_message_lock)
        self.assertNotIn("add_comment", self.merge_message_lock)

    def test_merge_state_machine_blocks_failed_or_stale_predecessors(self):
        for required in (
            "apply=true",
            "Command actor lacks write permission.",
            "collaborators/$ACTOR/permission",
            "skia-sync-review-state.py",
            "gh pr checks",
            'all(.[]; .bucket == "pass" or .bucket == "skipping")',
            "Prepare-SkiaReleaseBranches.ps1",
            "gh pr ready",
            'commit_title:$message[0].subject',
            'commit_message:$message[0].body',
            'merge_method:"merge"',
            'merge_method:"squash"',
            "uses: ./.github/workflows/auto-skia-submodule-sync.yml",
            "needs.native-state.result == 'success'",
            "needs.repin.result == 'success'",
            "Native PR changed before merge.",
            "Parent PR changed before merge.",
            "Require trusted main source for apply",
            "Authorize applying maintainer",
            "sleep 20",
            "Repin must contain exactly one mono/skia registration.",
            "registrations=$(jq -c",
            "check_sha mono/skia \"$native_head\"",
            "check_sha mono/SkiaSharp \"$marker\"",
            "review-provenance.json",
            "actions/artifacts/$review_artifact/zip",
            "Review artifact contents do not match the marker.",
            "artifact_name: skia-native-merge-message-${{ github.run_id }}",
            "artifact_name: skia-parent-merge-message-${{ github.run_id }}",
            "Download native merge message",
            "Download parent merge message",
            "Native merge-message artifact is invalid.",
            "Parent merge-message artifact is invalid.",
        ):
            self.assertIn(required, self.merge)
        self.assertEqual(2, self.merge.count("uses: ./.github/workflows/merge-message.lock.yml"))
        self.assertNotIn("parse-merge-message.py", self.merge)
        self.assertNotIn("COMMENT_ID", self.merge)
        self.assertNotIn("comment_url", self.merge)
        self.assertNotIn("parent_repinned", self.merge)
        self.assertNotIn("draft=false", self.merge)
        self.assertNotIn("component.other.commitHash", self.merge)
        self.assertNotIn("expected_head_sha: ${{ needs.repin.outputs.target_sha ||", self.merge)
        self.assertIn('steps.resolve.outputs.parent_merged != \'true\'', self.merge)

    def test_merge_coordinator_uses_only_read_global_permissions(self):
        self.assertTrue(all(value == "read" for value in self.merge_yaml["permissions"].values()))
        self.assertNotIn("contents: write", self.merge)


if __name__ == "__main__":
    unittest.main()
