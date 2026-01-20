# Release Checklist

Verification steps to complete before publishing a SkiaSharp release.

## Before Publish

 - Native Assets
    - All assets have not increased in size
    - All asset file sizes are in the range 4MB (x86/arm) to 6MB (x64/arm64)
 - Managed Assemblies
    - All assemblies are strong named (except forms and workbooks as they depend on unsigned assemblies)
    - All assemblies have assembly attributes
 - NuGets
    - All tags are correct (if new platforms were added)
    - All XML intellisense files are alongside the assemblies
    - All NuGets are signed
    - All NuGets are correct - fwlinks, legal, icons, license
 - Samples
    - All samples build and deploy in Release
    - All samples gallery samples render correctly for both CPU and GPU
    - All samples in "samples.zip" have valid paths/references and deploy in Release
    - The UWP samples pass the WACK tests (Windows App Certification Kit)
 - Workbooks
    - All workbooks are using the latest SkiaSharp
    - All workbooks run
 - Docs
    - All docs have been generated from the assemblies
    - All docs are completed (no "To be added.")
    - The docs may need to be re-generated if there are new extension methods

## Publish

 - Do not publish the preview NuGets
 - The "samples.zip" is attached to the release
 - Both "libSkiaSharp.so" and "libHarfBuzzSharp.so" are attached to the release

## After Publish

 - The repo is tagged with v1.MM.x
 - The milestones/projects are closed with a link to the release notes
 - The release notes are completed
