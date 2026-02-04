# Engagement Score Algorithm - Technical Specification

> This document describes the engagement scoring algorithm from the [triage-assistant](https://github.com/mattleibow/triage-assistant) 
> repository and how it will be integrated into the SkiaSharp Dashboard collector.

## Overview

The engagement score is a numerical measure of community interest and activity on an issue. It helps prioritize issues 
based on how much the community cares about them, rather than just age or labels.

**Key Concepts:**
- **Engagement Score**: A weighted sum of activity metrics (comments, reactions, contributors, time factors)
- **Previous Score**: The same calculation but using only data from 7+ days ago
- **Hot Classification**: Issues where current score > previous score (gaining momentum)

## Algorithm Formula

```
Score = (Comments × weight_comments) 
      + (Reactions × weight_reactions) 
      + (Contributors × weight_contributors) 
      + (1/DaysSinceLastActivity × weight_lastActivity) 
      + (1/DaysOpen × weight_issueAge) 
      + (LinkedPRs × weight_linkedPRs)
```

### Input Metrics

| Metric | Description | Calculation |
|--------|-------------|-------------|
| **Comments** | Number of comments | `comments.length` |
| **Reactions** | Total reactions on issue + comments | `issueReactions + Σ(commentReactions)` |
| **Contributors** | Unique users involved | `Set(author, assignees, commentAuthors).size` |
| **DaysSinceLastActivity** | Days since last update | `ceil((now - updatedAt) / day)`, min 1 |
| **DaysOpen** | Issue age in days | `ceil((now - createdAt) / day)`, min 1 |
| **LinkedPRs** | Number of linked PRs | Not implemented (always 0) |

### Default Weights

| Weight | Default Value | Rationale |
|--------|---------------|-----------|
| `comments` | 3 | Discussion volume indicates high interest |
| `reactions` | 1 | Emotional engagement and sentiment |
| `contributors` | 2 | Diversity of input reflects broad interest |
| `lastActivity` | 1 | Recent activity indicates current relevance |
| `issueAge` | 1 | Older issues may need more attention |
| `linkedPullRequests` | 2 | Active development work (not implemented) |

### Score Interpretation

| Range | Classification | Meaning |
|-------|----------------|---------|
| > 50 | High | Significant community engagement, needs immediate attention |
| 10-50 | Medium | Moderate activity, potential for growth |
| < 10 | Low | Limited engagement, may need promotion or closure |

## Historical Score Calculation

The "previous score" is calculated by filtering all activity to only include items from **7 or more days ago**:

### Filtering Logic

```
For each issue created >= 7 days ago:
  - Filter comments: keep only those where createdAt <= (now - 7 days)
  - Filter reactions: keep only those where createdAt <= (now - 7 days)
  - Filter comment reactions similarly
  - Set updatedAt = (now - 7 days)
  
For issues newer than 7 days:
  - Return 0 comments, 0 reactions
  - This gives a minimal baseline score from just contributors
```

### Hot Classification

An issue is classified as **"Hot"** when:

```
currentScore > previousScore
```

This means the issue is **gaining momentum** - there's been activity in the last 7 days that increased its score.

## Example Calculation

Given an issue with:
- 2 comments
- 5 total reactions (2 on issue, 3 on comments)
- 5 unique contributors
- Last activity: 2 days ago
- Issue age: 9 days
- 0 linked PRs

Using default weights:
```
Score = (3 × 2)           // comments: 6
      + (1 × 5)           // reactions: 5
      + (2 × 5)           // contributors: 10
      + (1 × 1/2)         // lastActivity: 0.5
      + (1 × 1/9)         // issueAge: 0.11
      + (2 × 0)           // linkedPRs: 0
      = 6 + 5 + 10 + 0.5 + 0.11 + 0
      = 21.61
      ≈ 22 (rounded)
```

## Integration into SkiaSharp Dashboard

### Data Collection Changes

The `issues` command will be enhanced to:

1. **Fetch additional data** per issue:
   - All comments with their `createdAt` timestamps
   - All reactions on the issue with `createdAt` timestamps
   - All reactions on each comment with `createdAt` timestamps

2. **Calculate scores**:
   - Current engagement score
   - Previous engagement score (7-day historic snapshot)
   - Hot classification (boolean)

### JSON Output Changes

Each issue in `issues.json` will include:

```json
{
  "number": 123,
  "title": "...",
  "engagement": {
    "score": 22,
    "previousScore": 15,
    "isHot": true
  }
}
```

### Aggregation for Dashboard

The `IssuesData` model will include:
- Top 10 "hot" issues (sorted by score, filtered where isHot = true)
- Average engagement score across all issues
- Distribution of scores (buckets: 0-10, 10-25, 25-50, 50+)

### Dashboard UI

The Issues page will show:
- 🔥 badge on "hot" issues
- Sortable "Engagement" column
- Filter: "Show only hot issues"
- Optional: Engagement score trend chart

## Implementation Plan

### Phase 1: Data Model & Collector (C#)

1. Create `EngagementScore` record:
   ```csharp
   public record EngagementScore(
       int Score,
       int PreviousScore,
       bool IsHot
   );
   ```

2. Create `EngagementWeights` configuration:
   ```csharp
   public record EngagementWeights(
       int Comments = 3,
       int Reactions = 1,
       int Contributors = 2,
       int LastActivity = 1,
       int IssueAge = 1,
       int LinkedPullRequests = 2
   );
   ```

3. Implement `EngagementCalculator` service:
   - `CalculateScore(IssueDetails issue, EngagementWeights weights)`
   - `CalculateHistoricalScore(IssueDetails issue, EngagementWeights weights)`
   - `GetUniqueContributorsCount(IssueDetails issue)`
   - `FilterToHistoricData(IssueDetails issue, DateTime cutoff)`

### Phase 2: GitHub API Integration

1. Enhance issue fetching to include:
   - Comments with reactions (GraphQL or REST pagination)
   - Issue reactions with timestamps
   - Comment reactions with timestamps

2. Handle rate limiting for additional API calls

### Phase 3: Collector Command Update

1. Update `IssuesCommand` to:
   - Calculate engagement for each issue
   - Include engagement data in output JSON
   - Add summary stats (hot count, average score)

### Phase 4: Dashboard UI

1. Add engagement column to Issues table
2. Add "Hot" filter and badge
3. Add engagement chart to Insights page

## API Data Requirements

### Current Data Collected
- Issue number, title, author, dates
- Labels, assignees
- Comment count (number only)

### Additional Data Needed
For full engagement scoring:
- **Comments** with `createdAt` and `user.login`
- **Issue reactions** with `createdAt` and `user.login`
- **Comment reactions** with `createdAt` and `user.login`

### GraphQL Query (Recommended)

```graphql
query($owner: String!, $repo: String!, $number: Int!) {
  repository(owner: $owner, name: $repo) {
    issue(number: $number) {
      reactions(first: 100) {
        nodes {
          user { login }
          content
          createdAt
        }
      }
      comments(first: 100) {
        nodes {
          author { login }
          createdAt
          reactions(first: 100) {
            nodes {
              user { login }
              content
              createdAt
            }
          }
        }
      }
    }
  }
}
```

### REST API Alternative

- `GET /repos/{owner}/{repo}/issues/{number}/comments`
- `GET /repos/{owner}/{repo}/issues/{number}/reactions`
- `GET /repos/{owner}/{repo}/issues/comments/{comment_id}/reactions`

**Note**: REST requires more API calls and may hit rate limits faster.

## Testing Strategy

Port the test cases from `issue-details.test.ts`:

1. **Score calculation tests**:
   - Basic score calculation with default weights
   - Custom weights
   - Zero weights
   - Minimum time factor values

2. **Historical filtering tests**:
   - Issues older than 7 days
   - Issues newer than 7 days
   - Empty activity
   - Reaction timestamp filtering

3. **Contributor counting tests**:
   - Unique contributors
   - Duplicate handling
   - Empty assignees/comments

4. **Hot classification tests**:
   - Score > previousScore = Hot
   - Score <= previousScore = not Hot
