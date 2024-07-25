# Contributing

Thank you for your interest in contributing to the Xamarin documentation!

This page covers the basic process for updating content in the [Xamarin documentation](https://learn.microsoft.com/xamarin).

- [Contributor License Agreement](LICENSE)

## Process for contributing

### Small changes & edits

To make corrections and small updates - you can click the **Edit** button on any page and use the GitHub website to contribute, or follow these steps:

1. Fork the `MicrosoftDocs/xamarin-docs` repo.

2. Create a `branch` for your changes.

3. Write your content. Refer to the [template](contributing-guidelines/template.md) and [style guide](contributing-guidelines/voice-tone.md).

4. Submit a Pull Request (PR) from your branch to `MicrosoftDocs/xamarin-docs/live`.

5. Make any necessary updates to your branch as discussed with the team via the PR.

6. The maintainers will merge your PR once feedback has been applied and your change looks good. It will be published soon after.

> [!NOTE]
> If your PR is addressing an existing issue, add the `Fixes #Issue_Number` keyword to the commit message or PR description, so the issue can be automatically closed when the PR is merged. For more information, see [Closing issues via commit messages](https://help.github.com/articles/closing-issues-via-commit-messages/).

### Big changes or new content

For large contributions and new content, [open an issue](https://github.com/MicrosoftDocs/xamarin-docs/issues) describing the article you wish to write and how it relates to existing content. The content inside the docs folder is organized into sections that are organized by product area (e.g. android and ios). Try to determine the correct folder for your new content. 

**Get feedback on your proposal via the issue before starting to write.**

If it's a new topic, you can use the [template file](../contributing-guidelines/template.md) as your starting point. It contains the writing guidelines and also explains the metadata required for each article, such as author information.

For images and other static resources, add them to the subfolder called **\<mypage>-images**. If you are creating a new folder for content, add an images folder to the new folder.

#### Example structure

```
docs
    /android
        mypage.md
        /mypage-images
            some-image.png
```

Be sure to follow the proper Markdown syntax. See the [style guide](../contributing-guidelines/template.md) for more information.

The actual submission steps are the same as for small changes ([above](#process-for-contributing)).

The Xamarin team will review your PR and let you know (via PR feedback) if the change looks good or if there are any other updates/changes necessary in order to approve it.

The maintainers will then merge your PR once feedback has been applied and your change looks good.

On a certain cadence, we push all commits from master branch into the live site and then you'll be able to see your contribution at [Xamarin documentation](https://learn.microsoft.com/xamarin/).

### Contributing to International content

Contributions for Machine Translated (MT) content are currently not accepted for now. In an effort to improve the quality of MT content, we've transitioned to a Neural MT engine. We accept and encourage contributions for Human Translated (HT) content, which is used to train the Neural MT engine. So over time, contributions to HT content will improve the quality of both HT and MT. MT topics will have a disclaimer stating that part of the topic may be MT, and the **Edit** button won't be displayed as it's disabled.

## DOs and DON'Ts

Below is a short list of guiding rules that you should keep in mind when you are contributing to the .NET documentation.

- **DON'T** surprise us with big pull requests. Instead, file an issue and start a discussion so we can agree on a direction before you invest a large amount of time.
- **DO** read the [style guide](contributing-guidelines/template.md) and [voice and tone](contributing-guidelines/voice-tone.md) guidelines.
- **DO** use the [template](contributing-guidelines/template.md) file as the starting point of your work.
- **DO** create a separate branch on your fork before working on the articles.
- **DO** follow the [GitHub Flow workflow](https://guides.github.com/introduction/flow/).
- **DO** blog and tweet (or whatever) about your contributions, frequently!

> [!NOTE]
> You might notice that some of the topics are not currently following all the guidelines specified here and on the [style guide](contributing-guidelines/template.md) as well. We're working towards achieving consistency throughout the site. 
