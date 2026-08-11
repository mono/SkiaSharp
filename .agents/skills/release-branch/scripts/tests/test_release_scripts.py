#!/usr/bin/env python3

import importlib.util
import json
from pathlib import Path
import os
import subprocess
import sys
import tempfile
import unittest


SCRIPT_DIR = Path(__file__).resolve().parent.parent


def load(name):
    path = SCRIPT_DIR / f"{name}.py"
    spec = importlib.util.spec_from_file_location(name.replace("-", "_"), path)
    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    return module


release = load("create-release-branches")
detector = load("detect-release-version")
CREATE_SCRIPT = SCRIPT_DIR / "create-release-branches.py"
DETECT_SCRIPT = SCRIPT_DIR / "detect-release-version.py"


def git(cwd, *args):
    return subprocess.run(
        ["git", *args],
        cwd=cwd,
        check=True,
        capture_output=True,
        text=True,
    ).stdout.strip()


def write_release_files(root, skia="4.152.0", harfbuzz="14.2.1"):
    scripts = root / "scripts"
    scripts.mkdir()
    (scripts / "VERSIONS.txt").write_text(
        "SkiaSharp assembly 4.152.0.0\n"
        f"SkiaSharp file {skia}.0\n"
        "HarfBuzzSharp assembly 1.0.0.0\n"
        f"HarfBuzzSharp file {harfbuzz}\n"
        f"SkiaSharp nuget {skia}\n"
        f"SkiaSharp.NativeAssets.Linux nuget {skia}\n"
        f"HarfBuzzSharp nuget {harfbuzz}\n"
        f"HarfBuzzSharp.NativeAssets.Linux nuget {harfbuzz}\n",
        encoding="utf-8",
    )
    (scripts / "azure-templates-variables.yml").write_text(
        "variables:\n"
        f"  SKIASHARP_VERSION: {skia}\n"
        "  PREVIEW_LABEL: 'preview.0'\n",
        encoding="utf-8",
    )


def create_repository_pair(root, integration_branch=None):
    remote = root / "skiasharp.git"
    skia_remote = root / "skia.git"
    seed = root / "skiasharp-seed"
    skia_seed = root / "skia-seed"
    work = root / "work"
    seed.mkdir()
    skia_seed.mkdir()
    git(
        root,
        "init",
        "--bare",
        "--quiet",
        "--initial-branch=main",
        str(remote),
    )
    git(
        root,
        "init",
        "--bare",
        "--quiet",
        "--initial-branch=main",
        str(skia_remote),
    )

    git(skia_seed, "init", "--quiet", "-b", "main")
    git(skia_seed, "config", "user.name", "Test User")
    git(skia_seed, "config", "user.email", "test@example.com")
    (skia_seed / "README").write_text("skia\n", encoding="utf-8")
    git(skia_seed, "add", "README")
    git(skia_seed, "commit", "-m", "Initial skia")
    git(skia_seed, "remote", "add", "origin", str(skia_remote))
    git(skia_seed, "push", "-u", "origin", "main")
    skia_sha = git(skia_seed, "rev-parse", "HEAD")

    git(seed, "init", "--quiet", "-b", "main")
    git(seed, "config", "user.name", "Test User")
    git(seed, "config", "user.email", "test@example.com")
    git(
        seed,
        "-c",
        "protocol.file.allow=always",
        "submodule",
        "add",
        "--quiet",
        str(skia_remote),
        "externals/skia",
    )
    write_release_files(seed)
    git(seed, "add", ".gitmodules", "externals/skia", "scripts")
    git(seed, "commit", "-m", "Initial versions")
    if integration_branch:
        git(seed, "branch", integration_branch)
    git(seed, "remote", "add", "origin", str(remote))
    git(seed, "push", "-u", "origin", "main")
    if integration_branch:
        git(seed, "push", "-u", "origin", integration_branch)
    git(root, "clone", "--quiet", str(remote), str(work))
    git(work, "config", "user.name", "Test User")
    git(work, "config", "user.email", "test@example.com")
    environment = {
        **os.environ,
        "GIT_ALLOW_PROTOCOL": "file",
    }
    return work, skia_sha, environment


