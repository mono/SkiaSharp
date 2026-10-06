# Writing Documentation

This guide covers the documentation that `mono/SkiaSharp` owns. For the system
map, read [docs-overview.md](docs-overview.md) first.

## Conceptual documentation

Conceptual tutorials and guides live in `documentation/docfx/guides/` and are
published at <https://mono.github.io/SkiaSharp/docs/>. Build and preview them
with the instructions in [site.md](site.md).

## Public API prose and compiler XML

Write public API prose as C# `///` comments in `binding/` and `source/` with
the corresponding API change. This is the authoritative source. Use the
[`api-docs` skill](../../.agents/skills/api-docs/SKILL.md) for authoring and
review criteria.

Managed builds emit compiler XML and package it next to each managed assembly:

```
lib/<tfm>/<Assembly>.dll
lib/<tfm>/<Assembly>.xml
ref/<tfm>/<Assembly>.dll
ref/<tfm>/<Assembly>.xml
```

The two XML files intentionally originate from the same compiler output. The
reference assembly and its XML define the consumer-visible documentation
surface; implementation-only XML entries do not make an API public.

Generated binding comments are source-controlled. Edit only their `///` trivia
directly and run:

```bash
pwsh -NoLogo -NoProfile -File ./utils/generate.ps1
```

Verify the comment survives regeneration. Never directly edit generated
declarations, interop code, or compiler XML.

### Build and package validation

Build the affected managed project and run the package assertion that guards
the `lib`/`ref` XML contract:

```bash
dotnet build binding/SkiaSharp/SkiaSharp.csproj
pwsh ./scripts/infra/package/tests/AssembleArcadeAssets.Tests.ps1
```

The package test creates representative archives and proves that every
reference assembly has matching XML at both its `ref/<tfm>` and `lib/<tfm>`
paths. Packaging itself enforces the same rule while assembling assets.

## External API-reference generation

`mono/SkiaSharp-API-docs` independently consumes published package/compiler-XML
and `_DocsMedia` inputs. It owns ECMA/mdoc generation, validation, Microsoft
Learn publishing, and the temporary deferral of Uno output. The parent
repository no longer contains a docs checkout, XML source, mdoc command, sync
workflow, or local writer workflow. Do not edit external generated output from
this repository.

`documentation/api-media/` is versioned here and packaged as `_DocsMedia`, a
nonshipping unsigned transport artifact. Retrieve a matching published package
family and media artifact when operating the external generator; the external
repository owns that retrieval and the media's final presentation.

## Acquiring package inputs

Use an existing acquisition path matching the build you need:

| Package source | Supported path |
|---|---|
| Pull request build | `pwsh scripts/get-skiasharp-pr.ps1 <PR> -SuccessfulOnly -Force`, then use `~/.skiasharp/hives/pr-<PR>/packages/` |
| Pull request build (macOS/Linux) | `./scripts/get-skiasharp-pr.sh <PR> --successful-only --force` |
| Exact public build | Download that build's canonical `nuget` pipeline artifact and extract non-symbol `.nupkg` files |
| Promoted build | Retrieve the branch-versioned `_NuGets` transport package from the public `dotnet-libraries-transport` feed and extract its package family |

The retired parent documentation-download Cake target is not an acquisition
path. Do not substitute `externals-download`: it retrieves native binaries for
managed development, not a package family for documentation generation.

## Release notes and API diffs

Release notes and API diffs are a separate parent-owned documentation system.
The API-diff engine compares published NuGet packages and writes package and
assembly scoped diffs under `documentation/docfx/releases/`; the release-notes
engine creates facts, accepts reviewed prose JSON, and renders release pages.
Their exact layout, baseline selection, supersession policy, and co-release
contract are specified in
[release-notes-and-api-diffs.md](release-notes-and-api-diffs.md).

Use the release-notes entry points for deterministic regeneration:

```bash
dotnet tool restore
.agents/skills/release-notes/scripts/prepare.sh --force
.agents/skills/release-notes/scripts/render.sh
```

Do not hand-edit generated release pages. Change the owned facts, prose, or
renderer and regenerate.
