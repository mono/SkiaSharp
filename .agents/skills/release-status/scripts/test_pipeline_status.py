#!/usr/bin/env python3

import importlib.util
import io
from pathlib import Path
from types import SimpleNamespace
import unittest
from unittest import mock


SCRIPT_PATH = Path(__file__).with_name("pipeline-status.py")
SPEC = importlib.util.spec_from_file_location("pipeline_status", SCRIPT_PATH)
pipeline_status = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(pipeline_status)


class PipelineStatusTests(unittest.TestCase):
    def test_az_uses_resolved_cmd_launcher(self):
        az_path = r"C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.CMD"

        with mock.patch.object(
                pipeline_status.shutil, "which", return_value=az_path) as which, \
                mock.patch.object(
                    pipeline_status.subprocess,
                    "run",
                    return_value=SimpleNamespace(stdout="[]\n"),
                ) as run:
            result = pipeline_status.az(["pipelines", "runs", "list"])

        which.assert_called_once_with("az")
        self.assertEqual(run.call_args.args[0][0], az_path)
        self.assertFalse(run.call_args.kwargs.get("shell", False))
        self.assertEqual(result, "[]")

    def test_az_fails_clearly_when_cli_is_missing(self):
        with mock.patch.object(pipeline_status.shutil, "which", return_value=None):
            with self.assertRaisesRegex(
                    SystemExit, "Azure CLI 'az' was not found on PATH"):
                pipeline_status.az(["pipelines", "runs", "list"])

    def test_output_escapes_non_ascii_for_legacy_streams(self):
        buffer = io.BytesIO()
        stream = io.TextIOWrapper(buffer, encoding="cp1252")

        with mock.patch.object(pipeline_status.sys, "stdout", stream):
            pipeline_status.output("caf\u00e9 \u2713 \U0001f600")
            stream.flush()

        self.assertEqual(
            buffer.getvalue().decode("ascii").splitlines(),
            ["caf\\xe9 \\u2713 \\U0001f600"],
        )
        stream.detach()

    def test_production_script_is_ascii_only(self):
        SCRIPT_PATH.read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
