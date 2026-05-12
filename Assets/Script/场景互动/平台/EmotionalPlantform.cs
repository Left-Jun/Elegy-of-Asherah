using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotionalPlatform : MonoBehaviour
{
    // 平台需要在哪种状态下激活
    public MaskControl.State activeInState = MaskControl.State.Normal;

    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();

        // 1. 找到玩家的状态控制中心
        MaskControl maskControl = FindObjectOfType<MaskControl>();
        
        if (maskControl != null)
        {
            // 2. 订阅状态变化事件（注意事件名称已改为 OnStateChangedEvent）
            maskControl.OnStateChangedEvent += OnStateChanged;
            // 3. 立即根据当前状态更新一次平台可见性
            OnStateChanged(maskControl.currentState);
        }
    }

    // 当状态变化时，这个函数会被调用
    private void OnStateChanged(MaskControl.State newState)
    {
        // 判断当前状态是否是我需要的状态
        bool shouldBeActive = (newState == activeInState);
        
        // 根据判断结果显示/隐藏平台
        if(spriteRenderer != null) spriteRenderer.enabled = shouldBeActive;
        if(platformCollider != null) platformCollider.enabled = shouldBeActive;
    }

    // 防止内存泄漏
    private void OnDestroy()
    {
        MaskControl maskControl = FindObjectOfType<MaskControl>();
        if (maskControl != null)
        {
            maskControl.OnStateChangedEvent -= OnStateChanged;
        }
    }
}