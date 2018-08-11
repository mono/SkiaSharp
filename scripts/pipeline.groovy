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

def getWSRoot() {
    cleanBranch = env.BRANCH_NAME.replace("/", "_").replace("\\", "_")
    wsRoot = "workspace"
    if (!isUnix()) {
        wsRoot = "C:/bld"
    }
    return "${wsRoot}/SkiaSharp/${cleanBranch}"
}

def createNativeBuilder(platform, host, label) {
    githubContext = "Build Native - ${platform} on ${host}"

    return {
        node(label) {
            timestamps {
                ws("${getWSRoot()}/Native-${platform.toLowerCase()}") {
                    try {
                        stage("Setup Native") {
                            reportGitHubStatus(commitHash, githubContext, env.BUILD_URL, "PENDING", "Building...")
                        }

                        stage("Checkout Native") {
                            // checkout scm
                            // cmd("git submodule update --init --recursive")
                        }

                        stage("Build Native") {
                            touch "output/native/${host}/${platform}/dummy.txt"
                            // target = "externals-${platform.toLowerCase()}"
                            // verbosity = "normal"

                            // if (host.toLowerCase() == "linux") {
                            //     install_tizen = ""
                            //     if (platform.toLowerCase() == "tizen") {
                            //         install_tizen = "./scripts/install-tizen.sh && "
                            //     }
                            //     chroot(
                            //         chrootName: "${label}-stable",
                            //         command: "bash ${install_tizen} ./bootstrapper.sh -t ${target} -v ${verbosity}",
                            //         additionalPackages: "xvfb xauth libfontconfig1-dev libglu1-mesa-dev g++ mono-complete msbuild curl ca-certificates-mono unzip python git referenceassemblies-pcl dotnet-sdk-2.0.0 ttf-ancient-fonts openjdk-8-jdk zip gettext openvpn acl libxcb-render-util0 libv4l-0 libsdl1.2debian libxcb-image0 bridge-utils rpm2cpio libxcb-icccm4 libwebkitgtk-1.0-0 cpio")
                            // } else if (host.toLowerCase() == "macos") {
                            //     sh("bash ./bootstrapper.sh -t ${target} -v ${verbosity}")
                            // } else if (host.toLowerCase() == "windows") {
                            //     powershell(".\\bootstrapper.ps1 -t ${target} -v ${verbosity}")
                            // } else {
                            //     throw new Exception("Unknown host platform: ${host}")
                            // }
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
                                virtualPath: "ArtifactsFor-${env.BUILD_NUMBER}/${commitHash}/${platform.toLowerCase()}_${host.toLowerCase()}"
                            ])
                        }

                        stage("Teardown Native") {
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

def createManagedBuilder(host, label) {
    githubContext = "Build Managed - ${host}"

    return {
        node(label) {
            timestamps {
                ws("${getWSRoot()}/Managed-${host.toLowerCase()}") {
                    try {
                        stage("Setup Managed") {
                            reportGitHubStatus(commitHash, githubContext, env.BUILD_URL, "PENDING", "Building...")
                        }

                        stage("Checkout Managed") {
                            // checkout scm
                        }

                        stage("Download Native") {
                            step([
                                $class: "AzureStorageBuilder",
                                downloadType: [
                                    value: "container",
                                    containerName: "SkiaSharp-Public-Artifacts",
                                ],
                                includeFilesPattern: "ArtifactsFor-${env.BUILD_NUMBER}/${commitHash}/**/*",
                                excludeFilesPattern: "",
                                downloadDirLoc: "",
                                flattenDirectories: false,
                                includeArchiveZips: false,
                                strAccName: "credential for xamjenkinsartifact",
                                storageCredentialId: "fbd29020e8166fbede5518e038544343"

                            ])
                        }

                        stage("Build Managed") {
                            touch "output/managed/${host}/dummy.txt"
                            // target = "Everything"
                            // verbosity = "normal"

                            // if (host.toLowerCase() == "linux") {
                            //     chroot(
                            //         chrootName: "${label}-stable",
                            //         command: "bash ./bootstrapper.sh -t ${target} -v ${verbosity}",
                            //         additionalPackages: "xvfb xauth libfontconfig1-dev libglu1-mesa-dev g++ mono-complete msbuild curl ca-certificates-mono unzip python git referenceassemblies-pcl dotnet-sdk-2.0.0 ttf-ancient-fonts openjdk-8-jdk zip gettext openvpn acl libxcb-render-util0 libv4l-0 libsdl1.2debian libxcb-image0 bridge-utils rpm2cpio libxcb-icccm4 libwebkitgtk-1.0-0 cpio")
                            // } else if (host.toLowerCase() == "macos") {
                            //     sh("bash ./bootstrapper.sh -t ${target} -v ${verbosity}")
                            // } else if (host.toLowerCase() == "windows") {
                            //     powershell(".\\bootstrapper.ps1 -t ${target} -v ${verbosity}")
                            // } else {
                            //     throw new Exception("Unknown host platform: ${host}")
                            // }
                        }

                        stage("Upload Managed") {
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
                                virtualPath: "ArtifactsFor-${env.BUILD_NUMBER}/${commitHash}/${host.toLowerCase()}"
                            ])
                        }

                        stage("Teardown Managed") {
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

// prepare the process
node("ubuntu-1604-amd64") {
    timestamps {
        stage("Setup") {
            checkout scm
            commitHash = cmdResult("git rev-parse HEAD").trim()
        }
    }
}

// run all the native builds
parallel([
    // // windows
    // win32:              createNativeBuilder("Win32",      "Windows",  "components-windows"),
    // uwp:                createNativeBuilder("UWP",        "Windows",  "components-windows"),
    // android_windows:    createNativeBuilder("Android",    "Windows",  "components-windows"),
    // tizen_windows:      createNativeBuilder("Tizen",      "Windows",  "components-windows"),

    // // macos
    // macos:              createNativeBuilder("macOS",      "macOS",    "components"),
    // ios:                createNativeBuilder("iOS",        "macOS",    "components"),
    // tvos:               createNativeBuilder("tvOS",       "macOS",    "components"),
    // watchos:            createNativeBuilder("watchOS",    "macOS",    "components"),
    // android_macos:      createNativeBuilder("Android",    "macOS",    "components"),
    // tizen_macos:        createNativeBuilder("Tizen",      "macOS",    "components"),

    // linux
    linux:              createNativeBuilder("Linux",      "Linux",    "ubuntu-1604-amd64"),
    tizen_linux:        createNativeBuilder("Tizen",      "Linux",    "ubuntu-1604-amd64")
])

// run all the managed builds
parallel ([
    linux: createManagedBuilder("Linux",    "ubuntu-1604-amd64")
])

// run the packaging
node("ubuntu-1604-amd64") {
    timestamps {
        stage("Packing") {
            // checkout scm
        }
    }
}

// clean up
node("ubuntu-1604-amd64") {
    timestamps {
        stage("Teardown") {
            // checkout scm
        }
    }
}
