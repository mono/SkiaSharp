using System.Security.Cryptography;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Model;
using NuGetHashAlgorithmName = NuGet.Common.HashAlgorithmName;

namespace SkiaSharp.ReleaseTool.NuGet
{
	internal sealed record VerifiedPackage(
		string Id,
		NuGetVersion Version,
		string SourceCommit,
		string SourceBranch,
		IReadOnlyList<PackageDependencyGroup> DependencyGroups);

	internal interface IPackageSignatureVerifier
	{
		Task VerifyAsync(
			PackageArchiveReader package,
			ReleasePolicies policies,
			CancellationToken cancellationToken);
	}

	internal sealed class NuGetPackageSignatureVerifier : IPackageSignatureVerifier
	{
		public async Task VerifyAsync(
			PackageArchiveReader package,
			ReleasePolicies policies,
			CancellationToken cancellationToken)
		{
			var trustedCertificates = policies.Certificates.Select(
				static certificate =>
					new KeyValuePair<string, NuGetHashAlgorithmName>(
						certificate.Fingerprint,
						NuGetHashAlgorithmName.SHA256));
			var verifier = new PackageSignatureVerifier(
			[
				new IntegrityVerificationProvider(),
				new SignatureTrustAndValidityVerificationProvider(trustedCertificates),
			]);
			var result = await verifier.VerifySignaturesAsync(
				package,
				SignedPackageVerifierSettings.GetVerifyCommandDefaultPolicy(null),
				cancellationToken,
				Guid.NewGuid()).ConfigureAwait(false);
			if (!result.IsSigned || !result.IsValid)
			{
				var errors = result.Results
					.SelectMany(static verification => verification.GetErrorIssues())
					.Select(static issue => issue.Message)
					.ToArray();
				throw new NuGetReceiptException(
					$"NuGet signature verification failed{(errors.Length == 0 ? "" : $": {string.Join("; ", errors)}")}");
			}

			var primary = await package.GetPrimarySignatureAsync(cancellationToken).ConfigureAwait(false);
			if (primary is null || primary.Type != SignatureType.Author)
				throw new NuGetReceiptException("package must have an author primary signature");
			RequireTrustedRole(primary, SigningCertificateRole.Author, policies);

			var repository = RepositoryCountersignature.GetRepositoryCountersignature(primary);
			if (repository is null || repository.Type != SignatureType.Repository)
				throw new NuGetReceiptException("package must have a NuGet.org repository countersignature");
			if (repository.V3ServiceIndexUrl != NuGetOrgPackageSource.ServiceIndex)
				throw new NuGetReceiptException("package repository countersignature is not for NuGet.org");
			RequireTrustedRole(repository, SigningCertificateRole.Repository, policies);
		}

		private static void RequireTrustedRole(
			Signature signature,
			SigningCertificateRole role,
			ReleasePolicies policies)
		{
			if (signature.SignerInfo.Certificate is not { } certificate)
				throw new NuGetReceiptException($"{role} signature has no signing certificate");
			if (certificate.NotAfter <= certificate.NotBefore)
				throw new NuGetReceiptException($"{role} signature certificate has an invalid validity period");

			var fingerprint = signature.GetSigningCertificateFingerprint(NuGetHashAlgorithmName.SHA256);
			var trusted = policies.Certificates.SingleOrDefault(
				entry => entry.Role == role && entry.Fingerprint == fingerprint);
			if (trusted is null)
				throw new NuGetReceiptException($"{role} signature certificate '{fingerprint}' is not trusted");
		}
	}

	internal static class PackageVerifier
	{
		public static async Task<VerifiedPackage> VerifyAsync(
			string expectedId,
			NuGetVersion expectedVersion,
			CatalogPackage catalog,
			byte[] bytes,
			bool verifySignature,
			IPackageSignatureVerifier signatureVerifier,
			ReleasePolicies policies,
			CancellationToken cancellationToken)
		{
			ValidateCatalog(expectedId, expectedVersion, catalog);
			if (bytes.LongLength != catalog.PackageSize)
				throw new NuGetReceiptException(
					$"{expectedId} {expectedVersion} package size {bytes.LongLength} does not match catalog size {catalog.PackageSize}");

			byte[] expectedHash;
			try
			{
				expectedHash = Convert.FromBase64String(catalog.PackageHash);
			}
			catch (FormatException ex)
			{
				throw new NuGetReceiptException($"{expectedId} {expectedVersion} catalog packageHash is malformed", ex);
			}
			var actualHash = SHA512.HashData(bytes);
			if (!CryptographicOperations.FixedTimeEquals(expectedHash, actualHash))
				throw new NuGetReceiptException($"{expectedId} {expectedVersion} package hash does not match the catalog");

			try
			{
				await using var stream = new MemoryStream(bytes, writable: false);
				using var archive = new PackageArchiveReader(stream);
				await archive.ValidatePackageEntriesAsync(cancellationToken).ConfigureAwait(false);
				var identity = archive.GetIdentity();
				if (identity.Id != expectedId ||
					!VersionComparer.VersionRelease.Equals(identity.Version, expectedVersion))
				{
					throw new NuGetReceiptException(
						$"{expectedId} nuspec identity '{identity}' does not match requested {expectedId} {expectedVersion}");
				}

				var repository = archive.NuspecReader.GetRepositoryMetadata();
				if (repository?.Type != "git")
					throw new NuGetReceiptException($"{expectedId} {expectedVersion} nuspec repository type is not 'git'");
				if (repository.Commit is null ||
					repository.Commit.Length != 40 ||
					repository.Commit.Any(static character =>
						character is not (>= '0' and <= '9' or >= 'a' and <= 'f')))
				{
					throw new NuGetReceiptException(
						$"{expectedId} {expectedVersion} nuspec repository commit is not a lowercase full SHA");
				}
				if (repository.Branch is null)
					throw new NuGetReceiptException($"{expectedId} {expectedVersion} nuspec repository branch is missing");
				_ = SkiaSharpReleaseIdentity.ParseBranch(repository.Branch);

				if (verifySignature)
					await signatureVerifier.VerifyAsync(archive, policies, cancellationToken).ConfigureAwait(false);

				return new VerifiedPackage(
					expectedId,
					expectedVersion,
					repository.Commit,
					repository.Branch,
					archive.NuspecReader.GetDependencyGroups().ToArray());
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (NuGetReceiptException)
			{
				throw;
			}
			catch (Exception ex) when (
				ex is InvalidDataException or
					PackagingException or
					System.Xml.XmlException or
					CryptographicException or
					ArgumentException or
					InvalidOperationException)
			{
				throw new NuGetReceiptException(
					$"{expectedId} {expectedVersion} package archive or nuspec is invalid",
					ex);
			}
		}

		public static void ValidateCatalog(
			string expectedId,
			NuGetVersion expectedVersion,
			CatalogPackage catalog)
		{
			if (!string.Equals(catalog.Id, expectedId, StringComparison.OrdinalIgnoreCase) ||
				!VersionComparer.VersionRelease.Equals(catalog.Version, expectedVersion))
			{
				throw new NuGetReceiptException(
					$"catalog identity mismatch: requested {expectedId} {expectedVersion}, got {catalog.Id} {catalog.Version}");
			}
			if (catalog.PackageHashAlgorithm != "SHA512" || string.IsNullOrEmpty(catalog.PackageHash))
				throw new NuGetReceiptException($"{expectedId} {expectedVersion} catalog has no SHA512 packageHash");
			if (catalog.PackageSize <= 0)
				throw new NuGetReceiptException($"{expectedId} {expectedVersion} catalog has no positive packageSize");
		}
	}
}
