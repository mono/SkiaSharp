#!/usr/bin/env python3

import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path


VULKAN_RESULT = re.compile(
    r"Passed!\s*-\s*Failed:\s*(?P<failed>\d+),\s*"
    r"Passed:\s*(?P<passed>\d+),\s*"
    r"Skipped:\s*(?P<skipped>\d+),\s*"
    r"Total:\s*(?P<total>\d+),.*"
    r"SkiaSharp\.Vulkan\.Tests\.dll"
)
REQUIRED_TEST_EVIDENCE = (
    "SkiaSharp.Vulkan.Tests.GRContextTest.CreateVkContextIsValid",
    "SkiaSharp.Vulkan.Tests.GraphiteVkBackendContextTest.GraphiteVkContextIsCreatedFromRawHandles",
    "ganesh-vulkan",
    "graphite-vulkan",
)
ALLOWED_SKIPPED_TESTS = (
    "SkiaSharp.Vulkan.Tests.SharpVkBackendContextTest.VkGpuSurfaceIsCreatedSharpVkTypes",
    "SkiaSharp.Vulkan.Tests.SKSurfaceTest.VkGpuSurfaceIsCreatedSharpVkTypes",
)


def parse_count(element: ET.Element, name: str) -> int:
    value = element.get(name)
    if value is None or not value.isdigit():
        raise ValueError(f"Vulkan xUnit report has invalid {name!r} count")
    return int(value)


def validate(test_output: str, xunit_report: str) -> str:
    results = list(VULKAN_RESULT.finditer(test_output))
    if not results:
        raise ValueError("full solution output has no Vulkan host result")

    result = results[-1]
    solution_counts = {
        name: int(result.group(name)) for name in ("failed", "passed", "skipped", "total")
    }

    try:
        root = ET.fromstring(xunit_report)
    except ET.ParseError as error:
        raise ValueError(f"Vulkan xUnit report is invalid: {error}") from error

    assembly = next(
        (
            candidate
            for candidate in root.findall(".//assembly")
            if candidate.get("name", "").replace("\\", "/").endswith(
                "/SkiaSharp.Vulkan.Tests.dll"
            )
            or candidate.get("name") == "SkiaSharp.Vulkan.Tests.dll"
        ),
        None,
    )
    if assembly is None:
        raise ValueError("Vulkan xUnit report has no SkiaSharp.Vulkan.Tests.dll assembly")

    report_counts = {
        name: parse_count(assembly, name) for name in ("failed", "passed", "skipped", "total")
    }
    errors = parse_count(assembly, "errors")
    not_run = parse_count(assembly, "not-run")
    if errors != 0 or not_run != 0:
        raise ValueError(f"Vulkan report contains {errors} error(s) and {not_run} not-run test(s)")
    if report_counts["failed"] != 0:
        raise ValueError(f"Vulkan report contains {report_counts['failed']} failed test(s)")
    if report_counts["passed"] == 0:
        raise ValueError("Vulkan report contains no passing tests")
    if report_counts["total"] != (
        report_counts["failed"] + report_counts["passed"] + report_counts["skipped"]
    ):
        raise ValueError(
            "Vulkan report total does not equal failed plus passed plus skipped"
        )
    if report_counts != solution_counts:
        raise ValueError(
            f"full solution counts {solution_counts} do not match Vulkan report {report_counts}"
        )

    tests = assembly.findall(".//test")
    if len(tests) != report_counts["total"]:
        raise ValueError(
            f"Vulkan report contains {len(tests)} named test result(s), "
            f"expected {report_counts['total']}"
        )

    passed_names = [test.get("name", "") for test in tests if test.get("result") == "Pass"]
    missing = [
        evidence
        for evidence in REQUIRED_TEST_EVIDENCE
        if not any(evidence in name for name in passed_names)
    ]
    if missing:
        raise ValueError(f"Vulkan report is missing passing coverage: {', '.join(missing)}")

    skipped_names = [test.get("name", "") for test in tests if test.get("result") == "Skip"]
    unexpected_skips = [
        name
        for name in skipped_names
        if not any(allowed in name for allowed in ALLOWED_SKIPPED_TESTS)
    ]
    if unexpected_skips:
        raise ValueError(f"Vulkan report contains unexpected skipped test(s): {unexpected_skips}")
    if len(skipped_names) > len(ALLOWED_SKIPPED_TESTS):
        raise ValueError(f"Vulkan report contains too many skipped tests: {len(skipped_names)}")

    return (
        f"Vulkan evidence verified: {report_counts['passed']} passed, "
        f"{report_counts['skipped']} expected skipped, {report_counts['total']} total; "
        "named Ganesh and Graphite tests passed."
    )


def main() -> int:
    if len(sys.argv) != 3:
        print(
            "usage: skia-sync-validate-tests.py TEST_OUTPUT VULKAN_XUNIT_REPORT",
            file=sys.stderr,
        )
        return 2

    try:
        test_output = Path(sys.argv[1]).read_text(encoding="utf-8")
        xunit_report = Path(sys.argv[2]).read_text(encoding="utf-8")
        print(validate(test_output, xunit_report))
        return 0
    except (OSError, UnicodeError, ValueError) as error:
        print(f"::error::{error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
