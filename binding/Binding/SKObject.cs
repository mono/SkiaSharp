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

namespace SkiaSharp
{
	public abstract class SKObject : IDisposable
	{
		private static readonly Dictionary<IntPtr, WeakReference> instances = new Dictionary<IntPtr, WeakReference>();

		private readonly List<SKObject> ownedObjects = new List<SKObject>();
		private IntPtr handle;

		[Preserve]
		internal SKObject(IntPtr handle, bool owns)
		{
			Handle = handle;
			OwnsHandle = owns;
		}

		~SKObject()
		{
			Dispose(false);
		}

		protected bool OwnsHandle { get; private set; }

		public IntPtr Handle
		{
			get { return handle; }
			protected set
			{
				handle = value;
				RegisterHandle(handle, this);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			lock (ownedObjects)
			{
				foreach (var child in ownedObjects)
				{
					child.Dispose();
				}
				ownedObjects.Clear();
			}

			DeregisterHandle(handle, this);
			handle = IntPtr.Zero;}

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
					if (instance != null && instance.handle != IntPtr.Zero)
					{
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

		internal static void DeregisterHandle(IntPtr handle, SKObject instance)
		{
			if (handle == IntPtr.Zero)
			{
				return;
			}

			lock (instances)
			{
				// find any references
				WeakReference reference;
				if (instances.TryGetValue(handle, out reference))
				{
					var shouldRemove =
						reference == null ||
						reference.Target == null ||
						reference.Target == instance;

					// remove it if it is dead or the correct object
					instances.Remove(handle);
				}
			}
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
	}
}
