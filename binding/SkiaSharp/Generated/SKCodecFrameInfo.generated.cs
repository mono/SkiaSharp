using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_codec_frameinfo_t
	/// <summary>Information about individual frames in a multi-framed image.</summary>
	/// <remarks />
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKCodecFrameInfo : IEquatable<SKCodecFrameInfo> {
		// public int fRequiredFrame
		private Int32 fRequiredFrame;
		/// <summary>Gets or sets the frame that this frame needs to be blended with, or -1.</summary>
		/// <value>The index of the required frame, or -1 if no frame is required.</value>
		/// <remarks />
		public Int32 RequiredFrame {
			readonly get => fRequiredFrame;
			set => fRequiredFrame = value;
		}

		// public int fDuration
		private Int32 fDuration;
		/// <summary>Gets or sets the number of milliseconds to show this frame.</summary>
		/// <value>The duration in milliseconds.</value>
		/// <remarks />
		public Int32 Duration {
			readonly get => fDuration;
			set => fDuration = value;
		}

		// public bool fFullyReceived
		private Byte fFullyReceived;
		/// <summary>Gets or sets a value indicating whether the end marker for this frame is contained in the stream.</summary>
		/// <value><see langword="true" /> if the frame has been fully received; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool FullyRecieved {
			readonly get => fFullyReceived > 0;
			set => fFullyReceived = value ? (byte)1 : (byte)0;
		}

		// public sk_alphatype_t fAlphaType
		private SKAlphaType fAlphaType;
		/// <summary>Gets or sets a value indicating the frame's alpha value.</summary>
		/// <value>The alpha type for this frame.</value>
		/// <remarks />
		public SKAlphaType AlphaType {
			readonly get => fAlphaType;
			set => fAlphaType = value;
		}

		// public bool fHasAlphaWithinBounds
		private Byte fHasAlphaWithinBounds;
		/// <summary>Gets or sets a value indicating whether this frame has alpha within its bounds.</summary>
		/// <value><see langword="true" /> if this frame contains transparency within its bounds; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool HasAlphaWithinBounds {
			readonly get => fHasAlphaWithinBounds > 0;
			set => fHasAlphaWithinBounds = value ? (byte)1 : (byte)0;
		}

		// public sk_codecanimation_disposalmethod_t fDisposalMethod
		private SKCodecAnimationDisposalMethod fDisposalMethod;
		/// <summary>Gets or sets the method indicating how the current frame should be modified before decoding the next one.</summary>
		/// <value>The disposal method for this frame.</value>
		/// <remarks />
		public SKCodecAnimationDisposalMethod DisposalMethod {
			readonly get => fDisposalMethod;
			set => fDisposalMethod = value;
		}

		// public sk_codecanimation_blend_t fBlend
		private SKCodecAnimationBlend fBlend;
		/// <summary>Gets or sets the blend mode for this frame.</summary>
		/// <value>The blend mode that specifies how the frame should be blended with the previous frame.</value>
		/// <remarks />
		public SKCodecAnimationBlend Blend {
			readonly get => fBlend;
			set => fBlend = value;
		}

		// public sk_irect_t fFrameRect
		private SKRectI fFrameRect;
		/// <summary>Gets or sets the rectangle within the image that this frame occupies.</summary>
		/// <value>The bounding rectangle for this frame within the image.</value>
		/// <remarks />
		public SKRectI FrameRect {
			readonly get => fFrameRect;
			set => fFrameRect = value;
		}

		/// <summary>Determines whether the specified <see cref="T:SkiaSharp.SKCodecFrameInfo" /> is equal to the current instance.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKCodecFrameInfo" /> to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKCodecFrameInfo obj) =>
#pragma warning disable CS8909
			fRequiredFrame == obj.fRequiredFrame && fDuration == obj.fDuration && fFullyReceived == obj.fFullyReceived && fAlphaType == obj.fAlphaType && fHasAlphaWithinBounds == obj.fHasAlphaWithinBounds && fDisposalMethod == obj.fDisposalMethod && fBlend == obj.fBlend && fFrameRect == obj.fFrameRect;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKCodecFrameInfo f && Equals (f);

		/// <summary>Determines whether two specified <see cref="T:SkiaSharp.SKCodecFrameInfo" /> objects have the same value.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKCodecFrameInfo" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKCodecFrameInfo" /> to compare.</param>
		/// <returns><see langword="true" /> if the value of <paramref name="left" /> is the same as the value of <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKCodecFrameInfo left, SKCodecFrameInfo right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified <see cref="T:SkiaSharp.SKCodecFrameInfo" /> objects have different values.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKCodecFrameInfo" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKCodecFrameInfo" /> to compare.</param>
		/// <returns><see langword="true" /> if the value of <paramref name="left" /> is different from the value of <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKCodecFrameInfo left, SKCodecFrameInfo right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fRequiredFrame);
			hash.Add (fDuration);
			hash.Add (fFullyReceived);
			hash.Add (fAlphaType);
			hash.Add (fHasAlphaWithinBounds);
			hash.Add (fDisposalMethod);
			hash.Add (fBlend);
			hash.Add (fFrameRect);
			return hash.ToHashCode ();
		}

	}
}
