
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class StartButtonHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Button References")]
    [SerializeField] private RectTransform buttonTransform;
    [SerializeField] private Graphic buttonGraphic;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Outline buttonOutline;

    [Header("Scale Effect")]
    [SerializeField] private float hoverScale = 1.05f;

    [Header("Color Effect")]
    [SerializeField] private Color normalTextColor =
        new Color(0.75f, 0.68f, 0.57f, 1f);

    [SerializeField] private Color hoverTextColor =
        new Color(1f, 0.85f, 0.62f, 1f);

    [Header("Glow Effect")]
    [SerializeField] private Color glowColor =
        new Color(1f, 0.35f, 0.12f, 0.65f);

    [SerializeField] private float glowDistance = 3f;

    [Header("Animation")]
    [SerializeField] private float smoothSpeed = 12f;

    private Vector3 originalScale;
    private Color originalButtonColor;
    private bool isHovered;

    private void Awake()
    {
        if (buttonTransform == null)
            buttonTransform = GetComponent<RectTransform>();

        if (buttonGraphic == null)
        {
            Button button = GetComponent<Button>();

            if (button != null)
                buttonGraphic = button.targetGraphic;
        }

        if (buttonText == null)
            buttonText = GetComponentInChildren<TMP_Text>();

        originalScale = buttonTransform.localScale;

        if (buttonGraphic != null)
            originalButtonColor = buttonGraphic.color;

        if (buttonText != null)
            normalTextColor = buttonText.color;

        if (buttonOutline != null)
        {
            buttonOutline.effectDistance =
                new Vector2(glowDistance, -glowDistance);

            Color color = glowColor;
            color.a = 0f;

            buttonOutline.effectColor = color;
        }
    }

    private void Update()
    {
        float blend = 1f - Mathf.Exp(
            -smoothSpeed * Time.unscaledDeltaTime
        );

        // 1. Button Scale
        Vector3 targetScale = isHovered
            ? originalScale * hoverScale
            : originalScale;

        buttonTransform.localScale = Vector3.Lerp(
            buttonTransform.localScale,
            targetScale,
            blend
        );

        // 2. Text Color
        if (buttonText != null)
        {
            Color targetColor = isHovered
                ? hoverTextColor
                : normalTextColor;

            buttonText.color = Color.Lerp(
                buttonText.color,
                targetColor,
                blend
            );
        }

        // 3. Button Color
        if (buttonGraphic != null)
        {
            Color targetColor = isHovered
                ? originalButtonColor * 1.15f
                : originalButtonColor;

            targetColor.a = originalButtonColor.a;

            buttonGraphic.color = Color.Lerp(
                buttonGraphic.color,
                targetColor,
                blend
            );
        }

        // 4. Glow Effect
        if (buttonOutline != null)
        {
            Color targetGlow = isHovered
                ? glowColor
                : new Color(
                    glowColor.r,
                    glowColor.g,
                    glowColor.b,
                    0f
                );

            buttonOutline.effectColor = Color.Lerp(
                buttonOutline.effectColor,
                targetGlow,
                blend
            );
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}
