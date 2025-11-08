SkiaSharp provides 2 types of documentation: concept and API.

## Concept Docs

The concept docs are found at MicrosoftDocs/xamarin-docs:
 - https://github.com/MicrosoftDocs/xamarin-docs/tree/live/docs/xamarin-forms/user-interface/graphics/skiasharp
 - https://github.com/MicrosoftDocs/xamarin-docs/tree/live/docs/graphics-games/skiasharp

## API Docs

The API docs found at https://docs.microsoft.com/dotnet/api/skiasharp are located in a separate SkiaSharp repository:

 - https://github.com/mono/SkiaSharp-API-docs

## Generating API Docs

Because the API docs are very large and contain examples as well as images and other assets that do not fit well in source code, they are hosted in a separate repository. At some point, the languages would also be expanded to include other non-English locales.

Generating the API docs is basically a single step process, but my require a pre-generation step if the source is not built locally first. Before generating any docs, make sure you are on a new branch in the `docs` submodule. This will allow you to make changes and then send a PR into [`mono/SkiaSharp-API-docs`](https://github.com/mono/SkiaSharp-API-docs).


### Without Building Source

To generate the API docs without building the source, the latest build artifacts need to be downloaded:

```
> .\bootstrapper.ps1 -t docs-download-output
```

If the latest master branch is not where the new APIs are coming from, you can specify an explicit build ID:

```
> .\bootstrapper.ps1 -t docs-download-output --azureBuildId=BUILD_ID
```

The `docs-download-output` task will download and extract all the necessary bits so that the docs can be generated for all the platforms.

> The build ID can be obtained from:
> ```
> https://dev.azure.com/xamarin/public/_build?definitionId=4
> ```
> Once you have determined the desired build, you can select the build ID from the URL. For example, if the URL is:
> ```
> https://dev.azure.com/xamarin/public/_build/results?buildId=8826
> ```
> Then the build ID is `8826`

### With Pre-Built Artifacts or Locally Built Source

Once you have checked out SkiaSharp an created a new branch in the `docs` submodule, run `update-docs`:

```
> .\bootstrapper.ps1 -t update-docs
```

This task will generate both the changelogs and the XML API docs in the `./docs/` directory. From here on, you can open the XML files in any text editor and add/update docs. Any docs that do not have actual content will have the "To be added." text.

As you edit the docs, you can apply formatting and see what docs are still missing by running `docs-format-docs`:

```
> .\bootstrapper.ps1 -t docs-format-docs
```

Once you are happy, you can push your changes to your fork of [`mono/SkiaSharp-API-docs`](https://github.com/mono/SkiaSharp-API-docs) and open a PR.
