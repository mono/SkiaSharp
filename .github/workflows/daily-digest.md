---
description: Every weekday, post a "Daily Digest" issue that summarises all open issues and pull requests, grouped by label.
on:
  schedule:
    - cron: "0 8 * * 1-5"   # 08:00 UTC, Monday–Friday
  workflow_dispatch: {}
permissions:
  issues: read
  pull-requests: read
timeout-minutes: 20
safe-outputs:
  mentions: false
  allowed-github-references: []
  max-bot-mentions: 1
  create-issue:
    title-prefix: "Daily Digest –"
    labels: [digest]
    close-older-issues: true
    expires: 7
---

# Daily Digest

Produce a concise daily digest of every open issue and open pull request in
this repository.

## Steps

1. Fetch all open issues (excluding pull requests) and all open pull requests.
   For each item collect:
   - Number and title
   - Author (login)
   - Labels (all of them)
   - Date opened → compute how long it has been open (e.g. "3 days", "2 weeks")

2. Group items into two top-level sections — **Open Issues** and
   **Open Pull Requests**.  Within each section, create one sub-group per
   label.  Items that carry multiple labels appear under each relevant label.
   Items with no labels appear under **Unlabelled**.

3. Within each label group list items as a markdown table with columns:
   `#` | `Title` | `Author` | `Open for`

4. Show a **Summary** at the top with:
   - Total open issues count
   - Total open pull requests count
   - Date of the digest (today's date, `YYYY-MM-DD`)

5. If either category is empty, include a brief note saying so instead of an
   empty section.

6. Use `###` for top-level section headers and `####` for label sub-group
   headers to respect the document hierarchy.

7. Output the report as a new GitHub issue titled
   `Daily Digest – <YYYY-MM-DD>` using `create-issue`.
