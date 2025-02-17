using System;
using Neocortex.API;
using Neocortex.Data;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections.Generic;

public class OllamaRequest : WebRequest
{
    private const string BASE_URL = "http://localhost:11434/api/chat";
    
    public event Action<ChatResponse> OnChatResponseReceived;
    
    private readonly List<Message> messages = new List<Message>();
    
    public async void Send(string input)
    {
        Headers = new Dictionary<string, string>()
        {
            { "Content-Type", "application/json" },
        };
        
        messages.Add(new Message() { content = input, role = "user" });
        
        var data = new
        {
            model = "llama3.2:3b",
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
