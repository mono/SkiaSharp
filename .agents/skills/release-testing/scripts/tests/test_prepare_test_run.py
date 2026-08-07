#!/usr/bin/env python3

import importlib.util
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest


SCRIPT_PATH = Path(__file__).resolve().parent.parent / "prepare-test-run.py"
SPEC = importlib.util.spec_from_file_location("prepare_test_run", SCRIPT_PATH)
preparer = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = preparer
SPEC.loader.exec_module(preparer)


class PrepareTestRunTests(unittest.TestCase):
    def test_reset_output_removes_only_integration_artifacts(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            integration = root / preparer.OUTPUT_PATH
            integration.mkdir(parents=True)
            (integration / "old.png").write_text("old", encoding="ascii")
            nested = integration / "nested"
            nested.mkdir()
            (nested / "old.txt").write_text("old", encoding="ascii")
            sibling = integration.parent / "keep.txt"
            sibling.write_text("keep", encoding="ascii")

            result = preparer.reset_output(root)

            self.assertEqual(result, integration.resolve())
            self.assertEqual(list(integration.iterdir()), [])
            self.assertEqual(sibling.read_text(encoding="ascii"), "keep")

    def test_scripts_are_ascii_only(self):
        SCRIPT_PATH.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")

    def test_preparation_restores_pinned_tools(self):
        self.assertIn(
            '["dotnet", "tool", "restore"]',
            SCRIPT_PATH.read_text(encoding="ascii"),
        )

    def test_help_is_inert(self):
        result = subprocess.run(
            [sys.executable, str(SCRIPT_PATH), "--help"],
            capture_output=True,
            text=True,
        )
        self.assertEqual(result.returncode, 0)
        self.assertIn("usage:", result.stdout)


if __name__ == "__main__":
    unittest.main()
