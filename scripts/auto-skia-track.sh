#!/usr/bin/env bash
#
# auto-skia-track.sh — Keep SkiaSharp's Skia submodule up-to-date with upstream milestones.
#
# Tracks two milestones:
#   1. NEXT  = current + 1
#   2. LATEST = highest chrome/m* branch found upstream
# If they're the same, only one branch is maintained.
#
# For each target milestone:
#   - Creates or updates autobump/skia-m{N} branches in mono/skia and mono/SkiaSharp
#   - Attempts git merge of upstream/chrome/m{N}
#   - On clean merge: pushes, updates submodule pointer, runs version/binding scripts, creates PRs
#   - On conflict: aborts, reports conflicting files
#
# Usage:
#   bash scripts/auto-skia-track.sh              # Normal run
#   bash scripts/auto-skia-track.sh --dry-run     # Report what would happen without changing anything
#

set -euo pipefail

# --- Configuration ---
UPSTREAM_URL="https://github.com/google/skia.git"
UPSTREAM_REMOTE="upstream"
SKIA_TARGET_BRANCH="skiasharp"
SKIASHARP_TARGET_BRANCH="main"
BRANCH_PREFIX="autobump/skia-m"

DRY_RUN=false
if [[ "${1:-}" == "--dry-run" ]]; then
    DRY_RUN=true
    echo "=== DRY RUN MODE — no changes will be made ==="
fi

REPO_ROOT="$(git rev-parse --show-toplevel)"
SKIA_DIR="$REPO_ROOT/externals/skia"

# --- Helper functions ---
log()  { echo "  $*"; }
info() { echo ":: $*"; }
warn() { echo "⚠️  $*"; }
err()  { echo "❌ $*"; }
ok()   { echo "✅ $*"; }

# --- Step 1: Read current milestone ---
info "Reading current milestone from VERSIONS.txt..."
CURRENT=$(grep '^libSkiaSharp.*milestone' "$REPO_ROOT/scripts/VERSIONS.txt" | awk '{print $NF}')
if [[ -z "$CURRENT" ]]; then
    err "Could not read current milestone from VERSIONS.txt"
    exit 1
fi
info "Current milestone: m$CURRENT"

# --- Step 2: Init submodule and set up upstream remote ---
info "Initializing submodule..."
cd "$REPO_ROOT"
git submodule update --init externals/skia 2>/dev/null || true

cd "$SKIA_DIR"

# Ensure upstream remote exists
if ! git remote get-url "$UPSTREAM_REMOTE" &>/dev/null; then
    log "Adding upstream remote: $UPSTREAM_URL"
    git remote add "$UPSTREAM_REMOTE" "$UPSTREAM_URL"
fi

info "Fetching upstream and origin..."
git fetch "$UPSTREAM_REMOTE" --quiet 2>/dev/null
git fetch origin --quiet 2>/dev/null

# --- Step 3: Determine target milestones ---
NEXT=$((CURRENT + 1))

# Find the latest upstream chrome/m* branch
LATEST=$(git branch -r | sed -n "s|.*$UPSTREAM_REMOTE/chrome/m\([0-9]*\).*|\1|p" | sort -n | tail -1)
if [[ -z "$LATEST" ]]; then
    err "Could not find any upstream chrome/m* branches"
    exit 1
fi

info "Next milestone: m$NEXT"
info "Latest upstream milestone: m$LATEST"

# Build deduplicated target list
TARGETS=()
if [[ "$NEXT" -le "$LATEST" ]]; then
    TARGETS+=("$NEXT")
fi
if [[ "$LATEST" -ne "$NEXT" && "$LATEST" -gt "$CURRENT" ]]; then
    TARGETS+=("$LATEST")
fi

