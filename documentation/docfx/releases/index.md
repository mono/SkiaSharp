# Release Notes

Release notes for SkiaSharp. SkiaSharp ships as NuGet packages whose minor version is the Chrome/Skia milestone it builds on. Two release lines are supported at a time — a **stable** line for production and a **preview** line for the milestone currently being stabilized — mirroring [Chrome's release channels](https://developer.chrome.com/docs/web-platform/chrome-release-channels) (stable / extended-stable and beta). Everything else stays published for reference but is no longer serviced.

## Release cadence

SkiaSharp 4.x follows Chrome's release cycle. Each SkiaSharp minor version corresponds to a Chrome/Skia milestone and progresses through four phases:

| Chrome Event | SkiaSharp Release | Purpose |
|---|---|---|
| Beta Promotion | Preview 1 | Merge upstream Skia, ship initial preview |
| Early Stable | Preview 2 | Bug fixes and API additions from preview feedback |
| Stable Cut | RC | Critical bug fixes only, no new features |
| Stable Release | Stable | Ship to NuGet.org, tag and create GitHub Release |

**Schedule for the two milestones currently in flight (m150 and m151), from the [Chromium release schedule](https://chromiumdash.appspot.com/schedule):**

| Date | Event | Package |
|------|-------|---------|
| Jun 3 | m150 Beta Promotion | `4.150.0-preview.1` |
| Jun 17 | m150 Early Stable | `4.150.0-preview.2` |
| Jun 23 | m150 Stable Cut | `4.150.0-rc.1` |
| Jun 30 | m150 Stable Release | `4.150.0` |
| Jul 1 | m151 Beta Promotion | `4.151.0-preview.1` |
| Jul 15 | m151 Early Stable | `4.151.0-preview.2` |
| Jul 21 | m151 Stable Cut | `4.151.0-rc.1` |
| Jul 28 | m151 Stable Release | `4.151.0` |

Two milestones are always in flight — as one enters its RC/stable phase, the next begins its preview phase.

> [!NOTE]
> Starting with Chrome 153 (September 2026), Chrome moves from a 4-week to a 3-week release cycle. Because SkiaSharp's cadence is driven by Chrome's actual schedule events, the phases above will naturally compress — preview through stable will complete in ~3 weeks instead of ~4.

### Versioning

Packages follow the scheme `4.{chrome_milestone}.{patch}` — the middle number **is** the Chrome milestone number. For example, `4.151.0` ships alongside Chrome 151's stable release.

- Preview: `4.151.0-preview.1`, `4.151.0-preview.2`
- Release candidate: `4.151.0-rc.1`
- Stable: `4.151.0`

Prerelease suffixes follow [NuGet semver conventions](https://learn.microsoft.com/nuget/concepts/package-versioning#pre-release-versions).

### Schedule reference

The full Chrome release calendar is published at [Chromium's release schedule](https://chromiumdash.appspot.com/schedule). SkiaSharp milestones are synced automatically from this schedule — check the [GitHub milestones](https://github.com/mono/SkiaSharp/milestones) for upcoming release dates.

## Support overview

- **Stable** — the line we recommend for production apps. Tracks Chrome's Stable / Extended Stable channel.
- **Preview** — prerelease NuGets for the next milestone, so you can test ahead of its stable release. Tracks Chrome's Beta channel.
- **Out of support** — older 3.x / 4.x lines, still listed below for reference but no longer serviced.
- **Obsolete** — SkiaSharp 1.x and 2.x, no longer maintained.

| Path | Version line | Latest release |
|------|--------------|----------------|
| Stable | 4.148.x | [4.148.0](4.148.0.md) |
| Preview | 4.150.x | [4.150.0](4.150.0.md) |

## Supported versions

- **Version 4.150.x** — Preview
  - [Version 4.150.0](4.150.0.md)
  - [Version 4.150.0 (Unreleased)](4.150.0-unreleased.md)
- **Version 4.148.x** — Stable
  - [Version 4.148.1 (Unreleased)](4.148.1-unreleased.md)
  - [Version 4.148.0](4.148.0.md)

## Out of support

These SkiaSharp 3.x and 4.x lines are no longer supported. They remain available for reference.

<details>
<summary>Show out-of-support releases</summary>

- **Version 4.147.x**
  - [Version 4.147.0](4.147.0.md)
- **Version 3.119.x**
  - [Version 3.119.5 (Unreleased)](3.119.5-unreleased.md)
  - [Version 3.119.4](3.119.4.md)
  - [Version 3.119.3](3.119.3.md)
  - [Version 3.119.2](3.119.2.md)
  - [Version 3.119.1](3.119.1.md)
  - [Version 3.119.0](3.119.0.md)
- **Version 3.118.x**
  - [Version 3.118.0](3.118.0.md)
- **Version 3.116.x**
  - [Version 3.116.1](3.116.1.md)
  - [Version 3.116.0](3.116.0.md)
- **Version 3.0.x**
  - [Version 3.0.0](3.0.0.md)

</details>

## Obsolete versions

SkiaSharp 1.x and 2.x are obsolete and no longer maintained.

<details>
<summary>Show obsolete releases</summary>

- **Version 2.88.x**
  - [Version 2.88.9](2.88.9.md)
  - [Version 2.88.8](2.88.8.md)
  - [Version 2.88.7](2.88.7.md)
  - [Version 2.88.6](2.88.6.md)
  - [Version 2.88.5](2.88.5.md)
  - [Version 2.88.4](2.88.4.md)
  - [Version 2.88.3](2.88.3.md)
  - [Version 2.88.2](2.88.2.md)
  - [Version 2.88.1](2.88.1.md)
  - [Version 2.88.0](2.88.0.md)
- **Version 2.80.x**
  - [Version 2.80.4](2.80.4.md)
  - [Version 2.80.3](2.80.3.md)
  - [Version 2.80.2](2.80.2.md)
  - [Version 2.80.1](2.80.1.md)
  - [Version 2.80.0](2.80.0.md)
- **Version 1.68.x**
  - [Version 1.68.3](1.68.3.md)
  - [Version 1.68.2.1](1.68.2.1.md)
  - [Version 1.68.2](1.68.2.md)
  - [Version 1.68.1.1](1.68.1.1.md)
  - [Version 1.68.1](1.68.1.md)
  - [Version 1.68.0](1.68.0.md)
- **Version 1.60.x**
  - [Version 1.60.3](1.60.3.md)
  - [Version 1.60.2](1.60.2.md)
  - [Version 1.60.1](1.60.1.md)
  - [Version 1.60.0](1.60.0.md)
- **Version 1.59.x**
  - [Version 1.59.3](1.59.3.md)
  - [Version 1.59.2](1.59.2.md)
  - [Version 1.59.1.1](1.59.1.1.md)
  - [Version 1.59.1](1.59.1.md)
  - [Version 1.59.0](1.59.0.md)
- **Version 1.58.x**
  - [Version 1.58.1.1](1.58.1.1.md)
  - [Version 1.58.1](1.58.1.md)
  - [Version 1.58.0](1.58.0.md)
- **Version 1.57.x**
  - [Version 1.57.1](1.57.1.md)
  - [Version 1.57.0](1.57.0.md)
- **Version 1.56.x**
  - [Version 1.56.2](1.56.2.md)
  - [Version 1.56.1](1.56.1.md)
  - [Version 1.56.0](1.56.0.md)
- **Version 1.55.x**
  - [Version 1.55.1](1.55.1.md)
  - [Version 1.55.0](1.55.0.md)
- **Version 1.54.x**
  - [Version 1.54.1.1](1.54.1.1.md)
  - [Version 1.54.1](1.54.1.md)
  - [Version 1.54.0](1.54.0.md)
- **Version 1.53.x**
  - [Version 1.53.2](1.53.2.md)
  - [Version 1.53.1.2](1.53.1.2.md)
  - [Version 1.53.1.1](1.53.1.1.md)
  - [Version 1.53.1](1.53.1.md)
  - [Version 1.53.0](1.53.0.md)
- **Version 1.49.x**
  - [Version 1.49.4](1.49.4.md)
  - [Version 1.49.3](1.49.3.md)
  - [Version 1.49.2.1](1.49.2.1.md)
  - [Version 1.49.2](1.49.2.md)
  - [Version 1.49.1](1.49.1.md)
  - [Version 1.49.0](1.49.0.md)

</details>

## HarfBuzzSharp

- **HarfBuzzSharp 14.2.x**
  - [HarfBuzzSharp 14.2.1](harfbuzzsharp/14.2.1.md)
  - [HarfBuzzSharp 14.2.0](harfbuzzsharp/14.2.0.md)
- **HarfBuzzSharp 8.3.x**
  - [HarfBuzzSharp 8.3.1.6 (Unreleased)](harfbuzzsharp/8.3.1.6-unreleased.md)
  - [HarfBuzzSharp 8.3.1.5](harfbuzzsharp/8.3.1.5.md)
  - [HarfBuzzSharp 8.3.1.3](harfbuzzsharp/8.3.1.3.md)
  - [HarfBuzzSharp 8.3.1.2](harfbuzzsharp/8.3.1.2.md)
  - [HarfBuzzSharp 8.3.1.1](harfbuzzsharp/8.3.1.1.md)
  - [HarfBuzzSharp 8.3.1 (Unreleased)](harfbuzzsharp/8.3.1-unreleased.md)
  - [HarfBuzzSharp 8.3.0.1](harfbuzzsharp/8.3.0.1.md)
  - [HarfBuzzSharp 8.3.0](harfbuzzsharp/8.3.0.md)
- **HarfBuzzSharp 7.3.x**
  - [HarfBuzzSharp 7.3.0.3](harfbuzzsharp/7.3.0.3.md)
  - [HarfBuzzSharp 7.3.0.2](harfbuzzsharp/7.3.0.2.md)
  - [HarfBuzzSharp 7.3.0.1](harfbuzzsharp/7.3.0.1.md)
  - [HarfBuzzSharp 7.3.0](harfbuzzsharp/7.3.0.md)
- **HarfBuzzSharp 2.8.x**
  - [HarfBuzzSharp 2.8.2.5](harfbuzzsharp/2.8.2.5.md)
  - [HarfBuzzSharp 2.8.2.4](harfbuzzsharp/2.8.2.4.md)
  - [HarfBuzzSharp 2.8.2.3](harfbuzzsharp/2.8.2.3.md)
  - [HarfBuzzSharp 2.8.2.2](harfbuzzsharp/2.8.2.2.md)
  - [HarfBuzzSharp 2.8.2.1](harfbuzzsharp/2.8.2.1.md)
  - [HarfBuzzSharp 2.8.2](harfbuzzsharp/2.8.2.md)
- **HarfBuzzSharp 2.6.x**
  - [HarfBuzzSharp 2.6.1.9](harfbuzzsharp/2.6.1.9.md)
  - [HarfBuzzSharp 2.6.1.8](harfbuzzsharp/2.6.1.8.md)
  - [HarfBuzzSharp 2.6.1.7](harfbuzzsharp/2.6.1.7.md)
  - [HarfBuzzSharp 2.6.1.6](harfbuzzsharp/2.6.1.6.md)
  - [HarfBuzzSharp 2.6.1.5](harfbuzzsharp/2.6.1.5.md)
  - [HarfBuzzSharp 2.6.1.4](harfbuzzsharp/2.6.1.4.md)
  - [HarfBuzzSharp 2.6.1.3](harfbuzzsharp/2.6.1.3.md)
  - [HarfBuzzSharp 2.6.1.2](harfbuzzsharp/2.6.1.2.md)
  - [HarfBuzzSharp 2.6.1.1](harfbuzzsharp/2.6.1.1.md)
  - [HarfBuzzSharp 2.6.1](harfbuzzsharp/2.6.1.md)
- **HarfBuzzSharp 1.4.x**
  - [HarfBuzzSharp 1.4.6.2](harfbuzzsharp/1.4.6.2.md)
  - [HarfBuzzSharp 1.4.6.1](harfbuzzsharp/1.4.6.1.md)
  - [HarfBuzzSharp 1.4.6](harfbuzzsharp/1.4.6.md)
  - [HarfBuzzSharp 1.4.5](harfbuzzsharp/1.4.5.md)
