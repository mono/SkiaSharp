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
        args = parser.parse_args(["finish", "publish", "--plan", "finish-plan.json"])
        self.assertIs(args.func, cli.cmd_finish_publish)

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


if __name__ == "__main__":
    unittest.main()
