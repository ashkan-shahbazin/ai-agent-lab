using Agent.Core.Abstractions;
using Agent.Core.Models;

namespace Agent.Tools.FileSystem;

public sealed class FileSystemTool : IFileSystemTool
{
    private static readonly string[] IgnoredDirectories =
    [
        "bin",
        "obj",
        ".vs",
        ".git"
    ];

    private static readonly string[] IgnoredExtensions =
    [
        ".dll",
        ".pdb",
        ".exe"
    ];

    public Task<string[]> ListFilesAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(path))
            return Task.FromResult(Array.Empty<string>());

        var files = Directory
            .EnumerateFiles(
                path,
                "*",
                SearchOption.AllDirectories)
            .Where(file =>
                !IsIgnoredDirectory(file) &&
                !IsIgnoredExtension(file))
            .ToArray();

        return Task.FromResult(files);
    }

    public async Task<string> ReadFileAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
            return $"File not found: {path}";

        return await File.ReadAllTextAsync(
            path,
            cancellationToken);
    }

    private static bool IsIgnoredDirectory(string file)
    {
        var directoryParts = file.Split(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);

        return directoryParts.Any(part =>
            IgnoredDirectories.Contains(
                part,
                StringComparer.OrdinalIgnoreCase));
    }

    private static bool IsIgnoredExtension(string file)
    {
        var extension = Path.GetExtension(file);

        return IgnoredExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase);
    }

    public Task<string[]> SearchFilesAsync(
        string directory,
        string searchPattern,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(directory))
            return Task.FromResult(Array.Empty<string>());

        var files = Directory
            .EnumerateFiles(
                directory,
                searchPattern,
                SearchOption.AllDirectories)
            .Where(file =>
                !IsIgnoredDirectory(file) &&
                !IsIgnoredExtension(file))
            .ToArray();

        return Task.FromResult(files);
    }

    public async Task<SearchMatch[]> SearchCodeAsync(string directory, string searchText, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(directory))
            return [];

        if (string.IsNullOrWhiteSpace(searchText))
            return [];

        var files = Directory
            .EnumerateFiles(
                directory,
                "*",
                SearchOption.AllDirectories)
            .Where(file =>
                !IsIgnoredDirectory(file) &&
                !IsIgnoredExtension(file));

        var matches = new List<SearchMatch>();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string[] lines;

            try
            {
                lines = await File.ReadAllLinesAsync(
                    file,
                    cancellationToken);
            }
            catch
            {
                continue;
            }

            for (var i = 0; i < lines.Length; i++)
            {
                if (!lines[i].Contains(
                        searchText,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                matches.Add(
                    new SearchMatch(
                        file,
                        i + 1,
                        lines[i].Trim()));
            }
        }

        return matches.ToArray();
    }
}
