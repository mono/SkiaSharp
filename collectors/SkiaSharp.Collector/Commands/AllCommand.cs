using Spectre.Console;
using Spectre.Console.Cli;

namespace SkiaSharp.Collector.Commands;

public class AllCommand : AsyncCommand<NuGetSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, NuGetSettings settings)
    {
        if (!settings.Quiet)
        {
            AnsiConsole.Write(new Rule("[bold blue]SkiaSharp Dashboard Collector[/]").RuleStyle("blue"));
            AnsiConsole.WriteLine();
        }

        var results = new List<(string name, bool success)>();

        // Run each collector
        var collectors = new (string name, Func<Task<int>> run)[]
        {
            ("GitHub Stats", () => new GitHubCommand().ExecuteAsync(context, settings)),
            ("NuGet Stats", () => new NuGetCommand().ExecuteAsync(context, settings)),
            ("Community Stats", () => new CommunityCommand().ExecuteAsync(context, settings)),
            ("Issues", () => new IssuesCommand().ExecuteAsync(context, settings)),
            ("PR Triage", () => new PrTriageCommand().ExecuteAsync(context, settings))
        };

        foreach (var (name, run) in collectors)
        {
            if (!settings.Quiet)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Rule($"[yellow]{name}[/]").RuleStyle("dim"));
            }

            try
            {
                var result = await run();
                results.Add((name, result == 0));
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error collecting {name}: {ex.Message}[/]");
                if (settings.Verbose)
                    AnsiConsole.WriteException(ex);
                results.Add((name, false));
            }
        }

        // Summary
        if (!settings.Quiet)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[bold]Summary[/]").RuleStyle("green"));

            var table = new Table()
                .AddColumn("Collector")
                .AddColumn("Status");

            foreach (var (name, success) in results)
            {
                table.AddRow(name, success ? "[green]✓ Success[/]" : "[red]✗ Failed[/]");
            }

            AnsiConsole.Write(table);

            var allSuccess = results.All(r => r.success);
            if (allSuccess)
                AnsiConsole.MarkupLine($"\n[bold green]All collectors completed successfully![/]");
            else
                AnsiConsole.MarkupLine($"\n[bold yellow]Some collectors failed. Check logs above.[/]");
        }

        return results.All(r => r.success) ? 0 : 1;
    }
}
