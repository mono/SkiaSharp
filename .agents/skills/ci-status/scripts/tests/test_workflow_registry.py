#!/usr/bin/env python3
"""Guards the ci-status workflow registry against drift.

The registry is a hand-maintained list of workflow *filenames*. Nothing previously
checked those filenames against the repository, and the failure mode is not an error —
it is a plausible-looking dashboard row.

Two real examples this suite would have caught:

* ``nightly-fix-finder.lock.yml`` was tracked but has never existed on ``main``; it only
  ever lived on an unmerged branch. GitHub still has the workflow registered from that
  branch push, so the API returns it as ``state: active`` with 17 successful runs from
  2026-05-15 rather than 404ing. Because the entry is ``scope: global`` the collector
  queries it with no branch filter, so those stale branch runs are reported as the
  workflow's current health — a green row for a workflow that does not exist.
* ``SKILL.md`` advertised an "API Diff" workflow. That one *did* exist (added 2026-05-22,
  removed 2026-06-18 when it was folded into "Sync - Release Notes & API Diffs"), so the
  row was a stale leftover quietly duplicating a workflow already listed below it.

These tests assert that every ``mono/SkiaSharp`` entry names a file that exists on this
branch, that the declared ``trigger`` still matches that file's ``on:`` block, and that the
display name matches. They parse the committed workflow YAML — including generated
``.lock.yml`` files, which carry their own ``on:`` block — so they never need the gh-aw
compiler to run and are not brittle to lock regeneration.
"""

from __future__ import annotations

import importlib.util
import os
import re
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


def on_block(data):
    # YAML 1.1 parses a bare `on:` key as the boolean True.
    return data.get("on", data.get(True)) or {}


def on_keys(data):
    block = on_block(data)
    if isinstance(block, str):
        return {block}
    if isinstance(block, list):
        return set(block)
    return set(block or {})


def crons_for(workflow_file):
    block = on_block(load_workflow(workflow_file))
    if not isinstance(block, dict):
        return []
    return [s.get("cron") for s in (block.get("schedule") or [])
            if isinstance(s, dict) and s.get("cron")]


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
        checked = 0
        for entry in local_workflows():
            if entry["trigger"] != "schedule":
                continue
            if not os.path.isfile(os.path.join(WORKFLOW_DIR, entry["workflow"])):
                continue
            checked += 1
            if not crons_for(entry["workflow"]):
                missing.append(entry["workflow"])
        self.assertEqual([], missing, f"Tracked as scheduled but define no cron: {missing}")
        # `missing` is also empty when nothing was examined at all. Re-typing the tracked
        # schedule entries to another trigger empties this loop while leaving the registry
        # full, and it does not trip the trigger test either, because a scheduled workflow
        # almost always declares workflow_dispatch as well. Assert the loop did work.
        self.assertGreater(
            checked, 0,
            "No scheduled workflow was examined, so this test asserted nothing — "
            "the registry has no entry with trigger='schedule'.")


class SkillDocTests(unittest.TestCase):
    """The skill's table must not advertise workflows the collector does not track."""

    def _skill_text(self):
        with open(os.path.join(SKILL_DIR, "SKILL.md"), encoding="utf-8") as fh:
            return fh.read()

    def test_skill_table_only_names_tracked_workflows(self):
        rows = [ln for ln in self._skill_text().splitlines() if ln.startswith("| ")]
        tracked = {w["name"] for w in GITHUB_WORKFLOWS}
        for removed in ("Nightly Fix Finder", "API Diff"):
            if removed in tracked:
                continue
            offending = [r for r in rows if r.startswith(f"| {removed} |")]
            self.assertEqual(
                [], offending,
                f"SKILL.md still advertises {removed!r}, which no tracked workflow provides.")

    def test_documented_schedules_match_the_workflow(self):
        """Any schedule the table states must be true of the workflow it names.

        gh-aw re-jitters the cron of every generated lock on upgrade, so a documented
        `HH:MM UTC` or literal cron silently goes stale the next time the compiler runs.
        Either omit the precision or keep it correct — this test refuses the third option.
        """
        rows = [ln for ln in self._skill_text().splitlines() if ln.startswith("| ")]
        by_name = {w["name"]: w["workflow"] for w in local_workflows()}
        stale = []
        unresolved = []
        compared = 0
        for row in rows:
            cells = [c.strip() for c in row.strip().strip("|").split("|")]
            if len(cells) < 3 or cells[1] != "mono/SkiaSharp":
                continue  # header, separator, or a row owned by another repository
            name = cells[0]
            if name not in by_name:
                # A renamed workflow must fail loudly rather than silently disabling the
                # schedule check for its row.
                unresolved.append(name)
                continue
            if not os.path.isfile(os.path.join(WORKFLOW_DIR, by_name[name])):
                continue
            crons = crons_for(by_name[name])
            trigger_cell = cells[2]

            for literal in re.findall(r"`([^`]*\*[^`]*)`", trigger_cell):
                compared += 1
                if literal not in crons:
                    stale.append(f"{name}: documents cron {literal!r}, file has {crons}")

            for hh, mm in re.findall(r"\b(\d{1,2}):(\d{2}) UTC", trigger_cell):
                compared += 1
                actual = {(c.split()[1], c.split()[0]) for c in crons if len(c.split()) >= 2}
                if (str(int(hh)), str(int(mm))) not in actual:
                    stale.append(f"{name}: documents {hh}:{mm} UTC, file has {crons}")
        # Report both independently: an unresolved row must not mask a stale schedule.
        self.assertEqual([], stale, "; ".join(stale))
        self.assertEqual([], unresolved,
                         f"SKILL.md rows name untracked workflows: {unresolved}")
        # A pass here means nothing unless at least one schedule was actually compared.
        # Strip every documented time from the table and the loops above simply never run,
        # leaving a green test that checks nothing — the same silent-pass shape this suite
        # exists to catch. Hand-written workflows must keep their exact time documented;
        # only gh-aw locks are allowed to be imprecise.
        self.assertGreater(
            compared, 0,
            "No documented schedule was compared, so this test asserted nothing. "
            "At least one SKILL.md row must state an exact 'HH:MM UTC' or literal cron "
            "for a hand-written (non-lock) workflow.")


