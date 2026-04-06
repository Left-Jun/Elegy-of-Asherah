using UnityEngine;

public class AttachmentImage : MonoBehaviour
{
    public static bool IsShowing { get; private set; } = false;

    private float previousTimeScale = 1f;

    private void OnEnable()
    {
        // 记录当前时间缩放
        previousTimeScale = Time.timeScale;
        // 暂停游戏时间
        Time.timeScale = 0f;
        IsShowing = true;
        Debug.Log("告示牌显示，游戏时间暂停");
    }

    private void OnDisable()
    {
        // 恢复游戏时间
        Time.timeScale = previousTimeScale;
        IsShowing = false;
        Debug.Log("告示牌关闭，游戏时间恢复");
    }

    private void Update()
    {
        // 按 ESC 键关闭图片
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
        }
    }
}