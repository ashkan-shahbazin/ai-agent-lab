namespace Agent.Core.Models;

public enum MessageRole
{
    System,
    User,
    Assistant
}

public sealed record ChatMessage(MessageRole Role, string Content);