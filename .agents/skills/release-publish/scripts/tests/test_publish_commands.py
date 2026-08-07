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
            "approve-or-wait-for-publish",
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

    def test_scripts_are_ascii_only(self):
        for path in SCRIPTS.glob("*.py"):
            path.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
