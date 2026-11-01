from contextlib import redirect_stderr, redirect_stdout
import io
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import unittest
from unittest.mock import ANY, patch
import uuid

import build_managed
import build_native
import run_tests
from validation_common import ARTIFACTS, argument_parser, run_validation, validation_paths


SCRIPTS = Path(__file__).resolve().parent
PUSH_SCRIPT = SCRIPTS.parents[3] / ".github/scripts/skia-sync-push-prs.sh"
FIXTURE = """\
import json
import os
from pathlib import Path
import sys

artifacts = Path(os.environ["FIXTURE_ARTIFACTS"])
record = {
    "executable": Path(sys.argv[0]).name,
    "args": sys.argv[1:],
    "cwd": os.getcwd(),
    "receipts": sorted(path.name for path in artifacts.glob("*-exit-code.txt")),
}
with open(os.environ["FIXTURE_CALLS"], "a", encoding="utf-8") as calls:
    calls.write(json.dumps(record) + "\\n")
os.write(1, b"fixture stdout\\n")
os.write(2, b"fixture stderr\\n")
if sys.argv[1:] == ["tool", "restore"]:
    sys.exit(int(os.environ.get("FIXTURE_RESTORE_EXIT", "0")))
sys.exit(int(os.environ.get("FIXTURE_EXIT", "0")))
"""


class ValidationFixture(unittest.TestCase):
    def setUp(self):
        self.root = SCRIPTS / f".validation-test-{uuid.uuid4().hex}"
        self.root.mkdir()
        self.addCleanup(shutil.rmtree, self.root)
        self.repo = self.root / "repo with spaces; not a shell"
        self.repo.mkdir()
        self.artifacts = self.root / "artifacts with spaces"
        self.artifacts.mkdir()
        self.bin = self.root / "bin"
        self.bin.mkdir()
        self.calls = self.root / "calls.jsonl"
        self.env = dict(os.environ)
        self.env.pop("ARTIFACT_DIR", None)
        self.env.update({
            "PATH": str(self.bin),
            "SKIA_SYNC_ARTIFACT_DIR": str(self.artifacts),
            "FIXTURE_ARTIFACTS": str(self.artifacts),
            "FIXTURE_CALLS": str(self.calls),
        })

    def records(self):
        return [
            json.loads(line) for line in self.calls.read_text(encoding="utf-8").splitlines()
        ] if self.calls.exists() else []

    def receipt(self, stage):
        return self.artifacts / ARTIFACTS[stage][1]

    def log(self, stage):
        return self.artifacts / ARTIFACTS[stage][0]


