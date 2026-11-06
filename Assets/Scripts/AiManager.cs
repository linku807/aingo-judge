using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Google.GenAI;
using Microsoft.Extensions.AI;
using TMPro;
using Dotenv;
using Manager;

public class AiChat
{
    public IChatClient AiClient;
    private readonly List<ChatMessage> ChatHistory = new();

    public AiChat(IChatClient client, string instruction)
    {
        AiClient = client;
        ChatHistory.Add(new ChatMessage(ChatRole.System, instruction));
    }

    public async IAsyncEnumerable<string> send_message(string prompt)
    {
        ChatHistory.Add(new ChatMessage(ChatRole.User, prompt));

        StringBuilder fullResponse = new StringBuilder();
        await foreach (var result in AiClient.GetStreamingResponseAsync(ChatHistory))
        {
            if (string.IsNullOrEmpty(result.Text)) continue;
            fullResponse.Append(result.Text);
            yield return result.Text;
        }

        ChatHistory.Add(new ChatMessage(ChatRole.Assistant, fullResponse.ToString()));
    }
}

public class AiManager : MonoBehaviour
{
    private const string AdvanceToolMarker = "[[COURT_TOOL:ADVANCE_PHASE]]";
    public static AiManager Instance { get; private set; }

    public TextMeshProUGUI dialogueText;
    public TMP_InputField inputField;
    [SerializeField] private string model = "gemini-3.5-flash-lite";

    private readonly StringBuilder currentDialogueBuilder = new StringBuilder();
    private AiChat gumsa_Chat;
    private AiChat jepanjang_Chat;
    private AiChat setupChat;
    private string apiKey;
    private bool isSending;
    private bool isReady;

