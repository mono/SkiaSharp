using System;
using System.Collections.Generic;

namespace SkiaSharp;

public class SKPdfMetadata
{
	public const float DefaultRasterDpi = SKDocument.DefaultRasterDpi;

	public const int DefaultEncodingQuality = 101;

	public SKPdfMetadata ()
	{
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

	public SKPdfStructure? Structure { get; set; }
}

public class SKPdfStructure
{
	public SKPdfStructure(int id, string type)
	{
		Id = id;
		Type = type;
	}
		
	public int Id { get; }

	public string Type { get; }
	
	public string? Alt { get; }

	public string? Language { get; }

	public IList<SKPdfStructure> Children { get; set; }

	public IList<int> AdditionalNodeIds { get; set; }

	public IList<SKPdfAttribute> Attributes { get; set; }
}
