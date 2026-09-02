#!/usr/bin/env python3
"""Guards the ci-status workflow registry against drift.

The collector previously queried ``nightly-fix-finder.lock.yml`` for months after that
workflow was deleted, and the skill's table advertised an "API Diff" workflow that never
existed. Both failures are invisible at runtime — the GitHub API simply returns nothing for
an unknown workflow file, so the dashboard silently under-reports instead of erroring.

These tests assert that every ``mono/SkiaSharp`` entry in ``GITHUB_WORKFLOWS`` names a real
file, that the declared ``trigger`` still matches that file's ``on:`` block, and that the
display name matches. They parse the committed workflow YAML — including generated
``.lock.yml`` files, which carry their own ``on:`` block — so they never need the gh-aw
compiler to run and are not brittle to lock regeneration.
"""

from __future__ import annotations

import importlib.util
import os
import unittest

import yaml

SCRIPTS_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SKILL_DIR = os.path.dirname(SCRIPTS_DIR)
REPO_ROOT = os.path.abspath(os.path.join(SKILL_DIR, "..", "..", ".."))
WORKFLOW_DIR = os.path.join(REPO_ROOT, ".github", "workflows")


def _load_collector():
    """Import ci-status.py, whose hyphenated name is not a valid module identifier."""
    path = os.path.join(SCRIPTS_DIR, "ci-status.py")
    spec = importlib.util.spec_from_file_location("ci_status_collector", path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


COLLECTOR = _load_collector()
GITHUB_WORKFLOWS = COLLECTOR.GITHUB_WORKFLOWS

# `on:` keys that satisfy each declared trigger. A workflow may legitimately carry more
# triggers than it is tracked for (most also allow workflow_dispatch), so this checks the
# declared trigger is *present*, not that it is exclusive.
TRIGGER_KEYS = {
    "schedule": {"schedule"},
    "dispatch": {"workflow_dispatch"},
    "push": {"push", "pull_request"},
    "event": {
        "issue_comment", "issues", "pull_request", "pull_request_target",
        "pull_request_review", "workflow_run", "push",
    },
}


def local_workflows():
    """Entries owned by this repository (the rest live in mono/SkiaSharp-API-docs)."""
    return [w for w in GITHUB_WORKFLOWS if w["repo"] == "mono/SkiaSharp"]


def load_workflow(name):
    with open(os.path.join(WORKFLOW_DIR, name), encoding="utf-8") as fh:
        return yaml.safe_load(fh)


def on_keys(data):
    # YAML 1.1 parses a bare `on:` key as the boolean True.
    block = data.get("on", data.get(True))
    if isinstance(block, str):
        return {block}
    if isinstance(block, list):
        return set(block)
    return set(block or {})


class RegistryTests(unittest.TestCase):
    def test_registry_is_not_empty(self):
        self.assertTrue(local_workflows(), "No mono/SkiaSharp workflows are tracked.")

    def test_every_tracked_workflow_file_exists(self):
        missing = [w["workflow"] for w in local_workflows()
                   if not os.path.isfile(os.path.join(WORKFLOW_DIR, w["workflow"]))]
        self.assertEqual([], missing,
                         f"ci-status tracks workflow files that do not exist: {missing}")

    def test_declared_trigger_matches_the_workflow(self):
        mismatched = []
        for entry in local_workflows():
            if not os.path.isfile(os.path.join(WORKFLOW_DIR, entry["workflow"])):
                continue  # reported by the test above
            keys = on_keys(load_workflow(entry["workflow"]))
            if not (keys & TRIGGER_KEYS[entry["trigger"]]):
                mismatched.append(
                    f"{entry['workflow']} declares trigger={entry['trigger']} "
                    f"but on={sorted(keys)}")
        self.assertEqual([], mismatched, "; ".join(mismatched))

    def test_display_names_match_the_workflow_name(self):
        wrong = []
        for entry in local_workflows():
            if not os.path.isfile(os.path.join(WORKFLOW_DIR, entry["workflow"])):
                continue
            actual = load_workflow(entry["workflow"]).get("name")
            if actual and actual != entry["name"]:
                wrong.append(
                    f"{entry['workflow']}: registry={entry['name']!r} file={actual!r}")
        self.assertEqual([], wrong, "; ".join(wrong))

    def test_no_duplicate_entries(self):
        seen = [(w["repo"], w["workflow"]) for w in GITHUB_WORKFLOWS]
        self.assertEqual(len(seen), len(set(seen)), "Duplicate workflow entries are tracked.")

    def test_scheduled_workflows_really_have_a_cron(self):
        """A workflow tracked as scheduled but with no cron would always look idle."""
        missing = []
        for entry in local_workflows():
            if entry["trigger"] != "schedule":
                continue
            if not os.path.isfile(os.path.join(WORKFLOW_DIR, entry["workflow"])):
                continue
            data = load_workflow(entry["workflow"])
            block = data.get("on", data.get(True)) or {}
            crons = [s.get("cron") for s in (block.get("schedule") or []) if isinstance(s, dict)]
            if not any(crons):
                missing.append(entry["workflow"])
        self.assertEqual([], missing, f"Tracked as scheduled but define no cron: {missing}")


class SkillDocTests(unittest.TestCase):
    """The skill's table must not advertise workflows the collector does not track."""

    def test_skill_table_only_names_tracked_workflows(self):
        with open(os.path.join(SKILL_DIR, "SKILL.md"), encoding="utf-8") as fh:
            text = fh.read()
        tracked = {w["name"] for w in GITHUB_WORKFLOWS}
        for removed in ("Nightly Fix Finder", "API Diff"):
            if removed in tracked:
                continue
            self.assertNotIn(
                f"| {removed} |", text,
                f"SKILL.md still advertises {removed!r}, which no tracked workflow provides.")


if __name__ == "__main__":
    unittest.main()
