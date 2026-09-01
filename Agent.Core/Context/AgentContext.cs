using Agent.Core.Models;

namespace Agent.Core.Context;

public class AgentContext
{
    public string SystemInstructions { get; }

    public ProjectContext Project { get; }

    public List<ChatMessage> Messages { get; } = [];

    public List<ToolDefinition> Tools { get; } = [];

    public AgentContext(string systemInstructions, ProjectContext project)
    {
        SystemInstructions = systemInstructions;
        Project = project;
    }
}