using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SimplePauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    private bool isPaused = false;
    
    void Update()
    {
            // 如果告示牌图片正在显示，不处理暂停菜单的 ESC
        if (AttachmentImage.IsShowing) return;
        if (Input.GetKeyDown(KeyCode.Escape))//检测到Esc键按下
        {
            if (isPaused)//如果已经暂停
            {
                Resume();//继续游戏
            }
            else
            {
                Pause();//暂停游戏
            }
        }
    }
    
    //暂停游戏
    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }
    //继续游戏
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }
    //重新开始当前场景
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    //返回主菜单场景
    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
    //退出游戏
    public void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}