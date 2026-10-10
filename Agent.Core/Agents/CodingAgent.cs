using Agent.Core.Abstractions;
using Agent.Core.Context;
using Agent.Core.Investigation;
using Agent.Core.Models;
using Agent.Core.Parsing;

namespace Agent.Core.Agents;

public class CodingAgent
{
    private readonly IChatModel _chatModel;
    private readonly AgentContext _context;
    private readonly IContextBuilder _contextBuilder;
    private readonly IToolRegistry _toolRegistry;
    private readonly IDecisionValidator _decisionValidator;
    private readonly AgentDecisionParser _parser;
    private readonly InvestigationTracker _investigationTracker;

    public CodingAgent(IChatModel chatModel, AgentContext context, IContextBuilder contextBuilder, IToolRegistry toolRegistry, IDecisionValidator decisionValidator, AgentDecisionParser parser, InvestigationTracker investigationTracker)
    {
        _chatModel = chatModel;
        _context = context;
        _contextBuilder = contextBuilder;
        _toolRegistry = toolRegistry;
        _decisionValidator = decisionValidator;
        _parser = parser;
        _investigationTracker = investigationTracker;

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

        const int maxIterations = 8;

        for (var iteration = 0; iteration < maxIterations; iteration++)
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

            Console.WriteLine();
            Console.WriteLine("===== TOOL EXECUTION HISTORY =====");

            foreach (var execution in _context.ToolExecutions)
            {
                Console.WriteLine(
                    $"Tool: {execution.ToolName} | Input: {execution.Input}");
            }

            Console.WriteLine("==================================");

            if (decision.Type == AgentDecisionType.Answer)
            {
                var validation = _decisionValidator.Validate(decision, _context);

                Console.WriteLine();
                Console.WriteLine("===== DECISION VALIDATION =====");
                Console.WriteLine($"Valid: {validation.IsValid}");
                Console.WriteLine($"Reason: {validation.Reason}");
                Console.WriteLine("===============================");

                if (!validation.IsValid)
                {
                    _context.Messages.Add(
                        new ChatMessage(
                            MessageRole.System,
                            $"""
                             The proposed final answer was rejected.

                             Reason:
                             {validation.Reason}

                             Continue investigating the user's request.
                             Do not return FINAL yet.
                             """));

                    continue;
                }

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

                var result = await tool.ExecuteAsync(decision.ToolInput ?? string.Empty);

                var toolExecution = new ToolExecution(result.ToolName, decision.ToolInput ?? string.Empty, result.Result);

                _context.ToolExecutions.Add(toolExecution);

                _investigationTracker.Record(_context, toolExecution);

                Console.WriteLine();
                Console.WriteLine("===== INVESTIGATION EVIDENCE =====");

                foreach (var evidence in _context.Evidence)
                {
                    Console.WriteLine(
                        $"Type: {evidence.Type} | Source: {evidence.Source} | Details: {evidence.Details}");
                }

                Console.WriteLine("==================================");

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