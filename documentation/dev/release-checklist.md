# SkiaSharp Release Checklist

## Purpose and scope

This document defines the SkiaSharp release process independently of the host
used to execute it. A knowledgeable operator should be able to follow the same
checks and produce the same evidence from a local machine, GitHub Actions,
Azure Pipelines, or a future release service.

- This is the normative process for:
  - preview releases;
  - release candidates (RCs);
  - stable releases;
  - four-part hotfix previews, RCs, and stable releases;
  - recovery when some release state already exists.
- The process has two repository-side phases separated by an external package
  build, test, promotion, and publication boundary:
  - **Prepare** creates and verifies release source state.
  - **Finish** begins only after the exact public package version exists on
    NuGet.org.
- The systems below are authoritative for their own state:
  - **Git repositories** are authoritative for commits, branches, gitlinks,
    and tags.
  - **dnceng/Azure Pipelines** is authoritative for build and connected-test
    runs.
  - **Arcade/Maestro/Darc** is authoritative for BAR identity, validation, and
    configured feed promotion.
  - **The team publication pipeline** is authoritative for the protected
    NuGet.org push decision.
  - **NuGet.org** is authoritative for the public package receipt and the
    source metadata embedded in published packages.
  - **GitHub Releases** are authoritative for draft/published release state and
    the reviewed release body.
  - **GitHub milestones, issues, and pull requests** are authoritative for
    release assignment and closeout state.
- A report, plan, local file, workflow artifact, job output, or chat history is
  evidence only. It must never override contradictory authoritative system
  state.

## Roles and ownership boundaries

- **Release operator**
  - Collects and freezes exact inputs.
  - Runs read-only checks before every mutation.
  - Presents irreversible actions and their exact targets for review.
  - Performs only the approved mutation.
  - Rereads authoritative state after every mutation.
  - Stops on ambiguity or conflict instead of selecting a convenient "latest"
    value.
- **Reviewer/approver**
  - Is distinct from the mutation itself.
  - Reviews exact branch/tag/package/body targets, not a general intent to
    "release."
  - Approves only the operation and immutable values shown.
  - Does not approve their own protected action when the enforcement system can
    prevent self-review.
- **SkiaSharp source repository owners**
  - Own `mono/SkiaSharp` branch protection, release refs, tags, releases,
    milestones, and release-note dispatch.
  - Own the matching `mono/skia` release ref at the pinned Skia gitlink.
- **dnceng/Arcade/Maestro owners**
  - Own Build, connected Tests, signing, API Scan, BAR registration,
    validation, and configured channel promotion.
  - Return immutable Build, Tests, and BAR identities.
- **Team publication pipeline owners**
  - Own the protected human decision to publish one exact BAR/package set to
    NuGet.org.
  - The repository-side operator may queue or monitor this pipeline but must
    not bypass or impersonate its approval.
- **NuGet.org**
  - Is the public package source of truth.
  - Package bytes and versions are immutable once published.
- **Smoke-test reviewer**
  - Approves an exact host/device matrix.
  - Reviews screenshots and test evidence.
  - Provides advisory signoff unless a separate protected policy explicitly
    makes smoke testing mandatory.

## Terminology and release identities

- **Release line:** `X.Y`, represented by maintenance branch
  `release/X.Y.x`.
- **Numeric version:**
  - normal release: `X.Y.Z`;
  - hotfix release: `X.Y.Z.F`.
- **Release identity:** the numeric version plus an optional channel and
  iteration:
  - preview: `X.Y.Z[-or-.F]-preview.N`;
  - RC: `X.Y.Z[-or-.F]-rc.N`;
  - stable: `X.Y.Z[-or-.F]`.
- **Public package version:**
  - stable: the release identity;
  - preview/RC: `{release identity}.{build revision}`.
- **Build revision:** the Arcade official build suffix, currently derived from
  `OfficialBuildId` as a short date and revision, such as `26418.3`.
- **Integration branch:** `main` for a new line before it is forked, otherwise
  `release/X.Y.x`.
- **Maintenance branch:** `release/X.Y.x`; it remains the integration point for
  that line.
- **Exact release branch:** `release/{release identity}`. It never includes the
  Arcade build revision.
- **Matching Skia branch:** the identically named `release/{release identity}`
  branch in `mono/skia`, at the exact `externals/skia` gitlink commit.
- **Public release tag:**
  - stable: `v{release identity}`;
  - preview/RC: historically `v{exact public package version}`, including the
    build revision.
- **BAR:** the immutable Arcade build record containing Shipping product
  packages, NonShipping transport packages, and symbol assets.
- **Public package family:** every package ID and exact version that
  `scripts/VERSIONS.txt` at the shipped source commit says belongs to the
  SkiaSharp or HarfBuzzSharp family.
- **Managed release body regions:**
  - a script-owned empty or reviewed SkiaSharp summary region;
  - a GitHub-generated-notes region;
  - bytes outside those regions remain human-owned.

### Release type and base policy

- **First preview on a new line**
  - Base the maintenance branch on an explicitly audited `main` commit whose
    version state is the target numeric version with `PREVIEW_LABEL:
    preview.0`.
  - Base the exact preview branch on that same commit.
- **Later preview or RC**
  - Use the existing `release/X.Y.x` line and an explicitly selected exact base
    commit.
  - The base may be the latest validated prior prerelease branch when that is
    the intended continuation point.
- **Stable, non-hotfix**
  - Cut from `release/X.Y.x`, not from an arbitrary prerelease branch.
  - Open, but never merge automatically, the post-cut next-version bump PR.
- **Hotfix preview/RC**
  - Base on the immutable parent stable tag `vX.Y.Z`.
  - Do not create or advance `release/X.Y.x` as part of the hotfix.
- **Hotfix stable**
  - Base on an explicitly selected prerelease branch for the same four-part
    version.
  - Do not create the normal stable bump PR.

## Required inputs

### Inputs required before Prepare

- [ ] Repository identity: `mono/SkiaSharp`.
- [ ] Tooling revision:
  - an exact trusted commit;
  - known to be on the approved default-branch history;
  - capable of reading the current repository format.
- [ ] Exact release identity.
  - It may be supplied by the operator.
  - For a next-preview request, it may be derived read-only by examining the
    integration branch version state and all existing exact release branches.
  - Once derived and reviewed, freeze it as an exact input.
