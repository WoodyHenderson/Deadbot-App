using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DeadBot.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
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
