using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_vk_alloc_t
	/// <summary>Represents Vulkan memory allocation information for use with Skia's GPU backend.</summary>
	/// <remarks />
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GRVkAlloc : IEquatable<GRVkAlloc> {
		// public uint64_t fMemory
		private UInt64 fMemory;
		/// <summary>Gets or sets the Vulkan device memory handle.</summary>
		/// <value>The VkDeviceMemory handle as an unsigned 64-bit integer.</value>
		/// <remarks />
		public UInt64 Memory {
			readonly get => fMemory;
			set => fMemory = value;
		}

		// public uint64_t fOffset
		private UInt64 fOffset;
		/// <summary>Gets or sets the offset within the device memory allocation.</summary>
		/// <value>The offset in bytes.</value>
		/// <remarks />
		public UInt64 Offset {
			readonly get => fOffset;
			set => fOffset = value;
		}

		// public uint64_t fSize
		private UInt64 fSize;
		/// <summary>Gets or sets the size of the memory allocation.</summary>
		/// <value>The size in bytes.</value>
		/// <remarks />
		public UInt64 Size {
			readonly get => fSize;
			set => fSize = value;
		}

		// public uint32_t fFlags
		private UInt32 fFlags;
		/// <summary>Gets or sets the allocation flags.</summary>
		/// <value>The allocation flags.</value>
		/// <remarks />
		public UInt32 Flags {
			readonly get => fFlags;
			set => fFlags = value;
		}

		// public gr_vk_backendmemory_t fBackendMemory
		private IntPtr fBackendMemory;
		/// <summary>Gets or sets the backend memory handle for custom memory allocators.</summary>
		/// <value>The backend memory handle.</value>
		/// <remarks />
		public IntPtr BackendMemory {
			readonly get => fBackendMemory;
			set => fBackendMemory = value;
		}

		// public bool _private_fUsesSystemHeap
		private Byte fUsesSystemHeap;

		/// <summary>Determines whether the specified <see cref="T:SkiaSharp.GRVkAlloc" /> is equal to the current instance.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.GRVkAlloc" /> to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (GRVkAlloc obj) =>
#pragma warning disable CS8909
			fMemory == obj.fMemory && fOffset == obj.fOffset && fSize == obj.fSize && fFlags == obj.fFlags && fBackendMemory == obj.fBackendMemory && fUsesSystemHeap == obj.fUsesSystemHeap;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is GRVkAlloc f && Equals (f);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.GRVkAlloc" /> instances are equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.GRVkAlloc" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.GRVkAlloc" /> to compare.</param>
		/// <returns><see langword="true" /> if the two instances are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (GRVkAlloc left, GRVkAlloc right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.GRVkAlloc" /> instances are not equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.GRVkAlloc" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.GRVkAlloc" /> to compare.</param>
		/// <returns><see langword="true" /> if the two instances are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (GRVkAlloc left, GRVkAlloc right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
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
