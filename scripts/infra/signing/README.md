# Arcade NuGet signing

SkiaSharp signs already-built NuGet packages in a separate internal stage. The
native and managed product builds remain owned by Cake.

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

## Artifact flow

```text
package stage: unsigned nuget
              |
              v
signing stage: stage under artifacts/packages/Release/Shipping
              |
              +-- validate eng/Signing.props against every recursive payload
              +-- Arcade SignTool test/real signing
              +-- Arcade recursive signature validation
              +-- compare signed and unsigned archive structure/hashes
              |
              +-- nuget_signed
              +-- nuget_preview_signed
              +-- nuget_signing_verification
              `-- nuget_signing_logs
```

The existing unsigned artifacts remain available to the test pipeline.
`nuget_symbols` and the internal `nuget_special` convenience packages are not
release signing inputs.

For a signing-only retry, queue the package pipeline with
`signingSourceBuildId` set to a successful build ID from that same definition.
The job verifies the build definition, repository, result, and trusted branch
requirements before downloading its `nuget` artifact.

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
the policy fails before signing.

`eng/SignCheckExclusionsFile.txt` mirrors the JavaScript subset of `SkippedFile`
using package/path-scoped `DO-NOT-SIGN` entries. SignCheck fails if one of those
files becomes signed. SignCheck does not verify Python signatures, so the
generated Python source is controlled only by `CertificateName=None` and the
payload fidelity verifier. That verifier requires every skipped file to remain
byte-identical.

## Test and real signing

The package pipeline selects signing mode using the repository's established
policy:

- `main` and `release/*` use real signing;
- an explicit `forceRealSigning` queue parameter uses real signing;
- all other branches use test signing.

Real signing must be enabled only after ESRP onboarding and protected-branch
checks are configured. Arcade then supplies:

- `MicroBuild Signing Task (DevDiv)`;
- the dnceng Windows PME endpoint;
- MicroBuild install and cleanup;
- `System.AccessToken` handling.

The package pipeline has no release trigger or feed-publishing step. Real-signed
artifacts remain internal pipeline artifacts until a separately protected
release definition selects and publishes an exact successful signing run.

## Local checks

Validate the versioned policy against an unsigned package directory:

```powershell
pwsh -NoLogo -NoProfile -File scripts/infra/signing/validate-signing-policy.ps1 `
  -PackageDirectory path/to/nugets `
  -SigningPropsPath eng/Signing.props `
  -OutputPath artifacts/signing-policy.json
```

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
