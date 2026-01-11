namespace Kernel.Utilities;

public static class RandomCodeGenerator
{
    private static readonly Random _random = new();
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string Generate(int length)
    {
        return new string(Enumerable.Repeat(Chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}
