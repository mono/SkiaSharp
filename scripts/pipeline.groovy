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

def createNode(label, platform, githubContext) {
    node(label) {
        stage("Checkout") {
            // clone and checkout repository
            checkout scm

            // get current commit sha
            commitHash = sh(script: "git rev-parse HEAD", returnStdout: true).trim()
            currentBuild.displayName = "${commitHash.substring(0, 7)}"

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

properties([
    compressBuildLog()
])

timestamps {
    createNode ("ubuntu-1604-amd64", "linux", "TEST - Build Native - Linux (Ubuntu 16.04 x64)")
    createNode ("ubuntu-1804-amd64", "linux", "TEST - Build Native - Linux (Ubuntu 18.04 x64)")
    createNode ("win-components", "windows", "TEST - Build Native - Windows")
    createNode ("components", "macos", "TEST - Build Native - macOS")
}