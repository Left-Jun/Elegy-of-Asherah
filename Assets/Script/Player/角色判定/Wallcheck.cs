using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wallcheck : MonoBehaviour
{
    [Header("检测设置")]
    [SerializeField] private float wallCheckDistance; // 墙壁检测距离
    [SerializeField] private LayerMask wallLayer; // 墙壁图层
    
    [Header("墙壁类型（可选）")]
    [SerializeField] private bool detectWallTypes = false; // 是否检测墙壁类型
    [SerializeField] private string normalWallTag = "Wall"; // 普通墙壁标签
    [SerializeField] private string iceWallTag = "Ice"; // 冰墙标签
    [SerializeField] private string climbableWallTag = "Climbable"; // 可攀爬墙壁标签
    
    [Header("状态")]
    public int wallDirection; // 墙壁方向（1=右，-1=左，0=无墙）
    public bool isWallLeft; // 左侧墙壁
    public bool isWallRight; // 右侧墙壁
    public bool isWall; // 任意一侧墙壁
    public string wallType; // 墙壁类型
    public float wallAngle; // 角度
    public Vector2 wallNormal; // 法线
    public Collider2D wallCollider; // 碰撞体
    
    void Update()
    {
        CheckWalls();//墙壁检测
        wallDirection = GetWallDirection();//获取墙壁方向
    }
    
    // 墙壁检测函数
    private void CheckWalls()
    {
        // 检测左侧墙壁
        RaycastHit2D hitWallLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);
        
        // 检测右侧墙壁
        RaycastHit2D hitWallRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);

        // 更新状态
        isWallLeft = hitWallLeft.collider != null;
        isWallRight = hitWallRight.collider != null;
        isWall = isWallLeft || isWallRight;

        // 保存墙壁信息
        if (isWallLeft)
        {
            // 保存左侧墙壁信息
            wallCollider = hitWallLeft.collider;
            wallNormal = hitWallLeft.normal;
            
            // 计算墙壁角度（相对于垂直方向）
            wallAngle = Vector2.Angle(wallNormal, Vector2.up);
        }
        else if (isWallRight)
        {
            // 保存右侧墙壁信息
            wallCollider = hitWallRight.collider;
            wallNormal = hitWallRight.normal;
            
            // 计算墙壁角度（相对于垂直方向）
            wallAngle = Vector2.Angle(wallNormal, Vector2.up);
        }
        else
        {
            wallCollider = null;
            wallNormal = Vector2.zero;
            wallAngle = 0f;
        }
 
        // 角色朝向调整
        if (isWall)
        {
            if (wallDirection != transform.localScale.x) // 确保角色朝向远离墙壁
                transform.localScale = new Vector2(wallDirection, 1); // 调整角色朝向远离墙壁
        }
        // 检测墙壁类型（可选）
        if (detectWallTypes && isWall)
        {
            DetectWallType(isWallLeft ? hitWallLeft : hitWallRight);
        }
        else
        {
            wallType = "None";
        }
    }
    
    // 检测墙壁类型
    private void DetectWallType(RaycastHit2D hit)
    {
        if (hit.collider == null)
        {
            wallType = "None";
            return;
        }
        
        string tag = hit.collider.tag;
        
        if (tag == normalWallTag)
        {
            wallType = "Normal";
        }
        else if (tag == climbableWallTag)
        {
            wallType = "Climbable";
        }
        else if (tag == iceWallTag)
        {
            wallType = "Ice";
        }
        else
        {
            wallType = tag; // 返回实际标签
        }
    }
    
    // 获取墙壁方向（1=右，-1=左，0=无墙）
    public int GetWallDirection()
    {
        if (isWallLeft) return -1;
        if (isWallRight) return 1;
        return 0;
    }


    // 获取墙壁的法线方向
    public Vector2 GetWallNormal()
    {
        if (isWallLeft)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);
            if (hit.collider != null) return hit.normal;//返回墙壁法线
        }
        else if (isWallRight)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);
            if (hit.collider != null) return hit.normal;//返回墙壁法线
        }
        
        return Vector2.zero;
    }


    // 可视化墙壁检测
    private void OnDrawGizmos()
    {
        // 左侧检测线
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x - wallCheckDistance, transform.position.y));
        
        // 右侧检测线
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + wallCheckDistance, transform.position.y));
        
        // 中心点
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.02f);
    }
}