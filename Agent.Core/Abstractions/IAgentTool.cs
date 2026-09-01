using Agent.Core.Models;

namespace Agent.Core.Abstractions;

public interface IAgentTool
{
    ToolDefinition Definition { get; }

    Task<ToolResult> ExecuteAsync(string input, CancellationToken cancellationToken = default);
}