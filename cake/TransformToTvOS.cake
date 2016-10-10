
var TransformToTvOS = new Action<string> ((root) => {
    var glob = root + "/*.xcodeproj/project.pbxproj";
    ReplaceTextInFiles (glob, "SDKROOT = iphoneos;", "SDKROOT = appletvos;");
    ReplaceTextInFiles (glob, "IPHONEOS_DEPLOYMENT_TARGET", "TVOS_DEPLOYMENT_TARGET");
    ReplaceTextInFiles (glob, "TARGETED_DEVICE_FAMILY = \"1,2\";", "TARGETED_DEVICE_FAMILY = 3;");
    ReplaceTextInFiles (glob, "\"CODE_SIGN_IDENTITY[sdk=iphoneos*]\" = \"iPhone Developer\";", "");
});
