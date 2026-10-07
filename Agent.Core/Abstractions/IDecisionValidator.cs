using Agent.Core.Context;
using Agent.Core.Models;

namespace Agent.Core.Abstractions;

public interface IDecisionValidator
{
    DecisionValidationResult Validate(AgentDecision decision, AgentContext context);
}