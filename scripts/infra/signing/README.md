# Arcade NuGet signing

SkiaSharp signs already-built NuGet packages in a separate internal stage. The
native and managed product builds remain owned by Cake.

`azure-pipelines-package.yml` defines package and signing as sibling stages. The
signing stage is defined in `azure-templates-stages-signing.yml`; the shared stage
aggregator and package-stage template do not own signing.

## Supported integration

The repository carries an automation-owned `eng/common` tree matched to the
`Microsoft.DotNet.Arcade.Sdk` version in `global.json` and
`eng/Version.Details.xml`. Do not edit `eng/common` directly or update the SDK
without syncing that tree through Darc dependency flow.

The signing job uses Arcade's supported entry points:

- `/eng/common/templates-official/job/job.yml`
- `eng/common/build.ps1 -restore -sign`
- `eng/common/sdk-task.ps1 -task SigningValidation`

It does not invoke SignTool directly or use Arcade's private bootstrap targets.
MicroBuild test signing validates MacDeveloper policy without rewriting dylib
payloads; real signing requires those dylibs to change.

Arcade performs the cryptographic checks:

- SignTool recursively verifies mapped outputs after each signing operation;
- real signing additionally runs `SigningValidation`, whose SignCheck NuGet
  verifier applies NuGet integrity and signature trust/validity verification.

`dotnet nuget verify --all` is not repeated because it currently performs the
same NuGet signature verification already executed by SignCheck.

The repository payload verifier compares unsigned and signed archives to reject
package-set changes, duplicate or case-colliding paths, stale or unmapped
policy entries, unexpected payload mutations, missing expected mutations, and
changes to intentionally skipped source files. Arcade's signature checks do not
compare signed output against the original archive.

`verify-signed-packages.ps1` performs that before/after fidelity comparison and
writes the verification result. It imports `NuGetPayload.psm1`; the module is
not executed directly.
Contents under `.dSYM` bundles are inventoried and must remain byte-identical,
but are not signing targets because they contain debug-symbol payloads rather
than runtime binaries.
Apple native-assets packages receive explicit `.symbols.nupkg` companions. They
contain the normal package payload plus flattened per-architecture DWARF Mach-O
files from the build's dSYM bundles. Mac Catalyst symbol packages also include
the unpacked framework binary because the customer package carries that binary
inside a framework ZIP. Arcade recursively indexes these files by Mach-O UUID.
`tests/Signing.Tests.ps1` is a local regression suite and is not a pipeline
signing step.

## Artifact flow

```text
package stage: unsigned nuget
              |
              v
signing stage: stage under artifacts/packages/Release/Shipping
              |
              +-- Arcade SignTool test/real signing and recursive post-sign checks
              +-- validate eng/Signing.props and compare unsigned/signed payloads
              +-- real only: Arcade SigningValidation trust/validity checks
              +-- create the signed preview-package view
              |
              +-- nuget_signed
              +-- nuget_signing_verification
              `-- nuget_signing_logs
