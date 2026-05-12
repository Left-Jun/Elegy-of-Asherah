using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SmoothGlobalLight : MonoBehaviour
{
    [Header("玩家参考")]
    [SerializeField] private Transform player;
    [SerializeField] private float yThreshold = 0f;   // 分界线，低于此值为地下

    [Header("地下光照参数")]
    [SerializeField] private Color undergroundColor = new Color(0.3f, 0.4f, 0.8f);
    [SerializeField] private float undergroundIntensity = 0.5f;

    [Header("地上光照参数")]
    [SerializeField] private Color abovegroundColor = new Color(1f, 0.9f, 0.7f);
    [SerializeField] private float abovegroundIntensity = 1.2f;

    [Header("过渡速度")]
    [SerializeField] private float transitionSpeed = 2f;   // 每秒变化量（单位：强度/颜色差值）

    private Light2D globalLight;
    private Color targetColor;
    private float targetIntensity;

    private void Start()
    {
        globalLight = GetComponent<Light2D>();
        if (globalLight == null)
        {
            Debug.LogError("SmoothGlobalLight: 未找到 Light2D 组件，请将此脚本挂载到带有 Light2D 的物体上。");
            enabled = false;
            return;
        }

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 初始目标值
        UpdateTargetByPlayerPosition();
        // 立即设置当前值为目标值（避免初始突变）
        globalLight.color = targetColor;
        globalLight.intensity = targetIntensity;
    }

    private void Update()
    {
        if (player == null) return;

        // 根据玩家位置更新目标值
        UpdateTargetByPlayerPosition();

        // 平滑过渡颜色
        globalLight.color = Color.Lerp(globalLight.color, targetColor, transitionSpeed * Time.deltaTime);
        // 平滑过渡强度
        globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, transitionSpeed * Time.deltaTime);
    }

    private void UpdateTargetByPlayerPosition()
    {
        bool isUnderground = player.position.y < yThreshold;
        if (isUnderground)
        {
            targetColor = undergroundColor;
            targetIntensity = undergroundIntensity;
        }
        else
        {
            targetColor = abovegroundColor;
            targetIntensity = abovegroundIntensity;
        }
    }
}