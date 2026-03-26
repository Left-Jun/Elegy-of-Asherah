using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
/// <summary>
/// 对话数据（可序列化）
/// </summary>
[Serializable]
public class DialogueData
{
    public string Speaker;
    [Multiline] public string Content;
    public bool AutoNext;
    public bool NeedTyping;
    public bool CanQuickShow;
    public Color textColor = Color.white;
    public float fontSize = 36f;
    public FontStyles fontStyle = FontStyles.Normal;

    // 选项分支（可选）
    public DialogueChoice[] choices;
}

[Serializable]
public class DialogueChoice
{
    public string Content;                   // 选项文本
    public EN_ShowDialogue nextDialogue;     // 选择后跳转的对话资产
}
/// <summary>
/// 显示对话的事件节点
/// </summary>
[CreateAssetMenu(fileName = "Node_", menuName = "Event/Message/Show Dialogue")]
public class EN_ShowDialogue : EventNodeBase
{
    public DialogueData[] datas;            // 对话数据数组（多句）
    public int boxStyle = 0;                // 对话框样式索引
    private int _index;

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
            // 注意：这里创建了一个新的DialogueData实例，但实际应该直接使用datas[_index]
            // 因为Datas[_index]已经是可序列化的，但为了避免修改原始资产，可以克隆。
            // 这里保持原样，但建议直接使用datas[_index]并设置NeedTyping。
            DialogueData data = new DialogueData()
            {
                Speaker = datas[_index].Speaker,
                Content = datas[_index].Content,
                CanQuickShow = datas[_index].CanQuickShow,
                AutoNext = datas[_index].AutoNext,
                NeedTyping = !forceDisplayDirectly && datas[_index].NeedTyping
            };
            UIManager.PrintDialogue(data);
            _index++;
        }
        else
        {
            state = NodeState.Finished;
            OnFinished?.Invoke(true);
        }
    }
}