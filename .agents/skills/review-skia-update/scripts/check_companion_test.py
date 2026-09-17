from __future__ import annotations

import importlib.util
from pathlib import Path
from unittest import TestCase, main, mock


SCRIPT_PATH = Path(__file__).with_name("check_companion.py")
SPEC = importlib.util.spec_from_file_location("check_companion", SCRIPT_PATH)
CHECK_COMPANION = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(CHECK_COMPANION)


class GeneratedDocumentationChangeTests(TestCase):
    def run_check(self, name_status: str, diff: str):
        calls = []

        def git_run(args, cwd):
            calls.append(args)
            if args[0] == "merge-base":
                return "base"
            if args[:2] == ["diff", "--name-status"]:
                return name_status
            return diff

        with mock.patch.object(CHECK_COMPANION, "git_run", side_effect=git_run):
            result = CHECK_COMPANION.run_check(
                repo_root="/repo",
                base_ref="origin/main",
                pr_ref="head",
                output_dir="",
            )
        return result, calls

    def test_includes_documentation_changes_for_all_generated_statuses(self):
        statuses = {
            "M\tbinding/Changed.generated.cs": "binding/Changed.generated.cs",
            "A\tbinding/Added.generated.cs": "binding/Added.generated.cs",
            "R100\tbinding/Old.generated.cs\tbinding/Renamed.generated.cs":
                "binding/Renamed.generated.cs",
            "C100\tbinding/Old.generated.cs\tbinding/Copied.generated.cs":
                "binding/Copied.generated.cs",
        }

        for name_status, path in statuses.items():
            with self.subTest(name_status=name_status):
                result, _ = self.run_check(name_status, "+/// <summary>Changed.</summary>\n")
                reviewed = result["added"] + result["changed"]
                self.assertEqual([path], [entry["path"] for entry in reviewed])

    def test_skips_generated_code_changes_without_documentation(self):
        result, _ = self.run_check(
            "M\tbinding/Changed.generated.cs",
            "+public static extern void Changed();\n",
        )

        self.assertEqual("PASS", result["status"])
        self.assertEqual(1, result["unchanged"])

    def test_uses_both_paths_when_reviewing_a_renamed_generated_file(self):
        result, calls = self.run_check(
            "R100\tbinding/Old.generated.cs\tbinding/Renamed.generated.cs",
            "+/// <summary>Renamed.</summary>\n",
        )

        self.assertEqual("binding/Renamed.generated.cs", result["changed"][0]["path"])
        diffs = [call for call in calls if call[:2] == ["diff", "base..head"]]
        self.assertTrue(diffs)
        self.assertTrue(all(call[-2:] == [
            "binding/Old.generated.cs",
            "binding/Renamed.generated.cs",
        ] for call in diffs))


if __name__ == "__main__":
    main()
