# SkiaSharp GitHub Repository Release Checklist

## Purpose and public boundary

This is the ordered, convergent public release checklist for the `mono/SkiaSharp` GitHub repository. It is not a complete organizational release coordinator, and it can be run from a local machine or any host with the required Git, GitHub, and NuGet.org access.

The checklist prepares explicit repository state, stops at a single opaque external handoff, and later resumes from public evidence. Run the same checklist again as authoritative state changes; do not switch to a separate "finish" process.

The public boundary question is: **Does the exact package version exist on NuGet.org, and is it listed, indexed, and retrievable?**

- If the answer is no, classify the run as **Waiting** and stop.
- The operator independently completes any manual testing and triggers or approves the organization's internal release process outside this checklist.
- A branch build may automatically flow through organizational systems before an operator runs an internal publication process, but that context is not repository release evidence.
- This checklist never accesses, queries, queues, validates, models, or requires credentials or identifiers for internal build systems, internal feeds, signing or API-scan pipelines, BAR, Darc, Maestro, or internal publication.
- For a prerelease, rerun later with the exact public version because its build revision cannot be derived from the build-revision-free release identity.
- For a stable release, the public version is the bare release identity, so the boundary can be checked immediately.

## Roles and systems

### Systems in scope

- **Git repositories** are authoritative for commits, branches, gitlinks, and tags.
- **NuGet.org** is authoritative for public registration, catalog state, listed state, package bytes, and package immutability.
- **GitHub Releases** is authoritative for draft and published release state and the reviewed release body.
- **GitHub milestones, pull requests, issues, and relationships** are authoritative for public closeout state.
- **The public Chromium milestone schedule** is advisory schedule input; its failure must not block repository closeout.
- A plan, local file, workflow artifact, job log, or chat history is evidence only and never overrides contradictory authoritative state.

### Systems and work explicitly out of scope

- dnceng and Azure DevOps build or test runs.
- Internal package feeds and package testing.
- Product signing and API-scan pipelines.
- BAR records, Darc, Maestro, and promotion channels.
- Internal publication pipelines, their queue parameters, approval URLs, credentials, and run identities.
- The operator's pre-publication device matrix and manual release testing.
- Any organizational decision to approve or publish packages.

Old release-status, internal-package testing, and internal publication skills cover parts of this opaque external gap. They are out of scope and are superseded for this repository checklist; their internal state must not be presented as checklist coverage or a prerequisite.

After NuGet.org publication, the separate public-package smoke-testing tool may test an exact public package on applicable hosts. It is optional and is not a prerequisite for tagging or publishing a GitHub Release unless a future public repository policy explicitly makes it one.

### Roles

- **Release operator:** freezes exact inputs, performs read-only checks, stages precise mutations, records public evidence, and stops on ambiguity.
- **Reviewer/approver:** is a person other than the actor, reviews immutable operation details rather than general release intent, and approves protected or irreversible changes.
- **Repository maintainers:** own `mono/SkiaSharp` branches, tags, releases, milestones, release-note dispatch, and the matching release ref in `mono/skia`.
- **Stable bump PR reviewer:** reviews and merges the post-cut bump through normal branch protection; release automation never merges it.
- **External release operator:** independently performs manual testing and operates the opaque organizational publication process; no internal evidence is consumed here.

## Release identity and explicit inputs

### Terms

- **Release line:** `X.Y`, represented by maintenance branch `release/X.Y.x`.
- **Numeric version:** `X.Y.Z` for a normal release or `X.Y.Z.F` for a four-part hotfix.
- **Release identity:** numeric version plus an optional channel and iteration: `X.Y.Z[-or-.F]-preview.N`, `X.Y.Z[-or-.F]-rc.N`, or bare stable `X.Y.Z[-or-.F]`.
- **Public package version:** bare release identity for stable; `{release identity}.{build revision}` for preview or RC.
- **Build revision:** the public prerelease suffix produced by the external build, such as `26426.14`; it is public package identity, not repository branch or new-tag identity.
- **Maintenance branch:** `release/X.Y.x`.
- **Exact release branch:** `release/{release identity}`; it never includes the public build revision.
- **Matching Skia branch:** `mono/skia:release/{release identity}` at the exact `externals/skia` gitlink commit.
- **New public tag:** `v{release identity}` for stable and prerelease releases; it never includes a build revision.
- **Historical prerelease tag:** an immutable existing tag that includes a one-part or two-part build revision after the release identity.
- **Public package family:** every SkiaSharp or HarfBuzzSharp package ID and exact version described by `scripts/VERSIONS.txt` and version composition at the shipped source commit.
- **Signature anchors:** the configured `SkiaSharp`, `SkiaSharp.HarfBuzz`, and `HarfBuzzSharp` packages.
- **Shipped source endpoint:** the source commit proven by the exact public package receipt, not a later branch tip.
- **Release cut projection:** the merge-base that projects an exact release endpoint onto the appropriate maintenance/integration or `main` first-parent history.

### Required inputs for every run

- [ ] Repository identity is exactly `mono/SkiaSharp`.
- [ ] Tooling is pinned to an exact trusted commit on approved default-branch history.
- [ ] Exact release identity is supplied or derived read-only, reviewed, and then frozen.
- [ ] Release type is one of preview, RC, stable, hotfix preview/RC, or hotfix stable.
- [ ] Release line, maintenance mode, and expected branch names are frozen.
- [ ] A fully qualified release base ref and its exact resolved SHA are frozen.
- [ ] The expected exact SkiaSharp release branch is frozen.
- [ ] The expected matching `mono/skia` branch is frozen.
- [ ] The expected release label is `preview.N`, `rc.N`, or the source sentinel `stable`.
- [ ] Current SkiaSharp and HarfBuzzSharp versions at the base are recorded.
- [ ] For stable non-hotfix releases, expected next versions, bump branch, and PR base are frozen.
- [ ] Repository reads and writes target fully qualified refs, never an ambient "latest" branch.

### Public-version input

- [ ] Exact public SkiaSharp package version is optional before the public boundary.
- [ ] Stable public version is derived as the bare release identity `X.Y.Z` or `X.Y.Z.F`; there is no `-stable` public suffix.
- [ ] Preview/RC public version must be supplied on the later rerun and must be `X.Y.Z[.F]-(preview|rc).N.{build revision}`.
- [ ] The public version must normalize back to the frozen release identity.
- [ ] Once supplied or derived, the public version is immutable input; never replace it with a search result for "latest."

### Release base and maintenance policy

- **Non-hotfix when maintenance exists:** use `release/X.Y.x` as the integration branch and release base.
- **First preview when maintenance is missing:** select an explicitly audited `main` commit whose numeric version is the target and whose `PREVIEW_LABEL` is `preview.0`; use it as both release base and maintenance creation point.
- **Later preview, RC, or stable when maintenance is missing:** continue from the latest exact validated prerelease lineage intended for the release, but independently create maintenance from the audited target-version `preview.0` point on `main`.
- **4.152 stable recovery:** continue from the RC1 source lineage and create `release/4.152.x` separately from the audited `4.152.0` `preview.0` point.
- **Hotfix preview/RC:** base on immutable parent stable tag `vX.Y.Z`; do not create or advance maintenance.
- **Hotfix stable:** base on an explicitly selected prerelease branch for the same four-part version; do not create maintenance and do not open the normal stable bump PR.
- A missing-maintenance creation point and a later release base are separate reviewed inputs; never collapse them to whichever ref is newest.

## Host-agnostic preconditions, safety, and status

### Preconditions

