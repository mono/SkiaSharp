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
	[Guid("905a0fef-bc53-11df-8c49-001e4fc686da")]
	internal class IBufferByteAccess : global::SkiaSharp.Views.Interop.IBufferByteAccess
	{
		[Guid("905a0fef-bc53-11df-8c49-001e4fc686da")]
		public struct Vftbl
		{
			public delegate int GetBufferDelegate(IntPtr thisPtr, out IntPtr buffer);

			internal IUnknownVftbl IUnknownVftbl;
			public GetBufferDelegate getBuffer;

			public static readonly Vftbl AbiToProjectionVftable;
			public static readonly IntPtr AbiToProjectionVftablePtr;

			static Vftbl()
			{
				AbiToProjectionVftable = new Vftbl
				{
					IUnknownVftbl = IUnknownVftbl.AbiToProjectionVftbl,
					getBuffer = DoAbiGetBuffer
				};
				AbiToProjectionVftablePtr = Marshal.AllocHGlobal(Marshal.SizeOf<Vftbl>());
				Marshal.StructureToPtr(AbiToProjectionVftable, AbiToProjectionVftablePtr, false);
			}

			public Vftbl(IntPtr ptr)
			{
				this = Marshal.PtrToStructure<Vftbl>(ptr);
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

		protected readonly ObjectReference<Vftbl> obj;

		public IBufferByteAccess(IObjectReference obj)
			: this(obj.As<Vftbl>())
		{
		}

		internal IBufferByteAccess(ObjectReference<Vftbl> obj)
		{
			this.obj = obj;
		}

		public IObjectReference ObjRef => obj;

		public IntPtr ThisPtr => obj.ThisPtr;

		public ObjectReference<I> AsInterface<I>() => obj.As<I>();

		public A As<A>() => obj.AsType<A>();

		public IntPtr Buffer
		{
			get
			{
				Marshal.ThrowExceptionForHR(obj.Vftbl.getBuffer(ThisPtr, out var buffer));
				return buffer;
			}
		}

		internal static ObjectReference<Vftbl> FromAbi(IntPtr thisPtr) =>
			ObjectReference<Vftbl>.FromAbi(thisPtr);

		public static implicit operator IBufferByteAccess(IObjectReference obj) =>
			(obj != null) ? new IBufferByteAccess(obj) : null;
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
