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
		public void Every_specific_error_type_derives_from_ReleaseToolException(Type type)
		{
			Assert.True(typeof(ReleaseToolException).IsAssignableFrom(type));
		}

		[Fact]
		public void ExitCodes_are_stable()
		{
			Assert.Equal(0, ExitCodes.Success);
			Assert.Equal(1, ExitCodes.GenericError);
		}
	}
}
