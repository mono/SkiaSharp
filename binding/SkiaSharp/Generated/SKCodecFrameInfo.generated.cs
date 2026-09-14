using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_codec_frameinfo_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKCodecFrameInfo : IEquatable<SKCodecFrameInfo> {
		// public int fRequiredFrame
		private Int32 fRequiredFrame;
		public Int32 RequiredFrame {
			readonly get => fRequiredFrame;
			set => fRequiredFrame = value;
		}

		// public int fDuration
		private Int32 fDuration;
		public Int32 Duration {
			readonly get => fDuration;
			set => fDuration = value;
		}

		// public bool fFullyReceived
		private Byte fFullyReceived;
		public bool FullyRecieved {
			readonly get => fFullyReceived > 0;
			set => fFullyReceived = value ? (byte)1 : (byte)0;
		}

		// public sk_alphatype_t fAlphaType
		private SKAlphaType fAlphaType;
		public SKAlphaType AlphaType {
			readonly get => fAlphaType;
			set => fAlphaType = value;
		}

		// public bool fHasAlphaWithinBounds
		private Byte fHasAlphaWithinBounds;
		public bool HasAlphaWithinBounds {
			readonly get => fHasAlphaWithinBounds > 0;
			set => fHasAlphaWithinBounds = value ? (byte)1 : (byte)0;
		}

		// public sk_codecanimation_disposalmethod_t fDisposalMethod
		private SKCodecAnimationDisposalMethod fDisposalMethod;
		public SKCodecAnimationDisposalMethod DisposalMethod {
			readonly get => fDisposalMethod;
			set => fDisposalMethod = value;
		}

		// public sk_codecanimation_blend_t fBlend
		private SKCodecAnimationBlend fBlend;
		public SKCodecAnimationBlend Blend {
			readonly get => fBlend;
			set => fBlend = value;
		}

		// public sk_irect_t fFrameRect
		private SKRectI fFrameRect;
		public SKRectI FrameRect {
			readonly get => fFrameRect;
			set => fFrameRect = value;
		}

		public readonly bool Equals (SKCodecFrameInfo obj) =>
#pragma warning disable CS8909
			fRequiredFrame == obj.fRequiredFrame && fDuration == obj.fDuration && fFullyReceived == obj.fFullyReceived && fAlphaType == obj.fAlphaType && fHasAlphaWithinBounds == obj.fHasAlphaWithinBounds && fDisposalMethod == obj.fDisposalMethod && fBlend == obj.fBlend && fFrameRect == obj.fFrameRect;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKCodecFrameInfo f && Equals (f);

		public static bool operator == (SKCodecFrameInfo left, SKCodecFrameInfo right) =>
			left.Equals (right);

		public static bool operator != (SKCodecFrameInfo left, SKCodecFrameInfo right) =>
			!left.Equals (right);

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
