#!/usr/bin/env python3

import importlib.util
from pathlib import Path
import sys
from types import SimpleNamespace
import unittest
from unittest import mock


SCRIPTS = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(SCRIPTS))


def load(name: str, filename: str):
    path = SCRIPTS / filename
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    return module


push = load("push_release_packages", "push-release-packages.py")
finalize = load("finalize_release", "finalize-release.py")


COMMON = [
    "release/4.152.0",
    "--expect-source-sha",
    "a" * 40,
    "--expect-managed-run",
    "10",
    "--expect-tests-run",
    "20",
]


class PublishCommandTests(unittest.TestCase):
    def test_no_flag_means_execution(self):
        self.assertFalse(push.create_parser().parse_args(COMMON).dry_run)
        self.assertFalse(finalize.create_parser().parse_args(COMMON).dry_run)

    def test_dry_run_is_explicit(self):
        self.assertTrue(
            push.create_parser().parse_args([*COMMON, "--dry-run"]).dry_run
        )
        self.assertTrue(
            finalize.create_parser()
            .parse_args([*COMMON, "--dry-run"])
            .dry_run
        )

    def test_execution_commands_omit_dry_run(self):
        push_args = push.create_parser().parse_args(
            [*COMMON, "--dry-run"]
        )
        push_command = push.execution_command(push_args, "a" * 40)
        self.assertNotIn("--dry-run", push_command)

        finalize_args = finalize.create_parser().parse_args(
            [
                *COMMON,
                "--previous-tag",
                "v4.151.0",
                "--teaser-file",
                "teaser.md",
                "--dry-run",
            ]
        )
        finalize_command = finalize.execution_command(
            finalize_args,
            "a" * 40,
        )
        self.assertNotIn("--dry-run", finalize_command)
        self.assertIn("--teaser-file teaser.md", finalize_command)

    def test_finalization_statuses_match_actionable_state(self):
        missing_previous = finalize.finalization_states(
            previous_tag=False,
            body_ready=False,
            tag_exists=False,
            published=False,
            sample_run=None,
        )
        self.assertEqual(
            missing_previous,
            {
                "tag": "blocked",
                "docs": "blocked",
                "teaser": "blocked",
                "release": "blocked",
                "samples": "blocked",
                "nextAction": "select-previous-tag",
            },
        )

        needs_teaser = finalize.finalization_states(
            previous_tag=True,
            body_ready=False,
            tag_exists=False,
            published=False,
            sample_run=None,
        )
        self.assertEqual(needs_teaser["tag"], "blocked")
        self.assertEqual(needs_teaser["teaser"], "awaiting-user")
        self.assertEqual(
            needs_teaser["nextAction"],
            "write-release-teaser",
        )

        ready = finalize.finalization_states(
            previous_tag=True,
            body_ready=True,
            tag_exists=False,
            published=False,
            sample_run=None,
        )
        self.assertEqual(ready["tag"], "pending")
        self.assertEqual(ready["docs"], "pending")
        self.assertEqual(ready["release"], "pending")
        self.assertEqual(
            ready["nextAction"],
            "confirm-finalize-release",
        )

        samples_done = finalize.finalization_states(
            previous_tag=True,
            body_ready=False,
            tag_exists=True,
            published=True,
            sample_run={
                "status": "completed",
                "conclusion": "success",
            },
        )
        self.assertEqual(samples_done["samples"], "done")
        self.assertEqual(
            samples_done["nextAction"],
            "start-release-milestones",
        )

        published_without_previous = finalize.finalization_states(
            previous_tag=False,
            body_ready=False,
            tag_exists=True,
            published=True,
            sample_run={
                "status": "completed",
                "conclusion": "success",
            },
        )
        self.assertEqual(
            published_without_previous["nextAction"],
            "start-release-milestones",
        )

    def test_sample_failure_stays_with_publish_skill(self):
        failed = finalize.finalization_states(
            previous_tag=True,
            body_ready=False,
            tag_exists=True,
            published=True,
            sample_run={
                "status": "completed",
                "conclusion": "failure",
            },
        )
        self.assertEqual(failed["samples"], "failed")
        self.assertEqual(failed["nextAction"], "investigate-samples")

    def test_package_statuses_match_external_state(self):
        ready = push.package_states("ready", None)
        self.assertEqual(ready["publish"], "done")
        self.assertEqual(ready["verify"], "done")
        self.assertEqual(
            ready["nextAction"],
            "start-release-finalization",
        )

        active = push.package_states(
            "missing",
            {"status": "inProgress", "result": None},
        )
        self.assertEqual(active["publish"], "running")
        self.assertEqual(active["verify"], "running")
        self.assertEqual(
            active["nextAction"],
            "approve-or-wait-for-publish",
        )

        indexing = push.package_states(
            "partial",
            {"status": "completed", "result": "succeeded"},
        )
        self.assertEqual(indexing["publish"], "running")
        self.assertEqual(indexing["verify"], "running")
        self.assertEqual(indexing["nextAction"], "wait-for-nuget")

        pending = push.package_states("missing", None)
        self.assertEqual(pending["publish"], "pending")
        self.assertEqual(pending["verify"], "blocked")
        self.assertEqual(
            pending["nextAction"],
            "confirm-publish-packages",
        )

    def test_scripts_are_ascii_only(self):
        for path in SCRIPTS.glob("*.py"):
            path.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")

    def test_helpers_live_with_their_owning_command(self):
        shared = (SCRIPTS / "release_publish.py").read_text(encoding="ascii")
        push_source = (SCRIPTS / "push-release-packages.py").read_text(
            encoding="ascii"
        )
        finalize_source = (SCRIPTS / "finalize-release.py").read_text(
            encoding="ascii"
        )
        self.assertNotIn("class AzurePublish", shared)
        self.assertNotIn("class GitHub", shared)
        self.assertNotIn("class TagVersion", shared)
        self.assertIn("class AzurePublish", push_source)
        self.assertIn("class GitHub", finalize_source)
        self.assertIn("class TagVersion", finalize_source)

    def test_finalizer_completes_all_remote_work_in_one_call(self):
        events = []

        class FakeRepository:
            def __init__(self, root):
                self.root = root

            def remote_tags(self):
                return {}

            def push_tag(self, tag, sha):
                events.append(("tag", tag, sha))

        class FakeGitHub:
            def release(self, tag):
                return None

            def dispatch_docs(self, version):
                events.append(("docs", version))

            def create_release(self, **kwargs):
                events.append(("release", kwargs["tag"]))

        release = finalize.publish.ReleaseVersion.parse(
            "release/4.152.0"
        )
        context = finalize.FinalizeContext(
            root=Path.cwd(),
            release=release,
            source_sha="a" * 40,
            tag="v4.152.0",
            previous_tag="v4.151.0",
            generated_log="generated",
            expected_body="body",
            report={},
        )
        args = SimpleNamespace(
            previous_tag="v4.151.0",
            teaser_file=Path("teaser.md"),
        )
        with (
            mock.patch.object(
                finalize.publish,
                "GitRepository",
                FakeRepository,
            ),
            mock.patch.object(finalize, "GitHub", FakeGitHub),
            mock.patch.object(
                finalize,
                "write_artifacts",
                return_value={"body": Path("release-body.md")},
            ),
            mock.patch.object(finalize, "audit", return_value=context),
        ):
            finalize.execute(args, context)

        self.assertEqual(
            events,
            [
                ("tag", "v4.152.0", "a" * 40),
                ("docs", "4.152.0"),
                ("release", "v4.152.0"),
            ],
        )


if __name__ == "__main__":
    unittest.main()
