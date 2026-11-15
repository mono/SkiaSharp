#!/usr/bin/env python3
"""Build the SkiaSharp managed bindings and record the actual build result."""

from validation_common import argument_parser, run_validation


def main() -> int:
    parser = argument_parser(__doc__)
    return run_validation("managed", parser, parser.parse_args(), [
        ["dotnet", "build", "binding/SkiaSharp/SkiaSharp.csproj"],
    ])


if __name__ == "__main__":
    raise SystemExit(main())
