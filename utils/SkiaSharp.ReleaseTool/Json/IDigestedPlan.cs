namespace SkiaSharp.ReleaseTool.Json
{
	/// <summary>
	/// A top-level artifact that carries a canonical digest field --
	/// <see cref="Artifacts.PreparePlan"/> and
	/// <see cref="Artifacts.FinishPlan"/>, the two "approval-bearing"
	/// plans a human/environment reviews before <c>apply</c>/
	/// <c>create-draft</c>/etc. consumes them. Mirrors the role Python's
	/// <c>release_common.DIGEST_FIELD</c> (<c>"planDigest"</c>) plays on
	/// any dict passed to <c>with_digest</c>/<c>verify_digest</c>.
	/// </summary>
	public interface IDigestedPlan
	{
		string? PlanDigest { get; set; }
	}
}
