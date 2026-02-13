using System;

public record A(B Output);
public record B(C Actionability);
public record C(string SuggestedAction);

public class Program
{
    public static void Main()
    {
        A issue = null;
        try {
            var action = issue?.Output.Actionability.SuggestedAction;
            Console.WriteLine($"Result: {action ?? "null"}");
        } catch (NullReferenceException) {
            Console.WriteLine("CRASH: NullReferenceException");
        }
    }
}
