using System.Text.RegularExpressions;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Workflows
{
	public sealed partial class ReleaseWorkflowContractTests
	{
		private static readonly string Root = FindRepositoryRoot();
		private static readonly string Prepare = Read(".github/workflows/release-prepare.yml");
		private static readonly string Finish = Read(".github/workflows/release-finish.yml");
		private static readonly string ToolingTests = Read(".github/workflows/release-tooling-tests.yml");
		private static readonly string SetupAction = Read(".github/actions/setup-release-tool/action.yml");

		[Fact]
		public void Release_workflows_are_manual_and_use_only_CSharp_core()
		{
			foreach (var workflow in new[] { Prepare, Finish })
			{
				Assert.Contains("workflow_dispatch:", workflow);
				Assert.DoesNotMatch(PushOrPullRequest(), workflow);
				Assert.Contains("utils/SkiaSharp.ReleaseTool/SkiaSharp.ReleaseTool.csproj", workflow);
				Assert.DoesNotContain("python3 scripts/infra/release/", workflow);
				Assert.DoesNotContain("gh auth", workflow);
				Assert.DoesNotMatch(GhInvocation(), workflow);
			}
		}

		[Fact]
		public void External_actions_are_pinned_and_local_setup_is_exact()
		{
			foreach (var text in new[] { Prepare, Finish, ToolingTests, SetupAction })
			{
				foreach (Match match in UsesAction().Matches(text))
				{
					var action = match.Groups[1].Value;
					if (action.StartsWith("./", StringComparison.Ordinal))
						Assert.Equal("./.github/actions/setup-release-tool", action);
					else
						Assert.Matches(@"@[0-9a-f]{40}$", action);
				}
			}

			Assert.Contains("actual=$(git rev-parse HEAD)", SetupAction);
			Assert.Contains("global-json-file: global.json", SetupAction);
			Assert.Contains("dotnet restore", SetupAction);
			Assert.Contains("--locked-mode", SetupAction);
			Assert.Contains("dotnet build", SetupAction);
			Assert.Contains("--no-restore", SetupAction);
			Assert.Contains("cache: true", SetupAction);
			Assert.Contains("SkiaSharp.ReleaseTool/packages.lock.json", SetupAction);
			Assert.Contains("SkiaSharp.ReleaseTool.Tests/packages.lock.json", SetupAction);
			Assert.Equal(2, Count(Prepare, "uses: ./.github/actions/setup-release-tool"));
			Assert.Equal(5, Count(Finish, "uses: ./.github/actions/setup-release-tool"));
		}

		[Fact]
		public void Prepare_propagates_PlanId_and_scopes_git_authentication()
		{
			Assert.Contains("plan_id: ${{ steps.outputs.outputs.plan_id }}", Prepare);
			Assert.Contains("echo \"plan_id=$(jq -r '.planId' \"$plan\")\"", Prepare);
			Assert.Contains("EXPECTED_PLAN_ID: ${{ needs.plan.outputs.plan_id }}", Prepare);
			Assert.Contains("--expected-plan-id \"$EXPECTED_PLAN_ID\"", Prepare);
			Assert.Contains("environment: release-branching", Prepare);
			Assert.Contains("ref: ${{ needs.plan.outputs.tooling_sha }}", Prepare);
			Assert.Contains("git merge-base --is-ancestor \"$TOOLING_SHA\"", Prepare);
			Assert.Contains("persist-credentials: false", Prepare);

			var apply = Job(Prepare, "apply");
			Assert.True(
				apply.IndexOf("--name release-branching", StringComparison.Ordinal) <
				apply.IndexOf("secrets.SKIASHARP_AUTOBUMP_TOKEN", StringComparison.Ordinal));
			Assert.Contains("GIT_ASKPASS", apply);
			Assert.Contains("GIT_TERMINAL_PROMPT=0", apply);
			Assert.Contains("trap cleanup EXIT", apply);
			Assert.Contains("RELEASE_GIT_TOKEN", apply);
			Assert.Contains("GH_TOKEN: ${{ secrets.SKIASHARP_AUTOBUMP_TOKEN }}", apply);
		}

		[Fact]
		public void Finish_propagates_both_correlation_ids_and_preserves_routing()
		{
			Assert.Contains("plan_id: ${{ steps.outputs.outputs.plan_id }}", Finish);
			Assert.Contains("publication_plan_id: ${{ steps.outputs.outputs.publication_plan_id }}", Finish);
			Assert.Equal(4, Count(Finish, "--expected-plan-id \"$EXPECTED_PLAN_ID\""));
			Assert.Equal(1, Count(Finish, "--expected-publication-plan-id \"$EXPECTED_PUBLICATION_PLAN_ID\""));
			Assert.Contains("EXPECTED_PUBLICATION_PLAN_ID: ${{ needs.plan-publication.outputs.publication_plan_id }}", Finish);
			Assert.Contains("if: steps.plan_command.outputs.status == '2'", Finish);
			Assert.Contains("[ \"$status\" -ne 0 ] && [ \"$status\" -ne 2 ]", Finish);
			Assert.Contains("environment: release-tag", Finish);
			Assert.Contains("environment: release-publish", Finish);
			Assert.Contains("release-finish/original/plan.json", Finish);
			Assert.Contains("release-finish/publication/publication-plan.json", Finish);
			Assert.Contains("needs.publish.result", Job(Finish, "closeout"));
			Assert.Contains("!cancelled() &&", Finish);
		}

		[Fact]
		public void Read_tokens_and_approved_write_tokens_remain_separated()
		{
			foreach (var name in new[] { "plan", "plan-publication" })
			{
				var job = Job(Finish, name);
				Assert.Contains("contents: write", job);
				Assert.Contains("GH_TOKEN: ${{ github.token }}", job);
				Assert.DoesNotContain("secrets.SKIASHARP_AUTOBUMP_TOKEN", job);
			}

			foreach (var (name, environment) in new[]
			{
				("create-draft", "release-tag"),
				("publish", "release-publish"),
			})
			{
				var job = Job(Finish, name);
				Assert.True(
					job.IndexOf($"--name {environment}", StringComparison.Ordinal) <
					job.IndexOf("secrets.SKIASHARP_AUTOBUMP_TOKEN", StringComparison.Ordinal));
			}

			var createDraft = Job(Finish, "create-draft");
			Assert.Contains("GIT_ASKPASS", createDraft);
			Assert.Contains("trap cleanup EXIT", createDraft);
			var closeout = Job(Finish, "closeout");
			Assert.DoesNotContain("environment:", closeout);
			Assert.Contains("secrets.SKIASHARP_AUTOBUMP_TOKEN", closeout);
		}

		[Fact]
		public void Tooling_gate_runs_only_retained_Python_suites()
		{
			Assert.Contains("pull_request:", ToolingTests);
			Assert.Contains(".github/actions/setup-release-tool/**", ToolingTests);
			Assert.Contains("scripts/infra/docs/release_notes/**", ToolingTests);
			Assert.Contains(".agents/skills/release-testing/**", ToolingTests);
			Assert.DoesNotContain("Run release automation tests", ToolingTests);
			Assert.Equal(2, Count(ToolingTests, "python3 -m unittest discover"));
			Assert.Contains("scripts/infra/caching/repo-deps.py validate", ToolingTests);
			Assert.Contains("--locked-mode", ToolingTests);
			Assert.Contains("--configuration Release", ToolingTests);
		}

		private static string Job(string workflow, string name)
		{
			var match = Regex.Match(
				workflow,
				$@"(?ms)^  {Regex.Escape(name)}:\n(.*?)(?=^  [a-z0-9-]+:\n|\z)");
			Assert.True(match.Success, $"Job '{name}' was not found.");
			return match.Groups[1].Value;
		}

		private static int Count(string text, string value) =>
			Regex.Matches(text, Regex.Escape(value)).Count;

		private static string Read(string path) =>
			File.ReadAllText(Path.Combine(Root, path));

		private static string FindRepositoryRoot()
		{
			var directory = new DirectoryInfo(AppContext.BaseDirectory);
			while (directory is not null)
			{
				if (File.Exists(Path.Combine(directory.FullName, "global.json")) &&
					Directory.Exists(Path.Combine(directory.FullName, ".github")))
				{
					return directory.FullName;
				}
				directory = directory.Parent;
			}
			throw new InvalidOperationException("Could not locate the repository root.");
		}

		[GeneratedRegex(@"(?m)^\s+(?:pull_request(?:_target)?|push):")]
		private static partial Regex PushOrPullRequest();

		[GeneratedRegex(@"(?m)^\s*(?:gh)(?:\s|$)")]
		private static partial Regex GhInvocation();

		[GeneratedRegex(@"(?m)^\s*uses:\s*(\S+)")]
		private static partial Regex UsesAction();
	}
}
