from __future__ import annotations

import importlib.util
import re
import tempfile
import unittest
from pathlib import Path
from unittest import mock

_DOCS_DIR = Path(__file__).resolve().parents[2]


def _load_renderer():
    spec = importlib.util.spec_from_file_location("_rn_pr_body_renderer", _DOCS_DIR / "release-notes-render.py")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


class PrBodyTests(unittest.TestCase):
    def setUp(self):
        self.module = _load_renderer()

    def test_body_preserves_template_structure_without_instructional_comments(self):
        template = (_DOCS_DIR.parents[2] / ".github" / "pull_request_template.md").read_text()
        body = self.module.render_pr_body("/tmp/gh-aw/release-notes-pr-body.md")

        self.assertEqual(re.findall(r"^## .+", body, re.MULTILINE),
                         re.findall(r"^## .+", template, re.MULTILINE))
        self.assertEqual(re.findall(r"^- \[[ x]\] (.+)", body, re.MULTILINE),
                         re.findall(r"^- \[[ x]\] (.+)", template, re.MULTILINE))
        self.assertNotIn("<!--", body)
        self.assertNotIn("-->", body)
        self.assertNotIn("SKPath.TryGetTightBounds", body)
        self.assertNotIn("Fixes #", body)
        self.assertIn("- [x] Documentation or samples", body)
        self.assertIn("Passed: `python3 scripts/infra/docs/release-notes-render.py --all --pr-body", body)

    def test_only_successful_full_render_writes_body(self):
        for failures in (0, 1):
            with self.subTest(failures=failures), tempfile.TemporaryDirectory() as tmp:
                output = Path(tmp) / "pr-body.md"
                with mock.patch.object(self.module, "render_all", return_value=failures) as render:
                    result = self.module.main(["renderer", "--all", "--pr-body", str(output)])
                render.assert_called_once_with()
                self.assertEqual(result, failures)
                self.assertEqual(output.exists(), failures == 0)

    def test_pr_body_requires_full_render_and_an_output_path(self):
        with mock.patch.object(self.module, "render_all") as render:
            self.assertEqual(self.module.main(["renderer", "--pr-body", "/tmp/body.md"]), 2)
            self.assertEqual(self.module.main(["renderer", "--all", "--pr-body"]), 2)
            render.assert_not_called()
