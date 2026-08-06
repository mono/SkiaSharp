import importlib.util
import json
import sys
import unittest
from pathlib import Path
from unittest.mock import Mock, patch


SCRIPT_PATH = Path(__file__).with_name("queue-publish-run.py")
SPEC = importlib.util.spec_from_file_location("queue_publish_run", SCRIPT_PATH)
if SPEC is None or SPEC.loader is None:
    raise RuntimeError(f"Could not load {SCRIPT_PATH}")
queue_publish_run = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = queue_publish_run
SPEC.loader.exec_module(queue_publish_run)


class QueuePublishRunTests(unittest.TestCase):
    @patch.object(
        queue_publish_run.shutil,
        "which",
        return_value=r"C:\Program Files\Azure CLI\az.cmd",
    )
    def test_resolves_platform_cli_launcher(self, which: Mock) -> None:
        command = queue_publish_run.cli_command("az", ["--version"])

        self.assertEqual(
            [r"C:\Program Files\Azure CLI\az.cmd", "--version"],
            command,
        )
        which.assert_called_once_with("az")

    @patch.object(
        queue_publish_run.locale,
        "getpreferredencoding",
        return_value="cp1252",
    )
    def test_decodes_utf8_before_platform_encoding(self, _: Mock) -> None:
        self.assertEqual(
            "Café",
            queue_publish_run.decode_output("Café".encode("utf-8")),
        )

    @patch.object(
        queue_publish_run.locale,
        "getpreferredencoding",
        return_value="cp1252",
    )
    def test_falls_back_to_platform_encoding(self, _: Mock) -> None:
        self.assertEqual(
            "Café",
            queue_publish_run.decode_output("Café".encode("cp1252")),
        )

    def test_stable_body_uses_build_number_and_pushes_stable(self) -> None:
        build_number = "4.151.1-stable.1+4.151.1"

        body = queue_publish_run.build_queue_body(build_number)
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
                body = queue_publish_run.build_queue_body(build_number)
                self.assertFalse(body["templateParameters"]["pushStable"])
                self.assertEqual(
                    build_number,
                    body["resources"]["pipelines"]["SkiaSharp"]["version"],
                )

    def test_rejects_numeric_run_id(self) -> None:
        with self.assertRaisesRegex(ValueError, "not a numeric run/build ID"):
            queue_publish_run.build_queue_body("14874440")

    def test_rejects_mismatched_or_main_build_numbers(self) -> None:
        invalid = [
            "4.151.1-stable.1+4.151.0",
            "4.152.0-preview.1.2+4.152.0-preview.2",
            "4.152.0-preview.0.7+main",
        ]

        for build_number in invalid:
            with self.subTest(build_number=build_number):
                with self.assertRaisesRegex(ValueError, "Managed build number must match"):
                    queue_publish_run.build_queue_body(build_number)

    def test_finds_active_publish_states(self) -> None:
        runs = [
            {"id": 1, "status": "completed"},
            {"id": 2, "status": "notStarted"},
            {"id": 3, "status": "inProgress"},
            {"id": 4, "status": "postponed"},
            {"id": 5, "status": "cancelling"},
        ]

        active = queue_publish_run.find_active_runs(runs)

        self.assertEqual([2, 3, 4, 5], [run["id"] for run in active])

    @patch.object(queue_publish_run, "run_az_json")
    def test_queue_returns_run_id_and_url(self, run_az_json: Mock) -> None:
        run_az_json.side_effect = [[], {"id": 14890000}]

        run_id, url = queue_publish_run.queue_publish(
            "4.151.1-stable.1+4.151.1"
        )

        self.assertEqual(14890000, run_id)
        self.assertEqual(
            "https://dev.azure.com/devdiv/DevDiv/_build/results?buildId=14890000",
            url,
        )
        queue_args = run_az_json.call_args_list[1].args[0]
        self.assertIn("POST", queue_args)
        self.assertIn("pipelineId=25298", queue_args)

    @patch.object(queue_publish_run, "run_az_json")
    def test_queue_rejects_active_run_before_posting(
        self, run_az_json: Mock
    ) -> None:
        run_az_json.return_value = [
            {"id": 14889999, "status": "inProgress", "buildNumber": "20260806.1"}
        ]

        with self.assertRaisesRegex(RuntimeError, "Do not queue a duplicate"):
            queue_publish_run.queue_publish("4.151.1-stable.1+4.151.1")

        run_az_json.assert_called_once()

    @patch.object(queue_publish_run, "queue_publish")
    def test_cli_requires_confirmation_flag(self, queue_publish: Mock) -> None:
        argv = [str(SCRIPT_PATH), "4.151.1-stable.1+4.151.1"]

        with patch.object(sys, "argv", argv):
            with self.assertRaises(SystemExit) as error:
                queue_publish_run.main()

        self.assertEqual(2, error.exception.code)
        queue_publish.assert_not_called()

    @patch.object(queue_publish_run, "run_az_json")
    def test_queue_fails_when_api_omits_run_id(self, run_az_json: Mock) -> None:
        run_az_json.side_effect = [[], {"state": "inProgress"}]

        with self.assertRaisesRegex(RuntimeError, "numeric publish run ID"):
            queue_publish_run.queue_publish("4.151.1-stable.1+4.151.1")


if __name__ == "__main__":
    unittest.main()
