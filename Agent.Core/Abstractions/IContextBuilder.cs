using Agent.Core.Context;

namespace Agent.Core.Abstractions;

public interface IContextBuilder
{
    string Build(AgentContext context, string userMessage);
}