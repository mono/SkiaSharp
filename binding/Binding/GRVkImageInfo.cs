using System.Runtime.InteropServices;

namespace SkiaSharp
{
	using VkImage = VkNonDispatchableHandle;
	using VkImageTiling = System.Int32;
	using VkImageLayout = System.Int32;
	using VkFormat = System.Int32;

	[StructLayout (LayoutKind.Sequential)]
	public struct GRVkImageInfo
	{
		public VkImage Image;
		public GRVkAlloc Alloc;
		public VkImageTiling ImageTiling;
		public VkImageLayout ImageLayout;
		public VkFormat Format;
		public uint LevelCount;
	}
}
