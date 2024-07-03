using System;
using System.Runtime.InteropServices;
using Windows.Storage.Streams;
using WinRT;
using WinRT.Interop;

namespace SkiaSharp.Views.Windows
{
	[WindowsRuntimeType("Windows.Foundation.UniversalApiContract")]
	[Guid("905a0fef-bc53-11df-8c49-001e4fc686da")]
	[WindowsRuntimeHelperType(typeof(ABI.SkiaSharp.Views.Windows))]
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

namespace ABI.SkiaSharp.Views.Windows
{
	[DynamicInterfaceCastableImplementation]
	[Guid("905a0fef-bc53-11df-8c49-001e4fc686da")]
	internal unsafe interface IBufferByteAccess : global::SkiaSharp.Views.Windows.IBufferByteAccess
	{
		[Guid("905a0fef-bc53-11df-8c49-001e4fc686da")]
		public struct Vftbl
		{
			internal IUnknownVftbl IUnknownVftbl;
			public delegate* unmanaged[Stdcall]<IntPtr, IntPtr*, int> getBuffer;

			public static readonly IntPtr AbiToProjectionVftablePtr;

			static unsafe Vftbl()
			{
				AbiToProjectionVftablePtr = ComWrappersSupport.AllocateVtableMemory(typeof(Vftbl), sizeof(IUnknownVftbl) + sizeof(IntPtr));
				(*(Vftbl*)AbiToProjectionVftablePtr) = new Vftbl
				{
					IUnknownVftbl = IUnknownVftbl.AbiToProjectionVftbl,
					getBuffer = &DoAbiGetBuffer,
				};
			}

			[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
			private static int DoAbiGetBuffer(IntPtr thisPtr, IntPtr* buffer)
			{
				*buffer = default;
				try
				{
					*buffer = ComWrappersSupport.FindObject<global::SkiaSharp.Views.Windows.IBufferByteAccess>(thisPtr).Buffer;
				}
				catch (Exception ex)
				{
					return Marshal.GetHRForException(ex);
				}
				return 0;
			}
		}
		internal static ObjectReference<IUnknownVftbl> FromAbi(IntPtr thisPtr) => ObjectReference<IUnknownVftbl>.FromAbi(thisPtr, IID.IID_IBufferByteAccess);

		IntPtr global::SkiaSharp.Views.Windows.IBufferByteAccess.Buffer
		{
			get
			{
				var obj = ((IWinRTObject)this).GetObjectReferenceForType(typeof(global::SkiaSharp.Views.Windows.IBufferByteAccess).TypeHandle);
				var ThisPtr = obj.ThisPtr;
				IntPtr buffer = default;
				Marshal.ThrowExceptionForHR(((delegate* unmanaged[Stdcall]<IntPtr, IntPtr*, int>)(*(void***)ThisPtr)[3])(ThisPtr, &buffer));
				return buffer;
			}
		}
	}
}
