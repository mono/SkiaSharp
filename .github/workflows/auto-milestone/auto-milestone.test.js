#!/usr/bin/env node
// Tests for auto-milestone logic.
// Run: node .github/workflows/auto-milestone/auto-milestone.test.js

'use strict';

const { parseVersion, compareVersions, isBaseVersionMatch, resolveMilestone, findOrCreateMilestone, parsePipelineVariables } = require('./auto-milestone.js');

let passed = 0;
let failed = 0;

function assert(condition, msg) {
  if (condition) { passed++; console.log(`  ✅ ${msg}`); }
  else { failed++; console.log(`  ❌ ${msg}`); }
}

function assertEqual(actual, expected, msg) {
  if (actual === expected) { passed++; console.log(`  ✅ ${msg}`); }
  else { failed++; console.log(`  ❌ ${msg}: expected "${expected}", got "${actual}"`); }
}

// --- parseVersion ---
console.log('\n📋 parseVersion');
{
  const v = parseVersion('4.148.0-preview.1');
  assert(v !== null, 'parses 4.148.0-preview.1');
  assertEqual(v.major, 4, '  major = 4');
  assertEqual(v.minor, 148, '  minor = 148');
  assertEqual(v.patch, 0, '  patch = 0');
  assertEqual(v.prerelease, 'preview.1', '  prerelease = preview.1');
}
{
  const v = parseVersion('3.119.x');
  assert(v !== null, 'parses 3.119.x');
  assertEqual(v.patch, -1, '  patch = -1 (wildcard)');
  assertEqual(v.prerelease, null, '  prerelease = null');
}
{
  const v = parseVersion('4.147.0');
  assert(v !== null, 'parses 4.147.0 (stable)');
  assertEqual(v.prerelease, null, '  prerelease = null');
}
{
  assertEqual(parseVersion('Backlog'), null, 'Backlog returns null');
}
{
  assertEqual(parseVersion('4.148'), null, '2-part version (no patch) returns null');
}
{
  assertEqual(parseVersion('4.148.0-preview.1/foo'), null, 'slash in prerelease returns null');
}

// --- compareVersions ---
console.log('\n📋 compareVersions');
{
  const a = parseVersion('4.148.0-preview.1');
  const b = parseVersion('4.148.0-preview.2');
  assert(compareVersions(a, b) < 0, 'preview.1 < preview.2');
}
{
  const a = parseVersion('4.148.0-preview.2');
  const b = parseVersion('4.148.0-rc.1');
  assert(compareVersions(a, b) < 0, 'preview.2 < rc.1');
}
{
  const a = parseVersion('4.148.0-rc.1');
  const b = parseVersion('4.148.0');
  assert(compareVersions(a, b) < 0, 'rc.1 < stable (4.148.0)');
}
{
  const a = parseVersion('3.119.0-preview.1');
  const b = parseVersion('4.147.0-preview.1');
  assert(compareVersions(a, b) < 0, '3.119 < 4.147');
}
{
  const a = parseVersion('4.147.0-preview.9');
  const b = parseVersion('4.147.0-preview.10');
  assert(compareVersions(a, b) < 0, 'preview.9 < preview.10 (numeric)');
}

// --- isBaseVersionMatch ---
console.log('\n📋 isBaseVersionMatch');
{
  assert(isBaseVersionMatch(parseVersion('4.147.0-preview.3'), parseVersion('4.147.0')), '4.147.0-preview.3 matches 4.147.0 base');
  assert(isBaseVersionMatch(parseVersion('4.147.0-rc.1'), parseVersion('4.147.0')), '4.147.0-rc.1 matches 4.147.0 base');
  assert(isBaseVersionMatch(parseVersion('4.147.0'), parseVersion('4.147.0')), '4.147.0 matches 4.147.0 (exact)');
  assert(!isBaseVersionMatch(parseVersion('4.148.0-preview.1'), parseVersion('4.147.0')), '4.148.0-preview.1 does NOT match 4.147.0');
  assert(!isBaseVersionMatch(parseVersion('3.119.0'), parseVersion('4.147.0')), '3.119.0 does NOT match 4.147.0');
}

