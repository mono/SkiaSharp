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
    def test_json_parser_ignores_command_noise(self):
        self.assertEqual(common.parse_json_output('WARN [tool] not JSON\nstatus\n{"ready": true}\n'), {"ready": True})

    def test_all_python_scripts_are_ascii_only(self):
        tests = Path(__file__).parent
        for script in (*SCRIPTS.glob("*.py"), *tests.glob("*.py")):
            script.read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
