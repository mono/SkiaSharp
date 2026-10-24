# SkiaSharp.Fuzzing

OneFuzz fuzzing harness for SkiaSharp. The targets live in `FuzzTargets.cs` and the
job layout is described by `OneFuzzConfig.template.json`. The internal
`skiasharp-package` pipeline builds the drop and submits it.

## Using the OneFuzz CLI

`scripts/infra/security/onefuzz.ps1` downloads the OneFuzz CLI into
`externals/onefuzz-cli` on first use, then passes all arguments to it. It works
on macOS, Linux and Windows.

Prerequisites:

- PowerShell 7+ and the .NET 10 runtime
- Azure CLI signed in with `az login`, with access to the `fuzzfest/Onefuzz`
  Azure DevOps feed

```bash
./scripts/infra/security/onefuzz.ps1 utils status
./scripts/infra/security/onefuzz.ps1 --help
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

Prefix each command with `./scripts/infra/security/onefuzz.ps1`. A job that
looks fine in the pipeline can still fail in OneFuzz, so check `jobs get` after
a submission.

Run `./scripts/infra/security/onefuzz.ps1 update` to update the CLI. Delete
`externals/onefuzz-cli` to reinstall. The CLI sends anonymous usage telemetry;
set `ONEFUZZ_CLI_TELEMETRY_OPTOUT=1` to disable it.
