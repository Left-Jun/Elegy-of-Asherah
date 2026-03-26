using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public GameObject dialoguePanel;      // 对话框面板（挂载 SimpleDialogue 的物体）
    public KeyCode triggerKey = KeyCode.E; // 触发按键

    private SimpleDialogue simpleDialogue;

    void Start()
    {
        if (dialoguePanel != null)
        {
            simpleDialogue = dialoguePanel.GetComponent<SimpleDialogue>();
            dialoguePanel.SetActive(false); // 确保初始关闭
        }
        else
        {
            Debug.LogError("DialogueTrigger: 未指定对话框面板！");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(triggerKey))
        {
            if (dialoguePanel != null && !dialoguePanel.activeSelf)
            {
                // 激活面板
                dialoguePanel.SetActive(true);
                // 开始对话
                if (simpleDialogue != null)
                {
                    simpleDialogue.StartDialogue();
                }
                else
                {
                    Debug.LogError("DialogueTrigger: 对话框面板上没有 SimpleDialogue 组件！");
                }
            }
        }
    }
}