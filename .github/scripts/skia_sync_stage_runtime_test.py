import shutil
import tempfile
import unittest
from pathlib import Path

from skia_sync_stage_runtime import REQUIRED_SKILL_ASSETS, stage_runtime_assets


class StageRuntimeAssetsTests(unittest.TestCase):
    def test_staged_skill_survives_release_checkout_replacement(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            root = Path(temporary_directory)
            repo_root = root / "product"
            skill_root = repo_root / ".agents" / "skills" / "update-skia"
            push_helper = (
                repo_root / ".github" / "scripts" / "skia-sync-push-prs.sh"
            )
            for relative_path in REQUIRED_SKILL_ASSETS:
                source = skill_root / relative_path
                source.parent.mkdir(parents=True, exist_ok=True)
                source.write_text(f"main:{relative_path}\n", encoding="utf-8")
            push_helper.parent.mkdir(parents=True, exist_ok=True)
            push_helper.write_text("main:push-helper\n", encoding="utf-8")

            runtime_dir = root / "runner-temp" / "skia-sync-runtime"
            github_env = root / "github-env"
            staged_skill = stage_runtime_assets(
                repo_root, runtime_dir, github_env
            )

            shutil.rmtree(repo_root / ".agents")

            for relative_path in REQUIRED_SKILL_ASSETS:
                self.assertEqual(
                    f"main:{relative_path}\n",
                    (staged_skill / relative_path).read_text(encoding="utf-8"),
                )
            self.assertEqual(
                "main:push-helper\n",
                (runtime_dir / "skia-sync-push-prs.sh").read_text(
                    encoding="utf-8"
                ),
            )
            self.assertEqual(
                [
                    f"SKIA_SYNC_RUNTIME_DIR={runtime_dir.resolve()}",
                    f"SKIA_SYNC_SKILL_DIR={staged_skill}",
                    (
                        "SKIA_SYNC_VERSION_HELPER="
                        f"{staged_skill / 'scripts' / 'update_versions.py'}"
                    ),
                ],
                github_env.read_text(encoding="utf-8").splitlines(),
            )


if __name__ == "__main__":
    unittest.main()
