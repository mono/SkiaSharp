# Workaround Search Strategy

Systematic procedure for finding workarounds during triage. Called from [research-by-type.md](research-by-type.md) after classifying the issue type.

**Goal:** Find something the reporter can use RIGHT NOW — even if a proper fix is needed later.

---

## Source Priority Order

Search in this order. Stop as soon as you find a viable workaround, but always check sources 1–3.

| Priority | Source | Why first | When to skip |
|----------|--------|-----------|-------------|
| 1 | Existing triages (`$CACHE/ai-triage/`) | Already analyzed, has `workaroundSummary` | Never — always check |
| 2 | Closed issues with comments (`$CACHE/github/items/`) | Reporters post "I solved it by..." | Never — always check |
| 3 | Known patterns (`references/skia-patterns.md`, `documentation/packages.md`) | Curated heuristics for common traps | Never — always check |
| 4 | SkiaSharp source code (`binding/SkiaSharp/*.cs`) | Alternative APIs visible in the class | Skip if issue is deployment/packaging |
| 5 | API docs (`docs/SkiaSharpAPI/SkiaSharp/*.xml`) | Method docs mention alternatives | Skip if issue is deployment/packaging |
| 6 | Tutorials (`.docs/docs/docs/`) | Step-by-step examples of correct usage | Skip if not a usage/how-to issue |
| 7 | Samples (`samples/`) | Working code for specific platforms | Skip if not a platform integration issue |
| 8 | GitHub closed issues (API fallback) | Broader search than local cache | Only if cache search found nothing |
| 9 | Web search (Stack Overflow, MS Learn) | Community solutions, framework-side fixes | Only if all local sources exhausted |

---

## Step 1 — Extract Search Terms

From the issue body, title, and stack traces, extract:
- **API/class names:** `SKCanvas`, `SKImage`, `DrawText`, `FromEncodedData`
- **Error messages:** `DllNotFoundException`, `AccessViolationException`, `EntryPointNotFoundException`
- **Platform/framework:** Android, iOS, WASM, Docker, MAUI, Blazor, Avalonia
- **Package/version:** `NativeAssets.Linux`, `NoDependencies`, `3.x`, `2.88.x`

Use both C# names (`SKImage`) and C API names (`sk_image`) since issues may reference either.

---

## Step 2 — Search Existing Triages

```bash
# Find triages with workarounds
grep -rl '"hasWorkaround": true' $CACHE/ai-triage/

# Keyword search across triage files
grep -li "SKImage\|FromEncoded\|DllNotFound" $CACHE/ai-triage/*.json

# Extract workaround details from matching triages
python3 -c "
import json, glob
for f in glob.glob('$CACHE/ai-triage/*.json'):
    with open(f) as fh:
        d = json.load(fh)
    blob = json.dumps(d).lower()
    if not any(kw in blob for kw in ['KEYWORD1', 'KEYWORD2']):
        continue
    bs = d.get('evidence',{}).get('bugSignals') or {}
    res = d.get('analysis',{}).get('resolution') or {}
    print(f'#{d[\"meta\"][\"number\"]}: {d.get(\"summary\",\"\")}')
    if bs.get('workaroundSummary'):
        print(f'  Workaround: {bs[\"workaroundSummary\"]}')
    for p in (res.get('proposals') or []):
        print(f'  Proposal: {p[\"title\"]} - {p[\"description\"][:120]}')
    print()
"
```

**Key JSON paths:** `evidence.bugSignals.hasWorkaround` (bool), `evidence.bugSignals.workaroundSummary` (text), `analysis.resolution.proposals[]` (solutions), `analysis.resolution.recommendedProposal` (suggested pick).

---

## Step 3 — Search Closed Issues

Cached issue JSON has comments at `engagement.comments[]` (each with `author`, `body`, `createdAt`).

```bash
# Fast keyword scan
grep -rli "workaround\|solved\|fixed it\|resolved.*by" $CACHE/github/items/ | head -20
```

```python
import json, glob

KEYWORDS = ['KEYWORD1', 'KEYWORD2']
SIGNALS = ['workaround', 'solved', 'fixed it', 'i resolved', 'as a workaround',
           'the fix is', 'what worked', 'try using', 'instead of', 'turned out']

for f in sorted(glob.glob('$CACHE/github/items/*.json')):
    with open(f) as fh:
        d = json.load(fh)
    if d.get('state') != 'closed':
        continue
    blob = (d.get('title','') + ' ' + (d.get('body') or '')).lower()
    if not any(kw.lower() in blob for kw in KEYWORDS):
        continue
    for c in d.get('engagement',{}).get('comments', []):
        body = (c.get('body') or '').lower()
        if any(sig in body for sig in SIGNALS):
            print(f"#{d['number']}: {d['title'][:60]}")
            print(f"  By: {c.get('author','?')}  {c.get('body','')[:300]}\n")
```

**What to look for:** OP says "I solved it by..." (high-value), maintainer suggests alternative (high-value), "closing because fixed in vX.Y.Z" (upgrade workaround), "this is by design" (no workaround — clarify usage).

