#!/usr/bin/env python3
"""Run the unfiltered console test solution and record the actual test result."""

from validation_common import argument_parser, run_validation


def main() -> int:
    parser = argument_parser(__doc__)
    return run_validation("tests", parser, parser.parse_args(), [
        ["dotnet", "test", "tests/SkiaSharp.Tests.Console.slnx", "-p:TargetFramework=net10.0", "-p:TargetFrameworks=net10.0"],
    ])


if __name__ == "__main__":
    raise SystemExit(main())
