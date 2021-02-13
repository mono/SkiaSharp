using System;
using System.Runtime.InteropServices;
using Windows.Storage.Streams;

#if __WINUI__ && !WINDOWS_UWP
using WinRT;
using WinRT.Interop;
#endif

namespace SkiaSharp.Views.Interop
{
#if __WINUI__ && !WINDOWS_UWP
	[WindowsRuntimeType("Windows.Foundation.UniversalApiContract")]
	[Guid("905a0fef-bc53-11df-8c49-001e4fc686da")]
	internal interface IBufferByteAccess
	{
		IntPtr Buffer { get; }
	}

	internal static class Utils
	{
		internal static IntPtr GetByteBuffer(this IBuffer buffer)
		{
			var byteBuffer = buffer.As<IBufferByteAccess>();
			if (byteBuffer == null)
				throw new InvalidCastException("Unable to convert WriteableBitmap.PixelBuffer to IBufferByteAccess.");

			return byteBuffer.Buffer;
		}
	}
}

namespace ABI.SkiaSharp.Views.Interop
{
	[DynamicInterfaceCastableImplementation]
	[Guid("905a0fef-bc53-11df-8c49-001e4fc686da")]
	internal interface IBufferByteAccess : global::SkiaSharp.Views.Interop.IBufferByteAccess
	{
		public struct VftblPtr
		{
			public IntPtr Vftbl;
		}

		[Guid("905a0fef-bc53-11df-8c49-001e4fc686da")]
		public struct Vftbl
		{
			public delegate int GetBufferDelegate(IntPtr thisPtr, out IntPtr buffer);

			internal IUnknownVftbl IUnknownVftbl;
			public GetBufferDelegate getBuffer;

			static unsafe Vftbl()
			{
				AbiToProjectionVftable = new Vftbl
				{
					IUnknownVftbl = IUnknownVftbl.AbiToProjectionVftbl,
					getBuffer = DoAbiGetBuffer
				};
				var nativeVftbl = (IntPtr*)Marshal.AllocCoTaskMem(Marshal.SizeOf<IUnknownVftbl>() + sizeof(IntPtr) * 12);
				Marshal.StructureToPtr(AbiToProjectionVftable.IUnknownVftbl, (IntPtr)nativeVftbl, false);
				nativeVftbl[3] = Marshal.GetFunctionPointerForDelegate(AbiToProjectionVftable.getBuffer);
				AbiToProjectionVftablePtr = (IntPtr)nativeVftbl;
			}

			public static readonly Vftbl AbiToProjectionVftable;
			public static readonly IntPtr AbiToProjectionVftablePtr;

			internal unsafe Vftbl(IntPtr ptr)
			{
				var vftblPtr = Marshal.PtrToStructure<VftblPtr>(ptr);
				var vftbl = (IntPtr*)vftblPtr.Vftbl;
				IUnknownVftbl = Marshal.PtrToStructure<IUnknownVftbl>(vftblPtr.Vftbl);
				getBuffer = Marshal.GetDelegateForFunctionPointer<GetBufferDelegate>(vftbl[3]);
			}

			private static int DoAbiGetBuffer(IntPtr thisPtr, out IntPtr buffer)
			{
				buffer = default;
				try
				{
					buffer = ComWrappersSupport.FindObject<global::SkiaSharp.Views.Interop.IBufferByteAccess>(thisPtr).Buffer;
				}
				catch (Exception ex)
				{
					return Marshal.GetHRForException(ex);
				}
				return 0;
			}
		}

		IntPtr global::SkiaSharp.Views.Interop.IBufferByteAccess.Buffer
		{
			get
			{
				var obj = (ObjectReference<Vftbl>)((IWinRTObject)this).GetObjectReferenceForType(typeof(global::SkiaSharp.Views.Interop.IBufferByteAccess).TypeHandle);
				var ThisPtr = obj.ThisPtr;
				Marshal.ThrowExceptionForHR(obj.Vftbl.getBuffer(ThisPtr, out IntPtr buffer));
				return buffer;
			}
		}

		internal static ObjectReference<Vftbl> FromAbi(IntPtr thisPtr) =>
			ObjectReference<Vftbl>.FromAbi(thisPtr);
	}
#else
	[ComImport]
	[Guid("905a0fef-bc53-11df-8c49-001e4fc686da")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IBufferByteAccess
	{
		long Buffer([Out] out IntPtr value);
	}

	internal static class Utils
	{
		internal static IntPtr GetByteBuffer(this IBuffer buffer)
		{
			var byteBuffer = buffer as IBufferByteAccess;
			if (byteBuffer == null)
				throw new InvalidCastException("Unable to convert WriteableBitmap.PixelBuffer to IBufferByteAccess.");

			var hr = byteBuffer.Buffer(out var ptr);
			if (hr < 0)
				throw new InvalidCastException("Unable to retrieve pixel address from WriteableBitmap.PixelBuffer.");

			return ptr;
		}
	}
#endif
}
