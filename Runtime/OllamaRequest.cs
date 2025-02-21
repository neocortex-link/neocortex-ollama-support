using System;
using Neocortex;
using System.Linq;
using Neocortex.API;
using Neocortex.Data;
using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace Neocortex
{
    public class OllamaRequest : WebRequest
    {
        private const string BASE_URL = "http://localhost:11434/api";

        private readonly List<Message> messages = new();

        public string ModelName { get; set; }

        public event Action<ChatResponse> OnChatResponseReceived;

        public void AddSystemMessage(string prompt)
        {
            messages.Add(new Message()
            {
                role = "system",
                content = prompt
            });
        }

        public async void Send(string input)
        {
            if (string.IsNullOrEmpty(ModelName))
            {
                throw new Exception("ModelName property is not set.");
            }

            Headers = new Dictionary<string, string>()
            {
                { "Content-Type", "application/json" },
            };

            messages.Add(new Message() { content = input, role = "user" });

            var data = new
            {
                model = ModelName,
                messages = messages.ToArray(),
                stream = false
            };

            ApiPayload payload = new ApiPayload()
            {
                url = $"{BASE_URL}/chat",
                data = GetBytes(data)
            };

            UnityWebRequest request = await Send(payload);
            OllamaResponse response = JsonConvert.DeserializeObject<OllamaResponse>(request.downloadHandler.text,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

            messages.Add(new Message() { content = response.message.content, role = "assistant" });
            OnChatResponseReceived?.Invoke(new ChatResponse()
            {
                message = response.message.content,
            });
        }

        public async Task<List<string>> GetTags()
        {
            Headers = new Dictionary<string, string>()
            {
                { "Content-Type", "application/json" },
            };

            ApiPayload payload = new ApiPayload()
            {
                url = $"{BASE_URL}/tags",
                method = "GET"
            };

            UnityWebRequest request = await Send(payload);

            Models models = JsonConvert.DeserializeObject<Models>(request.downloadHandler.text,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

            return models.models.Select(m => m.name).ToList();
        }

        public async Task PullModel(string modelName)
        {
            Headers = new Dictionary<string, string>()
            {
                { "Content-Type", "application/json" },
            };

            var data = new
            {
                model = modelName,
                stream = true
            };

            ApiPayload payload = new ApiPayload()
            {
                url = $"{BASE_URL}/pull",
                method = "POST",
                data = GetBytes(data)
            };

            await Send(payload);
        }

        public async Task DeleteModel(string modelName)
        {
            Headers = new Dictionary<string, string>()
            {
                { "Content-Type", "application/json" },
            };

            var data = new
            {
                model = modelName
            };

            ApiPayload payload = new ApiPayload()
            {
                url = $"{BASE_URL}/delete",
                method = "DELETE",
                data = GetBytes(data)
            };

            await Send(payload);
        }
    }
}