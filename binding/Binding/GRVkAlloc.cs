using System.Runtime.InteropServices;

namespace SkiaSharp
{
	using VkDeviceMemory = VkNonDispatchableHandle;
	using VkDeviceSize = System.UInt64;

	[StructLayout (LayoutKind.Sequential)]
	public struct GRVkAlloc
	{
		public VkDeviceMemory Memory;
		public VkDeviceSize Offset;
		public VkDeviceSize Size;
		public uint Flags;
	}
}
