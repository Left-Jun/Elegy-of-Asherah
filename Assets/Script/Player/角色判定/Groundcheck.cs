using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [Header("检测设置")]
    [SerializeField] private float groundCheckDistance; // 地面检测距离
    [SerializeField] private LayerMask groundLayer; // 地面图层
    public LayerMask GroundLayer => groundLayer;
    
    [Header("地面类型检测")]
    [SerializeField] private bool detectGroundTypes = false; // 是否检测地面类型
    [SerializeField] private string normalGroundTag = "Ground"; // 普通地面标签
    [SerializeField] private string iceGroundTag = "Ice"; // 冰面标签
    [SerializeField] private string mudGroundTag = "Mud"; // 泥沼地面标签
    [SerializeField] private string grassGroundTag = "Grass"; // 草地标签
    [SerializeField] private string climbableGroundTag = "Climbable"; // 可攀爬地面标签
    
    [Header("状态")]
    public bool isGrounded; // 地面状态
    public string groundType; // 地面类型
    public float groundAngle; // 地面角度
    public Vector2 groundNormal; // 地面法线
    public Collider2D groundCollider; // 地面碰撞体
    
    void Update()
    {
        CheckGround();
    }
    
    // 地面检测函数
    private void CheckGround()
    {
        // 检测下方地面（从角色位置发射）
        RaycastHit2D hitGround = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        
        // 更新状态
        isGrounded = hitGround.collider != null;
        
        if (isGrounded)
        {
            // 保存地面信息
            groundCollider = hitGround.collider;
            groundNormal = hitGround.normal;
            
            // 计算地面角度（相对于垂直方向）
            groundAngle = Vector2.Angle(groundNormal, Vector2.up);
            
            // 检测地面类型（可选）
            if (detectGroundTypes)
            {
                DetectGroundType(hitGround);
            }
            else
            {
                groundType = "Normal";
            }
        }
        else
        {
            // 重置状态
            groundCollider = null;
            groundNormal = Vector2.up; // 默认法线向上
            groundAngle = 0f;
            groundType = "None";
        }
    }
    
    // 可视化地面检测射线
    private void OnDrawGizmos()
    {
        if (isGrounded)
        {
            // 检测到地面时显示绿色
            Gizmos.color = Color.green;
        }
        else
        {
            // 没有地面时显示红色
            Gizmos.color = Color.red;
        }
        
        // 绘制检测线
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
    }

    // 检测地面类型
    private void DetectGroundType(RaycastHit2D hit)
    {
        if (hit.collider == null)
        {
            groundType = "None";
            return;
        }
        
        string tag = hit.collider.tag;
        
        // 根据标签判断地面类型
        if (tag == normalGroundTag)
        {
            groundType = "Normal";
        }
        else if (tag == iceGroundTag)
        {
            groundType = "Ice";
        }
        else if (tag == mudGroundTag)
        {
            groundType = "Mud";
        }
        else if (tag == grassGroundTag)
        {
            groundType = "Grass";
        }
        else if (tag == climbableGroundTag)
        {
            groundType = "Climbable";
        }
        else
        {
            groundType = tag; // 返回实际标签
        }
    }
    
    // 获取地面摩擦力（根据地面类型）
    public float GetGroundFriction()
    {
        switch (groundType)
        {
            case "Ice":
                return 0.1f;
            case "Mud":
                return 0.3f;
            case "Grass":
                return 0.8f;
            case "Normal":
            default:
                return 1.0f;
        }
    }
    
    // 获取地面弹力（根据地面类型）
    public float GetGroundBounce()
    {
        switch (groundType)
        {
            case "Ice":
                return 0.2f;
            case "Mud":
                return 0.1f;
            case "Grass":
                return 0.7f;
            case "Normal":
            default:
                return 0.5f;
        }
    }
    
    // 检查是否是可行走的地面（角度小于45度）
    public bool IsWalkableGround(float maxAngle = 45f)
    {
        return isGrounded && groundAngle <= maxAngle;
    }
    
    // 检查是否是可攀爬的地面（陡峭但可攀爬）
    public bool IsClimbableGround()
    {
        return isGrounded && groundType == "Climbable";
    }
    
    // 强制重新检测地面（某些情况下需要）
    public void ForceGroundCheck()
    {
        CheckGround();
    }
    
    // 获取地面移动速度乘数（不同地面移动速度不同）
    public float GetGroundSpeedMultiplier()
    {
        switch (groundType)
        {
            case "Ice":
                return 1.5f;
            case "Mud":
                return 0.6f;
            case "Grass":
                return 0.9f;
            case "Normal":
            default:
                return 1.0f;
        }
    }
    
    // 获取地面跳跃力乘数（不同地面跳跃力不同）
    public float GetGroundJumpMultiplier()
    {
        switch (groundType)
        {
            case "Ice":
                return 0.9f;
            case "Mud":
                return 0.7f;
            case "Grass":
                return 1.1f;
            case "Normal":
            default:
                return 1.0f;
        }
    }
}
