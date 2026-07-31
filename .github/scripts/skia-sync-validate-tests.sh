#!/usr/bin/env bash
#
# Rerun the exact final test gates without a write token before publication.

set -euo pipefail

export PATH=/usr/bin:/bin
export GIT_NO_REPLACE_OBJECTS=1
unset BASH_ENV ENV CDPATH GIT_CONFIG_COUNT GIT_EXEC_PATH PYTHONHOME PYTHONPATH
unset DOTNET_STARTUP_HOOKS LD_PRELOAD

ARTIFACT_DIR="${SKIA_SYNC_ARTIFACT_DIR:-/tmp/gh-aw/agent}"
TRUSTED_DIR="${SKIA_SYNC_TRUSTED_DIR:?SKIA_SYNC_TRUSTED_DIR is required}"
VALIDATION_DIR="${SKIA_SYNC_VALIDATION_DIR:?SKIA_SYNC_VALIDATION_DIR is required}"
EXPECTED_ENV="${SKIA_SYNC_EXPECTED_ENV:-$TRUSTED_DIR/skia-sync-expected.env}"
WORKSPACE="${SKIA_SYNC_WORKSPACE:-${GITHUB_WORKSPACE:?GITHUB_WORKSPACE is required}}"
INPUT_ATTESTATION="${SKIA_SYNC_INPUT_ATTESTATION:-}"
INPUT_PARENT_BUNDLE="${SKIA_SYNC_INPUT_PARENT_BUNDLE:-}"
INPUT_SKIA_BUNDLE="${SKIA_SYNC_INPUT_SKIA_BUNDLE:-}"
ENV_FILE="$ARTIFACT_DIR/skia-sync-env.sh"

# shellcheck source=/dev/null
source "$TRUSTED_DIR/skia-sync-common.sh"

if ! cmp -s "$ENV_FILE" "$EXPECTED_ENV"; then
  sync_error "Agent handoff values do not exactly match trusted pre-agent state."
  exit 1
fi

TRUSTED_HASHES=$(
  find "$TRUSTED_DIR" -type f -print0 \
    | sort -z \
    | xargs -0 sha256sum
)
GITHUB_ENV_HASH=$(sha256sum "$GITHUB_ENV")
GITHUB_PATH_HASH=$(sha256sum "$GITHUB_PATH")
EXPECTED_ENV_HASH=$(sha256sum "$EXPECTED_ENV")
SYSTEM_HASHES=$(
  sha256sum \
    /usr/bin/bash \
    /usr/bin/dotnet \
    /usr/bin/git \
    /usr/bin/gh \
    /usr/bin/jq \
    /usr/bin/python3
)

if [[ -n "${GH_TOKEN:-}" || -n "${SKIASHARP_AUTOBUMP_TOKEN:-}" ]]; then
  sync_error "Test validation must run without a GitHub write token."
  exit 1
fi

required_file() {
  local path="$1"
  if [[ ! -s "$path" ]]; then
    sync_error "Required sync artifact is missing or empty: $path"
    exit 1
  fi
}

required_file "$ENV_FILE"
required_file "$ARTIFACT_DIR/initial-test-output.txt"
required_file "$ARTIFACT_DIR/test-output.txt"
required_file "$ARTIFACT_DIR/skia-sync-skia-summary.md"
required_file "$ARTIFACT_DIR/skia-sync-skiasharp-summary.md"
required_file "$ARTIFACT_DIR/skia-breaking-change-analysis.md"
required_file "$ARTIFACT_DIR/skia-validation-review.md"
required_file "$ARTIFACT_DIR/skia-dependency-decisions.md"

load_sync_env "$ENV_FILE"
validate_sync_checkout "$WORKSPACE"