- [ ] Release type: preview, RC, stable, hotfix preview/RC, or hotfix stable.
- [ ] Integration branch.
- [ ] Fully qualified release base ref.
- [ ] Exact SHA currently resolved by the release base ref.
- [ ] Maintenance mode:
  - existing;
  - create;
  - not applicable for a hotfix.
- [ ] If creating a maintenance branch:
  - fully qualified creation ref;
  - exact creation SHA;
  - proof that the version state is the target numeric version and
    `preview.0`.
- [ ] Expected exact SkiaSharp release branch name.
- [ ] Expected matching `mono/skia` branch name.
- [ ] Exact Skia gitlink SHA read from the release base.
- [ ] Current SkiaSharp and HarfBuzzSharp versions from the base.
- [ ] For stable, non-hotfix releases:
  - expected next SkiaSharp patch version;
  - expected next HarfBuzzSharp revision;
  - expected bump branch and PR base.

### Inputs required at the external build/publication boundary

- [ ] Exact SkiaSharp release branch.
- [ ] Exact source commit to build.
- [ ] Exact dnceng Build run ID and build number.
- [ ] Exact connected Tests run ID.
- [ ] Exact BAR build ID registered by that Build run.
- [ ] Exact SkiaSharp and HarfBuzzSharp package versions.
- [ ] Evidence that Build, signing, API Scan, BAR validation, promotion, and
  connected Tests have reached their required state.
- [ ] Exact team publication pipeline run ID and approval URL.
- [ ] Explicit testing override, if any, including approver and rationale.

### Inputs required before Finish

- [ ] Exact public SkiaSharp package version on NuGet.org.
- [ ] Expected release identity derived from that public version.
- [ ] Expected exact release branch.
- [ ] Expected public tag under the tag policy above.
- [ ] Exact previous release tag, or an explicit statement that none exists.
  - Derive it by parsing release tags with NuGet release ordering.
  - Select the greatest valid SkiaSharp release tag below the current exact
    public version.
  - Include normal, prerelease, and four-part hotfix tags.
  - Freeze and review the result before generating notes or reconciling commit
    ranges.
- [ ] Public package anchor policy.
- [ ] Trusted signing-certificate policy, including role and validity dates.
- [ ] Expected GitHub Release title and prerelease flag.
- [ ] Reviewed publication body hash immediately before publication.

## Credentials, permissions, tooling, and network preconditions

- [ ] Git can fetch all refs and tags from `mono/SkiaSharp`.
- [ ] The operator can read the pinned commit and submodule gitlink.
- [ ] Read access is available for:
  - `mono/SkiaSharp`;
  - `mono/skia`;
  - GitHub Releases, including drafts;
  - GitHub milestones, PRs, issues, and closing-issue relationships;
  - dnceng Build and Tests;
  - Arcade/Maestro/Darc build records;
  - NuGet.org registration, catalog, and package content;
  - Chromium milestone schedules.
- [ ] Write access is scoped to the approved operation:
  - branch/ref creation in `mono/SkiaSharp` and `mono/skia`;
  - immutable tag creation;
  - GitHub Release draft/update/publication;
  - milestone and issue/PR assignment;
  - workflow dispatch.
- [ ] Git credentials are noninteractive and short-lived in the executing
  process. Do not persist them in the checkout.
- [ ] Publication credentials remain inside the protected team publication
  pipeline.
- [ ] The exact .NET SDK and locked tool/package dependencies are available
  when repository tooling is used.
- [ ] NuGet verification uses official NuGet APIs and package-signature
  verification.
- [ ] GitHub operations use GitHub APIs rather than scraping UI output.
- [ ] Network egress is limited to required Git, GitHub, dnceng/Azure, Maestro,
  NuGet.org, and Chromium schedule endpoints.
- [ ] The worktree is clean before any branch/version-file mutation, except for
  explicitly named audit outputs outside tracked source.

## Immutable safety rules

- **Never force-update a release branch or matching `mono/skia` branch.**
- **Never move or delete a release tag.**
- **Never overwrite a conflicting GitHub Release.**
- **Never republish or replace an existing NuGet.org package version.**
- **Never combine Build, Tests, BAR, package, branch, or commit evidence from
  different release attempts.**
- **Never select a mutable "latest" run, branch tip, package, BAR, or feed
  location after exact inputs have been approved.**
- **Never tag a branch tip merely because it is newer. Tag the source commit
  verified from the public package receipt.**
- **Never auto-merge the stable bump PR.**
- **Never infer success from a write call. Reread the resulting remote state.**
- **Never treat a missing protected approval boundary as implicit approval.**
- **Never use public CI definition 345 as a substitute for the protected
  release Build, connected Tests, signing, API Scan, BAR, and publication
  evidence.**
- **Never modify generated release notes outside their owned source inputs and
  managed markers.**
- **Stop before mutation when any immutable state conflicts.**

### Universal stop/block conditions

- A reviewed ref moved to another SHA.
- The tooling revision is no longer trusted or reachable.
- A release branch exists but is not descended from the approved base.
- Existing release-branch version files do not match the release identity.
- The `mono/skia` release ref exists at another SHA.
- The maintenance creation point is not the expected numeric version with
  `preview.0`.
- A stable bump branch exists with stale versions or unrelated ancestry.
- Build, Tests, BAR, or package identities cannot be connected to one source
  commit.
- A required public package is missing, unlisted, hash-invalid, unsigned,
  untrusted, or has conflicting source metadata.
- The source branch in the public package is not the expected exact release
  branch.
- The source commit does not exist or is not contained in that branch.
- An existing tag points anywhere other than the verified package source
  commit.
- Existing GitHub Release metadata conflicts with tag, title, target, or
  prerelease state.
- Managed body markers are duplicate, incomplete, or out of order.
- The draft body changed after publication approval.
- A previous tag cannot be resolved or is not an ancestor of the shipped source
  commit.
- A shipped milestone has open items but no unshipped target milestone.
- A write cannot be reread as the exact desired state.

## Ordered process

Every mutating step follows this sequence:

- [ ] Read current authoritative state.
- [ ] Compare it with the frozen desired state.
- [ ] Classify it as:
  - **Done:** exact desired state already exists;
  - **Ready:** mutation is safe and approval requirements are satisfied;
  - **Waiting:** an external or human-owned prerequisite is incomplete;
  - **Blocked:** state conflicts or is ambiguous;
  - **Skipped:** the step does not apply to this release type.
