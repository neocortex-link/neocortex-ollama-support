using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace Neocortex
{
    [SelectionBase]
    [AddComponentMenu("Neocortex/Ollama Support/Model Dropdown", 0)]
    public class OllamaModelDropdown : Dropdown
    {
        private JsonSerializerSettings jsonSettings = new () { NullValueHandling = NullValueHandling.Ignore };
        
        protected override async void Awake()
        {
            base.Awake();

            string[] models = await GetModels();
            
            options.Clear();
            
            foreach (string model in models)
            {
                options.Add(new OptionData(model));
            }
            
            RefreshShownValue();
        }

        private async Task<string[]> GetModels()
        {
            UnityWebRequest webRequest = new UnityWebRequest();
            webRequest.url = "http://localhost:11434/api/tags";
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            
            AsyncOperation asyncOperation = webRequest.SendWebRequest();
            while (!asyncOperation.isDone) await Task.Yield();
            
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Models resp = JsonConvert.DeserializeObject<Models>(webRequest.downloadHandler.text, jsonSettings);
                return resp.models.Select(m => m.name).ToArray();
            }

            return new[] { "No Models Found" };
        }
    }

    public struct Models
    {
        public Model[] models;
    }

    public struct Model
    {
        public string name;
        public DateTime modified_at;
        public long size;
        public string digest;
        public ModelDetails details;
    }

    public struct ModelDetails
    {
        public string format;
        public string family;
        public string[] families;
        public string parameter_size;
        public string quantization_level;
    }
}