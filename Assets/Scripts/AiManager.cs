using System;
using System.Text;
using System.Collections;
using UnityEngine;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.AI;
using TMPro;

public class AiManager : MonoBehaviour
{
    public static AiManager Instance { get; private set; }

    public TextMeshProUGUI dialogueText;
    public TMP_InputField inputField;
    private static string api_key = "";
    private static string model = "gemini-3.5-flash-lite";
    private IChatClient client = new Client(apiKey: AiManager.api_key).AsIChatClient(AiManager.model);

    private StringBuilder currentDialogueBuilder = new StringBuilder();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async void SendRequest(string prompt)
    {


        currentDialogueBuilder.Clear();
        if (dialogueText != null) dialogueText.text = "";

        await foreach (var result in client.GetStreamingResponseAsync(prompt))
        {
            currentDialogueBuilder.Append(result.Text);
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
    }
}