- [ ] Stop on Waiting or Blocked unless the Waiting item is explicitly a later
  human-owned step and earlier independent approved actions are intended.
- [ ] Obtain the required human decision for irreversible or protected changes.
- [ ] Reread the same authoritative state immediately before mutation.
- [ ] Apply only the approved mutation.
- [ ] Reread and require the exact desired end state.
- [ ] Record evidence.

## Phase A: Prepare source state

### A1. Validate inputs, tooling, and provenance

- **Preconditions**
  - Exact inputs listed under "Inputs required before Prepare" are available.
- **Checks**
  - [ ] Resolve the tooling revision exactly.
  - [ ] Confirm it belongs to the approved default-branch history.
  - [ ] Confirm the requested release identity is canonical.
  - [ ] Confirm release type, branch naming, and base policy agree.
  - [ ] Fetch refs and tags without changing the checkout.
- **Actions**
  - None.
- **Desired end state/evidence**
  - A frozen input record containing exact refs, SHAs, versions, and release
    type.
- **Approval/human decision**
  - The operator and reviewer agree on the exact release identity and base.
- **Rerun behavior**
  - Recompute from remote truth. If any frozen ref moved, stop and repeat the
    review with a new record.

### A2. Pin and validate the release base

- **Preconditions**
  - A fully qualified base ref and exact SHA were reviewed.
- **Checks**
  - [ ] The ref exists.
  - [ ] It resolves to the frozen SHA.
  - [ ] `scripts/azure-templates-variables.yml` and
    `scripts/VERSIONS.txt` are parseable and internally consistent.
  - [ ] The release type permits this base:
    - first line preview: audited `preview.0` creation point;
    - stable: maintenance integration branch;
    - hotfix preview/RC: parent stable tag;
    - hotfix stable: prerelease branch of the same four-part version.
- **Actions**
  - None.
- **Desired end state/evidence**
  - Base ref, commit, parsed version state, and file hashes.
- **Approval/human decision**
  - Required when selecting or changing the base.
- **Rerun behavior**
  - Matching ref/SHA is Done. A moved ref is Blocked, never silently rebound.

### A3. Create or verify the maintenance branch

- **Preconditions**
  - Applies only when the release line needs `release/X.Y.x`.
- **Checks**
  - [ ] If the branch exists, record its SHA and verify it is the intended line.
  - [ ] If it is absent, require an explicit creation ref/SHA.
  - [ ] The creation point is the target numeric version with
    `PREVIEW_LABEL: preview.0`.
- **Actions**
  - Create `release/X.Y.x` at the approved SHA.
- **Desired end state/evidence**
  - Remote maintenance branch exists at the approved creation point or an
    already accepted existing line point.
- **Approval/human decision**
  - Branching approval is required before creating the branch.
- **Rerun behavior**
  - Exact existing branch is Done. Concurrent exact creation is accepted after
  reread. Any other target is Blocked.

### A4. Pin the Skia gitlink and matching `mono/skia` ref

- **Preconditions**
  - Release base is exact.
- **Checks**
  - [ ] Read `externals/skia` as a gitlink from the exact base commit.
  - [ ] Record the 40-character Skia commit.
  - [ ] Check `mono/skia:refs/heads/release/{release identity}`.
- **Actions**
  - If absent, create that `mono/skia` ref at the exact gitlink SHA.
- **Desired end state/evidence**
  - Matching `mono/skia` branch HEAD equals the SkiaSharp gitlink SHA.
- **Approval/human decision**
  - Covered by branching approval.
- **Rerun behavior**
  - Exact existing ref is Done. Concurrent exact creation is accepted. A
  different SHA is Blocked.

### A5. Create or verify the exact SkiaSharp release branch

- **Preconditions**
  - Base and matching Skia ref are exact.
  - Worktree is clean.
- **Checks**
  - [ ] If the remote branch exists, its commit is descended from the approved
    base.
  - [ ] Its version state equals the exact numeric version and release label.
  - [ ] Its Skia gitlink remains the pinned Skia commit.
- **Actions**
  - [ ] Create `release/{release identity}` from the approved base.
  - [ ] Set `PREVIEW_LABEL` to `preview.N`, `rc.N`, or `stable`.
  - [ ] Update SkiaSharp/HarfBuzzSharp versions together when the base version
    differs, preserving valid four-part-zero normalization.
  - [ ] Commit only the owned version files.
  - [ ] Include the approved base SHA and Skia SHA in commit evidence.
  - [ ] Push without force.
- **Desired end state/evidence**
  - Remote exact release branch contains a verified version commit descended
    from the approved base and referencing the pinned Skia commit.
- **Approval/human decision**
  - Covered by branching approval.
- **Rerun behavior**
  - Exact branch is Done. A failed push followed by a concurrent exact branch is
    accepted after fetch/reread. Wrong ancestry or version state is Blocked.

### A6. Open the stable post-cut bump PR

- **Preconditions**
  - Applies only to stable, non-hotfix releases.
  - Exact release branch is safely cut.
- **Checks**
  - [ ] Read current maintenance branch state.
  - [ ] Calculate the next SkiaSharp patch version.
  - [ ] Increment HarfBuzzSharp using the established revision/bucket policy.
  - [ ] Require `PREVIEW_LABEL: preview.0`.
  - [ ] Check the expected bump branch and open PR.
- **Actions**
  - [ ] Create the bump branch from the maintenance branch.
  - [ ] Update both version files.
  - [ ] Commit and push without force.
  - [ ] Open a complete-template PR targeting `release/X.Y.x`.
- **Desired end state/evidence**
  - One verified bump branch and one open PR.
- **Approval/human decision**
  - A maintainer reviews and merges the PR through normal branch protection.
  - Release automation never merges it.
- **Rerun behavior**
  - Reuse an exact existing branch/PR. If the maintenance branch is already at
    or beyond the expected next version with matching HarfBuzz/preview state,
    mark Done. Otherwise wait for human merge.

## Phase B: External build, test, BAR, promotion, and publication

### B1. Establish the exact release build

- **Preconditions**
  - Exact release branch and source commit exist remotely.
- **Checks**
  - [ ] Select the protected release Build for that exact branch/commit.
  - [ ] Confirm real signing, API Scan, and BAR registration are enabled.
  - [ ] Do not treat safe-default or public-CI-only runs as release evidence.
- **Actions**
  - Queue only if the responsible build system does not trigger automatically.
