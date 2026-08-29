using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaSharp.ReleaseTool.Environments
{
	public sealed record EnvironmentRequiredReviewers(
		int ReviewerCount,
		bool PreventSelfReview);

	public sealed record EnvironmentBranchPolicy(
		string Name,
		string Kind);

	public sealed record GitHubEnvironmentSnapshot(
		string Name,
		IReadOnlyList<string> ProtectionRuleTypes,
		EnvironmentRequiredReviewers? RequiredReviewers,
		bool ProtectedBranches,
		bool CustomBranchPolicies,
		IReadOnlyList<EnvironmentBranchPolicy> BranchPolicies);

	public sealed record EnvironmentCheckReport(
		string Name,
		bool Exists,
		bool Ok,
		IReadOnlyList<string> Reasons,
		string DefaultBranch,
		IReadOnlyList<string> ProtectionRuleTypes,
		IReadOnlyList<string> AllowedBranches,
		int ReviewerCount,
		bool PreventSelfReview,
		bool CustomBranchPolicies);

	internal sealed record GitHubEnvironmentResponse(
		string? Name,
		[property: JsonPropertyName("protection_rules")]
		IReadOnlyList<GitHubEnvironmentProtectionRule>? ProtectionRules,
		[property: JsonPropertyName("deployment_branch_policy")]
		GitHubDeploymentBranchPolicy? DeploymentBranchPolicy);

	internal sealed record GitHubEnvironmentProtectionRule(
		string? Type,
		[property: JsonPropertyName("prevent_self_review")]
		bool PreventSelfReview,
		IReadOnlyList<JsonElement>? Reviewers);

	internal sealed record GitHubDeploymentBranchPolicy(
		[property: JsonPropertyName("protected_branches")]
		bool ProtectedBranches,
		[property: JsonPropertyName("custom_branch_policies")]
		bool CustomBranchPolicies);

	internal sealed record GitHubBranchPolicyPage(
		[property: JsonPropertyName("branch_policies")]
		IReadOnlyList<GitHubBranchPolicyResponse>? BranchPolicies);

	internal sealed record GitHubBranchPolicyResponse(
		string? Name,
		string? Type);
}
