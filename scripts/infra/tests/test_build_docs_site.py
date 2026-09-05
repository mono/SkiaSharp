from __future__ import annotations

import importlib.util
from pathlib import Path
import tempfile
import unittest


ROOT = Path(__file__).resolve().parents[3]
MODULE_PATH = ROOT / "scripts" / "build-docs-site.py"
SPEC = importlib.util.spec_from_file_location("build_docs_site", MODULE_PATH)
BUILD_DOCS = importlib.util.module_from_spec(SPEC)
assert SPEC.loader is not None
SPEC.loader.exec_module(BUILD_DOCS)


class BuildDocsSiteTests(unittest.TestCase):
    def test_normalizes_staging_site_base_url(self) -> None:
        self.assertEqual(
            "https://dotnet.github.io/SkiaSharp/staging/4988",
            BUILD_DOCS.normalize_public_site_base_url(
                "https://dotnet.github.io/SkiaSharp/staging/4988/"
            ),
        )
        for value in (
            "http://dotnet.github.io/SkiaSharp",
            "https://dotnet.github.io/SkiaSharp?preview=1",
            " https://dotnet.github.io/SkiaSharp",
        ):
            with self.subTest(value=value):
                with self.assertRaises(ValueError):
                    BUILD_DOCS.normalize_public_site_base_url(value)

    def test_prepares_temporary_content_and_contribution_metadata(self) -> None:
        config = {
            "build": {
                "content": [
                    {
                        "files": ["**/*.md", "**/*.yml"],
                        "exclude": ["**/*.notes.md"],
                    }
                ],
                "globalMetadata": {
                    "_gitContribute": {
                        "repo": "https://github.com/mono/SkiaSharp",
                    }
                },
            }
        }
        temporary_source = Path(".identity-preview.test")

        BUILD_DOCS.prepare_config(
            config,
            temporary_source,
            {"repositoryUrl": "https://github.com/dotnet/SkiaSharp"},
        )

        self.assertEqual(
            "https://github.com/dotnet/SkiaSharp",
            config["build"]["globalMetadata"]["_gitContribute"]["repo"],
        )
        self.assertEqual(
            {
                "src": ".identity-preview.test",
                "files": ["TOC.yml"],
            },
            config["build"]["content"][1],
        )
        self.assertIn("TOC.yml", config["build"]["content"][0]["exclude"])
        self.assertIn(
            ".identity-preview.*/**",
            config["build"]["content"][0]["exclude"],
        )

    def test_rebases_only_relative_toc_links(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "TOC.yml"
            path.write_text(
                "- name: Guides\n"
                "  href: guides/\n"
                "- name: Home\n"
                '  href: "https://docs.example/SkiaSharp/"\n'
                "- name: Root\n"
                "  href: ~/index.md\n",
                encoding="utf-8",
            )

            BUILD_DOCS.rebase_toc_relative_links(path)

            self.assertEqual(
                "- name: Guides\n"
                "  href: ../guides/\n"
                "- name: Home\n"
                '  href: "https://docs.example/SkiaSharp/"\n'
                "- name: Root\n"
                "  href: ~/index.md\n",
                path.read_text(encoding="utf-8"),
            )

    def test_finds_unresolved_placeholders_in_text_outputs(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            clean = root / "clean.html"
            unresolved = root / "unresolved.js"
            binary = root / "assembly.dll"
            clean.write_text("dotnet/SkiaSharp", encoding="utf-8")
            unresolved.write_text("{{Repository}}", encoding="utf-8")
            binary.write_bytes(b"\xff{{Repository}}")

            self.assertEqual(
                [unresolved],
                BUILD_DOCS.find_unresolved_placeholders(root),
            )


if __name__ == "__main__":
    unittest.main()
