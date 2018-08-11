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
            def githubContext = "Build Native - ${platform} on ${host}"
            def cleanBranch = BRANCH_NAME.replace('/', '_').replace('\\', '_')
            def cleanPlatform = platform.toLowerCase()

            def wsRoot = "workspace"
            if (!isUnix()) {
                wsRoot = "C:/bld"
            }
            ws("${wsRoot}/SkiaSharp/${cleanBranch}/${cleanPlatform}") {
                stage("Checkout") {
                    var = cmdResult("set").trim()
                    echo "=====> ${var}"

                    // // clone and checkout repository
                    // checkout scm

                    // // get current commit sha
                    // commitHash = cmdResult("git rev-parse HEAD").trim()

                    // // let GitHub know we are building
                    // reportGitHubStatus(commitHash, githubContext, env.BUILD_URL, "PENDING", "Building...")
                }

                try {
                    stage("Build") {
                        // do the main build
                    }

                    stage("Upload") {
                        // do the upload
                    }
                } catch (Exception e) {
                    reportGitHubStatus(commitHash, githubContext, env.BUILD_URL, "FAILURE", "Build failed.")
                    throw e
                }
            }
        }
    }
}

properties([
    compressBuildLog()
])

// run all the native builds
def nativeBuilders = [:]
nativeBuilders["linux"]             = createNativeBuilder("Linux",      "Linux",    "ubuntu-1604-amd64")
nativeBuilders["win32"]             = createNativeBuilder("Win32",      "Windows",  "components-windows")
// nativeBuilders["uwp"]               = createNativeBuilder("UWP",        "Windows",  "components-windows")
// nativeBuilders["android_windows"]   = createNativeBuilder("Android",    "Windows",  "components-windows")
// nativeBuilders["macos"]             = createNativeBuilder("macOS",      "macOS",    "components")
// nativeBuilders["android_macos"]     = createNativeBuilder("Android",    "macOS",    "components")
// nativeBuilders["ios"]               = createNativeBuilder("iOS",        "macOS",    "components")
parallel nativeBuilders

// run all the managed builds

// run the packaging
