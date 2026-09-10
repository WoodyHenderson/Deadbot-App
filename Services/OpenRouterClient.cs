using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace DeadBot.Services;

public sealed class OpenRouterClient
{
    private static readonly Uri ChatCompletionsEndpoint =
        new("https://openrouter.ai/api/v1/chat/completions");

    private readonly HttpClient httpClient;

    public OpenRouterClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<string> SendChatAsync(
        string apiKey,
        string model,
        IReadOnlyList<OpenRouterMessage> messages,
        CancellationToken cancellationToken)
    {
        var payload = new ChatRequest(model, messages);
        using var request = new HttpRequestMessage(HttpMethod.Post, ChatCompletionsEndpoint)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());
        request.Headers.Add("X-Title", "DeadBot");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new OpenRouterException(response.StatusCode, ReadErrorMessage(responseBody));

        try
        {
            var result = JsonSerializer.Deserialize<ChatResponse>(responseBody);
            var answer = result?.Choices is { Count: > 0 }
                ? result.Choices[0].Message?.Content
                : null;

            if (string.IsNullOrWhiteSpace(answer))
                throw new OpenRouterException(response.StatusCode, "OpenRouter returned an empty response.");

            return answer.Trim();
        }
        catch (JsonException)
        {
            throw new OpenRouterException(response.StatusCode, "OpenRouter returned an unreadable response.");
        }
    }

    private static string ReadErrorMessage(string responseBody)
    {
        try
        {
            var error = JsonSerializer.Deserialize<ErrorResponse>(responseBody);
            return string.IsNullOrWhiteSpace(error?.Error?.Message)
                ? "OpenRouter rejected the request."
                : error.Error.Message;
        }
        catch (JsonException)
        {
            return "OpenRouter returned an unreadable error response.";
        }
    }

    private sealed record ChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<OpenRouterMessage> Messages);

    private sealed record ChatResponse(
        [property: JsonPropertyName("choices")] List<ChatChoice>? Choices);

    private sealed record ChatChoice(
        [property: JsonPropertyName("message")] OpenRouterMessage? Message);

    private sealed record ErrorResponse(
        [property: JsonPropertyName("error")] ErrorDetails? Error);

    private sealed record ErrorDetails(
        [property: JsonPropertyName("message")] string? Message);
}

public sealed record OpenRouterMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);

public sealed class OpenRouterException : Exception
{
    public OpenRouterException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}