- **Desired end state/evidence**
  - Exact Build run ID, build number, source branch, source commit, and result.
- **Approval/human decision**
  - Follow dnceng queue and protected-resource policy.
- **Rerun behavior**
  - Never combine downstream state from another Build attempt. A replacement
    Build becomes a new release attempt requiring a new exact evidence chain.

### B2. Verify signing, API Scan, package artifacts, and BAR

- **Preconditions**
  - Exact Build run completed.
- **Checks**
  - [ ] Native and managed build stages succeeded.
  - [ ] Shipping packages entered the signing path.
  - [ ] NonShipping transport packages did not enter product signing.
  - [ ] API Scan reached its required result.
  - [ ] One BAR was registered for the exact Build.
  - [ ] BAR source repository, branch, and commit match.
  - [ ] BAR Shipping/NonShipping/symbol classifications match policy.
  - [ ] Standard BAR validation and configured Darc/Maestro promotion succeeded.
- **Actions**
  - None outside the owned pipeline.
- **Desired end state/evidence**
  - BAR build ID and validation/promotion records tied to the exact Build.
- **Approval/human decision**
  - Any waiver must be explicit, owned by the responsible team, and retained.
- **Rerun behavior**
  - Retry within the same exact run only where the pipeline supports it.
    Otherwise record a new Build/BAR attempt; do not merge evidence.

### B3. Verify the connected Tests run

- **Preconditions**
  - Exact Build identity is known.
- **Checks**
  - [ ] Resolve the connected Tests run from trigger/resource metadata.
  - [ ] Confirm it consumed the exact Build attempt.
  - [ ] Require all mandatory connected tests to pass.
- **Actions**
  - None unless the owning pipeline requires an explicit retry.
- **Desired end state/evidence**
  - Exact Tests run ID, result, and link tied to the Build.
- **Approval/human decision**
  - Testing may be overridden only by an explicit release-manager decision with
    rationale. The override does not change package/source identity.
- **Rerun behavior**
  - Never substitute a Tests run from another Build.

### B4. Inspect the exact BAR package set

- **Preconditions**
  - Build, Tests, and BAR identities are connected.
- **Checks**
  - [ ] Gather by exact BAR ID, never channel/latest.
  - [ ] Limit the inspection to the intended SkiaSharp/HarfBuzzSharp family.
  - [ ] Verify package signatures.
  - [ ] Verify exact expected package versions.
  - [ ] Confirm source repository/branch/commit metadata.
- **Actions**
  - Read/gather only.
- **Desired end state/evidence**
  - Exact package manifest, hashes, signatures, and BAR ID.
- **Approval/human decision**
  - Required before queueing public publication.
- **Rerun behavior**
  - Re-gathering the same BAR should produce identical package evidence.

### B5. Queue and approve the team NuGet.org publication

- **Preconditions**
  - Exact BAR/package audit is complete.
  - Required Tests evidence or explicit override is recorded.
- **Checks**
  - [ ] Dry-run/preview shows the exact BAR ID, package IDs, versions, and
    NuGet.org destination.
  - [ ] Those versions do not already exist publicly unless recovering an
    already successful publication.
- **Actions**
  - [ ] Queue the team-owned publication pipeline with the exact BAR ID.
  - [ ] Record run ID and approval URL.
  - [ ] A human reviews versions/destination and approves the protected stage.
- **Desired end state/evidence**
  - Successful publication run tied to the exact BAR and package list.
- **Approval/human decision**
  - Mandatory. Repository-side automation must not approve this stage.
- **Rerun behavior**
  - Recover the exact queued/running/succeeded publication run.
  - Do not queue a second run merely because NuGet indexing is delayed.

### B6. Wait for NuGet.org indexing

- **Preconditions**
  - Team publication reports success or is known to have pushed packages.
- **Checks**
  - Poll only the exact expected package versions.
  - Distinguish not-yet-indexed/unlisted status from permanent trust failure.
- **Actions**
  - Wait and retry read-only verification.
- **Desired end state/evidence**
  - Every required public package is listed and retrievable.
- **Approval/human decision**
  - None; waiting is not failure.
- **Rerun behavior**
  - Resume with the same exact publication/package identity.

## Optional Phase C: Public-package smoke testing

- **Preconditions**
  - Exact public SkiaSharp version is indexed.
- **Checks**
  - [ ] Verify the complete public receipt before producing test commands.
  - [ ] Derive the exact HarfBuzzSharp version.
  - [ ] Select only host-applicable tests.
  - [ ] Present the full proposed matrix for human approval.
- **Actions**
  - [ ] Restore pinned local tools.
  - [ ] Run all approved items even if an earlier item fails.
  - [ ] Pin NuGet.org and exact package versions in every command.
  - [ ] Capture screenshots and logs.
  - [ ] Clean up only runner-owned devices/resources.
- **Default coverage**
  - native load and console use;
  - Linux container when Docker is available;
  - Blazor/WASM when the required workload/browser exists;
  - Android API 26 and 37.1 when installed;
  - iOS 18.6 and 26.5 when installed;
  - Mac Catalyst and Windows where applicable.
- **Desired end state/evidence**
  - Per-item command, target, duration, initial result, retry result, logs, and
    screenshot review.
- **Approval/human decision**
  - Smoke testing is advisory by default.
  - Any decision to make it mandatory belongs in a protected team policy.
- **Rerun behavior**
  - Never substitute package version, feed, device runtime, image, simulator,
    or browser. Record repairs and rerun the same exact item.

## Phase D: Finish from the public receipt

### D1. Parse and freeze the exact public version

- **Preconditions**
  - The exact public SkiaSharp package version is known.
- **Checks**
  - [ ] Stable form is `X.Y.Z` or `X.Y.Z.F`.
  - [ ] Prerelease form is
    `X.Y.Z[.F]-(preview|rc).N.{build revision}`.
  - [ ] Derive the build-revision-free release identity and exact release
    branch.
  - [ ] Derive the expected public tag according to the approved tag policy.
- **Actions**
  - None.
- **Desired end state/evidence**
  - Exact public version, release identity, branch, tag, and build revision.
- **Approval/human decision**
  - Operator confirms the exact public version; no "latest" lookup.
- **Rerun behavior**
  - Same public version yields the same identity.

### D2. Verify NuGet registration, catalog, package trust, and family

- **Preconditions**
  - Public version is frozen.
