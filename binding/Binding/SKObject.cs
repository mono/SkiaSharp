//
// Bindings for SKBitmap
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2016 Xamarin Inc
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
	public abstract class SKObject : SKNativeObject
	{
		private static readonly Dictionary<IntPtr, WeakReference> instances = new Dictionary<IntPtr, WeakReference>();

		private readonly List<SKObject> ownedObjects = new List<SKObject>();
		private int referenceCount = 0;
		private bool ownsHandle = false;

		[Preserve]
		internal SKObject(IntPtr handle, bool owns)
			: base(handle, false)
		{
			OwnsHandle = owns;
		}

		protected bool OwnsHandle
		{
			get { return ownsHandle; }
			private set
			{
				if (ownsHandle != value)
				{
					ownsHandle = value;
					if (value)
						Interlocked.Increment(ref referenceCount);
					else
						Interlocked.Decrement(ref referenceCount);
				}
			}
		}

		public override IntPtr Handle
		{
			get { return base.Handle; }
			protected set
			{
				base.Handle = value;

				RegisterHandle(Handle, this);
			}
		}

		protected override void Dispose(bool disposing)
		{
			lock (ownedObjects)
			{
				foreach (var child in ownedObjects)
				{
					child.Dispose();
				}
				ownedObjects.Clear();
			}

			var zero = DeregisterHandle(Handle, this);

			base.Dispose(disposing);

			if (zero)
				Handle = IntPtr.Zero;
		}

		internal static TSkiaObject GetObject<TSkiaObject>(IntPtr handle, bool owns = true)
			where TSkiaObject : SKObject
		{
			if (handle == IntPtr.Zero)
			{
				return null;
			}

			lock (instances)
			{
				// find any existing managed werappers
				WeakReference reference;
				if (instances.TryGetValue(handle, out reference))
				{
					var instance = reference.Target as TSkiaObject;
					if (instance != null && instance.Handle != IntPtr.Zero)
					{
						if (owns)
							Interlocked.Increment(ref instance.referenceCount);
						return instance;
					}
				}
			}

			// create a new wrapper 
			// TODO: we could probably cache this
			var type = typeof(TSkiaObject);
			var constructor = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => 
				c.GetParameters().Length == 2 && c.GetParameters()[0].ParameterType == typeof(IntPtr) && c.GetParameters()[1].ParameterType == typeof(bool));
			if (constructor == null)
			{
				throw new MissingMethodException($"No constructor found for {type.FullName}.ctor(System.IntPtr, System.Boolean)");
			}
			return (TSkiaObject)constructor.Invoke(new object[] { handle, owns });
		}

		internal static void RegisterHandle(IntPtr handle, SKObject instance)
		{
			if (handle == IntPtr.Zero)
			{
				return;
			}

			lock (instances)
			{
				// find old references
				WeakReference reference;
				if (instances.TryGetValue(handle, out reference))
				{
					var shouldReplace =
						reference == null ||
						reference.Target == null ||
						((SKObject)reference.Target).Handle == IntPtr.Zero;

					Debug.WriteLineIf(!shouldReplace, "Not replacing existing, living, managed instance with new object.");

					// replace the old one if it is dead
					instances[handle] = new WeakReference(instance);
				}
				else
				{
					// add a new reference
					instances.Add(handle, new WeakReference(instance));
				}
			}
		}

		internal static bool DeregisterHandle(IntPtr handle, SKObject instance)
		{
			if (handle == IntPtr.Zero)
			{
				return false;
			}

			lock (instances)
			{
				// find any references
				WeakReference reference;
				if (Interlocked.Decrement(ref instance.referenceCount) <= 0 && instances.TryGetValue(handle, out reference))
				{
					// remove it if it is dead or the correct object
					instances.Remove(handle);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// This object will take ownership of the specified object.
		/// </summary>
		/// <param name="obj">The object to own.</param>
		internal void TakeOwnership(SKObject obj)
		{
			lock (ownedObjects)
			{
				ownedObjects.Add(obj);
			}
			obj.RevokeOwnership ();
		}

		/// <summary>
		/// This object will no longer own it's handle.
		/// </summary>
		internal void RevokeOwnership()
		{
			OwnsHandle = false;
		}

		/// <summary>
		/// This object will hand ownership over to the specified object.
		/// </summary>
		/// <param name="owner">The object to give ownership to.</param>
		internal void RevokeOwnership(SKObject owner)
		{
			if (owner != null) {
				owner.TakeOwnership (this);
			} else {
				this.RevokeOwnership ();
			}
		}

		internal static int SizeOf <T> ()
		{
#if WINDOWS_UWP || NET_STANDARD
			return Marshal.SizeOf <T> ();
#else
			return Marshal.SizeOf (typeof (T));
#endif
		}

		internal T PtrToStructure <T> (IntPtr intPtr)
		{
#if WINDOWS_UWP || NET_STANDARD
			return Marshal.PtrToStructure <T> (intPtr);
#else
			return (T) Marshal.PtrToStructure (intPtr, typeof (T));
#endif
		}

		internal T[] PtrToStructureArray <T> (IntPtr intPtr, int count)
		{
			var items = new T[count];
			var size = SizeOf <T> ();
			for (var i = 0; i < count; i++) {
				var newPtr = new IntPtr (intPtr.ToInt64 () + (i * size));
				items[i] = PtrToStructure <T> (newPtr);
			}
			return items;
		}

		internal T PtrToStructure <T> (IntPtr intPtr, int index)
		{
			var size = SizeOf <T> ();
			var newPtr = new IntPtr (intPtr.ToInt64 () + (index * size));
			return PtrToStructure <T> (newPtr);
		}
	}

	public class SKNativeObject : IDisposable
	{
		private readonly bool zero;

		internal SKNativeObject(IntPtr handle)
		{
			Handle = handle;
			zero = true;
		}

		internal SKNativeObject(IntPtr handle, bool zero)
		{
			Handle = handle;
			this.zero = zero;
		}

		~SKNativeObject()
		{
			Dispose(false);
		}

		public virtual IntPtr Handle { get; protected set; }

		protected virtual void Dispose(bool disposing)
		{
			if (zero)
				Handle = IntPtr.Zero;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
