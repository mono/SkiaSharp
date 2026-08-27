from __future__ import annotations

import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))
sys.path.insert(0, str(Path(__file__).resolve().parent))

from release_git import GitRepository, GitError
import gitrepo_helpers as helpers


class GitRepositoryTests(unittest.TestCase):
    def setUp(self):
        self._tmp = tempfile.TemporaryDirectory()
        self.root = Path(self._tmp.name)

    def tearDown(self):
        self._tmp.cleanup()

    def test_discover_finds_repo_root(self):
        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "file.txt").write_text("hi", encoding="utf-8")
        helpers.commit_all(worktree, "init")
        nested = worktree / "sub"
        nested.mkdir()
        repo = GitRepository.discover(nested)
        self.assertEqual(repo.root, worktree.resolve())

    def test_ref_exists_and_remote_sha_round_trip(self):
        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "file.txt").write_text("hi", encoding="utf-8")
        sha = helpers.commit_all(worktree, "init")
        helpers.push(worktree)
        repo = GitRepository(root=worktree)
        repo.fetch()
        self.assertTrue(repo.ref_exists("refs/remotes/origin/main"))
        self.assertEqual(repo.remote_sha("main"), sha)
        self.assertIsNone(repo.remote_sha("does-not-exist"))

    def test_read_ref_file(self):
        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "file.txt").write_text("hello world", encoding="utf-8")
        helpers.commit_all(worktree, "init")
        repo = GitRepository(root=worktree)
        self.assertEqual(repo.read_ref_file("HEAD", "file.txt"), "hello world")

    def test_read_gitlink(self):
        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        skia_sha = "a" * 40
        helpers.add_gitlink(worktree, submodule_path="externals/skia", sha=skia_sha)
        (worktree / "root.txt").write_text("x", encoding="utf-8")
        helpers.stage(worktree, "root.txt")
        helpers.commit_staged(worktree, "init")
        repo = GitRepository(root=worktree)
        self.assertEqual(repo.read_gitlink("HEAD", "externals/skia"), skia_sha)

    def test_read_gitlink_rejects_non_gitlink_path(self):
        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "file.txt").write_text("hi", encoding="utf-8")
        helpers.commit_all(worktree, "init")
        repo = GitRepository(root=worktree)
        with self.assertRaises(GitError):
            repo.read_gitlink("HEAD", "file.txt")

    def test_merge_base_and_is_ancestor(self):
        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "file.txt").write_text("v1", encoding="utf-8")
        first = helpers.commit_all(worktree, "first")
        (worktree / "file.txt").write_text("v2", encoding="utf-8")
        second = helpers.commit_all(worktree, "second")
        repo = GitRepository(root=worktree)
        self.assertEqual(repo.merge_base(first, second), first)
        self.assertTrue(repo.is_ancestor(first, second))
        self.assertFalse(repo.is_ancestor(second, first))

    def test_require_clean_detects_dirty_worktree(self):
        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "file.txt").write_text("v1", encoding="utf-8")
        helpers.commit_all(worktree, "first")
        repo = GitRepository(root=worktree)
        repo.require_clean()  # must not raise
        (worktree / "file.txt").write_text("dirty", encoding="utf-8")
        with self.assertRaises(GitError):
            repo.require_clean()

    def test_release_branches_lists_remote_release_branches(self):
        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "file.txt").write_text("v1", encoding="utf-8")
        helpers.commit_all(worktree, "first")
        helpers.push(worktree)
        repo = GitRepository(root=worktree)
        repo.git("branch", "release/3.119.0-preview.1")
        repo.push_branch("release/3.119.0-preview.1")
        repo.fetch()
        self.assertEqual(repo.release_branches(), ["release/3.119.0-preview.1"])

    def test_push_tag_and_remote_tags(self):
        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "file.txt").write_text("v1", encoding="utf-8")
        sha = helpers.commit_all(worktree, "first")
        helpers.push(worktree)
        repo = GitRepository(root=worktree)
        repo.push_tag("v3.119.0", sha)
        self.assertEqual(repo.remote_tags().get("v3.119.0"), sha)

    def test_contains_commit(self):
        bare, worktree = helpers.create_bare_and_worktree(self.root, "repo")
        (worktree / "file.txt").write_text("v1", encoding="utf-8")
        first = helpers.commit_all(worktree, "first")
        (worktree / "file.txt").write_text("v2", encoding="utf-8")
        helpers.commit_all(worktree, "second")
        helpers.push(worktree)
        repo = GitRepository(root=worktree)
        repo.fetch()
        self.assertTrue(repo.contains_commit("refs/remotes/origin/main", first))


if __name__ == "__main__":
    unittest.main()
