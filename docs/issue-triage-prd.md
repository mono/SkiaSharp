# SkiaSharp Issue Triage System - PRD

> **Status:** Draft - Exploring Ideas  
> **Last Updated:** 2026-02-06  
> **Owner:** @mattleibow  
> **Purpose:** Document goals, strategies, and ideas for improving repo health through better issue management.

---

## Vision

Transform the SkiaSharp issue tracker from an overwhelming backlog into a healthy, actionable queue where:
- Every issue has a clear type and status
- Non-bug issues (docs, samples, questions) are resolved quickly
- Real bugs and feature requests are visible and prioritized
- AI assists with the heavy lifting of triage and content generation

---

## Goals

### Primary Goal
**Clean the backlog** by identifying and resolving issues that don't require code fixes:
- **Docs/Samples needed** → AI generates content → close issue
- **Questions** → Move to Discussions → close issue
- **Already fixed** → AI verifies → close issue

### Secondary Goal
**Understand what's left** - after cleanup, the remaining issues are genuine bugs and feature requests that need engineering time.

### Success Metrics
| Metric | Current | Target |
|--------|---------|--------|
| Open issues | 658 | < 300 |
| Unlabeled issues | 361 (55%) | < 10% |
| Docs/Sample closures | 0 | 50+ |
| Questions moved | 0 | 30+ |

---

## Current State

### Issue Counts (SkiaSharp main repo)
- **658 open issues** total
- **361 unlabeled** (55%) - no triage at all
- **149 labeled `type/bug`**
- **32 labeled `type/feature-request`**

### Age Distribution
| Period | Count | Notes |
|--------|-------|-------|
| 2016-2018 | 21 | Very old, pre-.NET Core era |
| 2019-2020 | 131 | Pre-MAUI, Xamarin.Forms era |
| 2021-2022 | 205 | MAUI transition period |
| 2023-2024 | 209 | Recent |
| 2025-2026 | 92 | Current |

### Data Available Per Issue
- `title` - Short description
- `body` - Full issue text (markdown)
- `labels` - Current labels
- `comments` - Comment text, author, date, reactions
- `reactions` - 👍👎 etc counts on issue itself
- `createdAt`, `updatedAt` - Timestamps
- `author` - Who filed it

### Data Not Available (would need additional sync)
- Linked PRs/commits (to detect "already fixed")
- Timeline events (close/reopen history)
- Cross-references from other issues

**Note:** For "already fixed" detection, AI can scan the codebase and commit history directly rather than relying on cached links.

---

## Repo Health Indicators

What does a "healthy" issue tracker look like?

### Metrics to Track
| Metric | Healthy | Warning | Current |
|--------|---------|---------|---------|
| Unlabeled % | < 10% | > 30% | 55% ❌ |
| Median issue age | < 6 months | > 2 years | ~18 months ⚠️ |
| Response time (first reply) | < 7 days | > 30 days | ? |
| Issues opened vs closed (monthly) | Net negative | Net positive | ? |
| Engagement without response | < 5% | > 20% | ? |

### What Hurts Repo Health
- **Unlabeled issues** - Can't prioritize what you can't see
- **Questions in issues** - Clutter, should be Discussions
- **Zombie issues** - Open forever, no activity, unclear status
- **Missing info issues** - Can't act on them, they just sit there
- **Duplicate issues** - Same problem reported many times

### What Improves Repo Health
- **Fast initial triage** - Label and respond within days
- **Clear resolution paths** - Users know what happens next
- **Proactive documentation** - Fewer "how do I" questions
- **Regular backlog grooming** - Keep it fresh
- **Community involvement** - Help with reproduction, triage

---

## Issue Type Classification

The first step is classifying every issue into one of 5 types. Each type maps to a GitHub label.

### GitHub Labels

| Label | Color | Description |
|-------|-------|-------------|
| `type/bug` | 🔴 Red | Bugs, crashes, performance, rendering issues |
| `type/feature-request` | ⚪ Gray | New API, new type, new functionality |
| `type/enhancement` | 🔵 Blue | Improvement to existing feature or infrastructure |
| `type/question` | 🟢 Green | Support questions, how-to |
| `type/documentation` | 🔵 Blue | Documentation or sample needed |

---

### 1. 🐛 Bug (`type/bug`)
**Description:** Something is broken in SkiaSharp code.

**Signals:**
- Stack traces, exceptions, crashes
- "This worked in version X but broke in Y"
- "Incorrect output" with screenshots
- Performance problems with measurements

**Action:**
- Keep in backlog
- Further triage: severity, component, repro quality
- Eventually fix in code

**This is the real backlog** - these need engineering time.

---

### 2. ✨ Feature Request (`type/feature-request`)
**Description:** User wants new functionality that doesn't exist.

**Signals:**
- "It would be nice if..."
- "Feature request:"
- "Can you add support for..."
- New API, new type, new platform support
- Proposals, RFCs

**Action:**
- Keep in backlog
- Evaluate: feasibility, demand, scope
- May close as "won't do" with explanation

