# Azure Publish Queue and Approval

Use this procedure for publish pipeline `25298`. It separates three trust boundaries:

1. The numeric managed run ID identifies the source run to inspect.
2. The managed run's **build number string** selects the `SkiaSharp` pipeline resource.
3. A human approves the irreversible NuGet.org push in Azure DevOps.

Azure Pipelines documents `resources.pipelines.<alias>.version` as the run number, and the
Pipelines Runs API defines that value as a string. Never put the numeric managed run ID there.

## 1. Verify the managed source run

The release-status output reports the managed run ID and build number. Query that numeric ID:

```powershell
az pipelines runs show `
  --org https://dev.azure.com/devdiv `
  --project DevDiv `
  --id {managed-run-id} `
  --query "{id:id,definitionId:definition.id,buildNumber:buildNumber,status:status,result:result,sourceBranch:sourceBranch,sourceVersion:sourceVersion}" `
  --only-show-errors `
  -o json
if ($LASTEXITCODE -ne 0) { throw "Could not verify managed run {managed-run-id}." }
```

Before queueing, verify every field:

- `definitionId` is managed pipeline `10789`;
- `status/result` is exactly `completed/succeeded`;
- `sourceBranch` is exactly `refs/heads/release/{release-version}`;
- `sourceVersion` is the full 40-character commit that passed release testing;
- `buildNumber` has the expected release label:
  - stable: `X.Y.Z[.F]-stable.B+X.Y.Z[.F]`;
  - preview: `X.Y.Z[.F]-preview.N.B+X.Y.Z[.F]-preview.N`;
  - RC: `X.Y.Z[.F]-rc.N.B+X.Y.Z[.F]-rc.N`.

The numeric ID is lookup/evidence only. The next step receives only the verified build number.

## 2. Obtain confirmation and queue

Show the user the numeric lookup ID, managed build number, exact branch/commit, expected
`Push Stable` or `Push Preview` stage, and this request shape:

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

Use `ask_user` for explicit confirmation. Only after confirmation, invoke the small
cross-platform queue script:

```powershell
python .agents/skills/release-publish/scripts/queue-publish-run.py `
  "{managed-build-number}" `
  --confirm-queue
if ($LASTEXITCODE -ne 0) { throw "Publish pipeline was not queued." }
```

The script:

- accepts a managed **build number**, never a numeric run ID;
- rejects malformed, mismatched, or `+main` build numbers;
- infers `pushStable` from the validated `stable`/`preview`/`rc` label;
- refuses to queue while pipeline `25298` has an active run;
- propagates Azure CLI/API errors;
- posts the request and prints the publish run ID and URL.

Capture the returned publish run ID. Query it and verify the selected build number and parameters:

```powershell
az devops invoke `
  --org https://dev.azure.com/devdiv `
  --area pipelines `
  --resource runs `
  --route-parameters project=DevDiv pipelineId=25298 runId={publish-run-id} `
  --api-version 7.1 `
  --query "{id:id,name:name,state:state,result:result,resources:resources.pipelines.SkiaSharp,templateParameters:templateParameters}" `
  --only-show-errors `
  -o json
if ($LASTEXITCODE -ne 0) { throw "Could not verify queued publish run." }
```

Require `resources.version` to equal the confirmed managed build number. Require
`selectedResource == "SkiaSharp"`, `pushPackages == "true"`, and `pushStable` to match the
confirmed release type.

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
  stable resource and was later canceled. These runs demonstrate why build-number selection and
  the active-run guard both matter.
