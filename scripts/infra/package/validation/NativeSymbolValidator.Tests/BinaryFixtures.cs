using System.Buffers.Binary;

namespace SkiaSharp.PackageValidation.Tests;

/// <summary>
/// Builds the minimum valid Mach-O and ELF images the validator's readers and Microsoft.SymbolStore
/// accept. Real binaries cannot be committed and cannot be produced on every platform, so the
/// fixtures are synthesised instead: that keeps the tests hermetic and lets them express corrupt,
/// mismatched and incomplete inputs that a real toolchain would never emit.
/// </summary>
public static class BinaryFixtures
{
	public const uint CpuTypeArm64 = 0x0100000C;
	public const uint CpuTypeX86_64 = 0x01000007;

	public const uint MachOFileTypeDylib = 6;
	public const uint MachOFileTypeDsym = 0xA;

	private const uint MachO64Magic = 0xfeedfacf;
	private const uint FatMagic = 0xcafebabe;
	private const uint LcSegment64 = 0x19;
	private const uint LcUuid = 0x1b;

	private const int MachOHeaderSize = 32;
	private const int SegmentCommandSize = 72;
	private const int UuidCommandSize = 24;
	private const int FatSliceAlignment = 0x4000;

	public static string NewUuid () =>
		Guid.NewGuid ().ToString ("N").ToUpperInvariant ();

	public static string NewBuildId () =>
		Convert.ToHexString (Guid.NewGuid ().ToByteArray ().Concat (new byte[] { 1, 2, 3, 4 }).ToArray ()).ToLowerInvariant ();

	/// <summary>
	/// A thin 64-bit Mach-O image carrying a <c>__TEXT</c> segment and an <c>LC_UUID</c>, which is
	/// everything both the validator and Microsoft.SymbolStore read.
	/// </summary>
	public static byte[] MachO (uint cpuType, string uuid, uint fileType = MachOFileTypeDylib, int totalSize = 1024)
	{
		var minimum = MachOHeaderSize + SegmentCommandSize + UuidCommandSize;
		if (totalSize < minimum)
			totalSize = minimum;

		var image = new byte[totalSize];
		var span = image.AsSpan ();

		BinaryPrimitives.WriteUInt32LittleEndian (span[0..], MachO64Magic);
		BinaryPrimitives.WriteUInt32LittleEndian (span[4..], cpuType);
		BinaryPrimitives.WriteUInt32LittleEndian (span[8..], 0);
		BinaryPrimitives.WriteUInt32LittleEndian (span[12..], fileType);
		BinaryPrimitives.WriteUInt32LittleEndian (span[16..], 2);
		BinaryPrimitives.WriteUInt32LittleEndian (span[20..], SegmentCommandSize + UuidCommandSize);
		BinaryPrimitives.WriteUInt32LittleEndian (span[24..], 0);
		BinaryPrimitives.WriteUInt32LittleEndian (span[28..], 0);

		var offset = MachOHeaderSize;

		// LC_SEGMENT_64 __TEXT. The file offset/size must cover the whole image or the symbol store
		// reader treats every address in the file as unmapped.
		BinaryPrimitives.WriteUInt32LittleEndian (span[offset..], LcSegment64);
		BinaryPrimitives.WriteUInt32LittleEndian (span[(offset + 4)..], SegmentCommandSize);
		"__TEXT"u8.CopyTo (span[(offset + 8)..]);
		BinaryPrimitives.WriteUInt64LittleEndian (span[(offset + 24)..], 0);
		BinaryPrimitives.WriteUInt64LittleEndian (span[(offset + 32)..], (ulong) totalSize);
		BinaryPrimitives.WriteUInt64LittleEndian (span[(offset + 40)..], 0);
		BinaryPrimitives.WriteUInt64LittleEndian (span[(offset + 48)..], (ulong) totalSize);
		BinaryPrimitives.WriteUInt32LittleEndian (span[(offset + 56)..], 5);
		BinaryPrimitives.WriteUInt32LittleEndian (span[(offset + 60)..], 5);
		BinaryPrimitives.WriteUInt32LittleEndian (span[(offset + 64)..], 0);
		BinaryPrimitives.WriteUInt32LittleEndian (span[(offset + 68)..], 0);

		offset += SegmentCommandSize;

		BinaryPrimitives.WriteUInt32LittleEndian (span[offset..], LcUuid);
		BinaryPrimitives.WriteUInt32LittleEndian (span[(offset + 4)..], UuidCommandSize);
		Convert.FromHexString (uuid).CopyTo (span[(offset + 8)..]);

		return image;
	}

	/// <summary>
	/// A universal binary wrapping one thin image per architecture, matching what <c>lipo</c>
	/// produces for the shipped Apple modules.
	/// </summary>
	public static byte[] FatMachO (params (uint CpuType, byte[] Slice)[] slices)
	{
		var headerSize = 8 + (slices.Length * 20);
		var offsets = new int[slices.Length];
		var cursor = Align (headerSize);

		for (var i = 0; i < slices.Length; i++) {
			offsets[i] = cursor;
			cursor = Align (cursor + slices[i].Slice.Length);
		}

		var image = new byte[cursor];
		var span = image.AsSpan ();

		// Fat headers are always big-endian regardless of the slice endianness.
		BinaryPrimitives.WriteUInt32BigEndian (span[0..], FatMagic);
		BinaryPrimitives.WriteUInt32BigEndian (span[4..], (uint) slices.Length);

		for (var i = 0; i < slices.Length; i++) {
			var entry = 8 + (i * 20);
			BinaryPrimitives.WriteUInt32BigEndian (span[entry..], slices[i].CpuType);
			BinaryPrimitives.WriteUInt32BigEndian (span[(entry + 4)..], 0);
			BinaryPrimitives.WriteUInt32BigEndian (span[(entry + 8)..], (uint) offsets[i]);
			BinaryPrimitives.WriteUInt32BigEndian (span[(entry + 12)..], (uint) slices[i].Slice.Length);
			BinaryPrimitives.WriteUInt32BigEndian (span[(entry + 16)..], 14);

			slices[i].Slice.CopyTo (span[offsets[i]..]);
		}

		return image;

		static int Align (int value) =>
			(value + FatSliceAlignment - 1) / FatSliceAlignment * FatSliceAlignment;
	}

