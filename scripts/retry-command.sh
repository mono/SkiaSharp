function retry()
{
    local Attempt=0
    local RetryCount=$1

    shift
    while true; do
        "$@" && break || {
            ((Attempt++))
            if [[ $Attempt -lt $RetryCount ]]; then
                echo "##vso[task.logissue type=warning] ($Attempt of $RetryCount) Script failed to execute, retrying..."
            else
                echo "##vso[task.logissue type=warning] ($Attempt of $RetryCount) Script failed to execute.">&2
                exit 1
            fi
        }
    done
}

retry $*
