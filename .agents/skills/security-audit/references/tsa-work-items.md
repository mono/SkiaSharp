# TSA Azure Boards Work Items

Trust Services Automation (TSA) is existing legacy infrastructure for SkiaSharp security and
compliance findings. Retain and audit it for now. This workflow does **not** migrate TSA data or
ownership to WiM.

## Required Query

Every security audit must run the read-only query once and cache the full result:

```bash
python3 .agents/skills/security-audit/scripts/query-tsa-work-items.py \
  --output output/ai/tsa-work-items-cache.json
```

The script uses the current `az` authentication against:

- Organization: `https://dev.azure.com/devdiv`
- Project: `DevDiv`
- Narrow tag: `TSA-skiasharp.skiasharp_main`

Do not query the broad SkiaSharp area. It has more than 1,000 work items and may time out. The
codebase tag is the stable TSA routing key and includes both active and resolved history.

The script writes an error-shaped cache and exits nonzero if Azure Boards fails. Never replace a
failed query with an empty successful result. Reports with `queryStatus != "success"` fail semantic
validation.

## Evidence and State

The cache preserves every work item, including:

- ID, title, state, type, severity/priority, tags
- Area and iteration paths, owner, created/changed dates, portal URL
- Derived TSA category, tool, rule IDs, and deduplication key
- The selected Azure Boards fields in `rawFields`

`activity` separates currently actionable records from historical records:

- `active`: work remains actionable
- `historical`: resolved, closed, removed, or otherwise non-active history

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
text. Every work item receives a correlation object. Unmatched records remain in `items[]` with
`correlation.status: "unmatched"`; unmatched does not mean irrelevant or safe.

## Portal Links

- [SkiaSharp TSA work-item search](https://almsearch.dev.azure.com/devdiv/DevDiv/_search?type=workitem&text=TSA-skiasharp.skiasharp_main)
- Individual items: `https://dev.azure.com/devdiv/DevDiv/_workitems/edit/{id}`
