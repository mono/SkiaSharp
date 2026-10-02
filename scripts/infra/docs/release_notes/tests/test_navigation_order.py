from __future__ import annotations

import importlib.util
import unittest
from pathlib import Path
from unittest import mock


_DOCS_DIR = Path(__file__).resolve().parents[2]


def _load_renderer():
    path = _DOCS_DIR / "release-notes-render.py"
    spec = importlib.util.spec_from_file_location("_rn_navigation_tests", str(path))
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


class ReleaseNotesNavigationOrderTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.renderer = _load_renderer()

    def setUp(self):
        self.support = {
            "stable": ["4.154"],
            "preview": ["4.152"],
            "supported": {"4.154", "4.152"},
            "channels": {"4.154": "Stable", "4.152": "Preview"},
        }
        self.support_config = mock.patch.object(
            self.renderer, "load_support_config", return_value=self.support)
        self.support_config.start()
        self.addCleanup(self.support_config.stop)
        self.timeline = mock.patch.object(
            self.renderer, "render_cadence_timeline", return_value=[])
        self.timeline.start()
        self.addCleanup(self.timeline.stop)

    def test_navigation_orders_unreleased_before_releases_for_each_line(self):
        versions = ["4.152.0", "4.154.0", "4.154.2", "4.154.1"]
        next_versions = ["4.152.1", "4.154.0"]

        toc = self.renderer.generate_toc(versions, next_versions)
        index = self.renderer.generate_index(versions, next_versions)

        expected_154 = [
            "4.154.0-unreleased",
            "4.154.2",
            "4.154.1",
            "4.154.0",
        ]
        expected_152 = ["4.152.1-unreleased", "4.152.0"]
        for output in (toc, index):
            positions = [output.index(
                self._page_token(output, version)) for version in expected_154]
            self.assertEqual(positions, sorted(positions))
            positions = [output.index(
                self._page_token(output, version)) for version in expected_152]
            self.assertEqual(positions, sorted(positions))

        self.assertIn("- **Version 4.154.x** — Stable", index)
        self.assertIn("- **Version 4.152.x** — Preview", index)
        self.assertIn("href: 4.154.0-unreleased.md", toc)
        self.assertIn("href: 4.152.1-unreleased.md", toc)
        self.assertEqual(
            toc, self.renderer.generate_toc(
                list(reversed(versions)), list(reversed(next_versions))))
        self.assertEqual(
            index, self.renderer.generate_index(
                list(reversed(versions)), list(reversed(next_versions))))

    def test_folded_toc_sections_use_the_same_order(self):
        toc = self.renderer.generate_toc(
            ["4.151.0", "2.88.0"], ["4.151.1", "2.88.0"])

        for first, second in (
                ("href: 4.151.1-unreleased.md", "href: 4.151.0.md"),
                ("href: 2.88.0-unreleased.md", "href: 2.88.0.md")):
            self.assertLess(toc.index(first), toc.index(second))

    def test_navigation_without_unreleased_is_semver_descending_and_deterministic(self):
        versions = ["4.154.0", "4.154.2", "4.154.1"]

        toc = self.renderer.generate_toc(versions, [])
        index = self.renderer.generate_index(versions, [])
        self.assertLess(toc.index("4.154.2"), toc.index("4.154.1"))
        self.assertLess(toc.index("4.154.1"), toc.index("4.154.0"))
        self.assertLess(index.index("4.154.2"), index.index("4.154.1"))
        self.assertLess(index.index("4.154.1"), index.index("4.154.0"))

        self.assertEqual(toc, self.renderer.generate_toc(list(reversed(versions)), []))
        self.assertEqual(index, self.renderer.generate_index(
            list(reversed(versions)), []))

    @staticmethod
    def _page_token(output, version):
        if "href:" in output:
            return "href: {}.md".format(version)
        if version.endswith("-unreleased"):
            return "[Version {} (Unreleased)]({}.md)".format(
                version.removesuffix("-unreleased"), version)
        return "[Version {}]({}.md)".format(version, version)


if __name__ == "__main__":
    unittest.main()
