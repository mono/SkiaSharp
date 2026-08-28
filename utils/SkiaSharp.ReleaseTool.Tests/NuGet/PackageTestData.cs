using System.Security.Cryptography;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using SkiaSharp.ReleaseTool.NuGet;

namespace SkiaSharp.ReleaseTool.Tests.NuGet
{
	internal static class PackageTestData
	{
		public static byte[] Create(
			string id,
			string version,
			string commit,
			string branch,
			params (string Framework, string Dependency, string Range)[] dependencies)
		{
			var builder = new PackageBuilder
			{
				Id = id,
				Version = NuGetVersion.Parse(version),
				Description = "release tool test package",
				Repository = new RepositoryMetadata("git", "https://ignored.invalid", branch, commit),
			};
			builder.Authors.Add("SkiaSharp");
			foreach (var dependency in dependencies)
			{
				builder.DependencyGroups.Add(
					new PackageDependencyGroup(
						NuGetFramework.Parse(dependency.Framework),
						[
							new PackageDependency(
								dependency.Dependency,
								VersionRange.Parse(dependency.Range)),
						]));
			}
			if (dependencies.Length == 0)
			{
				builder.DependencyGroups.Add(
					new PackageDependencyGroup(
						NuGetFramework.AnyFramework,
						[new PackageDependency("Test.Dependency", VersionRange.Parse("[1.0.0]"))]));
			}
			using var stream = new MemoryStream();
			builder.Save(stream);
			return stream.ToArray();
		}

		public static CatalogPackage Catalog(string id, string version, byte[] bytes, bool listed = true) =>
			new(
				id,
				NuGetVersion.Parse(version),
				listed,
				Convert.ToBase64String(SHA512.HashData(bytes)),
				"SHA512",
				bytes.LongLength,
				new Uri($"https://api.nuget.org/v3/catalog0/data/2026.01.01/{id.ToLowerInvariant()}.json"));

		public static ReleasePolicies Policies() => new(
			new HashSet<string>(
				["SkiaSharp", "SkiaSharp.HarfBuzz", "HarfBuzzSharp"],
				StringComparer.Ordinal),
			[
				new(
					new string('A', 64),
					SigningCertificateRole.Author,
					"author",
					"author test certificate",
					null,
					null,
					new DateOnly(2030, 1, 1)),
				new(
					new string('B', 64),
					SigningCertificateRole.Repository,
					"repository",
					"repository test certificate",
					null,
					null,
					new DateOnly(2030, 1, 1)),
			]);
	}

	internal sealed class RecordingSignatureVerifier : IPackageSignatureVerifier
	{
		public List<string> VerifiedIds { get; } = [];
		public Exception? Failure { get; set; }

		public Task VerifyAsync(
			PackageArchiveReader package,
			ReleasePolicies policies,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (Failure is not null)
				throw Failure;
			VerifiedIds.Add(package.GetIdentity().Id);
			return Task.CompletedTask;
		}
	}
}
