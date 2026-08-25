namespace SkiaSharp.PackageValidation;

/// <summary>
/// Collects validation failures instead of throwing on the first one, so a single run reports
/// everything that is wrong with the packages rather than only the first problem.
/// </summary>
public sealed class ValidationLog
{
	private readonly List<string> errors = new ();
	private readonly List<string> warnings = new ();
	private readonly List<string> scopes = new ();
	private readonly TextWriter output;
	private readonly bool verbose;

	public ValidationLog (TextWriter output, bool verbose = false)
	{
		this.output = output;
		this.verbose = verbose;
	}

	public IReadOnlyList<string> Errors => errors;

	public IReadOnlyList<string> Warnings => warnings;

	public bool HasErrors => errors.Count > 0;

	public string CurrentScope => scopes.Count == 0 ? "" : string.Join (" / ", scopes);

	public IDisposable BeginScope (string name)
	{
		scopes.Add (name);
		return new ScopeToken (this);
	}

	/// <summary>
	/// Records a failure when <paramref name="condition"/> is false and returns the condition so
	/// callers can skip dependent checks that would only produce noise.
	/// </summary>
	public bool Check (bool condition, string message)
	{
		if (!condition)
			Error (message);
		return condition;
	}

	public void Error (string message)
	{
		var scoped = Format (message);
		errors.Add (scoped);
		output.WriteLine ($"  ERROR: {scoped}");
	}

	public void Warn (string message)
	{
		var scoped = Format (message);
		warnings.Add (scoped);
		output.WriteLine ($"  WARNING: {scoped}");
	}

	public void Info (string message) =>
		output.WriteLine ($"  {message}");

	public void Verbose (string message)
	{
		if (verbose)
			output.WriteLine ($"    {message}");
	}

	private string Format (string message)
	{
		var scope = CurrentScope;
		return string.IsNullOrEmpty (scope) ? message : $"[{scope}] {message}";
	}

	private sealed class ScopeToken : IDisposable
	{
		private readonly ValidationLog log;
		private bool disposed;

		public ScopeToken (ValidationLog log) => this.log = log;

		public void Dispose ()
		{
			if (disposed)
				return;
			disposed = true;
			log.scopes.RemoveAt (log.scopes.Count - 1);
		}
	}
}
