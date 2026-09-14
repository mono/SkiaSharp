# API diff: SkiaSharp.Vulkan.Silk.NET.dll

## SkiaSharp.Vulkan.Silk.NET.dll

> Assembly Version Changed: 4.152.0.0 vs 0.0.0.0

### New Namespace SkiaSharp

#### New Type: SkiaSharp.GRSilkNetBackendContext

```csharp
public class GRSilkNetBackendContext : SkiaSharp.GRVkBackendContext, System.IDisposable {
	// constructors
	public GRSilkNetBackendContext ();
	// properties
	public GRSilkNetGetProcedureAddressDelegate GetProcedureAddress { get; set; }
	public Silk.NET.Vulkan.Device VkDevice { get; set; }
	public Silk.NET.Vulkan.Instance VkInstance { get; set; }
	public Silk.NET.Vulkan.PhysicalDevice VkPhysicalDevice { get; set; }
	public Silk.NET.Vulkan.PhysicalDeviceFeatures? VkPhysicalDeviceFeatures { get; set; }
	public Silk.NET.Vulkan.Queue VkQueue { get; set; }
	// methods
	protected override void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.GRSilkNetGetProcedureAddressDelegate

```csharp
public sealed delegate GRSilkNetGetProcedureAddressDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public GRSilkNetGetProcedureAddressDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (string name, Silk.NET.Vulkan.Instance instance, Silk.NET.Vulkan.Device device, System.AsyncCallback callback, object object);
	public virtual IntPtr EndInvoke (System.IAsyncResult result);
	public virtual IntPtr Invoke (string name, Silk.NET.Vulkan.Instance instance, Silk.NET.Vulkan.Device device);
}
```

#### New Type: SkiaSharp.GRVkExtensionsSilkNetExtensions

```csharp
public static class GRVkExtensionsSilkNetExtensions {
	// methods
	public static void Initialize (this GRVkExtensions extensions, GRSilkNetGetProcedureAddressDelegate getProc, Silk.NET.Vulkan.Instance instance, Silk.NET.Vulkan.PhysicalDevice physicalDevice);
	public static void Initialize (this GRVkExtensions extensions, GRSilkNetGetProcedureAddressDelegate getProc, Silk.NET.Vulkan.Instance instance, Silk.NET.Vulkan.PhysicalDevice physicalDevice, string[] instanceExtensions, string[] deviceExtensions);
}
```