- [ ] Git can fetch all relevant `mono/SkiaSharp` refs and tags and the required `mono/skia` commit/ref.
- [ ] GitHub API access can read drafts, published releases, milestones, PRs, issues, comments, and closing-issue relationships.
- [ ] NuGet.org V3 registration, catalog, flat-container/package content, and signature verification endpoints are reachable.
- [ ] Exact .NET SDK and locked repository tooling are available when repository tools are used.
- [ ] The worktree is clean before any version-file mutation, except for explicitly named untracked audit outputs.
- [ ] Branch/version mutations use an isolated worktree or equivalent ref-level operation and do not alter the operator's active product checkout.
- [ ] Git credentials are noninteractive, operation-scoped, and not persisted in the checkout.
- [ ] Public writes use only the minimum GitHub permissions required for the approved operation.
- [ ] Every protected or irreversible mutation has an independently configured approval mechanism that can be verified before write credentials are loaded.
- [ ] Approval prevents self-review and binds exact observed state, target identity, desired mutation, and content hash or equivalent tamper-evident value.
- [ ] Mutating work is serialized by release identity; cancellation or retries never replace an in-flight mutation with a different identity.

### Immutable safety rules

- **Never force-update a maintenance branch, exact release branch, or matching `mono/skia` branch.**
- **Never move, delete, or rewrite a release tag.**
- **Never republish or replace a NuGet.org package version.**
- **Never overwrite a conflicting GitHub Release or human-owned body bytes.**
- **Never tag a branch tip merely because it is newer; tag only the shipped source endpoint proven by the public receipt.**
- **Never auto-merge the stable bump PR.**
- **Never infer success from a write response; reread authoritative remote state.**
- **Never load write credentials before verifying the applicable approval gate and no-self-review rule.**
- **Never select mutable "latest" state after exact inputs are approved.**
- **Never use internal pipeline state as a substitute for the NuGet.org public boundary.**
- **Never rewrite historical build-revision-bearing tags to adopt the new tag policy.**
- **Never edit generated release-note output outside its owned source data and managed body markers.**
- **Never let a public schedule lookup failure erase an existing milestone date or block unrelated closeout work.**

### Status model

- **Done:** exact desired authoritative state already exists and has been reread.
- **Ready:** all checks pass and any required independent approval is current.
- **Waiting:** a human-owned action or public state is incomplete but no conflict exists.
- **Blocked:** authoritative state conflicts, trust validation fails, or required identity is ambiguous.
- **Skipped:** the step does not apply to this release type.

Every mutation uses the same convergence protocol:

1. Read authoritative state.
2. Compare it with frozen desired state.
3. Classify the step.
4. Stage exact observations and the desired mutation in a tamper-evident approval record.
5. Obtain approval from a person other than the actor.
6. Verify the gate before loading write credentials.
7. Reread state and recompute the approval binding immediately before mutation.
8. Apply only the approved mutation.
9. Reread and require the exact desired end state.
10. Record public audit evidence.

A Waiting result normally stops the ordered checklist. The only exception is the stable bump PR's independently human-owned merge: record its Waiting state and continue to the read-only NuGet.org boundary unless public repository policy has made that merge a publication prerequisite.

### Universal block conditions

- A reviewed mutable ref moved after approval.
- The tooling commit is untrusted, unreachable, or no longer on approved history.
- An existing release branch has wrong ancestry, version state, label, or Skia gitlink.
- An existing matching `mono/skia` release branch points to a different commit.
- A required maintenance creation point is not the target numeric version at `preview.0`.
- A bump branch or PR has unrelated ancestry or stale/incorrect versions.
- A public package has conflicting identity, malformed catalog data, mismatched hash or size, invalid ZIP/nuspec, invalid configured-anchor signature, or conflicting source/dependency composition.
- A source package's branch/commit cannot be bound to the expected `mono/SkiaSharp` release branch and repository history.
- An existing public tag points anywhere other than the shipped source endpoint.
- A GitHub Release conflicts in tag, target, title, prerelease state, or ownership markers.
- The publication body changed after approval.
- A previous tag cannot be normalized or ordered unambiguously.
- Reconciliation endpoints cannot be projected unambiguously.
- An approval gate is absent, unverifiable, permits self-review, or does not bind the exact mutation.
- A milestone with open work has no eligible unshipped rollover target.
- Any public write cannot be reread as the exact desired state.

Missing, unlisted, not-yet-indexed, or not-yet-retrievable public packages are **Waiting**, not Blocked.

## Ordered checklist

### 1. Freeze inputs and validate tooling

**Checks**

- [ ] Resolve the tooling revision exactly and prove it is on approved default-branch history.
- [ ] Validate canonical release identity, release type, numeric version, release line, and expected branch names.
- [ ] Validate the public version if supplied; derive it only for stable.
- [ ] Resolve the fully qualified base ref to the frozen SHA without changing the checkout.
- [ ] Parse `scripts/VERSIONS.txt` and `scripts/azure-templates-variables.yml` at the base.
- [ ] Record file hashes, composed versions, release label, and `externals/skia` gitlink.
- [ ] Fetch refs and tags and inventory partial existing state.
- [ ] Confirm the selected base and maintenance policy agree with the release type.

**Action**

- No remote mutation.

**Desired end state**

- One frozen input record contains exact identity, type, refs, SHAs, versions, branch policy, and optional public version.

**Rerun**

- Recompute remote truth. Matching values are Done; movement or disagreement requires a new review and approval record.

### 2. Verify the base and create or verify maintenance

**Checks**

- [ ] The release base exists at the frozen SHA.
- [ ] Existing `release/X.Y.x`, when applicable, is the intended line and integration branch.
- [ ] If maintenance is missing, its independently frozen creation ref resolves to the frozen creation SHA.
- [ ] The creation commit has the target numeric version and `PREVIEW_LABEL: preview.0`.
- [ ] A later prerelease/stable release base remains distinct from the safe maintenance creation point.
- [ ] Hotfix policy marks maintenance creation as Skipped.

**Action**

- Create a missing `release/X.Y.x` once at the approved safe SHA and push without force.

**Approval**

- Branch creation approval identifies repository, full ref, and exact target SHA.

**Desired end state**

- Maintenance either exists as the accepted integration line or has been created at the audited `preview.0` point.

**Rerun**

- Exact existing state is Done. A concurrent exact create is accepted after reread. Any conflicting branch target is Blocked and is never moved.

### 3. Verify the Skia gitlink and matching `mono/skia` ref

**Checks**

- [ ] Read `externals/skia` as a gitlink from the exact approved release base or verified release source commit.
- [ ] Record its full 40-character commit.
- [ ] Read `mono/skia:refs/heads/release/{release identity}`.
- [ ] If present, require exact equality with the gitlink.
- [ ] If absent, prove the gitlink commit exists in `mono/skia`.

**Action**

- Create the missing matching branch at the exact gitlink commit and push without force.

**Approval**

- Branch creation approval binds repository, full ref, and gitlink SHA.

**Desired end state**

- The matching `mono/skia` release ref exists exactly at the SkiaSharp gitlink.

**Rerun**

- Exact existing state is Done. A concurrent exact create is accepted after reread. A different target is Blocked.

#### Resolved 4.152 RC1 Skia fact

- `mono/skia:release/4.152.0-rc.1` already exists at `9f0d864fd60e9907a8bf5ec84a71d603e6c8db5f`.
- That SHA matches the `externals/skia` gitlink for the RC1 source.
- This verification is **Done**; RC1 recovery must not report the matching Skia branch as missing or attempt to create it.

### 4. Create or verify the exact SkiaSharp branch and version commit

**Checks**

- [ ] The expected branch is exactly `release/{release identity}` with no public build revision.
- [ ] If it exists, it descends from the separately approved release base.
- [ ] Its version state equals the exact numeric version and `preview.N`, `rc.N`, or `stable` label.
- [ ] Its Skia gitlink remains the pinned commit.
- [ ] Four-part version normalization preserves the intended hotfix identity.
- [ ] Only owned version files need mutation.

**Action**

- Create the exact branch from the approved base when absent.
- Set the exact release label.
- Update SkiaSharp and HarfBuzzSharp versions together when required by version composition.
- Commit only owned version files with base/version/Skia evidence.
- Push without force.

**Approval**

- Branch/version approval binds base SHA, branch name, before/after version state, resulting commit, and Skia SHA.

**Desired end state**

