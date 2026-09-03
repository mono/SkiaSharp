import json
import os
from pathlib import Path
import stat
import subprocess
import tempfile
import textwrap
import unittest


SCRIPT = Path(__file__).resolve().parents[1] / "Prepare-SkiaReleaseBranches.ps1"
PARENT_BASE_SHA = "1" * 40
PARENT_HEAD_SHA = "2" * 40
NATIVE_BASE_SHA = "3" * 40


FAKE_GIT = textwrap.dedent(
    """\
    #!/usr/bin/env python3
    import json
    import os
    from pathlib import Path
    import sys

    args = sys.argv[1:]
    with Path(os.environ["FAKE_GIT_LOG"]).open("a", encoding="utf-8") as log:
        log.write(json.dumps(args) + "\\n")

    base_sha = os.environ["PARENT_BASE_SHA"]
    head_sha = os.environ["PARENT_HEAD_SHA"]
    native_sha = os.environ["NATIVE_BASE_SHA"]

    if args == ["rev-parse", "--show-toplevel"]:
        print(os.environ["FAKE_REPO_ROOT"])
    elif args[:2] == ["check-ref-format", "--branch"]:
        pass
    elif args == ["remote", "get-url", "origin"]:
        print("https://github.com/mono/SkiaSharp.git")
    elif args[:2] == ["fetch", "--no-tags"]:
        pass
    elif args[0] == "rev-parse":
        ref = args[1]
        if ref == "refs/remotes/origin/main^{commit}":
            print(base_sha)
        elif ref == "refs/remotes/origin/skia-sync/m153^{commit}":
            print(head_sha)
        else:
            raise SystemExit(f"Unexpected rev-parse ref: {ref}")
    elif args[0] == "show":
        commit, path = args[1].split(":", 1)
        if path == "cgmanifest.json":
            milestone = (
                os.environ["BASE_MILESTONE"]
                if commit == base_sha
                else os.environ["HEAD_MILESTONE"]
            )
            print(json.dumps({
                "registrations": [{
                    "component": {
                        "type": "other",
                        "other": {"name": "skia"},
                    },
                    "chrome_milestone": int(milestone),
                }]
            }))
        elif path == "scripts/VERSIONS.txt":
            product_line = (
                os.environ["BASE_PRODUCT_LINE"]
                if commit == base_sha
                else os.environ["HEAD_PRODUCT_LINE"]
            )
            print(f"SkiaSharp nuget {product_line}.0")
        else:
            raise SystemExit(f"Unexpected git show path: {path}")
    elif args[:2] == ["config", "--blob"]:
        print("https://github.com/mono/skia.git")
    elif args[0] == "ls-tree":
        print(f"160000 commit {native_sha}\\texternals/skia")
    elif args[:2] == ["ls-remote", "--heads"]:
        branch = args[3]
        if branch == "refs/heads/skiasharp":
            print(f"{native_sha}\\t{branch}")
    else:
        raise SystemExit(f"Unexpected git invocation: {args}")
    """
)


class PrepareSkiaReleaseBranchesTests(unittest.TestCase):
    def run_script(
        self,
        *,
        base_milestone: int,
        head_milestone: int,
        base_product_line: str,
        head_product_line: str,
    ) -> tuple[subprocess.CompletedProcess[str], list[list[str]]]:
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            bin_dir = root / "bin"
            bin_dir.mkdir()
            fake_git = bin_dir / "git"
            fake_git.write_text(FAKE_GIT, encoding="utf-8")
            fake_git.chmod(fake_git.stat().st_mode | stat.S_IXUSR)
            log_path = root / "git.log"

            env = os.environ.copy()
            env.update({
                "PATH": f"{bin_dir}{os.pathsep}{env['PATH']}",
                "FAKE_GIT_LOG": str(log_path),
                "FAKE_REPO_ROOT": str(root),
                "PARENT_BASE_SHA": PARENT_BASE_SHA,
                "PARENT_HEAD_SHA": PARENT_HEAD_SHA,
                "NATIVE_BASE_SHA": NATIVE_BASE_SHA,
                "BASE_MILESTONE": str(base_milestone),
                "HEAD_MILESTONE": str(head_milestone),
                "BASE_PRODUCT_LINE": base_product_line,
                "HEAD_PRODUCT_LINE": head_product_line,
            })
            result = subprocess.run(
                [
                    "pwsh",
                    "-NoLogo",
                    "-NoProfile",
                    "-File",
                    str(SCRIPT),
                    "-SkiaSharpBaseBranch",
                    "main",
                    "-SkiaSharpHeadBranch",
                    "skia-sync/m153",
                    "-SkiaBaseBranch",
                    "skiasharp",
                ],
                cwd=root,
                env=env,
                capture_output=True,
                text=True,
                check=False,
            )
            calls = [
                json.loads(line)
                for line in log_path.read_text(encoding="utf-8").splitlines()
            ]
            return result, calls

    def test_same_milestone_sync_skips_release_branch_preflight(self):
        result, calls = self.run_script(
            base_milestone=153,
            head_milestone=153,
            base_product_line="4.153",
            head_product_line="4.153",
        )

        self.assertEqual(0, result.returncode, result.stderr)
        self.assertIn("Same-milestone sync: m153 -> m153.", result.stdout)
        self.assertIn("No release branches are needed.", result.stdout)
        self.assertFalse(any(call[0] == "ls-remote" for call in calls))

    def test_milestone_bump_preflights_previous_product_line(self):
        result, calls = self.run_script(
            base_milestone=152,
            head_milestone=153,
            base_product_line="4.152",
            head_product_line="4.153",
        )

        self.assertEqual(0, result.returncode, result.stderr)
        self.assertIn("Milestone bump:     m152 -> m153 (4.152 -> 4.153)", result.stdout)
        self.assertIn("Release branch:     release/4.152.x", result.stdout)
        self.assertIn("DRY RUN: no remote refs were created.", result.stdout)
        self.assertTrue(any(call[0] == "ls-remote" for call in calls))

    def test_milestone_regression_is_rejected(self):
        result, _ = self.run_script(
            base_milestone=153,
            head_milestone=152,
            base_product_line="4.153",
            head_product_line="4.152",
        )

        self.assertNotEqual(0, result.returncode)
        self.assertIn("Skia milestone regresses from m153", result.stderr)

    def test_milestone_bump_requires_product_line_change(self):
        result, _ = self.run_script(
            base_milestone=152,
            head_milestone=153,
            base_product_line="4.152",
            head_product_line="4.152",
        )

        self.assertNotEqual(0, result.returncode)
        self.assertIn("Skia milestone changes from m152 to m153", result.stderr)
        self.assertIn("remains 4.152.", result.stderr)


if __name__ == "__main__":
    unittest.main()
