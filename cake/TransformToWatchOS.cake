
var TransformToWatchOS = new Action<string> ((root) => {
    var glob = root + "/*.xcodeproj/project.pbxproj";
    var files = GetFiles (glob);

    foreach (var file in files) {
        // load the file
        var text = System.IO.File.ReadAllText (file.ToString ());
        if (text.IndexOf ("SK_BUILD_FOR_WATCHOS") == -1) {
            // requirements for watchOS
            text = text.Replace ("SDKROOT = iphoneos;", "SDKROOT = watchos;");
            text = text.Replace ("IPHONEOS_DEPLOYMENT_TARGET", "WATCHOS_DEPLOYMENT_TARGET");
            text = text.Replace ("TARGETED_DEVICE_FAMILY = \"1,2\";", "TARGETED_DEVICE_FAMILY = 4;");
            text = text.Replace ("\"CODE_SIGN_IDENTITY[sdk=iphoneos*]\" = \"iPhone Developer\";", "");

            // update defines
            text = text.Replace ("SK_BUILD_FOR_IOS", "SK_BUILD_FOR_IOS, SK_BUILD_FOR_WATCHOS");
            // CODE_SIGN_IDENTITY = "iPhone Developer"; ==> APPLICATION_EXTENSION_API_ONLY = YES;

            // // CUSTOMIZATIONS
            // // disable inline assembly (not supported by bitcode)
            string dir = file.GetDirectory ().GetDirectoryName ();
            if (dir.IndexOf ("libwebp.xcodeproj") != -1) {
                text = text.Replace ("GCC_PREPROCESSOR_DEFINITIONS = (", "GCC_PREPROCESSOR_DEFINITIONS = ( WEBP_USE_INTRINSICS,");
            }

            // save the file
            System.IO.File.WriteAllText (file.ToString (), text);
        }
    }
});
