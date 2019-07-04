using System;
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

			[Preserve]
			public LifecycleObject(IntPtr handle, bool owns)
				: base(handle, owns)
			{
			}

			protected override void DisposeNative()
			{
				DestroyedNative = true;
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
	}
}