if [[ ${#TARGETS[@]} -eq 0 ]]; then
    ok "No milestones to track (next=m$NEXT, latest=m$LATEST, current=m$CURRENT)"
    exit 0
fi

info "Tracking milestones: ${TARGETS[*]}"
echo ""

# --- Step 4: Process each target milestone ---
RESULTS=()

for TARGET in "${TARGETS[@]}"; do
    echo "================================================"
    info "Processing milestone m$TARGET"
    echo "================================================"

    SKIA_BRANCH="${BRANCH_PREFIX}${TARGET}"
    UPSTREAM_REF="$UPSTREAM_REMOTE/chrome/m${TARGET}"

    # Check upstream branch exists
    if ! git rev-parse --verify "$UPSTREAM_REF" &>/dev/null; then
        warn "Upstream branch $UPSTREAM_REF does not exist. Skipping."
        RESULTS+=("m$TARGET: SKIPPED (upstream branch not available)")
        continue
    fi

    UPSTREAM_TIP=$(git rev-parse "$UPSTREAM_REF")
    log "Upstream tip: $UPSTREAM_TIP"

    # Check if our branch exists on origin
    BRANCH_EXISTS=false
    if git rev-parse --verify "origin/$SKIA_BRANCH" &>/dev/null; then
        BRANCH_EXISTS=true
    fi

    if $BRANCH_EXISTS; then
        # --- UPDATE mode: branch exists, check for new commits ---
        info "Branch $SKIA_BRANCH exists. Checking for updates..."

        git checkout "$SKIA_BRANCH" 2>/dev/null || git checkout -b "$SKIA_BRANCH" "origin/$SKIA_BRANCH"
        git reset --hard "origin/$SKIA_BRANCH"

        # Check if already up to date
        MERGE_BASE=$(git merge-base HEAD "$UPSTREAM_REF" 2>/dev/null || echo "")
        if [[ -z "$MERGE_BASE" ]]; then
            warn "Cannot compute merge-base. Attempting merge anyway..."
        elif [[ "$UPSTREAM_TIP" == "$MERGE_BASE" ]]; then
            ok "m$TARGET: Already up to date"
            RESULTS+=("m$TARGET: UP-TO-DATE")
            continue
        fi

        NEW_COMMITS=$(git log --oneline HEAD.."$UPSTREAM_REF" 2>/dev/null | wc -l | tr -d ' ')
        log "$NEW_COMMITS new upstream commit(s) to merge"
    else
        # --- CREATE mode: branch doesn't exist, create from skiasharp ---
        info "Branch $SKIA_BRANCH does not exist. Creating..."

        git checkout "origin/$SKIA_TARGET_BRANCH" --detach 2>/dev/null
        git checkout -b "$SKIA_BRANCH"
    fi

    # --- Attempt merge ---
    if $DRY_RUN; then
        log "[DRY RUN] Would merge $UPSTREAM_REF into $SKIA_BRANCH"
        RESULTS+=("m$TARGET: DRY-RUN (would merge)")
        continue
    fi

    info "Merging $UPSTREAM_REF..."
    if git merge "$UPSTREAM_REF" --no-edit -m "Merge upstream chrome/m${TARGET}"; then
        ok "Merge clean!"

        # Push skia branch
        git push origin "$SKIA_BRANCH" --force-with-lease 2>/dev/null || git push -u origin "$SKIA_BRANCH"

        # Check if mono/skia PR exists
        SKIA_PR=$(gh pr list --repo mono/skia --head "$SKIA_BRANCH" --json number --jq '.[0].number' 2>/dev/null || echo "")
        if [[ -z "$SKIA_PR" ]]; then
            info "Creating mono/skia PR..."
            DIFF_STAT=$(git diff --stat "origin/$SKIA_TARGET_BRANCH".."$SKIA_BRANCH" -- src/c/ include/c/ BUILD.gn DEPS | tail -1)
            SKIA_PR=$(gh pr create --repo mono/skia \
                --title "Update skia to milestone $TARGET" \
                --base "$SKIA_TARGET_BRANCH" \
                --head "$SKIA_BRANCH" \
                --draft \
                --body "Automated upstream merge of \`chrome/m${TARGET}\`.

**Diff stats (C API + build files):** $DIFF_STAT

Created by \`auto-skia-track.sh\`. This PR is auto-updated daily as new upstream commits land." \
                --json number --jq '.number' 2>/dev/null || echo "")
            if [[ -n "$SKIA_PR" ]]; then
                ok "Created mono/skia PR #$SKIA_PR"
            fi
        else
            log "mono/skia PR #$SKIA_PR already exists"
        fi

        # --- Update SkiaSharp parent repo ---
        info "Updating SkiaSharp parent repo..."
        cd "$REPO_ROOT"

        SKIASHARP_BRANCH="${BRANCH_PREFIX}${TARGET}"

        # Check if SkiaSharp branch exists
        git fetch origin --quiet 2>/dev/null
        if git rev-parse --verify "origin/$SKIASHARP_BRANCH" &>/dev/null; then
            git checkout "$SKIASHARP_BRANCH" 2>/dev/null || git checkout -b "$SKIASHARP_BRANCH" "origin/$SKIASHARP_BRANCH"
            git reset --hard "origin/$SKIASHARP_BRANCH"

            # Rebase on latest main to pick up any other changes
            git rebase "origin/$SKIASHARP_TARGET_BRANCH" 2>/dev/null || git rebase --abort 2>/dev/null
        else
            git checkout -b "$SKIASHARP_BRANCH" "origin/$SKIASHARP_TARGET_BRANCH"
        fi

        # Re-init submodule on this branch and point to our skia branch
        git submodule update --init externals/skia 2>/dev/null || true
        cd "$SKIA_DIR"
        git fetch origin --quiet 2>/dev/null
        git checkout "$SKIA_BRANCH"
        git reset --hard "origin/$SKIA_BRANCH"
        SKIA_SHA=$(git rev-parse HEAD)
        cd "$REPO_ROOT"

        # Stage submodule pointer
        git add externals/skia

        # Run version update script
        info "Running version update script..."
        if pwsh .agents/skills/update-skia/scripts/update-versions.ps1 -Current "$CURRENT" -Target "$TARGET" 2>&1; then
            log "Version files updated"
        else
            warn "Version update script failed — committing submodule pointer only"
        fi

        # Attempt binding regeneration
        info "Running binding regeneration..."
        if pwsh .agents/skills/update-skia/scripts/regenerate-bindings.ps1 2>&1; then
            log "Bindings regenerated"
        else
            warn "Binding regeneration failed — committing what we have"
        fi

        # Commit and push
        git add -A
        if git diff --cached --quiet; then
            log "No changes to commit in SkiaSharp"
        else
            git commit -m "Bump skia to milestone $TARGET

Automated merge of upstream chrome/m${TARGET}.
Submodule points to mono/skia branch: $SKIA_BRANCH ($SKIA_SHA)

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"
            git push origin "$SKIASHARP_BRANCH" --force-with-lease 2>/dev/null || git push -u origin "$SKIASHARP_BRANCH"
        fi

        # Check if SkiaSharp PR exists
        SS_PR=$(gh pr list --repo mono/SkiaSharp --head "$SKIASHARP_BRANCH" --json number --jq '.[0].number' 2>/dev/null || echo "")
        if [[ -z "$SS_PR" ]]; then
            info "Creating mono/SkiaSharp PR..."
            SKIA_PR_LINK=""
            if [[ -n "${SKIA_PR:-}" ]]; then
                SKIA_PR_LINK="**Companion skia PR:** https://github.com/mono/skia/pull/$SKIA_PR"
            fi
            SS_PR=$(gh pr create --repo mono/SkiaSharp \
                --title "Bump skia to milestone $TARGET" \
                --base "$SKIASHARP_TARGET_BRANCH" \
                --head "$SKIASHARP_BRANCH" \
                --draft \
                --body "Automated Skia milestone bump to m${TARGET}.

$SKIA_PR_LINK

Created by \`auto-skia-track.sh\`. This PR is auto-updated daily as new upstream commits land." \
                --json number --jq '.number' 2>/dev/null || echo "")
            if [[ -n "$SS_PR" ]]; then
                ok "Created mono/SkiaSharp PR #$SS_PR"

                # Cross-link: update skia PR body to reference SkiaSharp PR
                if [[ -n "${SKIA_PR:-}" ]]; then
                    EXISTING_BODY=$(gh pr view "$SKIA_PR" --repo mono/skia --json body --jq '.body' 2>/dev/null || echo "")
                    gh pr edit "$SKIA_PR" --repo mono/skia \
                        --body "$EXISTING_BODY

**Companion SkiaSharp PR:** https://github.com/mono/SkiaSharp/pull/$SS_PR" 2>/dev/null || true
                fi
            fi
        else
            log "mono/SkiaSharp PR #$SS_PR already exists"
        fi

        if $BRANCH_EXISTS; then
            RESULTS+=("m$TARGET: UPDATED (merged $NEW_COMMITS commit(s))")
        else
            RESULTS+=("m$TARGET: CREATED (skia PR #${SKIA_PR:-?}, SkiaSharp PR #${SS_PR:-?})")
        fi

        # Return to skia dir for next iteration
        cd "$SKIA_DIR"
    else
        # --- Merge failed (conflicts) ---
        CONFLICTED=$(git diff --name-only --diff-filter=U 2>/dev/null | head -20)
        git merge --abort 2>/dev/null || true

        err "Merge conflict for m$TARGET!"
        log "Conflicting files:"
        echo "$CONFLICTED" | while read -r f; do log "  - $f"; done

        if $BRANCH_EXISTS; then
            RESULTS+=("m$TARGET: CONFLICT (update merge failed — ${NEW_COMMITS} new commits)")

            # Post a comment on the existing skia PR about the conflict
            SKIA_PR=$(gh pr list --repo mono/skia --head "$SKIA_BRANCH" --json number --jq '.[0].number' 2>/dev/null || echo "")
            if [[ -n "$SKIA_PR" ]]; then
                CONFLICT_LIST=$(echo "$CONFLICTED" | head -10 | sed 's/^/- /')
                gh pr comment "$SKIA_PR" --repo mono/skia \
                    --body "⚠️ **Auto-merge conflict** — new upstream commits in \`chrome/m${TARGET}\` introduced merge conflicts:

$CONFLICT_LIST

Manual resolution needed. Consider using the \`update-skia\` skill." 2>/dev/null || true
            fi
        else
            RESULTS+=("m$TARGET: CONFLICT (initial merge — use update-skia skill)")
        fi
    fi

    echo ""
done

# --- Summary ---
echo ""
echo "================================================"
info "Summary (current: m$CURRENT)"
echo "================================================"
for r in "${RESULTS[@]}"; do
    echo "  $r"
done
