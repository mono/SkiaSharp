
var SetEnvironmentVariable = new Action<string, string> ((name, value) => {
    Information ("Setting Environment Variable {0} to {1}", name, value);
    Environment.SetEnvironmentVariable (name, value, EnvironmentVariableTarget.Process);
});

var AppendEnvironmentVariable = new Action<string, string> ((name, value) => {
    var old = EnvironmentVariable (name);
    var sep = IsRunningOnWindows () ? ';' : ':';
    
    if (!old.ToUpper ().Split (sep).Contains (value.ToUpper ())) {
        Information ("Adding {0} to Environment Variable {1}", value, name);
        value += sep + old;
        SetEnvironmentVariable (name, value);
    }
});

void ListEnvironmentVariables ()
{
    Information ("Environment Variables:");
    foreach (var envVar in EnvironmentVariables ()) {
        Information ("\tKey: {0}\tValue: \"{1}\"", envVar.Key, envVar.Value);
    }
}

FilePath GetToolPath (FilePath toolPath)
{
    var appRoot = Context.Environment.GetApplicationRoot ();
     var appRootExe = appRoot.Combine ("..").CombineWithFilePath (toolPath);
     if (FileExists (appRootExe))
         return appRootExe;
    throw new FileNotFoundException ("Unable to find tool: " + appRootExe); 
}
