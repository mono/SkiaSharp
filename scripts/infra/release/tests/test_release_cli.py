from __future__ import annotations

import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import release as cli
import release_common as common


class ParserWiringTests(unittest.TestCase):
    def test_prepare_plan_parses(self):
        parser = cli.create_parser()
        args = parser.parse_args(
            ["prepare", "plan", "--integration-target", "main", "--output", "out.json"]
        )
        self.assertIs(args.func, cli.cmd_prepare_plan)
        self.assertEqual(args.integration_target, "main")

    def test_prepare_apply_parses(self):
        parser = cli.create_parser()
        args = parser.parse_args(["prepare", "apply", "--plan", "plan.json"])
        self.assertIs(args.func, cli.cmd_prepare_apply)
        self.assertIsNone(args.output)

    def test_finish_plan_parses(self):
        parser = cli.create_parser()
        args = parser.parse_args(["finish", "plan", "--version", "3.119.0"])
        self.assertIs(args.func, cli.cmd_finish_plan)

    def test_finish_create_draft_parses(self):
        parser = cli.create_parser()
        args = parser.parse_args(["finish", "create-draft", "--plan", "finish-plan.json"])
        self.assertIs(args.func, cli.cmd_finish_create_draft)
        self.assertIsNone(args.output)

    def test_finish_create_draft_accepts_optional_output(self):
        parser = cli.create_parser()
        args = parser.parse_args(
            ["finish", "create-draft", "--plan", "finish-plan.json", "--output", "report.json"]
        )
        self.assertEqual(args.output, "report.json")

    def test_finish_plan_publication_parses(self):
        parser = cli.create_parser()
        args = parser.parse_args(["finish", "plan-publication", "--plan", "finish-plan.json"])
        self.assertIs(args.func, cli.cmd_finish_plan_publication)

    def test_finish_publish_parses(self):
        parser = cli.create_parser()
        args = parser.parse_args(
            ["finish", "publish", "--plan", "finish-plan.json", "--publication", "publication.json"]
        )
        self.assertIs(args.func, cli.cmd_finish_publish)
        self.assertEqual(args.publication, "publication.json")

    def test_finish_publish_requires_publication(self):
        parser = cli.create_parser()
        with self.assertRaises(SystemExit):
            parser.parse_args(["finish", "publish", "--plan", "finish-plan.json"])

    def test_finish_closeout_parses(self):
        parser = cli.create_parser()
        args = parser.parse_args(["finish", "closeout", "--plan", "finish-plan.json", "--dry-run"])
        self.assertIs(args.func, cli.cmd_finish_closeout)
        self.assertEqual(args.plan, "finish-plan.json")
        self.assertTrue(args.dry_run)
        self.assertIsNone(args.output)

    def test_inspect_parses(self):
        parser = cli.create_parser()
        args = parser.parse_args(["inspect", "--release-branch", "release/3.119.0"])
        self.assertIs(args.func, cli.cmd_inspect)
        self.assertIsNone(args.output)

    def test_inspect_accepts_optional_output(self):
        parser = cli.create_parser()
        args = parser.parse_args(
            ["inspect", "--release-branch", "release/3.119.0", "--output", "inspect.json"]
        )
        self.assertEqual(args.output, "inspect.json")

    def test_render_plan_parses(self):
        parser = cli.create_parser()
        args = parser.parse_args(["render-plan", "--plan", "prepare-plan.json", "--output", "summary.json"])
        self.assertIs(args.func, cli.cmd_render_plan)
        self.assertEqual(args.plan, "prepare-plan.json")
        self.assertEqual(args.output, "summary.json")

    def test_render_plan_output_is_optional(self):
        parser = cli.create_parser()
        args = parser.parse_args(["render-plan", "--plan", "prepare-plan.json"])
        self.assertIsNone(args.output)

    def test_check_environment_parses(self):
        parser = cli.create_parser()
        args = parser.parse_args(
            ["check-environment", "--name", "release-tag", "--default-branch", "main"]
        )
        self.assertIs(args.func, cli.cmd_check_environment)
        self.assertEqual(args.name, "release-tag")
        self.assertEqual(args.default_branch, "main")
        self.assertIsNone(args.output)

    def test_check_environment_accepts_optional_output(self):
        parser = cli.create_parser()
        args = parser.parse_args(
            [
                "check-environment", "--name", "release-publish", "--default-branch", "main",
                "--output", "check.json",
            ]
        )
        self.assertEqual(args.output, "check.json")

    def test_check_environment_requires_name_and_default_branch(self):
        parser = cli.create_parser()
        with self.assertRaises(SystemExit):
            parser.parse_args(["check-environment", "--default-branch", "main"])
        with self.assertRaises(SystemExit):
            parser.parse_args(["check-environment", "--name", "release-tag"])

    def test_missing_subcommand_errors(self):
        parser = cli.create_parser()
        with self.assertRaises(SystemExit):
            parser.parse_args(["prepare"])

    def test_main_reports_release_tool_errors_without_traceback(self):
        from unittest import mock

        def _raise(_args):
            raise cli.common.PlanError("boom")

        with mock.patch.object(cli, "cmd_inspect", side_effect=_raise):
            exit_code = cli.main(["inspect", "--release-branch", "release/3.119.0"])
        self.assertEqual(exit_code, 1)


