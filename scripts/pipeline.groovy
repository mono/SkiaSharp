import groovy.transform.Field

@Field def verbosity = "minimal"
@Field def isPr = false
@Field def branchName = null
@Field def commitHash = null
@Field def githubStatusSha = null

@Field def minimalLinuxPackages = "curl mono-complete msbuild"
@Field def nativeLinuxPackages = "python git libfontconfig1-dev"
@Field def nativeTizenPackages = "python git openjdk-8-jdk zip libxcb-xfixes0 libxcb-render-util0 libwebkitgtk-1.0-0 libxcb-image0 acl libsdl1.2debian libv4l-0 libxcb-randr0 libxcb-shape0 libxcb-icccm4 libsm6 gettext rpm2cpio cpio bridge-utils openvpn"
@Field def managedLinuxPackages = "dotnet-sdk-2.0.0 ttf-ancient-fonts"

@Field def nativeStashes = []
@Field def managedStashes = []

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

            isPr = env.ghprbPullId && !env.ghprbPullId.empty
            branchName = isPr ? "pr" : env.BRANCH_NAME
            githubStatusSha = isPr ? env.ghprbActualCommit : commitHash

            echo "Building SHA1: ${commitHash}..."
            echo " - PR: ${isPr}"
            echo " - Branch Name: ${branchName}"
            echo " - GitHub Status SHA1: ${githubStatusSha}"
        }
    }

    stage("Native Builds") {
        parallel([
            // windows
            win32:              createNativeBuilder("Windows",    "Windows",  "components-windows",     ""),
            // uwp:                createNativeBuilder("UWP",        "Windows",  "components-windows",     ""),
            // android_windows:    createNativeBuilder("Android",    "Windows",  "components-windows",     ""),
            // tizen_windows:      createNativeBuilder("Tizen",      "Windows",  "components-windows",     ""),

            // // macos
            // macos:              createNativeBuilder("macOS",      "macOS",    "components",             ""),
            // ios:                createNativeBuilder("iOS",        "macOS",    "components",             ""),
            // tvos:               createNativeBuilder("tvOS",       "macOS",    "components",             ""),
            // watchos:            createNativeBuilder("watchOS",    "macOS",    "components",             ""),
            // android_macos:      createNativeBuilder("Android",    "macOS",    "components",             ""),
            // tizen_macos:        createNativeBuilder("Tizen",      "macOS",    "components",             ""),

            // // linux
            linux:              createNativeBuilder("Linux",      "Linux",    "ubuntu-1604-amd64",      nativeLinuxPackages),
            // tizen_linux:        createNativeBuilder("Tizen",      "Linux",    "ubuntu-1604-amd64",      nativeTizenPackages),
        ])
    }

    stage("Managed Builds") {
        parallel([
            windows: createManagedBuilder("Windows",    "components-windows",   ""),
            // macos:   createManagedBuilder("macOS",      "components",           ""),
            linux:   createManagedBuilder("Linux",      "ubuntu-1604-amd64",    managedLinuxPackages),
        ])
    }

    stage("Packaging") {
        parallel([
            package: createPackagingBuilder(),
        ])
    }

    stage("Clean Up") {
        timestamps {
            cleanWs()
        }
    }
}

// ============================================================================
// Functions

def createNativeBuilder(platform, host, label, additionalPackages) {
    def githubContext = "Build Native - ${platform} on ${host}"
    platform = platform.toLowerCase()
    host = host.toLowerCase()

    reportGitHubStatus(githubContext, "PENDING", "Building...")

    return {
        stage(githubContext) {
            node(label) {
                timestamps {
                    withEnv(customEnv[host] + ["NODE_LABEL=${label}"]) {
                        ws("${getWSRoot()}/native-${platform}") {

                            touch("output/" + platform + "-native.txt")

                            def stashName = "${platform}_${host}"
                            nativeStashes.push(stashName)
                            stash(
                                name: stashName,
                                includes: "output/**/*",
                                allowEmpty: false
                            )

                            // try {
                            //     checkout scm
                            //     cmd("git submodule update --init --recursive")

                            //     def pre = ""
                            //     if (host == "linux" && platform == "tizen") {
                            //         pre = "./scripts/install-tizen.sh && "
                            //     }
                            //     bootstrapper("-t externals-${platform} -v ${verbosity}", host, pre, additionalPackages)

                            //     uploadBlobs("native-${platform}_${host}")

                            //     cleanWs()
                            //     reportGitHubStatus(githubContext, "SUCCESS", "Build complete.")
                            // } catch (Exception e) {
                            //     reportGitHubStatus(githubContext, "FAILURE", "Build failed.")
                            //     throw e
                            // }
                        }
                    }
                }
            }
        }
    }
}

