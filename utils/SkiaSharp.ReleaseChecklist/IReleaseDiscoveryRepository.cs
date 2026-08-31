namespace SkiaSharp.ReleaseChecklist;

/// <summary>Defines the repository reads required by SkiaSharp release discovery.</summary>
public interface IReleaseDiscoveryRepository
{
	/// <summary>Gets the authoritative remote name.</summary>
	/// <value>The remote name.</value>
	string RemoteName { get; }

	/// <summary>Gets the currently checked-out branch.</summary>
	/// <param name="cancellationToken">A token that cancels the lookup.</param>
	/// <returns>The short branch name.</returns>
	Task<string> CurrentBranchAsync(CancellationToken cancellationToken);

	/// <summary>Determines whether a fully qualified ref exists.</summary>
	/// <param name="reference">The fully qualified ref.</param>
	/// <param name="cancellationToken">A token that cancels the lookup.</param>
	/// <returns><see langword="true" /> if the ref exists; otherwise, <see langword="false" />.</returns>
	Task<bool> RefExistsAsync(string reference, CancellationToken cancellationToken);

	/// <summary>Resolves a revision to a commit SHA.</summary>
	/// <param name="reference">The revision to resolve.</param>
	/// <param name="cancellationToken">A token that cancels the lookup.</param>
	/// <returns>The commit SHA.</returns>
	Task<string> ResolveAsync(string reference, CancellationToken cancellationToken);

	/// <summary>Gets the target of a remote branch.</summary>
	/// <param name="branch">The short branch name.</param>
	/// <param name="cancellationToken">A token that cancels the lookup.</param>
	/// <returns>The target SHA, or <see langword="null" /> if absent.</returns>
	Task<string?> RemoteBranchShaAsync(string branch, CancellationToken cancellationToken);

	/// <summary>Determines whether a commit is published in a remote branch.</summary>
	/// <param name="sha">The commit SHA.</param>
	/// <param name="cancellationToken">A token that cancels the lookup.</param>
	/// <returns><see langword="true" /> if a remote branch contains the commit; otherwise, <see langword="false" />.</returns>
	Task<bool> IsContainedInRemoteBranchAsync(string sha, CancellationToken cancellationToken);

	/// <summary>Lists remote release branches.</summary>
	/// <param name="cancellationToken">A token that cancels the lookup.</param>
	/// <returns>The short branch names.</returns>
	Task<IReadOnlyList<string>> ReleaseBranchesAsync(CancellationToken cancellationToken);

	/// <summary>Reads a text file from a revision.</summary>
	/// <param name="reference">The revision containing the file.</param>
	/// <param name="path">The repository-relative path.</param>
	/// <param name="cancellationToken">A token that cancels the read.</param>
	/// <returns>The file content.</returns>
	Task<string> ReadRefFileAsync(string reference, string path, CancellationToken cancellationToken);

	/// <summary>Reads the commit recorded by a gitlink entry.</summary>
	/// <param name="reference">The revision containing the gitlink.</param>
	/// <param name="path">The repository-relative gitlink path.</param>
	/// <param name="cancellationToken">A token that cancels the read.</param>
	/// <returns>The gitlink commit SHA.</returns>
	Task<string> ReadGitlinkAsync(string reference, string path, CancellationToken cancellationToken);

	/// <summary>Determines whether one commit is an ancestor of another.</summary>
	/// <param name="ancestor">The proposed ancestor.</param>
	/// <param name="descendant">The proposed descendant.</param>
	/// <param name="cancellationToken">A token that cancels the check.</param>
	/// <returns><see langword="true" /> if the relationship exists; otherwise, <see langword="false" />.</returns>
	Task<bool> IsAncestorAsync(string ancestor, string descendant, CancellationToken cancellationToken);
}
