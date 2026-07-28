#!/usr/bin/env python3

import argparse
import collections
import pathlib
import sys
import xml.etree.ElementTree as ET


REQUIRED_TESTS = {
    "Ganesh Vulkan": "GRContextTest.CreateVkContextIsValid",
    "Graphite Vulkan": "GraphiteVkBackendContextTest.GraphiteVkContextIsCreatedFromRawHandles",
}


def read_results(path):
    try:
        root = ET.parse(path).getroot()
    except FileNotFoundError:
        raise ValueError(f"Vulkan TRX file does not exist: {path}") from None
    except ET.ParseError as error:
        raise ValueError(f"Vulkan TRX file is invalid XML: {error}") from error

    results = []
    for element in root.iter():
        if element.tag.rsplit("}", 1)[-1] != "UnitTestResult":
            continue
        name = element.get("testName", "")
        outcome = element.get("outcome", "Unknown")
        if name:
            results.append((name, outcome))
    return results


def verify_results(results):
    if not results:
        raise ValueError("Vulkan TRX contains no test results.")

    required = {}
    errors = []
    for label, suffix in REQUIRED_TESTS.items():
        matches = [(name, outcome) for name, outcome in results if suffix in name]
        passed = [(name, outcome) for name, outcome in matches if outcome.lower() == "passed"]
        if passed:
            required[label] = passed[0]
        elif matches:
            outcomes = ", ".join(sorted({outcome for _, outcome in matches}))
            errors.append(f"{label} test did not pass (outcomes: {outcomes}).")
        else:
            errors.append(f"{label} test is missing ({suffix}).")

    if errors:
        raise ValueError(" ".join(errors))
    return required


def format_evidence(results, required):
    counts = collections.Counter(outcome for _, outcome in results)
    counts_text = ", ".join(f"{outcome}={count}" for outcome, count in sorted(counts.items()))
    lines = [
        f"Vulkan test evidence: total={len(results)}; {counts_text}",
        *(
            f"- {label}: {name} [{outcome}]"
            for label, (name, outcome) in required.items()
        ),
    ]
    return "\n".join(lines)


def append_summary(path, results, required):
    counts = collections.Counter(outcome for _, outcome in results)
    with path.open("a", encoding="utf-8") as summary:
        summary.write("## Vulkan test evidence\n\n")
        summary.write(
            f"Total: **{len(results)}**; "
            + "; ".join(f"{outcome}: **{count}**" for outcome, count in sorted(counts.items()))
            + "\n\n"
        )
        summary.write("| Backend | Required test | Outcome |\n")
        summary.write("| --- | --- | --- |\n")
        for label, (name, outcome) in required.items():
            summary.write(f"| {label} | `{name}` | {outcome} |\n")
        summary.write("\n")


def main(argv=None):
    parser = argparse.ArgumentParser(
        description="Require passing Ganesh and Graphite Vulkan evidence in a TRX file."
    )
    parser.add_argument("trx", type=pathlib.Path)
    parser.add_argument("--summary-file", type=pathlib.Path)
    args = parser.parse_args(argv)

    try:
        results = read_results(args.trx)
        required = verify_results(results)
    except ValueError as error:
        print(f"Vulkan test evidence gate failed: {error}", file=sys.stderr)
        return 1

    print(format_evidence(results, required))
    if args.summary_file:
        append_summary(args.summary_file, results, required)
    return 0


if __name__ == "__main__":
    sys.exit(main())
