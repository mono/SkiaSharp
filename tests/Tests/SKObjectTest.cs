using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKObjectTest : SKTest
	{
		[SkippableFact]
		public void ConstructorsAreCached()
		{
			var handle = (IntPtr)123;

			SKObject.GetObject<LifecycleObject>(handle);

			Assert.True(SKObject.constructors.ContainsKey(typeof(LifecycleObject)));
		}

		[SkippableFact]
		public void CanInstantiateAbstractClassesWithImplementation()
		{
			var handle = (IntPtr)444;

			Assert.Throws<MemberAccessException>(() => SKObject.GetObject<AbstractObject>(handle));

			var obj = SKObject.GetObject<AbstractObject, ConcreteObject>(handle);

			Assert.NotNull(obj);
			Assert.IsType<ConcreteObject>(obj);
		}

		private abstract class AbstractObject : SKObject
		{
			public AbstractObject(IntPtr handle, bool owns)
				: base(handle, owns)
			{
			}
		}

		private class ConcreteObject : AbstractObject
		{
			public ConcreteObject(IntPtr handle, bool owns)
				: base(handle, owns)
			{
			}
		}

		[SkippableFact]
		public void SameHandleReturnsSameReferenceAndReleasesObject()
		{
			VerifyImmediateFinalizers();

			var handle = (IntPtr)234;
			TestConstruction(handle);

			CollectGarbage();

			Assert.False(SKObject.GetInstance<LifecycleObject>(handle, out var inst));
			Assert.Null(inst);

			void TestConstruction(IntPtr h)
			{
				LifecycleObject i = null;

				Assert.False(SKObject.GetInstance(h, out i));
				Assert.Null(i);

				var first = SKObject.GetObject<LifecycleObject>(h);

				Assert.True(SKObject.GetInstance(h, out i));
				Assert.NotNull(i);

				Assert.Same(first, i);

				var second = SKObject.GetObject<LifecycleObject>(h);

				Assert.Same(first, second);
			}
		}

		[SkippableFact]
		public void ObjectsWithTheSameHandleButDoNotOwnTheirHandlesAreCreatedAndCollectedCorrectly()
		{
			VerifyImmediateFinalizers();

			var handle = (IntPtr)566;

			Construct();

			CollectGarbage();

			Assert.False(SKObject.GetInstance<LifecycleObject>(handle, out _));

			void Construct()
			{
				var inst1 = new LifecycleObject(handle, false);
				var inst2 = new LifecycleObject(handle, false);

				Assert.NotSame(inst1, inst2);
			}
		}

		[SkippableFact]
		public void ObjectsWithTheSameHandleButDoNotOwnTheirHandlesAreCreatedAndDisposedCorrectly()
		{
			var handle = (IntPtr)567;

			var inst = Construct();

			CollectGarbage();

			Assert.True(SKObject.GetInstance<LifecycleObject>(handle, out var obj));
			Assert.Equal(2, obj.Value);
			Assert.Same(inst, obj);

			LifecycleObject Construct()
			{
				var inst1 = new LifecycleObject(handle, false) { Value = 1 };
				var inst2 = new LifecycleObject(handle, false) { Value = 2 };

				Assert.NotSame(inst1, inst2);

				return inst2;
			}
		}

		[SkippableFact]
		public void ObjectsWithTheSameHandleAndOwnTheirHandlesThrowInDebugBuildsButNotRelease()
		{
			var handle = (IntPtr)568;

			var inst1 = new LifecycleObject(handle, true) { Value = 1 };

#if THROW_OBJECT_EXCEPTIONS
			var ex = Assert.Throws<InvalidOperationException>(() => new LifecycleObject(handle, true) { Value = 2 });
			Assert.Contains("H: " + handle.ToString("x") + " ", ex.Message);
#else
			var inst2 = new LifecycleObject(handle, true) { Value = 2 };
			Assert.True(inst1.DestroyedNative);

			inst1.Dispose();
			inst2.Dispose();
#endif
		}

		[SkippableFact]
		public void DisposeInvalidatesObject()
		{
			var handle = (IntPtr)345;

			var obj = SKObject.GetObject<LifecycleObject>(handle);

			Assert.Equal(handle, obj.Handle);
			Assert.False(obj.DestroyedNative);

			obj.Dispose();

			Assert.Equal(IntPtr.Zero, obj.Handle);
			Assert.True(obj.DestroyedNative);
		}

		[SkippableFact]
		public void DisposeDoesNotInvalidateObjectIfItIsNotOwned()
		{
			var handle = (IntPtr)345;

			var obj = SKObject.GetObject<LifecycleObject>(handle, false);

			Assert.False(obj.DestroyedNative);

			obj.Dispose();

			Assert.False(obj.DestroyedNative);
		}

		[SkippableFact]
		public void ExceptionsThrownInTheConstructorFailGracefully()
		{
			BrokenObject broken = null;
			try
			{
				broken = new BrokenObject();
			}
			catch (Exception)
			{
			}
			finally
			{
				broken?.Dispose();
				broken = null;
			}

			// trigger the finalizer
			CollectGarbage();
		}

		private class LifecycleObject : SKObject
		{
			public bool DestroyedNative = false;
			public bool DestroyedManaged = false;

			[Preserve]
			public LifecycleObject(IntPtr handle, bool owns)
				: base(handle, owns)
			{
			}

			public object Value { get; set; }

			protected override void DisposeNative()
			{
				DestroyedNative = true;
			}

			protected override void DisposeManaged()
			{
				DestroyedManaged = true;
			}
		}

		private class BrokenObject : SKObject
		{
			public BrokenObject()
				: base(broken_native_method(), true)
			{
			}

			private static IntPtr broken_native_method()
			{
				throw new Exception("BREAK!");
			}
		}

		[SkippableTheory]
		[InlineData(1)]
		[InlineData(1000)]
		public async Task EnsureMultithreadingDoesNotThrow(int iterations)
		{
			var imagePath = Path.Combine(PathToImages, "baboon.jpg");

			var tasks = new Task[iterations];

			for (var i = 0; i < iterations; i++)
			{
				var task = new Task(() =>
				{
					using (var stream = File.OpenRead(imagePath))
					using (var data = SKData.Create(stream))
					using (var codec = SKCodec.Create(data))
					{
						var info = new SKImageInfo(codec.Info.Width, codec.Info.Height);
						using (var image = SKBitmap.Decode(codec, info))
						{
							var img = new byte[image.Height, image.Width];
						}
					}
				});

				tasks[i] = task;
				task.Start();
			}

			await Task.WhenAll(tasks);
		}

		[SkippableFact]
		public void EnsureConcurrencyResultsInCorrectDeregistration()
		{
			var handle = (IntPtr)446;

			var obj = new ImmediateRecreationObject(handle, true);
			Assert.Null(obj.NewInstance);
			Assert.Equal(obj, SKObject.instances[handle]?.Target);

			obj.Dispose();
			Assert.True(SKObject.GetInstance<ImmediateRecreationObject>(handle, out _));

			var newObj = obj.NewInstance;

			Assert.NotEqual(obj, SKObject.instances[handle]?.Target);
			Assert.Equal(newObj, SKObject.instances[handle]?.Target);

			newObj.Dispose();
			Assert.False(SKObject.GetInstance<ImmediateRecreationObject>(handle, out _));
		}

		private class ImmediateRecreationObject : SKObject
		{
			public ImmediateRecreationObject(IntPtr handle, bool shouldRecreate)
				: base(handle, true)
			{
				ShouldRecreate = shouldRecreate;
			}

			public bool ShouldRecreate { get; }

			public ImmediateRecreationObject NewInstance { get; private set; }

			protected override void DisposeNative()
			{
				base.DisposeNative();

				if (ShouldRecreate)
					NewInstance = new ImmediateRecreationObject(Handle, false);
			}
		}
	}
}