- The remote exact release branch contains a verified version commit with approved ancestry and the pinned Skia gitlink.

**Rerun**

- Exact branch state is Done. A concurrent exact push is accepted after fetch and reread. Wrong ancestry, version, label, or gitlink is Blocked.

### 5. Open and report the stable post-cut bump PR

**Applicability**

- [ ] Apply only to stable non-hotfix releases.
- [ ] Mark preview, RC, and every hotfix as Skipped.

**Checks**

- [ ] The exact stable release branch was safely cut.
- [ ] Read current maintenance state.
- [ ] Calculate the expected next SkiaSharp patch and HarfBuzzSharp revision using repository policy.
- [ ] Require next-line `PREVIEW_LABEL: preview.0`.
- [ ] Search for the expected bump branch and open PR.

**Action**

- Create the bump branch from maintenance when needed.
- Update both version files, commit, and push without force.
- Open one complete-template PR targeting `release/X.Y.x`.
- Do not merge it.

**Human state**

- A maintainer reviews and merges through normal branch protection.
- Record the PR URL, current checks/review state, and whether the merge is Done or Waiting.
- Waiting for human merge is reported but does not prevent the read-only public boundary unless repository policy explicitly requires merge first.

**Desired end state**

- One exact bump PR exists, or maintenance is already at or beyond the expected next version with matching HarfBuzzSharp and `preview.0`.

**Rerun**

- Reuse an exact branch/PR. A merged exact bump is Done. A stale or unrelated bump branch is Blocked. Automation never auto-merges or overwrites it.

### 6. Ask whether the exact package exists on NuGet.org

**Checks**

- [ ] Determine the exact public SkiaSharp version from frozen input; for prerelease, do not guess its build revision.
- [ ] Query only public NuGet.org for the exact `SkiaSharp` package registration leaf and catalog entry.
- [ ] Require the exact version to be listed, indexed, and retrievable from the public package-content endpoint.
- [ ] Do not query any internal system to explain absence or infer progress.

**Boundary result**

- If public version is unknown for a prerelease, classify **Waiting** and stop.
- If the exact registration/catalog entry is absent, classify **Waiting** and stop.
- If the package is unlisted, not indexed, or not retrievable, classify **Waiting** and stop.
- Record the exact public version and observation time for the next run.
- The operator independently performs manual testing and operates the opaque internal release process outside this checklist.

**Desired end state**

- The exact public version is listed, indexed, and retrievable, allowing public receipt verification.

**Rerun**

- Rerun this same checklist with the same release identity and exact public version. Never queue, inspect, or correlate an internal run from this step.

### 7. Verify the complete public receipt

**Initial package-to-source binding**

- [ ] Resolve the exact `SkiaSharp` registration and catalog leaves.
- [ ] Require consistent ID/version, listed state, catalog SHA512, package size, and content URL.
- [ ] Download the exact package and verify the downloaded SHA512 and size against catalog data.
- [ ] Validate ZIP central-directory structure, entry paths, duplicate/path-traversal protections, and nuspec identity.
- [ ] Read repository type/URL, source branch, and source commit from the verified nuspec.
- [ ] Require repository type `git` and the exact expected release branch.
- [ ] Record the repository URL as package metadata, but do not use it as the trust anchor: current packages use the approved `https://go.microsoft.com/fwlink/?linkid=868515` redirect rather than a literal GitHub URL.
- [ ] Fetch the source commit and require containment in the exact release branch.
- [ ] Bind repository identity through successful commit/branch containment in the checked `mono/SkiaSharp` repository; an unexpected or changing metadata URL alone must not reject an otherwise correctly bound package.
- [ ] Parse `scripts/VERSIONS.txt` and `scripts/azure-templates-variables.yml` at that source commit.
- [ ] Require source version composition to produce the exact public package version.

**Full historical family**

- [ ] Derive the complete SkiaSharp/HarfBuzzSharp family and exact versions from the shipped source commit, not current `main`.
- [ ] Include reused historical HarfBuzzSharp packages when the shipped version composition requires them.
- [ ] Resolve registration and catalog entries for every family package at its exact expected version.
- [ ] Require every required package to be listed, indexed, and retrievable.
- [ ] Download every family package.
- [ ] For every package, verify catalog SHA512, catalog size, downloaded hash/size, ZIP structure, nuspec ID/version, dependency versions, and repository/source metadata.
- [ ] Require all SkiaSharp-family packages to agree on source branch and source commit.
- [ ] Permit a reused HarfBuzzSharp package to name an older SkiaSharp commit only when exact version and dependency composition prove that reuse is intended.

**Configured-anchor signatures**

- [ ] Signature-verify only configured anchors `SkiaSharp`, `SkiaSharp.HarfBuzz`, and `HarfBuzzSharp`.
- [ ] Require the expected signature structure: author primary signature plus NuGet.org repository countersignature.
- [ ] Require the repository countersignature V3 service index to be exactly `https://api.nuget.org/v3/index.json`.
- [ ] Match trusted certificates by both signature role and configured SHA256 fingerprint.
- [ ] Record certificate subject, issuer, fingerprint, timestamp, `validFrom`, and `validUntil`.
- [ ] Treat certificate expiry as audit and rotation metadata only; a correctly timestamped valid signature remains acceptable after certificate expiry.
- [ ] Make trust rotation additive: add a new role/fingerprint before use and retain old fingerprints while any shipped anchor package still uses them.
- [ ] Do not require non-anchor family packages to have anchor signature evidence; they still require every hash, size, ZIP, nuspec, dependency, and source check.

**HarfBuzzSharp composition**

- [ ] Compose the exact HarfBuzzSharp version from shipped source facts and the public release build revision where applicable.
- [ ] Require `SkiaSharp.HarfBuzz` dependencies to select the intended HarfBuzzSharp version.
- [ ] Require all family dependencies to remain within the verified public family or explicitly allowed platform/framework dependencies.
- [ ] Record whether HarfBuzzSharp was newly produced or intentionally reused.

**Result**

- Missing or unlisted family packages remain Waiting and stop the checklist.
- Malformed catalog/package data, hash or size mismatch, invalid ZIP/nuspec, configured-anchor trust failure, or source/dependency conflict is Blocked.

**Desired end state**

- A public receipt binds every family package byte set to exact identity, source, dependency composition, and configured trust anchors.

**Rerun**

- Reverify public immutable bytes. The same exact package versions must yield identical hashes, metadata, and source binding.

#### Resolved public facts for 4.152 RC1

- Public SkiaSharp version: `4.152.0-rc.1.26426.14`.
- Source branch: `release/4.152.0-rc.1`.
- Source commit: `2357692e1e0fb1d3dc742e74fad4682adf5d4dec`.
- Public package family: 41 packages.
- HarfBuzzSharp version: `14.2.1.200-rc.1.26426.14`.
- Previous tag under transitional NuGet ordering: `v4.152.0-preview.1.1`.
- Expected new tag: `v4.152.0-rc.1`.
- At the captured recovery boundary, no RC1 tag, draft, or published GitHub Release existed.
- Reread current remote tag/release state before mutation; accept only exact convergent state.

### 8. Select and freeze the previous tag

**Checks**

- [ ] Enumerate all valid SkiaSharp release tags.
- [ ] Parse new build-revision-free tags as `v{release identity}`.
- [ ] Parse historical prerelease tags with either a one-part or two-part appended build revision.
- [ ] Normalize historical tags to their release identity without renaming or rewriting them.
- [ ] Order normalized identities using NuGet release ordering, including preview, RC, stable, and four-part hotfixes.
- [ ] Exclude the current tag and every historical or revision-free tag that normalizes to the frozen current release identity.
- [ ] Select the greatest valid normalized release identity strictly below the frozen current release identity, not below its build-revision-bearing public package version.
- [ ] Resolve the selected tag to its peeled commit.
- [ ] Record explicit `none` if there is no previous release.
- [ ] Do not require the previous tag to be an ancestor of the current shipped source endpoint; sibling release branches are normal.

