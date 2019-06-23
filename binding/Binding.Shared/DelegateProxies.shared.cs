using System;
using System.Runtime.InteropServices;

#if HARFBUZZ
namespace HarfBuzzSharp
#else
namespace SkiaSharp
#endif
{
	internal delegate Delegate GetMultiDelegateDelegate (Type index);

	internal static partial class DelegateProxies
	{
		public static T Create<T> (Delegate managedDel, T nativeDel, out GCHandle gch, out IntPtr contextPtr)
			where T : Delegate
		{
			gch = GCHandle.Alloc (managedDel);

			contextPtr = managedDel != null
				? GCHandle.ToIntPtr (gch)
				: IntPtr.Zero;

			return managedDel != null ? nativeDel : null;
		}

		public static void Create (Delegate managedDel, out GCHandle gch, out IntPtr contextPtr)
		{
			gch = GCHandle.Alloc (managedDel);

			contextPtr = managedDel != null
				? GCHandle.ToIntPtr (gch)
				: IntPtr.Zero;
		}

		public static T Get<T> (IntPtr contextPtr, out GCHandle gch)
			where T : Delegate
		{
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
