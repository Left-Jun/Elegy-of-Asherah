using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskControl : MonoBehaviour
{
    public GameObject HappyVisual => happyVisual;
    [Header("平静状态平台")]
    [SerializeField] private List<GameObject> neutralPlatforms = new List<GameObject>();
    [Header("背景容器")]
    [SerializeField] private GameObject bgNeutral;
    [SerializeField] private GameObject bgHappy;
    [SerializeField] private GameObject bgAngry;
    public enum EmotionState { Neutral, Happy, Angry }//情绪状态枚举
    public EmotionState currentEmotion;//当前情绪状态
    private PlayerJump playerJump;//跳跃脚本引用
    private PlayerMove playerMove;//移动脚本引用
    private PlayerDash playerDash; // 冲刺脚本引用
    
    // 不同情绪下的参数
    [Header("视觉子对象")]
    [SerializeField] private GameObject neutralVisual;
    [SerializeField] private GameObject happyVisual; 
    [SerializeField] private GameObject angryVisual;
    
    [System.Serializable] public class EmotionStats
    {
        [Header("移动速度设置")]
        public float moveSpeed;//移动速度
        
        [Header("跳跃力设置")]
        public float jumpForce;//跳跃力
        public float airjumpForce;//空中跳跃力
        public float additionalJumpForce;//追加跳跃力
        public float wallJumpForceX;//蹬墙跳跃力
        public float wallJumpForceY;//蹬墙跳跃力
        
        [Header("追加跳跃时间及加速度设置")]
        public float additionalJumpTime;//追加跳跃时间
        public float additionalFallForce; //下落加速度
        
        [Header("冲刺设置")]
        public float dashSpeed = 30f; // 冲刺速度
        public float dashDuration = 0.15f; // 冲刺持续时间
        public float dashCooldown = 0.5f; // 冲刺冷却时间
        public int maxAirDashes = 1; // 空中冲刺最大次数
    }

    public EmotionStats currentStats;//当前情绪状态参数
    [SerializeField]private EmotionStats neutralStats;
    [SerializeField]private EmotionStats happyStats;
    [SerializeField]private EmotionStats angryStats;
    // 情绪变化事件
    public event System.Action<EmotionState> OnEmotionChangedEvent;
    
    
    void Start()
    {
        currentEmotion = EmotionState.Neutral;
        playerJump = GetComponent<PlayerJump>();
        playerMove = GetComponent<PlayerMove>();
        playerDash = GetComponent<PlayerDash>(); // 获取冲刺组件
        SetActiveVisual(currentEmotion); // 放在获取组件等初始化代码之后
        ApplyEmotionStats(); // 初始应用参数
        OnEmotionChangedEvent?.Invoke(currentEmotion);
        UpdatePlatformsVisibility();// 初始更新平台可见性
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SetEmotion(currentEmotion);
            //根据当前情绪设置参数
            ApplyEmotionStats();
            // 更新外观（比如换材质、触发粒子效果）
        }
    }
    // 设置情绪状态并触发事件
    public void SetEmotion(EmotionState rightnowEmotion)
    {
        if (rightnowEmotion == EmotionState.Neutral)
        {
            currentEmotion = EmotionState.Happy;
            SetActiveVisual(currentEmotion);
            OnEmotionChangedEvent?.Invoke(currentEmotion);
            UpdatePlatformsVisibility();
        }
        else if (rightnowEmotion == EmotionState.Happy)
        {
            currentEmotion = EmotionState.Angry;
            SetActiveVisual(currentEmotion);
            OnEmotionChangedEvent?.Invoke(currentEmotion);
            UpdatePlatformsVisibility();
        }
        else if (rightnowEmotion == EmotionState.Angry)
        {
            currentEmotion = EmotionState.Neutral;
            SetActiveVisual(currentEmotion);
            OnEmotionChangedEvent?.Invoke(currentEmotion);
            UpdatePlatformsVisibility();
        }
    }
    private void UpdatePlatformsVisibility()
    {
        bool shouldShow = (currentEmotion == EmotionState.Neutral);
    
        foreach (GameObject platform in neutralPlatforms)
        {
            if (platform != null)
            {
                // ==== 新增关键代码：在禁用平台前，检查并解除与玩家的父子关系 ====
                if (!shouldShow) // 只有在要隐藏平台时才需要检查
                {
                    // 检查玩家是否是这个平台的子对象
                    if (transform.IsChildOf(platform.transform))
                        {
                        Debug.LogWarning($"玩家是 {platform.name} 的子对象，正在解除父子关系...");
                        // 将玩家移出平台家族，设置到场景根层级
                        transform.SetParent(null, true); // 第二个参数 true 会保持玩家的世界坐标
                    }
                }   
                // 原有的启用/禁用逻辑
                platform.SetActive(shouldShow);
            }
    }
}

    // 应用当前情绪状态的参数到玩家组件
    public void ApplyEmotionStats()
    {
        switch (currentEmotion)
        {
            case EmotionState.Neutral:
                currentStats = neutralStats;
                break;
            case EmotionState.Happy:
                currentStats = happyStats;
                break;
            case EmotionState.Angry:
                currentStats = angryStats;
                break;
        }
        
        // 应用移动速度
        playerMove.moveSpeed = currentStats.moveSpeed;
        
        // 应用跳跃参数
        playerJump.jumpForce = currentStats.jumpForce;
        playerJump.airjumpForce = currentStats.airjumpForce;
        playerJump.additionalJumpForce = currentStats.additionalJumpForce;
        playerJump.additionalJumpTime = currentStats.additionalJumpTime;
        playerJump.additionalFallForce = currentStats.additionalFallForce;
        playerJump.wallJumpForceX = currentStats.wallJumpForceX;
        playerJump.wallJumpForceY = currentStats.wallJumpForceY;
        
        // 应用冲刺参数
        if (playerDash != null)
        {
            playerDash.SetDashSpeed(currentStats.dashSpeed);
            playerDash.AddAirDash(currentStats.maxAirDashes - playerDash.GetCurrentAirDashCount());
        }
    }
    
    // 根据情绪状态启用对应的视觉子对象
    private void SetActiveVisual(EmotionState targetEmotion)
    {
        // 1. 禁用所有视觉子对象
        if(neutralVisual != null) neutralVisual.SetActive(false);
        if(happyVisual != null) happyVisual.SetActive(false);
        if(angryVisual != null) angryVisual.SetActive(false);

        // 2. 禁用所有背景
        if(bgNeutral != null) bgNeutral.SetActive(false);
        if(bgHappy != null) bgHappy.SetActive(false);
        if(bgAngry != null) bgAngry.SetActive(false);
        
        // 3. 启用目标情绪的背景
        switch(targetEmotion)
        {
            case EmotionState.Neutral:
                if(neutralVisual != null) neutralVisual.SetActive(true);
                if(bgNeutral != null) bgNeutral.SetActive(true);
                break;
            case EmotionState.Happy:
                if(happyVisual != null) happyVisual.SetActive(true);
                if(bgHappy != null) bgHappy.SetActive(true);
                break;
            case EmotionState.Angry:
                if(angryVisual != null) angryVisual.SetActive(true);
                if(bgAngry != null) bgAngry.SetActive(true);
                break;
        }
    }
    
    // 获取当前激活的视觉子对象
    public GameObject GetActiveVisual()
    {
        switch(currentEmotion)
        {
            case EmotionState.Neutral: return neutralVisual;
            case EmotionState.Happy: return happyVisual;
            case EmotionState.Angry: return angryVisual;
            default: return null;
        }
    }

    // 获取当前激活的视觉子对象上的Animator组件
    public Animator GetCurrentActiveAnimator()
    {
        GameObject activeVisual = GetActiveVisual();
        if (activeVisual != null)
        {
            return activeVisual.GetComponent<Animator>();
        }
        return null;
    }
}