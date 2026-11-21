# Banned-API scan (DevSkim)

CI runs [DevSkim](https://github.com/microsoft/DevSkim) over the Microsoft-owned sources and fails when it finds a
banned C or C++ function such as `strcpy`, `sprintf` or `getenv`. This supports the Microsoft security requirement
that code analysis tools must not detect banned APIs (Microsoft.Security.SystemsADM.10082). The scan makes the
findings visible and blocking; running it does not by itself establish compliance.

## Where it runs

The `DevSkim` stage of the public pipeline ([`azure-pipelines-complete.yml`](../../scripts/azure-pipelines-complete.yml)),
for pull requests and for CI builds of `main` and `release/*`. It is defined in
[`azure-templates-stages-devskim.yml`](../../scripts/azure-templates-stages-devskim.yml). It reads sources only, so it needs
no native build and runs in parallel with the other stages. The internal `skiasharp-package` pipeline does not run it.

The stage checks out the Skia submodule, runs the unit tests of the gate, installs the pinned DevSkim CLI from the
repository's own `nuget.config`, runs [`run_devskim.py`](../../scripts/infra/devskim/run_devskim.py), and publishes the
SARIF reports and logs as the `devskim` artifact (`devskim_failed_<attempt>` when it fails).

## What is scanned

| Tree                                       | What it is                                           | Why                             |
|--------------------------------------------|------------------------------------------------------|---------------------------------|
| `binding`                                  | The C# bindings (SkiaSharp, HarfBuzzSharp)           | Shipped in the NuGet packages   |
| `source`                                   | Views and integrations, including JavaScript         | Shipped in the NuGet packages   |
| `native`                                   | Native build glue and the WinUI native views         | Built into the native libraries |
| `externals/skia/{src,include}/{c,xamarin}` | The C API and the managed-callback shim of mono/skia | Built into libSkiaSharp         |
| `scripts`, `utils`, `eng`                  | CI, build and generator tooling (`eng` is Arcade)    | Build tooling                   |

The list is `TREES` in `run_devskim.py`. A new top-level folder with shipped or build code has to be added there.

These are not scanned:

- The rest of `externals/skia`. It is upstream Skia and its third-party dependencies, which the requirement exempts.
  Microsoft patches inside upstream files cannot be told apart from upstream code by path, so they are not covered.
- `tests`, `samples`, `benchmarks`, `documentation`, `images` and the tooling folders that start with a dot.
  They are not shipped.

## How a finding fails the build

DevSkim exits with 0 even when it reports findings, so [`check_devskim.py`](../../scripts/infra/devskim/check_devskim.py)
reads the SARIF reports. Every result tagged `API.DangerousAPI.BannedFunction` fails the stage, whatever its severity
(the general rule `DS154189` is only "Moderate"). Inline `DevSkim: ignore` comments are switched off. Other DevSkim
results stay in the reports for triage but do not fail the stage.

A clean result must not be empty. The stage also fails when:

- DevSkim analyzed no files in a tree, for example because the Skia submodule is empty. DevSkim reports the file count
  only in its debug log, so `run_devskim.py` reads it from there.
- DevSkim does not flag a banned function in a control file that `run_devskim.py` writes. This catches a scanner or a
  rule set that no longer detects anything.
- A report is missing or malformed, or DevSkim reports an execution error.

## False positives

DevSkim matches names, not calls. A C++ member function called `rewind` is reported like the C library function. The
reviewed cases are listed in [`devskim-exceptions.json`](../../scripts/infra/devskim/devskim-exceptions.json). An exception
names one rule, one file and the exact source line, and needs a reason. If the line changes, the finding comes back.

Do not add a real use of a banned function to that file. Replace it with a safer function. If that is not possible, get
the exception approved by the security team first.

## Run it locally

Install the pinned CLI outside the repository, so that `global.json` does not select the SDK, and initialize the Skia
submodule, which provides the C API sources:

```bash
repository="$PWD"
(cd /tmp && dotnet tool install Microsoft.CST.DevSkim.CLI --version 1.0.100 \
  --tool-path /tmp/devskim-tool --configfile "$repository/nuget.config")
git submodule update --init externals/skia

python3 scripts/infra/devskim/run_devskim.py --devskim /tmp/devskim-tool/devskim
python3 -m unittest discover -s scripts/infra/devskim -p "test_*.py"
```

The reports are written to `output/devskim`.

## Updating DevSkim

Change the version in `azure-templates-stages-devskim.yml` and in this document. DevSkim spells the switch that disables
inline suppressions `--disable-supression`, and it writes the rule tags on the results, not on the rules. Check both
after an update, run the gate tests and a real scan, and look at the control file result: it fails when the new version
no longer detects the banned function.
