namespace SkiaSharp.ReleaseTool.Git
{
	/// <summary>
	/// A single Git working copy (either the SkiaSharp checkout or the
	/// <c>externals/skia</c> submodule checkout). Mirrors Python's
	/// <c>release_git.GitRepository</c>: every operation goes through
	/// plain <c>git</c> argv invocations -- never shell strings, never
	/// network calls beyond <c>fetch</c>/<c>push</c>/<c>ls-remote</c>.
	/// </summary>
	public interface IGitRepository
	{
		string Root { get; }

		/// <summary>Fetches every ref and tag from <paramref name="remote"/>, pruning stale remote-tracking refs.</summary>
		void Fetch(string remote = "origin");

		/// <summary>True if <paramref name="reference"/> (e.g. <c>refs/remotes/origin/main</c>) resolves to a ref.</summary>
		bool RefExists(string reference);

		/// <summary>Resolves <paramref name="reference"/> to the 40-hex commit SHA it points at.</summary>
		string Resolve(string reference);

		/// <summary>Reads the exact bytes of <paramref name="path"/> as it exists at <paramref name="reference"/>.</summary>
		string ReadRefFile(string reference, string path);

		/// <summary>Returns the commit SHA a gitlink (submodule pointer) records for <paramref name="submodulePath"/> at <paramref name="reference"/>.</summary>
		string ReadGitlink(string reference, string submodulePath);

		/// <summary>The SHA <paramref name="remote"/>'s <c>refs/heads/&lt;branch&gt;</c> currently points at, or <see langword="null"/> if it does not exist.</summary>
		string? RemoteSha(string branch, string remote = "origin");

		/// <summary>Every tag on <paramref name="remote"/> matching <paramref name="pattern"/>, mapped to its (peeled, for annotated tags) commit SHA.</summary>
		IReadOnlyDictionary<string, string> RemoteTags(string remote = "origin", string pattern = "refs/tags/*");

		/// <summary>The short names (without the <c>release/</c> prefix) of every <c>release/*</c> branch on <paramref name="remote"/>.</summary>
		IReadOnlyList<string> ReleaseBranches(string remote = "origin");

		/// <summary>The best common ancestor commit of <paramref name="a"/> and <paramref name="b"/>.</summary>
		string MergeBase(string a, string b);

		/// <summary>True if <paramref name="ancestor"/> is an ancestor of (or equal to) <paramref name="descendant"/>.</summary>
		bool IsAncestor(string ancestor, string descendant);

		/// <summary>First-parent commit subjects for <paramref name="rangeSpec"/> (e.g. <c>a..b</c>), oldest-first.</summary>
		IReadOnlyList<string> CommitSubjectsFirstParent(string rangeSpec);

		/// <summary>Throws <see cref="GitException"/> if the working tree (ignoring submodules) is not clean.</summary>
		void RequireClean();

		string CurrentBranch();

		void CreateBranch(string branch, string startPoint);

		void Switch(string branch);

		void SwitchCreate(string branch, string startPoint);

		/// <summary>Stages <paramref name="paths"/> (when given) and commits, returning the new commit SHA.</summary>
		string Commit(string message, IReadOnlyList<string>? paths = null);

		void PushBranch(string branch, string remote = "origin", bool setUpstream = true);

		void PushTag(string tag, string sha, string remote = "origin");

		/// <summary>True if <paramref name="commit"/> is reachable from <paramref name="branchRef"/> (e.g. an <c>origin/release/...</c> ref).</summary>
		bool ContainsCommit(string branchRef, string commit);
	}
}
