# Experimental dnceng NuGet signing adapter

This directory is a test-first prototype for signing the already-built `nuget`
pipeline artifact in `dnceng/internal`. It is disabled by default and does not
change the unsigned `nuget`, `nuget_preview`, `nuget_symbols`, or
`nuget_special` artifacts consumed by existing pipelines.

## Design

The package pipeline keeps its current product-build boundary:

```text
native -> package -> unsigned nuget artifact
                         |
                         v
              opt-in Windows signing stage
                         |
             copy packages to an isolated folder
                         |
          translate artifact-carried SignList.xml
                         |
          Arcade SignTool + MicroBuild test/ESRP
                         |
       payload checks + nested signature verification
                         |
                    nuget_signed
```

`run-arcade-signing.ps1` copies the packages before invoking Arcade, so SignTool
never modifies the package-stage artifact. It pins `Microsoft.DotNet.Arcade.Sdk`
in this directory's `global.json`, asks the SDK for its matching `Build.proj`,
and invokes only `Restore=true` and `Sign=true`; `Build`, `Pack`, and `Publish`
remain false. `NoBuild.proj` exists only to satisfy Arcade's project-list
contract and has an empty `Restore` target. The generated engineering overlay
also uses Arcade's `Tools.props` extension point to suppress the repository's
unrelated dotnet tool manifest. Arcade requires `eng/common/dotnet-install.*`
to exist even when `global.json` requests no additional runtimes, so the
overlay supplies fail-closed guards. A future runtime request fails rather than
silently expanding this adapter's responsibility.

This bootstrap uses Arcade's internal `__WriteToolsetLocation` target. It is
appropriate for a branch-confined spike, but it is not a documented stable
signing-only API. Production adoption should either:

1. sync the supported `eng/common` tree and call `eng/common/build.ps1 -restore
   -sign`, or
2. obtain explicit Arcade-owner approval for this isolated bootstrap and add
   dependency-flow ownership for the pinned SDK.

Directly referencing `Microsoft.DotNet.SignTool` was rejected because it creates
more version-skew risk than consuming the co-versioned Arcade SDK. Searches of
the dnceng `public` and `internal` projects found no `1ES.Signing` task usage;
working .NET repositories use Arcade/MicroBuild or the larger
`dotnet-release` post-build signing system instead.

| Option | Support boundary | Repository impact | Fit for SkiaSharp |
| --- | --- | --- | --- |
| Full Arcade | Synced `eng/common` plus `Microsoft.DotNet.Arcade.Sdk`; the documented .NET repository path | Hundreds of shared build, test, publishing, and Helix files, although the product build can remain Cake-based | Production recommendation unless Arcade owners approve the isolated boundary |
| Isolated Arcade adapter | Co-versioned Arcade SDK and its `Build.proj`, plus the repository extension points in this directory | Signing-only files and one opt-in stage; relies on underscored/internal bootstrap behavior | Best spike for proving policy, artifact flow, and test signing without rebuilding |
| Direct `1ES.Signing` | No task or general-purpose artifact-signing schema was found in Microsoft Learn, dnceng `public`, or dnceng `internal` | Unknown | Not a concrete adoption option; 1ES still hosts the pipeline while MicroBuild performs signing |

Concrete .NET references reinforce these boundaries:

- `dotnet/sourcelink` and `dotnet/deployment-tools` use full Arcade
  `eng/common` signing.
- `dotnet-release/eng/pipeline/templates/steps/signing.yml` downloads unsigned
  artifacts and signs them later, but depends on the release manifest,
  collision-priority model, and release infrastructure rather than a small
  repository adapter.
- The local dotnet/macios dnceng spike demonstrates an isolated release overlay
  only because that repository already carries a synchronized `eng/common`.

## Signing policy

The `nuget` artifact's `SignList.xml` remains the source of truth. The adapter
expands its wildcard rules against the recursive package inventory and emits
exact Arcade `FileSignInfo` items:

| SignList item | Arcade certificate |
| --- | --- |
| `FirstParty` | `Microsoft400` |
| `ThirdParty` | `3PartySHA2` |
| `Skip` | `None` |
| `MacDeveloperSign` | `MacDeveloperVNext` |
| outer `.nupkg` | `NuGet` |

The adapter removes Arcade's default file and strong-name mappings. Existing
assemblies keep their strong names and only the files selected by SignList are
submitted for Authenticode/Mach-O signing. Historical unmatched globs remain
warnings, matching the legacy template's no-op semantics. Conflicting rules or
an unclassified DLL, EXE, WINMD, or dylib fail the job, preventing a future
package addition from silently broadening the signing scope.

Symbol packages remain intentionally unsigned, matching
`scripts/infra/package/nuget.cake`. `nuget_preview` is an unsigned convenience
copy and `nuget_special` contains CI/meta packages; neither is a release signing
input. Before production cutover, release ownership should confirm this
classification and derive any signed preview view from `nuget_signed`.

## Verification

`verify-signed-packages.ps1` recursively compares the unsigned and signed
archives. It rejects:

