using System.Text.RegularExpressions;
using Agent.Core.Models;

namespace Agent.Core.Parsing;

public sealed class AgentDecisionParser
{
    public AgentDecision Parse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return new AgentDecision(
                AgentDecisionType.Answer,
                Answer: string.Empty);
        }

        response = response.Trim();

        if (response.StartsWith(
                "TOOL_CALL",
                StringComparison.OrdinalIgnoreCase))
        {
            var name = ExtractValue(response, "name");
            var input = ExtractValue(response, "input");

            return new AgentDecision(
                AgentDecisionType.ToolCall,
                ToolName: name,
                ToolInput: input);
        }

        if (response.StartsWith(
                "FINAL",
                StringComparison.OrdinalIgnoreCase))
        {
            var answer = ExtractValue(response, "answer");

            return new AgentDecision(
                AgentDecisionType.Answer,
                Answer: answer ?? string.Empty);
        }

        return new AgentDecision(
            AgentDecisionType.Answer,
            Answer: response);
    }

    private static string? ExtractValue(
        string text,
        string key)
    {
        var match = Regex.Match(
            text,
            $@"(?im)^\s*{Regex.Escape(key)}\s*=\s*(.*)$");

        return match.Success
            ? match.Groups[1].Value.Trim()
            : null;
    }
}