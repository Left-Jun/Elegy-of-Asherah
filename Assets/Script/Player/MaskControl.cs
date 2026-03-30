using UnityEngine;

public class MaskControl : MonoBehaviour
{
    [Header("背景容器（按灵能阶段）")]
    [SerializeField] private GameObject[] normalBackgrounds;   // 索引0=高,1=中,2=低
    [SerializeField] private GameObject[] enhancedBackgrounds; // 索引0=高,1=中,2=低

    [Header("普通状态视觉子物体（左右独立，按灵能阶段）")]
    [SerializeField] private GameObject normalLeftVisual_High;
    [SerializeField] private GameObject normalRightVisual_High;
    [SerializeField] private GameObject normalLeftVisual_Medium;
    [SerializeField] private GameObject normalRightVisual_Medium;
    [SerializeField] private GameObject normalLeftVisual_Low;
    [SerializeField] private GameObject normalRightVisual_Low;

    [Header("强化状态视觉子物体（左右独立，按灵能阶段）")]
    [SerializeField] private GameObject enhancedLeftVisual_High;
    [SerializeField] private GameObject enhancedRightVisual_High;
    [SerializeField] private GameObject enhancedLeftVisual_Medium;
    [SerializeField] private GameObject enhancedRightVisual_Medium;
    [SerializeField] private GameObject enhancedLeftVisual_Low;
    [SerializeField] private GameObject enhancedRightVisual_Low;

    [Header("灵能阶段阈值（闭区间）")]
    [SerializeField] private float highThreshold = 80f;    // ≥ highThreshold 为高
    [SerializeField] private float mediumThreshold = 50f;  // [mediumThreshold, highThreshold) 为中

    public enum State { Normal, Enhanced }
    public State currentState;

    public enum EnergyStage { High, Medium, Low }

    public delegate void StateChangedHandler(State newState);
    public event StateChangedHandler OnStateChangedEvent;

    private PlayerJump playerJump;
    private PlayerMove playerMove;
    private PlayerDash playerDash;

    [System.Serializable]
    public class StateStats
    {
        public float moveSpeed;
        public float jumpForce;
        public float airjumpForce;
        public float additionalJumpForce;
        public float wallJumpForceX;
        public float wallJumpForceY;
        public float additionalJumpTime;
        public float additionalFallForce;
        public float dashSpeed = 30f;
        public float dashDuration = 0.15f;
        public float dashCooldown = 0.5f;
        public int maxAirDashes = 1;
    }

    public StateStats currentStats;
    [SerializeField] private StateStats normalStats;
    [SerializeField] private StateStats enhancedStats;

    private float lastFacing = 0f;
    private EnergyStage currentEnergyStage = EnergyStage.Medium;
    private EnergyManager energyManager;

    void Start()
    {
        currentState = State.Normal;
        playerJump = GetComponent<PlayerJump>();
        playerMove = GetComponent<PlayerMove>();
        playerDash = GetComponent<PlayerDash>();

        energyManager = EnergyManager.Instance;
        if (energyManager != null)
        {
            energyManager.OnEnergyChanged.AddListener(OnEnergyChanged);
            UpdateEnergyStage(energyManager.currentEnergy);
        }
        else
        {
            Debug.LogWarning("MaskControl: 未找到 EnergyManager，无法根据灵能切换视觉。");
        }

        UpdateDirectionalVisual();
        ApplyStateStats();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchState();
        }

