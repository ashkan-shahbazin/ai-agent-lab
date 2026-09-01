namespace Agent.Core.Abstractions;

public interface IToolRegistry
{
    IReadOnlyCollection<IAgentTool> Tools { get; }

    IAgentTool? Get(string name);
}