class RenderPlanExecutionTests(unittest.TestCase):
    """End-to-end: writes a real digest-stamped prepare plan file, then runs
    render-plan through the CLI's own argument parsing and I/O, exactly as a
    thin workflow step would."""

    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self):
        self._tmp.cleanup()

    def _write_prepare_plan(self) -> Path:
        import release_prepare as prepare

        plan = {
            "schemaVersion": 1,
            "operation": "prepare",
            "generatedAt": "2024-01-01T00:00:00Z",
            "toolingSha": "a" * 40,
            "nextAction": "apply",
            "input": {"integrationTarget": "main", "requestedVersion": None},
            "release": {
                "identity": "3.119.0-preview.1",
                "version": "3.119.0-preview.1",
                "numeric": "3.119.0",
                "label": "preview.1",
                "releaseType": "preview",
                "branch": "release/3.119.0-preview.1",
                "integrationBranch": "release/3.119.x",
                "isHotfix": False,
                "stable": False,
            },
            "base": {"ref": "refs/remotes/origin/main", "sha": "b" * 40},
            "maintenanceBranch": {
                "name": "release/3.119.x", "exists": False, "action": "create", "baseSha": "b" * 40
            },
            "skia": {"sha": "c" * 40, "releaseBranch": "release/3.119.0-preview.1", "remoteState": "missing"},
            "skiaSharpRemoteState": "missing",
            "versions": {"skiaSharp": "3.119.0", "requiresPackageBump": False},
            "operations": [],
            "stableBump": None,
            "warnings": [],
        }
        plan_path = self.root / "prepare-plan.json"
        common.write_plan(plan_path, plan, schema_name=prepare.PREPARE_SCHEMA)
        return plan_path

    def test_render_plan_writes_summary_file(self):
        plan_path = self._write_prepare_plan()
        output_path = self.root / "summary.json"
        exit_code = cli.main(
            ["render-plan", "--plan", str(plan_path), "--output", str(output_path)]
        )
        self.assertEqual(exit_code, 0)
        self.assertTrue(output_path.is_file())
        rendered = common.json.loads(output_path.read_text(encoding="utf-8"))
        self.assertEqual(rendered["releaseBranch"], "release/3.119.0-preview.1")
        self.assertEqual(rendered["releaseIdentity"], "3.119.0-preview.1")
        self.assertEqual(rendered["releaseVersion"], "3.119.0-preview.1")
        self.assertEqual(rendered["toolingSha"], "a" * 40)
        self.assertEqual(rendered["nextAction"], "apply")
        self.assertEqual(rendered["operation"], "prepare")
        self.assertRegex(rendered["planDigest"], r"^[0-9a-f]{64}$")

    def test_render_plan_without_output_still_succeeds(self):
        plan_path = self._write_prepare_plan()
        exit_code = cli.main(["render-plan", "--plan", str(plan_path)])
        self.assertEqual(exit_code, 0)

    def test_render_plan_rejects_tampered_plan(self):
        plan_path = self._write_prepare_plan()
        text = plan_path.read_text(encoding="utf-8").replace("3.119.0-preview.1", "9.9.9-preview.9")
        plan_path.write_text(text, encoding="utf-8")
        exit_code = cli.main(["render-plan", "--plan", str(plan_path)])
        self.assertEqual(exit_code, 1)

    def test_render_plan_rejects_unknown_operation(self):
        plan_path = self.root / "weird-plan.json"
        plan_path.write_text('{"operation": "something-else"}', encoding="utf-8")
        exit_code = cli.main(["render-plan", "--plan", str(plan_path)])
        self.assertEqual(exit_code, 1)

    def test_render_plan_also_supports_a_command_result_file(self):
        # A "result" file (e.g. from `finish create-draft --output`) has no
        # "operation" field but carries the same standardized envelope.
        plan_path = self._write_prepare_plan()
        plan = common.read_plan(plan_path, schema_name=__import__("release_prepare").PREPARE_SCHEMA)
        result = common.build_envelope(plan, next_action="done", tag="v3.119.0-preview.1")
        result_path = self.root / "apply-result.json"
        common.write_json_file(result_path, result)

        output_path = self.root / "result-summary.json"
        exit_code = cli.main(
            ["render-plan", "--plan", str(result_path), "--output", str(output_path)]
        )
        self.assertEqual(exit_code, 0)
        rendered = common.json.loads(output_path.read_text(encoding="utf-8"))
        self.assertNotIn("operation", rendered)
        self.assertEqual(rendered["nextAction"], "done")
        self.assertEqual(rendered["releaseBranch"], "release/3.119.0-preview.1")
        self.assertEqual(rendered["planDigest"], plan["planDigest"])


