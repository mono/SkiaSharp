using SkiaSharp.ReleaseTool.Errors;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Errors
{
	public class ErrorsTests
	{
		[Theory]
		[InlineData(typeof(PlanException))]
		[InlineData(typeof(ValidationException))]
		[InlineData(typeof(ConflictException))]
		[InlineData(typeof(NotReadyException))]
		public void Every_specific_error_type_derives_from_ReleaseToolException(Type type)
		{
			Assert.True(typeof(ReleaseToolException).IsAssignableFrom(type));
		}

		[Fact]
		public void ExitCodes_match_the_python_release_cli_exactly()
		{
			Assert.Equal(0, ExitCodes.Success);
			Assert.Equal(1, ExitCodes.GenericError);
			Assert.Equal(2, ExitCodes.FinishPlanPending);
		}

		[Fact]
		public void NotReadyException_carries_structured_polling_context()
		{
			MissingPackageRef[] missing = [new("SkiaSharp", "3.119.0"), new("HarfBuzzSharp", "1.8.8")];

			var ex = new NotReadyException(
				"2 package(s) not yet visible/listed on NuGet.org",
				missing: missing,
				elapsedSeconds: 1234.5,
				deadlineSeconds: 1200.0);

			Assert.Equal(missing, ex.Missing);
			Assert.Equal(1234.5, ex.ElapsedSeconds);
			Assert.Equal(1200.0, ex.DeadlineSeconds);
			Assert.IsAssignableFrom<ReleaseToolException>(ex);
		}

		[Fact]
		public void NotReadyException_defaults_to_an_empty_missing_list()
		{
			var ex = new NotReadyException("not ready yet");

			Assert.Empty(ex.Missing);
			Assert.Null(ex.ElapsedSeconds);
			Assert.Null(ex.DeadlineSeconds);
		}
	}
}
