using UnityEngine;  
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.VersionControl;
using Debug = UnityEngine.Debug;
using Task = System.Threading.Tasks.Task;

namespace Neocortex.OllamaSupport
{
    public class OllamaTest : MonoBehaviour
    {
        [SerializeField] private NeocortexChatPanel chatPanel;
        [SerializeField] private NeocortexTextChatInput chatInput;

        // private Process process;
        private ProcessStartInfo processStartInfo = new() {
            FileName = "ollama.exe",
            Arguments = "run deepseek-r1:1.5b",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardInputEncoding = Encoding.UTF8,
            StandardOutputEncoding = Encoding.UTF8
        };

        private SynchronizationContext mainThreadContext;
        private bool thinkingFinished = false;
        private string[] skipWords = { "<think>", "</think>", "\n", "" };
        private Process process;

        public void Awake()
        {
            mainThreadContext = SynchronizationContext.Current;
            chatInput.OnSendButtonClicked.AddListener(OnUserMessageReceived);
            process = new Process { StartInfo = processStartInfo };
            
            process.OutputDataReceived += (sender, eventArgs) =>
            {
                string responseLine = eventArgs.Data;

                if (responseLine == "</think>") thinkingFinished = true;
                if(skipWords.Any(s => responseLine == s)) return;

                if (thinkingFinished)
                {
                    mainThreadContext.Post(_ =>
                    {
                        OnUserMessageReceived(responseLine);
                    }, null);

                    thinkingFinished = false;
                }
            };
            
            process.ErrorDataReceived += (sender, eventArgs) =>
            {
                if (!string.IsNullOrEmpty(eventArgs.Data))
                {
                    Debug.Log("Error: " + eventArgs.Data);
                }
            };
            
            process.Start();
            process.BeginOutputReadLine();
            
            // process.BeginErrorReadLine();
        }
        private void OnUserMessageReceived(string message)
        {
            chatPanel.AddMessage(message, true);

            var writer = process.StandardInput;
            if (writer.BaseStream is { CanWrite: true })
            {
                writer.Write(message);
                writer.Flush();
            }
        }

        private void OnResponseReceived(string message)
        {
            chatPanel.AddMessage(message, false);
        }
    }
}
