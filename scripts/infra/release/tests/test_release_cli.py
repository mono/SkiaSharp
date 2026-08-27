from __future__ import annotations

import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

import release as cli


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

    def test_finish_plan_parses(self):
        parser = cli.create_parser()
        args = parser.parse_args(["finish", "plan", "--version", "3.119.0"])
        self.assertIs(args.func, cli.cmd_finish_plan)

    def test_finish_create_draft_parses(self):
        parser = cli.create_parser()
        args = parser.parse_args(["finish", "create-draft", "--plan", "finish-plan.json"])
        self.assertIs(args.func, cli.cmd_finish_create_draft)

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
        args = parser.parse_args(["finish", "closeout", "--dry-run"])
        self.assertIs(args.func, cli.cmd_finish_closeout)
        self.assertTrue(args.dry_run)

    def test_inspect_parses(self):
        parser = cli.create_parser()
        args = parser.parse_args(["inspect", "--release-branch", "release/3.119.0"])
        self.assertIs(args.func, cli.cmd_inspect)

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


if __name__ == "__main__":
    unittest.main()
