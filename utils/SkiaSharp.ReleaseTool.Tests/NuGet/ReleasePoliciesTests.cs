using System.Text.Json;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.NuGet;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.NuGet
{
	public sealed class ReleasePoliciesTests
	{
		[Fact]
		public void Repository_policy_files_load_as_strict_typed_documents()
		{
			var root = FindRepositoryRoot();

			var policies = ReleasePolicies.Load(root);

			Assert.NotEmpty(policies.AnchorPackages);
			Assert.All(
				policies.AnchorPackages,
				static id => Assert.DoesNotContain(id, char.IsWhiteSpace));
			Assert.Contains(policies.Certificates, certificate => certificate.Role == SigningCertificateRole.Author);
			Assert.Contains(policies.Certificates, certificate => certificate.Role == SigningCertificateRole.Repository);
		}

		[Fact]
		public void Policy_loader_rejects_unknown_members()
		{
			using var root = new TestDirectory("invalid-policy");
			var directory = Path.Combine(root.Path, "scripts", "infra", "release");
			Directory.CreateDirectory(directory);
			File.WriteAllText(
				Path.Combine(directory, "public-packages.json"),
				"""
				{"$schemaComment":null,"anchorPackages":["SkiaSharp","SkiaSharp.HarfBuzz","HarfBuzzSharp"],"unknown":true}
				""");
			File.WriteAllText(
				Path.Combine(directory, "trusted-signing-certificates.json"),
				"""
				{"$schemaComment":null,"hashAlgorithm":"SHA256","certificates":[
				  {"fingerprint":"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA","role":"author","subject":"a","description":"a","source":null,"validFrom":null,"validUntil":"2030-01-01"},
				  {"fingerprint":"BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB","role":"repository","subject":"r","description":"r","source":null,"validFrom":null,"validUntil":"2030-01-01"}
				]}
				""");

			Assert.Throws<NuGetReceiptException>(() => ReleasePolicies.Load(root.Path));
		}

		[Fact]
		public void Catalog_leaf_uses_source_generated_typed_fields()
		{
			const string json =
				"""
				{
				  "@id":"https://api.nuget.org/v3/catalog0/data/2026.01.01/skiasharp.json",
				  "@type":["PackageDetails","catalog:Permalink"],
				  "id":"SkiaSharp",
				  "version":"4.152.0",
				  "listed":true,
				  "packageHash":"abc=",
				  "packageHashAlgorithm":"SHA512",
				  "packageSize":123
				}
				""";

			var leaf = JsonSerializer.Deserialize(json, CatalogJsonContext.Strict.CatalogLeafDto);

			Assert.NotNull(leaf);
			Assert.Equal("SkiaSharp", leaf.Id);
			Assert.Equal(123, leaf.PackageSize);
			Assert.Equal("SHA512", leaf.PackageHashAlgorithm);
			Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
				json.Replace(
					"\"packageSize\":123",
					"\"packageSize\":123,\"packageSize\":123",
					StringComparison.Ordinal),
				CatalogJsonContext.Strict.CatalogLeafDto));
			Assert.Throws<JsonException>(() => JsonSerializer.Deserialize(
				json.Replace("\"packageHash\":\"abc=\",", "", StringComparison.Ordinal),
				CatalogJsonContext.Strict.CatalogLeafDto));
		}

		private static string FindRepositoryRoot()
		{
			var directory = new DirectoryInfo(AppContext.BaseDirectory);
			while (directory is not null)
			{
				if (File.Exists(Path.Combine(
					directory.FullName,
					"scripts",
					"infra",
					"release",
					"public-packages.json")))
				{
					return directory.FullName;
				}
				directory = directory.Parent;
			}
			throw new InvalidOperationException("could not find repository root");
		}
	}
}
