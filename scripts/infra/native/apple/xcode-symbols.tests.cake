DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../../.."));

#load "../../shared/shared.cake"
#load "xcode.cake"

Task("Default")
    .Does(() =>
{
    ValidateMachOUuidSets(
        Uuids("A", "B"),
        new[] { Uuids("A"), Uuids("B") },
        "matching UUIDs");

    ExpectFailure(
        () => ValidateMachOUuidSets(Uuids("A", "B"), new[] { Uuids("A") }, "missing UUID"),
        "missing UUID");
    ExpectFailure(
        () => ValidateMachOUuidSets(Uuids("A", "B"), new[] { Uuids("A"), Uuids("A") }, "duplicate UUID"),
        "duplicate UUID");
    ExpectFailure(
        () => ValidateMachOUuidSets(Uuids("A", "B"), new[] { Uuids("A"), Uuids("C") }, "wrong UUID"),
        "wrong UUID");
});

HashSet<string> Uuids(params string[] values) =>
    new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);

void ExpectFailure(Action action, string description)
{
    try {
        action();
    } catch (InvalidOperationException) {
        return;
    }
    throw new InvalidOperationException($"Expected UUID validation to fail for {description}.");
}

RunTarget(TARGET);
