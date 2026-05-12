using UnityEngine;
using TMPro;
using UnityEngine.UI;  
using UnityEngine.Events;
using System.Collections;
public class ShortcutBuilder : MonoBehaviour
{
    [Header("建造消耗")]
    [SerializeField] private int shardCost = 3;
    [SerializeField] private float energyCost = 20f;

    [Header("交互设置")]
    [SerializeField] private KeyCode buildKey = KeyCode.R;
    [SerializeField] private string playerTag = "Player";

    [Header("游戏对象")]
    [SerializeField] private GameObject verticalWall;
    [SerializeField] private GameObject promptObject;      // 按R提示
    [SerializeField] private GameObject warningText;       // 不足提示

    [Header("提示文本设置")]
    [SerializeField] private float warningDuration = 1.5f;

    private bool playerInRange = false;
    private bool alreadyBuilt = false;
    private EnergyManager energyManager;

    private void Start()
    {
        energyManager = EnergyManager.Instance;
        if (energyManager == null)
            Debug.LogError("ShortcutBuilder: 未找到 EnergyManager！");

        if (verticalWall != null) verticalWall.SetActive(false);
        if (promptObject != null) promptObject.SetActive(false);
        if (warningText != null) warningText.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (alreadyBuilt) return;

        playerInRange = true;
        // 仅当警告未显示时才显示提示
        if (promptObject != null && (!warningText || !warningText.activeSelf))
            promptObject.SetActive(true);
        Debug.Log($"进入捷径区域，按 {buildKey} 建造捷径");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        if (promptObject != null && !alreadyBuilt)
            promptObject.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (alreadyBuilt) return;
        if (Input.GetKeyDown(buildKey))
        {
            TryBuildShortcut();
        }
    }

    private void TryBuildShortcut()
    {
        if (energyManager == null) return;

        bool hasShard = energyManager.currentShard >= shardCost;
        bool hasEnergy = energyManager.currentEnergy >= energyCost;

        if (!hasShard || !hasEnergy)
        {
            ShowWarningMessage();
            return;
        }

        // 资源足够，建造
        energyManager.ConsumeShard(shardCost);
        energyManager.ConsumeEnergy(energyCost);

        if (verticalWall != null)
            verticalWall.SetActive(true);

        alreadyBuilt = true;
        if (promptObject != null)
            promptObject.SetActive(false);

        Debug.Log($"捷径已建造！消耗 {shardCost} 碎片 + {energyCost} 灵能");
    }

    private void ShowWarningMessage()
    {
        if (warningText == null) return;

        // 隐藏常规提示，避免重叠
        if (promptObject != null)
            promptObject.SetActive(false);

        warningText.SetActive(true);
        TextMeshProUGUI tmp = warningText.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            if (energyManager.currentShard < shardCost && energyManager.currentEnergy < energyCost)
                tmp.text = "灵能与碎片不足！";
            else if (energyManager.currentShard < shardCost)
                tmp.text = $"碎片不足（需要 {shardCost}）";
            else if (energyManager.currentEnergy < energyCost)
                tmp.text = $"灵能不足（需要 {energyCost}）";
        }

        CancelInvoke(nameof(HideWarning));
        Invoke(nameof(HideWarning), warningDuration);
    }

    private void HideWarning()
    {
        if (warningText != null)
            warningText.SetActive(false);

        // 重新显示常规提示，前提是玩家仍在区域内且尚未建造
        if (playerInRange && !alreadyBuilt && promptObject != null)
            promptObject.SetActive(true);
    }
}