#!/usr/bin/env python3
"""Network-free contracts for the review-only Skia sync command."""

from __future__ import annotations

import importlib.util
import json
from pathlib import Path
import unittest

import yaml


ROOT = Path(__file__).resolve().parents[3]
WORKFLOWS = ROOT / ".github" / "workflows"


def load_script(name, relative_path):
    path = ROOT / relative_path
    spec = importlib.util.spec_from_file_location("resolve_skia_sync_pair", path)
    assert spec and spec.loader
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


RESOLVER = load_script("resolve_skia_sync_pair", ".github/scripts/resolve-skia-sync-pair.py")
EVIDENCE = load_script("prepare_skia_sync_review_evidence", ".github/scripts/prepare-skia-sync-review-evidence.py")
CONTRACT = load_script("verify_skia_sync_review_contract", ".github/scripts/verify-skia-sync-review-contract.py")
VALIDATOR = load_script("validate_skia_review", ".agents/skills/review-skia-update/scripts/validate-skia-review.py")


def pr(repository, number, *, branch="skia-sync/m147", base="main", body=""):
    return {
        "number": number,
        "title": "[skia-sync] Update Skia chrome/m147",
        "body": body,
        "html_url": f"https://github.com/{repository}/pull/{number}",
        "state": "open",
        "draft": False,
        "user": {"login": "octocat"},
        "head": {"repo": {"full_name": repository}, "ref": branch, "sha": "a" * 40},
        "base": {"repo": {"full_name": repository}, "ref": base, "sha": "b" * 40},
    }


def review_pair(*, parent_base="main", branch="skia-sync/m147"):
    native_base = "skiasharp" if parent_base == "main" else parent_base
    parent = pr(
        "mono/SkiaSharp",
        5125,
        base=parent_base,
        branch=branch,
        body="https://github.com/mono/skia/pull/401",
    )
    native = pr(
        "mono/skia",
        401,
        base=native_base,
        branch=branch,
        body="https://github.com/mono/SkiaSharp/pull/5125",
    )
    return parent, native


class SkiaSyncPairResolverTests(unittest.TestCase):
    def test_freezes_a_valid_main_pair(self):
        contract = RESOLVER.resolve_pair(*review_pair())
        self.assertEqual("review", contract["mode"])
        self.assertEqual(147, contract["milestone"])
        self.assertEqual("skiasharp", contract["native"]["base_branch"])
        self.assertEqual("a" * 40, contract["parent"]["head_sha"])
        self.assertEqual("b" * 40, contract["native"]["base_sha"])

    def test_release_base_uses_the_same_native_base(self):
        contract = RESOLVER.resolve_pair(*review_pair(parent_base="release/3.119.x"))
        self.assertEqual("release/3.119.x", contract["native"]["base_branch"])

    def test_release_branch_extracts_one_metadata_milestone(self):
        parent, native = review_pair(branch="skia-sync/release-3.119.x")
        contract = RESOLVER.resolve_pair(parent, native)
        self.assertEqual(147, contract["milestone"])

    def test_rejects_ambiguous_links_unshared_branch_main_tip_and_wrong_base(self):
        parent, native = review_pair()
        parent["body"] += "\nhttps://github.com/mono/skia/pull/402"
        with self.assertRaises(RESOLVER.PairValidationError):
            RESOLVER.resolve_pair(parent, native)

        parent, native = review_pair(branch="skia-sync/main")
        with self.assertRaises(RESOLVER.PairValidationError):
            RESOLVER.resolve_pair(parent, native)

        parent, native = review_pair()
        native["base"]["ref"] = "main"
        with self.assertRaises(RESOLVER.PairValidationError):
            RESOLVER.resolve_pair(parent, native)

    def test_rejects_non_exact_link_suffix_and_milestone_disagreement(self):
        parent, native = review_pair()
        parent["body"] = "https://github.com/mono/skia/pull/401/files"
        with self.assertRaises(RESOLVER.PairValidationError):
            RESOLVER.resolve_pair(parent, native)

        parent, native = review_pair()
        native["title"] = "[skia-sync] Update Skia chrome/m148"
        with self.assertRaises(RESOLVER.PairValidationError):
            RESOLVER.resolve_pair(parent, native)