```

The `artifacts/packages/Release/Preview` directory is still required for Arcade
to select preview packages when it generates the V3 manifest. It is an internal
layout, not a separately published pipeline artifact.

Arcade also emits three standard publishing artifacts that must be preserved:

- `PackageArtifacts` contains package bytes staged for BAR-backed feed publishing.
- `BlobArtifacts` contains blob assets such as Arcade-generated symbol packages.
- `AssetManifests` describes the registered package and blob inventory consumed
  by the BAR publishing job.

The repository publishes one `nuget` pipeline artifact containing both normal
and `*.symbols.nupkg` packages. The complete set is signed and staged in the
active Arcade shipping or preview view.

Arcade classifies every existing `*.symbols.nupkg` as a symbol blob and indexes
its supported payload files in the target symbol servers. For a normal package
without a matching symbol package, Arcade generates one by copying the signed
normal package; this covers packages that embed portable or native PDBs. Android
native-assets projects provide real symbol packages because their large
`*.so.dbg` sidecars do not belong in customer shipping packages.

The existing unsigned artifacts remain available to pipeline consumers.
`nuget_special` contains transport wrappers (`_NuGets`, `_NativeAssets*`, and
dependency chunks). Official builds stage these under Arcade's `NonShipping`
package directory without signing them, verify they remain byte-identical and
unsigned, and record them in the same BAR with `NonShipping=true`. Package-scoped
`DO-NOT-SIGN`/`DO-NOT-UNPACK` entries in `eng/SignCheckExclusionsFile.txt`
require unsigned transport containers without recursively re-validating their
build-input payloads.

Arcade's global fallback symbol generation is disabled. SkiaSharp preserves
genuine `*.symbols.nupkg` files and creates explicit byte-identical fallback
copies only for shipping packages that embed their PDBs. Transport wrappers do
not receive symbol blobs.

SkiaSharp and Arcade use the same stable .NET SDK from `global.json`. The
10.0.2xx feature band includes the `dotnet package download` command required by
generated Arcade bootstrap.

Signing uses real ESRP certificates on `main` and `release/*`. Other branches
test-sign unless an authorized manual run explicitly sets `forceRealSigning`.
That override retains the same signed and verification artifacts as a trusted
branch run; it does not publish them to a package feed.

## Policy

`eng/Signing.props` is the only signing-policy source of truth. It removes
Arcade's broad extension defaults and lists every signable basename explicitly:

- `FirstPartyFile` uses `Microsoft400`;
- `MacDeveloperFile` uses `MacDeveloperVNext`;
- `ThirdPartyFile` uses `3PartySHA2`;
- `SkippedFile` uses `None`;
- outer `.nupkg` containers use `NuGet`.

Adding a DLL, EXE, WINMD, dylib, JavaScript, or Python payload without updating
the policy fails the payload-verification step.

Browser and Emscripten JavaScript ships as source and is not Authenticode-signed.
It is supported only when consumed from the author-signed NuGet package; the
pipeline does not authenticate loose copies. `NoSignJS` opts into Arcade's
current JavaScript policy, while every known basename remains explicitly listed
as `SkippedFile`.

`eng/SignCheckExclusionsFile.txt` mirrors the JavaScript subset of `SkippedFile`
using package/path-scoped `DO-NOT-SIGN` entries. SignCheck fails if one of those
files becomes signed. SignCheck does not verify Python signatures, so the
generated Python source is controlled only by `CertificateName=None` and the
payload fidelity verifier. That verifier requires every skipped file to remain
byte-identical. No detached catalog is generated or shipped.

## Test and real signing

The internal package pipeline signs automatically for every non-PR run. The
public pipeline never enables the signing stage. Internal signing mode uses the
repository's established policy:

- `main` and `release/*` use real signing;
- an explicit `forceRealSigning` queue parameter uses real signing;
- all other branches use test signing.

`forceRealSigning` does not register a BAR by itself. A non-main/release branch
must also set `registerInBar` to opt into BAR registration and validation.

API Scan runs on scheduled main builds as the asynchronous compliance check,
or when explicitly requested with `runApiScan`. It does not gate each ordinary
main or release branch build.

Real signing must be enabled only after ESRP onboarding and protected-branch
checks are configured. Arcade then supplies:

- `MicroBuild Signing Task (DevDiv)`;
- the dnceng Windows PME endpoint;
- MicroBuild install and cleanup;
- `System.AccessToken` handling.

The package pipeline registers real-signed packages in BAR. Maestro channel
promotion and final NuGet.org publication remain separate operations. CI stops
after BAR registration and Arcade validation; a selected BAR is promoted
manually only after the downstream Tests pipeline succeeds. Each run
produces one package family:

- normal labels register the signed `Preview` view;
- `PREVIEW_LABEL=stable` registers exact signed `Shipping` packages and sets
  `DotNetFinalVersionKind=release`.

Exact release mode is accepted only for an internal `release/*` branch, which
uses real signing. API Scan remains an independent scheduled-main or ad hoc
compliance stage. Arcade V3 marks the BAR stable and publishes packages to a
dynamically created isolated feed, not directly to NuGet.org or a permanent
shared feed.

## Local checks

Run archive-integrity tests:

```powershell
pwsh -NoLogo -NoProfile -File scripts/infra/signing/tests/Signing.Tests.ps1
```

Run an offline Arcade signing dry run by staging packages under
`artifacts/packages/Release/Shipping` and omitting `OfficialBuildId`:

```powershell
eng/common/build.ps1 -configuration Release -restore -sign -ci `
  /p:DotNetSignType=test `
  /p:TeamName=".NET MAUI" `
  /p:PostBuildSign=false
```
