using UnityEngine;
using UnityEngine.Events;

public class SafetyTimer : MonoBehaviour
{
    public static SafetyTimer Instance { get; private set; }

    [Header("计时器配置")]
    [SerializeField] private float initialTime = 120f;      // 初始安全时间（秒）
    [SerializeField] private float yThreshold = 0f;         // Y轴界限：低于此值（地下）倒计时，高于此值（地上）暂停

    [Header("玩家引用")]
    [SerializeField] private Transform playerTransform;      // 若为空，运行时自动查找Tag为"Player"的对象

    [Header("事件")]
    public UnityEvent<float, float> OnTimeChanged;          // 参数：当前时间，最大时间
    public UnityEvent OnTimeEnd;                            // 安全时间耗尽时触发

    // 公开只读属性，供外部获取初始时间（用于UI显示）
    public float InitialTime => initialTime;

    private float currentTime;
    private bool isRunning;          // 是否正在倒计时（且未暂停）
    private bool isPaused;           // 是否被手动暂停（如对话）

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
        ResetTimer();
    }

    private void Update()
    {
        if (TimeManager.Instance != null && TimeManager.Instance.IsPaused) return;
        if (!isRunning) return;
        if (isPaused) return;        // 手动暂停（对话等）时不倒计时

        bool isUnderground = IsPlayerUnderground();
        if (isUnderground)
        {
            currentTime -= Time.deltaTime;
            OnTimeChanged?.Invoke(currentTime, initialTime);

            if (currentTime <= 0f)
            {
                currentTime = 0f;
                isRunning = false;
                OnTimeEnd?.Invoke();
            }
        }
        // 地上时：时间暂停（不处理任何逻辑）
    }

    /// <summary>
    /// 判断玩家是否在地下（Y坐标小于界限）
    /// </summary>
    private bool IsPlayerUnderground()
    {
        if (playerTransform == null) return false;
        return playerTransform.position.y < yThreshold;
    }

    /// <summary>
    /// 重置计时器到初始值（并重新开始倒计时）
    /// </summary>
    public void ResetTimer()
    {
        currentTime = initialTime;
        isRunning = true;
        isPaused = false;
        OnTimeChanged?.Invoke(currentTime, initialTime);
    }

    /// <summary>
    /// 增加安全时间（例如捡到加时器道具时调用）
    /// </summary>
    /// <param name="seconds">增加的秒数</param>
    public void AddTime(float seconds)
    {
        if (seconds <= 0) return;

        currentTime += seconds;
        OnTimeChanged?.Invoke(currentTime, initialTime);

        // 如果之前时间已耗尽（isRunning == false），现在有剩余时间了，重新启动倒计时
        if (!isRunning && currentTime > 0f)
        {
            isRunning = true;
        }
    }

    /// <summary>
    /// 手动设置剩余时间（不触发时间耗尽事件，用于调试或特殊情况）
    /// </summary>
    public void SetTime(float time)
    {
        currentTime = Mathf.Max(0f, time);
        isRunning = (currentTime > 0f && !isPaused);
        OnTimeChanged?.Invoke(currentTime, initialTime);
    }

    /// <summary>
    /// 获取当前剩余时间
    /// </summary>
    public float GetCurrentTime() => currentTime;

    /// <summary>
    /// 暂停计时器（用于对话等）
    /// </summary>
    public void PauseTimer()
    {
        if (!isRunning) return;
        isPaused = true;
        Debug.Log("SafetyTimer: 计时器已暂停");
    }

    /// <summary>
    /// 恢复计时器
    /// </summary>
    public void ResumeTimer()
    {
        if (!isRunning) return;
        isPaused = false;
        Debug.Log("SafetyTimer: 计时器已恢复");
    }

    /// <summary>
    /// 外部调用：设置玩家引用（若运行时才生成玩家）
    /// </summary>
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
