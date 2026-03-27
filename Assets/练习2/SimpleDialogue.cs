using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SimpleDialogue : MonoBehaviour
{
    [Header("组件引用")]
    public AdvancedText textComponent;          // 文字组件（AdvancedText）
    public Widget panelWidget;                  // 对话框的Widget（用于淡入淡出）
    public Button nextButton;                   // 继续按钮（可选）
    public TextMeshProUGUI speakerNameText;     // 说话人名字（可选）

    [Header("对话资产")]
    public EN_ShowDialogue dialogueAsset;       // 对话资产（包含多句对话数据）

    [Header("设置")]
    public bool autoNext = false;               // 是否自动下一句
    public float fadeDuration = 0.3f;           // 淡入淡出时间

    [Header("角色控制")]
    public MonoBehaviour[] scriptsToDisable;    // 需要禁用的脚本（移动、跳跃等）
    public Rigidbody2D playerRigidbody;         // 2D 刚体组件（可选，用于停止物理运动）

    private List<DialogueData> dialogueDataList = new List<DialogueData>();
    private int currentIndex = 0;
    private bool isTyping = false;
    private bool isActive = false;
    private bool waitingForChoice = false;
    private bool[] originalScriptStates;        // 保存脚本原始启用状态
    private bool originalRigidbodyKinematic;    // 保存刚体原始 isKinematic 状态

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

    /// <summary>
    /// 外部调用：开始对话
    /// </summary>
    public void StartDialogue()
    {
        if (isActive) return;
        if (dialogueDataList.Count == 0) return;

        isActive = true;
        currentIndex = 0;
        isTyping = false;
        waitingForChoice = false;

        // 淡入对话框
        if (panelWidget != null)
        {
            panelWidget.RenderOpacity = 0;
            panelWidget.Fade(1, fadeDuration, null);
        }
        if (textComponent != null) textComponent.Clear();

        // 禁用角色控制
        DisablePlayerControl();

        // 显示第一句
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
            selected.onSelect?.Invoke();   // 触发自定义事件

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
        yield return null;   // 等待一帧，避免闪现

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
                // 恢复角色控制
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

    // ==================== 角色控制方法 ====================

    private void DisablePlayerControl()
    {
        // 保存并禁用移动脚本
        originalScriptStates = new bool[scriptsToDisable.Length];
        for (int i = 0; i < scriptsToDisable.Length; i++)
        {
            if (scriptsToDisable[i] != null)
            {
                originalScriptStates[i] = scriptsToDisable[i].enabled;
                scriptsToDisable[i].enabled = false;
            }
        }

        // 处理 2D 刚体
        if (playerRigidbody != null)
        {
            originalRigidbodyKinematic = playerRigidbody.isKinematic;
            playerRigidbody.velocity = Vector2.zero;
            playerRigidbody.isKinematic = true;
        }
    }

    private void EnablePlayerControl()
    {
        // 恢复移动脚本
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

        // 恢复刚体状态
        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = originalRigidbodyKinematic;
        }
    }
}