	/// <summary>
	/// A 64-bit little-endian ELF shared object carrying a single <c>PT_NOTE</c> GNU build ID note,
	/// which is what ties an Android <c>.so</c> to its <c>.so.dbg</c>.
	/// </summary>
	public static byte[] Elf (string buildId, int totalSize = 1024)
	{
		const int HeaderSize = 64;
		const int ProgramHeaderSize = 56;
		const int NoteOffset = HeaderSize + ProgramHeaderSize;

		var descriptor = Convert.FromHexString (buildId);
		var noteSize = 12 + 4 + descriptor.Length;
		var minimum = NoteOffset + noteSize;
		if (totalSize < minimum)
			totalSize = minimum;

		var image = new byte[totalSize];
		var span = image.AsSpan ();

		span[0] = 0x7f;
		span[1] = (byte) 'E';
		span[2] = (byte) 'L';
		span[3] = (byte) 'F';
		span[4] = 2; // ELFCLASS64
		span[5] = 1; // ELFDATA2LSB
		span[6] = 1; // EV_CURRENT

		BinaryPrimitives.WriteUInt16LittleEndian (span[16..], 3); // ET_DYN
		BinaryPrimitives.WriteUInt16LittleEndian (span[18..], 183); // EM_AARCH64
		BinaryPrimitives.WriteUInt32LittleEndian (span[20..], 1);
		BinaryPrimitives.WriteUInt64LittleEndian (span[24..], 0);
		BinaryPrimitives.WriteUInt64LittleEndian (span[32..], HeaderSize);
		BinaryPrimitives.WriteUInt64LittleEndian (span[40..], 0);
		BinaryPrimitives.WriteUInt32LittleEndian (span[48..], 0);
		BinaryPrimitives.WriteUInt16LittleEndian (span[52..], HeaderSize);
		BinaryPrimitives.WriteUInt16LittleEndian (span[54..], ProgramHeaderSize);
		BinaryPrimitives.WriteUInt16LittleEndian (span[56..], 1);
		BinaryPrimitives.WriteUInt16LittleEndian (span[58..], 64);
		BinaryPrimitives.WriteUInt16LittleEndian (span[60..], 0);
		BinaryPrimitives.WriteUInt16LittleEndian (span[62..], 0);

		BinaryPrimitives.WriteUInt32LittleEndian (span[HeaderSize..], 4); // PT_NOTE
		BinaryPrimitives.WriteUInt32LittleEndian (span[(HeaderSize + 4)..], 4); // PF_R
		BinaryPrimitives.WriteUInt64LittleEndian (span[(HeaderSize + 8)..], NoteOffset);
		BinaryPrimitives.WriteUInt64LittleEndian (span[(HeaderSize + 16)..], NoteOffset);
		BinaryPrimitives.WriteUInt64LittleEndian (span[(HeaderSize + 24)..], NoteOffset);
		BinaryPrimitives.WriteUInt64LittleEndian (span[(HeaderSize + 32)..], (ulong) noteSize);
		BinaryPrimitives.WriteUInt64LittleEndian (span[(HeaderSize + 40)..], (ulong) noteSize);
		BinaryPrimitives.WriteUInt64LittleEndian (span[(HeaderSize + 48)..], 4);

		BinaryPrimitives.WriteUInt32LittleEndian (span[NoteOffset..], 4);
		BinaryPrimitives.WriteUInt32LittleEndian (span[(NoteOffset + 4)..], (uint) descriptor.Length);
		BinaryPrimitives.WriteUInt32LittleEndian (span[(NoteOffset + 8)..], 3); // NT_GNU_BUILD_ID
		"GNU\0"u8.CopyTo (span[(NoteOffset + 12)..]);
		descriptor.CopyTo (span[(NoteOffset + 16)..]);

		return image;
	}

	public static byte[] InfoPlist (string name) =>
		System.Text.Encoding.UTF8.GetBytes (
			$"""
			<?xml version="1.0" encoding="UTF-8"?>
			<plist version="1.0">
			  <dict>
			    <key>CFBundleIdentifier</key>
			    <string>com.apple.xcode.dsym.{name}</string>
			  </dict>
			</plist>
			""");

	/// <summary>
	/// Produces a real zip archive, so tests can exercise the MacCatalyst framework.zip that is the
	/// only Mach-O a Catalyst customer ever consumes.
	/// </summary>
	public static byte[] Zip (params (string Entry, byte[] Content)[] entries)
	{
		using var buffer = new MemoryStream ();

		using (var archive = new System.IO.Compression.ZipArchive (buffer, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true)) {
			foreach (var (entry, content) in entries) {
				using var stream = archive.CreateEntry (entry).Open ();
				stream.Write (content);
			}
		}

		return buffer.ToArray ();
	}
}
