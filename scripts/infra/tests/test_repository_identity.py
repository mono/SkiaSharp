from __future__ import annotations

from contextlib import redirect_stderr, redirect_stdout
import importlib.util
import io
import json
from pathlib import Path
import tempfile
import unittest


ROOT = Path(__file__).resolve().parents[3]
MODULE_PATH = ROOT / "scripts" / "infra" / "repository_identity.py"
SPEC = importlib.util.spec_from_file_location("repository_identity", MODULE_PATH)
IDENTITY = importlib.util.module_from_spec(SPEC)
assert SPEC.loader is not None
SPEC.loader.exec_module(IDENTITY)

BASE_CONFIG = {
    "canonicalRepositoryId": 52293126,
    "offlineRepository": "mono/SkiaSharp",
    "upstreamSkiaRepository": "google/skia",
    "publicSiteBaseUrl": "https://mono.github.io/SkiaSharp",
    "skiaRepositoryKey": "github-52292286",
    "legacySkiaRepositoryKeys": ["mono-skia"],
}


class RepositoryIdentityTests(unittest.TestCase):
    def write_config(self, root: Path, **updates: object) -> Path:
        config = dict(BASE_CONFIG)
        config.update(updates)
        path = root / "repository-identity.json"
        path.write_text(json.dumps(config), encoding="utf-8")
        return path

    def write_gitmodules(
        self,
        root: Path,
        *,
        skia_url: str = "https://github.com/mono/skia.git",
        docs_url: str = "https://github.com/mono/SkiaSharp-API-docs",
    ) -> None:
        (root / ".gitmodules").write_text(
            '[submodule "externals/skia"]\n'
            "\tpath = externals/skia\n"
            f"\turl = {skia_url}\n"
            '[submodule "docs"]\n'
            "\tpath = docs\n"
            f"\turl = {docs_url}\n",
            encoding="utf-8",
        )

    def write_manifest(self, root: Path, *repository_urls: str) -> None:
        registrations = [
            {
                "component": {
                    "type": "git",
                    "git": {
                        "repositoryUrl": repository_url,
                        "commitHash": "0" * 40,
                    },
                }
            }
            for repository_url in repository_urls
        ]
        registrations.append(
            {
                "component": {
                    "type": "other",
                    "other": {
                        "name": "libpng",
                        "version": "1.0",
                    },
                }
            }
        )
        (root / "cgmanifest.json").write_text(
            json.dumps({"registrations": registrations}),
            encoding="utf-8",
        )

    def test_normalizes_supported_github_identities(self) -> None:
        for value, expected in (
            ("dotnet/SkiaSharp", "dotnet/SkiaSharp"),
            ("dotnet/SkiaSharp.git", "dotnet/SkiaSharp"),
            (
                "https://github.com/dotnet/SkiaSharp.git",
                "dotnet/SkiaSharp",
            ),
            ("https://github.com/dotnet/SkiaSharp/", "dotnet/SkiaSharp"),
            (
                "git://github.com/dotnet/SkiaSharp.git",
                "dotnet/SkiaSharp",
            ),
            ("git@github.com:dotnet/SkiaSharp.git", "dotnet/SkiaSharp"),
            (
                "ssh://git@github.com/dotnet/SkiaSharp.git",
                "dotnet/SkiaSharp",
            ),
            ("owner-name/repo.name_with-dots", "owner-name/repo.name_with-dots"),
            ("owner/.github", "owner/.github"),
            ("owner/_private", "owner/_private"),
        ):
            with self.subTest(value=value):
                self.assertEqual(
                    expected,
                    IDENTITY.normalize_github_repository(value),
                )

        self.assertEqual(
            "DoTnEt/SkIaShArP",
            IDENTITY.normalize_github_repository(
                "HTTPS://GITHUB.COM/DoTnEt/SkIaShArP.git"
            ),
        )

    def test_rejects_non_github_and_ambiguous_identities(self) -> None:
        for value in (
            "",
            " dotnet/SkiaSharp ",
            "SkiaSharp",
            "./repo",
            "../repo",
            "owner/.",
            "owner/..",
            "dotnet/",
            "dotnet/.git",
            "/SkiaSharp",
            "-owner/repo",
            "owner-/repo",
            "owner--name/repo",
            "owner_name/repo",
            "owner/repo name",
            "owner/repo\tname",
            "owner/repo\x00name",
            "https://example.test/dotnet/SkiaSharp",
            "https://github.com/dotnet",
            "https://github.com/dotnet/SkiaSharp/issues/1",
            "https://github.com/dotnet/SkiaSharp?ref=main",
            "https://github.com/dotnet/SkiaSharp#readme",
            "https://api.github.com/repos/dotnet/SkiaSharp",
        ):
            with self.subTest(value=value):
                with self.assertRaises(IDENTITY.IdentityError):
                    IDENTITY.normalize_github_repository(value)

    def test_loads_committed_config(self) -> None:
        config = IDENTITY.load_config()
        self.assertEqual(52293126, config["canonicalRepositoryId"])
        self.assertEqual("mono/SkiaSharp", config["offlineRepository"])
        self.assertEqual("google/skia", config["upstreamSkiaRepository"])
        self.assertEqual("github-52292286", config["skiaRepositoryKey"])
        self.assertEqual(["mono-skia"], config["legacySkiaRepositoryKeys"])

    def test_rejects_missing_and_malformed_config(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            with self.assertRaises(IDENTITY.IdentityError):
                IDENTITY.load_config(root / "missing.json")

            invalid_configs = (
                [],
                {key: value for key, value in BASE_CONFIG.items()
                 if key != "canonicalRepositoryId"},
                {**BASE_CONFIG, "canonicalRepositoryId": True},
                {**BASE_CONFIG, "offlineRepository": ""},
                {**BASE_CONFIG, "offlineRepository": "SkiaSharp"},
                {**BASE_CONFIG, "legacySkiaRepositoryKeys": []},
                {**BASE_CONFIG, "legacySkiaRepositoryKeys": [1]},
                {**BASE_CONFIG, "publicSiteBaseUrl": "http://example.test"},
                {
                    **BASE_CONFIG,
                    "publicSiteBaseUrl": "https://example.test/?preview=1",
                },
            )
            path = root / "identity.json"
            for config in invalid_configs:
                with self.subTest(config=config):
                    path.write_text(json.dumps(config), encoding="utf-8")
                    with self.assertRaises(IDENTITY.IdentityError):
                        IDENTITY.load_config(path)

            path.write_text("{", encoding="utf-8")
            with self.assertRaises(IDENTITY.IdentityError):
                IDENTITY.load_config(path)

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
        with self.assertRaises(IDENTITY.IdentityError):
            IDENTITY.resolve_current_repository(
                "",
                environ={"GITHUB_REPOSITORY": "runtime/SkiaSharp"},
                config=config,
            )
        with self.assertRaises(IDENTITY.IdentityError):
            IDENTITY.resolve_current_repository(
                environ={"GITHUB_REPOSITORY": "not-a-repository"},
                config=config,
            )

    def test_resolves_paired_repositories_for_both_organizations(self) -> None:
        for owner in ("mono", "dotnet"):
            with self.subTest(owner=owner), tempfile.TemporaryDirectory() as directory:
                root = Path(directory)
                self.write_gitmodules(
                    root,
                    skia_url=f"git@github.com:{owner}/skia.git",
                    docs_url=(
                        f"ssh://git@github.com/{owner}/"
                        "SkiaSharp-API-docs.git"
                    ),
                )
                config_path = self.write_config(root)
                identity = IDENTITY.resolve_identity(
                    root,
                    repository=f"https://github.com/{owner}/SkiaSharp.git",
                    config_path=config_path,
                )

                self.assertEqual(f"{owner}/SkiaSharp", identity["repository"])
                self.assertEqual(f"{owner}/skia", identity["skiaRepository"])
                self.assertEqual(
                    f"https://github.com/{owner}/skia.git",
                    identity["skiaGitUrl"],
                )
                self.assertEqual(
                    f"{owner}/SkiaSharp-API-docs",
                    identity["docsRepository"],
                )
                self.assertEqual(
                    "google/skia",
                    identity["upstreamSkiaRepository"],
                )
                self.assertEqual(
                    "github-52292286",
                    identity["skiaRepositoryKey"],
                )
                self.assertEqual(
                    ["mono-skia"],
                    identity["legacySkiaRepositoryKeys"],
                )

    def test_rejects_missing_and_malformed_submodule_configuration(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            with self.assertRaises(IDENTITY.IdentityError):
                IDENTITY.read_submodule_repository(root, "externals/skia")

            gitmodules = root / ".gitmodules"
            gitmodules.write_text("not a config file", encoding="utf-8")
            with self.assertRaises(IDENTITY.IdentityError):
                IDENTITY.read_submodule_repository(root, "externals/skia")

            gitmodules.write_text(
                '[submodule "docs"]\n\tpath = docs\n',
                encoding="utf-8",
            )
            with self.assertRaises(IDENTITY.IdentityError):
                IDENTITY.read_submodule_repository(root, "docs")
            with self.assertRaises(IDENTITY.IdentityError):
                IDENTITY.read_submodule_repository(root, "externals/skia")

            gitmodules.write_text(
                '[submodule "externals/skia"]\n'
                "\turl = https://example.test/mono/skia.git\n",
                encoding="utf-8",
            )
            with self.assertRaises(IDENTITY.IdentityError):
                IDENTITY.read_submodule_repository(root, "externals/skia")

    def test_manifest_requires_exactly_one_exact_skia_registration(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            expected = "https://github.com/dotnet/skia.git"
            identity = {"skiaGitUrl": expected}

            self.write_manifest(root, expected)
            IDENTITY.validate_manifest(root, identity)

            for urls in (
                (),
                (expected, expected),
                ("https://github.com/dotnet/skia",),
                ("https://github.com/mono/skia.git",),
            ):
                with self.subTest(urls=urls):
                    self.write_manifest(root, *urls)
                    with self.assertRaises(IDENTITY.IdentityError):
                        IDENTITY.validate_manifest(root, identity)

            (root / "cgmanifest.json").write_text(
                json.dumps(
                    {
                        "registrations": [
                            {
                                "component": {
                                    "type": "other",
                                    "git": {"repositoryUrl": expected},
                                }
                            }
                        ]
                    }
                ),
                encoding="utf-8",
            )
            with self.assertRaises(IDENTITY.IdentityError):
                IDENTITY.validate_manifest(root, identity)

    def test_rejects_malformed_manifest(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            identity = {"skiaGitUrl": "https://github.com/mono/skia.git"}
            path = root / "cgmanifest.json"
            invalid_manifests = (
                [],
                {},
                {"registrations": {}},
                {"registrations": ["invalid"]},
                {"registrations": [{}]},
                {"registrations": [{"component": "invalid"}]},
                {"registrations": [{"component": {}}]},
                {"registrations": [{"component": {"type": "git"}}]},
                {
                    "registrations": [
                        {"component": {"type": "git", "git": "invalid"}}
                    ]
                },
                {
                    "registrations": [
                        {"component": {"type": "git", "git": {}}}
                    ]
                },
                {
                    "registrations": [
                        {
                            "component": {
                                "type": "git",
                                "git": {"repositoryUrl": None},
                            }
                        }
                    ]
                },
            )
            for manifest in invalid_manifests:
                with self.subTest(manifest=manifest):
                    path.write_text(json.dumps(manifest), encoding="utf-8")
                    with self.assertRaises(IDENTITY.IdentityError):
                        IDENTITY.validate_manifest(root, identity)

            path.write_text("{", encoding="utf-8")
            with self.assertRaises(IDENTITY.IdentityError):
                IDENTITY.validate_manifest(root, identity)

    def test_complete_destination_identity_flip(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            config_path = self.write_config(
                root,
                offlineRepository="dotnet/SkiaSharp",
                publicSiteBaseUrl="https://docs.example/SkiaSharp/",
            )
            self.write_gitmodules(
                root,
                skia_url="https://github.com/dotnet/skia.git",
                docs_url="https://github.com/dotnet/SkiaSharp-API-docs.git",
            )
            self.write_manifest(
                root,
                "https://github.com/dotnet/skia.git",
            )

            identity = IDENTITY.resolve_identity(
                root,
                environ={},
                config_path=config_path,
            )
            IDENTITY.validate_manifest(root, identity)

            self.assertEqual("dotnet/SkiaSharp", identity["repository"])
            self.assertEqual("dotnet/skia", identity["skiaRepository"])
            self.assertEqual(
                "dotnet/SkiaSharp-API-docs",
                identity["docsRepository"],
            )
            self.assertEqual(
                "https://docs.example/SkiaSharp",
                identity["publicSiteBaseUrl"],
            )
            self.assertEqual(52293126, identity["canonicalRepositoryId"])
            self.assertEqual("github-52292286", identity["skiaRepositoryKey"])
            self.assertEqual(
                ["mono-skia"],
                identity["legacySkiaRepositoryKeys"],
            )

    def test_cli_json_get_validate_and_errors(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            config_path = self.write_config(root)
            self.write_gitmodules(root)
            self.write_manifest(root, "https://github.com/mono/skia.git")
            common = [
                "--root",
                str(root),
                "--config",
                str(config_path),
                "--repository",
                "https://github.com/dotnet/SkiaSharp.git",
            ]
            identity = IDENTITY.resolve_identity(
                root,
                repository="dotnet/SkiaSharp",
                config_path=config_path,
            )

            stdout = io.StringIO()
            with redirect_stdout(stdout):
                self.assertEqual(0, IDENTITY.main([*common, "json"]))
            self.assertEqual(
                json.dumps(identity, sort_keys=True) + "\n",
                stdout.getvalue(),
            )

            stdout = io.StringIO()
            with redirect_stdout(stdout):
                self.assertEqual(
                    0,
                    IDENTITY.main([*common, "get", "legacySkiaRepositoryKeys"]),
                )
            self.assertEqual('["mono-skia"]\n', stdout.getvalue())

            stdout = io.StringIO()
            with redirect_stdout(stdout):
                self.assertEqual(0, IDENTITY.main([*common, "validate"]))
            self.assertIn(
                "Repository identity is valid: dotnet/SkiaSharp",
                stdout.getvalue(),
            )

            stderr = io.StringIO()
            with redirect_stderr(stderr):
                self.assertEqual(
                    1,
                    IDENTITY.main([*common, "get", "missing"]),
                )
            self.assertIn(
                "Unknown repository identity field: missing",
                stderr.getvalue(),
            )

    def test_current_checkout_identity_and_manifest_are_consistent(self) -> None:
        identity = IDENTITY.resolve_identity(ROOT, environ={})
        self.assertEqual("mono/SkiaSharp", identity["repository"])
        self.assertEqual("mono/skia", identity["skiaRepository"])
        self.assertEqual(
            "mono/SkiaSharp-API-docs",
            identity["docsRepository"],
        )
        IDENTITY.validate_manifest(ROOT, identity)


if __name__ == "__main__":
    unittest.main()