- **Checks**
  - [ ] Resolve exact registration/catalog entries.
  - [ ] Require each package to be listed.
  - [ ] Verify catalog identity and SHA512 package metadata.
  - [ ] Download anchor packages.
  - [ ] Verify package signatures against trusted author/repository
    certificate policy, role, and validity period.
  - [ ] Read repository URL, source branch, and source commit from the
    hash/signature-verified nuspec.
  - [ ] Fetch the source commit.
  - [ ] Parse `scripts/VERSIONS.txt` and
    `scripts/azure-templates-variables.yml` at that commit.
  - [ ] Compose and verify the exact SkiaSharp and HarfBuzzSharp public
    versions.
  - [ ] Enumerate the historical package family from `VERSIONS.txt` at that
    commit, not from current `main`.
  - [ ] Verify every family package.
  - [ ] Require all SkiaSharp-family packages to agree on source commit/branch.
  - [ ] Permit a reused HarfBuzzSharp package to originate from an older
    SkiaSharp commit only when version/dependency composition proves it is the
    intended reused package.
- **Actions**
  - None.
- **Desired end state/evidence**
  - Public receipt containing package IDs, exact versions, catalog SHA512,
    downloaded-package hashes, signature/certificate evidence, source branch,
    source commit, and HarfBuzz composition.
- **Approval/human decision**
  - None for exact verification. Policy/certificate exceptions require security
    and release-owner review and must not be improvised.
- **Rerun behavior**
  - Missing/unlisted packages are Waiting. Hash, signature, metadata, family,
    or source disagreements are Blocked.

### D3. Verify source commit and branch

- **Preconditions**
  - Public receipt is trusted.
- **Checks**
  - [ ] Source repository is `mono/SkiaSharp`.
  - [ ] Source branch is the exact expected release branch.
  - [ ] Source commit exists.
  - [ ] The exact release branch contains that commit.
  - [ ] Version files at the commit compose the exact public package version.
- **Actions**
  - None.
- **Desired end state/evidence**
  - Package-to-source binding.
- **Approval/human decision**
  - None.
- **Rerun behavior**
  - A branch may advance after building; that is acceptable if it still
    contains the source commit. The package source commit remains the tag
    target.

### D4. Select and validate the previous tag

- **Preconditions**
  - Current public version and source commit are exact.
- **Checks**
  - [ ] Enumerate valid SkiaSharp release tags.
  - [ ] Order them using NuGet release ordering.
  - [ ] Select the greatest tag below the current public version.
  - [ ] Resolve it to a commit.
  - [ ] Require it to be an ancestor of the shipped source commit when it will
    define release-note or reconciliation boundaries.
- **Actions**
  - None.
- **Desired end state/evidence**
  - Exact previous tag and commit, or explicit `none`.
- **Approval/human decision**
  - Review boundary changes, cross-line ordering, and hotfix cases.
- **Rerun behavior**
  - A different derived previous tag is a reason to stop and review, not to
    silently change the range.

### D5. Create or verify the immutable public tag

- **Preconditions**
  - Public receipt, source, expected tag, and previous tag are approved.
- **Checks**
  - [ ] If the tag exists, read its peeled commit.
  - [ ] Require equality with the public package source commit.
- **Actions**
  - Create a lightweight tag directly at the verified source commit and push
    without force.
- **Desired end state/evidence**
  - Remote tag points exactly to the package source commit.
- **Approval/human decision**
  - Mandatory irreversible tag approval showing tag name and target SHA.
- **Rerun behavior**
  - Exact existing tag is Done. Concurrent exact creation is accepted after
  reread. Any other target is Blocked permanently.

### D6. Create, migrate, or verify the GitHub Release draft

- **Preconditions**
  - Exact tag exists.
- **Checks**
  - [ ] Search by exact tag.
  - [ ] Validate release ID, tag, title, target commit, prerelease flag, URL,
    and draft/published state.
  - [ ] Generate notes using the exact current and previous tags.
  - [ ] Validate managed summary/generated-note markers.
- **Actions**
  - If absent:
    - create a draft targeted to the exact source commit;
    - insert an empty managed summary region;
    - insert GitHub-generated notes in their managed region.
  - If a compatible legacy markerless draft exists:
    - preserve all existing body bytes as generated-note content;
    - add managed markers without deleting content.
  - If complete markers exist but generated notes are empty:
    - regenerate only the generated-note region;
    - preserve the managed summary and human-owned bytes.
- **Desired end state/evidence**
  - Exact draft ID/URL and complete managed regions with nonempty generated
    notes.
- **Approval/human decision**
  - Tag/draft approval covers creation or compatible migration.
- **Rerun behavior**
  - Matching marked draft is Done and untouched.
  - A published matching release skips draft mutation.
  - Duplicate/incomplete/out-of-order markers or conflicting metadata are
    Blocked.

### D7. Review and bind the exact publication body

- **Preconditions**
  - Draft is exact and ready.
- **Checks**
  - [ ] Download/reread the live draft.
  - [ ] Preserve GitHub-generated notes.
  - [ ] Preserve any reviewed summary and human-owned bytes.
  - [ ] Ensure the managed summary region is valid even when intentionally
    empty.
  - [ ] Calculate SHA256 over the exact UTF-8 body.
  - [ ] Record release ID, source commit, tag, title, prerelease flag, body
    SHA256, and observation time.
- **Actions**
  - Editorial work may update only the owned summary region through the
    deterministic release-note process.
- **Desired end state/evidence**
  - A human-readable body preview and exact body hash bound to one draft ID.
- **Approval/human decision**
  - Mandatory publication approval of that exact draft/body.
- **Rerun behavior**
  - Any body or release-ID change invalidates approval. Reread, review, and
    approve a new hash.

### D8. Publish the GitHub Release

- **Preconditions**
  - Exact publication approval is current.
- **Checks immediately before mutation**
  - [ ] Tag still targets the package source commit.
  - [ ] Source branch still contains the source commit.
  - [ ] Draft ID and metadata are unchanged.
  - [ ] Managed markers and generated notes are valid.
  - [ ] Live body SHA256 equals the approved hash.
- **Actions**
  - Publish the existing draft without rewriting its body.
- **Desired end state/evidence**
  - Exact release ID/URL is no longer a draft and retains the approved body.
- **Approval/human decision**
  - Mandatory irreversible publication approval.
