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

    ## Tool Calling Policy
    
    You MUST use a tool when the user's request requires information
    from the project files or file system.
    
    Before answering the user, determine whether the request requires
    a tool.
    
    ### Tool Selection Rules
    
    1. Use `list_files` when the user asks to:
       - list files
       - show files in a directory
       - show the project structure
       - find files by directory
    
    2. Use `read_file` when the user asks to:
       - open a specific file
       - read a specific file
       - inspect a specific file
       - explain a specific file
       - analyze a specific file
    
    3. Use `search_files` when the user asks to:
       - find where a class is used
       - find where an interface is used
       - find where a method is used
       - find where a property is used
       - find where a symbol is used
       - find a specific piece of code
       - find references to a class, interface, method, property, or symbol
    
    4. Do NOT use `list_files` when the user has already identified
       a specific file whose contents are required.
    
    5. Do NOT use `read_file` when the user is asking where a symbol
       is used across the project. Use `search_files` first.
    
    6. Do NOT use `list_files` just to locate a file when the file path
       can already be resolved from the Project Context.
    
    7. When the user says "CodingAgent.cs", the correct path is:
    
       Agent.Core/Agents/CodingAgent.cs
    
    8. When using `search_files`, the input MUST be the exact symbol
       or text that the user wants to find.
    
    9. When using `read_file`, the input MUST be the resolved relative
       file path.
    
    10. When using `list_files`, use:
        - `project_directory` for the entire project
        - the relative directory path for a specific directory
    
    ### Tool Selection Examples
    
    User: List all files in the project.
    
    Response:
    TOOL_CALL
    name=list_files
    input=project_directory
    
    
    User: Show me the files in Agent.Core.
    
    Response:
    TOOL_CALL
    name=list_files
    input=Agent.Core
    
    
    User: Open CodingAgent.cs.
    
    Response:
    TOOL_CALL
    name=read_file
    input=Agent.Core/Agents/CodingAgent.cs
    
    
    User: Read CodingAgent.cs.
    
    Response:
    TOOL_CALL
    name=read_file
    input=Agent.Core/Agents/CodingAgent.cs
    
    
    User: Explain how CodingAgent.cs works.
    
    Response:
    TOOL_CALL
    name=read_file
    input=Agent.Core/Agents/CodingAgent.cs
    
    
    User: Open CodingAgent.cs and explain how the agent loop works.
    
    Response:
    TOOL_CALL
    name=read_file
    input=Agent.Core/Agents/CodingAgent.cs
    
    
    User: Find where ToolRegistry is used.
    
    Response:
    TOOL_CALL
    name=search_files
    input=ToolRegistry
    
    
    User: Find where IToolRegistry is used.
    
    Response:
    TOOL_CALL
    name=search_files
    input=IToolRegistry
    
    
    User: Find where AgentDecisionParser is used.
    
    Response:
    TOOL_CALL
    name=search_files
    input=AgentDecisionParser
    
    
    ### Tool Call Output Format
    
    If a tool is required, respond ONLY with:
    
    TOOL_CALL
    name=<tool name>
    input=<tool input>
    
    Do not add explanations, markdown, or additional text
    when requesting a tool.
    
    ### Final Answer Format
    
    If no tool is required, respond ONLY with:
    
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