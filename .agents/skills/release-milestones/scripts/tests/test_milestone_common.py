#!/usr/bin/env python3

import importlib.util
from pathlib import Path
import sys
import tempfile
import unittest
from unittest import mock


SCRIPTS = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(SCRIPTS))
SCRIPT_PATH = SCRIPTS / "milestone_common.py"
SPEC = importlib.util.spec_from_file_location("milestone_common_test", SCRIPT_PATH)
common = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = common
SPEC.loader.exec_module(common)


class MilestoneCommonTests(unittest.TestCase):
    def test_reads_major_and_skia_milestone(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            path = root / common.VERSIONS_PATH
            path.parent.mkdir(parents=True)
            path.write_text(
                "SkiaSharp  nuget  4.152.0\n"
                "libSkiaSharp  milestone  152\n",
                encoding="ascii",
            )
            self.assertEqual(common.read_current_version(root), (4, 152))

    def test_github_retries_transient_gateway_errors(self):
        github = object.__new__(common.GitHub)
        github.gh_path = "gh"
        github.repository = "mono/SkiaSharp"
        with (
            mock.patch.object(
                common,
                "run_json",
                side_effect=[
                    common.MilestoneError("gh: HTTP 504"),
                    {"ok": True},
                ],
            ) as runner,
            mock.patch.object(common.time, "sleep"),
        ):
            self.assertEqual(github.json(["api", "example"]), {"ok": True})
        self.assertEqual(runner.call_count, 2)

    def test_scripts_are_ascii_only(self):
        SCRIPT_PATH.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
