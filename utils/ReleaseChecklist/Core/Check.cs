namespace ReleaseChecklist.Core;

/// <summary>Creates checklist checks and combines multiple desired-state checks.</summary>
public static class Check
{
	/// <summary>Creates an asynchronous check that receives a cancellation token.</summary>
	/// <param name="check">The callback that evaluates desired state.</param>
	/// <returns>A checklist check that invokes the callback.</returns>
	public static IChecklistCheck From(
		Func<CancellationToken, ValueTask<CheckResult>> check) =>
		new DelegateCheck(check);

	/// <summary>Creates a synchronous check.</summary>
	/// <param name="check">The callback that evaluates desired state.</param>
	/// <returns>A checklist check that invokes the callback.</returns>
	public static IChecklistCheck From(Func<CheckResult> check) =>
		From(_ => ValueTask.FromResult(check()));

	/// <summary>Creates a synchronous check that receives a cancellation token.</summary>
	/// <param name="check">The callback that evaluates desired state.</param>
	/// <returns>A checklist check that invokes the callback.</returns>
	public static IChecklistCheck From(Func<CancellationToken, CheckResult> check) =>
		From(token => ValueTask.FromResult(check(token)));

	/// <summary>Creates a check that evaluates and aggregates all specified checks.</summary>
	/// <param name="checks">The checks to evaluate.</param>
	/// <returns>A check whose status is the aggregate of every input check.</returns>
	public static IChecklistCheck All(params IChecklistCheck[] checks) =>
		new AllCheck(checks);

	private sealed class DelegateCheck(
		Func<CancellationToken, ValueTask<CheckResult>> check) : IChecklistCheck
	{
		public ValueTask<CheckResult> EvaluateAsync(CancellationToken cancellationToken) =>
			check(cancellationToken);
	}

	private sealed class AllCheck(IReadOnlyList<IChecklistCheck> checks) : IChecklistCheck
	{
		public async ValueTask<CheckResult> EvaluateAsync(CancellationToken cancellationToken)
		{
			var results = new List<CheckResult>(checks.Count);
			foreach (var check in checks)
			{
				results.Add(await check.EvaluateAsync(cancellationToken).ConfigureAwait(false));
			}

			var status = Aggregate(results.Select(static result => result.Status));
			var observation = new ObservationBuilder()
				.Add("count", results.Count)
				.Add("statuses", string.Join(',', results.Select(static result => result.Status)))
				.Add("observations", string.Join('|', results.Select(static result => result.Observation)))
				.Build();
			return new CheckResult(
				status,
				string.Join("; ", results.Select(static result => result.Detail)),
				observation);
		}
	}

	internal static ChecklistStatus Aggregate(IEnumerable<ChecklistStatus> statuses)
	{
		var values = statuses.ToArray();
		if (values.Any(static status => status == ChecklistStatus.Blocked))
			return ChecklistStatus.Blocked;
		if (values.Any(static status => status == ChecklistStatus.NotDone))
			return ChecklistStatus.NotDone;
		if (values.Length > 0 && values.All(static status => status == ChecklistStatus.Skipped))
			return ChecklistStatus.Skipped;
		return ChecklistStatus.Done;
	}
}
