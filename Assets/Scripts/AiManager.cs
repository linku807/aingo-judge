using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.AI;
using TMPro;
using Dotenv;

public class AiChat
{
    public IChatClient AiClient;
    private string SystemInstruction;

    List<ChatMessage> ChatHistory = new(){};

    public AiChat(IChatClient Client, string Instruction)
    {
        AiClient = Client;
        SystemInstruction = Instruction;
        ChatHistory.Add(new ChatMessage(ChatRole.System, Instruction));
    }

    public async IAsyncEnumerable<string> send_message(string prompt)
    {
        ChatHistory.Add(new ChatMessage(ChatRole.User, prompt));

        StringBuilder FullResponse = new StringBuilder();

        await foreach (var result in AiClient.GetStreamingResponseAsync(ChatHistory))
        {
            FullResponse.Append(result.Text);
            yield return result.Text;
        }

        ChatHistory.Add(new ChatMessage(ChatRole.Assistant, FullResponse.ToString()));
    }
}

public class AiManager : MonoBehaviour
{
    public static AiManager Instance { get; private set; }

    public TextMeshProUGUI dialogueText;
    public TMP_InputField inputField;
    private static string api_key;
    private static string model = "gemini-3.5-flash-lite";
    private StringBuilder currentDialogueBuilder = new StringBuilder();

    private AiChat Chat;

    private async void SendRequest(string prompt)
    {
        currentDialogueBuilder.Clear();
        if (dialogueText != null) dialogueText.text = "";

        if (Chat == null){Debug.LogWarning("not ready!"); return;}

        await foreach (var result in Chat.send_message(prompt))
        {
            currentDialogueBuilder.Append(result);
            if (dialogueText != null)
            {
                dialogueText.text = currentDialogueBuilder.ToString();
            }
        }
    }

    private void FromInput(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        SendRequest(text);

        inputField.text = "";
    }
    private void Start()
    {
        inputField.onSubmit.AddListener(FromInput);
        api_key = EnvLoader.Get("GEMINI_API");
        Debug.Log($"불러온 API Key: {api_key}"); 
        Chat = new AiChat(
        new Client(apiKey: AiManager.api_key)
            .AsIChatClient(AiManager.model),
        "you are kind and Competent lawyer, you should explain law simply and easily, say quite short"
        );
    }
}