**This is also real backlog** - requires product decisions.

---

### 3. 🔧 Enhancement (`type/enhancement`)
**Description:** Improvement to existing functionality or infrastructure.

**Signals:**
- "Could X be faster/better/easier?"
- "Make X poolable/reusable"
- "Better error messages for..."
- Performance improvements to existing APIs
- Developer experience improvements

**Action:**
- Keep in backlog
- Often lower priority than bugs
- Good for contributors

**Distinction from feature-request:** Enhancement improves what exists; feature-request adds something new.

---

### 4. ❓ Question (`type/question`)
**Description:** User asking for help, not reporting a problem with SkiaSharp itself.

**Signals:**
- "How do I achieve X effect?"
- "Best practice for..."
- "Can SkiaSharp do X?" (capability question)
- Stack Overflow-style questions

**Action:**
- Answer the question in a comment
- Convert to GitHub Discussion (or link to existing)
- Close issue

**Why this helps:** Questions clutter the issue tracker. Discussions are the right place.

---

### 5. 📚 Documentation (`type/documentation`)
**Description:** User is confused or needs an example, not reporting a bug.

**Signals:**
- "How do I...", "Example of...", "Tutorial for..."
- "What's the correct way to..."
- "I don't understand..."
- No error/crash, just confusion
- Requests for samples or code examples

**Action:** 
- AI generates documentation or sample code
- Post as comment, link to docs if applicable
- Close issue with "Resolved with documentation"

**Why AI is perfect:** AI can write docs and samples quickly and well. This is a huge time saver.

---

## "Already Fixed" Detection

**Note:** "Already fixed" is not a type - it's a resolution status that can apply to any type (mostly bugs).

Instead of a label, AI populates JSON properties:

```json
{
  "number": 1234,
  "fixed": true,
  "fixReason": "This was fixed when we merged the Metal backend in #2156"
}
```

**Signals:**
- Old version mentioned, issue not reproducible on latest
- Related PR was merged
- Comments say "works for me now"

**Action:**
- AI attempts to verify fix by checking code/PRs
- If verified, close with "Fixed in version X"

---

## AI-Assisted Triage Workflow

### Output Structure

AI classification is stored in `ai-triage.json`:

```json
{
  "1234": {
    "type": "documentation",
    "confidence": 0.92,
    "reason": "User asks 'How do I draw rounded rectangles?' - needs sample code",
    "fixed": false,
    "fixReason": null
  },
  "5678": {
    "type": "bug",
    "confidence": 0.88,
    "reason": "Stack trace present, reports crash on Android when disposing SKCanvasView",
    "fixed": false,
    "fixReason": null
  },
  "9012": {
    "type": "bug",
    "confidence": 0.75,
    "reason": "Reports rendering issue with gradients on iOS",
    "fixed": true,
    "fixReason": "Likely fixed in SkiaSharp 3.0 when we upgraded Skia - gradient rendering was rewritten"
  }
}
```

**Type values:** `bug` | `feature-request` | `enhancement` | `question` | `documentation`

### Phase 1: Classify All Issues

AI reads each issue's title + body + comments and classifies:

```
Issue #1234: "How do I draw rounded rectangles?"
→ Type: documentation
→ Confidence: 92%
→ Suggested Action: Generate sample code for rounded rectangles
```

```
Issue #5678: "App crashes on Android when disposing SKCanvasView"
→ Type: bug
→ Confidence: 88%
→ Has Stack Trace: Yes
→ Has Repro: Yes
→ Platforms: Android
```

### Phase 2: Auto-Generate Content (Docs/Samples)

For `type/documentation` issues:

1. AI reads the issue to understand what user wants
2. AI generates:
   - Code sample (complete, runnable)
   - Explanation of how it works
   - Link to relevant docs if they exist
3. Human reviews generated content
4. Post as comment, close issue

**Example Output:**

