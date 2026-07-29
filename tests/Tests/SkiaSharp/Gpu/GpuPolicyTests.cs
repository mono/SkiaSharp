using Xunit;

namespace SkiaSharp.Tests
{
	/// <summary>
	/// Guards the GPU availability policy itself, and records how it resolved on
	/// this host.
	///
	/// <para>
	/// Deliberately <b>not</b> tagged <c>Category=GPU</c>: a leg that filters the
	/// GPU suite out should still publish its policy report, because the report is
	/// how we tell "this backend was correctly skipped" from "this backend
	/// silently never ran".
	/// </para>
	/// </summary>
	public class GpuPolicyTests : BaseTest
	{
		/// <summary>
		/// Line prefix for the resolved-policy lines emitted into the test log.
		/// The TRX is the only output channel present on every host — desktop,
		/// device and browser — so this is how a CI leg reports which GPU backends
		/// it actually required.
		/// </summary>
		public const string PolicyMarker = "##SKIA-GPU-POLICY##";

		public GpuPolicyTests(ITestOutputHelper output)
			: base(output)
		{
		}

		/// <summary>
		/// An unidentified host would classify every backend as unavailable and
		/// quietly skip the entire GPU suite — precisely the silent hole this
		/// policy exists to close. Fail loudly in one obvious place instead.
		/// </summary>
		[Fact]
		public void HostPlatformIsRecognised()
		{
			Assert.True(
				TestPlatform.Current != TestPlatforms.None,
				"The host platform could not be identified, so no GPU backend can be required here. " +
				"Add it to TestPlatforms/TestPlatform.Detect.");
		}

		/// <summary>
		/// A typo in the opt-out list must never quietly leave a backend required
		/// (or, worse, look like it disabled something it did not). Parsing rejects
		/// unknown ids; this surfaces that as one obvious failure.
		/// </summary>
		[Fact]
		public void OptOutListNamesOnlyKnownBackends()
		{
			GpuPolicy.Validate();
		}

		/// <summary>
		/// Emits one <c>##SKIA-GPU-POLICY##</c> line per backend with the state it
		/// resolved to and why. This is the per-leg report that makes a skip
		/// auditable: every skipped backend has to name the reason it was skipped.
		/// </summary>
		[Fact]
		public void ReportsResolvedPolicy()
		{
			WriteOutput($"{PolicyMarker} platform={TestPlatform.Name}");

			foreach (var line in GpuPolicy.Describe())
				WriteOutput($"{PolicyMarker} {line}");
		}
	}
}
