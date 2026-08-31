using System.Collections.Concurrent;
using ReleaseChecklist.Core;

namespace ReleaseChecklist.Tests.Core;

public class RunnerTests
{
	[Fact]
	public async Task SequenceStopsAndReportsRemainingNodes()
	{
		var reached = false;
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
		{
			root.Step(new StepOptions("done", "Done")
			{
				Check = Check.From(_ => CheckResult.Done("yes")),
			});
			root.Step(new StepOptions("waiting", "Waiting")
			{
				Check = Check.From(_ => CheckResult.NotDone("wait")),
			});
			root.Step(new StepOptions("later", "Later")
			{
				Check = Check.From(_ =>
				{
					reached = true;
					return CheckResult.Done("bad");
				}),
			});
		});

		var report = await ChecklistRunner.RunAsync(definition);

		Assert.Equal(ChecklistStatus.NotDone, report.Root.Status);
		Assert.False(reached);
		Assert.False(report.Root.Children[2].Reached);
		Assert.Contains("waiting", report.Root.Children[2].NotReachedReason);
	}

	[Fact]
	public async Task DesiredStatesIncludeConditionsWhenJoiningAfterParallel()
	{
		var conditionCalls = 0;
		Step? left = null;
		Step? right = null;
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
		{
			root.Parallel("fanout", "Fan out", parallel =>
			{
				left = parallel.Step(new StepOptions(
					"left",
					"Left")
				{
					Check = Check.From(_ => CheckResult.Done("left ready")),
					When = Condition.From(_ =>
					{
						Interlocked.Increment(ref conditionCalls);
						return true;
					}),
				});
				right = parallel.Step(new StepOptions(
					"right",
					"Right")
				{
					Check = Check.From(_ => CheckResult.Done("right ready")),
				});
			});
			root.Step(new StepOptions(
				"join",
				"Join")
			{
				Check = Check.All(left!.DesiredState!, right!.DesiredState!),
			});
		});

		var report = await ChecklistRunner.RunAsync(definition);

		Assert.True(report.Successful);
		Assert.Equal(2, conditionCalls);
	}

	[Fact]
	public async Task ParallelBlockedBranchDoesNotSuppressSiblingAction()
	{
		var mutations = 0;
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
		{
			root.Parallel("fanout", "Fan out", parallel =>
			{
				parallel.Step(new StepOptions(
					"blocked",
					"Blocked")
				{
					Check = Check.From(_ => CheckResult.Blocked("conflict")),
					Action = ChecklistBuilder.Action(_ =>
					{
						Interlocked.Increment(ref mutations);
						return ValueTask.CompletedTask;
					}),
				});
				parallel.Step(new StepOptions(
					"ready",
					"Ready")
				{
					Check = Check.From(_ => CheckResult.NotDone("missing")),
					Action = ChecklistBuilder.Action(_ =>
					{
						Interlocked.Increment(ref mutations);
						return ValueTask.CompletedTask;
					}),
				});
			});
		});

		var report = await ChecklistRunner.RunAsync(
			definition,
			new ChecklistRunOptions { Mode = ChecklistRunMode.Apply });

		Assert.Equal(ChecklistStatus.Blocked, report.Root.Status);
		Assert.Equal(1, mutations);
		Assert.Equal(2, report.Root.Children[0].Children.Count);
	}

	[Fact]
	public async Task ParallelActionsRunConcurrentlyAndCompleteNaturally()
	{
		var states = new ConcurrentDictionary<string, bool>();
		var entered = new CountdownEvent(2);
		var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
		{
			root.Parallel("fanout", "Fan out", parallel =>
			{
				AddBranch(parallel, "a");
				AddBranch(parallel, "b");
			});
		});

		var running = ChecklistRunner.RunAsync(
			definition,
			new ChecklistRunOptions { Mode = ChecklistRunMode.Apply });
		Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));
		release.SetResult();
		var report = await running;

		Assert.True(report.Successful);
		Assert.All(report.Root.Children[0].Children, child => Assert.True(child.ActionCompleted));
		return;

		void AddBranch(IChecklistChildren parent, string id)
		{
			parent.Step(new StepOptions(
				id,
				id)
			{
				Check = Check.From(_ => states.ContainsKey(id)
					? CheckResult.Done("done", Observe(id, true))
					: CheckResult.NotDone("missing", Observe(id, false))),
				Action = ChecklistBuilder.Action(async _ =>
				{
					entered.Signal();
					await release.Task;
					states[id] = true;
				}),
			});
		}
	}

	[Fact]
	public async Task ParallelSequenceCompletesWhileReadOnlySiblingWaits()
	{
		var firstDone = false;
		var secondDone = false;
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
			root.Parallel("fanout", "Fan out", parallel =>
			{
				parallel.Sequence("ordered", "Ordered actions", ordered =>
				{
					ordered.Step(new StepOptions(
						"first",
						"First")
					{
						Check = Check.From(_ => firstDone
							? CheckResult.Done("done", Observe("done", true))
							: CheckResult.NotDone("pending", Observe("done", false))),
						Action = ChecklistBuilder.Action(_ =>
						{
							firstDone = true;
							return ValueTask.CompletedTask;
						}),
					});
					ordered.Step(new StepOptions(
						"second",
						"Second")
					{
						Check = Check.From(_ => secondDone
							? CheckResult.Done("done", Observe("done", true))
							: CheckResult.NotDone("pending", Observe("done", false))),
						Action = ChecklistBuilder.Action(_ =>
						{
							secondDone = true;
							return ValueTask.CompletedTask;
						}),
					});
				});
				parallel.Step(new StepOptions(
					"external",
					"External wait")
				{
					Check = Check.From(_ => CheckResult.NotDone("waiting for external state")),
				});
			}));

		var report = await ChecklistRunner.RunAsync(
			definition,
			new ChecklistRunOptions { Mode = ChecklistRunMode.Apply });

		Assert.True(firstDone);
		Assert.True(secondDone);
		Assert.Equal(ChecklistStatus.NotDone, report.Root.Status);
		Assert.True(report.Root.Children[0].Children[0].Children[0].ActionCompleted);
		Assert.True(report.Root.Children[0].Children[0].Children[1].ActionCompleted);
		Assert.Equal(ChecklistStatus.NotDone, report.Root.Children[0].Children[1].Status);
	}

	[Fact]
	public async Task PreMutationDriftPreventsMutation()
	{
		var checkCount = 0;
		var mutated = false;
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
			root.Step(new StepOptions(
				"branch",
				"Branch")
			{
				Check = Check.From(_ =>
				{
					var count = Interlocked.Increment(ref checkCount);
					return CheckResult.NotDone("missing", Observe("count", count));
				}),
				Action = ChecklistBuilder.Action(_ =>
				{
					mutated = true;
					return ValueTask.CompletedTask;
				}),
			}));
		var report = await ChecklistRunner.RunAsync(
			definition,
			new ChecklistRunOptions { Mode = ChecklistRunMode.Apply });

		Assert.False(mutated);
		Assert.True(report.Root.HasErrors);
		Assert.Contains(report.Root.Children[0].Errors, error => error.Phase == ChecklistPhase.PreMutation);
	}

	[Fact]
	public async Task ActionErrorStillRunsAuthoritativePostcheck()
	{
		var done = false;
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
			root.Step(new StepOptions(
				"branch",
				"Branch")
			{
				Check = Check.From(_ => done
					? CheckResult.Done("created", Observe("exists", true))
					: CheckResult.NotDone("missing", Observe("exists", false))),
				Action = ChecklistBuilder.Action(_ =>
				{
					done = true;
					throw new InvalidOperationException("connection dropped");
				}),
			}));

		var report = await ChecklistRunner.RunAsync(
			definition,
			new ChecklistRunOptions { Mode = ChecklistRunMode.Apply });

		Assert.Equal(ChecklistStatus.Done, report.Root.Children[0].Status);
		Assert.Contains(report.Root.Children[0].Phases, phase => phase.Phase == ChecklistPhase.Postcheck);
		Assert.False(report.Successful);
	}

	[Fact]
	public async Task AllCheckUsesReviewedAggregation()
	{
		var contextDefinition = new ChecklistBuilder().Sequence("root", "Root", root =>
		{
			root.Step(new StepOptions(
				"all-skipped",
				"All skipped")
			{
				Check = Check.All(
					Check.From(_ => CheckResult.Skipped("one")),
					Check.From(_ => CheckResult.Skipped("two"))),
			});
			root.Step(new StepOptions(
				"mixed",
				"Mixed")
			{
				Check = Check.All(
					Check.From(_ => CheckResult.Done("one")),
					Check.From(_ => CheckResult.Skipped("two"))),
			});
		});

		var report = await ChecklistRunner.RunAsync(contextDefinition);

		Assert.Equal(ChecklistStatus.Skipped, report.Root.Children[0].Status);
		Assert.Equal(ChecklistStatus.Done, report.Root.Children[1].Status);
	}

	[Fact]
	public async Task CancellationPreventsStartingNewAction()
	{
		var mutated = false;
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
			root.Step(new StepOptions(
				"cancel",
				"Cancel")
			{
				Check = Check.From(_ => CheckResult.NotDone("missing")),
				Action = ChecklistBuilder.Action(_ =>
				{
					mutated = true;
					return ValueTask.CompletedTask;
				}),
			}));
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();

		var report = await ChecklistRunner.RunAsync(
			definition,
			new ChecklistRunOptions
			{
				Mode = ChecklistRunMode.Apply,
			},
			cancellation.Token);

		Assert.False(mutated);
		Assert.True(report.Root.Children[0].CancellationObserved);
	}

	[Fact]
	public async Task NonconvergentActionIsAnExecutionError()
	{
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
			root.Step(new StepOptions(
				"stuck",
				"Stuck")
			{
				Check = Check.From(_ => CheckResult.NotDone("still missing")),
				Action = ChecklistBuilder.Action(_ => ValueTask.CompletedTask),
			}));

		var report = await ChecklistRunner.RunAsync(
			definition,
			new ChecklistRunOptions { Mode = ChecklistRunMode.Apply });

		Assert.Contains(
			report.Root.Children[0].Errors,
			error => error.Phase == ChecklistPhase.Postcheck);
	}

	[Fact]
	public async Task ActionOnlyStepRunsOnEveryApply()
	{
		var runs = 0;
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
			root.Step(new StepOptions("always", "Always run")
			{
				Action = ChecklistBuilder.Action(_ =>
				{
					runs++;
					return ValueTask.CompletedTask;
				}),
			}));

		var dryRun = await ChecklistRunner.RunAsync(definition);
		var firstApply = await ChecklistRunner.RunAsync(
			definition,
			new ChecklistRunOptions { Mode = ChecklistRunMode.Apply });
		var secondApply = await ChecklistRunner.RunAsync(
			definition,
			new ChecklistRunOptions { Mode = ChecklistRunMode.Apply });

		Assert.False(dryRun.Root.Children[0].ActionAttempted);
		Assert.Equal(ChecklistStatus.NotDone, dryRun.Root.Status);
		Assert.Equal(2, runs);
		Assert.True(firstApply.Successful);
		Assert.True(secondApply.Successful);
	}

	[Fact]
	public void StepWithoutCheckOrActionIsRejected()
	{
		var exception = Assert.Throws<ChecklistDefinitionException>(() =>
			new ChecklistBuilder().Sequence(
				"root",
				"Root",
				root => root.Step(new StepOptions("empty", "Empty"))));

		Assert.Contains("requires a check, an action, or both", exception.Message);
	}

	[Fact]
	public async Task ConditionalDesiredStateIsSkipped()
	{
		Step? producer = null;
		var definition = new ChecklistBuilder().Sequence("root", "Root", root =>
		{
			producer = root.Step(new StepOptions(
				"producer",
				"Producer")
			{
				Check = Check.From(_ => CheckResult.Done("value")),
				When = Condition.From(_ => false),
			});
			root.Step(new StepOptions(
				"consumer",
				"Consumer")
			{
				Check = Check.All(producer.DesiredState!),
			});
		});

		var report = await ChecklistRunner.RunAsync(definition);

		Assert.True(report.Successful);
		Assert.Equal(ChecklistStatus.Skipped, report.Root.Children[1].Status);
	}

	private static Observation Observe(string name, bool value) =>
		new ObservationBuilder().Add(name, value).Build();

	private static Observation Observe(string name, int value) =>
		new ObservationBuilder().Add(name, value).Build();

}
