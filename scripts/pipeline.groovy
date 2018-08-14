import groovy.transform.Field

@Field def isPr = (env.ghprbPullId && !env.ghprbPullId.empty ? true : false)
@Field def branchName = (isPr ? "pr" : env.BRANCH_NAME)
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
// Stages

node("ubuntu-1604-amd64") {
    stage("Prepare") {
        timestamps {
            checkout scm
            commitHash = cmdResult("git rev-parse HEAD").trim()
        }
    }

    stage("Native Builds") {
        parallel([
            failFast: true,

            // windows
            win32:              createNativeBuilder("Windows",    "Windows",  "components-windows"),
            uwp:                createNativeBuilder("UWP",        "Windows",  "components-windows"),
            android_windows:    createNativeBuilder("Android",    "Windows",  "components-windows"),
            tizen_windows:      createNativeBuilder("Tizen",      "Windows",  "components-windows"),

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
    }

    stage("Managed Builds") {
        parallel([
            failFast: true,

            windows: createManagedBuilder("Windows",    "components-windows"),
            macos:   createManagedBuilder("macOS",      "components"),
            linux:   createManagedBuilder("Linux",      "ubuntu-1604-amd64"),
        ])
    }

    stage("Packaging") {
        parallel([
            failFast: true,

            package: createPackagingBuilder(),
        ])
    }

    stage("Clean Up") {
        timestamps {

        }
    }
}

// ============================================================================
// Functions

def createNativeBuilder(platform, host, label) {
    def githubContext = "Build Native - ${platform} on ${host}"
    platform = platform.toLowerCase();
    host = host.toLowerCase();

    reportGitHubStatus(githubContext, "PENDING", "Building...")

    return {
        stage(githubContext) {
            node(label) {
                timestamps {
                    withEnv(customEnv[host] + ["NODE_LABEL=${label}"]) {
                        ws("${getWSRoot()}/native-${platform}") {
                            try {
                                checkout scm
                                cmd("git submodule update --init --recursive")

                                def pre = ""
                                if (host == "linux" && platform == "tizen") {
                                    pre = "./scripts/install-tizen.sh && "
                                }
                                bootstrapper("-t externals-${platform} -v normal", host, pre)

                                uploadBlobs("native-${platform}_${host}")

                                reportGitHubStatus(githubContext, "SUCCESS", "Build complete.")
                            } catch (Exception e) {
                                reportGitHubStatus(githubContext, "FAILURE", "Build failed.")
                                throw e
                            }
                        }
                    }
                }
            }
        }
    }
}

def createManagedBuilder(host, label) {
    def githubContext = "Build Managed - ${host}"
    host = host.toLowerCase();

    reportGitHubStatus(githubContext, "PENDING", "Building...")

    return {
        stage(githubContext) {
            node(label) {
                timestamps {
                    withEnv(customEnv[host] + ["NODE_LABEL=${label}"]) {
                        ws("${getWSRoot()}/managed-${host}") {
                            try {
                                checkout scm
                                downloadBlobs("native-*")

                                bootstrapper("-t everything -v normal --skipexternals=all", host, "")

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

                                uploadBlobs("managed-${host}")

                                reportGitHubStatus(githubContext, "SUCCESS", "Build complete.")
                            } catch (Exception e) {
                                reportGitHubStatus(githubContext, "FAILURE", "Build failed.")
                                throw e
                            }
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
    def label = "ubuntu-1604-amd64"

    reportGitHubStatus(githubContext, "PENDING", "Packing...")

    return {
        stage(githubContext) {
            node(label) {
                timestamps{
                    withEnv(customEnv[host] + ["NODE_LABEL=${label}"]) {
                        ws("${getWSRoot()}/packing-${host}") {
                            try {
                                checkout scm
                                downloadBlobs("managed-*");

                                bootstrapper("-t nuget-only -v normal", host, "")

                                uploadBlobs("packing-${host}")

                                reportGitHubStatus(githubContext, "SUCCESS", "Pack complete.")
                            } catch (Exception e) {
                                reportGitHubStatus(githubContext, "FAILURE", "Pack failed.")
                                throw e
                            }
                        }
                    }
                }
            }
        }
    }
}

def bootstrapper(args, host, pre) {
    host = host.toLowerCase()
    if (host == "linux") {
        chroot(
            chrootName: "${env.NODE_LABEL}-stable",
            command: "bash ${pre} ./bootstrapper.sh ${args}",
            additionalPackages: "${linuxPackages}")
    } else if (host == "macos") {
        sh("bash ${pre} ./bootstrapper.sh ${args}")
    } else if (host == "windows") {
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

def reportGitHubStatus(context, statusResult, statusResultMessage) {
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
            backref: env.BUILD_URL
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
