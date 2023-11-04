using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKObjectDelayedTest : SKTest
	{
		[SkippableFact]
		public async Task DelayedConstructionDoesNotCreateInvalidState()
		{
			var handle = GetNextPtr();

			DelayedConstructionObject objFast = null;
			DelayedConstructionObject objSlow = null;

			var order = new ConcurrentQueue<int>();

			var objFastStart = new AutoResetEvent(false);
			var objFastDelay = new AutoResetEvent(false);

			var fast = Task.Run(() =>
			{
				order.Enqueue(1);

				DelayedConstructionObject.ConstructionStartedEvent = objFastStart;
				DelayedConstructionObject.ConstructionDelayEvent = objFastDelay;
				objFast = DelayedConstructionObject.GetObject(handle);
				order.Enqueue(4);
			});

			var slow = Task.Run(() =>
			{
				order.Enqueue(1);

				objFastStart.WaitOne();
				order.Enqueue(2);

				var timer = new Timer(state => objFastDelay.Set(), null, 1000, Timeout.Infinite);
				order.Enqueue(3);

				objSlow = DelayedConstructionObject.GetObject(handle);
				order.Enqueue(5);

				timer.Dispose(objFastDelay);
			});

			await Task.WhenAll(new[] { fast, slow });

			// make sure it was the right order
			Assert.Equal(new[] { 1, 1, 2, 3, 4, 5 }, order);

			// make sure both were "created" and they are the same object
			Assert.NotNull(objFast);
			Assert.NotNull(objSlow);
			Assert.Same(objFast, objSlow);
		}

		[SkippableFact]
		public async Task DelayedDestructionDoesNotCreateInvalidState()
		{
			var handle = GetNextPtr();

			DelayedDestructionObject objFast = null;
			DelayedDestructionObject objSlow = null;

			using var secondThreadStarter = new AutoResetEvent(false);

			var order = new ConcurrentQueue<int>();

			var fast = Task.Run(() =>
			{
				order.Enqueue(1);

				objFast = DelayedDestructionObject.GetObject(handle);
				objFast.DisposeDelayEvent = new AutoResetEvent(false);

				Assert.True(SKObject.GetInstance<DelayedDestructionObject>(handle, out var beforeDispose));
				Assert.Same(objFast, beforeDispose);

				order.Enqueue(2);
				// start thread 2
				secondThreadStarter.Set();

				objFast.Dispose();
				order.Enqueue(7);
			});

			var slow = Task.Run(() =>
			{
				// wait for thread 1
				secondThreadStarter.WaitOne();

				order.Enqueue(3);
				// wait for the disposal to start
				objFast.DisposeStartedEvent.WaitOne();
				order.Enqueue(4);

				Assert.False(SKObject.GetInstance<DelayedDestructionObject>(handle, out var beforeCreate));
				Assert.Null(beforeCreate);

				var directRef = HandleDictionary.instances[handle];
				Assert.Same(objFast, directRef.Target);

				order.Enqueue(5);
				objSlow = DelayedDestructionObject.GetObject(handle);
				order.Enqueue(6);

				// finish the disposal
				objFast.DisposeDelayEvent.Set();
			});

			await Task.WhenAll(new[] { fast, slow });

			// make sure it was the right order
			Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7 }, order);

			// make sure both were "created" and they are NOT the same object
			Assert.NotNull(objFast);
			Assert.NotNull(objSlow);
			Assert.NotSame(objFast, objSlow);
			Assert.True(SKObject.GetInstance<DelayedDestructionObject>(handle, out var final));
			Assert.Same(objSlow, final);
		}

		private class DelayedConstructionObject : SKObject
		{
			public static AutoResetEvent ConstructionStartedEvent;
			public static AutoResetEvent ConstructionDelayEvent;

			public DelayedConstructionObject(IntPtr handle, bool owns)
				: base(GetHandle(handle), owns)
			{
			}

			private static IntPtr GetHandle(IntPtr handle)
			{
				var started = Interlocked.Exchange(ref ConstructionStartedEvent, null);
				var delay = Interlocked.Exchange(ref ConstructionDelayEvent, null);

				started?.Set();
				delay?.WaitOne();

				return handle;
			}

			public static DelayedConstructionObject GetObject(IntPtr handle, bool owns = true) =>
				GetOrAddObject(handle, owns, (h, o) => new DelayedConstructionObject(h, o));
		}

		private class DelayedDestructionObject : SKObject
		{
			public AutoResetEvent DisposeStartedEvent = new AutoResetEvent(false);
			public AutoResetEvent DisposeDelayEvent;

			public DelayedDestructionObject(IntPtr handle, bool owns)
				: base(handle, owns)
			{
			}

			protected override void DisposeManaged()
			{
				DisposeStartedEvent.Set();
				DisposeDelayEvent?.WaitOne();

				base.DisposeManaged();
			}

			public static DelayedDestructionObject GetObject(IntPtr handle, bool owns = true) =>
				GetOrAddObject(handle, owns, (h, o) => new DelayedDestructionObject(h, o));
		}
	}
}