**Approval**

- Review normalized ordering, duplicate normalized identities, cross-line selection, and hotfix boundaries.

**Desired end state**

- One exact previous tag and commit, or reviewed `none`, is frozen for generated notes and reconciliation context.

**Rerun**

- A changed selection is Blocked pending review; never silently change the notes boundary.

### 9. Create or verify the immutable public tag

**Checks**

- [ ] Expected new tag is exactly `v{release identity}`, including prereleases.
- [ ] The target is exactly the shipped source endpoint from the public receipt.
- [ ] If the tag exists, peel it and require exact target equality.
- [ ] If a historical shipped prerelease already has a build-revision-bearing tag, preserve and use it; do not create a replacement solely to adopt the new form.

**Action**

- Create a lightweight tag directly at the verified shipped source endpoint and push without force.

**Approval**

- Irreversible tag approval binds repository, tag name, exact target SHA, public version, and receipt hash.

**Desired end state**

- The remote tag resolves exactly to the package source commit.

**Rerun**

- Exact existing state is Done. A concurrent exact create is accepted after reread. A different target is permanently Blocked and the tag is never moved.

### 10. Create or verify the draft and generated notes

**Checks**

- [ ] Search GitHub Releases by exact tag, including drafts.
- [ ] Validate release ID, tag, title, target, prerelease flag, URL, and draft/published state.
- [ ] Generate notes using the exact previous and current tags.
- [ ] Validate exactly one ordered managed-summary region and one generated-notes region.
- [ ] Preserve all bytes outside managed regions as human-owned.

**Action when absent**

- Create a draft targeted to the exact shipped source commit.
- Insert an empty managed-summary region.
- Insert nonempty GitHub-generated notes in the generated-notes region.

**Compatible recovery**

- If a compatible markerless draft exists, preserve all body bytes as generated-note content and add ownership markers without deleting content.
- If complete markers exist but the generated-notes region is empty, regenerate only that region.
- If an exact matching release is already published, do not mutate draft state and continue to the shipped gate.

**Desired end state**

- One exact draft or published release has compatible metadata, valid managed regions, and nonempty generated notes.

**Rerun**

- Matching marked state is Done and untouched. Duplicate, incomplete, or out-of-order markers or conflicting release metadata are Blocked.

### 11. Approve the exact body and publish the GitHub Release

**Body review**

- [ ] Reread the live draft immediately before review.
- [ ] Preserve generated notes, reviewed summary, and human-owned bytes.
- [ ] Require a valid summary region even when intentionally empty.
- [ ] Compute SHA256 over the exact UTF-8 body bytes.
- [ ] Bind release ID, source commit, tag, title, prerelease flag, body SHA256, and observation time.
- [ ] Present the exact body for independent human approval.

**Pre-publication reread**

- [ ] Tag still targets the shipped source endpoint.
- [ ] Exact release branch still contains the endpoint.
- [ ] Draft ID and metadata are unchanged.
- [ ] Ownership markers and generated notes remain valid.
- [ ] Live body SHA256 equals the approved hash.
- [ ] Approval remains bound to this exact mutation and the actor is not the approver.

**Action**

- Publish the existing draft without rewriting its body.

**Desired end state**

- The exact release ID is published, retains the approved body, and has the expected tag, title, target, and prerelease state.

**Rerun**

- Exact existing publication is Done. Any body drift invalidates approval. A recreated release ID, conflicting target, or changed published body is Blocked.

### 12. Converge shipped state, milestones, reconciliation, and summaries

#### 12.1 Shipped-state gate

- [ ] Exact public receipt remains valid.
- [ ] Exact release branch contains the shipped source endpoint.
- [ ] Exact public tag resolves to that endpoint.
- [ ] GitHub Release exists and is published.
- [ ] Release title, tag, and prerelease state match.
- [ ] Exact source-commit `target_commitish` is preferred.
- [ ] A historical published release targeting `main` or its exact source branch is accepted only with a warning when the immutable tag still proves the source commit.
- [ ] Stop as Waiting when the release remains a draft; stop as Blocked on conflicting immutable state.

#### 12.2 Maintain 12 scheduled milestones

- [ ] Read the current Skia milestone `m` and current major/version policy from public repository state.
- [ ] Independently fetch public schedule data for `m`, `m+1`, and `m+2`.
- [ ] Expand each Skia milestone into exactly four SkiaSharp milestones: `preview.1`, `preview.2`, `rc.1`, and stable.
- [ ] Compare the resulting 12 desired milestone titles, due dates, and owned descriptions with all open and closed GitHub milestones.
- [ ] Create a missing scheduled milestone only when its due date is no more than 30 days stale.
- [ ] Update only mismatched owned fields on an existing milestone.
- [ ] Reread every created milestone and use its actual GitHub milestone number.
- [ ] Treat failure of any public schedule lookup as a warning only.
- [ ] Never erase existing dates, suppress independently available schedule work, or block reconciliation, rollover, closeout, dispatch, or summary convergence because schedule data is unavailable.

#### 12.3 Build the complete shipped PR range

- [ ] Enumerate the numeric release line's exact release branches in release order: previews, RCs, stable, and applicable hotfixes.
- [ ] Detect shipped identities from both historical revision-bearing and new revision-free tags after normalization.
- [ ] Identify the appropriate integration history for each cut: maintenance/integration when present, otherwise `main`.
- [ ] Project each exact release endpoint onto that integration history using `git merge-base`.
- [ ] For consecutive release cuts, traverse the integration/main first-parent range between their projected merge-bases.
- [ ] For every exact release branch included in the current shipped window, also traverse that branch's first-parent segment from its merge-base to its shipped source endpoint.
- [ ] Define the candidate commit set as the union of the projected integration/main first-parent range and all applicable exact-release-branch first-parent segments.
- [ ] This union is mandatory so exact-release-branch-only PRs and hotfix branch-exclusive work are not lost.
- [ ] Deduplicate commits and extracted PRs across all segments and against PRs already attributed to an earlier shipped release.
- [ ] Roll work from an unshipped preview or RC into the next shipped release window.
- [ ] Leave integration/main commits after the latest release cut unassigned until a later cut ships.
- [ ] Record raw endpoints, merge-bases, each first-parent segment, rolled-forward unshipped ranges, excluded post-cut tail, and deduplication decisions.
- [ ] Treat ambiguous merge-base projection or conflicting normalized shipped identities as Blocked pending human resolution.

#### 12.4 Reconcile PRs and linked issues

- [ ] Find the GitHub milestone titled exactly for the shipped release identity.
- [ ] If that milestone is absent, record a visible warning and classify exact-shipment reconciliation as Skipped; do not invent a hotfix/stale milestone or assign work to another milestone without an explicit policy.
- [ ] Extract merged PR numbers from repository merge history and deduplicate them.
- [ ] For every PR, query GitHub's `closingIssuesReferences`/closingIssues relationship.
- [ ] Also parse the PR body for case-insensitive closing keywords `close`, `closes`, `closed`, `fix`, `fixes`, `fixed`, `resolve`, `resolves`, and `resolved`.
- [ ] Accept GitHub-compatible issue references with optional colon and spacing after the keyword, including repository-qualified references when GitHub would treat them as closing syntax.
- [ ] Union and deduplicate issues from the GitHub relationship and body-keyword parser.
- [ ] Compare current PR and issue milestones with the shipped milestone.
- [ ] When the shipped milestone exists, assign each deduplicated shipped PR and linked issue to it.
- [ ] Record PR, linked issue, discovery source, previous milestone, and final milestone.
- [ ] Already correct assignments are no-ops.

#### 12.5 Roll open work and close shipped milestones

- [ ] For every open release milestone with a normalized shipped tag, list open items.
- [ ] Select the next greater open, unshipped milestone.
- [ ] Include a newly created scheduled milestone only after rereading its actual number.
- [ ] Move every open item to the selected target.
- [ ] Reread the shipped milestone and require zero open items.
- [ ] Close the shipped milestone.
- [ ] If no eligible target exists, classify Blocked for a human policy decision; never drop work or close early.

