using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using CoreAnimation;
using CoreFoundation;
using CoreGraphics;
using CoreVideo;
using Foundation;
using ObjCRuntime;
using SkiaSharp.Views.GlesInterop;
using UIKit;

namespace OpenGL
{
	[Register("CAOpenGLLayer", true)]
	[SupportedOSPlatform("maccatalyst")]
	[ObsoletedOSPlatform("maccatalyst31.1", "Use 'Metal' Framework instead.")]
	public class CAOpenGLLayer : CALayer
	{
		//[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		//private const string selCanDrawInCGLContext_PixelFormat_ForLayerTime_DisplayTime_X = "canDrawInCGLContext:pixelFormat:forLayerTime:displayTime:";

		//private static readonly NativeHandle selCanDrawInCGLContext_PixelFormat_ForLayerTime_DisplayTime_XHandle = Selector.GetHandle("canDrawInCGLContext:pixelFormat:forLayerTime:displayTime:");

		//[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		//private const string selCopyCGLContextForPixelFormat_X = "copyCGLContextForPixelFormat:";

		//private static readonly NativeHandle selCopyCGLContextForPixelFormat_XHandle = Selector.GetHandle("copyCGLContextForPixelFormat:");

		//[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		//private const string selCopyCGLPixelFormatForDisplayMask_X = "copyCGLPixelFormatForDisplayMask:";

		//private static readonly NativeHandle selCopyCGLPixelFormatForDisplayMask_XHandle = Selector.GetHandle("copyCGLPixelFormatForDisplayMask:");

