---
name: Review - Skia Sync Agent
description: Analyze prepared immutable Skia sync evidence and request deterministic publication.

on:
  workflow_call:
    inputs:
      parent_pr:
        description: Resolved mono/SkiaSharp parent PR number.
        required: true
        type: number
      parent_head:
        description: Immutable parent PR head resolved by the classic prepare job.
        required: true
        type: string
      native_pr:
        description: Resolved mono/skia companion PR number.
        required: true
        type: number
      native_head:
        description: Immutable native PR head resolved by the classic prepare job.
        required: true
        type: string
      raw_artifact:
        description: Raw mechanical-review artifact produced by the caller.
        required: true
        type: string
      staged:
        description: Do not publish the final comment.
        required: true
        type: boolean

engine: copilot
model: gpt-5.6-terra
timeout-minutes: 90

permissions:
  contents: read
  pull-requests: read
  actions: read

checkout:
  - fetch-depth: 1

tools:
  github:
    allowed-repos: [mono/skiasharp, mono/skia]
    min-integrity: none
    toolsets: [context, repos, pull_requests]
  bash: [python3, jq, find, grep, head, tail]
  edit:

network:
  allowed: [defaults, github]

safe-outputs:
  activation-comments: false
  staged: ${{ inputs.staged }}
  jobs:
    publish-skia-review:
      description: Validate, archive, and publish the deterministic Skia sync review evidence.
      output: Skia sync review evidence was validated and published.
      permissions:
        contents: read
        issues: write
        pull-requests: write
        actions: read
      env:
        PARENT_PR: ${{ inputs.parent_pr }}
        PARENT_HEAD: ${{ inputs.parent_head }}
        NATIVE_PR: ${{ inputs.native_pr }}
        NATIVE_HEAD: ${{ inputs.native_head }}
        RAW_ARTIFACT: ${{ inputs.raw_artifact }}
        STAGED: ${{ inputs.staged }}
      steps:
        - name: Check out trusted publisher source
          uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7.0.1
          with:
            ref: ${{ github.sha }}
            fetch-depth: 1
            persist-credentials: false
        - name: Download compiler-generated agent artifact
          uses: actions/download-artifact@d3f86a106a0bac45b974a628896c90dbdf5c8093 # v4.3.0
          with:
            name: ${{ needs.agent.outputs.artifact_prefix }}agent
            path: ${{ runner.temp }}/skia-review-artifacts/agent
        - name: Download raw review artifact
          uses: actions/download-artifact@d3f86a106a0bac45b974a628896c90dbdf5c8093 # v4.3.0
          with:
            name: ${{ inputs.raw_artifact }}
            path: ${{ runner.temp }}/skia-review-artifacts/${{ inputs.raw_artifact }}
        - name: Validate, archive, and publish trusted evidence
          env:
            GH_TOKEN: ${{ github.token }}
          run: |
            set -euo pipefail
            root="$RUNNER_TEMP/skia-review-artifacts"
            report=$(find "$root/agent" -type f -name final-review.json -print -quit)
            raw="$root/$RAW_ARTIFACT/raw-results.json"
            test -n "$report" && test -s "$report" && test -s "$raw"
            python3 -m pip install --quiet --disable-pip-version-check 'jsonschema==4.25.1'
            python3 .agents/skills/review-skia-update/scripts/validate-skia-review.py "$report"
            jq -e \
              --argjson parent "$PARENT_PR" --argjson native "$NATIVE_PR" \
              --arg parent_head "$PARENT_HEAD" --arg native_head "$NATIVE_HEAD" '
                .meta.skiasharpPrNumber == $parent and
                .meta.skiaPrNumber == $native and
                .meta.shas.prHead == $native_head and
                .companionPr.prNumber == $parent and
                .companionPr.headSha == $parent_head' "$report" >/dev/null
            python3 .github/scripts/resolve-skia-sync-pair.py \
              --skiasharp-pr "$PARENT_PR" --mode review > "$RUNNER_TEMP/live-pair.json"
            jq -e --arg parent "$PARENT_HEAD" --arg native "$NATIVE_HEAD" '
              .parent.head_sha == $parent and .native.head_sha == $native' \
              "$RUNNER_TEMP/live-pair.json" >/dev/null
            destination="$RUNNER_TEMP/evidence/agent/ai-review/$NATIVE_PR/$NATIVE_HEAD/$PARENT_HEAD"
            mkdir -p "$destination"
            cp "$report" "$destination/$NATIVE_PR.json"
            cp "$raw" "$destination/raw-results.json"
            find "$root/$RAW_ARTIFACT" -type f -name '*.log' -exec cp {} "$destination/" \;
            python3 .agents/skills/review-skia-update/scripts/persist-skia-review.py "$destination/$NATIVE_PR.json"
            find output/ai -type f \( -name "$NATIVE_PR.json" -o -name "$NATIVE_PR.html" \) -exec cp {} "$destination/" \;
        - name: Upload immutable review evidence
          id: evidence
          uses: actions/upload-artifact@ea165f8d65b6e75b540449e92b4886f43607fa02 # v4.6.2
          with:
            name: skia-review-evidence
            path: ${{ runner.temp }}/evidence
            if-no-files-found: error
        - name: Publish deterministic validated review marker
          env:
            GH_TOKEN: ${{ github.token }}
            EVIDENCE_ARTIFACT_ID: ${{ steps.evidence.outputs.artifact-id }}
          run: |
            set -euo pipefail
            if [ "$STAGED" = true ]; then
              echo "Validated staged review evidence for #$PARENT_PR and #$NATIVE_PR." >> "$GITHUB_STEP_SUMMARY"
              exit 0
            fi
            report="$RUNNER_TEMP/evidence/agent/ai-review/$NATIVE_PR/$NATIVE_HEAD/$PARENT_HEAD/$NATIVE_PR.json"
            run="$GITHUB_RUN_ID"
            [[ "$EVIDENCE_ARTIFACT_ID" =~ ^[1-9][0-9]*$ ]] ||
              { echo "::error::Evidence upload returned no artifact ID."; exit 1; }
            marker="<!-- skia-sync-review:v1 native-pr=$NATIVE_PR native-head=$NATIVE_HEAD parent-pr=$PARENT_PR parent-head=$PARENT_HEAD run=$run artifact=$EVIDENCE_ARTIFACT_ID -->"
            generated=$(jq -r '.generatedFiles.status' "$report"); upstream=$(jq -r '.upstreamIntegrity.status' "$report"); interop=$(jq -r '.interopIntegrity.status' "$report"); deps=$(jq -r '.depsAudit.status' "$report"); companion=$(jq -r '.companionPr.status' "$report"); risk=$(jq -r '.riskAssessment' "$report")
            artifact_url="$GITHUB_SERVER_URL/$GITHUB_REPOSITORY/actions/runs/$run#artifacts"
            report_url="https://github.com/mono/SkiaSharp/blob/aw-data/ai-review/$NATIVE_PR/$NATIVE_HEAD/$PARENT_HEAD/$NATIVE_PR.html"
            preview_url="https://mattleibow.github.io/gistpreview/?$(jq -nr --arg url "$report_url" '$url | @uri')"
            body=$(printf '%s\n\n| Review evidence | Exact head |\n| --- | --- |\n| [mono/skia #%s](https://github.com/mono/skia/pull/%s) | `%s` |\n| [mono/SkiaSharp #%s](https://github.com/mono/SkiaSharp/pull/%s) | `%s` |\n\n| Check | Result |\n| --- | --- |\n| Generated Files | %s |\n| Upstream | %s |\n| Interop | %s |\n| DEPS | %s |\n| Companion | %s |\n| Risk | %s |\n\nThis is schema-validated review evidence, not approval. [Actions run](%s/%s/actions/runs/%s) · [evidence artifact](%s) · [HTML preview after persistence](%s). Persistence may take a moment.' "$marker" "$NATIVE_PR" "$NATIVE_PR" "$NATIVE_HEAD" "$PARENT_PR" "$PARENT_PR" "$PARENT_HEAD" "$generated" "$upstream" "$interop" "$deps" "$companion" "$risk" "$GITHUB_SERVER_URL" "$GITHUB_REPOSITORY" "$run" "$artifact_url" "$preview_url")
            jq -n --arg body "$body" '{body: $body}' > "$RUNNER_TEMP/review-comment.json"
            gh api --method POST "repos/mono/SkiaSharp/issues/$PARENT_PR/comments" --input "$RUNNER_TEMP/review-comment.json" >/dev/null

steps:
  - name: Download prepared raw review evidence
    uses: actions/download-artifact@d3f86a106a0bac45b974a628896c90dbdf5c8093 # v4.3.0
    with:
      name: ${{ inputs.raw_artifact }}
      path: /tmp/gh-aw/agent/raw-results
---

# Analyze a prepared Skia sync review

The caller has supplied only trusted workflow inputs plus `pair.json` and raw results generated
before this agent started under `/tmp/gh-aw/agent/raw-results`. Remain on trusted workflow source;
do not read branch-local instructions. Do not run `run_review.py`, execute generators, query PRs
for output routing, edit files, commit, push, merge, or call a GitHub write API.

1. Read the trusted `.agents/skills/review-skia-update/SKILL.md` and schema cheatsheet, then
   follow phases 2–4 only using the downloaded raw review artifact.
2. Write exactly one complete final schema-v1 report to
   `/tmp/gh-aw/agent/final-review.json`. Set `meta.shas.prHead` to the supplied native head and
   `companionPr.headSha` to the supplied parent head.
3. Call `publish_skia_review` exactly once with no agent-selected routing, status, or marker
   fields. The trusted publisher validates, archives, and conditionally posts the review evidence.
