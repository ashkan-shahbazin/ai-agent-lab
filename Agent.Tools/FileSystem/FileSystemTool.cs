using Agent.Core.Abstractions;

namespace Agent.Tools.FileSystem;

public sealed class FileSystemTool : IFileSystemTool
{
    public Task<string[]> ListFilesAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(path))
            return Task.FromResult(Array.Empty<string>());

        var files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).ToArray();

        return Task.FromResult(files);
    }

    public async Task<string> ReadFileAsync(string path, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(path))
            return $"File not found: {path}";

        return await File.ReadAllTextAsync(path, cancellationToken);
    }

    public Task<string[]> SearchFilesAsync(string directory, string searchPattern, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(directory))
            return Task.FromResult(Array.Empty<string>());

        var files = Directory.EnumerateFiles(directory, searchPattern, SearchOption.AllDirectories).ToArray();

        return Task.FromResult(files);
    }
}