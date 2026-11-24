#!/usr/bin/env python3
"""Write the tracking project's version.props, pinning an exact SkiaSharp version.

Usage: write-version-props.py <version> <props-path>

Emits an exact `[version]` restore constraint (so a newer nightly publishing
mid-run cannot roll the pin up) plus the core MAJOR.MINOR.PATCH
(SkiaSharpResolvedVersion) the csproj uses to select API-level #if defines.
No network access.
"""

import sys


def main(argv: list[str]) -> int:
    if len(argv) != 2:
        print("usage: write-version-props.py <version> <props-path>", file=sys.stderr)
        return 2

    version, path = argv
    core = version.split("-", 1)[0]  # System.Version-parseable core, no prerelease suffix
    with open(path, "w", encoding="utf-8") as fh:
        fh.write(
            "<Project>\n"
            "  <!-- Generated in CI; do not commit. Pins one exact version across all legs. -->\n"
            "  <PropertyGroup>\n"
            f"    <SkiaSharpTrackingVersion>[{version}]</SkiaSharpTrackingVersion>\n"
            f"    <SkiaSharpResolvedVersion>{core}</SkiaSharpResolvedVersion>\n"
            "  </PropertyGroup>\n"
            "</Project>\n"
        )
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
