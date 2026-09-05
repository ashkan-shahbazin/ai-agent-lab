using Agent.Core.Abstractions;
using OllamaSharp;

namespace Agent.Tools.Models;

public class OllamaChatModel : IChatModel
{
    private readonly OllamaApiClient _client;

    public OllamaChatModel()
    {
        _client = new OllamaApiClient(
            new Uri("http://localhost:11434"))
        {
            SelectedModel = "qwen2.5-coder:7b"
        };
    }

    public async Task<string> GetResponseAsync(string message)
    {
        var response = string.Empty;

        await foreach (var answer in _client.GenerateAsync(message))
        {
            if (answer is not null && answer.Response is not null)
            {
                response += answer.Response ?? string.Empty;
            }
        }

        return response;
    }
}