- **Rerun behavior**
  - If an exact matching release was published concurrently or by a previous
    attempt, mark Done. A recreated release ID, conflicting exact SHA target,
    or changed body is Blocked.

## Phase E: Closeout

### E1. Require shipped state

- **Preconditions**
  - Finish receipt is available.
- **Checks**
  - [ ] Package source commit exists.
  - [ ] Exact release branch contains it.
  - [ ] Exact public tag exists at it.
  - [ ] GitHub Release exists and is published.
  - [ ] Release title, tag, and prerelease state match.
  - [ ] Exact source-commit target is preferred.
  - [ ] A historical published release targeting `main` or its exact source
    branch may be accepted only when the immutable tag still proves the source
    commit; record a warning.
- **Actions**
  - None.
- **Desired end state/evidence**
  - Shipped-state gate report.
- **Approval/human decision**
  - None for exact state; legacy-target acceptance must be visible.
- **Rerun behavior**
  - Draft/missing release is Waiting. Conflicting immutable state is Blocked.

### E2. Maintain upcoming release milestones

- **Preconditions**
  - Shipped-state gate passed.
- **Checks**
  - [ ] Read current major and Skia milestone from `main` version files.
  - [ ] Fetch Chromium schedules for the next three milestones.
  - [ ] Calculate SkiaSharp preview/RC/stable milestone titles, dates, and
    descriptions.
  - [ ] Compare with all open/closed GitHub milestones.
- **Actions**
  - Create missing future milestones and update only mismatched owned fields.
- **Desired end state/evidence**
  - Next three milestone schedules match policy.
- **Approval/human decision**
  - Schedule changes should be reviewed when source schedule data is missing or
    surprising.
- **Rerun behavior**
  - Matching milestones are no-ops. A Chromium lookup failure is a warning and
    must not erase existing dates or suppress independent closeout work.

### E3. Reconcile shipped PRs and linked issues

- **Preconditions**
  - Exact current and previous tag commits form an unambiguous ancestry range.
  - The release milestone exists.
- **Checks**
  - [ ] Read first-parent commit subjects from exclusive previous tag through
    inclusive shipped source commit.
  - [ ] Extract merged PR numbers from `(#NNN)` merge subjects.
  - [ ] Resolve issues closed by each PR.
  - [ ] Compare current PR/issue milestone assignments with the shipped
    milestone.
- **Actions**
  - Assign each shipped PR and linked issue to the shipped release milestone.
- **Desired end state/evidence**
  - Reconciliation list showing PR, linked issue, previous milestone, and final
    milestone.
- **Approval/human decision**
  - Ambiguous ancestry or missing boundary requires human resolution.
- **Rerun behavior**
  - Already assigned items are no-ops. Do not infer a range from unrelated
    history.

### E4. Move open work and close shipped milestones

- **Preconditions**
  - Tags and milestone inventory are current.
- **Checks**
  - [ ] For every open release milestone with a corresponding shipped tag,
    list open items.
  - [ ] Select the next greater open, unshipped milestone.
  - [ ] Include a newly created schedule milestone as a valid target only after
    rereading it and obtaining its real number.
- **Actions**
  - [ ] Move each open item to the selected target.
  - [ ] Reread the source milestone and require zero open items.
  - [ ] Close the shipped milestone.
- **Desired end state/evidence**
  - Shipped milestones closed; open work retained on the next eligible
    milestone.
- **Approval/human decision**
  - If no eligible target exists, stop for a human to create/select one.
- **Rerun behavior**
  - Completed moves and closures are no-ops. Never close a milestone while open
    items remain.

### E5. Dispatch release-note and issue-template convergence

- **Preconditions**
  - GitHub Release is published.
- **Checks**
  - [ ] Determine whether release-note generation already reflects the exact
    shipment.
  - [ ] For stable releases, determine whether issue-template version choices
    already include the new stable version.
- **Actions**
  - [ ] Dispatch targeted release-note generation for the exact package base.
  - [ ] For stable only, refresh issue-template release choices.
- **Desired end state/evidence**
  - Release-note facts/prose work is queued or complete.
  - Stable issue template lists current supported releases.
- **Approval/human decision**
  - Generated prose is reviewed in a normal PR before merging.
- **Rerun behavior**
  - Dispatches must be safe to repeat.
  - A failed dispatch does not roll back the release; rerun only the missing
    dispatch.

### E6. Converge reviewed release summaries

- **Preconditions**
  - Release-note facts are regenerated for the exact shipment tag.
- **Checks**
  - [ ] Deterministic data includes exact shipment tags and PR ranges.
  - [ ] Consumer prose passes the release-note schema and safety checks.
  - [ ] Rendered page, TOC, and index are deterministic.
- **Actions**
  - [ ] Open and review the release-note PR.
  - [ ] After merge, replace only the managed summary region of each exact
    GitHub Release.
- **Desired end state/evidence**
  - Website release page and GitHub summary converge without changing GitHub's
    generated-note region.
- **Approval/human decision**
  - Normal code review owns prose acceptance.
- **Rerun behavior**
  - Omitted shipment prose remains unconverged rather than fabricated.
  - Exact matching summaries are no-ops.

## Recovery matrix

