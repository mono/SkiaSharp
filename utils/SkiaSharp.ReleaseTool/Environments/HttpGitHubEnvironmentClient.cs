using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Planning;

namespace SkiaSharp.ReleaseTool.Environments
{
	internal sealed class HttpGitHubEnvironmentClient : IGitHubEnvironmentClient
	{
		private const string Repository = "mono/SkiaSharp";
		private readonly HttpClient httpClient;
		private readonly string? token;

		public HttpGitHubEnvironmentClient(HttpClient? httpClient = null, string? token = null)
		{
			this.httpClient = httpClient ?? new HttpClient
			{
				BaseAddress = new Uri("https://api.github.com/"),
				Timeout = TimeSpan.FromSeconds(60),
			};
			this.token = token ?? OctokitPrepareGitHubClient.ResolveToken(
				Environment.GetEnvironmentVariable);
		}

		public async Task<GitHubEnvironmentSnapshot?> GetEnvironmentAsync(
			string name,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ValidationException("environment name must not be empty");

			var encoded = Uri.EscapeDataString(name);
			var response = await GetAsync(
				$"repos/{Repository}/environments/{encoded}",
				allowNotFound: true,
				cancellationToken).ConfigureAwait(false);
			if (response is null)
				return null;

			var environment = await ReadAsync(
				response,
				EnvironmentJsonContext.Api.GitHubEnvironmentResponse,
				$"GitHub environment '{name}'",
				cancellationToken).ConfigureAwait(false);
			var rules = environment.ProtectionRules ?? [];
			if (rules.Any(rule => string.IsNullOrWhiteSpace(rule.Type)))
				throw new GitHubException($"GitHub environment '{name}' response has a protection rule without a type");
			var reviewerRules = rules.Where(rule => rule.Type == "required_reviewers").ToArray();
			if (reviewerRules.Length > 1)
				throw new GitHubException($"GitHub environment '{name}' response has multiple required_reviewers rules");
			var reviewerRule = reviewerRules.SingleOrDefault();
			var settings = environment.DeploymentBranchPolicy;
			var policies = await GetBranchPoliciesAsync(encoded, name, cancellationToken).ConfigureAwait(false);

			return new(
				Name: name,
				ProtectionRuleTypes: rules.Select(rule => rule.Type!).ToArray(),
				RequiredReviewers: reviewerRule is null
					? null
					: new(
						reviewerRule.Reviewers?.Count ?? 0,
						reviewerRule.PreventSelfReview),
				ProtectedBranches: settings?.ProtectedBranches ?? false,
				CustomBranchPolicies: settings?.CustomBranchPolicies ?? false,
				BranchPolicies: policies);
		}

		private async Task<IReadOnlyList<EnvironmentBranchPolicy>> GetBranchPoliciesAsync(
			string encodedName,
			string name,
			CancellationToken cancellationToken)
		{
			var policies = new List<EnvironmentBranchPolicy>();
			string? path =
				$"repos/{Repository}/environments/{encodedName}/deployment-branch-policies?per_page=100";
			while (path is not null)
			{
				var response = await GetAsync(path, allowNotFound: false, cancellationToken)
					.ConfigureAwait(false)
					?? throw new InvalidOperationException("A required response cannot be null.");
				var next = GetNextLink(response);
				var page = await ReadAsync(
					response,
					EnvironmentJsonContext.Api.GitHubBranchPolicyPage,
					$"GitHub environment '{name}' branch policies",
					cancellationToken).ConfigureAwait(false);
				foreach (var policy in page.BranchPolicies ?? [])
				{
					if (string.IsNullOrWhiteSpace(policy.Name) || string.IsNullOrWhiteSpace(policy.Type))
						throw new GitHubException($"GitHub environment '{name}' returned an invalid branch policy");
					policies.Add(new(policy.Name, policy.Type));
				}
				path = next;
			}
			return policies;
		}

		private async Task<HttpResponseMessage?> GetAsync(
			string path,
			bool allowNotFound,
			CancellationToken cancellationToken)
		{
			using var request = new HttpRequestMessage(HttpMethod.Get, path);
			request.Headers.UserAgent.ParseAdd("SkiaSharp.ReleaseTool");
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
			request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
			if (!string.IsNullOrEmpty(token))
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			HttpResponseMessage response;
			try
			{
				response = await httpClient.SendAsync(
					request,
					HttpCompletionOption.ResponseHeadersRead,
					cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
			{
				throw new GitHubException($"GitHub environment request timed out for '{path}'", ex);
			}
			catch (HttpRequestException ex)
			{
				throw new GitHubException($"GitHub environment request failed for '{path}'", ex);
			}

			if (allowNotFound && response.StatusCode == HttpStatusCode.NotFound)
			{
				response.Dispose();
				return null;
			}
			if (!response.IsSuccessStatusCode)
			{
				using (response)
				{
					throw new GitHubException(
						$"GitHub environment request failed ({(int)response.StatusCode} {response.ReasonPhrase}) for '{path}'");
				}
			}
			return response;
		}

		private static async Task<T> ReadAsync<T>(
			HttpResponseMessage response,
			System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo,
			string description,
			CancellationToken cancellationToken)
		{
			using (response)
			{
				try
				{
					return await JsonSerializer.DeserializeAsync(
						await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
						typeInfo,
						cancellationToken).ConfigureAwait(false)
						?? throw new GitHubException($"{description} response must contain a JSON object");
				}
				catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
				{
					throw new GitHubException($"{description} response timed out while being read", ex);
				}
				catch (Exception ex) when (ex is JsonException or IOException or HttpRequestException)
				{
					throw new GitHubException($"{description} response could not be read", ex);
				}
			}
		}

		private static string? GetNextLink(HttpResponseMessage response)
		{
			if (!response.Headers.TryGetValues("Link", out var values))
				return null;
			foreach (var link in string.Join(",", values).Split(','))
			{
				var parts = link.Split(';', StringSplitOptions.TrimEntries);
				if (parts.Length < 2 || !parts.Skip(1).Any(value => value == "rel=\"next\""))
					continue;
				var target = parts[0].Trim();
				if (target.StartsWith('<') && target.EndsWith('>'))
					return target[1..^1];
			}
			return null;
		}
	}
}
