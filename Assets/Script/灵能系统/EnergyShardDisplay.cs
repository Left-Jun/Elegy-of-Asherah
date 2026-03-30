using UnityEngine;
using TMPro;

public class EnergyShardDisplay : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private TextMeshProUGUI shardText;

    [Header("显示格式")]
    [SerializeField] private bool showPrefix = true;
    [SerializeField] private string prefix = "Shards: ";
    [SerializeField] private bool showMax = false;
    [SerializeField] private string emptyMessage = "None";

    private EnergyManager energyManager;

    private void Awake()
    {
        if (shardText == null)
            shardText = GetComponent<TextMeshProUGUI>();

        if (shardText == null)
            Debug.LogError("EnergyShardDisplay: 没有找到 TextMeshProUGUI 组件，请手动赋值。");
    }

    private void Start()
    {
        energyManager = EnergyManager.Instance;
        if (energyManager == null)
        {
            Debug.LogError("EnergyShardDisplay: 场景中未找到 EnergyManager 实例！");
            return;
        }

        energyManager.OnShardChanged.AddListener(UpdateDisplay);
        UpdateDisplay(energyManager.currentShard, energyManager.maxShard);
    }

    private void UpdateDisplay(int currentShard, int maxShard)
    {
        if (shardText == null) return;

        if (currentShard == 0 && !string.IsNullOrEmpty(emptyMessage))
        {
            shardText.text = emptyMessage;
            return;
        }

        string shardValue = showMax ? $"{currentShard}/{maxShard}" : currentShard.ToString();

        if (showPrefix)
            shardText.text = prefix + shardValue;
        else
            shardText.text = shardValue;
    }

    private void OnDestroy()
    {
        if (energyManager != null)
            energyManager.OnShardChanged.RemoveListener(UpdateDisplay);
    }
}