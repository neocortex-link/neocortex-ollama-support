using Neocortex;
using UnityEngine;
using Neocortex.Data;

public class OllamaApiTest : MonoBehaviour
{
    [SerializeField] private NeocortexChatPanel chatPanel;
    [SerializeField] private NeocortexTextChatInput chatInput;

    private OllamaRequest request;
    
    void Start()
    {
        request = new OllamaRequest();
        request.OnChatResponseReceived += OnChatResponseReceived;
        chatInput.OnSendButtonClicked.AddListener(OnUserMessageSent);
    }

    private void OnChatResponseReceived(ChatResponse response)
    {
        chatPanel.AddMessage(response.message, false);
    }

    private void OnUserMessageSent(string message)
    {
        request.Send(message);
        chatPanel.AddMessage(message, true);
    }
}
