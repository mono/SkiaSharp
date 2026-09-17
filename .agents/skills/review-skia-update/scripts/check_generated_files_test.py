from __future__ import annotations

import importlib.util
import subprocess
import tempfile
from pathlib import Path
from unittest import TestCase, main, mock


SCRIPT_PATH = Path(__file__).with_name("check_generated_files.py")
SPEC = importlib.util.spec_from_file_location("check_generated_files", SCRIPT_PATH)
CHECK_GENERATED_FILES = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(CHECK_GENERATED_FILES)


class GeneratedFilesTests(TestCase):
    def test_accepts_string_paths_and_preserves_split_tree_documentation(self):
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = Path(temporary_directory)
            repo_root = root / "repo"
            output_dir = root / "output"

            for _, _, project, _ in CHECK_GENERATED_FILES.PROJECTS:
                generated = repo_root / "binding" / project / "Generated"
                generated.mkdir(parents=True)
                (generated / "Documented.generated.cs").write_text("/// preserved")

            def run_generator(args, **kwargs):
                if "--output" in args:
                    output_index = args.index("--output")
                    generated = Path(args[output_index + 1])
                    self.assertEqual(
                        "/// preserved",
                        (generated / "Documented.generated.cs").read_text(),
                    )
                return subprocess.CompletedProcess(args, 0)

            with mock.patch.object(
                CHECK_GENERATED_FILES.subprocess,
                "run",
                side_effect=run_generator,
            ):
                result = CHECK_GENERATED_FILES.run_check(
                    repo_root=str(repo_root),
                    output_dir=str(output_dir),
                )

            self.assertEqual("PASS", result["status"])
            self.assertEqual(
                "generator-output.log",
                Path(result["generatorLog"]).name,
            )

    def test_preserves_legacy_single_file_documentation(self):
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = Path(temporary_directory)
            repo_root = root / "repo"
            output_dir = root / "output"

            for _, _, project, legacy_file in CHECK_GENERATED_FILES.PROJECTS:
                expected = repo_root / "binding" / project / legacy_file
                expected.parent.mkdir(parents=True)
                expected.write_text("/// preserved")

            def run_generator(args, **kwargs):
                if "--output" in args:
                    output_index = args.index("--output")
                    generated = Path(args[output_index + 1])
                    self.assertEqual("/// preserved", generated.read_text())
                return subprocess.CompletedProcess(args, 0)

            with mock.patch.object(
                CHECK_GENERATED_FILES.subprocess,
                "run",
                side_effect=run_generator,
            ):
                result = CHECK_GENERATED_FILES.run_check(
                    repo_root=repo_root,
                    output_dir=output_dir,
                )

            self.assertEqual("PASS", result["status"])


if __name__ == "__main__":
    main()
