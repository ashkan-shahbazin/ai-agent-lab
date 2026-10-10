
using Agent.Core.Context;
using Agent.Core.Models;

namespace Agent.Core.Investigation;

public sealed class InvestigationTracker
{
    public void Record(
        AgentContext context,
        ToolExecution execution)
    {
        if (execution.ToolName.Equals(
                "search_code",
                StringComparison.OrdinalIgnoreCase))
        {
            context.Evidence.Add(
                new InvestigationEvidence(
                    "CodeSearch",
                    execution.Input));

            return;
        }

        if (!execution.ToolName.Equals(
                "read_file",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var source = NormalizePath(execution.Input);

        var evidenceType = ClassifyFile(source);

        if (evidenceType is null)
        {
            return;
        }

        context.Evidence.Add(
            new InvestigationEvidence(
                evidenceType,
                source));
    }

    private static string? ClassifyFile(string source)
    {
        var fileName = Path.GetFileName(source);

        if (fileName.Equals(
                "ToolRegistry.cs",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Implementation";
        }

        if (fileName.Equals(
                "IToolRegistry.cs",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Contract";
        }

        if (fileName.Equals(
                "Program.cs",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Registration";
        }

        if (fileName.Equals(
                "CodingAgent.cs",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Consumer";
        }

        return null;
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/').Trim();
    }
}