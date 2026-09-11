using System;
using System.IO;

namespace DeadBot.Services;

/// <summary>
/// Loads the local development API key from the project-directory .env file.
/// The key is returned to the client service but is never logged.
public static class EnvironmentConfiguration
{
    public static string? GetOpenRouterApiKey()
    {
        // During local development, .env is expected in the project directory.
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (!File.Exists(envPath))
            return null;

        foreach (var rawLine in File.ReadLines(envPath))
        {
            var line = rawLine.Trim();
            if (line.StartsWith("OPENROUTER_API_KEY=", StringComparison.Ordinal))
            {
                var value = line["OPENROUTER_API_KEY=".Length..].Trim();
                if (value.Length >= 2 &&
                    ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
                {
                    value = value[1..^1];
                }

                return string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }

        return null;
    }
}
