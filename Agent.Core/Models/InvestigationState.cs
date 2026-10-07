namespace Agent.Core.Models;

public sealed class InvestigationState
{
    public bool HasCodeSearch { get; set; }

    public bool HasFileRead { get; set; }

    public bool HasImplementationRead { get; set; }

    public bool HasConsumerRead { get; set; }

    public bool HasSufficientEvidence { get; set; }
}