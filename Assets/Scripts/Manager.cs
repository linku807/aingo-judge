using UnityEngine;
using System;


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
        final//구형, 최종 변론
    }

    public static class GlobalManager
    {
        private static Scenes scene = Scenes.main; // 카메라 구도 선언 및 기본값 세팅(main)
        private static jepan_context trialContext = jepan_context.identify_question;

        public const string DefaultDefendantProfile =
            "피고인 피고민(32세)은 야간 배달 일을 한다. 평소 성실하고 조용한 편이며, 최근 생활비 부담이 커졌다. 전과는 없고 가족과 함께 살고 있다.";
        public const string DefaultChargeSummary =
            "검찰은 2026년 5월 18일 밤, 김도윤이 서울의 한 카페에서 손님이 두고 간 노트북을 가져가 절도했다고 주장한다. 카페 CCTV에는 김도윤이 노트북을 가방에 넣고 나가는 모습이 찍혔다. 김도윤은 분실물 보관을 위해 카운터로 옮기려 했다고 주장하며, 노트북을 팔거나 사용하지 않았다. 이 의도와 당시 상황이 쟁점이다.";

        public static string DefendantProfile { get; private set; } = string.Empty;
        public static string ChargeSummary { get; private set; } = string.Empty;
        public static string SetupDraft { get; private set; } = string.Empty;
        public static bool HasSetupDraft => !string.IsNullOrWhiteSpace(SetupDraft);

        public static event Action<Scenes> OnSceneChanged; // 카메라 구도 변경을 감지 하는 이벤트 선언
        public static event Action<jepan_context> OnTrialContextChanged;

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
                TrialContext = jepan_context.opening_State;
                message = "피고인 설정을 확정했습니다. 모두 진술의 기본 기소 내용을 수정하거나 보완해 주세요.";
                return true;
            }

            if (TrialContext == jepan_context.opening_State)
            {
                ChargeSummary = SetupDraft;
                SetupDraft = string.Empty;
                TrialContext = jepan_context.evidense_check;
                message = "기소 사실을 확정했습니다. 재판이 시작됩니다. 검사·변호사·재판장에게 질문하거나 증거를 제시해 보세요.";
                return true;
            }

            message = "현재 재판 설정 단계가 아닙니다.";
            return false;
        }

        public static string GetTrialCaseContext()
        {
            return $"[모의재판 사건 기록]\n피고인 설정: {DefendantProfile}\n기소 내용 및 사건 개요: {ChargeSummary}\n위 기록에 없는 사실이나 증거는 사실처럼 추가하지 마세요.";
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
        }
    }
}
