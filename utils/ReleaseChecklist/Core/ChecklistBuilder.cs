using System.Runtime.CompilerServices;

namespace ReleaseChecklist.Core;

/// <summary>Builds and validates checklist definitions.</summary>
public sealed class ChecklistBuilder
{
	/// <summary>Builds a checklist with an ordered root sequence.</summary>
	/// <param name="id">The stable root identifier used in reports.</param>
	/// <param name="title">The human-readable root title.</param>
	/// <param name="children">A callback that declares the root child nodes.</param>
	/// <param name="when">The root applicability condition, or <see langword="null" /> to always run.</param>
	/// <returns>A validated checklist definition.</returns>
	/// <exception cref="ChecklistDefinitionException">The definition contains invalid metadata.</exception>
	public ChecklistDefinition Sequence(
		string id,
		string title,
		Action<IChecklistChildren> children,
		IChecklistCondition? when = null)
	{
		var receiver = new ChildrenBuilder();
		children(receiver);
		var root = new Sequence(id, title, when, receiver.Nodes);
		Validate(root);
		return new ChecklistDefinition(root);
	}

	/// <summary>Creates an action from an asynchronous callback.</summary>
	/// <param name="action">The callback to run when the step is not done.</param>
	/// <returns>A checklist action that invokes the callback.</returns>
	public static IChecklistAction Action(
		Func<CancellationToken, ValueTask> action) =>
		new DelegateAction(action);

	private static void Validate(ChecklistContainer root)
	{
		var ids = new HashSet<string>(StringComparer.Ordinal);
		var instances = new HashSet<ChecklistNode>(ReferenceEqualityComparer.Instance);
		Visit(root);
		return;

		void Visit(ChecklistNode node)
		{
			if (string.IsNullOrWhiteSpace(node.Id) ||
				!string.Equals(node.Id, node.Id.Trim(), StringComparison.Ordinal))
			{
				throw new ChecklistDefinitionException("Every node requires a nonempty stable ID.");
			}
			if (string.IsNullOrWhiteSpace(node.Title))
				throw new ChecklistDefinitionException($"Node '{node.Id}' requires a nonempty title.");
			if (!ids.Add(node.Id))
				throw new ChecklistDefinitionException($"Duplicate node ID '{node.Id}'.");
			if (!instances.Add(node))
				throw new ChecklistDefinitionException($"Node '{node.Id}' is reused.");

			if (node is Step step)
			{
				if (step.Check is null && step.Action is null)
				throw new ChecklistDefinitionException(
					$"Step '{step.Id}' requires a check, an action, or both.");
				return;
			}

			var container = (ChecklistContainer)node;
			if (container.Children.Count == 0)
				throw new ChecklistDefinitionException($"Container '{node.Id}' must have children.");
			foreach (var child in container.Children)
				Visit(child);
		}
	}

	private sealed class ChildrenBuilder : IChecklistChildren
	{
		public List<ChecklistNode> Nodes { get; } = [];

		public Step Step(StepOptions options)
		{
			var step = new Step(
				options.Id,
				options.Title,
				options.When,
				options.Check,
				options.Action);
			Nodes.Add(step);
			return step;
		}

		public Sequence Sequence(
			string id,
			string title,
			Action<IChecklistChildren> children,
			IChecklistCondition? when = null)
		{
			var receiver = new ChildrenBuilder();
			children(receiver);
			var sequence = new Sequence(id, title, when, receiver.Nodes);
			Nodes.Add(sequence);
			return sequence;
		}

		public Parallel Parallel(
			string id,
			string title,
			Action<IChecklistChildren> children,
			IChecklistCondition? when = null)
		{
			var receiver = new ChildrenBuilder();
			children(receiver);
			var parallel = new Parallel(id, title, when, receiver.Nodes);
			Nodes.Add(parallel);
			return parallel;
		}
	}

	private sealed class DelegateAction(
		Func<CancellationToken, ValueTask> action) : IChecklistAction
	{
		public ValueTask ExecuteAsync(CancellationToken cancellationToken) =>
			action(cancellationToken);
	}

}
