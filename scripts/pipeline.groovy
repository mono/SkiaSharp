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
            def cleanBranch = env.BRANCH_NAME.replace('/', '_').replace('\\', '_')
            def cleanPlatform = platform.toLowerCase()

            def wsRoot = "workspace"
            if (!isUnix()) {
                wsRoot = "C:/bld"
            }
            ws("${wsRoot}/SkiaSharp/${cleanBranch}/${cleanPlatform}") {
                stage("Checkout") {
                    // clone and checkout repository
                    checkout scm

                    // get current commit sha
                    commitHash = cmdResult("git rev-parse HEAD").trim()

                    // let GitHub know we are building
                    reportGitHubStatus(commitHash, githubContext, env.BUILD_URL, "PENDING", "Building...")
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

reportGitHubStatus(env.GIT_COMMIT, "Pipeline", env.BUILD_URL, "PENDING", "Building...")

// run all the native builds
def nativeBuilders = [:]
// nativeBuilders["win32"]             = createNativeBuilder("Win32",      "Windows",  "components-windows")
// nativeBuilders["uwp"]               = createNativeBuilder("UWP",        "Windows",  "components-windows")
// nativeBuilders["android_windows"]   = createNativeBuilder("Android",    "Windows",  "components-windows")
// nativeBuilders["tizen_windows"]     = createNativeBuilder("Tizen",      "Windows",  "components-windows")
nativeBuilders["macos"]             = createNativeBuilder("macOS",      "macOS",    "components")
nativeBuilders["ios"]               = createNativeBuilder("iOS",        "macOS",    "components")
nativeBuilders["tvos"]              = createNativeBuilder("tvOS",       "macOS",    "components")
nativeBuilders["watchos"]           = createNativeBuilder("watchOS",    "macOS",    "components")
nativeBuilders["android_macos"]     = createNativeBuilder("Android",    "macOS",    "components")
nativeBuilders["tizen_macos"]       = createNativeBuilder("Tizen",      "macOS",    "components")
nativeBuilders["linux"]             = createNativeBuilder("Linux",      "Linux",    "ubuntu-1604-amd64")
nativeBuilders["tizen_linux"]       = createNativeBuilder("Tizen",      "Linux",    "ubuntu-1604-amd64")
parallel nativeBuilders

// run all the managed builds

// run the packaging

reportGitHubStatus(env.GIT_COMMIT, "Pipeline", env.BUILD_URL, "FAILURE", "Build failed.")
