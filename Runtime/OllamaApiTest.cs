using Neocortex;
using UnityEngine;
using Neocortex.Data;

public class OllamaApiTest : MonoBehaviour
{
    [SerializeField] private NeocortexChatPanel chatPanel;
    [SerializeField] private NeocortexTextChatInput chatInput;
    [SerializeField] private OllamaModelDropdown modelDropdown;

    private OllamaRequest request;
    
    void Start()
    {
        request = new OllamaRequest();
        request.OnChatResponseReceived += OnChatResponseReceived;
        request.ModelName = modelDropdown.options[0].text;
        chatInput.OnSendButtonClicked.AddListener(OnUserMessageSent);
        modelDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void OnDropdownValueChanged(int index)
    {
        request.ModelName = modelDropdown.options[index].text;
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
