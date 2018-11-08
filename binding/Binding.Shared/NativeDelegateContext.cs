using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if HARFBUZZ
namespace HarfBuzzSharp
#else
namespace SkiaSharp
#endif
{
	// This is the actual context passed to native code.
	// Instead of marshalling the user's data as an IntPtr and requiring 
	// him to wrap/unwarp, we do it via a proxy class. This also prevents 
	// us from having to marshal the user's callback too. 
	internal class NativeDelegateContext : IDisposable
	{
		// instead of pinning the struct, we pin a GUID which is paired to the struct
		private static readonly IDictionary<Guid, NativeDelegateContext> contexts = new Dictionary<Guid, NativeDelegateContext>();

		// the "managed version" of the callback 
		private readonly Delegate managedDelegate;

		public NativeDelegateContext(object context, Delegate get)
		{
			managedDelegate = get;
			ManagedContext = context;
			NativeContext = Wrap();
		}

		public object ManagedContext { get; }

		public IntPtr NativeContext { get; }

		public T GetDelegate<T>()
		{
			return (T)(object)managedDelegate;
		}

		// wrap this context into a "native" pointer
		public IntPtr Wrap()
		{
			var guid = Guid.NewGuid();
			lock (contexts)
			{
				contexts.Add(guid, this);
			}
			var gc = GCHandle.Alloc(guid, GCHandleType.Pinned);
			return GCHandle.ToIntPtr(gc);
		}

		// unwrap the "native" pointer into a managed context
		public static NativeDelegateContext Unwrap(IntPtr ptr)
		{
			var gchandle = GCHandle.FromIntPtr(ptr);
			var guid = (Guid)gchandle.Target;
			lock (contexts)
			{
				contexts.TryGetValue (guid, out var value);
				return value;
			}
		}

		public void Free()
		{
			Free(NativeContext);
		}

		// unwrap and free the context
		public static void Free(IntPtr ptr)
		{
			var gchandle = GCHandle.FromIntPtr(ptr);
			var guid = (Guid)gchandle.Target;
			lock (contexts)
			{
				contexts.Remove(guid);
			}
			gchandle.Free();
		}

		void IDisposable.Dispose() => Free();
	}

	[AttributeUsage(AttributeTargets.Method)]
	internal sealed class MonoPInvokeCallbackAttribute : Attribute
	{
		public MonoPInvokeCallbackAttribute(Type type)
		{
			Type = type;
		}

		public Type Type { get; private set; }
	}
}
