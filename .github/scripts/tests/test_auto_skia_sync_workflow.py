#!/usr/bin/env python3
"""Regression tests for the auto-Skia-sync dispatch and host bootstrap."""

from __future__ import annotations

from pathlib import Path
import base64
import copy
import json
import re
import subprocess
import tempfile
import unittest

import yaml


WORKFLOW = (
    Path(__file__).resolve().parents[2] / "workflows" / "auto-skia-sync.md"
)
COMPILED_WORKFLOW = WORKFLOW.with_suffix(".lock.yml")
REVIEW_WORKFLOW = WORKFLOW.with_name("review-skia-sync.md")


class AutoSkiaSyncWorkflowTests(unittest.TestCase):
    def test_config_archive_surrounds_artifact_transport_and_runtime_restoration(self):
        compiled = yaml.safe_load(COMPILED_WORKFLOW.read_text(encoding="utf-8"))
        steps = compiled["jobs"]["agent"]["steps"]
        names = [step.get("name") for step in steps]
        extract = names.index("Extract trusted agent configuration")
        self.assertGreater(extract, names.index("Download activation artifact"))
        self.assertLess(extract, names.index("Restore agent config folders from base branch"))
        self.assertLess(extract, names.index("Prepare Skia checkout"))
        self.assertLess(extract, names.index("Install native build dependencies"))
        activation = compiled["jobs"]["activation"]["steps"]
        names = [step.get("name") for step in activation]
        archive = names.index("Archive trusted agent configuration")
        self.assertGreater(archive, names.index("Save agent config folders for base branch restoration"))
        self.assertLess(archive, names.index("Upload activation artifact"))
        checkout = next(step for step in activation if step.get("name") == "Checkout .github and .agents folders")
        self.assertNotIn("ref", checkout["with"])
        snapshot = next(step for step in activation if step.get("name") == "Save agent config folders for base branch restoration")
        self.assertEqual(".agents .github", snapshot["env"]["GH_AW_AGENT_FOLDERS"])
        self.assertEqual("AGENTS.md", snapshot["env"]["GH_AW_AGENT_FILES"])

    def test_dispatch_inputs_keep_target_before_optional_branch(self):
        for path, expected in (
            (WORKFLOW, ["target", "target_branch"]),
            (COMPILED_WORKFLOW, ["aw_context", "target", "target_branch"]),
        ):
            with self.subTest(workflow=path.name):
                workflow = path.read_text(encoding="utf-8")
                dispatch = re.search(
                    r"(?m)^  workflow_dispatch:\n    inputs:\n"
                    r"((?:[ \t]+[^\n]*\n)+)",
                    workflow,
                )
                self.assertIsNotNone(dispatch)
                inputs = re.findall(r"(?m)^      (\w+):", dispatch.group(1))
                self.assertEqual(expected, inputs)
                self.assertEqual(sorted(inputs), inputs)

    def test_target_branch_reaches_both_detection_steps_and_concurrency(self):
        for path in (WORKFLOW, COMPILED_WORKFLOW):
            with self.subTest(workflow=path.name):
                workflow = path.read_text(encoding="utf-8")
                self.assertEqual(
                    3,
                    len(re.findall(
                        r"(?m)^\s+SYNC_BASE_BRANCH: "
                        r"\$\{\{ github\.event\.inputs\.target_branch \}\}$",
                        workflow,
                    )),
                )
                self.assertIn(
                    "group: skia-upstream-sync-${{ "
                    "github.event.inputs.target_branch || 'auto' }}",
                    workflow,
                )
                self.assertNotIn("github.event.inputs.base_branch", workflow)

    def test_publication_preserves_history_and_isolates_the_write_token(self):
        lines = WORKFLOW.read_text(encoding="utf-8").splitlines()
        workflow = yaml.safe_load("\n".join(lines[1:lines.index("---", 1)]))
        publisher = workflow["jobs"]["publish-sync"]
        steps = publisher["steps"]
        self.assertEqual("${{ secrets.SKIASHARP_AUTOBUMP_TOKEN }}", steps[-1]["env"]["GH_TOKEN"])
        self.assertEqual("${{ github.sha }}", steps[0]["with"]["ref"])
        self.assertFalse(steps[0]["with"]["persist-credentials"])
        for key in ("env", "steps", "pre-agent-steps", "post-steps"):
            self.assertNotIn("secrets.", str(workflow.get(key, {})))
        self.assertNotIn("create-pull-request", workflow["safe-outputs"])
        self.assertNotIn("jobs", workflow["safe-outputs"])
        compiled = yaml.safe_load(COMPILED_WORKFLOW.read_text(encoding="utf-8"))
        self.assertNotIn("SKIASHARP_AUTOBUMP_TOKEN", str(compiled["jobs"]["agent"]))
        self.assertIn("publish-sync", compiled["jobs"])

    def test_android_workload_uses_repository_sdk_bootstrap(self):
        workflow = WORKFLOW.read_text(encoding="utf-8")

        self.assertIn(
            "./eng/common/dotnet.sh workload install android --skip-sign-check",
            workflow,
        )
        self.assertNotRegex(
            workflow,
            re.compile(
                r"(?m)^\s*dotnet workload install android --skip-sign-check\s*$"
            ),
        )

    def test_review_dispatch_follows_reciprocal_links_and_labels(self):
        publisher = WORKFLOW.parents[1] / "scripts" / "skia-sync-push-prs.sh"
        script = publisher.read_text(encoding="utf-8")
        dispatch = script.index("gh workflow run review-skia-sync.lock.yml")
        self.assertGreater(dispatch, script.rindex('patch_pr mono/SkiaSharp'))
        self.assertGreater(dispatch, script.rindex('apply_labels mono/SkiaSharp'))
        self.assertIn('--ref main', script[dispatch:])
        self.assertIn('-f skiasharp_pr="$SS_PR"', script[dispatch:])


