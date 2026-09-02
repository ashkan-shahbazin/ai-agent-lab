using Agent.Core.Abstractions;
using Agent.Core.Context;
using Agent.Core.Models;

namespace Agent.Tools.Tools;

public sealed class ReadFileTool : IAgentTool
{
    private readonly IFileSystemTool _fileSystem;
    private readonly ProjectContext _project;

    public ReadFileTool(
        IFileSystemTool fileSystem,
        ProjectContext project)
    {
        _fileSystem = fileSystem;
        _project = project;
    }

    public ToolDefinition Definition =>
        new(
            "read_file",
            """
            Reads the contents of a file.

            Input rules:
            - Use a relative path such as "Agent.Core/Agents/CodingAgent.cs".
            - An absolute path may also be used.
            - Use this tool when the user asks to open, read, inspect, or explain a project file.
            """);

    public async Task<ToolResult> ExecuteAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        var path = Path.IsPathRooted(input)
            ? input
            : Path.Combine(_project.ProjectRoot, input);

        var content = await _fileSystem.ReadFileAsync(
            path,
            cancellationToken);

        return new ToolResult(
            Definition.Name,
            content);
    }
}