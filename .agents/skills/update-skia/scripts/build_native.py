#!/usr/bin/env python3
"""Restore tools and build native source for one explicit platform/architecture."""

from validation_common import argument_parser, run_validation


def main() -> int:
    parser = argument_parser(__doc__)
    parser.add_argument("--platform", required=True)
    parser.add_argument("--arch", required=True)
    args = parser.parse_args()
    return run_validation("native", parser, args, [
        ["dotnet", "tool", "restore"],
        ["dotnet", "cake", f"--target=externals-{args.platform}", f"--arch={args.arch}"],
    ])


if __name__ == "__main__":
    raise SystemExit(main())
