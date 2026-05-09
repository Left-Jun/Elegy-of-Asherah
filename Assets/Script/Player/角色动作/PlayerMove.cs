using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private Rigidbody2D rb;//刚体组件
    private Animator animator; //动画组件
    private PlayerJump playerJump;//跳跃组件
    private PlayerDash playerDash; // 冲刺组件
    private Platformmove currentPlatform;
    [SerializeField]private AudioSource walkSound;//行走音效
    public float moveSpeed;//移动速度
    public float MoveController;//水平输入
    private bool isRunning;//行走状态
    private bool isGroundCheck;//地面检测
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerJump = GetComponent<PlayerJump>();
        playerDash = GetComponent<PlayerDash>(); // 获取冲刺组件
    }

    // Update is called once per frame
    void Update()
    {   
        // 如果不是蹬墙跳状态
        if(playerJump == null || !playerJump.toWallJump)
        {
            // 如果不是冲刺状态，才执行移动
            if (playerDash == null || !playerDash.isDashing)
            {
                //左右移动
                Xmoving();
            }
        }
        
        //角色朝向
        Xturning();
        
        //动画参数更新
        AnimationControler();
    }
    
    //左右移动函数
    private void Xmoving()
    {
        //获取水平输入
        MoveController = Input.GetAxisRaw("Horizontal");
        
        //地面检测
        isGroundCheck = playerJump != null && playerJump.isGrounded;
        
        //行走音效
        if(MoveController != 0 && isGroundCheck){
            if(walkSound != null && !walkSound.isPlaying)
                walkSound.Play();
        }
        else{
            if(walkSound != null)
                walkSound.Pause();
        }
        
        //执行移动
        Vector2 targetVelocity = new Vector2(moveSpeed * MoveController, rb.velocity.y);
        if (currentPlatform != null && isGroundCheck)
        {
            targetVelocity += currentPlatform.CurrentVelocity;
        }
        rb.velocity = targetVelocity;
    }

    //角色朝向函数
    private void Xturning()
    {
        // 如果在冲刺中，使用冲刺方向决定朝向
        if (playerDash != null && playerDash.isDashing)
        {
            float dashX = playerDash.GetDashDirection().x;
            FaceByHorizontalDirection(dashX);
        }
        else if (playerJump != null && playerJump.toWallJump)
        {
            FaceByHorizontalDirection(rb.velocity.x);
        }
        else
        {
            // 普通移动时的朝向
            FaceByHorizontalDirection(MoveController);
        }
    }

    public void FaceByHorizontalDirection(float direction)
    {
        if (direction > 0)
            transform.localScale = new Vector2(1, transform.localScale.y);
        else if (direction < 0)
            transform.localScale = new Vector2(-1, transform.localScale.y);
    }
    
    //动画控制函数
    private void AnimationControler()
    {
        // 冲刺时显示冲刺动画，否则显示行走动画
        if (playerDash != null && playerDash.isDashing)
        {
            isRunning = false;
            // 冲刺动画由冲刺脚本控制
        }
        else
        {
            isRunning = Mathf.Abs(rb.velocity.x) > 0.1f;
        }
        
        if (animator != null)
            animator.SetBool("isRunning", isRunning);
    }

    public void SetCurrentPlatform(Platformmove platform)
    {
        currentPlatform = platform;
    }

    public void ClearCurrentPlatform(Platformmove platform)
    {
        if (currentPlatform == platform)
            currentPlatform = null;
    }
}