class RunValidationTests(ValidationFixture):
    def setUp(self):
        super().setUp()
        self.fixture = self.root / "fixture.py"
        self.fixture.write_text(FIXTURE, encoding="utf-8")

    def run_cli(self, stage, *args, runner_exit=0):
        script = {"native": build_native, "managed": build_managed, "tests": run_tests}[stage]
        def checked_run(selected_stage, parser, parsed_args, commands):
            from validation_common import validation_paths
            validation_paths(parser, parsed_args)
            return runner_exit

        with (
            patch.dict(os.environ, self.env, clear=True),
            patch.object(sys, "argv", [
                script.__file__, "--repo-root", str(self.repo), *args,
            ]),
            patch.object(script, "run_validation", side_effect=checked_run) as runner,
            patch(
                "validation_common.subprocess.run",
                side_effect=AssertionError("CLI command selection must not launch a process"),
            ),
            redirect_stderr(io.StringIO()),
        ):
            try:
                exit_code = script.main()
            except SystemExit as error:
                exit_code = error.code
        return exit_code, runner

    def run_commands(self, stage, commands):
        stdout, stderr = io.StringIO(), io.StringIO()
        with (
            patch.dict(os.environ, self.env, clear=True),
            redirect_stdout(stdout),
            redirect_stderr(stderr),
        ):
            parser = argument_parser("Fixture validation")
            args = parser.parse_args([
                "--repo-root", str(self.repo), "--artifact-dir", str(self.artifacts),
            ])
            exit_code = run_validation(stage, parser, args, commands)
        return subprocess.CompletedProcess(
            commands, exit_code, stdout.getvalue(), stderr.getvalue(),
        )

    def run_stage(self, stage, *arguments):
        commands = [
            [sys.executable, str(self.fixture), *args]
            for args in (arguments or ([],))
        ]
        return self.run_commands(stage, commands)

    def test_success_uses_exact_managed_and_unfiltered_test_commands(self):
        for stage, expected in (
            ("managed", ["build", "binding/SkiaSharp/SkiaSharp.csproj"]),
            ("tests", [
                "test", "tests/SkiaSharp.Tests.Console.slnx",
                "-p:TargetFramework=net10.0", "-p:TargetFrameworks=net10.0",
            ]),
        ):
            with self.subTest(stage=stage):
                exit_code, runner = self.run_cli(stage)
                self.assertEqual(0, exit_code)
                runner.assert_called_once_with(
                    stage, ANY, ANY, [["dotnet", *expected]],
                )
                self.assertEqual([], self.records())

    def test_native_forwards_platform_and_architecture_without_an_allowlist(self):
        for platform, arch in (
            ("linux", "x64"), ("linux", "riscv64"), ("windows", "arm64"),
            ("macos", "arm64"), ("tizen", "x64"), ("future-platform", "future-arch"),
        ):
            with self.subTest(platform=platform, arch=arch):
                exit_code, runner = self.run_cli(
                    "native", "--platform", platform, "--arch", arch,
                )
                self.assertEqual(0, exit_code)
                runner.assert_called_once_with(
                    "native", ANY, ANY, [
                        ["dotnet", "tool", "restore"],
                        ["dotnet", "cake", f"--target=externals-{platform}", f"--arch={arch}"],
                    ],
                )
                self.assertEqual([], self.records())

    def test_native_requires_explicit_arguments_and_rejects_extra_commands(self):
        for args in (
            (), ("--platform", "linux"), ("--arch", "x64"),
            ("--platform", "linux", "--arch", "x64", "--", "echo", "ignored"),
        ):
            with self.subTest(args=args):
                exit_code, runner = self.run_cli("native", *args)
                self.assertEqual(2, exit_code)
                runner.assert_not_called()
                self.assertEqual([], self.records())

    def test_non_native_stages_reject_native_options_and_arbitrary_commands(self):
        for stage in ("managed", "tests"):
            for args in (("--platform", "linux"), ("--filter", "Something"), ("echo",)):
                with self.subTest(stage=stage, args=args):
                    exit_code, runner = self.run_cli(stage, *args)
                    self.assertEqual(2, exit_code)
                    runner.assert_not_called()
                    self.assertEqual([], self.records())

    def test_each_script_has_an_independent_cli_without_stage_selection(self):
        for script in (build_native, build_managed, run_tests):
            with self.subTest(script=script.__name__):
                result = subprocess.run(
                    [sys.executable, script.__file__, "--help"],
                    env=self.env, capture_output=True, text=True, check=False,
                )
                self.assertEqual(0, result.returncode, result.stderr)
                self.assertIn("--artifact-dir", result.stdout)
                self.assertEqual(script is build_native, "--platform" in result.stdout)
                self.assertEqual(script is build_native, "--arch" in result.stdout)
                self.assertEqual([], self.records())
        for stage in ARTIFACTS:
            with self.subTest(stage=stage):
                exit_code, runner = self.run_cli(stage, stage)
                self.assertEqual(2, exit_code)
                runner.assert_not_called()

    def test_success_records_combined_output_and_receipt(self):
        for stage in ARTIFACTS:
            with self.subTest(stage=stage):
                self.calls.unlink(missing_ok=True)
                result = self.run_stage(stage)
                self.assertEqual(0, result.returncode, result.stderr)
                self.assertEqual(1, len(self.records()))
                self.assertEqual(self.repo.resolve(), Path(self.records()[0]["cwd"]).resolve())
                self.assertEqual("0\n", self.receipt(stage).read_text())
                self.assertIn("fixture stdout\nfixture stderr\n", self.log(stage).read_text())

    def test_failure_returns_original_exit_and_logs_both_streams_without_retry(self):
        self.env["FIXTURE_EXIT"] = "37"
        result = self.run_stage("managed")
        self.assertEqual(37, result.returncode, result.stderr)
        self.assertEqual("37\n", self.receipt("managed").read_text())
        self.assertIn("fixture stdout\nfixture stderr\n", self.log("managed").read_text())
        self.assertIn("Validation exited 37\n", self.log("managed").read_text())
        self.assertEqual(1, len(self.records()))
        self.assertFalse(self.log("tests").exists())

    def test_cli_preserves_runner_failure_without_launching_a_process(self):
        exit_code, runner = self.run_cli("managed", runner_exit=37)
        self.assertEqual(37, exit_code)
        runner.assert_called_once()
        self.assertEqual([], self.records())

    def test_restore_failure_stops_before_native_build(self):
        self.env["FIXTURE_RESTORE_EXIT"] = "41"
        result = self.run_stage("native", ["tool", "restore"], ["cake"])
        self.assertEqual(41, result.returncode, result.stderr)
        self.assertEqual("41\n", self.receipt("native").read_text())
        self.assertEqual([["tool", "restore"]], [call["args"] for call in self.records()])
        self.assertFalse(self.log("managed").exists())
        self.assertFalse(self.log("tests").exists())

    def test_native_build_failure_records_cake_status_and_stops(self):
        self.env["FIXTURE_EXIT"] = "53"
        result = self.run_stage("native", ["tool", "restore"], ["cake"])
        self.assertEqual(53, result.returncode, result.stderr)
        self.assertEqual("53\n", self.receipt("native").read_text())
        self.assertEqual(2, len(self.records()))
        self.assertFalse(self.log("managed").exists())

    def test_stale_success_receipts_are_invalidated_before_launch(self):
        for stage, remaining in (
            ("native", []),
            ("managed", ["native-build-exit-code.txt"]),
            ("tests", ["managed-build-exit-code.txt", "native-build-exit-code.txt"]),
        ):
            with self.subTest(stage=stage):
                self.calls.unlink(missing_ok=True)
                for name in ARTIFACTS:
                    self.receipt(name).write_text("0\n")
                self.env["FIXTURE_EXIT"] = "29"
                self.assertEqual(29, self.run_stage(stage).returncode)
                self.assertEqual(remaining, self.records()[0]["receipts"])
                self.assertEqual("29\n", self.receipt(stage).read_text())
                later = list(ARTIFACTS)[list(ARTIFACTS).index(stage) + 1:]
                for name in later:
                    self.assertFalse(self.receipt(name).exists())

    def test_missing_executable_records_failure_and_removes_stale_success(self):
        missing = self.bin / "missing-dotnet.exe"
        self.receipt("managed").write_text("0\n")
        self.receipt("tests").write_text("0\n")
        result = self.run_commands("managed", [[str(missing)]])
        self.assertEqual(127, result.returncode)
        self.assertEqual("127\n", self.receipt("managed").read_text())
        self.assertIn(f"Unable to launch {missing}:", self.log("managed").read_text())
        self.assertIn(f"Unable to launch {missing}:", result.stderr)
        self.assertFalse(self.receipt("tests").exists())
        self.assertEqual([], self.records())

    def test_invalid_executable_records_launch_failure(self):
        invalid = self.bin / "invalid-dotnet.exe"
        invalid.write_text("not an executable\n", encoding="utf-8")
        invalid.chmod(0o755)
        result = self.run_commands("tests", [[str(invalid)]])
        self.assertEqual(126, result.returncode)
        self.assertEqual("126\n", self.receipt("tests").read_text())
        self.assertIn(f"Unable to launch {invalid}:", self.log("tests").read_text())

    def test_signal_is_recorded_as_shell_failure_status(self):
        with patch(
            "validation_common.subprocess.run",
            return_value=subprocess.CompletedProcess([], -15),
        ) as child:
            result = self.run_stage("managed")
        child.assert_called_once()
        self.assertEqual(143, result.returncode)
        self.assertEqual("143\n", self.receipt("managed").read_text())

    def test_each_stage_archives_failed_or_interrupted_output_on_rerun(self):
        for stage, receipt in ((stage, receipt) for stage in ARTIFACTS for receipt in ("17\n", None)):
            with self.subTest(stage=stage, receipt=receipt):
                previous = f"previous failed output: {receipt}"
                self.log(stage).write_text(previous)
                self.receipt(stage).unlink(missing_ok=True)
                if receipt is not None:
                    self.receipt(stage).write_text(receipt)
                pattern = f"{self.log(stage).stem}-attempt-*{self.log(stage).suffix}"
                archives_before = set(self.artifacts.glob(pattern))
                result = self.run_stage(stage)
                self.assertEqual(0, result.returncode, result.stderr)
                added = set(self.artifacts.glob(pattern)) - archives_before
                self.assertEqual(1, len(added))
                self.assertEqual(previous, added.pop().read_text())
                self.assertEqual("0\n", self.receipt(stage).read_text())

    def test_explicit_artifact_directory_takes_precedence(self):
        other = self.root / "explicit artifacts"
        exit_code, runner = self.run_cli("managed", "--artifact-dir", str(other))
        self.assertEqual(0, exit_code)
        self.assertEqual(other, runner.call_args.args[2].artifact_dir)
        self.assertEqual([], self.records())

    def test_artifact_directory_environment_fallback_is_supported(self):
        self.env.pop("SKIA_SYNC_ARTIFACT_DIR")
        self.env["ARTIFACT_DIR"] = str(self.artifacts)
        exit_code, runner = self.run_cli("managed")
        self.assertEqual(0, exit_code)
        self.assertIsNone(runner.call_args.args[2].artifact_dir)
        self.assertEqual([], self.records())

    def test_local_invocation_defaults_to_temporary_artifact_directory(self):
        self.env.pop("SKIA_SYNC_ARTIFACT_DIR")
        self.env["TMPDIR"] = str(self.root)
        with patch.dict(os.environ, self.env, clear=True):
            parser = argument_parser("Local validation")
            args = parser.parse_args(["--repo-root", str(self.repo)])
            repo_root, artifact_dir = validation_paths(parser, args)
            self.assertEqual(self.repo.resolve(), repo_root)
            self.assertEqual((self.root / "skia-sync-agent").resolve(), artifact_dir)
        self.assertEqual([], self.records())