INPUT_PARENT_HEAD=""
INPUT_SKIA_HEAD=""
INPUT_GITLINK=""
INPUT_PARENT_BUNDLE_SHA=""
INPUT_SKIA_BUNDLE_SHA=""
if [[ -n "$INPUT_ATTESTATION" || -n "$INPUT_PARENT_BUNDLE" || -n "$INPUT_SKIA_BUNDLE" ]]; then
  required_file "$INPUT_ATTESTATION"
  required_file "$INPUT_PARENT_BUNDLE"
  required_file "$INPUT_SKIA_BUNDLE"
  INPUT_PARENT_HEAD=$(jq -er '.parentHead' "$INPUT_ATTESTATION")
  INPUT_SKIA_HEAD=$(jq -er '.skiaHead' "$INPUT_ATTESTATION")
  INPUT_GITLINK=$(jq -er '.gitlink' "$INPUT_ATTESTATION")
  INPUT_PARENT_BUNDLE_SHA=$(jq -er '.parentBundleSha256' "$INPUT_ATTESTATION")
  INPUT_SKIA_BUNDLE_SHA=$(jq -er '.skiaBundleSha256' "$INPUT_ATTESTATION")
  if [[ "$(jq -er '.headBranch' "$INPUT_ATTESTATION")" != "$HEAD_BRANCH" ||
        "$(git -C "$WORKSPACE" rev-parse HEAD)" != "$INPUT_PARENT_HEAD" ||
        "$(git -C "$WORKSPACE/externals/skia" rev-parse HEAD)" != "$INPUT_SKIA_HEAD" ||
        "$(git -C "$WORKSPACE" ls-tree "$INPUT_PARENT_HEAD" -- externals/skia | awk '{print $3}')" != "$INPUT_GITLINK" ||
        "$INPUT_GITLINK" != "$INPUT_SKIA_HEAD" ||
        "$(sha256sum "$INPUT_PARENT_BUNDLE" | awk '{print $1}')" != "$INPUT_PARENT_BUNDLE_SHA" ||
        "$(sha256sum "$INPUT_SKIA_BUNDLE" | awk '{print $1}')" != "$INPUT_SKIA_BUNDLE_SHA" ]]; then
    sync_error "Fresh verification input does not match its original attestation."
    exit 1
  fi
fi

export DOTNET_CLI_UI_LANGUAGE=en-US

run_full_solution() {
  local output="$ARTIFACT_DIR/test-output.txt"

  cp "$output" "$ARTIFACT_DIR/agent-test-output.txt"
  echo "Rerunning the required unfiltered full solution from the trusted token-free step."
  rm -f "$output"
  (
    cd "$WORKSPACE"
    printf '%s\n' \
      "SKIA_SYNC_TEST_EVIDENCE full stage=final solution=tests/SkiaSharp.Tests.Console.slnx tfm=net10.0 unfiltered=true"
    dotnet test tests/SkiaSharp.Tests.Console.slnx \
      -p:TargetFramework=net10.0 \
      -p:TargetFrameworks=net10.0
  ) 2>&1 | tee "$output"
}

run_vulkan_evidence() {
  local backend="$1"
  local filter="$2"
  local marker="$3"
  local output="$4"
  local project="tests/SkiaSharp.Vulkan.Tests.Console/SkiaSharp.Vulkan.Tests.Console.csproj"

  echo "Running required ${backend} Vulkan evidence test: ${filter}"
  rm -f "$output"
  (
    cd "$WORKSPACE"
    printf '%s\n' "$marker"
    dotnet test "$project" \
      -p:TargetFramework=net10.0 \
      -p:TargetFrameworks=net10.0 \
      -- --filter-method "$filter"
  ) 2>&1 | tee "$output"
}

run_full_solution
run_vulkan_evidence \
  "Ganesh" \
  "*CreateVkContextIsValid*" \
  "SKIA_SYNC_TEST_EVIDENCE vulkan backend=ganesh filter=*CreateVkContextIsValid*" \
  "$ARTIFACT_DIR/vulkan-ganesh-evidence.txt"
run_vulkan_evidence \
  "Graphite" \
  "*GraphiteVkContextIsCreatedFromRawHandles*" \
  "SKIA_SYNC_TEST_EVIDENCE vulkan backend=graphite filter=*GraphiteVkContextIsCreatedFromRawHandles*" \
  "$ARTIFACT_DIR/vulkan-graphite-evidence.txt"