		[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		private const string selDrawInCGLContext_PixelFormat_ForLayerTime_DisplayTime_X = "drawInCGLContext:pixelFormat:forLayerTime:displayTime:";

		private static readonly NativeHandle selDrawInCGLContext_PixelFormat_ForLayerTime_DisplayTime_XHandle = Selector.GetHandle("drawInCGLContext:pixelFormat:forLayerTime:displayTime:");

		//[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		//private const string selIsAsynchronousX = "isAsynchronous";

		//private static readonly NativeHandle selIsAsynchronousXHandle = Selector.GetHandle("isAsynchronous");

		//[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		//private const string selLayerX = "layer";

		//private static readonly NativeHandle selLayerXHandle = Selector.GetHandle("layer");

		[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		private const string selReleaseCGLContext_X = "releaseCGLContext:";

		private static readonly NativeHandle selReleaseCGLContext_XHandle = Selector.GetHandle("releaseCGLContext:");

		//[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		//private const string selReleaseCGLPixelFormat_X = "releaseCGLPixelFormat:";

		//private static readonly NativeHandle selReleaseCGLPixelFormat_XHandle = Selector.GetHandle("releaseCGLPixelFormat:");

		//[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		//private const string selSetAsynchronous_X = "setAsynchronous:";

		//private static readonly NativeHandle selSetAsynchronous_XHandle = Selector.GetHandle("setAsynchronous:");

		[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		private static readonly NativeHandle class_ptr = ObjCRuntime.Class.GetHandle("CAOpenGLLayer");

		public override NativeHandle ClassHandle => class_ptr;

		//[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		//public virtual bool Asynchronous
		//{
		//	[Export("isAsynchronous")]
		//	get
		//	{
		//		byte ret = ((!base.IsDirectBinding) ? Messaging.bool_objc_msgSendSuper(base.SuperHandle, selIsAsynchronousXHandle) : Messaging.bool_objc_msgSend(base.Handle, selIsAsynchronousXHandle));
		//		return ret != 0;
		//	}
		//	[Export("setAsynchronous:")]
		//	set
		//	{
		//		if (base.IsDirectBinding)
		//		{
		//			Messaging.void_objc_msgSend_bool(base.Handle, selSetAsynchronous_XHandle, value ? ((byte)1) : ((byte)0));
		//		}
		//		else
		//		{
		//			Messaging.void_objc_msgSendSuper_bool(base.SuperHandle, selSetAsynchronous_XHandle, value ? ((byte)1) : ((byte)0));
		//		}
		//	}
		//}


		internal static readonly IntPtr Init = GetHandle("init");

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "sel_registerName")]
		public static extern IntPtr GetHandle(string name);


		[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Export("init")]
		public CAOpenGLLayer()
			: base(NSObjectFlag.Empty)
		{
			if (base.IsDirectBinding)
			{
				InitializeHandle(IntPtr_objc_msgSend(base.Handle, Init), "init");
			}
			else
			{
				InitializeHandle(IntPtr_objc_msgSendSuper(base.SuperHandle, Init), "init");
			}
		}

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSendSuper")]
		public static extern IntPtr IntPtr_objc_msgSendSuper(IntPtr receiver, IntPtr selector);

		//[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		//[DesignatedInitializer]
		//[EditorBrowsable(EditorBrowsableState.Advanced)]
		//[Export("initWithCoder:")]
		//public CAOpenGLLayer(NSCoder coder)
		//	: base(NSObjectFlag.Empty)
		//{
		//	if (base.IsDirectBinding)
		//	{
		//		InitializeHandle(Messaging.IntPtr_objc_msgSend_IntPtr(base.Handle, Selector.InitWithCoder, coder.Handle), "initWithCoder:");
		//	}
		//	else
		//	{
		//		InitializeHandle(Messaging.IntPtr_objc_msgSendSuper_IntPtr(base.SuperHandle, Selector.InitWithCoder, coder.Handle), "initWithCoder:");
		//	}
		//}

		//[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		//[EditorBrowsable(EditorBrowsableState.Advanced)]
		//protected CAOpenGLLayer(NSObjectFlag t)
		//	: base(t)
		//{
		//}

		//[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		//[EditorBrowsable(EditorBrowsableState.Advanced)]
		//protected internal CAOpenGLLayer(NativeHandle handle)
		//	: base(handle)
		//{
		//}

		//[Export("canDrawInCGLContext:pixelFormat:forLayerTime:displayTime:")]
		//[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		//public unsafe virtual bool CanDrawInCGLContext(CGLContext glContext, CGLPixelFormat pixelFormat, double timeInterval, ref CVTimeStamp timeStamp)
		//{
		//	glContext.GetNonNullHandle("glContext");
		//	pixelFormat.GetNonNullHandle("pixelFormat");
		//	byte ret = ((!base.IsDirectBinding) ? Messaging.bool_objc_msgSendSuper_NativeHandle_NativeHandle_Double_ref_CVTimeStamp(base.SuperHandle, selCanDrawInCGLContext_PixelFormat_ForLayerTime_DisplayTime_XHandle, glContext.Handle, pixelFormat.Handle, timeInterval, (CVTimeStamp*)Unsafe.AsPointer(ref timeStamp)) : Messaging.bool_objc_msgSend_NativeHandle_NativeHandle_Double_ref_CVTimeStamp(base.Handle, selCanDrawInCGLContext_PixelFormat_ForLayerTime_DisplayTime_XHandle, glContext.Handle, pixelFormat.Handle, timeInterval, (CVTimeStamp*)Unsafe.AsPointer(ref timeStamp)));
		//	return ret != 0;
		//}

		//[Export("copyCGLPixelFormatForDisplayMask:")]
		//[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		//public virtual CGLPixelFormat CopyCGLPixelFormatForDisplayMask(uint mask)
		//{
		//	if (base.IsDirectBinding)
		//	{
		//		return Runtime.GetINativeObject<CGLPixelFormat>(Messaging.NativeHandle_objc_msgSend_UInt32(base.Handle, selCopyCGLPixelFormatForDisplayMask_XHandle, mask), owns: false);
		//	}
		//	return Runtime.GetINativeObject<CGLPixelFormat>(Messaging.NativeHandle_objc_msgSendSuper_UInt32(base.SuperHandle, selCopyCGLPixelFormatForDisplayMask_XHandle, mask), owns: false);
		//}

		//[Export("copyCGLContextForPixelFormat:")]
		//[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		//public virtual CGLContext CopyContext(CGLPixelFormat pixelFormat)
		//{
		//	pixelFormat.GetNonNullHandle("pixelFormat");
		//	if (base.IsDirectBinding)
		//	{
		//		return Runtime.GetINativeObject<CGLContext>(Messaging.NativeHandle_objc_msgSend_NativeHandle(base.Handle, selCopyCGLContextForPixelFormat_XHandle, pixelFormat.Handle), owns: false);
		//	}
		//	return Runtime.GetINativeObject<CGLContext>(Messaging.NativeHandle_objc_msgSendSuper_NativeHandle(base.SuperHandle, selCopyCGLContextForPixelFormat_XHandle, pixelFormat.Handle), owns: false);
		//}

		//[Export("layer")]
		//[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		//public new static CALayer Create()
		//{
		//	return Runtime.GetNSObject<CALayer>(Messaging.NativeHandle_objc_msgSend(class_ptr, selLayerXHandle));
		//}

		[Export("drawInCGLContext:pixelFormat:forLayerTime:displayTime:")]
		[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public unsafe virtual void DrawInCGLContext(NativeHandle glContext, NativeHandle pixelFormat, double timeInterval, ref CVTimeStamp timeStamp)
		{
			// glContext.GetNonNullHandle("glContext");
			// pixelFormat.GetNonNullHandle("pixelFormat");
			if (base.IsDirectBinding)
			{
				void_objc_msgSend_NativeHandle_NativeHandle_Double_ref_CVTimeStamp(base.Handle, selDrawInCGLContext_PixelFormat_ForLayerTime_DisplayTime_XHandle, glContext, pixelFormat, timeInterval, (CVTimeStamp*)Unsafe.AsPointer(ref timeStamp));
			}
			else
			{
				void_objc_msgSendSuper_NativeHandle_NativeHandle_Double_ref_CVTimeStamp(base.SuperHandle, selDrawInCGLContext_PixelFormat_ForLayerTime_DisplayTime_XHandle, glContext, pixelFormat, timeInterval, (CVTimeStamp*)Unsafe.AsPointer(ref timeStamp));
			}
		}

		//[Export("releaseCGLPixelFormat:")]
		//[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		//public virtual void Release(CGLPixelFormat pixelFormat)
		//{
		//	pixelFormat.GetNonNullHandle("pixelFormat");
		//	if (base.IsDirectBinding)
		//	{
		//		Messaging.void_objc_msgSend_NativeHandle(base.Handle, selReleaseCGLPixelFormat_XHandle, pixelFormat.Handle);
		//	}
		//	else
		//	{
		//		Messaging.void_objc_msgSendSuper_NativeHandle(base.SuperHandle, selReleaseCGLPixelFormat_XHandle, pixelFormat.Handle);
		//	}
		//}

		[Export("releaseCGLContext:")]
		[BindingImpl(BindingImplOptions.GeneratedCode | BindingImplOptions.Optimizable)]
		public virtual void Release(NativeHandle glContext)
		{
			// glContext.GetNonNullHandle("glContext");
			if (base.IsDirectBinding)
			{
				void_objc_msgSend_NativeHandle(base.Handle, selReleaseCGLContext_XHandle, glContext);
			}
			else
			{
				void_objc_msgSendSuper_NativeHandle(base.SuperHandle, selReleaseCGLContext_XHandle, glContext);
			}
		}

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		public unsafe static extern void void_objc_msgSend_NativeHandle_NativeHandle_Double_ref_CVTimeStamp(IntPtr receiver, IntPtr selector, NativeHandle arg1, NativeHandle arg2, double arg3, CVTimeStamp* arg4);
		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSendSuper")]
		public unsafe static extern void void_objc_msgSendSuper_NativeHandle_NativeHandle_Double_ref_CVTimeStamp(IntPtr receiver, IntPtr selector, NativeHandle arg1, NativeHandle arg2, double arg3, CVTimeStamp* arg4);
		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		public static extern void void_objc_msgSend_NativeHandle(IntPtr receiver, IntPtr selector, NativeHandle arg1);
		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSendSuper")]
		public static extern void void_objc_msgSendSuper_NativeHandle(IntPtr receiver, IntPtr selector, NativeHandle arg1);
	}

	// [SupportedOSPlatform("maccatalyst")]
	// [ObsoletedOSPlatform("maccatalyst31.1", "Use 'Metal' Framework instead.")]
	// public class CGLContext : NativeObject
	// {
	// 	public static CGLContext? CurrentContext
	// 	{
	// 		get
	// 		{
	// 			IntPtr ctx = CGLGetCurrentContext();
	// 			if (ctx != IntPtr.Zero)
	// 			{
	// 				return new CGLContext(ctx, owns: false);
	// 			}
	// 			return null;
	// 		}
	// 		set
	// 		{
	// 			if (CGLSetCurrentContext(value.GetHandle()) != 0)
	// 			{
	// 				throw new Exception("Error setting the Current Context");
	// 			}
	// 		}
	// 	}

	// 	[Preserve(Conditional = true)]
	// 	internal CGLContext(NativeHandle handle, bool owns)
	// 		: base(handle, owns, verify: true)
	// 	{
	// 	}

	// 	//[DllImport("/System/Library/Frameworks/OpenGL.framework/OpenGL")]
	// 	//private static extern void CGLRetainContext(IntPtr handle);

	// 	//[DllImport("/System/Library/Frameworks/OpenGL.framework/OpenGL")]
	// 	//private static extern void CGLReleaseContext(IntPtr handle);

	// 	//protected internal override void Retain()
	// 	//{
	// 	//	CGLRetainContext(GetCheckedHandle());
	// 	//}

	// 	//protected internal override void Release()
	// 	//{
	// 	//	CGLReleaseContext(GetCheckedHandle());
	// 	//}

	// 	//[DllImport("/System/Library/Frameworks/OpenGL.framework/OpenGL")]
	// 	//private static extern CGLErrorCode CGLLockContext(IntPtr ctx);

	// 	//public CGLErrorCode Lock()
	// 	//{
	// 	//	return CGLLockContext(base.Handle);
	// 	//}

	// 	//[DllImport("/System/Library/Frameworks/OpenGL.framework/OpenGL")]
	// 	//private static extern CGLErrorCode CGLUnlockContext(IntPtr ctx);

	// 	//public CGLErrorCode Unlock()
	// 	//{
	// 	//	return CGLUnlockContext(base.Handle);
	// 	//}

	// 	[DllImport("/System/Library/Frameworks/OpenGL.framework/OpenGL")]
	// 	private static extern CGLErrorCode CGLSetCurrentContext(IntPtr ctx);

	// 	[DllImport("/System/Library/Frameworks/OpenGL.framework/OpenGL")]
	// 	private static extern IntPtr CGLGetCurrentContext();
	// }

	[SupportedOSPlatform("maccatalyst")]
	[ObsoletedOSPlatform("maccatalyst31.1", "Use 'Metal' Framework instead.")]
	public enum CGLErrorCode : uint
	{
		NoError = 0u,
		BadAttribute = 10000u,
		BadProperty = 10001u,
		BadPixelFormat = 10002u,
		BadRendererInfo = 10003u,
		BadContext = 10004u,
		BadDrawable = 10005u,
		BadDisplay = 10006u,
		BadState = 10007u,
		BadValue = 10008u,
		BadMatch = 10009u,
		BadEnumeration = 10010u,
		BadOffScreen = 10011u,
		BadFullScreen = 10012u,
		BadWindow = 10013u,
		BadAddress = 10014u,
		BadCodeModule = 10015u,
		BadAlloc = 10016u,
		BadConnection = 10017u
	}

	// [SupportedOSPlatform("maccatalyst")]
	// [ObsoletedOSPlatform("maccatalyst31.1", "Use 'Metal' Framework instead.")]
	// public class CGLPixelFormat : NativeObject
	// {
	// 	//protected internal override void Retain()
	// 	//{
	// 	//	CGLRetainPixelFormat(GetCheckedHandle());
	// 	//}

	// 	//protected internal override void Release()
	// 	//{
	// 	//	CGLReleasePixelFormat(GetCheckedHandle());
	// 	//}

	// 	[Preserve(Conditional = true)]
	// 	internal CGLPixelFormat(NativeHandle handle, bool owns)
	// 		: base(handle, owns)
	// 	{
	// 	}

	// 	//[DllImport("/System/Library/Frameworks/OpenGL.framework/OpenGL")]
	// 	//private static extern void CGLRetainPixelFormat(IntPtr handle);

	// 	//[DllImport("/System/Library/Frameworks/OpenGL.framework/OpenGL")]
	// 	//private static extern void CGLReleasePixelFormat(IntPtr handle);

	// 	//[DllImport("/System/Library/Frameworks/OpenGL.framework/OpenGL")]
	// 	//private static extern CGLErrorCode CGLChoosePixelFormat(CGLPixelFormatAttribute[] attributes, out IntPtr pix, out int npix);

	// 	//public CGLPixelFormat(CGLPixelFormatAttribute[] attributes, out int npix)
	// 	//	: base(Create(attributes, out npix), owns: true)
	// 	//{
	// 	//}

	// 	//private static IntPtr Create(CGLPixelFormatAttribute[] attributes, out int npix)
	// 	//{
	// 	//	if (attributes == null)
	// 	//	{
	// 	//		ThrowHelper.ThrowArgumentNullException("attributes");
	// 	//	}
	// 	//	CGLPixelFormatAttribute[] marshalAttribs = new CGLPixelFormatAttribute[attributes.Length + 1];
	// 	//	Array.Copy(attributes, marshalAttribs, attributes.Length);
	// 	//	IntPtr pixelFormatOut;
	// 	//	CGLErrorCode ret = CGLChoosePixelFormat(marshalAttribs, out pixelFormatOut, out npix);
	// 	//	if (ret != 0)
	// 	//	{
	// 	//		throw new Exception("CGLChoosePixelFormat returned: " + ret);
	// 	//	}
	// 	//	return pixelFormatOut;
	// 	//}

	// 	//public CGLPixelFormat(params object[] attributes)
	// 	//	: base(Create(ConvertToAttributes(attributes), out var _), owns: true)
	// 	//{
	// 	//}

	// 	//public CGLPixelFormat(out int npix, params object[] attributes)
	// 	//	: this(ConvertToAttributes(attributes), out npix)
	// 	//{
	// 	//}

	// 	//private static CGLPixelFormatAttribute[] ConvertToAttributes(object[] args)
	// 	//{
	// 	//	List<CGLPixelFormatAttribute> list = new List<CGLPixelFormatAttribute>();
	// 	//	for (int i = 0; i < args.Length; i++)
	// 	//	{
	// 	//		CGLPixelFormatAttribute v = (CGLPixelFormatAttribute)args[i];
	// 	//		switch (v)
	// 	//		{
	// 	//			case CGLPixelFormatAttribute.AllRenderers:
	// 	//			case CGLPixelFormatAttribute.DoubleBuffer:
	// 	//			case CGLPixelFormatAttribute.Stereo:
	// 	//			case CGLPixelFormatAttribute.MinimumPolicy:
	// 	//			case CGLPixelFormatAttribute.MaximumPolicy:
	// 	//			case CGLPixelFormatAttribute.OffScreen:
	// 	//			case CGLPixelFormatAttribute.FullScreen:
	// 	//			case CGLPixelFormatAttribute.AuxDepthStencil:
	// 	//			case CGLPixelFormatAttribute.ColorFloat:
	// 	//			case CGLPixelFormatAttribute.Multisample:
	// 	//			case CGLPixelFormatAttribute.Supersample:
	// 	//			case CGLPixelFormatAttribute.SampleAlpha:
	// 	//			case CGLPixelFormatAttribute.SingleRenderer:
	// 	//			case CGLPixelFormatAttribute.NoRecovery:
	// 	//			case CGLPixelFormatAttribute.Accelerated:
	// 	//			case CGLPixelFormatAttribute.ClosestPolicy:
	// 	//			case CGLPixelFormatAttribute.Robust:
	// 	//			case CGLPixelFormatAttribute.BackingStore:
	// 	//			case CGLPixelFormatAttribute.MPSafe:
	// 	//			case CGLPixelFormatAttribute.Window:
	// 	//			case CGLPixelFormatAttribute.MultiScreen:
	// 	//			case CGLPixelFormatAttribute.Compliant:
	// 	//			case CGLPixelFormatAttribute.PixelBuffer:
	// 	//			case CGLPixelFormatAttribute.RemotePixelBuffer:
	// 	//			case CGLPixelFormatAttribute.AllowOfflineRenderers:
	// 	//			case CGLPixelFormatAttribute.AcceleratedCompute:
	// 	//				list.Add(v);
	// 	//				break;
	// 	//			case CGLPixelFormatAttribute.AuxBuffers:
	// 	//			case CGLPixelFormatAttribute.ColorSize:
	// 	//			case CGLPixelFormatAttribute.AlphaSize:
	// 	//			case CGLPixelFormatAttribute.DepthSize:
	// 	//			case CGLPixelFormatAttribute.StencilSize:
	// 	//			case CGLPixelFormatAttribute.AccumSize:
	// 	//			case CGLPixelFormatAttribute.SampleBuffers:
	// 	//			case CGLPixelFormatAttribute.Samples:
	// 	//			case CGLPixelFormatAttribute.RendererID:
	// 	//			case CGLPixelFormatAttribute.ScreenMask:
	// 	//			case CGLPixelFormatAttribute.VirtualScreenCount:
	// 	//				{
	// 	//					list.Add(v);
	// 	//					i++;
	// 	//					if (i >= args.Length)
	// 	//					{
	// 	//						throw new ArgumentException("Attribute " + v.ToString() + " needs a value");
	// 	//					}
	// 	//					object item = args[i];
	// 	//					CGLPixelFormatAttribute attr = ((!(item is CGLPixelFormatAttribute)) ? ((CGLPixelFormatAttribute)Convert.ChangeType(item, typeof(CGLPixelFormatAttribute)!.GetEnumUnderlyingType())) : ((CGLPixelFormatAttribute)item));
	// 	//					list.Add(attr);
	// 	//					break;
	// 	//				}
	// 	//		}
	// 	//	}
	// 	//	return list.ToArray();
	// 	//}
	// }
}
