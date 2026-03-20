using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtcheck : MonoBehaviour
{
    private GetRespawn getRespawn; // 引用父物体上的重生脚本
    [Header("伤害检测状态")]
    public bool ishurt;//受伤状态
    [SerializeField]private bool istraphurt;//陷阱受伤状态
    public bool isdead;//死亡状态
    //================================
    private void Awake()
    {
        ishurt = false;
        istraphurt = false;
        isdead = false;
    }

    void Start()
    {
         // 从当前子物体向上查找，获取父物体Player上的 GetRespawn 组件
        getRespawn = GetComponentInParent<GetRespawn>();
        if (getRespawn == null)
        {
            Debug.LogWarning("HurtCheck: 未在父物体上找到 GetRespawn 脚本。");
        }
    }


    void Update()
    {

    }

    //===========================================================================================
    //陷阱检测
private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.gameObject.CompareTag("Trap"))
    {
        Debug.Log("Trap Detected");
        ishurt = true;
        istraphurt = true;
        isdead = true;

        // 1. 播放当前情绪子对象的死亡动画
        MaskControl maskControl = GetComponentInParent<MaskControl>();
        if (maskControl != null)
        {
            Animator currentAnim = maskControl.GetCurrentActiveAnimator();
            if (currentAnim != null)
            {
                currentAnim.SetTrigger("isDead");
            }
        }

        // 2. 延时调用重生方法 (等待死亡动画播放)
        // 请根据你的“死亡”动画实际长度调整延时，例如1.5秒
        if (getRespawn != null)
        {
            Invoke(nameof(TriggerRespawn), 0.15f); // 1.5秒后执行
        }
        else
        {
            Debug.LogError("HurtCheck: 无法触发重生，重生脚本引用为空！");
        }
    }
//===========================================================================================
    if (collision.gameObject.CompareTag("BreakableWall"))
    {
        // 检查破坏条件：是否为愤怒状态，且正在冲刺
        if (CanBreakWall())
        {
            Debug.Log("条件满足，开始破坏墙壁！");
            // 触发墙壁的破坏效果
            BreakTheWall(collision.gameObject);
        }
        else
        {
            // 可选：给玩家一个提示，比如播放一个被阻挡的音效或屏幕震动
            Debug.Log("撞击墙壁，但未满足破坏条件（需要愤怒+冲刺）。");
        }
    }
        // 新增：情绪碎片收集逻辑
    if (collision.gameObject.CompareTag("EmotionShard"))
    {
        CollectShard(collision.gameObject);
    }

}
//===========================================================================================
// 收集情绪碎片的方法
private void CollectShard(GameObject shard)
{
    // 1. 通知游戏管理器分数增加
    GameManager.Instance?.CollectShard(); // 推荐使用单例管理器
    // 或直接调用：FindObjectOfType<UIManager>().AddScore(1);
    
    // 2. 播放收集音效（如果有）
    // AudioSource.PlayClipAtPoint(collectSound, transform.position);
    
    // 3. 播放粒子特效（如果有）
    // Instantiate(collectEffectPrefab, shard.transform.position, Quaternion.identity);
    
    // 4. 销毁碎片对象
    shard.SetActive(false);
    
    Debug.Log("情绪碎片已收集！");
}
//===========================================================================================
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Trap"))//离开陷阱
        {
            Debug.Log("Trap Left");
            ishurt = false;//受伤状态
            istraphurt = false;//陷阱受伤状态
            isdead = false;//死亡状态
        }
    }
    //===========================================================================================
    private void TriggerRespawn()
{
    // 调用父物体上的重生逻辑
    if (getRespawn != null)
    {
        getRespawn.Respawn();
    }

    // 重生后，重置本地状态
    ishurt = false;
    istraphurt = false;
    isdead = false;
    Debug.Log("角色已重生，状态重置。");
}
//===========================================================================================
// 判断是否满足破坏墙壁的条件
private bool CanBreakWall()
{
    // 条件一：必须处于愤怒状态
    MaskControl maskControl = GetComponentInParent<MaskControl>();
    if (maskControl == null || maskControl.currentEmotion != MaskControl.EmotionState.Angry)
    {
        return false;
    }
    
    // 条件二：必须正在冲刺中
    PlayerDash playerDash = GetComponentInParent<PlayerDash>();
    if (playerDash == null || !playerDash.IsDashing())
    {
        return false;
    }
    
    // 两个条件都满足
    return true;
}

// 执行墙壁破坏
private void BreakTheWall(GameObject wallToBreak)
{
    // 方法1：直接销毁（最简单）
    // Destroy(wallToBreak);
    
    // 方法2：播放破坏动画后销毁（推荐，效果更好）
    Animator wallAnimator = wallToBreak.GetComponent<Animator>();
    if (wallAnimator != null)
    {
        // 假设你的墙壁动画控制器有一个名为“Break”的触发器
        wallAnimator.SetTrigger("Break");
        // 动画播放完毕后销毁墙壁，你可以根据动画长度设置延时
        Destroy(wallToBreak, 0.5f); // 假设动画长0.5秒
    }
    else
    {
        // 如果没有动画，直接销毁
        wallToBreak.SetActive(false);
    }
    
    // 这里可以添加破坏音效、屏幕震动、粒子特效等
    // 例如：CameraShake.Instance.Shake(0.1f, 0.2f);
}

}



/*
    position = GetComponentInParent<Transform>();
    positionupdate();
    private Transform position;//位置
    public Transform lastJumpPosition;//最后跳跃位置
    public Transform lastGroundPosition;//最后地面位置
    private void positionupdate()
    {
        //记录最后跳跃位置
        if(GetComponentInParent<PlayerJump>().isGrounded==false)
        {
            lastJumpPosition.position = position.position;
            respawnPosition.position = lastJumpPosition.position;
        Debug.Log("Waiting for Ground");
        IEnumerator WaitForGround(){
            yield return new WaitUntil(() => GetComponentInParent<PlayerJump>().isGrounded == true);
        }
        }
        if(GetComponentInParent<PlayerJump>().isGrounded==true)
        {
            lastGroundPosition.position = position.position;
            respawnPosition.position = lastGroundPosition.position;
        }
    }   
    */