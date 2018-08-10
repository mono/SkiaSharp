def commitHash = null
def nativeBuilders = [:]

def reportGitHubStatus(commitHash, context, backref, statusResult, statusResultMessage) {
    step([
        $class: "GitHubCommitStatusSetter",
        commitShaSource: [$class: "ManuallyEnteredShaSource", sha: commitHash],
        contextSource: [$class: "ManuallyEnteredCommitContextSource", context: context],
        statusBackrefSource: [$class: "ManuallyEnteredBackrefSource", backref: backref],
        statusResultSource: [$class: "ConditionalStatusResultSource", results: [[$class: "AnyBuildResult", state: statusResult, message: statusResultMessage]]]
    ])
}

def createNativeBuilderNode(platform, host, label) {
    builderName = "${platform} on ${host}"
    githubContext = "Build Native - ${builderName}"

    nativeBuilders[builderName] = {
        node(label) {
            stage("Checkout") {
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
createNativeBuilderNode ("Linux", "Linux", "ubuntu-1604-amd64")
createNativeBuilderNode ("Win32", "Windows", "win-components")
createNativeBuilderNode ("UWP", "Windows", "win-components")
createNativeBuilderNode ("Android", "Windows", "win-components")
createNativeBuilderNode ("macOS", "macOS", "components")
createNativeBuilderNode ("Android", "macOS", "components")
createNativeBuilderNode ("iOS", "macOS", "components")
parallel nativeBuilders;

// run all the managed builds

// run the packaging
