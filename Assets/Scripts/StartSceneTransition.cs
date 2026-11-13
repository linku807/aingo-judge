
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartSceneTransition : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup fadeOverlay;
    [SerializeField] private Button startButton;

    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "SampleScene";

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 1.2f;
    [SerializeField] private float fadeOutDuration = 1.0f;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (fadeOverlay == null || startButton == null)
        {
            Debug.LogError("FadeOverlay 또는 StartButton이 연결되지 않았습니다.");
            enabled = false;
            return;
        }

        // 처음에는 검은 화면
        fadeOverlay.alpha = 1f;
        fadeOverlay.blocksRaycasts = true;
        fadeOverlay.interactable = false;
    }

    private void Start()
    {
        // 메인 메뉴 진입 시 페이드 인
        StartCoroutine(FadeIn());
    }

    // 화면이 서서히 밝아지는 효과
    private IEnumerator FadeIn()
    {
        yield return Fade(1f, 0f, fadeInDuration);

        fadeOverlay.blocksRaycasts = false;
    }

    // 시작 버튼에서 호출할 함수
    public void StartGame()
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        startButton.interactable = false;

        StartCoroutine(FadeOutAndLoad());
    }

    // 화면을 어둡게 한 뒤 다음 씬 로드
    private IEnumerator FadeOutAndLoad()
    {
        fadeOverlay.blocksRaycasts = true;

        yield return Fade(0f, 1f, fadeOutDuration);

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError(
                "씬을 찾을 수 없습니다: " + nextSceneName
            );

            isTransitioning = false;
            startButton.interactable = true;
            fadeOverlay.blocksRaycasts = false;

            yield break;
        }

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(nextSceneName);

        if (operation == null)
        {
            Debug.LogError("씬 로드에 실패했습니다.");

            isTransitioning = false;
            startButton.interactable = true;
            fadeOverlay.blocksRaycasts = false;

            yield break;
        }

        while (!operation.isDone)
        {
            yield return null;
        }
    }

    // 공통 페이드 함수
    private IEnumerator Fade(
        float startAlpha,
        float endAlpha,
        float duration
    )
    {
        float elapsed = 0f;

        fadeOverlay.alpha = startAlpha;

        if (duration <= 0f)
        {
            fadeOverlay.alpha = endAlpha;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            // 부드러운 움직임
            t = t * t * (3f - 2f * t);

            fadeOverlay.alpha =
                Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }

        fadeOverlay.alpha = endAlpha;
    }
}
