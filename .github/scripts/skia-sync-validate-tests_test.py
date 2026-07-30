#!/usr/bin/env python3

import importlib.util
import unittest
import xml.etree.ElementTree as ET
from pathlib import Path


SCRIPT = Path(__file__).with_name("skia-sync-validate-tests.py")
SPEC = importlib.util.spec_from_file_location("skia_sync_validate_tests", SCRIPT)
MODULE = importlib.util.module_from_spec(SPEC)
assert SPEC.loader is not None
SPEC.loader.exec_module(MODULE)

VALID_OUTPUT = """
  Passed! - Failed: 0, Passed: 5912, Skipped: 202, Total: 6114, Duration: 2m - SkiaSharp.Tests.dll
  Passed! - Failed: 0, Passed: 23, Skipped: 2, Total: 25, Duration: 2s - SkiaSharp.Vulkan.Tests.dll (net10.0|x64)
"""
REQUIRED_PASSES = [
    "SkiaSharp.Vulkan.Tests.GRContextTest.CreateVkContextIsValid",
    "SkiaSharp.Vulkan.Tests.GraphiteVkBackendContextTest.GraphiteVkContextIsCreatedFromRawHandles",
    'SkiaSharp.Vulkan.Tests.VulkanVisualTests.RenderMatchesGolden(rendererName: "ganesh-vulkan")',
    'SkiaSharp.Vulkan.Tests.VulkanVisualTests.RenderMatchesGolden(rendererName: "graphite-vulkan")',
]
ALLOWED_SKIPS = list(MODULE.ALLOWED_SKIPPED_TESTS)


def report(
    passed: list[str] | None = None,
    skipped: list[str] | None = None,
    failed: list[str] | None = None,
) -> str:
    passed = list(passed if passed is not None else REQUIRED_PASSES)
    skipped = list(skipped if skipped is not None else ALLOWED_SKIPS)
    failed = list(failed if failed is not None else [])
    while len(passed) + len(skipped) + len(failed) < 25:
        passed.append(f"SkiaSharp.Vulkan.Tests.Filler.Test{len(passed)}")

    assembly = ET.Element(
        "assembly",
        {
            "name": "/tmp/SkiaSharp.Vulkan.Tests.dll",
            "errors": "0",
            "failed": str(len(failed)),
            "not-run": "0",
            "passed": str(len(passed)),
            "skipped": str(len(skipped)),
            "total": str(len(passed) + len(skipped) + len(failed)),
        },
    )
    collection = ET.SubElement(assembly, "collection")
    for name in passed:
        ET.SubElement(collection, "test", {"name": name, "result": "Pass"})
    for name in skipped:
        ET.SubElement(collection, "test", {"name": name, "result": "Skip"})
    for name in failed:
        ET.SubElement(collection, "test", {"name": name, "result": "Fail"})
    root = ET.Element("assemblies")
    root.append(assembly)
    return ET.tostring(root, encoding="unicode")


class ValidateVulkanTests(unittest.TestCase):
    def test_accepts_executed_named_vulkan_coverage(self) -> None:
        message = MODULE.validate(VALID_OUTPUT, report())

        self.assertIn("23 passed", message)
        self.assertIn("25 total", message)

    def test_rejects_missing_vulkan_host(self) -> None:
        with self.assertRaisesRegex(ValueError, "no Vulkan host result"):
            MODULE.validate("Passed! - Failed: 0, Passed: 1, Skipped: 0, Total: 1", report())

    def test_rejects_failed_vulkan_test(self) -> None:
        failed = VALID_OUTPUT.replace("Failed: 0, Passed: 23", "Failed: 1, Passed: 22")
        failed_report = report(
            passed=REQUIRED_PASSES,
            skipped=ALLOWED_SKIPS,
            failed=["SkiaSharp.Vulkan.Tests.Broken.Test"],
        )
        with self.assertRaisesRegex(ValueError, "contains 1 failed"):
            MODULE.validate(failed, failed_report)

    def test_rejects_full_solution_report_mismatch(self) -> None:
        mismatch = VALID_OUTPUT.replace("Passed: 23, Skipped: 2", "Passed: 24, Skipped: 1")
        with self.assertRaisesRegex(ValueError, "do not match"):
            MODULE.validate(mismatch, report())

    def test_rejects_missing_named_backend_coverage(self) -> None:
        without_graphite = [
            name for name in REQUIRED_PASSES if "GraphiteVkBackendContextTest" not in name
        ]

        with self.assertRaisesRegex(ValueError, "missing passing coverage"):
            MODULE.validate(VALID_OUTPUT, report(passed=without_graphite))

    def test_rejects_unexpected_skipped_backend_test(self) -> None:
        skipped = ALLOWED_SKIPS + [
            "SkiaSharp.Vulkan.Tests.GRContextTest.CreateVkContextWithOptionsIsValid"
        ]
        passed = REQUIRED_PASSES
        output = VALID_OUTPUT.replace("Passed: 23, Skipped: 2", "Passed: 22, Skipped: 3")

        with self.assertRaisesRegex(ValueError, "unexpected skipped"):
            MODULE.validate(output, report(passed=passed, skipped=skipped))


if __name__ == "__main__":
    unittest.main()
