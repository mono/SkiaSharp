# Utils

This directory contains a few tools to help with the binding and development of SkiaSharp and related libraries.

## SkiaSharpGenerator

This is a small set of tools that help with generating the p/invoke layer from the C header files.

### Generate

This can be run with:

```pwsh
dotnet run --project=utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- generate --config binding/libSkiaSharp.json --skia externals/skia --output binding/Binding/SkiaApi.generated.cs
```

* `--config binding/libSkiaSharp.json`  
  The path to the JSON file that help generate a useful set of p/invoke definions and structures.
* `--skia externals/skia`  
  The path to the root of the skia source.
* `--output binding/Binding/SkiaApi.generated.cs`  
  The path to the generated file.

### Verify

This can be run with:

```pwsh
dotnet run --project=utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- verify --config binding/libSkiaSharp.json --skia externals/skia
```

* `--config binding/libSkiaSharp.json`  
  The path to the JSON file that help generate a useful set of p/invoke definions and structures.
* `--skia externals/skia`  
  The path to the root of the skia source.
* `--output binding/Binding/SkiaApi.generated.cs`  
  The path to the generated file.

## WasmTestRunner

Run the WASM unit tests in a browser.

This can be run with:

```pwsh
dotnet run --project=utils/WasmTestRunner/WasmTestRunner.csproj -- "http://localhost:5000/"
```

* `--output TestResults.xml`  
* `--timeout 30`  
* `--no-headless`  
