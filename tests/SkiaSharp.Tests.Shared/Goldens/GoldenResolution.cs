using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp.Tests.Shared;

/// <summary>Describes the selected reference and every candidate attempted before it.</summary>
public sealed class GoldenResolution
{
	private readonly Func<byte[]>? read;

	internal GoldenResolution(GoldenRequest request, string? key, string? location, IEnumerable<string> candidates, Func<byte[]>? read)
	{
		Request = request;
		Key = key;
		Location = location;
		Candidates = candidates.ToList().AsReadOnly();
		this.read = read;
	}

	public GoldenRequest Request { get; }
	public string? Key { get; }
	public string? Location { get; }
	public IReadOnlyList<string> Candidates { get; }

	public byte[] ReadBytes() => read == null ? throw new InvalidOperationException(Describe()) : read();

	public string Describe() =>
		"Golden request: " + Request.Platform +
		(Request.Version == null ? "" : " at " + Request.Version) + "/" + Request.CasePath + Environment.NewLine +
		(Key == null ? "MISSING reference" : "Selected: " + Key + " at " + Location) +
		Environment.NewLine + "Attempted (in order):" + Environment.NewLine + "  " +
		string.Join(Environment.NewLine + "  ", Candidates);
}
