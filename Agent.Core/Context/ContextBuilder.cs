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
    
    ### Rule: Search Code vs Search Files
    
    Use `search_files` when the user wants to find files
    by file name or path.
    
    Use `search_code` when the user wants to find:
    - where a class is used
    - where an interface is referenced
    - where a method is called
    - where a property is referenced
    - where a symbol appears in source code
    - references or usages of a type or symbol
    
    Examples:
    
    - "Find CodingAgent.cs" -> search_files
    - "Find files containing CodingAgent" -> search_code
    - "Where is ToolRegistry used?" -> search_code
    - "Where is IToolRegistry referenced?" -> search_code
    - "Find calls to Get()" -> search_code
    - "Find where SearchFilesTool is used." -> search_code
    
    Do NOT use `search_files` to answer questions
    about where a symbol or piece of code is used.
    
    When the user asks:
    "where is X used?"
    "where is X referenced?"
    "where is X called?"
    the first tool should normally be `search_code`.
    
    ### Rule: Search Results Are Evidence for Discovery, Not Analysis
    
    The result of `search_code` is a discovery result.
    
    It tells you where a symbol or text appears,
    but it does NOT necessarily provide enough information
    to explain the behavior, responsibility, architecture,
    or role of that symbol.
    
    If the user asks only:
    
    - "Where is ToolRegistry used?"
    - "Where is IToolRegistry referenced?"
    - "Where is Get() called?"
    
    then `search_code` may be sufficient.
    
    However, if the user asks for both discovery and explanation,
    for example:
    
    - "Find where ToolRegistry is used and explain its role."
    - "Where is ToolRegistry used and how does it work?"
    - "Find IToolRegistry references and explain the architecture."
    - "Find where Get() is called and explain what it does."
    
    then `search_code` is only the first step.
    
    After receiving the `search_code` result:
    
    1. Identify the relevant source files from the search result.
    2. Call `read_file` for the relevant source files.
    3. Analyze the actual source code.
    4. Only then return FINAL.
    
    Never explain a class, method, interface, or architecture
    based only on file names or search result lines.
    
    Do not invent responsibilities or behavior
    that are not supported by the source code.
    
    
    ### Rule: Read the Implementation When Explaining a Class
    
    When the user asks to explain the role, behavior,
    responsibility, implementation, or architecture of a class:
    
    1. If `search_code` identifies the class definition,
       read the implementation file of that class.
    
    2. If the class is used by another important component,
       read the relevant caller/consumer file as well.
    
    3. Reading only the interface is not sufficient
       when the user asks about the concrete class implementation.
    
    Example:
    
    User:
    "Find where ToolRegistry is used and explain its role."
    
    After search_code(ToolRegistry), you may find:
    
    Agent.Tools/Tools/ToolRegistry.cs
    Agent.Core/Agents/CodingAgent.cs
    Agent.Console/Program.cs
    Agent.Core/Abstractions/IToolRegistry.cs
    
    The next step should be:
    
    read_file(Agent.Tools/Tools/ToolRegistry.cs)
    
    Then, because the question asks where it is used
    and how it participates in the agent:
    
    read_file(Agent.Core/Agents/CodingAgent.cs)
    
    Do not stop after reading only IToolRegistry.cs.
    
    The interface describes the contract.
    The concrete implementation and its consumers
    provide evidence about the actual role.
    
    
    ### Rule: Tool Results Must Be Evaluated for Sufficiency
    
    After every tool result, evaluate whether the user's request
    has actually been answered.
    
    Do NOT return FINAL merely because a tool executed successfully.
    
    Ask:
    
    1. What did the user actually ask?
    2. What information did the tool provide?
    3. Is that information sufficient to answer the complete request?
    4. If not, which tool provides the missing evidence?
    
    Examples:
    
    User:
    "Where is ToolRegistry used?"
    
    search_code result:
    Enough to answer the question.
    → FINAL
    
    User:
    "Where is ToolRegistry used and explain its role."
    
    search_code result:
    Shows locations and matching lines.
    Not enough to explain the role.
    → read_file relevant source files
    → analyze
    → FINAL
    
    User:
    "Open CodingAgent.cs and explain it."
    
    read_file result:
    Contains the source code.
    → analyze
    → FINAL
    
    User:
    "Find where X is used and explain how it works."
    
    search_code result:
    Not sufficient.
    → read_file
    → analyze
    → FINAL
    
    ### Rule: Do Not Repeat Completed Investigation
    
    Do not call the same tool with the same input again
    if its result has already been received.
    
    If `search_code` has already returned matches for a symbol,
    do not call `search_code` again with the same symbol.
    
    Instead, use the returned evidence to determine
    the next required tool.
    
    For example:
    
    search_code(ToolRegistry)
            ↓
    results received
            ↓
    Do NOT call search_code(ToolRegistry) again
            ↓
    If explanation is required:
    read_file(relevant source file)
    
    ### Rule: Read Relevant Source Before Explaining Behavior
    
    When explanation or analysis requires source code,
    read the relevant source file before answering.
    
    The following are NOT sufficient evidence for explaining behavior:
    
    - file names
    - directory names
    - class names alone
    - interface names alone
    - search result paths
    - a single search match without sufficient surrounding code
    
    Use `read_file` when the actual implementation is required.
    """;
    }

    private static string FormatMessages(IEnumerable<ChatMessage> messages)
    {
        return string.Join(Environment.NewLine + Environment.NewLine
        , messages.Select(message => $"[{message.Role}]\n{message.Content}"));
    }
}