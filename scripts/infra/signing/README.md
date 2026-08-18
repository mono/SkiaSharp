# Arcade NuGet signing

SkiaSharp signs already-built NuGet packages in a separate internal stage. The
native and managed product builds remain owned by Cake.

`azure-templates-stages.yml` defines package and signing as sibling stages. The
signing stage imports `azure-templates-jobs-signing.yml`; the package stage
template does not import signing or any other stage.

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
              +-- nuget_preview_signed
              +-- nuget_signing_verification
              `-- nuget_signing_logs
```

The existing unsigned artifacts remain available to the test pipeline.
`nuget_symbols` and the internal `nuget_special` convenience packages are not
signing inputs.

Signing consumes only the current run's `nuget` artifact after the package stage
succeeds. Retry a signing failure within that run; starting a new pipeline run
rebuilds the packages instead of signing artifacts from an older run.

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

`eng/SignCheckExclusionsFile.txt` mirrors the JavaScript subset of `SkippedFile`
using package/path-scoped `DO-NOT-SIGN` entries. SignCheck fails if one of those
files becomes signed. SignCheck does not verify Python signatures, so the
generated Python source is controlled only by `CertificateName=None` and the
payload fidelity verifier. That verifier requires every skipped file to remain
byte-identical.

## Test and real signing

The internal package pipeline signs automatically for every non-PR run. The
public pipeline never enables the signing stage. Internal signing mode uses the
repository's established policy:

- `main` and `release/*` use real signing;
- an explicit `forceRealSigning` queue parameter uses real signing;
- all other branches use test signing.

Real signing must be enabled only after ESRP onboarding and protected-branch
checks are configured. Arcade then supplies:

- `MicroBuild Signing Task (DevDiv)`;
- the dnceng Windows PME endpoint;
- MicroBuild install and cleanup;
- `System.AccessToken` handling.

The package pipeline registers real-signed packages in BAR. Maestro channel
promotion and final NuGet.org publication remain separate operations.

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
