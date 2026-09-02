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
        new(
            "list_files",
            """
            Lists files from a directory.

            Input rules:
            - Use "project_directory" when the user asks for all files in the entire project.
            - Use a relative directory such as "Agent.Core" when the user asks for files inside a specific directory.
            - Use an absolute path only when explicitly required.
            """);

    public async Task<ToolResult> ExecuteAsync(string input, CancellationToken cancellationToken = default)
    {
        var path = input.Equals("project_directory", StringComparison.OrdinalIgnoreCase)
            ? _project.ProjectRoot : Path.IsPathRooted(input) ? input : Path.Combine(_project.ProjectRoot, input);

        var files = await _fileSystem.ListFilesAsync(path, cancellationToken);

        var result = string.Join(Environment.NewLine, files);

        return new ToolResult(Definition.Name, result);
    }
}