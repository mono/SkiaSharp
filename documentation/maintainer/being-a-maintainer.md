> [!NOTE]
> This document is still a draft.

Maintaining **our fork of Skia** and **SkiaSharp** is usually a straightforward process, but it can get complex when APIs change significantly. Still, it's a rewarding experience - SkiaSharp is one of only two major cross-platform graphics libraries, and it's used by a wide range of developers: from indie app creators to component vendors and full UI framework authors.

Given this variety of users, we’re always careful with how we introduce updates. We want to bring in new features and improvements, while doing our best to avoid breaking existing apps.

**Table of Contents**

- [Responsibilities](#responsibilities)
- [Update Cadence](#update-cadence)
- [Updating Native Skia](#updating-native-skia)
- [Updating .NET SkiaSharp](#updating-net-skiasharp)
- [Releasing Updates](#releasing-updates)
- [Compliance and Dependencies](#compliance-and-dependencies)


---

## Responsibilities

Being a maintainer has some core responsibilities other than updating the bindings. These include issue and PR management.

### Basic Rules & Expectations

* We aim to triage issues within 3 days
* We aim to triage issues the same day just after a release
* We aim to review PRs once a week
* We will release previews before any stable release goes out


### Issue Triage

We do not have a large number of issues being created, but triage is important to spot any regressions or bugs that are impacting customers. 

The process of triage is mostly a quick read and apply some labels. There is an engagement score to track to ensure nothing hot rises to the top needing more immediate attention.


> [!NOTE]
> We are in the process of testing out an AI agent to help with triage, labeling and scoring. 

#### Labels

There is a workflow that is not actually applying labels yet as we experiment, but can be used to see what labels the AI thinks we need: https://github.com/mono/SkiaSharp/actions/workflows/label-with-ai.yml

#### Scoring

There is also another AI workflow to try and calculate an "engagement score" based on comments, reactions and general interactions. This workflow is running now: https://github.com/mono/SkiaSharp/actions/workflows/engagement-scores.yml 

The project it writes to is still being evaluated to make sure our weights are good/useful: https://github.com/orgs/mono/projects/1/views/14


### PR Reviews

Similar to the issues, our PR influx is typically low. There are some fixes and new features from time to time.

Reviewing PRs is mostly making sure the APIs are useful to the general community and do not add any additional burden. However, most changes are smaller fixes or APIs that are useful and long standing.

It is important to make sure that the new code and bugfixes have tests demonstrating the feature. As the upgrades happen, we need to ensure that the feature is covered.


---

## Update Cadence

We typically update our fork of Skia and SkiaSharp **once or twice a year**. This slower cadence helps reduce instability, since the upstream Skia project (from Google) moves quickly and isn’t versioned in the traditional major/minor sense.

Skia operates as a moving target, with monthly "stabilization" branches. Google can (and often does) add, modify, or remove APIs at any time—Skia remains a public project but is still tightly integrated with internal Chromium and Android development.

Because of this, we aim to:
- **Avoid updating ahead of the stable Chromium branch**, to prevent instability.
- **Upstream fixes where possible**, though our slower update cycle means we often have to patch locally while we wait.

Chromium Release Channels: https://chromiumdash.appspot.com/branches

> [!IMPORTANT]
> Skia calls their versions "milestones".

|  Milestone | Channel         |
|------------|-----------------|
| Latest     | Canary          |
| Latest-1   | Dev             |
| Latest-2   | Beta            |
| Latest-3   | Stable          |

---

## Updating Native Skia

Updating native Skia is usually manageable, though it can become tricky when there are larger refactors or parallel edits from Google's side.

> [!WARNING]
> Skia does *not* guarantee API or ABI stability, so breaking changes are expected.

### General Update Steps

1. Review the upstream changelog for breaking changes.
2. Merge the target Skia version into a new branch from our current version.
3. Resolve any merge conflicts and get it compiling.
4. Identify and adapt to any changed or removed features.
5. Begin the corresponding SkiaSharp (.NET) update.

### Examples

- **Simple Update**: A method was renamed or moved. The compiler errors guide the fixes.
- **Complex Update**: A structural change, like the split of `SkPaint` into `SkPaint` and `SkFont`, required us to redesign the wrapper API to avoid breaking users.

Timelines can range from **a day** for simple merges to **a week or two** for more involved ones.

---

## Updating .NET SkiaSharp

Once native Skia is updated, updating SkiaSharp is typically more straightforward. Since we control the .NET API surface, we can mitigate upstream changes more gracefully.

### General Update Steps

1. Finish updating native Skia.
2. Update or rewrite .NET methods to match the new native APIs.
3. Add tests for everything.
4. Add more tests based on things in the changelog or new features.

We’re selective about which new native APIs to expose right away. Google iterates quickly, and early-stage APIs may be short-lived or evolve significantly. Lagging slightly behind can actually help — by the time we adopt a new feature, it’s often more stable.

We occasionally wrap a stable, future-looking API form even if the underlying native API is still in flux. This reduces future churn in our .NET surface.

---

## Releasing Updates

We aim to release updates as **previews as early as possible**. This allows our community to test changes and catch regressions early.

Some of our core users—including teams like **Uno Platform**, **Avalonia**, **Telerik**, and **Syncfusion** — are incredibly helpful in providing quick, actionable feedback. A special mention to [Nick from DrawnUI](https://github.com/taublast/DrawnUi), who consistently tests the latest features and pushes performance boundaries.

Since we’re not tied to any specific product cycle, we release when we feel the build is stable and feedback has been positive.

If regressions happen, they’re usually rare and quickly fixable, thanks to our established and reliable dependencies.

---

## Compliance and Dependencies

Google generally maintains solid compliance, but it’s always good to keep an eye on their `DEPS` file in native Skia for new dependencies.

We also:
- Rely on internal tooling to catch licensing or compliance issues.
- Use scanning tools when needed.
- Monitor our dependencies and bump versions as soon as any security issues are resolved.

In the rare case that a dependency (e.g., `libpng` or `zlib`) introduces a problem, we typically just wait a short time for the fix and update accordingly. These are very rare.
