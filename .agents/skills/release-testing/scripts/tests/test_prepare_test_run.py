#!/usr/bin/env python3

import importlib.util
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest


SCRIPTS = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(SCRIPTS))
SCRIPT_PATH = SCRIPTS / "prepare-test-run.py"
SPEC = importlib.util.spec_from_file_location("prepare_test_run", SCRIPT_PATH)
preparer = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = preparer
SPEC.loader.exec_module(preparer)


def simulator(version: str, device: str) -> dict:
    return {
        "isAvailable": True,
        "deviceType": {
            "name": device,
            "productFamily": "iPhone",
        },
        "runtime": {
            "name": f"iOS {version}",
            "version": version,
            "isAvailable": True,
        },
    }


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
        source = SCRIPT_PATH.read_text(encoding="ascii")
        self.assertIn(
            '["dotnet", "tool", "restore"]',
            source,
        )
        self.assertIn('"xcode",', source)

    def test_xcode_26_selects_ios_15_and_26(self):
        targets = preparer.select_apple_targets(
            "26.6",
            "/Applications/Xcode-26.6.0.app",
            [
                simulator("15.8", "iPhone 13 Pro"),
                simulator("15.0", "iPhone 13"),
                simulator("26.0", "iPhone 16"),
                simulator("26.5", "iPhone 17"),
            ],
        )

        self.assertEqual(targets["minimum"]["version"], "15.0")
        self.assertEqual(targets["minimum"]["device"], "iPhone 13")
        self.assertEqual(targets["maximum"]["version"], "26.5")
        self.assertEqual(targets["maximum"]["device"], "iPhone 17")

    def test_xcode_27_selects_ios_18_and_26(self):
        targets = preparer.select_apple_targets(
            "27.0",
            "/Applications/Xcode.app",
            [
                simulator("16.2", "iPhone 14"),
                simulator("18.2", "iPhone 16"),
                simulator("18.5", "iPhone 16 Pro"),
                simulator("26.5", "iPhone 17"),
                simulator("27.0", "iPhone 17 Pro"),
            ],
        )

        self.assertEqual(targets["minimum"]["version"], "18.2")
        self.assertEqual(targets["minimum"]["device"], "iPhone 16")
        self.assertEqual(targets["maximum"]["version"], "26.5")
        self.assertEqual(targets["maximum"]["device"], "iPhone 17")

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
