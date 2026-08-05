# Azure Publish Queue and Approval

Use this procedure for publish pipeline `25298`. It separates three trust boundaries:

1. The numeric managed run ID identifies the source run to inspect.
2. The managed run's **build number string** selects the `SkiaSharp` pipeline resource.
3. A human approves the irreversible NuGet.org push in Azure DevOps.

Azure Pipelines documents `resources.pipelines.<alias>.version` as the run number, and the
Pipelines Runs API defines that value as a string. Never put the numeric managed run ID there.

## 1. Prepare and validate the queue request

Use the exact release branch commit that passed release testing:

```powershell
$request = Join-Path ([IO.Path]::GetTempPath()) "skiasharp-publish-run.json"

python .agents/skills/release-publish/scripts/prepare-publish-run.py `
  --managed-run-id {managed-run-id} `
  --release-version {release-version} `
  --release-commit {full-40-character-release-commit} `
  --output $request
if ($LASTEXITCODE -ne 0) { throw "Publish request validation failed." }
```

`--release-version` is the release **branch suffix**, without the final CI build number: use
`4.151.1` or `4.152.0-preview.1`, not `4.152.0-preview.1.1`.

The helper performs read-only Azure CLI calls and refuses to produce a request unless:

- the run belongs to managed pipeline `10789`;
- it is `completed/succeeded`;
- its branch is exactly `refs/heads/release/{release-version}`;
- its source commit exactly matches the supplied full commit SHA;
- its build number has the expected `stable`, `preview.N`, or `rc.N` label;
- publish pipeline `25298` has no active run (`notStarted`, `inProgress`, `postponed`,
  `cancelling`, or `queued`).

It writes this shape:

```json
{
  "resources": {
    "pipelines": {
      "SkiaSharp": {
        "version": "4.151.1-stable.1+4.151.1"
      }
    }
  },
  "templateParameters": {
    "selectedResource": "SkiaSharp",
    "pushPackages": true,
    "pushStable": true
  }
}
```

For previews and RCs, `pushStable` is `false`. The numeric managed run ID is deliberately absent
from the body.

## 2. Obtain confirmation and queue

Show the helper's complete summary and JSON body to the user. Use `ask_user` to obtain explicit
confirmation of all of these values:

- numeric managed run ID used for validation;
- managed build number used as the resource version;
- exact release branch and commit;
- expected `Push Stable` or `Push Preview` stage.

After confirmation, rerun the helper once to close the active-run race, then queue with the tested
Pipelines Runs REST route:

```powershell
python .agents/skills/release-publish/scripts/prepare-publish-run.py `
  --managed-run-id {managed-run-id} `
  --release-version {release-version} `
  --release-commit {full-40-character-release-commit} `
  --output $request
if ($LASTEXITCODE -ne 0) { throw "Publish request revalidation failed; do not queue." }

$queuedJson = az devops invoke `
  --org https://dev.azure.com/devdiv `
  --area pipelines `
  --resource runs `
  --route-parameters project=DevDiv pipelineId=25298 `
  --http-method POST `
  --api-version 7.1 `
  --in-file $request `
  --encoding utf-8 `
  --only-show-errors `
  -o json
if ($LASTEXITCODE -ne 0) { throw "Azure rejected the publish queue request." }

$publishRunId = ($queuedJson | ConvertFrom-Json).id
if (-not $publishRunId) { throw "Azure did not return a publish run ID." }
```

The helper deletes any previous request before revalidation. If another publish run appeared,
the stale body is no longer available to queue.

Capture the returned publish run ID. Immediately query it and verify the selected resource:

```powershell
$verifiedJson = az devops invoke `
  --org https://dev.azure.com/devdiv `
  --area pipelines `
  --resource runs `
  --route-parameters project=DevDiv pipelineId=25298 runId=$publishRunId `
  --api-version 7.1 `
  --query "{id:id,name:name,state:state,result:result,resources:resources,templateParameters:templateParameters}" `
  --only-show-errors `
  -o json
if ($LASTEXITCODE -ne 0) { throw "Could not verify queued publish run $publishRunId." }

$expected = Get-Content -Raw $request | ConvertFrom-Json
$verified = $verifiedJson | ConvertFrom-Json
$selected = $verified.resources.pipelines.SkiaSharp
if ($selected.version -ne $expected.resources.pipelines.SkiaSharp.version) {
  throw "Queued run selected the wrong managed build number."
}
if ($verified.templateParameters.selectedResource -ne "SkiaSharp" -or
    [string]$verified.templateParameters.pushPackages -ne
      [string]$expected.templateParameters.pushPackages -or
    [string]$verified.templateParameters.pushStable -ne
      [string]$expected.templateParameters.pushStable) {
  throw "Queued run template parameters do not match the validated request."
}
```

The numeric run ID was already verified before queueing. These post-queue checks use Azure's
documented response fields to prove that the expected build-number string and confirmed release
type were selected.

## 3. Respect the human approval boundary

The agent may queue the run only after user confirmation. The NuGet.org push approval is a separate
Azure DevOps check and is always **human/manual**:

1. Wait for the run name to become `SkiaSharp {managed-build-number}`.
2. Inspect the timeline and verify the stage is `Push Stable` or `Push Preview` as expected:

   ```powershell
   az devops invoke `
     --org https://dev.azure.com/devdiv `
     --area build `
     --resource timeline `
     --route-parameters project=DevDiv buildId={publish-run-id} `
     --api-version 7.1 `
     --query "records[?type=='Stage'].{name:name,state:state,result:result}" `
     -o table
   ```

3. Ask the user to open the run and approve the push.
4. Do not call an approvals API, click approval controls, or otherwise approve on the user's behalf.
5. Poll the run until `status == completed`. Continue only when `result == succeeded`:

   ```powershell
   az pipelines runs show `
     --org https://dev.azure.com/devdiv `
     --project DevDiv `
     --id {publish-run-id} `
     --query "{id:id,buildNumber:buildNumber,status:status,result:result}" `
     -o json
   ```

Do not query or poll NuGet.org before that terminal success. A failed, canceled, or rejected run
stops the publish workflow.

## Verified behavior

- Official [Run Pipeline REST API](https://learn.microsoft.com/rest/api/azure/devops/pipelines/runs/run-pipeline?view=azure-devops-rest-7.1)
  defines `resources.pipelines.<alias>.version` as a string.
- Official [pipeline resource documentation](https://learn.microsoft.com/azure/devops/pipelines/process/resources?view=azure-devops)
  defines `version` as the specified run number.
- Successful publish run `14883940` selected source run `14874440` by build number
  `4.151.1-stable.1+4.151.1`, exposed `Push Stable`, and completed `succeeded`.
- Failed run `14883922` had no selected resources, while duplicate run `14885403` selected the same
  stable resource and was later canceled. These runs demonstrate why the request body and
  duplicate-active-run check both matter.
