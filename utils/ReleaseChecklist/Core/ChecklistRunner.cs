using System.Collections.Concurrent;
namespace ReleaseChecklist.Core;

/// <summary>Runs validated checklist definitions in dry-run or apply mode.</summary>
public static class ChecklistRunner
{
	/// <summary>Asynchronously runs a checklist definition.</summary>
	/// <param name="definition">The validated definition to run.</param>
	/// <param name="options">The run options, or <see langword="null" /> to use dry-run defaults.</param>
	/// <param name="cancellationToken">A token that cancels checks and prevents new actions from starting.</param>
	/// <returns>The structured checklist report.</returns>
	/// <exception cref="ArgumentOutOfRangeException">The completion timeout is not positive.</exception>
	public static async Task<ChecklistReport> RunAsync(
		ChecklistDefinition definition,
		ChecklistRunOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		options ??= new ChecklistRunOptions();
		if (options.CompletionTimeout <= TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(options), "Completion timeout must be positive.");
		var runner = new Runner(options, cancellationToken);
		return new ChecklistReport(await runner.RunNodeAsync(definition.Root).ConfigureAwait(false));
	}

	private sealed class Runner(ChecklistRunOptions options, CancellationToken callerCancellation)
	{
		private readonly ConcurrentDictionary<ChecklistNode, Lazy<Task<bool>>> conditions =
			new(ReferenceEqualityComparer.Instance);

		public async Task<NodeResult> RunNodeAsync(ChecklistNode node)
		{
			var condition = await EvaluateConditionAsync(node).ConfigureAwait(false);
			if (condition.Result is not null)
				return condition.Result;

			return node switch
			{
				Step step => await RunStepAsync(step, condition.Phase!).ConfigureAwait(false),
				Sequence sequence => await RunSequenceAsync(sequence, condition.Phase!).ConfigureAwait(false),
				Parallel parallel => await RunParallelAsync(parallel, condition.Phase!).ConfigureAwait(false),
				_ => throw new InvalidOperationException($"Unknown node type {node.GetType().Name}."),
			};
		}

		private async Task<(NodePhaseRecord? Phase, NodeResult? Result)> EvaluateConditionAsync(ChecklistNode node)
		{
			var result = new NodeResult(node);
			try
			{
				var lazy = conditions.GetOrAdd(
					node,
					static (key, token) => new Lazy<Task<bool>>(
						() => key.Condition.EvaluateAsync(token).AsTask(),
						LazyThreadSafetyMode.ExecutionAndPublication),
					callerCancellation);
				var enabled = await lazy.Value.ConfigureAwait(false);
				var phase = new NodePhaseRecord(
					ChecklistPhase.Condition,
					enabled ? null : ChecklistStatus.Skipped,
					enabled ? "Condition satisfied." : "Condition false; node skipped.");
				if (enabled)
					return (phase, null);

				result.Status = ChecklistStatus.Skipped;
				result.Detail = phase.Detail;
				result.Phases.Add(phase);
				MarkDescendantsNotReached(node, result, "Parent condition was false.");
				return (null, result);
			}
			catch (OperationCanceledException) when (callerCancellation.IsCancellationRequested)
			{
				result.Detail = "Condition canceled.";
				result.CancellationObserved = true;
				result.Phases.Add(new NodePhaseRecord(
					ChecklistPhase.Condition, null, "Caller cancellation observed."));
				MarkDescendantsNotReached(node, result, "Parent condition was canceled.");
				return (null, result);
			}
			catch (Exception ex)
			{
				result.Detail = "Condition failed.";
				result.Errors.Add(new ExecutionError(ChecklistPhase.Condition, ex.Message, ex));
				result.Phases.Add(new NodePhaseRecord(ChecklistPhase.Condition, null, ex.Message));
				MarkDescendantsNotReached(node, result, "Parent condition failed.");
				return (null, result);
			}
		}

