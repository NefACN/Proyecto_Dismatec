using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace proyect.infrastructure.Configuration;

public static partial class EnvironmentFileConfiguration
{
    // Local development only. Values become configuration entries, not process variables.
    public static IConfigurationBuilder AddProjectEnvironmentFile(this IConfigurationBuilder builder, string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Proyecto.slnx")))
            {
                var path = Path.Combine(directory.FullName, ".env");
                if (File.Exists(path)) builder.AddInMemoryCollection(Parse(File.ReadAllLines(path)));
                break;
            }
            directory = directory.Parent;
        }
        return builder;
    }

    public static IReadOnlyDictionary<string, string?> Parse(IEnumerable<string> lines)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var number = 0;
        foreach (var raw in lines)
        {
            number++;
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            if (line.StartsWith("export ", StringComparison.Ordinal)) line = line[7..].TrimStart();
            var separator = line.IndexOf('=');
            if (separator < 1 || !KeyPattern().IsMatch(line[..separator].Trim()))
                throw new FormatException($"Formato inválido en .env, línea {number}.");

            var key = line[..separator].Trim().Replace("__", ":");
            var value = line[(separator + 1)..].Trim();
            if (value.StartsWith('"'))
            {
                try { value = JsonSerializer.Deserialize<string>(value) ?? ""; }
                catch (JsonException) { throw new FormatException($"Comillas inválidas en .env, línea {number}."); }
            }
            else if (value.StartsWith('\''))
            {
                if (value.Length < 2 || !value.EndsWith('\''))
                    throw new FormatException($"Comillas inválidas en .env, línea {number}.");
                value = value[1..^1].Replace("\\'", "'").Replace("\\\\", "\\");
            }
            values[key] = value;
        }
        return values;
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex KeyPattern();
}
