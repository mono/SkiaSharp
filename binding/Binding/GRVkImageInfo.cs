using System.Runtime.InteropServices;
using VkImage = SkiaSharp.VkNonDispatchableHandle;
using VkImageTiling = System.Int32;
using VkImageLayout = System.Int32;
using VkFormat = System.Int32;

namespace SkiaSharp
{
	[StructLayout (LayoutKind.Sequential)]
	public struct GRVkImageInfo
	{
		private VkImage image;
		private GRVkAlloc alloc;
		private VkImageTiling imageTiling;
		private VkImageLayout imageLayout;
		private VkFormat format;
		private uint levelCount;

		public VkNonDispatchableHandle Image {
			get => image;
			set => image = value;
		}

		public GRVkAlloc Alloc {
			get => alloc;
			set => alloc = value;
		}

		public int ImageTiling {
			get => imageTiling;
			set => imageTiling = value;
		}

		public int ImageLayout {
			get => imageLayout;
			set => imageLayout = value;
		}

		public int Format {
			get => format;
			set => format = value;
		}

		public uint LevelCount {
			get => levelCount;
			set => levelCount = value;
		}
	}
}
