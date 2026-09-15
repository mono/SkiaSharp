# API documentation validation

Validate source comments through the managed build and package contract. Do
not validate by editing or formatting generated compiler XML.

```bash
dotnet build binding/SkiaSharp/SkiaSharp.csproj
dotnet test tests/SkiaSharp.Tests.Console.slnx \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0
```

For a package-producing change, the repository package test verifies that each
representative managed package with reference assemblies places the compiler
XML next to its assembly in both `lib/<tfm>` and `ref/<tfm>`. The copies are
intentionally the same compiler XML; consumers should treat the `ref` copy as
authoritative because it describes only the reference assembly surface.

Review build warnings caused by malformed XML comments, invalid `cref` values,
or unresolved inheritdoc content as documentation defects. Confirm paths and
commands before publishing guidance; the retired parent documentation targets
are not validation commands.
