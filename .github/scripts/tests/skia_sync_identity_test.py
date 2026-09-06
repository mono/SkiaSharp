#!/usr/bin/env python3

import importlib.util
import json
import os
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch


SCRIPT = Path(__file__).resolve().parents[1] / "skia_sync_identity.py"
SPEC = importlib.util.spec_from_file_location("skia_sync_identity", SCRIPT)
IDENTITY = importlib.util.module_from_spec(SPEC)
assert SPEC.loader is not None
SPEC.loader.exec_module(IDENTITY)


class SyncIdentityTests(unittest.TestCase):
    def setUp(self) -> None:
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)

    def tearDown(self) -> None:
        self.temp.cleanup()

    def write_repository(self, owner: str) -> None:
        (self.root / ".gitmodules").write_text(
            '[submodule "externals/skia"]\n'
            "\tpath = externals/skia\n"
            f"\turl = https://github.com/{owner}/skia.git\n",
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
                                    "repositoryUrl": f"https://github.com/{owner}/skia.git",
                                    "commitHash": "abc",
                                },
                            }
                        }
                    ]
                }
            ),
            encoding="utf-8",
        )

    def test_resolves_current_and_destination_repositories(self) -> None:
        for owner in ("mono", "dotnet"):
            with self.subTest(owner=owner):
                self.write_repository(owner)
                identity = IDENTITY.resolve_identity(
                    self.root,
                    f"{owner}/SkiaSharp",
                )
                self.assertEqual(f"{owner}/SkiaSharp", identity["repository"])
                self.assertEqual(f"{owner}/skia", identity["skiaRepository"])
                IDENTITY.validate_manifest(self.root, identity["skiaGitUrl"])

    def test_requires_runtime_repository(self) -> None:
        self.write_repository("mono")
        with patch.dict(os.environ, {}, clear=True):
            with self.assertRaisesRegex(
                IDENTITY.SyncIdentityError,
                "GITHUB_REPOSITORY is required",
            ):
                IDENTITY.resolve_identity(self.root, None)

    def test_rejects_stale_transition_manifest_registration(self) -> None:
        self.write_repository("dotnet")
        manifest_path = self.root / "cgmanifest.json"
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        manifest["registrations"].append(
            {
                "component": {
                    "type": "git",
                    "git": {
                        "repositoryUrl": "https://github.com/mono/skia.git",
                        "commitHash": "old",
                    },
                }
            }
        )
        manifest_path.write_text(json.dumps(manifest), encoding="utf-8")

        with self.assertRaisesRegex(
            IDENTITY.SyncIdentityError,
            "exactly one paired Skia git registration",
        ):
            IDENTITY.validate_manifest(
                self.root,
                "https://github.com/dotnet/skia.git",
            )


if __name__ == "__main__":
    unittest.main()
