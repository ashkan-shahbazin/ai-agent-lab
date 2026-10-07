using Agent.Core.Context;
using Agent.Core.Models;

namespace Agent.Core.Investigation;

public sealed class InvestigationTracker
{
    public void Record(AgentContext context, ToolExecution execution)
    {
        if (execution.ToolName.Equals("search_code", StringComparison.OrdinalIgnoreCase))
        {
            context.Evidence.Add(
                new InvestigationEvidence(
                    "CodeSearch",
                    execution.Input));

            return;
        }

        if (execution.ToolName.Equals("read_file", StringComparison.OrdinalIgnoreCase))
        {
            var source = execution.Input;

            if (source.Contains("ToolRegistry.cs", StringComparison.OrdinalIgnoreCase))
            {
                context.Evidence.Add(
                    new InvestigationEvidence(
                        "Implementation",
                        source));
            }
            else
            {
                context.Evidence.Add(
                    new InvestigationEvidence(
                        "Consumer",
                        source));
            }
        }
    }
}