using UnityEngine;

public class ExtraColliderManager : MonoBehaviour
{
    [Header("碰撞箱子物体")]
    public GameObject wallColliderObject;   // 贴墙时启用的额外碰撞箱

    [Header("依赖脚本")]
    public GroundCheck groundCheck;
    public WallCheck wallCheck;
    // 不需要 PlayerMove，直接使用角色朝向

    private bool isWallColliderActive = false;

    void Update()
    {
        bool shouldEnableWallCollider = ShouldEnableWallCollider();

        if (shouldEnableWallCollider && !isWallColliderActive)
        {
            EnableWallCollider(true);
        }
        else if (!shouldEnableWallCollider && isWallColliderActive)
        {
            EnableWallCollider(false);
        }
    }

    private bool ShouldEnableWallCollider()
    {
        // 1. 必须在空中
        if (groundCheck.isGrounded) return false;

        // 2. 必须检测到墙壁
        if (!wallCheck.isWall) return false;

        // 3. 角色面朝方向必须与墙壁方向一致
        int facingDir = GetFacingDirection();
        int wallDir = wallCheck.wallDirection;

        return facingDir == wallDir;
    }

    // 获取角色面朝方向（通常用 localScale.x）
    private int GetFacingDirection()
    {
        return (int)Mathf.Sign(transform.localScale.x);
    }

    private void EnableWallCollider(bool enable)
    {
        if (wallColliderObject != null)
        {
            wallColliderObject.SetActive(enable);
            isWallColliderActive = enable;
        }
    }
}