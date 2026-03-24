using System;
using UnityEngine;

/// <summary>
/// 顺序事件执行器，按顺序执行一组事件节点
/// </summary>
[CreateAssetMenu(fileName = "Executor_", menuName = "Event/Sequence Executor")]
public class SequenceEventExecutor : ScriptableObject
{
    public Action<bool> OnFinished;      // 执行完成回调，参数表示是否全部成功
    private int _index;
    public EventNodeBase[] nodes;        // 待执行的事件节点列表

    /// <summary>
    /// 初始化执行器，初始化所有子节点
    /// </summary>
    public void Init(Action<bool> onFinishedEvent)
    {
        _index = 0;
        foreach (EventNodeBase item in nodes)
        {
            if (item != null)
            {
                item.Init(OnNodeFinished);
            }
        }
        OnFinished = onFinishedEvent;
    }

    private void OnNodeFinished(bool success)
    {
        if (success)
        {
            ExecuteNextNode();
        }
        else
        {
            OnFinished?.Invoke(false);
        }
    }

    private void ExecuteNextNode()
    {
        if (_index < nodes.Length)
        {
            if (nodes[_index] != null && nodes[_index].state == NodeState.Waiting)
            {
                _index++;                     // 先增加索引，再执行前一个节点
                nodes[_index - 1].Execute();  // 执行当前节点
            }
            else
            {
                // 节点无效，直接跳过
                _index++;
                ExecuteNextNode();
            }
        }
        else
        {
            OnFinished?.Invoke(true);
        }
    }

    /// <summary>
    /// 开始执行
    /// </summary>
    public void Execute()
    {
        foreach (var node in nodes)
        {
            if (node != null)
                node.state = NodeState.Waiting;
        }
        _index = 0;
        ExecuteNextNode();
    }
}