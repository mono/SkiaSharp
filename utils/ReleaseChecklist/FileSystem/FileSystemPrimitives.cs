using ReleaseChecklist.Core;

namespace ReleaseChecklist.FileSystem;

/// <summary>Creates desired-state checks and actions for local files and directories.</summary>
public static class FileSystemPrimitives
{
	/// <summary>Creates a check that requires a directory to exist.</summary>
	/// <param name="path">The directory path.</param>
	/// <returns>A check that is done when the directory exists.</returns>
	public static IChecklistCheck DirectoryExists(string path) =>
		Check.From(_ =>
		{
			var fullPath = Path.GetFullPath(path);
			var exists = Directory.Exists(fullPath);
			var observation = new ObservationBuilder()
				.Add("path", fullPath)
				.Add("exists", exists)
				.Build();
			return ValueTask.FromResult(
				exists
					? CheckResult.Done($"Directory exists: {fullPath}", observation)
					: CheckResult.NotDone($"Directory is missing: {fullPath}", observation));
		});

	/// <summary>Creates an action that creates a directory and any missing parents.</summary>
	/// <param name="path">The directory path.</param>
	/// <returns>An action that ensures the directory exists.</returns>
	public static IChecklistAction EnsureDirectory(string path) =>
		ChecklistBuilder.Action(
			_ =>
			{
				Directory.CreateDirectory(Path.GetFullPath(path));
				return ValueTask.CompletedTask;
			});

	/// <summary>Creates a check that requires a file to contain exact text.</summary>
	/// <param name="path">The file path.</param>
	/// <param name="expectedContent">The exact expected text.</param>
	/// <returns>A check that is done when the file content matches.</returns>
	public static IChecklistCheck FileContentCheck(string path, string expectedContent) =>
		Check.From(async token =>
		{
			var fullPath = Path.GetFullPath(path);
			var exists = File.Exists(fullPath);
			var actual = exists
				? await File.ReadAllTextAsync(fullPath, token).ConfigureAwait(false)
				: null;
			var matches = string.Equals(actual, expectedContent, StringComparison.Ordinal);
			var observation = new ObservationBuilder()
				.Add("path", fullPath)
				.Add("exists", exists)
				.Add("actual-sha256", actual is null ? "" : Sha256(actual))
				.Add("expected-sha256", Sha256(expectedContent))
				.Build();
			return matches
				? CheckResult.Done($"File content matches: {fullPath}", observation)
				: CheckResult.NotDone($"File content differs: {fullPath}", observation);
		});

	/// <summary>Creates an action that atomically writes text to a file.</summary>
	/// <param name="path">The file path.</param>
	/// <param name="content">The text to write.</param>
	/// <returns>An action that writes the file.</returns>
	public static IChecklistAction WriteFile(
		string path,
		string content) =>
		ChecklistBuilder.Action(
			async token =>
			{
				var fullPath = Path.GetFullPath(path);
				var directory = Path.GetDirectoryName(fullPath)
					?? throw new InvalidOperationException("File has no parent directory.");
				Directory.CreateDirectory(directory);
				var temporary = Path.Combine(
					directory,
					$".{Path.GetFileName(fullPath)}.release-checklist-{Guid.NewGuid():N}");
				try
				{
					await File.WriteAllTextAsync(temporary, content, token).ConfigureAwait(false);
					File.Move(temporary, fullPath, overwrite: true);
				}
				finally
				{
					if (File.Exists(temporary))
						File.Delete(temporary);
				}
			});

	private static string Sha256(string value) =>
		Convert.ToHexStringLower(
			System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)));
}