class SkiaSyncEvidenceTests(unittest.TestCase):
    def test_canonical_evidence_derives_mechanical_statuses_and_risk(self):
        contract = RESOLVER.resolve_pair(*review_pair())
        raw = {
            "meta": {
                "skiaPrNumber": 401, "skiasharpPrNumber": 5125,
                "upstreamBranch": "chrome/m147", "oldUpstreamBranch": "chrome/m146",
                "analyzedAt": "2026-09-18T00:00:00Z",
                "shas": {"prHead": "a" * 40, "base": "b" * 40, "upstream": "c" * 40},
            },
            "generatedFiles": {"status": "PASS", "checked": ["binding/Generated"]},
            "upstreamIntegrity": {"status": "PASS", "unchanged": 0},
            "interopIntegrity": {"status": "PASS", "unchanged": 0},
            "depsAudit": {"status": "PASS", "unchanged": 0},
            "companionPr": {"prNumber": 5125, "status": "PASS", "unchanged": 0},
        }
        report = {
            "summary": "A sufficiently detailed independent review summary for validation.",
            "recommendations": ["Review the immutable evidence artifact."],
            **{
                name: {"summary": f"{name} summary", "recommendations": ["No additional action."]}
                for name in ("generatedFiles", "upstreamIntegrity", "interopIntegrity", "depsAudit", "companionPr")
            },
        }
        result = EVIDENCE.build(raw, report, contract)
        self.assertEqual("LOW", result["riskAssessment"])
        self.assertEqual("PASS", result["generatedFiles"]["status"])
        self.assertEqual("a" * 40, result["companionPr"]["headSha"])
        self.assertEqual([], VALIDATOR._schema_errors(result))

    def test_evidence_rejects_ai_controlled_status_and_credentials(self):
        contract = RESOLVER.resolve_pair(*review_pair())
        raw = {
            "meta": {"skiaPrNumber": 401, "skiasharpPrNumber": 5125, "upstreamBranch": "chrome/m147", "oldUpstreamBranch": "chrome/m146", "analyzedAt": "2026-09-18T00:00:00Z", "shas": {"prHead": "a" * 40, "base": "b" * 40, "upstream": "c" * 40}},
            "generatedFiles": {"status": "PASS", "checked": ["x"]},
            "upstreamIntegrity": {"status": "PASS", "unchanged": 0},
            "interopIntegrity": {"status": "PASS", "unchanged": 0},
            "depsAudit": {"status": "PASS", "unchanged": 0},
            "companionPr": {"prNumber": 5125, "status": "PASS", "unchanged": 0},
        }
        report = {"summary": "A sufficiently detailed independent review summary for validation.", "recommendations": ["Review it."], **{name: {"summary": name, "recommendations": []} for name in ("generatedFiles", "upstreamIntegrity", "interopIntegrity", "depsAudit", "companionPr")}}
        raw["upstreamIntegrity"]["added"] = [{"path": "src/x"}]
        with self.assertRaises(EVIDENCE.EvidenceError):
            EVIDENCE.build(raw, report, contract)
        raw["upstreamIntegrity"]["added"] = []
        report["summary"] = "ghp_" + "a" * 36
        with self.assertRaises(EVIDENCE.EvidenceError):
            EVIDENCE.scan_secrets(EVIDENCE.build(raw, report, contract))

    def test_companion_only_change_has_medium_risk(self):
        contract = RESOLVER.resolve_pair(*review_pair())
        raw = {
            "meta": {"skiaPrNumber": 401, "skiasharpPrNumber": 5125, "upstreamBranch": "chrome/m147", "oldUpstreamBranch": "chrome/m146", "analyzedAt": "2026-09-18T00:00:00Z", "shas": {"prHead": "a" * 40, "base": "b" * 40, "upstream": "c" * 40}},
            "generatedFiles": {"status": "PASS", "checked": ["x"]},
            "upstreamIntegrity": {"status": "PASS", "unchanged": 0},
            "interopIntegrity": {"status": "PASS", "unchanged": 0},
            "depsAudit": {"status": "PASS", "unchanged": 0},
            "companionPr": {"prNumber": 5125, "status": "REVIEW_REQUIRED", "added": [{"path": "binding/SkiaSharp/SKFoo.cs"}], "unchanged": 0},
        }
        report = {
            "summary": "A sufficiently detailed companion-only review summary for validation.",
            "recommendations": ["Review the managed companion change."],
            **{name: {"summary": name, "recommendations": []} for name in ("generatedFiles", "upstreamIntegrity", "interopIntegrity", "depsAudit")},
            "companionPr": {
                "summary": "Managed companion change requires review.",
                "recommendations": ["Review the new wrapper."],
                "added": [{"path": "binding/SkiaSharp/SKFoo.cs", "summary": "Adds the managed wrapper."}],
            },
        }
        result = EVIDENCE.build(raw, report, contract)
        self.assertEqual("MEDIUM", result["riskAssessment"])
        self.assertEqual([], VALIDATOR._schema_errors(result))
        result["riskAssessment"] = "LOW"
        self.assertIn(
            "riskAssessment should be MEDIUM or HIGH, not LOW",
            VALIDATOR._schema_errors(result),
        )

    def test_verifier_requires_each_frozen_contract_value(self):
        pair = RESOLVER.resolve_pair(*review_pair())
        expected = {
            "milestone": pair["milestone"], "head_branch": pair["head_branch"],
            "repositories": pair["repositories"], "links": pair["links"],
            **{side: {key: pair[side][key] for key in ("number", "head_branch", "head_sha", "base_branch", "base_sha")} for side in ("parent", "native")},
        }
        CONTRACT.verify(pair, expected)
        pair["native"]["base_sha"] = "c" * 40
        with self.assertRaises(CONTRACT.ContractError):
            CONTRACT.verify(pair, expected)


class SkiaSyncReviewWorkflowTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.entry = (WORKFLOWS / "skia-sync-review.yml").read_text(encoding="utf-8")
        cls.entry_yaml = yaml.load(cls.entry, Loader=yaml.BaseLoader)
        cls.agent = (WORKFLOWS / "skia-sync-review.md").read_text(encoding="utf-8")
        cls.lock = (WORKFLOWS / "skia-sync-review.lock.yml").read_text(encoding="utf-8")
        cls.persist = (WORKFLOWS / "persist-aw-data.yml").read_text(encoding="utf-8")
        cls.action_lock = json.loads(
            (ROOT / ".github/aw/actions-lock.json").read_text(encoding="utf-8")
        )
        cls.schema = json.loads(
            (ROOT / ".agents/skills/review-skia-update/references/skia-review-schema.json").read_text(
                encoding="utf-8"
            )
        )

    def test_command_dispatch_and_frozen_pair_contract(self):
        self.assertIn("workflow_dispatch", self.entry_yaml["on"])
        self.assertIn("issue_comment", self.entry_yaml["on"])
        self.assertIn("github.event.comment.body == '/skia-sync-review'", self.entry)
        for association in ("OWNER", "MEMBER", "COLLABORATOR"):
            self.assertIn(f"github.event.comment.author_association == '{association}'", self.entry)
        self.assertIn("skiasharp_pr:", self.entry)
        self.assertIn("staged:", self.entry)
        self.assertIn("default: true", self.entry)
        self.assertIn("github.ref != 'refs/heads/main'", self.entry)
        self.assertIn("resolve-skia-sync-pair.py", self.entry)
        self.assertIn("skia-sync-review-pair-${{ github.run_id }}", self.entry)
        for value in ("head_branch", "milestone", "parent_base", "parent_base_sha", "native_base", "native_base_sha"):
            self.assertIn(f"{value}:", self.entry)

    def test_mechanical_stage_is_tokenless_and_exports_only_prepared_evidence(self):
        mechanical = self.entry.split("  mechanical-review:", 1)[1].split("\n  review:", 1)[0]
        self.assertNotIn("GH_TOKEN:", mechanical)
        self.assertNotIn("GITHUB_TOKEN:", mechanical)
        self.assertNotIn("actions/checkout", mechanical)
        self.assertIn("git init .", mechanical)
        self.assertIn("docker run --rm", mechanical)
        self.assertIn("$GITHUB_WORKSPACE:/trusted:ro", mechanical)
        self.assertIn("$PAIR_JSON:/control/pair.json:ro", mechanical)
        self.assertIn("tools/git-sync-deps", mechanical)
        self.assertIn("/export/generator-check.log", mechanical)
        self.assertNotIn("check-generated-files.json", mechanical)
        self.assertIn('cp "$export_root/generator-output.log" "$RAW_DIR/generator-output.log"', mechanical)
        self.assertIn("--prepared-generated-root", mechanical)
        self.assertIn("-u GITHUB_ENV -u GITHUB_OUTPUT -u GITHUB_PATH -u GITHUB_STEP_SUMMARY", mechanical)
        self.assertIn("-u GITHUB_TOKEN -u GH_TOKEN", mechanical)
        self.assertEqual(1, mechanical.count("scripts/run_review.py"))
        self.assertLess(
            self.entry.index("name: skia-sync-review-raw-${{ github.run_id }}"),
            self.entry.index("uses: ./.github/workflows/skia-sync-review.lock.yml"),
        )

    def test_agent_and_publisher_separate_authority_and_artifact_order(self):
        self.assertIn("model: gpt-5.6-terra", self.agent)
        self.assertIn("Call `publish_skia_review` exactly once", self.agent)
        prompt = self.agent.split("---\n\n# Analyze prepared", 1)[1]
        self.assertNotIn("GH_TOKEN", prompt)
        self.assertNotIn("comment target", prompt)
        self.assertIn("companionPr.headSha", self.agent)
        self.assertIn("DETECTION_SUCCESS", self.agent)
        self.assertIn("verify-skia-sync-review-contract.py", self.agent)
        self.assertIn("repositories:{parent:\"mono/SkiaSharp\",native:\"mono/skia\"}", self.agent)
        self.assertIn("links:{parent_to_native:$native_pr,native_to_parent:$parent_pr}", self.agent)
        self.assertIn("prepare-skia-sync-review-evidence.py", self.agent)
        self.assertIn("report-sha256=$REPORT_SHA256", self.agent)
        self.assertIn("skia-review-evidence", self.agent)
        self.assertLess(
            self.agent.index("name: skia-review-evidence"),
            self.agent.index("skia-sync-review:v1 milestone="),
        )
        self.assertIn("This is schema-validated review evidence, not approval.", self.agent)
        self.assertIn("Generated Files", self.agent)
        self.assertNotIn("pip install", self.agent)
        self.assertNotIn("jsonschema", self.agent)
        self.assertIn("workflow_call:", self.lock)
        self.assertIn("publish_skia_review", self.lock)
        self.assertNotIn("SKIASHARP_AUTOBUMP_TOKEN", self.lock)
        self.assertNotIn("${{ secrets.GH_AW_DEFAULT_OTLP_HEADERS }}", self.lock)
        self.assertEqual(
            "ea165f8d65b6e75b540449e92b4886f43607fa02",
            self.action_lock["entries"]["actions/upload-artifact@v4"]["sha"],
        )

    def test_schema_is_backward_compatible_and_persistence_is_immutable(self):
        companion = self.schema["properties"]["companionPr"]
        self.assertIn("headSha", companion["properties"])
        self.assertNotIn("headSha", companion["required"])
        legacy = {
            "meta": {
                "schemaVersion": "1.0", "skiaPrNumber": 1, "skiasharpPrNumber": 2,
                "repo": "mono/skia", "upstreamBranch": "chrome/m1",
                "oldUpstreamBranch": "chrome/m1", "analyzedAt": "2026-01-01T00:00:00Z",
                "shas": {"prHead": "a" * 40, "base": "b" * 40, "upstream": "c" * 40},
            },
            "summary": "x" * 50, "recommendations": ["x"], "riskAssessment": "LOW",
            "generatedFiles": {"status": "PASS", "checked": ["x"], "summary": "x", "recommendations": []},
            "upstreamIntegrity": {"status": "PASS", "unchanged": 0, "summary": "x", "recommendations": []},
            "interopIntegrity": {"status": "PASS", "unchanged": 0, "summary": "x", "recommendations": []},
            "depsAudit": {"status": "PASS", "unchanged": 0, "summary": "x", "recommendations": []},
            "companionPr": {"prNumber": 2, "status": "PASS", "summary": "x", "recommendations": [], "unchanged": 0},
        }
        self.assertEqual([], VALIDATOR._schema_errors(legacy))
        self.assertIn('"Review - Skia Sync") echo "ai-review"', self.persist)
        self.assertIn('test("^skia-review-evidence-[0-9]+-[0-9]+$")', self.persist)
        self.assertIn("Refusing to overwrite immutable persisted evidence", self.persist)
        self.assertIn('WF_NAME="$WR_NAME"', self.persist)
        self.assertIn("ai-review/$NATIVE_PR/$NATIVE_HEAD/$PARENT_HEAD/run-$GITHUB_RUN_ID-$GITHUB_RUN_ATTEMPT/$report_sha", self.agent)

    def test_telemetry_is_blank_and_removed_before_agent_execution(self):
        for name in (
            "OTEL_EXPORTER_OTLP_HEADERS",
            "OTEL_EXPORTER_OTLP_ENDPOINT",
            "OTEL_RESOURCE_ATTRIBUTES",
            "GH_AW_DEFAULT_OTLP_HEADERS",
            "GH_AW_OTLP_ENDPOINTS",
        ):
            self.assertIn(f"{name}: \"\"", self.agent)
            self.assertIn(f'unset "$name"', self.agent)
        self.assertIn("Remove telemetry credentials before agent execution", self.lock)

    def test_review_surface_does_not_coordinate_sync_landing(self):
        combined = self.entry + self.agent
        for forbidden in (
            "skia-sync-merge",
            "auto-skia-submodule-sync",
            "merge-message",
            "gh pr create",
            "gh pr merge",
            "SKIASHARP_AUTOBUMP_TOKEN",
            "release-prepare",
        ):
            with self.subTest(forbidden=forbidden):
                self.assertNotIn(forbidden, combined)
        registry = (ROOT / ".agents/skills/ci-status/scripts/ci-status.py").read_text(encoding="utf-8")
        self.assertIn('"workflow": "skia-sync-review.yml"', registry)


if __name__ == "__main__":
    unittest.main()
