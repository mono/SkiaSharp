using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SkiaSharp.ReleaseTool.Artifacts;
using SkiaSharp.ReleaseTool.Errors;
using SkiaSharp.ReleaseTool.Json;
using Xunit;

namespace SkiaSharp.ReleaseTool.Tests.Json
{
	public sealed class PlanStoreTests : IDisposable
	{
		private readonly DirectoryInfo root = Directory.CreateTempSubdirectory("skiasharp-release-tool-planstore-tests-");

		public void Dispose()
		{
			try
			{
				root.Delete(recursive: true);
			}
			catch (IOException)
			{
			}
		}

		private static PreparePlan MinimalPlan() => new()
		{
			SchemaVersion = 1,
			Operation = "prepare",
			GeneratedAt = "2025-06-01T12:00:00Z",
			ToolingSha = new string('a', 40),
			NextAction = "done",
			Input = new PrepareInput { IntegrationTarget = "main", RequestedVersion = null },
			Release = new PrepareReleaseInfo
			{
				Identity = "3.119.0",
				Version = "3.119.0",
				Numeric = "3.119.0",
				Label = "stable",
				ReleaseType = "stable",
				Branch = "release/3.119.0",
				IntegrationBranch = "release/3.119.x",
				IsHotfix = false,
				Stable = true,
			},
			Base = new PrepareBaseInfo { Ref = "refs/remotes/origin/main", Sha = new string('b', 40) },
			MaintenanceBranch = new MaintenanceBranchInfo
			{
				Name = "release/3.119.x", Exists = true, Action = "none", BaseSha = null,
			},
			Skia = new PrepareSkiaInfo
			{
				Sha = new string('c', 40), ReleaseBranch = "release/3.119.0", RemoteState = "matching",
			},
			SkiaSharpRemoteState = "matching",
			Versions = new PrepareVersionsInfo { SkiaSharp = "3.119.0", RequiresPackageBump = false },
			Operations = [],
			StableBump = null,
			Warnings = [],
		};

		[Fact]
		public void Write_then_Read_round_trips_and_stamps_a_valid_digest()
		{
			var path = Path.Combine(root.FullName, "plan.json");

			var written = PlanStore.Write(path, MinimalPlan(), ReleaseJsonContext.Default.PreparePlan);

			Assert.Matches("^[0-9a-f]{64}$", written.PlanDigest);

			var read = PlanStore.Read(path, ReleaseJsonContext.Default.PreparePlan);
			Assert.Equal(written.PlanDigest, read.PlanDigest);
			Assert.Equal(written.Release.Identity, read.Release.Identity);
		}

		[Fact]
		public void Write_produces_UTF8_without_BOM_and_a_trailing_newline()
		{
			var path = Path.Combine(root.FullName, "plan.json");

			PlanStore.Write(path, MinimalPlan(), ReleaseJsonContext.Default.PreparePlan);

			var bytes = File.ReadAllBytes(path);
			Assert.NotEqual(0xEF, bytes[0]); // UTF-8 BOM starts with 0xEF 0xBB 0xBF.
			Assert.Equal((byte)'\n', bytes[^1]);
			Assert.NotEqual((byte)'\n', bytes[^2]); // Exactly one trailing newline, not two.
		}

		[Fact]
		public void Write_pretty_prints_with_sorted_keys()
		{
			var path = Path.Combine(root.FullName, "plan.json");

			PlanStore.Write(path, MinimalPlan(), ReleaseJsonContext.Default.PreparePlan);

			var text = File.ReadAllText(path, Encoding.UTF8);
			var baseIndex = text.IndexOf("\"base\":", StringComparison.Ordinal);
			var toolingShaIndex = text.IndexOf("\"toolingSha\":", StringComparison.Ordinal);
			Assert.True(baseIndex >= 0 && toolingShaIndex >= 0 && baseIndex < toolingShaIndex);
			Assert.Contains("  \"schemaVersion\": 1", text);
		}

		[Fact]
		public void Read_rejects_a_tampered_file()
		{
			var path = Path.Combine(root.FullName, "plan.json");
			PlanStore.Write(path, MinimalPlan(), ReleaseJsonContext.Default.PreparePlan);

			var text = File.ReadAllText(path);
			File.WriteAllText(path, text.Replace("\"stable\"", "\"blocked\""));

			var ex = Assert.Throws<ValidationException>(() => PlanStore.Read(path, ReleaseJsonContext.Default.PreparePlan));
			Assert.Contains("digest mismatch", ex.Message);
		}

		[Fact]
		public void Read_rejects_a_file_missing_its_digest()
		{
			var path = Path.Combine(root.FullName, "plan.json");
			var plan = MinimalPlan();
			var json = JsonSerializer.Serialize(plan, ReleaseJsonContext.Default.PreparePlan);
			File.WriteAllText(path, json);

			var ex = Assert.Throws<ValidationException>(() => PlanStore.Read(path, ReleaseJsonContext.Default.PreparePlan));
			Assert.Contains("missing its canonical digest", ex.Message);
		}

		[Fact]
		public void Read_rejects_an_unknown_top_level_field()
		{
			var path = Path.Combine(root.FullName, "plan.json");
			PlanStore.Write(path, MinimalPlan(), ReleaseJsonContext.Default.PreparePlan);

			// Inject an unmapped top-level field, then recompute+restamp
			// the digest over that same tampered shape so only the
			// strict-shape check -- not the digest check -- is exercised
			// here.
			var obj = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
			obj["injectedField"] = "not part of the schema";
			using (var withoutDigest = JsonDocument.Parse(obj.ToJsonString()))
				obj["planDigest"] = CanonicalJson.ComputeSha256Hex(withoutDigest.RootElement, "planDigest");
			File.WriteAllText(path, obj.ToJsonString());

			var ex = Assert.Throws<ValidationException>(() => PlanStore.Read(path, ReleaseJsonContext.Default.PreparePlan));
			Assert.Contains("shape validation", ex.Message);
		}

		[Fact]
		public void Read_throws_for_a_missing_file()
		{
			Assert.Throws<ValidationException>(
				() => PlanStore.Read(Path.Combine(root.FullName, "does-not-exist.json"), ReleaseJsonContext.Default.PreparePlan));
		}
	}
}