def createManagedBuilder(host, label, additionalPackages) {
    def githubContext = "Build Managed - ${host}"
    host = host.toLowerCase()

    reportGitHubStatus(githubContext, "PENDING", "Building...")

    return {
        stage(githubContext) {
            node(label) {
                timestamps {
                    withEnv(customEnv[host] + ["NODE_LABEL=${label}"]) {
                        ws("${getWSRoot()}/managed-${host}") {


                            for (stashName in nativeStashes) {
                                unstash(stashName)
                            }

                            touch("output/" + host + "-managed.txt")

                            def stashName = "${host}"
                            managedStashes.push(stashName)
                            stash(
                                name: stashName,
                                includes: "output/**/*",
                                allowEmpty: false
                            )


                            // archiveArtifacts("**/*")

                            // try {
                            //     checkout scm
                            //     downloadBlobs("native-*")

                            //     bootstrapper("-t everything -v ${verbosity} --skipexternals=all", host, "", additionalPackages)

                            //     step([
                            //         $class: "XUnitPublisher",
                            //         testTimeMargin: "3000",
                            //         thresholdMode: 1,
                            //         thresholds: [[
                            //             $class: "FailedThreshold",
                            //             failureNewThreshold: "0",
                            //             failureThreshold: "0",
                            //             unstableNewThreshold: "0",
                            //             unstableThreshold: "0"
                            //         ]],
                            //         tools: [[
                            //             $class: "XUnitDotNetTestType",
                            //             deleteOutputFiles: true,
                            //             failIfNotNew: true,
                            //             pattern: "output/tests/**/TestResult.xml",
                            //             skipNoTestFiles: false,
                            //             stopProcessingIfError: true
                            //         ]]
                            //     ])

                            //     uploadBlobs("managed-${host}")

                            //     cleanWs()
                            //     reportGitHubStatus(githubContext, "SUCCESS", "Build complete.")
                            // } catch (Exception e) {
                            //     reportGitHubStatus(githubContext, "FAILURE", "Build failed.")
                            //     throw e
                            // }
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


                            for (stashName in managedStashes) {
                                unstash(stashName)
                            }

                            touch("output/package.txt")

                            uploadBlobs("packing-${host}")

                            // try {
                            //     checkout scm
                            //     downloadBlobs("managed-*")

                            //     bootstrapper("-t nuget-only -v ${verbosity}", host, "", "")

                            //     uploadBlobs("packing-${host}")

                            //     cleanWs()
                            //     reportGitHubStatus(githubContext, "SUCCESS", "Pack complete.")
                            // } catch (Exception e) {
                            //     reportGitHubStatus(githubContext, "FAILURE", "Pack failed.")
                            //     throw e
                            // }
                        }
                    }
                }
            }
        }
    }
}

def bootstrapper(args, host, pre, additionalPackages) {
    host = host.toLowerCase()
    if (host == "linux") {
        chroot(
            chrootName: "${env.NODE_LABEL}-stable",
            command: "bash ${pre} ./bootstrapper.sh ${args}",
            additionalPackages: "${minimalLinuxPackages} ${additionalPackages}")
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
            sha: githubStatusSha
        ],
        contextSource: [
            $class: "ManuallyEnteredCommitContextSource",
            context: context + (isPr ? " (PR)" : "")
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
    def cleanBranch = branchName.replace("/", "_").replace("\\", "_")
    def wsRoot = isUnix() ? "workspace" : "C:/bld"
    return "${wsRoot}/SkiaSharp/${cleanBranch}"
}
