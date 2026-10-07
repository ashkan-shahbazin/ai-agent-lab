namespace Agent.Core.Models;

public sealed record DecisionValidationResult(bool IsValid, string? Reason = null);