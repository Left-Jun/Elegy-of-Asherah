using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 根据角色面朝方向切换左右视觉子物体。
/// 挂载在视觉状态的根物体上（如 Normal、Enhanced）。
/// </summary>
public class CharacterDirectionVisual : MonoBehaviour
{
    [Header("左右视觉子物体")]
    [SerializeField] private GameObject leftVisual;   // 朝左显示的物体
    [SerializeField] private GameObject rightVisual;  // 朝右显示的物体

    private Transform playerRoot;                     // 角色根物体（用于获取朝向）

    private void Start()
    {
        // 尝试找到角色根物体（通常是挂载 Player 脚本的物体）
        playerRoot = GetRootTransform();
        if (playerRoot == null)
            Debug.LogWarning("未找到角色根物体，将使用自身父级作为朝向参考。");
    }

    private void Update()
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // 获取角色朝向（通常通过 transform.localScale.x 的正负判断）
        float facing = GetFacingDirection();
        bool facingLeft = facing < 0;
        bool facingRight = facing > 0;

        // 根据朝向启用/禁用左右视觉
        if (leftVisual != null) leftVisual.SetActive(facingLeft);
        if (rightVisual != null) rightVisual.SetActive(facingRight);
    }

    /// <summary>
    /// 获取角色面朝方向（1=右，-1=左，0=未知）
    /// </summary>
    private float GetFacingDirection()
    {
        // 方法1：从 PlayerMove 组件获取
        PlayerMove playerMove = GetComponentInParent<PlayerMove>();
        if (playerMove != null)
        {
            // 假设 PlayerMove 中公开了朝向变量或可通过 transform.localScale 获取
            // 这里使用 playerMove.transform.localScale.x
            return playerMove.transform.localScale.x;
        }

        // 方法2：从根物体获取
        if (playerRoot != null)
            return playerRoot.localScale.x;

        // 方法3：从自身父物体获取
        if (transform.parent != null)
            return transform.parent.localScale.x;

        // 默认返回 1（向右）
        return 1f;
    }

    /// <summary>
    /// 查找角色根物体（包含 PlayerMove 等核心组件的物体）
    /// </summary>
    private Transform GetRootTransform()
    {
        Transform current = transform;
        while (current != null)
        {
            // 如果找到了 PlayerMove 组件，说明当前物体就是角色根物体
            if (current.GetComponent<PlayerMove>() != null)
                return current;

            // 尝试查找父级中的 PlayerMove
            PlayerMove moveInParent = current.GetComponentInParent<PlayerMove>();
            if (moveInParent != null)
                return moveInParent.transform;

            current = current.parent;
        }
        return null;
    }
}