using System;
using UnityEngine;
using TMPro;
using UnityEngine.Events;   

// ==================================================
// 对话数据定义
// ==================================================

/// <summary>
/// 单个对话条目的数据结构
/// </summary>
[Serializable]
public class DialogueData
{
    [Header("基本信息")]
    public string Speaker;                  // 说话人名称
    [Multiline] public string Content;      // 对话内容（支持TMP富文本标签）

    [Header("显示控制")]
    public bool AutoNext = false;           // 是否自动进入下一句（无需按键）
    public bool NeedTyping = true;          // 是否启用打字机效果
    public bool CanQuickShow = true;        // 是否允许按取消键快速显示全部文字

    [Header("文字样式（可选）")]
    public Color textColor = Color.white;   // 文字颜色
    public float fontSize = 36f;            // 字体大小
    public FontStyles fontStyle = FontStyles.Normal; // 字体样式（粗体、斜体等）

    [Header("分支选项（可选）")]
    public DialogueChoice[] choices;        // 如果非空，则显示选项面板
}

/// <summary>
/// 分支选项数据结构
/// </summary>
[Serializable]
public class DialogueChoice
{
    public string Content;                              // 选项显示文本
    public EN_ShowDialogue nextDialogue;                // 选择后跳转的对话资产
    public UnityEngine.Events.UnityEvent onSelect;      // 选择时触发的事件（可选）
}

// ==================================================
// 事件节点：显示对话
// ==================================================

/// <summary>
/// 显示对话的事件节点，可在 ScriptableObject 资产中配置多句对话
/// </summary>
[CreateAssetMenu(fileName = "Node_", menuName = "Event/Message/Show Dialogue")]
public class EN_ShowDialogue : EventNodeBase
{
    [Header("对话配置")]
    public DialogueData[] datas;            // 对话数组（支持多句）
    public int boxStyle = 0;                // 对话框样式索引（如果有多套背景）

    private int _index;                     // 当前正在显示的对话索引
    private Action<bool> _onFinished;       // 对话完成时的回调

    public override void Init(Action<bool> onFinishedEvent)
    {
        base.Init(onFinishedEvent);
        _onFinished = onFinishedEvent;
    }

    public override void Execute()
    {
        base.Execute();
        _index = 0;
        // 打开对话框，传入下一句的回调
        UIManager.OpenDialogueBox(ShowNextDialogue, boxStyle);
    }

    /// <summary>
    /// 显示下一句对话（由对话框点击“继续”时触发）
    /// </summary>
    /// <param name="forceDisplayDirectly">是否强制直接显示（跳过打字机）</param>
    private void ShowNextDialogue(bool forceDisplayDirectly)
    {
        if (_index < datas.Length)
        {
            var currentData = datas[_index];
            // 创建用于打印的数据（为了避免修改原资产，这里克隆一份）
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
                choices = currentData.choices   // 选项数据会传递给 SimpleDialogue 处理
            };
            UIManager.PrintDialogue(printData);
            _index++;
        }
        else
        {
            // 对话结束
            state = NodeState.Finished;
            _onFinished?.Invoke(true);
        }
    }
}