using System;
using SharpVk.Khronos;

using Device = SharpVk.Device;
using Instance = SharpVk.Instance;
using PhysicalDevice = SharpVk.PhysicalDevice;
using PhysicalDeviceFeatures = SharpVk.PhysicalDeviceFeatures;
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

		/// <summary>
		/// The API version the instance was created with. Also handed to
		/// <c>MaxAPIVersion</c>: left at zero, Skia takes the loader's version as its
		/// ceiling, which over-declares what this instance can actually serve.
		/// </summary>
		public virtual uint ApiVersion { get; protected set; }

		/// <summary>The features enabled on <see cref="Device"/>, for Skia's caps.</summary>
		public virtual PhysicalDeviceFeatures Features { get; protected set; }

		/// <summary>Extensions actually enabled, not merely available.</summary>
		public virtual string[] InstanceExtensions { get; protected set; }

		/// <summary>Extensions actually enabled, not merely available.</summary>
		public virtual string[] DeviceExtensions { get; protected set; }

		public virtual GRVkGetProcedureAddressDelegate GetProc { get; protected set; }

		public virtual GRSharpVkGetProcedureAddressDelegate SharpVkGetProc { get; protected set; }

		public virtual void Dispose() =>
			Instance?.Dispose();
	}
}
