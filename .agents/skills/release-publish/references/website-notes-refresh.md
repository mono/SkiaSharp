# Concurrency-Safe Website Notes Refresh

The **Sync - Release Notes & API Diffs** workflow uses one repository-wide concurrency group with
`cancel-in-progress: true`. A main-branch push can therefore cancel a manual dispatch. Treat a
canceled run as a handoff to a newer run, not as an automatic reason to dispatch again.

## 1. Record the tag boundary

Push the release tag, then record a conservative UTC threshold:

```bash
set -euo pipefail
git push origin "{tag}" || exit 1
TAG_PUSHED_AT="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
```

Only workflow runs created strictly after `TAG_PUSHED_AT` are suitable. This rejects runs that
could have started before the remote accepted the tag. A qualifying run created in the same
timestamp second may be conservatively ignored and superseded by a new dispatch.

## 2. Reuse a suitable active or successful main run

Query before dispatching. Prefer a suitable active run:

```bash
RUNS_JSON="$(gh run list \
  --workflow update-release-notes.lock.yml \
  --repo mono/SkiaSharp \
  --branch main \
  --limit 30 \
  --json databaseId,status,conclusion,event,createdAt,updatedAt,url)"

RUN_ID="$(printf '%s' "$RUNS_JSON" | jq -r --arg after "$TAG_PUSHED_AT" '
  [.[] |
    select(.createdAt > $after) |
    select(.status == "queued" or .status == "in_progress" or
           .status == "requested" or .status == "waiting" or .status == "pending")
  ] | sort_by(.createdAt) | last | .databaseId // empty')"
```

If none is active, reuse the newest suitable successful run:

```bash
if [ -z "$RUN_ID" ]; then
  RUN_ID="$(printf '%s' "$RUNS_JSON" | jq -r --arg after "$TAG_PUSHED_AT" '
    [.[] |
      select(.createdAt > $after) |
      select(.status == "completed" and .conclusion == "success")
    ] | sort_by(.createdAt) | last | .databaseId // empty')"
fi
```

If `RUN_ID` is present, watch or verify it instead of creating a duplicate.

## 3. Dispatch and identify the new run

If no suitable active or successful run exists, snapshot the current IDs before dispatching:

```bash
BEFORE_IDS="$(gh run list \
  --workflow update-release-notes.lock.yml \
  --repo mono/SkiaSharp \
  --branch main \
  --limit 30 \
  --json databaseId \
  --jq '[.[].databaseId]')"
DISPATCHED_AT="$(date -u +%Y-%m-%dT%H:%M:%SZ)"

gh workflow run update-release-notes.lock.yml \
  --repo mono/SkiaSharp \
  --ref main
```

The workflow-dispatch API does not guarantee a run ID in its response. Poll for a new
`workflow_dispatch` run created at or after `DISPATCHED_AT` whose ID was absent from `BEFORE_IDS`:

```bash
RUN_ID=""
attempt=0
while [ -z "$RUN_ID" ] && [ "$attempt" -lt 30 ]; do
  RUN_ID="$(gh run list \
    --workflow update-release-notes.lock.yml \
    --repo mono/SkiaSharp \
    --branch main \
    --limit 30 \
    --json databaseId,status,event,createdAt \
    | jq -r --arg after "$DISPATCHED_AT" --argjson before "$BEFORE_IDS" '
        [.[] |
          select(.event == "workflow_dispatch") |
          select(.createdAt >= $after) |
          select((.databaseId as $id | $before | index($id)) == null)
        ] | sort_by(.createdAt) | last | .databaseId // empty')"
  attempt=$((attempt + 1))
  [ -n "$RUN_ID" ] || sleep 2
done

[ -n "$RUN_ID" ] || {
  echo "Could not identify a new release-notes workflow run." >&2
  exit 1
}
```

This identifies a post-dispatch run rather than blindly selecting the latest run, which may have
predated the tag or may belong to another event.

## 4. Follow superseding runs

Watch the selected run, then inspect its terminal state:

```bash
gh run watch "$RUN_ID" --repo mono/SkiaSharp
gh run view "$RUN_ID" --repo mono/SkiaSharp \
  --json databaseId,status,conclusion,event,createdAt,updatedAt,url
```

Accept it only if it was created after the tag and completed successfully.

If it was canceled:

1. Query runs again.
2. Consider only runs created after both `TAG_PUSHED_AT` and the canceled run's `createdAt`.
3. Prefer the newest active run and watch it.
4. If none is active, accept a newer successful run.
5. Redispatch only when no suitable active or newer successful run exists.

For other failure conclusions, stop and investigate instead of silently redispatching.

This matches observed behavior: dispatch `31051533523` was canceled when main push run
`31051763562` entered the same concurrency group; the newer run succeeded and created PR `#4687`.

## 5. Verify the rolling notes branch when changes exist

After terminal success, inspect job conclusions:

```bash
gh run view "$RUN_ID" --repo mono/SkiaSharp --json jobs \
  --jq '.jobs[] | {name,conclusion}'
```

- If `agent` is `skipped`, download the Prepare artifact and inspect its patch:

  ```bash
  VERIFY_DIR="$(mktemp -d)"
  gh run download "$RUN_ID" --repo mono/SkiaSharp \
    --name release-notes-prepare --dir "$VERIFY_DIR"
  if [ -s "$VERIFY_DIR/prepare.patch" ]; then
    echo "Prepare found changes but the agent did not run." >&2
    rm -rf -- "$VERIFY_DIR"
    exit 1
  fi
  rm -rf -- "$VERIFY_DIR"
  ```

  Accept the run as a no-op only when the artifact exists and `prepare.patch` is empty. A missing
  artifact or non-empty patch with a skipped agent means required processing did not complete;
  stop and report it.
- If `agent` is `success`, require `safe_outputs` to be `success`, then verify the rolling branch
  and open PR were created or updated after the run began:

  ```bash
  RUN_CREATED_AT="$(gh run view "$RUN_ID" --repo mono/SkiaSharp \
    --json createdAt --jq '.createdAt')"
  gh api repos/mono/SkiaSharp/git/ref/heads/bot/release-notes --jq '.object.sha'
  gh pr list --repo mono/SkiaSharp --state open --head bot/release-notes \
    --json number,title,updatedAt,headRefOid,url \
    | jq --arg after "$RUN_CREATED_AT" '.[] | select(.updatedAt >= $after)'
  ```

Do not accept a successful run with changes if `safe_outputs` failed or
`bot/release-notes` was not created or updated.
