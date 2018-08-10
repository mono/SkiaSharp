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

def cmd(script, returnStdout = false) {

}

def createNativeBuilder(platform, host, label) {
    return {
        githubContext = "Build Native - ${platform} on ${host}"

        node(label) {
            stage("Checkout") {
                echo 'test'
                ooss = System.properties['os.name']
                echo "test => {ooss}"

                if (platform.toLowerCase() == "windows") {
                    bat(script: "set")
                } else {
                    sh(script: "printenv")
                }

                // clone and checkout repository
                checkout scm

                // get current commit sha
                commitHash = sh(script: "git rev-parse HEAD", returnStdout: true).trim()

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

properties([
    compressBuildLog()
])

// run all the native builds
def nativeBuilders = [:]
nativeBuilders["linux"]             = createNativeBuilder("Linux",      "Linux",    "ubuntu-1604-amd64")
nativeBuilders["win32"]             = createNativeBuilder("Win32",      "Windows",  "win-components")
nativeBuilders["uwp"]               = createNativeBuilder("UWP",        "Windows",  "win-components")
nativeBuilders["android_windows"]   = createNativeBuilder("Android",    "Windows",  "win-components")
nativeBuilders["macos"]             = createNativeBuilder("macOS",      "macOS",    "components")
nativeBuilders["android_macos"]     = createNativeBuilder("Android",    "macOS",    "components")
nativeBuilders["ios"]               = createNativeBuilder("iOS",        "macOS",    "components")
parallel nativeBuilders

// run all the managed builds

// run the packaging