python3 "$TRUSTED_DIR/validate-test-output.py" \
  --initial "$ARTIFACT_DIR/initial-test-output.txt" \
  --final "$ARTIFACT_DIR/test-output.txt" \
  --ganesh "$ARTIFACT_DIR/vulkan-ganesh-evidence.txt" \
  --graphite "$ARTIFACT_DIR/vulkan-graphite-evidence.txt"

python3 "$TRUSTED_DIR/audit-fork-patches.py" \
  --skia-root "$WORKSPACE/externals/skia" \
  --old-upstream "$BASE_UPSTREAM_SHA" \
  --new-upstream "$TARGET_UPSTREAM_SHA" \
  --fork-base "$SKIA_BASE_SHA" \
  --merged-head "$HEAD_BRANCH" \
  --output "$ARTIFACT_DIR/skia-fork-patch-audit.md" \
  --validate

validate_sync_checkout "$WORKSPACE"
if [[ -n "$INPUT_ATTESTATION" ]] &&
   [[ "$(git -C "$WORKSPACE" rev-parse HEAD)" != "$INPUT_PARENT_HEAD" ||
      "$(git -C "$WORKSPACE/externals/skia" rev-parse HEAD)" != "$INPUT_SKIA_HEAD" ||
      "$(git -C "$WORKSPACE" ls-tree "$INPUT_PARENT_HEAD" -- externals/skia | awk '{print $3}')" != "$INPUT_GITLINK" ||
      "$(sha256sum "$INPUT_PARENT_BUNDLE" | awk '{print $1}')" != "$INPUT_PARENT_BUNDLE_SHA" ||
      "$(sha256sum "$INPUT_SKIA_BUNDLE" | awk '{print $1}')" != "$INPUT_SKIA_BUNDLE_SHA" ]]; then
  sync_error "Candidate code changed the originally attested commits or bundles."
  exit 1
fi

CURRENT_TRUSTED_HASHES=$(
  find "$TRUSTED_DIR" -type f -print0 \
    | sort -z \
    | xargs -0 sha256sum
)
if [[ "$CURRENT_TRUSTED_HASHES" != "$TRUSTED_HASHES" ]]; then
  sync_error "A test process modified the trusted post-step assets."
  exit 1
fi
if [[ "$(sha256sum /usr/bin/bash /usr/bin/dotnet /usr/bin/git /usr/bin/gh /usr/bin/jq /usr/bin/python3)" != "$SYSTEM_HASHES" ]]; then
  sync_error "A test process modified a system executable used for publication."
  exit 1
fi
if [[ "$(sha256sum "$GITHUB_ENV")" != "$GITHUB_ENV_HASH" || "$(sha256sum "$GITHUB_PATH")" != "$GITHUB_PATH_HASH" ]]; then
  sync_error "A test process modified the runner command environment."
  exit 1
fi
if [[ "$(sha256sum "$EXPECTED_ENV")" != "$EXPECTED_ENV_HASH" ||
      ! -f "$ENV_FILE" ]] ||
   ! cmp -s "$ENV_FILE" "$EXPECTED_ENV"; then
  sync_error "A test process modified the trusted or handoff sync state."
  exit 1
fi

sanitize_git_config() {
  local repo="$1"
  local key

  while IFS= read -r key; do
    [[ -n "$key" ]] && git -C "$repo" config --local --unset-all "$key" || true
  done < <(
    git -C "$repo" config --local --name-only --get-regexp \
      '^(url\..*\.(insteadof|pushinsteadof)|include(if)?\..*|credential\..*|http\..*|core\.sshcommand|core\.hookspath|core\.fsmonitor)$' \
      || true
  )
  git -C "$repo" config --local core.hooksPath /dev/null
  git -C "$repo" config --local core.fsmonitor false
}

sanitize_git_config "$WORKSPACE"
sanitize_git_config "$WORKSPACE/externals/skia"

