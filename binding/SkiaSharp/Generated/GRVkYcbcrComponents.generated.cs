using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_vk_ycbcr_components_t
	/// <summary>Describes the component mapping for a Vulkan YCbCr conversion.</summary>
	/// <remarks><![CDATA[
	/// ## Remarks
	///
	/// `GRVkYcbcrComponents` maps each color component (R, G, B, A) to a source channel index as used by Vulkan's `VkComponentMapping`. It is part of <xref:SkiaSharp.GRVkYcbcrConversionInfo>.
	///
	/// Each property stores a `VkComponentSwizzle` value (as a `uint`) indicating which Vulkan image component feeds the corresponding output channel.
	///
	/// ## Examples
	///
	/// Creating a default component mapping (identity swizzle):
	///
	/// ```csharp
	/// var components = new GRVkYcbcrComponents
	/// {
	///     R = 0, // VK_COMPONENT_SWIZZLE_IDENTITY
	///     G = 0,
	///     B = 0,
	///     A = 0,
	/// };
	/// ```
	/// ]]></remarks>
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GRVkYcbcrComponents : IEquatable<GRVkYcbcrComponents> {
		// public uint32_t r
		private UInt32 r;
		/// <summary>Gets or sets the Vulkan component swizzle for the red channel.</summary>
		/// <value>A <c>VkComponentSwizzle</c> value indicating the source for the red output channel.</value>
		/// <remarks />
		public UInt32 R {
			readonly get => r;
			set => r = value;
		}

		// public uint32_t g
		private UInt32 g;
		/// <summary>Gets or sets the Vulkan component swizzle for the green channel.</summary>
		/// <value>A <c>VkComponentSwizzle</c> value indicating the source for the green output channel.</value>
		/// <remarks />
		public UInt32 G {
			readonly get => g;
			set => g = value;
		}

		// public uint32_t b
		private UInt32 b;
		/// <summary>Gets or sets the Vulkan component swizzle for the blue channel.</summary>
		/// <value>A <c>VkComponentSwizzle</c> value indicating the source for the blue output channel.</value>
		/// <remarks />
		public UInt32 B {
			readonly get => b;
			set => b = value;
		}

		// public uint32_t a
		private UInt32 a;
		/// <summary>Gets or sets the Vulkan component swizzle for the alpha channel.</summary>
		/// <value>A <c>VkComponentSwizzle</c> value indicating the source for the alpha output channel.</value>
		/// <remarks />
		public UInt32 A {
			readonly get => a;
			set => a = value;
		}

		/// <summary>Indicates whether this component mapping is equal to another <see cref="T:SkiaSharp.GRVkYcbcrComponents" />.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.GRVkYcbcrComponents" /> to compare with this instance.</param>
		/// <returns><see langword="true" /> if both instances have identical component swizzle values; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (GRVkYcbcrComponents obj) =>
#pragma warning disable CS8909
			r == obj.r && g == obj.g && b == obj.b && a == obj.a;
#pragma warning restore CS8909

		/// <summary>Indicates whether this component mapping is equal to the specified object.</summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> is a <see cref="T:SkiaSharp.GRVkYcbcrComponents" /> with identical component swizzle values; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is GRVkYcbcrComponents f && Equals (f);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.GRVkYcbcrComponents" /> values are equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.GRVkYcbcrComponents" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.GRVkYcbcrComponents" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (GRVkYcbcrComponents left, GRVkYcbcrComponents right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.GRVkYcbcrComponents" /> values are not equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.GRVkYcbcrComponents" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.GRVkYcbcrComponents" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (GRVkYcbcrComponents left, GRVkYcbcrComponents right) =>
			!left.Equals (right);

		/// <summary>Returns a hash code for this component mapping.</summary>
		/// <returns>A hash code for this <see cref="T:SkiaSharp.GRVkYcbcrComponents" /> instance.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (r);
			hash.Add (g);
			hash.Add (b);
			hash.Add (a);
			return hash.ToHashCode ();
		}

	}
}
