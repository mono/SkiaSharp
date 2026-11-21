#!/usr/bin/env python3
"""Scan the Microsoft-owned sources with DevSkim and fail on banned-function findings.

  python3 scripts/infra/devskim/run_devskim.py --devskim /path/to/devskim

CI runs the same command. The trees are scanned one by one: a scan of the whole repository
runs into open-file limits and gives no per-tree diagnostics. A clean result must not be
vacuous, so a tree fails when DevSkim analyzed no files in it (an empty Skia submodule, for
example), and the whole scan fails when DevSkim does not flag a banned function in a control
file that is written here.

Exit codes are the ones of check_devskim.py: 0 clean, 1 findings, 2 a scan or report is invalid.
See documentation/dev/devskim.md.
"""

import argparse
import json
import re
import subprocess
import sys
import tempfile
from pathlib import Path

import check_devskim

ROOT = Path(__file__).resolve().parents[3]
EXCEPTIONS = Path(__file__).with_name("devskim-exceptions.json")

# Microsoft-owned code that ships, and the build tooling. The rest of externals/skia is
# upstream Skia and its third-party dependencies. Tests, samples and documentation are not
# shipped.
TREES = [
    "binding",
    "source",
    "native",
    "externals/skia/src/c",
    "externals/skia/include/c",
    "externals/skia/src/xamarin",
    "externals/skia/include/xamarin",
    "scripts",
    "utils",
    "eng",
]

SEVERITIES = "Critical,Important,Moderate,BestPractice,ManualReview"
CONFIDENCES = "High,Medium,Low"
IGNORE_GLOBS = "**/.git,**/.git/**,**/bin/**,**/obj/**,**/node_modules/**"

CONTROL_SOURCE = """\
#include <string.h>

void copy(char *destination, const char *source)
{
    strcpy(destination, source);
}
"""


class ScanError(Exception):
    pass


def analyzed_files(output):
    # DevSkim only logs the file counts at the Debug level, not in the SARIF report.
    match = re.search(r"Files analyzed: (\d+)", output)
    if not match:
        raise ScanError("The DevSkim output does not say how many files were analyzed")
    return int(match.group(1))


def scan(devskim, source, base, report):
    """Run DevSkim on one directory and return the number of files it analyzed."""
    command = [
        str(devskim), "analyze",
        "--source-code", str(source),
        "--base-path", str(base),
        "--output-file", str(report),
        "--file-format", "sarif",
        "--severity", SEVERITIES,
        "--confidence", CONFIDENCES,
        "--disable-supression",  # sic: this is how DevSkim spells it
        "--disable-parallel",
        "--ignore-globs", IGNORE_GLOBS,
        "--console-verbosity", "Debug",
    ]
    try:
        result = subprocess.run(command, stdout=subprocess.PIPE, stderr=subprocess.STDOUT,
                                encoding="utf-8", errors="replace")
    except OSError as error:
        raise ScanError(f"Could not run {devskim}: {error}")
    report.with_suffix(".log").write_text(result.stdout, encoding="utf-8")
    if result.returncode != 0:
        raise ScanError(f"DevSkim exited with code {result.returncode}:\n{result.stdout}")
    return analyzed_files(result.stdout)


def check_control(devskim, reports):
    """DevSkim must flag the banned function in a file that this script writes."""
    with tempfile.TemporaryDirectory() as directory:
        source = Path(directory)
        (source / "control.c").write_text(CONTROL_SOURCE, encoding="utf-8")
        report = reports / "control.sarif"
        scan(devskim, source, source, report)
    try:
        findings = check_devskim.banned_findings(json.loads(report.read_text(encoding="utf-8")))
    except (OSError, ValueError) as error:
        raise ScanError(f"The control report is invalid: {error}")
    if not findings:
        raise ScanError("DevSkim did not flag the banned function in the control file, "
                        "so a clean result would mean nothing")


def run(devskim, root, reports, trees, exceptions):
    reports.mkdir(parents=True, exist_ok=True)
    scanned = []
    errors = []
    for tree in trees:
        report = reports / (tree.replace("/", "_") + ".sarif")
        try:
            analyzed = scan(devskim, root / tree, root, report)
            if analyzed == 0:
                raise ScanError("DevSkim analyzed no files; is the directory empty "
                                "(was the Skia submodule checked out)?")
            print(f"{tree}: {analyzed} files analyzed")
            scanned.append(report)
        except ScanError as error:
            errors.append(f"{tree}: {error}")
    try:
        check_control(devskim, reports)
        print("control: the banned function in the control file was flagged")
    except ScanError as error:
        errors.append(f"control: {error}")

    exit_code = 0
    if scanned:
        arguments = ["--root", str(root)]
        if exceptions:
            arguments += ["--exceptions", str(exceptions)]
        exit_code = check_devskim.main(arguments + [str(report) for report in scanned])
    for error in errors:
        print(f"DevSkim scan error: {error}", file=sys.stderr)
    return 2 if errors else exit_code


def main(argv=None):
    parser = argparse.ArgumentParser(description="Scan the Microsoft-owned sources with DevSkim and fail on banned functions.")
    parser.add_argument("--devskim", default="devskim", help="the DevSkim executable (default: devskim from PATH)")
    parser.add_argument("--reports", type=Path, default=ROOT / "output" / "devskim",
                        help="where the SARIF reports and logs are written (default: output/devskim)")
    args = parser.parse_args(argv)
    return run(args.devskim, ROOT, args.reports.resolve(), TREES, EXCEPTIONS)


if __name__ == "__main__":
    sys.exit(main())
