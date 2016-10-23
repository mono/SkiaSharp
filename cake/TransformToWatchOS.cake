
var TransformToWatchOS = new Action<string> ((root) => {
    var glob = root + "/*.xcodeproj/project.pbxproj";
    ReplaceTextInFiles (glob, "SDKROOT = iphoneos;", "SDKROOT = watchos;");
    ReplaceTextInFiles (glob, "IPHONEOS_DEPLOYMENT_TARGET", "WATCHOS_DEPLOYMENT_TARGET");
    ReplaceTextInFiles (glob, "TARGETED_DEVICE_FAMILY = \"1,2\";", "TARGETED_DEVICE_FAMILY = 4;");
    ReplaceTextInFiles (glob, "\"CODE_SIGN_IDENTITY[sdk=iphoneos*]\" = \"iPhone Developer\";", "");
    // CODE_SIGN_IDENTITY = "iPhone Developer"; ==> APPLICATION_EXTENSION_API_ONLY = YES;
});
