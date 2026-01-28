# Contributing

Thank you for your interest in contributing to the SkiaSharp documentation!

This page covers the basic process for updating content in the SkiaSharp documentation.

- [Contributor License Agreement](LICENSE)

## Process for contributing

### Small changes & edits

To make corrections and small updates - you can click the **Edit** button on any page and use the GitHub website to contribute, or follow these steps:

1. Fork the `mono/SkiaSharp` repo.

2. Create a `branch` for your changes from the `docs` branch.

3. Write your content. Refer to the [template](contributing-guidelines/template.md) and [style guide](contributing-guidelines/voice-tone.md).

4. Submit a Pull Request (PR) from your branch to `mono/SkiaSharp/docs`.

5. Make any necessary updates to your branch as discussed with the team via the PR.

6. The maintainers will merge your PR once feedback has been applied and your change looks good.

> [!NOTE]
> If your PR is addressing an existing issue, add the `Fixes #Issue_Number` keyword to the commit message or PR description, so the issue can be automatically closed when the PR is merged. For more information, see [Closing issues via commit messages](https://help.github.com/articles/closing-issues-via-commit-messages/).

### Big changes or new content

For large contributions and new content, [open an issue](https://github.com/mono/SkiaSharp/issues) describing the article you wish to write and how it relates to existing content. The content inside the docs folder is organized into sections by topic area (basics, paths, transforms, curves, bitmaps, effects).

**Get feedback on your proposal via the issue before starting to write.**

If it's a new topic, you can use the [template file](contributing-guidelines/template.md) as your starting point. It contains the writing guidelines and also explains the metadata required for each article, such as author information.

For images and other static resources, add them to the subfolder called **\<mypage>-images**. If you are creating a new folder for content, add an images folder to the new folder.

#### Example structure

```
docs/docs
    /basics
        mypage.md
        /mypage-images
            some-image.png
```

Be sure to follow the proper Markdown syntax. See the [style guide](contributing-guidelines/template.md) for more information.

The actual submission steps are the same as for small changes ([above](#process-for-contributing)).

The SkiaSharp team will review your PR and let you know (via PR feedback) if the change looks good or if there are any other updates/changes necessary in order to approve it.

## DOs and DON'Ts

Below is a short list of guiding rules that you should keep in mind when you are contributing to the SkiaSharp documentation.

- **DON'T** surprise us with big pull requests. Instead, file an issue and start a discussion so we can agree on a direction before you invest a large amount of time.
- **DO** read the [style guide](contributing-guidelines/template.md) and [voice and tone](contributing-guidelines/voice-tone.md) guidelines.
- **DO** use the [template](contributing-guidelines/template.md) file as the starting point of your work.
- **DO** create a separate branch on your fork before working on the articles.
- **DO** follow the [GitHub Flow workflow](https://guides.github.com/introduction/flow/).
- **DO** blog and tweet (or whatever) about your contributions, frequently!

> [!NOTE]
> You might notice that some of the topics are not currently following all the guidelines specified here and on the [style guide](contributing-guidelines/template.md) as well. We're working towards achieving consistency throughout the site. 
