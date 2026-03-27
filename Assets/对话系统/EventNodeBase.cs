using System;
using UnityEngine;

/// <summary>
/// 事件节点状态
/// </summary>
public enum NodeState
{
    Waiting,     // 等待执行
    Executing,   // 执行中
    Finished     // 已完成
}

/// <summary>
/// 事件节点基类，所有具体事件都继承此类
/// </summary>
public class EventNodeBase : ScriptableObject
{
    protected Action<bool> OnFinished;      // 执行完成回调，参数表示是否成功
    [HideInInspector] public NodeState state;

    /// <summary>
    /// 初始化节点，设置完成回调
    /// </summary>
    public virtual void Init(Action<bool> onFinishedEvent)
    {
        OnFinished = onFinishedEvent;
        state = NodeState.Waiting;
    }

    /// <summary>
    /// 执行节点（由子类重写）
    /// </summary>
    public virtual void Execute()
    {
        if (state != NodeState.Waiting) return;
        state = NodeState.Executing;
    }
}