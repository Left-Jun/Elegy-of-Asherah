using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Widget))]
public class AutoTypewriterText : MonoBehaviour
{
    [Header("Text")]
    [TextArea(2, 8)]
    [SerializeField] private string textContent;
    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private Widget widget;

    [Header("Activation")]
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool restartEveryEnable = true;
    [SerializeField] private bool disableAfterFadeOut = true;

    [Header("Timing")]
    [SerializeField] private float characterInterval = 0.04f;
    [SerializeField] private float characterFadeDuration = 0.1f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private Coroutine typingCoroutine;
    private Coroutine fadeTextCoroutine;
    private string cachedText = string.Empty;
    private bool hasPlayed;
    private bool pendingTriggerPlay;

    private void Awake()
    {
        BindReferences();
        CacheInitialText();
        HideCharactersInstant();
        SetWidgetOpacity(0f);
    }

    private void OnEnable()
    {
        BindReferences();

        if (playOnEnable || pendingTriggerPlay)
        {
            pendingTriggerPlay = false;

            if (restartEveryEnable || !hasPlayed)
                Play();
            else
                ShowCurrentTextInstant();
        }
    }

    private void OnDisable()
    {
        StopRunningCoroutines();
    }

    private void OnValidate()
    {
        characterInterval = Mathf.Max(0f, characterInterval);
        characterFadeDuration = Mathf.Max(0f, characterFadeDuration);
        fadeOutDuration = Mathf.Max(0f, fadeOutDuration);
        BindReferences();
    }

    public void ActivateByTrigger()
    {
        pendingTriggerPlay = true;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            return;
        }

        pendingTriggerPlay = false;
        Play();
    }

    public void DeactivateByTrigger()
    {
        if (!gameObject.activeInHierarchy)
        {
            HideInstant();
            return;
        }

        StopRunningCoroutines();

        if (fadeOutDuration <= 0f)
        {
            HideInstant();
            return;
        }

        if (widget != null)
        {
            widget.Fade(0f, fadeOutDuration, () =>
            {
                if (disableAfterFadeOut)
                    gameObject.SetActive(false);
            });
            return;
        }

        fadeTextCoroutine = StartCoroutine(FadeTextAlphaOut());
    }

    public void HideInstant()
    {
        StopRunningCoroutines();
        HideCharactersInstant();
        SetWidgetOpacity(0f);

        if (disableAfterFadeOut && gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    public void Play()
    {
        BindReferences();
        CacheInitialText();
        StopRunningCoroutines();

        if (textComponent == null)
            return;

        hasPlayed = true;
        gameObject.SetActive(true);
        SetWidgetOpacity(1f);
        typingCoroutine = StartCoroutine(TypeRoutine(ResolveText()));
    }

    private IEnumerator TypeRoutine(string content)
    {
        textComponent.text = content;
        textComponent.maxVisibleCharacters = int.MaxValue;
        textComponent.ForceMeshUpdate();
        SetAllCharacterAlpha(0);

        TMP_TextInfo textInfo = textComponent.textInfo;
        int characterCount = textInfo.characterCount;

        for (int i = 0; i < characterCount; i++)
        {
            FadeCharacterInstant(i, 0);

            if (characterInterval > 0f)
                yield return new WaitForSecondsRealtime(characterInterval);

            if (!textInfo.characterInfo[i].isVisible)
                continue;

            if (characterFadeDuration <= 0f)
            {
                FadeCharacterInstant(i, 255);
                continue;
            }

            float elapsed = 0f;
            while (elapsed < characterFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                byte alpha = (byte)Mathf.RoundToInt(Mathf.Lerp(0f, 255f, Mathf.Clamp01(elapsed / characterFadeDuration)));
                FadeCharacterInstant(i, alpha);
                yield return null;
            }

            FadeCharacterInstant(i, 255);
        }

        typingCoroutine = null;
    }

    private IEnumerator FadeTextAlphaOut()
    {
        Color originalColor = textComponent.color;
        Color targetColor = originalColor;
        targetColor.a = 0f;

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            textComponent.color = Color.Lerp(originalColor, targetColor, Mathf.Clamp01(elapsed / fadeOutDuration));
            yield return null;
        }

        textComponent.color = originalColor;
        fadeTextCoroutine = null;

        if (disableAfterFadeOut)
            gameObject.SetActive(false);
    }

    private void StopRunningCoroutines()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (fadeTextCoroutine != null)
        {
            StopCoroutine(fadeTextCoroutine);
            fadeTextCoroutine = null;
        }
    }

    private void BindReferences()
    {
        if (textComponent == null)
            textComponent = GetComponent<TMP_Text>();

        if (widget == null)
            widget = GetComponent<Widget>();
    }

    private void CacheInitialText()
    {
        if (!string.IsNullOrEmpty(textContent))
        {
            cachedText = textContent;
            return;
        }

        if (textComponent != null && !string.IsNullOrEmpty(textComponent.text))
            cachedText = textComponent.text;
    }

    private string ResolveText()
    {
        if (!string.IsNullOrEmpty(textContent))
            return textContent;

        return cachedText ?? string.Empty;
    }

    private void ShowCurrentTextInstant()
    {
        if (textComponent == null)
            return;

        textComponent.text = ResolveText();
        textComponent.maxVisibleCharacters = int.MaxValue;
        textComponent.ForceMeshUpdate();
        SetAllCharacterAlpha(255);
        SetWidgetOpacity(1f);
    }

    private void HideCharactersInstant()
    {
        if (textComponent == null)
            return;

        textComponent.text = ResolveText();
        textComponent.maxVisibleCharacters = int.MaxValue;
        textComponent.ForceMeshUpdate();
        SetAllCharacterAlpha(0);
    }

    private void SetWidgetOpacity(float opacity)
    {
        if (widget != null)
            widget.Fade(opacity, 0f, null);
    }

    private void SetAllCharacterAlpha(byte alpha)
    {
        if (textComponent == null)
            return;

        TMP_TextInfo textInfo = textComponent.textInfo;
        for (int i = 0; i < textInfo.characterCount; i++)
            FadeCharacterInstant(i, alpha);

        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private void FadeCharacterInstant(int characterIndex, byte alpha)
    {
        TMP_TextInfo textInfo = textComponent.textInfo;
        if (characterIndex < 0 || characterIndex >= textInfo.characterCount)
            return;

        TMP_CharacterInfo charInfo = textInfo.characterInfo[characterIndex];
        if (!charInfo.isVisible)
            return;

        int materialIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;
        Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;

        vertexColors[vertexIndex + 0].a = alpha;
        vertexColors[vertexIndex + 1].a = alpha;
        vertexColors[vertexIndex + 2].a = alpha;
        vertexColors[vertexIndex + 3].a = alpha;

        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
}
