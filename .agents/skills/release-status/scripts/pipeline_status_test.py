import importlib.util
import io
import os
import subprocess
import sys
import tempfile
import unittest
from contextlib import redirect_stdout
from pathlib import Path
from unittest.mock import Mock, patch


SCRIPT_PATH = Path(__file__).with_name("pipeline-status.py")
SPEC = importlib.util.spec_from_file_location("pipeline_status", SCRIPT_PATH)
if SPEC is None or SPEC.loader is None:
    raise RuntimeError(f"Could not load {SCRIPT_PATH}")
pipeline_status = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = pipeline_status
SPEC.loader.exec_module(pipeline_status)


class PipelineStatusTests(unittest.TestCase):
    def test_script_source_is_ascii_only(self) -> None:
        SCRIPT_PATH.read_bytes().decode("ascii")

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

        buffer = io.BytesIO()
        stream = io.TextIOWrapper(buffer, encoding="ascii", errors="strict")
        with redirect_stdout(stream):
            pipeline_status.emit(output)
        stream.flush()

        self.assertEqual(b"Caf\\xe9", buffer.getvalue().rstrip(b"\r\n"))

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

        with self.assertRaises(RuntimeError) as context:
            pipeline_status.az(["test"])

        self.assertEqual(
            "Azure CLI failed with exit code 2: "
            r"\xe9chec d'authentification",
            str(context.exception),
        )

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

    def test_dynamic_output_is_ascii(self) -> None:
        records = [
            {
                "type": "Job",
                "name": "Café\njob",
                "state": "inProgress",
                "result": "",
            }
        ]

        buffer = io.BytesIO()
        stream = io.TextIOWrapper(buffer, encoding="ascii", errors="strict")
        with redirect_stdout(stream):
            pipeline_status.format_job_summary(records, "| ")
        stream.flush()

        output = buffer.getvalue().decode("ascii").replace("\r\n", "\n")
        self.assertIn("Running: Caf\\xe9\njob", output)
        self.assertNotIn("\ufffd", output)


if __name__ == "__main__":
    unittest.main()
