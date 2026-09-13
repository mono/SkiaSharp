using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_context_options_t
	/// <summary>Specifies configuration options used when creating a <see cref="T:SkiaSharp.SKGraphiteContext" />.</summary>
	/// <remarks />
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKGraphiteContextOptions : IEquatable<SKGraphiteContextOptions> {
		// public bool fDisableDriverCorrectnessWorkarounds
		private Byte fDisableDriverCorrectnessWorkarounds;
		/// <summary>Gets or sets a value indicating whether driver correctness workarounds are disabled.</summary>
		/// <value><see langword="true" /> to disable driver correctness workarounds; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool DisableDriverCorrectnessWorkarounds {
			readonly get => fDisableDriverCorrectnessWorkarounds > 0;
			set => fDisableDriverCorrectnessWorkarounds = value ? (byte)1 : (byte)0;
		}

		// public int32_t fInternalMultisampleCount
		private Int32 fInternalMultisampleCount;
		/// <summary>Gets or sets the sample count used for internal multisampled rendering.</summary>
		/// <value>The internal multisample count. Must be 0 to use the Skia default, or one of 1, 2, 4, 8, or 16.</value>
		/// <remarks />
		public Int32 InternalMultisampleCount {
			readonly get => fInternalMultisampleCount;
			set => fInternalMultisampleCount = value;
		}

		// public int64_t fGpuBudgetInBytes
		private Int64 fGpuBudgetInBytes;
		/// <summary>Gets or sets the maximum number of bytes of GPU memory the context may use for its resource cache.</summary>
		/// <value>The GPU resource cache budget, in bytes. A negative value uses the Skia default budget.</value>
		/// <remarks />
		public Int64 GpuBudgetInBytes {
			readonly get => fGpuBudgetInBytes;
			set => fGpuBudgetInBytes = value;
		}

		// public bool fRequireOrderedRecordings
		private Byte fRequireOrderedRecordings;
		/// <summary>Gets or sets a value indicating whether recordings must be inserted in the order they were snapped.</summary>
		/// <value><see langword="true" /> if recordings must be inserted in order; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool RequireOrderedRecordings {
			readonly get => fRequireOrderedRecordings > 0;
			set => fRequireOrderedRecordings = value ? (byte)1 : (byte)0;
		}

		// public bool fSetBackendLabels
		private Byte fSetBackendLabels;
		/// <summary>Gets or sets a value indicating whether Skia labels the backend objects it creates, which aids debugging.</summary>
		/// <value><see langword="true" /> to label backend objects; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool SetBackendLabels {
			readonly get => fSetBackendLabels > 0;
			set => fSetBackendLabels = value ? (byte)1 : (byte)0;
		}

		/// <summary>Determines whether the specified context options is equal to the current context options.</summary>
		/// <param name="obj">The context options to compare with the current context options.</param>
		/// <returns><see langword="true" /> if the two values are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKGraphiteContextOptions obj) =>
#pragma warning disable CS8909
			fDisableDriverCorrectnessWorkarounds == obj.fDisableDriverCorrectnessWorkarounds && fInternalMultisampleCount == obj.fInternalMultisampleCount && fGpuBudgetInBytes == obj.fGpuBudgetInBytes && fRequireOrderedRecordings == obj.fRequireOrderedRecordings && fSetBackendLabels == obj.fSetBackendLabels;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current context options.</summary>
		/// <param name="obj">The object to compare with the current context options.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current value; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKGraphiteContextOptions f && Equals (f);

		/// <summary>Indicates whether two context options values are equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true" /> if the two values are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKGraphiteContextOptions left, SKGraphiteContextOptions right) =>
			left.Equals (right);

		/// <summary>Indicates whether two context options values are not equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true" /> if the two values are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKGraphiteContextOptions left, SKGraphiteContextOptions right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this context options.</summary>
		/// <returns>A hash code for the current value.</returns>
		/// <remarks />
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
