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
draft = load("create_release_draft", "create-release-draft.py")
release = load("publish_release_command", "publish-release.py")
github = load("release_github_command", "release_github.py")


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
        self.assertFalse(draft.create_parser().parse_args(COMMON).dry_run)
        self.assertFalse(release.create_parser().parse_args(COMMON).dry_run)

    def test_dry_run_is_explicit(self):
        for module in (push, draft, release):
            self.assertTrue(
                module.create_parser()
                .parse_args([*COMMON, "--dry-run"])
                .dry_run
            )

    def test_execution_commands_omit_dry_run(self):
        push_args = push.create_parser().parse_args(
            [*COMMON, "--dry-run"]
        )
        self.assertNotIn(
            "--dry-run",
            push.execution_command(push_args, "a" * 40),
        )

        draft_args = draft.create_parser().parse_args(
            [*COMMON, "--dry-run"]
        )
        draft_command = draft.execution_command(draft_args, "a" * 40)
        self.assertNotIn("--dry-run", draft_command)

        release_args = release.create_parser().parse_args(
            [*COMMON, "--dry-run"]
        )
        release_command = release.execution_command(
            release_args,
            "a" * 40,
        )
        self.assertNotIn("--dry-run", release_command)

    def test_package_statuses_match_external_state(self):
        self.assertEqual(
            push.package_states("ready", None)["nextAction"],
            "start-release-draft",
        )
        self.assertEqual(
            push.package_states(
                "missing",
                {"status": "inProgress", "result": None},
            )["nextAction"],
            "approve-publish-run",
        )
        self.assertEqual(
            push.package_states(
                "partial",
                {"status": "completed", "result": "succeeded"},
            )["nextAction"],
            "wait-for-nuget",
        )
        self.assertEqual(
            push.package_states("missing", None)["nextAction"],
            "confirm-publish-packages",
        )

    def test_wait_validates_exact_managed_resource(self):
        report = {
            "release": {
                "buildNumber": "4.152.0-preview.1.1+4.152.0-preview.1",
                "type": "preview",
            }
        }
        detail = {
            "resources": {
                "pipelines": {
                    "SkiaSharp": {
                        "pipeline": {"id": 10},
                        "version": (
                            "4.152.0-preview.1.1+4.152.0-preview.1"
                        ),
                    }
                }
            },
            "templateParameters": {
                "selectedResource": "SkiaSharp",
                "pushPackages": "true",
                "pushStable": "false",
            },
        }
        args = SimpleNamespace(expect_managed_run=10)
        push.validate_run_detail(
            detail,
            managed_run_id=args.expect_managed_run,
            managed_build_number=report["release"]["buildNumber"],
            stable=False,
        )
        detail["resources"]["pipelines"]["SkiaSharp"]["pipeline"]["id"] = 11
        with self.assertRaisesRegex(
            push.publish.PublishError,
            "different managed run",
        ):
            push.validate_run_detail(
                detail,
                managed_run_id=args.expect_managed_run,
                managed_build_number=report["release"]["buildNumber"],
                stable=False,
            )

    def test_queue_returns_approval_url_and_wait_command(self):
        args = SimpleNamespace(
            release_branch="release/4.152.0-preview.1",
            expect_source_sha="a" * 40,
            expect_managed_run=10,
            expect_tests_run=20,
            publish_run=None,
            wait_minutes=60,
        )
        state = {
            "dryRun": False,
            "release": {
                "sourceSha": "a" * 40,
                "buildNumber": (
                    "4.152.0-preview.1.1+4.152.0-preview.1"
                ),
            },
            "nuget": {"state": "missing"},
            "publishRun": None,
            "operations": [{}, {}],
        }

        class FakeAzure:
            def queue(self, build_number, *, stable):
                self.build_number = build_number
                self.stable = stable
                return {
                    "id": 14911788,
                    "name": "SkiaSharp preview",
                    "state": "inProgress",
                    "result": None,
                }

        with (
            mock.patch.object(push, "current_state", return_value=state),
            mock.patch.object(push, "AzurePublish", FakeAzure),
        ):
            result = push.execute(args)

        self.assertEqual(result["publishRun"]["runId"], 14911788)
        self.assertEqual(
            result["publishRun"]["url"],
            "https://dev.azure.com/devdiv/DevDiv/_build/results"
            "?buildId=14911788&view=results",
        )
        self.assertIn(
            "--publish-run 14911788",
            result["resumeCommand"],
        )

    def test_pending_run_recovers_with_wait_only(self):
        args = SimpleNamespace(
            release_branch="release/4.152.0-preview.1",
            expect_source_sha="a" * 40,
            expect_managed_run=10,
            expect_tests_run=20,
            dry_run=True,
            publish_run=None,
            wait_minutes=60,
        )
        release_version = push.publish.ReleaseVersion.parse(
            args.release_branch
        )
        handoff = {
            "managed": {
                "runId": 10,
                "buildNumber": (
                    "4.152.0-preview.1.1+4.152.0-preview.1"
                ),
            },
            "versions": {
                "test": {
                    "SkiaSharp": "4.152.0-preview.1.1",
                    "HarfBuzzSharp": "14.2.1-preview.1.1",
                },
                "public": {
                    "SkiaSharp": "4.152.0-preview.1.1",
                    "HarfBuzzSharp": "14.2.1-preview.1.1",
                },
            },
        }

        class FakeNuGet:
            def check(self, versions):
                return {"state": "missing", "packages": {}}

        class FakeAzure:
            def matching_runs(self, *unused, **kwargs):
                return [
                    {
                        "runId": 14911788,
                        "name": "SkiaSharp preview",
                        "status": "inProgress",
                        "result": None,
                        "url": push.run_url(14911788),
                    }
                ]

            def preview(self, *unused, **kwargs):
                return True

        with (
            mock.patch.object(
                push,
                "load_release",
                return_value=(
                    object(),
                    release_version,
                    {"warnings": []},
                    handoff,
                    "a" * 40,
                ),
            ),
            mock.patch.object(push.publish, "NuGet", FakeNuGet),
            mock.patch.object(push, "AzurePublish", FakeAzure),
        ):
            result = push.current_state(args)

        self.assertEqual(result["nextAction"], "approve-publish-run")
        self.assertIsNone(result["executionCommand"])
        self.assertIn("--publish-run 14911788", result["resumeCommand"])

    def test_successful_run_nuget_timeout_is_resumable(self):
        args = SimpleNamespace(
            publish_run=14912429,
            wait_minutes=60,
            poll_seconds=30,
        )
        state = {
            "dryRun": False,
            "nextAction": "wait-for-nuget",
            "publishRun": {
                "runId": 14912429,
                "status": "completed",
                "result": "succeeded",
                "url": push.run_url(14912429),
            },
            "nuget": {
                "state": "partial",
                "packages": {
                    "SkiaSharp": {
                        "version": "4.152.0-preview.1.1",
                        "available": False,
                    },
                    "HarfBuzzSharp": {
                        "version": "14.2.1-preview.1.1",
                        "available": True,
                    },
                },
            },
            "warnings": [],
            "resumeCommand": "resume",
        }
        with (
            mock.patch.object(push, "execute", return_value=state),
            mock.patch.object(push, "current_state", return_value=state),
            mock.patch.object(
                push.time,
                "monotonic",
                side_effect=[0, 60 * 60 + 1],
            ),
        ):
            result = push.execute_and_wait(args)

        self.assertEqual(result["nextAction"], "wait-for-nuget")
        self.assertTrue(result["wait"]["timedOut"])
        self.assertEqual(
            result["wait"]["missingPackages"],
            [
                {
                    "package": "SkiaSharp",
                    "version": "4.152.0-preview.1.1",
                }
            ],
        )
        self.assertIn("succeeded", result["warnings"][0])
        self.assertEqual(result["resumeCommand"], "resume")

    def test_create_script_pushes_tag_then_creates_draft(self):
        events = []

        class FakeRepository:
            def push_tag(self, tag, sha):
                events.append(("tag", tag, sha))

        class FakeGitHub:
            def create_draft(self, **kwargs):
                events.append(("draft", kwargs["tag"]))

        context = SimpleNamespace(
            root=Path.cwd(),
            repository=FakeRepository(),
            release=SimpleNamespace(
                title="Version 4.152.0",
                stable=True,
            ),
            source_sha="a" * 40,
            tag="v4.152.0",
            tags={},
            github=FakeGitHub(),
            github_release=None,
        )
        args = SimpleNamespace()
        with (
            mock.patch.object(
                draft.github_release,
                "write_generated_artifacts",
                return_value={"generated": Path("generated-release-body.md")},
            ),
            mock.patch.object(
                draft,
                "audit",
                return_value=(context, {}, "generated"),
            ),
        ):
            draft.execute(args, context, "generated")

        self.assertEqual(
            events,
            [
                ("tag", "v4.152.0", "a" * 40),
                ("draft", "v4.152.0"),
            ],
        )

    def test_github_release_is_created_as_draft(self):
        with (
            mock.patch.object(
                github.shutil,
                "which",
                return_value="/usr/bin/gh",
            ),
            mock.patch.object(github.publish, "run") as command,
        ):
            github.GitHub().create_draft(
                tag="v4.152.0",
                title="Version 4.152.0",
                source_sha="a" * 40,
                notes_file=Path("generated-release-body.md"),
                prerelease=False,
            )

        argv = command.call_args.args[0]
        self.assertIn("--draft", argv)
        self.assertIn("--verify-tag", argv)
        self.assertNotIn("--draft=false", argv)

    def test_publish_script_publishes_before_dispatching_docs(self):
        events = []

        class FakeGitHub:
            def dispatch_docs(self, version):
                events.append(("docs", version))

            def publish_draft(self, **kwargs):
                events.append(("publish", kwargs["tag"]))

        context = SimpleNamespace(
            root=Path.cwd(),
            release=SimpleNamespace(numeric="4.152.0", title="Version 4.152.0"),
            tag="v4.152.0",
            github=FakeGitHub(),
            github_release={"isDraft": True},
        )
        args = SimpleNamespace()
        completed = (
            context,
            {
                "operations": [
                    {
                        "id": "dispatch-release-notes",
                        "status": "pending",
                        "detail": "dispatch",
                    }
                ],
                "nextAction": "dispatch-release-notes",
                "executionCommand": "retry",
                "milestonesCommand": None,
            },
        )
        with mock.patch.object(
            release,
            "audit",
            return_value=completed,
        ):
            release.execute(args, context)

        self.assertEqual(
            events,
            [
                ("publish", "v4.152.0"),
                ("docs", "4.152.0"),
            ],
        )

    def test_publish_script_retries_docs_dispatch_after_publication(self):
        events = []

        class FakeGitHub:
            def dispatch_docs(self, version):
                events.append(("docs", version))

            def publish_draft(self, **kwargs):
                events.append(("publish", kwargs["tag"]))

        context = SimpleNamespace(
            root=Path.cwd(),
            release=SimpleNamespace(numeric="4.152.0", title="Version 4.152.0"),
            tag="v4.152.0",
            github=FakeGitHub(),
            github_release={"isDraft": False},
        )
        args = SimpleNamespace()
        completed = (
            context,
            {
                "operations": [
                    {
                        "id": "dispatch-release-notes",
                        "status": "pending",
                        "detail": "retry",
                    }
                ],
                "nextAction": "dispatch-release-notes",
                "executionCommand": "retry",
                "milestonesCommand": None,
            },
        )
        with mock.patch.object(
            release,
            "audit",
            return_value=completed,
        ):
            _, report = release.execute(args, context)

        self.assertEqual(events, [("docs", "4.152.0")])
        self.assertEqual(report["operations"][0]["status"], "done")
        self.assertEqual(report["nextAction"], "start-release-milestones")
        self.assertIsNone(report["executionCommand"])
        self.assertIsNotNone(report["milestonesCommand"])

    def test_publish_script_skips_docs_below_history_floor(self):
        events = []

        class FakeGitHub:
            def dispatch_docs(self, version):
                events.append(("docs", version))

            def publish_draft(self, **kwargs):
                events.append(("publish", kwargs["tag"]))

        context = SimpleNamespace(
            root=Path.cwd(),
            release=SimpleNamespace(
                numeric="3.119.5",
                title="Version 3.119.5",
            ),
            tag="v3.119.5",
            github=FakeGitHub(),
            github_release={"isDraft": True},
        )
        completed = (
            context,
            {
                "operations": [
                    {
                        "id": "dispatch-release-notes",
                        "status": "skipped",
                        "detail": "below floor",
                    }
                ],
                "nextAction": "start-release-milestones",
                "executionCommand": None,
                "milestonesCommand": "milestones",
            },
        )
        with (
            mock.patch.object(
                release.github_release,
                "docs_workflow_supports",
                return_value=False,
            ),
            mock.patch.object(
                release,
                "audit",
                return_value=completed,
            ),
        ):
            _, report = release.execute(SimpleNamespace(), context)

        self.assertEqual(events, [("publish", "v3.119.5")])
        self.assertEqual(report["operations"][0]["status"], "skipped")
        self.assertEqual(report["nextAction"], "start-release-milestones")

    def test_published_historical_release_audit_advances_to_milestones(self):
        context = SimpleNamespace(
            root=Path.cwd(),
            release=SimpleNamespace(
                branch="release/3.119.5",
                raw="3.119.5",
                numeric="3.119.5",
                release_type="stable",
                title="Version 3.119.5",
            ),
            source_sha="a" * 40,
            status={"warnings": []},
            handoff={
                "managed": {"runId": 10},
                "tests": {"runId": 20},
                "versions": {
                    "public": {
                        "SkiaSharp": "3.119.5",
                        "HarfBuzzSharp": "8.3.1.7",
                    }
                },
            },
            nuget={"state": "ready"},
            tag="v3.119.5",
            github_release={
                "isDraft": False,
                "body": github.mark_generated_notes("generated notes\n"),
                "url": "https://github.com/mono/SkiaSharp/releases/tag/v3.119.5",
            },
        )
        args = SimpleNamespace(dry_run=True)
        with (
            mock.patch.object(
                release.github_release,
                "load_release",
                return_value=context,
            ),
            mock.patch.object(
                release.github_release,
                "docs_workflow_supports",
                return_value=False,
            ),
        ):
            _, report = release.audit(args)

        self.assertEqual(report["nextAction"], "start-release-milestones")
        self.assertIsNone(report["executionCommand"])
        self.assertIsNotNone(report["milestonesCommand"])
        self.assertEqual(report["operations"][0]["status"], "skipped")
        self.assertIn("history_floor.skiasharp", report["warnings"][0])

    def test_helpers_live_with_github_release_domain(self):
        shared = (SCRIPTS / "release_publish.py").read_text(encoding="ascii")
        github_source = (SCRIPTS / "release_github.py").read_text(
            encoding="ascii"
        )
        self.assertNotIn("class GitHub", shared)
        self.assertNotIn("class TagVersion", shared)
        self.assertIn("class GitHub", github_source)
        self.assertIn("class TagVersion", github_source)

    def test_scripts_are_ascii_only(self):
        for path in SCRIPTS.glob("*.py"):
            path.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
