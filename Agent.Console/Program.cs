using Agent.Core.Abstractions;
using Agent.Core.Agents;
using Agent.Core.Context;
using Agent.Core.Parsing;
using Agent.Core.Tools;
using Agent.Tools.FileSystem;
using Agent.Tools.Models;
using Agent.Tools.Tools;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IChatModel, OllamaChatModel>();
services.AddSingleton<IContextBuilder, ContextBuilder>();
services.AddSingleton<IFileSystemTool, FileSystemTool>();
services.AddSingleton<IAgentTool, ListFilesTool>();
services.AddSingleton<IToolRegistry, ToolRegistry>();
services.AddSingleton<AgentDecisionParser>();

services.AddSingleton<ProjectContext>(
    _ => new ProjectContext
    {
        ProjectName = "DotNet Agent Lab",

        ProjectRoot = @"C:\DotNetAgentLab",

        Description =
            "An experimental Coding Agent built with C# and .NET.",

        DotNetVersion = ".NET 9",

        Architecture =
            "Clean Architecture",

        Technologies =
        [
            "C#",
            ".NET 9",
            "Ollama",
            "Qwen2.5-Coder",
            "Dependency Injection",
            "EF Core"
        ],

        CodingRules =
        [
            "Follow SOLID principles.",
            "Use async/await for I/O operations.",
            "Keep business logic outside controllers.",
            "Prefer dependency injection.",
            "Write maintainable and testable code.",
            "Avoid unnecessary complexity."
        ]
    });

services.AddSingleton<AgentContext>(sp =>
    {
        var project = sp.GetRequiredService<ProjectContext>();

        return new AgentContext("""
               You are a Senior C# and .NET Software Engineer.

        Your primary expertise is:
        - C#
        - .NET
        - ASP.NET Core
        - Entity Framework Core
        - Software Architecture

        Follow the project's coding rules.
        Stay consistent with the project architecture.
        Do not invent project-specific information.
        """, project);
    });
services.AddSingleton<CodingAgent>();

var serviceProvider = services.BuildServiceProvider();

var registry = serviceProvider.GetRequiredService<IToolRegistry>();

Console.WriteLine("===== REGISTERED TOOLS =====");

foreach (var tool in registry.Tools)
{
    Console.WriteLine(
        $"{tool.Definition.Name} - {tool.Definition.Description}");
}

var testTool = registry.Get("list_files");

Console.WriteLine(
    $"TEST: {(testTool is null ? "NOT FOUND" : "FOUND")}");
Console.WriteLine("============================");

var agent = serviceProvider.GetRequiredService<CodingAgent>();

//var files = await agent.ListProjectFilesAsync();

//foreach (var file in files)
//{
//    Console.WriteLine(file);
//}

Console.WriteLine("DotNet Agent Lab 🤖");
Console.WriteLine("Type 'exit' to quit.");
Console.WriteLine();

while (true)
{
    Console.Write("You: ");

    var userMessage = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userMessage))
        continue;

    if (userMessage.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    Console.WriteLine();
    Console.Write("Agent: ");

    var response = await agent.AskAsync(userMessage);

    Console.WriteLine(response);
    Console.WriteLine();
}