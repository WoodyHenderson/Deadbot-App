using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeadBot.Services;

namespace DeadBot.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly OpenRouterClient openRouterClient;
    private readonly List<OpenRouterMessage> messages =
    [
        new("system", "You are DeadBot, a helpful assistant for the game Deadlock. Answer clearly and say when you are uncertain. Local knowledgebase context is not connected yet, so never claim that an answer is grounded in local files.")
    ];
    private bool hasConversation;

    public MainViewModel()
        : this(new OpenRouterClient(new HttpClient()))
    {
    }

    public MainViewModel(OpenRouterClient openRouterClient)
    {
        this.openRouterClient = openRouterClient;
        KnowledgebaseStatus = Directory.Exists(KnowledgebasePath)
            ? "Downloaded"
            : "Not downloaded";
    }

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

    public string KnowledgebasePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DeadBot", "knowledgebase");

    [RelayCommand]
    private async Task DownloadKnowledgebaseAsync()
    {
        if (IsDownloading)
            return;

        IsDownloading = true;
        Status = "Downloading public Deadlock data…";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(KnowledgebasePath)!);
            var isClone = Directory.Exists(Path.Combine(KnowledgebasePath, ".git"));
            var arguments = isClone
                ? $"-C \"{KnowledgebasePath}\" pull --ff-only"
                : $"clone --depth 1 https://github.com/WoodyHenderson/Deadlock-Public-Data.git \"{KnowledgebasePath}\"";

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process is null)
                throw new InvalidOperationException("Git could not be started.");

            var error = await process.StandardError.ReadToEndAsync();
            await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? "Git clone failed." : error.Trim());

            KnowledgebaseStatus = "Downloaded";
            Status = isClone ? "Knowledgebase updated" : "Knowledgebase downloaded";
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            Status = ex.Message.Contains("git", StringComparison.OrdinalIgnoreCase)
                ? "Git was not found. Install Git and try again."
                : $"Download failed: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
        }
    }

    [ObservableProperty]
    private bool isDownloading;

    private string knowledgebaseStatus = "Not downloaded";

    public string KnowledgebaseStatus
    {
        get => knowledgebaseStatus;
        private set => SetProperty(ref knowledgebaseStatus, value);
    }

    [ObservableProperty]
    private bool isSending;

    [RelayCommand]
    private async Task SendAsync()
    {
        if (IsSending)
            return;

        var question = Prompt.Trim();
        if (string.IsNullOrWhiteSpace(question))
        {
            Status = "Enter a question first";
            return;
        }

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            Status = "Enter your OpenRouter API key first";
            return;
        }

        IsSending = true;
        Status = $"Waiting for {SelectedModel}…";

        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            var requestMessages = new List<OpenRouterMessage>(messages)
            {
                new("user", question)
            };

            var answer = await openRouterClient.SendChatAsync(
                ApiKey,
                SelectedModel,
                requestMessages,
                timeout.Token);

            messages.Add(new OpenRouterMessage("user", question));
            messages.Add(new OpenRouterMessage("assistant", answer));
            AppendExchange(question, answer);
            Prompt = string.Empty;
            Status = $"Response received from {SelectedModel}";
        }
        catch (OpenRouterException ex)
        {
            Status = ex.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "OpenRouter rejected the API key",
                HttpStatusCode.PaymentRequired => "OpenRouter reports insufficient credits",
                HttpStatusCode.TooManyRequests => "OpenRouter rate limit reached — try again shortly",
                _ => $"OpenRouter error: {ex.Message}"
            };
        }
        catch (OperationCanceledException)
        {
            Status = "The request timed out after 60 seconds";
        }
        catch (HttpRequestException ex)
        {
            Status = $"Could not reach OpenRouter: {ex.Message}";
        }
        finally
        {
            IsSending = false;
        }
    }

    private void AppendExchange(string question, string answer)
    {
        var exchange = $"You\n{question}\n\nDeadBot\n{answer}";
        Conversation = hasConversation
            ? $"{Conversation}\n\n────────────────────────\n\n{exchange}"
            : exchange;
        hasConversation = true;
    }
}
