namespace DeadBot.Models;

public sealed record ChatMessage(string Role, string Content)
{
    public bool IsAssistant => Role == "DeadBot";
    public bool IsUser => !IsAssistant;
}
