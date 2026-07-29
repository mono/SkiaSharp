import unittest

from regenerate_bindings import PROJECTS, added_internal_functions, select_projects


class RegenerateBindingsTests(unittest.TestCase):
    def test_selects_all_projects_by_default(self) -> None:
        self.assertEqual(PROJECTS, select_projects(None))

    def test_selects_config_by_file_name(self) -> None:
        selected = select_projects("binding/libSkiaSharp.json")
        self.assertEqual(("libSkiaSharp.json",), tuple(project[0] for project in selected))

    def test_rejects_unknown_config(self) -> None:
        with self.assertRaisesRegex(ValueError, "Unknown config"):
            select_projects("missing.json")

    def test_lists_only_added_internal_functions(self) -> None:
        diff = (
            "+++ b/file.cs\n"
            "+ internal static void sk_new_api();\n"
            "+ public void Wrapper() {}\n"
            "- internal static void sk_old_api();\n"
        )
        self.assertEqual(
            ["internal static void sk_new_api();"],
            added_internal_functions(diff),
        )


if __name__ == "__main__":
    unittest.main()
