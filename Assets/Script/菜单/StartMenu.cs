using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // 添加这行

public class Start : MonoBehaviour
{
    [Header("联系界面")]
    public GameObject contactPanel; // 在Inspector中拖入你的图片/面板
    public void GameStart()
    {
        SceneManager.LoadScene(1);
    }
    // 在Update中检测ESC键
    void Update()
    {
        // 按ESC键关闭联系面板
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (contactPanel != null && contactPanel.activeSelf)
            {
                contactPanel.SetActive(false);
            }
        }
    }

    public void GameQuit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
        // 新添加的方法
    public void ShowContactPanel()
    {
        if (contactPanel != null)
        {
            contactPanel.SetActive(true);
        }
    }
}
