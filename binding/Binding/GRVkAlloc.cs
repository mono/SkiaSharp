using System.Runtime.InteropServices;
using VkDeviceMemory = SkiaSharp.VkNonDispatchableHandle;
using VkDeviceSize = System.UInt64;

namespace SkiaSharp
{
	[StructLayout (LayoutKind.Sequential)]
	public struct GRVkAlloc
	{
		private VkDeviceMemory memory;
		private VkDeviceSize offset;
		private VkDeviceSize size;
		private uint flags;

		public VkNonDispatchableHandle Memory {
			get => memory;
			set => memory = value;
		}

		public ulong Offset {
			get => offset;
			set => offset = value;
		}

		public ulong Size {
			get => size;
			set => size = value;
		}

		public uint Flags {
			get => flags;
			set => flags = value;
		}
	}
}