class FinishPublishExecutionTests(unittest.TestCase):
    """End-to-end: ``finish publish`` must read ``--publication`` from disk
    (never recompute ``plan_publication`` itself -- item 5) and validate it
    is bound to the given ``--plan`` before publishing, with
    ``GhCliGitHubClient`` swapped for a fake so no real ``gh`` call
    happens."""

    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self):
        self._tmp.cleanup()

    @staticmethod
    def _finish_plan_dict(*, title="Version 3.119.0 (Preview 1)"):
        return {
            "schemaVersion": 1,
            "operation": "finish",
            "generatedAt": "2024-01-01T00:00:00Z",
            "toolingSha": "b" * 40,
            "nextAction": "plan-publication",
            "input": {"requestedVersion": "3.119.0-preview.1.42"},
            "receipt": {
                "skiaSharpVersion": "3.119.0-preview.1.42",
                "base": "3.119.0",
                "label": "preview.1",
                "buildRevision": "42",
                "sourceCommit": "a" * 40,
                "sourceBranch": "release/3.119.0-preview.1",
                "harfBuzzSharpVersion": "1.8.8.1-preview.1.42",
                "packages": [],
            },
            "release": {
                "identity": "3.119.0-preview.1", "version": "3.119.0-preview.1.42",
                "branch": "release/3.119.0-preview.1", "raw": "3.119.0-preview.1",
                "numeric": "3.119.0", "label": "preview.1", "releaseType": "preview",
                "stable": False, "title": title, "tag": "v3.119.0-preview.1",
            },
            "tag": {
                "name": "v3.119.0-preview.1", "targetCommit": "a" * 40,
                "existingSha": None, "status": "pending",
            },
            "previousTag": "v3.118.0",
            "draft": {"exists": False, "isPublished": False, "status": "pending"},
            "warnings": [],
        }

    def _write_finish_plan(self, *, title="Version 3.119.0 (Preview 1)"):
        import release_finish as finish

        return common.write_plan(
            self.root / "finish-plan.json", self._finish_plan_dict(title=title),
            schema_name=finish.FINISH_SCHEMA,
        )

    class _FakeGitHubClient:
        def __init__(self, *, body):
            import release_github as release_gh

            self._gh = release_gh
            self.release_info = release_gh.ReleaseInfo(
                tag_name="v3.119.0-preview.1", name="Version 3.119.0 (Preview 1)", is_draft=True,
                is_prerelease=True, target_commitish="a" * 40, body=body, url="https://example.invalid",
            )
            self.published = False

        def get_release(self, tag):
            return self.release_info

        def publish_release(self, *, tag, title, body):
            self.published = True
            self.release_info = self._gh.ReleaseInfo(
                tag_name=tag, name=title, is_draft=False, is_prerelease=self.release_info.is_prerelease,
                target_commitish=self.release_info.target_commitish, body=body, url=self.release_info.url,
            )

    def test_publish_reads_publication_from_disk_and_succeeds(self):
        from unittest import mock
        import release_github as release_gh

        plan = self._write_finish_plan()
        body = release_gh.build_initial_body("notes")
        fake_github = self._FakeGitHubClient(body=body)

        publication = common.build_envelope(
            plan, next_action="publish", tag="v3.119.0-preview.1",
            draftUrl="https://example.invalid", isDraft=True, isPublished=False,
            bodySha256=release_gh.body_sha256(body), hasManagedMarkers=True, readyToPublish=True,
        )
        publication_path = self.root / "publication.json"
        common.write_json_file(publication_path, publication)

        output_path = self.root / "publish-result.json"
        with mock.patch.object(cli.gh, "GhCliGitHubClient", return_value=fake_github):
            exit_code = cli.main(
                [
                    "finish", "publish",
                    "--plan", str(self.root / "finish-plan.json"),
                    "--publication", str(publication_path),
                    "--output", str(output_path),
                ]
            )

        self.assertEqual(exit_code, 0)
        self.assertTrue(fake_github.published)
        report = common.json.loads(output_path.read_text(encoding="utf-8"))
        self.assertEqual(report["status"], "published")

    def test_publish_rejects_publication_from_a_different_plan(self):
        from unittest import mock
        import release_github as release_gh

        self._write_finish_plan()
        body = release_gh.build_initial_body("notes")
        fake_github = self._FakeGitHubClient(body=body)

        # A publication built from a plan with the same tag but different
        # content (title) -- a different planDigest -- must be rejected
        # even though the tag/body-hash would otherwise line up.
        other_plan = common.with_digest(self._finish_plan_dict(title="Version 9.9.9 (Different)"))
        publication = common.build_envelope(
            other_plan, next_action="publish", tag="v3.119.0-preview.1",
            draftUrl="https://example.invalid", isDraft=True, isPublished=False,
            bodySha256=release_gh.body_sha256(body), hasManagedMarkers=True, readyToPublish=True,
        )
        publication_path = self.root / "publication.json"
        common.write_json_file(publication_path, publication)

        with mock.patch.object(cli.gh, "GhCliGitHubClient", return_value=fake_github):
            exit_code = cli.main(
                [
                    "finish", "publish",
                    "--plan", str(self.root / "finish-plan.json"),
                    "--publication", str(publication_path),
                ]
            )

        self.assertEqual(exit_code, 1)
        self.assertFalse(fake_github.published)

    def test_publish_rejects_a_malformed_publication_file(self):
        self._write_finish_plan()
        publication_path = self.root / "publication.json"
        # Missing every required result-envelope field.
        common.write_json_file(publication_path, {"onlyField": "x"})

        exit_code = cli.main(
            [
                "finish", "publish",
                "--plan", str(self.root / "finish-plan.json"),
                "--publication", str(publication_path),
            ]
        )
        self.assertEqual(exit_code, 1)


