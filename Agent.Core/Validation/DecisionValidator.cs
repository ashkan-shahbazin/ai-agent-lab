using Agent.Core.Abstractions;
using Agent.Core.Context;
using Agent.Core.Models;

namespace Agent.Core.Validation;

public sealed class DecisionValidator : IDecisionValidator
{
    public DecisionValidationResult Validate(AgentDecision decision, AgentContext context)
    {
        if (decision.Type != AgentDecisionType.Answer)
        {
            return new DecisionValidationResult(true);
        }

        var userRequest = context.Messages.FirstOrDefault(x => x.Role == MessageRole.User)?.Content;

        if (string.IsNullOrWhiteSpace(userRequest))
        {
            return new DecisionValidationResult(true);
        }

        var requiresExplanation =
            userRequest.Contains("explain", StringComparison.OrdinalIgnoreCase) ||
            userRequest.Contains("role", StringComparison.OrdinalIgnoreCase) ||
            userRequest.Contains("how does", StringComparison.OrdinalIgnoreCase);

        if (!requiresExplanation)
        {
            return new DecisionValidationResult(true);
        }

        var hasCodeSearch = context.ToolExecutions.Any(x => x.ToolName.Equals("search_code", StringComparison.OrdinalIgnoreCase));

        var hasImplementationRead = context.ToolExecutions.Any(
            x =>
                x.ToolName.Equals(
                    "read_file",
                    StringComparison.OrdinalIgnoreCase)
                &&
                x.Input.Contains(
                    "ToolRegistry.cs",
                    StringComparison.OrdinalIgnoreCase));

        var hasConsumerRead = context.ToolExecutions.Any(
            x =>
                x.ToolName.Equals(
                    "read_file",
                    StringComparison.OrdinalIgnoreCase)
                &&
                !x.Input.Contains(
                    "ToolRegistry.cs",
                    StringComparison.OrdinalIgnoreCase));

        if (hasCodeSearch &&
            hasImplementationRead &&
            !hasConsumerRead)
        {
            return new DecisionValidationResult(
                false,
                """
                The request requires explaining the role of the component.

                The agent has:
                - searched for the component
                - read its implementation

                But it has not yet read a consumer of the component.

                Continue the investigation by reading a relevant consumer,
                such as CodingAgent.cs.
                """);
        }

        return new DecisionValidationResult(true);
    }
}