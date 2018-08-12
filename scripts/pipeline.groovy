import groovy.transform.Field

@Field def commitHash = null
@Field def linuxPackages = "xvfb xauth libfontconfig1-dev libglu1-mesa-dev g++ mono-complete msbuild curl ca-certificates-mono unzip python git referenceassemblies-pcl dotnet-sdk-2.0.0 ttf-ancient-fonts openjdk-8-jdk zip gettext openvpn acl libxcb-render-util0 libv4l-0 libsdl1.2debian libxcb-image0 bridge-utils rpm2cpio libxcb-icccm4 libwebkitgtk-1.0-0 cpio"

@Field def customEnv = [
    "windows": [
        "TIZEN_STUDIO_HOME=C:\\Tizen",
        "ANDROID_NDK_ROOT=C:\\ProgramData\\Microsoft\\AndroidNDK64\\android-ndk-r14b"
    ],
    "macos": [
        "ANDROID_NDK_HOME=/Users/builder/Library/Developer/Xamarin/android-ndk",
    ],
    "linux": [
    ]
]

properties([
    compressBuildLog()
])

// ============================================================================
// Prepare

node {
    timestamps {
        stage("Setup") {
            checkout scm
            commitHash = cmdResult("git rev-parse HEAD").trim()
        }
    }
}

// ============================================================================
// Native Builds

parallel([
    // // windows
    // win32:              createNativeBuilder("Win32",      "Windows",  "components-windows"),
    // uwp:                createNativeBuilder("UWP",        "Windows",  "components-windows"),
    // android_windows:    createNativeBuilder("Android",    "Windows",  "components-windows"),
    // tizen_windows:      createNativeBuilder("Tizen",      "Windows",  "components-windows"),

    // macos
    macos:              createNativeBuilder("macOS",      "macOS",    "components"),
    ios:                createNativeBuilder("iOS",        "macOS",    "components"),
    tvos:               createNativeBuilder("tvOS",       "macOS",    "components"),
    watchos:            createNativeBuilder("watchOS",    "macOS",    "components"),
    android_macos:      createNativeBuilder("Android",    "macOS",    "components"),
    tizen_macos:        createNativeBuilder("Tizen",      "macOS",    "components"),

    // linux
    linux:              createNativeBuilder("Linux",      "Linux",    "ubuntu-1604-amd64"),
    tizen_linux:        createNativeBuilder("Tizen",      "Linux",    "ubuntu-1604-amd64"),
])

// ============================================================================
// Managed Builds

parallel ([
    // windows: createManagedBuilder("Windows",    "components-windows"),
    macos:   createManagedBuilder("macOS",      "components"),
    linux:   createManagedBuilder("Linux",      "ubuntu-1604-amd64"),
])

// ============================================================================
// Packaging

parallel([
    package: createPackagingBuilder()
])

// ============================================================================
// Clean Up

node {
    timestamps {
        stage("Teardown") {

        }
    }
}

// ============================================================================
// Functions