class CheckEnvironmentExecutionTests(unittest.TestCase):
    """End-to-end: runs ``check-environment`` through the CLI's own argument
    parsing and I/O, with ``GhCliEnvironmentClient`` swapped for a fake so no
    real ``gh`` call happens -- exactly the injection point a workflow test
    would use, and proof this is a real read-only gate, not a stub."""

    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self):
        self._tmp.cleanup()

    def _patch_client(self, snapshot):
        from unittest import mock

        class _FakeClient:
            def __init__(self, *args, **kwargs):
                pass

            def get_environment(self, name):
                return snapshot

        return mock.patch.object(cli.environment, "GhCliEnvironmentClient", _FakeClient)

    def test_well_configured_environment_exits_zero_and_writes_report(self):
        import release_environment as environment

        snapshot = environment.EnvironmentSnapshot(
            name="release-tag",
            protection_rule_types=("required_reviewers", "branch_policy"),
            required_reviewers=environment.RequiredReviewersRule(reviewer_count=2, prevent_self_review=True),
            protected_branches=False,
            custom_branch_policies=True,
            branch_policies=(environment.BranchPolicy(name="main", kind="branch"),),
        )
        output_path = self.root / "check.json"
        with self._patch_client(snapshot):
            exit_code = cli.main(
                [
                    "check-environment", "--name", "release-tag", "--default-branch", "main",
                    "--output", str(output_path),
                ]
            )
        self.assertEqual(exit_code, 0)
        report = common.json.loads(output_path.read_text(encoding="utf-8"))
        self.assertTrue(report["ok"])
        self.assertEqual(report["reasons"], [])
        self.assertEqual(report["allowedBranches"], ["main"])
        self.assertEqual(report["reviewerCount"], 2)

    def test_missing_environment_exits_nonzero_and_still_writes_report(self):
        output_path = self.root / "check.json"
        with self._patch_client(None):
            exit_code = cli.main(
                [
                    "check-environment", "--name", "release-publish", "--default-branch", "main",
                    "--output", str(output_path),
                ]
            )
        self.assertEqual(exit_code, 1)
        report = common.json.loads(output_path.read_text(encoding="utf-8"))
        self.assertFalse(report["exists"])
        self.assertFalse(report["ok"])
        self.assertTrue(report["reasons"])

    def test_misconfigured_environment_exits_nonzero_without_output(self):
        import release_environment as environment

        snapshot = environment.EnvironmentSnapshot(
            name="release-branching",
            protection_rule_types=("branch_policy",),
            required_reviewers=None,
            protected_branches=False,
            custom_branch_policies=True,
            branch_policies=(environment.BranchPolicy(name="main", kind="branch"),),
        )
        with self._patch_client(snapshot):
            exit_code = cli.main(
                ["check-environment", "--name", "release-branching", "--default-branch", "main"]
            )
        self.assertEqual(exit_code, 1)


class EmitHelperTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self):
        self._tmp.cleanup()

    def test_emit_without_output_only_prints(self):
        common.emit({"a": 1})  # must not raise

    def test_emit_with_output_writes_file(self):
        output_path = self.root / "nested" / "report.json"
        common.emit({"a": 1}, output=output_path)
        self.assertEqual(common.json.loads(output_path.read_text(encoding="utf-8")), {"a": 1})


class RecordingRunner:
    """Captures every argv list passed to it and returns one canned stdout
    payload, so tests can assert the *exact* command shape (in particular,
    whether ``-X GET`` is present) rather than just behavior."""

    def __init__(self, stdout: str = "[]"):
        self.calls: list[list[str]] = []
        self.stdout = stdout

    def run(self, args, *, cwd, check=True, timeout=120, input=None):
        args_list = list(args)
        self.calls.append(args_list)
        return common.CommandResult(args=tuple(args_list), returncode=0, stdout=self.stdout, stderr="")


def _assert_explicit_method(test: unittest.TestCase, argv: list[str], method: str) -> None:
    test.assertIn("-X", argv, f"{argv} has no explicit -X flag")
    index = argv.index("-X")
    test.assertEqual(argv[index + 1], method, f"{argv} did not use -X {method}")


class GhCliMilestoneClientArgvTests(unittest.TestCase):
    """Any ``gh api`` read that includes ``-f``/``-F`` must explicitly pass
    ``-X GET``: gh silently switches its default HTTP method to POST as
    soon as a form-field flag is present, which would make a read (list
    milestones, list issues, read a PR/issue) fail against a read-only
    listing/resource endpoint."""

    def test_milestones_passes_explicit_get(self):
        runner = RecordingRunner(stdout="[]")
        client = cli.GhCliMilestoneClient(repository="mono/SkiaSharp")
        client.runner = runner
        client.milestones()
        self.assertEqual(len(runner.calls), 1)
        argv = runner.calls[0]
        self.assertEqual(argv[:2], ["gh", "api"])
        self.assertIn("repos/mono/SkiaSharp/milestones", argv)
        _assert_explicit_method(self, argv, "GET")
        self.assertIn("--paginate", argv)
        self.assertIn("--slurp", argv)
        self.assertIn("state=all", argv)
        self.assertIn("per_page=100", argv)

    def test_open_milestone_items_passes_explicit_get(self):
        runner = RecordingRunner(stdout="[]")
        client = cli.GhCliMilestoneClient(repository="mono/SkiaSharp")
        client.runner = runner
        client.open_milestone_items(7)
        argv = runner.calls[0]
        self.assertIn("repos/mono/SkiaSharp/issues", argv)
        _assert_explicit_method(self, argv, "GET")
        self.assertIn("milestone=7", argv)
        self.assertIn("state=open", argv)

    def test_pull_request_milestone_passes_explicit_get(self):
        runner = RecordingRunner(stdout='{"milestone": {"title": "3.119.0"}}')
        client = cli.GhCliMilestoneClient(repository="mono/SkiaSharp")
        client.runner = runner
        title = client.pull_request_milestone(123)
        self.assertEqual(title, "3.119.0")
        argv = runner.calls[0]
        self.assertIn("repos/mono/SkiaSharp/pulls/123", argv)
        _assert_explicit_method(self, argv, "GET")

    def test_issue_milestone_passes_explicit_get(self):
        runner = RecordingRunner(stdout='{"milestone": null}')
        client = cli.GhCliMilestoneClient(repository="mono/SkiaSharp")
        client.runner = runner
        title = client.issue_milestone(456)
        self.assertIsNone(title)
        argv = runner.calls[0]
        self.assertIn("repos/mono/SkiaSharp/issues/456", argv)
        _assert_explicit_method(self, argv, "GET")

    def test_create_milestone_uses_explicit_post(self):
        runner = RecordingRunner(stdout='{"number": 5, "title": "3.120.0", "state": "open"}')
        client = cli.GhCliMilestoneClient(repository="mono/SkiaSharp")
        client.runner = runner
        client.create_milestone("3.120.0", due_on=None, description=None)
        argv = runner.calls[0]
        _assert_explicit_method(self, argv, "POST")

    def test_update_item_milestone_uses_explicit_patch(self):
        runner = RecordingRunner(stdout="{}")
        client = cli.GhCliMilestoneClient(repository="mono/SkiaSharp")
        client.runner = runner
        client.update_item_milestone(1, 2)
        argv = runner.calls[0]
        self.assertIn("repos/mono/SkiaSharp/issues/1", argv)
        _assert_explicit_method(self, argv, "PATCH")
        self.assertIn("milestone=2", argv)

    def test_close_milestone_uses_explicit_patch(self):
        runner = RecordingRunner(stdout="{}")
        client = cli.GhCliMilestoneClient(repository="mono/SkiaSharp")
        client.runner = runner
        client.close_milestone(3)
        argv = runner.calls[0]
        self.assertIn("repos/mono/SkiaSharp/milestones/3", argv)
        _assert_explicit_method(self, argv, "PATCH")
        self.assertIn("state=closed", argv)

    def test_closing_issues_uses_graphql(self):
        runner = RecordingRunner(
            stdout='{"data": {"repository": {"pullRequest": {"closingIssuesReferences": {"nodes": [{"number": 9}]}}}}}'
        )
        client = cli.GhCliMilestoneClient(repository="mono/SkiaSharp")
        client.runner = runner
        numbers = client.closing_issues(42)
        self.assertEqual(numbers, [9])
        argv = runner.calls[0]
        self.assertIn("graphql", argv)


