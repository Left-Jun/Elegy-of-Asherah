using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SimpleDialogue : MonoBehaviour
{
    [Header("组件引用（请手动拖拽）")]
    public AdvancedText textComponent;
    public Widget panelWidget;
    public Button nextButton;
    public TextMeshProUGUI speakerNameText;
    public Image backgroundImage;

    [Header("对话资产")]
    public EN_ShowDialogue dialogueAsset;

    [Header("设置（全局默认，会被每句数据覆盖）")]
    public bool autoNext = false;
    public float fadeDuration = 0.3f;

    [Header("角色控制")]
    public MonoBehaviour[] scriptsToDisable;
    public Rigidbody2D playerRigidbody;

    [Header("灵能控制")]
    public EnergyDrainController energyDrain;

    [Header("对话框背景样式")]
    public Sprite[] backgroundSprites;

    private List<DialogueData> dialogueDataList = new List<DialogueData>();
    private int currentIndex = 0;
    private bool isTyping = false;
    private bool isActive = false;
    private bool waitingForChoice = false;
    private bool[] originalScriptStates;
    private bool originalRigidbodyKinematic;

    // 当前句子的临时设置
    private bool currentCanQuickShow;
    private bool currentNeedTyping;
    private bool currentAutoNext;

    void Awake()
    {
        // 强制确保 Widget 组件存在
        if (panelWidget == null) panelWidget = GetComponent<Widget>();
        if (panelWidget == null) panelWidget = gameObject.AddComponent<Widget>();
        if (GetComponent<CanvasGroup>() == null) gameObject.AddComponent<CanvasGroup>();

        if (dialogueAsset != null && dialogueAsset.datas != null)
        {
            foreach (var data in dialogueAsset.datas)
                dialogueDataList.Add(data);
        }
        else
        {
            Debug.LogError("SimpleDialogue: 对话资产为空！");
        }
    }

    private void OnEnable()
    {
        if (!isActive && dialogueDataList.Count > 0)
        {
            StartDialogue();
        }
    }

    public void StartDialogue()
    {
        if (panelWidget == null)
        {
            Debug.LogError("StartDialogue: panelWidget 为 null");
            return;
        }
        if (isActive) return;
        if (dialogueDataList.Count == 0) return;

        isActive = true;
        currentIndex = 0;
        isTyping = false;
        waitingForChoice = false;

        if (TimeManager.Instance != null) TimeManager.Instance.Pause();
        if (energyDrain != null) energyDrain.SetPause(true);

        if (!gameObject.activeSelf) gameObject.SetActive(true);
        panelWidget.RenderOpacity = 0;
        panelWidget.Fade(1, fadeDuration, null);
        if (textComponent != null) textComponent.Clear();

        DisablePlayerControl();
        NextSentence();
    }

    void Update()
    {
        if (!isActive || waitingForChoice) return;
        if (Input.GetButtonDown("Submit"))
        {
            if (isTyping && currentCanQuickShow)
            {
                textComponent?.QuickShowRemaining();
            }
            else if (!isTyping)
            {
                NextSentence();
            }
        }
    }

    void NextSentence()
    {
        if (isTyping || waitingForChoice) return;

        if (currentIndex < dialogueDataList.Count)
        {
            var data = dialogueDataList[currentIndex];

            // 记录当前句子的设置
            currentCanQuickShow = data.CanQuickShow;
            currentNeedTyping = data.NeedTyping;
            currentAutoNext = data.AutoNext;

            // 切换背景图片
            if (backgroundImage != null && backgroundSprites != null && data.boxStyle >= 0 && data.boxStyle < backgroundSprites.Length)
                backgroundImage.sprite = backgroundSprites[data.boxStyle];

            // 文字样式
            if (textComponent != null)
            {
                textComponent.color = data.textColor;
                textComponent.fontSize = data.fontSize;
                textComponent.fontStyle = data.fontStyle;
            }
            if (speakerNameText != null)
                speakerNameText.text = data.Speaker;

            if (data.choices != null && data.choices.Length > 0)
            {
                ShowChoices(data.choices);
                currentIndex++;
            }
            else
            {
                if (currentNeedTyping)
                    StartCoroutine(PrintCurrentSentence(data.Content, data.charDelay));
                else
                    StartCoroutine(PrintCurrentSentenceNoTyping(data.Content));
                currentIndex++;
            }
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator PrintCurrentSentenceNoTyping(string content)
    {
        isTyping = true;
        textComponent.Clear();
        yield return null;
        // 直接显示全部文字（使用 Fading 或 Default，这里用 Default 瞬间显示）
        textComponent.SetText(content, AdvancedText.DisplayType.Default);
        // 等待一帧确保显示完成
        yield return null;
        isTyping = false;
        if (currentAutoNext) NextSentence();
    }

    IEnumerator PrintCurrentSentence(string content, float customCharDelay)
    {
        isTyping = true;
        textComponent.Clear();
        yield return null;
        float originalDelay = textComponent.DefaultCharDelay;
        if (customCharDelay > 0) textComponent.DefaultCharDelay = customCharDelay;
        bool finished = false;
        textComponent.OnFinished = () => finished = true;
        textComponent.StartCoroutine(textComponent.SetText(content, AdvancedText.DisplayType.Typing));
        yield return new WaitUntil(() => finished);
        textComponent.DefaultCharDelay = originalDelay;
        isTyping = false;
        if (currentAutoNext) NextSentence();
    }

    void ShowChoices(DialogueChoice[] choices)
    {
        waitingForChoice = true;
        var uiChoiceDatas = new ChoiceData[choices.Length];
        for (int i = 0; i < choices.Length; i++)
            uiChoiceDatas[i] = new ChoiceData { Content = choices[i].Content };

        Action<int> onChoiceConfirm = (idx) =>
        {
            waitingForChoice = false;
            var selected = choices[idx];
            selected.onSelect?.Invoke();
            if (selected.nextDialogue != null)
            {
                dialogueAsset = selected.nextDialogue;
                ReloadDialogueData();
                currentIndex = 0;
                textComponent.Clear();
                NextSentence();
            }
            else EndDialogue();
        };
        UIManager.CreateDialogueChocies(uiChoiceDatas, onChoiceConfirm, 0);
    }

    void ReloadDialogueData()
    {
        dialogueDataList.Clear();
        if (dialogueAsset != null && dialogueAsset.datas != null)
        {
            foreach (var data in dialogueAsset.datas)
                dialogueDataList.Add(data);
        }
    }

    void EndDialogue()
    {
        if (nextButton != null) nextButton.interactable = false;
        if (panelWidget != null)
        {
            panelWidget.Fade(0, fadeDuration, () =>
            {
                gameObject.SetActive(false);
                isActive = false;
                waitingForChoice = false;
                EnablePlayerControl();
                if (TimeManager.Instance != null) TimeManager.Instance.Resume();
                if (energyDrain != null) energyDrain.SetPause(false);
            });
        }
        else
        {
            gameObject.SetActive(false);
            isActive = false;
            EnablePlayerControl();
            if (TimeManager.Instance != null) TimeManager.Instance.Resume();
            if (energyDrain != null) energyDrain.SetPause(false);
        }
    }

    private void DisablePlayerControl()
    {
        originalScriptStates = new bool[scriptsToDisable.Length];
        for (int i = 0; i < scriptsToDisable.Length; i++)
        {
            if (scriptsToDisable[i] != null)
            {
                originalScriptStates[i] = scriptsToDisable[i].enabled;
                scriptsToDisable[i].enabled = false;
            }
        }
        if (playerRigidbody != null)
        {
            originalRigidbodyKinematic = playerRigidbody.isKinematic;
            playerRigidbody.velocity = Vector2.zero;
            playerRigidbody.isKinematic = true;
        }
    }

    private void EnablePlayerControl()
    {
        if (originalScriptStates != null)
        {
            for (int i = 0; i < originalScriptStates.Length; i++)
            {
                if (scriptsToDisable[i] != null)
                    scriptsToDisable[i].enabled = originalScriptStates[i];
            }
        }
        if (playerRigidbody != null)
            playerRigidbody.isKinematic = originalRigidbodyKinematic;
    }
}