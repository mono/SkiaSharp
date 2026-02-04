# SkiaSharp Dashboard Data Cache

This branch contains cached data from GitHub and NuGet APIs for the SkiaSharp Dashboard.

## Structure

```
docs-data-cache/
├── meta.json              # Global metadata
├── github/
│   ├── sync-meta.json     # GitHub sync state
│   ├── index.json         # All items (issues + PRs) lightweight
│   └── items/             # One file per issue/PR
│       └── {number}.json
├── nuget/
│   ├── sync-meta.json     # NuGet sync state
│   ├── index.json         # All packages lightweight
│   └── packages/          # One file per package
│       └── {package-id}.json
```

## Sync Schedule

- **Hourly**: Data is synced from GitHub and NuGet APIs via the `sync-data-cache.yml` workflow
- **Layer 1**: Basic item data (all issues/PRs)
- **Layer 2**: Engagement data (comments, reactions) for top 50 most recently updated items

## Usage

This branch is used by the dashboard build workflow:
1. Checkout `docs-dashboard` branch (source code)
2. Checkout `docs-data-cache` branch (this) as `.data-cache`
3. Run `dotnet run -- generate --from-cache .data-cache` to create dashboard JSON

## Do Not Edit Manually

Files in this branch are automatically updated by GitHub Actions.