def createNativeBuilder(platform, host, label) {
    def githubContext = "Build Native - ${platform} on ${host}"

    return {
        node(label) {
            timestamps {
                withEnv(customEnv[host]) {
                    ws("${getWSRoot()}/native-${platform.toLowerCase()}") {
                        try {
                            stage("Begin Native") {
                                reportGitHubStatus(commitHash, githubContext, env.BUILD_URL, "PENDING", "Building...")

                                checkout scm
                                cmd("git submodule update --init --recursive")
                            }

                            stage("Build Native") {
                                def pre = ""
                                if (host.toLowerCase() == "linux" && platform.toLowerCase() == "tizen") {
                                    pre = "./scripts/install-tizen.sh && "
                                }
                                bootstrapper("-t externals-${platform.toLowerCase()} -v normal", host, pre)
                            }

                            stage("End Native") {
                                uploadBlobs("native-${platform.toLowerCase()}_${host.toLowerCase()}")

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
}

def createManagedBuilder(host, label) {
    def githubContext = "Build Managed - ${host}"

    return {
        node(label) {
            timestamps {
                withEnv(customEnv[host]) {
                    ws("${getWSRoot()}/managed-${host.toLowerCase()}") {
                        try {
                            stage("Begin Managed") {
                                reportGitHubStatus(commitHash, githubContext, env.BUILD_URL, "PENDING", "Building...")

                                checkout scm
                                downloadBlobs("native-*")
                            }

                            stage("Build Managed") {
                                bootstrapper("-t everything -v normal --skipexternals=all", host)
                            }

                            stage("Test Managed") {
                                step([
                                    $class: "XUnitBuilder",
                                    testTimeMargin: "3000",
                                    thresholdMode: 1,
                                    thresholds: [[
                                        $class: "FailedThreshold",
                                        failureNewThreshold: "0",
                                        failureThreshold: "0",
                                        unstableNewThreshold: "0",
                                        unstableThreshold: "0"
                                    ], [
                                        $class: "SkippedThreshold",
                                        failureNewThreshold: "",
                                        failureThreshold: "",
                                        unstableNewThreshold: "",
                                        unstableThreshold: ""
                                    ]],
                                    tools: [[
                                        $class: "NUnitJunitHudsonTestType",
                                        deleteOutputFiles: true,
                                        failIfNotNew: true,
                                        pattern: "output/tests/*/TestResult.xml",
                                        skipNoTestFiles: false,
                                        stopProcessingIfError: true
                                    ]]
                                ])
                            }

                            stage("End Managed") {
                                uploadBlobs("managed-${host.toLowerCase()}")

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
}

def createPackagingBuilder() {
    def githubContext = "Packing"
    def host = "linux"

    return {
        node("ubuntu-1604-amd64") {
            timestamps{
                withEnv(customEnv[host]) {
                    ws("${getWSRoot()}/package-${platform.toLowerCase()}") {
                        try {
                            reportGitHubStatus(commitHash, githubContext, env.BUILD_URL, "PENDING", "Packing...")

                            checkout scm
                            downloadBlobs("managed-*");

                            bootstrapper("-t nuget-only -v normal", host)

                            uploadBlobs("package-${host.toLowerCase()}")

                            reportGitHubStatus(commitHash, githubContext, env.BUILD_URL, "SUCCESS", "Pack complete.")
                        } catch (Exception e) {
                            reportGitHubStatus(commitHash, githubContext, env.BUILD_URL, "FAILURE", "Pack failed.")
                            throw e
                        }
                    }
                }
            }
        }
    }
}

def bootstrapper(args, host, pre) {
    if (host.toLowerCase() == "linux") {
        chroot(
            chrootName: "${env.NODE_ID}-stable",
            command: "bash ${pre} ./bootstrapper.sh ${args}",
            additionalPackages: "${linuxPackages}")
    } else if (host.toLowerCase() == "macos") {
        sh("bash ${pre} ./bootstrapper.sh ${args}")
    } else if (host.toLowerCase() == "windows") {
        powershell("${pre} .\\bootstrapper.ps1 ${args}")
    } else {
        throw new Exception("Unknown host platform: ${host}")
    }
}

def uploadBlobs(blobs) {
    fingerprint("output/**/*")
    step([
        $class: "WAStoragePublisher",
        allowAnonymousAccess: true,
        cleanUpContainer: false,
        cntPubAccess: true,
        containerName: "skiasharp-public-artifacts",
        doNotFailIfArchivingReturnsNothing: false,
        doNotUploadIndividualFiles: false,
        doNotWaitForPreviousBuild: true,
        excludeFilesPath: "",
        filesPath: "output/**/*",
        storageAccName: "credential for xamjenkinsartifact",
        storageCredentialId: "fbd29020e8166fbede5518e038544343",
        uploadArtifactsOnlyIfSuccessful: false,
        uploadZips: false,
        virtualPath: "ArtifactsFor-${env.BUILD_NUMBER}/${commitHash}/${blobs}/",
    ])
}

def downloadBlobs(blobs) {
    step([
        $class: "AzureStorageBuilder",
        downloadType: [
            value: "container",
            containerName: "skiasharp-public-artifacts",
        ],
        includeFilesPattern: "ArtifactsFor-${env.BUILD_NUMBER}/${commitHash}/${blobs}/**/*",
        excludeFilesPattern: "",
        downloadDirLoc: "",
        flattenDirectories: false,
        includeArchiveZips: false,
        strAccName: "credential for xamjenkinsartifact",
        storageCredentialId: "fbd29020e8166fbede5518e038544343",
    ])
    if (isUnix()) {
        sh("cp -rf ArtifactsFor-${env.BUILD_NUMBER}/${commitHash}/*/* .")
        sh("rm -rf ArtifactsFor-*")
    } else {
        powershell("copy -recurse -force ArtifactsFor-${env.BUILD_NUMBER}/${commitHash}/*/* .")
        powershell("del -recurse -force ArtifactsFor-*")
    }
}

def reportGitHubStatus(commitHash, context, backref, statusResult, statusResultMessage) {
    step([
        $class: "GitHubCommitStatusSetter",
        commitShaSource: [
            $class: "ManuallyEnteredShaSource",
            sha: commitHash
        ],
        contextSource: [
            $class: "ManuallyEnteredCommitContextSource",
            context: context
        ],
        statusBackrefSource: [
            $class: "ManuallyEnteredBackrefSource",
            backref: backref
        ],
        statusResultSource: [
            $class: "ConditionalStatusResultSource",
            results: [[
                $class: "AnyBuildResult",
                state: statusResult,
                message: statusResultMessage
            ]]
        ]
    ])
}

def cmd(script) {
    if (isUnix()) {
        return sh(script)
    } else {
        return powershell(script)
    }
}

def cmdResult(script) {
    if (isUnix()) {
        return sh(script: script, returnStdout: true)
    } else {
        return powershell(script: script, returnStdout: true)
    }
}

def getWSRoot() {
    def cleanBranch = env.BRANCH_NAME.replace("/", "_").replace("\\", "_")
    def wsRoot = (isUnix()) ? "workspace" : "C:/bld"
    return "${wsRoot}/SkiaSharp/${cleanBranch}"
}
