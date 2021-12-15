using System;
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
	/// <summary>
	/// Abstracts a platform dependant lock implementation
	/// </summary>
	public interface IPlatformLock
	{
		void EnterReadLock ();
		void ExitReadLock ();
		void EnterWriteLock ();
		void ExitWriteLock ();
		void EnterUpgradeableReadLock ();
		void ExitUpgradeableReadLock ();
	}

	/// <summary>
	/// Helper class to create a IPlatformLock instance, by default according to the current platform
	/// but also client toolkits can plugin their own implementation.
	/// </summary>
	public static class PlatformLock
	{
		/// <summary>
		/// Creates a platform lock
		/// </summary>
		/// <returns></returns>
		public static IPlatformLock Create ()
		{
			// Just call the factory
			return Factory ();
		}

		/// <summary>
		/// The factory for creating platform locks
		/// </summary>
		/// <remarks>
		/// Use this to plugin your own lock implementation.  Must be set
		/// before using other SkiaSharp functionality that causes the lock
		/// to be created (currently only used by SkiaSharps internal
		/// HandleDictionary).
		/// </remarks>
		public static Func<IPlatformLock> Factory { get; set; } = DefaultFactory;

		/// <summary>
		/// Default platform lock factory
		/// </summary>
		/// <returns>A reference to a new platform lock implementation</returns>
		public static IPlatformLock DefaultFactory ()
		{
			if (PlatformConfiguration.IsWindows)
				return new NonAlertableWin32Lock ();
			else
				return new ReadWriteLock ();
		}


		/// <summary>
		/// Non-Windows platform lock uses ReaderWriteLockSlim
		/// </summary>
		class ReadWriteLock : IPlatformLock
		{
			public void EnterReadLock () => _lock.EnterReadLock ();
			public void ExitReadLock () => _lock.ExitReadLock ();
			public void EnterWriteLock () => _lock.EnterWriteLock ();
			public void ExitWriteLock () => _lock.ExitWriteLock ();
			public void EnterUpgradeableReadLock () => _lock.EnterUpgradeableReadLock ();
			public void ExitUpgradeableReadLock () => _lock.ExitUpgradeableReadLock ();

			ReaderWriterLockSlim _lock = new ReaderWriterLockSlim ();
		}

		/// <summary>
		/// Windows platform lock uses Win32 CRITICAL_SECTION
		/// </summary>
		class NonAlertableWin32Lock : IPlatformLock
		{
			public NonAlertableWin32Lock ()
			{
				_cs = Marshal.AllocHGlobal (Marshal.SizeOf<CRITICAL_SECTION> ());
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
				EnterCriticalSection (_cs);
			}

			void Leave ()
			{
				LeaveCriticalSection (_cs);
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

			[DllImport ("Kernel32.dll", SetLastError = true)]
			[return: MarshalAs (UnmanagedType.Bool)]
			static extern bool InitializeCriticalSectionEx (IntPtr lpCriticalSection, uint dwSpinCount, uint Flags);
			[DllImport ("Kernel32.dll")]
			static extern void DeleteCriticalSection (IntPtr lpCriticalSection);
			[DllImport ("Kernel32.dll")]
			static extern void EnterCriticalSection (IntPtr lpCriticalSection);
			[DllImport ("Kernel32.dll")]
			static extern void LeaveCriticalSection (IntPtr lpCriticalSection);
		}
	}

}