        float currentFacing = GetCurrentFacing();
        if (Mathf.Abs(currentFacing - lastFacing) > 0.01f)
        {
            lastFacing = currentFacing;
            UpdateDirectionalVisual();
        }
    }

    void LateUpdate()
    {
        // 保险机制：每帧检查灵能阶段是否变化，防止事件丢失导致视觉卡死
        if (energyManager != null)
        {
            EnergyStage newStage = GetEnergyStage(energyManager.currentEnergy);
            if (newStage != currentEnergyStage)
            {
                currentEnergyStage = newStage;
                UpdateDirectionalVisual();
            }
        }
    }

    private void OnEnergyChanged(float currentEnergy, float maxEnergy)
    {
        EnergyStage newStage = GetEnergyStage(currentEnergy);
        if (newStage != currentEnergyStage)
        {
            currentEnergyStage = newStage;
            UpdateDirectionalVisual();
        }
    }

    private EnergyStage GetEnergyStage(float energy)
    {
        if (energy >= highThreshold)
            return EnergyStage.High;
        else if (energy >= mediumThreshold)
            return EnergyStage.Medium;
        else
            return EnergyStage.Low;
    }

    private void UpdateEnergyStage(float energy)
    {
        currentEnergyStage = GetEnergyStage(energy);
    }

    private void SwitchState()
    {
        currentState = (currentState == State.Normal) ? State.Enhanced : State.Normal;

        UpdateDirectionalVisual();
        ApplyStateStats();

        OnStateChangedEvent?.Invoke(currentState);
        Debug.Log($"状态切换为: {currentState}");
    }

    private void UpdateDirectionalVisual()
    {
        // 1. 更新角色视觉
        DisableAllVisuals();
        bool facingRight = GetCurrentFacing() > 0;
        GameObject targetVisual = GetTargetVisual(currentState, currentEnergyStage, facingRight);
        if (targetVisual != null)
            targetVisual.SetActive(true);

        // 2. 更新背景（根据状态和灵能阶段）
        UpdateBackground();
    }

    private void UpdateBackground()
    {
        int stageIndex = (int)currentEnergyStage; // High=0, Medium=1, Low=2

        // 先禁用所有背景
        if (normalBackgrounds != null)
        {
            foreach (var bg in normalBackgrounds)
                if (bg != null) bg.SetActive(false);
        }
        if (enhancedBackgrounds != null)
        {
            foreach (var bg in enhancedBackgrounds)
                if (bg != null) bg.SetActive(false);
        }

        // 激活当前状态对应的背景
        if (currentState == State.Normal)
        {
            if (normalBackgrounds != null && stageIndex < normalBackgrounds.Length && normalBackgrounds[stageIndex] != null)
                normalBackgrounds[stageIndex].SetActive(true);
        }
        else
        {
            if (enhancedBackgrounds != null && stageIndex < enhancedBackgrounds.Length && enhancedBackgrounds[stageIndex] != null)
                enhancedBackgrounds[stageIndex].SetActive(true);
        }
    }

    private void DisableAllVisuals()
    {
        // 普通状态
        if (normalLeftVisual_High != null) normalLeftVisual_High.SetActive(false);
        if (normalRightVisual_High != null) normalRightVisual_High.SetActive(false);
        if (normalLeftVisual_Medium != null) normalLeftVisual_Medium.SetActive(false);
        if (normalRightVisual_Medium != null) normalRightVisual_Medium.SetActive(false);
        if (normalLeftVisual_Low != null) normalLeftVisual_Low.SetActive(false);
        if (normalRightVisual_Low != null) normalRightVisual_Low.SetActive(false);

        // 强化状态
        if (enhancedLeftVisual_High != null) enhancedLeftVisual_High.SetActive(false);
        if (enhancedRightVisual_High != null) enhancedRightVisual_High.SetActive(false);
        if (enhancedLeftVisual_Medium != null) enhancedLeftVisual_Medium.SetActive(false);
        if (enhancedRightVisual_Medium != null) enhancedRightVisual_Medium.SetActive(false);
        if (enhancedLeftVisual_Low != null) enhancedLeftVisual_Low.SetActive(false);
        if (enhancedRightVisual_Low != null) enhancedRightVisual_Low.SetActive(false);
    }

    private GameObject GetTargetVisual(State state, EnergyStage stage, bool facingRight)
    {
        if (state == State.Normal)
        {
            switch (stage)
            {
                case EnergyStage.High:   return facingRight ? normalRightVisual_High : normalLeftVisual_High;
                case EnergyStage.Medium: return facingRight ? normalRightVisual_Medium : normalLeftVisual_Medium;
                case EnergyStage.Low:    return facingRight ? normalRightVisual_Low : normalLeftVisual_Low;
            }
        }
        else
        {
            switch (stage)
            {
                case EnergyStage.High:   return facingRight ? enhancedRightVisual_High : enhancedLeftVisual_High;
                case EnergyStage.Medium: return facingRight ? enhancedRightVisual_Medium : enhancedLeftVisual_Medium;
                case EnergyStage.Low:    return facingRight ? enhancedRightVisual_Low : enhancedLeftVisual_Low;
            }
        }
        return null;
    }

    private float GetCurrentFacing()
    {
        if (playerMove != null && playerMove.transform != null)
            return playerMove.transform.localScale.x;
        return transform.localScale.x;
    }

    public void ApplyStateStats()
    {
        currentStats = (currentState == State.Normal) ? normalStats : enhancedStats;

        if (playerMove != null) playerMove.moveSpeed = currentStats.moveSpeed;
        if (playerJump != null)
        {
            playerJump.jumpForce = currentStats.jumpForce;
            playerJump.airjumpForce = currentStats.airjumpForce;
            playerJump.additionalJumpForce = currentStats.additionalJumpForce;
            playerJump.additionalJumpTime = currentStats.additionalJumpTime;
            playerJump.additionalFallForce = currentStats.additionalFallForce;
            playerJump.wallJumpForceX = currentStats.wallJumpForceX;
            playerJump.wallJumpForceY = currentStats.wallJumpForceY;
        }
        if (playerDash != null)
        {
            playerDash.SetDashSpeed(currentStats.dashSpeed);
            playerDash.AddAirDash(currentStats.maxAirDashes - playerDash.GetCurrentAirDashCount());
        }
    }

    public GameObject GetActiveVisual()
    {
        bool facingRight = GetCurrentFacing() > 0;
        return GetTargetVisual(currentState, currentEnergyStage, facingRight);
    }

    public Animator GetCurrentActiveAnimator()
    {
        GameObject activeVisual = GetActiveVisual();
        return activeVisual != null ? activeVisual.GetComponent<Animator>() : null;
    }

    public void SetState(State newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            UpdateDirectionalVisual();
            ApplyStateStats();
            OnStateChangedEvent?.Invoke(currentState);
        }
    }

    public EnergyStage GetCurrentEnergyStage() => currentEnergyStage;

    private void OnDestroy()
    {
        if (energyManager != null)
            energyManager.OnEnergyChanged.RemoveListener(OnEnergyChanged);
    }
}