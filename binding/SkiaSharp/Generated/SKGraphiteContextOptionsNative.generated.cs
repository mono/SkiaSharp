using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_context_options_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKGraphiteContextOptionsNative : IEquatable<SKGraphiteContextOptionsNative> {
		// public bool fDisableDriverCorrectnessWorkarounds
		public Byte fDisableDriverCorrectnessWorkarounds;

		// public int32_t fInternalMultisampleCount
		public Int32 fInternalMultisampleCount;

		// public int64_t fGpuBudgetInBytes
		public Int64 fGpuBudgetInBytes;

		// public bool fRequireOrderedRecordings
		public Byte fRequireOrderedRecordings;

		// public bool fSetBackendLabels
		public Byte fSetBackendLabels;

		// public sk_graphite_shader_error_handler_t* fShaderErrorHandler
		public sk_graphite_shader_error_handler_t fShaderErrorHandler;

		public readonly bool Equals (SKGraphiteContextOptionsNative obj) =>
#pragma warning disable CS8909
			fDisableDriverCorrectnessWorkarounds == obj.fDisableDriverCorrectnessWorkarounds && fInternalMultisampleCount == obj.fInternalMultisampleCount && fGpuBudgetInBytes == obj.fGpuBudgetInBytes && fRequireOrderedRecordings == obj.fRequireOrderedRecordings && fSetBackendLabels == obj.fSetBackendLabels && fShaderErrorHandler == obj.fShaderErrorHandler;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteContextOptionsNative f && Equals (f);

		public static bool operator == (SKGraphiteContextOptionsNative left, SKGraphiteContextOptionsNative right) =>
			left.Equals (right);

		public static bool operator != (SKGraphiteContextOptionsNative left, SKGraphiteContextOptionsNative right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fDisableDriverCorrectnessWorkarounds);
			hash.Add (fInternalMultisampleCount);
			hash.Add (fGpuBudgetInBytes);
			hash.Add (fRequireOrderedRecordings);
			hash.Add (fSetBackendLabels);
			hash.Add (fShaderErrorHandler);
			return hash.ToHashCode ();
		}

	}
}
