using Microsoft.FileFormats;
using Microsoft.FileFormats.ELF;
using Microsoft.FileFormats.MachO;
using Microsoft.SymbolStore;
using Microsoft.SymbolStore.KeyGenerators;

namespace SkiaSharp.PackageValidation;

public sealed record MachOSlice (string Architecture, uint CpuType, uint CpuSubType, string Uuid, string FileType);

public sealed record MachOImage (bool IsFat, IReadOnlyList<MachOSlice> Slices, IReadOnlyList<string> UnreadableArchitectures)
{
	public IReadOnlyList<string> Architectures =>
		Slices.Select (s => s.Architecture).ToArray ();

	public IReadOnlySet<string> Uuids =>
		Slices.Select (s => s.Uuid).ToHashSet (StringComparer.OrdinalIgnoreCase);

	public string? GetUuid (string architecture) =>
		Slices.FirstOrDefault (s => string.Equals (s.Architecture, architecture, StringComparison.OrdinalIgnoreCase))?.Uuid;
}

/// <summary>
/// Reads native binary identity using the same Microsoft.FileFormats and Microsoft.SymbolStore
/// implementation Arcade uses to index symbols. Deliberately cross-platform: no <c>dwarfdump</c>,
/// no <c>otool</c>, no platform-specific tooling, so validation runs on the Windows package job.
/// </summary>
public static class NativeBinary
{
	private const uint CpuTypeX86 = 7;
	private const uint CpuTypeArm = 12;
	private const uint CpuTypeX86_64 = 0x01000007;
	private const uint CpuTypeArm64 = 0x0100000C;

	public const string MachODsymFileType = "Dsym";
	public const string MachODylibFileType = "Dylib";

	public static string DescribeArchitecture (uint cpuType) => cpuType switch {
		CpuTypeArm64 => "arm64",
		CpuTypeX86_64 => "x86_64",
		CpuTypeArm => "arm",
		CpuTypeX86 => "i386",
		_ => $"cpu-0x{cpuType:x}",
	};

	/// <summary>
	/// Malformed-image failures. Microsoft.FileFormats raises <see cref="BadInputFormatException"/>
	/// and <see cref="InvalidVirtualAddressException"/> for most corruption, but it also treats
	/// <see cref="OverflowException"/> as an expected outcome internally (ELFFile catches it in its
	/// own BuildID, Sections and Notes accessors), and a truncated image can surface as an
	/// out-of-range read. A validator that crashes on the very inputs it exists to reject is worse
	/// than useless, so all of these are turned into "unreadable", which the callers report as a
	/// validation error.
	/// </summary>
	private static bool IsMalformedImage (Exception exception) =>
		exception is InvalidVirtualAddressException
			or BadInputFormatException
			or OverflowException
			or ArgumentOutOfRangeException
			or IndexOutOfRangeException
			or EndOfStreamException;

	/// <summary>
	/// Reads a thin or fat Mach-O image, returning one slice per architecture. Returns
	/// <see langword="null"/> when the file is not a Mach-O image at all.
	/// </summary>
	public static MachOImage? TryReadMachO (string filePath)
	{
		using var stream = File.OpenRead (filePath);
		var addressSpace = new StreamAddressSpace (stream);

		try {
			var fat = new MachOFatFile (addressSpace);
			if (fat.IsValid ()) {
				var slices = new List<MachOSlice> ();
				var unreadable = new List<string> ();
				var arches = fat.Arches;
				var files = fat.ArchSpecificFiles;
				for (var i = 0; i < arches.Length && i < files.Length; i++) {
					var slice = TryDescribe (files[i], arches[i].CpuType, arches[i].CpuSubType);
					if (slice is not null)
						slices.Add (slice);
					else
						unreadable.Add (DescribeArchitecture (arches[i].CpuType));
				}
				return slices.Count == 0 && unreadable.Count == 0
					? null
					: new MachOImage (true, slices, unreadable);
			}
		} catch (Exception ex) when (IsMalformedImage (ex)) {
			// Not a fat image; fall through and try a thin image.
		}

		try {
			var thin = new MachOFile (addressSpace);
			if (!thin.IsValid ())
				return null;
			var slice = TryDescribe (thin, thin.Header.CpuType, thin.Header.CpuSubType);
			return slice is null
				? null
				: new MachOImage (false, new[] { slice }, Array.Empty<string> ());
		} catch (Exception ex) when (IsMalformedImage (ex)) {
			return null;
		}
	}

