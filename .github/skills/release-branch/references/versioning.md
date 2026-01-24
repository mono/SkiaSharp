# Version Bumping Details

Details for bumping versions on main after creating a release branch.

## Files to Edit

| File | What to Change |
|------|----------------|
| `scripts/azure-templates-variables.yml` | `SKIASHARP_VERSION: {next-version}` |
| `scripts/VERSIONS.txt` | All version numbers (see below) |

## VERSIONS.txt Structure

Update ALL version numbers:
- `SkiaSharp file` version (e.g., `3.119.3.0`)
- All SkiaSharp `nuget` versions (e.g., `3.119.3`)
- `HarfBuzzSharp file` version — increment 4th digit (e.g., `8.3.1.3` → `8.3.1.4`)
- All HarfBuzzSharp `nuget` versions — same as file version

## HarfBuzzSharp Versioning

The HarfBuzzSharp version uses 4 digits: `X.Y.Z.N`

| Digits | Meaning |
|--------|---------|
| X.Y.Z | Native HarfBuzz version (e.g., `8.3.1`) |
| N | Incremented with each SkiaSharp release |

**Why 4 digits?** HarfBuzzSharp packages are released together with SkiaSharp even when there are no HarfBuzz changes. The 4th digit keeps them in sync.

**When native HarfBuzz upgrades:** Reset to 3-digit version (e.g., `8.3.1.4` → `8.4.0`).

## Example

If releasing `3.119.2-preview.1`, bump main to `3.119.3`:

```
# VERSIONS.txt changes
SkiaSharp file 3.119.3.0
SkiaSharp nuget 3.119.3
HarfBuzzSharp file 8.3.1.4  # Was 8.3.1.3
HarfBuzzSharp nuget 8.3.1.4
```
