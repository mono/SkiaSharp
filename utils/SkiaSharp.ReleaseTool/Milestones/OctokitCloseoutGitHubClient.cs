using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Octokit;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Finishing;
using SkiaSharp.ReleaseTool.Planning;

namespace SkiaSharp.ReleaseTool.Milestones
{
	internal sealed class OctokitCloseoutGitHubClient : ICloseoutGitHubClient
	{
		private const string Owner = "mono";
		private const string Repository = "SkiaSharp";
		private const string ClosingIssuesQuery =
			"query($owner:String!,$name:String!,$number:Int!,$after:String){" +
			"repository(owner:$owner,name:$name){pullRequest(number:$number){" +
			"closingIssuesReferences(first:100,after:$after){nodes{number}" +
			"pageInfo{hasNextPage endCursor}}}}}";
		private readonly GitHubClient client;
		private readonly OctokitFinishGitHubClient releases;
		private readonly HttpClient httpClient;
		private readonly string? token;

		public OctokitCloseoutGitHubClient(HttpClient? httpClient = null, string? token = null)
		{
			this.token = token ?? OctokitPrepareGitHubClient.ResolveToken(Environment.GetEnvironmentVariable);
			client = new GitHubClient(new Octokit.ProductHeaderValue("SkiaSharp.ReleaseTool"));
			if (!string.IsNullOrEmpty(this.token))
				client.Credentials = new Credentials(this.token);
			releases = new OctokitFinishGitHubClient(this.token);
			this.httpClient = httpClient ?? new HttpClient
			{
				BaseAddress = new Uri("https://api.github.com/"),
				Timeout = TimeSpan.FromSeconds(60),
			};
		}

		public Task<FinishGitHubRelease?> GetReleaseAsync(
			string tag,
			CancellationToken cancellationToken = default) =>
			releases.GetReleaseAsync(tag, cancellationToken);

		public async Task<IReadOnlyList<GitHubMilestone>> GetMilestonesAsync(
			CancellationToken cancellationToken = default)
		{
			var values = await ExecuteAsync(
				() => client.Issue.Milestone.GetAllForRepository(
					Owner,
					Repository,
					new MilestoneRequest { State = ItemStateFilter.All },
					new ApiOptions { StartPage = 1, PageSize = 100 }),
				"list milestones",
				cancellationToken).ConfigureAwait(false);
			return values.Select(Map).ToArray();
		}

		public async Task<GitHubMilestone> CreateMilestoneAsync(
			string title,
			DateTimeOffset dueOn,
			string description,
			CancellationToken cancellationToken = default)
		{
			var request = new NewMilestone(title)
			{
				DueOn = dueOn,
				Description = description,
			};
			var value = await ExecuteAsync(
				() => client.Issue.Milestone.Create(Owner, Repository, request),
				$"create milestone '{title}'",
				cancellationToken).ConfigureAwait(false);
			return Map(value);
		}

		public async Task UpdateMilestoneAsync(
			int number,
			DateTimeOffset dueOn,
			string description,
			CancellationToken cancellationToken = default)
		{
			var current = await ExecuteAsync(
				() => client.Issue.Milestone.Get(Owner, Repository, number),
				$"read milestone {number}",
				cancellationToken).ConfigureAwait(false);
			var request = new MilestoneUpdate
			{
				Title = current.Title,
				DueOn = dueOn,
				Description = description,
			};
			_ = await ExecuteAsync(
				() => client.Issue.Milestone.Update(Owner, Repository, number, request),
				$"update milestone {number}",
				cancellationToken).ConfigureAwait(false);
		}

		public async Task<IReadOnlyList<GitHubMilestoneItem>> GetOpenMilestoneItemsAsync(
			int milestoneNumber,
			CancellationToken cancellationToken = default)
		{
			var request = new RepositoryIssueRequest
			{
				Filter = IssueFilter.All,
				State = ItemStateFilter.Open,
				Milestone = milestoneNumber.ToString(CultureInfo.InvariantCulture),
			};
			var values = await ExecuteAsync(
				() => client.Issue.GetAllForRepository(
					Owner,
					Repository,
					request,
					new ApiOptions { StartPage = 1, PageSize = 100 }),
				$"list open items for milestone {milestoneNumber}",
				cancellationToken).ConfigureAwait(false);
			return values.Select(issue => new GitHubMilestoneItem(
				issue.Number,
				issue.Title,
				new Uri(issue.HtmlUrl),
				IsPullRequest: false)).ToArray();
		}

		public async Task<string?> GetPullRequestMilestoneAsync(
			int pullRequestNumber,
			CancellationToken cancellationToken = default)
		{
			var pullRequest = await ExecuteAsync(
				() => client.PullRequest.Get(Owner, Repository, pullRequestNumber),
				$"read pull request {pullRequestNumber}",
				cancellationToken).ConfigureAwait(false);
			return pullRequest.Milestone?.Title;
		}

		public async Task<string?> GetPullRequestBodyAsync(
			int pullRequestNumber,
			CancellationToken cancellationToken = default)
		{
			var pullRequest = await ExecuteAsync(
				() => client.PullRequest.Get(Owner, Repository, pullRequestNumber),
				$"read pull request {pullRequestNumber}",
				cancellationToken).ConfigureAwait(false);
			return pullRequest.Body;
		}

		public async Task<string?> GetIssueMilestoneAsync(
			int issueNumber,
			CancellationToken cancellationToken = default)
		{
			var issue = await ExecuteAsync(
				() => client.Issue.Get(Owner, Repository, issueNumber),
				$"read issue {issueNumber}",
				cancellationToken).ConfigureAwait(false);
			return issue.Milestone?.Title;
		}

