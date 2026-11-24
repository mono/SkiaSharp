from __future__ import annotations

import importlib.util
import unittest
from pathlib import Path


_DOCS_DIR = Path(__file__).resolve().parents[2]


def _load_renderer():
    path = _DOCS_DIR / "release-notes-render.py"
    spec = importlib.util.spec_from_file_location("_rn_banner_tests", str(path))
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


class PreviewBannerRenderTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.renderer = _load_renderer()

    def test_legacy_page_uses_exact_preview_tag_instead_of_synthetic_url(self):
        data = {
            "banner": {
                "kind": "preview",
                "preview_nuget_url": (
                    "https://www.nuget.org/packages/SkiaSharp/1.49.0-preview"
                ),
            },
            "previews": [{"key": "v1.49.0-preview1"}],
        }

        line = self.renderer.banner_line(data, {"theme": "Legacy preview"})

        self.assertIn(
            "https://www.nuget.org/packages/SkiaSharp/1.49.0-preview1",
            line,
        )
        self.assertNotIn("SkiaSharp/1.49.0-preview)", line)


class HarfBuzzSummaryRenderTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.renderer = _load_renderer()

    def test_reviewed_package_changes_render_without_collected_harfbuzz_prs(self):
        summary = "HarfBuzzSharp 14.2.1.102 adds span-based tag parsing."
        data = {
            "version": "4.151.2",
            "harfbuzz": {"version": "14.2.1.102", "prs": []},
        }

        text = self.renderer.render(data, {"harfbuzz_summary": summary})

        self.assertIn(summary, text)
        self.assertNotIn(self.renderer.NO_CHANGES_BODY, text)

    def test_empty_summary_keeps_the_no_changes_body(self):
        data = {
            "version": "4.151.2",
            "harfbuzz": {"version": "14.2.1.102", "prs": []},
        }

        text = self.renderer.render(data, {"harfbuzz_summary": None})

        self.assertIn(self.renderer.NO_CHANGES_BODY, text)


if __name__ == "__main__":
    unittest.main()
