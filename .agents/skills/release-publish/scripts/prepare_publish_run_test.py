import importlib.util
import json
import sys
import tempfile
import unittest
from pathlib import Path
from unittest.mock import Mock, patch


SCRIPT_PATH = Path(__file__).with_name("prepare-publish-run.py")
SPEC = importlib.util.spec_from_file_location("prepare_publish_run", SCRIPT_PATH)
if SPEC is None or SPEC.loader is None:
    raise RuntimeError(f"Could not load {SCRIPT_PATH}")
prepare_publish_run = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = prepare_publish_run
SPEC.loader.exec_module(prepare_publish_run)


STABLE_COMMIT = "279f93f4ffa7f9fe4e9c0bc298bedc3c9e439764"
PREVIEW_COMMIT = "f258a99744e10288d794859b523ca3c115e66819"


def managed_run(
    *,
    run_id: int = 14874440,
    build_number: str = "4.151.1-stable.1+4.151.1",
    branch: str = "refs/heads/release/4.151.1",
    commit: str = STABLE_COMMIT,
    status: str = "completed",
    result: str = "succeeded",
    definition_id: int = prepare_publish_run.MANAGED_PIPELINE_ID,
) -> dict:
    return {
        "id": run_id,
        "buildNumber": build_number,
        "sourceBranch": branch,
        "sourceVersion": commit,
        "status": status,
        "result": result,
        "definition": {"id": definition_id, "name": "SkiaSharp"},
    }


