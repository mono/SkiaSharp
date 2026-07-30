import subprocess
import tempfile
import unittest
from pathlib import Path

from audit_fork_patches import (
    compute_audit,
    normalize_patch,
    read_decisions,
    render,
    validate,
)


def run(root: Path, *args: str) -> str:
    return subprocess.run(
        [*args],
        cwd=root,
        check=True,
        capture_output=True,
        text=True,
    ).stdout.strip()


class ForkPatchAuditTests(unittest.TestCase):
    def setUp(self) -> None:
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)
        run(self.root, "git", "init", "-q")
        run(self.root, "git", "config", "user.email", "audit@example.com")
        run(self.root, "git", "config", "user.name", "Audit")

        self.write("lost.txt", "base\n")
        self.write("changed.txt", "base\n")
        self.write("stable.txt", "".join(f"line {index}\n" for index in range(10)))
        self.commit("old upstream")
        run(self.root, "git", "branch", "old-upstream")

        self.write("lost.txt", "base\nfork\n")
        self.write("changed.txt", "base\nfork\n")
        self.write(
            "stable.txt",
            "".join(f"line {index}\n" for index in range(10)) + "fork\n",
        )
        self.commit("fork base")
        run(self.root, "git", "branch", "fork-base")

        run(self.root, "git", "checkout", "-q", "-b", "target", "old-upstream")
        self.write("changed.txt", "base\nupstream\n")
        self.write(
            "stable.txt",
            "upstream prefix\n" + "".join(f"line {index}\n" for index in range(10)),
        )
        self.commit("new upstream")
        run(self.root, "git", "branch", "new-upstream")

        self.write("changed.txt", "base\nupstream\nadapted\n")
        self.write(
            "stable.txt",
            "upstream prefix\n"
            + "".join(f"line {index}\n" for index in range(10))
            + "fork\n",
        )
        self.write("added.txt", "new fork patch\n")
        self.commit("merged head")
        run(self.root, "git", "branch", "merged-head")

    def tearDown(self) -> None:
        self.temp.cleanup()

    def write(self, path: str, content: str) -> None:
        (self.root / path).write_text(content, encoding="utf-8")

    def commit(self, message: str) -> None:
        run(self.root, "git", "add", ".")
        run(self.root, "git", "commit", "-q", "-m", message)

    def test_classifies_fork_delta_changes(self) -> None:
        audit = compute_audit(
            self.root, "old-upstream", "new-upstream", "fork-base", "merged-head"
        )

        self.assertEqual(["added.txt"], audit["added"])
        self.assertEqual(["lost.txt"], audit["removed"])
        self.assertEqual(["changed.txt"], audit["changed"])
        self.assertEqual(["stable.txt"], audit["unchanged"])

    def test_preserves_and_validates_decisions(self) -> None:
        audit = compute_audit(
            self.root, "old-upstream", "new-upstream", "fork-base", "merged-head"
        )
        output = self.root / "audit.md"
        output.write_text(
            render(
                audit,
                {
                    "lost.txt": (
                        "removed",
                        audit["fingerprints"]["lost.txt"],
                        "upstreamed",
                        "Target contains equivalent behavior.",
                    ),
                    "changed.txt": (
                        "changed",
                        audit["fingerprints"]["changed.txt"],
                        "adapted",
                        "Fork behavior retained on target code | verified.",
                    ),
                    "added.txt": (
                        "added",
                        audit["fingerprints"]["added.txt"],
                        "intentional-addition",
                        "Required compatibility fix.",
                    ),
                },
                "old-upstream",
                "new-upstream",
                "fork-base",
                "merged-head",
            ),
            encoding="utf-8",
        )

        decisions = read_decisions(output)
        self.assertEqual([], validate(audit, decisions))
        refreshed = render(
            audit,
            decisions,
            "old-upstream",
            "new-upstream",
            "fork-base",
            "merged-head",
        )
        self.assertIn("adapted", refreshed)
        self.assertIn("target code | verified", refreshed)

    def test_rejects_todo_and_wrong_disposition(self) -> None:
        audit = compute_audit(
            self.root, "old-upstream", "new-upstream", "fork-base", "merged-head"
        )
        errors = validate(
            audit,
            {
                "lost.txt": (
                    "removed",
                    audit["fingerprints"]["lost.txt"],
                    "adapted",
                    "TODO",
                ),
                "changed.txt": (
                    "changed",
                    audit["fingerprints"]["changed.txt"],
                    "preserved",
                    "Verified.",
                ),
                "added.txt": (
                    "added",
                    audit["fingerprints"]["added.txt"],
                    "intentional-addition",
                    "Verified.",
                ),
            },
        )

        self.assertTrue(any("invalid for removed" in error for error in errors))
        self.assertTrue(any("concrete evidence" in error for error in errors))

    def test_rejects_stale_category_and_fingerprint(self) -> None:
        audit = compute_audit(
            self.root, "old-upstream", "new-upstream", "fork-base", "merged-head"
        )
        decisions = {
            "lost.txt": (
                "changed",
                "0" * 16,
                "obsolete",
                "Reviewed an older patch.",
            ),
            "changed.txt": (
                "changed",
                audit["fingerprints"]["changed.txt"],
                "adapted",
                "Verified.",
            ),
            "added.txt": (
                "added",
                audit["fingerprints"]["added.txt"],
                "intentional-addition",
                "Verified.",
            ),
        }

        errors = validate(audit, decisions)
        self.assertTrue(any("recorded change" in error for error in errors))
        self.assertTrue(any("patch changed" in error for error in errors))

    def test_ignores_diff_metadata_but_keeps_hunk_context(self) -> None:
        first = (
            "diff --git a/a.cpp b/a.cpp\n"
            "index 111..222 100644\n"
            "--- a/a.cpp\n+++ b/a.cpp\n"
            "@@ -10,2 +10,3 @@ void Draw()\n context\n+fork"
        )
        shifted = (
            "diff --git a/a.cpp b/a.cpp\n"
            "index 333..444 100644\n"
            "--- a/a.cpp\n+++ b/a.cpp\n"
            "@@ -30,2 +30,3 @@ void Draw()\n context\n+fork"
        )
        moved = "@@ -30,2 +30,3 @@ void Flush()\n other context\n+fork"

        self.assertEqual(normalize_patch(first), normalize_patch(shifted))
        self.assertNotEqual(normalize_patch(first), normalize_patch(moved))

    def test_keeps_content_that_looks_like_file_headers(self) -> None:
        removed_a = (
            "diff --git a/a.lua b/a.lua\nindex 111..222 100644\n"
            "--- a/a.lua\n+++ b/a.lua\n@@ -1 +0,0 @@\n--- old fork note\n"
        )
        removed_b = removed_a.replace("old fork note", "different fork note")
        added_a = (
            "diff --git a/a.txt b/a.txt\nindex 111..222 100644\n"
            "--- a/a.txt\n+++ b/a.txt\n@@ -0,0 +1 @@\n+++ apples\n"
        )
        added_b = added_a.replace("apples", "oranges")

        self.assertNotEqual(normalize_patch(removed_a), normalize_patch(removed_b))
        self.assertNotEqual(normalize_patch(added_a), normalize_patch(added_b))

    def test_keeps_binary_payload_but_ignores_binary_index(self) -> None:
        first = (
            "diff --git a/image.bin b/image.bin\n"
            "index 111..222 100644\n"
            "GIT binary patch\nliteral 1\nAAAA\n"
        )
        same_payload = first.replace("111..222", "333..444")
        changed_payload = first.replace("AAAA", "BBBB")

        self.assertEqual(normalize_patch(first), normalize_patch(same_payload))
        self.assertNotEqual(normalize_patch(first), normalize_patch(changed_payload))


if __name__ == "__main__":
    unittest.main()