**GitHub API fallback** (when cache misses):

```bash
gh search issues "SKImage FromEncoded crash" --repo mono/SkiaSharp --state closed --limit 10
gh issue view {N} --repo mono/SkiaSharp --json comments \
  --jq '.comments[] | select(.body | test("workaround|solved|fixed"; "i")) | {author: .author.login, body: .body[:300]}'
```

---

## Step 4 — Check Known Patterns

```bash
grep -n "DllNotFound\|NoDependencies\|container\|Docker\|Alpine\|ARM64" documentation/packages.md
grep -n "KEYWORD" .github/skills/triage-issue/references/skia-patterns.md
```

**Common instant workarounds:**
- DllNotFoundException in container → `NativeAssets.Linux` + install fontconfig
- DllNotFoundException in publish → add direct PackageReference in app project
- Alpine → use `linux-musl-*` RID
- EntryPointNotFoundException → version mismatch, align managed + native versions
- AccessViolationException → check disposal patterns, threading

---

## Step 5 — Search Source Code

```bash
# Public methods on a class (find alternatives)
grep -n "public " binding/SkiaSharp/{CLASS}.cs | grep -v "private\|internal\|protected" | head -30

# Factory methods (alternative construction)
grep -n "public static" binding/SkiaSharp/{CLASS}.cs

# Find related implementations across classes
grep -rln "{METHOD}\|{CONCEPT}" binding/SkiaSharp/*.cs
```

### Common API-level workarounds

| Problem | Alternative |
|---------|-------------|
| `SKBitmap` memory issues | Use `SKImage` (immutable, shareable) |
| `SKCanvas.DrawText` limitations | Use `SKShaper` (HarfBuzz) for complex text |
| GPU surface issues | Fall back to CPU: `SKSurface.Create()` instead of `SKSurface.CreateAsRenderTarget()` |
| `SKTypeface` not finding fonts | Use `SKTypeface.FromFile()` or `SKTypeface.FromData()` directly |
| Color space problems | Explicitly set `SKColorSpace.CreateSrgb()` |
| `SKImage.FromEncodedData` returns null | Check codec with `SKCodec.Create()` first |

---

## Step 6 — Search API Docs

```bash
grep -rn "FromEncodedData\|FromEncoded" docs/SkiaSharpAPI/SkiaSharp/{CLASS}.xml
grep -rln "thread\|dispose\|alternative" docs/SkiaSharpAPI/SkiaSharp/
```

Look for: `<remarks>` mentioning alternatives, `<returns>` mentioning null (factory-null-on-failure), `<see cref="..."/>` cross-references.

---

## Step 7 — Search Tutorials and Samples

```bash
grep -rln "KEYWORD" .docs/docs/docs/
grep -rln "KEYWORD" samples/ --include="*.cs" | head -10
```

---

## Step 8 — Web Search (Last Resort)

Use only when local sources (steps 1–7) are exhausted. Target framework-interaction issues, .NET runtime errors, or platform SDK problems.

```
web_search: "SkiaSharp {ERROR_MESSAGE} workaround site:stackoverflow.com"
mslearn/microsoft_docs_search: "{FRAMEWORK} SkiaSharp {PROBLEM}"
```

Trust: GitHub issues (high), MS Learn (high), SO accepted answers (medium-high), blog posts (check version), AI answers (low — often hallucinate APIs).

---

## Search Patterns by Issue Type

**Crash/exception:** triages for same exception → cached issues for error message → `skia-patterns.md` common traps → source code for the method in stack trace. *Workaround: null check, try-catch, or alternative API.*

**Wrong output/rendering:** triages for the API → tutorials for correct usage → source for parameter validation → samples for working examples. *Workaround: minimal working example showing correct approach.*

**Deployment/DllNotFoundException:** `documentation/packages.md` FIRST (almost always has the answer) → `skia-patterns.md` native loading → cached issues for same platform combo. *Workaround: package swap, direct PackageReference, or system dependency install.*

**Question (how to do X):** tutorials → API docs → samples → cached closed issues (many questions repeat) → web search MS Learn. *Workaround: assemble code snippet from docs + source.*

**Feature request (API doesn't exist):** source code for partial solutions → Skia C++ headers for upstream capability → triages for similar requests. *Workaround: show existing APIs that achieve the result, even if verbose.*

---

## Constructing a Workaround from Scratch

When no existing workaround is found:

1. **Identify failure boundary** — What specific call fails? API call itself, or setup/teardown?
2. **Find nearest working path:**
   ```bash
   grep "public " binding/SkiaSharp/{CLASS}.cs | grep -v "private\|internal\|protected"
   grep "public static" binding/SkiaSharp/{CLASS}.cs
   grep -rln "{METHOD}\|{CONCEPT}" binding/SkiaSharp/*.cs
   ```
3. **Draft the workaround** — format as: one-sentence summary, copy-pasteable code, limitations, and what a proper fix would look like.
4. **Validate** — Does it avoid the crash path? Produce correct output? Work on the reporter's platform? Reasonable burden?
