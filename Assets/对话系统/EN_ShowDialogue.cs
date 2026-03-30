using System;
using UnityEngine;
using UnityEngine.Events; 
using TMPro;
using System.Collections.Generic;

[Serializable]
public class DialogueData
{
    [Header("基本信息")]
    public string Speaker;
    [Multiline] public string Content;

    [Header("显示控制")]
    public bool AutoNext = false;
    public bool NeedTyping = true;
    public bool CanQuickShow = true;

    [Header("文字样式（可选）")]
    public Color textColor = Color.white;
    public float fontSize = 36f;
    public FontStyles fontStyle = FontStyles.Normal;

    [Header("打字机设置")]
    public float charDelay = -1f;   // 若 >0，则覆盖全局默认间隔

    [Header("分支选项（可选）")]
    public DialogueChoice[] choices;
}

[Serializable]
public class DialogueChoice
{
    public string Content;
    public EN_ShowDialogue nextDialogue;
    public UnityEngine.Events.UnityEvent onSelect;
}

[CreateAssetMenu(fileName = "Node_", menuName = "Event/Message/Show Dialogue")]
public class EN_ShowDialogue : EventNodeBase
{
    public DialogueData[] datas;
    public int boxStyle = 0;
    private int _index;
    private Action<bool> _onFinished;

    public override void Init(Action<bool> onFinishedEvent)
    {
        base.Init(onFinishedEvent);
        _onFinished = onFinishedEvent;
    }

    public override void Execute()
    {
        base.Execute();
        _index = 0;
        UIManager.OpenDialogueBox(ShowNextDialogue, boxStyle);
    }

    private void ShowNextDialogue(bool forceDisplayDirectly)
    {
        if (_index < datas.Length)
        {
            var currentData = datas[_index];
            var printData = new DialogueData
            {
                Speaker = currentData.Speaker,
                Content = currentData.Content,
                AutoNext = currentData.AutoNext,
                NeedTyping = !forceDisplayDirectly && currentData.NeedTyping,
                CanQuickShow = currentData.CanQuickShow,
                textColor = currentData.textColor,
                fontSize = currentData.fontSize,
                fontStyle = currentData.fontStyle,
                charDelay = currentData.charDelay,
                choices = currentData.choices
            };
            UIManager.PrintDialogue(printData);
            _index++;
        }
        else
        {
            state = NodeState.Finished;
            _onFinished?.Invoke(true);
        }
    }
}