#### 12.6 Dispatch repository follow-up

- [ ] Determine whether release-note generation already reflects the exact shipment.
- [ ] Dispatch release-note generation for the exact package base only when needed.
- [ ] For stable only, determine whether issue-template version choices include the new stable version and dispatch refresh only when needed.
- [ ] Record dispatch receipts and resulting PRs or commits.
- [ ] A dispatch failure does not roll back the shipped release; retry only the missing convergent follow-up.

#### 12.7 Converge reviewed summaries

- [ ] Require deterministic release-note facts to include exact shipment tags and complete PR ranges.
- [ ] Require reviewed prose to pass release-note schema and safety checks.
- [ ] Require deterministic rendered page, TOC, and index.
- [ ] Merge prose only through normal PR review.
- [ ] The #4895 updater selects only exact published releases that already contain valid managed markers.
- [ ] The updater never edits drafts and never adds markers to unmarked releases.
- [ ] Preflight and stage the entire intended batch without writing.
- [ ] Immediately before the first write, reread every staged release body and abort the entire batch with no writes if any body drifted.
- [ ] Replace only the managed-summary region, preserving generated notes and all human-owned bytes.
- [ ] Reread and verify the exact full body after every write; stop later writes on any mismatch.
- [ ] Drafts, missing releases, unmarked releases, omitted prose, and already matching summaries remain untouched.

**Desired end state**

- Public shipped state is exact, scheduled milestones converge where public schedule data is available, shipped PRs/issues are fully attributed, open work is retained, shipped milestones are closed, follow-up is dispatched, and reviewed summaries converge safely.

**Rerun**

- Recompute every public range and authoritative object, then apply only missing convergent mutations. Never widen the release range to consume the post-cut integration tail.

## Recovery matrix

| Observed state | Safe continuation | Block or never do |
| --- | --- | --- |
| Nothing exists for a reviewed release | Revalidate identity, base, version state, and gitlink; begin at step 2 | Do not derive a new version after approval |
| Maintenance exists | Verify intended line and record its current SHA | Never force it to a planned SHA |
| Maintenance missing for first preview | Create once from the approved target-version `preview.0` point | Do not guess from current `main` |
| Maintenance missing for later preview/RC/stable | Continue from approved prerelease lineage and create maintenance separately at audited `preview.0` | Do not create maintenance at the later prerelease tip |
| Matching `mono/skia` branch exists | Require exact gitlink equality and mark Done | Never move it |
| Matching `mono/skia` branch is absent | Create it once at the proven gitlink and reread | Do not use the default-branch tip |
| Exact SkiaSharp branch exists | Verify ancestry, versions, label, and gitlink; mark Done | Never rewrite or force-push it |
| Release branch push raced | Fetch and accept only concurrent exact state | Reject unrelated ancestry or state |
| Stable bump PR is open | Verify exact branch, versions, and base; report human Waiting | Never auto-merge it |
| Stable bump is merged | Verify maintenance is at or beyond expected versions with `preview.0` | Do not open a duplicate |
| Exact public version is unknown for prerelease | Stop Waiting and rerun when externally known | Do not derive a build revision |
| Exact public package is absent, unlisted, or indexing | Stop Waiting and poll only NuGet.org on a later run | Do not query internal systems or republish |
| One family package is absent or unlisted | Stop Waiting and rerun the same public receipt | Do not tag from a partial receipt |
| Family hash/size/ZIP/nuspec fails | Redownload once, preserve evidence, and Block | Never tag or release untrusted bytes |
| Configured-anchor signature fails | Verify role/fingerprint policy and Block for security review | Do not reject solely because a correctly timestamped certificate later expired |
| Exact tag exists at shipped endpoint | Verify peeled target and mark Done | Never recreate it |
| Exact tag exists elsewhere | Preserve evidence and Block for a new-version decision | Never delete or move it |
| Historical prerelease tag has a build revision | Normalize and preserve it | Never rename it or create a cosmetic replacement |
| No GitHub draft exists | Verify tag/source/title/previous tag and create the marked draft | Do not publish directly without body review |
| Matching marked draft exists | Preserve it and continue to body review | Do not erase reviewed summary bytes |
| Compatible markerless draft exists | Preserve every byte while adding owned markers | Do not discard existing content |
| Marked draft has empty generated notes | Regenerate only the generated-notes region | Do not rewrite other regions |
| Draft markers are malformed | Preserve the body and require human repair | Do not guess ownership boundaries |
| Draft changed after approval | Show drift, recompute hash, and obtain new approval | Never publish under stale approval |
| Matching release already published | Verify shipped-state gate and continue step 12 | Do not recreate or rewrite it during recovery |
| Published legacy release targets a branch | Continue with a warning only if exact tag proves source | Do not accept a wrong exact SHA |
| Schedule lookup fails | Warn and continue all other closeout work | Do not clear dates or abort closeout |
| Reconciliation is partly applied | Recompute both first-parent range families and apply missing assignments | Do not omit branch-only work or include post-cut integration work |
| Shipped release milestone is absent | Warn and skip exact-shipment assignment/closure; continue independent dispatch and summary convergence | Do not invent a hotfix/stale milestone or assign to another release without policy |
| Milestone has open work and no target | Preserve the item list and obtain a target decision | Never close or drop items |
| Release-note dispatch failed | Verify shipment remains exact and redispatch only missing work | Do not roll back tag or release |
| Summary batch drifted before first write | Abort with no writes, restage, and rereview | Do not partially apply the stale batch |
| Summary verification fails after a write | Stop later writes and preserve before/after evidence | Do not continue the batch |

## Host adaptation

The host executes this checklist; it does not define release truth. Local shells, GitHub Actions, and Azure Pipelines are interchangeable execution hosts only when they provide the same public inputs, approvals, checks, writes, rereads, and evidence.

### Every host

- [ ] Accepts exact immutable inputs and displays them before mutation.
- [ ] Pins tooling and dependencies.
- [ ] Provides only Git, GitHub, NuGet.org, and public schedule access needed by this checklist.
- [ ] Keeps internal release credentials and identifiers outside the process.
- [ ] Uses operation-scoped write credentials.
- [ ] Verifies an approval gate before loading write credentials.
- [ ] Prevents self-review.
- [ ] Binds approval to exact state and mutation by hash or equivalent.
- [ ] Serializes work by release identity without cancel-in-progress replacement.
- [ ] Uses a clean isolated checkout for source mutations.
- [ ] Rereads remote state after every write.
- [ ] Retains durable public audit evidence.

### Local execution

- Use pinned CLI/API tools and a clean isolated worktree.
- Obtain independent approval through a durable mechanism outside the mutating process.
- Load operation-scoped credentials only after validating that approval.
- Save exact commands, outputs, hashes, and URLs without adding audit artifacts to source commits.
- Local execution does not weaken branch, tag, package, release, or no-self-review rules.

### GitHub Actions execution

- Pin reusable workflows and actions by immutable commit.
- Use an environment or equivalent protected gate configured before the job starts.
- Require at least one reviewer and prevent self-review.
- Bind environment approval to the exact staged operation record, not merely a workflow name.
- Use least-privilege job permissions and short-lived credentials.
- Disable `cancel-in-progress` for release-identity mutation groups.
- Treat artifacts and logs as evidence, never as authoritative state.

### Azure Pipelines execution

- Azure Pipelines is only an execution host here; no Azure build, feed, BAR, promotion, or publication state is in scope.
- Pin repository and template revisions.
- Use an existing protected environment/check that prevents self-review and binds the exact staged operation.
- Verify the gate before retrieving GitHub write credentials.
- Keep internal service connections unavailable to this checklist.
- Serialize by release identity and preserve public evidence.

## Public audit evidence

Retain enough evidence for an independent reviewer to reproduce every public decision:

