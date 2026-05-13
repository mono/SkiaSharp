using System;
using System.Runtime.InteropServices;
using SkiaSharp.Tests.Visual;
using Xunit;

namespace SkiaSharp.Tests.Graphite
{
	/// <summary>
	/// Wrap an externally-allocated VkImage as a
	/// <see cref="SKGraphiteBackendTexture"/>, render into it via Graphite,
	/// and verify that disposing the wrapper does NOT free the underlying
	/// VkImage (caller retains ownership).
	/// </summary>
	public unsafe class GraphiteBackendTextureTests : BaseTest
	{
		private const int VK_FORMAT_R8G8B8A8_UNORM         = 37;
		private const int VK_IMAGE_TILING_OPTIMAL          = 0;
		private const int VK_IMAGE_LAYOUT_UNDEFINED        = 0;
		private const int VK_SHARING_MODE_EXCLUSIVE        = 0;
		private const uint VK_IMAGE_USAGE_TRANSFER_SRC_BIT  = 0x00000001;
		private const uint VK_IMAGE_USAGE_TRANSFER_DST_BIT  = 0x00000002;
		private const uint VK_IMAGE_USAGE_SAMPLED_BIT       = 0x00000004;
		private const uint VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT = 0x00000010;
		private const uint VK_IMAGE_USAGE_INPUT_ATTACHMENT_BIT = 0x00000080;
		private const uint VK_IMAGE_ASPECT_COLOR_BIT        = 0x00000001;
		private const uint VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT = 0x00000001;

		// Render-target usage matches Skia's VulkanCaps default (see
		// src/gpu/graphite/vk/VulkanCaps.cpp). The wrapped VkImage must allow
		// this combination — including INPUT_ATTACHMENT, which Skia uses for
		// LoadOp::Load and similar render-pass plumbing.
		private const uint RENDER_TARGET_USAGE =
			VK_IMAGE_USAGE_TRANSFER_SRC_BIT |
			VK_IMAGE_USAGE_TRANSFER_DST_BIT |
			VK_IMAGE_USAGE_SAMPLED_BIT |
			VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT |
			VK_IMAGE_USAGE_INPUT_ATTACHMENT_BIT;

		[SkippableFact]
		public void Vulkan_WrapBackendTexture_RoundTrip ()
		{
			Skip.IfNot (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan), "Graphite/Vulkan unavailable.");
			Skip.IfNot (VulkanLoader.Shared.IsAvailable, $"Vulkan loader unavailable: {VulkanLoader.Shared.FailureReason}");

			const int W = 64, H = 64;

			// Allocate a VkImage outside Skia. The wrapper is supposed to leave
			// this VkImage's lifetime entirely to the caller.
			using var vkImg = new VulkanImage (W, H);

			using var ctx = MakeContext ();
			Assert.NotNull (ctx);
			using var rec = ctx.CreateRecorder ();
			Assert.NotNull (rec);

			var info = new SKGraphiteVkTextureInfo {
				SampleCount     = 1,
				Mipmapped       = false,
				Flags           = 0,
				Format          = VK_FORMAT_R8G8B8A8_UNORM,
				ImageTiling     = VK_IMAGE_TILING_OPTIMAL,
				ImageUsageFlags = RENDER_TARGET_USAGE,
				SharingMode     = VK_SHARING_MODE_EXCLUSIVE,
				AspectMask      = VK_IMAGE_ASPECT_COLOR_BIT,
			};

			using var bt = SKGraphiteBackendTexture.CreateVulkan (
				W, H, info,
				VK_IMAGE_LAYOUT_UNDEFINED,
				VulkanLoader.Shared.QueueFamilyIndex,
				vkImg.Image);
			Assert.NotNull (bt);
			Assert.True (bt.IsValid);
			Assert.Equal (SKGraphiteBackend.Vulkan, bt.Backend);
			Assert.Equal (new SKSizeI (W, H), bt.Dimensions);

