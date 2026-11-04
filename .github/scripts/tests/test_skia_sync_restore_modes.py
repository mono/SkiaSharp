"""Reproduce trusted config artifact permission loss in a real Git checkout."""

import importlib.util
from pathlib import Path
import stat
import subprocess
import tempfile
import unittest


SCRIPT = Path(__file__).resolve().parents[1] / "skia-sync-restore-modes.py"
SPEC = importlib.util.spec_from_file_location("restore_modes", SCRIPT)
RESTORE = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(RESTORE)


class RestoreModesTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.repo = self.root / "repo"
        self.repo.mkdir()
        self.git("init", "--initial-branch=main")
        self.git("config", "user.name", "Mode Test")
        self.git("config", "user.email", "test@example.invalid")
        self.git("config", "core.filemode", "true")
        self.files = {
            ".agents/skills/tool/run.py": 0o755,
            ".github/scripts/with space\nand newline.sh": 0o755,
            ".github/workflows/workflow.md": 0o644,
            "AGENTS.md": 0o644,
            "unrelated.sh": 0o755,
        }
        for name, mode in self.files.items():
            path = self.repo / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(f"committed {name}\n")
            path.chmod(mode)
        self.git("add", ".")
        self.git("commit", "-m", "Trusted snapshot")
        self.snapshot = self.git("rev-parse", "HEAD").strip()

    def git(self, *args, check=True):
        result = subprocess.run(
            ["git", "-C", str(self.repo), *args],
            check=check, capture_output=True, text=True,
        )
        return result.stdout if check else result

    def lose_execute_bits(self):
        for name in self.files:
            (self.repo / name).chmod(0o644)

    def test_artifact_permission_loss_restores_only_snapshot_paths(self):
        contents = {name: (self.repo / name).read_bytes() for name in self.files}
        untracked = self.repo / ".github/scripts/untracked.sh"
        untracked.write_text("untracked\n")
        untracked.chmod(0o644)
        self.lose_execute_bits()
        (self.repo / ".github/workflows/workflow.md").chmod(0o755)
        RESTORE.restore_modes(self.repo, self.snapshot)
        RESTORE.restore_modes(self.repo, self.snapshot)
        for name, mode in self.files.items():
            path = self.repo / name
            expected = 0o644 if name == "unrelated.sh" else mode
            self.assertEqual(expected, stat.S_IMODE(path.stat().st_mode))
            self.assertEqual(contents[name], path.read_bytes())
        self.assertEqual(0o644, stat.S_IMODE(untracked.stat().st_mode))
        self.assertEqual("unrelated.sh\n", self.git("diff", "--name-only"))
        self.assertEqual("", self.git("diff", "--cached"))

    def test_uses_snapshot_not_current_head_or_target_branch(self):
        script = self.repo / ".agents/skills/tool/run.py"
        script.chmod(0o644)
        self.git("commit", "-am", "Different target branch modes")
        self.lose_execute_bits()
        RESTORE.restore_modes(self.repo, self.snapshot)
        self.assertEqual(0o755, stat.S_IMODE(script.stat().st_mode))
        self.assertIn("mode change 100644 => 100755", self.git("diff", "--summary"))

    def test_content_changes_still_fail_clean_tree_check(self):
        self.lose_execute_bits()
        script = self.repo / ".agents/skills/tool/run.py"
        script.write_text("real semantic change\n")
        instructions = self.repo / "AGENTS.md"
        instructions.write_text("staged semantic change\n")
        self.git("add", "AGENTS.md")
        index_before = self.git("ls-files", "--stage")
        RESTORE.restore_modes(self.repo, self.snapshot)
        self.assertEqual("real semantic change\n", script.read_text())
        self.assertEqual("staged semantic change\n", instructions.read_text())
        self.assertEqual(index_before, self.git("ls-files", "--stage"))
        self.assertEqual(1, self.git("diff", "--quiet", check=False).returncode)
        self.assertEqual(1, self.git("diff", "--cached", "--quiet", check=False).returncode)
        self.assertIn(".agents/skills/tool/run.py", self.git("diff", "--name-only"))

    def test_file_symlink_cannot_change_external_permissions(self):
        external = self.root / "external"
        external.write_text("outside\n")
        external.chmod(0o600)
        script = self.repo / ".agents/skills/tool/run.py"
        script.unlink()
        script.symlink_to(external)
        with self.assertRaises(OSError):
            RESTORE.restore_modes(self.repo, self.snapshot)
        self.assertEqual(0o600, stat.S_IMODE(external.stat().st_mode))

    def test_parent_symlink_cannot_change_external_permissions(self):
        directory = self.repo / ".agents/skills/tool"
        outside = self.root / "outside"
        directory.rename(outside)
        (outside / "run.py").chmod(0o600)
        directory.symlink_to(outside, target_is_directory=True)
        with self.assertRaises(OSError):
            RESTORE.restore_modes(self.repo, self.snapshot)
        self.assertEqual(0o600, stat.S_IMODE((outside / "run.py").stat().st_mode))

    def test_missing_snapshot_file_or_revision_fails(self):
        with self.assertRaises(subprocess.CalledProcessError):
            RESTORE.restore_modes(self.repo, "0" * 40)
        (self.repo / "AGENTS.md").unlink()
        with self.assertRaises(FileNotFoundError):
            RESTORE.restore_modes(self.repo, self.snapshot)


if __name__ == "__main__":
    unittest.main()