class PushValidationPreflightTests(ValidationFixture):
    def setUp(self):
        super().setUp()
        self.bash = shutil.which("bash")
        if not self.bash:
            self.skipTest("Bash is not available for the shipping preflight")
        self.env.pop("BASH_ENV", None)
        self.env.pop("ENV", None)
        self.env.update({
            "SKIA_SYNC_TARGET": "152",
            "SKIA_SYNC_CURRENT": "151",
            "SKIA_SYNC_UPSTREAM_REF": "chrome/m152",
            "SKIA_SYNC_IS_RELEASE": "false",
            "SKIA_SYNC_BASE_BRANCH": "main",
            "SKIA_SYNC_SKIA_BASE_BRANCH": "skiasharp",
            "SKIA_SYNC_SKIA_BASE_SHA": "fork-base",
            "SKIA_SYNC_HEAD_BRANCH": "skia-sync/m152",
            "SKIA_SYNC_BASE_UPSTREAM_SHA": "old-upstream",
            "SKIA_SYNC_TARGET_UPSTREAM_SHA": "new-upstream",
            "GITHUB_WORKSPACE": str(self.repo),
            "GH_TOKEN": "fixture-not-a-real-token",
        })
        for name in (
            "skia-sync-skia-summary.md", "skia-sync-skiasharp-summary.md",
            "skia-breaking-change-analysis.md", "skia-validation-review.md",
            "skia-dependency-decisions.md", "skia-dependency-changes.json",
            "skia-fork-patch-audit.md",
        ):
            (self.artifacts / name).write_text("fixture report\n")
        self.reset_validation()

    def reset_validation(self):
        for stage in ARTIFACTS:
            self.log(stage).write_text("fixture combined output\n")
            self.receipt(stage).write_text("0\n")

    def run_push(self):
        # Shell builtins and an isolated PATH keep real git/gh out of these fixtures.
        return subprocess.run(
            [
                self.bash, "--noprofile", "--norc", "-c", """
git() { printf '%s\\0' git "$@" >>"$FIXTURE_CALLS"; return 91; }
gh() { printf '%s\\0' gh "$@" >>"$FIXTURE_CALLS"; return 92; }
tr() {
  [[ "$#" -eq 2 && "$1" == -d && "$2" == '[:space:]' ]] || return 93
  local line
  while IFS= read -r line || [[ -n "$line" ]]; do
    printf '%s' "${line//[[:space:]]/}"
  done
}
readonly -f git gh tr
source "$1"
""",
                "preflight-fixture", PUSH_SCRIPT.as_posix(),
            ],
            cwd=self.repo, env=self.env,
            capture_output=True, text=True, check=False,
        )

    def test_missing_empty_failed_or_malformed_evidence_blocks_before_git_or_gh(self):
        for stage in ARTIFACTS:
            for problem in ("missing", "empty", "failed", "malformed", "no-log", "empty-log"):
                with self.subTest(stage=stage, problem=problem):
                    self.reset_validation()
                    if problem == "missing":
                        self.receipt(stage).unlink()
                    elif problem == "no-log":
                        self.log(stage).unlink()
                    elif problem == "empty-log":
                        self.log(stage).write_text("")
                    else:
                        self.receipt(stage).write_text({
                            "empty": "", "failed": "37\n", "malformed": "success\n",
                        }[problem])
                    result = self.run_push()
                    self.assertEqual(1, result.returncode, result.stderr)
                    self.assertIn("::error::", result.stdout)
                    self.assertFalse(self.calls.exists())

    def test_green_receipts_reach_read_only_preflight_not_remote_mutation(self):
        result = self.run_push()
        self.assertEqual(91, result.returncode, result.stderr)
        self.assertEqual([
            "git", "-C", str(self.repo), "show", "skia-sync/m152:cgmanifest.json", "",
        ], self.calls.read_bytes().decode("utf-8").split("\0"))


if __name__ == "__main__":
    unittest.main()
