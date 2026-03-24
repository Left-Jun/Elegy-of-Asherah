using System;
using UnityEngine;

/// <summary>
/// 选项数据
/// </summary>
[Serializable]
public class ChoiceData
{
    public string Content;                 // 选项文本
    public bool bQuickLocate;              // 是否快速定位（未使用，可扩展）
    // 音效等可添加
}

/// <summary>
/// 显示选项分支的事件节点
/// </summary>
[CreateAssetMenu(fileName = "Node_", menuName = "Event/Message/Show Choices")]
public class EN_ShowChoice : EventNodeBase
{
    public int DefaultSelectIndex = 0;                // 默认选中的选项索引
    public ChoiceData[] datas;                        // 选项列表
    public SequenceEventExecutor[] executors;         // 每个选项对应的后续事件执行器

    public override void Init(Action<bool> onFinishedEvent)
    {
        base.Init(onFinishedEvent);
        foreach (var item in executors)
        {
            if (item != null)
            {
                item.Init(OnFinished);
            }
        }
    }

    public override void Execute()
    {
        base.Execute();
        UIManager.CreateDialogueChocies(datas, OnChoiceConfirm, DefaultSelectIndex);
    }

    private void OnChoiceConfirm(int index)
    {
        if (index < executors.Length && executors[index] != null)
        {
            executors[index].Execute();
        }
        else
        {
            OnFinished?.Invoke(true);
        }
    }
}