class DispatchWorkflowArgvTests(unittest.TestCase):
    """release_finish.apply_closeout dispatches update-release-notes.lock.yml
    and (for a stable release) auto-update-issue-template-versions.yml
    through GitHubClient.dispatch_workflow, which uses the ``gh workflow
    run`` subcommand -- not raw ``gh api`` -- so it is unaffected by the
    GET/POST default quirk, but its exact argv shape is still pinned here."""

    def test_dispatch_workflow_argv_shape(self):
        import release_github as gh_module

        runner = RecordingRunner(stdout="")
        client = gh_module.GhCliGitHubClient(repository="mono/SkiaSharp", runner=runner)
        client.dispatch_workflow(
            workflow="update-release-notes.lock.yml",
            ref="main",
            inputs={"source_branch": "main", "min_version": "3.119.0", "max_version": "3.119.0", "force": "false"},
        )
        argv = runner.calls[0]
        self.assertEqual(
            argv[:6], ["gh", "workflow", "run", "update-release-notes.lock.yml", "--repo", "mono/SkiaSharp"]
        )
        self.assertIn("--ref", argv)
        self.assertIn("main", argv)
        self.assertIn("source_branch=main", argv)
        self.assertIn("min_version=3.119.0", argv)
        self.assertIn("max_version=3.119.0", argv)
        self.assertIn("force=false", argv)

    def test_dispatch_workflow_with_no_inputs(self):
        import release_github as gh_module

        runner = RecordingRunner(stdout="")
        client = gh_module.GhCliGitHubClient(repository="mono/SkiaSharp", runner=runner)
        client.dispatch_workflow(workflow="auto-update-issue-template-versions.yml", ref="main", inputs={})
        argv = runner.calls[0]
        self.assertEqual(
            argv,
            ["gh", "workflow", "run", "auto-update-issue-template-versions.yml", "--repo", "mono/SkiaSharp", "--ref", "main"],
        )


class RenderPlanMarkdownFormatTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self):
        self._tmp.cleanup()

    def _write_prepare_plan(self) -> Path:
        import release_prepare as prepare

        plan = {
            "schemaVersion": 1,
            "operation": "prepare",
            "generatedAt": "2024-01-01T00:00:00Z",
            "toolingSha": "a" * 40,
            "nextAction": "apply",
            "input": {"integrationTarget": "main", "requestedVersion": None},
            "release": {
                "identity": "3.119.0-preview.1",
                "version": "3.119.0-preview.1",
                "numeric": "3.119.0",
                "label": "preview.1",
                "releaseType": "preview",
                "branch": "release/3.119.0-preview.1",
                "integrationBranch": "release/3.119.x",
                "isHotfix": False,
                "stable": False,
            },
            "base": {"ref": "refs/remotes/origin/main", "sha": "b" * 40},
            "maintenanceBranch": {
                "name": "release/3.119.x", "exists": False, "action": "create", "baseSha": "b" * 40
            },
            "skia": {"sha": "c" * 40, "releaseBranch": "release/3.119.0-preview.1", "remoteState": "missing"},
            "skiaSharpRemoteState": "missing",
            "versions": {"skiaSharp": "3.119.0", "requiresPackageBump": False},
            "operations": [
                {"id": "create-maintenance-branch", "kind": "git-ref", "status": "pending", "detail": None},
            ],
            "stableBump": None,
            "warnings": ["maintenance branch will be created"],
        }
        plan_path = self.root / "prepare-plan.json"
        common.write_plan(plan_path, plan, schema_name=prepare.PREPARE_SCHEMA)
        return plan_path

    def test_format_defaults_to_json(self):
        parser = cli.create_parser()
        args = parser.parse_args(["render-plan", "--plan", "plan.json"])
        self.assertEqual(args.format, "json")

    def test_format_markdown_parses(self):
        parser = cli.create_parser()
        args = parser.parse_args(["render-plan", "--plan", "plan.json", "--format", "markdown"])
        self.assertEqual(args.format, "markdown")

    def test_format_rejects_unknown_value(self):
        parser = cli.create_parser()
        with self.assertRaises(SystemExit):
            parser.parse_args(["render-plan", "--plan", "plan.json", "--format", "yaml"])

    def test_render_plan_markdown_writes_deterministic_report(self):
        plan_path = self._write_prepare_plan()
        output_path = self.root / "summary.md"
        exit_code = cli.main(
            ["render-plan", "--plan", str(plan_path), "--format", "markdown", "--output", str(output_path)]
        )
        self.assertEqual(exit_code, 0)
        text = output_path.read_text(encoding="utf-8")
        self.assertIn("# Release 3.119.0-preview.1", text)
        self.assertIn("Next action", text)
        self.assertIn("apply", text)
        self.assertIn("maintenance branch will be created", text)
        self.assertIn("## Operations", text)
        self.assertIn("create-maintenance-branch", text)

    def test_render_plan_markdown_is_deterministic_across_runs(self):
        plan_path = self._write_prepare_plan()
        output_path_1 = self.root / "summary1.md"
        output_path_2 = self.root / "summary2.md"
        cli.main(["render-plan", "--plan", str(plan_path), "--format", "markdown", "--output", str(output_path_1)])
        cli.main(["render-plan", "--plan", str(plan_path), "--format", "markdown", "--output", str(output_path_2)])
        self.assertEqual(output_path_1.read_text(), output_path_2.read_text())

    def test_render_plan_markdown_validates_inputs_same_as_json(self):
        plan_path = self._write_prepare_plan()
        text = plan_path.read_text(encoding="utf-8").replace("3.119.0-preview.1", "9.9.9-preview.9")
        plan_path.write_text(text, encoding="utf-8")
        exit_code = cli.main(["render-plan", "--plan", str(plan_path), "--format", "markdown"])
        self.assertEqual(exit_code, 1)


