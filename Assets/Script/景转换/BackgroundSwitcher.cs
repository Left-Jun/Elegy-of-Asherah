using UnityEngine;

public class BackgroundSwitcher : MonoBehaviour
{
    [Header("Y轴分界")]
    [SerializeField] private float threshold = 0f;          // 低于此值为地下，高于为地上
    [SerializeField] private Transform playerTransform;     // 若为空，自动查找Tag为"Player"的物体

    [Header("背景物体")]
    [SerializeField] private GameObject aboveGroundBackground;   // 地上背景
    [SerializeField] private GameObject underGroundBackground;   // 地下背景

    private bool isUnderground = false;

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else Debug.LogError("BackgroundSwitcher: 未找到玩家物体！");
        }

        // 初始根据玩家位置设置一次
        UpdateBackground();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        bool currentlyUnderground = playerTransform.position.y < threshold;
        if (currentlyUnderground != isUnderground)
        {
            isUnderground = currentlyUnderground;
            UpdateBackground();
        }
    }

    private void UpdateBackground()
    {
        if (aboveGroundBackground != null)
            aboveGroundBackground.SetActive(!isUnderground);
        if (underGroundBackground != null)
            underGroundBackground.SetActive(isUnderground);

        Debug.Log($"背景切换：{(isUnderground ? "地下" : "地上")}");
    }

    // 可选：供外部手动设置玩家引用
    public void SetPlayerTransform(Transform player) => playerTransform = player;
}