using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

public class OllamaSupportWindow : EditorWindow
{
    private Texture2D logo;
    private bool isOllamaInstalled = false;
    private bool isPlatformSupported = true;
    private string platformName;

    private string downloadUrl;
    private readonly string downloadUrlMac = "https://ollama.com/download/mac";
    private readonly string downloadUrlLinux = "https://ollama.com/download/linux";
    private readonly string downloadUrlWindows = "https://ollama.com/download/windows";
    
    public class ModelInfo
    {
        public string model;
        public string parameters;
        public string size;
        public string name;
        public bool isDownloaded;

        public string DisplayName => $"{model} {parameters} ({size}) {(isDownloaded ? " [Downloaded]" : "")}";
    }
    
    private int selectedModelIndex = 0;
    
    List<ModelInfo> models = new ()
    {
        new () { model = "DeepSeek-R1", parameters = "1.5B", size = "1.1GB", name = "deepseek-r1:1.5b" },
        new () { model = "DeepSeek-R1", parameters = "7B",   size = "4.7GB", name = "deepseek-r1:7b" },
        new () { model = "DeepSeek-R1", parameters = "8B",   size = "4.9GB", name = "deepseek-r1:8b" },
        new () { model = "DeepSeek-R1", parameters = "14B",  size = "9GB",   name = "deepseek-r1:14b" },
        
        new () { model = "Phi 4",       parameters = "14B",  size = "9.1GB", name = "phi4" },
        new () { model = "Phi 3",       parameters = "3.8B", size = "2.2GB", name = "phi3" },
        new () { model = "Phi 3",       parameters = "14B",  size = "7.9GB", name = "phi3:14b" },
        
        new () { model = "Llama 3.2",   parameters = "1B",   size = "1.3GB", name = "llama3.2:1b" },
        new () { model = "Llama 3.2",   parameters = "3B",   size = "2.0GB", name = "llama3.2:3b" },
        new () { model = "Llama 3.1",   parameters = "8B",   size = "4.7GB", name = "llama3.1" },

        new () { model = "Gemma 2",     parameters = "2B",   size = "1.6GB", name = "gemma2:2b" },
        new () { model = "Gemma 2",     parameters = "9B",   size = "5.5GB", name = "gemma2" },
        new () { model = "Gemma 2",     parameters = "27B",  size = "16GB",  name = "gemma2:27b" },
        
        new () { model = "Mistral",     parameters = "7B",   size = "4.1GB", name = "mistral" },
    };
    
    [MenuItem("Tools/Neocortex/Ollama Support")]
    public static void ShowWindow()
    {
        var window = GetWindow<OllamaSupportWindow>(false, "Neocortex Ollama Support", true);
        window.minSize = new Vector2(512, 512);
        window.maxSize = new Vector2(512, 512);
    }

    private void OnEnable()
    {
        logo = Resources.Load<Texture2D>("Visuals/ollama_x_neocortex");
        SetPlatformDependedStrings();
        CheckOllamaInstallation();
        CheckInstalledModels();
    }

    private void OnGUI()
    {
        if (logo)
        {
            float x = (position.width - 512) / 2;
            GUILayout.BeginHorizontal();
            GUILayout.Space(x);
            GUILayout.Label(logo, GUILayout.Width(512), GUILayout.Height(132));
            GUILayout.Space(x);
            GUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.LabelField("Logo not found", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
        }

        EditorGUILayout.BeginVertical("Box");
        
        EditorGUILayout.HelpBox("This window provides support for Ollama." +
                                "\nClick the button below to download the necessary tools.", MessageType.Info);

        string buttonMessage = isPlatformSupported
            ? isOllamaInstalled ? "Ollama is Already Installed" : $"Download Ollama for {platformName}"
            : "Platform Unsupported";
        GUI.enabled = !isOllamaInstalled && isPlatformSupported;
        if (GUILayout.Button(buttonMessage, GUILayout.Height(30)))
        {
            Application.OpenURL(downloadUrl);
        }
        GUI.enabled = true;
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("At least 8 GB RAM is needed for 7B models, 16 GB for 13B models, and 32 GB for 33B models." +
                                $"\nCurrent Device: VRAM: {SystemInfo.graphicsMemorySize/1024f:F1} GB, RAM:  {SystemInfo.systemMemorySize/1024f:F1} GB", MessageType.Warning);
        
        selectedModelIndex = EditorGUILayout.Popup("Select Model:", selectedModelIndex, models.Select(m => m.DisplayName).ToArray());

        bool isSelectedModelDownloaded = models[selectedModelIndex].isDownloaded;
        GUI.enabled = !isSelectedModelDownloaded;
        if (GUILayout.Button(isSelectedModelDownloaded ? "Model is Already Downloaded" : "Download Model", GUILayout.Height(30)))
        {
            DownloadModel();
        }
        GUI.enabled = true;

        EditorGUILayout.EndVertical();
    }

    private void CheckOllamaInstallation()
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "where ollama",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            isOllamaInstalled = !string.IsNullOrEmpty(output);
        }
    }

    private void CheckInstalledModels()
    {
        if (isOllamaInstalled)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "ollama.exe",
                Arguments = "list",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                List<string> downloadedModels = ExtractModelNames(output);

                foreach (var model in models)
                {
                    model.isDownloaded = downloadedModels.Contains(model.name);
                }
            }
        }
    }

    private void DownloadModel()
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "ollama.exe",
            Arguments = "pull " + models[selectedModelIndex].name,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            Debug.Log(output);
        }
    }
    
    private List<string> ExtractModelNames(string rawText)
    {
        List<string> modelNames = new List<string>();
        string[] lines = rawText.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (columns.Length > 0)
            {
                modelNames.Add(columns[0]);
            }
        }

        return modelNames;
    }
    
    private void SetPlatformDependedStrings()
    {
        if (SystemInfo.operatingSystem.Contains("Windows"))
        {
            downloadUrl = downloadUrlWindows;
            platformName = "Windows";
            return;
        }

        if (SystemInfo.operatingSystem.Contains("Linux"))
        {
            downloadUrl = downloadUrlLinux;
            platformName = "Linux";
            return;
        }

        if (SystemInfo.operatingSystem.Contains("Mac OS"))
        {
            downloadUrl = downloadUrlMac;
            platformName = "Mac OS";
            return;
        }

        isPlatformSupported = false;
        platformName = "Unknown";
    }
}

