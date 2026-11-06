using UnityEngine;
using System;
using System.Collections.Generic;


namespace Manager
{
    // 카메라 구도를 편하게 관리하고 확인하기 위한 열거형
    public enum Scenes
    {
        main,
        whole,
        gumsa,
        byunhosa,
        jepanjang,
    }

    public enum jepan_context
    {
        identify_question,//인정신문
        opening_State,//모두진술
        evidense_check,//증거조사
        main_question,//쟁점 확인
        final,//구형, 최종 변론
        completed
    }

    public enum CourtRole
    {
        playerLawyer,
        prosecutor,
        judge
    }

    public static class GlobalManager
    {
        private static Scenes scene = Scenes.main; // 카메라 구도 선언 및 기본값 세팅(main)
        private static jepan_context trialContext = jepan_context.identify_question;

        public const string DefaultDefendantProfile =
            "피고인 김도윤(32세)은 야간 배달 일을 한다. 평소 성실하고 조용한 편이며, 최근 생활비 부담이 커졌다. 전과는 없고 가족과 함께 살고 있다.";
        public const string DefaultChargeSummary =
            "검찰은 2026년 5월 18일 밤, 김도윤이 서울의 한 카페에서 손님이 두고 간 노트북을 가져가 절도했다고 주장한다. 카페 CCTV에는 김도윤이 노트북을 가방에 넣고 나가는 모습이 찍혔다. 김도윤은 분실물 보관을 위해 카운터로 옮기려 했다고 주장하며, 노트북을 팔거나 사용하지 않았다. 이 의도와 당시 상황이 쟁점이다.";

        public static string DefendantProfile { get; private set; } = string.Empty;
        public static string ChargeSummary { get; private set; } = string.Empty;
        public static string SetupDraft { get; private set; } = string.Empty;
        public static bool HasSetupDraft => !string.IsNullOrWhiteSpace(SetupDraft);
        public static CourtRole CurrentSpeaker { get; private set; } = CourtRole.playerLawyer;
        public static IReadOnlyList<string> TrialLog => trialLog;
        private static readonly List<string> trialLog = new List<string>();
        private static readonly List<string> evidence = new List<string>();

        public static event Action<Scenes> OnSceneChanged; // 카메라 구도 변경을 감지 하는 이벤트 선언
        public static event Action<jepan_context> OnTrialContextChanged;
        public static event Action<CourtRole> OnSpeakerChanged;

        public static jepan_context TrialContext
        {
            get => trialContext;
            private set
            {
                if (trialContext == value) return;
                trialContext = value;
                OnTrialContextChanged?.Invoke(trialContext);
            }
        }

        public static void SetSetupDraft(string draft)
        {
            SetupDraft = draft?.Trim() ?? string.Empty;
        }

        public static string GetSetupDefault()
        {
            if (TrialContext == jepan_context.identify_question)
                return DefaultDefendantProfile;

            if (TrialContext == jepan_context.opening_State)
                return DefaultChargeSummary;

            return string.Empty;
        }

        public static bool ConfirmSetupDraft(out string message)
        {
            if (!HasSetupDraft)
            {
                message = "먼저 설정 내용을 입력하고 AI 제안을 받아 주세요.";
                return false;
            }

            if (TrialContext == jepan_context.identify_question)
            {
                DefendantProfile = SetupDraft;
                SetupDraft = string.Empty;
                AddTrialLog(CourtRole.judge, "인정신문: 피고인 설정이 확정되었습니다.");
                TrialContext = jepan_context.opening_State;
                message = "피고인 설정을 확정했습니다. 모두 진술의 기본 기소 내용을 수정하거나 보완해 주세요.";
                return true;
            }

            if (TrialContext == jepan_context.opening_State)
            {
                ChargeSummary = SetupDraft;
                SetupDraft = string.Empty;
                AddTrialLog(CourtRole.judge, "모두 진술: 기소 사실과 사건 설정이 확정되었습니다.");
                TrialContext = jepan_context.evidense_check;
                message = "기소 사실을 확정했습니다. 재판이 시작됩니다. 검사·변호사·재판장에게 질문하거나 증거를 제시해 보세요.";
                return true;
            }

            message = "현재 재판 설정 단계가 아닙니다.";
            return false;
        }

