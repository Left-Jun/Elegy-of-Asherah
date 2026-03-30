using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SimpleDialogue : MonoBehaviour
{
    [Header("组件引用")]
    public AdvancedText textComponent;
    public Widget panelWidget;
    public Button nextButton;
    public TextMeshProUGUI speakerNameText;

    [Header("对话资产")]
    public EN_ShowDialogue dialogueAsset;

    [Header("设置")]
    public bool autoNext = false;
    public float fadeDuration = 0.3f;

    [Header("角色控制")]
    public MonoBehaviour[] scriptsToDisable;
    public Rigidbody2D playerRigidbody;

    private List<DialogueData> dialogueDataList = new List<DialogueData>();
    private int currentIndex = 0;
    private bool isTyping = false;
    private bool isActive = false;
    private bool waitingForChoice = false;
    private bool[] originalScriptStates;
    private bool originalRigidbodyKinematic;

    private EnergyDrainController energyDrain;

    void Awake()
    {
        energyDrain = FindObjectOfType<EnergyDrainController>();
        if (dialogueAsset != null && dialogueAsset.datas != null)
        {
            foreach (var data in dialogueAsset.datas)
            {
                dialogueDataList.Add(data);
            }
        }
        else
        {
            Debug.LogError("SimpleDialogue: 对话资产为空或没有数据！");
        }
    }

    void Start()
    {
        if (gameObject.activeSelf)
        {
            StartDialogue();
        }
    }

    public void StartDialogue()
    {
        if (isActive) return;
        if (dialogueDataList.Count == 0) return;
    if (energyDrain != null) energyDrain.SetPause(true);
    SafetyTimer timer = FindObjectOfType<SafetyTimer>();
    if (timer != null) timer.PauseTimer();

        isActive = true;
        currentIndex = 0;
        isTyping = false;
        waitingForChoice = false;

        if (panelWidget != null)
        {
            panelWidget.RenderOpacity = 0;
            panelWidget.Fade(1, fadeDuration, null);
        }
        if (textComponent != null) textComponent.Clear();

        DisablePlayerControl();
        NextSentence();
    }

    void Update()
    {
        if (!isActive || waitingForChoice) return;
        if (!autoNext && !isTyping && Input.GetButtonDown("Submit"))
        {
            NextSentence();
        }
    }

    void NextSentence()
    {
        if (isTyping || waitingForChoice) return;

        if (currentIndex < dialogueDataList.Count)
        {
            var data = dialogueDataList[currentIndex];
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
                StartCoroutine(PrintCurrentSentence(data.Content, data.charDelay));
                currentIndex++;
            }
        }
        else
        {
            EndDialogue();
        }
    }

    void ShowChoices(DialogueChoice[] choices)
    {
        waitingForChoice = true;

        var uiChoiceDatas = new ChoiceData[choices.Length];
        for (int i = 0; i < choices.Length; i++)
        {
            uiChoiceDatas[i] = new ChoiceData { Content = choices[i].Content };
        }

        Action<int> onChoiceConfirm = (selectedIndex) =>
        {
            waitingForChoice = false;
            var selected = choices[selectedIndex];
            selected.onSelect?.Invoke();

            if (selected.nextDialogue != null)
            {
                dialogueAsset = selected.nextDialogue;
                ReloadDialogueData();
                currentIndex = 0;
                textComponent.Clear();
                NextSentence();
            }
            else
            {
                EndDialogue();
            }
        };

        UIManager.CreateDialogueChocies(uiChoiceDatas, onChoiceConfirm, 0);
    }

    void ReloadDialogueData()
    {
        dialogueDataList.Clear();
        if (dialogueAsset != null && dialogueAsset.datas != null)
        {
            foreach (var data in dialogueAsset.datas)
            {
                dialogueDataList.Add(data);
            }
        }
    }

    IEnumerator PrintCurrentSentence(string content, float customCharDelay)
    {
        isTyping = true;
        textComponent.Clear();
        yield return null;

        float originalDelay = textComponent.DefaultCharDelay;
        if (customCharDelay > 0)
            textComponent.DefaultCharDelay = customCharDelay;

        bool finished = false;
        textComponent.OnFinished = () => finished = true;
        textComponent.StartCoroutine(textComponent.SetText(content, AdvancedText.DisplayType.Typing));
        yield return new WaitUntil(() => finished);

        textComponent.DefaultCharDelay = originalDelay;

        isTyping = false;
        if (autoNext) NextSentence();
    }

    void EndDialogue()
    {
            if (energyDrain != null) energyDrain.SetPause(false);
            SafetyTimer timer = FindObjectOfType<SafetyTimer>();
    if (timer != null) timer.ResumeTimer();
        if (nextButton != null) nextButton.interactable = false;
        if (panelWidget != null)
        {
            panelWidget.Fade(0, fadeDuration, () =>
            {
                gameObject.SetActive(false);
                isActive = false;
                waitingForChoice = false;
                EnablePlayerControl();
            });
        }
        else
        {
            gameObject.SetActive(false);
            isActive = false;
            EnablePlayerControl();
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
                {
                    scriptsToDisable[i].enabled = originalScriptStates[i];
                }
            }
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = originalRigidbodyKinematic;
        }
    }
}