With the introduction of the 3.x series, we need a way to properly build 2.x and 3.x versions of SkiaSharp.

As we need a way to build for more than one "main", we will have multiple target branches for any new changes:

 - `main` - this is where all the latest things will be, so 3.x things
 - `release/2.x` - this is where any new things for the previous version, so 2.x things

Once we feel we are near a release, we will branch from either `main` or `release/#.x` and create a new "release" branch:

 - `release/#.0.0-preview.1`  
   This will contain any last-minute fixes or release specific changes for the `#.0.0-preview.1` release. No new features will be added and typically all changes should first go through the appropriate `main` or `release/#.x` branch first.

As part of the release, we will do a few things:

 - Previews
    - Tag the `release/#.0.0-preview.1` branch commit that we actually release with the full release version: `v#.0.0-preview.1.246`
 - Stable
    - Tag the `release/#.0.0` branch commit with the final version: `v#.0.0`

If we ever need to service a version that we released, we can create a new branch from the previous branch:

 - Next Version
    - Branch from the previous stable `release/3.0.0` to a new branch, bumping the patch: `release/3.0.1-preview.1` 
 - Patch Version
    - Branch from the old version `release/2.88.4` to a new branch, bumping the build: `release/2.88.4.1-preview.1`
    - This should only be done in extreme situations - like a security issue or a massive crash
    - The preferred fix is to update to the latest patch
