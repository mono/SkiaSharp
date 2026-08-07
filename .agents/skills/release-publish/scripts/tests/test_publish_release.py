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
push = load("push_release_packages_test", "push-release-packages.py")
finalize = load("finalize_release_test", "finalize-release.py")


class PublishReleaseTests(unittest.TestCase):
    def test_release_version_derives_exact_metadata(self):
        preview = publish.ReleaseVersion.parse(
            "release/4.152.0-preview.2"
        )
        self.assertEqual(preview.numeric, "4.152.0")
        self.assertEqual(preview.release_type, "preview")
        self.assertEqual(preview.title, "Version 4.152.0 (Preview 2)")
        preview.validate_public_version("4.152.0-preview.2.3")

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

    def test_previous_tag_candidates_exclude_newer_releases(self):
        release = publish.ReleaseVersion.parse(
            "release/4.152.0-preview.2"
        )
        candidates = finalize.previous_tag_candidates(
            release,
            "v4.152.0-preview.2.2",
            [
                "v4.152.0-preview.2.3",
                "v4.152.0-preview.2.1",
                "v4.152.0-preview.1.4",
                "v4.151.1",
                "v4.151.0",
            ],
        )
        self.assertEqual(
            candidates[:4],
            [
                "v4.152.0-preview.2.1",
                "v4.152.0-preview.1.4",
                "v4.151.1",
                "v4.151.0",
            ],
        )
        self.assertNotIn("v4.152.0-preview.2.3", candidates)

    def test_azure_request_pins_exact_managed_run(self):
        request = push.AzurePublish.request_body(
            12345,
            stable=True,
            preview=True,
        )
        self.assertTrue(request["previewRun"])
        self.assertEqual(
            request["resources"]["pipelines"]["SkiaSharp"]["version"],
            "12345",
        )
        self.assertEqual(
            request["templateParameters"],
            {
                "selectedResource": "SkiaSharp",
                "pushPackages": True,
                "pushStable": True,
            },
        )

    def test_generated_log_count_excludes_new_contributors(self):
        generated = """## What's Changed
* Add feature by @one in https://github.com/mono/SkiaSharp/pull/1
* Fix bug by @two in https://github.com/mono/SkiaSharp/pull/2

## New Contributors
* @one made their first contribution in https://github.com/mono/SkiaSharp/pull/1

**Full Changelog**: https://github.com/mono/SkiaSharp/compare/v1...v2
"""
        compare, changes, count = finalize.generated_log_parts(generated)
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
            f"{finalize.TEASER_LINKS_MARKER}\n\n"
            "## What's New\n- Added a feature by @one (#1)\n"
        )
        generated = """## What's Changed
* Add feature by @one in https://github.com/mono/SkiaSharp/pull/1

**Full Changelog**: https://github.com/mono/SkiaSharp/compare/v1...v2
"""
        body = finalize.assemble_release_body(
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
        self.assertNotIn(finalize.TEASER_LINKS_MARKER, body)

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
                finalize.publish.PublishError,
                message,
            ):
                finalize.assemble_release_body(
                    teaser,
                    generated,
                    public_version="4.152.0",
                    notes_version="4.152.0",
                )

    def test_status_handoff_rejects_changed_run(self):
        release = publish.ReleaseVersion.parse("release/4.152.0")
        status = {
            "branch": release.branch,
            "commit": "a" * 40,
            "nextAction": "start-release-testing",
            "managedRun": {
                "runId": 10,
                "sourceVersion": "a" * 40,
            },
            "testsRun": {"runId": 20},
            "packageVersions": {
                "test": {
                    "SkiaSharp": "4.152.0-stable.1",
                    "HarfBuzzSharp": "1.0.0-stable.1",
                },
                "public": {
                    "SkiaSharp": "4.152.0",
                    "HarfBuzzSharp": "1.0.0",
                },
            },
        }
        with self.assertRaisesRegex(
            publish.PublishError,
            "managed run changed",
        ):
            publish.validate_status_handoff(
                status,
                release,
                expected_sha="a" * 40,
                expected_managed_run=11,
                expected_tests_run=20,
            )

    def test_scripts_are_ascii_only(self):
        for path in SCRIPTS.glob("*.py"):
            path.read_text(encoding="ascii")
        Path(__file__).read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