			byte[] pixels;
			using (var wrapped = SKSurface.Create (rec, bt, SKColorType.Rgba8888)) {
				Assert.NotNull (wrapped);
				wrapped.Canvas.Clear (SKColors.Yellow);

				using (var recording = rec.Snap ()) {
					Assert.NotNull (recording);
					Assert.Equal (SKGraphiteInsertStatus.Success, ctx.InsertRecording (recording));
				}

				var rgba = new SKImageInfo (W, H, SKColorType.Rgba8888, SKAlphaType.Premul);
				pixels = new byte[rgba.BytesSize];
				fixed (byte* p = pixels) {
					Assert.True (ctx.ReadPixels (wrapped, rgba, (IntPtr)p, rgba.RowBytes, 0, 0));
				}
			}

			// Pixel(32,32) should be yellow (R=255, G=255, B=0, A=255).
			int idx = (32 * W + 32) * 4;
			Assert.True (pixels[idx + 0] > 200, $"R={pixels[idx + 0]} (expected >200)");
			Assert.True (pixels[idx + 1] > 200, $"G={pixels[idx + 1]} (expected >200)");
			Assert.True (pixels[idx + 2] < 50,  $"B={pixels[idx + 2]} (expected <50)");

			// `bt` and `wrapped` are disposed at end-of-using, BEFORE `vkImg.Dispose`
			// runs. If the wrapper had accidentally taken ownership, the subsequent
			// vkDestroyImage in `vkImg.Dispose` would either validation-error or
			// crash. Test reaching the end clean = caller-owned semantics held.
		}

		[SkippableFact]
		public void Vulkan_RecorderAllocatedBackendTexture_RoundTrip ()
		{
			// Skia-allocated path: the Recorder allocates the
			// underlying VkImage; we don't manage Vulkan ourselves. The wrapper's
			// Dispose hands the texture back to Skia for release.
			Skip.IfNot (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan), "Graphite/Vulkan unavailable.");
			Skip.IfNot (VulkanLoader.Shared.IsAvailable, $"Vulkan loader unavailable: {VulkanLoader.Shared.FailureReason}");

			const int W = 64, H = 64;

			using var ctx = MakeContext ();
			Assert.NotNull (ctx);
			using var rec = ctx.CreateRecorder ();
			Assert.NotNull (rec);

			var vkInfo = new SKGraphiteVkTextureInfo {
				SampleCount     = 1,
				Mipmapped       = false,
				Flags           = 0,
				Format          = VK_FORMAT_R8G8B8A8_UNORM,
				ImageTiling     = VK_IMAGE_TILING_OPTIMAL,
				ImageUsageFlags = RENDER_TARGET_USAGE,
				SharingMode     = VK_SHARING_MODE_EXCLUSIVE,
				AspectMask      = VK_IMAGE_ASPECT_COLOR_BIT,
			};
			using var info = SKGraphiteTextureInfo.CreateVulkan (vkInfo);
			Assert.NotNull (info);
			Assert.Equal (SKGraphiteBackend.Vulkan, info.Backend);

			using var bt = rec.CreateBackendTexture (W, H, info);
			Assert.NotNull (bt);
			Assert.True (bt.IsValid);
			Assert.Equal (new SKSizeI (W, H), bt.Dimensions);

			byte[] pixels;
			using (var wrapped = SKSurface.Create (rec, bt, SKColorType.Rgba8888)) {
				Assert.NotNull (wrapped);
				wrapped.Canvas.Clear (SKColors.Yellow);

				using (var recording = rec.Snap ()) {
					Assert.NotNull (recording);
					Assert.Equal (SKGraphiteInsertStatus.Success, ctx.InsertRecording (recording));
				}

				var rgba = new SKImageInfo (W, H, SKColorType.Rgba8888, SKAlphaType.Premul);
				pixels = new byte[rgba.BytesSize];
				Assert.True (ctx.ReadPixelsSync (wrapped, rgba, pixels, 0, 0));
			}

			// Pixel(32,32) should be yellow (R=255, G=255, B=0, A=255).
			int idx = (32 * W + 32) * 4;
			Assert.True (pixels[idx + 0] > 200, $"R={pixels[idx + 0]} (expected >200)");
			Assert.True (pixels[idx + 1] > 200, $"G={pixels[idx + 1]} (expected >200)");
			Assert.True (pixels[idx + 2] < 50,  $"B={pixels[idx + 2]} (expected <50)");

