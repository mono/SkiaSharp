# Checkout Skia PR Feature - Status and Requirements

## Overview

The `checkout-skia.ps1` script is **currently disabled** in the Azure Pipelines configuration. This feature was designed to automatically checkout a specific skia PR branch when building SkiaSharp PRs.

## How It Should Work

When contributors create a SkiaSharp PR that requires changes in the skia submodule:

1. Contributor adds the required skia PR link to their PR description using the format:
   ```markdown
   **Required skia PR**
   https://github.com/mono/skia/pull/123
   ```

2. The build script would:
   - Read the SkiaSharp PR description via GitHub API
   - Extract the skia PR number using regex
   - Checkout that specific skia PR into the `externals/skia` submodule
   - Build SkiaSharp with the custom skia version

## Why It's Disabled

The feature requires GitHub API access to fetch the PR description, but:

1. **GitHub blocks unauthenticated API requests** - Cannot make API calls without authentication
2. **No GitHub token available** - The original implementation used `$(GitHub.Token.PublicAccess)` which was blocked on public CI
3. **Service connection token not accessible** - While Azure Pipelines authenticates with GitHub for code checkout, that token is managed internally and not exposed to custom scripts
4. **Cannot configure pipeline variables** - In this environment, pipeline variables/secrets cannot be added

### Azure Pipelines GitHub Authentication

When Azure Pipelines checks out code from GitHub:
- **For private repos**: Uses service connection credentials (OAuth/PAT/GitHub App)
- **For public repos**: May use unauthenticated access or service connection
- **Documentation**: [Azure Pipelines GitHub integration](https://learn.microsoft.com/en-us/azure/devops/pipelines/repos/github)

The authentication used for checkout is handled by Azure Pipelines internally and is **not accessible to PowerShell scripts**.

### Why System.AccessToken Doesn't Work

`System.AccessToken` is documented here: [Azure Pipelines predefined variables](https://learn.microsoft.com/en-us/azure/devops/pipelines/build/variables#systemaccesstoken)

- **Purpose**: Authenticate with Azure DevOps REST APIs, not GitHub
- **Scope**: Limited to the Azure DevOps project/collection
- **Cannot**: Access GitHub API

## Potential Solutions (Require Changes Outside This Repository)

### Option 1: GitHub App with Elevated Permissions
If the repository connection uses a GitHub App with API permissions, Azure Pipelines can provide a token through resources:

```yaml
resources:
  repositories:
  - repository: self
    type: github
    endpoint: MyGitHubConnection
    
steps:
- script: |
    # Token available as $(resources.repositories.self.accessToken)
```

**Documentation**: [Azure Pipelines resources](https://learn.microsoft.com/en-us/azure/devops/pipelines/yaml-schema/resources-repositories-repository)

**Limitation**: Requires GitHub App configuration with appropriate permissions.

### Option 2: Configure Pipeline Variable with GitHub Token
Create a pipeline variable containing a GitHub Personal Access Token:

1. Create a GitHub PAT with `repo` scope
2. Add as pipeline variable: `GITHUB_TOKEN`
3. Modify YAML to pass to script

**Documentation**: [Azure Pipelines variables](https://learn.microsoft.com/en-us/azure/devops/pipelines/process/variables)

**Limitation**: Requires ability to configure pipeline variables (not available in current environment).

### Option 3: Azure Key Vault Integration
Store GitHub token in Azure Key Vault and reference it in the pipeline.

**Documentation**: [Use Azure Key Vault secrets](https://learn.microsoft.com/en-us/azure/devops/pipelines/release/azure-key-vault)

**Limitation**: Requires Azure Key Vault setup and permissions.

## Current Workaround

Contributors must manually ensure their local skia submodule is on the correct PR branch before pushing changes. The build will use whatever version is committed in the submodule.

## Technical Details

### Original Script Logic

```powershell
# From scripts/checkout-skia.ps1 (currently commented out in YAML)
Param([string] $GitHubToken = '')

# Fetch PR info from GitHub API
$json = Invoke-RestMethod `
    -Uri "https://api.github.com/repos/mono/SkiaSharp/pulls/$PR_NUMBER" `
    -Headers @{ Authorization = "token $GitHubToken" }

# Extract skia PR number from description
$regex = '\*\*Required\ skia\ PR\*\*[\\rn\s-]+https?\://github\.com/mono/skia/pull/(\d+)'
$match = [regex]::Match($json.body, $regex)

# Checkout skia PR
cd externals/skia
git fetch origin +refs/pull/$skiaPR/merge:refs/remotes/pull/$skiaPR/merge
git checkout refs/remotes/pull/$skiaPR/merge
```

### Pipeline Configuration

In `scripts/azure-templates-jobs-bootstrapper.yml`, the step is commented out:

```yaml
# # checkout required skia PR
# - pwsh: .\scripts\checkout-skia.ps1 -GitHubToken $(GitHub.Token.PublicAccess)
#   displayName: Checkout required skia PR
#   condition: eq(variables['Build.Reason'], 'PullRequest')
```

## References

- [GitHub REST API - Pull Requests](https://docs.github.com/en/rest/pulls/pulls#get-a-pull-request)
- [Azure Pipelines and GitHub](https://learn.microsoft.com/en-us/azure/devops/pipelines/repos/github)
- [Azure Pipelines Variables](https://learn.microsoft.com/en-us/azure/devops/pipelines/build/variables)
- [Service Connections in Azure Pipelines](https://learn.microsoft.com/en-us/azure/devops/pipelines/library/service-endpoints)
