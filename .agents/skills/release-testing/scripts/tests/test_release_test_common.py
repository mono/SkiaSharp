#!/usr/bin/env python3

import importlib.util
from pathlib import Path
import sys
import unittest


SCRIPTS = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(SCRIPTS))
SCRIPT_PATH = SCRIPTS / "release_test_common.py"
SPEC = importlib.util.spec_from_file_location("release_test_common", SCRIPT_PATH)
common = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = common
SPEC.loader.exec_module(common)


class ReleaseTestCommonTests(unittest.TestCase):
    def test_mobile_test_versions_are_centralized(self):
        self.assertEqual(common.ANDROID_MIN_VERSION, "26")
        self.assertEqual(common.ANDROID_MAX_VERSION, "37.1")
        self.assertEqual(common.IOS_MIN_VERSION, "18.6")
        self.assertEqual(common.IOS_MAX_VERSION, "26.5")

    def test_json_parser_ignores_command_noise(self):
        self.assertEqual(common.parse_json_output('WARN [tool] not JSON\nstatus\n{"ready": true}\n'), {"ready": True})

    def test_scripts_are_ascii_only(self):
        SCRIPT_PATH.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