- Release operator, approvers, actor separation, UTC timestamps, and approval-binding hashes.
- Tooling repository, exact commit, SDK, and locked dependency identities.
- Release identity/type, numeric version, and exact public package version.
- Base, maintenance, exact release, and matching Skia refs with exact SHAs.
- Maintenance creation point and proof of target numeric version at `preview.0`, when applicable.
- Version-file before/after values, file hashes, release commit, and Skia gitlink.
- Stable bump branch, commit, PR URL, review state, and human merge result.
- NuGet.org registration/catalog URLs, listed state, catalog SHA512 and size, downloaded hashes/sizes, and observation times.
- Every family package ZIP/nuspec validation, ID/version, dependencies, repository metadata, source branch, and source commit.
- Configured-anchor author-primary and NuGet.org repository-countersignature structure, role, fingerprint, timestamp, service index, and audit-only certificate validity dates.
- Full historical family derivation and exact HarfBuzzSharp composition/reuse decision.
- Previous tag, normalized identity, and peeled commit without an ancestry assertion.
- Public tag and exact peeled target.
- GitHub draft/release ID, URL, tag, title, target, prerelease state, markers, generated-notes evidence, approved body SHA256, and publication reread.
- Twelve-milestone desired schedule, public schedule warnings, created/updated milestone numbers, and 30-day stale decisions.
- Raw release endpoints, projected integration/main merge-bases, integration first-parent ranges, branch first-parent segments, rolled-forward unshipped work, and excluded post-cut tail.
- Deduplicated PRs/issues, GitHub relationship results, body-keyword matches, prior/final milestones, rollover moves, and closure rereads.
- Release-note and issue-template dispatch receipts and resulting PRs/commits.
- Summary updater batch preflight, drift reread, per-write exact-body verification, skipped drafts/unmarked releases, and final convergence.
- Every Waiting, warning, Blocked conflict, recovery choice, and approved public-policy exception.

The audit record may live in host-native immutable logs or another durable public release record, but it never overrides changed authoritative state and must not contain internal pipeline identifiers or credentials.

## Open public-repository policy decisions

These are genuinely open policy questions, not permission to query the opaque external release process:

1. **Stable bump timing:** decide whether human merge is merely parallel follow-up or a required prerequisite before public publication.
2. **Optional public-package smoke testing:** decide whether it remains advisory or becomes a protected public-repository prerequisite, and if so define minimum host coverage, owner, override authority, and durable evidence.
3. **Reviewed summary timing:** decide whether the GitHub Release may publish with generated notes and converge reviewed summary later, or must wait for reviewed summary prose.
4. **Credential isolation:** define the narrowly scoped GitHub App or equivalent permissions for branch, tag, release, milestone, and dispatch mutations.
5. **Draft read permissions:** document the minimum GitHub token permission needed to inventory drafts without conflating read access with mutation approval.
6. **Dispatch idempotency:** define a durable public receipt or require target workflows to converge harmlessly for repeated exact inputs.
7. **Legacy `target_commitish`:** define when support for historical releases targeting `main` or a source branch can be retired.
8. **Audit retention:** define the canonical durable public location and retention period for the evidence above.
9. **Signature trust policy location:** define the reviewed repository location and update process for role/fingerprint anchors and additive certificate rotation.
10. **Missing shipped milestones:** decide whether patch/hotfix and more-than-30-days-late releases should create an exact shipped milestone on demand, reuse another milestone, or permanently skip exact-shipment assignment. Until decided, absence is Warning/Skipped and never permits guessed assignment.

Until resolved, apply the conservative behavior stated in the ordered checklist and record the decision as open; do not invent internal prerequisites.

## Non-normative source and history notes

This section explains provenance only. It does not add release gates, restore old phase architecture, or authorize use of internal systems.

- Earlier release documentation and retired release skills informed branch safety, exact source identity, partial-state recovery, and human approval intent.
- Old release-status and internal-package testing skills cover the opaque external organizational gap and are out of scope/superseded by this public repository checklist.
- Their build/test definitions, run identities, feed topology, publication queue interfaces, and pipeline credentials are intentionally not part of this document.
- Historical `-stable` package assumptions are retired; current stable public identity is bare `X.Y.Z` or `X.Y.Z.F`.
- New prerelease tags omit build revision, while transition-aware parsers continue to accept immutable historical one-part and two-part revision tags.
- The #4895 summary updater supplies the normative published-only, preflight-all, batch-drift-barrier, managed-region-only, and post-write-verification behavior captured in step 12.
- `documentation/dev/releasing.md`, `documentation/dev/versioning.md`, release tooling and tests, historical branches/tags, and PR #4895 are implementation/history references rather than alternative checklists.
- `update-skia`, native-dependency update, and security-audit processes prepare product source before a release identity is frozen; they are upstream change-management work, not steps in this checklist.

## Illustrative C# checklist definition

This sketch is non-normative and does not prescribe the final SDK API. It illustrates how one host-independent C# definition could compose reusable Git, GitHub, and NuGet primitives with a small amount of SkiaSharp-specific policy code. Handles represent lazily evaluated, compiler-typed values or step outputs; dependencies determine evaluation order without requiring every top-level item to be created at once.

