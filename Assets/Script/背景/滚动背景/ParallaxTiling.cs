using UnityEngine;

/// <summary>
/// 2D 无限循环视差背景脚本
/// 功能：自动铺满屏幕、基于材质偏移的无限循环、视差滚动
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxTiling : MonoBehaviour
{
    [Header("目标设置")]
    [Tooltip("跟随的目标（通常是玩家或摄像机）。留空则默认为主摄像机。")]
    public Transform followTarget;

    [Header("视差强度")]
    [Tooltip("0 = 静止 (如星空), 0.5 = 中速 (如远山), 1 = 同步 (如UI), >1 = 快速前景")]
    [Range(0f, 2f)]
    public float parallaxFactor = 0.5f;

    [Header("轴向控制")]
    public bool affectX = true;  // 是否影响水平滚动
    public bool affectY = false; // 是否影响垂直滚动

    // 私有变量
    private SpriteRenderer sr;
    private Material mat;
    private Vector3 previousTargetPos;
    private float textureUnitSizeX;
    private float textureUnitSizeY;

    void Start()
    {
        // 1. 获取组件
        sr = GetComponent<SpriteRenderer>();
        
        // 2. 材质实例化 (防止修改材质影响其他物体)
        mat = sr.material;

        // 3. 设置目标
        if (followTarget == null)
        {
            followTarget = Camera.main.transform;
        }
        previousTargetPos = followTarget.position;

        // 4. 设置纹理为重复模式
        if (mat.mainTexture != null)
        {
            mat.mainTexture.wrapMode = TextureWrapMode.Repeat;
        }

        // 5. 自动计算并铺满屏幕
        AutoSizeToScreen();
    }

    void LateUpdate()
    {
        if (followTarget == null) return;

        // 1. 计算目标移动量
        Vector3 movement = followTarget.position - previousTargetPos;

        // 2. 计算视差偏移
        float moveX = affectX ? movement.x * parallaxFactor : 0;
        float moveY = affectY ? movement.y * parallaxFactor : 0;

        // 3. 修改材质偏移量 (Offset)
        // 注意：这里使用的是 -move，因为摄像机向右移，背景纹理应该向左移
        Vector2 currentOffset = mat.GetTextureOffset("_MainTex");
        mat.SetTextureOffset("_MainTex", new Vector2(currentOffset.x - moveX, currentOffset.y - moveY));

        // 4. 更新上一帧位置
        previousTargetPos = followTarget.position;
    }

    /// <summary>
    /// 自动计算屏幕大小并设置材质的 Tiling (Scale)，使背景填满屏幕
    /// </summary>
    private void AutoSizeToScreen()
    {
        if (sr == null || Camera.main == null) return;

        Camera cam = Camera.main;
        
        // 获取摄像机在物体所在Z轴平面上的视口大小
        // 正交相机高度的一半
        float height = 2.0f * cam.orthographicSize;
        // 根据长宽比计算宽度
        float width = height * cam.aspect;

        // 获取纹理原始像素尺寸
        float texturePixelWidth = sr.sprite.texture.width;
        float texturePixelHeight = sr.sprite.texture.height;

        // 获取 Sprite 的 Pixels Per Unit
        float ppu = sr.sprite.pixelsPerUnit;

        // 计算纹理在Unity世界单位中的大小
        float textureWorldWidth = texturePixelWidth / ppu;
        float textureWorldHeight = texturePixelHeight / ppu;

        // 计算需要重复多少次才能填满屏幕
        // 稍微多一点点防止边缘缝隙
        float tilesX = width / textureWorldWidth;
        float tilesY = height / textureWorldHeight;

        // 应用 Tiling (Scale) 到材质
        // _MainTex_ST 的 z,w 分量控制 Scale
        mat.mainTextureScale = new Vector2(tilesX, tilesY);
        
        // 记录单位尺寸供调试（可选）
        textureUnitSizeX = textureWorldWidth;
        textureUnitSizeY = textureWorldHeight;
    }
}
