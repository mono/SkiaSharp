using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if HARFBUZZ
namespace HarfBuzzSharp
#else
namespace SkiaSharp
#endif
{
	// helper delegates

	internal delegate Delegate GetMultiDelegateDelegate (Type index);

	internal static partial class DelegateProxies
	{
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static T Create<T> (object managedDel, T nativeDel, out GCHandle gch, out IntPtr contextPtr)
		{
			if (managedDel == null) {
				gch = default (GCHandle);
				contextPtr = IntPtr.Zero;
				return default (T);
			}

			gch = GCHandle.Alloc (managedDel);
			contextPtr = GCHandle.ToIntPtr (gch);
			return nativeDel;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static void Create (object managedDel, out GCHandle gch, out IntPtr contextPtr)
		{
			if (managedDel == null) {
				gch = default (GCHandle);
				contextPtr = IntPtr.Zero;
				return;
			}

			gch = GCHandle.Alloc (managedDel);
			contextPtr = GCHandle.ToIntPtr (gch);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static T Get<T> (IntPtr contextPtr, out GCHandle gch)
		{
			if (contextPtr == IntPtr.Zero) {
				gch = default (GCHandle);
				return default (T);
			}

			gch = GCHandle.FromIntPtr (contextPtr);
			return (T)gch.Target;
		}
	}

	[AttributeUsage (AttributeTargets.Method)]
	internal sealed class MonoPInvokeCallbackAttribute : Attribute
	{
		public MonoPInvokeCallbackAttribute (Type type)
		{
			Type = type;
		}

		public Type Type { get; private set; }
	}
}
