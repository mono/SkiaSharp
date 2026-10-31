from __future__ import annotations

import importlib.util
import tempfile
import unittest
from datetime import datetime, timezone
from pathlib import Path
from unittest.mock import patch


class GenerationDateTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        path = Path(__file__).resolve().parents[2] / "release-notes-render.py"
        spec = importlib.util.spec_from_file_location("_rn_generation_date_tests", path)
        cls.renderer = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(cls.renderer)

    def stamp(self, text, previous_text="", day=31):
        with patch.object(self.renderer, "datetime") as clock:
            clock.now.return_value = datetime(2026, 10, day, tzinfo=timezone.utc)
            result = self.renderer.with_generation_date(text, previous_text)
            clock.now.assert_called_once_with(timezone.utc)
            return result

    def test_new_and_legacy_pages_get_utc_date(self):
        text = "# Version 4.156.0\n\nNotes.\n"
        for previous in ("", text):
            with self.subTest(previous=previous):
                result = self.stamp(text, previous)
                self.assertEqual(
                    result,
                    text + '\n---\n\n<small class="text-body-secondary">'
                    'Release notes generated on 2026-10-31 (UTC).</small>\n',
                )

    def test_unchanged_page_keeps_date_without_reading_clock(self):
        text = "# Version 4.156.0\n\nNotes.\n"
        previous = self.stamp(text, day=30)
        with patch.object(self.renderer, "datetime") as clock:
            self.assertEqual(
                self.renderer.with_generation_date(text, previous), previous
            )
            clock.now.assert_not_called()

    def test_changed_page_replaces_date_without_duplicating_footer(self):
        previous = self.stamp("# Old notes\n", day=30)
        result = self.stamp("# Updated notes\n", previous)
        self.assertTrue(result.startswith("# Updated notes\n"))
        self.assertNotIn("2026-10-30", result)
        self.assertEqual(result.count("Release notes generated on"), 1)

    def test_writing_page_preserves_date_on_unchanged_rerun(self):
        with tempfile.TemporaryDirectory() as directory:
            page = Path(directory) / "4.156.0.md"
            self.renderer.write_page(page, "# Notes\n")
            first = page.read_text()
            with patch.object(self.renderer, "datetime") as clock:
                self.renderer.write_page(page, "# Notes\n")
                clock.now.assert_not_called()
            self.assertEqual(page.read_text(), first)


if __name__ == "__main__":
    unittest.main()