    private const string ProsecutorInstruction =
        "너는 한국 형사 모의재판의 검사다. 검사의 관점에서 혐의와 증거를 제시하고 질문에 답하라. 사건 기록에 없는 사실이나 증거를 만들어내지 말고, 변호인 주장을 공정하게 검토하라. 재판 대사는 간결하고 자연스러운 한국어로 말하며 모의재판 역할극을 유지하라.";
    private const string JudgeInstruction =
        "너는 한국 형사 모의재판의 재판장이다. 중립을 지키고 양측의 주장을 정리하며 절차에 맞는 질문과 진행 안내를 하라. 한쪽의 편을 들거나 사건 기록에 없는 사실을 만들지 말고, 필요한 경우 양측에 추가 설명을 요청하라. 간결하고 자연스러운 한국어로 말하라.";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        GlobalManager.OnTrialContextChanged += OnTrialContextChanged;
        if (inputField != null)
            inputField.onSubmit.AddListener(FromInput);
    }

    private void OnDisable()
    {
        GlobalManager.OnTrialContextChanged -= OnTrialContextChanged;
        if (inputField != null)
            inputField.onSubmit.RemoveListener(FromInput);
    }

    private void Start()
    {
        SetSetupInputDefault();
        ChatSetup();
        if (isReady)
            ShowTrialPrompt(GlobalManager.TrialContext);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void ChatSetup()
    {
        apiKey = EnvLoader.Get("GEMINI_API");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            ShowStatus("AI 설정 오류: Assets/Scripts/.env에 GEMINI_API 값을 설정해 주세요.");
            Debug.LogError("GEMINI_API is missing. Check Assets/Scripts/.env.");
            return;
        }

        try
        {
            gumsa_Chat = CreateChat(apiKey, ProsecutorInstruction);
            jepanjang_Chat = CreateChat(apiKey, JudgeInstruction);
            ConfigureSetupChat(GlobalManager.TrialContext);
            isReady = true;
        }
        catch (Exception exception)
        {
            ShowStatus("AI 초기화 실패: " + exception.Message);
            Debug.LogException(exception);
        }
    }

    private AiChat CreateChat(string apiKey, string instruction)
    {
        return new AiChat(
            new Client(apiKey: apiKey).AsIChatClient(model),
            instruction
        );
    }

    private void OnTrialContextChanged(jepan_context context)
    {
        SetSetupInputDefault();
        if (isReady)
        {
            ConfigureSetupChat(context);
            ShowTrialPrompt(context);
        }
    }

    private void ConfigureSetupChat(jepan_context context)
    {
        if (context == jepan_context.identify_question)
        {
            setupChat = CreateChat(apiKey,
                "너는 한국어 모의재판의 피고인 설정 작가다. 사용자가 제공한 기본 설정을 존중하면서 인물의 성격, 생활 배경, 말투, 동기와 재판에서 드러날 만한 세부사항을 자연스럽게 구체화하라. 사용자의 내용을 임의로 뒤집지 말고, 새로 보탠 설정은 허구의 제안임을 전제로 작성하라. 피고인 설정 초안만 항목으로 제시하라. 초안 바깥의 안내 문구나 사용자 지시를 덧붙이지 마라.");
        }
        else if (context == jepan_context.opening_State)
        {
            setupChat = CreateChat(apiKey,
                "너는 한국어 모의재판의 사건 설정 작가다. 사용자가 제공한 기소 사실의 핵심을 유지하면서 시간순 경위, 장소, 당사자 관계, 검찰 주장, 피고인의 반박, 다툼이 되는 쟁점을 구체화하라. 새 증거를 확정 사실처럼 만들지 말고 '제안 증거' 또는 '확인할 사실'로 구분하라. 허구의 사건 초안만 항목으로 제시하라. 초안 바깥의 안내 문구나 사용자 지시를 덧붙이지 마라.");
        }
        else
        {
            setupChat = null;
        }
    }

    private void SetSetupInputDefault()
    {
        if (inputField != null &&
            (GlobalManager.TrialContext == jepan_context.identify_question ||
             GlobalManager.TrialContext == jepan_context.opening_State))
        {
            inputField.text = GlobalManager.GetSetupDefault();
        }
    }

    private void ShowTrialPrompt(jepan_context context)
    {
        if (context == jepan_context.identify_question)
            ShowStatus("인정신문 — 입력창에 피고인 기본 설정이 준비되어 있습니다. 직접 수정하거나 보충한 뒤 제출하면 AI가 설정을 구체화합니다. 제안을 수정한 뒤 '확정'을 입력하세요.");
        else if (context == jepan_context.opening_State)
            ShowStatus("모두 진술 — 입력창의 기본 기소 사실을 수정하거나 보충한 뒤 제출하세요. AI가 사건 경위와 양측 주장을 구체화합니다. 제안을 수정한 뒤 '확정'을 입력하세요.");
        else if (context == jepan_context.evidense_check)
            ShowStatus("증거 조사 — 변호인으로 발언하세요. /증거 제목 | 내용 으로 증거를 제출하고 /도움 으로 도구를 확인할 수 있습니다.");
        else if (context == jepan_context.main_question)
            ShowStatus("쟁점 확인 — 변호인으로 쟁점에 대해 발언하세요. 검사와 재판장이 차례로 응답합니다. 충분히 논의한 뒤 /다음단계를 사용할 수 있습니다.");
        else if (context == jepan_context.final)
            ShowStatus("최종 변론 — 변호인으로 최종 의견을 말하세요. 검사와 재판장이 응답한 뒤 /다음단계로 판결을 요청할 수 있습니다.");
        else
            ShowStatus("재판이 종료되었습니다.");
    }

    private void FromInput(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (inputField != null)
            inputField.text = string.Empty;

        if (GlobalManager.TrialContext == jepan_context.identify_question ||
            GlobalManager.TrialContext == jepan_context.opening_State)
        {
            if (!isReady || setupChat == null)
            {
                ShowStatus("AI를 사용할 수 없습니다. Assets/Scripts/.env의 GEMINI_API 설정을 확인해 주세요.");
                return;
            }

            if (isSending)
            {
                ShowStatus("AI 제안을 기다리는 중입니다.");
                return;
            }

            if (string.Equals(text.Trim(), "확정", StringComparison.OrdinalIgnoreCase))
            {
                GlobalManager.ConfirmSetupDraft(out string setupMessage);
                ShowStatus(setupMessage);
                return;
            }

            _ = SendSetupRequest(text.Trim());
            return;
        }

        if (!isReady)
        {
            ShowStatus("AI를 사용할 수 없습니다. Assets/Scripts/.env의 GEMINI_API 설정을 확인해 주세요.");
            return;
        }

        if (GlobalManager.TrialContext == jepan_context.completed)
        {
            ShowStatus("재판이 종료되었습니다. 새 재판을 시작하려면 장면을 다시 실행해 주세요.");
            return;
        }

        if (isSending)
        {
            ShowStatus("응답을 기다리는 중입니다.");
            return;
        }

        string playerInput = text.Trim();
        if (playerInput.StartsWith("/", StringComparison.Ordinal))
        {
            HandleCourtTool(playerInput);
            return;
        }

        _ = PlayCourtTurn(playerInput, false);
    }

    private void HandleCourtTool(string input)
    {
        if (input == "/도움")
        {
            ShowStatus("재판 도구: /증거 제목 | 내용 (증거 조사 단계), /다음단계 (다음 절차로 이동), /도움");
            return;
        }

        if (input == "/다음단계")
        {
            if (GlobalManager.AdvanceTrialPhase(out string message))
            {
                if (GlobalManager.TrialContext == jepan_context.completed)
                    _ = RequestVerdict();
                else
                    ShowTranscript();
            }
            else
                ShowStatus(message);
            return;
        }

        if (input.StartsWith("/증거 ", StringComparison.Ordinal))
        {
            string evidenceInput = input.Substring("/증거 ".Length);
            string[] parts = evidenceInput.Split(new[] { '|' }, 2);
            if (parts.Length != 2)
            {
                ShowStatus("형식: /증거 제목 | 증거 설명");
                return;
            }

            if (GlobalManager.PresentEvidence(parts[0], parts[1], out string message))
                _ = PlayCourtTurn(string.Empty, true);
            else
                ShowStatus(message);
            return;
        }

        ShowStatus("알 수 없는 도구입니다. /도움 으로 사용 가능한 도구를 확인하세요.");
    }

    private async System.Threading.Tasks.Task PlayCourtTurn(string playerInput, bool playerActionAlreadyLogged)
    {
        isSending = true;
        try
        {
            if (!playerActionAlreadyLogged)
            {
                GlobalManager.AddTrialLog(CourtRole.playerLawyer, playerInput);
                ShowTranscript();
            }

            string caseContext = GlobalManager.GetTrialCaseContext();
            string log = string.Join("\n", GlobalManager.TrialLog);
            string phaseInstructions = GetPhaseInstructions(GlobalManager.TrialContext);

            GlobalManager.SetCurrentSpeaker(CourtRole.prosecutor);
            string prosecutorPrompt = $"{caseContext}\n\n[재판 대화 기록]\n{log}\n\n{phaseInstructions}\n검사 역할로 변호인의 마지막 발언 또는 제출 증거에 답하라. 기록에 없는 사실을 추가하지 말고, 다음 순서인 재판장이 판단할 수 있도록 쟁점에 집중해 간결하게 발언하라.";
            string prosecutorResponse = await GetRoleResponse(gumsa_Chat, CourtRole.prosecutor, prosecutorPrompt);
            if (string.IsNullOrWhiteSpace(prosecutorResponse)) return;

            GlobalManager.AddTrialLog(CourtRole.prosecutor, prosecutorResponse);
            ShowTranscript();

            GlobalManager.SetCurrentSpeaker(CourtRole.judge);
            string judgeLog = string.Join("\n", GlobalManager.TrialLog);
            string judgePrompt = $"{caseContext}\n\n[재판 대화 기록]\n{judgeLog}\n\n{phaseInstructions}\n재판장 역할로 양측 발언을 중립적으로 정리하고, 다음에 필요한 절차나 질문을 간결하게 제시하라. 아직 최종 변론 단계가 아니면 유무죄를 판결하지 마라.\n현재 단계의 논의가 충분히 끝났을 때에만 마지막 줄에 {AdvanceToolMarker}를 단독으로 출력해 다음 단계 진행 도구를 호출할 수 있다. 증거 조사 단계에서는 등록된 증거가 최소 하나 있어야 하고, 쟁점 확인 단계에서는 양측 주장을 검토한 뒤에만 호출하라. 최종 변론 단계에서는 이 도구를 호출하지 마라.";
            string judgeResponse = await GetRoleResponse(jepanjang_Chat, CourtRole.judge, judgePrompt);
            if (string.IsNullOrWhiteSpace(judgeResponse)) return;

            bool advanceRequested = judgeResponse.Contains(AdvanceToolMarker);
            judgeResponse = judgeResponse.Replace(AdvanceToolMarker, string.Empty).Trim();
            GlobalManager.AddTrialLog(CourtRole.judge, judgeResponse);
            ShowTranscript();
            if (advanceRequested && GlobalManager.AdvanceTrialPhaseFromJudgeTool(out string toolMessage))
            {
                Debug.Log(toolMessage);
                ShowTranscript();
            }
        }
        catch (Exception exception)
        {
            ShowStatus("재판 대화 오류: " + exception.Message);
            Debug.LogException(exception);
        }
        finally
        {
            GlobalManager.SetCurrentSpeaker(CourtRole.playerLawyer);
            isSending = false;
        }
    }

    private async System.Threading.Tasks.Task<string> GetRoleResponse(AiChat chat, CourtRole role, string prompt)
    {
        if (chat == null)
        {
            ShowStatus("AI 역할이 준비되지 않았습니다.");
            return null;
        }

        currentDialogueBuilder.Clear();
        try
        {
            await foreach (var result in chat.send_message(prompt))
            {
                currentDialogueBuilder.Append(result);
                ShowTranscript(role, currentDialogueBuilder.ToString());
            }

            return currentDialogueBuilder.ToString().Trim();
        }
        catch (Exception exception)
        {
            ShowStatus("AI 응답 오류: " + exception.Message);
            Debug.LogException(exception);
            return null;
        }
    }

    private async System.Threading.Tasks.Task RequestVerdict()
    {
        if (isSending) return;
        isSending = true;
        GlobalManager.SetCurrentSpeaker(CourtRole.judge);
        try
        {
            string prompt = $"{GlobalManager.GetTrialCaseContext()}\n\n[재판 대화 기록]\n{string.Join("\n", GlobalManager.TrialLog)}\n\n재판장으로서 모든 기록과 제출 증거만 근거로 모의 판결을 내려라. 검찰 측 주장과 변호인 측 반박을 각각 요약하고, 유죄 또는 무죄 중 하나를 선택해 이유를 설명하라. 기록에 없는 사실은 보충하지 말고, 법률 자문이 아닌 허구의 모의재판 결과임을 간결히 밝혀라. 도구 호출 표식은 출력하지 마라.";
            string verdict = await GetRoleResponse(jepanjang_Chat, CourtRole.judge, prompt);
            if (!string.IsNullOrWhiteSpace(verdict))
            {
                verdict = verdict.Replace(AdvanceToolMarker, string.Empty).Trim();
                GlobalManager.AddTrialLog(CourtRole.judge, "모의 판결 — " + verdict);
                ShowTranscript();
            }
        }
        finally
        {
            GlobalManager.SetCurrentSpeaker(CourtRole.playerLawyer);
            isSending = false;
        }
    }

    private string GetPhaseInstructions(jepan_context context)
    {
        switch (context)
        {
            case jepan_context.evidense_check:
                return "현재 절차: 증거 조사. 제출된 증거의 의미와 신빙성을 다루고, 필요한 추가 증거를 요청하라.";
            case jepan_context.main_question:
                return "현재 절차: 쟁점 확인. 기소 사실에서 다투는 쟁점을 구분하고 양측 입장을 정리하라.";
            case jepan_context.final:
                return "현재 절차: 최종 변론. 검사는 구형 의견을, 재판장은 양측의 최종 주장을 정리하라.";
            default:
                return "현재 절차에 맞춰 발언하라.";
        }
    }

    private void ShowTranscript()
    {
        ShowTranscript(null, null);
    }

    private void ShowTranscript(CourtRole? streamingRole, string streamingText)
    {
        if (dialogueText == null) return;

        StringBuilder transcript = new StringBuilder(string.Join("\n\n", GlobalManager.TrialLog));
        if (streamingRole.HasValue && !string.IsNullOrEmpty(streamingText))
        {
            if (transcript.Length > 0) transcript.Append("\n\n");
            string label = streamingRole.Value == CourtRole.prosecutor ? "검사" : "재판장";
            transcript.Append(label).Append(": ").Append(streamingText);
        }
        dialogueText.text = transcript.ToString();
    }

    private async System.Threading.Tasks.Task SendSetupRequest(string input)
    {
        bool revisingDraft = GlobalManager.HasSetupDraft;
        string prompt = revisingDraft
            ? $"사용자가 기존 제안을 직접 수정한 전체 내용이다. 이 내용을 새 기준으로 삼아 일관성을 유지하며 구체화한 전체 초안을 다시 작성하라. 사용자가 입력한 수정본: {input}"
            : $"다음 기본 설정을 존중해 더 구체적인 설정 초안을 작성하라. 사용자가 직접 수정하거나 보충한 부분도 반영하라. 기본 설정: {input}";

        isSending = true;
        currentDialogueBuilder.Clear();
        if (dialogueText != null)
            dialogueText.text = string.Empty;

        try
        {
            await foreach (var result in setupChat.send_message(prompt))
            {
                currentDialogueBuilder.Append(result);
                if (dialogueText != null)
                    dialogueText.text = currentDialogueBuilder.ToString();
            }

            if (currentDialogueBuilder.Length == 0)
            {
                ShowStatus("AI가 빈 제안을 반환했습니다. 기본 설정을 수정해 다시 제출해 주세요.");
                return;
            }

            GlobalManager.SetSetupDraft(currentDialogueBuilder.ToString());
            if (dialogueText != null)
                dialogueText.text += "\n\n입력창의 제안을 직접 수정해 다시 제출할 수 있습니다. 이 설정을 사용할 때는 '확정'을 입력하세요.";
            if (inputField != null)
                inputField.text = currentDialogueBuilder.ToString();
        }
        catch (Exception exception)
        {
            ShowStatus("AI 설정 제안 오류: " + exception.Message);
            Debug.LogException(exception);
        }
        finally
        {
            isSending = false;
        }
    }

    private void ShowStatus(string message)
    {
        if (dialogueText != null)
            dialogueText.text = message;
    }
}
