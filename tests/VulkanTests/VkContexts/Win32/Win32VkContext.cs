using System;
using System.Linq;
using SharpVk;
using SharpVk.Interop;
using SharpVk.Khronos;
using Device = SharpVk.Device;
using DeviceQueueCreateInfo = SharpVk.DeviceQueueCreateInfo;
using Instance = SharpVk.Instance;
using PhysicalDevice = SharpVk.PhysicalDevice;
using Queue = SharpVk.Queue;

namespace SkiaSharp.Tests
{
	public sealed class Win32VkContext : VkContext
	{
		public override Instance Instance { get; }
		public override PhysicalDevice PhysicalDevice { get; }

		public override Surface Surface { get; }
		public override Device Device { get; }

		public override Queue GraphicsQueue { get; }
		public override Queue PresentQueue { get; }

		public override uint GraphicsFamily { get; }
		public override uint PresentFamily { get; }

		public override GRVkGetProcDelegate GetProc { get; }

		private ushort wcId;
		private IntPtr hWnd;

		private const string VulkanTestWindow = "VulkanTestWindow";

		public Win32VkContext()
		{
			Instance = Instance.Create(
				null,
				new[] {"VK_KHR_surface", "VK_KHR_win32_surface"});

			PhysicalDevice = Instance.EnumeratePhysicalDevices().First();

			CreateWindow();

			Surface = Instance.CreateWin32Surface(Kernel32.CurrentModuleHandle, hWnd);

			(GraphicsFamily, PresentFamily) = FindQueueFamilies();

			Device = PhysicalDevice.CreateDevice(
				new[]
				{
					new DeviceQueueCreateInfo {QueueFamilyIndex = GraphicsFamily, QueuePriorities = new[] {1f}},
					new DeviceQueueCreateInfo {QueueFamilyIndex = PresentFamily, QueuePriorities = new[] {1f}}
				},
				null,
				null);

			GraphicsQueue = Device.GetQueue(GraphicsFamily, 0);
			PresentQueue = Device.GetQueue(PresentFamily, 0);

			GetProc = GetProcImpl;
		}

		private IntPtr GetProcImpl(object context, string name, IntPtr instance, IntPtr device)
		{
			if (device != IntPtr.Zero)
			{
				return Device.GetProcedureAddress(name);
			}

			return Instance.GetProcedureAddress(name);
		}

		private (uint, uint) FindQueueFamilies()
		{
			QueueFamilyProperties[] queueFamilyProperties = PhysicalDevice.GetQueueFamilyProperties();

			var graphicsFamily = queueFamilyProperties
				.Select((properties, index) => new {properties, index})
				.SkipWhile(pair => !pair.properties.QueueFlags.HasFlag(QueueFlags.Graphics))
				.FirstOrDefault();

			if (graphicsFamily == null)
			{
				throw new Exception("Unable to find graphics queue");
			}

			uint? presentFamily = default;

			for (uint i = 0; i < queueFamilyProperties.Length; ++i)
			{
				if (PhysicalDevice.GetSurfaceSupport(i, Surface))
				{
					presentFamily = i;
				}
			}

			if (!presentFamily.HasValue)
			{
				throw new Exception("Unable to find present queue");
			}

			return ((uint)graphicsFamily.index, presentFamily.Value);
		}

		public override void Dispose()
		{
			DestroyWindow();
			base.Dispose();
		}

		private void CreateWindow()
		{
			var wc = new WNDCLASS
			{
				cbClsExtra = 0,
				cbWndExtra = 0,
				hbrBackground = IntPtr.Zero,
				hCursor = User32.LoadCursor(IntPtr.Zero, (int)User32.IDC_ARROW),
				hIcon = User32.LoadIcon(IntPtr.Zero, (IntPtr)User32.IDI_APPLICATION),
				hInstance = Kernel32.CurrentModuleHandle,
				lpfnWndProc = (WNDPROC)User32.DefWindowProc,
				lpszClassName = VulkanTestWindow,
				lpszMenuName = null,
				style = User32.CS_HREDRAW | User32.CS_VREDRAW | User32.CS_OWNDC
			};

			wcId = User32.RegisterClass(ref wc);

			if (wcId == 0)
			{
				throw new Exception("Could not register window class.");
			}

			hWnd = User32.CreateWindow(
				VulkanTestWindow,
				VulkanTestWindow,
				WindowStyles.WS_OVERLAPPEDWINDOW,
				0,
				0,
				1,
				1,
				IntPtr.Zero,
				IntPtr.Zero,
				Kernel32.CurrentModuleHandle,
				IntPtr.Zero);

			if (hWnd == IntPtr.Zero)
			{
				throw new Exception("Could not create window.");
			}
		}

		private void DestroyWindow()
		{
			if (hWnd != IntPtr.Zero)
			{
				User32.DestroyWindow(hWnd);
			}

			if (wcId != 0)
			{
				User32.UnregisterClass(VulkanTestWindow, Kernel32.CurrentModuleHandle);
			}
		}
	}
}
