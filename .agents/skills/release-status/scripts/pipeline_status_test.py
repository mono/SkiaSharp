import importlib.util
import os
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path
from unittest.mock import Mock, patch


SCRIPT_PATH = Path(__file__).with_name("pipeline-status.py")
SPEC = importlib.util.spec_from_file_location("pipeline_status", SCRIPT_PATH)
if SPEC is None or SPEC.loader is None:
    raise RuntimeError(f"Could not load {SCRIPT_PATH}")
pipeline_status = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = pipeline_status
SPEC.loader.exec_module(pipeline_status)


class Stream:
    def __init__(self, encoding: str):
        self.encoding = encoding


class PipelineStatusTests(unittest.TestCase):
    @patch.object(pipeline_status.shutil, "which", return_value="/usr/bin/az")
    def test_resolves_native_cli(self, which: Mock) -> None:
        command = pipeline_status.cli_command("az", ["--version"])

        self.assertEqual(["/usr/bin/az", "--version"], command)
        which.assert_called_once_with("az")

    @patch.object(pipeline_status.shutil, "which", return_value=r"C:\Tools\Azure CLI\az.cmd")
    def test_resolves_official_windows_launcher(self, which: Mock) -> None:
        command = pipeline_status.cli_command("az", ["pipelines", "runs", "list"])

        self.assertEqual(
            [
                r"C:\Tools\Azure CLI\az.cmd",
                "pipelines",
                "runs",
                "list",
            ],
            command,
        )
        which.assert_called_once_with("az")

    @unittest.skipUnless(os.name == "nt", "Windows .cmd launchers require Windows")
    def test_executes_windows_launcher_from_path_with_spaces(self) -> None:
        with tempfile.TemporaryDirectory(prefix="pipeline status ") as directory:
            launcher = Path(directory) / "fake az.cmd"
            launcher.write_text("@echo off\r\necho launcher-%1\r\n", encoding="utf-8")

            with patch.object(
                pipeline_status.shutil, "which", return_value=str(launcher)
            ):
                command = pipeline_status.cli_command("az", ["working"])

            result = subprocess.run(
                command,
                capture_output=True,
                text=True,
                encoding="utf-8",
                check=True,
            )

        self.assertEqual("launcher-working", result.stdout.strip())

    @patch.object(pipeline_status, "cli_command", return_value=["az", "test"])
    @patch.object(pipeline_status.subprocess, "run")
    def test_azure_cli_uses_checked_binary_execution(
        self, run: Mock, cli_command: Mock
    ) -> None:
        run.return_value = subprocess.CompletedProcess(
            ["az", "test"], 0, stdout=b"[]\n", stderr=b""
        )

        self.assertEqual("[]", pipeline_status.az(["test"]))

        cli_command.assert_called_once_with("az", ["test"])
        self.assertTrue(run.call_args.kwargs["check"])
        self.assertNotIn("text", run.call_args.kwargs)
        self.assertNotIn("encoding", run.call_args.kwargs)

    @patch.object(pipeline_status.locale, "getencoding", return_value="cp1252")
    @patch.object(pipeline_status, "cli_command")
    def test_azure_cli_preserves_cp1252_subprocess_bytes(
        self, cli_command: Mock, _: Mock
    ) -> None:
        payload = "Café".encode("cp1252")
        cli_command.return_value = [
            sys.executable,
            "-c",
            f"import sys; sys.stdout.buffer.write({payload!r})",
        ]

        output = pipeline_status.az(["test"])

        self.assertEqual("Café", output)
        self.assertNotIn("\ufffd", output)
        output.encode("cp1252")

    @patch.object(pipeline_status, "cli_command", return_value=["az", "test"])
    @patch.object(pipeline_status.subprocess, "run")
    def test_azure_cli_error_includes_stderr(self, run: Mock, _: Mock) -> None:
        run.side_effect = subprocess.CalledProcessError(
            2, ["az", "test"], stderr=b"authentication required"
        )

        with self.assertRaisesRegex(
            RuntimeError, "exit code 2: authentication required"
        ):
            pipeline_status.az(["test"])

    @patch.object(pipeline_status.locale, "getencoding", return_value="cp1252")
    @patch.object(pipeline_status, "cli_command", return_value=["az", "test"])
    @patch.object(pipeline_status.subprocess, "run")
    def test_azure_cli_error_preserves_cp1252_stderr(
        self, run: Mock, _: Mock, __: Mock
    ) -> None:
        run.side_effect = subprocess.CalledProcessError(
            2, ["az", "test"], stderr="échec d'authentification".encode("cp1252")
        )

        with self.assertRaisesRegex(
            RuntimeError, "exit code 2: échec d'authentification"
        ):
            pipeline_status.az(["test"])

    @patch.object(pipeline_status, "cli_command", return_value=["az", "test"])
    @patch.object(pipeline_status.subprocess, "run")
    def test_azure_cli_rejects_empty_stdout(self, run: Mock, _: Mock) -> None:
        run.return_value = subprocess.CompletedProcess(
            ["az", "test"], 0, stdout=b"  ", stderr=b"unexpected response"
        )

        with self.assertRaisesRegex(
            RuntimeError, "returned no output: unexpected response"
        ):
            pipeline_status.az(["test"])

    def test_cp1252_output_uses_ascii_fallback(self) -> None:
        style = pipeline_status.output_style(Stream("cp1252"))
        output = "".join(style.icons.values()) + (
            style.first_prefix
            + style.middle_prefix
            + style.last_prefix
            + style.continuation
            + style.horizontal
            + style.separator
            + style.upstream
            + style.resolved
            + style.ellipsis
        )

        output.encode("cp1252")
        self.assertIs(pipeline_status.ASCII_STYLE, style)


if __name__ == "__main__":
    unittest.main()
