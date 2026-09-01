namespace Agent.Core.Abstractions;

public interface IChatModel
{
    Task<string> GetResponseAsync(string message);
}