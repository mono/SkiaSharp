using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Mono.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Edge;

namespace WasmTestRunner
{
	public class Program
	{
		private const string DefaultUrl = "http://localhost:5000/";
		private const string ResultsFileName = "TestResults.xml";

		private static string chromeDriverPath;

		public static string ChromeDriverPath
		{
			get => chromeDriverPath;
			set
			{
				chromeDriverPath = value;
				var fname = Path.GetFileNameWithoutExtension(chromeDriverPath);
				if (fname.Equals("msedgedriver", StringComparison.OrdinalIgnoreCase))
					UseEdge = true;
			}
		}

		public static string OutputPath { get; set; } = Directory.GetCurrentDirectory();

		public static int Timeout { get; set; } = 30;

		public static bool UseHeadless { get; set; } = true;

		public static bool UseEdge { get; set; }

		public static bool ShowHelp { get; set; }

		public static bool Verbose { get; set; }

		public static string Url { get; set; } = DefaultUrl;

		public static int Main(string[] args)
		{
			var p = new OptionSet
			{
				{ "d|driver=", "the path to the ChromeDriver executable. Default is use the local version.", v => ChromeDriverPath = v },
				{ "o|output=", "the path to the test results file. Default is the current directory.", v => OutputPath = v },
				{ "t|timeout=", "the number of seconds to wait before timing out. Default is 30.", (int v) => Timeout = v },
				{ "no-headless", "do not use a headless browser.", v => UseHeadless = false },
				{ "v|verbose",  "show verbose error messages.", v => Verbose= true },
				{ "h|help",  "show this message and exit.", v => ShowHelp = true },
			};

			List<string> extra;
			try
			{
				extra = p.Parse(args);

				if (extra.Count > 1)
					throw new OptionException("To many extras provided.", "extras");

				Url = extra.FirstOrDefault() ?? DefaultUrl;
				if (string.IsNullOrEmpty(OutputPath))
					OutputPath = Directory.GetCurrentDirectory();
				OutputPath = Path.Combine(OutputPath, ResultsFileName);
				var dir = Path.GetDirectoryName(OutputPath);
				if (!string.IsNullOrEmpty(dir))
					Directory.CreateDirectory(dir);
			}
			catch (OptionException ex)
			{
				Console.Error.Write("wasm-test: ");
				Console.Error.WriteLine(ex.Message);
				Console.Error.WriteLine("Try `wasm-test --help' for more information.");
				if (Verbose)
					Console.Error.WriteLine(ex);

				return 1;
			}

			if (ShowHelp)
			{
				Console.WriteLine("Usage: wasm-test [OPTIONS]+ URL");
				Console.WriteLine("Run WASM tests in the browser.");
				Console.WriteLine();
				Console.WriteLine("Options:");
				p.WriteOptionDescriptions(Console.Out);

				return 0;
			}

			try
			{
				RunTests();

				var xdoc = XDocument.Load(OutputPath);
				var failed = xdoc.Root.Element("assembly").Attribute("failed").Value;
				if (failed != "0")
					throw new Exception($"There were test failures: {failed}");
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"There was an error running the tests: {ex.Message}");
				if (Verbose)
					Console.Error.WriteLine(ex);

				return 1;
			}

			return 0;
		}

		private static ChromiumOptions CreateOptions()
		{
			if (UseEdge)
			{
				return new EdgeOptions();
			}
			else
			{
				return new ChromeOptions();
			}
		}	

		private static (ChromiumDriverService Service, ChromiumDriver Driver) CreateDriver(ChromiumOptions options)
		{
			if (UseEdge)
			{
				var service = string.IsNullOrEmpty(ChromeDriverPath)
					? EdgeDriverService.CreateDefaultService()
					: EdgeDriverService.CreateDefaultService(ChromeDriverPath);
				var driver = new EdgeDriver(service, (EdgeOptions)options);
				return (service, driver);
			}
			else
			{
				var service = string.IsNullOrEmpty(ChromeDriverPath)
					? ChromeDriverService.CreateDefaultService()
					: ChromeDriverService.CreateDefaultService(ChromeDriverPath);
				var driver = new ChromeDriver(service, (ChromeOptions)options);
				return (service, driver);
			}
		}	

		private static void RunTests()
		{
			var options = CreateOptions();
			if (UseHeadless)
			{
				options.AddArgument("no-sandbox");
				options.AddArgument("headless");
			}

			options.AddArgument("window-size=1024x768");

			var (service, driver) = CreateDriver(options);
			using var _ = service;
			using var __ = driver;

			driver.Url = Url;

			var index = 0;
			var currentTimeout = Timeout;

			do
			{
				var pre = driver.FindElements(By.TagName("PRE")).Skip(index).ToArray();
				if (pre.Length > 0)
				{
					index += pre.Length;
					currentTimeout = Timeout; // reset the timeout

					foreach (var e in pre)
						Console.WriteLine(e.Text);
				}

				var resultsElement = driver.FindElements(By.Id("results"));
				if (resultsElement.Count == 0)
				{
					if (driver.FindElements(By.ClassName("neterror")).Count > 0)
					{
						var errorCode = driver.FindElements(By.ClassName("error-code")).FirstOrDefault()?.Text;
						throw new Exception($"There was an error loading the page: {errorCode}");
					}

					Thread.Sleep(500);
					continue;
				}

				var text = resultsElement[0].Text;
				var bytes = Convert.FromBase64String(text);
				File.WriteAllBytes(OutputPath, bytes);
				break;
			} while (--currentTimeout > 0);

			if (currentTimeout <= 0)
				throw new TimeoutException();
		}
	}
}
