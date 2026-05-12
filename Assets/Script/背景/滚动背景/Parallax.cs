using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private Transform cam;
    private Vector3 lastCamPos;
    [SerializeField] private float parallaxEffect;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;
    }

    // Update is called once per frame
    void Update()
    {
        // 计算摄像机移动的距离
        Vector3 deltaMovement = cam.position - lastCamPos;
        // 根据视差效果调整背景位置,只在x轴上移动
        transform.position += new Vector3(deltaMovement.x * parallaxEffect, 0, 0);
        lastCamPos = cam.position;
    }
}
