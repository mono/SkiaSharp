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

	internal static class EnvironmentCheckReportValidator
	{
		public static void Validate(EnvironmentCheckReport report)
		{
			if (string.IsNullOrWhiteSpace(report.Name))
				throw new ValidationException("environment report name must not be empty");
			if (string.IsNullOrWhiteSpace(report.DefaultBranch))
				throw new ValidationException("environment report defaultBranch must not be empty");
			ValidateStrings(report.Reasons, "reasons");
			ValidateStrings(report.ProtectionRuleTypes, "protectionRuleTypes");
			ValidateStrings(report.AllowedBranches, "allowedBranches");
			if (report.ReviewerCount < 0)
				throw new ValidationException("environment report reviewerCount must be nonnegative");
			if (report.Ok != (report.Exists && report.Reasons.Count == 0))
				throw new ValidationException("environment report ok is inconsistent with exists and reasons");
			if (!report.Exists &&
				(report.ReviewerCount != 0 ||
				 report.PreventSelfReview ||
				 report.CustomBranchPolicies ||
				 report.AllowedBranches.Count != 0 ||
				 report.ProtectionRuleTypes.Count != 0))
			{
				throw new ValidationException("missing environment report contains configured protection state");
			}
			if (report.Ok &&
				(!report.CustomBranchPolicies ||
				 !report.PreventSelfReview ||
				 report.ReviewerCount < 1 ||
				 report.AllowedBranches.Count != 1 ||
				 report.AllowedBranches[0] != report.DefaultBranch))
			{
				throw new ValidationException("successful environment report contains unsafe protection state");
			}
		}

		private static void ValidateStrings(IReadOnlyList<string> values, string name)
		{
			if (values is null)
				throw new ValidationException($"environment report {name} must not be null");
			if (values.Any(string.IsNullOrWhiteSpace))
				throw new ValidationException($"environment report {name} must not contain empty values");
			if (values.Distinct(StringComparer.Ordinal).Count() != values.Count)
				throw new ValidationException($"environment report {name} must not contain duplicates");
		}
	}
}
