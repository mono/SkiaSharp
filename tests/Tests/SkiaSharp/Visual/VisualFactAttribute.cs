using System;
using Xunit;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Marks a test as part of the visual-regression matrix. Cells in this
	/// matrix need infrastructure that most hosts don't have (a booted iOS
	/// Simulator, an Android emulator, a published WASM payload, a Vulkan
	/// ICD, …) so they are <b>opt-in</b>: skipped at discovery time unless
	/// <c>SKIASHARP_VISUAL_TESTS=1</c> is set.
	///
	/// <para>
	/// Runtime per-cell skips (renderer unavailable on this host, transport
	/// bring-up failed, …) still use <see cref="Xunit.Skip"/> from inside
	/// the test body. This attribute only gates whether the matrix runs at
	/// all.
	/// </para>
	///
	/// <para>See <c>documentation/dev/visual-tests.md</c>.</para>
	/// </summary>
	[AttributeUsage (AttributeTargets.Method, AllowMultiple = false)]
	public sealed class VisualFactAttribute : SkippableFactAttribute
	{
		public VisualFactAttribute ()
		{
			if (!VisualTestGate.Enabled)
				Skip = VisualTestGate.SkipReason;
		}
	}

	[AttributeUsage (AttributeTargets.Method, AllowMultiple = false)]
	public sealed class VisualTheoryAttribute : SkippableTheoryAttribute
	{
		public VisualTheoryAttribute ()
		{
			if (!VisualTestGate.Enabled)
				Skip = VisualTestGate.SkipReason;
		}
	}

	internal static class VisualTestGate
	{
		public const string EnvVar = "SKIASHARP_VISUAL_TESTS";

		public static bool Enabled
		{
			get {
				var v = Environment.GetEnvironmentVariable (EnvVar);
				if (string.IsNullOrEmpty (v)) return false;
				return v == "1" || v.Equals ("true", StringComparison.OrdinalIgnoreCase);
			}
		}

		public static string SkipReason =>
			$"Visual tests are opt-in. Set {EnvVar}=1 to enable them " +
			$"(see documentation/dev/visual-tests.md).";
	}
}
