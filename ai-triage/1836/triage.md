# Issue Triage Report — #1836

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T22:22:16Z |
| Type | type/question (0.85 (85%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** Reporter expects SKTypeface.OpenStream() on a typeface loaded from a TTC (TrueType Collection) file to return only the stream for the specific TTF inside the collection, but it returns the entire TTC file stream; the correct approach is to use the OpenStream(out int ttcIndex) overload and pass ttcIndex to FromStream.

**Analysis:** SKTypeface.OpenStream() is designed to return the full TTC file stream along with a ttcIndex indicating which font in the collection this typeface represents. The reporter is not using the ttcIndex overload and is also ignoring the index when calling FromStream, so they receive the wrong font. The solution is to use OpenStream(out int ttcIndex) and pass ttcIndex to SKTypeface.FromStream(stream, ttcIndex).

**Recommendations:** **close-as-not-a-bug** — OpenStream behavior for TTC fonts is by design. The fix is to use OpenStream(out int ttcIndex) + FromStream(stream, ttcIndex). Two independent users confirm the same usage error.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** SkiaSharp 2.80.3, Visual Studio 2019

**Code snippets:**

```csharp
SKTypeface typeface = SKTypeface.FromFamilyName("GungSuhChe"); SKStreamAsset fontStream = typeface.OpenStream(); byte[] fontData = new byte[fontStream.Length - 1]; fontStream.Read(fontData, fontStream.Length); var stream = new MemoryStream(fontData); stream.Position = 0; SKTypeface typeface2 = SKTypeface.FromStream(stream);
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKTypeface.OpenStream behavior has not changed — by design it returns the TTC container stream plus a ttcIndex. |

## Analysis

### Technical Summary

SKTypeface.OpenStream() is designed to return the full TTC file stream along with a ttcIndex indicating which font in the collection this typeface represents. The reporter is not using the ttcIndex overload and is also ignoring the index when calling FromStream, so they receive the wrong font. The solution is to use OpenStream(out int ttcIndex) and pass ttcIndex to SKTypeface.FromStream(stream, ttcIndex).

### Rationale

The behavior described is by design in Skia and SkiaSharp. The two-argument overload OpenStream(out int ttcIndex) exists precisely for TTC round-tripping. The reporter's code has two bugs: (1) allocates fontData with Length-1 bytes (off-by-one) and (2) calls FromStream without the ttcIndex, causing the first font in the TTC to be selected instead of GungSuhChe. This is a usage question, not a broken API.

### Key Signals

- "stream retrieved from typeface gives TTC file (collection of TTF file) stream instead of particular TTF file stream" — **issue body** (Reporter expects isolated TTF bytes but OpenStream intentionally returns the TTC container and a ttcIndex.)
- "SKTypeface typeface2 = SKTypeface.FromStream(stream);" — **issue body code snippet** (Reporter calls FromStream without the ttcIndex, so Skia picks the first font (index 0) in the TTC instead of the target font.)
- "byte[] fontData = new byte[fontStream.Length - 1];" — **issue body code snippet** (Off-by-one: should be fontStream.Length, not Length-1.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 270-278 | direct | OpenStream() (no-arg) discards ttcIndex via 'out _'. OpenStream(out int ttcIndex) correctly surfaces the TTC index needed to round-trip the typeface. Both overloads return the full TTC stream — this is by design from Skia's SkTypeface::openStream. |
| `binding/SkiaSharp/SKTypeface.cs` | 92-96 | direct | FromStream(Stream stream, int index = 0) and FromStream(SKStreamAsset stream, int index = 0) both accept an index parameter for TTC collections. Passing the ttcIndex from OpenStream here correctly selects the intended font. |

### Workarounds

- Use the OpenStream(out int ttcIndex) overload to capture the TTC font index, then pass that index to SKTypeface.FromStream(stream, ttcIndex) to recreate the correct typeface.
- If isolated TTF bytes are needed (not just round-tripping the typeface), use GetTableTags()/GetTableData() to read individual SFNT tables and manually reconstruct a TTF — this is complex and usually unnecessary.

### Next Questions

- Does the reporter need isolated TTF bytes for a non-SkiaSharp API, or just to recreate the same typeface via SkiaSharp?

### Resolution Proposals

**Hypothesis:** Reporter did not know about the ttcIndex overload of OpenStream and did not pass the index when calling FromStream, causing the wrong font to be returned.

1. **Use OpenStream(out ttcIndex) and FromStream(stream, ttcIndex)** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Call the two-argument OpenStream overload to get the TTC stream and the font index, then pass both to FromStream to recreate the exact typeface.

```csharp
using var typeface = SKTypeface.FromFamilyName("GungSuhChe");
using var fontStream = typeface.OpenStream(out int ttcIndex);

int length = fontStream.Length;
byte[] fontData = new byte[length];
fontStream.Read(fontData, length);

using var memStream = new MemoryStream(fontData);
memStream.Position = 0;
using var typeface2 = SKTypeface.FromStream(memStream, ttcIndex);
Console.WriteLine(typeface2.FamilyName); // GungSuhChe
```

**Recommended proposal:** Use OpenStream(out ttcIndex) and FromStream(stream, ttcIndex)

**Why:** Directly addresses the root cause with minimal code change. The OpenStream(out int) overload was designed for exactly this TTC round-trip scenario.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | OpenStream behavior for TTC fonts is by design. The fix is to use OpenStream(out int ttcIndex) + FromStream(stream, ttcIndex). Two independent users confirm the same usage error. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply question and SkiaSharp area labels | labels=type/question, area/SkiaSharp |
| add-comment | medium | 0.92 (92%) | Post explanation of OpenStream TTC behavior and correct usage with code sample | — |
| close-issue | medium | 0.80 (80%) | Close as answered — behavior is by design, correct usage documented in comment | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! The behavior you're seeing is by design — `OpenStream()` returns the **entire TTC file** (the container), not a single extracted TTF. This matches Skia's own `SkTypeface::openStream` behavior.

To correctly round-trip a typeface from a TTC back through a stream, use the overload that also returns the **TTC index**, then pass that index to `FromStream`:

```csharp
using var typeface = SKTypeface.FromFamilyName("GungSuhChe");
using var fontStream = typeface.OpenStream(out int ttcIndex);

int length = fontStream.Length;
byte[] fontData = new byte[length];  // Note: Length, not Length-1
fontStream.Read(fontData, length);

using var memStream = new MemoryStream(fontData);
memStream.Position = 0;
using var typeface2 = SKTypeface.FromStream(memStream, ttcIndex);  // pass ttcIndex!
Console.WriteLine(typeface2.FamilyName);  // GungSuhChe
```

There were two issues in the original code:
1. `new byte[fontStream.Length - 1]` — off-by-one; should be `fontStream.Length`
2. `SKTypeface.FromStream(stream)` without the index — this selects font 0 in the TTC, not necessarily the one you loaded

If you need the raw isolated TTF bytes (e.g., to pass to a non-SkiaSharp API), that requires manually reconstructing the font from its SFNT tables via `GetTableTags()` / `GetTableData()`, which is considerably more complex. Let us know if that's your actual goal.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1836,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T22:22:16Z"
  },
  "summary": "Reporter expects SKTypeface.OpenStream() on a typeface loaded from a TTC (TrueType Collection) file to return only the stream for the specific TTF inside the collection, but it returns the entire TTC file stream; the correct approach is to use the OpenStream(out int ttcIndex) overload and pass ttcIndex to FromStream.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    }
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "SKTypeface typeface = SKTypeface.FromFamilyName(\"GungSuhChe\"); SKStreamAsset fontStream = typeface.OpenStream(); byte[] fontData = new byte[fontStream.Length - 1]; fontStream.Read(fontData, fontStream.Length); var stream = new MemoryStream(fontData); stream.Position = 0; SKTypeface typeface2 = SKTypeface.FromStream(stream);"
      ],
      "environmentDetails": "SkiaSharp 2.80.3, Visual Studio 2019",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SKTypeface.OpenStream behavior has not changed — by design it returns the TTC container stream plus a ttcIndex."
    }
  },
  "analysis": {
    "summary": "SKTypeface.OpenStream() is designed to return the full TTC file stream along with a ttcIndex indicating which font in the collection this typeface represents. The reporter is not using the ttcIndex overload and is also ignoring the index when calling FromStream, so they receive the wrong font. The solution is to use OpenStream(out int ttcIndex) and pass ttcIndex to SKTypeface.FromStream(stream, ttcIndex).",
    "rationale": "The behavior described is by design in Skia and SkiaSharp. The two-argument overload OpenStream(out int ttcIndex) exists precisely for TTC round-tripping. The reporter's code has two bugs: (1) allocates fontData with Length-1 bytes (off-by-one) and (2) calls FromStream without the ttcIndex, causing the first font in the TTC to be selected instead of GungSuhChe. This is a usage question, not a broken API.",
    "keySignals": [
      {
        "text": "stream retrieved from typeface gives TTC file (collection of TTF file) stream instead of particular TTF file stream",
        "source": "issue body",
        "interpretation": "Reporter expects isolated TTF bytes but OpenStream intentionally returns the TTC container and a ttcIndex."
      },
      {
        "text": "SKTypeface typeface2 = SKTypeface.FromStream(stream);",
        "source": "issue body code snippet",
        "interpretation": "Reporter calls FromStream without the ttcIndex, so Skia picks the first font (index 0) in the TTC instead of the target font."
      },
      {
        "text": "byte[] fontData = new byte[fontStream.Length - 1];",
        "source": "issue body code snippet",
        "interpretation": "Off-by-one: should be fontStream.Length, not Length-1."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "270-278",
        "finding": "OpenStream() (no-arg) discards ttcIndex via 'out _'. OpenStream(out int ttcIndex) correctly surfaces the TTC index needed to round-trip the typeface. Both overloads return the full TTC stream — this is by design from Skia's SkTypeface::openStream.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "92-96",
        "finding": "FromStream(Stream stream, int index = 0) and FromStream(SKStreamAsset stream, int index = 0) both accept an index parameter for TTC collections. Passing the ttcIndex from OpenStream here correctly selects the intended font.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Use the OpenStream(out int ttcIndex) overload to capture the TTC font index, then pass that index to SKTypeface.FromStream(stream, ttcIndex) to recreate the correct typeface.",
      "If isolated TTF bytes are needed (not just round-tripping the typeface), use GetTableTags()/GetTableData() to read individual SFNT tables and manually reconstruct a TTF — this is complex and usually unnecessary."
    ],
    "nextQuestions": [
      "Does the reporter need isolated TTF bytes for a non-SkiaSharp API, or just to recreate the same typeface via SkiaSharp?"
    ],
    "resolution": {
      "hypothesis": "Reporter did not know about the ttcIndex overload of OpenStream and did not pass the index when calling FromStream, causing the wrong font to be returned.",
      "proposals": [
        {
          "title": "Use OpenStream(out ttcIndex) and FromStream(stream, ttcIndex)",
          "description": "Call the two-argument OpenStream overload to get the TTC stream and the font index, then pass both to FromStream to recreate the exact typeface.",
          "category": "workaround",
          "codeSnippet": "using var typeface = SKTypeface.FromFamilyName(\"GungSuhChe\");\nusing var fontStream = typeface.OpenStream(out int ttcIndex);\n\nint length = fontStream.Length;\nbyte[] fontData = new byte[length];\nfontStream.Read(fontData, length);\n\nusing var memStream = new MemoryStream(fontData);\nmemStream.Position = 0;\nusing var typeface2 = SKTypeface.FromStream(memStream, ttcIndex);\nConsole.WriteLine(typeface2.FamilyName); // GungSuhChe",
          "validated": "yes",
          "confidence": 0.92,
          "effort": "cost/xs"
        }
      ],
      "recommendedProposal": "Use OpenStream(out ttcIndex) and FromStream(stream, ttcIndex)",
      "recommendedReason": "Directly addresses the root cause with minimal code change. The OpenStream(out int) overload was designed for exactly this TTC round-trip scenario."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "OpenStream behavior for TTC fonts is by design. The fix is to use OpenStream(out int ttcIndex) + FromStream(stream, ttcIndex). Two independent users confirm the same usage error.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post explanation of OpenStream TTC behavior and correct usage with code sample",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "Thanks for the report! The behavior you're seeing is by design — `OpenStream()` returns the **entire TTC file** (the container), not a single extracted TTF. This matches Skia's own `SkTypeface::openStream` behavior.\n\nTo correctly round-trip a typeface from a TTC back through a stream, use the overload that also returns the **TTC index**, then pass that index to `FromStream`:\n\n```csharp\nusing var typeface = SKTypeface.FromFamilyName(\"GungSuhChe\");\nusing var fontStream = typeface.OpenStream(out int ttcIndex);\n\nint length = fontStream.Length;\nbyte[] fontData = new byte[length];  // Note: Length, not Length-1\nfontStream.Read(fontData, length);\n\nusing var memStream = new MemoryStream(fontData);\nmemStream.Position = 0;\nusing var typeface2 = SKTypeface.FromStream(memStream, ttcIndex);  // pass ttcIndex!\nConsole.WriteLine(typeface2.FamilyName);  // GungSuhChe\n```\n\nThere were two issues in the original code:\n1. `new byte[fontStream.Length - 1]` — off-by-one; should be `fontStream.Length`\n2. `SKTypeface.FromStream(stream)` without the index — this selects font 0 in the TTC, not necessarily the one you loaded\n\nIf you need the raw isolated TTF bytes (e.g., to pass to a non-SkiaSharp API), that requires manually reconstructing the font from its SFNT tables via `GetTableTags()` / `GetTableData()`, which is considerably more complex. Let us know if that's your actual goal."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — behavior is by design, correct usage documented in comment",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
