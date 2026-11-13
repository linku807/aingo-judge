
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class StartMenuParallax : MonoBehaviour
{
    [Header("UI Objects")]
    public RectTransform background;
    public RectTransform titleImage;
    public RectTransform startButton;

    [Header("Movement")]
    public Vector2 backgroundMove = new Vector2(6f, 4f);
    public Vector2 titleMove = new Vector2(16f, 10f);

    [Header("Button Rotation")]
    public float maxButtonTilt = 2.5f;

    [Header("Smoothness")]
    public float smoothSpeed = 5f;

    private Vector2 backgroundStart;
    private Vector2 titleStart;
    private Quaternion buttonStart;

    void Awake()
    {
        backgroundStart = background.anchoredPosition;
        titleStart = titleImage.anchoredPosition;
        buttonStart = startButton.localRotation;
    }

    void Update()
    {
        Vector2 mouse;

#if ENABLE_INPUT_SYSTEM
        mouse = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : new Vector2(Screen.width / 2f, Screen.height / 2f);
#elif ENABLE_LEGACY_INPUT_MANAGER
        mouse = Input.mousePosition;
#else
        mouse = new Vector2(Screen.width / 2f, Screen.height / 2f);
#endif

        Vector2 direction = new Vector2(
            (mouse.x / Screen.width - 0.5f) * 2f,
            (mouse.y / Screen.height - 0.5f) * 2f
        );

        direction = Vector2.ClampMagnitude(direction, 1f);

        float smooth = 1f - Mathf.Exp(
            -smoothSpeed * Time.unscaledDeltaTime
        );

        // Background: opposite mouse movement
        Vector2 bgTarget = backgroundStart -
            Vector2.Scale(direction, backgroundMove);

        background.anchoredPosition = Vector2.Lerp(
            background.anchoredPosition,
            bgTarget,
            smooth
        );

        // Title: stronger movement
        Vector2 titleTarget = titleStart -
            Vector2.Scale(direction, titleMove);

        titleImage.anchoredPosition = Vector2.Lerp(
            titleImage.anchoredPosition,
            titleTarget,
            smooth
        );

        // Button: slight rotation
        float angle = -direction.x * maxButtonTilt;

        Quaternion buttonTarget =
            buttonStart * Quaternion.Euler(0, 0, angle);

        startButton.localRotation = Quaternion.Slerp(
            startButton.localRotation,
            buttonTarget,
            smooth
        );
    }
}
