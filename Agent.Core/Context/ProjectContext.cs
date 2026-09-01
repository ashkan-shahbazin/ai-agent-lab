namespace Agent.Core.Context;

public sealed class ProjectContext
{
    public string ProjectName { get; init; } = string.Empty;

    public string ProjectRoot { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string DotNetVersion { get; init; } = string.Empty;

    public string Architecture { get; init; } = string.Empty;

    public List<string> Technologies { get; init; } = [];

    public List<string> CodingRules { get; init; } = [];
}