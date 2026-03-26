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

    private List<DialogueData> dialogueDataList = new List<DialogueData>();
    private int currentIndex = 0;
    private bool isTyping = false;
    private bool isActive = false;
    private bool waitingForChoice = false;

    void Awake()
    {
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
            // 应用样式
            if (textComponent != null)
            {
                textComponent.color = data.textColor;
                textComponent.fontSize = data.fontSize;
                textComponent.fontStyle = data.fontStyle;
            }
            if (speakerNameText != null)
                speakerNameText.text = data.Speaker;

            // 如果有选项，显示选项面板
            if (data.choices != null && data.choices.Length > 0)
            {
                ShowChoices(data.choices);
                currentIndex++; // 推进索引，避免重复显示同一句
            }
            else
            {
                StartCoroutine(PrintCurrentSentence(data.Content));
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

        // 准备 UIManager 需要的 ChoiceData 数组（仅文本）
        var uiChoiceDatas = new ChoiceData[choices.Length];
        for (int i = 0; i < choices.Length; i++)
        {
            uiChoiceDatas[i] = new ChoiceData { Content = choices[i].Content };
        }

        Action<int> onChoiceConfirm = (selectedIndex) =>
        {
            waitingForChoice = false;
            var selected = choices[selectedIndex];
            if (selected.nextDialogue != null)
            {
                // 切换对话资产并重新加载
                dialogueAsset = selected.nextDialogue;
                ReloadDialogueData();
                currentIndex = 0;
                textComponent.Clear();
                NextSentence();  // 开始新对话
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

    IEnumerator PrintCurrentSentence(string content)
    {
        isTyping = true;
        textComponent.Clear();
        yield return null;

        bool finished = false;
        textComponent.OnFinished = () => finished = true;
        textComponent.StartCoroutine(textComponent.SetText(content, AdvancedText.DisplayType.Typing));
        yield return new WaitUntil(() => finished);

        isTyping = false;
        if (autoNext) NextSentence();
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
            });
        }
        else
        {
            gameObject.SetActive(false);
            isActive = false;
        }
    }
}