		private async Task<NodeResult> RunStepAsync(Step step, NodePhaseRecord conditionPhase)
		{
			var result = new NodeResult(step);
			result.Phases.Add(conditionPhase);
			if (step.Check is null)
			{
				result.Status = ChecklistStatus.NotDone;
				result.Detail = "Action-only step runs on every apply.";
				result.ActionAvailable = true;
				result.Phases.Add(new NodePhaseRecord(
					ChecklistPhase.Precheck,
					ChecklistStatus.NotDone,
					result.Detail));
				if (options.Mode == ChecklistRunMode.Apply)
					await RunActionOnlyAsync(step, result).ConfigureAwait(false);
				return result;
			}

			CheckResult checkedState;
			try
			{
				checkedState = await step.Check.EvaluateAsync(callerCancellation).ConfigureAwait(false);
				result.Phases.Add(Phase(ChecklistPhase.Precheck, checkedState));
			}
			catch (OperationCanceledException) when (callerCancellation.IsCancellationRequested)
			{
				result.Detail = "Precheck canceled.";
				result.CancellationObserved = true;
				result.Phases.Add(new NodePhaseRecord(
					ChecklistPhase.Precheck, null, "Caller cancellation observed."));
				return result;
			}

			catch (Exception ex)
			{
				result.Detail = "Precheck failed.";
				result.Errors.Add(new ExecutionError(ChecklistPhase.Precheck, ex.Message, ex));
				result.Phases.Add(new NodePhaseRecord(ChecklistPhase.Precheck, null, ex.Message));
				return result;
			}

			result.Status = checkedState.Status;
			result.Detail = checkedState.Detail;

			result.ActionAvailable = checkedState.Status == ChecklistStatus.NotDone && step.Action is not null;
			if (result.ActionAvailable && options.Mode == ChecklistRunMode.Apply)
			{
				await TryExecuteAsync(step, checkedState, result).ConfigureAwait(false);
			}

			return result;
		}

		private async Task RunActionOnlyAsync(Step step, NodeResult result)
		{
			if (callerCancellation.IsCancellationRequested)
			{
				result.CancellationObserved = true;
				return;
			}

			using var completion = new CancellationTokenSource(options.CompletionTimeout);
			result.ActionAttempted = true;
			try
			{
				await step.Action!.ExecuteAsync(completion.Token).ConfigureAwait(false);
				result.ActionCompleted = true;
				result.Status = ChecklistStatus.Done;
				result.Detail = "Action-only step completed.";
				result.Phases.Add(new NodePhaseRecord(ChecklistPhase.Action, result.Status, result.Detail));
			}
			catch (Exception ex)
			{
				result.Errors.Add(new ExecutionError(ChecklistPhase.Action, ex.Message, ex));
				result.Phases.Add(new NodePhaseRecord(ChecklistPhase.Action, result.Status, ex.Message));
			}

			if (callerCancellation.IsCancellationRequested)
				result.CancellationObserved = true;
		}

