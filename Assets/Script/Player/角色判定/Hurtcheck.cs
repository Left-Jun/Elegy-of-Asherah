using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtcheck : MonoBehaviour
{
    private GetRespawn getRespawn;
    [Header("伤害检测状态")]
    public bool ishurt;
    [SerializeField]private bool istraphurt;
    public bool isdead;

    private void Awake()
    {
        ishurt = false;
        istraphurt = false;
        isdead = false;
    }

    void Start()
    {
        getRespawn = GetComponentInParent<GetRespawn>();
        if (getRespawn == null)
        {
            Debug.LogWarning("HurtCheck: 未在父物体上找到 GetRespawn 脚本。");
        }
    }

    void Update() { }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            Debug.Log("Trap Detected");
            ishurt = true;
            istraphurt = true;
            isdead = true;

            MaskControl maskControl = GetComponentInParent<MaskControl>();
            if (maskControl != null)
            {
                Animator currentAnim = maskControl.GetCurrentActiveAnimator();
                if (currentAnim != null)
                {
                    currentAnim.SetTrigger("isDead");
                }
            }

            if (getRespawn != null)
            {
                Invoke(nameof(TriggerRespawn), 0.15f);
            }
            else
            {
                Debug.LogError("HurtCheck: 无法触发重生，重生脚本引用为空！");
            }
        }

        // 灵能碎片收集（原来的“情绪碎片”改为灵能碎片）
        if (collision.gameObject.CompareTag("EmotionShard"))
        {
            CollectShard(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            Debug.Log("Trap Left");
            ishurt = false;
            istraphurt = false;
            isdead = false;
        }
    }

    // 收集灵能碎片（原为情绪碎片）
    private void CollectShard(GameObject shard)
    {
        // 增加灵能碎片
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.AddShard(1);
            Debug.Log("灵能碎片已收集！");
        }
        else
        {
            Debug.LogError("EnergyManager 实例不存在！无法收集碎片。");
        }

        // 播放收集音效（可选）
        // AudioSource.PlayClipAtPoint(collectSound, transform.position);

        // 播放粒子特效（可选）
        // Instantiate(collectEffectPrefab, shard.transform.position, Quaternion.identity);

        // 销毁碎片对象
        shard.SetActive(false);
    }

    private void TriggerRespawn()
    {
        if (getRespawn != null)
        {
            getRespawn.Respawn();
        }

        ishurt = false;
        istraphurt = false;
        isdead = false;
        Debug.Log("角色已重生，状态重置。");
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