using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaSharp.ReleaseTool.NuGet
{
	internal sealed record PublicPackagesPolicyDocument(
		[property: JsonPropertyName("$schemaComment")] string? SchemaComment,
		IReadOnlyList<string> AnchorPackages);

	internal enum SigningCertificateRole
	{
		[JsonStringEnumMemberName("author")]
		Author,

		[JsonStringEnumMemberName("repository")]
		Repository,
	}

	internal sealed record SigningCertificatePolicyDocument(
		[property: JsonPropertyName("$schemaComment")] string? SchemaComment,
		string HashAlgorithm,
		IReadOnlyList<SigningCertificatePolicyEntry> Certificates);

	internal sealed record SigningCertificatePolicyEntry(
		string Fingerprint,
		SigningCertificateRole Role,
		string Subject,
		string Description,
		Uri? Source,
		DateOnly? ValidFrom,
		DateOnly? ValidUntil);

	internal sealed record CatalogLeafDto(
		[property: JsonPropertyName("@id")] Uri CatalogUri,
		string Id,
		string Version,
		bool Listed,
		string PackageHash,
		string PackageHashAlgorithm,
		long PackageSize);

	[JsonSourceGenerationOptions(
		GenerationMode = JsonSourceGenerationMode.Metadata,
		PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
		RespectNullableAnnotations = true,
		RespectRequiredConstructorParameters = true,
		UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow)]
	[JsonSerializable(typeof(PublicPackagesPolicyDocument))]
	[JsonSerializable(typeof(SigningCertificatePolicyDocument))]
	internal partial class PolicyJsonContext : JsonSerializerContext
	{
		public static PolicyJsonContext Strict { get; } = new(CreateOptions());

		private static JsonSerializerOptions CreateOptions()
		{
			var options = new JsonSerializerOptions
			{
				AllowDuplicateProperties = false,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				RespectNullableAnnotations = true,
				RespectRequiredConstructorParameters = true,
				UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
			};
			options.Converters.Add(new JsonStringEnumConverter<SigningCertificateRole>(allowIntegerValues: false));
			return options;
		}
	}

	[JsonSourceGenerationOptions(
		GenerationMode = JsonSourceGenerationMode.Metadata,
		PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
		RespectNullableAnnotations = true,
		RespectRequiredConstructorParameters = true)]
	[JsonSerializable(typeof(CatalogLeafDto))]
	internal partial class CatalogJsonContext : JsonSerializerContext
	{
		public static CatalogJsonContext Strict { get; } = new(new JsonSerializerOptions
		{
			AllowDuplicateProperties = false,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			RespectNullableAnnotations = true,
			RespectRequiredConstructorParameters = true,
		});
	}
}
