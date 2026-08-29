# Release checklist framework

## Status and intent

This document specifies a minimal reusable C# `ReleaseChecklist` SDK. It models a hierarchical release process with authoritative desired-state checks, guarded mutations, exact approvals, and audit output. Durable truth remains in Git, GitHub, NuGet.org, and other public systems.

## Goals and non-goals

The SDK should:

- make a release definition readable as one C# composition file;
- preserve declared ordering and explicit fan-out/fan-in;
- use the same checks in preview, local execution, GitHub Actions, and Azure DevOps;
- make every mutation idempotent, capability-gated, observation-bound, scope-declared, and authoritatively rechecked; and
- expose public Git, GitHub, and NuGet primitives without coupling Core to protocol clients.

It is not a generic DAG engine, custom DSL, plugin system, persisted state machine, rollback coordinator, or internal publication-pipeline client. There are no forward references, arbitrary graph edges, or `DependsOn`.

## Assembly and host boundary

The first version may ship as one assembly with layered namespaces:

```text
ReleaseChecklist.Core
ReleaseChecklist.Git
ReleaseChecklist.GitHub
ReleaseChecklist.NuGet
```

References flow toward `Core`; `Core` does not reference a protocol namespace. The separate `SkiaSharp.ReleaseChecklist` console app supplies frozen inputs and repository policy without defining a second node hierarchy.

A host supplies parsed arguments, frozen environment inputs, credentials, an approval provider, caller cancellation, a bounded completion/recheck timeout, and output sinks. Adapters do not alter traversal, inject dependencies, or bypass checks. Credentials are released only for an approved exact capability and observation.

## Core types

Every node has a globally unique stable ID and a separate nonempty title. SDK status describes desired state only:

```csharp
public enum ChecklistStatus { Done, NotDone, Blocked, Skipped }

public readonly record struct ChecklistCapability(string Name)
{
    public string Name { get; init; } = CanonicalCapabilityName.Require(Name);
}

public enum MutationScopeKind
{
    LocalWorktree,
    GitRef,
    GitHubRelease,
    GitHubMilestones,
    GitHubWorkflowTarget,
}

public readonly record struct MutationScope(MutationScopeKind Kind, string Target)
{
    public string Target { get; init; } = CanonicalScopeTarget.Require(Kind, Target);
}
```

`ChecklistCapability.Name` uses the canonical ASCII form `^[a-z][a-z0-9.-]*$`. Scope targets use the protocol-specific canonical forms below. Constructors and `Build()` reject noncanonical values. Equality, ordering, hashing, and duplicate detection are ordinal. Applications expose typed static constants such as `ReleaseCapabilities.Branching`; they do not define a parallel capability enum.

Scopes are definition-time constants made from literals or frozen discovery, never `ChecklistValue<T>`. Their canonical targets are:

- `LocalWorktree`: the frozen worktree identity;
- `GitRef`: canonical repository plus full ref;
- `GitHubRelease`: frozen repository plus tag, never a runtime release ID;
- `GitHubMilestones`: the frozen repository milestone collection; and
- `GitHubWorkflowTarget`: canonical repository plus the workflow's effective full write branch, not its dispatch endpoint.

The node model is deliberately small:

```csharp
public abstract record ChecklistNode(string Id, string Title, IChecklistCondition Condition,
    IReadOnlySet<MutationScope> MutationScopes);
public abstract record ChecklistContainer(string Id, string Title, IChecklistCondition Condition,
    IReadOnlySet<MutationScope> MutationScopes)
    : ChecklistNode(Id, Title, Condition, MutationScopes);
public sealed record Step(string Id, string Title, IChecklistCondition Condition,
    IChecklistCheck Check, IChecklistAction? Action, ChecklistCapability? Capability,
    IReadOnlySet<MutationScope> MutationScopes, ChecklistContainer? Children = null)
    : ChecklistNode(Id, Title, Condition, MutationScopes);
public sealed record Sequence(string Id, string Title, IChecklistCondition Condition,
    IReadOnlySet<MutationScope> MutationScopes, IReadOnlyList<ChecklistNode> Children)
    : ChecklistContainer(Id, Title, Condition, MutationScopes);
public sealed record Parallel(string Id, string Title, IChecklistCondition Condition,
    IReadOnlySet<MutationScope> MutationScopes, IReadOnlyList<ChecklistNode> Children)
    : ChecklistContainer(Id, Title, Condition, MutationScopes);
```

`Step.Children`, when present, is exactly one `Sequence` or `Parallel`. Builders expose the common `IChecklistChildren` receiver; receiver-specific validation, not separate execution types, enforces where actions are safe. Builders store declared scopes on nodes. They may compute container unions outside a `Parallel`, but every direct parallel child must supply its complete set explicitly.

Conditions are pure. The runner evaluates and memoizes each node's condition exactly once per run. The node's check, `DesiredState`, recursive prechecks, and child traversal reuse that outcome. A false condition reports `Skipped` and does not enter the subtree.