			// Hand the texture back to Skia. Disposal of `bt` afterwards just
			// frees the wrapper — the underlying VkImage is already scheduled
			// for release.
			rec.DeleteBackendTexture (bt);
		}

		private static SKGraphiteContext MakeContext ()
		{
			var vk = VulkanLoader.Shared;
			using var bc = new SKGraphiteVkBackendContext {
				VkInstance         = vk.Instance,
				VkPhysicalDevice   = vk.PhysicalDevice,
				VkDevice           = vk.Device,
				VkQueue            = vk.Queue,
				GraphicsQueueIndex = vk.QueueFamilyIndex,
				MaxApiVersion      = VulkanLoader.VK_API_VERSION_1_3,
				ProtectedContext   = false,
				GetProcedureAddress = (name, instance, device) => vk.GetProc (name, instance, device),
			};
			return SKGraphiteContext.CreateVulkan (bc);
		}

		// Helper class wrapping VkImage allocation + binding + cleanup so the
		// test method stays focused on the SkiaSharp surface area.
		private sealed unsafe class VulkanImage : IDisposable
		{
			private const string libvulkan = "vulkan";

			public IntPtr Image { get; private set; }
			public IntPtr Memory { get; private set; }

			public VulkanImage (int width, int height)
			{
				var dev = VulkanLoader.Shared.Device;

				var ici = new VkImageCreateInfo {
					sType = 14,  // VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO
					imageType = 1,  // VK_IMAGE_TYPE_2D
					format = VK_FORMAT_R8G8B8A8_UNORM,
					extent = new VkExtent3D { width = (uint)width, height = (uint)height, depth = 1 },
					mipLevels = 1,
					arrayLayers = 1,
					samples = 1,  // VK_SAMPLE_COUNT_1_BIT
					tiling = VK_IMAGE_TILING_OPTIMAL,
					usage = RENDER_TARGET_USAGE,
					sharingMode = VK_SHARING_MODE_EXCLUSIVE,
					initialLayout = VK_IMAGE_LAYOUT_UNDEFINED,
				};

				IntPtr image;
				int rc = vkCreateImage (dev, &ici, IntPtr.Zero, out image);
				if (rc != 0) throw new InvalidOperationException ($"vkCreateImage rc={rc}");
				Image = image;

				vkGetImageMemoryRequirements (dev, image, out var req);
				uint memTypeIndex = FindMemoryTypeIndex (req.memoryTypeBits, VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);

				var mai = new VkMemoryAllocateInfo {
					sType = 5, // VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO
					allocationSize = req.size,
					memoryTypeIndex = memTypeIndex,
				};
				IntPtr mem;
				rc = vkAllocateMemory (dev, &mai, IntPtr.Zero, out mem);
				if (rc != 0) throw new InvalidOperationException ($"vkAllocateMemory rc={rc}");
				Memory = mem;

				rc = vkBindImageMemory (dev, image, mem, 0);
				if (rc != 0) throw new InvalidOperationException ($"vkBindImageMemory rc={rc}");
			}

			private uint FindMemoryTypeIndex (uint typeBits, uint required)
			{
				vkGetPhysicalDeviceMemoryProperties (VulkanLoader.Shared.PhysicalDevice, out var props);
				for (uint i = 0; i < props.memoryTypeCount; i++) {
					if ((typeBits & (1u << (int)i)) != 0
						&& (props.memoryTypes[(int)i].propertyFlags & required) == required) {
						return i;
					}
				}
				// Fall back to the first usable type if no exact match.
				for (uint i = 0; i < props.memoryTypeCount; i++) {
					if ((typeBits & (1u << (int)i)) != 0)
						return i;
				}
				throw new InvalidOperationException ("No usable Vulkan memory type");
			}

			public void Dispose ()
			{
				var dev = VulkanLoader.Shared.Device;
				if (Image != IntPtr.Zero)  { vkDestroyImage (dev, Image, IntPtr.Zero);  Image = IntPtr.Zero; }
				if (Memory != IntPtr.Zero) { vkFreeMemory   (dev, Memory, IntPtr.Zero); Memory = IntPtr.Zero; }
			}

			[StructLayout (LayoutKind.Sequential)]
			private struct VkImageCreateInfo {
				public uint sType;
				public IntPtr pNext;
				public uint flags;
				public int imageType;
				public int format;
				public VkExtent3D extent;
				public uint mipLevels;
				public uint arrayLayers;
				public int samples;
				public int tiling;
				public uint usage;
				public int sharingMode;
				public uint queueFamilyIndexCount;
				public uint* pQueueFamilyIndices;
				public int initialLayout;
			}

			[StructLayout (LayoutKind.Sequential)]
			private struct VkExtent3D {
				public uint width, height, depth;
			}

			[StructLayout (LayoutKind.Sequential)]
			private struct VkMemoryRequirements {
				public ulong size;
				public ulong alignment;
				public uint  memoryTypeBits;
			}

			[StructLayout (LayoutKind.Sequential)]
			private struct VkMemoryAllocateInfo {
				public uint sType;
				public IntPtr pNext;
				public ulong allocationSize;
				public uint memoryTypeIndex;
			}

			[StructLayout (LayoutKind.Sequential)]
			private struct VkMemoryType {
				public uint propertyFlags;
				public uint heapIndex;
			}

			[StructLayout (LayoutKind.Sequential)]
			private struct VkMemoryHeap {
				public ulong size;
				public uint flags;
			}

			[StructLayout (LayoutKind.Sequential)]
			private struct VkPhysicalDeviceMemoryProperties {
				public uint memoryTypeCount;
				public VkMemoryType_32 memoryTypes;
				public uint memoryHeapCount;
				public VkMemoryHeap_16 memoryHeaps;
			}

			[StructLayout (LayoutKind.Sequential)]
			private struct VkMemoryType_32 {
				// Vulkan spec: VK_MAX_MEMORY_TYPES = 32
				public VkMemoryType m0,  m1,  m2,  m3,  m4,  m5,  m6,  m7;
				public VkMemoryType m8,  m9,  m10, m11, m12, m13, m14, m15;
				public VkMemoryType m16, m17, m18, m19, m20, m21, m22, m23;
				public VkMemoryType m24, m25, m26, m27, m28, m29, m30, m31;

				public VkMemoryType this[int i] {
					get {
						switch (i) {
							case 0:  return m0;  case 1:  return m1;  case 2:  return m2;  case 3:  return m3;
							case 4:  return m4;  case 5:  return m5;  case 6:  return m6;  case 7:  return m7;
							case 8:  return m8;  case 9:  return m9;  case 10: return m10; case 11: return m11;
							case 12: return m12; case 13: return m13; case 14: return m14; case 15: return m15;
							case 16: return m16; case 17: return m17; case 18: return m18; case 19: return m19;
							case 20: return m20; case 21: return m21; case 22: return m22; case 23: return m23;
							case 24: return m24; case 25: return m25; case 26: return m26; case 27: return m27;
							case 28: return m28; case 29: return m29; case 30: return m30; case 31: return m31;
							default: throw new ArgumentOutOfRangeException ();
						}
					}
				}
			}

			[StructLayout (LayoutKind.Sequential)]
			private struct VkMemoryHeap_16 {
				// VK_MAX_MEMORY_HEAPS = 16
				public VkMemoryHeap h0, h1, h2,  h3,  h4,  h5,  h6,  h7;
				public VkMemoryHeap h8, h9, h10, h11, h12, h13, h14, h15;
			}

			[DllImport (libvulkan)] private static extern int vkCreateImage (IntPtr device, VkImageCreateInfo* createInfo, IntPtr allocator, out IntPtr image);
			[DllImport (libvulkan)] private static extern void vkDestroyImage (IntPtr device, IntPtr image, IntPtr allocator);
			[DllImport (libvulkan)] private static extern void vkGetImageMemoryRequirements (IntPtr device, IntPtr image, out VkMemoryRequirements req);
			[DllImport (libvulkan)] private static extern int vkAllocateMemory (IntPtr device, VkMemoryAllocateInfo* info, IntPtr allocator, out IntPtr memory);
			[DllImport (libvulkan)] private static extern void vkFreeMemory (IntPtr device, IntPtr memory, IntPtr allocator);
			[DllImport (libvulkan)] private static extern int vkBindImageMemory (IntPtr device, IntPtr image, IntPtr memory, ulong offset);
			[DllImport (libvulkan)] private static extern void vkGetPhysicalDeviceMemoryProperties (IntPtr physicalDevice, out VkPhysicalDeviceMemoryProperties props);
		}

	}
}