```csharp
using NuGet.Versioning;
using ReleaseChecklist;
using ReleaseChecklist.Git;
using ReleaseChecklist.GitHub;
using ReleaseChecklist.NuGet;

var builder = ReleaseChecklistApplication.CreateBuilder(args);

// Inputs are host-independent. Inspecting requires no write capability.
var releaseIdentity = builder.AddRequiredInput(
    "release",
    SkiaSharpVersions.ParseReleaseIdentity);

var releaseBase = builder.AddRequiredInput(
    "base",
    GitReference.Parse);

var approvedBaseSha = builder.AddRequiredInput(
    "base-sha",
    GitCommit.Parse);

var maintenanceBase = builder.AddOptionalInput(
    "maintenance-base",
    GitReference.Parse);

var maintenanceBaseSha = builder.AddOptionalInput(
    "maintenance-base-sha",
    GitCommit.Parse);

// A prerelease public version is intentionally unavailable during the first run.
// Stable can derive its public version from the release identity.
var suppliedPublicVersion = builder.AddOptionalInput(
    "public-version",
    NuGetVersion.Parse);

var publicVersion = builder.AddDerivedValue(
    "exact-public-version",
    context => SkiaSharpVersions.ResolvePublicVersion(
        releaseIdentity.Get(context),
        suppliedPublicVersion.GetOptional(context)));

var skiasharpGit = builder.AddGitRepository(
    "skiasharp-git",
    workingDirectory: builder.RepositoryRoot,
    remote: "origin");

var skiasharpGitHub = builder.AddGitHubRepository(
    "skiasharp-github",
    owner: "mono",
    name: "SkiaSharp");

var skiaGitHub = builder.AddGitHubRepository(
    "skia-github",
    owner: "mono",
    name: "skia");

var nugetOrg = builder.AddNuGetSource(
    "nuget-org",
    "https://api.nuget.org/v3/index.json");

var releaseBranchName = releaseIdentity.Select(
    value => GitBranchName.Parse($"release/{value}"));

var maintenanceBranchName = releaseIdentity.Select(
    value => GitBranchName.Parse($"release/{value.Major}.{value.Minor}.x"));

var releaseTagName = releaseIdentity.Select(
    value => GitTagName.Parse($"v{value}"));

// Top-level 1: establish public repository source state.
var prepare = builder.AddGroup("prepare-repository");

var inputs = prepare.AddCheck(
    "validate-release-inputs",
    async context =>
    {
        var identity = releaseIdentity.Get(context);
        var baseRef = releaseBase.Get(context);
        var baseSha = approvedBaseSha.Get(context);

        await skiasharpGit.RequireRefAtAsync(baseRef, baseSha, context.CancellationToken);
        await SkiaSharpPolicy.RequireValidReleaseBaseAsync(
            skiasharpGit,
            identity,
            baseRef,
            baseSha,
            maintenanceBase.GetOptional(context),
            context.CancellationToken);
        await SkiaSharpPolicy.RequireMaintenanceBaseAsync(
            skiasharpGit,
            identity,
            maintenanceBase.GetOptional(context),
            maintenanceBaseSha.GetOptional(context),
            context.CancellationToken);

        return CheckResult.Done(
            $"Release {identity} will use {baseRef} at {baseSha}.");
    });

var maintenanceBranch = prepare.AddGitBranch(
        "maintenance-branch",
        repository: skiasharpGit,
        name: maintenanceBranchName,
        target: context => SkiaSharpPolicy.ResolveMaintenanceCreationCommit(
            context,
            releaseIdentity,
            releaseBase,
            approvedBaseSha,
            maintenanceBase,
            maintenanceBaseSha))
    .AcceptExistingWhen((context, actual) =>
        SkiaSharpPolicy.IsValidMaintenanceBranchAsync(
            context,
            actual,
            releaseIdentity,
            approvedBaseSha))
    .When(context => SkiaSharpPolicy.RequiresMaintenanceBranch(
        releaseIdentity.Get(context)))
    .DependsOn(inputs)
    .RequiresCapability(ReleaseCapability.Branching);

var skiaCommit = prepare.AddDerivedValue(
    "skia-gitlink",
    async context => await skiasharpGit.ReadGitlinkAsync(
        approvedBaseSha.Get(context),
        "externals/skia",
        context.CancellationToken))
    .DependsOn(inputs);

var skiaReleaseBranch = prepare.AddGitHubBranch(
        "skia-release-branch",
        repository: skiaGitHub,
        name: releaseBranchName,
        target: skiaCommit)
    .DependsOn(inputs)
    .RequiresCapability(ReleaseCapability.Branching);

var skiasharpReleaseBranch = prepare.AddGitBranch(
        "skiasharp-release-branch",
        repository: skiasharpGit,
        name: releaseBranchName,
        target: approvedBaseSha,
        configureCommit: async context =>
        {
            await SkiaSharpVersionFiles.SetReleaseIdentityAsync(
                context.Worktree,
                releaseIdentity.Get(context),
                context.CancellationToken);

            return GitCommitDescription.Create(
                $"Bump the version to {releaseIdentity.Get(context)}",
                trailers:
                [
                    $"Release-Base: {approvedBaseSha.Get(context)}",
                    $"Release-Skia: {skiaCommit.Get(context)}",
                ]);
        })
    .AcceptExistingWhen((context, actual) =>
        SkiaSharpPolicy.IsValidExistingReleaseBranchAsync(
            context,
            actual,
            releaseIdentity,
            approvedBaseSha,
            skiaCommit))
    .DependsOn(skiaReleaseBranch)
    .RequiresCapability(ReleaseCapability.Branching);

// The bump PR is a sibling follow-up. Its human merge is observable but is not
// automatically made a prerequisite of the public NuGet boundary.
var stableBump = prepare.AddGroup("stable-bump")
    .When(context => SkiaSharpPolicy.RequiresStableBump(
        releaseIdentity.Get(context)));

var nextVersion = stableBump.AddDerivedValue(
    "next-version",
    context => SkiaSharpVersions.NextDevelopmentVersion(
        releaseIdentity.Get(context),
        skiasharpGit.ReadVersions(maintenanceBranchName.Get(context))))
    .DependsOn(maintenanceBranch, skiasharpReleaseBranch);

var bumpBranch = stableBump.AddGitBranch(
        "stable-bump-branch",
        repository: skiasharpGit,
        name: nextVersion.Select(value => GitBranchName.Parse($"bump-version-{value.SkiaSharp}")),
        target: maintenanceBranchName,
        configureCommit: context => SkiaSharpVersionFiles.SetNextPreviewAsync(
            context.Worktree,
            nextVersion.Get(context),
            context.CancellationToken))
    .AcceptExistingWhen((context, actual) =>
        SkiaSharpPolicy.IsValidStableBumpBranchAsync(
            context,
            actual,
            nextVersion,
            maintenanceBranchName))
    .DependsOn(maintenanceBranch, skiasharpReleaseBranch)
    .RequiresCapability(ReleaseCapability.Branching);

var bumpPullRequest = stableBump.AddGitHubPullRequest(
        "stable-bump-pull-request",
        repository: skiasharpGitHub,
        head: bumpBranch,
        @base: maintenanceBranchName,
        title: nextVersion.Select(value => $"Bump to {value.SkiaSharp} after release"))
    .DependsOn(bumpBranch)
    .RequiresCapability(ReleaseCapability.Branching);

stableBump.AddWait(
        "stable-bump-human-merge",
        until: context => SkiaSharpPolicy.StableBumpHasMergedAsync(
            context,
            nextVersion,
            maintenanceBranchName))
    .DependsOn(bumpPullRequest);

// Top-level 2: the only opaque external boundary.
var publicBoundary = builder.AddGroup("public-nuget-boundary")
    .DependsOn(skiasharpReleaseBranch);

var publicVersionKnown = publicBoundary.AddWait(
    "exact-public-version-known",
    until: context => publicVersion.TryGet(context) is not null,
    waitingMessage: "Supply --public-version after the external release process publishes the exact package.");

var skiasharpPackage = publicBoundary.AddNuGetPackage(
        "skiasharp-package",
        source: nugetOrg,
        packageId: "SkiaSharp",
        version: publicVersion)
    .WaitUntilListed()
    .DependsOn(publicVersionKnown);

var publicReceipt = publicBoundary.AddNuGetReceipt(
        "complete-public-receipt",
        primaryPackage: skiasharpPackage,
        configure: receipt =>
        {
            receipt.RequireCatalogHash("SHA512");
            receipt.RequireRepositoryType("git");
            receipt.RequireSourceBranch(releaseBranchName);
            receipt.RequireSourceCommitContainedBy(skiasharpGit, releaseBranchName);
            receipt.AddFamilyFromFile(
                skiasharpGit,
                sourceCommit: receipt.SourceCommit,
                path: "scripts/VERSIONS.txt",
                configure: SkiaSharpPackages.ConfigureHistoricalFamily);
            receipt.AddSignatureAnchor("SkiaSharp");
            receipt.AddSignatureAnchor("SkiaSharp.HarfBuzz");
            receipt.AddSignatureAnchor("HarfBuzzSharp");
            receipt.UseTrustedCertificates("scripts/infra/release/trusted-signing-certificates.json");
        })
    .DependsOn(skiasharpPackage);

// Top-level 3: publish the public GitHub repository release.
var publish = builder.AddGroup("publish-github-release")
    .DependsOn(publicReceipt);

var currentTag = publish.AddGitTagForIdentity(
        "current-release-tag",
        repository: skiasharpGit,
        identity: releaseIdentity,
        preferredName: releaseTagName,
        normalize: SkiaSharpTags.NormalizeHistoricalOrCurrent,
        target: publicReceipt.SourceCommit)
    .AcceptHistoricalNameWhenTargetMatches()
    .BlockOnMultipleNamesForSameIdentity()
    .DependsOn(publicReceipt);

var previousTag = publish.AddGitTagSelection(
        "previous-release-tag",
        repository: skiasharpGit,
        currentIdentity: releaseIdentity,
        normalize: SkiaSharpTags.NormalizeHistoricalOrCurrent,
        excludeCurrentIdentity: true);

var releaseTag = publish.AddGitTag(
        "release-tag",
        repository: skiasharpGit,
        name: currentTag.EffectiveName,
        target: publicReceipt.SourceCommit)
    .CreatePreferredNameOnlyWhenIdentityHasNoExistingTag(releaseTagName)
    .DependsOn(currentTag)
    .RequiresCapability(ReleaseCapability.Tag);

var releaseDraft = publish.AddGitHubRelease(
        "release-draft",
        repository: skiasharpGitHub,
        tag: currentTag.EffectiveName,
        target: publicReceipt.SourceCommit,
        title: releaseIdentity.Select(SkiaSharpVersions.ReleaseTitle),
        prerelease: releaseIdentity.Select(value => value.IsPrerelease),
        body: context => SkiaSharpReleaseBody.CreateOrMigrateAsync(
            context,
            previousTag,
            publicReceipt))
    .AsDraft()
    .DependsOn(releaseTag, previousTag)
    .RequiresCapability(ReleaseCapability.Draft);

var publicationObservation = publish.AddObservation(
        "publication-observation",
        tag: currentTag.EffectiveName,
        releaseId: releaseDraft.Id,
        targetCommit: publicReceipt.SourceCommit,
        title: releaseIdentity.Select(SkiaSharpVersions.ReleaseTitle),
        prerelease: releaseIdentity.Select(value => value.IsPrerelease),
        bodySha256: releaseDraft.BodySha256,
        observedAt: builder.Clock.UtcNow,
        mutation: ReleaseMutation.PublishGitHubRelease)
    .DependsOn(releaseDraft);

var publishedRelease = publish.AddGitHubReleasePublication(
        "publish-release",
        release: releaseDraft,
        expected: publicationObservation)
    .DependsOn(publicationObservation)
    .RequiresCapability(ReleaseCapability.Publish);

// Top-level 4: converge public repository state after publication.
var closeout = builder.AddGroup("closeout")
    .DependsOn(publishedRelease);

var schedules = closeout.AddPublicSchedule(
        "scheduled-milestones",
        source: ChromiumSchedule.Public,
        milestones: context => SkiaSharpMilestones.CurrentAndNextTwo(context))
    .Expand(SkiaSharpMilestones.CreatePreviewRcStableDefinitions)
    .CreateOrUpdateGitHubMilestones(skiasharpGitHub, staleCreateCutoff: TimeSpan.FromDays(30))
    .RequiresCapability(ReleaseCapability.Closeout);

var shippedChanges = closeout.AddCheck(
        "shipped-change-range",
        context => SkiaSharpReconciliation.ResolveCompleteRangeAsync(
            context,
            skiasharpGit,
            releaseIdentity,
            publicReceipt.SourceCommit,
            previousTag))
    .DependsOn(publishedRelease);

var assignments = closeout.AddGitHubMilestoneAssignments(
        "reconcile-pull-requests-and-issues",
        repository: skiasharpGitHub,
        milestone: releaseIdentity.Select(value => value.ToString()),
        items: shippedChanges.Select(SkiaSharpReconciliation.FindPullRequestsAndClosingIssues))
    .WhenMilestoneExists(otherwise: StepStatus.Skipped)
    .DependsOn(shippedChanges, schedules)
    .RequiresCapability(ReleaseCapability.Closeout);

var closedMilestones = closeout.AddGitHubMilestoneRollover(
        "rollover-and-close-shipped-milestones",
        repository: skiasharpGitHub,
        tagNormalizer: SkiaSharpTags.NormalizeHistoricalOrCurrent)
    .DependsOn(assignments, schedules)
    .RequiresCapability(ReleaseCapability.Closeout);

var releaseNotes = closeout.AddGitHubWorkflowDispatch(
        "release-notes",
        repository: skiasharpGitHub,
        workflow: "update-release-notes.lock.yml",
        @ref: "main",
        inputs: context => SkiaSharpReleaseNotes.DispatchInputs(
            releaseIdentity.Get(context)))
    .SatisfiedWhen(context => SkiaSharpReleaseNotes.ExactShipmentIsQueuedOrRenderedAsync(
        context,
        releaseIdentity))
    .RecordReceipt(context => SkiaSharpReleaseNotes.FindDispatchReceiptAsync(
        context,
        releaseIdentity))
    .DependsOn(publishedRelease)
    .RequiresCapability(ReleaseCapability.Closeout);

var issueTemplate = closeout.AddGitHubWorkflowDispatch(
        "issue-template",
        repository: skiasharpGitHub,
        workflow: "auto-update-issue-template-versions.yml",
        @ref: "main")
    .When(context => !releaseIdentity.Get(context).IsPrerelease)
    .SatisfiedWhen(context => SkiaSharpIssueTemplates.ContainsReleaseAsync(
        context,
        releaseIdentity))
    .RecordReceipt(context => SkiaSharpIssueTemplates.FindDispatchReceiptAsync(
        context,
        releaseIdentity))
    .DependsOn(publishedRelease)
    .RequiresCapability(ReleaseCapability.Closeout);

var reviewedSummary = closeout.AddStep(
        "reviewed-release-summary",
        step => step
            .Check(context => SkiaSharpReleaseNotes.CheckReviewedSummaryAsync(
                context,
                currentTag,
                releaseIdentity))
            .Apply(context => SkiaSharpReleaseNotes.ApplyReviewedSummaryAsync(
                context,
                currentTag,
                releaseIdentity))
            .RequiresCapability(ReleaseCapability.Closeout))
    .DependsOn(releaseNotes);

closeout.AddCompletion(
    "repository-release-complete",
    dependencies:
    [
        closedMilestones,
        releaseNotes,
        issueTemplate,
        reviewedSummary,
    ]);

return await builder.RunAsync();
```

