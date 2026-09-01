using Agent.Core.Abstractions;
using Agent.Core.Context;
using Agent.Core.Models;

namespace Agent.Tools.Tools;

public sealed class ListFilesTool : IAgentTool
{
    private readonly IFileSystemTool _fileSystem;
    private readonly ProjectContext _project;

    public ListFilesTool(IFileSystemTool fileSystem, ProjectContext project)
    {
        _fileSystem = fileSystem;
        _project = project;
    }

    public ToolDefinition Definition =>
        new("list_files", "Lists all files inside the project directory.");

    public async Task<ToolResult> ExecuteAsync(string input, CancellationToken cancellationToken = default)
    {
        var files = await _fileSystem.ListFilesAsync(_project.ProjectRoot, cancellationToken);

        var result = string.Join(Environment.NewLine, files);

        return new ToolResult(Definition.Name, result);
    }
}