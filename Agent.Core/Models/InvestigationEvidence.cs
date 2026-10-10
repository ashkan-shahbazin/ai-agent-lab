namespace Agent.Core.Models;

public sealed record InvestigationEvidence(
    string Type,
    string Source,
    string? Details = null,
    string? Content = null);