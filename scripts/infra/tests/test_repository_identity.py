from __future__ import annotations

import importlib.util
import json
import os
from pathlib import Path
import shutil
import tempfile
import unittest


ROOT = Path(__file__).resolve().parents[3]
MODULE_PATH = ROOT / "scripts" / "infra" / "repository_identity.py"
SPEC = importlib.util.spec_from_file_location("repository_identity", MODULE_PATH)
IDENTITY = importlib.util.module_from_spec(SPEC)
assert SPEC.loader is not None
SPEC.loader.exec_module(IDENTITY)


class RepositoryIdentityTests(unittest.TestCase):
    def test_normalizes_supported_github_identities(self) -> None:
        for value in (
            "dotnet/SkiaSharp",
            "https://github.com/dotnet/SkiaSharp",
            "https://github.com/dotnet/SkiaSharp.git",
            "git://github.com/dotnet/SkiaSharp.git",
            "git@github.com:dotnet/SkiaSharp.git",
            "ssh://git@github.com/dotnet/SkiaSharp.git",
        ):
            with self.subTest(value=value):
                self.assertEqual(
                    "dotnet/SkiaSharp",
                    IDENTITY.normalize_github_repository(value),
                )

    def test_rejects_non_github_and_ambiguous_identities(self) -> None:
        for value in ("SkiaSharp", "https://example.test/dotnet/SkiaSharp", ""):
            with self.subTest(value=value):
                with self.assertRaises(IDENTITY.IdentityError):
                    IDENTITY.normalize_github_repository(value)

    def test_current_repository_precedence(self) -> None:
        config = {"offlineRepository": "fallback/SkiaSharp"}
        self.assertEqual(
            "explicit/SkiaSharp",
            IDENTITY.resolve_current_repository(
                "explicit/SkiaSharp",
                environ={"GITHUB_REPOSITORY": "runtime/SkiaSharp"},
                config=config,
            ),
        )
        self.assertEqual(
            "runtime/SkiaSharp",
            IDENTITY.resolve_current_repository(
                environ={"GITHUB_REPOSITORY": "runtime/SkiaSharp"},
                config=config,
            ),
        )
        self.assertEqual(
            "fallback/SkiaSharp",
            IDENTITY.resolve_current_repository(environ={}, config=config),
        )

    def test_resolves_paired_repositories_for_both_organizations(self) -> None:
        for owner in ("mono", "dotnet"):
            with self.subTest(owner=owner), tempfile.TemporaryDirectory() as directory:
                root = Path(directory)
                (root / ".gitmodules").write_text(
                    '[submodule "externals/skia"]\n'
                    "\tpath = externals/skia\n"
                    f"\turl = git@github.com:{owner}/skia.git\n"
                    '[submodule "docs"]\n'
                    "\tpath = docs\n"
                    f"\turl = https://github.com/{owner}/SkiaSharp-API-docs\n",
                    encoding="utf-8",
                )
                config_path = root / "identity.json"
                config_path.write_text(
                    json.dumps(
                        {
                            "canonicalRepositoryId": 52293126,
                            "offlineRepository": "fallback/SkiaSharp",
                            "upstreamSkiaRepository": "google/skia",
                            "publicSiteBaseUrl": "https://pages.example/SkiaSharp",
                            "repositoryKey": "github-52293126",
                            "legacyRepositoryKeys": ["legacy-SkiaSharp"],
                            "skiaRepositoryKey": "github-52292286",
                            "legacySkiaRepositoryKeys": ["legacy-skia"],
                        }
                    ),
                    encoding="utf-8",
                )

                resolved = IDENTITY.resolve_identity(
                    root,
                    repository=f"{owner}/SkiaSharp",
                    config_path=config_path,
                )

                self.assertEqual(f"{owner}/SkiaSharp", resolved["repository"])
                self.assertEqual(f"{owner}/skia", resolved["skiaRepository"])
                self.assertEqual(
                    f"https://github.com/{owner}/skia.git",
                    resolved["skiaGitUrl"],
                )
                self.assertEqual(
                    f"{owner}/SkiaSharp-API-docs",
                    resolved["docsRepository"],
                )
                self.assertEqual("google/skia", resolved["upstreamSkiaRepository"])
                self.assertEqual("github-52293126", resolved["repositoryKey"])
                self.assertEqual("github-52292286", resolved["skiaRepositoryKey"])

    def test_manifest_requires_exactly_one_matching_skia_registration(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            identity = {"skiaGitUrl": "https://github.com/dotnet/skia.git"}
            manifest_path = root / "cgmanifest.json"
            manifest_path.write_text(
                json.dumps(
                    {
                        "registrations": [
                            {
                                "component": {
                                    "git": {
                                        "repositoryUrl": identity["skiaGitUrl"],
                                        "commitHash": "0" * 40,
                                    }
                                }
                            }
                        ]
                    }
                ),
                encoding="utf-8",
            )
            IDENTITY.validate_manifest(root, identity)

            manifest_path.write_text(
                json.dumps({"registrations": []}),
                encoding="utf-8",
            )
            with self.assertRaises(IDENTITY.IdentityError):
                IDENTITY.validate_manifest(root, identity)

    def test_renders_current_site_repository_links(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            site = Path(directory)
            page = site / "index.html"
            historical = site / "docs" / "releases" / "4.151.0.html"
            historical.parent.mkdir(parents=True)
            historical.write_text(
                "https://github.com/mono/SkiaSharp/releases/tag/v4.151.0",
                encoding="utf-8",
            )
            performance = site / "perf" / "index.html"
            performance.parent.mkdir()
            performance.write_text(
                "https://raw.githubusercontent.com/mono/SkiaSharp/aw-data/",
                encoding="utf-8",
            )
            page.write_text(
                "https://github.com/mono/SkiaSharp/tree/main "
                "mono/SkiaSharp "
                "https://github.com/mono/SkiaSharp-API-docs/issues "
                "https://raw.githubusercontent.com/mono/SkiaSharp/aw-data/",
                encoding="utf-8",
            )
            count = IDENTITY.render_site_identity(
                site,
                {
                    "repository": "dotnet/SkiaSharp",
                    "repositoryUrl": "https://github.com/dotnet/SkiaSharp",
                    "docsRepository": "mono/SkiaSharp-API-docs",
                    "docsUrl": "https://github.com/mono/SkiaSharp-API-docs",
                },
            )
            self.assertEqual(2, count)
            self.assertEqual(
                "https://github.com/dotnet/SkiaSharp/tree/main "
                "dotnet/SkiaSharp "
                "https://github.com/mono/SkiaSharp-API-docs/issues "
                "https://raw.githubusercontent.com/dotnet/SkiaSharp/aw-data/",
                page.read_text(encoding="utf-8"),
            )
            self.assertEqual(
                "https://github.com/mono/SkiaSharp/releases/tag/v4.151.0",
                historical.read_text(encoding="utf-8"),
            )
            self.assertEqual(
                "https://raw.githubusercontent.com/dotnet/SkiaSharp/aw-data/",
                performance.read_text(encoding="utf-8"),
            )

    def test_real_site_rewrite_only_changes_repository_identities(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            site = Path(directory)
            source_files = {
                ROOT / "documentation/site/index.html": site / "index.html",
                ROOT / "documentation/site/404.html": site / "404.html",
                ROOT / "documentation/site/ai/index.html": site / "ai/index.html",
                ROOT / "documentation/site/ai/dashboard-data.json":
                    site / "ai/dashboard-data.json",
                ROOT / "scripts/infra/perf/templates/dashboard.html":
                    site / "perf/index.html",
            }
            for source, destination in source_files.items():
                destination.parent.mkdir(parents=True, exist_ok=True)
                shutil.copy2(source, destination)

            before = {
                path: path.read_text(encoding="utf-8")
                for path in source_files.values()
            }
            IDENTITY.render_site_identity(
                site,
                {
                    "repository": "dotnet/SkiaSharp",
                    "repositoryUrl": "https://github.com/dotnet/SkiaSharp",
                    "docsRepository": "dotnet/SkiaSharp-API-docs",
                    "docsUrl": "https://github.com/dotnet/SkiaSharp-API-docs",
                },
            )
            after = {
                path: path.read_text(encoding="utf-8")
                for path in source_files.values()
            }

            for path in source_files.values():
                self.assertEqual(
                    before[path].count("https://www.nuget.org/packages/SkiaSharp"),
                    after[path].count("https://www.nuget.org/packages/SkiaSharp"),
                )
                self.assertEqual(
                    before[path].count("https://learn.microsoft.com/"),
                    after[path].count("https://learn.microsoft.com/"),
                )
            self.assertEqual(before[site / "404.html"], after[site / "404.html"])
            self.assertIn(
                "https://raw.githubusercontent.com/dotnet/SkiaSharp/aw-data/",
                after[site / "perf/index.html"],
            )
            self.assertNotIn(
                "https://www.nuget.org/dotnet/SkiaSharp",
                "\n".join(after.values()),
            )

    def test_identity_scan_rejects_new_executable_old_owner_literal(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            subprocess = __import__("subprocess")
            subprocess.run(["git", "init", "--quiet"], cwd=root, check=True)
            script = root / "scripts" / "new-tool.sh"
            script.parent.mkdir()
            script.write_text(
                'gh issue view 1 --repo mono/SkiaSharp\n',
                encoding="utf-8",
            )
            self.assertEqual(
                ["scripts/new-tool.sh:1: gh issue view 1 --repo mono/SkiaSharp"],
                IDENTITY.scan_identity_drift(root),
            )

    def test_identity_scan_rejects_tibdex_backport(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            subprocess = __import__("subprocess")
            subprocess.run(["git", "init", "--quiet"], cwd=root, check=True)
            workflow = root / ".github" / "workflows" / "backport.yml"
            workflow.parent.mkdir(parents=True)
            workflow.write_text("uses: tibdex/backport@v2\n", encoding="utf-8")
            self.assertEqual(
                [
                    ".github/workflows/backport.yml: "
                    "forbidden executable tibdex/backport reference"
                ],
                IDENTITY.scan_identity_drift(root),
            )

    def test_identity_scan_catches_all_legacy_identity_shapes(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            subprocess = __import__("subprocess")
            subprocess.run(["git", "init", "--quiet"], cwd=root, check=True)
            script = root / "scripts" / "new-tool.py"
            script.parent.mkdir()
            script.write_text(
                "\n".join(
                    (
                        'url = "https://github.com/orgs/mono/projects/1"',
                        'cache = "repos/mono-SkiaSharp"',
                        'skia_cache = "repos/mono-skia"',
                        'owners = "repository_owners: mono,dotnet"',
                    )
                ),
                encoding="utf-8",
            )
            violations = IDENTITY.scan_identity_drift(root)
            self.assertEqual(4, len(violations))


if __name__ == "__main__":
    unittest.main()
