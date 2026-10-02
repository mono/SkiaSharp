# API documentation validation

Validate C# source comments through managed compilation and the package
contract. Do not validate by editing, formatting, or regenerating ECMA/mdoc or
compiler XML by hand.

## 0. Refresh conventions from official sources

For a broad documentation pass or when a convention is uncertain, retrieve the
current [C# recommended XML tag guidance](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags),
[documentation rules](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/documentation-warnings),
and [CA1200 `cref` guidance](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1200).
Compare any change against the managed build and the external API-docs
consumer. Official style guidance does not by itself justify rewriting valid
repository-specific rich Markdown/CDATA comments or their media links.

## 1. Build the changed managed project

Build each project that owns changed source comments. For core SkiaSharp:

```bash
dotnet build binding/SkiaSharp/SkiaSharp.csproj
```

Use the corresponding changed managed project for other assemblies (for
example, a views or HarfBuzzSharp project). Treat compiler warnings about
malformed documentation, unresolved `cref`, or invalid `inheritdoc` as
documentation defects and correct the source comment.

When generated binding documentation changed, edit only the `///` trivia in
the source-controlled generated file, then run a preservation round trip:

```bash
pwsh -NoLogo -NoProfile -File ./utils/generate.ps1
dotnet build binding/SkiaSharp/SkiaSharp.csproj
```

Verify the intended comments remain and no generated declarations or interop
implementation were manually changed.

## 2. Confirm compiler XML

After the build, locate the compiler-produced XML next to the built managed
assembly and verify that it is well-formed:

```bash
find binding/SkiaSharp/bin -type f -name 'SkiaSharp.xml' -print
```

For each resulting artifact, parse it rather than relying only on file
presence:

```powershell
Get-ChildItem binding/SkiaSharp/bin -Recurse -Filter SkiaSharp.xml |
    ForEach-Object { [xml](Get-Content $_.FullName -Raw) | Out-Null; $_.FullName }
```

The compiler XML is generated output and must not be manually corrected.

## 3. Verify the package XML contract

The package assembly test exercises the packaging assertion that every
reference assembly has matching compiler XML beside both its reference and
implementation assemblies:

```bash
pwsh -NoLogo -NoProfile -File ./scripts/infra/package/tests/AssembleArcadeAssets.Tests.ps1
```

For a package-producing change, also inspect the produced representative
`.nupkg` archive. For every `ref/<tfm>/<Assembly>.dll`, require:

```text
ref/<tfm>/<Assembly>.xml
lib/<tfm>/<Assembly>.dll
lib/<tfm>/<Assembly>.xml
```

The XML copies intentionally originate from the same compiler output. The
`ref/<tfm>` XML is the consumer-visible API documentation contract; the
`lib/<tfm>` copy is required alongside its implementation assembly but does
not turn implementation-only APIs into public surface.

## Completion evidence

Record the changed projects built, the compiler XML paths found, the package
test result, and (when packaging applies) the archive paths inspected. Do not
run retired parent documentation targets: this repository has no mdoc/XML
source-editing validation workflow.