class CollectorWiringTests(unittest.TestCase):
    """The registry is only worth validating if the collector actually reads it.

    Every other test here checks the *contents* of GITHUB_WORKFLOWS. None of them
    execute ``collect_github_data``, so the line joining the registry to the collector
    is untested: point that loop at an empty list, or at a hardcoded entry, and the
    whole suite still passes while the dashboard reports nothing — or reports a
    workflow that was never tracked, which is the exact failure this suite exists to
    catch. This test covers the wiring rather than the parts.
    """

    def _collect_with_stubs(self):
        """Run the collector with every network call replaced by a recorder."""
        requested = []

        def fake_runs(repo, workflow, branch, top=5):
            requested.append((repo, workflow))
            return []

        original_runs = COLLECTOR.get_github_workflow_runs
        original_enrich = COLLECTOR._enrich_failed_runs
        COLLECTOR.get_github_workflow_runs = fake_runs
        COLLECTOR._enrich_failed_runs = lambda wf_data: None
        try:
            collected = COLLECTOR.collect_github_data(["main"], 1)
        finally:
            COLLECTOR.get_github_workflow_runs = original_runs
            COLLECTOR._enrich_failed_runs = original_enrich
        return requested, collected

    def test_collector_queries_exactly_the_tracked_workflows(self):
        requested, collected = self._collect_with_stubs()
        self.assertTrue(requested, "collect_github_data queried nothing at all.")
        self.assertEqual(
            {(w["repo"], w["workflow"]) for w in GITHUB_WORKFLOWS},
            set(requested),
            "collect_github_data did not query exactly the tracked workflows — the "
            "registry and the collector have come apart.")
        self.assertEqual(
            len(GITHUB_WORKFLOWS), len(collected),
            "collect_github_data returned a different number of entries than it tracks.")

    def test_report_carries_the_collected_workflows_through_unchanged(self):
        """The assembly layer must not drop or filter what the collector returned.

        Covering collect_github_data is not enough: the payload is assembled a layer up,
        and that join can silently narrow the result. Replacing the assignment with a
        filtered comprehension — the same 'drop the branch-scoped entries' mistake this
        suite catches one level down — leaves a report that still looks complete.
        `branches=[]` skips the Azure DevOps loop entirely, so this needs no `az`.
        """
        # Shaped like a real entry so a filtering mutation fails on the assertion below
        # rather than incidentally raising KeyError on a missing field.
        sentinel = [
            {"repo": "mono/SkiaSharp", "workflow": "sentinel-a.yml", "name": "Sentinel A",
             "scope": "branch", "trigger": "push", "branches": [], "runs": [],
             "stats": None, "error": None},
            {"repo": "mono/SkiaSharp", "workflow": "sentinel-b.yml", "name": "Sentinel B",
             "scope": "global", "trigger": "schedule", "branches": [], "runs": [],
             "stats": None, "error": None},
        ]
        original = COLLECTOR.collect_github_data
        COLLECTOR.collect_github_data = lambda branches, top_builds: list(sentinel)
        try:
            data = COLLECTOR.collect_data([], 1, False)
        finally:
            COLLECTOR.collect_github_data = original

        self.assertEqual(
            sentinel, data["github_actions"],
            "The report did not carry through what collect_github_data returned — the "
            "assembly layer dropped, filtered or replaced it.")
        self.assertEqual(
            GITHUB_WORKFLOWS, data["github_workflows"],
            "The report's github_workflows payload is not the tracked registry.")


if __name__ == "__main__":
    unittest.main()