	/// <summary>
	/// Reads the GNU build ID from an ELF image, or <see langword="null"/> when the file is not an
	/// ELF image or carries no build ID note.
	/// </summary>
	public static string? TryReadElfBuildId (string filePath)
	{
		using var stream = File.OpenRead (filePath);

		try {
			var elf = new ELFFile (new StreamAddressSpace (stream));
			if (!elf.IsValid ())
				return null;
			var buildId = elf.BuildID;
			return buildId is null || buildId.Length == 0 ? null : Convert.ToHexString (buildId).ToLowerInvariant ();
		} catch (Exception ex) when (IsMalformedImage (ex)) {
			return null;
		}
	}

	public static bool IsElf (string filePath)
	{
		using var stream = File.OpenRead (filePath);
		Span<byte> magic = stackalloc byte[4];
		return stream.ReadAtLeast (magic, 4, throwOnEndOfStream: false) == 4 &&
			magic[0] == 0x7f && magic[1] == (byte)'E' && magic[2] == (byte)'L' && magic[3] == (byte)'F';
	}

	/// <summary>
	/// Produces the symbol-store keys Arcade would index for this file. <paramref name="fileName"/>
	/// matters: the key generators use it verbatim for identity keys.
	/// </summary>
	public static IReadOnlyList<string> GetSymbolStoreKeys (string filePath, string fileName, KeyTypeFlags flags)
	{
		try {
			using var stream = File.OpenRead (filePath);
			var generator = new FileKeyGenerator (NullTracer.Instance, new SymbolStoreFile (stream, fileName));
			if (!generator.IsValid ())
				return Array.Empty<string> ();

			stream.Position = 0;
			// GetKeys is lazy, so any parse failure surfaces here rather than above.
			return generator.GetKeys (flags).Select (k => k.Index).ToArray ();
		} catch (Exception ex) when (IsMalformedImage (ex)) {
			// No keys is itself a validation failure at every call site, so a file that passes the
			// magic check but cannot be keyed is reported rather than crashing the run.
			return Array.Empty<string> ();
		}
	}

	public static IReadOnlyList<string> GetIdentityKeys (string filePath, string fileName) =>
		GetSymbolStoreKeys (filePath, fileName, KeyTypeFlags.IdentityKey);

	public static IReadOnlyList<string> GetSymbolKeys (string filePath, string fileName) =>
		GetSymbolStoreKeys (filePath, fileName, KeyTypeFlags.SymbolKey);

	public static IReadOnlyList<string> GetIdentityAndSymbolKeys (string filePath, string fileName) =>
		GetSymbolStoreKeys (filePath, fileName, KeyTypeFlags.IdentityKey | KeyTypeFlags.SymbolKey);

	private static MachOSlice? TryDescribe (MachOFile file, uint cpuType, uint cpuSubType)
	{
		try {
			if (!file.IsValid ())
				return null;

			var uuid = file.Uuid;
			if (uuid is null || uuid.Length == 0)
				return null;

			return new MachOSlice (
				DescribeArchitecture (cpuType),
				cpuType,
				cpuSubType,
				Convert.ToHexString (uuid).ToLowerInvariant (),
				file.Header.FileType.ToString ());
		} catch (Exception ex) when (IsMalformedImage (ex)) {
			// One damaged slice must not abort the whole fat image; the caller records it as
			// unreadable so the failure is reported against the right architecture.
			return null;
		}
	}

	private sealed class NullTracer : ITracer
	{
		public static readonly NullTracer Instance = new ();

		public void WriteLine (string message) { }
		public void WriteLine (string format, params object[] arguments) { }
		public void Information (string message) { }
		public void Information (string format, params object[] arguments) { }
		public void Warning (string message) { }
		public void Warning (string format, params object[] arguments) { }
		public void Error (string message) { }
		public void Error (string format, params object[] arguments) { }
		public void Verbose (string message) { }
		public void Verbose (string format, params object[] arguments) { }
	}
}
