#nullable disable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

/*
 * This is a fix for issue #1383.
 *
 *    https://github.com/mono/SkiaSharp/issues/1383
 *
 * On Windows, .NET locks are alertable when using the STA threading model and can
 * cause the Windows message loop to be dispatched (typically on WM_PAINT messages).
 * This can lead to re-entrancy and a deadlock on the HandleDictionary lock.
 *
 * This fix replaces the ReaderWriteLockSlim instance on Windows with a native Win32
 * CRITICAL_SECTION.
 */



namespace SkiaSharp.Internals
{
	/// <summary>Defines a platform-specific reader-writer lock for thread synchronization.</summary>
	/// <remarks />
	public interface IPlatformLock
	{
		/// <summary>Acquires a read lock, allowing multiple concurrent readers.</summary>
		/// <remarks />
		void EnterReadLock ();
		/// <summary>Releases the read lock.</summary>
		/// <remarks />
		void ExitReadLock ();
		/// <summary>Acquires a write lock, providing exclusive access.</summary>
		/// <remarks />
		void EnterWriteLock ();
		/// <summary>Releases the write lock.</summary>
		/// <remarks />
		void ExitWriteLock ();
		/// <summary>Acquires an upgradeable read lock that can later be upgraded to a write lock.</summary>
		/// <remarks />
		void EnterUpgradeableReadLock ();
		/// <summary>Releases the upgradeable read lock.</summary>
		/// <remarks />
		void ExitUpgradeableReadLock ();
	}

	/// <summary>Provides a factory for creating platform-specific synchronization locks.</summary>
	/// <remarks />
	public static partial class PlatformLock
	{
		/// <summary>Creates a new platform lock instance using the current factory.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.Internals.IPlatformLock" /> instance.</returns>
		/// <remarks />
		public static IPlatformLock Create ()
		{
			// Just call the factory
			return Factory ();
		}

		/// <summary>Gets or sets the factory function used to create new platform lock instances.</summary>
		/// <value>A function that creates <see cref="T:SkiaSharp.Internals.IPlatformLock" /> instances.</value>
		/// <remarks />
		public static Func<IPlatformLock> Factory { get; set; } = DefaultFactory;

		/// <summary>Creates a new platform lock instance using the default implementation.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.Internals.IPlatformLock" /> instance using the default reader-writer lock.</returns>
		/// <remarks />
		public static IPlatformLock DefaultFactory ()
		{
#if !(__IOS__ || __TVOS__ || __MACOS__ || __MACCATALYST__ || __ANDROID__)
			if (PlatformConfiguration.IsWindows)
				return new NonAlertableWin32Lock ();
			else
#endif
				return new ReadWriteLock ();
		}


		/// <summary>Non-Windows platform lock uses ReaderWriteLockSlim</summary>
		class ReadWriteLock : IPlatformLock
		{
			public void EnterReadLock () => _lock.EnterReadLock ();
			public void ExitReadLock () => _lock.ExitReadLock ();
			public void EnterWriteLock () => _lock.EnterWriteLock ();
			public void ExitWriteLock () => _lock.ExitWriteLock ();
			public void EnterUpgradeableReadLock () => _lock.EnterUpgradeableReadLock ();
			public void ExitUpgradeableReadLock () => _lock.ExitUpgradeableReadLock ();

			ReaderWriterLockSlim _lock = new ReaderWriterLockSlim (LockRecursionPolicy.NoRecursion);
		}

#if !(__IOS__ || __TVOS__ || __MACOS__ || __MACCATALYST__ || __ANDROID__)
		/// <summary>Windows platform lock uses Win32 CRITICAL_SECTION</summary>
		partial class NonAlertableWin32Lock : IPlatformLock
		{
			public NonAlertableWin32Lock ()
			{
				_cs = Marshal.AllocHGlobal (Unsafe.SizeOf<CRITICAL_SECTION>());
				if (_cs == IntPtr.Zero)
					throw new OutOfMemoryException ("Failed to allocate memory for critical section");

				InitializeCriticalSectionEx (_cs, 4000, 0);
			}

			~NonAlertableWin32Lock ()
			{
				if (_cs != IntPtr.Zero) {
					DeleteCriticalSection (_cs);
					Marshal.FreeHGlobal (_cs);
					_cs = IntPtr.Zero;
				}
			}

			IntPtr _cs;

			void Enter ()
			{
				if (_cs != IntPtr.Zero) {
					EnterCriticalSection(_cs);
				}
			}

			void Leave ()
			{
				if (_cs != IntPtr.Zero) {
					LeaveCriticalSection(_cs);
				}
			}

			public void EnterReadLock () { Enter (); }
			public void ExitReadLock () { Leave (); }
			public void EnterWriteLock () { Enter (); }
			public void ExitWriteLock () { Leave (); }
			public void EnterUpgradeableReadLock () { Enter (); }
			public void ExitUpgradeableReadLock () { Leave (); }

			[StructLayout (LayoutKind.Sequential)]
			public struct CRITICAL_SECTION
			{
				public IntPtr DebugInfo;
				public int LockCount;
				public int RecursionCount;
				public IntPtr OwningThread;
				public IntPtr LockSemaphore;
				public UIntPtr SpinCount;
			}

#if USE_LIBRARY_IMPORT
			[LibraryImport ("Kernel32.dll", SetLastError = true)]
			[return: MarshalAs (UnmanagedType.Bool)]
			private static partial bool InitializeCriticalSectionEx (IntPtr lpCriticalSection, uint dwSpinCount, uint Flags);
			[LibraryImport ("Kernel32.dll")]
			private static partial void DeleteCriticalSection (IntPtr lpCriticalSection);
			[LibraryImport ("Kernel32.dll")]
			private static partial void EnterCriticalSection (IntPtr lpCriticalSection);
			[LibraryImport ("Kernel32.dll")]
			private static partial void LeaveCriticalSection (IntPtr lpCriticalSection);
#else
			[DllImport ("Kernel32.dll", SetLastError = true)]
			[return: MarshalAs (UnmanagedType.Bool)]
			static extern bool InitializeCriticalSectionEx (IntPtr lpCriticalSection, uint dwSpinCount, uint Flags);
			[DllImport ("Kernel32.dll")]
			static extern void DeleteCriticalSection (IntPtr lpCriticalSection);
			[DllImport ("Kernel32.dll")]
			static extern void EnterCriticalSection (IntPtr lpCriticalSection);
			[DllImport ("Kernel32.dll")]
			static extern void LeaveCriticalSection (IntPtr lpCriticalSection);
#endif
		}
#endif
	}
}
