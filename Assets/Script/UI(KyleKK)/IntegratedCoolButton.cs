using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class IntegratedCoolButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    ISelectHandler,
    IDeselectHandler
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private Transform scaleTarget;
    [SerializeField] private Widget darkRingWidget;
    [SerializeField] private Widget brightRingWidget;

    [Header("Press Scale")]
    [SerializeField] private Vector3 pressedScale = new Vector3(0.95f, 0.95f, 1f);
    [SerializeField] private float scaleRecoverSpeed = 18f;

    [Header("Ring Flash")]
    [SerializeField] private float flashSpeed = 2.8f;
    [SerializeField, Range(0f, 1f)] private float darkRingMinAlpha = 0.35f;
    [SerializeField, Range(0f, 1f)] private float darkRingMaxAlpha = 0.8f;
    [SerializeField, Range(0f, 1f)] private float brightRingMinAlpha = 0f;
    [SerializeField, Range(0f, 1f)] private float brightRingMaxAlpha = 1f;
    [SerializeField] private bool hideRingWhenInactive = true;

    private Vector3 originalScale;
    private bool isPressed;
    private bool isFlashing;
    private float flashTimer;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (scaleTarget == null)
            scaleTarget = transform;

        originalScale = scaleTarget.localScale;
        SetRingOpacity(0f, 0f);
    }

    private void OnEnable()
    {
        isPressed = false;
        scaleTarget.localScale = originalScale;

        if (!isFlashing && hideRingWhenInactive)
            SetRingOpacity(0f, 0f);
    }

    private void OnDisable()
    {
        isPressed = false;
        isFlashing = false;

        if (scaleTarget != null)
            scaleTarget.localScale = originalScale;

        if (hideRingWhenInactive)
            SetRingOpacity(0f, 0f);
    }

    private void Update()
    {
        UpdateScale();
        UpdateRingFlash();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.interactable)
            button.Select();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPressed = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (button != null && button.interactable)
            isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            isPressed = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        StartFlash();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        StopFlash();
    }

    private void UpdateScale()
    {
        if (scaleTarget == null)
            return;

        Vector3 targetScale = isPressed
            ? new Vector3(
                originalScale.x * pressedScale.x,
                originalScale.y * pressedScale.y,
                originalScale.z * pressedScale.z)
            : originalScale;

        scaleTarget.localScale = Vector3.Lerp(
            scaleTarget.localScale,
            targetScale,
            Time.unscaledDeltaTime * scaleRecoverSpeed);
    }

    private void UpdateRingFlash()
    {
        if (!isFlashing)
            return;

        flashTimer += Time.unscaledDeltaTime * flashSpeed;
        float pulse = (Mathf.Sin(flashTimer * Mathf.PI * 2f) + 1f) * 0.5f;

        float darkAlpha = Mathf.Lerp(darkRingMaxAlpha, darkRingMinAlpha, pulse);
        float brightAlpha = Mathf.Lerp(brightRingMinAlpha, brightRingMaxAlpha, pulse);
        SetRingOpacity(darkAlpha, brightAlpha);
    }

    private void StartFlash()
    {
        if (button != null && !button.interactable)
            return;

        isFlashing = true;
        flashTimer = 0f;
    }

    private void StopFlash()
    {
        isFlashing = false;
        isPressed = false;

        if (hideRingWhenInactive)
            SetRingOpacity(0f, 0f);
        else
            SetRingOpacity(darkRingMinAlpha, brightRingMinAlpha);
    }

    private void SetRingOpacity(float darkAlpha, float brightAlpha)
    {
        if (darkRingWidget != null)
            darkRingWidget.RenderOpacity = darkAlpha;

        if (brightRingWidget != null)
            brightRingWidget.RenderOpacity = brightAlpha;
    }
}
