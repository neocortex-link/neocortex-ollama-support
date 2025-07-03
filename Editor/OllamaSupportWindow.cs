using UnityEditor;
using UnityEngine;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace Neocortex
{
    public class OllamaSupportWindow : EditorWindow
    {
        private OllamaSupport ollama;
        private Texture2D logo;

        private int selectedModelIndex;
        private bool isDownloading;

        private const string NeocortexUrl = "https://neocortex.link";
        private const string NeocortexSignUpUrl = "https://neocortex.link/register";

        [MenuItem("Tools/Neocortex/Ollama Support")]
        public static void ShowWindow()
        {
            var window = GetWindow<OllamaSupportWindow>(false, "Neocortex Ollama Support", true);
            window.minSize = new Vector2(512, 400);  // Adjusted height for extra button
            window.maxSize = new Vector2(512, 400);
        }

        private void OnEnable()
        {
            ollama = new OllamaSupport();
            logo = Resources.Load<Texture2D>("Visuals/ollama_x_neocortex");
            ollama.CheckOllamaInstallation();
            ollama.CheckOllamaRunning();
            ollama.CheckInstalledModels();
            ollama.SetPlatformDependedStrings();

            if (!ollama.IsOllamaRunning)
            {
                EditorUtility.DisplayDialog("Ollama Not Running", "Ollama is not running. Please start Ollama and try again.", "OK");
            }
        }

        private void OnGUI()
        {
            if (logo)
            {
                float x = (position.width - 512) / 2;
                GUILayout.BeginHorizontal();
                GUILayout.Space(x);

                if (GUILayout.Button(logo, GUIStyle.none, GUILayout.Width(512), GUILayout.Height(132)))
                {
                    Application.OpenURL(NeocortexUrl);
                }

                GUILayout.Space(x);
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("Logo not found", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            }

            EditorGUILayout.BeginVertical("Box");
            
            if (GUILayout.Button("Sign Up for Neocortex", GUILayout.Height(30)))
            {
                Application.OpenURL(NeocortexSignUpUrl);
            }
            
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This window provides support for Ollama.\nClick the button below to download the necessary tools.",
                MessageType.Info);

            string buttonMessage = ollama.IsOllamaInstalled
                ? "Ollama is Already Installed"
                : $"Download Ollama for {ollama.PlatformName}";

            GUI.enabled = !ollama.IsOllamaInstalled;
            if (GUILayout.Button(buttonMessage, GUILayout.Height(30)))
            {
                Application.OpenURL(ollama.DownloadUrl);
            }
            GUI.enabled = true;

            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "At least 8 GB RAM is needed for 7B models, 16 GB for 13B models, and 32 GB for 33B models." +
                $"\nCurrent Device: VRAM: {SystemInfo.graphicsMemorySize / 1024f:F1} GB, RAM: {SystemInfo.systemMemorySize / 1024f:F1} GB",
                MessageType.Warning);

            GUI.enabled = ollama.IsOllamaInstalled;
            selectedModelIndex = EditorGUILayout.Popup("Select Model:", selectedModelIndex,
                ollama.Models.Select(m => m.DisplayName).ToArray());
            GUI.enabled = true;

            bool isSelectedModelDownloaded = ollama.Models[selectedModelIndex].isDownloaded;
            GUI.enabled = !isSelectedModelDownloaded && ollama.IsOllamaInstalled;
            if (GUILayout.Button("Download Model", GUILayout.Height(30)))
            {
                if (isDownloading)
                {
                    Debug.LogWarning("A download is already in progress.");
                }
                else
                {
                    EditorApplication.update += UpdateProgressBar;
                    isDownloading = true;
                    ollama.DownloadModel(selectedModelIndex, () =>
                    {
                        isDownloading = false;
                        ollama.CheckInstalledModels();
                    });
                }
            }

            GUI.enabled = true;

            GUI.enabled = isSelectedModelDownloaded && ollama.IsOllamaInstalled;
            if (GUILayout.Button("Delete Model", GUILayout.Height(30)))
            {
                ollama.DeleteModel(selectedModelIndex);
                ollama.CheckInstalledModels();
            }

            GUI.enabled = true;
            EditorGUILayout.EndVertical();
        }

        private void UpdateProgressBar()
        {
            if (isDownloading)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Downloading Model", ollama.ProgressText,
                        ollama.ProgressValue))
                {
                    isDownloading = false;
                    ollama.CancelRequest();
                }
            }
            else
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