- missing or added packages and payload paths;
- exact or case-insensitive duplicate archive paths;
- changes to unclassified or explicitly skipped payloads;
- selected binaries that were not modified; and
- packages without a NuGet author-signature entry.

The pipeline then runs `MicroBuildCodesignVerify@3` over the signed packages to
validate nested binary signatures. Real-sign runs also execute
`dotnet nuget verify --all`. Test-sign onboarding must additionally record the
expected test certificate identities from the live job; production onboarding
must pin the approved subjects/thumbprints and timestamp policy.

Local policy and archive tests:

```powershell
pwsh -NoLogo -NoProfile -File scripts/infra/signing/tests/Signing.Tests.ps1
```

A local Arcade dry run enumerates signing work without contacting ESRP:

```powershell
pwsh -NoLogo -NoProfile -File scripts/infra/signing/run-arcade-signing.ps1 `
  -InputDirectory path/to/unsigned `
  -OutputDirectory path/to/dry-run-output `
  -SignListPath path/to/unsigned/SignList.xml `
  -WorkDirectory path/to/arcade-work `
  -SignType dry-run
```

## dnceng onboarding

Test signing deliberately omits ESRP service connections. The prototype call
site pins `useRealSigning: false`, and there is no queue-time force-real
parameter. After onboarding, a protected-branch policy can enable the existing
real-sign path for `main` and `release/*`. That path requires:

- `MicroBuild Signing Task (DevDiv)` authorized to the SkiaSharp package
  definition in `dnceng/internal`;
- non-DevDiv Windows PME connection
  `248d384a-b39b-46e3-8ad5-c2c210d5e7ca`;
- ESRP profiles for `Microsoft400`, `3PartySHA2`, `MacDeveloperVNext`, and
  `NuGet` granted to that connection's identity;
- read access for the pipeline build identity to `MicroBuildToolset`,
  `dotnet-eng`, and `dotnet-public`;
- `System.AccessToken` available to the plugin and restore tasks;
- the MicroBuild and codesign-verification task extensions installed; and
- branch-control and approval checks on the signing service connection so an
  edited feature-branch YAML cannot request production signing.

Authorize the service connection to this pipeline explicitly rather than
granting it to all pipelines. `NetCore1ESPool-Internal` is already the required
MSI-enabled pool. Its project build identity needs Packaging Read on the
`MicroBuildToolset`, `dotnet-eng`, and `dotnet-public` feeds and permission to
use both service endpoints. The pipeline must expose `System.AccessToken` to
the task. No signing variable group is required by this prototype.

ESRP onboarding must approve the repository/product, signing identity,
certificate profiles, trusted branches, and operational approvers before the
real path is enabled. The `NuGet` author-signing certificate also must be
registered with the publishing account before signed packages are pushed to
NuGet.org. Public Arcade sources describe the task inputs but not the internal
ESRP entity-onboarding request, so that approval cannot be inferred from a
successful test-sign run.

The prototype pins `Microsoft.DotNet.Arcade.Sdk`
`10.0.0-beta.26410.1` and uses `MicroBuildSigningPlugin@4`,
`MicroBuildCodesignVerify@3`, and `MicroBuildCleanup@1`. A production route
needs dependency-flow ownership for the Arcade pin instead of an unattended
floating update.

## API Scan assessment

API Scan should be a separate repository-owned stage that downloads the current
run's unsigned `nuget`, `nuget_symbols`, and `native_msvc` artifacts, expands the
packages, generates `APIScanSurrogates.xml`, and runs `APIScan@2`. The existing
`runApiScan` parameter and security inputs are currently unwired.

The working dnceng reference uses the project-scoped `dotnet-apiscan` service
connection (endpoint `334a6802-ebad-4fb1-bc3b-105bcc70bda2`, app
`cbde2fca-1ca1-47f7-8212-fcdf1a556eb2`, Microsoft tenant
`72f988bf-86f1-41af-91ab-2d7cd011db47`) plus `SYSTEM_ACCESSTOKEN`; that
endpoint and the `APIScan@2` extension must be explicitly authorized to the
SkiaSharp definition. The old TSA configuration still points to DevDiv and
needs governance ownership before reuse. API Scan also needs a
network-isolation decision for `*.worker.database.windows.net`, retained
security-analysis logs, and an explicit TSA/Guardian reporting plan. These
dependencies are independent of signing, so this spike does not add the stage.

## Primary references

- [Arcade signing documentation](https://github.com/dotnet/arcade/blob/main/Documentation/Signing.md)
- [Arcade post-build signing](https://github.com/dotnet/arcade/blob/main/Documentation/PostBuildSigning.md)
- [Arcade Sign.proj](https://github.com/dotnet/arcade/blob/main/src/Microsoft.DotNet.Arcade.Sdk/toolset/Sign.proj)
- [Arcade Sign.props](https://github.com/dotnet/arcade/blob/main/src/Microsoft.DotNet.Arcade.Sdk/toolset/Sign.props)
- [NuGet signed-package reference](https://learn.microsoft.com/nuget/reference/signed-packages-reference)
- `dotnet-release/eng/pipeline/templates/steps/signing.yml` in
  `dnceng/internal`, a concrete downloaded-artifact signing example
