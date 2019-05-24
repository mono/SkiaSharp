function retry()
{
    local Attempt=0
    local RetryCount=$1

    shift
    while true; do
        "$@" && break || {
            if [[ $Attempt -lt $RetryCount ]]; then
                ((Attempt++))
                echo "[$Attempt of $RetryCount] Script failed to execute, retrying..."
            else
                echo "[$Attempt of $RetryCount] Script failed to execute.">&2
                exit 1
            fi
        }
    done
}

retry $*
