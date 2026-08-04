import json
import os
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

from update_versions import (
    TRACKED_SKIA_DEPENDENCIES,
    DependencyReviewRequired,
    main,
    reconcile_dependency_metadata,
    update_versions,
)


class UpdateVersionsTests(unittest.TestCase):
    def setUp(self) -> None:
        self.temporary_directory = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary_directory.name)
        (self.root / "scripts").mkdir()
        (self.root / "externals" / "skia" / "include" / "c").mkdir(parents=True)

        (self.root / "scripts" / "VERSIONS.txt").write_text(
            "skia release m151\n"
            "libSkiaSharp milestone 151\n"
            "libSkiaSharp increment 7\n"
            "libSkiaSharp soname 151.0.0\n"
            "SkiaSharp assembly 4.151.3.0\n"
            "SkiaSharp file 4.151.3.0\n"
            "SkiaSharp nuget 4.151.3\n",
            encoding="utf-8",
        )
        (self.root / "scripts" / "azure-templates-variables.yml").write_text(
            "SKIASHARP_VERSION: 4.151.3\n", encoding="utf-8"
        )
        (self.root / "externals" / "skia" / "include" / "c" / "sk_types.h").write_text(
            "#define SK_C_INCREMENT 7\n", encoding="utf-8"
        )
        (self.root / "externals" / "skia" / "DEPS").write_text(
            "deps = {\n"
            + "".join(
                f'  "third_party/externals/{dependency_name}": '
                f'"https://example.test/{TRACKED_SKIA_DEPENDENCIES[dependency_name]}@'
                f'{"old-vma-sha" if dependency_name == "vulkanmemoryallocator" else dependency_name + "-sha"}",\n'
                for dependency_name in TRACKED_SKIA_DEPENDENCIES
            )
            + "}\n",
            encoding="utf-8",
        )
        (self.root / "cgmanifest.json").write_text(
            json.dumps(
                {
                    "registrations": [
                        {
                            "component": {
                                "type": "git",
                                "git": {
                                    "repositoryUrl": "https://github.com/mono/skia.git",
                                    "commitHash": "old",
                                },
                            }
                        },
                        {
                            "component": {
                                "type": "other",
                                "other": {"name": "skia", "version": "chrome/m151"},
                            },
                            "chrome_milestone": 151,
                            "upstream_merge_commit": "old",
                        },
                    ]
                    + [
                        {
                            "component": {
                                "type": "other",
                                "other": {
                                    "name": manifest_name,
                                    "version": (
                                        "3.2.1"
                                        if dependency_name == "vulkanmemoryallocator"
                                        else "1.0.0"
                                    ),
                                },
                            },
                            "skia_dependency": {
                                "name": dependency_name,
                                "revision": (
                                    "old-vma-sha"
                                    if dependency_name == "vulkanmemoryallocator"
                                    else dependency_name + "-sha"
                                ),
                                "version_reviewed_identity": (
                                    f"https://example.test/{manifest_name}@"
                                    + (
                                        "old-vma-sha"
                                        if dependency_name == "vulkanmemoryallocator"
                                        else dependency_name + "-sha"
                                    )
                                ),
                                "version_source": "fixture: authoritative version",
                            },
                        }
                        for dependency_name, manifest_name in TRACKED_SKIA_DEPENDENCIES.items()
                    ]
                }
            ),
            encoding="utf-8",
        )

        skia = self.root / "externals" / "skia"
        subprocess.run(["git", "init", "--quiet"], cwd=skia, check=True)
        subprocess.run(["git", "config", "user.email", "fixture@example.test"], cwd=skia, check=True)
        subprocess.run(["git", "config", "user.name", "fixture"], cwd=skia, check=True)
        subprocess.run(["git", "add", "."], cwd=skia, check=True)
        subprocess.run(["git", "commit", "--quiet", "-m", "fixture"], cwd=skia, check=True)
        subprocess.run(["git", "branch", "upstream/chrome/m152"], cwd=skia, check=True)
        self.skia_base_sha = subprocess.run(
            ["git", "rev-parse", "HEAD"],
            cwd=skia,
            check=True,
            capture_output=True,
            text=True,
        ).stdout.strip()

        subprocess.run(["git", "init", "--quiet"], cwd=self.root, check=True)
        subprocess.run(
            ["git", "config", "user.email", "fixture@example.test"],
            cwd=self.root,
            check=True,
        )
        subprocess.run(
            ["git", "config", "user.name", "fixture"],
            cwd=self.root,
            check=True,
        )
        subprocess.run(["git", "add", "cgmanifest.json"], cwd=self.root, check=True)
        subprocess.run(
            ["git", "commit", "--quiet", "-m", "parent fixture"],
            cwd=self.root,
            check=True,
        )
        self.parent_base_sha = subprocess.run(
            ["git", "rev-parse", "HEAD"],
            cwd=self.root,
            check=True,
            capture_output=True,
            text=True,
        ).stdout.strip()

    def tearDown(self) -> None:
        self.temporary_directory.cleanup()

    def registration(self, manifest: dict, name: str) -> dict:
        return next(
            registration
            for registration in manifest["registrations"]
            if registration.get("component", {}).get("other", {}).get("name") == name
        )

    def test_updates_all_version_surfaces(self) -> None:
        update_versions(self.root, 151, 152, "chrome/m152")

        versions = (self.root / "scripts" / "VERSIONS.txt").read_text(encoding="utf-8")
        self.assertIn("release m152", versions)
        self.assertIn("milestone 152", versions)
        self.assertIn("increment 0", versions)
        self.assertIn("4.152.0", versions)
        self.assertIn(
            "SKIASHARP_VERSION: 4.152.0",
            (self.root / "scripts" / "azure-templates-variables.yml").read_text(
                encoding="utf-8"
            ),
        )
        self.assertIn(
            "#define SK_C_INCREMENT 0",
            (
                self.root
                / "externals"
                / "skia"
                / "include"
                / "c"
                / "sk_types.h"
            ).read_text(encoding="utf-8"),
        )

        cgmanifest = json.loads((self.root / "cgmanifest.json").read_text(encoding="utf-8"))
        skia = cgmanifest["registrations"][1]
        self.assertEqual(152, skia["chrome_milestone"])
        self.assertEqual("chrome/m152", skia["component"]["other"]["version"])
        self.assertEqual("chrome/m152", skia["upstream_ref"])

    def test_same_milestone_updates_only_manifest_hashes(self) -> None:
        versions_path = self.root / "scripts" / "VERSIONS.txt"
        pipeline_path = self.root / "scripts" / "azure-templates-variables.yml"
        sk_types_path = (
            self.root / "externals" / "skia" / "include" / "c" / "sk_types.h"
        )
        versions_path.write_text(
            versions_path.read_text(encoding="utf-8").replace(
                "libSkiaSharp increment 7", "libSkiaSharp increment 3"
            ),
            encoding="utf-8",
        )
        pipeline_path.write_text(
            "SKIASHARP_VERSION: 4.151.3\n", encoding="utf-8"
        )
        sk_types_path.write_text("#define SK_C_INCREMENT 3\n", encoding="utf-8")
        skia_root = self.root / "externals" / "skia"
        subprocess.run(["git", "branch", "upstream/main"], cwd=skia_root, check=True)

        before = {
            path: path.read_bytes()
            for path in (versions_path, pipeline_path, sk_types_path)
        }
        update_versions(self.root, 151, 151, "main")

        for path, content in before.items():
            self.assertEqual(content, path.read_bytes())
        cgmanifest = json.loads((self.root / "cgmanifest.json").read_text(encoding="utf-8"))
        self.assertEqual("chrome/m151", cgmanifest["registrations"][1]["component"]["other"]["version"])
        self.assertEqual("main", cgmanifest["registrations"][1]["upstream_ref"])
        self.assertNotEqual("old", cgmanifest["registrations"][1]["upstream_merge_commit"])

    def test_release_line_preserves_servicing_version_surfaces(self) -> None:
        versions_path = self.root / "scripts" / "VERSIONS.txt"
        pipeline_path = self.root / "scripts" / "azure-templates-variables.yml"
        sk_types_path = (
            self.root / "externals" / "skia" / "include" / "c" / "sk_types.h"
        )
        versions_path.write_text(
            "skia release m150\n"
            "libSkiaSharp milestone 150\n"
            "libSkiaSharp increment 7\n"
            "libSkiaSharp soname 150.0.0\n"
            "SkiaSharp assembly 4.150.2.0\n"
            "SkiaSharp file 4.150.2.0\n"
            "SkiaSharp nuget 4.150.2\n",
            encoding="utf-8",
        )
        pipeline_path.write_text(
            "SKIASHARP_VERSION: 4.150.2\n", encoding="utf-8"
        )
        sk_types_path.write_text("#define SK_C_INCREMENT 7\n", encoding="utf-8")

        manifest_path = self.root / "cgmanifest.json"
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        for registration in manifest["registrations"]:
            registration.pop("skia_dependency", None)
        skia_registration = manifest["registrations"][1]
        skia_registration["component"]["other"]["version"] = "chrome/m150"
        skia_registration["chrome_milestone"] = 150
        manifest_path.write_text(json.dumps(manifest), encoding="utf-8")
        expected_manifest = json.loads(json.dumps(manifest))

        skia_root = self.root / "externals" / "skia"
        subprocess.run(
            ["git", "branch", "upstream/chrome/m150"], cwd=skia_root, check=True
        )
        upstream_hash = subprocess.run(
            ["git", "rev-parse", "upstream/chrome/m150"],
            cwd=skia_root,
            check=True,
            capture_output=True,
            text=True,
        ).stdout.strip()
        expected_manifest["registrations"][0]["component"]["git"][
            "commitHash"
        ] = upstream_hash
        expected_skia = expected_manifest["registrations"][1]
        expected_skia["upstream_ref"] = "chrome/m150"
        expected_skia["upstream_merge_commit"] = upstream_hash
        before = {
            path: path.read_bytes()
            for path in (versions_path, pipeline_path, sk_types_path)
        }

        update_versions(self.root, 150, 150, "chrome/m150")

        for path, content in before.items():
            self.assertEqual(content, path.read_bytes())
        updated_manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        self.assertEqual(expected_manifest, updated_manifest)

    def test_missing_upstream_ref_does_not_modify_files(self) -> None:
        tracked = (
            self.root / "scripts" / "VERSIONS.txt",
            self.root / "scripts" / "azure-templates-variables.yml",
            self.root / "externals" / "skia" / "include" / "c" / "sk_types.h",
            self.root / "cgmanifest.json",
        )
        before = {path: path.read_bytes() for path in tracked}

        with self.assertRaises(subprocess.CalledProcessError):
            update_versions(self.root, 151, 152, "missing")

        for path, content in before.items():
            self.assertEqual(content, path.read_bytes())

    def test_milestone_update_is_idempotent_after_submodule_commit_changes(self) -> None:
        update_versions(self.root, 151, 152, "chrome/m152")
        tracked_versions = (
            self.root / "scripts" / "VERSIONS.txt",
            self.root / "scripts" / "azure-templates-variables.yml",
            self.root / "externals" / "skia" / "include" / "c" / "sk_types.h",
        )
        before = {path: path.read_bytes() for path in tracked_versions}

        skia_root = self.root / "externals" / "skia"
        marker = skia_root / "post-merge-fix.txt"
        marker.write_text("fix\n", encoding="utf-8")
        subprocess.run(["git", "add", marker.name], cwd=skia_root, check=True)
        subprocess.run(
            ["git", "commit", "--quiet", "-m", "post merge fix"],
            cwd=skia_root,
            check=True,
        )
        new_head = subprocess.run(
            ["git", "rev-parse", "HEAD"],
            cwd=skia_root,
            check=True,
            capture_output=True,
            text=True,
        ).stdout.strip()

        update_versions(self.root, 151, 152, "chrome/m152")

        for path, content in before.items():
            self.assertEqual(content, path.read_bytes())
        cgmanifest = json.loads((self.root / "cgmanifest.json").read_text(encoding="utf-8"))
        self.assertEqual(new_head, cgmanifest["registrations"][0]["component"]["git"]["commitHash"])

    def test_records_exact_upstream_sha_when_branch_moves(self) -> None:
        skia_root = self.root / "externals" / "skia"
        exact_sha = subprocess.run(
            ["git", "rev-parse", "upstream/chrome/m152"],
            cwd=skia_root,
            check=True,
            capture_output=True,
            text=True,
        ).stdout.strip()

        marker = skia_root / "later-upstream-commit.txt"
        marker.write_text("later\n", encoding="utf-8")
        subprocess.run(["git", "add", marker.name], cwd=skia_root, check=True)
        subprocess.run(
            ["git", "commit", "--quiet", "-m", "later upstream commit"],
            cwd=skia_root,
            check=True,
        )
        subprocess.run(
            ["git", "branch", "-f", "upstream/chrome/m152", "HEAD"],
            cwd=skia_root,
            check=True,
        )

        update_versions(
            self.root,
            151,
            152,
            "chrome/m152",
            upstream_sha=exact_sha,
        )

        cgmanifest = json.loads((self.root / "cgmanifest.json").read_text(encoding="utf-8"))
        self.assertEqual(
            exact_sha,
            cgmanifest["registrations"][1]["upstream_merge_commit"],
        )

    def test_dependency_change_requires_and_records_version_review(self) -> None:
        deps_path = self.root / "externals" / "skia" / "DEPS"
        deps_path.write_text(
            deps_path.read_text(encoding="utf-8").replace(
                "old-vma-sha", "new-vma-sha"
            ),
            encoding="utf-8",
        )
        artifact_dir = self.root / "artifacts"

        with self.assertRaises(DependencyReviewRequired):
            update_versions(
                self.root,
                151,
                152,
                "chrome/m152",
                parent_base_sha=self.parent_base_sha,
                skia_base_sha=self.skia_base_sha,
                artifact_dir=artifact_dir,
            )

        manifest_path = self.root / "cgmanifest.json"
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        registration = self.registration(manifest, "VulkanMemoryAllocator")
        metadata = registration["skia_dependency"]
        self.assertEqual("new-vma-sha", metadata["revision"])
        self.assertEqual(
            "https://example.test/VulkanMemoryAllocator@old-vma-sha",
            metadata["version_reviewed_identity"],
        )

        changes = json.loads(
            (artifact_dir / "skia-dependency-changes.json").read_text(encoding="utf-8")
        )
        self.assertEqual("vulkanmemoryallocator", changes["changes"][0]["name"])
        self.assertEqual("new-vma-sha", changes["changes"][0]["final"]["revision"])

        registration["component"]["other"]["version"] = "3.4.0"
        metadata["version_reviewed_identity"] = (
            "https://example.test/VulkanMemoryAllocator@new-vma-sha"
        )
        metadata["version_source"] = (
            "CMakeLists.txt: project(VMA VERSION 3.4.0 LANGUAGES CXX)"
        )
        manifest_path.write_text(json.dumps(manifest), encoding="utf-8")

        update_versions(
            self.root,
            151,
            152,
            "chrome/m152",
            parent_base_sha=self.parent_base_sha,
            skia_base_sha=self.skia_base_sha,
            artifact_dir=artifact_dir,
        )

    def test_dependency_change_can_keep_version_after_explicit_review(self) -> None:
        deps_path = self.root / "externals" / "skia" / "DEPS"
        deps_path.write_text(
            deps_path.read_text(encoding="utf-8").replace(
                "old-vma-sha", "new-vma-sha"
            ),
            encoding="utf-8",
        )
        with self.assertRaises(DependencyReviewRequired):
            update_versions(
                self.root,
                151,
                152,
                "chrome/m152",
                parent_base_sha=self.parent_base_sha,
                skia_base_sha=self.skia_base_sha,
            )

        manifest_path = self.root / "cgmanifest.json"
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        metadata = self.registration(
            manifest, "VulkanMemoryAllocator"
        )["skia_dependency"]
        metadata["version_reviewed_identity"] = (
            "https://example.test/VulkanMemoryAllocator@new-vma-sha"
        )
        metadata["version_source"] = "README.md: release remains 3.2.1"
        manifest_path.write_text(json.dumps(manifest), encoding="utf-8")

        update_versions(
            self.root,
            151,
            152,
            "chrome/m152",
            parent_base_sha=self.parent_base_sha,
            skia_base_sha=self.skia_base_sha,
        )

    def test_rejects_manifest_version_bump_without_dependency_change(self) -> None:
        manifest_path = self.root / "cgmanifest.json"
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        self.registration(
            manifest, "VulkanMemoryAllocator"
        )["component"]["other"]["version"] = "9.9.9"
        manifest_path.write_text(json.dumps(manifest), encoding="utf-8")

        with self.assertRaisesRegex(
            DependencyReviewRequired,
            "without a vulkanmemoryallocator DEPS change",
        ):
            update_versions(
                self.root,
                151,
                152,
                "chrome/m152",
                parent_base_sha=self.parent_base_sha,
                skia_base_sha=self.skia_base_sha,
            )

    def test_rejects_missing_version_source_without_dependency_change(self) -> None:
        dependency = {
            "path": "third_party/externals/vulkanmemoryallocator",
            "url": "https://example.test/VulkanMemoryAllocator",
            "revision": "vma-sha",
        }
        manifest = {
            "registrations": [
                {
                    "component": {
                        "type": "other",
                        "other": {
                            "name": "VulkanMemoryAllocator",
                            "version": "3.2.1",
                        },
                    },
                    "skia_dependency": {
                        "name": "vulkanmemoryallocator",
                        "revision": "vma-sha",
                        "version_reviewed_identity": (
                            "https://example.test/VulkanMemoryAllocator@vma-sha"
                        ),
                    },
                }
            ]
        }

        _, errors = reconcile_dependency_metadata(
            json.loads(json.dumps(manifest)),
            manifest,
            {"vulkanmemoryallocator": dependency},
            {"vulkanmemoryallocator": dependency},
        )

        self.assertIn(
            "vulkanmemoryallocator lacks skia_dependency.version_source for its "
            "current semantic version.",
            errors,
        )

    def test_disabled_dependency_requires_manifest_registration_removal(self) -> None:
        deps_path = self.root / "externals" / "skia" / "DEPS"
        deps_path.write_text(
            "\n".join(
                line
                for line in deps_path.read_text(encoding="utf-8").splitlines()
                if "vulkanmemoryallocator" not in line
            )
            + "\n",
            encoding="utf-8",
        )

        with self.assertRaisesRegex(
            DependencyReviewRequired,
            "disabled or removed from final DEPS",
        ):
            update_versions(
                self.root,
                151,
                152,
                "chrome/m152",
                parent_base_sha=self.parent_base_sha,
                skia_base_sha=self.skia_base_sha,
            )

        manifest_path = self.root / "cgmanifest.json"
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        manifest["registrations"] = [
            registration
            for registration in manifest["registrations"]
            if registration.get("component", {}).get("other", {}).get("name")
            != "VulkanMemoryAllocator"
        ]
        manifest_path.write_text(json.dumps(manifest), encoding="utf-8")

        update_versions(
            self.root,
            151,
            152,
            "chrome/m152",
            parent_base_sha=self.parent_base_sha,
            skia_base_sha=self.skia_base_sha,
        )

    def test_reenabled_dependency_reports_stale_base_metadata(self) -> None:
        base_manifest = {
            "registrations": [
                {
                    "component": {
                        "type": "other",
                        "other": {
                            "name": "VulkanMemoryAllocator",
                            "version": "3.2.1",
                        },
                    },
                    "skia_dependency": {
                        "name": "vulkanmemoryallocator",
                        "revision": "old-vma-sha",
                    },
                }
            ]
        }
        final_manifest = json.loads(json.dumps(base_manifest))
        final_deps = {
            "vulkanmemoryallocator": {
                "path": "third_party/externals/vulkanmemoryallocator",
                "url": "https://example.test/VulkanMemoryAllocator",
                "revision": "new-vma-sha",
            }
        }

        _, errors = reconcile_dependency_metadata(
            final_manifest,
            base_manifest,
            {},
            final_deps,
        )

        self.assertIn(
            "Base cgmanifest metadata for vulkanmemoryallocator does not match base DEPS.",
            errors,
        )

    def test_automation_uses_environment_without_arguments(self) -> None:
        exact_sha = subprocess.run(
            ["git", "rev-parse", "upstream/chrome/m152"],
            cwd=self.root / "externals" / "skia",
            check=True,
            capture_output=True,
            text=True,
        ).stdout.strip()
        environment = {
            "SKIA_SYNC_AUTOMATION": "1",
            "SKIA_SYNC_CURRENT": "151",
            "SKIA_SYNC_TARGET": "152",
            "SKIA_SYNC_UPSTREAM_REF": "chrome/m152",
            "SKIA_SYNC_TARGET_UPSTREAM_SHA": exact_sha,
            "SKIA_SYNC_PARENT_BASE_SHA": self.parent_base_sha,
            "SKIA_SYNC_SKIA_BASE_SHA": self.skia_base_sha,
            "SKIA_SYNC_ARTIFACT_DIR": str(self.root / "artifacts"),
        }
        arguments = [
            "update_versions.py",
            "--repo-root",
            str(self.root),
        ]

        with patch.dict(os.environ, environment, clear=False), patch.object(
            sys, "argv", arguments
        ):
            self.assertEqual(0, main())

        manifest = json.loads(
            (self.root / "cgmanifest.json").read_text(encoding="utf-8")
        )
        registration = manifest["registrations"][1]
        self.assertEqual("chrome/m152", registration["upstream_ref"])
        self.assertEqual(exact_sha, registration["upstream_merge_commit"])

if __name__ == "__main__":
    unittest.main()
