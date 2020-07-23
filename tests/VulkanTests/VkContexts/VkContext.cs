using System;
using SharpVk.Khronos;

using Device = SharpVk.Device;
using Instance = SharpVk.Instance;
using PhysicalDevice = SharpVk.PhysicalDevice;
using Queue = SharpVk.Queue;

namespace SkiaSharp.Tests
{
	public class VkContext : IDisposable
	{
		public virtual Instance Instance { get; protected set; }

		public virtual PhysicalDevice PhysicalDevice { get; protected set; }

		public virtual Surface Surface { get; protected set; }

		public virtual Device Device { get; protected set; }

		public virtual Queue GraphicsQueue { get; protected set; }

		public virtual Queue PresentQueue { get; protected set; }

		public virtual uint GraphicsFamily { get; protected set; }

		public virtual uint PresentFamily { get; protected set; }

		public virtual GRVkGetProcedureAddressDelegate GetProc { get; protected set; }

		public virtual GRSharpVkGetProcedureAddressDelegate SharpVkGetProc { get; protected set; }

		public virtual void Dispose() =>
			Instance?.Dispose();
	}
}
