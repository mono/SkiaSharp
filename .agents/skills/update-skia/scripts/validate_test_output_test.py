import tempfile
import unittest
from pathlib import Path

from validate_test_output import (
    FINAL_MARKER,
    GANESH_MARKER,
    GRAPHITE_MARKER,
    INITIAL_MARKER,
    validate_files,
)


def summary(
    assembly: str,
    *,
    failed: int = 0,
    passed: int = 1,
    skipped: int = 0,
) -> str:
    total = failed + passed + skipped
    return (
        f"Passed! - Failed: {failed}, Passed: {passed}, Skipped: {skipped}, "
        f"Total: {total}, Duration: 1s - {assembly} (net10.0|x64)"
    )


class ValidateTestOutputTests(unittest.TestCase):
    def setUp(self) -> None:
        self.temporary_directory = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary_directory.name)
        self.initial = self.write(
            "initial.txt",
            f"{INITIAL_MARKER}\n"
            + "\n".join(
                (
                    summary("SkiaSharp.Tests.SingletonInit.dll"),
                    summary("SkiaSharp.Tests.dll", passed=5915),
                    summary("SkiaSharp.Direct3D.Tests.dll", passed=2, skipped=3),
                    summary(
                        "SkiaSharp.Vulkan.Tests.dll",
                        failed=9,
                        passed=14,
                        skipped=2,
                    ),
                )
            ),
        )
        self.final = self.write(
            "final.txt",
            f"{FINAL_MARKER}\n"
            + "\n".join(
                (
                    summary("SkiaSharp.Tests.SingletonInit.dll"),
                    summary("SkiaSharp.Tests.dll", passed=5915, skipped=202),
                    summary("SkiaSharp.Direct3D.Tests.dll", passed=2, skipped=3),
                    summary("SkiaSharp.Vulkan.Tests.dll", passed=23, skipped=2),
                )
            ),
        )
        self.ganesh = self.write(
            "ganesh.txt",
            f"{GANESH_MARKER}\n{summary('SkiaSharp.Vulkan.Tests.dll')}",
        )
        self.graphite = self.write(
            "graphite.txt",
            f"{GRAPHITE_MARKER}\n{summary('SkiaSharp.Vulkan.Tests.dll')}",
        )

    def tearDown(self) -> None:
        self.temporary_directory.cleanup()

    def write(self, name: str, content: str) -> Path:
        path = self.root / name
        path.write_text(content, encoding="utf-8")
        return path

    def validate(self) -> list[str]:
        return validate_files(
            self.initial, self.final, self.ganesh, self.graphite
        )[0]

    def test_accepts_complete_green_evidence(self) -> None:
        self.assertEqual([], self.validate())

    def test_rejects_missing_initial_output(self) -> None:
        self.initial.unlink()

        errors = self.validate()

        self.assertTrue(any("initial full-solution output is missing" in e for e in errors))

    def test_rejects_missing_full_solution_host(self) -> None:
        self.final.write_text(
            f"{FINAL_MARKER}\n"
            + summary("SkiaSharp.Vulkan.Tests.dll", passed=23, skipped=2),
            encoding="utf-8",
        )

        errors = self.validate()

        self.assertTrue(
            any("exactly one SkiaSharp.Tests.dll summary" in e for e in errors)
        )

    def test_rejects_only_skipped_vulkan_host(self) -> None:
        content = self.final.read_text(encoding="utf-8").replace(
            summary("SkiaSharp.Vulkan.Tests.dll", passed=23, skipped=2),
            summary("SkiaSharp.Vulkan.Tests.dll", passed=0, skipped=25),
        )
        self.final.write_text(content, encoding="utf-8")

        errors = self.validate()

        self.assertIn("Vulkan host contained only skipped tests", errors)

    def test_rejects_failed_full_solution_host(self) -> None:
        content = self.final.read_text(encoding="utf-8").replace(
            summary("SkiaSharp.Tests.dll", passed=5915, skipped=202),
            summary("SkiaSharp.Tests.dll", failed=1, passed=5914, skipped=202),
        )
        self.final.write_text(content, encoding="utf-8")

        errors = self.validate()

        self.assertIn("SkiaSharp.Tests.dll reported 1 failed test(s)", errors)

    def test_rejects_skipped_targeted_vulkan_evidence(self) -> None:
        self.graphite.write_text(
            f"{GRAPHITE_MARKER}\n"
            + summary("SkiaSharp.Vulkan.Tests.dll", passed=0, skipped=1),
            encoding="utf-8",
        )

        errors = self.validate()

        self.assertTrue(
            any(
                "Graphite Vulkan evidence must execute exactly one passing" in e
                for e in errors
            )
        )

    def test_rejects_broad_targeted_vulkan_filter(self) -> None:
        self.ganesh.write_text(
            f"{GANESH_MARKER}\n"
            + summary("SkiaSharp.Vulkan.Tests.dll", passed=2),
            encoding="utf-8",
        )

        errors = self.validate()

        self.assertTrue(
            any(
                "Ganesh Vulkan evidence must execute exactly one passing" in e
                for e in errors
            )
        )

    def test_rejects_missing_invocation_marker(self) -> None:
        self.final.write_text(
            self.final.read_text(encoding="utf-8").replace(FINAL_MARKER, ""),
            encoding="utf-8",
        )

        errors = self.validate()

        self.assertIn(
            "final full-solution output is missing its exact invocation marker",
            errors,
        )

    def test_rejects_same_log_for_both_vulkan_backends(self) -> None:
        self.graphite.write_text(
            self.ganesh.read_text(encoding="utf-8"),
            encoding="utf-8",
        )

        errors = self.validate()

        self.assertIn(
            "Graphite Vulkan evidence is missing its exact invocation marker",
            errors,
        )

    def test_rejects_combined_vulkan_backend_markers(self) -> None:
        combined = (
            f"{GANESH_MARKER}\n{GRAPHITE_MARKER}\n"
            f"{summary('SkiaSharp.Vulkan.Tests.dll')}"
        )
        self.ganesh.write_text(combined, encoding="utf-8")
        self.graphite.write_text(combined, encoding="utf-8")

        errors = self.validate()

        self.assertIn(
            "Ganesh Vulkan evidence contains the other backend marker", errors
        )
        self.assertIn(
            "Graphite Vulkan evidence contains the other backend marker", errors
        )

    def test_rejects_marker_only_initial_output(self) -> None:
        self.initial.write_text(f"{INITIAL_MARKER}\n", encoding="utf-8")

        errors = self.validate()

        self.assertIn(
            "initial test output must contain exactly one SkiaSharp.Tests.dll summary; found 0",
            errors,
        )

    def test_rejects_incomplete_initial_output(self) -> None:
        self.initial.write_text(
            f"{INITIAL_MARKER}\n{summary('SkiaSharp.Tests.dll')}",
            encoding="utf-8",
        )

        errors = self.validate()

        self.assertIn(
            "initial test output must contain exactly one "
            "SkiaSharp.Vulkan.Tests.dll summary; found 0",
            errors,
        )


if __name__ == "__main__":
    unittest.main()
