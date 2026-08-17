import importlib.util
import json
from pathlib import Path
import tempfile
from types import SimpleNamespace
import unittest
from unittest import mock
import zipfile


SCRIPT = Path(__file__).resolve().parents[1] / "download-darc-packages.py"
SPEC = importlib.util.spec_from_file_location("download_darc_packages", SCRIPT)
download = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(download)

COMMIT = "a" * 40
CHANNEL = "SkiaSharp Internal Testing"
REPOSITORY = "https://github.com/mono/SkiaSharp"


def build(build_id=42, *, channels=None, commit=COMMIT):
    return {
        "id": build_id,
        "repository": REPOSITORY,
        "branch": "refs/heads/release/4.0.0",
        "commit": commit,
        "buildNumber": "4.0.0-preview.1",
        "azdoBuildId": 100,
        "channels": channels if channels is not None else [CHANNEL],
    }


def write_package(root: Path, package_id: str, version: str, suffix=""):
    path = root / "shipping" / "packages" / (
        f"{package_id}.{version}{suffix}.nupkg"
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    nuspec = (
        "<package><metadata>"
        f"<id>{package_id}</id><version>{version}</version>"
        "</metadata></package>"
    )
    with zipfile.ZipFile(path, "w") as archive:
        archive.writestr(f"{package_id}.nuspec", nuspec)
    return path


def arguments(output: Path, **overrides):
    values = {
        "build_id": None,
        "channel": CHANNEL,
        "expected_commit": COMMIT,
        "expected_channel": None,
        "expected_branch": None,
        "expected_package": [
            "SkiaSharp=4.0.0-preview.1",
            "HarfBuzzSharp=9.0.0-preview.1",
        ],
        "repository": REPOSITORY,
        "output_dir": output,
        "evidence": None,
        "darc": "darc",
        "dotnet": "dotnet",
        "bar_uri": download.BAR_URI,
        "bar_password_env": None,
        "azdev_pat_env": None,
        "ci": True,
        "download_timeout": 30,
    }
    values.update(overrides)
    return SimpleNamespace(**values)


class DownloadDarcPackagesTests(unittest.TestCase):
    def test_channel_resolution_downloads_pinned_build_and_emits_evidence(self):
        with tempfile.TemporaryDirectory() as temporary:
            output = Path(temporary) / "drop"

            def fake_run(command, *, timeout):
                if command[1] == "get-latest-build":
                    return SimpleNamespace(
                        returncode=0,
                        stdout="log\n" + json.dumps([build()]),
                        stderr="",
                    )
                if command[0] == "dotnet":
                    self.assertEqual(command[1:4], ["nuget", "verify", "--all"])
                    return SimpleNamespace(returncode=0, stdout="", stderr="")
                self.assertEqual(command[1], "gather-drop")
                self.assertIn("--no-workarounds", command)
                self.assertNotIn("--latest-location", command)
                self.assertEqual(command[command.index("--id") + 1], "42")
                write_package(output, "SkiaSharp", "4.0.0-preview.1")
                write_package(output, "HarfBuzzSharp", "9.0.0-preview.1")
                (output / "manifest.json").write_text("{}\n", encoding="ascii")
                return SimpleNamespace(returncode=0, stdout="", stderr="")

            with mock.patch.object(download, "run", side_effect=fake_run):
                report = download.execute(arguments(output))

            self.assertEqual(report["resolvedBuild"]["id"], 42)
            self.assertEqual(len(report["packages"]), 2)
            self.assertTrue(report["signatureVerification"]["verified"])
            self.assertEqual(
                report["download"]["packageSource"],
                str((output / "shipping" / "packages").resolve()),
            )
            evidence = json.loads(
                (output / "darc-provenance.json").read_text(encoding="ascii")
            )
            self.assertEqual(evidence["selector"]["expectedCommit"], COMMIT)

    def test_rejects_ambiguous_exact_matches(self):
        with self.assertRaisesRegex(
            download.DownloadError,
            "expected exactly one BAR build",
        ):
            download.select_build(
                [build(41), build(42)],
                repository=REPOSITORY,
                commit=COMMIT,
                channel=CHANNEL,
            )

    def test_rejects_build_not_on_exact_channel(self):
        with self.assertRaisesRegex(
            download.DownloadError,
            "expected exactly one BAR build",
        ):
            download.select_build(
                [build(channels=["SkiaSharp Internal Testing Preview"])],
                repository=REPOSITORY,
                commit=COMMIT,
                channel=CHANNEL,
            )

    def test_rejects_missing_required_package(self):
        with tempfile.TemporaryDirectory() as temporary:
            output = Path(temporary)
            write_package(output, "SkiaSharp", "4.0.0-preview.1")
            with self.assertRaisesRegex(
                download.DownloadError,
                "missing required package IDs: HarfBuzzSharp",
            ):
                download.inspect_packages(output, set())

    def test_rejects_duplicate_package_identity(self):
        with tempfile.TemporaryDirectory() as temporary:
            output = Path(temporary)
            write_package(output, "SkiaSharp", "4.0.0-preview.1")
            write_package(
                output,
                "SkiaSharp",
                "4.0.0-preview.1",
                suffix=".duplicate",
            )
            write_package(output, "HarfBuzzSharp", "9.0.0-preview.1")
            with self.assertRaisesRegex(
                download.DownloadError,
                "duplicate package identity",
            ):
                download.inspect_packages(output, set())

    def test_redacts_credentials(self):
        self.assertEqual(
            download.redact(
                ["darc", "gather-drop", "--azdev-pat", "secret", "-p", "bar"]
            ),
            ["darc", "gather-drop", "--azdev-pat", "***", "-p", "***"],
        )

    def test_requires_exact_versions_for_both_base_packages(self):
        with self.assertRaisesRegex(
            download.DownloadError,
            "must pin exact versions for: HarfBuzzSharp",
        ):
            download.parse_expected(["SkiaSharp=4.0.0-preview.1"])

    def test_pipeline_enables_v3_registration_after_signing(self):
        root = SCRIPT.parents[3]
        package_pipeline = (
            root / "scripts/azure-pipelines-package.yml"
        ).read_text(encoding="ascii")
        signing = (
            root / "scripts/azure-templates-stages-signing.yml"
        ).read_text(encoding="ascii")
        publishing = (root / "eng/Publishing.props").read_text(encoding="ascii")
        self.assertIn("enablePublishing: ${{ or(", package_pipeline)
        self.assertIn("publish_build_assets", package_pipeline)
        self.assertIn("requireDefaultChannels: true", package_pipeline)
        self.assertIn("-publish", signing)
        self.assertIn("<PublishingVersion>3</PublishingVersion>", publishing)
        self.assertIn(
            '<Artifact Include="$(ArtifactsShippingPackagesDir)**\\*.nupkg"',
            publishing,
        )

    def test_files_are_ascii(self):
        SCRIPT.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
