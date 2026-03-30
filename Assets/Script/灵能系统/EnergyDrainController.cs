using UnityEngine;

public class EnergyDrainController : MonoBehaviour
{
    [Header("必要引用")]
    [SerializeField] private EnergyManager energyManager;
    [SerializeField] private MaskControl maskControl;
    [SerializeField] private SafetyTimer safetyTimer;
    [SerializeField] private Transform playerTransform;

    [Header("Y 轴区域判定")]
    [SerializeField] private float undergroundThreshold = 0f;

    [Header("流失速率（灵能/秒）")]
    [SerializeField] private float normal_AboveGround = 2f;
    [SerializeField] private float normal_Underground_Safe = 0f;
    [SerializeField] private float normal_Underground_Danger = 5f;
    [SerializeField] private float enhanced_AboveGround = 4f;
    [SerializeField] private float enhanced_Underground_Safe = 2f;
    [SerializeField] private float enhanced_Underground_Danger = 8f;

    private float currentDrainRate = 0f;
    private bool isDead = false;
    private bool isPaused = false;      // 手动暂停标志（用于对话等）

    private void Start()
    {
        if (energyManager == null) energyManager = EnergyManager.Instance;
        if (maskControl == null) maskControl = GetComponent<MaskControl>();
        if (safetyTimer == null) safetyTimer = FindObjectOfType<SafetyTimer>();
        if (playerTransform == null) playerTransform = transform;

        if (energyManager == null)
            Debug.LogError("EnergyDrainController: 未找到 EnergyManager！");
        if (maskControl == null)
            Debug.LogError("EnergyDrainController: 未找到 MaskControl！");
        if (safetyTimer == null)
            Debug.LogError("EnergyDrainController: 未找到 SafetyTimer！");
    }

    private void Update()
    {
        if (isPaused) return;  // 暂停时不进行流失

        if (energyManager == null || maskControl == null || safetyTimer == null) return;
        if (energyManager.currentEnergy <= 0f) return;
        if (isDead) return;

        UpdateDrainRate();
        if (currentDrainRate > 0f)
        {
            float drainAmount = currentDrainRate * Time.deltaTime;
            bool success = energyManager.ConsumeEnergy(drainAmount);
            if (!success)
            {
                EndingManager.Instance?.TriggerDeathEnding();
                isDead = true;
            }
        }
    }

    private void UpdateDrainRate()
    {
        bool isUnderground = playerTransform.position.y < undergroundThreshold;
        bool isSafetyZero = (safetyTimer.GetCurrentTime() <= 0f);
        bool isEnhanced = (maskControl.currentState == MaskControl.State.Enhanced);

        if (isEnhanced)
        {
            if (isUnderground)
            {
                currentDrainRate = isSafetyZero ? enhanced_Underground_Danger : enhanced_Underground_Safe;
            }
            else
            {
                currentDrainRate = enhanced_AboveGround;
            }
        }
        else
        {
            if (isUnderground)
            {
                currentDrainRate = isSafetyZero ? normal_Underground_Danger : normal_Underground_Safe;
            }
            else
            {
                currentDrainRate = normal_AboveGround;
            }
        }
    }

    /// <summary>
    /// 设置暂停状态（用于对话等需要暂停灵能流失的场合）
    /// </summary>
    public void SetPause(bool pause)
    {
        isPaused = pause;
        Debug.Log($"灵能流失 {(isPaused ? "已暂停" : "已恢复")}");
    }

    public void ResetDeathFlag() => isDead = false;
}