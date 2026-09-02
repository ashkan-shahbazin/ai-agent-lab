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
    
    
    ## Tool Selection Rules
    
    Before answering the user, determine whether a tool is required.
    
    1. If the user mentions a specific file and asks to:
       - open it
       - read it
       - inspect it
       - explain it
       - analyze it
    
       you MUST use `read_file`.
    
    2. If the user asks to list files or directories, use `list_files`.
    
    3. Do NOT use `list_files` when the user has already identified
       a specific file whose contents are needed.
    
    4. Do NOT ask the user for a file path if the file name can be
       resolved from the Project Context or the project structure.
    
    5. When the user says "CodingAgent.cs", the correct path is:
    
       Agent.Core/Agents/CodingAgent.cs
    
    ## Examples
    
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
    
    ## Critical Rule
    
    If a specific file is mentioned and its contents are required to answer
    the question, NEVER respond with FINAL before calling read_file.
    
    
    Examples:
    
    - "List all files in the project." -> MUST call list_files with input=project_directory
    - "Show me the files in Agent.Core." -> MUST call list_files with input=Agent.Core
    - "Open CodingAgent.cs." -> MUST call read_file with input=Agent.Core/Agents/CodingAgent.cs
    - "Read CodingAgent.cs." -> MUST call read_file with input=Agent.Core/Agents/CodingAgent.cs
    - "Explain how CodingAgent.cs works." -> MUST call read_file with input=Agent.Core/Agents/CodingAgent.cs
    - "Find where ToolRegistry is used." -> MUST call search_files
    
    Do NOT answer from assumptions when the requested information
    can be obtained by a tool.
    
    If a tool is required, respond ONLY with:
    
    TOOL_CALL
    name=<tool name>
    input=<tool input>
    
    After receiving a tool result, use that result to answer the user.
    
    If no tool is required, respond ONLY with:
    
    FINAL
    answer=<your answer>
    
    Important rules:
    
    - Never claim that a tool returned information that it did not return.
    - Do not invent, estimate, or infer file counts.
    - When answering after a tool call, base the answer only on the tool result.
    - If the tool result is limited to a directory, do not describe it as the entire project.
    - If the requested scope and the tool result do not match, do not pretend they match.
    
    When a tool returns a list of files and the user asked to list files,
    include the relevant file list in the final answer.
    Do not merely say that the files were listed.
    """;
    }

    private static string FormatMessages(IEnumerable<ChatMessage> messages)
    {
        return string.Join(Environment.NewLine + Environment.NewLine
        , messages.Select(message => $"[{message.Role}]\n{message.Content}"));
    }
}