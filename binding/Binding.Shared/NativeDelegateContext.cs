using System;
using System.Collections.Concurrent;
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
		// instead of pinning the class, we pin a GUID which is paired to the class
		private static readonly ConcurrentDictionary<Guid, NativeDelegateContext> contextsMap = new ConcurrentDictionary<Guid, NativeDelegateContext> ();

		// the "managed version" of the callback
		private readonly Delegate[] managedDelegates;
		private readonly object[] managedContexts;

		public NativeDelegateContext (object context, Delegate del)
			: this (new[] { context }, new[] { del })
		{
		}

		public NativeDelegateContext (object[] contexts, Delegate[] delegates)
		{
			_ = contexts ?? throw new ArgumentNullException (nameof (contexts));
			_ = delegates ?? throw new ArgumentNullException (nameof (delegates));

			managedDelegates = delegates;
			managedContexts = contexts;

			NativeContext = Wrap ();
		}

		public IntPtr NativeContext { get; }

		public object ManagedContext => managedContexts[0];

		public object GetManagedContext (int i) => managedContexts[i];

		public T GetDelegate<T> () => GetDelegate<T> (0);

		public T GetDelegate<T> (int i) => (T)(object)managedDelegates[i];

		// wrap this context into a "native" pointer
		public IntPtr Wrap ()
		{
			var guid = Guid.NewGuid ();
			contextsMap[guid] = this;
			var gc = GCHandle.Alloc (guid, GCHandleType.Pinned);
			return GCHandle.ToIntPtr (gc);
		}

		// unwrap the "native" pointer into a managed context
		public static NativeDelegateContext Unwrap (IntPtr ptr)
		{
			var gchandle = GCHandle.FromIntPtr (ptr);
			var guid = (Guid)gchandle.Target;
			contextsMap.TryGetValue (guid, out var value);
			return value;
		}

		public void Free () => Free (NativeContext);

		// unwrap and free the context
		public static void Free (IntPtr ptr)
		{
			var gchandle = GCHandle.FromIntPtr (ptr);
			var guid = (Guid)gchandle.Target;
			contextsMap.TryRemove (guid, out _);
			gchandle.Free ();
		}

		void IDisposable.Dispose () => Free ();
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