// --- resolveMilestone ---
console.log('\n📋 resolveMilestone — stable target (preview.0)');
assertEqual(
  resolveMilestone('4.147.0', ['4.147.0-preview.3', '4.147.0-rc.1', '4.147.0', '4.148.0-preview.1', 'Backlog']),
  '4.147.0-preview.3',
  'picks lowest milestone in the 4.147.0 train (preview.3)'
);
assertEqual(
  resolveMilestone('4.147.0', ['4.147.0-rc.1', '4.147.0', '4.148.0-preview.1']),
  '4.147.0-rc.1',
  'picks lowest in train when preview is closed (rc.1)'
);
assertEqual(
  resolveMilestone('4.147.0', ['4.147.0', '4.148.0-preview.1']),
  '4.147.0',
  'picks stable milestone when it is the only one in the train'
);
assertEqual(
  resolveMilestone('4.147.0', ['4.148.0-preview.1', '4.148.0', 'Backlog']),
  '4.147.0',
  'no milestones matching 4.147.0 base → creates 4.147.0'
);
assertEqual(
  resolveMilestone('3.119.4', ['3.119.4', '3.119.5-preview.1']),
  '3.119.4',
  'servicing: picks matching base version'
);
assertEqual(
  resolveMilestone('3.119.5', ['3.119.4']),
  '3.119.5',
  'servicing: no match → creates exact version'
);
assertEqual(
  resolveMilestone('4.147.0', ['3.119.x', '4.148.x', 'Backlog']),
  '4.147.0',
  'ignores .x milestones and higher versions → creates base version'
);

console.log('\n📋 resolveMilestone — specific prerelease');
assertEqual(
  resolveMilestone('4.148.0-preview.2', ['4.148.0-preview.2', '4.148.0-rc.1', '4.148.0']),
  '4.148.0-preview.2',
  'prerelease → exact milestone (preview.2)'
);
assertEqual(
  resolveMilestone('4.148.0-preview.2', ['Backlog']),
  '4.148.0-preview.2',
  'prerelease with no match → creates exact (preview.2)'
);
assertEqual(
  resolveMilestone('4.148.0-rc.1', ['4.148.0-preview.2', '4.148.0-rc.1']),
  '4.148.0-rc.1',
  'rc → exact milestone (rc.1)'
);

console.log('\n📋 resolveMilestone — edge cases');
assertEqual(resolveMilestone(null, ['4.148.0-preview.1']), null, 'null version → null');
assertEqual(resolveMilestone('', ['4.148.0-preview.1']), null, 'empty version → null');
assertEqual(resolveMilestone('not-a-version', ['4.148.0-preview.1']), null, 'invalid version → null');
assertEqual(resolveMilestone('3.119.x', ['4.148.0-preview.1']), null, 'wildcard version → null');

