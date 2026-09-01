using Agent.Core.Abstractions;

namespace Agent.Core.Tools;

public sealed class ToolRegistry : IToolRegistry
{
    private readonly IReadOnlyCollection<IAgentTool> _tools;

    public ToolRegistry(IEnumerable<IAgentTool> tools)
    {
        _tools = tools.ToArray();
    }

    public IReadOnlyCollection<IAgentTool> Tools => _tools;

    public IAgentTool? Get(string name)
    {
        return _tools.FirstOrDefault(x => x.Definition.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}