| Observed partial state | Required checks | Safe continuation | Block/never do |
|---|---|---|---|
| Nothing exists for a reviewed release | Revalidate exact identity, base, version state, and Skia gitlink | Start at A3/A4 | Do not derive a different version after approval |
| Maintenance branch exists | Verify intended line and approved creation ancestry | Skip creation and continue | Never force it to the planned SHA |
| Maintenance branch missing for first preview | Require explicit `preview.0` creation ref/SHA | Create once, reread, continue | Do not guess from current `main` after review |
| `mono/skia` branch exists | Require exact equality with gitlink SHA | Mark Done | Never move it |
| `mono/skia` branch missing but SkiaSharp branch exists | Read gitlink from the verified SkiaSharp/base commit | Create exact counterpart, then reread both | Do not use `mono/skia` default-branch tip |
| Exact SkiaSharp release branch exists | Verify base ancestry, version files, label, and gitlink | Mark Done and continue to external build | Never rewrite or force-push it |
| Release branch push raced | Fetch and verify concurrent remote branch | Accept exact matching result | Reject unrelated ancestry/state |
| Stable bump branch/PR exists | Verify ancestry and exact next versions | Reuse and wait for human merge | Never auto-merge or overwrite stale branch |
| Stable bump already merged | Verify maintenance state is at/above expected version with `preview.0` and matching HarfBuzz | Mark Done | Do not open a duplicate PR |
| Build running | Verify exact source commit and attempt identity | Wait/monitor same run | Do not combine with another Tests/BAR attempt |
| Build failed | Preserve exact failure evidence | Retry according to pipeline ownership or start a new explicit attempt | Do not relabel a failed run as release evidence |
| Connected Tests incomplete | Verify connection to exact Build | Wait; explicit override only by release manager | Do not substitute another test run |
| BAR exists | Verify source/run/package classifications | Reuse exact BAR | Do not gather by latest channel |
| Publication pipeline queued/running | Match exact BAR and package set | Resume/monitor same run | Do not queue duplicates because approval is pending |
| Publication succeeded but NuGet is lagging | Poll exact versions | Wait and retry D2 | Do not republish |
| One family package is missing | Confirm publication manifest and indexing | Wait if genuinely pending | Block if package was omitted from the published set |
| Public anchor hash/signature fails | Re-download once and verify policy/time | Escalate to package/security owners | Never tag or release untrusted bytes |
| Exact tag exists at source commit | Verify peeled target | Mark Done | Never recreate/move it |
| Exact tag exists elsewhere | Preserve evidence | Human resolution requires a new release/version policy | Never delete or force-move it |
| Tag push raced | Reread remote tag | Accept only exact source target | Reject any other target |
| No GitHub draft exists | Verify tag/source/title/previous tag | Create generated-notes draft | Do not publish directly without review |
| Matching marked draft exists | Verify metadata, markers, and generated notes | Preserve and continue to body review | Do not regenerate or erase reviewed summary bytes |
| Markerless compatible draft exists | Preserve all body bytes | Migrate by adding owned markers | Do not discard body |
| Marked draft has empty generated notes | Preserve summary/human bytes | Regenerate generated-note region only | Do not rewrite other regions |
| Draft markers malformed | Preserve body and report exact marker issue | Human repairs draft; rerun checks | Do not guess ownership boundaries |
| Draft edited after approval | Recompute body hash and show diff | Obtain new publication approval | Never publish under stale approval |
| Release published between check and apply | Reread ID/body/tag/target/prerelease state | Accept only exact matching publication | Block recreated ID or changed exact target |
| Matching release already published | Verify shipped-state gate | Skip tag/draft/publish and start E2 | Do not rewrite body during recovery |
| Published legacy release targets branch name | Require exact immutable tag at verified source commit; record warning | Continue closeout if all other metadata matches | Do not accept a wrong exact SHA |
| Schedule milestone already exists | Compare owned due date/description | Update only mismatches | Do not duplicate or rename unrelated milestone |
| Reconciliation partly applied | Recompute first-parent range and current assignments | Apply only remaining assignments | Do not infer a new previous boundary |
| Some open items already moved | Reread both milestones | Move remaining items, verify zero, close | Never close early |
| Milestone has open items and no target | Preserve item list | Human creates/selects eligible unshipped target | Do not drop items or close milestone |
| Release-note dispatch failed | Verify release remains published | Redispatch exact notes work | Do not roll back tag/release |
| Issue-template refresh failed | Verify stable release and current template | Rerun stable-only refresh | Do not run for preview/RC |

### Recovery for manually released 4.152 RC1

Treat the existing `release/4.152.0-rc.1` branch and any public package as
authoritative observed state, not as proof that every later step happened.

- [ ] Query NuGet.org for the exact public SkiaSharp RC1 version, including its
  build revision.
- [ ] Verify the complete historical package family, hashes, signatures, nuspec
  source branch, source commit, and HarfBuzz composition.
- [ ] Require source branch `release/4.152.0-rc.1`.
- [ ] Require the source commit to exist and be contained in that branch.
- [ ] Read its `externals/skia` gitlink.
- [ ] Verify or create only the missing matching
  `mono/skia:release/4.152.0-rc.1` ref at that gitlink.
- [ ] Determine the exact RC1 tag under the approved prerelease tag policy.
- [ ] If no tag exists, obtain tag approval and create it at the verified
  package source commit.
- [ ] If an exact tag already exists, accept it only at that commit.
- [ ] Select and verify the immediate previous release tag.
- [ ] Create, migrate, or verify the draft without losing body bytes.
- [ ] If already published, skip draft/publication mutations and require the
  shipped-state gate.
- [ ] Resume schedule, reconciliation, rollover, closure, release-note
  dispatch, and issue-template rules from current remote state.
- [ ] Record as unresolved before mutation:
  - the exact public RC1 build revision;
  - whether a historical-form tag already exists remotely;
  - whether the parent automation's build-revision-free tag proposal is being
    rejected or adopted by explicit policy.

## Conflicts requiring human resolution

- A release version or tag name was already used for different bytes/source.
- A release ref or tag points to a conflicting commit.
- A public package's repository metadata is absent, malformed, or names another
  source branch/repository.
- SkiaSharp family packages disagree on source commit/branch.
- Trusted-signing policy does not cover the observed signature.
- The exact Build/Tests/BAR/publication chain cannot be reconstructed.
- A stable/preview destination in the team pipeline does not match the package
  set.
- A draft/release has conflicting metadata or malformed ownership markers.
- Previous-tag ordering selects an unexpected line/hotfix boundary.
- The previous tag is not an ancestor of the shipped commit.
- A published release exists without the authoritative exact tag.
- A milestone boundary is ambiguous or has no rollover target.
- A policy override is requested but no authorized owner accepts it.

### Things automation must never overwrite, move, or merge

- Existing conflicting release/maintenance branches.
- Existing conflicting `mono/skia` refs.
- Existing tags.
- Published NuGet versions.
- Conflicting GitHub Releases.
- Human-owned release body bytes.
- GitHub-generated notes outside the generated-note owner operation.
- Stable bump PRs or protected branches by automatic merge.
- Milestone assignments outside the exact shipped range.
- Developer-owned simulators, devices, or local resources during smoke tests.

## Evidence and audit record to retain

- Release operator and approvers.
- UTC timestamps for observations and decisions.
- Tooling repository, commit, and dependency lock identities.
- Release identity/type and exact public package version.
- Integration, maintenance, base, release, Skia gitlink, and matching Skia refs
  with SHAs.
