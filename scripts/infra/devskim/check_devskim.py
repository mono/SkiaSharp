#!/usr/bin/env python3
"""Fail on DevSkim banned-function findings, at any severity.

DevSkim exits with 0 even when it reports findings, so the SARIF reports are evaluated
here. A result is a banned-function finding when DevSkim tags it with
API.DangerousAPI.BannedFunction. The severity does not matter: the general rule DS154189
is only "Moderate". Inline suppressions do not count.

DevSkim matches names, not calls, so a C++ member function that happens to be called
`rewind` is reported like the C library function. The exceptions file lists these false
positives one by one. An exception applies only to a finding of the same rule in the same
file whose source line is exactly the given text.

Exit codes: 0 no findings, 1 findings, 2 a report, or the exceptions file, is missing or invalid.
"""

import argparse
import json
import sys
from pathlib import Path

BANNED_FUNCTION_TAG = "API.DangerousAPI.BannedFunction"


def banned_findings(document):
    """Return the banned-function results of a DevSkim SARIF document."""
    if not isinstance(document, dict) or document.get("version") != "2.1.0":
        raise ValueError("Expected SARIF 2.1.0")
    runs = document.get("runs")
    if not isinstance(runs, list) or not runs:
        raise ValueError("Expected at least one SARIF run")

    findings = []
    for run in runs:
        if not isinstance(run, dict):
            raise ValueError("Expected a DevSkim run")
        tool = run.get("tool")
        driver = tool.get("driver") if isinstance(tool, dict) else None
        if not isinstance(driver, dict) or driver.get("name") != "devskim":
            raise ValueError("Expected a DevSkim run")
        for invocation in run.get("invocations", []):
            if invocation.get("executionSuccessful") is False:
                raise ValueError("DevSkim execution was unsuccessful")
            for notification in invocation.get("toolExecutionNotifications", []):
                if notification.get("level") == "error":
                    raise ValueError("DevSkim reported an execution error")
        results = run.get("results")
        if not isinstance(results, list):
            raise ValueError("Expected a results array")
        for result in results:
            if not isinstance(result, dict) or not isinstance(result.get("ruleId"), str):
                raise ValueError("Expected a result with a ruleId")
            properties = result.get("properties")
            tags = properties.get("tags") if isinstance(properties, dict) else None
            # DevSkim 1.0.100 writes rule tags on results, not on driver.rules.
            if not isinstance(tags, list) or not all(isinstance(tag, str) for tag in tags):
                raise ValueError("Expected DevSkim result tags")
            if BANNED_FUNCTION_TAG in tags:
                findings.append(result)
    return findings


def location(finding):
    """Return the file (relative to the repository root), line and matched text of a finding."""
    physical = (finding.get("locations") or [{}])[0].get("physicalLocation", {})
    uri = physical.get("artifactLocation", {}).get("uri", "?")
    region = physical.get("region", {})
    return uri, region.get("startLine", 0), region.get("snippet", {}).get("text", "")


def load_exceptions(path):
    document = json.loads(Path(path).read_text(encoding="utf-8"))
    exceptions = document.get("exceptions") if isinstance(document, dict) else None
    if not isinstance(exceptions, list):
        raise ValueError("Expected an exceptions array")
    for exception in exceptions:
        for key in ("path", "rule", "text", "reason"):
            value = exception.get(key) if isinstance(exception, dict) else None
            if not isinstance(value, str) or not value.strip():
                raise ValueError(f"Every exception needs a non-empty {key}")
    return exceptions


def exception_for(finding, exceptions, root):
    """Return the exception that excuses the finding, or None."""
    uri, line, _ = location(finding)
    candidates = [e for e in exceptions if e["path"] == uri and e["rule"] == finding["ruleId"]]
    if not candidates or line < 1:
        return None
    try:
        text = (root / uri).read_text(encoding="utf-8", errors="replace").split("\n")[line - 1].strip()
    except (OSError, IndexError):
        return None
    return next((e for e in candidates if e["text"] == text), None)


def main(argv=None):
    parser = argparse.ArgumentParser(description="Fail on DevSkim banned-function findings, at any severity.")
    parser.add_argument("reports", type=Path, nargs="+")
    parser.add_argument("--root", type=Path, default=Path.cwd(),
                        help="repository root, which the paths in the reports are relative to")
    parser.add_argument("--exceptions", type=Path, help="JSON file with the reviewed false positives")
    args = parser.parse_args(argv)

    try:
        exceptions = load_exceptions(args.exceptions) if args.exceptions else []
    except (OSError, ValueError) as error:
        print(f"DevSkim exceptions error in {args.exceptions}: {error}", file=sys.stderr)
        return 2

    count = 0
    excused = []
    for report in args.reports:
        try:
            findings = banned_findings(json.loads(report.read_text(encoding="utf-8")))
        except (OSError, ValueError) as error:
            print(f"DevSkim report error in {report}: {error}", file=sys.stderr)
            return 2
        for finding in findings:
            uri, line, text = location(finding)
            exception = exception_for(finding, exceptions, args.root)
            if exception:
                excused.append((uri, line, finding["ruleId"], text, exception["reason"]))
                continue
            message = finding.get("message", {}).get("text", "Banned API detected")
            print(f"{uri}:{line}: {finding['ruleId']}: {message}" + (f" ({text})" if text else ""))
            count += 1

    for uri, line, rule, text, reason in excused:
        print(f"{uri}:{line}: {rule}: excused ({text}): {reason}")
    print(f"DevSkim: {count} banned-function finding(s), {len(excused)} excused by the exceptions file. "
          "Full results are in the devskim artifact.")
    return 1 if count else 0


if __name__ == "__main__":
    sys.exit(main())
