# Curated release facts for <version>

Use this sidecar for important facts that PR titles and generated API diffs do
not explain clearly. Write evidence, not polished changelog prose. Delete empty
sections.

## Security

### <Security change title>

**Release-note blurb:** <One sentence the Polish agent can adapt directly.>

**Evidence:** <Advisory/CVE IDs, shipped fix commits, and PR links.>

**Scope:** <What shipped and any important reachability or wording limits.>

**Recommended action:** <Only when consumers must act; otherwise “None.”>

## Breaking changes

### <Breaking change title>

**Compatibility:** <Behavioral | Source | Binary | Source and binary>

**Release-note blurb:** <One concise sentence stating the change and impact.>

**Affected APIs:** <Types/members/packages/targets, or “None.”>

#### Previous behavior

<What existing applications observed before this release.>

#### New behavior

<What applications observe after upgrading. Include a small snippet when it
clarifies the change.>

#### Reason

<Why the change was necessary.>

#### Recommended action

<Concrete migration, workaround, or replacement API. Include a small before/
after snippet when useful.>
