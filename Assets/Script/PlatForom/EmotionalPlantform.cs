using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotionalPlatform : MonoBehaviour
{
    // 平台需要在哪种情绪下激活
    public MaskControl.EmotionState activeInEmotion = MaskControl.EmotionState.Neutral;

    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();

        // 1. 找到玩家的情绪控制中心
        MaskControl maskControl = FindObjectOfType<MaskControl>();
        
        if (maskControl != null)
        {
            // 2. 订阅情绪变化事件
            maskControl.OnEmotionChangedEvent += OnEmotionChanged;
            // 3. 立即根据当前情绪更新一次状态
            OnEmotionChanged(maskControl.currentEmotion);
        }
    }

    // 当情绪变化时，这个函数会被调用
    private void OnEmotionChanged(MaskControl.EmotionState newEmotion)
    {
        // 判断当前情绪是否是我需要的情绪
        bool shouldBeActive = (newEmotion == activeInEmotion);
        
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
            maskControl.OnEmotionChangedEvent -= OnEmotionChanged;
        }
    }
}