def create_fake_gh(root):
    bin_dir = root / "bin"
    bin_dir.mkdir()
    log = root / "gh.log"
    executable = bin_dir / "gh"
    executable.write_text(
        "#!/bin/sh\n"
        "printf '%s\\n' \"$*\" >> \"$GH_LOG\"\n"
        "if [ \"$1 $2\" = \"api repos/mono/SkiaSharp\" ]; then\n"
        "  echo '{\"allowAutoMerge\":true,\"allowSquashMerge\":true}'\n"
        "elif [ \"$1 $2\" = \"pr list\" ]; then\n"
        "  if [ -f \"$GH_LOG.pr\" ]; then\n"
        "    echo 'https://github.test/mono/SkiaSharp/pull/1'\n"
        "  fi\n"
        "elif [ \"$1 $2\" = \"pr create\" ]; then\n"
        "  : > \"$GH_LOG.pr\"\n"
        "  echo 'https://github.test/mono/SkiaSharp/pull/1'\n"
        "elif [ \"$1 $2\" = \"pr merge\" ]; then\n"
        "  exit 9\n"
        "fi\n"
        "exit 0\n",
        encoding="ascii",
    )
    executable.chmod(0o755)
    return bin_dir, log


class ReleaseScriptTests(unittest.TestCase):
    def test_next_preview(self):
        branches = [
            "release/4.152.0-preview.1",
            "release/4.151.0",
        ]
        self.assertEqual(
            detector.calculate_next_preview(
                "4.152.0",
                "preview.0",
                branches,
            ),
            "4.152.0-preview.2",
        )

    def test_next_preview_refuses_stable(self):
        with self.assertRaisesRegex(detector.DetectionError, "stable branch"):
            detector.calculate_next_preview(
                "4.152.0",
                "preview.0",
                ["release/4.152.0"],
            )

    def test_release_version_parsing(self):
        cases = {
            "4.152.0-preview.2": ("preview.2", "preview"),
            "4.152.0-rc.1": ("rc.1", "rc"),
            "4.152.0": ("stable", "stable"),
            "4.152.0.1-preview.1": ("preview.1", "hotfix preview"),
            "4.152.0.1": ("stable", "hotfix stable"),
        }
        for value, expected in cases.items():
            with self.subTest(value=value):
                parsed = release.parse_release_version(value)
                self.assertEqual(
                    (parsed.label, parsed.release_type),
                    expected,
                )
        for value in ("4.152.0-preview.0", "4.152.0-rc.0"):
            with self.subTest(value=value):
                with self.assertRaisesRegex(
                    release.ReleaseError,
                    "iterations must start at 1",
                ):
                    release.parse_release_version(value)

    def test_git_tree_paths_are_posix_strings(self):
        for path in (
            release.SKIA_PATH,
            release.VARIABLES_PATH,
            release.VERSIONS_PATH,
        ):
            self.assertIsInstance(path, str)
            self.assertNotIn("\\", path)

    def test_clean_check_ignores_unrelated_submodule_gitlink_drift(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            child = root / "child-source"
            parent = root / "parent"
            child.mkdir()
            parent.mkdir()
            git(child, "init", "--quiet", "-b", "main")
            git(child, "config", "user.name", "Test User")
            git(child, "config", "user.email", "test@example.com")
            (child / "value.txt").write_text("one\n", encoding="utf-8")
            git(child, "add", "value.txt")
            git(child, "commit", "-m", "One")
            first = git(child, "rev-parse", "HEAD")
            (child / "value.txt").write_text("two\n", encoding="utf-8")
            git(child, "commit", "-am", "Two")

            git(parent, "init", "--quiet", "-b", "main")
            git(parent, "config", "user.name", "Test User")
            git(parent, "config", "user.email", "test@example.com")
            git(
                parent,
                "-c",
                "protocol.file.allow=always",
                "submodule",
                "add",
                "--quiet",
                str(child),
                "externals/depot_tools",
            )
            (parent / "tracked.txt").write_text("clean\n", encoding="utf-8")
            git(parent, "add", ".")
            git(parent, "commit", "-m", "Parent")
            git(parent / "externals/depot_tools", "checkout", "--quiet", first)

            release.Repository(parent).require_clean()
            git(parent, "add", "externals/depot_tools")
            with self.assertRaisesRegex(
                release.ReleaseError,
                "staged changes",
            ):
                release.Repository(parent).require_clean()
            git(parent, "reset", "--quiet")
            (parent / "tracked.txt").write_text("dirty\n", encoding="utf-8")
            with self.assertRaisesRegex(
                release.ReleaseError,
                "working tree is not clean",
            ):
                release.Repository(parent).require_clean()

    def test_latest_hotfix_prerelease_prefers_rc(self):
        branches = [
            "release/4.151.0.1-preview.3",
            "release/4.151.0.1-rc.1",
        ]
        self.assertEqual(
            release.latest_prerelease_branch(
                branches,
                "4.151.0.1",
            ),
            "release/4.151.0.1-rc.1",
        )

    def test_harfbuzz_increment(self):
        self.assertEqual(
            release.increment_harfbuzz("14.2.1"),
            "14.2.1.1",
        )
        self.assertEqual(
            release.increment_harfbuzz("14.2.1.1"),
            "14.2.1.2",
        )

    def test_next_versions(self):
        self.assertEqual(
            release.calculate_next_versions("4.151.2", "14.2.1.2"),
            ("4.151.3", "14.2.1.3"),
        )

    def test_update_version_files_supports_four_part_hotfix(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            write_release_files(root)

            changed = release.update_version_files(
                root,
                preview_label="preview.1",
                skia_version="4.152.0.1",
                harfbuzz_version="14.2.1.1",
            )

            self.assertEqual(
                set(changed),
                {
                    "scripts/VERSIONS.txt",
                    "scripts/azure-templates-variables.yml",
                },
            )
            updated = (root / "scripts/VERSIONS.txt").read_text(encoding="utf-8")
            self.assertIn("SkiaSharp file 4.152.0.1", updated)
            self.assertIn("SkiaSharp nuget 4.152.0.1", updated)
            self.assertIn("HarfBuzzSharp nuget 14.2.1.1", updated)

    def test_update_version_files_label_only(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            write_release_files(root)

            changed = release.update_version_files(
                root,
                preview_label="preview.1",
            )

            self.assertEqual(
                changed,
                ["scripts/azure-templates-variables.yml"],
            )
            variables = (
                root / "scripts/azure-templates-variables.yml"
            ).read_text(encoding="utf-8")
            self.assertIn("PREVIEW_LABEL: 'preview.1'", variables)

    def test_main_preview_detection_and_execution(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            work, skia_sha, environment = create_repository_pair(root)
            before = git(work, "rev-parse", "HEAD")

            detection = subprocess.run(
                [
                    sys.executable,
                    str(DETECT_SCRIPT),
                    "main",
                ],
                cwd=work,
                env=environment,
                check=True,
                capture_output=True,
                text=True,
            ).stdout
            self.assertIn(
                '"releaseVersion": "4.152.0-preview.1"',
                detection,
            )
            self.assertIn("create-release-branches.py", detection)
            self.assertEqual(git(work, "branch", "--show-current"), "main")
            self.assertEqual(git(work, "rev-parse", "HEAD"), before)
            self.assertEqual(git(work, "status", "--porcelain"), "")

            dry_run = subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "4.152.0-preview.1",
                    "--dryrun",
                ],
                cwd=work,
                env=environment,
                check=True,
                capture_output=True,
                text=True,
            ).stdout
            dry_plan = json.loads(dry_run)
            self.assertIn('"version": "4.152.0-preview.1"', dry_run)
            self.assertIn('"SkiaSharp": "missing"', dry_run)
            self.assertEqual(git(work, "branch", "--show-current"), "main")
            self.assertEqual(git(work, "rev-parse", "HEAD"), before)
            self.assertEqual(git(work, "status", "--porcelain"), "")

            subprocess.run(
                [
                    "git",
                    "submodule",
                    "update",
                    "--init",
                    "--recursive",
                    "--checkout",
                    "--",
                    "externals/skia",
                ],
                cwd=work,
                env=environment,
                check=True,
                capture_output=True,
                text=True,
            )
            expected_skia_url = git(
                work,
                "config",
                "-f",
                ".gitmodules",
                "--get",
                "submodule.externals/skia.url",
            )
            git(
                work / "externals/skia",
                "remote",
                "set-url",
                "origin",
                str(root / "stale-skia.git"),
            )

            execution = subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "4.152.0-preview.1",
                    "--expect-base-sha",
                    dry_plan["baseSha"],
                    "--expect-skia-sha",
                    dry_plan["skiaSha"],
                ],
                cwd=work,
                env=environment,
                check=True,
                capture_output=True,
                text=True,
            ).stdout
            self.assertEqual(
                git(work, "branch", "--show-current"),
                "release/4.152.0-preview.1",
            )
            self.assertNotEqual(git(work, "rev-parse", "HEAD"), before)
            self.assertEqual(git(work, "status", "--porcelain"), "")
            self.assertEqual(
                git(work / "externals/skia", "branch", "--show-current"),
                "release/4.152.0-preview.1",
            )
            self.assertEqual(
                git(work / "externals/skia", "rev-parse", "HEAD"),
                skia_sha,
            )
            self.assertEqual(
                git(
                    work / "externals/skia",
                    "remote",
                    "get-url",
                    "origin",
                ),
                expected_skia_url,
            )
            self.assertIn('"SkiaSharp": "matching"', execution)
            self.assertIn('"mono/skia": "matching"', execution)
            result = json.loads(execution)
            self.assertEqual(
                result["afterPush"]["remoteState"],
                {"SkiaSharp": "matching", "mono/skia": "matching"},
            )

            audit = json.loads(
                subprocess.run(
                    [
                        sys.executable,
                        str(CREATE_SCRIPT),
                        "4.152.0-preview.1",
                        "--dry-run",
                    ],
                    cwd=work,
                    env=environment,
                    check=True,
                    capture_output=True,
                    text=True,
                ).stdout
            )
            self.assertTrue(
                all(
                    operation["status"] == "done"
                    for operation in audit["operations"]
                )
            )

            git(work, "switch", "main")
            (work / "after-release.txt").write_text(
                "integration advanced\n",
                encoding="utf-8",
            )
            git(work, "add", "after-release.txt")
            git(work, "commit", "-m", "Advance integration")
            git(work, "push", "origin", "main")
            audit_after_advance = json.loads(
                subprocess.run(
                    [
                        sys.executable,
                        str(CREATE_SCRIPT),
                        "4.152.0-preview.1",
                        "--dry-run",
                    ],
                    cwd=work,
                    env=environment,
                    check=True,
                    capture_output=True,
                    text=True,
                ).stdout
            )
            self.assertEqual(
                audit_after_advance["baseSha"],
                dry_plan["baseSha"],
            )
            self.assertTrue(
                all(
                    operation["status"] == "done"
                    for operation in audit_after_advance["operations"]
                )
            )

    def test_maintenance_line_detects_next_preview(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            work, _, environment = create_repository_pair(
                root,
                integration_branch="release/4.152.x",
            )
            before = git(work, "rev-parse", "HEAD")

            detection = subprocess.run(
                [
                    sys.executable,
                    str(DETECT_SCRIPT),
                    "release/4.152.x",
                ],
                cwd=work,
                env=environment,
                check=True,
                capture_output=True,
                text=True,
            ).stdout

            self.assertIn(
                '"integrationBranch": "release/4.152.x"',
                detection,
            )
            self.assertIn(
                '"releaseVersion": "4.152.0-preview.1"',
                detection,
            )
            self.assertEqual(git(work, "branch", "--show-current"), "main")
            self.assertEqual(git(work, "rev-parse", "HEAD"), before)
            self.assertEqual(git(work, "status", "--porcelain"), "")

    def test_partial_push_is_audited_and_resumable(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            work, _, environment = create_repository_pair(root)
            plan = json.loads(
                subprocess.run(
                    [
                        sys.executable,
                        str(CREATE_SCRIPT),
                        "4.152.0-preview.1",
                        "--dry-run",
                    ],
                    cwd=work,
                    env=environment,
                    check=True,
                    capture_output=True,
                    text=True,
                ).stdout
            )
            parent_remote = Path(git(work, "remote", "get-url", "origin"))
            hook = parent_remote / "hooks/pre-receive"
            hook.write_text("#!/bin/sh\nexit 1\n", encoding="ascii")
            hook.chmod(0o755)

            failed = subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "4.152.0-preview.1",
                    "--expect-base-sha",
                    plan["baseSha"],
                    "--expect-skia-sha",
                    plan["skiaSha"],
                ],
                cwd=work,
                env=environment,
                capture_output=True,
                text=True,
            )
            self.assertNotEqual(failed.returncode, 0)
            self.assertTrue(
                git(
                    work / "externals/skia",
                    "ls-remote",
                    "--heads",
                    "origin",
                    "refs/heads/release/4.152.0-preview.1",
                )
            )
            self.assertEqual(
                git(
                    work,
                    "ls-remote",
                    "--heads",
                    "origin",
                    "refs/heads/release/4.152.0-preview.1",
                ),
                "",
            )
            hook.unlink()

            git(work, "switch", "main")
            (work / "integration-moved.txt").write_text(
                "moved\n",
                encoding="utf-8",
            )
            git(work, "add", "integration-moved.txt")
            git(work, "commit", "-m", "Move integration after partial push")
            git(work, "push", "origin", "main")
            git(work, "switch", "release/4.152.0-preview.1")

            audit = json.loads(
                subprocess.run(
                    [
                        sys.executable,
                        str(CREATE_SCRIPT),
                        "4.152.0-preview.1",
                        "--dry-run",
                    ],
                    cwd=work,
                    env=environment,
                    check=True,
                    capture_output=True,
                    text=True,
                ).stdout
            )
            statuses = {
                operation["id"]: operation["status"]
                for operation in audit["operations"]
            }
            self.assertEqual(
                statuses,
                {
                    "skiasharp-release-commit": "done",
                    "skia-release-branch": "done",
                    "skiasharp-release-push": "pending",
                },
            )
            self.assertEqual(len(audit["warnings"]), 1)
            self.assertIn("integration tip is now", audit["warnings"][0])

            subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "4.152.0-preview.1",
                    "--expect-base-sha",
                    audit["baseSha"],
                    "--expect-skia-sha",
                    audit["skiaSha"],
                ],
                cwd=work,
                env=environment,
                check=True,
                capture_output=True,
                text=True,
            )
            self.assertTrue(
                git(
                    work,
                    "ls-remote",
                    "--heads",
                    "origin",
                    "refs/heads/release/4.152.0-preview.1",
                )
            )

    def test_prepared_release_rejects_uncommitted_extra_edit(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            work, _, environment = create_repository_pair(root)
            plan = json.loads(
                subprocess.run(
                    [
                        sys.executable,
                        str(CREATE_SCRIPT),
                        "4.152.0-preview.1",
                        "--dry-run",
                    ],
                    cwd=work,
                    env=environment,
                    check=True,
                    capture_output=True,
                    text=True,
                ).stdout
            )
            subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "4.152.0-preview.1",
                    "--expect-base-sha",
                    plan["baseSha"],
                    "--expect-skia-sha",
                    plan["skiaSha"],
                ],
                cwd=work,
                env=environment,
                check=True,
                capture_output=True,
                text=True,
            )
            variables = work / release.VARIABLES_PATH
            variables.write_text(
                variables.read_text(encoding="utf-8")
                + "  VERBOSITY: detailed\n",
                encoding="utf-8",
            )

            audit = subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "4.152.0-preview.1",
                    "--dry-run",
                ],
                cwd=work,
                env=environment,
                capture_output=True,
                text=True,
            )
            self.assertNotEqual(audit.returncode, 0)
            self.assertIn(
                "already prepared but has uncommitted changes",
                audit.stderr,
            )

    def test_uncommitted_version_update_is_audited_and_completed(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            work, _, environment = create_repository_pair(root)
            plan = json.loads(
                subprocess.run(
                    [
                        sys.executable,
                        str(CREATE_SCRIPT),
                        "4.152.0-preview.1",
                        "--dry-run",
                    ],
                    cwd=work,
                    env=environment,
                    check=True,
                    capture_output=True,
                    text=True,
                ).stdout
            )
            git(
                work,
                "switch",
                "-c",
                "release/4.152.0-preview.1",
                plan["baseSha"],
            )
            release.update_version_files(
                work,
                preview_label="preview.1",
            )

            audit = json.loads(
                subprocess.run(
                    [
                        sys.executable,
                        str(CREATE_SCRIPT),
                        "4.152.0-preview.1",
                        "--dry-run",
                    ],
                    cwd=work,
                    env=environment,
                    check=True,
                    capture_output=True,
                    text=True,
                ).stdout
            )
            commit_operation = next(
                operation
                for operation in audit["operations"]
                if operation["id"] == "skiasharp-release-commit"
            )
            self.assertEqual(commit_operation["status"], "pending")
            self.assertIn("commit", commit_operation["detail"])

            subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "4.152.0-preview.1",
                    "--expect-base-sha",
                    audit["baseSha"],
                    "--expect-skia-sha",
                    audit["skiaSha"],
                ],
                cwd=work,
                env=environment,
                check=True,
                capture_output=True,
                text=True,
            )
            self.assertEqual(git(work, "status", "--porcelain"), "")
            self.assertTrue(
                git(
                    work,
                    "ls-remote",
                    "--heads",
                    "origin",
                    "refs/heads/release/4.152.0-preview.1",
                )
            )

    def test_untrusted_local_release_branch_is_rejected(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            work, _, environment = create_repository_pair(root)
            git(work, "switch", "-c", "release/4.152.0-preview.1")
            release.update_version_files(
                work,
                preview_label="preview.1",
            )
            git(work, "add", "scripts")
            git(work, "commit", "-m", "Locally prepared release")

            result = subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "4.152.0-preview.1",
                    "--dry-run",
                ],
                cwd=work,
                env=environment,
                capture_output=True,
                text=True,
            )

            self.assertNotEqual(result.returncode, 0)
            self.assertIn(
                "lacks release automation provenance",
                result.stderr,
            )

    def test_execution_rejects_base_that_moved_after_dry_run(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            work, _, environment = create_repository_pair(root)
            plan = json.loads(
                subprocess.run(
                    [
                        sys.executable,
                        str(CREATE_SCRIPT),
                        "4.152.0-preview.1",
                        "--dry-run",
                    ],
                    cwd=work,
                    env=environment,
                    check=True,
                    capture_output=True,
                    text=True,
                ).stdout
            )
            (work / "advance.txt").write_text("advance\n", encoding="utf-8")
            git(work, "add", "advance.txt")
            git(work, "commit", "-m", "Advance main")
            git(work, "push", "origin", "main")

            result = subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "4.152.0-preview.1",
                    "--expect-base-sha",
                    plan["baseSha"],
                    "--expect-skia-sha",
                    plan["skiaSha"],
                ],
                cwd=work,
                env=environment,
                capture_output=True,
                text=True,
            )
            self.assertNotEqual(result.returncode, 0)
            self.assertIn("base moved", result.stderr)
            self.assertFalse(
                git(
                    work,
                    "ls-remote",
                    "--heads",
                    "origin",
                    "refs/heads/release/4.152.0-preview.1",
                )
            )

    def test_hotfix_preview_plan_uses_stable_tag(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            work, _, environment = create_repository_pair(root)
            variables = work / "scripts/azure-templates-variables.yml"
            variables.write_text(
                variables.read_text(encoding="utf-8").replace(
                    "'preview.0'",
                    "'stable'",
                ),
                encoding="utf-8",
            )
            git(work, "add", str(variables.relative_to(work)))
            git(work, "commit", "-m", "Stable baseline")
            git(work, "tag", "v4.152.0")
            before = git(work, "rev-parse", "HEAD")

            plan = subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "4.152.0.1-preview.1",
                    "--dry-run",
                ],
                cwd=work,
                env=environment,
                check=True,
                capture_output=True,
                text=True,
            ).stdout

            self.assertIn('"type": "hotfix preview"', plan)
            self.assertIn('"baseRef": "refs/tags/v4.152.0"', plan)
            self.assertIn('"skiaSharpVersion": "4.152.0.1"', plan)
            self.assertIn('"harfBuzzSharpVersion": "14.2.1.1"', plan)
            self.assertIn('"requiresPackageBump": true', plan)
            self.assertEqual(git(work, "branch", "--show-current"), "main")
            self.assertEqual(git(work, "rev-parse", "HEAD"), before)

    def test_release_candidate_plan_uses_maintenance_line(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            work, _, environment = create_repository_pair(
                root,
                integration_branch="release/4.152.x",
            )

            plan = subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "4.152.0-rc.1",
                    "--dry-run",
                ],
                cwd=work,
                env=environment,
                check=True,
                capture_output=True,
                text=True,
            ).stdout

            self.assertIn('"type": "rc"', plan)
            self.assertIn(
                '"baseRef": "refs/remotes/origin/release/4.152.x"',
                plan,
            )
            self.assertIn('"previewLabel": "rc.1"', plan)

    def test_hotfix_stable_plan_uses_prerelease_branch(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            work, _, environment = create_repository_pair(root)
            prerelease = "release/4.152.0.1-preview.1"
            git(work, "switch", "-c", prerelease)
            release.update_version_files(
                work,
                preview_label="preview.1",
                skia_version="4.152.0.1",
                harfbuzz_version="14.2.1.1",
            )
            git(work, "add", "scripts")
            git(work, "commit", "-m", "Hotfix preview")
            git(work, "push", "-u", "origin", prerelease)
            git(work, "switch", "main")

            plan = subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "4.152.0.1",
                    "--dry-run",
                ],
                cwd=work,
                env=environment,
                check=True,
                capture_output=True,
                text=True,
            ).stdout

            self.assertIn('"type": "hotfix stable"', plan)
            self.assertIn(
                '"baseRef": '
                '"refs/remotes/origin/release/4.152.0.1-preview.1"',
                plan,
            )
            self.assertIn('"requiresPackageBump": false', plan)

    def test_executor_rejects_integration_branch(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            work, _, environment = create_repository_pair(root)

            result = subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "main",
                    "--dry-run",
                ],
                cwd=work,
                env=environment,
                capture_output=True,
                text=True,
            )

            self.assertNotEqual(result.returncode, 0)
            self.assertIn("version must be", result.stderr)

    def test_stable_release_creates_bump_pr_for_user_merge(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            work, _, environment = create_repository_pair(
                root,
                integration_branch="release/4.152.x",
            )
            bin_dir, gh_log = create_fake_gh(root)
            environment["PATH"] = f"{bin_dir}{os.pathsep}{environment['PATH']}"
            environment["GH_LOG"] = str(gh_log)

            dry_run = subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "4.152.0",
                    "--dry-run",
                ],
                cwd=work,
                env=environment,
                check=True,
                capture_output=True,
                text=True,
            ).stdout
            dry_plan = json.loads(dry_run)
            self.assertIn('"bumpBranch": "bump-version-4.152.1"', dry_run)
            self.assertIn("open a PR", dry_run)

            execution = subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "4.152.0",
                    "--expect-base-sha",
                    dry_plan["baseSha"],
                    "--expect-skia-sha",
                    dry_plan["skiaSha"],
                ],
                cwd=work,
                env=environment,
                check=True,
                capture_output=True,
                text=True,
            ).stdout

            self.assertEqual(
                git(work, "branch", "--show-current"),
                "bump-version-4.152.1",
            )
            self.assertTrue(
                git(
                    work,
                    "ls-remote",
                    "--heads",
                    "origin",
                    "refs/heads/release/4.152.0",
                )
            )
            self.assertTrue(
                git(
                    work / "externals/skia",
                    "ls-remote",
                    "--heads",
                    "origin",
                    "refs/heads/release/4.152.0",
                )
            )
            self.assertTrue(
                git(
                    work,
                    "ls-remote",
                    "--heads",
                    "origin",
                    "refs/heads/bump-version-4.152.1",
                )
            )
            calls = gh_log.read_text(encoding="ascii")
            self.assertIn("auth status --hostname github.com", calls)
            self.assertIn(
                "pr create --base release/4.152.x "
                "--head bump-version-4.152.1",
                calls,
            )
            self.assertNotIn("pr merge", calls)
            self.assertIn("postStablePullRequest", execution)

            audit = json.loads(
                subprocess.run(
                    [
                        sys.executable,
                        str(CREATE_SCRIPT),
                        "4.152.0",
                        "--dry-run",
                    ],
                    cwd=work,
                    env=environment,
                    check=True,
                    capture_output=True,
                    text=True,
                ).stdout
            )
            statuses = {
                operation["id"]: operation["status"]
                for operation in audit["operations"]
            }
            self.assertEqual(
                statuses,
                {
                    "skiasharp-release-commit": "done",
                    "skia-release-branch": "done",
                    "skiasharp-release-push": "done",
                    "post-stable-version-pr": "awaiting-user",
                },
            )
            self.assertEqual(
                audit["postStableBump"]["pullRequest"],
                "https://github.test/mono/SkiaSharp/pull/1",
            )

    def test_untrusted_local_stable_bump_branch_is_rejected(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            work, _, environment = create_repository_pair(
                root,
                integration_branch="release/4.152.x",
            )
            bin_dir, gh_log = create_fake_gh(root)
            environment["PATH"] = f"{bin_dir}{os.pathsep}{environment['PATH']}"
            environment["GH_LOG"] = str(gh_log)
            plan = json.loads(
                subprocess.run(
                    [
                        sys.executable,
                        str(CREATE_SCRIPT),
                        "4.152.0",
                        "--dry-run",
                    ],
                    cwd=work,
                    env=environment,
                    check=True,
                    capture_output=True,
                    text=True,
                ).stdout
            )
            git(
                work,
                "switch",
                "-c",
                "bump-version-4.152.1",
                "origin/release/4.152.x",
            )
            (work / "unrelated.txt").write_text(
                "not version metadata\n",
                encoding="utf-8",
            )
            git(work, "add", "unrelated.txt")
            git(work, "commit", "-m", "Unrelated change")
            release.update_version_files(
                work,
                preview_label="preview.0",
                skia_version="4.152.1",
                harfbuzz_version="14.2.1.1",
            )
            git(work, "add", "scripts")
            git(work, "commit", "-m", "Bump")

            result = subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "4.152.0",
                    "--expect-base-sha",
                    plan["baseSha"],
                    "--expect-skia-sha",
                    plan["skiaSha"],
                ],
                cwd=work,
                env=environment,
                capture_output=True,
                text=True,
            )
            self.assertNotEqual(result.returncode, 0)
            self.assertIn(
                "contains 2 commits after its integration base",
                result.stderr,
            )
            self.assertEqual(
                git(
                    work,
                    "ls-remote",
                    "--heads",
                    "origin",
                    "refs/heads/bump-version-4.152.1",
                ),
                "",
            )

    def test_prepare_release_branch_rejects_dirty_tree(self):
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            git(root, "init", "--quiet", "-b", "main")
            git(root, "config", "user.name", "Test User")
            git(root, "config", "user.email", "test@example.com")
            (root / "tracked.txt").write_text("clean\n", encoding="utf-8")
            git(root, "add", "tracked.txt")
            git(root, "commit", "-m", "Initial")
            (root / "tracked.txt").write_text("dirty\n", encoding="utf-8")

            result = subprocess.run(
                [
                    sys.executable,
                    str(CREATE_SCRIPT),
                    "4.152.0-preview.1",
                ],
                cwd=root,
                capture_output=True,
                text=True,
            )

            self.assertNotEqual(result.returncode, 0)
            self.assertIn("working tree is not clean", result.stderr)

    def test_python_scripts_are_ascii_only(self):
        for path in SCRIPT_DIR.glob("*.py"):
            path.read_text(encoding="ascii")


if __name__ == "__main__":
    unittest.main()
