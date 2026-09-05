using Agent.Core.Abstractions;
using Agent.Core.Context;
using Agent.Core.Models;

namespace Agent.Tools.Tools;

public sealed class SearchFilesTool : IAgentTool
{
    private readonly IFileSystemTool _fileSystem;
    private readonly ProjectContext _project;

    public SearchFilesTool(
        IFileSystemTool fileSystem,
        ProjectContext project)
    {
        _fileSystem = fileSystem;
        _project = project;
    }

    public ToolDefinition Definition =>
        new(
            "search_files",
            """
            Searches for files inside the project directory.

            Input rules:
            - Use a file name or search pattern.
            - Example: "ToolRegistry"
            - Searches recursively inside the project directory.
            """);

    public async Task<ToolResult> ExecuteAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        var pattern = $"*{input}*";

        var files = await _fileSystem.SearchFilesAsync(
            _project.ProjectRoot,
            pattern,
            cancellationToken);

        var result = files.Length == 0
            ? $"No files found matching: {input}"
            : string.Join(Environment.NewLine, files);

        return new ToolResult(
            Definition.Name,
            result);
    }
}