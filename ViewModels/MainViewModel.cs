using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DeadBot.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public string[] AvailableModels { get; } =
    {
        "z-ai/glm-5.3-flash",
        "openai/gpt-5.6-luna"
    };

    [ObservableProperty]
    private string? apiKey;

    [ObservableProperty]
    private string selectedModel = "z-ai/glm-5.3-flash";

    [ObservableProperty]
    private string prompt = string.Empty;

    [ObservableProperty]
    private string conversation = "Ask a question about Deadlock to get started.\n\nAnswers will use the local public knowledgebase and include source references.";

    [ObservableProperty]
    private string status = "Ready — no request has been sent";

    [RelayCommand]
    private void Send()
    {
        if (string.IsNullOrWhiteSpace(Prompt))
        {
            Status = "Enter a question first";
            return;
        }

        Conversation = $"You\n{Prompt}\n\nDeadBot\nYour question is queued. Knowledgebase retrieval and OpenRouter integration are next.\n\n{Conversation}";
        Prompt = string.Empty;
        Status = "Ready for OpenRouter integration";
    }
}
