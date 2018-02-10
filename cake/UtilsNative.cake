
var RunLipo = new Action<DirectoryPath, FilePath, FilePath[]> ((directory, output, inputs) =>
{
    if (!IsRunningOnMac ()) {
        throw new InvalidOperationException ("lipo is only available on Unix.");
    }
    
    EnsureDirectoryExists (directory.CombineWithFilePath (output).GetDirectory ());

    var inputString = string.Join(" ", inputs.Select (i => string.Format ("\"{0}\"", i)));
    RunProcess ("lipo", new ProcessSettings {
        Arguments = string.Format("-create -output \"{0}\" {1}", output, inputString),
        WorkingDirectory = directory,
    });
});
