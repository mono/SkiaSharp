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
    "--expect-build-run",
    "10",
    "--expect-tests-run",
    "20",
    "--expect-bar-build",
    "30",
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
        push_command = push.execution_command(push_args, "a" * 40)
        self.assertNotIn("--dry-run", push_command)
        self.assertIn("--expect-bar-build 30", push_command)

        draft_args = draft.create_parser().parse_args(
            [*COMMON, "--dry-run"]
        )
        draft_command = draft.execution_command(draft_args, "a" * 40)
        self.assertNotIn("--dry-run", draft_command)
        self.assertIn("--expect-bar-build 30", draft_command)

        release_args = release.create_parser().parse_args(
            [
                *COMMON,
                "--teaser-file",
                "teaser.md",
                "--dry-run",
            ]
        )
        release_command = release.execution_command(
            release_args,
            "a" * 40,
        )
        self.assertNotIn("--dry-run", release_command)
        self.assertIn("--teaser-file teaser.md", release_command)
        self.assertIn("--expect-bar-build 30", release_command)

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

    def test_run_url_targets_dnceng_internal(self):
        self.assertEqual(
            push.run_url(14911788),
            "https://dev.azure.com/dnceng/internal/_build/results"
            "?buildId=14911788&view=results",
        )

    def test_publish_pipeline_id_fails_closed_when_unset(self):
        with mock.patch.dict(push.os.environ, {}, clear=True):
            with self.assertRaisesRegex(
                push.publish.PublishError,
                push.PUBLISH_PIPELINE_ID_ENV,
            ):
                push.publish_pipeline_id()

    def test_publish_pipeline_id_fails_closed_when_not_integer(self):
        with mock.patch.dict(
            push.os.environ,
            {push.PUBLISH_PIPELINE_ID_ENV: "not-an-integer"},
            clear=True,
        ):
            with self.assertRaisesRegex(
                push.publish.PublishError,
                "integer",
            ):
                push.publish_pipeline_id()

    def test_publish_pipeline_id_reads_configured_env_var(self):
        with mock.patch.dict(
            push.os.environ,
            {push.PUBLISH_PIPELINE_ID_ENV: "4242"},
            clear=True,
        ):
            self.assertEqual(push.publish_pipeline_id(), 4242)

    def test_wait_validates_exact_build_resource(self):
        build_number = "4.152.0-preview.1.1+4.152.0-preview.1"
        detail = {
            "resources": {
                "pipelines": {
                    push.RESOURCE_ALIAS: {
                        "pipeline": {
                            "id": 10,
                            "name": push.RESOURCE_NAME,
                            "folder": push.RESOURCE_FOLDER,
                        },
                        "version": build_number,
                    }
                }
            },
            "templateParameters": {
                "selectedResource": push.RESOURCE_ALIAS,
                "buildRunId": 10,
                "barBuildId": 30,
                "pushPackages": "true",
                "pushStable": "false",
            },
        }
        push.validate_run_detail(
            detail,
            bar_build_id=30,
            build_run_id=10,
            build_number=build_number,
            stable=False,
        )

        full_path = {
            **detail,
            "resources": {
                "pipelines": {
                    push.RESOURCE_ALIAS: {
                        "pipeline": {
                            "id": 10,
                            "name": push.RESOURCE_PATH,
                            "folder": push.RESOURCE_FOLDER,
                        },
                        "version": build_number,
                    }
                }
            },
        }
        push.validate_run_detail(
            full_path,
            bar_build_id=30,
            build_run_id=10,
            build_number=build_number,
            stable=False,
        )

        with self.assertRaisesRegex(
            push.publish.PublishError,
            "has not resolved",
        ):
            push.validate_run_detail(
                {"state": "completed"},
                bar_build_id=30,
                build_run_id=10,
                build_number=build_number,
                stable=False,
            )

        # A different Build run must be rejected.
        mismatched = {
            **detail,
            "resources": {
                "pipelines": {
                    push.RESOURCE_ALIAS: {
                        "pipeline": {
                            "id": 11,
                            "name": push.RESOURCE_NAME,
                            "folder": push.RESOURCE_FOLDER,
                        },
                        "version": build_number,
                    }
                }
            },
        }
        with self.assertRaisesRegex(
            push.publish.PublishError,
            "different Build run",
        ):
            push.validate_run_detail(
                mismatched,
                bar_build_id=30,
                build_run_id=10,
                build_number=build_number,
                stable=False,
            )

        # A resource that is not the folder-qualified skiasharp-package
        # pipeline path must be rejected.
        wrong_path = {
            **detail,
            "resources": {
                "pipelines": {
                    push.RESOURCE_ALIAS: {
                        "pipeline": {
                            "id": 10,
                            "name": "SkiaSharp",
                            "folder": push.RESOURCE_FOLDER,
                        },
                        "version": build_number,
                    }
                }
            },
        }
        with self.assertRaisesRegex(
            push.publish.PublishError,
            r"expected.*skiasharp-package",
        ):
            push.validate_run_detail(
                wrong_path,
                bar_build_id=30,
                build_run_id=10,
                build_number=build_number,
                stable=False,
            )

        # A different BAR build must be rejected.
        wrong_bar = {
            **detail,
            "templateParameters": {
                "selectedResource": push.RESOURCE_ALIAS,
                "buildRunId": 10,
                "barBuildId": 99,
                "pushPackages": "true",
                "pushStable": "false",
            },
        }
        with self.assertRaisesRegex(
            push.publish.PublishError,
            "different BAR build",
        ):
            push.validate_run_detail(
                wrong_bar,
                bar_build_id=30,
                build_run_id=10,
                build_number=build_number,
                stable=False,
            )

    def test_queue_returns_approval_url_and_wait_command(self):
        args = SimpleNamespace(
            release_branch="release/4.152.0-preview.1",
            expect_source_sha="a" * 40,
            expect_build_run=10,
            expect_tests_run=20,
            expect_bar_build=30,
            publish_run=None,
            wait_minutes=60,
        )
        state = {
            "dryRun": False,
            "release": {
                "sourceSha": "a" * 40,
                "buildRunId": 10,
                "barBuildId": 30,
                "buildNumber": (
                    "4.152.0-preview.1.1+4.152.0-preview.1"
                ),
            },
            "nuget": {"state": "missing"},
            "publishRun": None,
            "operations": [{}, {}],
        }

        class FakeAzure:
            def queue(self, bar_build_id, build_run_id, build_number, *, stable):
                self.bar_build_id = bar_build_id
                self.build_run_id = build_run_id
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
            "https://dev.azure.com/dnceng/internal/_build/results"
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
            expect_build_run=10,
            expect_tests_run=20,
            expect_bar_build=30,
            dry_run=True,
            publish_run=None,
            wait_minutes=60,
        )
        release_version = push.publish.ReleaseVersion.parse(
            args.release_branch
        )
        handoff = {
            "build": {
                "runId": 10,
                "buildNumber": (
                    "4.152.0-preview.1.1+4.152.0-preview.1"
                ),
            },
            "bar": {
                "assets": {
                    "SkiaSharp": {
                        "version": "4.152.0-preview.1.1",
                        "locations": ["source"],
                    },
                    "HarfBuzzSharp": {
                        "version": "14.2.1-preview.1.1",
                        "locations": ["source"],
                    },
                },
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

    def test_ambiguous_matches_require_explicit_publish_run(self):
        args = SimpleNamespace(
            release_branch="release/4.152.0-preview.1",
            expect_source_sha="a" * 40,
            expect_build_run=10,
            expect_tests_run=20,
            expect_bar_build=30,
            dry_run=True,
            publish_run=None,
            wait_minutes=60,
        )
        release_version = push.publish.ReleaseVersion.parse(
            args.release_branch
        )
        handoff = {
            "build": {
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
                        "runId": 2,
                        "name": "SkiaSharp preview",
                        "status": "completed",
                        "result": "succeeded",
                        "url": push.run_url(2),
                    },
                    {
                        "runId": 1,
                        "name": "SkiaSharp preview",
                        "status": "completed",
                        "result": "succeeded",
                        "url": push.run_url(1),
                    },
                ]

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
            with self.assertRaisesRegex(
                push.publish.PublishError,
                r"multiple publication runs.*--publish-run",
            ):
                push.current_state(args)

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
                return_value={"generated": Path("generated-log.md")},
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
                notes_file=Path("generated-log.md"),
                prerelease=False,
            )

        argv = command.call_args.args[0]
        self.assertIn("--draft", argv)
        self.assertIn("--verify-tag", argv)
        self.assertNotIn("--draft=false", argv)

    def test_publish_script_updates_draft_after_teaser(self):
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
        args = SimpleNamespace(teaser_file=Path("teaser.md"))
        completed = (
            context,
            {"nextAction": "start-release-milestones"},
            "body",
        )
        with (
            mock.patch.object(
                release.github_release,
                "write_release_body",
                return_value={"body": Path("release-body.md")},
            ),
            mock.patch.object(release, "audit", return_value=completed),
        ):
            release.execute(args, context, "body")

        self.assertEqual(
            events,
            [
                ("docs", "4.152.0"),
                ("publish", "v4.152.0"),
            ],
        )

    def test_helpers_live_with_github_release_domain(self):
        shared = (SCRIPTS / "release_publish.py").read_text(encoding="ascii")
        github_source = (SCRIPTS / "release_github.py").read_text(
            encoding="ascii"
        )
        self.assertNotIn("class GitHub", shared)
        self.assertNotIn("class TagVersion", shared)
        self.assertIn("class GitHub", github_source)
        self.assertIn("class TagVersion", github_source)

    def test_no_legacy_devdiv_or_managed_references_remain(self):
        for path in SCRIPTS.glob("*.py"):
            source = path.read_text(encoding="ascii").lower()
            self.assertNotIn("devdiv", source, path)
            self.assertNotIn("25298", source, path)
            self.assertNotIn("managed", source, path)

    def test_scripts_are_ascii_only(self):
        for path in SCRIPTS.glob("*.py"):
            path.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
