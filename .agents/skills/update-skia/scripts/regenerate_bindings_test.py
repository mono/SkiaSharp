import unittest

from regenerate_bindings import (
    PROJECTS,
    added_internal_functions,
    preserve_function_region_order,
    select_projects,
)


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

    def test_preserves_existing_function_region_order_and_content_changes(self) -> None:
        original = (
            "prefix\n"
            "\t\t#region second.h\nold second\n\t\t#endregion\n"
            "\t\t#region first.h\nold first\n\t\t#endregion\n"
            "suffix\n"
        )
        generated = (
            "new prefix\n"
            "\t\t#region first.h\nnew first\n\t\t#endregion\n"
            "\t\t#region second.h\nnew second\n\t\t#endregion\n"
            "new suffix\n"
        )

        result = preserve_function_region_order(original, generated)

        self.assertLess(result.index("#region second.h"), result.index("#region first.h"))
        self.assertIn("new first", result)
        self.assertIn("new second", result)
        self.assertTrue(result.startswith("new prefix"))
        self.assertTrue(result.endswith("new suffix\n"))

    def test_drops_removed_regions_and_appends_new_regions_deterministically(self) -> None:
        original = (
            "prefix\n"
            "\t\t#region existing.h\nold\n\t\t#endregion\n"
            "\t\t#region removed.h\nold\n\t\t#endregion\n"
            "suffix\n"
        )
        generated = (
            "prefix\n"
            "\t\t#region z-new.h\nnew z\n\t\t#endregion\n"
            "\t\t#region existing.h\nnew existing\n\t\t#endregion\n"
            "\t\t#region a-new.h\nnew a\n\t\t#endregion\n"
            "suffix\n"
        )

        result = preserve_function_region_order(original, generated)

        self.assertNotIn("removed.h", result)
        self.assertLess(result.index("existing.h"), result.index("a-new.h"))
        self.assertLess(result.index("a-new.h"), result.index("z-new.h"))


if __name__ == "__main__":
    unittest.main()
