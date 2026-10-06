#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>An abstract base class for receiving memory tracing callbacks from Skia.</summary>
	/// <remarks />
	public unsafe class SKTraceMemoryDump : SKObject, ISKSkipObjectRegistration
	{
		private static readonly SKManagedTraceMemoryDumpDelegates delegates;
		private readonly IntPtr userData;

		static SKTraceMemoryDump ()
		{
			delegates = new SKManagedTraceMemoryDumpDelegates {
				fDumpNumericValue = DelegateProxies.SKManagedTraceMemoryDumpDumpNumericValueProxy,
				fDumpStringValue = DelegateProxies.SKManagedTraceMemoryDumpDumpStringValueProxy,
			};

			SkiaApi.sk_managedtracememorydump_set_procs (delegates);
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKTraceMemoryDump" />.</summary>
		/// <param name="detailedDump">Whether to include detailed memory information.</param>
		/// <param name="dumpWrappedObjects">Whether to include wrapped objects in the dump.</param>
		/// <remarks />
		protected SKTraceMemoryDump (bool detailedDump, bool dumpWrappedObjects)
			: base (IntPtr.Zero, true)
		{
			userData = DelegateProxies.CreateUserData (this, true);
			Handle = SkiaApi.sk_managedtracememorydump_new (detailedDump, dumpWrappedObjects, (void*)userData);

			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKTraceMemoryDump instance.");
		}

		/// <summary>Releases the native resources associated with this object.</summary>
		/// <remarks />
		protected override void DisposeNative ()
		{
			DelegateProxies.GetUserData<SKTraceMemoryDump> (userData, out var gch);

			SkiaApi.sk_managedtracememorydump_delete (Handle);

			gch.Free ();
		}

		/// <summary>Called when a numeric value is being dumped.</summary>
		/// <param name="dumpName">The name of the memory dump entry.</param>
		/// <param name="valueName">The name of the numeric value.</param>
		/// <param name="units">The units of measurement for the value.</param>
		/// <param name="value">The numeric value.</param>
		/// <remarks />
		protected internal virtual void OnDumpNumericValue (string dumpName, string valueName, string units, ulong value)
		{
		}

		/// <summary>Called when a string value is being dumped.</summary>
		/// <param name="dumpName">The name of the memory dump entry.</param>
		/// <param name="valueName">The name of the string value.</param>
		/// <param name="value">The string value.</param>
		/// <remarks />
		protected internal virtual void OnDumpStringValue (string dumpName, string valueName, string value)
		{
		}
	}
}
