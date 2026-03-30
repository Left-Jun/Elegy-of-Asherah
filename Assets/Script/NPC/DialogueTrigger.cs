using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("触发设置")]
    [SerializeField] private string playerTag = "Player";          // 玩家标签
    [SerializeField] private bool disableOnExit = true;           // 离开区域时是否禁用物体
    [SerializeField] private GameObject[] objectsToActivate;      // 需要激活的物体列表（如对话面板、UI等）

    [Header("对话类型（可选）")]
    public DialogueType dialogueType = DialogueType.Normal;       // 枚举类型，便于对话系统区分

    

    public enum DialogueType
    {
        Normal,     // 普通对话
        Quest,      // 任务对话
        Shop,       // 商店界面
        Info        // 提示信息
    }

    private bool isPlayerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = true;
            SetObjectsActive(true);
            OnPlayerEnter();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = false;
            if (disableOnExit)
                SetObjectsActive(false);
            OnPlayerExit();
        }
    }

    /// <summary>
    /// 激活或禁用所有指定的物体
    /// </summary>
    private void SetObjectsActive(bool active)
    {
        foreach (var obj in objectsToActivate)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }

    /// <summary>
    /// 玩家进入时调用（可重写或通过事件扩展）
    /// </summary>
    protected virtual void OnPlayerEnter()
    {
        // 示例：触发对话系统
        // DialogueManager.Instance.StartDialogue(dialogueType);
        Debug.Log($"玩家进入触发器，对话类型：{dialogueType}");
    }

    /// <summary>
    /// 玩家离开时调用
    /// </summary>
    protected virtual void OnPlayerExit()
    {
        Debug.Log("玩家离开触发器区域");
    }

    // 可选：供外部手动控制（如按钮强制显示）
    public void ForceActivate(bool active)
    {
        // 如果玩家不在区域内且要激活，可自行决定是否执行（这里默认执行）
        SetObjectsActive(active);
    }
}