class SkiaReviewActivationTests(unittest.TestCase):
    def setUp(self):
        lines = REVIEW_WORKFLOW.read_text(encoding="utf-8").splitlines()
        self.workflow = yaml.safe_load("\n".join(lines[1:lines.index("---", 1)]))
        self.script = self.workflow["steps"][0]["with"]["script"]
        self.parent = {
            "number": 5841, "state": "open", "title": "[skia-sync] Update skia to milestone 159",
            "head": {"ref": "skia-sync/m159", "sha": "a" * 40, "repo": {"full_name": "mono/SkiaSharp"}},
            "labels": [{"name": "type/milestone-sync"}, {"name": "partner/agentic-workflows"}],
            "body": "Requires https://github.com/mono/skia/pull/470",
        }
        self.native = copy.deepcopy(self.parent)
        self.native.update(number=470, body="Requires https://github.com/mono/SkiaSharp/pull/5841")
        self.native["head"].update(sha="b" * 40, repo={"full_name": "mono/skia"})
        self.manifest = {"registrations": [
            {"component": {"other": {"name": "skia", "version": "chrome/m159"}}},
            {"component": {"git": {"repositoryUrl": "https://github.com/mono/skia.git", "commitHash": "b" * 40}}},
        ]}

    def activate(self, number="5841"):
        harness = r"""
        const input = JSON.parse(require('fs').readFileSync(0, 'utf8'));
        const github = { rest: {
          pulls: { get: async ({repo, pull_number}) => {
            const pr = repo === 'skia' ? input.native : input.parent;
            if (pull_number !== pr.number) throw new Error('Unexpected PR');
            return { data: pr };
          }},
          repos: { getContent: async ({ref}) => {
            if (ref !== input.parent.head.sha) throw new Error('Expected immutable head');
            return { data: { content: input.content } };
          }},
        }};
        const fs = {
          mkdirSync: () => {},
          writeFileSync: (path, content) => process.stdout.write(content),
        };
        const AsyncFunction = Object.getPrototypeOf(async function(){}).constructor;
        new AsyncFunction('github', 'context', 'require', 'process', input.script)(
          github, {repo: {owner: 'mono', repo: 'SkiaSharp'}},
          name => { if (name !== 'fs') throw new Error('Unexpected module'); return fs; },
          {env: {SKIASHARP_PR: input.number}}
        ).catch(error => { console.error(error.message); process.exitCode = 1; });
        """
        return subprocess.run(
            ["node", "-e", harness], text=True, capture_output=True,
            input=json.dumps({
                "script": self.script, "parent": self.parent, "native": self.native,
                "number": number,
                "content": base64.b64encode(json.dumps(self.manifest).encode()).decode(),
            }),
        )

    def test_complete_pair_produces_exact_review_context(self):
        result = self.activate()
        self.assertEqual(0, result.returncode, result.stderr)
        self.assertEqual({
            "skiasharpPr": 5841, "skiaPr": 470, "milestone": 159,
            "skiasharpHead": "a" * 40, "skiaHead": "b" * 40,
        }, json.loads(result.stdout))

    def test_closed_fork_or_unlabelled_prs_do_not_activate(self):
        for field, value in (
            ("state", "closed"),
            ("labels", []),
            ("head", {**self.native["head"], "repo": {"full_name": "someone/skia"}}),
        ):
            with self.subTest(field=field):
                original = self.native[field]
                self.native[field] = value
                result = self.activate()
                self.assertNotEqual(0, result.returncode)
                self.assertIn("not an open, same-repository automatic", result.stderr)
                self.assertEqual("", result.stdout)
                self.native[field] = original

    def test_missing_ambiguous_or_nonreciprocal_links_do_not_activate(self):
        for body in ("", self.parent["body"] + "\nRequires https://github.com/mono/skia/pull/471"):
            with self.subTest(body=body):
                self.parent["body"] = body
                self.assertIn("exactly one required", self.activate().stderr)
        self.parent["body"] = "Requires https://github.com/mono/skia/pull/470"
        self.native["body"] = "Requires https://github.com/mono/SkiaSharp/pull/58410"
        result = self.activate()
        self.assertNotEqual(0, result.returncode)
        self.assertIn("reciprocal links", result.stderr)

    def test_manifest_pin_must_match_native_pr_head(self):
        self.manifest["registrations"][1]["component"]["git"]["commitHash"] = "c" * 40
        result = self.activate()
        self.assertNotEqual(0, result.returncode)
        self.assertIn("exact companion Skia head", result.stderr)

    def test_invalid_dispatch_number_is_rejected_before_fetch(self):
        result = self.activate("5841; echo unsafe")
        self.assertNotEqual(0, result.returncode)
        self.assertIn("positive PR number", result.stderr)

    def test_installed_skill_does_not_trigger_orchestrator_dirty_tree_guard(self):
        step = self.workflow["pre-agent-steps"][0]
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            subprocess.run(["git", "init", "-q", temporary], check=True)
            skill = root / ".github/skills/review-skia-update/SKILL.md"
            skill.parent.mkdir(parents=True)
            skill.write_text("Installed runtime skill\n")
            subprocess.run(["bash", "-e", "-c", step["run"]], cwd=root, check=True)
            status = subprocess.check_output(["git", "status", "--porcelain"], cwd=root, text=True)
            self.assertEqual("", status)


if __name__ == "__main__":
    unittest.main()
