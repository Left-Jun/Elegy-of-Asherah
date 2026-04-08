using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class EnvironmentLightSwitcher : MonoBehaviour
{
    [Header("玩家参考")]
    [SerializeField] private Transform player;
    [SerializeField] private float yThreshold = 0f;

    [Header("地下光源组")]
    [SerializeField] private Light2D[] undergroundLights;

    [Header("地上光源组")]
    [SerializeField] private Light2D[] abovegroundLights;

    [Header("切换延迟（秒）")]
    [SerializeField] private float waitTime = 0.02f;   // 改用时间等待，更可靠

    private bool isSwitching = false;
    private bool isUnderground = true;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // 检查全局光冲突
        CheckGlobalLightConflicts();
        
        SetLightsByPositionImmediate();
    }

    private void Update()
    {
        if (player == null || isSwitching) return;
        bool nowUnderground = player.position.y < yThreshold;
        if (nowUnderground != isUnderground)
        {
            isUnderground = nowUnderground;
            StartCoroutine(SwitchLightsCoroutine(isUnderground));
        }
    }

    private IEnumerator SwitchLightsCoroutine(bool toUnderground)
    {
        isSwitching = true;

        // 1. 禁用所有光源
        SetAllLights(false);

        // 2. 等待一小段时间，确保渲染更新
        yield return new WaitForSeconds(waitTime);

        // 3. 启用目标组
        if (toUnderground)
            SetLightsActive(undergroundLights, true);
        else
            SetLightsActive(abovegroundLights, true);

        isSwitching = false;
        Debug.Log($"光照切换至 {(toUnderground ? "地下" : "地上")}");
    }

    private void SetAllLights(bool active)
    {
        SetLightsActive(undergroundLights, active);
        SetLightsActive(abovegroundLights, active);
    }

    private void SetLightsActive(Light2D[] lights, bool active)
    {
        if (lights == null) return;
        foreach (var light in lights)
        {
            if (light != null)
                light.enabled = active;
        }
    }

    private void SetLightsByPositionImmediate()
    {
        if (player == null) return;
        bool nowUnderground = player.position.y < yThreshold;
        isUnderground = nowUnderground;

        SetLightsActive(undergroundLights, nowUnderground);
        SetLightsActive(abovegroundLights, !nowUnderground);
    }

    private void CheckGlobalLightConflicts()
    {
        // 检查地下组和地上组中是否有多个全局光使用相同的 Blend Style
        var allLights = new System.Collections.Generic.List<Light2D>();
        if (undergroundLights != null) allLights.AddRange(undergroundLights);
        if (abovegroundLights != null) allLights.AddRange(abovegroundLights);

        foreach (var light in allLights)
        {
            if (light != null && light.lightType == Light2D.LightType.Global)
            {
                Debug.LogWarning($"全局光 {light.name} 使用了 Blend Style {light.blendStyleIndex}。确保没有其他同时启用的全局光使用相同索引，否则会报错。");
            }
        }
    }
}