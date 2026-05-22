// Auto-milestone logic for SkiaSharp PRs.
// Used by auto-milestone.yml workflow and testable via auto-milestone.test.js.

'use strict';

/**
 * Parse a version string into comparable components.
 * "4.148.0-preview.1" → { major: 4, minor: 148, patch: 0, prerelease: "preview.1" }
 * "3.119.x" → { major: 3, minor: 119, patch: -1, prerelease: null }
 */
function parseVersion(title) {
  const match = title.match(/^(\d+)\.(\d+)\.(\d+|x)(?:-([a-zA-Z0-9.]+))?$/);
  if (!match) return null;
  return {
    major: parseInt(match[1], 10),
    minor: parseInt(match[2], 10),
    patch: match[3] === 'x' ? -1 : parseInt(match[3], 10),
    prerelease: match[4] || null,
  };
}

/**
 * Compare two parsed versions for sorting.
 * Stable versions (no prerelease) sort AFTER prereleases of the same base version.
 * This matches semver: 4.148.0-preview.1 < 4.148.0-preview.2 < 4.148.0-rc.1 < 4.148.0
 */
function compareVersions(a, b) {
  if (a.major !== b.major) return a.major - b.major;
  if (a.minor !== b.minor) return a.minor - b.minor;
  if (a.patch !== b.patch) return a.patch - b.patch;

  if (!a.prerelease && !b.prerelease) return 0;
  if (!a.prerelease) return 1;  // stable sorts AFTER prerelease
  if (!b.prerelease) return -1;

  return a.prerelease.localeCompare(b.prerelease, undefined, { numeric: true });
}

/**
 * Check if a milestone's base version (major.minor.patch) matches the target.
 */
function isBaseVersionMatch(milestoneVersion, targetVersion) {
  return milestoneVersion.major === targetVersion.major
    && milestoneVersion.minor === targetVersion.minor
    && milestoneVersion.patch === targetVersion.patch;
}

/**
 * Derive the milestone name from a branch name and current version.
 *
 * @param {string} baseBranch - The PR's base branch (e.g., "main", "release/3.119.x")
 * @param {string} skiaSharpVersion - Current version from VERSIONS.txt (e.g., "4.147.0")
 * @param {string[]} openMilestones - Titles of all open milestones
 * @returns {string|null} The milestone name to assign, or null if undetermined
 */
function resolveMilestone(baseBranch, skiaSharpVersion, openMilestones) {
  // Specific release branches (not servicing) use the branch name directly
  if (baseBranch.startsWith('release/') && !baseBranch.endsWith('.x')) {
    const name = baseBranch.slice('release/'.length);
    return parseVersion(name) ? name : null;
  }

  // For main and release/*.x branches, use pipeline variables to find the target
  if (baseBranch === 'main' || (baseBranch.startsWith('release/') && baseBranch.endsWith('.x'))) {
    if (!skiaSharpVersion) return null;

    const currentVersion = parseVersion(skiaSharpVersion);
    if (!currentVersion) return null;
    if (currentVersion.patch === -1) return null;

    // If the version has a specific prerelease (e.g., preview.3, rc.1),
    // the milestone must be that exact version — no searching.
    if (currentVersion.prerelease) {
      return skiaSharpVersion;
    }

    // Stable target (preview.0) — find lowest open milestone in the same train
    const candidates = openMilestones
      .map(title => ({ title, parsed: parseVersion(title) }))
      .filter(({ parsed }) => {
        if (!parsed) return false;
        if (parsed.patch === -1) return false;
        return isBaseVersionMatch(parsed, currentVersion);
      })
      .sort((a, b) => compareVersions(a.parsed, b.parsed));

    if (candidates.length > 0) {
      return candidates[0].title;
    }

    // No matching milestone exists — create the base version
    return skiaSharpVersion;
  }

  return null;
}

/**
 * Find or create a milestone by title.
 * Handles race conditions (422 on create → re-fetch).
 */
async function findOrCreateMilestone({ milestoneName, openMilestones, closedMilestones, createMilestone, listMilestones, dryRun, log }) {
  let milestone = openMilestones.find(m => m.title === milestoneName);

  if (!milestone) {
    milestone = closedMilestones.find(m => m.title === milestoneName);
    if (milestone) {
      log(`⚠️ Milestone "${milestoneName}" is closed — assigning anyway (late merge or backport)`);
    }
  }

  if (!milestone) {
    if (dryRun) {
      log(`[DRY RUN] Would create milestone "${milestoneName}"`);
      return null;
    }

    log(`Creating milestone "${milestoneName}"`);
    try {
      milestone = await createMilestone(milestoneName);
    } catch (e) {
      if (e.status === 422) {
        log(`Milestone "${milestoneName}" was created by another run, re-fetching...`);
        const refreshed = await listMilestones();
        milestone = refreshed.find(m => m.title === milestoneName);
        if (!milestone) throw e;
      } else {
        throw e;
      }
    }
  }

  return milestone;
}

/**
 * Extract SKIASHARP_VERSION and PREVIEW_LABEL from azure-templates-variables.yml content.
 * Combines them into the effective target version:
 *   - PREVIEW_LABEL = "preview.0" → base version only (e.g., "4.147.0")
 *   - PREVIEW_LABEL = "preview.3" → "4.147.0-preview.3"
 *   - PREVIEW_LABEL = "rc.1" → "4.147.0-rc.1"
 *
 * @param {string} content - File content of azure-templates-variables.yml
 * @returns {string|null} The effective target version, or null if not found
 */
function parsePipelineVariables(content) {
  const versionMatch = content.match(/^\s*SKIASHARP_VERSION:\s*['"]?(\S+?)['"]?\s*$/m);
  if (!versionMatch) return null;
  const baseVersion = versionMatch[1];

  const labelMatch = content.match(/^\s*PREVIEW_LABEL:\s*['"]?(\S+?)['"]?\s*$/m);
  const label = labelMatch ? labelMatch[1] : null;

  // "preview.0" means stable target (no prerelease suffix)
  if (!label || label === 'preview.0') {
    return baseVersion;
  }

  return `${baseVersion}-${label}`;
}

module.exports = { parseVersion, compareVersions, isBaseVersionMatch, resolveMilestone, findOrCreateMilestone, parsePipelineVariables };
