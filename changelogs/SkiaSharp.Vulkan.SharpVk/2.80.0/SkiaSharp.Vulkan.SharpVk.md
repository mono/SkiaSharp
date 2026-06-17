# API diff: SkiaSharp.Vulkan.SharpVk.dll

## SkiaSharp.Vulkan.SharpVk.dll

> Assembly Version Changed: 2.80.0.0 vs 0.0.0.0

### New Namespace SkiaSharp

#### New Type: SkiaSharp.GRSharpVkBackendContext

```csharp
public class GRSharpVkBackendContext : SkiaSharp.GRVkBackendContext {
	// constructors
	public GRSharpVkBackendContext ();
	// properties
	public GRSharpVkGetProcedureAddressDelegate GetProcedureAddress { get; set; }
	public SharpVk.Device VkDevice { get; set; }
	public SharpVk.Instance VkInstance { get; set; }
	public SharpVk.PhysicalDevice VkPhysicalDevice { get; set; }
	public SharpVk.PhysicalDeviceFeatures? VkPhysicalDeviceFeatures { get; set; }
	public SharpVk.Queue VkQueue { get; set; }
	// methods
	protected override void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.GRSharpVkGetProcedureAddressDelegate

```csharp
public sealed delegate GRSharpVkGetProcedureAddressDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public GRSharpVkGetProcedureAddressDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (string name, SharpVk.Instance instance, SharpVk.Device device, System.AsyncCallback callback, object object);
	public virtual IntPtr EndInvoke (System.IAsyncResult result);
	public virtual IntPtr Invoke (string name, SharpVk.Instance instance, SharpVk.Device device);
}
```

#### New Type: SkiaSharp.GRVkExtensionsSharpVkExtensions

```csharp
public static class GRVkExtensionsSharpVkExtensions {
	// methods
	public static void Initialize (this GRVkExtensions extensions, GRSharpVkGetProcedureAddressDelegate getProc, SharpVk.Instance instance, SharpVk.PhysicalDevice physicalDevice);
	public static void Initialize (this GRVkExtensions extensions, GRSharpVkGetProcedureAddressDelegate getProc, SharpVk.Instance instance, SharpVk.PhysicalDevice physicalDevice, string[] instanceExtensions, string[] deviceExtensions);
}
```

