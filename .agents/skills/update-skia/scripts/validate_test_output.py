#!/usr/bin/env python3

import argparse
import re
import sys
from dataclasses import dataclass
from pathlib import Path


EXPECTED_HOSTS = (
    "SkiaSharp.Tests.dll",
    "SkiaSharp.Tests.SingletonInit.dll",
    "SkiaSharp.Direct3D.Tests.dll",
    "SkiaSharp.Vulkan.Tests.dll",
)
VULKAN_HOST = "SkiaSharp.Vulkan.Tests.dll"
INITIAL_MARKER = (
    "SKIA_SYNC_TEST_EVIDENCE full stage=initial "
    "solution=tests/SkiaSharp.Tests.Console.slnx tfm=net10.0 unfiltered=true"
)
FINAL_MARKER = (
    "SKIA_SYNC_TEST_EVIDENCE full stage=final "
    "solution=tests/SkiaSharp.Tests.Console.slnx tfm=net10.0 unfiltered=true"
)
GANESH_MARKER = (
    "SKIA_SYNC_TEST_EVIDENCE vulkan backend=ganesh "
    "filter=*CreateVkContextIsValid*"
)
GRAPHITE_MARKER = (
    "SKIA_SYNC_TEST_EVIDENCE vulkan backend=graphite "
    "filter=*GraphiteVkContextIsCreatedFromRawHandles*"
)

ANSI_ESCAPE = re.compile(r"\x1b\[[0-?]*[ -/]*[@-~]")
SUMMARY = re.compile(
    r"(?:Passed!|Failed!)\s*-\s*"
    r"Failed:\s*(?P<failed>\d+),\s*"
    r"Passed:\s*(?P<passed>\d+),\s*"
    r"Skipped:\s*(?P<skipped>\d+),\s*"
    r"Total:\s*(?P<total>\d+),[^\r\n]*?-\s*"
    r"(?P<assembly>[A-Za-z0-9_.]+\.dll)"
)


@dataclass(frozen=True)
class TestSummary:
    assembly: str
    failed: int
    passed: int
    skipped: int
    total: int


def read_required(path: Path, label: str, errors: list[str]) -> str:
    if not path.is_file():
        errors.append(f"{label} is missing: {path}")
        return ""

    content = path.read_text(encoding="utf-8", errors="replace")
    if not content.strip():
        errors.append(f"{label} is empty: {path}")
    return ANSI_ESCAPE.sub("", content)


def parse_summaries(content: str) -> list[TestSummary]:
    return [
        TestSummary(
            assembly=match.group("assembly"),
            failed=int(match.group("failed")),
            passed=int(match.group("passed")),
            skipped=int(match.group("skipped")),
            total=int(match.group("total")),
        )
        for match in SUMMARY.finditer(content)
    ]


def require_marker(
    content: str, marker: str, label: str, errors: list[str]
) -> None:
    if marker not in content:
        errors.append(f"{label} is missing its exact invocation marker")


def validate_summary(summary: TestSummary, label: str, errors: list[str]) -> None:
    if summary.total != summary.failed + summary.passed + summary.skipped:
        errors.append(
            f"{label} has inconsistent counts: "
            f"{summary.failed} failed + {summary.passed} passed + "
            f"{summary.skipped} skipped != {summary.total} total"
        )
    if summary.failed != 0:
        errors.append(f"{label} reported {summary.failed} failed test(s)")


def validate_final(content: str, errors: list[str]) -> dict[str, TestSummary]:
    summaries = parse_summaries(content)
    by_assembly: dict[str, list[TestSummary]] = {}
    for summary in summaries:
        by_assembly.setdefault(summary.assembly, []).append(summary)
        validate_summary(summary, summary.assembly, errors)

    validated: dict[str, TestSummary] = {}
    for assembly in EXPECTED_HOSTS:
        matches = by_assembly.get(assembly, [])
        if len(matches) != 1:
            errors.append(
                f"final test output must contain exactly one {assembly} summary; "
                f"found {len(matches)}"
            )
            continue

        summary = matches[0]
        validated[assembly] = summary
        if summary.passed == 0:
            errors.append(f"{assembly} executed zero passing tests")

    vulkan = validated.get(VULKAN_HOST)
    if vulkan and vulkan.passed + vulkan.failed == 0:
        errors.append("Vulkan host contained only skipped tests")

    return validated


