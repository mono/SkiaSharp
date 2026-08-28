using System.Net;
using System.Text.Json;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.NuGet
{
	internal sealed record CatalogPackage(
		string Id,
		NuGetVersion Version,
		bool Listed,
		string PackageHash,
		string PackageHashAlgorithm,
		long PackageSize,
		Uri CatalogUri);

	internal interface IPublicPackageSource
	{
		Task<CatalogPackage?> GetCatalogPackageAsync(
			string id,
			NuGetVersion version,
			CancellationToken cancellationToken);

		Task<byte[]> DownloadPackageAsync(
			string id,
			NuGetVersion version,
			CancellationToken cancellationToken);
	}

	internal sealed class NuGetOrgPackageSource : IPublicPackageSource, IDisposable
	{
		internal static readonly Uri ServiceIndex = new("https://api.nuget.org/v3/index.json");

		private readonly SourceRepository repository;
		private readonly HttpClient httpClient;
		private readonly bool disposeHttpClient;

		public NuGetOrgPackageSource(HttpClient? httpClient = null)
			: this(
				Repository.Factory.GetCoreV3(ServiceIndex.AbsoluteUri),
				httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(60) },
				httpClient is null)
		{
		}

		internal NuGetOrgPackageSource(
			SourceRepository repository,
			HttpClient httpClient,
			bool disposeHttpClient = false)
		{
			if (repository.PackageSource.SourceUri != ServiceIndex)
				throw new ValidationException($"finish receipt source must be exactly {ServiceIndex}");
			this.repository = repository;
			this.httpClient = httpClient;
			this.disposeHttpClient = disposeHttpClient;
		}

		public async Task<CatalogPackage?> GetCatalogPackageAsync(
			string id,
			NuGetVersion version,
			CancellationToken cancellationToken)
		{
			PackageMetadataResource? resource;
			try
			{
				resource = await repository.GetResourceAsync<PackageMetadataResource>(
					cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
			{
				throw Transient($"NuGet.org resource discovery timed out for {id} {version}", ex);
			}
			catch (FatalProtocolException ex) when (IsTransient(ex))
			{
				throw Transient($"NuGet.org resource discovery temporarily failed for {id} {version}", ex);
			}
			catch (FatalProtocolException ex)
			{
				throw new NuGetReceiptException($"NuGet.org resource discovery failed for {id} {version}", ex);
			}
			if (resource is null)
				throw new NuGetReceiptException("NuGet.org does not expose PackageMetadataResource");
			using var cache = new SourceCacheContext
			{
				NoCache = true,
				DirectDownload = true,
			};
			IPackageSearchMetadata? metadata;
			try
			{
				metadata = await resource.GetMetadataAsync(
					new PackageIdentity(id, version),
					cache,
					NullLogger.Instance,
					cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
			{
				throw Transient($"NuGet.org metadata lookup timed out for {id} {version}", ex);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex) when (
				(ex is FatalProtocolException or HttpRequestException) &&
				IsTransient(ex))
			{
				throw Transient($"NuGet.org metadata lookup temporarily failed for {id} {version}", ex);
			}
			catch (Exception ex) when (ex is FatalProtocolException or HttpRequestException)
			{
				throw new NuGetReceiptException($"NuGet.org metadata lookup failed for {id} {version}", ex);
			}
			if (metadata is null)
				return null;
			if (metadata.Identity is null ||
				!string.Equals(metadata.Identity.Id, id, StringComparison.OrdinalIgnoreCase) ||
				!VersionComparer.VersionRelease.Equals(metadata.Identity.Version, version))
			{
				throw new NuGetReceiptException($"NuGet.org returned mismatched metadata for {id} {version}");
			}
			if (metadata is not PackageSearchMetadataRegistration registration ||
				registration.CatalogUri is null)
			{
				throw new NuGetReceiptException($"{id} {version} metadata has no catalog URI");
			}

			CatalogLeafDto leaf;
			try
			{
				using var response = await httpClient.GetAsync(
					registration.CatalogUri,
					HttpCompletionOption.ResponseHeadersRead,
					cancellationToken).ConfigureAwait(false);
				if (IsTransient(response.StatusCode))
				{
					throw Transient(
						$"NuGet.org catalog leaf temporarily returned {(int)response.StatusCode} for {id} {version}",
						new HttpRequestException(
							response.ReasonPhrase,
							null,
							response.StatusCode));
				}
				if (!response.IsSuccessStatusCode)
				throw new NuGetReceiptException(
					$"NuGet.org catalog leaf returned {(int)response.StatusCode} for {id} {version}");
				await using var stream = await response.Content.ReadAsStreamAsync(
					cancellationToken).ConfigureAwait(false);
				leaf = await JsonSerializer.DeserializeAsync(
					stream,
					CatalogJsonContext.Strict.CatalogLeafDto,
					cancellationToken).ConfigureAwait(false)
					?? throw new NuGetReceiptException($"{id} {version} catalog leaf was empty");
			}
			catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
			{
				throw Transient($"NuGet.org catalog leaf timed out for {id} {version}", ex);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (NuGetTransientException)
			{
				throw;
			}
			catch (NuGetReceiptException)
			{
				throw;
			}
			catch (HttpRequestException ex) when (IsTransient(ex))
			{
				throw Transient($"NuGet.org catalog leaf temporarily failed for {id} {version}", ex);
			}
			catch (Exception ex) when (ex is HttpRequestException or JsonException or NotSupportedException)
			{
				throw new NuGetReceiptException($"could not read catalog leaf for {id} {version}", ex);
			}

			if (leaf.CatalogUri != registration.CatalogUri)
				throw new NuGetReceiptException($"{id} {version} catalog leaf identity URI does not match registration");
			if (!NuGetVersion.TryParse(leaf.Version, out var leafVersion))
				throw new NuGetReceiptException($"{id} catalog leaf has invalid version '{leaf.Version}'");
			return new CatalogPackage(
				leaf.Id,
				leafVersion,
				leaf.Listed,
				leaf.PackageHash,
				leaf.PackageHashAlgorithm,
				leaf.PackageSize,
				leaf.CatalogUri);
		}

		public async Task<byte[]> DownloadPackageAsync(
			string id,
			NuGetVersion version,
			CancellationToken cancellationToken)
		{
			FindPackageByIdResource? resource;
			try
			{
				resource = await repository.GetResourceAsync<FindPackageByIdResource>(
					cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
			{
				throw Transient($"NuGet.org resource discovery timed out for {id} {version}", ex);
			}
			catch (FatalProtocolException ex) when (IsTransient(ex))
			{
				throw Transient($"NuGet.org resource discovery temporarily failed for {id} {version}", ex);
			}
			catch (FatalProtocolException ex)
			{
				throw new NuGetReceiptException($"NuGet.org resource discovery failed for {id} {version}", ex);
			}
			if (resource is null)
				throw new NuGetReceiptException("NuGet.org does not expose FindPackageByIdResource");
			using var cache = new SourceCacheContext
			{
				NoCache = true,
				DirectDownload = true,
			};
			await using var stream = new MemoryStream();
			bool found;
			try
			{
				found = await resource.CopyNupkgToStreamAsync(
					id,
					version,
					stream,
					cache,
					NullLogger.Instance,
					cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
			{
				throw Transient($"NuGet.org package download timed out for {id} {version}", ex);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex) when (
				(ex is FatalProtocolException or HttpRequestException) &&
				IsTransient(ex))
			{
				throw Transient($"NuGet.org package download temporarily failed for {id} {version}", ex);
			}
			catch (Exception ex) when (ex is FatalProtocolException or HttpRequestException)
			{
				throw new NuGetReceiptException($"NuGet.org package download failed for {id} {version}", ex);
			}
			if (!found)
				throw new NuGetReceiptException($"{id} {version} was cataloged but its package could not be downloaded");
			return stream.ToArray();
		}

		public void Dispose()
		{
			if (disposeHttpClient)
				httpClient.Dispose();
		}

		private static NuGetTransientException Transient(string message, Exception innerException) =>
			new(message, innerException);

		private static bool IsTransient(Exception exception) =>
			exception switch
			{
				HttpRequestException { StatusCode: { } status } => IsTransient(status),
				HttpRequestException => true,
				_ when exception.InnerException is not null => IsTransient(exception.InnerException),
				_ => false,
			};

		private static bool IsTransient(HttpStatusCode status) =>
			status is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests ||
			(int)status >= 500;
	}
}
