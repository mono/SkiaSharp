using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_vk_alloc_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GRVkAlloc : IEquatable<GRVkAlloc> {
		// public uint64_t fMemory
		private UInt64 fMemory;
		public UInt64 Memory {
			readonly get => fMemory;
			set => fMemory = value;
		}

		// public uint64_t fOffset
		private UInt64 fOffset;
		public UInt64 Offset {
			readonly get => fOffset;
			set => fOffset = value;
		}

		// public uint64_t fSize
		private UInt64 fSize;
		public UInt64 Size {
			readonly get => fSize;
			set => fSize = value;
		}

		// public uint32_t fFlags
		private UInt32 fFlags;
		public UInt32 Flags {
			readonly get => fFlags;
			set => fFlags = value;
		}

		// public gr_vk_backendmemory_t fBackendMemory
		private IntPtr fBackendMemory;
		public IntPtr BackendMemory {
			readonly get => fBackendMemory;
			set => fBackendMemory = value;
		}

		// public bool _private_fUsesSystemHeap
		private Byte fUsesSystemHeap;

		public readonly bool Equals (GRVkAlloc obj) =>
#pragma warning disable CS8909
			fMemory == obj.fMemory && fOffset == obj.fOffset && fSize == obj.fSize && fFlags == obj.fFlags && fBackendMemory == obj.fBackendMemory && fUsesSystemHeap == obj.fUsesSystemHeap;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GRVkAlloc f && Equals (f);

		public static bool operator == (GRVkAlloc left, GRVkAlloc right) =>
			left.Equals (right);

		public static bool operator != (GRVkAlloc left, GRVkAlloc right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fMemory);
			hash.Add (fOffset);
			hash.Add (fSize);
			hash.Add (fFlags);
			hash.Add (fBackendMemory);
			hash.Add (fUsesSystemHeap);
			return hash.ToHashCode ();
		}

	}
}
