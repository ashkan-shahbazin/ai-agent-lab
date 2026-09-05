using Agent.Core.Abstractions;
using Agent.Core.Context;
using Agent.Core.Models;
using Agent.Core.Parsing;

namespace Agent.Core.Agents;

public class CodingAgent
{
    private readonly IChatModel _chatModel;
    private readonly AgentContext _context;
    private readonly IContextBuilder _contextBuilder;
    private readonly IToolRegistry _toolRegistry;
    private readonly AgentDecisionParser _parser;

    public CodingAgent(
        IChatModel chatModel,
        AgentContext context,
        IContextBuilder contextBuilder,
        IToolRegistry toolRegistry,
    AgentDecisionParser parser)
    {
        _chatModel = chatModel;
        _context = context;
        _contextBuilder = contextBuilder;
        _toolRegistry = toolRegistry;
        _parser = parser;

        foreach (var tool in _toolRegistry.Tools)
        {
            _context.Tools.Add(tool.Definition);
        }
    }


    public async Task<string> AskAsync(string message)
    {
        _context.Messages.Add(
            new ChatMessage(
                MessageRole.User,
                message));

        for (var iteration = 0; iteration < 5; iteration++)
        {
            Console.WriteLine(
                $"\n========== AGENT ITERATION {iteration + 1} ==========\n");

            var prompt = _contextBuilder.Build(
                _context,
                message);

            var response = await _chatModel.GetResponseAsync(prompt);

            var decision = _parser.Parse(response);

            Console.WriteLine();
            Console.WriteLine("===== RAW MODEL RESPONSE =====");
            Console.WriteLine(response);
            Console.WriteLine("==============================");

            Console.WriteLine();
            Console.WriteLine("===== PARSED DECISION =====");
            Console.WriteLine($"Type: {decision.Type}");
            Console.WriteLine($"ToolName: [{decision.ToolName}]");
            Console.WriteLine($"ToolInput: [{decision.ToolInput}]");
            Console.WriteLine("============================");

            if (decision.Type == AgentDecisionType.Answer)
            {
                _context.Messages.Add(
                    new ChatMessage(
                        MessageRole.Assistant,
                        decision.Answer ?? string.Empty));

                return decision.Answer ?? string.Empty;
            }

            if (decision.Type == AgentDecisionType.ToolCall)
            {
                var tool = _toolRegistry.Get(
                    decision.ToolName ?? string.Empty);

                if (tool is null)
                {
                    return $"Tool not found: {decision.ToolName}";
                }

                var result = await tool.ExecuteAsync(
                    decision.ToolInput ?? string.Empty);

                Console.WriteLine();
                Console.WriteLine("===== TOOL EXECUTED =====");
                Console.WriteLine($"Tool: {result.ToolName}");
                Console.WriteLine("Result:");
                Console.WriteLine(result.Result);
                Console.WriteLine("=========================");
                Console.WriteLine();

                _context.Messages.Add(
                    new ChatMessage(
                        MessageRole.System,
                        $"""
                    Tool Result:
                    Tool: {result.ToolName}

                    {result.Result}
                    """));

                _context.Messages.Add(
                    new ChatMessage(
                        MessageRole.User,
                        "Use the tool result above to answer the user's request."));
            }
        }

        return "Agent reached the maximum number of iterations.";
    }

    //public async Task<string[]> ListProjectFilesAsync(
    //    CancellationToken cancellationToken = default)
    //{
    //    return await _fileSystem.ListFilesAsync(
    //        _context.Project.ProjectRoot,
    //        cancellationToken);
    //}
}