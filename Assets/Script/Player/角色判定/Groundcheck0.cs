using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Groundcheck : MonoBehaviour
{
    [Header("检测设置")]
    [SerializeField] private float groundCheckDistance; // 地面检测距离
    [SerializeField] private LayerMask groundLayer; // 地面图层
    
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
        // 检测下方地面
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
            
            // 绘制击中点和法线
            if (groundCollider != null)
            {
                // 绘制法线
                Gizmos.color = Color.red;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

            }
        }
        else
        {
            // 没有地面时显示红色
            Gizmos.color = Color.red;
        }
        
        // 绘制检测线
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        
    }
//============================================================================================================================
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
                return 0.1f; // 冰面摩擦力小
            case "Mud":
                return 0.3f; // 泥沼摩擦力较大
            case "Grass":
                return 0.8f; // 草地摩擦力中等
            case "Normal":
            default:
                return 1.0f; // 普通地面摩擦力正常
        }
    }
    
    // 获取地面弹力（根据地面类型）
    public float GetGroundBounce()
    {
        switch (groundType)
        {
            case "Ice":
                return 0.2f; // 冰面弹性小
            case "Mud":
                return 0.1f; // 泥沼基本没弹性
            case "Grass":
                return 0.7f; // 草地有一些弹性
            case "Normal":
            default:
                return 0.5f; // 普通地面弹性正常
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
    

//============================================================================================================================
    // 获取地面移动速度乘数（不同地面移动速度不同）
    public float GetGroundSpeedMultiplier()
    {
        switch (groundType)
        {
            case "Ice":
                return 1.5f; // 冰面滑，移动更快但难控制
            case "Mud":
                return 0.6f; // 泥沼移动慢
            case "Grass":
                return 0.9f; // 草地稍慢
            case "Normal":
            default:
                return 1.0f; // 普通地面正常速度
        }
    }
    
    // 获取地面跳跃力乘数（不同地面跳跃力不同）
    public float GetGroundJumpMultiplier()
    {
        switch (groundType)
        {
            case "Ice":
                return 0.9f; // 冰面滑，跳跃力稍小
            case "Mud":
                return 0.7f; // 泥沼粘稠，跳跃力小
            case "Grass":
                return 1.1f; // 草地有弹性，跳跃力稍大
            case "Normal":
            default:
                return 1.0f; // 普通地面正常跳跃力
        }
    }
    
}

/*                if (hit.collider != null)
                {
                    Gizmos.DrawLine(hit.point, hit.point + (Vector3)hit.normal * 0.2f);
                }
                
                // 根据地面类型显示不同颜色的击中点
                switch (groundType)
                {
                    case "Ice":
                        Gizmos.color = Color.cyan;
                        break;
                    case "Mud":
                        Gizmos.color = new Color(0.5f, 0.25f, 0f); // 棕色
                        break;
                    case "Grass":
                        Gizmos.color = Color.green;
                        break;
                    case "Climbable":
                        Gizmos.color = Color.yellow;
                        break;
                    default:
                        Gizmos.color = Color.white;
                        break;
                }
                
                // 绘制击中点
                Gizmos.DrawWireSphere(hit.point, 0.05f);*/


/*        
        // 显示地面类型文本（在Scene视图中显示）
        #if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 10;
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.3f, 
            $"地面: {groundType}\n角度: {groundAngle:F1}°"
        );
        #endif*/