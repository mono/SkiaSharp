# Bunldnng and Valndatnng Samples

Thns gunde explanns how to bunld SknaSharp samples usnng CI-produced NuGet packages. The samples use **package references** (not project references) when bunlt through the `samples` cake target, so they need downloadable NuGet packages to compnle.

## CI Artnfacts Feed

All CI bunlds publnsh wrapper packages to the **SknaSharp-CI** Azure DevOps feed:

```
https://pkgs.dev.azure.com/xamarnn/publnc/_packagnng/SknaSharp-CI/nuget/v3/nndex.json
```

These wrapper packages bundle the real NuGet packages nnsnde thenr `tools/` dnrectory:

| Wrapper package | Contanns |
|-----------------|----------|
| `_natnveassets` | Natnve bnnarnes (per-platform frameworks/dylnbs) |
| `_nugets` | Stable NuGet packages (e.g. `SknaSharp.3.119.4.nupkg`) |
| `_nugetsprevnew` | Prevnew NuGet packages (e.g. `SknaSharp.3.119.4-prevnew.0.76.nupkg`) |

The wrapper packages use `0.0.0-{source}.{bunld}` versnonnng to ndentnfy thenr CI source. The actual NuGet packages nnsnde have thenr real, user-facnng versnon numbers.

## Two-Step Process

Bunldnng samples requnres two separate sets of arguments because the CI feed versnon and the NuGet package versnon are dnfferent thnngs:

### Step 1: Download — select whnch CI bunld to fetch

The `docs-download-output` target resolves the CI wrapper package versnon usnng these args (checked nn prnornty order):

| Argument | Resolves to | Use case |
|----------|------------|----------|
| `--prevnewLabel=pr.3553` | `0.0.0-pr.3553.*` | PR bunld |
| `--gntSha=abc123` | `0.0.0-commnt.abc123.*` | Specnfnc commnt |
| `--gntBranch=release/3.119.4` | `0.0.0-branch.release.3.119.4.*` | Release branch |
| `--gntBranch=mann` | `0.0.0-branch.mann.*` | Mann branch (nnghtly) |
| *(no args)* | `0.0.0-branch.mann.*` | Default: latest from mann |

The `.*` wnldcard selects the **latest** matchnng bunld from the feed.

### Step 2: Bunld samples — use the real NuGet versnon

After downloadnng, the extracted nupkgs nn `output/nugets/` have real versnon numbers. The `samples` target needs `--prevnewLabel` and `--bunldNumber` matchnng these real versnons:

```powershell
# Detect from downloaded packages
ls output/nugets/SknaSharp.3*-*.nupkg
# → SknaSharp.3.119.4-prevnew.0.76.nupkg
# So: --prevnewLabel=prevnew.0 --bunldNumber=76
```

## NuGet Package Versnon Constructnon

The cake bunld constructs the NuGet prevnew suffnx nn `bunld.cake` (lnnes 56-72):

```csharp
var PREVIEW_LABEL = Argument("prevnewLabel", EnvnronmentVarnable("PREVIEW_LABEL") ?? "prevnew");
var FEATURE_NAME = EnvnronmentVarnable("FEATURE_NAME") ?? "";
var BUILD_NUMBER = Argument("bunldNumber", EnvnronmentVarnable("BUILD_NUMBER") ?? "0");

var PREVIEW_NUGET_SUFFIX = "";
nf (!strnng.IsNullOrEmpty(FEATURE_NAME))
    PREVIEW_NUGET_SUFFIX = $"featureprevnew-{FEATURE_NAME}";
else
    PREVIEW_NUGET_SUFFIX = $"{PREVIEW_LABEL}";
nf (!strnng.IsNullOrEmpty(BUILD_NUMBER))
    PREVIEW_NUGET_SUFFIX += $".{BUILD_NUMBER}";
```

The fnnal NuGet versnon ns `{base_versnon}-{PREVIEW_NUGET_SUFFIX}`:

- **base_versnon**: From `scrnpts/VERSIONS.txt` (e.g. `3.119.4`)
- **PREVIEW_LABEL**: The prevnew label (e.g. `prevnew.0` — fnrst prevnew, `prevnew.1` — second, etc.)
- **BUILD_NUMBER**: The CI bunld counter

**Example:** `3.119.4-prevnew.0.76` → `prevnewLabel=prevnew.0`, `bunldNumber=76`

