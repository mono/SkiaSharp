using System;
using System.Buffers;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// Compares the attribute/varying UTF-8 string marshalling in SKMeshSpecification.CreateCore:
//   - Old: StringUtilities.GetEncodedText(name, Utf8, addNull: true) + AsMemory().Pin() per name
//   - New: Utils.RentUtf8Strings(count) using ArrayPool<byte> + GCHandle.Alloc(Pinned)
[MemoryDiagnoser]
public unsafe class RentedUtf8StringsBenchmark
{
	private static readonly string[] Names = {
		"position",
		"uvDepth",
		"normalSpec",
		"tangent",
		"color",
		"vUvDepth",
		"vNormalSpec",
		"vColor",
	};

	[Benchmark (Baseline = true)]
	public IntPtr OldGetEncodedTextAndPin ()
	{
		var pins = new MemoryHandle[Names.Length];
		IntPtr sum = IntPtr.Zero;
		try {
			for (var i = 0; i < Names.Length; i++) {
				var bytes = StringUtilities.GetEncodedText (Names[i], SKTextEncoding.Utf8, true);
				var pin = bytes.AsMemory ().Pin ();
				pins[i] = pin;
				sum += (int)*(byte*)pin.Pointer;
			}
		} finally {
			for (var i = 0; i < Names.Length; i++)
				pins[i].Dispose ();
		}
		return sum;
	}

	[Benchmark]
	public IntPtr NewRentUtf8Strings ()
	{
		using var names = Utils.RentUtf8Strings (Names.Length);
		IntPtr sum = IntPtr.Zero;
		for (var i = 0; i < Names.Length; i++) {
			var ptr = (byte*)names.Add (Names[i]);
			sum += *ptr;
		}
		return sum;
	}
}
