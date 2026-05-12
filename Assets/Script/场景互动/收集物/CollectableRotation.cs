using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CollectibleRotation : MonoBehaviour
{
    [Header("旋转设置")]
    [SerializeField] private float rotationSpeed = 90f; // 旋转速度（度/秒）
    [SerializeField] private Vector3 rotationAxis = Vector3.up; // 旋转轴，默认为Y轴

    [Header("浮动效果 (可选)")]
    [SerializeField] private bool enableFloating = false; // 是否启用浮动
    [SerializeField] private float floatAmplitude = 0.5f; // 浮动幅度
    [SerializeField] private float floatFrequency = 1f; // 浮动频率
    private Vector3 startPosition; // 记录初始位置

    void Start()
    {
        // 记录初始位置，用于浮动效果
        startPosition = transform.position;
    }

    void Update()
    {
        // 1. 核心：绕轴持续旋转
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.World);
        
        // 2. 可选：上下浮动效果（更生动）
        if (enableFloating)
        {
            Float();
        }
    }

    // 上下浮动方法
    void Float()
    {
        // 使用正弦函数计算一个平滑的上下位移
        float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}