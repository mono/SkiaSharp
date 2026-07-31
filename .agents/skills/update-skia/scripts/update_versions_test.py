import json
import subprocess
import tempfile
import unittest
from pathlib import Path

from update_versions import update_versions


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

    def tearDown(self) -> None:
        self.temporary_directory.cleanup()

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
        self.assertNotEqual("old", cgmanifest["registrations"][1]["upstream_merge_commit"])

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


if __name__ == "__main__":
    unittest.main()
