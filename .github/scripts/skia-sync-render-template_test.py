#!/usr/bin/env python3

import json
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path


SCRIPT = Path(__file__).with_name("skia-sync-render-template.py")


class RenderTemplateTests(unittest.TestCase):
    def render(self, template: str, values: object) -> subprocess.CompletedProcess[str]:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            template_path = root / "template.md"
            values_path = root / "values.json"
            output_path = root / "output.md"
            template_path.write_text(template, encoding="utf-8")
            values_path.write_text(json.dumps(values), encoding="utf-8")

            result = subprocess.run(
                [sys.executable, SCRIPT, template_path, values_path, output_path],
                capture_output=True,
                check=False,
                text=True,
            )
            result.rendered = output_path.read_text(encoding="utf-8") if output_path.exists() else ""
            return result

    def test_renders_placeholders_without_reprocessing_inserted_content(self) -> None:
        result = self.render(
            "Title: {{TITLE}}\n{{REPORT}}\n",
            {"TITLE": "Sync", "REPORT": "Literal {{VERSION}} example"},
        )

        self.assertEqual(0, result.returncode, result.stderr)
        self.assertEqual("Title: Sync\nLiteral {{VERSION}} example\n", result.rendered)

    def test_rejects_missing_template_value(self) -> None:
        result = self.render("{{TITLE}} {{REPORT}}\n", {"TITLE": "Sync"})

        self.assertNotEqual(0, result.returncode)
        self.assertIn("missing template values: REPORT", result.stderr)

    def test_rejects_non_string_template_value(self) -> None:
        result = self.render("{{TITLE}}\n", {"TITLE": 152})

        self.assertNotEqual(0, result.returncode)
        self.assertIn("template value TITLE must be a string", result.stderr)


if __name__ == "__main__":
    unittest.main()
