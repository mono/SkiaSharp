var testRunner = new Xunit.Sdk.ThreadlessXunitTestRunner();

var result = testRunner.Run(typeof(Program).Assembly.GetName().Name + ".dll", []);

return result ? 1 : 0;