class PreparePublishRunTests(unittest.TestCase):
    def test_stable_body_uses_build_number_not_numeric_run_id(self) -> None:
        build_number, push_stable = prepare_publish_run.validate_managed_run(
            managed_run(),
            14874440,
            "4.151.1",
            STABLE_COMMIT,
        )
        body = prepare_publish_run.build_queue_body(build_number, push_stable)
        serialized = json.dumps(body)

        self.assertEqual(
            "4.151.1-stable.1+4.151.1",
            body["resources"]["pipelines"]["SkiaSharp"]["version"],
        )
        self.assertTrue(body["templateParameters"]["pushStable"])
        self.assertNotIn("14874440", serialized)

    def test_preview_body_selects_push_preview(self) -> None:
        run = managed_run(
            run_id=14880765,
            build_number="4.152.0-preview.1.1+4.152.0-preview.1",
            branch="refs/heads/release/4.152.0-preview.1",
            commit=PREVIEW_COMMIT,
        )

        build_number, push_stable = prepare_publish_run.validate_managed_run(
            run,
            14880765,
            "4.152.0-preview.1",
            PREVIEW_COMMIT,
        )
        body = prepare_publish_run.build_queue_body(build_number, push_stable)

        self.assertFalse(body["templateParameters"]["pushStable"])
        self.assertEqual("SkiaSharp", body["templateParameters"]["selectedResource"])
        self.assertTrue(body["templateParameters"]["pushPackages"])

    def test_rejects_wrong_pipeline_state_branch_commit_and_label(self) -> None:
        cases = [
            (
                managed_run(definition_id=999),
                "4.151.1",
                STABLE_COMMIT,
                "not managed pipeline",
            ),
            (
                managed_run(status="inProgress", result=None),
                "4.151.1",
                STABLE_COMMIT,
                "completed/succeeded",
            ),
            (
                managed_run(branch="refs/heads/main"),
                "4.151.1",
                STABLE_COMMIT,
                "expected 'refs/heads/release/4.151.1'",
            ),
            (
                managed_run(commit="a" * 40),
                "4.151.1",
                STABLE_COMMIT,
                "expected '279f93f4",
            ),
            (
                managed_run(build_number="4.151.1-preview.1.1+4.151.1-preview.1"),
                "4.151.1",
                STABLE_COMMIT,
                "expected 'stable' release label",
            ),
        ]

        for run, version, commit, message in cases:
            with self.subTest(message=message):
                with self.assertRaisesRegex(ValueError, message):
                    prepare_publish_run.validate_managed_run(
                        run,
                        run["id"],
                        version,
                        commit,
                    )

    def test_rejects_abbreviated_commit(self) -> None:
        with self.assertRaisesRegex(ValueError, "full 40-character"):
            prepare_publish_run.validate_managed_run(
                managed_run(),
                14874440,
                "4.151.1",
                STABLE_COMMIT[:12],
            )

    def test_finds_all_active_publish_states(self) -> None:
        runs = [
            {"id": 1, "status": "completed"},
            {"id": 2, "status": "notStarted"},
            {"id": 3, "status": "inProgress"},
            {"id": 4, "status": "postponed"},
            {"id": 5, "status": "cancelling"},
            {"id": 6, "status": "queued"},
        ]

        active = prepare_publish_run.find_active_publish_runs(runs)

        self.assertEqual([2, 3, 4, 5, 6], [run["id"] for run in active])

    @patch.object(prepare_publish_run, "run_az_json")
    def test_prepare_request_rejects_active_publish_run(
        self, run_az_json: Mock
    ) -> None:
        run_az_json.side_effect = [
            managed_run(),
            [{"id": 14890000, "status": "inProgress", "buildNumber": "20260806.1"}],
        ]

        with self.assertRaisesRegex(RuntimeError, "Do not queue a duplicate"):
            prepare_publish_run.prepare_queue_request(
                14874440,
                "4.151.1",
                STABLE_COMMIT,
            )

    @patch.object(prepare_publish_run, "run_az_json")
    def test_prepare_request_queries_by_id_but_queues_by_number(
        self, run_az_json: Mock
    ) -> None:
        run_az_json.side_effect = [managed_run(), []]

        body, build_number, push_stable = prepare_publish_run.prepare_queue_request(
            14874440,
            "4.151.1",
            STABLE_COMMIT,
        )

        self.assertEqual("4.151.1-stable.1+4.151.1", build_number)
        self.assertTrue(push_stable)
        self.assertEqual(
            build_number,
            body["resources"]["pipelines"]["SkiaSharp"]["version"],
        )
        self.assertEqual(
            "14874440",
            run_az_json.call_args_list[0].args[0][
                run_az_json.call_args_list[0].args[0].index("--id") + 1
            ],
        )

    @patch.object(prepare_publish_run.locale, "getencoding", return_value="cp1252")
    @patch.object(prepare_publish_run, "cli_command", return_value=["az", "test"])
    @patch.object(prepare_publish_run.subprocess, "run")
    def test_azure_json_preserves_cp1252_bytes(
        self, run: Mock, _: Mock, __: Mock
    ) -> None:
        run.return_value = prepare_publish_run.subprocess.CompletedProcess(
            ["az", "test"],
            0,
            stdout='{"message":"Café"}'.encode("cp1252"),
            stderr=b"",
        )

        output = prepare_publish_run.run_az_json(["test"])

        self.assertEqual("Café", output["message"])
        self.assertNotIn("encoding", run.call_args.kwargs)

    @patch.object(prepare_publish_run, "prepare_queue_request")
    def test_failed_revalidation_removes_stale_request(
        self, prepare_queue_request: Mock
    ) -> None:
        prepare_queue_request.side_effect = RuntimeError("active publish run")

        with tempfile.TemporaryDirectory() as temp_dir:
            output = Path(temp_dir) / "publish-request.json"
            output.write_text('{"stale": true}\n', encoding="utf-8")
            argv = [
                str(SCRIPT_PATH),
                "--managed-run-id",
                "14874440",
                "--release-version",
                "4.151.1",
                "--release-commit",
                STABLE_COMMIT,
                "--output",
                str(output),
            ]

            with patch.object(sys, "argv", argv):
                with self.assertRaisesRegex(RuntimeError, "active publish run"):
                    prepare_publish_run.main()

            self.assertFalse(output.exists())


if __name__ == "__main__":
    unittest.main()
