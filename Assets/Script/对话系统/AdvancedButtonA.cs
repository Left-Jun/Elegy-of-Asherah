using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 带特效的选项按钮（高亮光圈、动画、光标跟随）
/// </summary>
public class AdvancedButtonA : AdvancedButton
{
    private Widget _frontRing;       // 高亮光圈组件
    private Animator _animator;      // 按钮动画

    protected override void Awake()
    {
        base.Awake();
        Transform frontRingTransform = transform.Find("Front Ring White");
        if (frontRingTransform != null)
            _frontRing = frontRingTransform.GetComponent<Widget>();
        else
            Debug.LogError("AdvancedButtonA: 'Front Ring White' child not found.");

        _animator = GetComponent<Animator>();
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        _frontRing?.Fade(1, 0.1f, null);
        UIManager.UpdateCursorA(transform.position);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        _frontRing?.Fade(0, 0.25f, null);
    }

    private static readonly int Click = Animator.StringToHash("Click");
    protected override void OnClickEvent()
    {
        base.OnClickEvent();
        _animator?.SetTrigger(Click);
        UIManager.ClickCursorA();
    }

    public override void Init(string content, int index, Action<int> onConfirmEvent)
    {
        base.Init(content, index, onConfirmEvent);
        AdvancedText text = GetComponentInChildren<AdvancedText>();
        if (text != null)
            text.StartCoroutine(text.SetText(content, AdvancedText.DisplayType.Default));
        else
            Debug.LogError("AdvancedButtonA: AdvancedText component not found in children.");
    }
}