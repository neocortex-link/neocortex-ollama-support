using System;
using Neocortex.API;
using Neocortex.Data;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections.Generic;

public class OllamaRequest : WebRequest
{
    private const string BASE_URL = "http://localhost:11434/api/chat";

    private readonly List<Message> messages = new ();
    
    public string ModelName { get; set; }
    public event Action<ChatResponse> OnChatResponseReceived;

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
            url = BASE_URL,
            data = GetBytes(data)
        };
        
        UnityWebRequest request = await Send(payload);
        OllamaResponse response = JsonConvert.DeserializeObject<OllamaResponse>(request.downloadHandler.text, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });
        
        messages.Add(new Message() { content = response.message.content, role = "assistant" });
        OnChatResponseReceived?.Invoke(new ChatResponse()
        {
            message = response.message.content,
        });
    }
}
