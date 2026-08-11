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

## Test and real signing

The pipeline currently calls the signing template with `useRealSigning: false`.
That produces MicroBuild test signatures without referencing an ESRP service
connection.

Real signing must be enabled only after ESRP onboarding and protected-branch
checks are configured. Arcade then supplies:

- `MicroBuild Signing Task (DevDiv)`;
- the dnceng Windows PME endpoint;
- MicroBuild install and cleanup;
- `System.AccessToken` handling.

There is no queue-time force-real parameter.

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