The SDK implied by this sketch has a deliberately small execution model:

- `AddGroup` and `DependsOn` define hierarchy and ordering.
- Waiting or Blocked state prevents only dependent descendants from running; independent sibling branches continue. The public NuGet boundary depends on `skiasharp-release-branch`, not `stable-bump-human-merge`.
- `AddWait` represents an external or human-owned prerequisite without inventing persisted state.
- Git, GitHub, and NuGet methods are reusable primitives that own existence checks, safe create-if-absent actions, conflicts, retries, and post-action rereads.
- `Check` and callback parameters are escape hatches for SkiaSharp policy that cannot be generalized honestly.
- `AcceptExistingWhen` lets a reusable create-if-absent primitive accept a safely advanced existing branch without weakening its default exact-target conflict rule.
- `RequiresCapability` prevents mutation unless the execution host supplies the explicitly approved capability.
- Capabilities are operation-scoped: branch creation, tag creation, draft mutation, publication, and closeout are distinct grants. A host may approve multiple exact staged operations together, but `--apply tag` cannot mutate a draft.
- The publication observation binds tag, release ID, target commit, title, prerelease state, exact body SHA256, observation time, and the single publish mutation; the capability grant separately records actor/approver separation.
- A run without capabilities is inspection-only.
- A run with a capability recomputes all dependencies from authoritative state, verifies the approved observation, performs only Ready actions covered by that capability, and checks convergence afterward.
- `StepStatus.Done`, `Ready`, `Waiting`, `Blocked`, and `Skipped` are sufficient; the repository/GitHub/NuGet state remains the durable state machine.
- The external organizational build, manual testing, and package publication process is represented only by `exact-public-version-known` and `skiasharp-package`; no internal client or identifier appears in the definition.
- Building the definition fails immediately for duplicate IDs, unresolved handles, dependency cycles, a child depending on a later impossible parent, a mutating step without a capability, or an action without a check/post-check contract.

For example, an inspection-only local run resuming the existing RC1 branch could remain minimal:

```bash
dotnet run --project utils/SkiaSharp.ReleaseChecklist -- \
  --release 4.152.0-rc.1 \
  --base refs/remotes/origin/release/4.152.0-rc.1 \
  --base-sha 2357692e1e0fb1d3dc742e74fad4682adf5d4dec
```

After the opaque external process publishes packages, the same definition resumes by adding the exact public version:

```bash
dotnet run --project utils/SkiaSharp.ReleaseChecklist -- \
  --release 4.152.0-rc.1 \
  --base refs/remotes/origin/release/4.152.0-rc.1 \
  --base-sha 2357692e1e0fb1d3dc742e74fad4682adf5d4dec \
  --public-version 4.152.0-rc.1.26426.14
```

A mutating host supplies one reviewed capability and the immutable observation it approved:

```bash
dotnet run --project utils/SkiaSharp.ReleaseChecklist -- \
  ...same exact inputs... \
  --apply tag \
  --expect-observation-sha256 0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef
```
