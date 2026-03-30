using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("显示格式")]
    [SerializeField] private bool showLabel = true;
    [SerializeField] private string label = "Time: ";
    [SerializeField] private string timeFormat = "{0:00}:{1:00}";
    [SerializeField] private string timeOutMessage = "Time's Up!"; // 时间耗尽时完全显示此文本

    private SafetyTimer safetyTimer;

    private void Awake()
    {
        if (timerText == null)
            timerText = GetComponent<TextMeshProUGUI>();

        if (timerText == null)
            Debug.LogError("TimerDisplay: 没有找到 TextMeshProUGUI 组件，请手动赋值。");
    }

    private void Start()
    {
        safetyTimer = FindObjectOfType<SafetyTimer>();

        if (safetyTimer == null)
        {
            Debug.LogError("TimerDisplay: 场景中未找到 SafetyTimer 组件！");
            return;
        }

        safetyTimer.OnTimeChanged.AddListener(UpdateDisplay);
        UpdateDisplay(safetyTimer.GetCurrentTime(), safetyTimer.InitialTime);
    }

    private void UpdateDisplay(float currentTime, float maxTime)
    {
        if (timerText == null) return;

        // 时间耗尽：直接显示自定义消息，不拼接标签
        if (currentTime <= 0f)
        {
            timerText.text = timeOutMessage;
            return;
        }

        // 正常情况：格式化时间，可选添加标签
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        string timeString = string.Format(timeFormat, minutes, seconds);

        if (showLabel)
            timerText.text = label + timeString;
        else
            timerText.text = timeString;
    }

    private void OnDestroy()
    {
        if (safetyTimer != null)
            safetyTimer.OnTimeChanged.RemoveListener(UpdateDisplay);
    }
}