using Agent.Core.Abstractions;
using Agent.Core.Context;
using Agent.Core.Models;

namespace Agent.Core.Validation;

public sealed class DecisionValidator : IDecisionValidator
{
    public DecisionValidationResult Validate(
        AgentDecision decision,
        AgentContext context)
    {
        if (decision.Type != AgentDecisionType.Answer)
        {
            return new DecisionValidationResult(true);
        }

        var hasCodeSearch = context.Evidence.Any(
            x => x.Type.Equals(
                "CodeSearch",
                StringComparison.OrdinalIgnoreCase));

        var hasImplementation = context.Evidence.Any(
            x => x.Type.Equals(
                "Implementation",
                StringComparison.OrdinalIgnoreCase));

        var hasConsumer = context.Evidence.Any(
            x => x.Type.Equals(
                "Consumer",
                StringComparison.OrdinalIgnoreCase));

        if (!hasCodeSearch)
        {
            return new DecisionValidationResult(
                false,
                """
                The agent has not searched the codebase for the requested component.

                Perform a code search before producing a final answer.
                """);
        }

        if (!hasImplementation)
        {
            return new DecisionValidationResult(
                false,
                """
                The agent found references to the requested component,
                but has not read its implementation.

                Read the implementation before producing a final answer.
                """);
        }

        if (!hasConsumer)
        {
            return new DecisionValidationResult(
                false,
                """
                The agent has read the implementation,
                but has not investigated how the component is used.

                Read at least one relevant consumer before producing a final answer.
                """);
        }

        return new DecisionValidationResult(true);
    }
}