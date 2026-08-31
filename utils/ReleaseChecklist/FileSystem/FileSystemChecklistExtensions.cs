using System.Security.Cryptography;
using System.Text;
using ReleaseChecklist.Core;

namespace ReleaseChecklist.FileSystem;

/// <summary>Adds reusable file-system steps to checklist containers.</summary>
public static class FileSystemChecklistExtensions
{
	/// <summary>Adds a step that transforms a text file only when its desired content differs.</summary>
	/// <param name="parent">The container receiving the step.</param>
	/// <param name="options">The file transformation configuration.</param>
	/// <returns>The added file step.</returns>
	public static Step FileContents(
		this IChecklistChildren parent,
		FileContentsOptions options)
	{
		var check = Check.From(async token =>
		{
			var fullPath = Path.GetFullPath(options.Path);
			var current = File.Exists(fullPath)
				? await File.ReadAllTextAsync(fullPath, token).ConfigureAwait(false)
				: null;
			var desired = options.Transform(current);
			var matches = string.Equals(current, desired, StringComparison.Ordinal);
			var observation = new ObservationBuilder()
				.Add("path", fullPath)
				.Add("exists", current is not null)
				.Add("current-sha256", current is null ? "" : Sha256(current))
				.Add("desired-sha256", Sha256(desired))
				.Build();
			return matches
				? CheckResult.Done($"File content matches: {fullPath}", observation)
				: CheckResult.NotDone($"File content requires transformation: {fullPath}", observation);
		});
		var action = ChecklistBuilder.Action(async token =>
		{
			var fullPath = Path.GetFullPath(options.Path);
			var current = File.Exists(fullPath)
				? await File.ReadAllTextAsync(fullPath, token).ConfigureAwait(false)
				: null;
			var desired = options.Transform(current);
			var directory = Path.GetDirectoryName(fullPath)
				?? throw new InvalidOperationException("File has no parent directory.");
			Directory.CreateDirectory(directory);
			var temporary = Path.Combine(
				directory,
				$".{Path.GetFileName(fullPath)}.release-checklist-{Guid.NewGuid():N}");
			try
			{
				await File.WriteAllTextAsync(temporary, desired, token).ConfigureAwait(false);
				File.Move(temporary, fullPath, overwrite: true);
			}
			finally
			{
				if (File.Exists(temporary))
					File.Delete(temporary);
			}
		});
		return parent.Step(new StepOptions(options.Id, options.Title)
		{
			Check = check,
			Action = action,
			When = options.When,
		});
	}

	private static string Sha256(string value) =>
		Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
