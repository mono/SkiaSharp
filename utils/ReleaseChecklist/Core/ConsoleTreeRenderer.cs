namespace ReleaseChecklist.Core;

/// <summary>Renders structured checklist reports as an indented text tree.</summary>
public static class ConsoleTreeRenderer
{
	/// <summary>Renders a checklist report.</summary>
	/// <param name="report">The report to render.</param>
	/// <returns>The indented text representation.</returns>
	public static string Render(ChecklistReport report)
	{
		var writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
		Render(report.Root, writer, "", isLast: true, root: true);
		return writer.ToString();
	}

	private static void Render(
		NodeResult node,
		TextWriter writer,
		string indent,
		bool isLast,
		bool root)
	{
		if (!root)
		{
			writer.Write(indent);
			writer.Write(isLast ? "└── " : "├── ");
		}
		var state = node.Reached ? node.Status?.ToString() ?? "Error" : "Not reached";
		writer.WriteLine($"{node.Id} — {node.Title} [{node.Kind}; {state}]");
		var detailIndent = root ? indent : indent + (isLast ? "    " : "│   ");
		if (!node.Reached)
			writer.WriteLine($"{detailIndent}    {node.NotReachedReason}");
		foreach (var phase in node.Phases)
		{
			var observation = phase.Observation is { Fields.Count: > 0 }
				? $" ({phase.Observation})"
				: "";
			writer.WriteLine($"{detailIndent}    {phase.Phase}: {phase.Detail}{observation}");
		}
		if (node.ActionAvailable && !node.ActionAttempted)
			writer.WriteLine($"{detailIndent}    action available");
		foreach (var error in node.Errors)
			writer.WriteLine($"{detailIndent}    ERROR {error.Phase}: {error.Message}");

		for (var index = 0; index < node.Children.Count; index++)
			Render(node.Children[index], writer, detailIndent, index == node.Children.Count - 1, root: false);
	}
}
