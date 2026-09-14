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


if __name__ == "__main__":
    unittest.main()
