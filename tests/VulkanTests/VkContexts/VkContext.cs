using System;
using System.Linq;
using SharpVk;
using SharpVk.Glfw;
using SharpVk.Interop;
using SharpVk.Khronos;
using Device = SharpVk.Device;
using Instance = SharpVk.Instance;
using PhysicalDevice = SharpVk.PhysicalDevice;
using Queue = SharpVk.Queue;

namespace SkiaSharp.Tests
{
	public abstract class VkContext : IDisposable
	{
		public abstract Instance Instance { get; }
		public abstract PhysicalDevice PhysicalDevice { get; }

		public abstract Surface Surface { get; }
		public abstract Device Device { get; }

		public abstract Queue GraphicsQueue { get; }
		public abstract Queue PresentQueue { get; }

		public abstract uint GraphicsFamily { get; }
		public abstract uint PresentFamily { get; }

		public abstract GRVkGetProcDelegate GetProc { get; }

		public virtual void Dispose() => Instance?.Dispose();
	}
}
