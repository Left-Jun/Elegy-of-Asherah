using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CharacterLightController : MonoBehaviour
{
    [Header("光源引用")]
    [SerializeField] private Light2D characterLight;   // 角色的跟随光源（点光源或聚光灯）

    [Header("不同状态的光照范围")]
    [SerializeField] private float normalRange = 2f;    // 普通状态下的光照半径/范围
    [SerializeField] private float enhancedRange = 4f;  // 强化状态下的光照半径/范围

    [Header("过渡速度")]
    [SerializeField] private float transitionSpeed = 3f; // 每秒变化速率

    private MaskControl maskControl;
    private float targetRange;

    private void Start()
    {
        if (characterLight == null)
            characterLight = GetComponent<Light2D>();
        
        if (characterLight == null)
        {
            Debug.LogError("CharacterLightController: 未找到 Light2D 组件！");
            enabled = false;
            return;
        }

        maskControl = FindObjectOfType<MaskControl>();
        if (maskControl == null)
        {
            Debug.LogWarning("CharacterLightController: 未找到 MaskControl，无法获取状态变化。");
        }
        else
        {
            // 监听状态变化事件
            maskControl.OnStateChangedEvent += OnStateChanged;
            // 初始设置目标范围
            OnStateChanged(maskControl.currentState);
        }

        // 初始直接设置范围，避免突变
        characterLight.pointLightOuterRadius = targetRange;
        // 如果是聚光灯，可能需要设置 spotLightAngle 等，这里以点光源为例
    }

    private void OnStateChanged(MaskControl.State newState)
    {
        targetRange = (newState == MaskControl.State.Normal) ? normalRange : enhancedRange;
    }

    private void Update()
    {
        // 平滑过渡光源范围（点光源的 outerRadius）
        float currentRange = characterLight.pointLightOuterRadius;
        if (Mathf.Abs(currentRange - targetRange) > 0.01f)
        {
            float newRange = Mathf.Lerp(currentRange, targetRange, transitionSpeed * Time.deltaTime);
            characterLight.pointLightOuterRadius = newRange;
        }
    }

    private void OnDestroy()
    {
        if (maskControl != null)
            maskControl.OnStateChangedEvent -= OnStateChanged;
    }
}