        public static string GetTrialCaseContext()
        {
            string evidenceText = evidence.Count == 0 ? "제출된 증거 없음" : string.Join("\n", evidence);
            return $"[모의재판 사건 기록]\n재판 단계: {TrialContext}\n피고인 설정: {DefendantProfile}\n기소 내용 및 사건 개요: {ChargeSummary}\n제출된 증거:\n{evidenceText}\n위 기록에 없는 사실이나 증거는 사실처럼 추가하지 마세요.";
        }

        public static void SetCurrentSpeaker(CourtRole role)
        {
            if (CurrentSpeaker == role) return;
            CurrentSpeaker = role;
            OnSpeakerChanged?.Invoke(role);
        }

        public static void AddTrialLog(CourtRole role, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return;
            string speaker = role == CourtRole.playerLawyer ? "변호인(플레이어)" :
                role == CourtRole.prosecutor ? "검사" : "재판장";
            trialLog.Add($"{speaker}: {content.Trim()}");
        }

        public static bool PresentEvidence(string title, string description, out string message)
        {
            if (TrialContext != jepan_context.evidense_check)
            {
                message = "증거 제출은 증거 조사 단계에서만 가능합니다.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            {
                message = "증거 제목과 내용을 모두 입력해 주세요. 형식: /증거 제목 | 내용";
                return false;
            }

            string item = $"{title.Trim()}: {description.Trim()}";
            evidence.Add(item);
            AddTrialLog(CourtRole.playerLawyer, $"증거 제출 — {item}");
            message = $"증거 '{title.Trim()}'을(를) 기록했습니다.";
            return true;
        }

        public static bool AdvanceTrialPhase(out string message)
        {
            switch (TrialContext)
            {
                case jepan_context.evidense_check:
                    if (evidence.Count == 0)
                    {
                        message = "증거 조사 단계는 최소 한 건의 증거가 제출된 뒤에만 넘어갈 수 있습니다.";
                        return false;
                    }
                    TrialContext = jepan_context.main_question;
                    message = "쟁점 확인 단계로 진행합니다.";
                    break;
                case jepan_context.main_question:
                    TrialContext = jepan_context.final;
                    message = "최종 변론 단계로 진행합니다.";
                    break;
                case jepan_context.final:
                    TrialContext = jepan_context.completed;
                    message = "재판 절차를 마쳤습니다.";
                    break;
                default:
                    message = "현재 단계에서는 재판을 진행할 수 없습니다.";
                    return false;
            }

            AddTrialLog(CourtRole.judge, message);
            return true;
        }

        public static bool AdvanceTrialPhaseFromJudgeTool(out string message)
        {
            switch (TrialContext)
            {
                case jepan_context.evidense_check:
                    if (evidence.Count == 0)
                    {
                        message = "증거 조사 단계는 최소 한 건의 증거가 제출된 뒤에만 넘어갈 수 있습니다.";
                        return false;
                    }
                    TrialContext = jepan_context.main_question;
                    message = "재판장 도구: 증거 조사를 마치고 쟁점 확인 단계로 진행합니다.";
                    break;
                case jepan_context.main_question:
                    TrialContext = jepan_context.final;
                    message = "재판장 도구: 쟁점 확인을 마치고 최종 변론 단계로 진행합니다.";
                    break;
                default:
                    message = "재판장 도구가 현재 단계에서는 진행할 수 없습니다.";
                    return false;
            }

            AddTrialLog(CourtRole.judge, message);
            return true;
        }

        public static Scenes Scene
        {
            get => scene;
            set
            {
                if (scene == value) return;
                scene = value;

                OnSceneChanged?.Invoke(scene);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            scene = Scenes.main;
            trialContext = jepan_context.identify_question;
            DefendantProfile = string.Empty;
            ChargeSummary = string.Empty;
            SetupDraft = string.Empty;
            trialLog.Clear();
            evidence.Clear();
            CurrentSpeaker = CourtRole.playerLawyer;
        }
    }
}
