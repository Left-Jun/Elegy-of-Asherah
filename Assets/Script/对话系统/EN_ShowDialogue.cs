using System;
using UnityEngine;
using TMPro;

[Serializable]
public class DialogueData
{
    [Header("基本信息")] public string Speaker; [Multiline] public string Content;
    [Header("显示控制")] public bool AutoNext = false; public bool NeedTyping = true; public bool CanQuickShow = true;public float displayDelay = 0f;   // 新增：文字显示后等待多少秒再自动下一句（仅当 AutoNext=true 时生效）
    [Header("文字样式")] public Color textColor = Color.white; public float fontSize = 36f; public FontStyles fontStyle = FontStyles.Normal;
    [Header("打字机设置")] public float charDelay = -1f;
    [Header("对话框样式")] public int boxStyle = 0;
    [Header("分支选项")] public DialogueChoice[] choices;
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

    public override void Init(Action<bool> onFinishedEvent) { base.Init(onFinishedEvent); _onFinished = onFinishedEvent; }
    public override void Execute() { base.Execute(); _index = 0; UIManager.OpenDialogueBox(ShowNextDialogue, boxStyle); }
    private void ShowNextDialogue(bool forceDisplayDirectly)
    {
        if (_index < datas.Length)
        {
            var cur = datas[_index];
            var printData = new DialogueData
            {
                Speaker = cur.Speaker, Content = cur.Content, AutoNext = cur.AutoNext,
                NeedTyping = !forceDisplayDirectly && cur.NeedTyping, CanQuickShow = cur.CanQuickShow,
                textColor = cur.textColor, fontSize = cur.fontSize, fontStyle = cur.fontStyle,
                charDelay = cur.charDelay, choices = cur.choices
            };
            UIManager.PrintDialogue(printData);
            _index++;
        }
        else { state = NodeState.Finished; _onFinished?.Invoke(true); }
    }
}