- Version-file before/after values and release commit.
- Stable bump branch, commit, PR URL, and human merge result.
- dnceng Build ID/number/URL/source/result.
- Connected Tests ID/URL/result and exact Build relationship.
- BAR ID, source metadata, channel/promotion status, and package manifest.
- Team publication pipeline ID/URL/approval/result.
- NuGet registration/catalog entries, listed state, catalog SHA512, downloaded
  hashes, signatures, certificate fingerprints/roles/validity, nuspec source,
  dependency composition, and full historical package family.
- Optional smoke-test matrix, exact commands, devices/runtimes, results,
  retries, logs, screenshots, and signoff/override.
- Previous tag and resolved commit.
- Public tag and peeled target.
- GitHub draft/release ID, URL, title, target, prerelease state, marker state,
  generated-notes evidence, approved UTF-8 body SHA256, and publication result.
- Milestone schedule changes, PR/issue reconciliation range and assignments,
  rollover moves, and closure rereads.
- Release-note and issue-template dispatch outcomes and resulting PR/commit.
- Every warning, blocked conflict, exception, and explicit override.

The audit record may be stored in host-native logs or an immutable release
record, but it is never an action input after authoritative state has changed.

## Host adaptation appendix

### Any host must provide

- Exact immutable inputs and a way to display them before mutation.
- Read access to all authoritative systems.
- Operation-scoped credentials.
- A human approval mechanism for each protected/irreversible boundary.
- Noninteractive Git/API tooling.
- A clean worktree or isolated checkout for version commits.
- Durable logs with links and hashes.
- Cancellation, timeout, and retry behavior that preserves exact identity.
- A way to reread remote state after writes.

### Local operator

- May use CLI tools and a local checkout.
- Must:
  - pin the tooling commit;
  - avoid ambient mutable credentials where possible;
  - obtain approval outside the process before irreversible actions;
  - save exact command/output evidence;
  - keep generated audit files out of source mutations.
- Local execution does not weaken source, package, or approval checks.

### GitHub Actions

- May use separate manual Prepare and Finish workflows because external package
  publication may take days and Finish requires the later exact public version.
- May map approvals to protected environments or another reviewed deployment
  gate.
- Must not:
  - rely on artifacts as authoritative state;
  - expose write credentials to read-only jobs;
  - combine all phases into one long-running job to avoid approvals;
  - use workflow run identity as a substitute for Git/NuGet evidence.
- Draft release reads may require elevated repository permission even when the
  job performs no mutations; keep the mutation token separate.

### Azure Pipelines

- May host repository-side checks, but it remains distinct from:
  - the protected release Build;
  - connected Tests;
  - BAR/Maestro state;
  - the team NuGet publication pipeline.
- Must carry exact source/run/resource identities across stages.
- Approval checks must bind exact versions/destinations, not merely a stage
  name.

### Future release service

- Must implement the same ordered checks, approvals, rereads, and recovery
  semantics.
- It may store an audit journal, but actions must converge from current
  authoritative system state.
- It must not invent a generic state machine that changes repository-specific
  release policy.

## Current process gaps and open policy decisions

These items are not normative steps and must be resolved explicitly before
automation is declared authoritative.

- **Prerelease tag identity**
  - Historical tags include the exact public build revision, for example
    `v4.151.0-rc.1.1`.
  - The parent C# tooling prototype derives a build-revision-free tag such as
    `v4.151.0-rc.1`.
  - Choose one policy deliberately. Existing tags and RC1 recovery must not be
    rewritten to fit the new choice.
- **4.152 RC1 facts**
  - Confirm the exact public build revision, source commit, package family,
    existing tag, draft/release state, and previous tag from NuGet/GitHub before
    mutating anything.
- **Pipeline topology**
  - Current parent documentation names protected Build 1642 and connected Tests
    1630.
  - Older retained status material refers to a native/managed/tests chain with
    different IDs.
  - Identify the currently authoritative release chain and update all operator
    material together; never combine the two models.
- **Package publication handoff**
  - Decide which immutable Build/BAR/publication IDs repository-side Finish
    should retain as audit evidence even though NuGet.org remains the public
    source of truth.
- **Smoke-test gate**
  - It is currently advisory.
  - If made mandatory, define the protected owner, minimum host coverage,
    override authority, and durable evidence location.
- **Credential isolation**
  - The existing broad repository token can write both repositories.
  - Replace it with a narrowly scoped GitHub App or equivalent without changing
    release semantics.
- **Draft read permissions**
  - GitHub's API permission behavior for drafts can force write-level scope on a
    read-only process.
  - Document and audit the minimum viable token separately from mutation
    authorization.
- **Dispatch idempotency**
  - GitHub workflow dispatch does not provide a natural exact-input idempotency
    key.
  - Define a durable remote receipt or ensure each target workflow converges
    harmlessly when repeated.
- **Previous-tag ordering across lines**
  - Confirm whether ordering is global across all SkiaSharp versions or scoped
    to a product line for release notes and milestone reconciliation.
- **Stable bump timing**
  - Existing policy opens the bump PR immediately after cutting the stable
    branch.
  - Confirm whether merge must complete before Build/publication or may happen
    independently.
- **Legacy published release targets**
  - Decide the sunset policy for accepting `main` or source-branch
    `target_commitish` when an exact immutable tag proves the source commit.
- **Milestone schedule failures**
  - Current behavior treats Chromium lookup failure as a warning and continues
    independent closeout.
  - Confirm which schedule failures, if any, must block rollover.
- **Audit retention**
  - Define the canonical durable location and retention period for the evidence
    listed above.

## Source inventory used for this definition

- `origin/main`
  - `documentation/dev/releasing.md`;
  - `documentation/dev/versioning.md`;
  - release branch/status/testing/publish/milestone skill contracts;
  - historical release branches and tags.
- Parent branch `mattleibow-release-tooling-inventory`
  - `documentation/dev/releasing.md`;
  - Prepare and Finish workflow design;
  - C# receipt, branch, draft, publish, and closeout behavior;
  - recovery and conflict tests;
  - package and release-note documentation.
- Retained current skills
  - `.agents/skills/release-testing/`;
  - `.agents/skills/release-notes/`.
- Retired release implementation immediately before the C# switch
  - `scripts/infra/release/release_prepare.py`;
  - `release_finish.py`;
  - `release_nuget.py`;
  - `release_github.py`;
  - `release_environment.py`;
  - `release_milestones.py`;
  - their schema, CLI, Git, model, summary, and recovery tests.

