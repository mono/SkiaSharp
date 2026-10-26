# SkiaSharp.Fuzzing

Fuzzing harness for SkiaSharp. The targets live in `FuzzTargets.cs` and the job
layout is described by `OneFuzzConfig.template.json`. The internal
`skiasharp-package` pipeline prepares the drop (`scripts/infra/security/fuzzing/fuzzing.cake`)
and submits it to OneFuzz.

## Reproducing a crash locally

The fuzzing service keeps the inputs that crashed a target. `--replay` runs one target over
saved inputs, and `scripts/infra/security/fuzzing/docker/run-local.sh` does that in a Linux
container, which is where the service runs the targets. It needs Docker (on macOS, for
example, `colima start`) and the native libraries in `./output/native`
(`dotnet cake --target=externals-download`, or your own builds).

```bash
# Download some crash inputs, then replay them through the target that found them.
./scripts/infra/security/fuzzing/onefuzz.ps1 containers download --name <crashes container> -o output/crashes
./scripts/infra/security/fuzzing/docker/run-local.sh managed-streams output/crashes
```

The targets are `managed-streams`, `picture-deserialize`, `harfbuzz-managed-callbacks` and
`skottie-resource-providers`. Each input is printed before it runs, so when a native crash
kills the process (exit code 139), the last `Replaying` line names the input that did it.
The container runs on Docker's default platform, so on Apple silicon it is arm64, and the
build writes its `bin/obj` folders next to your sources.

This replays known inputs. It does not mutate them or search for new crashes; the service
does that.

## Using the OneFuzz CLI

`scripts/infra/security/fuzzing/onefuzz.ps1` downloads the OneFuzz CLI into
`externals/onefuzz-cli` on first use, then passes all arguments to it. It works
on macOS, Linux and Windows.

Prerequisites:

- PowerShell 7+ and the .NET 10 runtime
- Azure CLI signed in with `az login`, with access to the `fuzzfest/Onefuzz`
  Azure DevOps feed

```bash
./scripts/infra/security/fuzzing/onefuzz.ps1 utils status
./scripts/infra/security/fuzzing/onefuzz.ps1 --help
```

Common commands:

| Task | Command |
|------|---------|
| Validate a config against the service | `onefuzzconfig validate --config OneFuzzConfig.ubuntu.json` |
| What happened to a pipeline run's submission | `submissions get --pipeline-url <build url>` |
| Why a submission failed or what it did | `submissions audit -s <submission id>` |
| Job and task status, with errors | `jobs get --job-id <guid>` |
| Download task logs | `logs get --help` |
| Cluster crashes into unique bugs | `crashes dedupe --help` |
| Reproduce a crash locally | `repro --help` |
| Download a drop or one file from it | `drop download -o dnceng -d <drop name> --dest <dir> -f <file>` |

Prefix each command with `./scripts/infra/security/fuzzing/onefuzz.ps1`. A job that
looks fine in the pipeline can still fail in OneFuzz, so check `jobs get` after
a submission.

Run `./scripts/infra/security/fuzzing/onefuzz.ps1 update` to update the CLI. Delete
`externals/onefuzz-cli` to reinstall. The CLI sends anonymous usage telemetry;
set `ONEFUZZ_CLI_TELEMETRY_OPTOUT=1` to disable it.
