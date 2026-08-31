namespace ReleaseChecklist.Core;

/// <summary>Performs an operation intended to satisfy a checklist check.</summary>
public interface IChecklistAction
{
	/// <summary>Asynchronously performs the operation.</summary>
	/// <param name="cancellationToken">A token that bounds completion of the operation.</param>
	/// <returns>A task that represents the operation.</returns>
	ValueTask ExecuteAsync(CancellationToken cancellationToken);
}
