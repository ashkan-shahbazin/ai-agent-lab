namespace Agent.Core.Models;

public sealed record AgentDecision(AgentDecisionType Type, string? ToolName = null, string? ToolInput = null, string? Answer = null);