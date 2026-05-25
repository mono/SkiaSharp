using System;
using SkiaSharp.Tests.Visual;
using Xunit;

namespace SkiaSharp.Tests.Graphite
{
	/// <summary>
	/// Disposal-ordering and -idempotence tests for the Graphite types.
	///
	/// Convention (matches the rest of SkiaSharp): dispose dependents before
	/// parents. Disposal order context → recorder → recording is undefined
	/// behavior in upstream Skia and we don't try to make it safe — the tests
	/// here verify the canonical safe orderings + idempotent Dispose, which
	/// is what callers actually need.
	/// </summary>
	public class GraphiteDisposalTests : BaseTest
	{
		[SkippableFact]
		public void DoubleDispose_OnContext_IsNoOp ()
		{
			Skip.IfNot (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan), "Graphite/Vulkan unavailable.");
			Skip.IfNot (VulkanLoader.Shared.IsAvailable,
				$"Vulkan loader unavailable: {VulkanLoader.Shared.FailureReason}");

			var ctx = MakeContext ();
			Assert.NotNull (ctx);
			ctx.Dispose ();
			ctx.Dispose ();   // must not throw, must not crash
		}

		[SkippableFact]
		public void DoubleDispose_OnVkBackendContext_IsNoOp ()
		{
			Skip.IfNot (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan), "Graphite/Vulkan unavailable.");
			Skip.IfNot (VulkanLoader.Shared.IsAvailable, "Vulkan loader unavailable.");

			var bc = NewVkBackendContext ();
			bc.Dispose ();
			bc.Dispose ();   // must not crash even on a never-handed-off bc
		}

		[SkippableFact]
		public void Snap_OnEmptyRecorder_IsLegalAndDoesNotCrash ()
		{
			// Spec § "Edge Cases": snapping a recording from a recorder with
			// no draws must not error. The result may be null (Skia returns
			// no Recording if there's nothing to play back) — the contract is
			// "no crash", not "non-null".
			Skip.IfNot (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan), "Graphite/Vulkan unavailable.");
			Skip.IfNot (VulkanLoader.Shared.IsAvailable,
				$"Vulkan loader unavailable: {VulkanLoader.Shared.FailureReason}");

			using var ctx = MakeContext ();
			using var recorder = ctx.CreateRecorder ();
			Assert.NotNull (recorder);

			// Snap with no draws: documented to be legal. May return null OR
			// return an empty Recording. Both are fine; the test asserts only
			// that we don't crash.
			using var recording = recorder.Snap ();
			// If non-null, it's an empty recording — inserting it must succeed.
			if (recording != null)
				Assert.Equal (SKGraphiteInsertStatus.Success, ctx.InsertRecording (recording));
		}

		[SkippableFact]
		public void CanonicalDisposalOrder_IsClean ()
		{
			// recording → surface → recorder → context → bc(already-transferred)
			// is the safe order — each child disposed before its parent.
			Skip.IfNot (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan), "Graphite/Vulkan unavailable.");
			Skip.IfNot (VulkanLoader.Shared.IsAvailable,
				$"Vulkan loader unavailable: {VulkanLoader.Shared.FailureReason}");

			var ctx = MakeContext ();
			var recorder = ctx.CreateRecorder ();
			var info = new SKImageInfo (32, 32, SKColorType.Rgba8888, SKAlphaType.Premul);
			var surface = SKSurface.Create (recorder, info);
			Assert.NotNull (surface);
			surface.Canvas.Clear (SKColors.Magenta);
			var recording = recorder.Snap ();
			Assert.NotNull (recording);

			// Dispose in canonical order. Each call must succeed without crash.
			recording.Dispose ();
			surface.Dispose ();
			recorder.Dispose ();
			ctx.Dispose ();

			// Idempotent — second pass through the same disposes is a no-op.
			recording.Dispose ();
			surface.Dispose ();
			recorder.Dispose ();
			ctx.Dispose ();
		}

		// -- helpers --

		private static SKGraphiteContext MakeContext ()
		{
			using var bc = NewVkBackendContext ();
			return SKGraphiteContext.CreateVulkan (bc);
		}

		private static SKGraphiteVkBackendContext NewVkBackendContext ()
		{
			var vk = VulkanLoader.Shared;
			return new SKGraphiteVkBackendContext {
				VkInstance         = vk.Instance,
				VkPhysicalDevice   = vk.PhysicalDevice,
				VkDevice           = vk.Device,
				VkQueue            = vk.Queue,
				GraphicsQueueIndex = vk.QueueFamilyIndex,
				MaxApiVersion      = VulkanLoader.VK_API_VERSION_1_3,
				ProtectedContext   = false,
				GetProcedureAddress = (name, instance, device) => vk.GetProc (name, instance, device),
			};
		}
	}
}
