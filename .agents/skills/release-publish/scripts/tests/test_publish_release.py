#!/usr/bin/env python3

import importlib.util
from pathlib import Path
import sys
import unittest


SCRIPTS = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(SCRIPTS))


def load(name: str, filename: str):
    path = SCRIPTS / filename
    spec = importlib.util.spec_from_file_location(name, path)
    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    return module


publish = load("publish_release", "release_publish.py")
github = load("release_github_test", "release_github.py")


class PublishReleaseTests(unittest.TestCase):
    def test_release_version_derives_exact_metadata(self):
        preview = publish.ReleaseVersion.parse(
            "release/4.152.0-preview.2"
        )
        self.assertEqual(preview.numeric, "4.152.0")
        self.assertEqual(preview.release_type, "preview")
        self.assertEqual(preview.title, "Version 4.152.0 (Preview 2)")
        preview.validate_public_version("4.152.0-preview.2.3")

        rc = publish.ReleaseVersion.parse("release/4.152.0-rc.1")
        self.assertEqual(rc.release_type, "rc")
        self.assertEqual(rc.title, "Version 4.152.0 (RC 1)")
        rc.validate_public_version("4.152.0-rc.1.26425.1")

        stable = publish.ReleaseVersion.parse("release/4.152.0")
        self.assertEqual(stable.release_type, "stable")
        self.assertEqual(stable.title, "Version 4.152.0")
        stable.validate_public_version("4.152.0")

        hotfix = publish.ReleaseVersion.parse("release/3.119.4.1-rc.1")
        self.assertEqual(hotfix.release_type, "hotfix rc")
        self.assertEqual(hotfix.title, "Version 3.119.4.1 (RC 1)")

    def test_git_repository_holds_mutable_runtime_state(self):
        repository = publish.GitRepository(Path("/tmp/example"))
        self.assertEqual(repository.root, Path("/tmp/example"))

    def test_public_version_must_match_release(self):
        preview = publish.ReleaseVersion.parse(
            "release/4.152.0-preview.1"
        )
        with self.assertRaisesRegex(
            publish.PublishError,
            "does not match",
        ):
            preview.validate_public_version("4.152.0-preview.2.1")
        stable = publish.ReleaseVersion.parse("release/4.152.0")
        with self.assertRaisesRegex(
            publish.PublishError,
            "stable public version",
        ):
            stable.validate_public_version("4.152.0-stable.1")

    def test_previous_tag_is_immediate_version_predecessor(self):
        previous = github.previous_release_tag(
            "v4.152.0-preview.2.2",
            [
                "v4.152.0-preview.2.3",
                "v4.152.0-preview.2.1",
                "v4.152.0-preview.1.4",
                "v4.151.1",
                "v4.151.0",
            ],
        )
        self.assertEqual(previous, "v4.152.0-preview.2.1")

    def test_previous_tag_orders_stable_and_hotfix_releases(self):
        self.assertEqual(
            github.previous_release_tag(
                "v4.152.0",
                [
                    "v4.151.1",
                    "v4.152.0-preview.2.1",
                    "v4.152.0-rc.1.1",
                ],
            ),
            "v4.152.0-rc.1.1",
        )
        self.assertEqual(
            github.previous_release_tag(
                "v4.152.0.1-preview.1.1",
                [
                    "v4.152.0-preview.2.1",
                    "v4.152.0-rc.1.1",
                    "v4.152.0",
                ],
            ),
            "v4.152.0",
        )

    def test_generated_log_count_excludes_new_contributors(self):
        generated = """## What's Changed
* Add feature by @one in https://github.com/mono/SkiaSharp/pull/1
* Fix bug by @two in https://github.com/mono/SkiaSharp/pull/2

## New Contributors
* @one made their first contribution in https://github.com/mono/SkiaSharp/pull/1

**Full Changelog**: https://github.com/mono/SkiaSharp/compare/v1...v2
"""
        compare, changes, count = github.generated_log_parts(generated)
        self.assertEqual(
            compare,
            "https://github.com/mono/SkiaSharp/compare/v1...v2",
        )
        self.assertEqual(count, 2)
        self.assertIn("## New Contributors", changes)
        self.assertNotIn("## What's Changed", changes)
        self.assertNotIn("**Full Changelog**:", changes)

    def test_release_body_assembly_is_deterministic(self):
        teaser = (
            "A focused release.\n\n"
            f"{github.TEASER_LINKS_MARKER}\n\n"
            "## What's New\n- Added a feature by @one (#1)\n"
        )
        generated = """## What's Changed
* Add feature by @one in https://github.com/mono/SkiaSharp/pull/1

**Full Changelog**: https://github.com/mono/SkiaSharp/compare/v1...v2
"""
        body = github.assemble_release_body(
            teaser,
            generated,
            public_version="4.152.0-preview.1.1",
            notes_version="4.152.0",
        )
        self.assertIn(
            "https://www.nuget.org/packages/SkiaSharp/"
            "4.152.0-preview.1.1",
            body,
        )
        self.assertIn(
            "docs/releases/4.152.0.html",
            body,
        )
        self.assertIn("All changes (1 pull requests)", body)
        self.assertEqual(body.count("Full changelog"), 1)
        self.assertNotIn(github.TEASER_LINKS_MARKER, body)

    def test_release_body_rejects_unsafe_teaser_output(self):
        generated = "## What's Changed\n"
        for teaser, message in (
            (
                "```markdown\nsubtitle\n```\n\n<!-- RELEASE_LINKS -->",
                "code fence",
            ),
            (
                "Security fix for CVE-1234\n\n<!-- RELEASE_LINKS -->",
                "security",
            ),
            (
                "## What's New\n\n<!-- RELEASE_LINKS -->",
                "plain-language subtitle",
            ),
        ):
            with self.assertRaisesRegex(
                github.publish.PublishError,
                message,
            ):
                github.assemble_release_body(
                    teaser,
                    generated,
                    public_version="4.152.0",
                    notes_version="4.152.0",
                )

    def test_status_handoff_rejects_changed_run(self):
        release = publish.ReleaseVersion.parse("release/4.152.0")
        status = self._stable_status(release)
        status["buildRun"]["runId"] = 10
        with self.assertRaisesRegex(
            publish.PublishError,
            "Build run changed",
        ):
            publish.validate_status_handoff(
                status,
                release,
                expected_sha="a" * 40,
                expected_build_run=11,
                expected_tests_run=20,
                expected_bar_build=30,
            )

    def test_status_handoff_rejects_changed_bar_build(self):
        release = publish.ReleaseVersion.parse("release/4.152.0")
        status = self._stable_status(release)
        with self.assertRaisesRegex(
            publish.PublishError,
            "BAR build changed",
        ):
            publish.validate_status_handoff(
                status,
                release,
                expected_sha="a" * 40,
                expected_build_run=10,
                expected_tests_run=20,
                expected_bar_build=31,
            )

    def test_status_handoff_rejects_bar_commit_mismatch(self):
        release = publish.ReleaseVersion.parse("release/4.152.0")
        status = self._stable_status(release)
        status["barBuild"]["commit"] = "b" * 40
        with self.assertRaisesRegex(
            publish.PublishError,
            "does not match the tested source commit",
        ):
            publish.validate_status_handoff(
                status,
                release,
                expected_sha="a" * 40,
                expected_build_run=10,
                expected_tests_run=20,
                expected_bar_build=30,
            )

    def test_status_handoff_does_not_require_named_channel(self):
        release = publish.ReleaseVersion.parse("release/4.152.0")
        status = self._stable_status(release)
        status["barBuild"]["channels"] = []
        handoff = publish.validate_status_handoff(
            status,
            release,
            expected_sha="a" * 40,
            expected_build_run=10,
            expected_tests_run=20,
            expected_bar_build=30,
        )
        self.assertEqual(handoff["bar"]["id"], 30)

    def test_status_handoff_rejects_missing_default_channel_mapping(self):
        release = publish.ReleaseVersion.parse("release/4.152.0")
        for channel_ids in ([], [3882]):
            with self.subTest(channel_ids=channel_ids):
                status = self._stable_status(release)
                status["barBuild"]["defaultChannelIds"] = channel_ids
                with self.assertRaisesRegex(
                    publish.PublishError,
                    r"\.NET Libraries channel 1648",
                ):
                    publish.validate_status_handoff(
                        status,
                        release,
                        expected_sha="a" * 40,
                        expected_build_run=10,
                        expected_tests_run=20,
                        expected_bar_build=30,
                    )

    def test_status_handoff_rejects_transport_only_route(self):
        release = publish.ReleaseVersion.parse("release/4.152.0")
        status = self._stable_status(release)
        status["barBuild"]["assets"]["SkiaSharp"]["locations"] = [
            "https://pkgs.dev.azure.com/dnceng/public/"
            "_packaging/dotnet-libraries-transport/nuget/v3/index.json"
        ]
        with self.assertRaisesRegex(
            publish.PublishError,
            "does not route SkiaSharp exclusively to dotnet-libraries",
        ):
            publish.validate_status_handoff(
                status,
                release,
                expected_sha="a" * 40,
                expected_build_run=10,
                expected_tests_run=20,
                expected_bar_build=30,
            )

    def test_status_handoff_rejects_cross_feed_locations(self):
        release = publish.ReleaseVersion.parse("release/4.152.0")
        cases = (
            ("assets", "SkiaSharp", publish.TRANSPORT_FEED_MARKER),
            (
                "transportAssets",
                "_NuGets",
                publish.PRODUCT_FEED_MARKER,
            ),
        )
        for group, package_id, wrong_marker in cases:
            with self.subTest(package_id=package_id):
                status = self._stable_status(release)
                status["barBuild"][group][package_id]["locations"].append(
                    f"https://example.test{wrong_marker}nuget/v3/index.json"
                )
                with self.assertRaisesRegex(
                    publish.PublishError,
                    "exclusively",
                ):
                    publish.validate_status_handoff(
                        status,
                        release,
                        expected_sha="a" * 40,
                        expected_build_run=10,
                        expected_tests_run=20,
                        expected_bar_build=30,
                    )

    def test_status_handoff_allows_neutral_source_artifact_locations(self):
        release = publish.ReleaseVersion.parse("release/4.152.0")
        status = self._stable_status(release)
        neutral = (
            "https://dev.azure.com/dnceng/internal/_apis/build/"
            "builds/3058119/artifacts"
        )
        for group in ("assets", "transportAssets"):
            for asset in status["barBuild"][group].values():
                asset["locations"].append(neutral)
        handoff = publish.validate_status_handoff(
            status,
            release,
            expected_sha="a" * 40,
            expected_build_run=10,
            expected_tests_run=20,
            expected_bar_build=30,
        )
        self.assertEqual(handoff["bar"]["id"], 30)

    def test_status_handoff_rejects_unverified_prerequisites(self):
        release = publish.ReleaseVersion.parse("release/4.152.0")
        status = self._stable_status(release)
        status["prerequisites"] = {
            "state": "missing",
            "missing": [{"id": "combined-build"}],
        }
        with self.assertRaisesRegex(
            publish.PublishError,
            "tooling prerequisites",
        ):
            publish.validate_status_handoff(
                status,
                release,
                expected_sha="a" * 40,
                expected_build_run=10,
                expected_tests_run=20,
                expected_bar_build=30,
            )

    def test_status_handoff_rejects_duplicate_transport_ids(self):
        release = publish.ReleaseVersion.parse("release/4.152.0")
        status = self._stable_status(release)
        status["barBuild"]["nonShippingAssets"] = {
            "_nugets": [
                "0.0.0-branch.release-4.152.0.1",
                "0.0.0-commit.abc123.1",
            ]
        }
        with self.assertRaisesRegex(
            publish.PublishError,
            "duplicate NonShipping transport",
        ):
            publish.validate_status_handoff(
                status,
                release,
                expected_sha="a" * 40,
                expected_build_run=10,
                expected_tests_run=20,
                expected_bar_build=30,
            )

    def test_status_handoff_requires_exact_stable_versions(self):
        # Stable BAR package versions must be exact X.Y.Z, never a
        # X.Y.Z-stable.{build} pre-release suffix.
        release = publish.ReleaseVersion.parse("release/4.152.0")
        status = self._stable_status(release)
        status["barBuild"]["assets"]["SkiaSharp"]["version"] = (
            "4.152.0-stable.1"
        )
        status["packageVersions"]["test"]["SkiaSharp"] = "4.152.0-stable.1"
        status["packageVersions"]["public"]["SkiaSharp"] = "4.152.0-stable.1"
        with self.assertRaisesRegex(
            publish.PublishError,
            "stable public version",
        ):
            publish.validate_status_handoff(
                status,
                release,
                expected_sha="a" * 40,
                expected_build_run=10,
                expected_tests_run=20,
                expected_bar_build=30,
            )

    def test_status_handoff_rejects_distinct_test_package_family(self):
        release = publish.ReleaseVersion.parse("release/4.152.0")
        status = self._stable_status(release)
        status["packageVersions"]["test"]["SkiaSharp"] = (
            "4.152.0-preview.99"
        )
        with self.assertRaisesRegex(
            publish.PublishError,
            "same BAR assets",
        ):
            publish.validate_status_handoff(
                status,
                release,
                expected_sha="a" * 40,
                expected_build_run=10,
                expected_tests_run=20,
                expected_bar_build=30,
            )

    def test_status_handoff_accepts_exact_stable_versions(self):
        release = publish.ReleaseVersion.parse("release/4.152.0")
        status = self._stable_status(release)
        handoff = publish.validate_status_handoff(
            status,
            release,
            expected_sha="a" * 40,
            expected_build_run=10,
            expected_tests_run=20,
            expected_bar_build=30,
        )
        self.assertEqual(handoff["bar"]["assets"]["SkiaSharp"]["version"], "4.152.0")
        self.assertEqual(handoff["build"]["runId"], 10)
        self.assertEqual(handoff["bar"]["id"], 30)

    @staticmethod
    def _stable_status(release: "publish.ReleaseVersion") -> dict:
        return {
            "branch": release.branch,
            "commit": "a" * 40,
            "nextAction": "start-release-testing",
            "prerequisites": {"state": "ready", "missing": []},
            "buildRun": {
                "runId": 10,
                "pipelineId": publish.BUILD_DEFINITION_ID,
                "sourceVersion": "a" * 40,
                "sourceBranch": "refs/heads/release/4.152.0",
                "buildNumber": "4.152.0+4.152.0",
            },
            "testsRun": {
                "runId": 20,
                "pipelineId": publish.TESTS_DEFINITION_ID,
                "sourceVersion": "a" * 40,
            },
            "barBuild": {
                "id": 30,
                "state": "ready",
                "commit": "a" * 40,
                "buildRunId": 10,
                "buildDefinitionId": publish.BUILD_DEFINITION_ID,
                "branch": "refs/heads/release/4.152.0",
                "buildNumber": "4.152.0+4.152.0",
                "defaultChannelIds": [1648],
                "channels": [".NET Libraries"],
                "routedAssets": {
                    "SkiaSharp": True,
                    "HarfBuzzSharp": True,
                },
                "routedTransportAssets": {
                    "_NativeAssets": True,
                    "_NuGets": True,
                },
                "assets": {
                    "SkiaSharp": {
                        "version": "4.152.0",
                        "locations": [
                            "https://pkgs.dev.azure.com/dnceng/public/"
                            "_packaging/dotnet-libraries/nuget/v3/index.json"
                        ],
                    },
                    "HarfBuzzSharp": {
                        "version": "1.0.0",
                        "locations": [
                            "https://pkgs.dev.azure.com/dnceng/public/"
                            "_packaging/dotnet-libraries/nuget/v3/index.json"
                        ],
                    },
                },
                "transportAssets": {
                    "_NativeAssets": {
                        "version": "0.0.0-branch.release-4.152.0.1",
                        "locations": [
                            "https://pkgs.dev.azure.com/dnceng/public/"
                            "_packaging/dotnet-libraries-transport/"
                            "nuget/v3/index.json"
                        ],
                    },
                    "_NuGets": {
                        "version": "0.0.0-branch.release-4.152.0.1",
                        "locations": [
                            "https://pkgs.dev.azure.com/dnceng/public/"
                            "_packaging/dotnet-libraries-transport/"
                            "nuget/v3/index.json"
                        ],
                    },
                },
            },
            "packageVersions": {
                "test": {
                    "SkiaSharp": "4.152.0",
                    "HarfBuzzSharp": "1.0.0",
                },
                "public": {
                    "SkiaSharp": "4.152.0",
                    "HarfBuzzSharp": "1.0.0",
                },
            },
        }

    def test_status_handoff_accepts_exact_feed_proof_without_bar_locations(self):
        release = publish.ReleaseVersion.parse("release/4.152.0")
        status = self._stable_status(release)
        for asset in status["barBuild"]["assets"].values():
            asset["locations"] = []
        for asset in status["barBuild"]["transportAssets"].values():
            asset["locations"] = []
        handoff = publish.validate_status_handoff(
            status,
            release,
            expected_sha="a" * 40,
            expected_build_run=10,
            expected_tests_run=20,
            expected_bar_build=30,
        )
        self.assertEqual(handoff["bar"]["state"], "ready")

    def test_status_handoff_rejects_missing_location_and_feed_proof(self):
        release = publish.ReleaseVersion.parse("release/4.152.0")
        status = self._stable_status(release)
        status["barBuild"]["assets"]["SkiaSharp"]["locations"] = []
        status["barBuild"]["routedAssets"]["SkiaSharp"] = False
        with self.assertRaisesRegex(
            publish.PublishError,
            "no BAR location or exact dotnet-libraries package proof",
        ):
            publish.validate_status_handoff(
                status,
                release,
                expected_sha="a" * 40,
                expected_build_run=10,
                expected_tests_run=20,
                expected_bar_build=30,
            )

    def test_status_handoff_rejects_missing_transport_feed_proof(self):
        release = publish.ReleaseVersion.parse("release/4.152.0")
        status = self._stable_status(release)
        status["barBuild"]["transportAssets"]["_NuGets"]["locations"] = []
        status["barBuild"]["routedTransportAssets"]["_NuGets"] = False
        with self.assertRaisesRegex(
            publish.PublishError,
            "no BAR location or exact dotnet-libraries-transport "
            "package proof",
        ):
            publish.validate_status_handoff(
                status,
                release,
                expected_sha="a" * 40,
                expected_build_run=10,
                expected_tests_run=20,
                expected_bar_build=30,
            )

    def test_scripts_are_ascii_only(self):
        for path in SCRIPTS.glob("*.py"):
            path.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