> Here's how to draw a rounded rectangle in SkiaSharp:
> 
> ```csharp
> using var paint = new SKPaint
> {
>     Color = SKColors.Blue,
>     IsAntialias = true
> };
> 
> var rect = new SKRect(10, 10, 200, 100);
> float cornerRadius = 20;
> 
> canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, paint);
> ```
> 
> This uses `DrawRoundRect` which takes the rectangle bounds and X/Y corner radii.
> 
> See also: [SkiaSharp Drawing Basics](https://docs.microsoft.com/...)


### Phase 3: Verify Fixed Issues

For old issues (especially pre-2.88):

1. AI reads the issue and understands the problem
2. AI checks:
   - Was there a related PR merged?
   - Does the code mentioned still exist?
   - Can it write a test that would have caught this?
3. AI attempts to reproduce on latest version
4. If not reproducible, suggest closing

### Phase 4: Convert Questions to Discussions

For "Question" type issues:

1. AI drafts an answer
2. Human reviews
3. Post answer as comment
4. Optionally create a Discussion for ongoing Q&A
5. Close issue

---

## Backlog Reduction Strategies

### Strategy 1: Docs/Sample Sprint
- Filter: All issues classified as "Docs/Sample Needed"
- Action: AI generates content for each
- Goal: Close 50+ issues in one sprint

### Strategy 2: Question Migration
- Filter: All issues classified as "Question"
- Action: Answer and/or move to Discussions
- Goal: Close 30+ issues

### Strategy 3: Version Verification
- Filter: Issues mentioning versions < 2.88
- Action: AI checks if reproducible on latest
- Goal: Close issues that are no longer valid

### Strategy 4: Duplicate Detection
- AI finds semantically similar issues
- Human reviews and consolidates
- Close duplicates, link to canonical issue

### Strategy 5: "Needs Info" Cleanup
- Filter: Issues labeled "needs-info" with no response > 60 days
- Action: Close with polite message
- Goal: Clear out abandoned issues

---

## What AI Can Do

### High Confidence (AI can do well)
| Task | Notes |
|------|-------|
| **Classify issue type** | Bug vs docs vs question - very reliable |
| **Generate sample code** | Given clear requirements, AI writes good samples |
| **Write documentation** | Explanations, how-tos, API usage examples |
| **Answer questions** | Technical Q&A based on SkiaSharp knowledge |
| **Find duplicates** | Semantic similarity to cluster related issues |
| **Extract structured data** | Platform, version, has-repro, has-stacktrace |
| **Summarize issues** | One-line summaries for quick scanning |
| **Draft responses** | Templates for common situations |

### Medium Confidence (AI can assist)
| Task | Notes |
|------|-------|
| **Check if issue is fixed** | Can look at code/PRs but needs verification |
| **Reproduce issues** | Can write test code, but environment-dependent |
| **Suggest priority** | Based on signals, but human judgment needed |
| **Identify root cause** | Can analyze stack traces, suggest areas to look |

### Human Required
| Task | Notes |
|------|-------|
| **Actually fix bugs** | Engineering work (though AI can help) |
| **Make design decisions** | Product judgment |
| **Final approval** | Human should review before posting/closing |

---

## AI-Powered Workflows (Ideas)

### Workflow: "Issue Doctor"
AI analyzes an issue and provides:
- Classification (type, severity, area)
- Summary (one paragraph)
- Suggested action
- If docs/sample: draft content
- If question: draft answer
- If bug: key details extracted (platform, version, repro?)
- Similar issues (potential duplicates)

### Workflow: "Reproduction Attempt"
For bug reports:
1. AI reads the issue and extracts repro steps
2. AI writes a minimal test case
3. AI attempts to compile/run against latest SkiaSharp
4. Reports: reproducible / not reproducible / needs more info

### Workflow: "Fix Verification"
For old issues:
1. AI identifies the claimed bug
2. AI searches for related PRs/commits
3. AI writes a test for the issue
4. AI runs test on latest version
5. Reports: likely fixed / still broken / inconclusive

### Workflow: "Docs Generator"
For docs/sample requests:
1. AI understands what user is asking
2. AI writes complete, working code sample
3. AI adds explanation
4. Human reviews
5. Post and close

### Workflow: "Question Answerer"
For support questions:
1. AI understands the question
2. AI searches docs, samples, Stack Overflow
3. AI drafts comprehensive answer
4. Human reviews
5. Post answer, convert to Discussion or close

---

## Dashboard Views Needed

### 1. Classification Queue
- All 658 issues with AI-suggested type
- Filter by type, confidence, age
- Bulk approve/reject classifications

### 2. Docs/Sample Generator
- Issues needing docs/samples
- One-click "Generate Content"
- Preview, edit, post

### 3. Questions to Answer
- Issues that are questions
- AI-drafted answers
- Convert to Discussion button

### 4. Verification Queue
- Old issues to check if fixed
- AI analysis of likely status
- Close with reason

### 5. True Backlog
- After cleanup: real bugs and features
- Prioritized by severity, engagement
- This is what needs engineering

---

## Open Questions

1. **AI Provider:** OpenAI API? Azure OpenAI? Local model?

2. **GitHub Integration:** 
   - Just read issues and display insights?
   - Or also post comments and close issues via API?

3. **Human in the Loop:**
   - AI suggests, human approves every action?
   - Or AI can auto-post for high-confidence cases?

4. **Scope:** 
   - Start with SkiaSharp only?
   - Include Extended repo too?

---

## Notes & Ideas

_Space for ongoing thoughts..._

- Consider a "triage party" - community event to help classify
- Maybe gamify: leaderboard for issues closed
- Could integrate with GitHub Projects for workflow
- Webhook to auto-classify new issues as they come in
- AI could analyze code changes and proactively check if old issues are fixed
- For recurring questions, AI could build an FAQ automatically
- AI could draft release notes from closed issues
- Sentiment analysis could surface frustrated users who need attention
- Cross-reference with Stack Overflow questions about SkiaSharp
- Track which areas generate most docs/sample requests → improve docs proactively
- Monthly "health report" summarizing backlog trends

