#!/usr/bin/env python3
"""Guards the Auto Triage workflow's migration-only repository allowlist change."""

from __future__ import annotations

import hashlib
import json
import os
import re
import unittest

import yaml

SCRIPTS_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SKILL_DIR = os.path.dirname(SCRIPTS_DIR)
REPO_ROOT = os.path.abspath(os.path.join(SKILL_DIR, "..", "..", ".."))
WORKFLOW_DIR = os.path.join(REPO_ROOT, ".github", "workflows")
SOURCE_PATH = os.path.join(WORKFLOW_DIR, "auto-triage.md")
LOCK_PATH = os.path.join(WORKFLOW_DIR, "auto-triage.lock.yml")

ALLOWED_REPOS = [
    "mono/skiasharp",
    "dotnet/skiasharp",
    "mono/skia",
    "dotnet/skia",
    "google/skia",
]
PROJECT_URL = "https://github.com/orgs/mono/projects/1"
PROJECT_TOKEN = "${{ secrets.GH_AW_WRITE_PROJECT_TOKEN }}"
SOURCE_CONTRACT_SHA256 = "a84f811a7d12bce5a712f4b3106a03b97cdfc6495f6db80030ce6041b473fa39"
SOURCE_BODY_SHA256 = "4a18024ebf8b7adda4858ae808290dee4de8629afc136afcbfdc071b884e4078"
LOCK_BODY_SHA256 = "54be34cd79632f70c9f3e965ca3e1a35c569abdb8f0bb4f284599d2cf02be8bb"


def load_source():
    with open(SOURCE_PATH, encoding="utf-8") as fh:
        text = fh.read()
    match = re.fullmatch(r"---\n(.*?)\n---\n(.*)", text, re.DOTALL)
    if not match:
        raise ValueError("auto-triage.md does not contain valid YAML frontmatter")
    return yaml.safe_load(match.group(1)), match.group(2)


def load_lock():
    with open(LOCK_PATH, encoding="utf-8") as fh:
        text = fh.read()
    return yaml.safe_load(text), text


def comment_json(text, name):
    match = re.search(rf"^# gh-aw-{name}: (.+)$", text, re.MULTILINE)
    if not match:
        raise ValueError(f"auto-triage.lock.yml has no gh-aw-{name} header")
    return json.loads(match.group(1))


def values_for(data, key):
    values = []
    if isinstance(data, dict):
        for current_key, value in data.items():
            if current_key == key:
                values.append(value)
            values.extend(values_for(value, key))
    elif isinstance(data, list):
        for value in data:
            values.extend(values_for(value, key))
    return values


class AutoTriageWorkflowTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.source, cls.body = load_source()
        cls.lock, cls.lock_text = load_lock()
        cls.metadata = comment_json(cls.lock_text, "metadata")
        cls.manifest = comment_json(cls.lock_text, "manifest")

    def test_source_and_lock_have_the_exact_transition_allowlist(self):
        source_repos = self.source["tools"]["github"]["allowed-repos"]
        lock_values = values_for(self.lock, "GH_AW_GITHUB_REPOS")

        self.assertEqual(ALLOWED_REPOS, source_repos)
        self.assertEqual([json.dumps(ALLOWED_REPOS, separators=(",", ":"))], lock_values)

    def test_source_preserves_the_existing_behavior_and_prompt(self):
        source_contract = {
            "description": self.source["description"],
            "engine": self.source["engine"],
            "model": self.source["model"],
            "on": self.source[True],
            "jobs": self.source["jobs"],
            "if": self.source["if"],
            "steps": self.source["steps"],
            "tools": json.loads(json.dumps(self.source["tools"])),
            "permissions": self.source["permissions"],
            "network": self.source["network"],
            "safe-outputs": self.source["safe-outputs"],
        }
        source_contract["tools"]["github"].pop("allowed-repos")
        encoded = json.dumps(source_contract, sort_keys=True, separators=(",", ":")).encode()

        self.assertEqual(SOURCE_CONTRACT_SHA256, hashlib.sha256(encoded).hexdigest())
        self.assertEqual(SOURCE_BODY_SHA256, hashlib.sha256(self.body.encode()).hexdigest())

    def test_mono_project_configuration_is_unchanged(self):
        update_project = self.source["safe-outputs"]["update-project"]
        safe_outputs = json.loads(values_for(self.lock, "GH_AW_SAFE_OUTPUTS_HANDLER_CONFIG")[0])

        self.assertEqual(PROJECT_URL, update_project["project"])
        self.assertEqual(PROJECT_TOKEN, update_project["github-token"])
        self.assertEqual(PROJECT_URL, safe_outputs["update_project"]["project"])
        self.assertEqual(PROJECT_TOKEN, safe_outputs["update_project"]["github-token"])

    def test_compiled_lock_preserves_runtime_contract(self):
        expected_permissions = {
            "activation": {"actions": "read", "contents": "read"},
            "agent": {"contents": "read", "issues": "read"},
            "conclusion": {"actions": "read", "issues": "write", "pull-requests": "write"},
            "detection": {"contents": "read"},
            "pre_activation": {"issues": "read"},
            "safe_outputs": {"issues": "write", "pull-requests": "write"},
        }
        expected_mcp_servers = [
            {
                "name": "github",
                "tools": ["issue_read", "list_issue_types", "list_issues", "search_issues"],
            },
            {
                "name": "safeoutputs",
                "tools": ["add_labels", "missing_data", "missing_tool", "noop", "update_project"],
            },
        ]

        self.assertEqual("Sync - Issue Triage", self.lock["name"])
        self.assertEqual({}, self.lock["permissions"])
        self.assertEqual(
            {
                "schedule": [{"cron": "5 4 * * *"}],
                "workflow_dispatch": {
                    "inputs": {
                        "aw_context": {
                            "default": "",
                            "description": "Agent caller context (used internally by Agentic Workflows).",
                            "required": False,
                            "type": "string",
                        },
                        "issue_number": {
                            "description": "Issue number to triage (leave blank for auto-select)",
                            "required": False,
                            "type": "string",
                        },
                    },
                },
            },
            self.lock[True],
        )
        self.assertEqual(
            expected_permissions,
            {name: job.get("permissions", {}) for name, job in self.lock["jobs"].items()},
        )
        self.assertEqual(["gpt-5.6-terra"], values_for(self.lock, "GH_AW_INFO_MODEL"))
        self.assertEqual(["[\"defaults\",\"python\"]"], values_for(self.lock, "GH_AW_INFO_ALLOWED_DOMAINS"))
        self.assertEqual(["0"], values_for(self.lock, "GH_AW_ACTION_FAILURE_ISSUE_EXPIRES_HOURS"))
        self.assertEqual(expected_mcp_servers, self.manifest["mcp_servers"])
        self.assertTrue(self.metadata["strict"])
        self.assertEqual("copilot", self.metadata["agent_id"])
        self.assertEqual("gpt-5.6-terra", self.metadata["agent_model"])
        self.assertEqual(LOCK_BODY_SHA256, self.metadata["body_hash"])


if __name__ == "__main__":
    unittest.main()
