using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private Animator animator;//动画组件
    private bool isActivated = false;//是否激活
    [SerializeField] private Transform checkPointTransform;//检查点位置

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            isActivated = true;
            Debug.Log("Checkpoint Activated");
            animator.SetBool("Activate", isActivated);
            collision.gameObject.GetComponent<Checkpoints>().checkPoint = checkPointTransform;
        }
    }
}
