using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 高级按钮基类，扩展了选中效果和确认事件
/// </summary>
public class AdvancedButton : Button
{
    protected override void Awake()
    {
        onClick.AddListener(OnClickEvent);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        Select();   // 自动选中
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        UIManager.SetCurrentSelectable(this);
    }

    protected int _index;
    protected virtual void OnClickEvent() { }

    public Action<int> OnConfirm;          // 确认时触发，参数为按钮索引

    /// <summary>
    /// 初始化按钮
    /// </summary>
    public virtual void Init(string content, int index, Action<int> onConfirmEvent)
    {
        _index = index;
        OnConfirm += onConfirmEvent;
    }

    /// <summary>
    /// 外部调用确认
    /// </summary>
    public void Confirm()
    {
        OnConfirm?.Invoke(_index);
    }
}