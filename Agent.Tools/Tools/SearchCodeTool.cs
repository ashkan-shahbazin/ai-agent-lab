using Agent.Core.Abstractions;
using Agent.Core.Context;
using Agent.Core.Models;

namespace Agent.Tools.Tools;

public sealed class SearchCodeTool : IAgentTool
{
    private readonly IFileSystemTool _fileSystem;
    private readonly ProjectContext _project;

    public SearchCodeTool(IFileSystemTool fileSystem, ProjectContext project) 
    {
        _fileSystem = fileSystem;
        _project = project;
    }

    public ToolDefinition Definition =>
        new(
            "search_code",
            """
            Searches inside source files for a text or symbol.

            Use this tool when you need to find:
            - where a class is used
            - where an interface is referenced
            - where a method is called
            - where a property is referenced
            - where a symbol appears in source code
            - references or usages of a type or symbol

            Input:
            The text or symbol to search for.

            Example:
            ToolRegistry
            """);

    public async Task<ToolResult> ExecuteAsync(string input, CancellationToken cancellationToken = default)
    {
        var matches = await _fileSystem.SearchCodeAsync(_project.ProjectRoot, input, cancellationToken);

        if (matches.Length == 0)
        {
            return new ToolResult(Definition.Name, $"No code matches found for: {input}");
        }

        var result = string.Join(Environment.NewLine, matches.Select(match => $"{match.FilePath}:{match.LineNumber} | {match.Line}"));

        return new ToolResult(Definition.Name, result);
    }
}