PARENT_HEAD=$(git -C "$WORKSPACE" rev-parse "refs/heads/${HEAD_BRANCH}")
SKIA_HEAD=$(git -C "$WORKSPACE/externals/skia" rev-parse "refs/heads/${HEAD_BRANCH}")
GITLINK=$(git -C "$WORKSPACE" ls-tree "$PARENT_HEAD" -- externals/skia | awk '{print $3}')

rm -rf "$VALIDATION_DIR"
mkdir -p "$VALIDATION_DIR"
if [[ -n "$INPUT_ATTESTATION" ]]; then
  cp "$INPUT_PARENT_BUNDLE" "$VALIDATION_DIR/skiasharp.bundle"
  cp "$INPUT_SKIA_BUNDLE" "$VALIDATION_DIR/skia.bundle"
else
  git -C "$WORKSPACE" bundle create "$VALIDATION_DIR/skiasharp.bundle" "refs/heads/${HEAD_BRANCH}"
  git -C "$WORKSPACE/externals/skia" bundle create "$VALIDATION_DIR/skia.bundle" "refs/heads/${HEAD_BRANCH}"
fi
git -C "$WORKSPACE" bundle verify "$VALIDATION_DIR/skiasharp.bundle"
git -C "$WORKSPACE/externals/skia" bundle verify "$VALIDATION_DIR/skia.bundle"

cp "$EXPECTED_ENV" "$VALIDATION_DIR/skia-sync-expected.env"
cp "$ENV_FILE" "$VALIDATION_DIR/skia-sync-env.sh"
cp "$ARTIFACT_DIR/skia-sync-skia-summary.md" "$VALIDATION_DIR/skia-sync-skia-summary.md"
cp "$ARTIFACT_DIR/skia-sync-skiasharp-summary.md" "$VALIDATION_DIR/skia-sync-skiasharp-summary.md"
cp "$ARTIFACT_DIR/skia-breaking-change-analysis.md" "$VALIDATION_DIR/skia-breaking-change-analysis.md"
cp "$ARTIFACT_DIR/skia-validation-review.md" "$VALIDATION_DIR/skia-validation-review.md"
cp "$ARTIFACT_DIR/skia-dependency-decisions.md" "$VALIDATION_DIR/skia-dependency-decisions.md"
cp "$ARTIFACT_DIR/initial-test-output.txt" "$VALIDATION_DIR/initial-test-output.txt"
cp "$ARTIFACT_DIR/test-output.txt" "$VALIDATION_DIR/test-output.txt"
cp "$ARTIFACT_DIR/vulkan-ganesh-evidence.txt" "$VALIDATION_DIR/vulkan-ganesh-evidence.txt"
cp "$ARTIFACT_DIR/vulkan-graphite-evidence.txt" "$VALIDATION_DIR/vulkan-graphite-evidence.txt"
cp "$ARTIFACT_DIR/skia-fork-patch-audit.md" "$VALIDATION_DIR/skia-fork-patch-audit.md"

PARENT_BUNDLE_SHA=$(sha256sum "$VALIDATION_DIR/skiasharp.bundle" | awk '{print $1}')
SKIA_BUNDLE_SHA=$(sha256sum "$VALIDATION_DIR/skia.bundle" | awk '{print $1}')
jq -n \
  --arg headBranch "$HEAD_BRANCH" \
  --arg parentHead "$PARENT_HEAD" \
  --arg skiaHead "$SKIA_HEAD" \
  --arg gitlink "$GITLINK" \
  --arg parentBundleSha256 "$PARENT_BUNDLE_SHA" \
  --arg skiaBundleSha256 "$SKIA_BUNDLE_SHA" \
  --arg validationRunId "${GITHUB_RUN_ID:-local}" \
  '{
    headBranch: $headBranch,
    parentHead: $parentHead,
    skiaHead: $skiaHead,
    gitlink: $gitlink,
    parentBundleSha256: $parentBundleSha256,
    skiaBundleSha256: $skiaBundleSha256,
    validationRunId: $validationRunId
  }' >"$VALIDATION_DIR/attestation.json"
chmod -R a-w "$VALIDATION_DIR"
