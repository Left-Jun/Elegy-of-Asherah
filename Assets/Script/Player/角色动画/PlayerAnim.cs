using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private enum AnimationState { Idle, Run, Jump, Fall, InWall, Dash }
    private AnimationState currentState;
    private Animator animator;
    private PlayerMove playermove;
    private PlayerJump playerjump;
    private Wallcheck wallcheck;
    private PlayerDash playerdash;

    void Start()
    {
        // 尝试从自身或父物体获取 Animator
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInParent<Animator>();
        
        // 获取脚本组件（根据您的项目结构，这些组件通常在父物体上）
        playermove = GetComponentInParent<PlayerMove>();
        playerjump = GetComponentInParent<PlayerJump>();
        wallcheck = GetComponentInParent<Wallcheck>();
        playerdash = GetComponentInParent<PlayerDash>();

        // 输出引用状态以便调试
        Debug.Log($"PlayerAnimation 初始化: animator={animator != null}, move={playermove != null}, jump={playerjump != null}, wall={wallcheck != null}, dash={playerdash != null}");
    }

    void Update()
    {
        // 如果有任何必要组件缺失，则跳过更新
        if (playermove == null || playerjump == null || wallcheck == null || animator == null)
        {
            Debug.LogWarning("PlayerAnimation 组件引用缺失，无法更新动画");
            return;
        }

        // 冲刺优先级最高
        if (playerdash != null && playerdash.isDashing)
        {
            currentState = AnimationState.Dash;
        }
        else
        {
            // 状态判断顺序：贴墙 > 跳跃 > 下落 > 跑 > 待机
            if (wallcheck.isWall && !playerjump.isGrounded)
                currentState = AnimationState.InWall;
            else if (playerjump.isJumping)
                currentState = AnimationState.Jump;
            else if (playerjump.isFalling)
                currentState = AnimationState.Fall;
            else if (playermove.MoveController != 0)
                currentState = AnimationState.Run;
            else
                currentState = AnimationState.Idle;
        }

        // 更新 Animator 的 States 参数
        int stateValue = (int)currentState;
        animator.SetInteger("States", stateValue);

        // 可选：输出当前状态用于调试
        // Debug.Log($"当前动画状态: {currentState}, 值: {stateValue}");
    }
}