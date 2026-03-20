using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameTimer : MonoBehaviour
{
    private float startTime; // 游戏开始的时间点
    private float endTime;   // 游戏结束的时间点
    private bool isRunning = false; // 计时器是否正在运行
    private float pausedTime; // 暂停时的时间点（如果需要暂停功能）

    void Start()
    {
        // 游戏开始时自动开始计时
        StartTimer();
    }

    // 开始计时
    public void StartTimer()
    {
        startTime = Time.time;
        isRunning = true;
        Debug.Log("计时器开始: " + GetFormattedTime());
    }

    // 停止计时
    public void StopTimer()
    {
        if (isRunning)
        {
            endTime = Time.time;
            isRunning = false;
            Debug.Log("计时器停止: " + GetFormattedTime());
        }
    }

    // 重置计时器
    public void ResetTimer()
    {
        startTime = Time.time;
        endTime = 0;
        isRunning = true;
        Debug.Log("计时器重置");
    }

    // 获取从开始到现在的经过时间（如果计时器还在运行）或从开始到结束的时间（如果已停止）
    public float GetElapsedTime()
    {
        if (isRunning)
        {
            return Time.time - startTime;
        }
        else
        {
            return endTime - startTime;
        }
    }

    // 将时间转换为格式化的字符串（分钟:秒.毫秒）
    public string GetFormattedTime()
    {
        float elapsed = GetElapsedTime();
        int minutes = (int)elapsed / 60;
        int seconds = (int)elapsed % 60;
        int milliseconds = (int)((elapsed * 1000) % 1000);
        return string.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, milliseconds);
    }

    // 将时间转换为格式化的字符串（仅分钟:秒）
    public string GetFormattedTimeSimple()
    {
        float elapsed = GetElapsedTime();
        int minutes = (int)elapsed / 60;
        int seconds = (int)elapsed % 60;
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // 获取原始时间数据（秒，带小数）
    public float GetRawTime()
    {
        return GetElapsedTime();
    }

    // 在游戏对象被禁用时自动停止计时（可选）
    void OnDisable()
    {
        if (isRunning)
        {
            StopTimer();
        }
    }
}