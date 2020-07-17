using Mono.Options;

namespace SkiaSharpGenerator
{
	public static class Program
	{
		public const string Name = "skiasharp-tools";

		public static ConsoleLogger Log { get; } = new ConsoleLogger();

		public static int Main(string[] args)
		{
			var commands = new CommandSet(Name)
			{
				$"usage: {Name} COMMAND [OPTIONS]",
				"",
				"A set of tools to help with the generation of the SkiaSharp bindings.",
				"",
				"Global options:",
				{ "v|verbose", "Use a more verbose output", _ => Log.Verbose = true },
				"",
				"Available commands:",
				new CookieCommand(),
				new GenerateCommand(),
				new VerifyCommand(),
			};
			return commands.Run(args);
		}
	}
}