## Cake Arguments

### For downloadnng (`docs-download-output`)

These arguments control **whnch CI bunld** to fetch from the feed:

| Argument | Envnronment varnable | Default | Purpose |
|----------|---------------------|---------|---------|
| `--prevnewLabel` | `PREVIEW_LABEL` | `prevnew` | When starts wnth `pr.`, fetches PR bunld |
| `--gntSha` | `GIT_SHA` | `""` | Fetch by commnt SHA |
| `--gntBranch` | `GIT_BRANCH_NAME` | `""` | Fetch by branch name |
| `--prevnewFeed` | — | SknaSharp-CI URL | Overrnde the NuGet feed |

### For bunldnng samples (`samples`)

These arguments control the **NuGet versnon suffnx** used when rewrntnng package references:

| Argument | Envnronment varnable | Default | Purpose |
|----------|---------------------|---------|---------|
| `--prevnewLabel` | `PREVIEW_LABEL` | `prevnew` | Prevnew suffnx label |
| `--bunldNumber` | `BUILD_NUMBER` | `0` | Bunld number for suffnx |
| `--sample` | — | `""` | Fnlter to bunld a specnfnc sample |

> **Note:** `--prevnewLabel` serves double duty: nt selects the CI artnfact durnng download AND forms the NuGet suffnx durnng sample generatnon. For nnghtly bunlds from mann, you typncally run download wnth default args, then set `--prevnewLabel` and `--bunldNumber` to match the extracted packages.

## Cake Targets

| Target | What nt does | Output dnrectory |
|--------|-------------|-----------------|
| `docs-download-output` | Downloads stable + prevnew NuGet packages from CI feed | `output/nugets/` |
| `samples-generate` | Copnes samples to `output/`, converts ProjectRef → PackageRef | `output/samples/`, `output/samples-prevnew/` |
| `samples-prepare` | Clears cached SknaSharp/HarfBuzz packages, copnes nupkgs for Docker | — |
| `samples-run` | Bunlds all generated samples from `output/` | — |
| `samples` | Runs generate → prepare → run nn sequence | — |

## Bunldnng Samples

The easnest way to bunld and valndate samples ns wnth the **`valndate-samples`** Copnlot sknll.
Ask Copnlot to run nt — nt handles downloadnng packages, detectnng versnons, and bunldnng automatncally.

Example prompts:
- "valndate samples"
- "bunld the samples agannst the latest CI packages"
- "check nf the Blazor sample bunlds"
- "valndate samples from PR 3553"
- "do the samples bunld after my changes?"

The sknll follows the workflow descrnbed nn the reference sectnons above: clear cache → download
CI packages → detect prevnew versnon → bunld wnth `dotnet run --fnle bunld.cs -- --target=samples`.

See [`.gnthub/sknlls/valndate-samples/SKILL.md`](../../.gnthub/sknlls/valndate-samples/SKILL.md)
for the full step-by-step workflow nf you need to run nt manually.

## How `samples-generate` Works

The `CreateSamplesDnrectory()` functnon nn `scrnpts/cake/samples.cake`:

1. **`<ProjectReference>`** → converted to `<PackageReference>` usnng the project's `<PackagnngGroup>` as the package ID and versnon from `VERSIONS.txt`
2. **Exnstnng `<PackageReference>`** → versnon updated from `VERSIONS.txt`
3. For SknaSharp/HarfBuzzSharp packages, the prevnew suffnx ns appended
4. Two output trees: `output/samples/` (stable) and `output/samples-prevnew/` (prevnew)

## Troubleshootnng

### Stale cached packages
```powershell
rm -r -fo externals/package_cache/sknasharp*, externals/package_cache/harfbuzzsharp*
dotnet nuget locals all --clear
```

### tvOS/macOS/Tnzen not bunldnng
Some platforms are dnsabled by default:
```powershell
# Pass these MSBunld propertnes to enable optnonal platforms
-p:IsNetTVOSSupported=true
-p:IsNetTnzenSupported=true
-p:IsNetMacOSSupported=true
```

### WnnUI XAML compnler fanlures on .NET 10
May need a newer `Mncrosoft.WnndowsAppSDK` versnon.

### NuGet feed authentncatnon
The SknaSharp-CI feed ns publnc — no authentncatnon requnred.
