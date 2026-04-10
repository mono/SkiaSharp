dotnet tool restore
dotnet run --file build.cs -- $args

exit $LASTEXITCODE
