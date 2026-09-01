using Agent.Core.Abstractions;
using Agent.Core.Models;

namespace Agent.Core.Context;

public class ContextBuilder : IContextBuilder
{
    public string Build(AgentContext context, string userMessage)
    {
        var messages = new List<ChatMessage>
        {
            new(MessageRole.System, BuildSystemContext(context))
        };

        messages.AddRange(context.Messages);

        return FormatMessages(messages);
    }

    private static string BuildSystemContext(AgentContext context)
    {
        var technologies = string.Join(", ", context.Project.Technologies);

        var rules = string.Join(Environment.NewLine, context.Project.CodingRules.Select(rule => $"- {rule}"));

        var tools = string.Join(
            Environment.NewLine,
            context.Tools.Select(tool =>
                $"- {tool.Name}: {tool.Description}"));

        return $"""
    {context.SystemInstructions}

    ## Project Context

    Project:
    {context.Project.ProjectName}

    Description:
    {context.Project.Description}

    .NET Version:
    {context.Project.DotNetVersion}

    Architecture:
    {context.Project.Architecture}

    Technologies:
    {technologies}

    Coding Rules:
    {rules}

    ## Available Tools

    {tools}

    ## Tool Calling Protocol

    If you need a tool, respond ONLY with:

    TOOL_CALL
    name=<tool name>
    input=<tool input>

    Otherwise respond ONLY with:

    FINAL
    answer=<your answer>
    """;
    }

    private static string FormatMessages(IEnumerable<ChatMessage> messages)
    {
        return string.Join(Environment.NewLine + Environment.NewLine
        , messages.Select(message => $"[{message.Role}]\n{message.Content}"));
    }
}