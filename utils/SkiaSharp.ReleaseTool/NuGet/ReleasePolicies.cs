using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.NuGet
{
	internal sealed record ReleasePolicies(
		IReadOnlySet<string> AnchorPackages,
		IReadOnlyList<SigningCertificatePolicyEntry> Certificates)
	{
		public static ReleasePolicies Load(string repositoryRoot)
		{
			var directory = Path.Combine(repositoryRoot, "scripts", "infra", "release");
			var packages = Read(
				Path.Combine(directory, "public-packages.json"),
				PolicyJsonContext.Strict.PublicPackagesPolicyDocument);
			var signing = Read(
				Path.Combine(directory, "trusted-signing-certificates.json"),
				PolicyJsonContext.Strict.SigningCertificatePolicyDocument);

			var anchors = packages.AnchorPackages
				?? throw new NuGetReceiptException("public-packages.json anchorPackages must not be null");
			if (anchors.Count == 0)
				throw new NuGetReceiptException("public-packages.json has no anchor packages");
			if (anchors.Any(anchor =>
				string.IsNullOrWhiteSpace(anchor) ||
				anchor.Any(static character =>
					!char.IsAsciiLetterOrDigit(character) &&
					character is not ('.' or '-' or '_'))))
			{
				throw new NuGetReceiptException("public-packages.json contains an invalid anchor package ID");
			}
			if (anchors.Distinct(StringComparer.Ordinal).Count() != anchors.Count)
				throw new NuGetReceiptException("public-packages.json contains a duplicate anchor package");

			if (signing.HashAlgorithm != "SHA256")
				throw new NuGetReceiptException("trusted-signing-certificates.json hashAlgorithm must be SHA256");
			var certificates = signing.Certificates
				?? throw new NuGetReceiptException("trusted-signing-certificates.json certificates must not be null");
			if (certificates.Count == 0)
				throw new NuGetReceiptException("trusted-signing-certificates.json has no certificates");

			var fingerprints = new HashSet<string>(StringComparer.Ordinal);
			var roles = new HashSet<SigningCertificateRole>();
			foreach (var certificate in certificates)
			{
				if (certificate.Fingerprint.Length != 64 ||
					certificate.Fingerprint.Any(static character =>
						character is not (>= '0' and <= '9' or >= 'A' and <= 'F')))
				{
					throw new NuGetReceiptException(
						$"trusted-signing-certificates.json has invalid fingerprint '{certificate.Fingerprint}'");
				}
				if (!fingerprints.Add(certificate.Fingerprint))
					throw new NuGetReceiptException(
						$"trusted-signing-certificates.json has duplicate fingerprint '{certificate.Fingerprint}'");
				if (string.IsNullOrWhiteSpace(certificate.Subject) ||
					string.IsNullOrWhiteSpace(certificate.Description))
				{
					throw new NuGetReceiptException(
						$"trusted signing certificate '{certificate.Fingerprint}' requires subject and description");
				}
				if (certificate.ValidFrom is { } from &&
					certificate.ValidUntil is { } until &&
					from > until)
				{
					throw new NuGetReceiptException(
						$"trusted signing certificate '{certificate.Fingerprint}' has an inverted validity range");
				}
				roles.Add(certificate.Role);
			}
			if (!roles.SetEquals([SigningCertificateRole.Author, SigningCertificateRole.Repository]))
				throw new NuGetReceiptException("trusted signing certificates must cover author and repository roles");

			return new ReleasePolicies(
				anchors.ToHashSet(StringComparer.Ordinal),
				certificates);
		}

		private static T Read<T>(string path, JsonTypeInfo<T> typeInfo)
		{
			try
			{
				return JsonSerializer.Deserialize(File.ReadAllBytes(path), typeInfo)
					?? throw new NuGetReceiptException($"{Path.GetFileName(path)} must contain a JSON object");
			}
			catch (JsonException ex)
			{
				throw new NuGetReceiptException($"{Path.GetFileName(path)} is invalid: {ex.Message}", ex);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				throw new NuGetReceiptException($"could not read release policy '{path}'", ex);
			}
		}
	}
}
