using System.Reflection;

namespace SpiceDb.Tests;

internal static class ResourceExtensions
{
    public static async Task<string> ReadResourceAsync(this Assembly assembly, string name)
    {
        // Determine path
        string resourcePath = name;
        // Format: "{Namespace}.{Folder}.{filename}.{Extension}"

        var resources = assembly.GetManifestResourceNames();
        resourcePath = resources.Single(str => str.EndsWith(name));

        await using System.IO.Stream stream = assembly.GetManifestResourceStream(resourcePath)!;
        using StreamReader reader = new(stream);
        return await reader.ReadToEndAsync();
    }
}