def validate_initial(content: str, errors: list[str]) -> None:
    summaries = parse_summaries(content)
    by_assembly: dict[str, list[TestSummary]] = {}
    for summary in summaries:
        by_assembly.setdefault(summary.assembly, []).append(summary)
        if summary.total != summary.failed + summary.passed + summary.skipped:
            errors.append(
                f"initial {summary.assembly} has inconsistent counts: "
                f"{summary.failed} failed + {summary.passed} passed + "
                f"{summary.skipped} skipped != {summary.total} total"
            )

    for assembly in EXPECTED_HOSTS:
        matches = by_assembly.get(assembly, [])
        if len(matches) != 1:
            errors.append(
                f"initial test output must contain exactly one {assembly} summary; "
                f"found {len(matches)}"
            )
            continue
        if matches[0].total == 0:
            errors.append(f"initial {assembly} executed zero tests")


def validate_vulkan_evidence(
    content: str, backend: str, marker: str, errors: list[str]
) -> TestSummary | None:
    require_marker(content, marker, f"{backend} Vulkan evidence", errors)
    other_marker = GRAPHITE_MARKER if marker == GANESH_MARKER else GANESH_MARKER
    if other_marker in content:
        errors.append(f"{backend} Vulkan evidence contains the other backend marker")
    summaries = [
        summary
        for summary in parse_summaries(content)
        if summary.assembly == VULKAN_HOST
    ]
    if len(summaries) != 1:
        errors.append(
            f"{backend} evidence must contain exactly one {VULKAN_HOST} summary; "
            f"found {len(summaries)}"
        )
        return None

    summary = summaries[0]
    validate_summary(summary, f"{backend} Vulkan evidence", errors)
    if summary.passed != 1 or summary.skipped != 0 or summary.total != 1:
        errors.append(
            f"{backend} Vulkan evidence must execute exactly one passing, non-skipped "
            f"test; got {summary.passed} passed, {summary.skipped} skipped, "
            f"{summary.total} total"
        )
    return summary


def validate_files(
    initial_path: Path,
    final_path: Path,
    ganesh_path: Path,
    graphite_path: Path,
) -> tuple[list[str], dict[str, TestSummary]]:
    errors: list[str] = []
    initial = read_required(initial_path, "initial full-solution output", errors)
    final = read_required(final_path, "final full-solution output", errors)
    ganesh = read_required(ganesh_path, "Ganesh Vulkan evidence", errors)
    graphite = read_required(graphite_path, "Graphite Vulkan evidence", errors)

    if initial:
        require_marker(
            initial, INITIAL_MARKER, "initial full-solution output", errors
        )
        validate_initial(initial, errors)
    if final:
        require_marker(final, FINAL_MARKER, "final full-solution output", errors)
    summaries = validate_final(final, errors) if final else {}
    if ganesh:
        validate_vulkan_evidence(ganesh, "Ganesh", GANESH_MARKER, errors)
    if graphite:
        validate_vulkan_evidence(
            graphite, "Graphite", GRAPHITE_MARKER, errors
        )
    return errors, summaries


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Validate Skia sync full-suite and explicit Vulkan evidence logs."
    )
    parser.add_argument("--initial", required=True, type=Path)
    parser.add_argument("--final", required=True, type=Path)
    parser.add_argument("--ganesh", required=True, type=Path)
    parser.add_argument("--graphite", required=True, type=Path)
    args = parser.parse_args()

    errors, summaries = validate_files(
        args.initial, args.final, args.ganesh, args.graphite
    )
    if errors:
        for error in errors:
            print(f"ERROR: {error}", file=sys.stderr)
        return 1

    counts = ", ".join(
        f"{assembly}={summaries[assembly].passed} passed/"
        f"{summaries[assembly].skipped} skipped"
        for assembly in EXPECTED_HOSTS
    )
    print(f"Validated Skia sync test evidence: {counts}; Ganesh=1; Graphite=1")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
