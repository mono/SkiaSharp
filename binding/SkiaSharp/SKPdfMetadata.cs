using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharp;

public class SKPdfMetadata
{
	public const float DefaultRasterDpi = SKDocument.DefaultRasterDpi;

	public const int DefaultEncodingQuality = 101;

	public SKPdfMetadata ()
	{
	}

	internal SKPdfMetadata (SKDocumentPdfMetadata metadata)
	{
		Title = metadata.Title;
		Author = metadata.Author;
		Subject = metadata.Subject;
		Keywords = metadata.Keywords;
		Creator = metadata.Creator;
		Producer = metadata.Producer;
		Creation = metadata.Creation;
		Modified = metadata.Modified;
		RasterDpi = metadata.RasterDpi;
		PdfA = metadata.PdfA;
		EncodingQuality = metadata.EncodingQuality;
	}

	public string? Title { get; set; }

	public string? Author { get; set; }

	public string? Subject { get; set; }

	public string? Keywords { get; set; }

	public string? Creator { get; set; }

	public string? Producer { get; set; }

	public DateTime? Creation { get; set; }

	public DateTime? Modified { get; set; }

	public float RasterDpi { get; set; } = DefaultRasterDpi;

	public bool PdfA { get; set; }

	public int EncodingQuality { get; set; } = DefaultEncodingQuality;

	public SKPdfCompression Compression { get; set; } = SKPdfCompression.Default;

	public SKPdfStructureElement? Structure { get; set; }
}

public class SKPdfStructureElement
{
	private List<SKPdfStructureElement>? children;
	private List<int>? additionalNodeIds;
	private SKPdfAttributeList? attributes;

	public SKPdfStructureElement (int id, string type)
	{
		Id = id;
		Type = type;
	}

	public int Id { get; }

	public string Type { get; }

	public string? Alt { get; set; }

	public string? Language { get; set; }

	public IList<SKPdfStructureElement> Children => children ??= new ();

	public IList<int> AdditionalNodeIds => additionalNodeIds ??= new ();

	public SKPdfAttributeList Attributes => attributes ??= new ();
}

public class SKPdfAttributeList
{
	private List<(string Owner, string Name, object Value)> attributes = new ();

	internal IReadOnlyList<(string Owner, string Name, object Value)> Inner => attributes;

	public void Add (string owner, string name, int value) =>
		attributes.Add ((owner, name, value));

	public void Add (string owner, string name, float value) =>
		attributes.Add ((owner, name, value));

	public void Add (string owner, string name, string value) =>
		attributes.Add ((owner, name, value));

	public void Add (string owner, string name, IEnumerable<int> value) =>
		attributes.Add ((owner, name, value));

	public void Add (string owner, string name, IEnumerable<float> value) =>
		attributes.Add ((owner, name, value));
}
