# Arcade package signing and publishing

SkiaSharp builds packages with Cake, then uses Arcade for signing, BAR
registration, validation, and Darc promotion.

## Pipeline flow

```text
Package
  nuget         normal + explicit .symbols.nupkg packages
  nuget_special unsigned _NuGets / _NativeAssets transport packages
      |
      v
Sign NuGets
  download nuget
  sign and verify package payloads
  publish nuget_signed
      |
      v
Assemble Arcade assets
  download nuget_signed + nuget_special
  stage signed packages in Shipping
  stage unsigned transport in NonShipping
  add byte-identical fallback .symbols.nupkg copies when needed
  generate PackageArtifacts, BlobArtifacts, and AssetManifests
      |
      v
Register BAR -> Arcade validation -> Darc promotion
```

The publish assembly stage runs only when BAR registration is enabled. Transport
packages never enter the signing stage.

## Signing policy

`eng/Signing.props` is the source of truth:

- shipping `.nupkg` files use the NuGet certificate;
- first-party binaries use `Microsoft400`;
- Apple runtime dylibs use `MacDeveloperVNext`;
- source payloads listed as `SkippedFile` remain unchanged.

Normal and explicit symbol packages share the `nuget` artifact and are signed
together. dSYM DWARF payloads are inventoried but not signing targets. Fallback
symbol packages copy an already-signed package.

Arcade SignTool verifies the signed outputs. Standard post-build NuGet and
SigningValidation jobs validate the final BAR inventory.

## Publishing policy

`eng/Publishing.props` publishes:

- signed product and symbol packages from `Shipping`;
- unsigned transport packages from `NonShipping`.

Arcade records both in one BAR. Maestro routes shipping packages, non-shipping
transport, and symbol blobs to their configured destinations.

`eng/SignCheckExclusionsFile.txt` marks transport packages
`DO-NOT-SIGN, DO-NOT-UNPACK`, preventing signatures and recursive checks of raw
build inputs.

Each run produces one package family. `PREVIEW_LABEL=stable` creates exact
release versions; other labels create prerelease versions in the same Shipping
view.

## Signing modes

- `main` and `release/*`: real signing and BAR publishing.
- Other internal branches: test signing.
- `forceRealSigning=true`: real signing.
- A feature branch also needs `registerInBar=true` to assemble and publish a BAR.

Public PR builds do not run the signing or publishing stages.