## Checks, actions, values, and errors

```csharp
public sealed record CheckResult(ChecklistStatus Status, string Detail, Observation Observation);
public interface IChecklistCheck
{
    ValueTask<CheckResult> EvaluateAsync(ChecklistContext context, CancellationToken token);
}

public interface IChecklistAction
{
    ValueTask ExecuteAsync(ChecklistContext context, Observation approved, CancellationToken token);
}

public interface IConcurrentChecklistAction : IChecklistAction
{
    IReadOnlySet<MutationScope> MutationScopes { get; }
}
```

A mutating step's own check returns the observation approved for that same node and action. Wrapper actions implement `IConcurrentChecklistAction` or their wrapper declares their complete scopes. The generic delegate-action overload requires an explicit `mutationScope`; it never infers one from callback code.

`Observation` is a canonical ordered collection of typed scalar fields. Large or binary content is represented by identity metadata plus SHA256. Every action uses protocol compare-and-swap or `If-Match` when available. If the protocol has no conditional mutation, the action performs an immediate authoritative reread and exact observation comparison inside the action before mutating.

Unexpected failures are not desired-state values:

```csharp
public sealed record ExecutionError(string Phase, string Message, Exception Exception);
public sealed record NodeResult(ChecklistStatus Status,
    IReadOnlyList<NodePhaseRecord> Phases,
    IReadOnlyList<ExecutionError> Errors,
    bool CancellationObserved);
```

Reports append ordered phase records per node—condition, precheck, approval, action, postcheck—rather than overwriting one result. A check exception records an execution error and starts no action. An action exception records attempted/not completed, preserves its evidence, runs one bounded non-cancelable authoritative recheck, and starts no further action in that branch. There is no rollback.

Checks may publish typed values:

```csharp
public sealed class ChecklistValue<T>
{
    public T Get(ChecklistContext context);
    public bool TryGet(ChecklistContext context, out T value);
    public ChecklistValue<TResult> Select<TResult>(Func<T, TResult> selector);
}

public sealed record StepHandle<T>(ChecklistValue<T> Value, IChecklistCheck DesiredState);
```

The runner sets a value only when its producer reaches `Done`; `Get` throws otherwise and `TryGet` supports an optional value. Typed builder and wrapper parameters accept only producers that are ordered predecessors or reachable ancestors. An arbitrary callback may call `Get`, but remains runtime guarded. A conditional producer may feed a required consumer only when validation can prove consumer-condition implication; otherwise the consumer uses `TryGet`.

## Step execution and approval

```text
RunStep(step):
  use the memoized condition; if false, append condition=Skipped and return Skipped
  authoritatively check and append precheck
  on check error, record it and stop the branch
  publish a typed value only if the result is Done
  if result is NotDone and the step has an action authorized in execute mode:
    if caller cancellation is requested, record cancellation and do not start
    require the exact capability grant
    obtain approval for (step ID, capability, observation hash)
    freshly check and require an exact match to the approved observation; drift is an execution error
    start the action with operation credentials and the bounded completion token
    append attempted/completed action evidence
    authoritatively postcheck with the same bounded token
    publish a value only if the postcheck is Done
    if postcheck is not Done, record a convergence execution error with both observations
  if desired state is Blocked or NotDone, do not enter Children
  otherwise Run(Children), if present, and aggregate
```

Preview performs conditions and checks and reports available actions without approval or execution. A mutating step always owns its approval observation; a capability without an action is invalid.

The host binds approval to mutation node ID, exact capability, canonical observation hash, approver, actor-separation decision, and host gate. Before calling the action, the runner's fresh check must return the same status and hash. For `publish-release`, the check captures frozen tag, runtime release ID, source target, title, prerelease flag, exact body SHA256, observation time, and mutation kind. Any difference invalidates approval.

Caller cancellation is checked before every action. Once an action starts, action and postcheck use a bounded token independent of caller cancellation so the remote outcome is recorded. The run then reports caller cancellation. Already-running independent actions receive the same completion treatment; cancellation is not used to erase their outcomes.

An action error does not imply failure or success of the remote mutation, so the bounded recheck's desired-state status is retained alongside the execution error. Convergence failure is an execution error with pre- and post-observations, not a fabricated `Blocked` status.

## Sequence semantics

```text
RunSequence(sequence):
  for each child in declared order:
    result = Run(child)
    if result has an execution error or cancellation, mark later children not reached and stop
    if result status is Blocked or NotDone, mark later children not reached and stop
  aggregate all reached child statuses
```

`Done` and `Skipped` satisfy ordering. Ordered actions may reuse the same mutation scope.

## Parallel semantics

A `Parallel` is direct-branch fan-out:

```text
RunParallel(parallel):
  recursively Precheck each direct child branch with actions disabled; await every branch
  if any branch aggregates Blocked or has a precheck execution error:
    run no action anywhere in this Parallel scope; report every branch; aggregate and return
  concurrently Run(child) for every direct child branch to ordinary natural completion
  await and aggregate every branch, including every execution error and cancellation
```