// --- findOrCreateMilestone ---
console.log('\n📋 findOrCreateMilestone');
(async () => {
  // Found in open
  {
    const result = await findOrCreateMilestone({
      milestoneName: '4.148.0-preview.1',
      openMilestones: [{ title: '4.148.0-preview.1', number: 61 }],
      closedMilestones: [],
      createMilestone: async () => { throw new Error('should not create'); },
      listMilestones: async () => [],
      dryRun: false,
      log: () => {},
    });
    assertEqual(result.number, 61, 'finds existing open milestone');
  }

  // Found in closed (with warning)
  {
    const logs = [];
    const result = await findOrCreateMilestone({
      milestoneName: '3.119.4',
      openMilestones: [],
      closedMilestones: [{ title: '3.119.4', number: 50 }],
      createMilestone: async () => { throw new Error('should not create'); },
      listMilestones: async () => [],
      dryRun: false,
      log: (msg) => logs.push(msg),
    });
    assertEqual(result.number, 50, 'finds existing closed milestone');
    assert(logs[0].includes('closed'), '  warns about closed milestone');
  }

  // Creates new
  {
    const result = await findOrCreateMilestone({
      milestoneName: '4.149.0-preview.1',
      openMilestones: [],
      closedMilestones: [],
      createMilestone: async (title) => ({ title, number: 99 }),
      listMilestones: async () => [],
      dryRun: false,
      log: () => {},
    });
    assertEqual(result.number, 99, 'creates new milestone');
  }

  // Handles 422 race condition
  {
    let createAttempts = 0;
    const result = await findOrCreateMilestone({
      milestoneName: '4.149.0-preview.1',
      openMilestones: [],
      closedMilestones: [],
      createMilestone: async () => { createAttempts++; const e = new Error('exists'); e.status = 422; throw e; },
      listMilestones: async () => [{ title: '4.149.0-preview.1', number: 100 }],
      dryRun: false,
      log: () => {},
    });
    assertEqual(result.number, 100, 'handles 422 race → re-fetches');
    assertEqual(createAttempts, 1, '  only tried create once');
  }

  // Dry run returns null when milestone doesn't exist
  {
    const logs = [];
    const result = await findOrCreateMilestone({
      milestoneName: '4.149.0-preview.1',
      openMilestones: [],
      closedMilestones: [],
      createMilestone: async () => { throw new Error('should not create'); },
      listMilestones: async () => [],
      dryRun: true,
      log: (msg) => logs.push(msg),
    });
    assertEqual(result, null, 'dry-run returns null when milestone missing');
    assert(logs[0].includes('DRY RUN'), '  logs dry-run message');
  }

  // Dry run returns milestone object when it already exists (workflow must check DRY_RUN before using it)
  {
    const logs = [];
    const result = await findOrCreateMilestone({
      milestoneName: '4.148.0-preview.1',
      openMilestones: [{ title: '4.148.0-preview.1', number: 42 }],
      closedMilestones: [],
      createMilestone: async () => { throw new Error('should not create'); },
      listMilestones: async () => [],
      dryRun: true,
      log: (msg) => logs.push(msg),
    });
    assertEqual(result?.number, 42, 'dry-run with existing milestone returns it (caller must guard)');
  }

  // --- parsePipelineVariables ---
  console.log('\n📋 parsePipelineVariables');
  assertEqual(
    parsePipelineVariables("  SKIASHARP_VERSION: 4.147.0\n  PREVIEW_LABEL: 'preview.0'\n"),
    '4.147.0',
    'preview.0 means stable target'
  );
  assertEqual(
    parsePipelineVariables("  SKIASHARP_VERSION: 4.147.0\n  PREVIEW_LABEL: 'preview.3'\n"),
    '4.147.0-preview.3',
    'preview.3 appends prerelease suffix'
  );
  assertEqual(
    parsePipelineVariables("  SKIASHARP_VERSION: 4.148.0\n  PREVIEW_LABEL: 'rc.1'\n"),
    '4.148.0-rc.1',
    'rc.1 appends rc suffix'
  );
  assertEqual(
    parsePipelineVariables("  SKIASHARP_VERSION: 4.147.0\n"),
    '4.147.0',
    'missing PREVIEW_LABEL defaults to stable'
  );
  assertEqual(
    parsePipelineVariables("  PREVIEW_LABEL: 'preview.3'\n"),
    null,
    'missing SKIASHARP_VERSION returns null'
  );
  assertEqual(
    parsePipelineVariables(''),
    null,
    'empty content returns null'
  );

  // --- Final summary ---
  console.log(`\n${passed + failed} tests: ${passed} passed, ${failed} failed`);
  process.exit(failed > 0 ? 1 : 0);
})().catch(e => { console.error(e); process.exit(1); });
