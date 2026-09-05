# TSA Azure Boards Work Items

Trust Services Automation (TSA) is existing legacy infrastructure for SkiaSharp security and
compliance findings. Retain and audit it for now. The authoritative routing moved with SkiaSharp
CI to dnceng; do not query the obsolete DevDiv codebase history. This workflow does **not** migrate
TSA data or ownership to WiM.

## Required Query

Every security audit must run the read-only query once and cache the full result:

```bash
python3 .agents/skills/security-audit/scripts/query-tsa-work-items.py \
  --output output/ai/tsa-work-items-cache.json
```

The script uses the current `az` authentication against:

- Organization: `https://dev.azure.com/dnceng`
- Project: `internal`
- Area: `internal\Dotnet-Core-Engineering`
- Iteration: `internal`
- Narrow tag: `TSA-skiasharp.skiasharp_main`

Do not query the broad SkiaSharp area. It has more than 1,000 work items and may time out. The
codebase tag is the stable TSA routing key. Include active and resolved history when dnceng has
records; do not merge obsolete DevDiv records into the authoritative result.

The script writes an error-shaped cache and exits nonzero if Azure Boards authentication, WIQL, or
complete-record hydration fails. Never replace a failed query with an empty successful result.
Reports with `queryStatus != "success"` fail semantic validation. A successful dnceng response
with zero records is legitimate during migration and is recorded as `emptyResult: true`.

## Evidence and State

The cache preserves every work item, including:

- ID, title, state, type, severity/priority, tags
- Area and iteration paths, owner, created/changed dates, portal URL
- Derived TSA category, tool, rule IDs, impacted file, and occurrence-level deduplication key
- Normalized triage evidence where present, including repro, risk, exception, and mitigation data
- Every field returned by the complete-record API in `rawFields`

`activity` separates currently actionable records from historical records:

- `active`: work remains actionable
- `historical`: known terminal states such as resolved, closed, done, removed, or rejected

Treat all other states as active. This avoids hiding actionable work when a team uses a custom
nonterminal state such as `To Do`.

Historical records remain important for deduplication and suppression decisions. Do not discard
them. The `groups` array preserves active and historical IDs for each tool/rule deduplication key.

## Correlate with Audit Findings

After assembling the base security audit JSON, load and correlate the cache:

```bash
python3 .agents/skills/security-audit/scripts/correlate-tsa-work-items.py \
  --report output/ai/security-audit-{date}.json \
  --tsa-cache output/ai/tsa-work-items-cache.json
```

The correlator matches exact CVE/GHSA identifiers first, then conservative dependency/component
text across titles, tags, normalized evidence, and complete raw fields. Every work item receives a
correlation object. Unmatched records remain in `items[]` with `correlation.status: "unmatched"`;
unmatched does not mean irrelevant or safe.

## Portal Links

- [SkiaSharp TSA work-item search](https://almsearch.dev.azure.com/dnceng/internal/_search?type=workitem&text=TSA-skiasharp.skiasharp_main)
- Individual items: `https://dev.azure.com/dnceng/internal/_workitems/edit/{id}`
