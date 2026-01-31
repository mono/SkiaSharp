# Versioning

This document explains SkiaSharp's version numbering scheme.

Versioning in SkiaSharp is mostly simple, but is slightly different because it is based on the version of the underlying skia library version.

## Stable Versions

As with all version numbers, there are 4 parts. However, these parts are not strictly the `major.minor.build[.revision]` that is typically associated with .NET versioning.

The version number for SkiaSharp is as follows: `major.skia.minor.patch`

 - **`major` represents a major upgrade that radically changes the underlying API and/or _behavior_**  
   This usually happens after a long period of using the same skia version and several iterations have finally made APIs obsolete. Or, it may be the case that there are fairly large behavior changes that might warrant some closer testing.  
   _An example would be when v2 changed the way color spaces were loaded and handled as well as the way text was processed._
 - **`skia` represents the milestone/tag provided by Google**  
   In many cases, this is not really a major change in the same way `major` because even though the engine has received many updates, the overall behavior is very similar. There are often some slight behavior changes, but not in such a way that will break existing apps/libraries.
 - **`minor` represents an update with either bug fixes or new APIs**  
   This is usually when a new feature is added or a bug is fixed. This is typically not a breaking change, and usually does not have behavior changes (except to fix the bug).
 - **`patch` represents a very minor change to either the assemblies or packaging**  
   This is not very common, but sometimes there is an issue that affects the usage of the library, but not the library itself.
   An example was with v1.68.1.1 when there was an issue with the assembly strong naming and it was only delay signed.

## Preview Labels

With regards to the pre-release versioning, it follows the typical pattern. However, there is one type that is a bit different.

> Although several preview versions are found on [nuget.org](https://nuget.org), many more are also periodically released to the preview feed:  
> https://aka.ms/skiasharp-eap/index.json

> **Note:** There are two Azure DevOps feeds:
> - **Preview feed** (`SkiaSharp`): Contains regular packages (`SkiaSharp`, `HarfBuzzSharp`, etc.) for public testing
> - **CI feed** (`SkiaSharp-CI`): Contains internal build artifacts (`_NuGets`, `_Symbols`, `_NativeAssets`, etc.) used by the release pipeline

Typically, the pre-release labels are:
 - `-alpha` is very early and has not really been tested
 - `-preview` is mostly good and works, but a few more things need to be done (bugs, features, discussions)
 - `-rc` is almost ready to go out, but is waiting on third party feedback (not too common)

### PR Packages

PR builds (`-pr.xxxx.yy` versions) are **not** published to any NuGet feed. These builds are unsigned and only available as pipeline artifacts from the public Azure DevOps CI.

**Download with a single command (no repo clone needed):**

```powershell
# Download PR #1234 packages
iex "& { $(irm https://raw.githubusercontent.com/mono/SkiaSharp/main/scripts/get-skiasharp-pr.ps1) } 1234"

# Then add as NuGet source
dotnet nuget add source ~/.skiasharp/hives/pr-1234/packages --name skiasharp-pr-1234
```

Packages are installed to `~/.skiasharp/hives/pr-{number}/packages/`.

**Options:**
- `-SuccessfulOnly` - Only use successful builds (if latest build failed but you want last good one)
- `-Force` - Overwrite existing packages
- `-List` - Show available artifacts without downloading

**Manual download via Azure DevOps UI:**

1. Go to: https://dev.azure.com/xamarin/public/_build?definitionId=4
2. Find the build for your PR
3. Click "Artifacts" → download the `nuget_preview` (or `nuget`) artifact
4. Extract and use as a local NuGet source

## Native API Versions

Many of the releases of SkiaSharp use the exact same git SHA between releases (ie: 1.68.2, 1.68.2.1, 1.68.3) so we make use of the fact that the API is being versioned, not the library. So, for example, if the C++ library has a bug and we fix that but do not touch the C API, then we do not have to bump the second version number - because the API did not change.

There are probably going to be two numbers used to make up a version: the skia milestone and the C API iteration (restarting with each milestone). For example, the new v2.80.0 will be "80.0". If we do a managed-only update, then the native version is unchanged. If we do a new binding and add a new C API, then the native version becomes "80.1". If we then do another bugfix somewhere in the native code, but do not touch the C API, then we leave the version number as the same.

With regards to backwards compat, I'll make sure that the C API never breaks for a major milestone and is always backwards compatible in that milestone. So, if the native library is "80.2", then this will work with the managed libraries that are compatible with "80.0" to "80.2". But not with a managed library built against "80.3" or later.

| Changes | NuGet/Managed | Native |
| :--------- | :------------------ | :------ |
| Initial release | v2.80.0 | **80.0** |
| C# change | v2.80.1 | 80.0 |
| C API added | v2.80.2 | **80.2** |
| C++ bug fixed | v2.80.3 | 80.2 |
| Packaging fix | v2.80.3.1 | 80.2 |
| C# API added | v2.80.4 | 80.2 |
| C API added | v2.80.5 | **80.5** |
| Update skia | v2.84.0 | **84.0** |

_We do skip iteration values, but that is not so important._