"""Exercise the workflow's tar hooks across artifact-style permission loss."""

import os
from pathlib import Path
import shutil
import stat
import subprocess
import tempfile
import unittest

import yaml


WORKFLOW = Path(__file__).resolve().parents[2] / "workflows/auto-skia-sync.md"
LINES = WORKFLOW.read_text().splitlines()
SOURCE = "\n".join(LINES[1:LINES.index("---", 1)])
CONFIG = yaml.safe_load(SOURCE)
ARCHIVE = CONFIG["jobs"]["activation"]["steps"][0]["run"]
EXTRACT = CONFIG["steps"][0]["run"]


class ConfigArchiveTests(unittest.TestCase):
    def run_command(self, *args, **kwargs):
        return subprocess.run(args, check=True, capture_output=True, **kwargs)

    def run_hook(self, script, runtime):
        # Remap only the framework's fixed temp root to this disposable fixture.
        self.assertIn("/tmp/gh-aw/", script)
        script = script.replace("/tmp/gh-aw/", '"$TEST_RUNTIME"/')
        return self.run_command(
            "bash", "-e", "-c", script,
            env=os.environ | {"TEST_RUNTIME": str(runtime)},
        )

    def test_trusted_snapshot_round_trip_preserves_modes_contents_and_symlinks(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            repo = root / "repo"
            repo.mkdir()
            git = lambda *args: self.run_command("git", "-C", str(repo), *args)
            git("init", "--initial-branch=main")
            git("config", "user.name", "Archive Test")
            git("config", "user.email", "test@example.invalid")
            git("config", "core.filemode", "true")
            files = {
                ".agents/skills/tool/run.py": 0o755,
                ".github/scripts/with space\nand newline.sh": 0o755,
                ".github/workflows/workflow.md": 0o644,
                "AGENTS.md": 0o644,
                "unrelated.sh": 0o755,
            }
            for name, mode in files.items():
                path = repo / name
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text(f"committed {name}\n")
                path.chmod(mode)
            external = root / "outside"
            external.write_text("outside the snapshot\n")
            external.chmod(0o600)
            (repo / ".agents/external-link").symlink_to(external)
            git("add", ".")
            git("commit", "-m", "Trusted snapshot")
            activation = root / "activation"
            snapshot = activation / "base"
            snapshot.mkdir(parents=True)
            for name in (".agents", ".github"):
                shutil.copytree(repo / name, snapshot / name, symlinks=True)
            shutil.copy2(repo / "AGENTS.md", snapshot / "AGENTS.md")
            self.run_hook(ARCHIVE, activation)
            self.assertEqual(["agent-config.tar"], [p.name for p in snapshot.iterdir()])

            agent = root / "agent"
            # Artifact transport carries the opaque tar file with mode 0644.
            shutil.copytree(snapshot, agent / "base", copy_function=shutil.copyfile)
            (agent / "base/agent-config.tar").chmod(0o644)
            self.run_hook(EXTRACT, agent)
            (repo / "unrelated.sh").write_text("unrelated change\n")
            (repo / "AGENTS.md").write_text("checkout content to replace\n")
            for name in (".agents", ".github"):
                shutil.rmtree(repo / name)
                # BSD cp needs -P to match the Linux runtime's cp -r symlink behavior.
                self.run_command("cp", "-RP", str(agent / "base" / name), str(repo / name))
            self.run_command("cp", str(agent / "base/AGENTS.md"), str(repo / "AGENTS.md"))
            git("diff", "--exit-code", "--", ".agents", ".github", "AGENTS.md")
            git("diff", "--cached", "--exit-code")
            for name, mode in files.items():
                self.assertEqual(mode, stat.S_IMODE((repo / name).stat().st_mode))
                if name != "unrelated.sh":
                    self.assertEqual(f"committed {name}\n", (repo / name).read_text())
            self.assertTrue((repo / ".agents/external-link").is_symlink())
            self.assertEqual(0o600, stat.S_IMODE(external.stat().st_mode))
            self.assertEqual("outside the snapshot\n", external.read_text())
            self.assertEqual("unrelated change\n", (repo / "unrelated.sh").read_text())
            (repo / ".agents/skills/tool/run.py").write_text("agent semantic change\n")
            self.assertNotEqual(
                0,
                subprocess.run(
                    ["git", "-C", str(repo), "diff", "--quiet", "--", ".agents"],
                    check=False,
                ).returncode,
            )

    def test_missing_archive_fails(self):
        with tempfile.TemporaryDirectory() as temporary:
            with self.assertRaises(subprocess.CalledProcessError):
                self.run_hook(EXTRACT, Path(temporary))


if __name__ == "__main__":
    unittest.main()
