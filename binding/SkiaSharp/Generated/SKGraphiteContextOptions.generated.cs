using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_context_options_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKGraphiteContextOptions : IEquatable<SKGraphiteContextOptions> {
		// public bool fDisableDriverCorrectnessWorkarounds
		private Byte fDisableDriverCorrectnessWorkarounds;
		public bool DisableDriverCorrectnessWorkarounds {
			readonly get => fDisableDriverCorrectnessWorkarounds > 0;
			set => fDisableDriverCorrectnessWorkarounds = value ? (byte)1 : (byte)0;
		}

		// public int32_t fInternalMultisampleCount
		private Int32 fInternalMultisampleCount;
		public Int32 InternalMultisampleCount {
			readonly get => fInternalMultisampleCount;
			set => fInternalMultisampleCount = value;
		}

		// public int64_t fGpuBudgetInBytes
		private Int64 fGpuBudgetInBytes;
		public Int64 GpuBudgetInBytes {
			readonly get => fGpuBudgetInBytes;
			set => fGpuBudgetInBytes = value;
		}

		// public bool fRequireOrderedRecordings
		private Byte fRequireOrderedRecordings;
		public bool RequireOrderedRecordings {
			readonly get => fRequireOrderedRecordings > 0;
			set => fRequireOrderedRecordings = value ? (byte)1 : (byte)0;
		}

		// public bool fSetBackendLabels
		private Byte fSetBackendLabels;
		public bool SetBackendLabels {
			readonly get => fSetBackendLabels > 0;
			set => fSetBackendLabels = value ? (byte)1 : (byte)0;
		}

		public readonly bool Equals (SKGraphiteContextOptions obj) =>
#pragma warning disable CS8909
			fDisableDriverCorrectnessWorkarounds == obj.fDisableDriverCorrectnessWorkarounds && fInternalMultisampleCount == obj.fInternalMultisampleCount && fGpuBudgetInBytes == obj.fGpuBudgetInBytes && fRequireOrderedRecordings == obj.fRequireOrderedRecordings && fSetBackendLabels == obj.fSetBackendLabels;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteContextOptions f && Equals (f);

		public static bool operator == (SKGraphiteContextOptions left, SKGraphiteContextOptions right) =>
			left.Equals (right);

		public static bool operator != (SKGraphiteContextOptions left, SKGraphiteContextOptions right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fDisableDriverCorrectnessWorkarounds);
			hash.Add (fInternalMultisampleCount);
			hash.Add (fGpuBudgetInBytes);
			hash.Add (fRequireOrderedRecordings);
			hash.Add (fSetBackendLabels);
			return hash.ToHashCode ();
		}

	}
}
