using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 是否处于暂停状态（对话等）
    /// </summary>
    public bool IsPaused => isPaused;

    /// <summary>
    /// 暂停所有时间相关逻辑
    /// </summary>
    public void Pause()
    {
        isPaused = true;
        Debug.Log("TimeManager: 时间系统已暂停");
    }

    /// <summary>
    /// 恢复所有时间相关逻辑
    /// </summary>
    public void Resume()
    {
        isPaused = false;
        Debug.Log("TimeManager: 时间系统已恢复");
    }
}