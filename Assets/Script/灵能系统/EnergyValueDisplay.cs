using UnityEngine;
using TMPro;

public class EnergyValueDisplay : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private TextMeshProUGUI energyText;

    [Header("显示格式")]
    [SerializeField] private bool showPrefix = true;
    [SerializeField] private string prefix = "Energy: ";
    [SerializeField] private DisplayMode displayMode = DisplayMode.CurrentAndMax;
    [SerializeField] private int decimalPlaces = 0;               // 小数位数（仅数值模式有效）
    [SerializeField] private string emptyMessage = "Depleted";    // 灵能为0时显示的文本

    public enum DisplayMode
    {
        CurrentOnly,        // 仅显示当前值
        CurrentAndMax,      // 当前值/最大值
        Percentage          // 百分比（0-100%）
    }

    private EnergyManager energyManager;

    private void Awake()
    {
        if (energyText == null)
            energyText = GetComponent<TextMeshProUGUI>();

        if (energyText == null)
            Debug.LogError("EnergyValueDisplay: 没有找到 TextMeshProUGUI 组件，请手动赋值。");
    }

    private void Start()
    {
        energyManager = EnergyManager.Instance;
        if (energyManager == null)
        {
            Debug.LogError("EnergyValueDisplay: 场景中未找到 EnergyManager 实例！");
            return;
        }

        energyManager.OnEnergyChanged.AddListener(UpdateDisplay);
        UpdateDisplay(energyManager.currentEnergy, energyManager.maxEnergy);
    }

    private void UpdateDisplay(float currentEnergy, float maxEnergy)
    {
        if (energyText == null) return;

        // 灵能为0时显示自定义消息
        if (currentEnergy <= 0f && !string.IsNullOrEmpty(emptyMessage))
        {
            energyText.text = emptyMessage;
            return;
        }

        string valueText = "";
        switch (displayMode)
        {
            case DisplayMode.CurrentOnly:
                valueText = FormatValue(currentEnergy);
                break;
            case DisplayMode.CurrentAndMax:
                valueText = $"{FormatValue(currentEnergy)}/{FormatValue(maxEnergy)}";
                break;
            case DisplayMode.Percentage:
                float percent = maxEnergy > 0 ? (currentEnergy / maxEnergy) * 100f : 0f;
                valueText = $"{percent:F0}%";
                break;
        }

        if (showPrefix)
            energyText.text = prefix + valueText;
        else
            energyText.text = valueText;
    }

    private string FormatValue(float value)
    {
        if (decimalPlaces <= 0)
            return Mathf.FloorToInt(value).ToString();
        else
            return value.ToString($"F{decimalPlaces}");
    }

    private void OnDestroy()
    {
        if (energyManager != null)
            energyManager.OnEnergyChanged.RemoveListener(UpdateDisplay);
    }
}