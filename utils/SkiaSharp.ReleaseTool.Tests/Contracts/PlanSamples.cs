using SkiaSharp.ReleaseTool.Contracts;

namespace SkiaSharp.ReleaseTool.Tests.Contracts
{
	internal static class PlanSamples
	{
		public static readonly Guid PlanId = Guid.Parse("c80c7170-e4e7-4388-8100-b7169a2e16cd");

		public static PreparePlan Prepare() => new(
			SchemaVersion: 1,
			Operation: ReleaseOperation.Prepare,
			PlanId: PlanId,
			GeneratedAt: new DateTimeOffset(2026, 8, 28, 12, 0, 0, TimeSpan.Zero),
			ToolingSha: Sha('a'),
			NextAction: PrepareNextAction.Done,
			Input: new PrepareInput(
				"release/3.119.x",
				"3.119.0-preview.2",
				null),
			Release: new PrepareReleaseInfo(
				"3.119.0-preview.2",
				"3.119.0-preview.2",
				"release/3.119.0-preview.2"),
			Base: new PrepareBaseInfo("refs/remotes/origin/release/3.119.x", Sha('b')),
			MaintenanceBranch: new MaintenanceBranchInfo(
				"release/3.119.x",
				true,
				MaintenanceBranchAction.None,
				null),
			Skia: new PrepareSkiaInfo(
				Sha('c'),
				RemoteState.Matching),
			SkiaSharpRemoteState: RemoteState.Matching,
			Versions: new PrepareVersionsInfo(false),
			Operations:
			[
				new PlanOperation(
					PlanOperationId.CreateMaintenanceBranch,
					PlanOperationKind.GitRef,
					PlanOperationStatus.Done,
					null),
				new PlanOperation(
					PlanOperationId.CreateSkiaRef,
					PlanOperationKind.GitHubRef,
					PlanOperationStatus.Done,
					null),
				new PlanOperation(
					PlanOperationId.CreateReleaseBranch,
					PlanOperationKind.GitRef,
					PlanOperationStatus.Done,
					null),
			],
			StableBump: null,
			Warnings: []);

		public static PreparePlan StablePrepare()
		{
			var preview = Prepare();
			return preview with
			{
				NextAction = PrepareNextAction.Apply,
				Input = preview.Input with { RequestedVersion = "3.119.0" },
				Release = new PrepareReleaseInfo(
					"3.119.0",
					"3.119.0",
					"release/3.119.0"),
				Operations =
				[
					.. preview.Operations,
					new PlanOperation(
						PlanOperationId.OpenStableBumpPullRequest,
						PlanOperationKind.GitHubPullRequest,
						PlanOperationStatus.Pending,
						null),
				],
				StableBump = new StableBumpInfo(
					"release/3.119.x",
					"bump-version-3.119.1",
					"3.119.1",
					"1.8.8.4",
					PlanOperationStatus.Pending,
					null,
					"Bump to the next version (3.119.1) after release"),
			};
		}

		public static string Sha(char character) => new(character, 40);
	}
}
