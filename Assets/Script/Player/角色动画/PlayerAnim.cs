using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private enum AnimationState{ Idle, Run, Jump, Fall , inwall};//动画状态枚举
    private AnimationState currentState;//当前动画状态
    private Animator animator;//动画组件
    private PlayerMove playermove;//移动脚本引用
    private PlayerJump playerjump;//跳跃脚本引用
    private Wallcheck wallcheck;//贴墙检测脚本引用
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        playermove = GetComponentInParent<PlayerMove>();   
        playerjump = GetComponentInParent<PlayerJump>();
        wallcheck = GetComponentInParent<Wallcheck>();
    }

    // Update is called once per frame
    void Update()
    {
        //动画状态更新
        if(playermove.MoveController!=0)
            currentState = AnimationState.Run;
        if(playermove.MoveController == 0)
            currentState = AnimationState.Idle;
        if(playerjump.isJumping)
            currentState = AnimationState.Jump;
        if(playerjump.isFalling)
            currentState = AnimationState.Fall;
        if(wallcheck.isWall&&!playerjump.isGrounded)
            currentState = AnimationState.inwall;
        //动画参数更新
        animator.SetInteger("States",(int)currentState);
    }
}
