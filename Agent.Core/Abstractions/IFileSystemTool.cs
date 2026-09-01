namespace Agent.Core.Abstractions;

public interface IFileSystemTool
{
    Task<string[]> ListFilesAsync(string path, CancellationToken cancellationToken = default);

    Task<string> ReadFileAsync(string path, CancellationToken cancellationToken = default);

    Task<string[]> SearchFilesAsync(string directory, string searchPattern, CancellationToken cancellationToken = default);
}