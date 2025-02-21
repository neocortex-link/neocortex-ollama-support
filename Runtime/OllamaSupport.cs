using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Neocortex.API;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using SystemInfo = UnityEngine.Device.SystemInfo;

public class OllamaSupport
{
    public bool IsOllamaInstalled { get; private set; }

    public List<ModelInfo> Models = new()
    {
        new() { model = "DeepSeek-R1", parameters = "1.5B", size = "1.1GB", name = "deepseek-r1:1.5b" },
        new() { model = "DeepSeek-R1", parameters = "7B", size = "4.7GB", name = "deepseek-r1:7b" },
        new() { model = "DeepSeek-R1", parameters = "8B", size = "4.9GB", name = "deepseek-r1:8b" },
        new() { model = "DeepSeek-R1", parameters = "14B", size = "9GB", name = "deepseek-r1:14b" },

        new() { model = "Phi 4", parameters = "14B", size = "9.1GB", name = "phi4" },
        new() { model = "Phi 3", parameters = "3.8B", size = "2.2GB", name = "phi3" },
        new() { model = "Phi 3", parameters = "14B", size = "7.9GB", name = "phi3:14b" },

        new() { model = "Llama 3.2", parameters = "1B", size = "1.3GB", name = "llama3.2:1b" },
        new() { model = "Llama 3.2", parameters = "3B", size = "2.0GB", name = "llama3.2:3b" },
        new() { model = "Llama 3.1", parameters = "8B", size = "4.7GB", name = "llama3.1" },

        new() { model = "Gemma 2", parameters = "2B", size = "1.6GB", name = "gemma2:2b" },
        new() { model = "Gemma 2", parameters = "9B", size = "5.5GB", name = "gemma2" },
        new() { model = "Gemma 2", parameters = "27B", size = "16GB", name = "gemma2:27b" },

        new() { model = "Mistral", parameters = "7B", size = "4.1GB", name = "mistral" },
    };

    public float ProgressValue => request.Progress;
    public string ProgressText { get; private set; } = "Downloading the model...";

    public string PlatformName { get; private set; } = "Unknown";
    public string DownloadUrl { get; private set; }
    private readonly string downloadUrlMac = "https://ollama.com/download/mac";
    private readonly string downloadUrlLinux = "https://ollama.com/download/linux";
    private readonly string downloadUrlWindows = "https://ollama.com/download/windows";

    private OllamaRequest request = new OllamaRequest();

    private Process CreateProcess(string fileName, string args)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardErrorEncoding = Encoding.UTF8,
            StandardOutputEncoding = Encoding.UTF8,
        };

        return new Process { StartInfo = psi };
    }

    public void CheckOllamaInstallation()
    {
        using (Process process = CreateProcess("cmd.exe", "where ollama"))
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            IsOllamaInstalled = !string.IsNullOrEmpty(output);
        }
    }

    public async void CheckInstalledModels()
    {
        if (IsOllamaInstalled)
        {
            var tags = await request.GetTags();

            foreach (var model in Models)
            {
                model.isDownloaded = tags.Contains(model.name);
            }
        }
    }

    public async void DownloadModel(int modelIndex, Action onDownloadCompleted)
    {
        string modelName = Models[modelIndex].name;
        await request.PullModel(modelName);
        onDownloadCompleted?.Invoke();
    }

    public async void DeleteModel(int modelIndex)
    {
        string modelName = Models[modelIndex].name;
        await request.DeleteModel(modelName);
    }

    public void CancelRequest()
    {
        request.Abort();
    }

    public void SetPlatformDependedStrings()
    {
        if (SystemInfo.operatingSystem.Contains("Windows"))
        {
            DownloadUrl = downloadUrlWindows;
            PlatformName = "Windows";
            return;
        }

        if (SystemInfo.operatingSystem.Contains("Linux"))
        {
            DownloadUrl = downloadUrlLinux;
            PlatformName = "Linux";
            return;
        }

        if (SystemInfo.operatingSystem.Contains("Mac OS"))
        {
            DownloadUrl = downloadUrlMac;
            PlatformName = "Mac OS";
        }

        PlatformName = "Unknown";
    }
}