The barrier is a one-time precheck of this parallel scope; it is not re-armed between action completions. `Run(child)` consumes barrier results for nodes already reached rather than repeating a scope-wide precheck; nodes first reached later check normally. It otherwise uses ordinary semantics, so a nested `Sequence` may execute several consecutive authorized steps. A new conflict, check error, action error, convergence error, or cancellation stops new actions only in that branch. Already-running independent actions may complete and recheck. Parallel collection is never fail-fast.

Each direct branch declares its complete set of mutation scopes. Its set must equal the union of all nested mutating action scopes; sibling branch sets must be disjoint, and an empty set is explicit for a read-only branch. A nested ordered `Sequence` may reuse a scope. There is no possible-frontier analysis: validation compares complete direct-branch sets, and execution fans out only direct children.

The same worktree, Git ref, release tag, milestone collection, or effective workflow write branch cannot occur in sibling sets. A read-only `NotDone` branch does not suppress an authorized independent sibling after the barrier.

## Aggregation

Desired-state aggregation is independent from execution errors:

- `Blocked` dominates `NotDone`, which dominates satisfied states.
- All `Skipped` yields `Skipped`.
- Otherwise all `Done`/`Skipped` yields `Done`.
- Any execution error remains attached and makes the run unsuccessful even if an authoritative reread reports `Done`.

`Check.All` evaluates every input. It returns `Blocked` if any is blocked, otherwise `NotDone` if any is not done, `Skipped` if all are skipped, and otherwise `Done` when all are `Done` or `Skipped`.

SDK `NotDone` is the normative checklist's **Waiting** state when no action is available. The report can render “Waiting,” but the SDK does not add a fifth status.

## Dependency composition and validation

The tree is the dependency model. An enclosing `Sequence` after a `Parallel` is natural fan-in. Use `Check.All` only when a later check must deliberately reread specific desired states. An explicit check such as `release-source-ready` can be an authoritative reread barrier after branch work; it is not required merely to join parallel children.

`Build()` validates all metadata and bound policy:

- IDs are nonempty and globally unique; titles are nonempty; node instances are not reused.
- Shapes, canonical capabilities, and canonical scopes are valid.
- Every action has exactly one capability and complete constant mutation scopes; no action means no capability.
- Every direct parallel branch declares exactly the union of its nested action scopes, and sibling sets are disjoint.
- `DesiredState` includes its producer's memoized condition.
- Typed producers are ordered predecessors or reachable ancestors, and required conditional uses prove implication.

Nested construction prevents cycles and forward references. Stage 1 has no custom source generator for definition validation. Source-generated `System.Text.Json` metadata may serialize reports later, but `Build()` owns definition validation.

## Public primitives and reviewed summary convergence

Protocol wrappers include Git branches/tags, GitHub branches/releases/pull requests/milestones/workflow dispatch, NuGet packages/receipts, and workflow convergence. Wrappers own idempotency, conflicts, authoritative checks, typed outputs, conditional desired state, and mutation scopes; they do not define another runner.

The reviewed release summary is not a direct full-body `GitHubRelease` write. The #4895 primitive is a check plus workflow dispatch/convergence operation. It owns published-only and unmarked-release skipping, managed-region-only patching, a batch drift barrier, authoritative post-write verification, and its effective workflow write-branch scope. It may appear as `GitHubWorkflowConvergence` in the ordered release-notes branch; there is no second direct writer.

## Discovery, reports, and testing

Discovery is read-only and completes before composition. It freezes typed inputs for `main`, maintenance `release/X.Y.x`, exact `release/{identity}`, explicitly validated hotfix sources, exact stable public versions, and unique prerelease versions from an exact public prefix. Missing and ambiguous results remain distinct `Discovered<T>` values with evidence.

Reports preserve the reached tree, ordered phases, desired status, not-reached reasons, observations and hashes, action availability/approval/attempt/completion, scopes, capabilities, actors, timestamps, warnings, cancellation, and every execution error. Reports are audit output, never resume state.

Core tests cover all statuses, `Check.All`, condition memoization, value guards, sequence stopping, parallel precheck barriers and natural branch completion, complete/disjoint scopes, approval drift, check/action errors, convergence, and cancellation. Protocol tests cover absent, matching, conflicting, concurrent-create, unauthorized, compare-and-swap, fallback reread, and postcheck cases. Composition tests build exactly one branch-selected public subtree for preview, RC, stable, and hotfix definitions.

## Staged implementation

1. Implement Core nodes, builders, `Build()` validation, values, checks, actions, approvals, reports, and deterministic runners.
2. Add Git primitives and worktree/ref scopes.
3. Add GitHub branch, release, pull request, milestone, workflow-dispatch, and workflow-convergence primitives.
4. Add NuGet package discovery and receipt verification.
5. Build the `SkiaSharp.ReleaseChecklist` composition app and host adapters.

Each layer ships with tests before the next layer. Later assembly splitting changes references, not composition or execution semantics.
