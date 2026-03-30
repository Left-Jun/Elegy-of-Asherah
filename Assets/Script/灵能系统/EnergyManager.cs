using UnityEngine;
using UnityEngine.Events;

public class EnergyManager : MonoBehaviour
{
    public static EnergyManager Instance { get; private set; }

    [Header("当前数值")]
    public float currentEnergy;
    public int currentShard;

    [Header("上限配置")]
    public float maxEnergy = 100f;
    public int maxShard = 99;

    [Header("开局配置")]
    [SerializeField] private float startingEnergy = 100f;   // 开局灵能值
    [SerializeField] private int startingShard = 0;         // 开局碎片数量  ← 新增

    [Header("事件")]
    public UnityEvent<float, float> OnEnergyChanged;
    public UnityEvent<int, int> OnShardChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 初始化数值
        currentEnergy = Mathf.Clamp(startingEnergy, 0f, maxEnergy);
        currentShard = Mathf.Clamp(startingShard, 0, maxShard);   // 新增
        TriggerEvents();
    }

    public void AddEnergy(float amount)
    {
        if (amount <= 0) return;
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
        TriggerEvents();
    }

    public bool ConsumeEnergy(float amount)
    {
        if (currentEnergy < amount) return false;
        currentEnergy -= amount;
        TriggerEvents();
        return true;
    }

    public void AddShard(int amount)
    {
        if (amount <= 0) return;
        currentShard = Mathf.Clamp(currentShard + amount, 0, maxShard);
        TriggerEvents();
    }

    public bool ConsumeShard(int amount)
    {
        if (currentShard < amount) return false;
        currentShard -= amount;
        TriggerEvents();
        return true;
    }

    public void SetEnergy(float value)
    {
        currentEnergy = Mathf.Clamp(value, 0, maxEnergy);
        TriggerEvents();
    }

    public void SetShard(int value)
    {
        currentShard = Mathf.Clamp(value, 0, maxShard);
        TriggerEvents();
    }

    private void TriggerEvents()
    {
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        OnShardChanged?.Invoke(currentShard, maxShard);
    }
}