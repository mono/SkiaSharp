using System.Reflection;

namespace HarfBuzzSharp.Tests
{
	public static class Extensions
	{
		private static readonly FieldInfo isDisposedField = typeof(NativeObject).GetField("isDisposed", BindingFlags.NonPublic | BindingFlags.Instance);

		public static bool IsDisposed(this NativeObject obj) =>
			(bool)isDisposedField.GetValue(obj);
	}
}
