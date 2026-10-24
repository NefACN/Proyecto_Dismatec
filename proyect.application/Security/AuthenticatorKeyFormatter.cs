namespace proyect.application.Security;

public static class AuthenticatorKeyFormatter
{
    public static string Format(string key) => string.Join(" ", Enumerable.Range(0, (key.Length + 3) / 4)
        .Select(index => key.Substring(index * 4, Math.Min(4, key.Length - index * 4))));
}
