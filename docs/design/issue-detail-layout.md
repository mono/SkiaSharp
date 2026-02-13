# AI Triage Dashboard - Detail Page Design

## 1. Hero (Always Visible)

**Goal:** Understand context and status at a glance without reading.

**Layout:**
*   **Top Row:** `[Repo/Owner] / Issue #123` (Breadcrumb style)
*   **Title:** Large, bold issue title.
*   **Metadata Row:**
    *   Badges: `[Type: Bug]` `[Severity: High]` `[Area: Rendering]` `[Platform: Android]`
    *   *Color coded: Red for High/Critical/Bug, Blue for Feature, Gray for Question.*
*   **AI Verdict Banner:** Full-width colored strip detected from Triage/Repro/Fix status.
    *   🔴 **CONFIRMED BUG** (If Repro = Reproduced)
    *   🟡 **NEEDS INFO** (If Repro = Missing Info)
    *   🟢 **RESOLVED** (If Fix = Validated)
    *   ⚪ **TRIAGED** (If only Triage exists)

---

## 2. Tabs (Max 3)

1.  **⚡ Triage & Action** (Default) - "What do I do?"
2.  **🔍 Investigation** - "Why did we decide this?"
3.  **📄 Source** - "What did the user say?"

---

## 3. Tab Contents

### Tab 1: ⚡ Triage & Action
**Goal:** Review the AI's work and take action (Close, Reply, Label).

*   **Correction Alert (Conditional):**
    *   *Visible ONLY if Repro Conclusion contradicts Triage Prediction.*
    *   Style: Yellow/Red warning box at the very top.
    *   Content: "⚠️ **Correction**: AI Triage predicted **User Error**, but Reproduction confirmed **Crash**."
*   **Left Column (The Situation):**
    *   **Summary**: 2-sentence AI summary of the issue.
    *   **Repro Status**: Big status indicator (e.g., "✅ Reproduced on Android 12").
    *   **Recommended Action**: "Close as Fixed" / "Request More Info".
*   **Right Column (The Response):**
    *   **Draft Response**: Markdown editor pre-filled with the AI's proposed comment.
    *   **Action Buttons**: `[ Post Comment ]` `[ Copy to Clipboard ]`

### Tab 2: 🔍 Investigation
**Goal:** Verify the AI's findings if you doubt the summary.

*   **Reproduction Details:**
    *   **Steps Taken**: The exact steps the AI used.
    *   **Evidence**: Screenshots or Logs (collapsible).
    *   **Matrix**: Table of versions tested (e.g., SkiaSharp 2.80: ❌, 2.88: ✅).
*   **Deep Analysis:**
    *   **Signals**: Bullet points of keywords/stack traces found.
    *   **Code Search**: "Investigated `SKCanvas.cs` lines 40-50".
*   **Fix (If available):**
    *   **Root Cause**: Explanation of the bug.
    *   **PR Link**: Link to the fix PR.

### Tab 3: 📄 Source
**Goal:** Reference original context without leaving the dashboard.

*   Rendered Markdown of the original GitHub issue body.
*   Link to view on GitHub.

---

## 4. Triage Corrections
*   **Location:** Top of **Tab 1 (Triage & Action)**.
*   **Format:** High-contrast alert box (Warning/Danger color).
*   **Content:** Explicitly states the conflict. "Triage said X, but Repro proved Y."
*   **Why:** Ensures the maintainer sees the most critical new information immediately.

## Text Wireframe

```text
+-----------------------------------------------------------------------+
|  mono/SkiaSharp #1245                                                 |
|  **Application Crashes on Startup when using GPU View**               |
|  [Bug] [Severity: High] [Area: GPU] [Android]                         |
|                                                                       |
|  [🔴 CONFIRMED BUG - FIX REQUIRED __________________________________] |
+-----------------------------------------------------------------------+
|  [ **⚡ Triage & Action** ]  [ 🔍 Investigation ]  [ 📄 Source ]      |
+-----------------------------------------------------------------------+
|                                                                       |
|  [⚠️ CORRECTION: Triage predicted 'Question', Repro found 'Crash']    |
|                                                                       |
|  +---------------------------+  +----------------------------------+  |
|  | **Summary**               |  | **Draft Response**               |  |
|  | User reports crash on     |  |                                  |  |
|  | Pixel 6. Confirmed as     |  | Hi @user,                        |  |
|  | regression in v2.88.      |  |                                  |  |
|  |                           |  | I reproduced this on Android 12. |  |
|  | **Repro Status**          |  | It seems to be a regression.     |  |
|  | ✅ Reproduced             |  |                                  |  |
|  |                           |  | We are working on a fix.         |  |
|  | **Suggested Action**      |  |                                  |  |
|  | Fix Priority: High        |  | [ Post Comment ] [ Copy ]        |  |
|  +---------------------------+  +----------------------------------+  |
|                                                                       |
+-----------------------------------------------------------------------+
```
