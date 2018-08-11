def commitHash = null

def reportGitHubStatus(commitHash, context, backref, statusResult, statusResultMessage) {
    step([
        $class: "GitHubCommitStatusSetter",
        commitShaSource: [$class: "ManuallyEnteredShaSource", sha: commitHash],
        contextSource: [$class: "ManuallyEnteredCommitContextSource", context: context],
        statusBackrefSource: [$class: "ManuallyEnteredBackrefSource", backref: backref],
        statusResultSource: [$class: "ConditionalStatusResultSource", results: [[$class: "AnyBuildResult", state: statusResult, message: statusResultMessage]]]
    ])
}

def cmd(script) {
    if (isUnix()) {
        return sh(script)
    } else {
        return bat(script)
    }
}

def cmdResult(script) {
    if (isUnix()) {
        return sh(script: script, returnStdout: true)
    } else {
        return bat(script: script, returnStdout: true)
    }
}

def createNativeBuilder(platform, host, label) {
    return {
        node(label) {
            timestamps {
                def githubContext = "Build Native - ${platform} on ${host}"
                def cleanBranch = env.BRANCH_NAME.replace("/", "_").replace("\\", "_")
                def cleanPlatform = platform.toLowerCase()

                def wsRoot = "workspace"
                if (!isUnix()) {
                    wsRoot = "C:/bld"
                }
                ws("${wsRoot}/SkiaSharp/${cleanBranch}/${cleanPlatform}") {
                    stage("Checkout Native") {
                        // clone and checkout repository
                        checkout scm

                        // get current commit sha
                        commitHash = cmdResult("git rev-parse HEAD").trim()

                        // let GitHub know we are building
                        reportGitHubStatus(commitHash, githubContext, env.BUILD_URL, "PENDING", "Building...")
                    }

                    try {
                        stage("Build Native") {
                            if (host.toLowerCase() == "linux") {
                                chroot(
                                    chrootName: "${label}-stable",
                                    command: "bash ./bootstrapper.sh -t externals-${platform.toLowerCase()} -v normal",
                                    additionalPackages: "xvfb xauth libfontconfig1-dev libglu1-mesa-dev g++ mono-complete msbuild curl ca-certificates-mono unzip python git referenceassemblies-pcl dotnet-sdk-2.0.0 ttf-ancient-fonts openjdk-8-jdk zip gettext openvpn acl libxcb-render-util0 libv4l-0 libsdl1.2debian libxcb-image0 bridge-utils rpm2cpio libxcb-icccm4 libwebkitgtk-1.0-0 cpio")
                            } else if (host.toLowerCase() == "macos") {
                                sh("bash ./bootstrapper.sh -t externals-${platform.toLowerCase()} -v normal")
                            } else if (host.toLowerCase() == "windows") {
                                powershell(".\\bootstrapper.ps1 -t externals-${platform.toLowerCase()} -v normal")
                            } else {
                                throw new Exception("Unknown host platform: ${host}")
                            }
                        }

                        stage("Upload Native") {
                            fingerprint("output/**/*")
                            step([
                                $class: "WAStoragePublisher",
                                allowAnonymousAccess: true,
                                cleanUpContainer: false,
                                cntPubAccess: true,
                                containerName: "SkiaSharp-Public-Artifacts",
                                doNotFailIfArchivingReturnsNothing: false,
                                doNotUploadIndividualFiles: false,
                                doNotWaitForPreviousBuild: true,
                                excludeFilesPath: "",
                                filesPath: "output/**/*",
                                storageAccName: "credential for xamjenkinsartifact",
                                storageCredentialId: "fbd29020e8166fbede5518e038544343",
                                uploadArtifactsOnlyIfSuccessful: false,
                                uploadZips: false,
                                virtualPath: "ArtifactsFor-${env.BUILD_NUMBER}/${commitHash}/"
                            ])

                            reportGitHubStatus(commitHash, githubContext, env.BUILD_URL, "SUCCESS", "Build complete.")
                        }
                    } catch (Exception e) {
                        reportGitHubStatus(commitHash, githubContext, env.BUILD_URL, "FAILURE", "Build failed.")
                        throw e
                    }
                }
            }
        }
    }
}

properties([
    compressBuildLog()
])

echo "env.GIT_COMMIT: ${env.GIT_COMMIT}"
echo "env.GIT_BRANCH: ${env.GIT_BRANCH}"
echo "env.GIT_PREVIOUS_COMMIT: ${env.GIT_PREVIOUS_COMMIT}"
echo "env.GIT_PREVIOUS_SUCCESSFUL_COMMIT: ${env.GIT_PREVIOUS_SUCCESSFUL_COMMIT}"
echo "env.GIT_URL: ${env.GIT_URL}"
echo "env.CHANGE_ID: ${env.CHANGE_ID}"


// // run all the native builds
// parallel([
//     // // windows
//     // win32:              createNativeBuilder("Win32",      "Windows",  "components-windows"),
//     // uwp:                createNativeBuilder("UWP",        "Windows",  "components-windows"),
//     // android_windows:    createNativeBuilder("Android",    "Windows",  "components-windows"),
//     // tizen_windows:      createNativeBuilder("Tizen",      "Windows",  "components-windows"),

//     // macos
//     macos:              createNativeBuilder("macOS",      "macOS",    "components"),
//     ios:                createNativeBuilder("iOS",        "macOS",    "components"),
//     tvos:               createNativeBuilder("tvOS",       "macOS",    "components"),
//     watchos:            createNativeBuilder("watchOS",    "macOS",    "components"),
//     android_macos:      createNativeBuilder("Android",    "macOS",    "components"),
//     tizen_macos:        createNativeBuilder("Tizen",      "macOS",    "components"),

//     // linux
//     linux:              createNativeBuilder("Linux",      "Linux",    "ubuntu-1604-amd64"),
//     tizen_linux:        createNativeBuilder("Tizen",      "Linux",    "ubuntu-1604-amd64")
// ])

// run all the managed builds
// def managedBuilders = [:]
// nativeBuilders["macos"]             = createNativeBuilder("macOS",      "macOS",    "components")
// parallel managedBuilders

// run the packaging