		public async Task UpdateItemMilestoneAsync(
			int itemNumber,
			int milestoneNumber,
			CancellationToken cancellationToken = default)
		{
			_ = await ExecuteAsync(
				() => client.Issue.Update(
					Owner,
					Repository,
					itemNumber,
					new IssueUpdate { Milestone = milestoneNumber }),
				$"assign item {itemNumber} to milestone {milestoneNumber}",
				cancellationToken).ConfigureAwait(false);
		}

		public async Task CloseMilestoneAsync(
			int milestoneNumber,
			CancellationToken cancellationToken = default)
		{
			var current = await ExecuteAsync(
				() => client.Issue.Milestone.Get(Owner, Repository, milestoneNumber),
				$"read milestone {milestoneNumber}",
				cancellationToken).ConfigureAwait(false);
			_ = await ExecuteAsync(
				() => client.Issue.Milestone.Update(
					Owner,
					Repository,
					milestoneNumber,
					new MilestoneUpdate
					{
						Title = current.Title,
						State = ItemState.Closed,
						Description = current.Description,
						DueOn = current.DueOn,
					}),
				$"close milestone {milestoneNumber}",
				cancellationToken).ConfigureAwait(false);
		}

		public async Task DispatchWorkflowAsync(
			string workflow,
			string reference,
			IReadOnlyDictionary<string, string> inputs,
			CancellationToken cancellationToken = default)
		{
			var request = new CreateWorkflowDispatch(reference)
			{
				Inputs = inputs.ToDictionary(
					static pair => pair.Key,
					static pair => (object)pair.Value,
					StringComparer.Ordinal),
			};
			await ExecuteAsync(
				() => client.Actions.Workflows.CreateDispatch(Owner, Repository, workflow, request),
				$"dispatch workflow '{workflow}'",
				cancellationToken).ConfigureAwait(false);
		}

		public async Task<IReadOnlyList<int>> GetClosingIssuesAsync(
			int pullRequestNumber,
			CancellationToken cancellationToken = default)
		{
			var numbers = new List<int>();
			string? after = null;
			do
			{
				var requestPayload = new ClosingIssuesGraphQlRequest(
					ClosingIssuesQuery,
					new(Owner, Repository, pullRequestNumber, after));
				var json = JsonSerializer.Serialize(
					requestPayload,
					CloseoutGitHubJsonContext.Default.ClosingIssuesGraphQlRequest);
				using var request = new HttpRequestMessage(HttpMethod.Post, "graphql")
				{
					Content = new StringContent(json, Encoding.UTF8, "application/json"),
				};
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
					throw new GitHubException($"closing-issue lookup timed out for pull request {pullRequestNumber}", ex);
				}
				catch (HttpRequestException ex)
				{
					throw new GitHubException($"closing-issue lookup failed for pull request {pullRequestNumber}", ex);
				}
				using (response)
				{
					if (!response.IsSuccessStatusCode)
					{
						throw new GitHubException(
							$"closing-issue lookup failed ({(int)response.StatusCode} {response.ReasonPhrase}) for pull request {pullRequestNumber}");
					}
					ClosingIssuesGraphQlResponse payload;
					try
					{
						payload = await JsonSerializer.DeserializeAsync(
							await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
							CloseoutGitHubJsonContext.Default.ClosingIssuesGraphQlResponse,
							cancellationToken).ConfigureAwait(false)
							?? throw new GitHubException("closing-issue GraphQL response must contain an object");
					}
					catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
					{
						throw new GitHubException($"closing-issue response timed out for pull request {pullRequestNumber}", ex);
					}
					catch (Exception ex) when (ex is JsonException or IOException or HttpRequestException)
					{
						throw new GitHubException($"closing-issue response could not be read for pull request {pullRequestNumber}", ex);
					}
					if (payload.Errors is { Count: > 0 })
					{
						throw new GitHubException(
							$"closing-issue GraphQL lookup failed for pull request {pullRequestNumber}: " +
							string.Join("; ", payload.Errors.Select(error => error.Message ?? "unknown GraphQL error")));
					}
					var connection = payload.Data?.Repository?.PullRequest?.ClosingIssuesReferences
						?? throw new GitHubException($"closing-issue GraphQL response was incomplete for pull request {pullRequestNumber}");
					numbers.AddRange((connection.Nodes ?? []).Where(node => node is not null).Select(node => node!.Number));
					if (connection.PageInfo?.HasNextPage == true &&
						string.IsNullOrWhiteSpace(connection.PageInfo.EndCursor))
					{
						throw new GitHubException($"closing-issue GraphQL pagination cursor was missing for pull request {pullRequestNumber}");
					}
					after = connection.PageInfo?.HasNextPage == true
						? connection.PageInfo.EndCursor
						: null;
				}
			}
			while (after is not null);
			return numbers.Distinct().ToArray();
		}

		private static GitHubMilestone Map(Milestone milestone) =>
			new(
				milestone.Number,
				milestone.Title,
				milestone.State == ItemState.Open,
				milestone.DueOn,
				milestone.Description);

		private static async Task<T> ExecuteAsync<T>(
			Func<Task<T>> operation,
			string description,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				return await operation().WaitAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (ApiException ex)
			{
				throw new GitHubException($"GitHub could not {description}", ex);
			}
		}

		private static async Task ExecuteAsync(
			Func<Task> operation,
			string description,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				await operation().WaitAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (ApiException ex)
			{
				throw new GitHubException($"GitHub could not {description}", ex);
			}
		}
	}
}
