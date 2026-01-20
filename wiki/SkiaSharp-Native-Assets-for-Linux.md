As of 1.68.0, we have started to release a separate NuGet package `SkiaSharp.NativeAssets.Linux` that we hope will better support our friends running on Linux.

## The Package

This new package will just contain the various Linux binaries that have had some level of testing and we can confirm that it works in our scenarios. Currently, this package is just going to be the single binary that we have used as part of our core Linux builds: Ubuntu 16.04 amd64. We have tested this binary, and it appears to work on several distros (Ubuntu 14, OpenSUSe and CentOS), so we have opted to place it in the `linux-x64` runtime folder. 

_Note: This package may not work on Alpine just yet, but we will be adding a build for that distro in the near future._

## More Distros

If more distros are needed to enable you to easily consume SkiaSharp, please add your distro configuration to this issue so that we can track requests and make sure we focus on the most popular: https://github.com/mono/SkiaSharp/issues/453

## Usage

The overall result of this is that you will do everything as usual, but in the "app" part of the solution, just add the package. You should not release a NuGet to nuget.org that depends on this directly as you will then force all your uses to use my binary.

## Example Use Case

Imagine a world where I (Matthew) maintain SkiaSharp, and there are 4 other great developers (John, Janet, Joe, and Jane)...

#### Stage 1

 - **Matthew** builds `SkiaSharp` and a binary for Ubuntu 16.
 - **John** builds an Ubuntu app and includes the `SkiaSharp` and `SkiaSharp.NativeAssets.Linux` packages. John is very happy.
 - **Janet** builds an Alpine app and includes `SkiaSharp` and her custom build of `libSkiaSharp.so`. Janet is not as happy as John. Janet has SKILLZ and can build for Alpine.
 - **Joe** is a great guy. Joe likes community. Joe likes Alpine Linux. Joe builds `libSkiaSharp.so` for Alpine. Joe creates a new NuGet package (`JoesCodes.SkiaSharp.NativeAssets.Alpine`) that just contains the Alpine binary.
 - **Jane** is also building for Alpine. Jane uses the `JoesCodes.SkiaSharp.NativeAssets.Alpine` package. Jane is happy.

#### Stage 2

 - **Matthew** has a look at `JoesCodes.SkiaSharp.NativeAssets.Alpine` and sees **1 million downloads**! Matthew is blown away. Matthew knows everyone is using Alpine. Matthew has a chat to Big Boss: "We NEED Alpine". Big Boss has a look. Big Boss agrees. Matthew builds for Alpine and adds it to the `SkiaSharp.NativeAssets.Linux` package. Everyone is happy.

#### Stage 3

 - **Janet** and **Jane** now have options: use "official" or use "community". Janet is very happy. Jane is very happy.
    - **Janet** is Matthew's friend. Janet decides to use `SkiaSharp.NativeAssets.Linux`. Janet and Matthew are happy.
    - **Jane** is happy with `JoesCodes.SkiaSharp.NativeAssets.Alpine`. Jane is Joe's friend. Jane does not change.
 - **Joe** also has options: keep independent and free, or, update `JoesCodes.SkiaSharp.NativeAssets.Alpine` to be an empty shell package with a dependency.
    - **Joe** decides to update to depend on Matthew.
       - **Janet** updates. Janet is happy.
       - **Jane** updates. Jane gets the new package that gets Matthew's build. Jane is happy.
    - **Joe** stays free and builds the next version.
       - **Janet** updates. Janet is happy.
       - **Jane** updates. Jane gets Joe's new package. Jane is happy.
