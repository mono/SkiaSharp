using SkiaSharp.ReleaseTool.Errors;

namespace SkiaSharp.ReleaseTool.Environments
{
	internal interface IGitHubEnvironmentClient
	{
		Task<GitHubEnvironmentSnapshot?> GetEnvironmentAsync(
			string name,
			CancellationToken cancellationToken = default);
	}

	internal static class GitHubEnvironmentPolicy
	{
		public static EnvironmentCheckReport Check(
			GitHubEnvironmentSnapshot? snapshot,
			string name,
			string defaultBranch)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ValidationException("environment name must not be empty");
			if (string.IsNullOrWhiteSpace(defaultBranch))
				throw new ValidationException("default branch must not be empty");

			if (snapshot is null)
			{
				return new(
					Name: name,
					Exists: false,
					Ok: false,
					Reasons:
					[
						"environment does not exist: GitHub would auto-create an unprotected environment on first use and run the job without any required reviewers or branch restriction",
					],
					DefaultBranch: defaultBranch,
					ProtectionRuleTypes: [],
					AllowedBranches: [],
					ReviewerCount: 0,
					PreventSelfReview: false,
					CustomBranchPolicies: false);
			}

			var reasons = new List<string>();
			var reviewers = snapshot.RequiredReviewers;
			if (reviewers is null)
				reasons.Add("no 'required_reviewers' protection rule is configured");
			else if (reviewers.ReviewerCount < 1)
				reasons.Add("the 'required_reviewers' protection rule has no reviewers configured");
			if (reviewers is not null && !reviewers.PreventSelfReview)
				reasons.Add("'prevent_self_review' is not enabled on the required_reviewers rule");

			if (!snapshot.CustomBranchPolicies)
			{
				reasons.Add(
					"custom deployment branch policies are not enabled " +
					$"(protected_branches={snapshot.ProtectedBranches.ToString().ToLowerInvariant()}, " +
					$"custom_branch_policies={snapshot.CustomBranchPolicies.ToString().ToLowerInvariant()})");
			}

			var branchNames = snapshot.BranchPolicies
				.Where(policy => policy.Kind == "branch")
				.Select(policy => policy.Name)
				.Order(StringComparer.Ordinal)
				.ToArray();
			var tagNames = snapshot.BranchPolicies
				.Where(policy => policy.Kind == "tag")
				.Select(policy => policy.Name)
				.Order(StringComparer.Ordinal)
				.ToArray();
			if (tagNames.Length > 0)
				reasons.Add($"tag deployment policies are configured and not allowed: [{FormatNames(tagNames)}]");
			if (branchNames.Length != 1 || branchNames[0] != defaultBranch)
			{
				reasons.Add(
					$"allowed deployment branches are [{FormatNames(branchNames)}], " +
					$"expected exactly ['{defaultBranch}']");
			}

			return new(
				Name: name,
				Exists: true,
				Ok: reasons.Count == 0,
				Reasons: reasons,
				DefaultBranch: defaultBranch,
				ProtectionRuleTypes: snapshot.ProtectionRuleTypes,
				AllowedBranches: branchNames,
				ReviewerCount: reviewers?.ReviewerCount ?? 0,
				PreventSelfReview: reviewers?.PreventSelfReview ?? false,
				CustomBranchPolicies: snapshot.CustomBranchPolicies);
		}

		private static string FormatNames(IEnumerable<string> names) =>
			string.Join(", ", names.Select(name => $"'{name}'"));
	}
}