class PlanConsumptionDocumentationTests(unittest.TestCase):
    """finish publish/plan-publication/closeout all take --plan pointing at
    the *original* finish-plan.json (schema-validated via
    finish.FINISH_SCHEMA), never a result-envelope file that a previous
    create-draft/publish/closeout step wrote -- verified here both by
    inspecting the parser wiring/help text and by proving a result-envelope
    file is rejected by the schema those commands actually use."""

    @staticmethod
    def _finish_subparser(name: str):
        parser = cli.create_parser()
        finish_parser = None
        for action in parser._subparsers._group_actions:
            if hasattr(action, "choices") and "finish" in action.choices:
                finish_parser = action.choices["finish"]
                break
        for action in finish_parser._subparsers._group_actions:
            if hasattr(action, "choices") and name in action.choices:
                return action.choices[name]
        raise AssertionError(f"no 'finish {name}' subparser found")

    def test_help_text_documents_original_plan_consumption(self):
        for name in ("publish", "plan-publication", "closeout", "create-draft"):
            help_text = self._finish_subparser(name).format_help().lower().replace("\n", " ")
            self.assertIn(
                "original finish plan", help_text, f"'finish {name} --help' does not document plan consumption"
            )

    def test_publish_help_text_documents_publication_is_a_persisted_result(self):
        help_text = self._finish_subparser("publish").format_help().lower().replace("\n", " ")
        self.assertIn("plan-publication", help_text)
        self.assertIn("persisted result", help_text)

    def test_publish_plan_publication_closeout_use_finish_schema(self):
        # Parser wiring: all three subcommands share --plan and route through
        # functions that call common.read_plan(..., schema_name=finish.FINISH_SCHEMA).
        parser = cli.create_parser()
        for args in (
            ["finish", "publish", "--plan", "finish-plan.json", "--publication", "publication.json"],
            ["finish", "plan-publication", "--plan", "finish-plan.json"],
            ["finish", "closeout", "--plan", "finish-plan.json"],
        ):
            parsed = parser.parse_args(args)
            self.assertEqual(parsed.plan, "finish-plan.json")

    def test_result_envelope_file_is_rejected_as_a_finish_plan(self):
        import release_finish as finish

        plan = {
            "schemaVersion": 1, "operation": "finish", "toolingSha": "a" * 40, "nextAction": "create-draft",
            "release": {"identity": "3.119.0-preview.1", "version": "3.119.0-preview.1.42", "branch": "release/3.119.0-preview.1"},
        }
        plan = common.with_digest(plan)
        result = common.build_envelope(plan, next_action="plan-publication", tag="v3.119.0-preview.1")

        with tempfile.TemporaryDirectory() as tmp:
            result_path = Path(tmp) / "create-draft-result.json"
            common.write_json_file(result_path, result)
            with self.assertRaises(common.ValidationError):
                common.read_plan(result_path, schema_name=finish.FINISH_SCHEMA)


if __name__ == "__main__":
    unittest.main()