		private async Task TryExecuteAsync(Step step, CheckResult precheckedState, NodeResult result)
		{
			if (callerCancellation.IsCancellationRequested)
			{
				result.CancellationObserved = true;
				result.Phases.Add(new NodePhaseRecord(
					ChecklistPhase.PreMutation, result.Status, "Caller cancellation prevented action start."));
				return;
			}

			CheckResult fresh;
			try
			{
				fresh = await step.Check!.EvaluateAsync(callerCancellation).ConfigureAwait(false);
				result.Phases.Add(Phase(ChecklistPhase.PreMutation, fresh));
			}
			catch (OperationCanceledException) when (callerCancellation.IsCancellationRequested)
			{
				result.CancellationObserved = true;
				result.Phases.Add(new NodePhaseRecord(
					ChecklistPhase.PreMutation, null, "Caller cancellation observed."));
				return;
			}
			catch (Exception ex)
			{
				result.Errors.Add(new ExecutionError(ChecklistPhase.PreMutation, ex.Message, ex));
				result.Phases.Add(new NodePhaseRecord(ChecklistPhase.PreMutation, null, ex.Message));
				return;
			}
			if (fresh.Status != precheckedState.Status ||
				!fresh.Observation.Equals(precheckedState.Observation))
			{
				var error = new InvalidOperationException(
					"State changed between the initial check and action; action was not started.");
				result.Status = fresh.Status;
				result.Detail = fresh.Detail;
				result.Errors.Add(new ExecutionError(ChecklistPhase.PreMutation, error.Message, error));
				return;
			}
			if (callerCancellation.IsCancellationRequested)
			{
				result.CancellationObserved = true;
				return;
			}

			using var completion = new CancellationTokenSource(options.CompletionTimeout);
			result.ActionAttempted = true;
			try
			{
				await step.Action!.ExecuteAsync(completion.Token).ConfigureAwait(false);
				result.ActionCompleted = true;
				result.Phases.Add(new NodePhaseRecord(ChecklistPhase.Action, null, "Action completed."));
			}
			catch (Exception ex)
			{
				result.Errors.Add(new ExecutionError(ChecklistPhase.Action, ex.Message, ex));
				result.Phases.Add(new NodePhaseRecord(ChecklistPhase.Action, null, ex.Message));
			}

			using var postcheck = new CancellationTokenSource(options.CompletionTimeout);
			try
			{
				var post = await step.Check!.EvaluateAsync(postcheck.Token).ConfigureAwait(false);
				result.Phases.Add(Phase(ChecklistPhase.Postcheck, post));
				result.Status = post.Status;
				result.Detail = post.Detail;
				if (post.Status is not (ChecklistStatus.Done or ChecklistStatus.Skipped))
				{
					var convergence = new InvalidOperationException(
						$"Action did not converge: authoritative postcheck is {post.Status}.");
					result.Errors.Add(new ExecutionError(
						ChecklistPhase.Postcheck, convergence.Message, convergence));
				}
			}
			catch (Exception ex)
			{
				result.Errors.Add(new ExecutionError(ChecklistPhase.Postcheck, ex.Message, ex));
				result.Phases.Add(new NodePhaseRecord(ChecklistPhase.Postcheck, null, ex.Message));
			}

			if (callerCancellation.IsCancellationRequested)
				result.CancellationObserved = true;
		}

		private async Task<NodeResult> RunSequenceAsync(Sequence sequence, NodePhaseRecord conditionPhase)
		{
			var result = new NodeResult(sequence);
			result.Phases.Add(conditionPhase);
			for (var index = 0; index < sequence.Children.Count; index++)
			{
				var child = await RunNodeAsync(sequence.Children[index]).ConfigureAwait(false);
				result.Children.Add(child);
				if (ShouldStop(child))
				{
					for (var rest = index + 1; rest < sequence.Children.Count; rest++)
						result.Children.Add(NotReached(sequence.Children[rest], $"Stopped after '{child.Id}'."));
					break;
				}
			}
			AggregateContainer(result);
			return result;
		}

		private async Task<NodeResult> RunParallelAsync(Parallel parallel, NodePhaseRecord conditionPhase)
		{
			var result = new NodeResult(parallel);
			result.Phases.Add(conditionPhase);
			var branches = await Task.WhenAll(
				parallel.Children.Select(RunNodeAsync)).ConfigureAwait(false);
			result.Children.AddRange(branches);
			AggregateContainer(result);
			return result;
		}

		private static bool ShouldStop(NodeResult result) =>
			result.HasErrors ||
			result.CancellationObserved ||
			result.Status is ChecklistStatus.Blocked or ChecklistStatus.NotDone or null;

		private static void AggregateContainer(NodeResult result)
		{
			var reached = result.Children.Where(static child => child.Reached).ToArray();
			result.Status = reached.Length == 0
				? ChecklistStatus.Skipped
				: Check.Aggregate(reached.Select(static child => child.Status ?? ChecklistStatus.NotDone));
			result.Detail = $"{reached.Length} node(s) reached.";
			result.CancellationObserved = reached.Any(static child => child.CancellationObserved);
		}

		private static NodePhaseRecord Phase(ChecklistPhase phase, CheckResult result) =>
			new(phase, result.Status, result.Detail, result.Observation);

		private static NodeResult NotReached(ChecklistNode node, string reason)
		{
			var result = new NodeResult(node)
			{
				Reached = false,
				NotReachedReason = reason,
			};
			MarkDescendantsNotReached(node, result, reason);
			return result;
		}

		private static void MarkDescendantsNotReached(
			ChecklistNode node,
			NodeResult result,
			string reason)
		{
			if (node is ChecklistContainer container)
				foreach (var child in container.Children)
					result.Children.Add(NotReached(child, reason));
		}
	}
}
