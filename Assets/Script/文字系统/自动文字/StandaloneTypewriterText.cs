using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("UI/Standalone Typewriter Text")]
public class StandaloneTypewriterText : MonoBehaviour
{
    public enum DisplayMode
    {
        Typing,
        Fading,
        Instant
    }

    [Header("Text")]
    [SerializeField] private AdvancedText advancedText;
    [SerializeField] private TMP_Text textComponent;
    [TextArea(2, 8)]
    [SerializeField] private string textOverride;
    [SerializeField] private bool useCurrentTextWhenOverrideEmpty = true;

    [Header("Playback")]
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool hideTextUntilPlay = true;
    [SerializeField] private DisplayMode displayMode = DisplayMode.Typing;
    [SerializeField] private float startDelay = 0f;
    [SerializeField] private float charDelay = 0.06f;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private bool clearOnDisable = false;

    [Header("Skip")]
    [SerializeField] private bool allowQuickComplete = false;
    [SerializeField] private KeyCode quickCompleteKey = KeyCode.Return;

    [Header("Events")]
    [SerializeField] private UnityEvent onStarted;
    [SerializeField] private UnityEvent onFinished;

    private Coroutine playCoroutine;
    private string cachedText = string.Empty;
    private int fallbackVisibleCharacters;

    public bool IsPlaying { get; private set; }
    public bool IsFinished { get; private set; }

    private void Awake()
    {
        AutoBindText();
        CaptureCurrentText();

        if (hideTextUntilPlay)
            HideText();
    }

    private void OnEnable()
    {
        if (playOnEnable)
            Play();
    }

    private void OnDisable()
    {
        StopPlayback();

        if (clearOnDisable)
            HideText();
    }

    private void Update()
    {
        if (allowQuickComplete && IsPlaying && Input.GetKeyDown(quickCompleteKey))
            CompleteImmediately();
    }

    private void OnValidate()
    {
        startDelay = Mathf.Max(0f, startDelay);
        charDelay = Mathf.Max(0.001f, charDelay);
        fadeDuration = Mathf.Max(0f, fadeDuration);
    }

    public void Play()
    {
        AutoBindText();
        Play(ResolveContent());
    }

    public void Play(string content)
    {
        cachedText = content ?? string.Empty;
        StopPlayback();
        playCoroutine = StartCoroutine(PlayRoutine(cachedText));
    }

    public void SetTextAndPlay(string content)
    {
        textOverride = content ?? string.Empty;
        Play(textOverride);
    }

    public void CompleteImmediately()
    {
        if (!IsPlaying)
            return;

        if (advancedText != null && displayMode == DisplayMode.Typing)
        {
            advancedText.QuickShowRemaining();
            return;
        }

        StopPlayback();
        ShowTextImmediately(cachedText);
        FinishPlayback();
    }

    public void StopPlayback()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        IsPlaying = false;
    }

    private IEnumerator PlayRoutine(string content)
    {
        IsPlaying = true;
        IsFinished = false;
        onStarted?.Invoke();

        if (startDelay > 0f)
            yield return new WaitForSecondsRealtime(startDelay);

        if (advancedText != null)
            yield return PlayAdvancedText(content);
        else
            yield return PlayFallbackText(content);

        FinishPlayback();
    }

    private IEnumerator PlayAdvancedText(string content)
    {
        float originalDelay = advancedText.DefaultCharDelay;
        advancedText.DefaultCharDelay = charDelay;
        advancedText.Clear();

        bool finished = false;
        advancedText.OnFinished = () => finished = true;

        AdvancedText.DisplayType type = ToAdvancedDisplayType(displayMode);
        advancedText.StartCoroutine(advancedText.SetText(content, type, fadeDuration));

        yield return new WaitUntil(() => finished);

        advancedText.DefaultCharDelay = originalDelay;
        advancedText.OnFinished = null;
    }

    private IEnumerator PlayFallbackText(string content)
    {
        if (textComponent == null)
            yield break;

        textComponent.text = content;
        textComponent.ForceMeshUpdate();
        fallbackVisibleCharacters = textComponent.textInfo.characterCount;

        switch (displayMode)
        {
            case DisplayMode.Instant:
                textComponent.maxVisibleCharacters = int.MaxValue;
                break;
            case DisplayMode.Fading:
                yield return FadeTextAlpha(content);
                break;
            case DisplayMode.Typing:
                yield return TypeFallbackText();
                break;
        }
    }

    private IEnumerator TypeFallbackText()
    {
        textComponent.maxVisibleCharacters = 0;

        for (int i = 0; i <= fallbackVisibleCharacters; i++)
        {
            textComponent.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(charDelay);
        }
    }

    private IEnumerator FadeTextAlpha(string content)
    {
        Color originalColor = textComponent.color;
        Color transparentColor = originalColor;
        transparentColor.a = 0f;

        textComponent.text = content;
        textComponent.maxVisibleCharacters = int.MaxValue;
        textComponent.color = transparentColor;

        if (fadeDuration <= 0f)
        {
            textComponent.color = originalColor;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            textComponent.color = Color.Lerp(transparentColor, originalColor, t);
            yield return null;
        }

        textComponent.color = originalColor;
    }

    private void FinishPlayback()
    {
        playCoroutine = null;
        IsPlaying = false;
        IsFinished = true;
        onFinished?.Invoke();
    }

    private void AutoBindText()
    {
        if (advancedText == null)
            advancedText = GetComponent<AdvancedText>();
        if (advancedText == null)
            advancedText = GetComponentInChildren<AdvancedText>(true);

        if (textComponent == null)
            textComponent = advancedText != null ? advancedText : GetComponent<TMP_Text>();
        if (textComponent == null)
            textComponent = GetComponentInChildren<TMP_Text>(true);
    }

    private void CaptureCurrentText()
    {
        if (!useCurrentTextWhenOverrideEmpty || textComponent == null || !string.IsNullOrEmpty(textOverride))
            return;

        cachedText = textComponent.text;
    }

    private string ResolveContent()
    {
        if (!string.IsNullOrEmpty(textOverride))
            return textOverride;

        if (useCurrentTextWhenOverrideEmpty && textComponent != null && !string.IsNullOrEmpty(textComponent.text))
        {
            cachedText = textComponent.text;
        }

        return cachedText ?? string.Empty;
    }

    private void HideText()
    {
        if (advancedText != null)
        {
            advancedText.Clear();
            return;
        }

        if (textComponent != null)
            textComponent.maxVisibleCharacters = 0;
    }

    private void ShowTextImmediately(string content)
    {
        if (advancedText != null)
        {
            advancedText.Clear();
            advancedText.StartCoroutine(advancedText.SetText(content, AdvancedText.DisplayType.Default, 0f));
            return;
        }

        if (textComponent == null)
            return;

        textComponent.text = content;
        textComponent.maxVisibleCharacters = int.MaxValue;
    }

    private AdvancedText.DisplayType ToAdvancedDisplayType(DisplayMode mode)
    {
        switch (mode)
        {
            case DisplayMode.Instant:
                return AdvancedText.DisplayType.Default;
            case DisplayMode.Fading:
                return AdvancedText.DisplayType.Fading;
            default:
                return AdvancedText.DisplayType.Typing;
        }
    }
}
