import importlib.util
import json
import subprocess
import sys
import unittest
from pathlib import Path
from unittest.mock import Mock, patch


SCRIPT_PATH = Path(__file__).with_name("queue-publish.py")
SPEC = importlib.util.spec_from_file_location("queue_publish", SCRIPT_PATH)
if SPEC is None or SPEC.loader is None:
    raise RuntimeError(f"Could not load {SCRIPT_PATH}")
queue_publish = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = queue_publish
SPEC.loader.exec_module(queue_publish)


class QueuePublishTests(unittest.TestCase):
    def test_stable_body_uses_build_number_and_pushes_stable(self) -> None:
        build_number = "4.151.1-stable.1+4.151.1"

        body = queue_publish.build_queue_body(build_number)
        serialized = json.dumps(body)

        self.assertEqual(
            build_number,
            body["resources"]["pipelines"]["SkiaSharp"]["version"],
        )
        self.assertTrue(body["templateParameters"]["pushStable"])
        self.assertNotIn("14874440", serialized)

    def test_preview_and_rc_bodies_do_not_push_stable(self) -> None:
        build_numbers = [
            "4.152.0-preview.1.2+4.152.0-preview.1",
            "4.152.0-rc.1.3+4.152.0-rc.1",
        ]

        for build_number in build_numbers:
            with self.subTest(build_number=build_number):
                body = queue_publish.build_queue_body(build_number)
                self.assertFalse(body["templateParameters"]["pushStable"])
                self.assertEqual(
                    build_number,
                    body["resources"]["pipelines"]["SkiaSharp"]["version"],
                )

    def test_rejects_numeric_run_id(self) -> None:
        with self.assertRaisesRegex(ValueError, "not a numeric run/build ID"):
            queue_publish.build_queue_body("14874440")

    def test_rejects_mismatched_or_main_build_numbers(self) -> None:
        invalid = [
            "4.151.1-stable.1+4.151.0",
            "4.152.0-preview.1.2+4.152.0-preview.2",
            "4.152.0-preview.0.7+main",
        ]

        for build_number in invalid:
            with self.subTest(build_number=build_number):
                with self.assertRaisesRegex(ValueError, "Managed build number must match"):
                    queue_publish.build_queue_body(build_number)

    @patch.object(queue_publish.shutil, "which", return_value=r"C:\Tools\az.cmd")
    @patch.object(queue_publish.subprocess, "run")
    def test_queue_returns_run_id_and_url(
        self, run: Mock, _: Mock
    ) -> None:
        run.return_value = subprocess.CompletedProcess(
            ["az"], 0, stdout="14890000\n", stderr=""
        )

        run_id, url = queue_publish.queue_publish(
            "4.151.1-stable.1+4.151.1"
        )

        self.assertEqual(14890000, run_id)
        self.assertEqual(
            "https://dev.azure.com/devdiv/DevDiv/_build/results?buildId=14890000",
            url,
        )
        queue_args = run.call_args.args[0]
        self.assertIn("POST", queue_args)
        self.assertIn("pipelineId=25298", queue_args)
        self.assertIn("--query", queue_args)
        self.assertTrue(run.call_args.kwargs["text"])
        self.assertTrue(run.call_args.kwargs["check"])

    @patch.object(queue_publish.shutil, "which", return_value="az")
    @patch.object(queue_publish.subprocess, "run")
    def test_queue_propagates_cli_failure(self, run: Mock, _: Mock) -> None:
        run.side_effect = subprocess.CalledProcessError(
            1, ["az"], stderr="permission denied"
        )

        with self.assertRaisesRegex(RuntimeError, "permission denied"):
            queue_publish.queue_publish("4.151.1-stable.1+4.151.1")

    @patch.object(queue_publish.shutil, "which", return_value="az")
    @patch.object(queue_publish.subprocess, "run")
    def test_queue_rejects_missing_run_id(self, run: Mock, _: Mock) -> None:
        run.return_value = subprocess.CompletedProcess(
            ["az"], 0, stdout="", stderr=""
        )

        with self.assertRaisesRegex(RuntimeError, "numeric publish run ID"):
            queue_publish.queue_publish("4.151.1-stable.1+4.151.1")


if __name__ == "__main__":
    unittest.main()
